using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using MyCAM.Helper;
using MyCAM.Helper.CAM;
using MyCAM.PathCache;
using OCC.TopoDS;
using OCCViewer;

namespace MyCAM.Editor
{
	internal class StartPointAction : EditActionBase
	{
		public StartPointAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, List<string> pathIDList, SelectPathAction pathIndexAction )
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
				return EditActionType.StartPoint;
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
			m_Dlg = new StartPointDlg();
			m_Dlg.Cancel += End;
			m_Dlg.ModeChanged += OnModeChanged;
			m_Dlg.Apply += OnApply;
			m_Dlg.AddOrRemoveStrategy += OnAddOrRemoveStrategy;
			m_Dlg.IncreaseOrDecreasePriority += OnIncreaseOrDecreasePriority;
			m_Dlg.Show( MyApp.MainForm );

			// start path index action
			m_PathIndexAction.AddLockedPaths( CreateLockList() );
			m_PathIndexAction.Start();
			m_PathIndexAction.SelectionChange += OnPathIndexChanged;

			// activate point selection for initial path
			if( IsSinglePathSelected ) {
				ActivatePointSelection();
			}

			if( TryBuildStartPointParameter( !IsPointSelectionMode, IsSinglePathSelected, new List<StartPointSelectStrategy>(), 0.0 ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		public override void End()
		{
			if( IsPointSelectionMode ) {
				string pointPathID = m_PathIDList.First();
				DeactivatePointSelection( pointPathID );
			}

			m_PathIndexAction.SelectionChange -= OnPathIndexChanged;
			m_PathIndexAction.End();

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

				// is not closed path, lock it
				if( !DataGettingHelper.GetGeomDataByID( pathID, out IGeomData geomData ) || !geomData.IsClosed ) {
					lockList.Add( pathID );
				}
			}
			return lockList;
		}

		#region Path Selection

		bool TryChangePath( List<string> pathIDList )
		{
			if( pathIDList == null ) {
				return false;
			}

			List<string> newPathIDList = pathIDList;
			List<CraftData> newCraftDataList = new List<CraftData>();
			List<IPathCache> newPathCacheList = new List<IPathCache>();

			foreach( string pathID in newPathIDList ) {

				string pathName = m_DataManager.GetPathName( pathID );

				// check is closed path
				if( !DataGettingHelper.GetGeomDataByID( pathID, out IGeomData geomData ) ) {
					MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetPathGeomInfo", CANNOT_GET_PATH_GEOM_INFO ), pathName ), MyApp.NoticeType.Warning );
					return false;
				}
				if( !geomData.IsClosed ) {
					MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "PathNotClosed", PATH_NOT_CLOSED ), pathName ), MyApp.NoticeType.Warning );
					return false;
				}

