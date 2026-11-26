using System;
using System.Threading;
using System.Windows.Forms;
using CncManager;
using CncManager.CNCCore;
using CncManager.Definition;

namespace CncUI
{
	internal sealed class CNCPanelController : IDisposable
	{
		public CNCPanelController( CNCPanel panel, CncClient client, CNCUIManager manager )
		{
			m_Panel = panel;
			m_Client = client;
			m_Manager = manager;
		}

		#region Lifecycle

		public bool Init( out string error )
		{
			error = null;
			if( m_Disposed || m_Panel == null || m_Panel.IsDisposed || m_Client == null || m_Manager == null ) {
				error = CNCUIManager.Language.TryGetString( "Error_PanelUnavailable", PANEL_UNAVAILABLE );
				Dispose();
				return false;
			}
			if( m_Initialized ) {
				error = CNCUIManager.Language.TryGetString( "Error_PanelInitialized", PANEL_INITIALIZED );
				return false;
			}
			SynchronizationContext uiContext = SynchronizationContext.Current as WindowsFormsSynchronizationContext;
			if( uiContext == null ) {
				error = CNCUIManager.Language.TryGetString( "Error_PanelUIThread", PANEL_UI_THREAD );
				Dispose();
				return false;
			}
			try {
				m_UIContext = uiContext;
				m_Initialized = true;
				m_Panel.Disposed += OnPanelDisposed;
				m_Manager.AxesChanged += OnAxesChanged;
				OnAxesChanged();

				// Panel click events
				SubscribePanelEvents();

				// CNC resource subscriptions
				if( !SubscribeResources() ) {
					error = CNCUIManager.Language.TryGetString( "Error_PanelSubscription", PANEL_SUBSCRIPTION );
					Dispose();
					return false;
				}
				m_Panel.SetCommandsEnabled( true );
				UpdateMachiningButtonsEnabled();
				UpdateManualControlsEnabled();
				UpdateHomeAndStrategyEnabled();
				UpdateJogMode();
				UpdateJogStep();
				UpdateEndpointState();
				UpdateHandwheelSimulationState();
				UpdateThreePointCalibration();
				UpdateFeedOverride();
				UpdateShutterState();
				UpdateLaserEnabled();
				UpdateLaserState();
				UpdateAirState();
				UpdateFollowState();
				return true;
			}
			catch( Exception ex ) {
				error = ex.Message;
				Dispose();
				return false;
			}
		}

		public void Dispose()
		{
			if( m_Disposed ) {
				return;
			}
			m_Disposed = true;
			m_UIContext = null;
			if( m_Manager != null ) {
				m_Manager.AxesChanged -= OnAxesChanged;
			}
			UnsubscribeResources();
			if( m_Panel == null ) {
				return;
			}
			m_Panel.Disposed -= OnPanelDisposed;
			UnsubscribePanelEvents();
			try {
				OnJogReleased();
				if( !m_Panel.IsDisposed ) {
					m_Panel.CancelJog();
					m_Panel.SetJogEnabled( false );
					m_Panel.SetCommandsEnabled( false );
					m_Panel.SetButtonEnabled( CNCButton.Start, false );
					m_Panel.SetButtonEnabled( CNCButton.Continue, false );
					m_Panel.SetButtonEnabled( CNCButton.Pause, false );
				}
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC panel cleanup failed: {0}", ex );
			}
		}

		void OnPanelDisposed( object sender, EventArgs e )
		{
			Dispose();
		}

		#endregion

		#region Panel subscriptions

		void SubscribePanelEvents()
		{
			// Machining controls
			m_Panel.StartClicked += OnStartClicked;
			m_Panel.ContinueClicked += OnContinueClicked;
			m_Panel.PauseClicked += OnPauseClicked;
			m_Panel.StopClicked += OnStopClicked;
			m_Panel.ThreePointCalibrationChanged += OnThreePointCalibrationChanged;
			m_Panel.JogPressed += OnJogPressed;
			m_Panel.JogReleased += OnJogReleased;
			m_Panel.JogModeClicked += OnJogModeClicked;
			m_Panel.EndpointClicked += OnEndpointClicked;
			m_Panel.JogStepClicked += OnJogStepClicked;
			m_Panel.HomeProgramClicked += OnHomeProgramClicked;
			m_Panel.HomeMachineClicked += OnHomeMachineClicked;
			m_Panel.SafePositionClicked += OnSafePositionClicked;
			m_Panel.DryRunClicked += OnDryRunClicked;
			m_Panel.ResumeCuttingClicked += OnResumeCuttingClicked;
			m_Panel.HandwheelSimulationClicked += OnHandwheelSimulationClicked;
			m_Panel.AirClicked += OnAirClicked;
			m_Panel.LaserClicked += OnLaserClicked;
			m_Panel.FocusClicked += OnFocusClicked;
			m_Panel.CalibrationClicked += OnCalibrationClicked;
			m_Panel.ShutterClicked += OnShutterClicked;
			m_Panel.FollowClicked += OnFollowClicked;
			m_Panel.FeedOverrideChanged += OnFeedOverrideChanged;
			m_Panel.OuterEquipParametersClicked += OnOuterEquipParametersClicked;
			m_Panel.ResumeCutParametersClicked += OnResumeCutParametersClicked;
		}

