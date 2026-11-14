using System;
using System.Collections.Generic;
using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using MyCAM.LoadingManager;
using OCC.BRepBuilderAPI;
using OCC.BRepProj;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using OCCViewer;

namespace MyCAM.Editor
{
	internal class GlassDXFImportAction : EditActionBase
	{
		public event Action ActionCompleted;

		public bool IsImportSuccess
		{
			get; private set;
		}

		public string ImportedFileName
		{
			get; private set;
		}

		public GlassDXFImportAction( DataManager dataManager, Viewer viewer, ViewManager viewManager, string filePath )
			: base( dataManager )
		{
			if( string.IsNullOrEmpty( filePath ) ) {
				throw new ArgumentNullException( nameof( filePath ) );
			}
			if( viewer == null || viewManager == null ) {
				throw new ArgumentNullException( "GlassDXFImportAction constructing argument null" );
			}
			m_FilePath = filePath;
			m_Viewer = viewer;
			m_ViewManager = viewManager;
			IsImportSuccess = false;
			ImportedFileName = System.IO.Path.GetFileName( filePath );
		}

		public override EditActionType ActionType
		{
			get { return EditActionType.ImportDxfGlass; }
		}

		public override void Start()
		{
			if( m_IsRunning ) {
				return;
			}
			m_IsRunning = true;
			try {
				base.Start();

				// Show parameter dialog
				GlassDXFImportDlg dlg = new GlassDXFImportDlg();
				dlg.ShowDialog( MyApp.MainForm );
				if( !dlg.Accepted ) {
					End();
					return;
				}

				// Execute import
				ExecuteImport( m_FilePath, dlg.Radius, dlg.SurfaceHeight );
				End();
			}
			finally {
				m_LoadingBarManager?.Dispose();
				m_LoadingBarManager = null;
				m_IsRunning = false;
			}
		}

		public override void End()
		{
			m_LoadingBarManager?.Dispose();
			m_LoadingBarManager = null;
			ActionCompleted?.Invoke();
			base.End();
		}

		void ExecuteImport( string szFileName, double radius, double height )
		{
			try {
				m_LoadingBarManager = new LoadingBarManager( MyApp.MainForm );
				m_LoadingBarManager.ShowImport( ImportedFileName, LoadingWorkflow.DxfImport );
				Helper.FileIO.DxfReader reader = new Helper.FileIO.DxfReader();
				TopoDS_Shape shape = reader.Read( szFileName, m_LoadingBarManager.Report );
				if( shape == null || shape.IsNull() ) {
					MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "DXFImportFailedNoGeom", DXF_IMPORT_FAILED_NO_GEOM ), MyApp.NoticeType.Error );
					return;
				}

