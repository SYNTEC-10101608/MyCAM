using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using MyCAM.Helper;
using OCC.gp;

namespace MyCAM.Editor
{
	internal class AutoSortPathAction : EditActionBase
	{

		public AutoSortPathAction( DataManager dataManager, SelectPathAction selectPathAction )
			: base( dataManager )
		{
			m_SelectPathAction = selectPathAction;
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.AutoSortPath;
			}
		}

		public Action<List<string>> SortCompleted;

		public override void Start()
		{
			base.Start();

			m_SelectPathAction.EnterSingleSelectMode();
			m_SelectPathAction.Start();
			m_SelectPathAction.SelectionChange += OnSelectionChanged;

			m_Dialog = new AutoSortPathDlg();
			m_Dialog.Confirm += OnConfirm;
			m_Dialog.Cancel += OnCancel;
			m_Dialog.Show( MyApp.MainForm );
		}

		public override void End()
		{
			m_SelectPathAction.SelectionChange -= OnSelectionChanged;
			m_SelectPathAction.ExitSingleSelectMode();
			m_SelectPathAction.End();

			if( m_Dialog != null && !m_Dialog.IsDisposed ) {
				m_Dialog.Confirm -= OnConfirm;
				m_Dialog.Cancel -= OnCancel;
				m_Dialog.Close();
				m_Dialog.Dispose();
				m_Dialog = null;
			}

			base.End();
		}

		void OnSelectionChanged()
		{
			if( m_Dialog == null || m_Dialog.CurrentSortMethod != SortMethod.ShortestPath ) {
				return;
			}

			List<string> selectedIDs = m_SelectPathAction.GetSelectedIDs();
			string pathID = selectedIDs.Count > 0 ? selectedIDs.First() : null;
			m_Dialog.UpdateSelectedPathStatus( pathID );
		}

		void OnConfirm( SortParams sortParams )
		{
			// Early return without End(): keep dialog open so user can retry
			// when precondition is not met (e.g. insufficient paths)
			List<string> pathIDList = new List<string>( m_DataManager.PathIDList );
			if( pathIDList.Count < 2 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Log_InsufficientPaths", LOG_INSUFFICIENT_PATHS ), MyApp.NoticeType.Warning );
				return;
			}

			List<string> result = null;

			switch( sortParams.Method ) {
				case SortMethod.ShortestPath:
					string startPathID = m_SelectPathAction.GetSelectedIDs().FirstOrDefault();
					result = PathSortHelper.SortByShortestPath( pathIDList, startPathID );
					break;

				case SortMethod.CylinderVertical:
					gp_Dir axisDir1 = GetAxisDirFromType( sortParams.CylinderAxis );
					gp_Pnt axisLoc1 = new gp_Pnt( 0, 0, 0 );
					result = PathSortHelper.SortCylinderVertical(
						pathIDList, axisDir1, axisLoc1,
						sortParams.IsRotationDescending,
						sortParams.IsExtrusionDescending,
						sortParams.RotationBandwidth_deg
						);
					break;

				case SortMethod.CylinderHorizontal:
					gp_Dir axisDir2 = GetAxisDirFromType( sortParams.CylinderAxis );
					gp_Pnt axisLoc2 = new gp_Pnt( 0, 0, 0 );
					result = PathSortHelper.SortCylinderHorizontal(
						pathIDList, axisDir2, axisLoc2,
						sortParams.IsExtrusionDescending,
						sortParams.IsRotationDescending,
						sortParams.ExtrusionBandwidth_mm
						);
					break;
			}

			if( result != null && result.Count > 0 ) {
				SortCompleted?.Invoke( result );
			}
			else {
				string methodName = GetSortMethodName( sortParams.Method );
				MyApp.Logger.ShowOnLogPanel(
					string.Format( m_LanguageManager.TryGetString( "Log_SortFailed", LOG_SORT_FAILED ), methodName ),
					MyApp.NoticeType.Warning );
			}

			// End this action last: closes dialog and notifies action management (EndAction callback)
			End();
		}

		static gp_Dir GetAxisDirFromType( CylinderAxisType axisType )
		{
			switch( axisType ) {
				case CylinderAxisType.X:
					return new gp_Dir( 1, 0, 0 );
				case CylinderAxisType.Y:
					return new gp_Dir( 0, 1, 0 );
				case CylinderAxisType.Z:
				default:
					return new gp_Dir( 0, 0, 1 );
			}
		}

		string GetSortMethodName( SortMethod method )
		{
			switch( method ) {
				case SortMethod.ShortestPath:
					return m_LanguageManager.TryGetString( "SortMethod_ShortestPath", SORT_METHOD_SHORTEST_PATH );
				case SortMethod.CylinderVertical:
					return m_LanguageManager.TryGetString( "SortMethod_CylinderVertical", SORT_METHOD_CYLINDER_VERTICAL );
				case SortMethod.CylinderHorizontal:
					return m_LanguageManager.TryGetString( "SortMethod_CylinderHorizontal", SORT_METHOD_CYLINDER_HORIZONTAL );
				default:
					return m_LanguageManager.TryGetString( "SortMethod_Unknown", SORT_METHOD_UNKNOWN );
			}
		}

		void OnCancel()
		{
			End();
		}

		SelectPathAction m_SelectPathAction;
		AutoSortPathDlg m_Dialog;
		const string LOG_INSUFFICIENT_PATHS = "[路徑排序] 路徑數量不足，至少需要 2 條路徑";
		const string LOG_SORT_FAILED = "[路徑排序] {0}排序未產生有效結果，請確認路徑資料是否正確";
		const string SORT_METHOD_SHORTEST_PATH = "最短路徑";
		const string SORT_METHOD_CYLINDER_VERTICAL = "圓筒直排";
		const string SORT_METHOD_CYLINDER_HORIZONTAL = "圓筒橫排";
		const string SORT_METHOD_UNKNOWN = "未知";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "AutoSortPathAction" );
	}
}