		void UnsubscribePanelEvents()
		{
			// Machining controls
			m_Panel.StartClicked -= OnStartClicked;
			m_Panel.ContinueClicked -= OnContinueClicked;
			m_Panel.PauseClicked -= OnPauseClicked;
			m_Panel.StopClicked -= OnStopClicked;
			m_Panel.ThreePointCalibrationChanged -= OnThreePointCalibrationChanged;
			m_Panel.JogPressed -= OnJogPressed;
			m_Panel.JogReleased -= OnJogReleased;
			m_Panel.JogModeClicked -= OnJogModeClicked;
			m_Panel.EndpointClicked -= OnEndpointClicked;
			m_Panel.JogStepClicked -= OnJogStepClicked;
			m_Panel.HomeProgramClicked -= OnHomeProgramClicked;
			m_Panel.HomeMachineClicked -= OnHomeMachineClicked;
			m_Panel.SafePositionClicked -= OnSafePositionClicked;
			m_Panel.DryRunClicked -= OnDryRunClicked;
			m_Panel.ResumeCuttingClicked -= OnResumeCuttingClicked;
			m_Panel.HandwheelSimulationClicked -= OnHandwheelSimulationClicked;
			m_Panel.AirClicked -= OnAirClicked;
			m_Panel.LaserClicked -= OnLaserClicked;
			m_Panel.FocusClicked -= OnFocusClicked;
			m_Panel.CalibrationClicked -= OnCalibrationClicked;
			m_Panel.ShutterClicked -= OnShutterClicked;
			m_Panel.FollowClicked -= OnFollowClicked;
			m_Panel.FeedOverrideChanged -= OnFeedOverrideChanged;
			m_Panel.OuterEquipParametersClicked -= OnOuterEquipParametersClicked;
			m_Panel.ResumeCutParametersClicked -= OnResumeCutParametersClicked;
		}

		#endregion

		void OnOuterEquipParametersClicked()
		{
			OpenParameters( PollingAction.ShowCNCMainScreen );
		}

		void OnResumeCutParametersClicked()
		{
			OpenParameters( PollingAction.ResumeCutParameters );
		}

		public void ShowProcessSettings()
		{
			OpenParameters( PollingAction.ProcessSettings );
		}

		void OpenParameters( PollingAction action )
		{
			if( !CanExecuteCommand() ) {
				return;
			}
			if( !CncApi.WriteRRegister( CncConstants.R_PollingAction, (int)action ) ) {
				m_Manager.Report( m_Panel.Language.TryGetString( "Error_Parameters", ERROR_PARAMETERS ), true );
				return;
			}
			m_Manager.RequestController();
		}

		#region Resource subscriptions

