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

		// the shape is already added to the data manager by the caller, only place it and build the paths
		public PlaceRingShapeAction( DataManager dataManager, Viewer viewer, ViewManager viewManager, TopoDS_Shape addedShape )
			: this( dataManager, viewer, viewManager )
		{
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
			base.Start();

			// the shape comes from the caller, otherwise read it from a file
			TopoDS_Shape fileShape = m_AddedShape;
			if( fileShape == null ) {
				Import3DFile( out fileShape );
				if( fileShape == null ) {
					End();
					return;
				}
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

			// the shape may not be a revolution one, then it is kept as it is
			bool isSuccess = RingShapedIdentifyHelper.ComputeRevolutionToG54Transform( fileShape, out gp_Trsf trsf, targetAxis );
			if( isSuccess == false ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "RevolutionPlaceFailed", REVOLUTION_PLACE_FAILED ), MyApp.NoticeType.Warning );
				EndWithViewUpdate();
				return;
			}
			TransformHelper transformHelper = new TransformHelper( m_Viewer, m_DataManager, m_ViewManager, trsf );

			// do not refresh viewer right now
			transformHelper.TransformData( false );

			// get transformed visible part shapes
			List<TopoDS_Shape> partShapes = GetVisiblePartShapes();
			if( partShapes.Count == 0 ) {
				EndWithViewUpdate();
				return;
			}

			BoundingBox bbox = GetVisibleWorkpieceBBox();
			bool isGetSuccess = FindWireOnOuterShellHelper.TubeStrategies( bbox, targetAxis, out gp_Pnt bboxCenter, out List<gp_Ax1> stretchedStrategies );
			if( isGetSuccess == false ) {
				EndWithViewUpdate();
				return;
			}

			// use FindWireOnOuterShellHelper to find wires on outer shell
			FindWireOnOuterShellHelper.WireFindingResult result =
				FindWireOnOuterShellHelper.FindWiresOnOuterShell( partShapes, stretchedStrategies, bboxCenter );

			if( !result.IsSuccess ) {
				string errorMessage = FindWireOnOuterShellHelper.GetWireFindingErrorMessage( result.Error );
				MyApp.Logger.ShowOnLogPanel( errorMessage, MyApp.NoticeType.Hint );
				EndWithViewUpdate();
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

		// the part is already in the data manager, refresh the viewer before leaving
		void EndWithViewUpdate()
		{
			UpdateAllViewData();
			End();
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
			if( openDialog.ShowDialog( MyApp.MainForm ) != DialogResult.OK ) {
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
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "ImportFailed", IMPORT_FAILED ), MyApp.NoticeType.Error );
				return;
			}
			Reader.TransferRoots();

			// prevent from empty shape or null shape
			if( Reader.NbShapes() == 0 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "ImportFailed", IMPORT_FAILED ), MyApp.NoticeType.Error );
				return;
			}
			oneShape = Reader.OneShape();
			if( oneShape == null || oneShape.IsNull() ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "ImportFailed", IMPORT_FAILED ), MyApp.NoticeType.Error );
				return;
			}
			oneShape = ShapeTool.SewShape( new List<TopoDS_Shape>() { oneShape } );

			// add the read shape to the manager
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
