using OCC.TopoDS;
using OCCViewer;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.CAD
{
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

			// reset activation mode
			foreach( ViewObject viewObject in m_ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Activate( viewObject.AISHandle );
			}
			m_bSuppressTreeViewSync = false;
			SyncSelectionFromTreeToView();
		}

		public override void End()
		{
			// clear selection
			m_Viewer.GetAISContext().ClearSelected( false );
			m_Viewer.UpdateView();

			// reset activation mode
			foreach( ViewObject viewObject in m_ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Deactivate();
			}
			m_bSuppressTreeViewSync = false;
			base.End();
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
			// clear tree view selection, this set does not trigger the AfterSelect event
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
}