		bool SubscribeResources()
		{
			m_RSubscription = m_Manager.RPool.Subscribe( CncConstants.R_CONTINUE_BLOCKED, OnContinueBlockedChanged );
			m_SystemSubscription = m_Manager.Resources.SystemVarPool.Subscribe( CncConstants.SYSTEM_VAR_1820, OnSystemVariableChanged );
			m_KSubscription = m_Manager.Resources.KVarPool.Subscribe( CncConstants.K_CNC_STATUS, OnMachiningStatusChanged );
			m_ShutterSubscription = m_Manager.RPool.Subscribe( CncConstants.R_SHUTTER, OnShutterChanged );
			m_LaserStateSubscription = m_Manager.Resources.GlobalVarPool.Subscribe( CncConstants.GLOBAL_VAR_LASER_STATE, OnLaserStateChanged );
			m_AirSubscription = m_Manager.RPool.Subscribe( CncConstants.R_AIR_STATE, OnAirChanged );
			m_FollowSubscription = m_Manager.RPool.Subscribe( CncConstants.R_FOLLOW_STATE, OnFollowChanged );
			m_JogModeSubscription = m_Manager.RPool.Subscribe( CncConstants.R_JOG_MODE, OnJogModeChanged );
			m_JogStepSubscription = m_Manager.RPool.Subscribe( CncConstants.R_JOG_STEP, OnJogStepChanged );
			m_ResumeBlocked1Subscription = m_Manager.RPool.Subscribe( CncConstants.R_RESUME_CUTTING_BLOCKED_1, OnResumeBlockedChanged );
			m_ResumeBlocked2Subscription = m_Manager.RPool.Subscribe( CncConstants.R_RESUME_CUTTING_BLOCKED_2, OnResumeBlockedChanged );
			m_HandwheelSimulationSubscription = m_Manager.RPool.Subscribe( CncConstants.R_HANDWHEEL_SIMULATION, OnHandwheelSimulationChanged );
			m_EndpointSubscription = m_Manager.Resources.SystemVarPool.Subscribe( CncConstants.SYSTEM_VAR_ENDPOINT, OnEndpointChanged );
			m_ThreePointCalibrationSubscription = m_Manager.RPool.Subscribe( CncConstants.R_THREE_POINT_CALIBRATION, OnThreePointCalibrationResourceChanged );
			m_FeedOverrideSubscription = m_Manager.RPool.Subscribe( CncConstants.R_FEED_OVERRIDE, OnFeedOverrideResourceChanged );
			return m_RSubscription != null && m_SystemSubscription != null && m_KSubscription != null &&
				m_ShutterSubscription != null && m_LaserStateSubscription != null &&
				m_AirSubscription != null && m_FollowSubscription != null &&
				m_JogModeSubscription != null && m_JogStepSubscription != null &&
				m_ResumeBlocked1Subscription != null && m_ResumeBlocked2Subscription != null &&
				m_HandwheelSimulationSubscription != null && m_EndpointSubscription != null && m_ThreePointCalibrationSubscription != null &&
				m_FeedOverrideSubscription != null;
		}

		void UnsubscribeResources()
		{
			m_RSubscription?.Dispose();
			m_RSubscription = null;
			m_SystemSubscription?.Dispose();
			m_SystemSubscription = null;
			m_KSubscription?.Dispose();
			m_KSubscription = null;
			m_ShutterSubscription?.Dispose();
			m_ShutterSubscription = null;
			m_LaserStateSubscription?.Dispose();
			m_LaserStateSubscription = null;
			m_AirSubscription?.Dispose();
			m_AirSubscription = null;
			m_FollowSubscription?.Dispose();
			m_FollowSubscription = null;
			m_JogModeSubscription?.Dispose();
			m_JogModeSubscription = null;
			m_JogStepSubscription?.Dispose();
			m_JogStepSubscription = null;
			m_ResumeBlocked1Subscription?.Dispose();
			m_ResumeBlocked1Subscription = null;
			m_ResumeBlocked2Subscription?.Dispose();
			m_ResumeBlocked2Subscription = null;
			m_HandwheelSimulationSubscription?.Dispose();
			m_HandwheelSimulationSubscription = null;
			m_EndpointSubscription?.Dispose();
			m_EndpointSubscription = null;
			m_ThreePointCalibrationSubscription?.Dispose();
			m_ThreePointCalibrationSubscription = null;
			m_FeedOverrideSubscription?.Dispose();
			m_FeedOverrideSubscription = null;
		}

		#endregion

		#region Auxiliary and feed controls

		void OnAirClicked()
		{
			if( !CanExecuteCommand() || !IsMachiningReady() ) {
				return;
			}
			CncApi.WriteRRegister( CncConstants.R_PollingAction, (int)PollingAction.Air );
		}

		void OnLaserClicked()
		{
			if( !CanExecuteCommand() || !IsLaserEnabled() ) {
				return;
			}
			CncApi.WriteRRegister( CncConstants.R_PollingAction, (int)PollingAction.Laser );
		}

		void OnFocusClicked()
		{
			if( !CanExecuteCommand() || !IsMachiningReady() ) {
				return;
			}
			CncApi.WriteRRegister( CncConstants.R_PollingAction, (int)PollingAction.Focus );
		}

		void OnCalibrationClicked()
		{
			if( !CanExecuteCommand() || !IsMachiningReady() ) {
				return;
			}
			if( MessageBox.Show( m_Panel,
				m_Panel.Language.TryGetString( "Confirm_Calibration", CONFIRM_CALIBRATION ),
				m_Panel.Language.TryGetString( "m_btnCalibration", CALIBRATION_TITLE ),
				MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2 ) != DialogResult.OK ) {
				return;
			}
			if( !CanExecuteCommand() || !IsMachiningReady() ) {
				return;
			}
			CncApi.WriteRRegister( CncConstants.R_OuterTiggerSource, 1 );
			CncApi.WriteRRegister( CncConstants.R_PollingAction, (int)PollingAction.NormalCalibration );
		}

