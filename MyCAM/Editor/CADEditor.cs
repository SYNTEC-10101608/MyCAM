using MyCAM.App;
using MyCAM.Data;
using OCC.AIS;
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
	internal enum FileFormat
	{
		BREP = 0,
		STEP = 1,
		IGES = 2
	}

	internal class CADEditor : EditorBase
	{
		public Action<EditActionType, EActionStatus> RaiseCADActionStatusChange;

		public CADEditor( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
			m_DataManager.PartChanged += OnPartChanged;
			m_DataManager.FeatureAdded += OnFeatureAdded;

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
			// stop current action
			EndActionIfNotDefault();
			OpenFileDialog openDialog = new OpenFileDialog();
			string filter = "STEP Files (*.stp;*.step)|*.stp;*.step|" +
							"IGES Files (*.igs;*.iges)|*.igs;*.iges|" +
							"All files (*.*)|*.*";
			openDialog.Filter = filter;

			// show file dialog
			if( openDialog.ShowDialog() != DialogResult.OK )
				return;
			string szFileName = openDialog.FileName;
			if( string.IsNullOrEmpty( szFileName ) )
				return;

			// get this file format
			string szFileExtension = Path.GetExtension( szFileName ).ToLowerInvariant();
			FileFormat format = FileFormat.STEP;
			if( szFileExtension == ".igs" || szFileExtension == ".iges" ) {
				format = FileFormat.IGES;
			}
			if( szFileExtension == ".stp" || szFileExtension == ".step" ) {
				format = FileFormat.STEP;
			}
			ReadFileData( format, szFileName );
		}

		public void ImportProjectFile()
		{
			// stop current action
			EndActionIfNotDefault();
			ReadProjectFileAction action = new ReadProjectFileAction( m_DataManager, m_Viewer, m_ViewManager );
			StartEditAction( action );
		}

		public void SaveProjectFile()
		{
			// stop current action
			EndActionIfNotDefault();
			SaveProjectFileAction action = new SaveProjectFileAction( m_DataManager );
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

		public void AddPoint( AddPointType type )
		{
			// user reclick same action enterace
			if( ( m_CurrentAction.ActionType == EditActionType.AddPoint_CircArcCenter && type == AddPointType.CircArcCenter ) ||
				( m_CurrentAction.ActionType == EditActionType.AddPoint_EdgeMidPoint && type == AddPointType.EdgeMidPoint ) ||
				( m_CurrentAction.ActionType == EditActionType.AddPoint_TwoVertexMidPoint && type == AddPointType.TwoVertexMidPoint ) ) {
				m_CurrentAction.End();
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

		public void StartManaulTransform()
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

		// manager events
		void OnPartChanged()
		{
			// clear the tree view and viewer
			m_ViewManager.PartNode.Nodes.Clear();
			m_ViewManager.PathNode.Nodes.Clear();
			foreach( ViewObject viewObject in m_ViewManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Remove( viewObject.AISHandle, false );
			}

			// update view manager data
			m_ViewManager.ViewObjectMap.Clear();
			m_ViewManager.TreeNodeMap.Clear();
			foreach( var szNewDataID in m_DataManager.PartIDList ) {
				PartObject data = (PartObject)m_DataManager.ObjectMap[ szNewDataID ];

				// add node to the tree view
				TreeNode node = new TreeNode( data.UID );
				m_ViewManager.PartNode.Nodes.Add( node );
				m_ViewManager.TreeNodeMap.Add( data.UID, node );

				// add shape to the viewer
				if( !DataGettingHelper.GetShapeObject( szNewDataID, out IShapeObject shapeObject ) ) {
					continue;
				}
				AIS_Shape aisShape = ViewHelper.CreatePathAIS( shapeObject.Shape );
				m_ViewManager.ViewObjectMap.Add( data.UID, new ViewObject( aisShape ) );
				m_Viewer.GetAISContext().Display( aisShape, false ); // this will also activate
			}

			// update tree view and viewer
			m_ViewManager.PartNode.ExpandAll();
			m_Viewer.UpdateView();
			m_DefaultAction.ClearSelection();
		}

		void OnFeatureAdded( List<string> newFeatureIDs )
		{
			foreach( string szID in newFeatureIDs ) {
				if( string.IsNullOrEmpty( szID ) ) {
					return;
				}

				// add a new node to the tree view
				TreeNode node = new TreeNode( szID );
				m_ViewManager.PartNode.Nodes.Add( node );
				m_ViewManager.TreeNodeMap.Add( szID, node );

				// add a new shape to the viewer
				if( !DataGettingHelper.GetShapeObject( szID, out IShapeObject shapeObj ) ) {
					continue;
				}
				AIS_Shape aisShape = ViewHelper.CreateFeatureAIS( shapeObj.Shape );
				m_ViewManager.ViewObjectMap.Add( szID, new ViewObject( aisShape ) );
				m_Viewer.GetAISContext().Display( aisShape, false ); // this will also activate
			}

			// update tree view and viewer
			m_ViewManager.PartNode.ExpandAll();
			m_Viewer.UpdateView();
		}

		// private methods
		void ReadFileData( FileFormat format, string szFileName )
		{
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
			TopoDS_Shape oneShape = Reader.OneShape();
			if( oneShape == null || oneShape.IsNull() ) {
				MyApp.Logger.ShowOnLogPanel( "匯入失敗", MyApp.NoticeType.Error );
				return;
			}
			oneShape = ShapeTool.SewShape( new List<TopoDS_Shape>() { oneShape } );

			// add the read shape to the manager
			m_DataManager.AddPart( oneShape );
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
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先選擇零件", MyApp.NoticeType.Hint );
				return false;
			}
			return true;
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
	}
}
