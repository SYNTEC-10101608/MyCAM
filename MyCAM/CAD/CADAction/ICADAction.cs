using OCC.TopoDS;
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
		public CADACtionBase( Viewer viewer, TreeView treeView, CADManager cadManager )
		{
			if( viewer == null || treeView == null || cadManager == null ) {
				throw new ArgumentNullException( "Viewer, TreeView and CADManager cannot be null." );
			}
			m_Viewer = viewer;
			m_TreeView = treeView;
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
		protected CADManager m_CADManager;
	}

	internal class CADActionDefault : CADACtionBase
	{
		public CADActionDefault( Viewer viewer, TreeView treeView, CADManager cadManager,
			Dictionary<string, ViewObject> viewObjectMap, Dictionary<string, TreeNode> treeNodeMap )
			: base( viewer, treeView, cadManager )
		{
			if( viewObjectMap == null || treeNodeMap == null ) {
				throw new ArgumentNullException( "ViewObjectMap and TreeNodeMap cannot be null." );
			}
			m_ViewObjectMap = viewObjectMap;
			m_TreeNodeMap = treeNodeMap;
		}

		public override CADActionType ActionType
		{
			get
			{
				return CADActionType.Default;
			}
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
			if( e.KeyCode == Keys.Space ) {
				ChangeObjectVisibility();
			}
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
			if( e.KeyCode == Keys.Space ) {
				ChangeObjectVisibility();
			}
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

		Dictionary<string, ViewObject> m_ViewObjectMap;
		Dictionary<string, TreeNode> m_TreeNodeMap;
		bool m_bSuppressTreeViewSync = false;
	}
}
