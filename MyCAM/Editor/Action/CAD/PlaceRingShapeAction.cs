using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using MyCAM.App;
using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.Helper.FileIO;
using MyCAM.LoadingManager;
using OCC.gp;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;

namespace MyCAM.Editor
{
	internal class PlaceRingShapeAction : EditActionBase
	{
		public event Action ActionCompleted;

		LoadingBarManager m_LoadingBarManager;
		bool m_IsRunning;
		bool m_IsExternalLoading;

		public bool IsImportSuccess
		{
			get; private set;
		}

		public string ImportedFileName
		{
			get; private set;
		}

		public PlaceRingShapeAction( DataManager dataManager, Viewer viewer, ViewManager viewManager )
			: base( dataManager )
		{
			if( viewer == null || viewManager == null ) {
				throw new ArgumentNullException( "PlaceRingShapeAction constructing argument null" );
			}
			m_Viewer = viewer;
			m_ViewManager = viewManager;
			IsImportSuccess = false;
			ImportedFileName = string.Empty;
		}

		// the shape is already added to the data manager by the caller, only place it and build the paths
		public PlaceRingShapeAction( DataManager dataManager, Viewer viewer, ViewManager viewManager, TopoDS_Shape addedShape, LoadingBarManager loadingBarManager = null )
			: this( dataManager, viewer, viewManager )
		{
			m_LoadingBarManager = loadingBarManager;
			m_IsExternalLoading = loadingBarManager != null;
			if( addedShape != null && addedShape.IsNull() == false ) {
				m_AddedShape = addedShape;
			}

			// the caller already added the part, placing or path building failure does not revert it
			IsImportSuccess = true;
		}

		public override EditActionType ActionType
		{
			get { return EditActionType.PlaceRingShape; }
		}

		public override void Start()
		{
			if( m_IsRunning ) {
				return;
			}
			base.Start();
			m_IsRunning = true;
			try {
				ImportAndPlaceShape();
			}
			finally {
				CloseLoadingBar();
				m_IsRunning = false;
			}
		}

		void ImportAndPlaceShape()
		{
			// the shape comes from the caller, otherwise read it from a file
			TopoDS_Shape fileShape = m_AddedShape;
			if( fileShape == null ) {
				Import3DFile( out fileShape );
				if( fileShape == null ) {
					End();
					return;
				}
			}
			else if( m_LoadingBarManager == null ) {
				m_LoadingBarManager = new LoadingBarManager( MyApp.MainForm );
				m_LoadingBarManager.ShowPlacement();
			}
			if( m_DataManager.PartIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "NoSolidObjectInFile", NO_SOLID_OBJECT_IN_FILE ), MyApp.NoticeType.Hint );
				End();
				return;
			}

			// add part success
			IsImportSuccess = true;

			// the tube direction of the machine decides how the ring shape is placed, fall back to Z when it is absent
			EAlignAxis targetAxis = EAlignAxis.Z;
			if( DataGettingHelper.GetMachineData( out FiveAxisMachineData machineData ) ) {
				targetAxis = RotaryAxisHelper.ConvertToAlignAxis( machineData.TubeDir );
			}

			ReportProgress( LoadingStep.PrepareWorkpiece );

