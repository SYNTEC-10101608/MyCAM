using MyCAM.App;
using MyCAM.Data;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using MyCAM.LoadingManager;
using MyCAM.Helper.FileIO;

namespace MyCAM.Editor
{
	internal class CADEditor : EditorBase
	{
		public Action<EditActionType, EActionStatus> RaiseCADActionStatusChange;
		public Action<bool, string> RaiseFileImported;

		bool m_IsImporting = false;
		LoadingBarManager m_PartLoading;

		public CADEditor( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
			m_DataManager.PartChanged += OnPartChanged;
			m_DataManager.FeatureAdded += OnFeatureAdded;
			m_DataManager.PartRemoved += OnPartRemoved;

			// default action is select object action
			m_DefaultAction = new SelectPartAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
		}

		// editor
		public override EEditorType Type
		{
			get
			{
				return EEditorType.CAD;
			}
		}

		public override void EditStart()
		{
			base.EditStart();

			// init tree
			m_TreeView.Nodes.Add( m_ViewManager.PartNode );
			m_ViewManager.PartNode.ExpandAll();
		}

		public override void EditEnd()
		{
			// clear tree
			m_TreeView.Nodes.Clear();
			base.EditEnd();
		}

		// APIs
		public void Import3DFile()
		{
			if( m_IsImporting ) {
				return;
			}

			// stop current action
			EndActionIfNotDefault();
			OpenFileDialog openDialog = new OpenFileDialog();

			string filter = "STEP Files (*.stp;*.step)|*.stp;*.step|" +
										"IGES Files (*.igs;*.iges)|*.igs;*.iges|" +
										"DXF Files (*.dxf)|*.dxf|" +
										"All files (*.*)|*.*";
			openDialog.Filter = filter;

			// show file dialog
			if( openDialog.ShowDialog( MyApp.MainForm ) != DialogResult.OK )
				return;
			string szFileName = openDialog.FileName;
			if( string.IsNullOrEmpty( szFileName ) )
				return;

			// get this file format
			FileFormat format = CadFileReader.GetFormat( szFileName, allowDxf: true );

			if( format == FileFormat.DXF ) {
				GlassDXFImportAction action = new GlassDXFImportAction( m_DataManager, m_Viewer, m_ViewManager, szFileName );
				action.ActionCompleted += () =>
				{
					m_DefaultAction.ClearSelection();
					RaiseFileImported?.Invoke( action.IsImportSuccess, action.ImportedFileName );
				};
				StartImportAction( action );
			}
			else {
				ImportFile( format, szFileName );
			}
		}

		public void ImportProjectFile()
		{
			if( m_IsImporting ) {
				return;
			}

			// stop current action
			EndActionIfNotDefault();
			ReadProjectFileAction action = new ReadProjectFileAction( m_DataManager, m_Viewer, m_ViewManager );
			action.ActionCompleted += () =>
			{
				m_DefaultAction.ClearSelection();
				RaiseFileImported?.Invoke( action.IsImportSuccess, action.ImportedFileName );
			};
			StartImportAction( action );
		}

		public void ImportRingPart()
		{
			if( m_IsImporting ) {
				return;
			}

			// stop current action
			EndActionIfNotDefault();
			PlaceRingShapeAction action = new PlaceRingShapeAction( m_DataManager, m_Viewer, m_ViewManager );
			action.ActionCompleted += () =>
			{
				m_DefaultAction.ClearSelection();
				RaiseFileImported?.Invoke( action.IsImportSuccess, action.ImportedFileName );
			};
			StartImportAction( action );
		}

		public void SaveProjectFile()
		{
			if( m_IsImporting ) {
				return;
			}

			// stop current action
			EndActionIfNotDefault();
			SaveProjectFileAction action = new SaveProjectFileAction( m_DataManager );
			action.ActionCompleted += () =>
			{
				RaiseFileImported?.Invoke( action.IsSaveSuccess, action.SavedFileName );
			};
			StartEditAction( action );
		}

