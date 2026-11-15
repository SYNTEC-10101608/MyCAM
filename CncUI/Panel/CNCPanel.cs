using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CncManager.Definition;
using CncUI.Controls;
using CommonUI.Controls;
using CommonUI.Forms;

namespace CncUI
{
	public partial class CNCPanel : PanelBase
	{
		public CNCPanel()
		{
			InitializeComponent();
			m_Buttons = new Dictionary<CNCButton, ThemedButton> {
				{ CNCButton.Start, m_btnStart },
				{ CNCButton.Continue, m_btnContinue },
				{ CNCButton.Pause, m_btnPause },
				{ CNCButton.Stop, m_btnStop },
				{ CNCButton.Laser, m_btnLaser },
				{ CNCButton.Air, m_btnAir },
				{ CNCButton.Follow, m_btnFollow },
				{ CNCButton.Focus, m_btnFocus },
				{ CNCButton.Calibration, m_btnCalibration },
				{ CNCButton.HomeProgram, m_btnHomeProgram },
				{ CNCButton.HomeMachine, m_btnHomeMachine },
				{ CNCButton.SafePosition, m_btnSafePosition },
				{ CNCButton.DryRun, m_btnDryRun },
				{ CNCButton.ResumeCutting, m_btnResumeCutting },
				{ CNCButton.Shutter, m_btnShutter },
				{ CNCButton.Endpoint, m_btnEndpoint },
				{ CNCButton.HandwheelSimulation, m_btnHandwheelSimulation }
			};
			m_JogStepButtons = new Dictionary<JogStep, ThemedButton> {
				{ JogStep.Step5, m_btnJogStep5 },
				{ JogStep.Step10, m_btnJogStep10 },
				{ JogStep.Step50, m_btnJogStep50 },
				{ JogStep.Step100, m_btnJogStep100 },
				{ JogStep.Step200, m_btnJogStep200 }
			};
			foreach( var step in m_JogStepButtons ) {
				step.Value.Click += ( sender, e ) => JogStepClicked?.Invoke( step.Key );
			}
			m_JogButtons = new Dictionary<string, Button[]> {
				{ "X", new Button[] { m_btnXPlus, m_btnXMinus } },
				{ "Y", new Button[] { m_btnYPlus, m_btnYMinus } },
				{ "Z", new Button[] { m_btnZPlus, m_btnZMinus } },
				{ "Rotary1", new Button[] { m_btnBPlus, m_btnBMinus } },
				{ "Rotary2", new Button[] { m_btnCPlus, m_btnCMinus } }
			};
			foreach( var axis in m_JogButtons ) {
				BindJogButton( axis.Value[ 0 ], axis.Key, true );
				BindJogButton( axis.Value[ 1 ], axis.Key, false );
			}
			SetJogEnabled( false );
			m_btnStart.Click += ( sender, e ) => StartClicked?.Invoke();
			m_btnContinue.Click += ( sender, e ) => ContinueClicked?.Invoke();
			m_btnPause.Click += ( sender, e ) => PauseClicked?.Invoke();
			m_btnStop.Click += ( sender, e ) => StopClicked?.Invoke();
			m_trbFeedOverride.ValueChanged += ( sender, e ) => FeedOverrideChanged?.Invoke( m_trbFeedOverride.Value );
			m_btnJog.Click += ( sender, e ) => JogModeClicked?.Invoke();
			m_btnEndpoint.Click += ( sender, e ) => EndpointClicked?.Invoke();
			m_btnHomeProgram.Click += ( sender, e ) => HomeProgramClicked?.Invoke();
			m_btnHomeMachine.Click += ( sender, e ) => HomeMachineClicked?.Invoke();
			m_btnSafePosition.Click += ( sender, e ) => SafePositionClicked?.Invoke();
			m_btnDryRun.Click += ( sender, e ) => DryRunClicked?.Invoke();
			m_btnResumeCutting.Click += ( sender, e ) => ResumeCuttingClicked?.Invoke();
			m_btnHandwheelSimulation.Click += ( sender, e ) => HandwheelSimulationClicked?.Invoke();
			m_btnShutter.Click += ( sender, e ) => ShutterClicked?.Invoke();
			m_btnAir.Click += ( sender, e ) => AirClicked?.Invoke();
			m_btnFollow.Click += ( sender, e ) => FollowClicked?.Invoke();
			m_btnLaser.Click += ( sender, e ) => LaserClicked?.Invoke();
			m_btnFocus.Click += ( sender, e ) => FocusClicked?.Invoke();
			m_btnCalibration.Click += ( sender, e ) => CalibrationClicked?.Invoke();
			m_btnOuterEquipParameters.Click += ( sender, e ) => OuterEquipParametersClicked?.Invoke();
			m_btnResumeCutParameters.Click += ( sender, e ) => ResumeCutParametersClicked?.Invoke();
			m_chbThreePointCalibration.CheckedChanged += ( sender, e ) =>
			{
				if( !m_UpdatingThreePointCalibration ) {
					ThreePointCalibrationChanged?.Invoke( m_chbThreePointCalibration.Checked );
				}
			};
			m_Layout = new ResponsiveLayout( m_pnlCommands );
			ApplyResources();
			UpdateResponsiveLayout();
		}

