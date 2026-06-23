using MyCAM.App;
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

		// single select mode: only allow one path to be selected at a time
		public bool isSingleSelectMode
		{
			get { return m_SingleSelectMode; }
			set
			{
				m_SingleSelectMode = value;
				// sync TreeView's multi-select permission
				if( m_TreeView is MultiSelectTreeView multiTree ) {
					multiTree.isAllowMultiSelect = !value;
				}
			}
		}

		public SelectPathAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
		}

		public void EnterSingleSelectMode()
		{
			if( m_SingleSelectMode ) {
				// already in single select mode, no action needed
				return;
			}

			// clear any existing multi-selection before switching mode
			ClearSelection();

			// switch to single select mode
			isSingleSelectMode = true;
		}

		public void ExitSingleSelectMode()
		{
			if( !m_SingleSelectMode ) {
				// already in multi-select mode, no action needed
				return;
			}

			// switch back to multi-select mode
			isSingleSelectMode = false;
		}

		public void SelectPathByID( string pathID )
		{
			if( string.IsNullOrEmpty( pathID ) ) {
				return;
			}

			// enforce single select: clear previous selection first
			if( isSingleSelectMode ) {
				m_SelectedIDSet.Clear();
			}

			if( !m_SelectedIDSet.Contains( pathID ) ) {
				m_SelectedIDSet.Add( pathID );
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

			// enforce single select: don't auto-add to selection
			if( !isSingleSelectMode ) {
				m_SelectedIDSet.Add( pathID );
			}

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

		// block Ctrl+A / Ctrl+R in single select mode
		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( isSingleSelectMode ) {
				// only forward non-select-all key events
				OnKeyDown( e );
				return;
			}
			base.ViewerKeyDown( e );
		}

		// block rubber band drag in single select mode
		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			if( isSingleSelectMode ) {
				// do not setup rubber band, just record position for click detection
				return;
			}
			base.ViewerMouseDown( e );
		}

		protected override void ViewerMouseMove( MouseEventArgs e )
		{
			if( isSingleSelectMode ) {
				// do not draw rubber band
				return;
			}
			base.ViewerMouseMove( e );
		}

		protected override void ViewerMouseUp( MouseEventArgs e )
		{
			if( isSingleSelectMode ) {
				// do not perform rectangle selection
				return;
			}
			base.ViewerMouseUp( e );
		}

		// single click: always replace selection (ignore Ctrl for XOR)
		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			if( isSingleSelectMode ) {
				if( e.Button == MouseButtons.Left ) {
					// always single replace select, ignore Ctrl modifier
					m_Viewer.Select();
					SyncSelectionFromView();
				}
				return;
			}
			base.ViewerMouseClick( e );
		}

		protected override void SyncSelectionFromView()
		{
			base.SyncSelectionFromView();

			if( isSingleSelectMode && m_SelectedIDSet.Count > 1 ) {
				MyApp.Logger.ShowOnLogPanel(
					string.Format( "[選取異常] 路徑單選模式下偵測到多重選取 ({0} 個項目)，已自動清除", m_SelectedIDSet.Count ),
					MyApp.NoticeType.Hint );

				// Clear all selections to restore valid state
				m_SelectedIDSet.Clear();
				SyncSelectionFromSet();
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
				if( int.TryParse( nodeID.Substring( CAMEditor.PATH_NODE_PREFIX_LENGTH ), out int index ) ) {

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

			if( isSingleSelectMode && m_SelectedIDSet.Count > 1 ) {
				MyApp.Logger.ShowOnLogPanel(
					string.Format( "[選取異常] 路徑單選模式下從路徑樹偵測到多重選取 ({0} 個項目)，已自動清除", m_SelectedIDSet.Count ),
					MyApp.NoticeType.Hint );

				// Clear all selections to restore valid state
				m_SelectedIDSet.Clear();
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

		bool m_SingleSelectMode = false;
		HashSet<string> m_ExcludedIDSet;
	}
}