		void OnShutterClicked()
		{
			if( !CanExecuteCommand() ) {
				return;
			}
			CncApi.WriteRRegister( CncConstants.R_SHUTTER, IsShutterActive() ? 0 : 1 );
		}

		void OnFollowClicked()
		{
			if( !CanExecuteCommand() || !IsMachiningReady() ) {
				return;
			}
			PollingAction command = IsFollowActive() ? PollingAction.FollowOff : PollingAction.FollowOn;
			CncApi.WriteRRegister( CncConstants.R_PollingAction, (int)command );
		}

		void OnFeedOverrideChanged( int value )
		{
			if( !CanExecuteCommand() ) {
				return;
			}
			CncApi.WriteRRegister( CncConstants.R_FEED_OVERRIDE, value );
		}

		#endregion

		#region Jog

		void OnJogPressed( string axis, bool positive )
		{
			if( !CanExecuteCommand() || !IsMachiningReady() || m_JogActive ||
				( m_Manager.Axes.Count != 4 && m_Manager.Axes.Count != 5 ) ) {
				return;
			}
			int slot;
			switch( axis ) {
				case "X":
					slot = 0;
					break;
				case "Y":
					slot = 1;
					break;
				case "Z":
					slot = 2;
					break;
				case "Rotary1":
					slot = 3;
					break;
				case "Rotary2":
					slot = 4;
					break;
				default:
					return;
			}
			int axisNumber = slot + 1;
			int command = checked(2 * axisNumber - ( positive ? 1 : 0 ));
			m_JogActive = true;
			CncApi.WriteRRegister( CncConstants.R_JOG_COMMAND, command );
		}

		void OnJogReleased()
		{
			if( !m_JogActive ) {
				return;
			}
			m_JogActive = false;
			if( m_Client.IsConnected && !CncApi.WriteRRegister( CncConstants.R_JOG_COMMAND, 0 ) ) {
				System.Diagnostics.Trace.TraceError( "CNC Jog stop command failed." );
			}
		}

		void OnJogModeClicked()
		{
			if( !CanExecuteCommand() ) {
				return;
			}
			CncApi.WriteRRegister( CncConstants.R_JOG_MODE, IsInching() ? 0 : 1 );
		}

		void OnEndpointClicked()
		{
			if( !CanExecuteCommand() ) {
				return;
			}
			CncApi.ExecuteBlocks( "G339" );
		}

		void OnJogStepClicked( JogStep value )
		{
			if( !CanExecuteCommand() ) {
				return;
			}
			CncApi.WriteRRegister( CncConstants.R_JOG_STEP, (int)value );
		}

		#endregion

		#region Home and strategy clicks

		void OnHomeProgramClicked()
		{
			Home( PollingAction.HomeProgram, "Title_HomeProgram", HOME_PROGRAM_TITLE );
		}

		void OnHomeMachineClicked()
		{
			Home( PollingAction.HomeMachine, "Title_HomeMachine", HOME_MACHINE_TITLE );
		}

		void Home( PollingAction action, string titleKey, string title )
		{
			if( !CanExecuteCommand() || !IsMachiningReady() ) {
				return;
			}
			if( MessageBox.Show( m_Panel,
				m_Panel.Language.TryGetString( "Confirm_Home", CONFIRM_HOME ),
				m_Panel.Language.TryGetString( titleKey, title ),
				MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2 ) != DialogResult.OK ) {
				return;
			}
			if( !CanExecuteCommand() || !IsMachiningReady() ) {
				return;
			}
			CncApi.WriteRRegister( CncConstants.R_OuterTiggerSource, 1 );
			CncApi.WriteRRegister( CncConstants.R_PollingAction, (int)action );
		}

		void OnSafePositionClicked()
		{
			if( !CanExecuteCommand() || !IsMachiningReady() ) {
				return;
			}
			CncApi.WriteRRegister( CncConstants.R_PollingAction, (int)PollingAction.SafePosition );
		}

		void OnDryRunClicked()
		{
			StartProgram( true );
		}

		void OnResumeCuttingClicked()
		{
			if( !CanExecuteCommand() || !IsResumeCuttingEnabled() ) {
				return;
			}
			CncApi.WriteRRegister( CncConstants.R_PollingAction, (int)PollingAction.ResumeCutting );
		}

		void OnHandwheelSimulationClicked()
		{
			if( !CanExecuteCommand() ) {
				return;
			}
			bool active = !IsHandwheelSimulationActive();
			try {
				if( active ) {
					CncApi.WriteRRegister( CncConstants.R_CNC_MODE, CncConstants.HANDWHEEL_MODE );
				}
			}
			finally {
				CncApi.WriteRRegister( CncConstants.R_HANDWHEEL_SIMULATION, active ? 1 : 0 );
			}
		}

