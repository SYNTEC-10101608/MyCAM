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
	internal class LockABAction : EditActionBase
	{
		public Action<Dictionary<MachineComponentType, List<gp_Trsf>>, bool> RaiseTrans;
		public Action<bool, bool> RaiseActionStart;
		public Action HideMachine;
		public Action ResetPartUI;
		public Action NeedOptimizedIK;

		public LockABAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, List<string> pathIDList, SelectPathAction pathIndexAction )
			: base( dataManager )
		{
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;

			// for path index control
			m_PathIndexAction = pathIndexAction;
			m_CurrentPathIDList = pathIDList ?? new List<string>();
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
			if( !DataGettingHelper.GetMachineData( out m_MachineData ) ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotGetMachineInfo", CANNOT_GET_MACHINE_INFO ), MyApp.NoticeType.Warning );
				End();
				return;
			}
			m_RotaryAxisConfig = m_MachineData.RotaryAxisConfig;
			m_Dlg = new LockMSAxisDlg( m_RotaryAxisConfig, m_MachineData.FiveAxisType );
			m_Dlg.Cancel += End;
			m_Dlg.OnNormalMode += OnModeChanged;
			m_Dlg.MasterChanged += ( master_deg ) => OnMasterChange( master_deg );
			m_Dlg.SlaveChanged += ( slave_deg ) => OnSlaveChange( slave_deg );
			m_Dlg.checkLockMasterChanged += ( isChecked ) => OnLockMasterCheckedChanged( isChecked );
			m_Dlg.checkLockSlaveChanged += ( isChecked ) => OnLockSlaveCheckedChanged( isChecked );
			m_Dlg.FlipRotaryAxis += ( isFlip ) => OnFlipRotary( isFlip );
			m_Dlg.MachiningDirChanged += ( machiningDir ) => OnMachiningDirChanged( machiningDir );
			HandlePathSelection();


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
			}
			m_Dlg.Show( MyApp.MainForm );
			RefreshSimuResult();
		}

		public override void End()
		{
			// cleanup path index action
			m_PathIndexAction.SelectionChange -= OnPathSelectionChanged;
			m_PathIndexAction.WithPathCantSelect -= SelectPathAlert;
			m_PathIndexAction.End();
			m_ToolVecEditRender?.Remove();

			if( m_Dlg != null ) {
				m_Dlg.Cancel -= End;
				m_Dlg.OnNormalMode -= OnModeChanged;
				m_Dlg.MasterChanged -= ( master_deg ) => OnMasterChange( master_deg );
				m_Dlg.SlaveChanged -= ( slave_deg ) => OnSlaveChange( slave_deg );
				m_Dlg.checkLockMasterChanged -= ( isChecked ) => OnLockMasterCheckedChanged( isChecked );
				m_Dlg.checkLockSlaveChanged -= ( isChecked ) => OnLockSlaveCheckedChanged( isChecked );
				m_Dlg.FlipRotaryAxis -= ( isFlip ) => OnFlipRotary( isFlip );
				m_Dlg.MachiningDirChanged -= ( machiningDir ) => OnMachiningDirChanged( machiningDir );
				m_Dlg.Close();
				m_Dlg.Dispose();
				m_Dlg = null;
			}

			RaiseActionStart?.Invoke( false, true );
			base.End();
		}

		#region Select event

		void OnPathSelectionChanged()
		{
			if( m_PathIndexAction == null ) {
				End();
				return;
			}

			// clear all tool vec
			m_ToolVecEditRender?.Remove();
			m_ToolVecEditRender = null;

			// get current selected path IDs from SelectPathAction
			List<string> selectedPathIDs = m_PathIndexAction.GetSelectedIDs();

			// update current path ID list
			m_CurrentPathIDList = selectedPathIDs;

			if( IsNoPathSelected() ) {

				// do not clear in place, the list is owned by the caller
				m_CurrentPathIDList = new List<string>();
				HandlePathSelection();
				RefreshSimuResult();
				return;
			}

			// trigger IK optimization
			NeedOptimizedIK?.Invoke();

			// change dialog UI
			HandlePathSelection();

			// render tool vec on viewer
			m_ToolVecEditRender = new ToolVecEditRender( m_Viewer, m_DataManager, m_CurrentPathIDList, true );
			RefreshSimuResult();
		}

		// the param for UI setting
		void SetMode()
		{
			if( m_Dlg == null ) {
				End();
			}
			if( IsNoPathSelected() ) {
				m_IsNormalMode = true;
				return;
			}
			if( IsPathSameParam() == false ) {
				m_IsNormalMode = true;
				return;
			}
			// same param
			bool GetSuccess = GetPathSpecialToolVecData( m_CurrentPathIDList.First(), out SpecialToolVecData specialToolVecData );

			// every path has same param as no lock any axis, so treat as normal mode
			m_IsNormalMode = specialToolVecData.Mode == ToolVecMode.Interpolate;
		}

		// switch dialog type and sync MS UI
		void HandlePathSelection()
		{
			SetMode();
			SyncDialog();
		}

		void OnModeChanged( bool isNormal )
		{
			if( IsNoPathSelected() ) {
				return;
			}

			// set value
			foreach( string pathID in m_CurrentPathIDList ) {

				if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
					continue;
				}

				// lock axis
				if( isNormal == false ) {
					craftData.SetSpecialToolVecData( new SpecialToolVecData() );
					craftData.SpecialToolVecData.LockMaster = true;
					craftData.SpecialToolVecData.LockSlave = true;
				}
				else {
					craftData.SetSpecialToolVecData( new SpecialToolVecData() );
				}
			}
			SetMode();
			SyncDialog();
			RefreshSimuResult();
		}

		void SelectPathAlert()
		{
			MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "PathCannotSelect", PATH_CANNOT_SELECT ) ), MyApp.NoticeType.Warning );
		}

		#endregion

		#region UI Setting

		void SyncDialog()
		{
			if( m_Dlg == null ) {
				End();
				return;
			}
			m_Dlg.SwitchTypeAsNormal( m_IsNormalMode );
			ApplyDlgUIPermissions();
			SyncDlgMachiningDir();
			SetMSSilent();
		}

		// show the real posture only when every selected path agrees on it, otherwise show 0
		void SetMSSilent()
		{
			// protection
			if( m_Dlg == null ) {
				return;
			}
			if( IsNoPathSelected() || m_IsNormalMode ) {
				m_Dlg.ResetMSAngle( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
				return;
			}

			if( m_IsSingleSelect ) {
				SetSinglePathMSValue();
				return;
			}
			SetMultiPathMSValue();
		}

		void SetSinglePathMSValue()
		{
			// protection
			if( m_Dlg == null ) {
				return;
			}

			// normal type, show 0
			if( m_IsNormalMode ) {
				m_Dlg.ResetMSAngle( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
				return;
			}

			bool bSuccess = GetPathSpecialToolVecData( m_CurrentPathIDList.First(), out SpecialToolVecData specialToolVecData );
			bool bGetDefaultDone = GetDefaultLockABValue( m_CurrentPathIDList.First(), out Tuple<double, double> DefaultLockAngle );
			if( bSuccess == false || bGetDefaultDone == false ) {
				m_Dlg.ResetMSAngle( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
				return;
			}
			double dMaster = specialToolVecData.MasterAngle_deg == null ? DefaultLockAngle.Item1 : specialToolVecData.MasterAngle_deg.Value;
			double dSlave = specialToolVecData.SlaveAngle_deg == null ? DefaultLockAngle.Item2 : specialToolVecData.SlaveAngle_deg.Value;

			if( specialToolVecData.MachiningDir == EMachiningDir.None ) {
				m_Dlg.ResetMSAngle( dMaster, dSlave, specialToolVecData.MasterAngle_deg.HasValue, specialToolVecData.SlaveAngle_deg.HasValue );
			}
			else {
				CAMPoint startPnt = CacheHelper.GetMainPathStartPoint( m_CurrentPathIDList.First() );
				m_Dlg.ResetMSAngle( startPnt.ModMaster_rad * 180.0 / Math.PI, startPnt.ModSlave_rad * 180.0 / Math.PI );
			}
		}

		void SetMultiPathMSValue()
		{
			// protection
			if( m_Dlg == null ) {
				return;
			}
			if( m_IsNormalMode ) {
				m_Dlg.ResetMSAngle( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
				return;
			}

			bool bSuccess = GetPathSpecialToolVecData( m_CurrentPathIDList.First(), out SpecialToolVecData specialToolVecData );
			bool bGetDefaultDone = GetDefaultLockABValue( m_CurrentPathIDList.First(), out Tuple<double, double> DefaultLockAngle );
			if( bSuccess == false || bGetDefaultDone == false ) {
				m_Dlg.ResetMSAngle( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
				return;
			}

			double dMaster = specialToolVecData.MasterAngle_deg.HasValue ? specialToolVecData.MasterAngle_deg.Value : DEFAULT_Angle_Deg;
			double dSlave = specialToolVecData.SlaveAngle_deg.HasValue ? specialToolVecData.SlaveAngle_deg.Value : DEFAULT_Angle_Deg;
			switch( specialToolVecData.Mode ) {
				case ToolVecMode.AssignMS:
					m_Dlg.ResetMSAngle( dMaster, dSlave, true, true );
					break;
				case ToolVecMode.LockMasterWithAssign:
					m_Dlg.ResetMSAngle( dMaster, DEFAULT_Angle_Deg, true, false );
					break;
				case ToolVecMode.LockSlaveWithAssign:
					m_Dlg.ResetMSAngle( DEFAULT_Angle_Deg, dSlave, false, true );
					break;
				case ToolVecMode.LockMasterWithoutAssignButAssignSlave:
					m_Dlg.ResetMSAngle( DEFAULT_Angle_Deg, dSlave, false, true );
					break;
				case ToolVecMode.LockSlaveWithoutAssignButAssignMaster:
					m_Dlg.ResetMSAngle( dMaster, DEFAULT_Angle_Deg, true, false );
					break;

				// MachineDir, LockDueAxisWithoutAssign, LockMasterWithoutAssign, LockSlaveWithoutAssign, Interpolate
				default:
					m_Dlg.ResetMSAngle( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
					break;
			}

		}

		void SyncDlgMachiningDir()
		{
			if( IsNoPathSelected() || !IsPathSameParam()
				|| !GetPathSpecialToolVecData( m_CurrentPathIDList.First(), out SpecialToolVecData specialToolVecData ) ) {
				m_Dlg.ResetMachiningDir( EMachiningDir.None );
				return;
			}
			m_Dlg.ResetMachiningDir( specialToolVecData.MachiningDir );
		}

		void ApplyDlgUIPermissions()
		{
			if( m_Dlg == null ) {
				End();
				return;
			}
			if( m_IsNormalMode ) {
				m_Dlg.CheckLockMSArea( false, false );
				return;
			}
			if( IsNoPathSelected() ) {
				return;
			}

			// all path with same param, so just get the first path's data
			if( DataGettingHelper.GetCraftDataByID( m_CurrentPathIDList.First(), out CraftData craftData ) ) {
				bool bLockMaster = craftData.SpecialToolVecData.LockMaster;
				bool bLockSlave = craftData.SpecialToolVecData.LockSlave;
				m_Dlg.CheckLockMSArea( bLockMaster, bLockSlave, m_IsSingleSelect );
			}
		}

		void OnlyShowSelectedToolVec()
		{
			HideMachine?.Invoke();
			m_ToolVecEditRender?.Show();

			// put workpiece back to untrasform location
			ResetPartUI?.Invoke();
			m_Viewer.UpdateView();
		}

		#endregion

		#region Simulation fuction

		void RefreshSimuResult()
		{
			if( IsNoPathSelected() ) {
				OnlyShowSelectedToolVec();
				return;
			}

			// not same param or normal type
			if( m_IsNormalMode ) {
				OnlyShowSelectedToolVec();
				return;
			}

			// protection
			bool isGotDefaultMS = GetPathSpecialToolVecData( m_CurrentPathIDList.First(), out SpecialToolVecData specialToolVecData );
			if( !isGotDefaultMS ) {
				return;
			}
			CAMPoint startPnt = CacheHelper.GetMainPathStartPoint( m_CurrentPathIDList.First() );
			bool isGotSpecialToolVecData = GetSimulationData( m_MachineData, out Dictionary<MachineComponentType, List<gp_Trsf>> frameTransMap, startPnt );
			if( isGotSpecialToolVecData == false || ValidateFrameTransformMap( frameTransMap ) == false ) {
				return;
			}

			// is single select or multi select with same param
			if( m_IsSingleSelect || IsPathSameParam() ) {

				// false means pause the viewer, because m_ToolVecEditRender will cause viewer flash
				RaiseTrans?.Invoke( frameTransMap, false );

				m_ToolVecEditRender?.ShowTrans( frameTransMap[ MachineComponentType.WorkPiece ].Last() );
			}
			else {
				OnlyShowSelectedToolVec();
			}
			EnableViewImmediateUpdate();
			m_Viewer.UpdateView();
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

		void EnableViewImmediateUpdate()
		{
			V3d_View view = m_Viewer.GetView();
			view.SetImmediateUpdate( true );
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
			PostPoint orientationPoint = new PostPoint() { Master = dMasterAngle_rad, Slave = dSlaveAngle_rad, ExternalAxisPos = targetPnt.ExternalAxisPos };
			SimuPathBuilderBase simuPathBuilder = SimuPathBuilderFactory.Create( machineData );
			bool calSuccess = simuPathBuilder.BuildSinglePointFrameTransform(
				machineData,
				targetPnt.Point,
				orientationPoint,
				out frameTransformMap );
			if( !calSuccess ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotSimulatePosture", CANNOT_SIMULATE_POSTURE ), MyApp.NoticeType.Warning );
				return false;
			}
			return true;
		}

		#endregion

		#region Get Data

		bool TryGetPathDefaultMS( string pathID, out Tuple<double, double> msAngle_deg )
		{
			msAngle_deg = new Tuple<double, double>( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
			if( !DataGettingHelper.GetContourCacheByID( pathID, out ContourCache contourCache ) ) {
				return false;
			}
			msAngle_deg = new Tuple<double, double>( contourCache.DefaultLockABAngle_deg.Item1, contourCache.DefaultLockABAngle_deg.Item2 );
			return true;
		}

		bool GetPathSpecialToolVecData( string pathID, out SpecialToolVecData specialToolVecData )
		{
			specialToolVecData = new SpecialToolVecData();
			if( string.IsNullOrEmpty( pathID ) ) {
				return false;
			}
			bool isGotCraftData = DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData );
			if( !isGotCraftData ) {
				int pathOrderIndex = m_DataManager.PathIDList.IndexOf( pathID );
				string pathName = m_DataManager.GetPathName( pathOrderIndex );
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetCraftData", CANNOT_GET_CRAFT_DATA ), pathName ), MyApp.NoticeType.Warning );
				return false;
			}
			specialToolVecData = craftData.SpecialToolVecData;
			return true;
		}

		bool GetDefaultLockABValue( string pathID, out Tuple<double, double> DefaultLockAngle )
		{
			DefaultLockAngle = new Tuple<double, double>( 0, 0 );
			if( !DataGettingHelper.GetContourCacheByID( pathID, out ContourCache contourCache ) ) {
				return false;
			}
			DefaultLockAngle = new Tuple<double, double>( contourCache.DefaultLockABAngle_deg.Item1, contourCache.DefaultLockABAngle_deg.Item2 );
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

		bool IsPathSameParam()
		{
			if( IsNoPathSelected() ) {
				return false;
			}
			if( m_IsSingleSelect ) {
				return true;
			}
			bool isGotSpecialToolVecData = GetPathSpecialToolVecData( m_CurrentPathIDList.First(), out SpecialToolVecData firstSpecialToolVecData );
			if( !isGotSpecialToolVecData ) {
				return false;
			}

			return m_CurrentPathIDList.Skip( 1 ).All( pathID =>
				GetPathSpecialToolVecData( pathID, out SpecialToolVecData specialToolVecData )

				// lock same axis
				&& specialToolVecData.LockMaster == firstSpecialToolVecData.LockMaster
				&& specialToolVecData.LockSlave == firstSpecialToolVecData.LockSlave

				// same ms angle
				&& specialToolVecData.MasterAngle_deg == firstSpecialToolVecData.MasterAngle_deg
				&& specialToolVecData.SlaveAngle_deg == firstSpecialToolVecData.SlaveAngle_deg

				// same assigned machining direction
				&& specialToolVecData.MachiningDir == firstSpecialToolVecData.MachiningDir );
		}

		#endregion

		#region Dlg Event

		void OnMasterChange( double master_deg )
		{
			if( IsNoPathSelected() ) {
				SelectPathAlert();
				return;
			}
			bool isAssigned = true;
			foreach( string pathID in m_CurrentPathIDList ) {
				if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
					isAssigned = false;
					continue;
				}
				craftData.SpecialToolVecData.MasterAngle_deg = master_deg;
				craftData.SpecialToolVecData.LockMaster = true;
				craftData.SpecialToolVecData.MachiningDir = EMachiningDir.None;
			}
			m_Dlg.ShowMAngleAsAssignMode( isAssigned );
			m_Dlg.ResetMachiningDir( EMachiningDir.None );
			RefreshSimuResult();
		}

		void OnSlaveChange( double slave_deg )
		{
			if( IsNoPathSelected() ) {
				SelectPathAlert();
				return;
			}
			bool isAssigned = true;
			foreach( string pathID in m_CurrentPathIDList ) {
				if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
					isAssigned = false;
					continue;
				}
				craftData.SpecialToolVecData.SlaveAngle_deg = slave_deg;
				craftData.SpecialToolVecData.LockSlave = true;
				craftData.SpecialToolVecData.MachiningDir = EMachiningDir.None;
			}
			m_Dlg.ShowSAngleAsAssignMode( isAssigned );
			m_Dlg.ResetMachiningDir( EMachiningDir.None );
			RefreshSimuResult();
		}

		void OnLockSlaveCheckedChanged( bool isChecked )
		{
			if( IsNoPathSelected() ) {
				SelectPathAlert();
				return;
			}
			m_Dlg.EnableLockSArea( isChecked, m_IsSingleSelect );

			// set param
			foreach( string pathID in m_CurrentPathIDList ) {
				TryGetPathDefaultMS( pathID, out Tuple<double, double> defaultMSValue );
				if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
					continue;
				}
				craftData.SpecialToolVecData.SlaveAngle_deg = null;
				craftData.SpecialToolVecData.MachiningDir = EMachiningDir.None;
				if( isChecked ) {
					craftData.SpecialToolVecData.LockSlave = true;
				}
				else {
					craftData.SpecialToolVecData.LockSlave = false;
				}
			}
			SetMSSilent();
			m_Dlg.ResetMachiningDir( EMachiningDir.None );
			RefreshSimuResult();
		}

		void OnLockMasterCheckedChanged( bool isChecked )
		{
			if( m_CurrentPathIDList == null || m_CurrentPathIDList.Count == 0 ) {
				SelectPathAlert();
				return;
			}
			m_Dlg.EnableLockMArea( isChecked );

			foreach( string pathID in m_CurrentPathIDList ) {
				TryGetPathDefaultMS( pathID, out Tuple<double, double> defaultMSValue );
				if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
					continue;
				}
				craftData.SpecialToolVecData.MachiningDir = EMachiningDir.None;
				craftData.SpecialToolVecData.MasterAngle_deg = null;
				if( isChecked ) {
					craftData.SpecialToolVecData.LockMaster = true;
				}
				else {
					craftData.SpecialToolVecData.LockMaster = false;
				}
			}
			SetMSSilent();
			m_Dlg.ResetMachiningDir( EMachiningDir.None );
			RefreshSimuResult();
		}

		void OnFlipRotary( bool IsFlipToPositive )
		{
			if( IsNoPathSelected() || m_IsSingleSelect == false ) {
				SelectPathAlert();
				return;
			}

			if( DataGettingHelper.GetCraftDataByID( m_CurrentPathIDList.First(), out CraftData craftData ) ) {
				m_Dlg.GetMSAngle( out double master_deg, out double slave_deg );
				Tuple<double, double> flipedMSAngle_deg = ResolveAngle( master_deg, slave_deg, IsFlipToPositive );
				craftData.SpecialToolVecData.MachiningDir = EMachiningDir.None;
				craftData.SpecialToolVecData.LockSlave = true;
				craftData.SpecialToolVecData.LockMaster = true;
				craftData.SpecialToolVecData.SlaveAngle_deg = flipedMSAngle_deg.Item2;
				craftData.SpecialToolVecData.MasterAngle_deg = flipedMSAngle_deg.Item1;
				SyncDialog();
				RefreshSimuResult();
			}
		}

		void OnMachiningDirChanged( EMachiningDir dir )
		{
			if( IsNoPathSelected() ) {
				SelectPathAlert();
				return;
			}
			foreach( string pathID in m_CurrentPathIDList ) {
				if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
					continue;
				}
				craftData.SetSpecialToolVecData( new SpecialToolVecData() );
				craftData.SpecialToolVecData.LockMaster = true;
				craftData.SpecialToolVecData.LockSlave = true;
				craftData.SpecialToolVecData.MachiningDir = dir;

			}
			SyncDialog();
			RefreshSimuResult();
		}

		#endregion

		// false means the angle cannot be decided, the caller then writes nothing at all
		Tuple<double, double> ResolveAngle( double master_deg, double slave_deg, bool isPositive )
		{
			Tuple<double, double> msAngle_deg = new Tuple<double, double>( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
			msAngle_deg = ToolVecHelper.FlipRotaryAxis( master_deg, slave_deg, isPositive );
			return msAngle_deg;
		}


		// list for path index action to lock, only contour path can be edited
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

		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;
		SelectPathAction m_PathIndexAction;

		// path index param
		List<string> m_CurrentPathIDList;
		ToolVecEditRender m_ToolVecEditRender;
		bool m_IsNormalMode = true;

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "LockABAction" );

		RotaryAxisConfig m_RotaryAxisConfig = null;

		FiveAxisMachineData m_MachineData = null;

		LockMSAxisDlg m_Dlg = null;

		#region Language string

		const string CANNOT_GET_MACHINE_INFO = "無法獲取機台資訊";
		const string PATH_CANNOT_SELECT = "選取到的路徑不支援孔姿態";
		const string CANNOT_SIMULATE_POSTURE = "無法順利模擬該點姿態";
		const string CANNOT_GET_TOOL_VECTOR_INFO = "無法獲取目標點的工具向量資訊";
		const string CANNOT_GET_CRAFT_DATA = "無法獲取路徑 {0} 的加工資訊";

		#endregion

		const double DEFAULT_Angle_Deg = 0.0;

		const int SINGLE_Select_Path_Count = 1;

		// current selection situation
		bool m_IsSingleSelect
		{
			get
			{
				return m_CurrentPathIDList != null && m_CurrentPathIDList.Count == SINGLE_Select_Path_Count;
			}
		}
	}
}
