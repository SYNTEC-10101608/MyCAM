using MyCAM.App;
using MyCAM.Data;
using MyCAM.Helper;
using OCC.gp;
using OCC.IFSelect;
using OCC.IGESControl;
using OCC.STEPControl;
using OCC.TopoDS;
using OCC.XSControl;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class PlaceRingShapeAction : EditActionBase
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

		public override EditActionType ActionType
		{
			get { return EditActionType.PlaceRingShape; }
		}

		public override void Start()
		{
			base.Start();

			Import3DFile( out TopoDS_Shape fileShape );
			if( fileShape == null ) {
				End();
				return;
			}
			if( m_DataManager.PartIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]圖檔內沒有實體物件", MyApp.NoticeType.Hint );
				End();
				return;
			}
			bool isSuccess = RingShapedIdentifyHelper.ComputeRevolutionToG54Transform( fileShape, out gp_Trsf trsf );
			if( isSuccess == false ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]迴轉體放置失敗", MyApp.NoticeType.Warning );
				End();
				return;
			}
			TransformHelper transformHelper = new TransformHelper( m_Viewer, m_DataManager, m_ViewManager, trsf );

			// do not refresh viewer right now
			transformHelper.TransformData( false );

			// get transformedvisible part shapes
			List<TopoDS_Shape> partShapes = GetVisiblePartShapes();
			if( partShapes.Count == 0 ) {
				End();
				return;
			}

			BoundingBox bbox = GetVisibleWorkpieceBBox();
			bool isGetSuccess = FindWireOnOuterShellHelper.TubeStrategies( bbox, out gp_Pnt bboxCenter, out List<gp_Ax1> stretchedStrategies );
			if( isGetSuccess == false ) {
				End();
				return;
			}

			// use FindWireOnOuterShellHelper to find wires on outer shell
			FindWireOnOuterShellHelper.WireFindingResult result =
				FindWireOnOuterShellHelper.FindWiresOnOuterShell( partShapes, stretchedStrategies, bboxCenter );

			if( !result.IsSuccess ) {
				string errorMessage = FindWireOnOuterShellHelper.GetWireFindingErrorMessage( result.Error );
				MyApp.Logger.ShowOnLogPanel( errorMessage, MyApp.NoticeType.Hint );
				End();
				return;
			}

			// add paths to data manager
			m_DataManager.AddPath( result.Wires, result.EdgeFaceMap, false );
			UpdateAllViewData();

			// set import success
			IsImportSuccess = true;
			End();
		}

		public override void End()
		{
			ActionCompleted?.Invoke();
			base.End();
		}

		void UpdateAllViewData()
		{
			m_ViewManager.RebuildAllViews( m_DataManager, true );
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
			if( openDialog.ShowDialog() != DialogResult.OK ) {
				return;
			}

			string szFileName = openDialog.FileName;
			if( string.IsNullOrEmpty( szFileName ) ) {
				return;
			}

			// get this file format
			string szFileExtension = Path.GetExtension( szFileName ).ToLowerInvariant();
			FileFormat format = FileFormat.STEP;
			if( szFileExtension == ".igs" || szFileExtension == ".iges" ) {
				format = FileFormat.IGES;
			}
			if( szFileExtension == ".stp" || szFileExtension == ".step" ) {
				format = FileFormat.STEP;
			}
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
			oneShape = null;

			// read the file
			XSControl_Reader Reader;
			switch( format ) {
				case FileFormat.BREP:
					Reader = new XSControl_Reader();
					break;
				case FileFormat.STEP:
					Reader = new STEPControl_Reader();
					break;
				case FileFormat.IGES:
					Reader = new IGESControl_Reader();
					break;
				default:
					Reader = new XSControl_Reader();
					break;
			}
			IFSelect_ReturnStatus status = Reader.ReadFile( szFileName );

			// check the status
			if( status != IFSelect_ReturnStatus.IFSelect_RetDone ) {
				MyApp.Logger.ShowOnLogPanel( "匯入失敗", MyApp.NoticeType.Error );
				return;
			}
			Reader.TransferRoots();

			// prevent from empty shape or null shape
			if( Reader.NbShapes() == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "匯入失敗", MyApp.NoticeType.Error );
				return;
			}
			oneShape = Reader.OneShape();
			if( oneShape == null || oneShape.IsNull() ) {
				MyApp.Logger.ShowOnLogPanel( "匯入失敗", MyApp.NoticeType.Error );
				return;
			}
			oneShape = ShapeTool.SewShape( new List<TopoDS_Shape>() { oneShape } );

			// add the read shape to the manager
			m_DataManager.AddPart( oneShape );
		}

		Viewer m_Viewer;
		ViewManager m_ViewManager;
	}
}