				// Build target face based on parameters
				m_LoadingBarManager.Report( LoadingStep.BuildTargetSurface );
				TopoDS_Face targetFace = BuildTargetFace( radius, height );
				if( targetFace == null || targetFace.IsNull() ) {
					MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "BuildSurfaceFailed", BUILD_SURFACE_FAILED ), MyApp.NoticeType.Error );
					return;
				}

				// Compute projection offset: place wires above surface
				double projectionOffset = ( radius > 0 ) ? radius * OFFSET_MULTIPLIER : height + FLAT_OFFSET_MARGIN;

				// Project DXF wires onto the target face
				List<TopoDS_Wire> projectedWires = ProjectWiresOntoFace( shape, targetFace, projectionOffset, m_LoadingBarManager.Report );
				if( projectedWires.Count == 0 ) {
					MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "DXFProjectionFailed", DXF_PROJECTION_FAILED ), MyApp.NoticeType.Error );
					return;
				}

				// Add target face and projected wires as part
				List<TopoDS_Shape> partShapes = new List<TopoDS_Shape>();
				partShapes.Add( targetFace );
				foreach( var wire in projectedWires ) {
					partShapes.Add( wire );
				}
				TopoDS_Shape partCompound = ShapeTool.MakeCompound( partShapes );

				// Build edge map and add path
				TopTools_IndexedDataMapOfShapeListOfShape edgeMap = BuildEdgeMapForFace( projectedWires, targetFace, m_LoadingBarManager.Report );

				// Batch add part and paths without triggering events
				m_DataManager.AddPartAndPathsSilently( partCompound, projectedWires, edgeMap, m_LoadingBarManager.Report );

				// Manually update view
				UpdateAllViewData();

				// set import success
				IsImportSuccess = true;
				m_LoadingBarManager.Report( LoadingStep.ImportComplete );
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "DXFImportException", DXF_IMPORT_EXCEPTION ) + ex.Message, MyApp.NoticeType.Error );
				IsImportSuccess = false;
			}
		}

		TopoDS_Face BuildTargetFace( double radius, double height )
		{
			if( radius == 0 ) {

				// Build a flat plane (large enough XY plane)
				gp_Pln plane = new gp_Pln( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ) );
				BRepBuilderAPI_MakeFace faceMaker = new BRepBuilderAPI_MakeFace( plane, -FLAT_PLANE_SIZE, FLAT_PLANE_SIZE, -FLAT_PLANE_SIZE, FLAT_PLANE_SIZE );
				if( !faceMaker.IsDone() ) {
					return null;
				}
				return faceMaker.Face();
			}

			// Build spherical cap with given radius and height
			gp_Ax3 ax3 = new gp_Ax3( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ) );
			gp_Sphere sphere = new gp_Sphere( ax3, radius );

			// V parameter: from arccos((R-H)/R) to PI/2 (top of sphere)
			double vMin = Math.PI / 2 - Math.Acos( ( radius - height ) / radius );
			BRepBuilderAPI_MakeFace sphereFaceMaker = new BRepBuilderAPI_MakeFace( sphere, 0, 2 * Math.PI, vMin, Math.PI / 2 );
			if( !sphereFaceMaker.IsDone() ) {
				return null;
			}
			return sphereFaceMaker.Face();
		}

		List<TopoDS_Wire> ProjectWiresOntoFace( TopoDS_Shape wireShape, TopoDS_Face targetFace, double offsetZ, Action<LoadingBarInfo> reportProgress = null )
		{
			List<TopoDS_Wire> projectedWires = new List<TopoDS_Wire>();
			gp_Dir projDir = new gp_Dir( 0, 0, -1 );
			int wireCount = CountShapes( wireShape, TopAbs_ShapeEnum.TopAbs_WIRE );
			int total = wireCount;

			// Report progress to the loading bar.
			int completed = 0;
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.ProjectContours, 0, total ) );

			// Explore wires from the DXF compound
			TopExp_Explorer wireExp = new TopExp_Explorer( wireShape, TopAbs_ShapeEnum.TopAbs_WIRE );
			for( ; wireExp.More(); wireExp.Next() ) {
				TopoDS_Wire wire = TopoDS.ToWire( wireExp.Current() );

				// Offset wire above the surface
				gp_Trsf trsf = new gp_Trsf();
				trsf.SetTranslation( new gp_Vec( 0, 0, offsetZ ) );
				BRepBuilderAPI_Transform transform = new BRepBuilderAPI_Transform( wire, trsf, true );
				TopoDS_Wire offsetWire = TopoDS.ToWire( transform.Shape() );

				// Project
				BRepProj_Projection projector = new BRepProj_Projection( offsetWire, targetFace, projDir );
				while( projector.More() ) {
					projectedWires.Add( projector.Current() );
					projector.Next();
				}

				// Report progress to the loading bar.
				reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.ProjectContours, ++completed, total ) );
			}

			// If no wires found, try projecting individual edges as single-edge wires
			if( projectedWires.Count == 0 ) {
				// Report progress to the loading bar.
				total = CountShapes( wireShape, TopAbs_ShapeEnum.TopAbs_EDGE );
				completed = 0;
				reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.ProjectContourEdges, 0, total ) );
				TopExp_Explorer edgeExp = new TopExp_Explorer( wireShape, TopAbs_ShapeEnum.TopAbs_EDGE );
				for( ; edgeExp.More(); edgeExp.Next() ) {
					TopoDS_Edge edge = TopoDS.ToEdge( edgeExp.Current() );
					BRepBuilderAPI_MakeWire wireMaker = new BRepBuilderAPI_MakeWire( edge );
					if( !wireMaker.IsDone() ) {
						// Report progress to the loading bar.
						reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.ProjectContourEdges, ++completed, total ) );
						continue;
					}

					gp_Trsf trsf = new gp_Trsf();
					trsf.SetTranslation( new gp_Vec( 0, 0, offsetZ ) );
					BRepBuilderAPI_Transform transform = new BRepBuilderAPI_Transform( wireMaker.Wire(), trsf, true );
					TopoDS_Wire offsetWire = TopoDS.ToWire( transform.Shape() );

					BRepProj_Projection projector = new BRepProj_Projection( offsetWire, targetFace, projDir );
					while( projector.More() ) {
						projectedWires.Add( projector.Current() );
						projector.Next();
					}
					// Report progress to the loading bar.
					reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.ProjectContourEdges, ++completed, total ) );
				}
			}
			return projectedWires;
		}

		static int CountShapes( TopoDS_Shape shape, TopAbs_ShapeEnum shapeType )
		{
			int count = 0;
			for( TopExp_Explorer explorer = new TopExp_Explorer( shape, shapeType ); explorer.More(); explorer.Next() ) {
				count++;
			}
			return count;
		}

		TopTools_IndexedDataMapOfShapeListOfShape BuildEdgeMapForFace( List<TopoDS_Wire> wireList, TopoDS_Face face, Action<LoadingBarInfo> reportProgress = null )
		{
			TopTools_IndexedDataMapOfShapeListOfShape edgeMap = new TopTools_IndexedDataMapOfShapeListOfShape();

			// Report progress to the loading bar.
			int completed = 0;
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.BuildEdgeMap, 0, wireList.Count ) );
			foreach( var wire in wireList ) {
				TopExp_Explorer edgeExp = new TopExp_Explorer( wire, TopAbs_ShapeEnum.TopAbs_EDGE );
				for( ; edgeExp.More(); edgeExp.Next() ) {
					TopoDS_Edge edge = TopoDS.ToEdge( edgeExp.Current() );
					if( !edgeMap.Contains( edge ) ) {
						TopTools_ListOfShape faceList = new TopTools_ListOfShape();
						faceList.Append( face );
						edgeMap.Add( edge, faceList );
					}
				}
				// Report progress to the loading bar.
				reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.BuildEdgeMap, ++completed, wireList.Count ) );
			}
			return edgeMap;
		}

		void UpdateAllViewData()
		{
			m_ViewManager.RebuildAllViews( m_DataManager, true, m_LoadingBarManager.Report );
			m_Viewer.UpdateView();
		}

		string m_FilePath;
		LoadingBarManager m_LoadingBarManager;
		bool m_IsRunning;
		Viewer m_Viewer;
		ViewManager m_ViewManager;

		// Offset multiplier: place wires at 1.2x radius above origin for projection
		const double OFFSET_MULTIPLIER = 1.2;

		// Flat mode offset margin above plane
		const double FLAT_OFFSET_MARGIN = 100.0;

		// Half-size of the flat plane
		const double FLAT_PLANE_SIZE = 5000.0;

		const string DXF_IMPORT_FAILED_NO_GEOM = "DXF 匯入失敗: 無有效幾何";
		const string BUILD_SURFACE_FAILED = "建構曲面失敗";
		const string DXF_PROJECTION_FAILED = "DXF 投影失敗: 無有效投影結果";
		const string DXF_IMPORT_EXCEPTION = "DXF 匯入失敗: ";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "GlassDXFImportAction" );
	}
}