		#endregion

		#region UI rules

		bool IsMachiningReady()
		{
			return GetMachiningStatus() == 1;
		}

		int? GetRValue( int index )
		{
			ResourceValue<int?> value;
			return m_Manager.RPool.TryGetValue( index, out value ) && value.IsValid ? value.Value : null;
		}

		bool IsInching()
		{
			return GetRValue( CncConstants.R_JOG_MODE ) == 1;
		}

		bool IsHandwheelSimulationActive()
		{
			return GetRValue( CncConstants.R_HANDWHEEL_SIMULATION ) == 1;
		}

		bool IsEndpointActive()
		{
			ResourceValue<double?> value;
			return m_Manager.Resources.SystemVarPool.TryGetValue( CncConstants.SYSTEM_VAR_ENDPOINT, out value ) &&
				value.IsValid && value.Value.HasValue && (int)value.Value.Value == CncConstants.ENDPOINT_ACTIVE;
		}

		bool IsResumeCuttingEnabled()
		{
			return IsMachiningReady() && GetRValue( CncConstants.R_RESUME_CUTTING_BLOCKED_1 ) == 0 &&
				GetRValue( CncConstants.R_RESUME_CUTTING_BLOCKED_2 ) == 0;
		}

		void UpdateHomeAndStrategyEnabled()
		{
			if( !CanUpdate() ) {
				return;
			}
			bool enabled = IsMachiningReady();
			m_Panel.SetButtonEnabled( CNCButton.HomeProgram, enabled );
			m_Panel.SetButtonEnabled( CNCButton.HomeMachine, enabled );
			m_Panel.SetButtonEnabled( CNCButton.SafePosition, enabled );
			m_Panel.SetButtonEnabled( CNCButton.DryRun, enabled );
			UpdateResumeCuttingEnabled();
		}

		void UpdateResumeCuttingEnabled()
		{
			if( !CanUpdate() ) {
				return;
			}
			m_Panel.SetButtonEnabled( CNCButton.ResumeCutting, IsResumeCuttingEnabled() );
		}

		void UpdateJogMode()
		{
			if( !CanUpdate() ) {
				return;
			}
			m_Panel.SetJogMode( IsInching() );
		}

		void UpdateJogStep()
		{
			if( !CanUpdate() ) {
				return;
			}
			m_Panel.SetJogStep( (JogStep?)GetRValue( CncConstants.R_JOG_STEP ) );
		}

		void UpdateHandwheelSimulationState()
		{
			if( !CanUpdate() ) {
				return;
			}
			m_Panel.SetButtonActive( CNCButton.HandwheelSimulation, IsHandwheelSimulationActive() );
		}

		void UpdateEndpointState()
		{
			if( !CanUpdate() ) {
				return;
			}
			m_Panel.SetButtonActive( CNCButton.Endpoint, IsEndpointActive() );
		}

		void UpdateThreePointCalibration()
		{
			if( !CanUpdate() ) {
				return;
			}
			int? value = GetRValue( CncConstants.R_THREE_POINT_CALIBRATION );
			if( value.HasValue ) {
				m_Panel.SetThreePointCalibration( value.Value == 1 );
			}
		}

		void UpdateFeedOverride()
		{
			if( !CanUpdate() ) {
				return;
			}
			m_Panel.SetFeedOverride( GetRValue( CncConstants.R_FEED_OVERRIDE ) );
		}

		bool IsShutterActive()
		{
			ResourceValue<int?> value;
			return m_Manager.RPool.TryGetValue( CncConstants.R_SHUTTER, out value ) && value.IsValid && value.Value == 1;
		}

		bool IsLaserEnabled()
		{
			return IsShutterActive() && IsMachiningReady();
		}

		bool IsFollowActive()
		{
			ResourceValue<int?> value;
			return m_Manager.RPool.TryGetValue( CncConstants.R_FOLLOW_STATE, out value ) && value.IsValid && value.Value == 1;
		}

		void UpdateManualControlsEnabled()
		{
			if( !CanUpdate() ) {
				return;
			}
			bool enabled = IsMachiningReady();
			m_Panel.SetButtonEnabled( CNCButton.Air, enabled );
			m_Panel.SetButtonEnabled( CNCButton.Follow, enabled );
			m_Panel.SetButtonEnabled( CNCButton.Focus, enabled );
			m_Panel.SetButtonEnabled( CNCButton.Calibration, enabled );
			bool valid = m_Manager.Axes.Count == 4 || m_Manager.Axes.Count == 5;
			m_Panel.SetJogEnabled( enabled && valid && m_Client.IsConnected, m_Manager.Axes.Count );
		}