		public void SewPart()
		{
			if( m_CurrentAction.ActionType == EditActionType.SewPart ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeEdit( out List<string> szIDList ) ) {
				return;
			}
			SewPartAction action = new SewPartAction( m_DataManager, m_Viewer, m_ViewManager, szIDList );
			StartEditAction( action );
		}

		public void StartDesignSupportBracket()
		{
			if( m_CurrentAction.ActionType == EditActionType.DesignSupportBracket ) {
				m_CurrentAction.End();
				return;
			}

			if( m_DataManager.PartIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "NoPartData", NO_PART_DATA ), MyApp.NoticeType.Warning );
				return;
			}

			DesignSupportBracketAction action = new DesignSupportBracketAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			StartEditAction( action );
		}

		public void AddPoint( AddPointType type )
		{
			// user reclick same action enterace
			if( ( m_CurrentAction.ActionType == EditActionType.AddPoint_CircArcCenter && type == AddPointType.CircArcCenter ) ||
				( m_CurrentAction.ActionType == EditActionType.AddPoint_EdgeMidPoint && type == AddPointType.EdgeMidPoint ) ||
				( m_CurrentAction.ActionType == EditActionType.AddPoint_TwoVertexMidPoint && type == AddPointType.TwoVertexMidPoint ) ) {
				m_CurrentAction.End();
				return;
			}
			if( type == AddPointType.BoundingBoxCenter ) {
				EndActionIfNotDefault();
				AddBoundingBoxCenterPoint();
				return;
			}
			AddPointAction action = new AddPointAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, type );
			StartEditAction( action );
		}

		public void AddLine( AddLineType type )
		{
			if( m_CurrentAction.ActionType == EditActionType.AddLine ) {
				m_CurrentAction.End();
				return;
			}
			AddLineAction action = new AddLineAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, type );
			StartEditAction( action );
		}

		public void ThreePointTransform()
		{
			if( m_CurrentAction.ActionType == EditActionType.ThreePtTransform ) {
				m_CurrentAction.End();
				return;
			}
			ThreePtTransformAction action = new ThreePtTransformAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			StartEditAction( action );
		}

		public void StartManualTransform()
		{
			if( m_CurrentAction.ActionType == EditActionType.ManualTransform ) {
				m_CurrentAction.End();
				return;
			}

			ManualTransformAction action = new ManualTransformAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			StartEditAction( action );
		}

		public void StartAxisTransform()
		{
			if( m_CurrentAction.ActionType == EditActionType.AxisTransform ) {
				m_CurrentAction.End();
				return;
			}

			// need to use shape data to decide the cneter in the begin, so have to add this preotection
			if( m_DataManager.PartIDList.Count == 0 ) {
				return;
			}
			AxisTransformAction action = new AxisTransformAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			StartEditAction( action );
		}

		public void ApplyManualTransform( ETrsfConstraintType type )
		{
			if( m_CurrentAction.ActionType != EditActionType.ManualTransform ) {
				return;
			}
			( (ManualTransformAction)m_CurrentAction ).ApplyTransform( type );
		}

		public void StartMeasure()
		{
			if( m_CurrentAction.ActionType == EditActionType.Measure ) {
				m_CurrentAction.End();
				return;
			}

			MeasureAction action = new MeasureAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			StartEditAction( action );
		}

		public void StartWireDiscreteVertex()
		{
			if( m_CurrentAction.ActionType == EditActionType.WireDiscreteVertex ) {
				m_CurrentAction.End();
				return;
			}

			WireDiscreteVertexAction action = new WireDiscreteVertexAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			StartEditAction( action );
		}

		public void ImportTube( TopoDS_Shape tubeShape )
		{
			if( m_IsImporting ) {
				return;
			}
			m_IsImporting = true;
			LoadingBarManager loadingBarManager = new LoadingBarManager( Form.ActiveForm ?? MyApp.MainForm );
			try {
				// stop current action
				EndActionIfNotDefault();
				loadingBarManager.ShowTubeImport();
				tubeShape = ShapeTool.SewShape( new List<TopoDS_Shape>() { tubeShape } );
				loadingBarManager.Report( LoadingStep.AddPart );
				m_DataManager.AddPart( tubeShape );

				// Reuse placement and path building without submitting the part twice.
				PlaceRingShapeAction action = new PlaceRingShapeAction( m_DataManager, m_Viewer, m_ViewManager, tubeShape, loadingBarManager );
				action.ActionCompleted += () =>
				{
					loadingBarManager.Dispose();
					m_DefaultAction.ClearSelection();
					RaiseFileImported?.Invoke( action.IsImportSuccess, action.ImportedFileName );
				};
				StartEditAction( action );
			}
			finally {
				loadingBarManager.Dispose();
				m_IsImporting = false;
			}
		}

		// manager events
		void OnPartChanged()
		{
			m_PartLoading?.Report( LoadingStep.ClearDisplay );
			m_ViewManager.ClearAll();

			int completed = 0;
			m_PartLoading?.Report( new LoadingBarInfo( LoadingStep.DisplayParts, 0, m_DataManager.PartIDList.Count ) );
			foreach( var szNewDataID in m_DataManager.PartIDList ) {
				if( !DataGettingHelper.GetShapeObject( szNewDataID, out IShapeObject shapeObject ) ) {
					m_PartLoading?.Report( new LoadingBarInfo( LoadingStep.DisplayParts, ++completed, m_DataManager.PartIDList.Count ) );
					continue;
				}
				m_ViewManager.AddPart( szNewDataID, shapeObject.Shape );
				m_PartLoading?.Report( new LoadingBarInfo( LoadingStep.DisplayParts, ++completed, m_DataManager.PartIDList.Count ) );
			}

			// update tree view and viewer
			m_PartLoading?.Report( LoadingStep.UpdateDisplayTree );
			m_ViewManager.PartNode.ExpandAll();
			m_Viewer.UpdateView();
			m_DefaultAction.ClearSelection();
		}

		void OnFeatureAdded( List<string> newFeatureIDs, bool bIsShownAsFeature )
		{
			foreach( string szID in newFeatureIDs ) {
				if( string.IsNullOrEmpty( szID ) ) {
					return;
				}

				if( !DataGettingHelper.GetShapeObject( szID, out IShapeObject shapeObj ) ) {
					continue;
				}
				m_ViewManager.AddPart( szID, shapeObj.Shape, bIsShownAsFeature );
			}

			// update tree view and viewer
			m_ViewManager.PartNode.ExpandAll();
			m_Viewer.UpdateView();
		}

		void OnPartRemoved( List<string> removedPartIDs )
		{
			foreach( string szID in removedPartIDs ) {
				if( string.IsNullOrEmpty( szID ) ) {
					continue;
				}
				m_ViewManager.RemovePart( szID );
			}

			// removed part may be under selection
			m_DefaultAction.ClearSelection();
			m_Viewer.UpdateView();
		}

		// private methods
		void StartImportAction( IEditorAction action )
		{
			if( m_IsImporting ) {
				return;
			}
			m_IsImporting = true;
			try {
				StartEditAction( action );
			}
			finally {
				m_IsImporting = false;
			}
		}

		void ImportFile( FileFormat format, string szFileName )
		{
			if( m_IsImporting ) {
				return;
			}
			m_IsImporting = true;
			string szJustFileName = Path.GetFileName( szFileName );
			LoadingBarManager loadingBarManager = new LoadingBarManager( MyApp.MainForm );
			bool bIsAdded = false;
			try {
				loadingBarManager.ShowImport( szJustFileName, LoadingWorkflow.ModelImport );
				TopoDS_Shape oneShape = CadFileReader.Read( format, szFileName, loadingBarManager.Report );
				if( oneShape == null ) {
					MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "ImportFailed", IMPORT_FAILED ), MyApp.NoticeType.Error );
					return;
				}

				loadingBarManager.Report( LoadingStep.AddPart );
				m_PartLoading = loadingBarManager;
				m_DataManager.AddPart( oneShape );
				bIsAdded = true;
				loadingBarManager.Report( LoadingStep.ImportComplete );
			}
			catch( Exception ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "ImportFailed", IMPORT_FAILED ), MyApp.NoticeType.Error );
			}
			finally {
				m_PartLoading = null;
				loadingBarManager.Dispose();
				m_IsImporting = false;
			}

			if( bIsAdded ) {
				RaiseFileImported?.Invoke( true, szJustFileName );
			}
		}

		List<string> GetSelectedIDList()
		{
			return m_DefaultAction.GetSelectedIDs();
		}

		bool ValidateBeforeEdit( out List<string> szIDList )
		{
			EndActionIfNotDefault();
			szIDList = GetSelectedIDList();

			// nothing selected
			if( szIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "PleaseSelectPart", PLEASE_SELECT_PART ), MyApp.NoticeType.Hint );
				return false;
			}
			return true;
		}

		void AddBoundingBoxCenterPoint()
		{
			// Need at least one part
			if( m_DataManager.PartIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "NoPartData", NO_PART_DATA ), MyApp.NoticeType.Warning );
				return;
			}

			// Collect raw part shapes
			List<TopoDS_Shape> shapeList = new List<TopoDS_Shape>();
			foreach( var partID in m_DataManager.PartIDList ) {
				// Exclude part been produced by user
				if( partID.StartsWith( "Ref_" ) ) {
					continue;
				}
				if( DataGettingHelper.GetShapeObject( partID, out IShapeObject shapeObject ) == false ) {
					continue;
				}
				shapeList.Add( shapeObject.Shape );
			}
			if( shapeList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "NoPartData", NO_PART_DATA ), MyApp.NoticeType.Warning );
				return;
			}

			// Make compound and get bounding box center
			TopoDS_Shape compound = ShapeTool.MakeCompound( shapeList );
			BoundingBox bbox = new BoundingBox( compound );
			gp_Pnt center = new gp_Pnt( bbox.XCenter, bbox.YCenter, bbox.ZCenter );

			// Create vertex and add to manager
			BRepBuilderAPI_MakeVertex makeVertex = new BRepBuilderAPI_MakeVertex( center );
			if( !makeVertex.IsDone() ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CreatePointFailed", CREATE_POINT_FAILED ), MyApp.NoticeType.Warning );
				return;
			}
			m_DataManager.AddReferenceFeature( makeVertex.Vertex() );

			// Align bounding box center to G54 origin (0,0,0)
			gp_Trsf trsf = new gp_Trsf();
			trsf.SetTranslation( center, new gp_Pnt( 0, 0, 0 ) );
			TransformHelper transformHelper = new TransformHelper( m_Viewer, m_DataManager, m_ViewManager, trsf );
			transformHelper.TransformData();
		}

		// edit actions
		protected override void OnEditActionStart( IEditorAction action )
		{
			base.OnEditActionStart( action );
			RaiseCADActionStatusChange?.Invoke( action.ActionType, EActionStatus.Start );
		}

		protected override void OnEditActionEnd( IEditorAction action )
		{
			RaiseCADActionStatusChange?.Invoke( action.ActionType, EActionStatus.End );
			base.OnEditActionEnd( action );
		}

		const string CREATE_POINT_FAILED = "建立點位失敗";
		const string IMPORT_FAILED = "匯入失敗";
		const string PLEASE_SELECT_PART = "[操作提醒]請先選擇零件";
		const string NO_PART_DATA = "無零件資料";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "CADEditor" );
	}
}