				// init data for current path
				if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
					MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetPathCraftInfo", CANNOT_GET_PATH_CRAFT_INFO ), pathName ), MyApp.NoticeType.Warning );
					return false;
				}
				newCraftDataList.Add( craftData );

				// init ContourCache data for current path
				if( !DataGettingHelper.GetPathCacheByID( pathID, out IPathCache pathCache ) ) {
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

		void OnPathIndexChanged()
		{
			List<string> selectedIDs = m_PathIndexAction.GetSelectedIDs();

			if( selectedIDs.Count() == 0 ) {
				return;
			}

			// construct/ destruct point selection
			if( m_SuppressPathSelectionChanged ) {
				return;
			}

			bool isManulSelect = IsPointSelectionMode;
			if( IsPointSelectionMode ) {
				string oldPathID = m_PathIDList.First();
				DeactivatePointSelection( oldPathID );
			}

			if( !TryChangePath( selectedIDs ) ) {
				m_PathIDList.Clear();
				m_CraftDataList.Clear();
				m_PathCacheList.Clear();
			}

			if( isManulSelect ) {
				ActivatePointSelection();
			}

			if( TryBuildStartPointParameter( !IsPointSelectionMode, IsSinglePathSelected, m_Param.Strategies, m_Param.Tolerance ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		#endregion

		#region Point Selection

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
			m_PathIndexAction.RestoreFromExclusion( m_PathIDList.First() );
			m_PathIndexAction.ExitSingleSelectMode();
			m_SuppressPathSelectionChanged = false;
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
			SetStartPoint( m_PathIDList.First(), m_CraftDataList.First(), nSelectIndex );
			PropertyChanged?.Invoke( new List<string>() { m_PathIDList.First() } );
			m_Viewer.GetAISContext().ClearSelected( true );
		}

		void SetStartPoint( string pathID, CraftData craftData, int cadIndex )
		{
			craftData.SetStartPntIndex( cadIndex );

			// update coupler craft data
			if( m_DataManager.ObjectMap[ pathID ] is StdPatternObjectBase stdPatternObject && stdPatternObject.GeomData != null ) {
				StdPatternCraftCoupler stdPatternCraftCoupler = new StdPatternCraftCoupler();
				stdPatternCraftCoupler.HandleCouplerCraftForStartPoint( ref craftData, stdPatternObject.GeomData );
			}
		}

		#endregion

		#region Dialog Handling

		bool TryBuildStartPointParameter( bool isAutoMode, bool isSingleSelectMode, List<StartPointSelectStrategy> strategies, double tolerance )
		{
			if( m_Dlg == null || tolerance < 0.0 ) {
				return false;
			}

			m_Param = new StartPointParam( isAutoMode, isSingleSelectMode, strategies, tolerance );

			return true;
		}

		void OnModeChanged()
		{
			if( m_Param == null ) {
				return;
			}

			if( IsPointSelectionMode ) {
				string oldPathID = m_PathIDList.First();
				DeactivatePointSelection( oldPathID );
				m_PathIndexAction.SelectPathByID( oldPathID );
				m_Param.IsAutoMode = true;
			}
			else {
				ActivatePointSelection();
				m_Param.IsAutoMode = false;
			}

			if( TryBuildStartPointParameter( !IsPointSelectionMode, IsSinglePathSelected, m_Param.Strategies, m_Param.Tolerance ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		void OnApply()
		{
			if( m_Param == null || !m_Param.IsAutoMode ) {
				return;
			}

			for( int i = 0; i < m_PathIDList.Count; ++i ) {

				string pathID = m_PathIDList[ i ];
				IPathCache pathCache = m_PathCacheList[ i ];
				CraftData craftdata = m_CraftDataList[ i ];

				string pathName = m_DataManager.GetPathName( pathID );

				List<CAMPoint> camPointList = null;
				List<StartPointSelectStrategy> strategies = m_Param.Strategies;

				if( pathCache is IStdPatternCache stdPatternCache ) {
					strategies = strategies
					.Where( strategy => strategy != StartPointSelectStrategy.LongestSegmentMidpoint )
					.ToList();
					camPointList = stdPatternCache.KeyCAMPointList;
				}
				else {
					camPointList = pathCache.MainPathPointList;
				}

				if( !StartPointHelper.TryChooseByPriority( camPointList, strategies, m_Param.Tolerance, out int camIndex ) ) {
					MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetAutoStartPoint", CANNOT_GET_AUTO_START_POINT ), pathName ), MyApp.NoticeType.Warning );
					continue;
				}

				SetStartPoint( pathID, craftdata, camPointList[ camIndex ].InitPathIndex );
			}

			PropertyChanged?.Invoke( m_PathIDList );
		}

		void OnIncreaseOrDecreasePriority( StartPointSelectStrategy strategy, bool increaseOrDecrease )
		{
			if( m_Param == null || !m_Param.IsAutoMode ) {
				return;
			}

			int offset = increaseOrDecrease ? -1 : 1;

			int oldIndex = m_Param.Strategies.IndexOf( strategy );
			int newIndex = oldIndex + offset;

			if( oldIndex < 0 || newIndex < 0 || newIndex >= m_Param.Strategies.Count ) {
				return;
			}

			m_Param.Strategies.RemoveAt( oldIndex );
			m_Param.Strategies.Insert( newIndex, strategy );

			m_Dlg.ResetParam( m_Param );
		}

		void OnAddOrRemoveStrategy( StartPointSelectStrategy strategy, bool addOrRemove )
		{
			if( m_Param == null || !m_Param.IsAutoMode ) {
				return;
			}

			if( addOrRemove ) {
				if( !m_Param.Strategies.Contains( strategy ) ) {
					m_Param.Strategies.Add( strategy );
				}
			}
			else {
				m_Param.Strategies.Remove( strategy );
			}

			m_Dlg.ResetParam( m_Param );
		}

		#endregion

		readonly Viewer m_Viewer;
		readonly TreeView m_TreeView;
		readonly ViewManager m_ViewManager;

		// Path selection
		bool IsSinglePathSelected { get { return m_PathIDList.Count() == 1; } }
		List<string> m_PathIDList = null;
		List<CraftData> m_CraftDataList = null;
		List<IPathCache> m_PathCacheList = null;
		bool m_SuppressPathSelectionChanged = false;
		SelectPathAction m_PathIndexAction = null;

		// Point selection
		bool IsPointSelectionMode { get { return m_PointIndexAction != null; } }
		IndexSelectAction m_PointIndexAction = null;

		// Dialog Handling
		StartPointDlg m_Dlg = null;
		StartPointParam m_Param = null;

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "StartPointAction" );
		const string CANNOT_GET_PATH_CRAFT_INFO = "無法獲取路徑 {0} 的加工資訊";
		const string CANNOT_GET_PATH_GEOM_INFO = "無法獲取路徑 {0} 的幾何資訊";
		const string CANNOT_GET_AUTO_START_POINT = "無法計算路徑 {0} 的自動起點";
		const string PATH_NOT_CLOSED = "路徑 {0} 為非封閉路徑，無法設定起點";
	}
}
