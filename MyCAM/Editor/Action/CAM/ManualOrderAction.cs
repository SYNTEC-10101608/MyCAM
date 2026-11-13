using MyCAM.App;
using MyCAM.Data;
using OCC.AIS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class ManualOrderAction : KeyMouseActionBase
	{
		public Action ShowOrderData;
		public Action<bool> RaiseActionStart;
		public Action NeedDoOptimized;

		public ManualOrderAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, SelectPathAction selectPathAction )
			: base( dataManager, viewer, treeView, viewManager )
		{
			if( selectPathAction == null ) {
				throw new ArgumentNullException( nameof( selectPathAction ) );
			}

			m_OrderMode = OrderMode.Sequential;
			m_CurrentFlagIndex = 0;
			m_SelectPathAction = selectPathAction;
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.ManualOrder;
			}
		}

		public override void Start()
		{
			base.Start();

			// start the shared select path action
			m_SelectPathAction.Start();

			m_OrderDialog = new OrderDialog();
			m_OrderDialog.OrderModeChanged += ( mode ) => ChangeOrderMode( mode );
			m_OrderDialog.EnterKeyPressed += HandleSequentialOrderEnter;
			m_OrderDialog.MoveToTop += MoveToTop;
			m_OrderDialog.MoveUp += () => MoveProcess( true );
			m_OrderDialog.MoveDown += () => MoveProcess( false );
			m_OrderDialog.Cancel = End;
			m_OrderDialog.Confirm += End;

			// subscribe to TreeView double click event
			if( m_TreeView is MultiSelectTreeView multiSelectTreeView ) {
				multiSelectTreeView.NodeDoubleClicked += TreeViewNodeDoubleClick;
			}

			m_OrderDialog.Show( MyApp.MainForm );
			RaiseActionStart?.Invoke( true );
			HighLightFlagPath();
		}

		public override void End()
		{
			// this end is triggered by other editor will need to close Dlg
			if( m_OrderDialog != null && !m_OrderDialog.IsDisposed ) {
				m_OrderDialog.OrderModeChanged = null;
				m_OrderDialog.EnterKeyPressed = null;
				m_OrderDialog.MoveToTop = null;
				m_OrderDialog.MoveUp = null;
				m_OrderDialog.MoveDown = null;
				m_OrderDialog.Cancel = null;
				m_OrderDialog.Confirm = null;
				m_OrderDialog.CloseDialog();
				m_OrderDialog.Dispose();
				m_OrderDialog = null;
			}

			// unsubscribe TreeView double click event
			if( m_TreeView is MultiSelectTreeView multiSelectTreeView ) {
				multiSelectTreeView.NodeDoubleClicked -= TreeViewNodeDoubleClick;
			}

			// end the shared select path action
			m_SelectPathAction.End();

			ClearFlagPath();

			// IK optimization before UI update
			NeedDoOptimized?.Invoke();

			// trigger UI update
			RaiseActionStart?.Invoke( false );
			base.End();
		}

		protected override void ViewerMouseDoubleClick( MouseEventArgs e )
		{
			// single select
			if( e.Button == MouseButtons.Left ) {
				if( ( Control.ModifierKeys & Keys.Control ) == Keys.Control ) {
					m_Viewer.Select( AIS_SelectionScheme.AIS_SelectionScheme_XOR );
				}
				else {
					m_Viewer.Select();
				}

				// only sequential mode can change flag index by double click
				if( m_OrderMode == OrderMode.Sequential ) {
					ChangeFlagIndex();
				}
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				HandleSequentialOrderEnter();
				e.Handled = true;
			}
		}

		protected override void TreeViewKeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				HandleSequentialOrderEnter();
				e.Handled = true;
			}
		}

		void TreeViewNodeDoubleClick( object sender, TreeNodeMouseClickEventArgs e )
		{
			if( m_OrderMode == OrderMode.Sequential ) {
				ChangeFlagIndex();
			}
		}

		void ChangeFlagIndex()
		{
			bool bGetSuccess = GetSelectUniqueIDAndOrderIdx( out string uniquePathID, out int selectedIndex );
			if( bGetSuccess == false ) {
				return;
			}

			// in sequential mode, the flag will move with the selected path
			if( m_OrderMode == OrderMode.Sequential ) {
				m_CurrentFlagIndex = selectedIndex;
				HighLightFlagPath();
			}
		}

		void ChangeOrderMode( OrderMode mode )
		{
			m_OrderMode = mode;
			if( m_OrderMode == OrderMode.Sequential ) {
				m_CurrentFlagIndex = 0;
				HighLightFlagPath();
			}
			else {
				ClearFlagPath();
			}
		}

		void HandleSequentialOrderEnter()
		{
			bool bGetSuccess = GetSelectUniqueIDAndOrderIdx( out string uniquePathID, out int selectedIndex );
			if( bGetSuccess == false ) {
				return;
			}

			// validate flag index before proceeding
			if( m_CurrentFlagIndex < 0 || m_CurrentFlagIndex >= m_DataManager.PathIDList.Count ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "InvalidFlagPosition", INVALID_FLAG_POSITION ), MyApp.NoticeType.Warning );
				return;
			}

			// insert the selected path after the flag position
			m_DataManager.PathRemoveAt( selectedIndex );
			if( selectedIndex <= m_CurrentFlagIndex ) {
				m_DataManager.PathInsert( m_CurrentFlagIndex, uniquePathID );
			}
			else {
				m_DataManager.PathInsert( m_CurrentFlagIndex + 1, uniquePathID );
			}

			// find index by ID
			int newIndex = m_DataManager.PathIndexOf( uniquePathID );
			m_CurrentFlagIndex = newIndex;

			// re-select the moved path to sync tree view
			m_SelectPathAction.ClearSelection();
			m_SelectPathAction.SelectPathByID( uniquePathID );
			RefreshOrderDataAndFlag();
		}

		void MoveToTop()
		{
			bool bGetSuccess = GetSelectUniqueIDAndOrderIdx( out string uniquePathID, out int selectedIndex );
			if( bGetSuccess == false ) {
				return;
			}

			if( m_DataManager?.PathIDList == null ) {
				return;
			}

			if( selectedIndex < 0 || selectedIndex >= m_DataManager.PathIDList.Count ) {
				return;
			}

			// move the selected path to top of the list
			m_DataManager.PathRemoveAt( selectedIndex );
			m_DataManager.PathInsert( START_Indx, uniquePathID );
			m_CurrentFlagIndex = START_Indx;
			// re-select the moved path to sync tree view
			m_SelectPathAction.ClearSelection();
			m_SelectPathAction.SelectPathByID( uniquePathID );
			RefreshOrderDataAndFlag();
		}

		void MoveProcess( bool bUp )
		{
			bool bGetSuccess = GetSelectUniqueIDAndOrderIdx( out string uniquePathID, out int selectIndex );
			if( bGetSuccess == false ) {
				return;
			}

			if( m_DataManager?.PathIDList == null ) {
				return;
			}

			// check boundary
			if( selectIndex < 0 || selectIndex > m_DataManager.PathIDList.Count - 1
				|| bUp && selectIndex == 0
				|| !bUp && selectIndex == m_DataManager.PathIDList.Count - 1 ) {
				return;
			}

			// move process
			m_DataManager.PathRemoveAt( selectIndex );
			int newIndex;
			if( bUp ) {
				newIndex = selectIndex - 1;
				m_DataManager.PathInsert( newIndex, uniquePathID );
			}
			else {
				newIndex = selectIndex + 1;
				m_DataManager.PathInsert( newIndex, uniquePathID );
			}

			// re-select the moved path to sync tree view
			m_SelectPathAction.ClearSelection();
			m_SelectPathAction.SelectPathByID( uniquePathID );
			RefreshOrderDataAndFlag();
		}

		void RefreshOrderDataAndFlag()
		{
			ShowOrderData?.Invoke();
			if( m_OrderMode == OrderMode.Sequential ) {
				HighLightFlagPath();
			}
		}

		bool GetSelectUniqueIDAndOrderIdx( out string uniquePathID, out int pathOrderIdx )
		{
			uniquePathID = null;
			pathOrderIdx = -1;

			// get current selected path
			List<string> selectedIDs = m_SelectPathAction.GetSelectedIDs();
			if( selectedIDs.Count != 1 ) {

				if( selectedIDs.Count == 0 ) {
					MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "NoPathSelected", NO_PATH_SELECTED ), MyApp.NoticeType.Hint );
				}
				else {

					// only handle single selection
					MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "OnlySinglePathSelection", ONLY_SINGLE_PATH_SELECTION ), MyApp.NoticeType.Hint );
					m_SelectPathAction.ClearSelection();
				}
				return false;
			}
			uniquePathID = selectedIDs.First();
			pathOrderIdx = m_DataManager.PathIndexOf( uniquePathID );
			if( pathOrderIdx == -1 ) {
				return false;
			}
			return true;
		}

		void HighLightFlagPath()
		{
			ClearFlagPath();

			if( m_DataManager?.PathIDList == null ) {
				return;
			}

			// check index
			if( m_CurrentFlagIndex < 0 || m_CurrentFlagIndex >= m_DataManager.PathIDList.Count ) {
				return;
			}
			// get node idx
			string pathID = m_DataManager.PathIDList[ m_CurrentFlagIndex ];

			// find node by id
			if( m_ViewManager?.TreeNodeMap == null || !m_ViewManager.TreeNodeMap.ContainsKey( pathID ) ) {
				return;
			}

			TreeNode node = m_ViewManager.TreeNodeMap[ pathID ];
			if( node == null ) {
				return;
			}
			m_FlagNode = node;

			// set flag display
			if( m_TreeView is MultiSelectTreeView multiSelectTreeView ) {
				multiSelectTreeView.SetFlagNode( node, true );
			}
		}

		void ClearFlagPath()
		{
			if( m_FlagNode != null && m_TreeView is MultiSelectTreeView multiSelectTreeView ) {
				multiSelectTreeView.SetFlagNode( m_FlagNode, false );
				m_FlagNode = null;
			}
		}

		SelectPathAction m_SelectPathAction;
		TreeNode m_FlagNode;
		OrderDialog m_OrderDialog;
		OrderMode m_OrderMode;
		int m_CurrentFlagIndex;
		const int START_Indx = 0;

		const string INVALID_FLAG_POSITION = "[操作提醒]旗標位置無效";
		const string NO_PATH_SELECTED = "[操作提醒]未選取任何路徑";
		const string ONLY_SINGLE_PATH_SELECTION = "[操作提醒]本功能僅支援單路徑選擇";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ManualOrderAction" );
	}
}
