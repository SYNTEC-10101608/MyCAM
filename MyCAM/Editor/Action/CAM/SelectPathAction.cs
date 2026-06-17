using MyCAM.Data;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class SelectPathAction : SelectObjectAction
	{
		public Action RemovePath;

		public SelectPathAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
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

		public void ExcludeFromSelection( string pathID )
		{
			if( string.IsNullOrEmpty( pathID ) ) {
				return;
			}
			if( m_ExcludedIDSet == null ) {
				m_ExcludedIDSet = new HashSet<string>();
			}
			m_ExcludedIDSet.Add( pathID );
			m_SelectedIDSet.Remove( pathID );

			// deactivate excluded path on viewer
			m_ViewManager.DeactivePath( pathID );
		}

		public void RestoreFromExclusion( string pathID )
		{
			if( string.IsNullOrEmpty( pathID ) || m_ExcludedIDSet == null ) {
				return;
			}
			m_ExcludedIDSet.Remove( pathID );
			m_SelectedIDSet.Add( pathID );

			// re-activate restored path on viewer
			m_ViewManager.ActivePath( pathID );
		}

		protected override void ActivateObject()
		{
			m_ViewManager.ActivePaths( m_DataManager.PathIDList );
		}

		protected override void DeactivateObject()
		{
			m_ViewManager.DeactivePaths( m_DataManager.PathIDList );
		}

		protected override void OnKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Delete ) {
				RemovePath?.Invoke();
			}
		}

		protected override void SyncSelectionFromTree()
		{
			if( m_bSuppressTreeViewSync ) {
				return;
			}
			m_SelectedIDSet.Clear();

			// get the selected ID
			foreach( TreeNode node in ( m_TreeView as MultiSelectTreeView ).SelectedNodes ) {
				if( node == null || node == m_ViewManager.PathNode ) {
					continue;
				}

				// use Tag for ID mapping to avoid dependency on display text
				string nodeID = node.Tag as string;
				if( string.IsNullOrEmpty( nodeID ) ) {
					continue;
				}
				if( int.TryParse( nodeID.Substring( CAMEditor.PATH_NODE_PERFIX_LENGTH ), out int index ) ) {

					// check index, note that index is 1 based
					if( index < 1 || index > m_DataManager.PathIDList.Count ) {
						continue;
					}
					string szPathID = m_DataManager.PathIDList[ index - 1 ];
					if( m_ExcludedIDSet != null && m_ExcludedIDSet.Contains( szPathID ) ) {
						continue;
					}
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

		HashSet<string> m_ExcludedIDSet;
	}
}
