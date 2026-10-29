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
	internal class SpecialToolVecEditAction : EditActionBase
	{
		public Action<Dictionary<MachineComponentType, List<gp_Trsf>>, bool> RaiseTrans;
		public Action<bool, bool> RaiseActionStart;
		public Action HideMachine;
		public Action ResetPartUI;
		public Action NeedOptimizedIK;

		public SpecialToolVecEditAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, List<string> pathIDList, SelectPathAction pathIndexAction )
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

			// subscribe to path selection changes
			m_Dlg = new SpecialToolVecEditDlg( m_RotaryAxisConfig );
			m_Dlg.MSAngleChanged += OnMSAngleChanged;
			m_Dlg.TypeChanged += OnTypeChanged;
			m_Dlg.Cancel += OnFormClose;
			m_Dlg.FlipRotaryAxis += OnFlipRotaryAxis;
			m_Dlg.RotateToFixDir += OnRotateToFixDir;
			m_Dlg.RotateToZDir += OnRotateToZDir;
			m_Dlg.MachiningDirChanged += OnMachiningDirChanged;
			HandlePathSelection();
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
			m_ToolVecEditRender?.Remove();

			if( m_Dlg != null ) {
				m_Dlg.MSAngleChanged = null;
				m_Dlg.TypeChanged = null;
				m_Dlg.Confirm = null;
				m_Dlg.Cancel = null;
				m_Dlg.FlipRotaryAxis = null;
				m_Dlg.RotateToFixDir = null;
				m_Dlg.RotateToZDir = null;
				m_Dlg.MachiningDirChanged = null;
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
		FiveAxisMachineData m_MachineData = null;

		SpecialToolVecEditDlg m_Dlg = null;

		const int SINGLE_Select_Path_Count = 1;

		ToolVecEditRender m_ToolVecEditRender; // path

		// composition references
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;
		SelectPathAction m_PathIndexAction;

		// current selection situation
		bool m_IsSingleSelect
		{
			get
			{
				return m_CurrentPathIDList != null && m_CurrentPathIDList.Count == SINGLE_Select_Path_Count;
			}
		}

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

		// every dialog operation goes through, each step decides by itself whether it applies
		enum EEditIntent
		{
			SwitchType,
			EditMSAngle,
			FlipRotaryAxis,
			RotateToFixDir,
			RotateToZDir,
			EditMachiningDir,
		}

		#endregion

		void OnFormClose()
		{
			End();
			return;
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
			if( selectedPathIDs == null || selectedPathIDs.Count == 0 ) {

				// do not clear in place, the list is owned by the caller
				m_CurrentPathIDList = new List<string>();
				HandlePathSelection();
				RefreshSimuResult();
				return;
			}

			// trigger IK optimization
			NeedOptimizedIK?.Invoke();

			// update current path ID list
			m_CurrentPathIDList = selectedPathIDs;
			HandlePathSelection();
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
			// nothing selected
			if( IsNoPathSelected() ) {
				m_Type = ESpecialToolVecType.Normal;
				SyncDialog();
				return;
			}

			// paths with different setting cannot be shown as one, fall back to normal
			if( IsPathSameParam() == false ) {
				m_Type = ESpecialToolVecType.Normal;
				SyncDialog();
				return;
			}
			if( !GetPathSpecialToolVecData( m_CurrentPathIDList.First(), out SpecialToolVecData specialToolVecData ) ) {
				return;
			}

			// the selection changed, the type of the new selection becomes the current one
			m_Type = specialToolVecData.Type;
			SyncDialog();
		}

		// - Normal   : MSDeg disabled, SwitchDir disabled
		// - AssignMS : MSDeg enabled, SwitchDir enabled
		// - FixDir / ZDir : MSDeg enabled only when single path is selected, SwitchDir disabled
		// - LockMaster / LockSlave : the locked angle comes from init IK result of start point
		// - MachiningDir is only meaningful for ZDir
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
				case ESpecialToolVecType.LockMaster:
				case ESpecialToolVecType.LockSlave:
				case ESpecialToolVecType.Normal:
				default:
					enableMSDeg = false;
					enableSwitchDir = false;
					break;
			}
			bool isSpindleType = m_MachineData.FiveAxisType == FiveAxisType.Spindle;
			m_Dlg.SetUIPermissions( enableMSDeg, enableSwitchDir, ( type == ESpecialToolVecType.ZDir && !isSpindleType ) );
			m_Dlg.SetMachiningDirHintVisible( type == ESpecialToolVecType.ZDir && isSpindleType );
		}

		#endregion

		void SetUIMSAsFirstPathFirstPoint()
		{
			if( IsNoPathSelected() ) {
				return;
			}
			if( !TryGetCurrentMS( m_CurrentPathIDList.First(), out Tuple<double, double> msAngle_deg ) ) {
				return;
			}
			m_Dlg.ResetMSAngle( msAngle_deg.Item1, msAngle_deg.Item2 );
		}

		#region Multi select UI setting

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

			// fixdir and zdir have same ms as 0, so only assignMS will have diff ms
			// skip first path because it is used as reference
			return m_CurrentPathIDList.Skip( 1 ).All( pathID =>
				GetPathSpecialToolVecData( pathID, out SpecialToolVecData specialToolVecData )

				// same type
				&& specialToolVecData.Type == firstSpecialToolVecData.Type

				// same ms angle
				&& specialToolVecData.MasterAngle_deg == firstSpecialToolVecData.MasterAngle_deg
				&& specialToolVecData.SlaveAngle_deg == firstSpecialToolVecData.SlaveAngle_deg

				// same assigned machining direction
				&& specialToolVecData.MachiningDir == firstSpecialToolVecData.MachiningDir );
		}

		// show the shared direction, otherwise fall back to None when the selection is mixed
		void SyncDlgMachiningDir()
		{
			if( IsNoPathSelected() || !IsPathSameParam()
				|| !GetPathSpecialToolVecData( m_CurrentPathIDList.First(), out SpecialToolVecData specialToolVecData ) ) {
				m_Dlg.ResetMachiningDir( EMachiningDir.None );
				return;
			}
			m_Dlg.ResetMachiningDir( specialToolVecData.MachiningDir );
		}

		// only meaningful for ZDir, the dialog only raises this while ZDir is the current type
		void OnMachiningDirChanged( EMachiningDir dir )
		{
			ExecutePipeline( new EditIntentArgs
			{
				Intent = EEditIntent.EditMachiningDir,
				TargetMachiningDir = dir,
			} );
		}

		#endregion

		#region Set param

		// dialog only, m_Type is owned by PrepareType
		void SwitchDlgType( ESpecialToolVecType type )
		{
			m_Dlg.SwitchType( type );
		}

		#endregion

		#region Dlg param change

		// change MS angle by textbox
		void OnMSAngleChanged( double masterAngle_deg, double slaveAngle_deg )
		{
			ExecutePipeline( new EditIntentArgs
			{
				Intent = EEditIntent.EditMSAngle,
				MasterAngle_deg = masterAngle_deg,
				SlaveAngle_deg = slaveAngle_deg,
			} );
		}

		// change type by radio button
		void OnTypeChanged( ESpecialToolVecType type )
		{
			ExecutePipeline( new EditIntentArgs
			{
				Intent = EEditIntent.SwitchType,
				TargetToolVecType = type,
			} );
		}

		void OnFlipRotaryAxis( bool isPositive )
		{
			ExecutePipeline( new EditIntentArgs
			{
				Intent = EEditIntent.FlipRotaryAxis,
				IsFlipToPositive = isPositive,
			} );
		}

		void OnRotateToFixDir()
		{
			ExecutePipeline( new EditIntentArgs { Intent = EEditIntent.RotateToFixDir } );
		}

		void OnRotateToZDir()
		{
			ExecutePipeline( new EditIntentArgs { Intent = EEditIntent.RotateToZDir } );
		}

		// show the real posture only when every selected path agrees on it, otherwise show 0
		void SetMSSilent()
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
				: ( IsPathSameParam() && craftDataFirst.SpecialToolVecData.Type == ESpecialToolVecType.AssignMS );

			if( bUseRealMS ) {
				SetUIMSAsFirstPathFirstPoint();
			}
			else {
				m_Dlg.ResetMSAngle( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
			}
		}

		#endregion

		#region Edit intent pipeline

		// carries what the user just did, only the fields the intent needs are filled
		struct EditIntentArgs
		{
			// what the user did, decides which fields below are meaningful
			public EEditIntent Intent;

			// the type the user picked on the radio button
			public ESpecialToolVecType TargetToolVecType;

			// the angle the user typed in the text box
			public double MasterAngle_deg;
			public double SlaveAngle_deg;

			// true means rotary axis +180 
			public bool IsFlipToPositive;

			// direction the user picked, only kept when the type is ZDir
			public EMachiningDir TargetMachiningDir;
		}

		void ExecutePipeline( EditIntentArgs args )
		{
			// check this intention is valid
			if( !CanExecuteIntent( args.Intent ) ) {
				return;
			}

			// change m_Type, means path will be set as m_Type
			ESpecialToolVecType typeBeforeEdit = m_Type;
			PrepareType( args );

			// cal MS Angle
			if( !ResolveAngle( args, out Tuple<double, double> msAngle_deg ) ) {

				// nothing is written, put the type back so the dialog does not show a setting the path does not have
				m_Type = typeBeforeEdit;
				SyncDialog();
				RefreshSimuResult();
				return;
			}
			SetSelectedPathData( m_Type, msAngle_deg.Item1, msAngle_deg.Item2, args.TargetMachiningDir );
			SyncDialog( args.Intent == EEditIntent.EditMSAngle );
			RefreshSimuResult();
		}

		bool CanExecuteIntent( EEditIntent intent )
		{
			switch( intent ) {

				// user change type
				case EEditIntent.SwitchType:
				case EEditIntent.EditMSAngle:
					return CheckPathIsSelect();

				case EEditIntent.RotateToFixDir:
					if( IsNoPathSelected() ) {
						return false;
					}

					// only single select can set same ms for fixdir, multi select will get diff ms from each path
					if( m_IsSingleSelect == false ) {
						MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "MutiPathCantSetSameMSForFixDir", CANNOT_SET_SAME_MS_FOR_MULTI_FIXDIR ), MyApp.NoticeType.Warning );
						return false;
					}
					return true;

				// FlipRotaryAxis and RotateToZDir, silently ignored when nothing is selected
				default:
					return IsNoPathSelected() == false;
			}
		}

		// decide which type the selected paths should be written as
		void PrepareType( EditIntentArgs args )
		{
			// case 1 : the user picked a radio button
			if( args.Intent == EEditIntent.SwitchType ) {
				m_Type = args.TargetToolVecType;
				return;
			}

			// case 2 : RotateToFixDir and RotateToZDir only recalculate the angle
			if( args.Intent == EEditIntent.RotateToFixDir || args.Intent == EEditIntent.RotateToZDir ) {
				m_Type = ESpecialToolVecType.AssignMS;
				return;
			}

			// case 3 : single select and change MS or flip axis, turn in to assign MS
			if( m_IsSingleSelect &&
				( args.Intent == EEditIntent.EditMSAngle || args.Intent == EEditIntent.FlipRotaryAxis ) ) {
				m_Type = ESpecialToolVecType.AssignMS;
				return;
			}

			// anything else keeps the current type
		}

		// false means the angle cannot be decided, the caller then writes nothing at all
		bool ResolveAngle( EditIntentArgs args, out Tuple<double, double> msAngle_deg )
		{
			msAngle_deg = new Tuple<double, double>( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
			switch( args.Intent ) {
				case EEditIntent.EditMSAngle:
					msAngle_deg = new Tuple<double, double>( args.MasterAngle_deg, args.SlaveAngle_deg );
					return true;

				case EEditIntent.FlipRotaryAxis:
					m_Dlg.GetMSAngleFromDialog( out double dlgMaster_deg, out double dlgSlave_deg );
					msAngle_deg = ToolVecHelper.FlipRotaryAxis( dlgMaster_deg, dlgSlave_deg, args.IsFlipToPositive );
					return true;

				case EEditIntent.RotateToFixDir:

					// multi select to set fix dir has been lock by CanExecuteIntent
					if( !TryGetMSForFixDir( m_CurrentPathIDList.First(), out msAngle_deg ) ) {
						int pathOrderIndex = m_DataManager.PathIDList.IndexOf( m_CurrentPathIDList.First() );
						string pathName = m_DataManager.GetPathName( pathOrderIndex );
						MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetGeomData", CANNOT_GET_GEOM_DATA ), pathName ), MyApp.NoticeType.Warning );
						return false;
					}
					return true;

				case EEditIntent.RotateToZDir:

					// several path will get z dir with diff ms all set as 0
					if( m_IsSingleSelect == false ) {
						return true;
					}
					return TryGetMSForZDir( m_CurrentPathIDList.First(), out msAngle_deg );

				case EEditIntent.EditMachiningDir:

					// only raised under ZDir, whose MS angle is always the default
					return true;

				case EEditIntent.SwitchType:
				default:
					if( m_Type != ESpecialToolVecType.AssignMS || m_IsSingleSelect == false ) {
						return true;
					}

					// single select to set AssignMS, use campoint[0] MS
					return TryGetCurrentMS( m_CurrentPathIDList.First(), out msAngle_deg );
			}
		}

		// keepMSValue means this value change trigger by the user, do not need to overwrite
		void SyncDialog( bool keepMSValue = false )
		{
			if( m_Dlg == null ) {
				return;
			}

			// the radio may be checked by the user already, but the type can also be coerced or come from a new selection
			SwitchDlgType( m_Type );
			ApplyDlgUIPermissions( m_Type );
			SyncDlgMachiningDir();
			if( keepMSValue ) {
				return;
			}
			SetMSSilent();
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
				int pathOrderIndex = m_DataManager.PathIDList.IndexOf( pathID );
				string pathName = m_DataManager.GetPathName( pathOrderIndex );
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetCraftData", CANNOT_GET_CRAFT_DATA ), pathName ), MyApp.NoticeType.Warning );
				return false;
			}
			specialToolVecData = craftData.SpecialToolVecData;
			return true;
		}

		// MS angle solved by the average tool vector of the contour
		bool TryGetMSForFixDir( string pathID, out Tuple<double, double> msAngle_deg )
		{
			msAngle_deg = new Tuple<double, double>( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
			if( !DataGettingHelper.GetGeomDataByID( pathID, out IGeomData geomData ) ) {
				return false;
			}
			ContourGeomData contourGeomData = geomData as ContourGeomData;
			if( contourGeomData == null ) {
				return false;
			}
			if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
				return false;
			}

			// get average tool vec
			gp_Dir avgDir = contourGeomData.RefCenterDir.Direction();
			if( craftData.IsToolVecReverse ) {
				avgDir = new gp_Dir( avgDir.Reversed().XYZ() );
			}
			if( !TryGetPathFirstPoint( pathID, out CAMPoint pathFirstPnt ) ) {
				return false;
			}
			msAngle_deg = ToolVecHelper.GetMSAngleFromToolVec( avgDir, pathFirstPnt );
			return true;
		}

		// MS angle solved by the positive direction of the rotary axis
		bool TryGetMSForZDir( string pathID, out Tuple<double, double> msAngle_deg )
		{
			msAngle_deg = new Tuple<double, double>( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
			if( !TryGetPathFirstPoint( pathID, out CAMPoint pathFirstPnt ) ) {
				return false;
			}
			gp_Dir rotaryDir = RotaryAxisHelper.GetRotaryAxisDir( m_RotaryAxisConfig.RotaryAxisType );
			msAngle_deg = ToolVecHelper.GetMSAngleFromToolVec( rotaryDir, pathFirstPnt );
			return true;
		}

		// current MS angle of the path start point, cache rebuilds itself when dirty
		bool TryGetCurrentMS( string pathID, out Tuple<double, double> msAngle_deg )
		{
			msAngle_deg = new Tuple<double, double>( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
			if( !TryGetPathFirstPoint( pathID, out CAMPoint pathFirstPnt ) ) {
				return false;
			}
			msAngle_deg = new Tuple<double, double>( pathFirstPnt.ModMaster_rad * RAD2DEG, pathFirstPnt.ModSlave_rad * RAD2DEG );
			return true;
		}

		bool TryGetPathFirstPoint( string pathID, out CAMPoint pathFirstPnt )
		{
			pathFirstPnt = null;
			if( !DataGettingHelper.GetContourCacheByID( pathID, out ContourCache contourCache ) ) {
				return false;
			}
			if( contourCache.MainPathPointList == null || contourCache.MainPathPointList.Count == 0 ) {
				return false;
			}
			pathFirstPnt = contourCache.MainPathPointList.First().Clone();
			return true;
		}

		// the machining direction only exists under ZDir, any other type is forced back to None
		void SetSelectedPathData( ESpecialToolVecType type, double masterAngle_deg = DEFAULT_Angle_Deg, double slaveAngle_deg = DEFAULT_Angle_Deg, EMachiningDir machiningDir = EMachiningDir.None )
		{
			if( m_CurrentPathIDList == null ) {
				return;
			}
			EMachiningDir dirToWrite = type == ESpecialToolVecType.ZDir ? machiningDir : EMachiningDir.None;
			foreach( string pathID in m_CurrentPathIDList ) {
				if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
					continue;
				}
				craftData.SetSpecialToolVecData( new SpecialToolVecData( type, masterAngle_deg, slaveAngle_deg, dirToWrite ) );
			}
		}

		#endregion

		#region Simulation

		void RefreshSimuResult()
		{
			if( m_CurrentPathIDList == null || m_CurrentPathIDList.Count() == 0 ) {
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
			PostPoint orientationPoint = new PostPoint()
			{
				Master = dMasterAngle_rad,
				Slave = dSlaveAngle_rad,
				ExternalAxis_deg = targetPnt.ExternalAxis_deg
			};
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