		void OnAxesChanged()
		{
			if( !CanUpdate() ) {
				return;
			}
			m_Panel.CancelJog();
			CncAxis first = m_Manager.Axes.Count > 3 ? m_Manager.Axes[ 3 ] : null;
			CncAxis second = m_Manager.Axes.Count > 4 ? m_Manager.Axes[ 4 ] : null;
			m_Panel.SetRotaryAxisNames( first?.Name, second?.Name );
			m_Panel.SetSecondRotaryAxisVisible( m_Manager.Axes.Count != 4 );
			UpdateManualControlsEnabled();
		}

		void UpdateShutterState()
		{
			if( !CanUpdate() ) {
				return;
			}
			m_Panel.SetButtonActive( CNCButton.Shutter, IsShutterActive() );
		}

		void UpdateLaserEnabled()
		{
			if( !CanUpdate() ) {
				return;
			}
			m_Panel.SetButtonEnabled( CNCButton.Laser, IsLaserEnabled() );
		}

		void UpdateLaserState()
		{
			if( !CanUpdate() ) {
				return;
			}
			ResourceValue<double?> value;
			bool active = m_Manager.Resources.GlobalVarPool.TryGetValue( CncConstants.GLOBAL_VAR_LASER_STATE, out value ) &&
				value.IsValid && value.Value == 1;
			m_Panel.SetButtonActive( CNCButton.Laser, active );
		}

		void UpdateAirState()
		{
			if( !CanUpdate() ) {
				return;
			}
			ResourceValue<int?> value;
			bool active = m_Manager.RPool.TryGetValue( CncConstants.R_AIR_STATE, out value ) && value.IsValid && value.Value == 1;
			m_Panel.SetButtonActive( CNCButton.Air, active );
		}

		void UpdateFollowState()
		{
			if( !CanUpdate() ) {
				return;
			}
			m_Panel.SetButtonActive( CNCButton.Follow, IsFollowActive() );
		}

		void UpdateMachiningButtonsEnabled()
		{
			UpdateStartEnabled();
			UpdateContinueAndPauseEnabled();
		}

		void UpdateStartEnabled()
		{
			if( !CanUpdate() ) {
				return;
			}
			bool enabled = GetMachiningStatus() == (int)CncStatus.READY;
			m_Panel.SetButtonEnabled( CNCButton.Start, enabled );
			m_Panel.SetButtonEnabled( CNCButton.DryRun, enabled );
		}

		void UpdateContinueEnabled()
		{
			if( !CanUpdate() ) {
				return;
			}
			ResourceValue<int?> value;
			bool continueReady = m_Manager.RPool.TryGetValue( CncConstants.R_CONTINUE_BLOCKED, out value ) && value.IsValid && value.Value == 0;
			m_Panel.SetButtonEnabled( CNCButton.Continue,
				GetMachiningStatus() == (int)CncStatus.FEEDHOLD && continueReady && IsSystemReady() );
		}

		void UpdatePauseEnabled()
		{
			if( !CanUpdate() ) {
				return;
			}
			m_Panel.SetButtonEnabled( CNCButton.Pause,
				GetMachiningStatus() == (int)CncStatus.START && IsSystemReady() );
		}

		void UpdateContinueAndPauseEnabled()
		{
			UpdateContinueEnabled();
			UpdatePauseEnabled();
		}

		int? GetMachiningStatus()
		{
			ResourceValue<int?> statusValue;
			if( m_Manager.Resources.KVarPool.TryGetValue( CncConstants.K_CNC_STATUS, out statusValue ) && statusValue.IsValid ) {
				return statusValue.Value;
			}
			return null;
		}

		bool IsSystemReady()
		{
			ResourceValue<double?> systemValue;
			return m_Manager.Resources.SystemVarPool.TryGetValue( CncConstants.SYSTEM_VAR_1820, out systemValue ) &&
				systemValue.IsValid && systemValue.Value == 0;
		}

		#endregion

		#region Resource updates

		void OnContinueBlockedChanged( ResourceValue<int?> value )
		{
			PostToUI( UpdateContinueEnabled );
		}

		void OnSystemVariableChanged( ResourceValue<double?> value )
		{
			PostToUI( UpdateContinueAndPauseEnabled );
		}

		void OnMachiningStatusChanged( ResourceValue<int?> value )
		{
			PostToUI( () =>
			{
				UpdateMachiningButtonsEnabled();
				UpdateLaserEnabled();
				UpdateManualControlsEnabled();
				UpdateHomeAndStrategyEnabled();
			} );
		}

		void OnShutterChanged( ResourceValue<int?> value )
		{
			PostToUI( () =>
			{
				UpdateShutterState();
				UpdateLaserEnabled();
			} );
		}

