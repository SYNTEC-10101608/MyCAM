using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Renderer;
using MyCAM.Helper;
using MyCAM.PathCache;
using OCC.gp;
using OCC.V3d;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class SpecialToolVecEditAction : EditCAMActionBase
	{
		public Action<Dictionary<MachineComponentType, List<gp_Trsf>>, bool> RaiseTrans;
		public Action<bool, bool> RaiseActionStart;
		public Action HideMachine;
		public Action ResetPartUI;
		public Action NeedOptimizedIK;

		public SpecialToolVecEditAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, List<string> pathIDList, SelectPathAction pathIndexAction )
			: base( dataManager, pathIDList )
		{
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;

			// for path index control
			m_PathIndexAction = pathIndexAction;
			m_CurrentPathIDList = pathIDList;
			m_IsSingleSelect = pathIDList.Count == SINGLE_Select_Path_Count;
			m_RotaryAxisConfig = RotaryAxisHelper.CreateRotaryAxisConfig();
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.SpecialToolVecEdit;
			}
		}

		public override void Start()
		{
			base.Start();

			// subscribe to path selection changes
			m_Dlg = new SpecialToolVecEditDlg( m_RotaryAxisConfig );
			m_Dlg.MSAngleChanged += OnMSAngleChanged;
			m_Dlg.TypeChanged += OnTypeChanged;
			m_Dlg.Cancel += OnFormClose;
			m_Dlg.FlipRotaryAxis += OnFlipRotaryAxis;
			m_Dlg.RotateToFixDir += OnRotateToFixDir;
			m_Dlg.RotateToZDir += OnRotateToZDir;
			UpdateDialogUI();
			m_Dlg.Show( MyApp.MainForm );

			// start path index action
			m_PathIndexAction.AddLockedPaths( CreateLockList() );
			m_PathIndexAction.Start();
			m_PathIndexAction.SelectionChange += OnPathSelectionChanged;
			m_PathIndexAction.WithPathCantSelect += SelectPathAlert;

			// show machine
			RaiseActionStart?.Invoke( true, false );

			if( IsNoPathSelected() == false ) {
				m_ToolVecEditRender?.Remove();
				m_ToolVecEditRender = new ToolVecEditRender( m_Viewer, m_DataManager, m_CurrentPathIDList, true );
				RefreshSimuResult();
			}
		}

		public override void End()
		{
			// cleanup path index action
			m_PathIndexAction.SelectionChange -= OnPathSelectionChanged;
			m_PathIndexAction.WithPathCantSelect -= SelectPathAlert;
			m_PathIndexAction.End();
			m_ToolVecEditRender.Remove();

			if( m_Dlg != null ) {
				m_Dlg.MSAngleChanged = null;
				m_Dlg.TypeChanged = null;
				m_Dlg.Confirm = null;
				m_Dlg.Cancel = null;
				m_Dlg.FlipRotaryAxis = null;
				m_Dlg.RotateToFixDir = null;
				m_Dlg.RotateToZDir = null;
				m_Dlg.Close();
				m_Dlg = null;
			}
			RaiseActionStart?.Invoke( false, true );
			base.End();
		}

		#region Property area

		ESpecialToolVecType m_Type = ESpecialToolVecType.Normal;
		const double DEFAULT_Angle_Deg = 0;
		const double RAD2DEG = 180.0 / Math.PI;

		// path index param
		List<string> m_CurrentPathIDList;

		// global param
		RotaryAxisConfig m_RotaryAxisConfig = null;

		SpecialToolVecEditDlg m_Dlg = null;

		const int SINGLE_Select_Path_Count = 1;

		ToolVecEditRender m_ToolVecEditRender; // path

		// composition references
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;
		SelectPathAction m_PathIndexAction;

		bool m_IsSingleSelect = true;

		// language manager
		const string CANNOT_GET_GEOM_DATA = "無法獲取路徑 {0} 的幾何資訊";
		const string CANNOT_GET_CRAFT_DATA = "無法獲取路徑 {0} 的加工資訊";
		const string CANNOT_GET_MACHINE_DATA = "無法獲得機構資訊";
		const string PLEASE_SELECT_PATH = "請先選擇路徑";
		const string CANNOT_SIMULATE_POSTURE = "無法順利模擬該點姿態";
		const string CANNOT_GET_TOOL_VECTOR_INFO = "無法獲取目標點的工具向量資訊";
		const string CANNOT_GET_MACHINE_INFO = "無法獲取機台資訊";
		const string CANNOT_SET_SAME_MS_FOR_MULTI_FIXDIR = "多條路徑無法設置相同曲面法向角度，請改為單選";
		const string PATH_CANNOT_SELECT = "選取到的路徑不支援孔姿態";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "SpecialToolVecEditAction" );

		#endregion

		void OnFormClose()
		{
			End();
			return;
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

		void OnPathSelectionChanged()
		{
			if( m_PathIndexAction == null ) {
				End();
				return;
			}

			// clear all tool vec
			m_ToolVecEditRender?.Remove();

			// get current selected path IDs from SelectPathAction
			List<string> selectedPathIDs = m_PathIndexAction.GetSelectedIDs();
			if( selectedPathIDs == null || selectedPathIDs.Count == 0 ) {
				m_CurrentPathIDList.Clear();
				RefreshSimuResult();
				return;
			}

			// trigger IK optimization
			NeedOptimizedIK?.Invoke();

			// update current path ID list
			m_CurrentPathIDList = selectedPathIDs;

			// update single select flag based on selection count
			m_IsSingleSelect = selectedPathIDs.Count == SINGLE_Select_Path_Count;
			UpdateDialogUI();
			m_ToolVecEditRender = new ToolVecEditRender( m_Viewer, m_DataManager, m_CurrentPathIDList, true );
			RefreshSimuResult();
		}

		void SelectPathAlert()
		{
			MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "PathCannotSelect", PATH_CANNOT_SELECT ) ), MyApp.NoticeType.Warning );
		}

		#region Select action trigger UI change

		void HandlePathSelection()
		{
			if( IsNoPathSelected() ) {
				return;
			}
			bool isSameParam = IsPathSameParam();

			// not all path with same param
			if( isSameParam == false ) {
				SpecialToolVecData specialToolVec = new SpecialToolVecData();

				// set UI to default value
				SetDlgSettingForSameType( specialToolVec );
				SetParamUI( false );
				return;
			}

			bool isGotSpecialToolVecData = GetPathSpecialToolVecData( m_CurrentPathIDList.First(), out SpecialToolVecData specialToolVecData );
			if( !isGotSpecialToolVecData ) {
				return;
			}
			SetDlgSettingForSameType( specialToolVecData );
			SetParamUI( isSameParam );
		}

		void SetDlgSettingForSameType( SpecialToolVecData specialToolVecData )
		{
			SwitchDlgType( specialToolVecData.Type );
			ApplyDlgUIPermissions( specialToolVecData.Type );
		}

		// - Normal   : MSDeg disabled, SwitchDir disabled
		// - AssignMS : MSDeg enabled, SwitchDir enabled
		// - FixDir / ZDir : MSDeg enabled only when single path is selected, SwitchDir disabled
		void ApplyDlgUIPermissions( ESpecialToolVecType type )
		{
			if( m_Dlg == null ) {
				return;
			}
			bool enableMSDeg;
			bool enableSwitchDir;
			switch( type ) {
				case ESpecialToolVecType.AssignMS:
					enableMSDeg = true;
					enableSwitchDir = true;
					break;
				case ESpecialToolVecType.FixDir:
				case ESpecialToolVecType.ZDir:
					enableMSDeg = m_IsSingleSelect;
					enableSwitchDir = false;
					break;
				case ESpecialToolVecType.Normal:
				default:
					enableMSDeg = false;
					enableSwitchDir = false;
					break;
			}
			m_Dlg.SetUIPermissions( enableMSDeg, enableSwitchDir );
		}

		#endregion

		void UpdateDialogUI()
		{
			// dialog must be created
			if( m_Dlg == null ) {
				End();
				return;
			}
			HandlePathSelection();
			return;
		}
		void SetUIMSAsFirstPathFirstPoint()
		{
			if( IsNoPathSelected() ) {
				return;
			}
			if( !DataGettingHelper.GetContourCacheByID( m_CurrentPathIDList.First(), out ContourCache contourCache ) ) {
				return;
			}
			m_Dlg.ResetMSAngle( contourCache.MainPathPointList.First().ModMaster_rad * RAD2DEG, contourCache.MainPathPointList.First().ModSlave_rad * RAD2DEG );
		}

		#region Multi select UI setting

		// trigger by select change
		void SetParamUI( bool isSameParam = true )
		{
			if( IsNoPathSelected() ) {
				return;
			}
			SetMSSilent( isSameParam );
		}

		bool IsPathSameParam()
		{
			if( m_CurrentPathIDList == null ) {
				return false;
			}
			if( m_IsSingleSelect ) {
				return true;
			}
			bool isGotSpecialToolVecData = GetPathSpecialToolVecData( m_CurrentPathIDList.First(), out SpecialToolVecData firstSpecialToolVecData );
			if( !isGotSpecialToolVecData ) {
				return false;
			}
			for( int i = 1; i < m_CurrentPathIDList.Count(); i++ ) {
				if( GetPathSpecialToolVecData( m_CurrentPathIDList[ i ], out SpecialToolVecData specialToolVecData ) == false ) {
					return false;
				}
				if( specialToolVecData.Type != firstSpecialToolVecData.Type ) {
					return false;
				}

				// fixdir and zdir have same ms as 0, so only assignMS will have diff ms
				if( specialToolVecData.MasterAngle_deg != firstSpecialToolVecData.MasterAngle_deg ) {
					return false;
				}
				if( specialToolVecData.SlaveAngle_deg != firstSpecialToolVecData.SlaveAngle_deg ) {
					return false;
				}
			}
			return true;
		}

		#endregion

		#region Set param
		void SwitchDlgType( ESpecialToolVecType type )
		{
			m_Dlg.SwitchType( type );
			m_Type = type;
		}

		// change the type as AssignMS while keeping the value the user just entered
		void Change2AssignMSIfNeeded()
		{
			if( m_Dlg == null ) {
				return;
			}
			if( m_IsSingleSelect == false ) {
				return;
			}
			if( m_Type != ESpecialToolVecType.FixDir && m_Type != ESpecialToolVecType.ZDir ) {
				return;
			}
			SwitchDlgType( ESpecialToolVecType.AssignMS );
			ApplyDlgUIPermissions( ESpecialToolVecType.AssignMS );
		}

		void SetAllPathAsFixDirData()
		{
			foreach( string pathID in m_CurrentPathIDList ) {
				if( SelectPathAction.IsStdPatternPath( pathID ) ) {
					continue;
				}
				SpecialToolVecData specialToolVecData = new SpecialToolVecData( ESpecialToolVecType.FixDir, DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
				bool isGotCraftData = DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData );
				if( isGotCraftData ) {
					craftData.SetSpecialToolVecData( specialToolVecData );
				}
			}
		}

		void SetAllPathWithDefaultAssignMSData()
		{
			if( IsNoPathSelected() ) {
				return;
			}
			if( m_CurrentPathIDList.Count == SINGLE_Select_Path_Count ) {
				AssignMSAsCurrentMSAngle();
				return;
			}

			// several path will get z dir with diff ms, so do not need to cal ms, all set as 0
			AssignMSUsingDefaultAngle();
		}

		void AssignMSAsCurrentMSAngle()
		{
			if( m_CurrentPathIDList.Count != SINGLE_Select_Path_Count ) {
				return;
			}

			if( !DataGettingHelper.GetContourCacheByID( m_CurrentPathIDList.First(), out ContourCache contourCache ) ) {
				return;
			}
			SpecialToolVecData specialToolVecData = new SpecialToolVecData( ESpecialToolVecType.AssignMS, contourCache.MainPathPointList.First().ModMaster_rad*RAD2DEG, contourCache.MainPathPointList.First().ModSlave_rad * RAD2DEG );
			bool isGotCraftData = DataGettingHelper.GetCraftDataByID( m_CurrentPathIDList.FirstOrDefault(), out CraftData craftData );
			if( isGotCraftData ) {
				craftData.SetSpecialToolVecData( specialToolVecData );
			}
		}

		void AssignMSUsingDefaultAngle()
		{
			// several path will get z dir with diff ms, so do not need to cal ms, all set as 0
			foreach( var pathID in m_CurrentPathIDList ) {
				if( SelectPathAction.IsStdPatternPath( pathID ) ) {
					continue;
				}
				bool isGotCraftData = DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData );
				if( isGotCraftData ) {
					craftData.SetSpecialToolVecData( new SpecialToolVecData( ESpecialToolVecType.AssignMS, DEFAULT_Angle_Deg, DEFAULT_Angle_Deg ) );
				}
			}
		}

		void SetAllPathAsNormalParam()
		{
			if( m_CurrentPathIDList == null || m_CurrentPathIDList.Count == 0 ) {
				return;
			}
			foreach( var pathID in m_CurrentPathIDList ) {
				if( SelectPathAction.IsStdPatternPath( pathID ) ) {
					continue;
				}
				SpecialToolVecData specialData = new SpecialToolVecData();
				bool isGotCraftData = DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData );
				if( !isGotCraftData ) {
					continue;
				}
				craftData.SetSpecialToolVecData( specialData );
			}
		}

		void SetAllPathAsZDirCraftData()
		{
			if( IsNoPathSelected() ) {
				return;
			}
			foreach( var pathID in m_CurrentPathIDList ) {
				if( SelectPathAction.IsStdPatternPath( pathID ) ) {
					continue;
				}
				bool isGotCraftData = DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData );
				if( isGotCraftData ) {
					SpecialToolVecData specialData = new SpecialToolVecData( ESpecialToolVecType.ZDir, DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
					craftData.SetSpecialToolVecData( specialData );
				}
			}
		}

		#endregion

		#region Dlg param change

		void OnMSAngleChanged( double masterAngle_deg, double slaveAngle_deg )
		{
			if( !CheckPathIsSelect() ) {
				return;
			}
			Change2AssignMSIfNeeded();
			foreach( string pathID in m_CurrentPathIDList ) {
				if( SelectPathAction.IsStdPatternPath( pathID ) ) {
					continue;
				}
				bool isGotCraftData = DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData );
				if( !isGotCraftData ) {
					return;
				}
				SpecialToolVecData specialToolVecData = new SpecialToolVecData( m_Type, masterAngle_deg, slaveAngle_deg );
				craftData.SetSpecialToolVecData( specialToolVecData );
			}
			RefreshSimuResult();
		}

		void OnTypeChanged( ESpecialToolVecType type )
		{
			if( !CheckPathIsSelect() ) {
				return;
			}
			m_Type = type;

			// set UI protection
			ApplyDlgUIPermissions( type );

			// set param
			switch( type ) {
				case ESpecialToolVecType.FixDir:
					SetAllPathAsFixDirData();
					break;
				case ESpecialToolVecType.AssignMS:
					SetAllPathWithDefaultAssignMSData();
					break;
				case ESpecialToolVecType.ZDir:
					SetAllPathAsZDirCraftData();
					break;
				case ESpecialToolVecType.Normal:
				default:
					SetAllPathAsNormalParam();
					break;
			}

			// refresh drawer
			RefreshSimuResult();

			// need to refresh UI after simu because it might trigger UpdateSpecialToolVecDataSilently() to change MS value
			SetMSSilent();
		}

		void SetMSSilent( bool isSameParam = true )
		{
			// protection
			if( m_Dlg == null ) {
				return;
			}
			if( IsNoPathSelected() ) {
				return;
			}
			bool isGotFirstCraftData = DataGettingHelper.GetCraftDataByID( m_CurrentPathIDList.First(), out CraftData craftDataFirst );
			if( !isGotFirstCraftData || craftDataFirst == null || craftDataFirst.SpecialToolVecData == null ) {
				return;
			}

			// UI setting
			bool bUseRealMS = m_IsSingleSelect
				? craftDataFirst.SpecialToolVecData.Type != ESpecialToolVecType.Normal
				: ( isSameParam && craftDataFirst.SpecialToolVecData.Type == ESpecialToolVecType.AssignMS );

			if( bUseRealMS ) {
				SetUIMSAsFirstPathFirstPoint();
			}
			else {
				m_Dlg.ResetMSAngle( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
			}
		}

		void OnFlipRotaryAxis( bool isPositive )
		{
			if( IsNoPathSelected() ) {
				return;
			}
			Change2AssignMSIfNeeded();
			SetFlipRotaryAxisData( isPositive );
			SetUIMSAsFirstPathFirstPoint();
			RefreshSimuResult();
		}

		void SetFlipRotaryAxisData( bool isPositive )
		{
			m_Dlg.GetMSAngleFromDialog( out double masterAngle_deg, out double slaveAngle_deg );
			Tuple<double, double> rotated = ToolVecHelper.FlipRotaryAxis( masterAngle_deg, slaveAngle_deg, isPositive );

			foreach( var pathID in m_CurrentPathIDList ) {
				if( SelectPathAction.IsStdPatternPath( pathID ) ) {
					continue;
				}
				bool isGotCraftDataLoop = DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftDataLoop );
				if( !isGotCraftDataLoop ) {
					continue;
				}
				SpecialToolVecData specialToolVecDataLoop = new SpecialToolVecData( m_Type, rotated.Item1, rotated.Item2 );
				craftDataLoop.SetSpecialToolVecData( specialToolVecDataLoop );
			}
		}

		void OnRotateToFixDir()
		{
			if( IsNoPathSelected() ) {
				return;
			}
			if( m_IsSingleSelect == false ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "MutiPathCantSetSameMSForFixDir", CANNOT_SET_SAME_MS_FOR_MULTI_FIXDIR ), MyApp.NoticeType.Warning );
				return;
			}
			bool isGotDefaultMS = GetMSForFixDirMode( m_CurrentPathIDList.First(), out Tuple<double, double> MSAngle );
			if( !isGotDefaultMS ) {
				return;
			}
			SpecialToolVecData specialToolVecData = new SpecialToolVecData( ESpecialToolVecType.AssignMS, MSAngle.Item1, MSAngle.Item2 );
			bool isGotCraftData = DataGettingHelper.GetCraftDataByID( m_CurrentPathIDList.First(), out CraftData craftData );
			if( isGotCraftData ) {
				craftData.SetSpecialToolVecData( specialToolVecData );
			}
			SetUIMSAsFirstPathFirstPoint();
			RefreshSimuResult();
		}

		void OnRotateToZDir()
		{
			if( IsNoPathSelected() ) {
				return;
			}

			// serval path will get z dir with diff ms, so do not need to cal ms, all set as 0
			if( m_IsSingleSelect == false ) {
				AssignMSUsingDefaultAngle();
			}

			// is single select
			else {
				bool isGotDefaultMS = GetMSAngleForZDir( m_CurrentPathIDList.First(), out Tuple<double, double> MSAngle );
				bool isGotCraftData = DataGettingHelper.GetCraftDataByID( m_CurrentPathIDList.First(), out CraftData craftData );
				if( isGotDefaultMS && isGotCraftData ) {
					SpecialToolVecData specialData = new SpecialToolVecData( ESpecialToolVecType.AssignMS, MSAngle.Item1, MSAngle.Item2 );
					craftData.SetSpecialToolVecData( specialData );
				}
			}
			SetUIMSAsFirstPathFirstPoint();
			RefreshSimuResult();
		}

		#endregion

		#region MS cal

		bool GetMSForFixDirMode( string pathID, out Tuple<double, double> MSAngle )
		{
			MSAngle = new Tuple<double, double>( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
			if( SelectPathAction.IsStdPatternPath( pathID ) ) {
				return false;
			}
			bool isGotGeomData = DataGettingHelper.GetGeomDataByID( pathID, out IGeomData geomData );
			if( !isGotGeomData ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetGeomData", CANNOT_GET_GEOM_DATA ), pathID ), MyApp.NoticeType.Warning );
				return false;
			}
			bool isGotCraftData = DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData );
			if( !isGotCraftData ) {
				return false;
			}

			// get average tool vec
			gp_Dir avgDir = ( geomData as ContourGeomData ).RefCenterDir.Direction();
			if( craftData.IsToolVecReverse ) {
				avgDir = new gp_Dir( avgDir.Reversed().XYZ() );
			}

			bool isGetCache = DataGettingHelper.GetContourCacheByID( pathID, out ContourCache contourCache );
			if( !isGetCache ) {
				return false;
			}

			// to get start pnt init MS
			CAMPoint PathFirstPnt = contourCache.MainPathPointList.First().Clone();

			// get ms angle
			Tuple<double, double> avgMS_deg = ToolVecHelper.GetMSAngleFromToolVec( avgDir, PathFirstPnt );
			MSAngle = avgMS_deg;
			return true;
		}

		bool GetMSAngleForZDir( string pathID, out Tuple<double, double> MSAngle )
		{
			MSAngle = new Tuple<double, double>( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );

			gp_Dir ZDir = new gp_Dir( 0, 0, 1 );

			bool isGetCache = DataGettingHelper.GetContourCacheByID( pathID, out ContourCache contourCache );
			if( !isGetCache ) {
				return false;
			}

			// to get start pnt init MS
			CAMPoint PathFirstPnt = contourCache.MainPathPointList.First().Clone();

			// get ms angle
			Tuple<double, double> msAngle_Deg = ToolVecHelper.GetMSAngleFromToolVec( ZDir, PathFirstPnt );
			MSAngle = msAngle_Deg;
			return true;
		}

		#endregion

		#region Get data

		bool GetPathSpecialToolVecData( string pathID, out SpecialToolVecData specialToolVecData )
		{
			specialToolVecData = new SpecialToolVecData();
			if( string.IsNullOrEmpty( pathID ) ) {
				return false;
			}
			bool isGotCraftData = DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData );
			if( !isGotCraftData ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetCraftData", CANNOT_GET_CRAFT_DATA ), pathID ), MyApp.NoticeType.Warning );
				return false;
			}
			specialToolVecData = craftData.SpecialToolVecData;
			return true;
		}

		#endregion

		#region Simulation

		void RefreshSimuResult()
		{
			if( m_CurrentPathIDList.Count() == 0 ) {
				OnlyShowSelectedToolVec();
				return;
			}

			// protection
			bool isGotMachineData = DataGettingHelper.GetMachineData( out FiveAxisMachineData machineData );
			if( !isGotMachineData ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotGetMachineData", CANNOT_GET_MACHINE_DATA ), MyApp.NoticeType.Warning );
				return;
			}
			if( m_CurrentPathIDList == null || m_CurrentPathIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "PleaseSelectPath", PLEASE_SELECT_PATH ), MyApp.NoticeType.Warning );
				return;
			}

			bool isGotDefaultMS = GetPathSpecialToolVecData( m_CurrentPathIDList.First(), out SpecialToolVecData specialToolVecData );
			if( !isGotDefaultMS ) {
				return;
			}

			CAMPoint startPnt = CacheHelper.GetMainPathStartPoint( m_CurrentPathIDList.First() );
			bool isGotSpecialToolVecData = GetSimulationData( machineData, out Dictionary<MachineComponentType, List<gp_Trsf>> frameTransMap, startPnt );
			if( isGotSpecialToolVecData == false || ValidateFrameTransformMap( frameTransMap ) == false ) {
				return;
			}

			// single select mode
			if( m_IsSingleSelect ) {

				// do not need to simulation
				if( specialToolVecData.Type == ESpecialToolVecType.Normal ) {
					OnlyShowSelectedToolVec();
					return;
				}

				// false means pause the viewer, because m_ToolVecEditRender will cause viewer flash
				RaiseTrans?.Invoke( frameTransMap, false );
				m_ToolVecEditRender.ShowTrans( frameTransMap[ MachineComponentType.WorkPiece ].Last() );
				EnableViewImmediateUpdate();
				m_Viewer.UpdateView();
				return;
			}

			// multi and assignMS as same param
			if( IsPathSameParam() && specialToolVecData.Type == ESpecialToolVecType.AssignMS ) {

				// false means pause the viewer, because m_ToolVecEditRender will cause viewer flash
				RaiseTrans?.Invoke( frameTransMap, false );
				m_ToolVecEditRender.ShowTrans( frameTransMap[ MachineComponentType.WorkPiece ].Last() );
				EnableViewImmediateUpdate();
				m_Viewer.UpdateView();
				return;
			}

			// multi select with diff param or all paths are normal type
			OnlyShowSelectedToolVec();
		}

		void OnlyShowSelectedToolVec()
		{
			HideMachine?.Invoke();
			m_ToolVecEditRender?.Show();

			// put workpiece back to untrasform location
			ResetPartUI?.Invoke();
			m_Viewer.UpdateView();
		}

		void EnableViewImmediateUpdate()
		{
			V3d_View view = m_Viewer.GetView();
			view.SetImmediateUpdate( true );
		}

		bool ValidateFrameTransformMap( Dictionary<MachineComponentType, List<gp_Trsf>> frameTransformMap )
		{
			if( frameTransformMap == null
				|| !frameTransformMap.ContainsKey( MachineComponentType.WorkPiece )
					|| frameTransformMap[ MachineComponentType.WorkPiece ] == null
					|| frameTransformMap[ MachineComponentType.WorkPiece ].Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotSimulatePosture", CANNOT_SIMULATE_POSTURE ), MyApp.NoticeType.Warning );
				return false;
			}
			return true;
		}

		bool GetSimulationData( FiveAxisMachineData machineData, out Dictionary<MachineComponentType, List<gp_Trsf>> frameTransformMap, ISetToolVecPoint targetPnt )
		{
			frameTransformMap = new Dictionary<MachineComponentType, List<gp_Trsf>>();
			if( targetPnt == null ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotGetToolVectorInfo", CANNOT_GET_TOOL_VECTOR_INFO ), MyApp.NoticeType.Warning );
				return false;
			}
			if( machineData == null ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotGetMachineInfo", CANNOT_GET_MACHINE_INFO ), MyApp.NoticeType.Warning );
				return false;
			}

			double dMasterAngle_rad = targetPnt.ModMaster_rad;
			double dSlaveAngle_rad = targetPnt.ModSlave_rad;

			// call the product builder api
			PostPoint orientationPoint = new PostPoint() { Master = dMasterAngle_rad, Slave = dSlaveAngle_rad };
			SimuPathBuilderBase simuPathBuilder = SimuPathBuilderFactory.Create( machineData );
			bool calSuccess = simuPathBuilder.BuildSinglePointFrameTransform(
				machineData,
				targetPnt.Point,
				orientationPoint,
				out frameTransformMap );
			return true;
		}

		#endregion

		#region UI protection check

		bool IsNoPathSelected()
		{
			if( m_CurrentPathIDList == null || m_CurrentPathIDList.Count == 0 ) {
				return true;
			}
			return false;
		}

		bool CheckPathIsSelect()
		{
			if( IsNoPathSelected() ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "PleaseSelectPath", PLEASE_SELECT_PATH ), MyApp.NoticeType.Warning );
				return false;
			}
			return true;
		}

		#endregion
	}
}