		internal LanguageManager.LanguageManager Language { get; } = new LanguageManager.LanguageManager( "CNCPanel" );

		public void ApplyResources()
		{
			// captions whose ID is the control name
			LanguageApplier.Apply( this, Language );
			SetJogMode( m_Inching );
		}

		public override Size GetPreferredSize( Size proposedSize )
		{
			return GetPreferredSizeForDpi( DeviceDpi );
		}

		public static Size GetPreferredSizeForDpi( int dpi )
		{
			float scale = dpi / 96F;
			return new Size( (int)Math.Ceiling( 240 * scale ), (int)Math.Ceiling( 620 * scale ) );
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			base.OnSizeChanged( e );
			UpdateResponsiveLayout();
		}

		protected override void OnDpiChangedAfterParent( EventArgs e )
		{
			base.OnDpiChangedAfterParent( e );
			UpdateResponsiveLayout();
		}

		void UpdateResponsiveLayout()
		{
			if( m_Layout == null || DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime ) {
				return;
			}
			int inset = Math.Max( 1, DeviceDpi / 96 );
			m_pnlCommands.SetBounds( inset, inset, Math.Max( 1, ClientSize.Width - 2 * inset ),
				Math.Max( 1, ClientSize.Height - 2 * inset ) );
			m_Layout.Apply( m_pnlCommands );
		}

		#region Auxiliary and feed events

		internal event Action ShutterClicked;
		internal event Action AirClicked;
		internal event Action FollowClicked;
		internal event Action LaserClicked;
		internal event Action FocusClicked;
		internal event Action CalibrationClicked;
		internal event Action OuterEquipParametersClicked;
		internal event Action<int> FeedOverrideChanged;

		#endregion

		#region Machining events

		internal event Action StartClicked;
		internal event Action ContinueClicked;
		internal event Action PauseClicked;
		internal event Action StopClicked;
		internal event Action<bool> ThreePointCalibrationChanged;

		#endregion

		#region Jog events

		internal event Action<string, bool> JogPressed;
		internal event Action JogReleased;
		internal event Action JogModeClicked;
		internal event Action EndpointClicked;
		internal event Action<JogStep> JogStepClicked;

		#endregion

		#region Home and strategy events

		internal event Action HomeProgramClicked;
		internal event Action HomeMachineClicked;
		internal event Action SafePositionClicked;
		internal event Action DryRunClicked;
		internal event Action ResumeCuttingClicked;
		internal event Action HandwheelSimulationClicked;
		internal event Action ResumeCutParametersClicked;

		#endregion

		#region Display

		internal void SetButtonEnabled( CNCButton button, bool enabled )
		{
			m_Buttons[ button ].Enabled = enabled;
		}

		internal void SetButtonActive( CNCButton button, bool active )
		{
			m_Buttons[ button ].Active = active;
		}

		internal void SetCommandsEnabled( bool enabled )
		{
			m_pnlCommands.Enabled = enabled;
		}

		internal void SetJogEnabled( bool enabled, int axisCount = 5 )
		{
			foreach( var axis in m_JogButtons ) {
				bool available = axis.Key == "Rotary2" ? axisCount > 4 : axisCount > 3;
				foreach( Button button in axis.Value ) {
					button.Enabled = enabled && available;
				}
			}
		}

		internal void SetSecondRotaryAxisVisible( bool visible )
		{
			m_btnCPlus.Visible = visible;
			m_btnCMinus.Visible = visible;
		}

		internal void SetRotaryAxisNames( string first, string second )
		{
			m_btnBPlus.Text = ( first ?? "—" ) + "+";
			m_btnBMinus.Text = ( first ?? "—" ) + "-";
			m_btnCPlus.Text = ( second ?? "—" ) + "+";
			m_btnCMinus.Text = ( second ?? "—" ) + "-";
		}

