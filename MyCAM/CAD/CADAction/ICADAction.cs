using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.CAD
{
	public enum CADActionType
	{
		None = 0,
		Default = 1,
		AddPoint = 2,
	}

	internal interface ICADAction
	{
		CADActionType ActionType
		{
			get;
		}

		void Start();

		void End();
	}

	internal class CADACtionBase : ICADAction
	{
		public CADACtionBase( Viewer viewer, TreeView treeView,
			Dictionary<string, ViewObject> viewObjectMap, Dictionary<string, TreeNode> treeNodeMap,
			CADManager cadManager )
		{
			if( viewer == null || treeView == null
				|| viewObjectMap == null || treeNodeMap == null
				|| cadManager == null ) {
				throw new ArgumentNullException( "Action constructing argument null" );
			}
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewObjectMap = viewObjectMap;
			m_TreeNodeMap = treeNodeMap;
			m_CADManager = cadManager;
		}

		public virtual CADActionType ActionType
		{
			get
			{
				return CADActionType.None;
			}
		}

		public virtual void Start()
		{
			// Register events
			m_Viewer.MouseDown += ViewerMouseDown;
			m_Viewer.KeyDown += ViewerKeyDown;
			m_TreeView.AfterSelect += TreeViewAfterSelect;
			m_TreeView.KeyDown += TreeViewKeyDown;
		}

		public virtual void End()
		{
			// Unregister events
			m_Viewer.MouseDown -= ViewerMouseDown;
			m_Viewer.KeyDown -= ViewerKeyDown;
			m_TreeView.AfterSelect -= TreeViewAfterSelect;
			m_TreeView.KeyDown -= TreeViewKeyDown;
		}

		protected virtual void ViewerMouseDown( MouseEventArgs e )
		{
			// Default mouse down action
		}

		protected virtual void ViewerKeyDown( KeyEventArgs e )
		{
			// Default key down action
		}

		protected virtual void TreeViewAfterSelect( object sender, TreeViewEventArgs e )
		{
			// Default tree view after select action
		}

		protected virtual void TreeViewKeyDown( object sender, KeyEventArgs e )
		{
			// Default tree view key down action
		}

		protected Viewer m_Viewer;
		protected TreeView m_TreeView;
		protected Dictionary<string, ViewObject> m_ViewObjectMap;
		protected Dictionary<string, TreeNode> m_TreeNodeMap;
		protected CADManager m_CADManager;
	}

	internal class DefaultAction : CADACtionBase
	{
		public DefaultAction( Viewer viewer, TreeView treeView,
			CADManager cadManager, Dictionary<string, ViewObject> viewObjectMap,
			Dictionary<string, TreeNode> treeNodeMap )
			: base( viewer, treeView, viewObjectMap, treeNodeMap, cadManager )
		{
		}

		public override CADActionType ActionType
		{
			get
			{
				return CADActionType.Default;
			}
		}

		public override void Start()
		{
			base.Start();
			m_Viewer.GetAISContext().ClearSelected( false );
			m_Viewer.UpdateView();
			m_bSuppressTreeViewSync = false;
		}

		public override void End()
		{
			base.End();
			m_Viewer.GetAISContext().ClearSelected( false );
			m_Viewer.UpdateView();
			m_bSuppressTreeViewSync = false;
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			// select
			if( e.Button == MouseButtons.Left ) {
				m_Viewer.GetAISContext().SelectDetected();
				m_Viewer.UpdateView();
				SyncSelectionFromViewToTree();
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			OnKeyDown( e );
		}

		protected override void TreeViewAfterSelect( object sender, TreeViewEventArgs e )
		{
			foreach( TreeNode node in m_TreeView.Nodes ) {
				node.BackColor = System.Drawing.Color.White;
			}
			if( e.Node == null ) {
				return;
			}
			e.Node.BackColor = System.Drawing.Color.Cyan;
			SyncSelectionFromTreeToView();
		}

		protected override void TreeViewKeyDown( object sender, KeyEventArgs e )
		{
			OnKeyDown( e );
		}

		public void ChangeObjectVisibility()
		{
			TreeNode selectedNode = m_TreeView.SelectedNode;
			if( selectedNode == null || string.IsNullOrEmpty( selectedNode.Text ) ) {
				return;
			}
			string szUID = selectedNode.Text;
			ChangeObjectVisibility( szUID );
		}

		void OnKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Space ) {
				ChangeObjectVisibility();
			}
		}

		void ChangeObjectVisibility( string szUID )
		{
			// toggle the visibility of the selected object
			if( !m_ViewObjectMap.ContainsKey( szUID ) ) {
				return;
			}
			ViewObject viewObject = m_ViewObjectMap[ szUID ];
			if( viewObject == null || viewObject.AISHandle == null ) {
				return;
			}
			if( !m_TreeNodeMap.ContainsKey( szUID ) ) {
				return;
			}
			TreeNode node = m_TreeNodeMap[ szUID ];
			if( node == null ) {
				return;
			}

			// toggle visibility
			viewObject.Visible = !viewObject.Visible;
			if( viewObject.Visible ) {
				node.ForeColor = System.Drawing.Color.Black;
				m_Viewer.GetAISContext().Display( viewObject.AISHandle, true );
			}
			else {
				node.ForeColor = System.Drawing.Color.LightGray;
				m_Viewer.GetAISContext().Erase( viewObject.AISHandle, true );
			}
		}

		void SyncSelectionFromViewToTree()
		{
			// clear tree view selection
			m_TreeView.SelectedNode = null;
			foreach( TreeNode node in m_TreeView.Nodes ) {
				node.BackColor = System.Drawing.Color.White;
			}

			// get the selected shape
			m_Viewer.GetAISContext().InitSelected();
			if( !m_Viewer.GetAISContext().MoreSelected() ) {
				return;
			}
			TopoDS_Shape selectedShape = m_Viewer.GetAISContext().SelectedShape();
			if( selectedShape == null || selectedShape.IsNull() ) {
				return;
			}

			// find the corresponding UID
			string szUID = m_CADManager.GetUIDByShape( selectedShape );
			if( string.IsNullOrEmpty( szUID ) ) {
				return;
			}

			// find the node in the tree view
			if( !m_TreeNodeMap.ContainsKey( szUID ) ) {
				return;
			}
			m_bSuppressTreeViewSync = true;
			m_TreeView.SelectedNode = m_TreeNodeMap[ szUID ];
			m_bSuppressTreeViewSync = false;
		}

		void SyncSelectionFromTreeToView()
		{
			if( m_bSuppressTreeViewSync ) {
				return;
			}

			// clear viewer slection
			m_Viewer.GetAISContext().ClearSelected( false );

			// get the selected node
			TreeNode node = m_TreeView.SelectedNode;
			if( node == null || string.IsNullOrEmpty( node.Text ) ) {
				return;
			}
			string szUID = node.Text;

			// find the corresponding view object
			if( !m_ViewObjectMap.ContainsKey( szUID ) ) {
				return;
			}
			ViewObject viewObject = m_ViewObjectMap[ szUID ];
			if( viewObject == null || viewObject.AISHandle == null ) {
				return;
			}

			// select the shape in the viewer
			m_Viewer.GetAISContext().SetSelected( viewObject.AISHandle, true );
		}

		bool m_bSuppressTreeViewSync = false;
	}

	internal class AddPointAction : CADACtionBase
	{
		public AddPointAction( Viewer viewer, TreeView treeView,
			CADManager cadManager, Dictionary<string, ViewObject> viewObjectMap,
			Dictionary<string, TreeNode> treeNodeMap,
			AddPointType addPointType )
			: base( viewer, treeView, viewObjectMap, treeNodeMap, cadManager )
		{
			m_AddPointType = addPointType;
		}

		public override void Start()
		{
			base.Start();
			m_Viewer.GetAISContext().ClearSelected( false );
			m_Viewer.UpdateView();

			// disable tree view
			m_TreeView.Enabled = false;

			// activate edge slection mode
			foreach( ViewObject viewObject in m_ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Activate( viewObject.AISHandle, (int)AISActiveMode.Edge );
			}
		}

		public override void End()
		{
			base.End();
			m_Viewer.GetAISContext().ClearSelected( false );
			m_Viewer.UpdateView();

			// enable tree view
			m_TreeView.Enabled = true;

			// deactivate
			foreach( ViewObject viewObject in m_ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Deactivate();
			}
		}

		public override CADActionType ActionType
		{
			get
			{
				return CADActionType.AddPoint;
			}
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			if( e.Button != MouseButtons.Left ) {
				return;
			}

			// get selection edge
			m_Viewer.GetAISContext().SelectDetected();
			m_Viewer.GetAISContext().InitSelected();
			if( !m_Viewer.GetAISContext().MoreSelected() ) {
				return;
			}
			TopoDS_Shape selectedShape = m_Viewer.GetAISContext().SelectedShape();

			// validate the edge
			if( selectedShape == null || selectedShape.IsNull() ) {
				return;
			}
			if( selectedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_EDGE ) {
				return;
			}
			TopoDS_Edge edge = TopoDS.ToEdge( selectedShape );

			// add the point
			bool isAdded = false;
			if( m_AddPointType == AddPointType.CircArcCenter ) {
				isAdded = AddCircArcCenter( edge );
			}
			else if( m_AddPointType == AddPointType.EdgeMidPoint ) {
				isAdded = AddEdgeMidPoint( edge );
			}

			// end action if the point is added
			if( isAdded ) {
				End();
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		bool AddCircArcCenter( TopoDS_Edge edge )
		{
			bool isValidCircle = GeometryTool.IsCircularArc( edge, out gp_Pnt center, out _, out _ );
			if( !isValidCircle ) {
				MessageBox.Show( "Bad Arc" );
				return false;
			}
			AddToManager( center );
			return true;
		}

		bool AddEdgeMidPoint( TopoDS_Edge edge )
		{
			bool isValidEdge = GeometryTool.GetEdgeMidPoint( edge, out gp_Pnt midPoint );
			if( !isValidEdge ) {
				MessageBox.Show( "Bad Edge" );
				return false;
			}
			AddToManager( midPoint );
			return true;
		}

		void AddToManager( gp_Pnt pointToAdd )
		{
			// create the vertex to add
			BRepBuilderAPI_MakeVertex makeVertex = new BRepBuilderAPI_MakeVertex( pointToAdd );
			if( !makeVertex.IsDone() ) {
				return;
			}
			TopoDS_Vertex vertex = makeVertex.Vertex();
			m_CADManager.AddCADModel( vertex, "AddPoint" );
		}

		AddPointType m_AddPointType;
	}
}