		void OnLaserStateChanged( ResourceValue<double?> value )
		{
			PostToUI( UpdateLaserState );
		}

		void OnAirChanged( ResourceValue<int?> value )
		{
			PostToUI( UpdateAirState );
		}

		void OnFollowChanged( ResourceValue<int?> value )
		{
			PostToUI( UpdateFollowState );
		}

		void OnJogModeChanged( ResourceValue<int?> value )
		{
			PostToUI( UpdateJogMode );
		}

		void OnJogStepChanged( ResourceValue<int?> value )
		{
			PostToUI( UpdateJogStep );
		}

		void OnResumeBlockedChanged( ResourceValue<int?> value )
		{
			PostToUI( UpdateResumeCuttingEnabled );
		}

		void OnHandwheelSimulationChanged( ResourceValue<int?> value )
		{
			PostToUI( UpdateHandwheelSimulationState );
		}

		void OnEndpointChanged( ResourceValue<double?> value )
		{
			PostToUI( UpdateEndpointState );
		}

		void OnThreePointCalibrationResourceChanged( ResourceValue<int?> value )
		{
			PostToUI( UpdateThreePointCalibration );
		}

		void OnFeedOverrideResourceChanged( ResourceValue<int?> value )
		{
			PostToUI( UpdateFeedOverride );
		}

		#endregion

		#region Machining clicks

		void OnThreePointCalibrationChanged( bool enabled )
		{
			if( !CanExecuteCommand() ) {
				UpdateThreePointCalibration();
				return;
			}
			try {
				if( !CncApi.WriteRRegister( CncConstants.R_THREE_POINT_CALIBRATION, enabled ? 1 : 0 ) ) {
					UpdateThreePointCalibration();
					m_Manager.Report( m_Panel.Language.TryGetString( "Error_ThreePointCalibration", ERROR_THREE_POINT_CALIBRATION ), true );
					return;
				}
				if( enabled ) {
					m_Manager.ShowCalibration();
				}
			}
			catch( Exception ex ) {
				UpdateThreePointCalibration();
				m_Manager.Report( string.Format( m_Panel.Language.TryGetString( "Error_ThreePointCalibrationDetails", ERROR_THREE_POINT_CALIBRATION_DETAILS ), ex.Message ), true );
			}
		}

		void OnStartClicked()
		{
			StartProgram( false );
		}

		void StartProgram( bool dryRun )
		{
			if( !CanExecuteCommand() || !IsMachiningReady() ) {
				return;
			}
			try {
				if( dryRun ) {
					CncApi.WriteRRegister( CncConstants.R_SignalLock, 1 );
				}
			}
			finally {
				CncApi.WriteRRegister( CncConstants.R_PollingAction, (int)PollingAction.Start );
			}
		}

		// 保留原產檔、傳檔流程，待規格恢復後再啟用。
		/*
		bool m_StartRunning;
		CancellationTokenSource m_StartCancellation;

		Task StartProgramAsync( bool dryRun )
		{
			if( m_StartRunning || !CanExecuteCommand() || !IsMachiningReady() ) {
				return Task.CompletedTask;
			}
			m_StartRunning = true;
			CancellationTokenSource cancellation = new CancellationTokenSource();
			m_StartCancellation = cancellation;
			CancellationToken token = cancellation.Token;
			string ip = m_Client.IP;
			string action = dryRun ? "空走" : "開始加工";
			UpdateStartEnabled();
			try {
				// 暫停產檔、傳檔及指定主程式；恢復時需將此方法改回 async。
				//m_Manager.Report( "正在產生並傳輸 NC 檔…", false );
				//var preparation = await m_Manager.PrepareProgramAsync( ip, token );
				//CheckStartAllowed( token, ip );
				//if( !preparation.IsSuccess || string.IsNullOrWhiteSpace( preparation.Data ) ) {
				//	m_Manager.Report( "加工準備失敗：" + preparation.ErrorMessage, true );
				//	return;
				//}
				//var assignment = await CncApi.AssignMainProgramAsync( preparation.Data, token );
				//CheckStartAllowed( token, ip );
				//if( !assignment.IsSuccess ) {
				//	m_Manager.Report( "指定加工檔失敗：" + assignment.ErrorMessage, true );
				//	return;
				//}
				CheckStartAllowed( token, ip );
				bool signalLockWritten = true;
				bool started = false;
				try {
					if( dryRun ) {
						signalLockWritten = CncApi.WriteRRegister( CncConstants.R_SignalLock, 1 );
					}
				}
				finally {
					started = CncApi.WriteRRegister( CncConstants.R_PollingAction, (int)PollingAction.Start );
				}
				if( signalLockWritten && started ) {
					m_Manager.Report( "已送出" + action + "指令。", false );
				}
				else {
					m_Manager.Report( action + "指令寫入失敗。", true );
				}
			}
			catch( OperationCanceledException ) {
				if( CanUpdate() ) {
					m_Manager.Report( "操作已取消，未送出" + ( dryRun ? "空走" : "開始" ) + "指令。", false );
				}
			}
			catch( Exception ex ) {
				if( CanUpdate() ) {
					m_Manager.Report( action + "失敗：" + ex.Message, true );
				}
			}
			finally {
				m_StartCancellation = null;
				cancellation.Dispose();
				m_StartRunning = false;
				UpdateStartEnabled();
			}
			return Task.CompletedTask;
		}

		void CheckStartAllowed( CancellationToken token, string ip )
		{
			token.ThrowIfCancellationRequested();
			if( !CanExecuteCommand() || !IsMachiningReady() || m_Client.IP != ip ) {
				throw new OperationCanceledException( "CNC 狀態或連線已改變。" );
			}
		}
		*/

