using MyCAM.App;
using MyCAM.Data;
using MyCAM.PathCache;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class ExtendedGCodeAction : EditActionBase
	{
		public ExtendedGCodeAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, List<string> pathIDList, SelectPathAction pathIndexAction )
			: base( dataManager )
		{
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;

			// for path index control
			m_PathIDList = pathIDList;
			m_PathIndexAction = pathIndexAction;
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.ExtendedGCode;
			}
		}

		public Action<List<string>> PropertyChanged;

		public override void Start()
		{
			base.Start();

			// setup initial path
			if( !TryChangePath( m_PathIDList ) ) {
				End();
				return;
			}

			// create dialog
			m_Dlg = new ExtendedGCodeDlg();
			m_Dlg.Cancel += End;
			m_Dlg.AddRequest += OnAddRequest;
			m_Dlg.DeleteRequest += OnDeleteRequest;
			m_Dlg.PositionChanged += OnPositionChanged;
			m_Dlg.ContentChanged += OnContentChanged;
			m_Dlg.ToStartOrEnd += OnToStartOrEnd;
			m_Dlg.Show( MyApp.MainForm );

			// start path index action
			m_PathIndexAction.AddLockedPaths( CreateLockList() );
			m_PathIndexAction.Start();
			m_PathIndexAction.SelectionChange += OnPathIndexChanged;

			if( TryBuildExtendedGCodeParameter( ExtendedGCodeType.TraverseHead, null ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		public override void End()
		{
			m_Dlg?.FlushContent();

			if( IsPointSelectionMode ) {
				string pointPathID = m_PathIDList.First();
				DeactivatePointSelection( pointPathID );
			}

			// cleanup path index action
			m_PathIndexAction.SelectionChange -= OnPathIndexChanged;
			m_PathIndexAction.End();

			// close dialog
			if( m_Dlg != null ) {
				m_Dlg.Cancel -= End;
				m_Dlg.Close();
				m_Dlg = null;
			}

			base.End();
		}

		IEnumerable<string> CreateLockList()
		{
			List<string> lockList = new List<string>();
			foreach( string pathID in m_DataManager.PathIDList ) {

				// is std pattern path, lock it
				if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) || pathType != PathType.Contour ) {
					lockList.Add( pathID );
				}
			}
			return lockList;
		}

		#region Path Selection

		// path index control
		bool IsNonPathSelected { get { return m_PathIDList.Count == 0; } }

		bool IsSinglePathSelected { get { return m_PathIDList?.Count == 1; } }

		bool m_SuppressPathSelectionChanged = false;

		List<string> m_PathIDList = null;
		List<CraftData> m_CraftDataList = null;
		List<ContourCache> m_PathCacheList = null;

		SelectPathAction m_PathIndexAction = null;

		const string NOT_CONTOUR_PATH = "路徑 {0} 不是輪廓路徑，無法編輯";
		const string CANNOT_GET_PATH_CRAFT_INFO = "無法獲取路徑 {0} 的加工資訊";

		void OnPathIndexChanged()
		{
			List<string> selectedIDs = m_PathIndexAction.GetSelectedIDs();

			// construct/ destruct point selection
			if( m_SuppressPathSelectionChanged ) {
				return;
			}

			ExtendedGCodeType gCodeType = IsNonPathSelected
				? ExtendedGCodeType.TraverseHead
				: IsPointSelectionMode
					? ExtendedGCodeType.ControlPoint
					: m_Param.GCodeType;

			if( IsPointSelectionMode ) {
				// cannot change to non selected mode while point selection mode
				if( selectedIDs.Count == 0 ) {
					return;
				}

				string oldPathID = m_PathIDList.First();
				DeactivatePointSelection( oldPathID );
			}

			if( !TryChangePath( selectedIDs ) ) {
				m_PathIDList.Clear();
				m_PathCacheList.Clear();
				m_CraftDataList.Clear();
			}

			m_Dlg.FlushContent();

			if( IsNonPathSelected ) {
				m_Param = null;
				m_Dlg.ResetParam( m_Param );
				return;
			}

			if( gCodeType == ExtendedGCodeType.ControlPoint ) {
				ActivatePointSelection();
				return;
			}

			if( TryBuildExtendedGCodeParameter( gCodeType, null ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		bool TryChangePath( List<string> pathIDList )
		{
			if( pathIDList == null ) {
				return false;
			}

			List<string> newPathIDList = pathIDList;
			List<CraftData> newCraftDataList = new List<CraftData>();
			List<ContourCache> newPathCacheList = new List<ContourCache>();


			foreach( string pathID in newPathIDList ) {
				string pathName = m_DataManager.GetPathName( pathID );

				// validate new path (contour only)
				if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) || pathType != PathType.Contour ) {
					MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "NotContourPath", NOT_CONTOUR_PATH ), pathName ), MyApp.NoticeType.Warning );
					return false;
				}

				// init data for current path
				if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
					MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetPathCraftInfo", CANNOT_GET_PATH_CRAFT_INFO ), pathName ), MyApp.NoticeType.Warning );
					return false;
				}
				newCraftDataList.Add( craftData );

				// init ContourCache data for current path
				if( !DataGettingHelper.GetContourCacheByID( pathID, out ContourCache pathCache ) ) {
					MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetPathCraftInfo", CANNOT_GET_PATH_CRAFT_INFO ), pathName ), MyApp.NoticeType.Warning );
					return false;
				}
				newPathCacheList.Add( pathCache );
			}

			m_PathIDList = newPathIDList;
			m_CraftDataList = newCraftDataList;
			m_PathCacheList = newPathCacheList;

			return true;
		}



		#endregion

		#region Point Selection

		bool IsPointSelectionMode { get { return m_PointIndexAction != null; } }

		bool IsNonPointSelected { get { return m_nSelectCADIndex == null; } }

		// point index control
		int? m_nSelectCADIndex = null;
		IndexSelectAction m_PointIndexAction;

		void ActivatePointSelection()
		{
			if( IsPointSelectionMode || !IsSinglePathSelected ) {
				return;
			}

			string pathID = m_PathIDList.First();

			CreatePointIndexAction( pathID );

			m_SuppressPathSelectionChanged = true;
			m_PathIndexAction.EnterSingleSelectMode();
			m_PathIndexAction.ExcludeFromSelection( pathID );
			m_SuppressPathSelectionChanged = false;

			OnSelectedPointIndexChanged( null );
		}

		void CreatePointIndexAction( string pathID )
		{
			m_PointIndexAction = new IndexSelectAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, pathID );
			m_PointIndexAction.IndexChanged += OnPointIndexChanged;
			m_PointIndexAction.Start();
		}

		void DeactivatePointSelection( string szOldPathID )
		{
			DestroyPointIndexAction();

			m_SuppressPathSelectionChanged = true;
			m_PathIndexAction.RestoreFromExclusion( szOldPathID );
			m_PathIndexAction.SelectPathByID( szOldPathID );
			m_PathIndexAction.ExitSingleSelectMode();
			m_SuppressPathSelectionChanged = false;

			m_nSelectCADIndex = null;
		}

		void DestroyPointIndexAction()
		{
			if( m_PointIndexAction != null ) {
				m_PointIndexAction.IndexChanged -= OnPointIndexChanged;
				m_PointIndexAction.End();
				m_PointIndexAction = null;
			}
		}

		void OnPointIndexChanged( int nSelectIndex, TopoDS_Shape selectedVertex )
		{
			if( nSelectIndex == m_nSelectCADIndex && m_Param.GCodeType == ExtendedGCodeType.ControlPoint ) {
				return;
			}
			OnSelectedPointIndexChanged( nSelectIndex );
		}

		void OnSelectedPointIndexChanged( int? nSelectIndex )
		{
			if( nSelectIndex != null && m_nSelectCADIndex == nSelectIndex && m_Param.GCodeType == ExtendedGCodeType.ControlPoint ) {
				return;
			}

			m_Dlg.FlushContent();

			m_nSelectCADIndex = nSelectIndex;

			// should not happen, but just in case
			if( IsNonPointSelected ) {
				m_Param = null;
				m_Dlg.ResetParam( m_Param );
				return;
			}

			// refresh dialog
			if( TryBuildExtendedGCodeParameter( ExtendedGCodeType.ControlPoint, m_nSelectCADIndex ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		int? GetCurrentStoredIndex()
		{
			if( m_Param.GCodeType != ExtendedGCodeType.ControlPoint || m_nSelectCADIndex == null ) {
				return null;
			}
			return GetStoredIndex( m_nSelectCADIndex.Value, m_Param.IsPathStartPoint );
		}

		int GetStoredIndex( int cadIndex, bool? isPathStartPoint )
		{
			// end point and start point share the same CAD index,
			// use closed point index to distinguish the end point
			if( isPathStartPoint == false ) {
				return SpecialPointIndex.CLOSED_POINT_INDEX;
			}
			return cadIndex;
		}

		#endregion

		#region Dialog Handling

		ExtendedGCodeDlg m_Dlg = null;
		ExtendedGCodeParam m_Param = null;

		bool TryBuildExtendedGCodeParameter( ExtendedGCodeType gCodeType, int? cadIndex, bool? isPathStartPoint = null )
		{
			if( gCodeType != ExtendedGCodeType.ControlPoint ) {
				m_Param = BuildExtendedGCodeParameter( gCodeType, null, null );
				return true;
			}

			// must have CAD index for control point
			if( cadIndex == null ) {
				return false;
			}

			// only closed path start point can toggle between start and end
			if( !IsClosedPathStartPoint( cadIndex.Value ) ) {
				isPathStartPoint = null;
			}
			else if( isPathStartPoint == null ) {
				isPathStartPoint = true;
			}

			int nStoredIndex = GetStoredIndex( cadIndex.Value, isPathStartPoint );
			m_Param = BuildExtendedGCodeParameter( gCodeType, nStoredIndex, isPathStartPoint );

			return true;
		}

		ExtendedGCodeParam BuildExtendedGCodeParameter( ExtendedGCodeType gCodeType, int? cadIndex, bool? isPathStartPoint )
		{
			Tuple<ExtendedGCodeType, int?> key = Tuple.Create( gCodeType, cadIndex );

			if( IsSinglePathSelected ) {
				CraftData craftData = m_CraftDataList.First();

				if( craftData.ExtendedGCodeMap.TryGetValue( key, out string content ) ) {
					return new ExtendedGCodeParam( true, gCodeType, content, isPathStartPoint, true );
				}

				return new ExtendedGCodeParam( false, gCodeType, string.Empty, isPathStartPoint, true );
			}

			// mixed multi path values are blank but remain editable when any path has data
			bool hasAnyValue = false;
			bool allHaveValue = true;
			string firstContent = null;
			bool hasSameContent = true;
			foreach( CraftData data in m_CraftDataList ) {
				if( data.ExtendedGCodeMap.TryGetValue( key, out string content ) ) {
					if( !hasAnyValue ) {
						firstContent = content;
					}
					else if( firstContent != content ) {
						hasSameContent = false;
						break;
					}
					hasAnyValue = true;
				}
				else {
					allHaveValue = false;
				}
			}

			string displayContnet = ( hasAnyValue && allHaveValue && hasSameContent ) ? firstContent : string.Empty;
			bool isMixData = hasAnyValue && ( !allHaveValue || !hasSameContent );
			return new ExtendedGCodeParam( hasAnyValue, gCodeType, displayContnet, isPathStartPoint, false, isMixData );
		}

		bool IsClosedPathStartPoint( int cadIndex )
		{
			if( m_PathCacheList.First().MainPathPointList.Count == 0 ) {
				return false;
			}

			return SpecialPointIndex.IsClosedPointIndex( m_PathCacheList.First().MainPathPointList.Last().InitPathIndex )
				&& ( cadIndex == m_PathCacheList.First().MainPathPointList.First().InitPathIndex );
		}

		void OnAddRequest()
		{
			if( m_CraftDataList == null || m_Param == null ) {
				return;
			}

			m_Param.IsModified = true;
			foreach( CraftData craftData in m_CraftDataList ) {
				craftData.SetExtendedGCode( m_Param.GCodeType, GetCurrentStoredIndex(), m_Param.Content );
			}
			PropertyChanged?.Invoke( m_PathIDList );
		}

		void OnDeleteRequest()
		{
			if( m_CraftDataList == null || m_Param == null ) {
				return;
			}

			foreach( CraftData craftData in m_CraftDataList ) {
				craftData.RemoveExtendedGCode( m_Param.GCodeType, GetCurrentStoredIndex() );
			}
			m_Param.IsModified = false;
			m_Param.Content = string.Empty;
			PropertyChanged?.Invoke( m_PathIDList );
		}

		void OnPositionChanged( ExtendedGCodeType gCodeType )
		{
			if( m_Param == null ) {
				return;
			}

			if( gCodeType == ExtendedGCodeType.ControlPoint ) {
				ActivatePointSelection();
				return;
			}

			if( IsPointSelectionMode ) {
				string oldPathID = m_PathIDList.First();
				DeactivatePointSelection( oldPathID );
			}

			if( TryBuildExtendedGCodeParameter( gCodeType, null ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		void OnContentChanged( string content )
		{
			if( m_CraftDataList == null || m_Param == null ) {
				return;
			}

			if( !m_Param.IsModified ) {
				return;
			}

			if( m_Param.Content == content ) {
				return;
			}

			m_Param.Content = content;

			foreach( CraftData craftData in m_CraftDataList ) {
				craftData.SetExtendedGCode( m_Param.GCodeType, GetCurrentStoredIndex(), m_Param.Content );
			}
			PropertyChanged?.Invoke( m_PathIDList );
		}

		// only single path selection
		void OnToStartOrEnd()
		{
			if( !IsPointSelectionMode || !IsSinglePathSelected || m_Param == null || m_Param.IsPathStartPoint == null ) {
				return;
			}

			// should not happen, but just in case
			if( m_PathCacheList == null || m_PathCacheList.Count != 1 || m_PathCacheList.First().MainPathPointList.Count <= 0 ) {
				return;
			}

			bool toStart = ( m_Param.IsPathStartPoint != true );
			if( TryBuildExtendedGCodeParameter( ExtendedGCodeType.ControlPoint, m_nSelectCADIndex, toStart ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		#endregion

		// composition references
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ExtendedGCodeAction" );
	}
}