			// the shape may not be a revolution one, then it is kept as it is
			bool isSuccess = RingShapedIdentifyHelper.ComputeRevolutionToG54Transform( fileShape, out gp_Trsf trsf, targetAxis, ReportProgress );
			if( isSuccess == false ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "RevolutionPlaceFailed", REVOLUTION_PLACE_FAILED ), MyApp.NoticeType.Warning );
				EndWithViewUpdate();
				return;
			}
			TransformHelper transformHelper = new TransformHelper( m_Viewer, m_DataManager, m_ViewManager, trsf );

			// do not refresh viewer right now
			ReportProgress( LoadingStep.ApplyPlacement );
			transformHelper.TransformData( false );

			// get transformed visible part shapes
			List<TopoDS_Shape> partShapes = GetVisiblePartShapes();
			if( partShapes.Count == 0 ) {
				EndWithViewUpdate();
				return;
			}

			ReportProgress( LoadingStep.PreparePaths );
			BoundingBox bbox = GetVisibleWorkpieceBBox();
			bool isGetSuccess = FindWireOnOuterShellHelper.TubeStrategies( bbox, targetAxis, out gp_Pnt bboxCenter, out List<gp_Ax1> stretchedStrategies );
			if( isGetSuccess == false ) {
				EndWithViewUpdate();
				return;
			}

			// use FindWireOnOuterShellHelper to find wires on outer shell
			FindWireOnOuterShellHelper.WireFindingResult result =
				FindWireOnOuterShellHelper.FindWiresOnOuterShell( partShapes, stretchedStrategies, bboxCenter,
					reportProgress: ReportProgress );

			if( !result.IsSuccess ) {
				string errorMessage = FindWireOnOuterShellHelper.GetWireFindingErrorMessage( result.Error );
				MyApp.Logger.ShowOnLogPanel( errorMessage, MyApp.NoticeType.Hint );
				EndWithViewUpdate();
				return;
			}

			// add paths to data manager
			ReportProgress( LoadingStep.CalculatePaths );
			m_DataManager.AddPath( result.Wires, result.EdgeFaceMap, false, reportProgress: ReportProgress );
			UpdateAllViewData();
			ReportProgress( LoadingStep.PathsComplete );

			// set import success
			IsImportSuccess = true;
			End();
		}

		public override void End()
		{
			CloseLoadingBar();
			ActionCompleted?.Invoke();
			base.End();
		}

		void CloseLoadingBar()
		{
			if( m_IsExternalLoading ) {
				return;
			}
			m_LoadingBarManager?.Dispose();
			m_LoadingBarManager = null;
		}

		void ReportProgress( LoadingStep step )
		{
			m_LoadingBarManager?.Report( step );
		}

		void ReportProgress( LoadingBarInfo progress )
		{
			m_LoadingBarManager?.Report( progress );
		}

		// the part is already in the data manager, refresh the viewer before leaving
		void EndWithViewUpdate()
		{
			UpdateAllViewData();
			End();
		}

		void UpdateAllViewData()
		{
			m_ViewManager.RebuildAllViews( m_DataManager, true, ReportProgress );
			m_Viewer.UpdateView();
		}

		List<TopoDS_Shape> GetVisiblePartShapes()
		{
			List<TopoDS_Shape> shapeList = new List<TopoDS_Shape>();
			foreach( string partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				if( DataGettingHelper.GetShapeObject( partID, out IShapeObject shapeObject ) == false ) {
					continue;
				}
				shapeList.Add( shapeObject.Shape );
			}
			return shapeList;
		}

		void Import3DFile( out TopoDS_Shape fileShape )
		{
			fileShape = null;

			OpenFileDialog openDialog = new OpenFileDialog();
			string filter = "STEP Files (*.stp;*.step)|*.stp;*.step|" +
							"IGES Files (*.igs;*.iges)|*.igs;*.iges";

			openDialog.Filter = filter;

			// show file dialog
			if( openDialog.ShowDialog( MyApp.MainForm ) != DialogResult.OK ) {
				return;
			}

			string szFileName = openDialog.FileName;
			if( string.IsNullOrEmpty( szFileName ) ) {
				return;
			}

			// get this file format
			FileFormat format = CadFileReader.GetFormat( szFileName );
			m_LoadingBarManager = new LoadingBarManager( MyApp.MainForm );
			m_LoadingBarManager.ShowImport( Path.GetFileName( szFileName ) );
			ReadFileData( format, szFileName, out fileShape );

			// store file name
			if( fileShape != null && fileShape.IsNull() == false ) {
				ImportedFileName = Path.GetFileName( szFileName );
			}
		}

		BoundingBox GetVisibleWorkpieceBBox()
		{
			List<TopoDS_Shape> shapeList = new List<TopoDS_Shape>();
			foreach( string partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				if( DataGettingHelper.GetShapeObject( partID, out IShapeObject shapeObject ) == false ) {
					continue;
				}
				shapeList.Add( shapeObject.Shape );
			}
			if( shapeList.Count == 0 ) {
				return null;
			}
			TopoDS_Shape compound = ShapeTool.MakeCompound( shapeList );
			return new BoundingBox( compound );
		}

		void ReadFileData( FileFormat format, string szFileName, out TopoDS_Shape oneShape )
		{
			oneShape = CadFileReader.Read( format, szFileName, ReportProgress );
			if( oneShape == null || oneShape.IsNull() ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "ImportFailed", IMPORT_FAILED ), MyApp.NoticeType.Error );
				return;
			}

			// add the read shape to the manager
			ReportProgress( LoadingStep.AddPart );
			m_DataManager.AddPart( oneShape );
		}

		Viewer m_Viewer;
		ViewManager m_ViewManager;

		// not null when the shape is supplied by the caller instead of read from a file
		TopoDS_Shape m_AddedShape;

		const string NO_SOLID_OBJECT_IN_FILE = "[操作提醒]圖檔內沒有實體物件";
		const string REVOLUTION_PLACE_FAILED = "[操作提醒]迴轉體放置失敗";
		const string IMPORT_FAILED = "匯入失敗";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "PlaceRingShapeAction" );
	}
}
