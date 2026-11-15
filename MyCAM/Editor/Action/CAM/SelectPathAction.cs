using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CommonUI.Controls;
using MyCAM.App;
using MyCAM.Data;
using OCCViewer;

namespace MyCAM.Editor
{
	internal class SelectPathAction : SelectObjectAction
	{
		public Action RemovePath;
		public Action WithPathCantSelect;

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

		// must be called before Start(); locked paths are cleared automatically on End().
		public void AddLockedPaths( IEnumerable<string> pathIDs )
		{
			if( pathIDs == null ) {
				return;
			}

			// reset List before restart
			m_LockedIDSet.Clear();
			foreach( string pathID in pathIDs ) {
				if( string.IsNullOrEmpty( pathID ) ) {
					continue;
				}

				// silently ignore unknown paths
				if( !m_DataManager.PathIDList.Contains( pathID ) ) {
					continue;
				}
				m_LockedIDSet.Add( pathID );
			}
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
			if( !IsSelectable( pathID ) ) {
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

		bool IsSelectable( string pathID )
		{
			if( string.IsNullOrEmpty( pathID ) ) {
				return false;
			}
			if( m_ExcludedIDSet != null && m_ExcludedIDSet.Contains( pathID ) ) {
				return false;
			}
			if( m_LockedIDSet.Contains( pathID ) ) {
				WithPathCantSelect?.Invoke();
				return false;
			}
			return true;
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

		public static bool IsStdPatternPath( string pathID )
		{
			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) ) {
				return false;
			}
			return DataGettingHelper.IsStdPattern( pathType );
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
			return specialToolVecData.Mode != ToolVecMode.Interpolate;
		}

		public override void Start()
		{
			base.Start();

			// enforce locked paths: deactivate on viewer and drop from current selection
			foreach( string pathID in m_LockedIDSet ) {
				m_ViewManager.DeactivePath( pathID );
				m_SelectedIDSet.Remove( pathID );
			}
		}

		public override void End()
		{
			base.End();

			// base.End() has already deactivated all paths; just release the lock list
			m_LockedIDSet.Clear();
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

		// in single select mode, disable XOR (Ctrl) selection so click always replaces
		protected override bool AllowXorSelection
		{
			get { return !isSingleSelectMode; }
		}

		protected override void ViewerMouseUp( MouseEventArgs e )
		{
			if( isSingleSelectMode ) {
				// do not perform rectangle selection
				return;
			}
			base.ViewerMouseUp( e );
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
				string szPathID = node.Tag as string;
				if( string.IsNullOrEmpty( szPathID ) ) {
					continue;
				}

				// this path is editing
				if( !IsSelectable( szPathID ) ) {
					continue;
				}
				m_SelectedIDSet.Add( szPathID );
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
				( m_TreeView as MultiSelectTreeView ).SelectNode( m_ViewManager.TreeNodeMap[ szUID ] );
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
		HashSet<string> m_LockedIDSet = new HashSet<string>();
		LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "SelectPathAction" );
		const string LOG_MULTI_SELECT_ERROR_VIEW = "[選取異常] 路徑單選模式下偵測到多重選取 ({0} 個項目)，已自動清除";
		const string LOG_MULTI_SELECT_ERROR_TREE = "[選取異常] 路徑單選模式下從路徑樹偵測到多重選取 ({0} 個項目)，已自動清除";
	}
}