		void OnContinueClicked()
		{
			if( !CanExecuteCommand() ) {
				return;
			}
			CncApi.WriteRRegister( CncConstants.R_PollingAction, (int)PollingAction.Continue );
		}

		void OnPauseClicked()
		{
			if( !CanExecuteCommand() ) {
				return;
			}
			CncApi.WriteRRegister( CncConstants.R_PollingAction, (int)PollingAction.FeedHold );
		}

		void OnStopClicked()
		{
			if( !CanExecuteCommand() ) {
				return;
			}
			try {
				CncApi.ResetCnc();
			}
			finally {
				CncApi.WriteRRegister( CncConstants.R_STOP_REQUEST, CncConstants.STOP_REQUEST );
			}
		}

		bool CanExecuteCommand()
		{
			return CanUpdate() && m_Client.IsConnected;
		}

		#endregion

		void PostToUI( Action update )
		{
			SynchronizationContext context = m_UIContext;
			if( context == null || !CanUpdate() ) {
				return;
			}
			try {
				context.Post( state =>
				{
					if( !CanUpdate() ) {
						return;
					}
					try {
						update();
					}
					catch( Exception ex ) {
						System.Diagnostics.Trace.TraceError( "CNC UI update failed: {0}", ex );
					}
				}, null );
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC UI update scheduling failed: {0}", ex );
			}
		}

		bool CanUpdate()
		{
			return m_Initialized && !m_Disposed && !m_Panel.IsDisposed;
		}

		const string ERROR_PARAMETERS = "參數設置指令寫入失敗。";
		const string CONFIRM_CALIBRATION = "請先移動噴頭置靠近1~5mm距離，並保持機台靜止，不要震動。";
		const string CALIBRATION_TITLE = "電容標定";
		const string CONFIRM_HOME = "軸向即將移動，請確認路徑無障礙物。";
		const string HOME_PROGRAM_TITLE = "回程式零";
		const string HOME_MACHINE_TITLE = "回機械零";
		const string ERROR_THREE_POINT_CALIBRATION = "三點校正啟用狀態寫入失敗。";
		const string ERROR_THREE_POINT_CALIBRATION_DETAILS = "三點校正啟用狀態寫入失敗：{0}";
		const string PANEL_UNAVAILABLE = "CNC 控制面板無法使用。";
		const string PANEL_INITIALIZED = "CNC 控制面板已初始化。";
		const string PANEL_UI_THREAD = "請在 WinForms UI 執行緒初始化 CNC 控制面板。";
		const string PANEL_SUBSCRIPTION = "無法初始化 CNC 控制面板資源訂閱。";

		readonly CNCPanel m_Panel;
		readonly CncClient m_Client;
		readonly CNCUIManager m_Manager;
		SynchronizationContext m_UIContext;
		IDisposable m_RSubscription;
		IDisposable m_SystemSubscription;
		IDisposable m_KSubscription;
		IDisposable m_ShutterSubscription;
		IDisposable m_LaserStateSubscription;
		IDisposable m_AirSubscription;
		IDisposable m_FollowSubscription;
		IDisposable m_JogModeSubscription;
		IDisposable m_JogStepSubscription;
		IDisposable m_ResumeBlocked1Subscription;
		IDisposable m_ResumeBlocked2Subscription;
		IDisposable m_HandwheelSimulationSubscription;
		IDisposable m_EndpointSubscription;
		IDisposable m_ThreePointCalibrationSubscription;
		IDisposable m_FeedOverrideSubscription;
		bool m_JogActive;
		volatile bool m_Initialized;
		volatile bool m_Disposed;
	}
}
