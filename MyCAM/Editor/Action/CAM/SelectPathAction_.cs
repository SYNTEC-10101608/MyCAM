using MyCAM.Data;
using OCCViewer;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class SelectPathAction_ : SelectObjectAction
	{
		public Action RemovePath;
		public Action<bool> PathOrderMove;

		public SelectPathAction_( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
		}

		public void SelectPathByID( string pathID )
		{
			if( string.IsNullOrEmpty( pathID ) ) {
				return;
			}

			// Add the path ID to the selected set
			if( !m_SelectedIDSet.Contains( pathID ) ) {
				m_SelectedIDSet.Add( pathID );

				// Sync the selection to both tree view and viewer
				SyncSelectionFromSet();
			}
		}
		protected override void ActivateObject()
		{
			foreach( var pathID in m_DataManager.PathIDList ) {
				m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ pathID ].AISHandle );
			}
		}

		protected override void DeactivateObject()
		{
			foreach( var pathID in m_DataManager.PathIDList ) {
				m_Viewer.GetAISContext().Deactivate( m_ViewManager.ViewObjectMap[ pathID ].AISHandle );
			}
		}

		protected override void OnKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Delete ) {
				RemovePath?.Invoke();
			}
			base.OnKeyDown( e );
		}

		protected override void SyncSelectionFromTree()
		{
			if( m_bSuppressTreeViewSync ) {
				return;
			}
			m_SelectedIDSet.Clear();

			// get the selected ID
			foreach( TreeNode node in ( m_TreeView as MultiSelectTreeView ).SelectedNodes ) {
				if( node == null || string.IsNullOrEmpty( node.Text ) || node == m_ViewManager.PathNode ) {
					continue;
				}

				// the node text is "Path_xxx", xxx is the order of path in pathID list
				if( int.TryParse( node.Text.Substring( CAMEditor.PATH_NODE_PERFIX_LENGTH ), out int index ) ) {

					// check index, note that index is 1 based
					if( index < 1 || index > m_DataManager.PathIDList.Count ) {
						continue;
					}
					string szPathID = m_DataManager.PathIDList[ index - 1 ];
					m_SelectedIDSet.Add( szPathID );
				}
			}
			SyncSelectionFromSet();
		}

		protected override void SyncSelectionFromSet()
		{
			// sync to tree
			m_bSuppressTreeViewSync = true;
			( m_TreeView as MultiSelectTreeView ).ClearSelection();
			foreach( string szUID in m_SelectedIDSet ) {

				// convert path ID to order
				int index = m_DataManager.PathIDList.IndexOf( szUID );
				if( index == -1 ) {
					continue;
				}
				string szNodeID = CAMEditor.PATH_NODE_PREFIX + ( index + 1 ).ToString();
				if( !m_ViewManager.TreeNodeMap.ContainsKey( szNodeID ) ) {
					continue;
				}
				( m_TreeView as MultiSelectTreeView ).SelectNode( m_ViewManager.TreeNodeMap[ szNodeID ] );
			}
			m_bSuppressTreeViewSync = false;

			// sync to view
			m_Viewer.GetAISContext().ClearSelected( false );
			foreach( string szUID in m_SelectedIDSet ) {
				if( !m_ViewManager.ViewObjectMap.ContainsKey( szUID ) ) {
					continue;
				}
				ViewObject viewObject = m_ViewManager.ViewObjectMap[ szUID ];
				if( viewObject == null || viewObject.AISHandle == null ) {
					continue;
				}
				m_Viewer.GetAISContext().AddOrRemoveSelected( viewObject.AISHandle, false );
			}
			m_Viewer.UpdateView();
			SelectionChange?.Invoke();
		}
	}
}
