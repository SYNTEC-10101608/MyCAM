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

		public SpecialToolVecEditAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, List<string> pathIDList, SelectPathAction pathIndexAction )
			: base( dataManager, pathIDList )
		{
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;

			// for path index control
			m_PathIndexAction = pathIndexAction;
			m_CurrentPathIDList = pathIDList;
			m_IsSingleSelcet = pathIDList.Count == SINGLE_Select_Path_Count;
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
			UpdateDialogUI();
			m_Dlg.Show();

			// start path index action (disallow selecting std pattern paths in this action)
			m_PathIndexAction.CanSelectStdPattern = false;
			m_PathIndexAction.Start();
			m_PathIndexAction.SelectionChange += OnPathSelectionChanged;

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
			m_PathIndexAction.End();
			m_ToolVecEditRender.Remove();

			if( m_Dlg != null ) {
				m_Dlg.MSAngleChanged = null;
				m_Dlg.TypeChanged = null;
				m_Dlg.Confirm = null;
				m_Dlg.Cancel = null;
				m_Dlg.FlipRotaryAxis = null;
				m_Dlg.RotateToFixDir = null;
				m_Dlg.Close();
				m_Dlg = null;
			}
			RaiseActionStart?.Invoke( false, true );
			base.End();
		}

		#region Property area

		ESpecialToolVecType m_Type = ESpecialToolVecType.Normal;
		const double DEFAULT_Angle_Deg = 0;

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

		bool m_IsSingleSelcet = true;

		// language manager
		const string CANNOT_GET_GEOM_DATA = "無法獲取路徑 {0} 的幾何資訊";
		const string CANNOT_GET_CRAFT_DATA = "無法獲取路徑 {0} 的加工資訊";
		const string CANNOT_GET_MACHINE_DATA = "無法獲得機構資訊";
		const string PLEASE_SELECT_PATH = "請先選擇路徑";
		const string CANNOT_SIMULATE_POSTURE = "無法順利模擬該點姿態";
		const string CANNOT_GET_TOOL_VECTOR_INFO = "無法獲取目標點的工具向量資訊";
		const string CANNOT_GET_MACHINE_INFO = "無法獲取機台資訊";
		const string CANNOT_SET_SAME_MS_FOR_MULTI_FIXDIR = "多條路徑無法設置相同曲面法向角度，請改為單選";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "SpecialToolVecEditAction" );

		#endregion

		void OnFormClose()
		{
			End();
			return;
		}

		void OnPathSelectionChanged()
		{
			if( m_PathIndexAction == null ) {
				End();
				return;
			}
			m_ToolVecEditRender?.Remove();
			// get current selected path IDs from SelectPathAction
			List<string> selectedPathIDs = m_PathIndexAction.GetSelectedIDs();
			if( selectedPathIDs == null || selectedPathIDs.Count == 0 ) {
				m_CurrentPathIDList.Clear();
				RefreshSimuResult();
				return;
			}

			// update current path ID list
			m_CurrentPathIDList = selectedPathIDs;

			// update single select flag based on selection count
			m_IsSingleSelcet = selectedPathIDs.Count == 1;
			UpdateDialogUI();
			m_ToolVecEditRender = new ToolVecEditRender( m_Viewer, m_DataManager, m_CurrentPathIDList, true );
			RefreshSimuResult();
		}

		void SwitchDlgType( ESpecialToolVecType type )
		{
			m_Dlg.SwitchType( type );
			m_Type = type;
		}

		#region Select action trigger UI change


		void HandlePathSelection()
		{
			if( IsNoPathSelected() ) {
				return;
			}
			bool isSameParam = IsPathSameParam();
			if( isSameParam == false ) {
				SpecialToolVecData specialToolVec = new SpecialToolVecData();
				SetupDlgForSameType( specialToolVec );
				return;
			}

			bool isGotSpecialToolVecData = GetPathSpecialToolVecData( m_CurrentPathIDList.First(), out SpecialToolVecData specialToolVecData );
			if( !isGotSpecialToolVecData ) {
				return;
			}
			SetupDlgForSameType( specialToolVecData );
			SetParamUI();

		}

		void SetupDlgForSameType( SpecialToolVecData specialToolVecData )
		{
			SwitchDlgType( specialToolVecData.Type );
			m_Dlg.SetUIPermissions( specialToolVecData.Type );
		}

		#endregion

		void UpdateDialogUI()
		{
			// dialog must be created
			if( m_Dlg == null ) {
				End();
				return;
			}
			if( IsNoPathSelected() ) {
				return;
			}
			HandlePathSelection();
			return;
		}

		#region Multi select UI setting

		void SetParamUI()
		{
			if( IsNoPathSelected() ) {
				return;
			}
			bool isGotSpecialToolVecData = GetPathSpecialToolVecData( m_CurrentPathIDList.First(), out SpecialToolVecData specialToolVecData );
			if( !isGotSpecialToolVecData ) {
				return;
			}

			// assgin MS need to show MS
			if( specialToolVecData.Type == ESpecialToolVecType.AssignMS ) {
				SetMultiAssignMSUIParam();
			}
		}

		bool IsPathSameParam()
		{
			if( m_CurrentPathIDList == null ) {
				return false;
			}
			if( m_IsSingleSelcet ) {
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

				// assignMS need to compare MS angle
				if( specialToolVecData.Type == ESpecialToolVecType.AssignMS ) {
					if( specialToolVecData.MasterAngle_deg != firstSpecialToolVecData.MasterAngle_deg ) {
						return false;
					}
					if( specialToolVecData.SlaveAngle_deg != firstSpecialToolVecData.SlaveAngle_deg ) {
						return false;
					}
				}
			}
			return true;
		}

		void SetAssignMSParamAndUI()
		{
			if( m_Dlg == null ) {
				return;
			}
			SetAllPathWithDefaultAssignMSData();
			bool isGotFirstCraftData = DataGettingHelper.GetCraftDataByID( m_CurrentPathIDList.First(), out CraftData craftDataFirst );
			if( !isGotFirstCraftData ) {
				return;
			}
			m_Dlg.SetMSAngleSilent( craftDataFirst.SpecialToolVecData.MasterAngle_deg, craftDataFirst.SpecialToolVecData.SlaveAngle_deg );

		}

		// need to check all slected path is same type and same param
		void SetMultiAssignMSUIParam()
		{
			if( m_Dlg == null ) {
				return;
			}
			if( IsNoPathSelected() ) {
				return;
			}
			bool isGotFirstCraftData = DataGettingHelper.GetCraftDataByID( m_CurrentPathIDList.First(), out CraftData craftDataFirst );
			if( !isGotFirstCraftData ) {
				return;
			}
			m_Dlg.SetMSAngleSilent( craftDataFirst.SpecialToolVecData.MasterAngle_deg, craftDataFirst.SpecialToolVecData.SlaveAngle_deg );
		}

		#endregion

		#region Set param

		void SetAllPathAsFixDirData()
		{
			foreach( string pathID in m_CurrentPathIDList ) {
				if( SelectPathAction.IsStdPatternPath( pathID ) ) {
					continue;
				}
				bool isGotDefaultMS = GetMSForFixDirMode( pathID, out Tuple<double, double> MSAngle );
				if( !isGotDefaultMS ) {
					return;
				}
				SpecialToolVecData specialToolVecData = new SpecialToolVecData( ESpecialToolVecType.FixDir, MSAngle.Item1, MSAngle.Item2 );
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
			if( m_CurrentPathIDList.Count == 1 ) {
				bool isGotCraftData = DataGettingHelper.GetCraftDataByID( m_CurrentPathIDList.First(), out CraftData craftData );
				bool isGotDefaultMS = GetMSForAssignMode( m_CurrentPathIDList.First(), out Tuple<double, double> MSAngle );
				if( isGotCraftData && isGotDefaultMS ) {
					craftData.SetSpecialToolVecData( new SpecialToolVecData( ESpecialToolVecType.AssignMS, MSAngle.Item1, MSAngle.Item2 ) );
				}
				return;
			}

			// servial path will get z dir with diff ms, so do not need to cal ms, all set as 0
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
				bool isGotDefaultMS = GetMSForAssignMode( pathID, out Tuple<double, double> MSAngle );
				bool isGotCraftData = DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData );
				if( isGotDefaultMS && isGotCraftData ) {
					SpecialToolVecData specialData = new SpecialToolVecData( ESpecialToolVecType.ZDir, MSAngle.Item1, MSAngle.Item2 );
					craftData.SetSpecialToolVecData( specialData );
				}
			}
		}

		#endregion

		#region Dlg param change

		void OnMSAngleChanged( double masterAngle_deg, double slaveAngle_deg )
		{
			if( !CheckPathIsSelcect() ) {
				return;
			}
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
			RefreshUI();
		}


		void OnTypeChanged( ESpecialToolVecType type )
		{
			if( !CheckPathIsSelcect() ) {
				return;
			}
			m_Type = type;
			switch( type ) {
				case ESpecialToolVecType.FixDir:
					SetAllPathAsFixDirData();
					break;
				case ESpecialToolVecType.AssignMS:
					SetAssignMSParamAndUI();
					break;
				case ESpecialToolVecType.ZDir:
					SetAllPathAsZDirCraftData();
					break;
				case ESpecialToolVecType.Normal:
				default:
					SetAllPathAsNormalParam();
					break;
			}
			RefreshUI();
		}

		void OnFlipRotaryAxis( bool isPositive )
		{
			if( IsNoPathSelected() ) {
				return;
			}
			SetFlipRotaryAxisData( isPositive );

			bool isGotSpecialToolVecData = GetPathSpecialToolVecData( m_CurrentPathIDList.First(), out SpecialToolVecData specialToolVecData );
			if( !isGotSpecialToolVecData ) {
				return;
			}
			m_Dlg.SetMSAngleSilent( specialToolVecData.MasterAngle_deg, specialToolVecData.SlaveAngle_deg );
			RefreshUI();
		}

		void SetFlipRotaryAxisData( bool isPositive )
		{
			// get fisrt path flip result
			bool isGetSpecialDataDone = GetPathSpecialToolVecData( m_CurrentPathIDList.FirstOrDefault(), out SpecialToolVecData specialToolVecData );
			if( isGetSpecialDataDone == false ) {
				return;
			}
			Tuple<double, double> rotated = ToolVecHelper.FlipRotaryAxis( specialToolVecData.MasterAngle_deg, specialToolVecData.SlaveAngle_deg, isPositive );

			foreach( var pathID in m_CurrentPathIDList ) {
				if( SelectPathAction.IsStdPatternPath( pathID ) ) {
					continue;
				}
				bool isGotCraftDataLoop = DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftDataLoop );
				if( !isGotCraftDataLoop ) {
					continue;
				}
				SpecialToolVecData specialToolVecDataLoop = new SpecialToolVecData( specialToolVecData.Type, rotated.Item1, rotated.Item2 );
				craftDataLoop.SetSpecialToolVecData( specialToolVecDataLoop );
			}
		}

		void OnRotateToFixDir()
		{
			if( IsNoPathSelected() ) {
				return;
			}
			if( m_IsSingleSelcet == false ) {
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
			m_Dlg.SetMSAngleSilent( MSAngle.Item1, MSAngle.Item2 );
			RefreshUI();
		}

		#endregion

		#region MS cal

		bool GetMSForFixDirMode( string pathID, out Tuple<double, double> MSAngle )
		{
			if( SelectPathAction.IsStdPatternPath( pathID ) ) {
				MSAngle = new Tuple<double, double>( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
				return false;
			}
			MSAngle = new Tuple<double, double>( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
			bool isGotSpecialToolVecData = DataGettingHelper.GetGeomDataByID( pathID, out IGeomData geomData );

			if( !isGotSpecialToolVecData ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetGeomData", CANNOT_GET_GEOM_DATA ), pathID ), MyApp.NoticeType.Warning );
				return false;
			}
			// get averge tool vec
			gp_Dir avgDir = ( geomData as ContourGeomData ).RefCenterDir.Direction();
			bool isGotCraftData = DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData );
			if( !isGotCraftData ) {
				return false;
			}
			if( craftData.IsToolVecReverse ) {
				avgDir = new gp_Dir( avgDir.Reversed().XYZ() );
			}
			// get start point
			bool isGotStartPnt = GetFirstTimeIKSolvedStartPnt( pathID, out ISetToolVecPoint startPnt );
			if( !isGotStartPnt ) {
				return false;
			}

			// get ms angle
			Tuple<double, double> avgMS_deg = ToolVecHelper.GetMSAngleFromToolVec( avgDir, startPnt );
			MSAngle = avgMS_deg;
			return true;
		}

		bool GetMSForAssignMode( string pathID, out Tuple<double, double> MSAngle )
		{
			MSAngle = new Tuple<double, double>( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );

			gp_Dir ZDir = new gp_Dir( 0, 0, 1 );

			// get start point
			bool isGotStartPnt = GetFirstTimeIKSolvedStartPnt( pathID, out ISetToolVecPoint startPnt );
			if( !isGotStartPnt ) {
				return false;
			}

			// get ms angle
			Tuple<double, double> msAngle_Deg = ToolVecHelper.GetMSAngleFromToolVec( ZDir, startPnt );
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

		bool GetFirstTimeIKSolvedStartPnt( string pathID, out ISetToolVecPoint startPnt )
		{
			startPnt = null;

			// get craft data
			if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetCraftData", CANNOT_GET_CRAFT_DATA ), pathID ), MyApp.NoticeType.Warning );
				return false;
			}

			SpecialToolVecData defaultData = new SpecialToolVecData();
			SpecialToolVecData backupData = new SpecialToolVecData(
				craftData.SpecialToolVecData.Type,
				craftData.SpecialToolVecData.MasterAngle_deg,
				craftData.SpecialToolVecData.SlaveAngle_deg
				);
			craftData.SetSpecialToolVecData( defaultData );
			if( !DataGettingHelper.GetPathCacheByID( pathID, out IPathCache pathCache ) ) {
				return false;
			}
			CAMPoint startPoint = pathCache.MainPathPointList.First();
			startPnt = startPoint;
			craftData.SetSpecialToolVecData( backupData );
			return true;
		}

		#endregion

		#region Simulation

		void RefreshSimuResult()
		{
			if( m_CurrentPathIDList.Count() == 0 ) {
				HideMachine?.Invoke();
				m_ToolVecEditRender?.Show();
				m_Viewer.UpdateView();
			}

			bool isGotMachineData = DataGettingHelper.GetMachineData( out MachineData machineData );
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

			if( specialToolVecData.Type == ESpecialToolVecType.Normal ) {
				HideMachine?.Invoke();
				m_ToolVecEditRender?.Show();
				m_Viewer.UpdateView();
				return;
			}

			// multi select with diff dir
			if( m_IsSingleSelcet == false && specialToolVecData.Type != ESpecialToolVecType.AssignMS ) {
				HideMachine?.Invoke();
				m_ToolVecEditRender?.Show();
				m_Viewer.UpdateView();
				return;
			}

			CAMPoint startPnt = CacheHelper.GetMainPathStartPoint( m_CurrentPathIDList.First() );
			bool isGotSpecialToolVecData = GetSimulationData( machineData, out Dictionary<MachineComponentType, List<gp_Trsf>> frameTransMap, startPnt );
			if( !isGotSpecialToolVecData ) {
				return;
			}
			if( !ValidateFrameTransformMap( frameTransMap ) ) {
				return;
			}

			if( m_IsSingleSelcet ) {

				// false means pause the viewer, because m_ToolVecEditRender will cause viewer flash
				RaiseTrans?.Invoke( frameTransMap, false );
				m_ToolVecEditRender.ShowTrans( frameTransMap[ MachineComponentType.WorkPiece ].Last() );
				EnableViewImmediateUpdate();
			}
			else {

				// multi and assignMS as same param
				if( IsPathSameParam() ) {
					// false means pause the viewer, because m_ToolVecEditRender will cause viewer flash
					RaiseTrans?.Invoke( frameTransMap, false );
					m_ToolVecEditRender.ShowTrans( frameTransMap[ MachineComponentType.WorkPiece ].Last() );
					EnableViewImmediateUpdate();
				}
			}
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

		bool GetSimulationData( MachineData machineData, out Dictionary<MachineComponentType, List<gp_Trsf>> frameTransformMap, ISetToolVecPoint targetPnt )
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

			// call the new static API
			bool calSuccess = SimulationHelper.BuildSinglePointFrameTransform(
				machineData,
				targetPnt.Point,
				dMasterAngle_rad,
				dSlaveAngle_rad,
				m_DataManager.WorkPieceChain,
				m_DataManager.MachineChainListMap,
				out frameTransformMap );
			return true;
		}
		void RefreshUI()
		{
			PropertyChanged?.Invoke( m_CurrentPathIDList );
			RefreshSimuResult();
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

		bool CheckPathIsSelcect()
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