		internal void SetJogMode( bool inching )
		{
			m_Inching = inching;
			m_btnJog.Text = inching ? Language.TryGetString( "JogMode_Inching", JOG_INCHING ) : Language.TryGetString( "JogMode_Continuous", JOG_CONTINUOUS );
		}

		internal void SetJogStep( JogStep? value )
		{
			foreach( var step in m_JogStepButtons ) {
				step.Value.Active = step.Key == value;
			}
		}

		internal void SetThreePointCalibration( bool enabled )
		{
			m_UpdatingThreePointCalibration = true;
			try {
				m_chbThreePointCalibration.Checked = enabled;
			}
			finally {
				m_UpdatingThreePointCalibration = false;
			}
		}

		internal void SetFeedOverride( int? value )
		{
			m_lblFeedOverrideValue.Text = "[" + ( value.HasValue ? value.Value.ToString() : "--" ) + "]%";
		}

		#endregion

		#region Jog input

		void BindJogButton( Button button, string axis, bool positive )
		{
			button.MouseDown += ( sender, e ) =>
			{
				if( e.Button == MouseButtons.Left ) {
					PressJog( button, axis, positive );
				}
			};
			button.MouseUp += ( sender, e ) =>
			{
				if( e.Button == MouseButtons.Left ) {
					ReleaseJog( button );
				}
			};
			button.MouseCaptureChanged += ( sender, e ) =>
			{
				if( !button.Capture ) {
					ReleaseJog( button );
				}
			};
			button.KeyDown += ( sender, e ) =>
			{
				if( e.KeyCode == Keys.Space ) {
					e.SuppressKeyPress = true;
					PressJog( button, axis, positive );
				}
			};
			button.KeyUp += ( sender, e ) =>
			{
				if( e.KeyCode == Keys.Space ) {
					e.SuppressKeyPress = true;
					ReleaseJog( button );
				}
			};
			button.LostFocus += ( sender, e ) => ReleaseJog( button );
			button.EnabledChanged += ( sender, e ) =>
			{
				if( !button.Enabled ) {
					ReleaseJog( button );
				}
			};
		}

		void PressJog( Button button, string axis, bool positive )
		{
			if( m_PressedJogButton != null || !button.Enabled ) {
				return;
			}
			m_PressedJogButton = button;
			JogPressed?.Invoke( axis, positive );
		}

		void ReleaseJog( Button button )
		{
			if( m_PressedJogButton == button ) {
				CancelJog();
			}
		}

		internal void CancelJog()
		{
			if( m_PressedJogButton == null ) {
				return;
			}
			m_PressedJogButton = null;
			JogReleased?.Invoke();
		}

		#endregion

		const string AUXILIARY_TITLE = "功能控制";
		const string PARAMETERS = "參數設置";
		const string SHUTTER = "光閘";
		const string AIR = "吹氣";
		const string FOLLOW = "隨動";
		const string LASER = "雷射";
		const string FOCUS = "調焦";
		const string CALIBRATION = "電容標定";
		const string JOG_TITLE = "軸向移動";
		const string JOG_INCHING = "寸動";
		const string JOG_CONTINUOUS = "點動";
		const string ENDPOINT = "控端點";
		const string HOME_TITLE = "回零";
		const string HOME_PROGRAM = "回程式零";
		const string HOME_MACHINE = "回機械零";
		const string SAFE_POSITION = "回安全位置";
		const string STRATEGY_TITLE = "加工策略";
		const string DRY_RUN = "空走";
		const string RESUME_CUTTING = "斷點續切";
		const string HANDWHEEL_SIMULATION = "手輪模擬";
		const string MACHINING_TITLE = "加工控制";
		const string THREE_POINT_CALIBRATION = "三點校正";
		const string FEED_OVERRIDE = "進給倍率";
		const string START = "開始";
		const string PAUSE = "暫停";
		const string CONTINUE = "繼續";
		const string STOP = "停止";

		readonly Dictionary<CNCButton, ThemedButton> m_Buttons;
		readonly Dictionary<JogStep, ThemedButton> m_JogStepButtons;
		readonly Dictionary<string, Button[]> m_JogButtons;
		readonly ResponsiveLayout m_Layout;
		Button m_PressedJogButton;
		bool m_Inching;
		bool m_UpdatingThreePointCalibration;
	}

	internal enum CNCButton
	{
		Start,
		Continue,
		Pause,
		Stop,
		Laser,
		Air,
		Follow,
		Focus,
		Calibration,
		HomeProgram,
		HomeMachine,
		SafePosition,
		DryRun,
		ResumeCutting,
		Shutter,
		Endpoint,
		HandwheelSimulation
	}
}
