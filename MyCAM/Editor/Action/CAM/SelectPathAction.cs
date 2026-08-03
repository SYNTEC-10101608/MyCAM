using MyCAM.App;
using MyCAM.Data;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
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

		public void SelectPathByIDs( List<string> pathIDList )
		{
			if( pathIDList == null || pathIDList.Count == 0 ) {
				return;
			}
			// enforce single select: only select the first ID
			if( isSingleSelectMode ) {
				if( pathIDList.Count > 1 ) {
					MyApp.Logger.ShowOnLogPanel(
						string.Format( m_LanguageManager.TryGetString( "Log_MultiSelectError_View", LOG_MULTI_SELECT_ERROR_VIEW ), m_SelectedIDSet.Count ),
						MyApp.NoticeType.Hint );

					// Clear all selections to restore valid state
					m_SelectedIDSet.Clear();
					SyncSelectionFromSet();
					return;
				}
				SelectPathByID( pathIDList.First() );
				return;
			}

			foreach( string pathID in pathIDList ) {
				if( !string.IsNullOrEmpty( pathID ) && !m_SelectedIDSet.Contains( pathID ) ) {
					m_SelectedIDSet.Add( pathID );
				}
			}
			SyncSelectionFromSet();
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

		// control whether standard pattern paths are selectable in this action
		public bool CanSelectStdPattern
		{
			get { return m_CanSelectStdPattern; }
			set
			{
				if( m_CanSelectStdPattern == value ) {
					return;
				}
				m_CanSelectStdPattern = value;
				if( value ) {
					ShowStdPathAndNode();
				}
				else {
					HideStdPathAndNode();
				}
			}
		}

		public static bool IsStdPatternPath( string pathID )
		{
			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) ) {
				return false;
			}
			return DataGettingHelper.IsStdPattern( pathType );
		}

		// control whether special-tool-vector paths are selectable in this action
		public bool CanSelectSpecialPath
		{
			get { return m_CanSelectSpecialPath; }
			set
			{
				if( m_CanSelectSpecialPath == value ) {
					return;
				}
				m_CanSelectSpecialPath = value;
				if( value ) {
					ShowSpecialPathAndNode();
				}
				else {
					HideSpecialPathAndNode();
				}
			}
		}

		public static bool IsSpecialPath( string pathID )
		{
			if( string.IsNullOrEmpty( pathID ) ) {
				return false;
			}
			if( !DataGettingHelper.GetGeomDataByID( pathID, out IGeomData geomData ) ) {
				return false;
			}
			if( geomData.PathType != PathType.Contour ) {
				return false;
			}
			if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
				return false;
			}
			SpecialToolVecData specialToolVecData = craftData.SpecialToolVecData;
			if( specialToolVecData == null ) {
				return false;
			}
			return specialToolVecData.Type != ESpecialToolVecType.Normal;
		}

		public override void End()
		{
			// force restore std pattern paths/nodes in case caller forgot
			CanSelectStdPattern = true;
			CanSelectSpecialPath = true;
			base.End();
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
					string.Format( m_LanguageManager.TryGetString( "Log_MultiSelectError_View", LOG_MULTI_SELECT_ERROR_VIEW ), m_SelectedIDSet.Count ),
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
					string.Format( m_LanguageManager.TryGetString( "Log_MultiSelectError_Tree", LOG_MULTI_SELECT_ERROR_TREE ), m_SelectedIDSet.Count ),
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

		void HideStdPathAndNode()
		{
			if( m_HiddenStdPatternNodes == null ) {
				m_HiddenStdPatternNodes = new Dictionary<string, (TreeNode, int)>();
			}
			m_HiddenStdPatternNodes.Clear();
			if( m_DataManager?.PathIDList == null ) {
				return;
			}
			foreach( string pathID in m_DataManager.PathIDList ) {
				if( !IsStdPatternPath( pathID ) ) {
					continue;
				}
				m_ViewManager.ErasePath( pathID );
				if( m_ViewManager.TreeNodeMap.TryGetValue( pathID, out TreeNode node ) ) {
					int index = m_ViewManager.PathNode.Nodes.IndexOf( node );
					m_ViewManager.PathNode.Nodes.Remove( node );
					m_HiddenStdPatternNodes[ pathID ] = (node, index);
				}
			}
			m_Viewer.UpdateView();
		}

		void ShowStdPathAndNode()
		{
			if( m_DataManager?.PathIDList == null || m_HiddenStdPatternNodes == null || m_HiddenStdPatternNodes.Count == 0 ) {
				return;
			}
			foreach( var kvp in m_HiddenStdPatternNodes ) {
				TreeNode node = kvp.Value.Node;
				int index = kvp.Value.Index;
				int insertAt = Math.Min( index, m_ViewManager.PathNode.Nodes.Count );
				m_ViewManager.PathNode.Nodes.Insert( insertAt, node );
				m_ViewManager.DisplayPath( kvp.Key );
			}
			m_HiddenStdPatternNodes.Clear();
			m_Viewer.UpdateView();
		}

		void HideSpecialPathAndNode()
		{
			if( m_HiddenSpecialPathNodes == null ) {
				m_HiddenSpecialPathNodes = new Dictionary<string, (TreeNode, int)>();
			}
			m_HiddenSpecialPathNodes.Clear();
			if( m_DataManager?.PathIDList == null ) {
				return;
			}
			foreach( string pathID in m_DataManager.PathIDList ) {
				if( !IsSpecialPath( pathID ) ) {
					continue;
				}
				m_ViewManager.ErasePath( pathID );
				if( m_ViewManager.TreeNodeMap.TryGetValue( pathID, out TreeNode node ) ) {
					int index = m_ViewManager.PathNode.Nodes.IndexOf( node );
					m_ViewManager.PathNode.Nodes.Remove( node );
					m_HiddenSpecialPathNodes[ pathID ] = (node, index);
				}
			}
			m_Viewer.UpdateView();
		}

		void ShowSpecialPathAndNode()
		{
			if( m_DataManager?.PathIDList == null || m_HiddenSpecialPathNodes == null || m_HiddenSpecialPathNodes.Count == 0 ) {
				return;
			}
			foreach( var kvp in m_HiddenSpecialPathNodes ) {
				TreeNode node = kvp.Value.Node;
				int index = kvp.Value.Index;
				int insertAt = Math.Min( index, m_ViewManager.PathNode.Nodes.Count );
				m_ViewManager.PathNode.Nodes.Insert( insertAt, node );
				m_ViewManager.DisplayPath( kvp.Key );
			}
			m_HiddenSpecialPathNodes.Clear();
			m_Viewer.UpdateView();
		}

		bool m_SingleSelectMode = false;
		bool m_CanSelectStdPattern = true;
		bool m_CanSelectSpecialPath = true;
		Dictionary<string, (TreeNode Node, int Index)> m_HiddenStdPatternNodes = new Dictionary<string, (TreeNode, int)>();
		Dictionary<string, (TreeNode Node, int Index)> m_HiddenSpecialPathNodes = new Dictionary<string, (TreeNode, int)>();
		HashSet<string> m_ExcludedIDSet;
		LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "SelectPathAction" );
		const string LOG_MULTI_SELECT_ERROR_VIEW = "[選取異常] 路徑單選模式下偵測到多重選取 ({0} 個項目)，已自動清除";
		const string LOG_MULTI_SELECT_ERROR_TREE = "[選取異常] 路徑單選模式下從路徑樹偵測到多重選取 ({0} 個項目)，已自動清除";
	}
}
