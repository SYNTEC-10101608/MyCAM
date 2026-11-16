using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CncManager.Definition;
using CommonUI.Controls;
using CommonUI.Forms;
using CommonUI.Theme;

namespace CncUI
{
	public partial class CNCInfoPanel : PanelBase
	{
		public CNCInfoPanel()
		{
			SetStyle( ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.ResizeRedraw, true );
			InitializeComponent();
			m_DesignSize = Size;
			m_Values = new Dictionary<CNCInfoField, Label> {
				{ CNCInfoField.X, m_lblXValue },
				{ CNCInfoField.Y, m_lblYValue },
				{ CNCInfoField.Z, m_lblZValue },
				{ CNCInfoField.B, m_lblBValue },
				{ CNCInfoField.C, m_lblCValue },
				{ CNCInfoField.Capacitance, m_lblCapacitanceValue },
				{ CNCInfoField.HeightAxisPosition, m_lblHeightValue },
				{ CNCInfoField.FollowHeight, m_lblFollowHeightValue },
				{ CNCInfoField.ActualHeight, m_lblActualHeightValue },
				{ CNCInfoField.FeedSpeed, m_lblSpeedValue },
				{ CNCInfoField.Power, m_lblPowerValue },
				{ CNCInfoField.Frequency, m_lblFrequencyValue },
				{ CNCInfoField.DutyCycle, m_lblDutyCycleValue },
				{ CNCInfoField.FocusPosition, m_lblFocusValue },
				{ CNCInfoField.MachiningTime, m_lblTimeValue }
			};
			m_AxisColors = new Dictionary<Label, Color> {
				{ m_lblXTitle, X_AXIS_COLOR },
				{ m_lblYTitle, Y_AXIS_COLOR },
				{ m_lblZTitle, Z_AXIS_COLOR },
				{ m_lblBTitle, B_AXIS_COLOR },
				{ m_lblCTitle, C_AXIS_COLOR }
			};
			m_btnCoordinates.Click += ( sender, e ) => CoordinateModeClicked?.Invoke();
			m_pnlCoordinates.Paint += OnCoordinatesPaint;
			m_Layout = new ResponsiveLayout( this );
			m_VerticalLayout = CreateVerticalLayout( out m_VerticalDesignSize );
			ApplyResources();
			UpdateResponsiveLayout();
		}

		internal LanguageManager.LanguageManager Language { get; } = new LanguageManager.LanguageManager( "CNCInfoPanel" );

		[DefaultValue( false )]
		public bool VerticalLayout
		{
			get => m_IsVertical;
			set
			{
				if( m_IsVertical == value ) {
					return;
				}
				m_IsVertical = value;
				AutoScrollPosition = Point.Empty;
				AutoScroll = false;
				UpdateResponsiveLayout();
			}
		}

		public void ApplyResources()
		{
			// captions whose ID is the control name, then the texts set at runtime
			ApplyLanguage( Language );
			SetCoordinateMode( m_MachineCoordinates );
			SetRotaryAxisRoles( m_FirstAxisRole, m_SecondAxisRole );
			SetMachiningStatus( m_MachiningStatus );
			UpdateResponsiveLayout();
		}

		public override Size GetPreferredSize( Size proposedSize )
		{
			// the landscape designer size
			return m_DesignSize;
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			base.OnSizeChanged( e );
			UpdateResponsiveLayout();
		}

		void UpdateResponsiveLayout()
		{
			if( m_Layout == null || m_VerticalLayout == null || m_ApplyingLayout || DesignMode ||
				LicenseManager.UsageMode == LicenseUsageMode.Designtime ) {
				return;
			}
			m_ApplyingLayout = true;
			try {
				AutoScrollMinSize = Size.Empty;
				AutoScroll = false;
				if( m_IsVertical ) {
					m_VerticalLayout.Apply( this );
					AlignVerticalColumns();
				}
				else {
					m_Layout.Apply( this );
				}
				m_lblCoordinateHeader.Left = m_lblXValue.Right - m_lblCoordinateHeader.Width;
				int gap = ThemeMetrics.CONTROL_GAP;
				int titleWidth = TextRenderer.MeasureText( m_lblMainProgramTitle.Text, m_lblMainProgramTitle.Font ).Width;
				int titleRight = Math.Min( m_lblMainProgramTitle.Left + titleWidth, m_lblSpeedValue.Right - gap - 1 );
				m_lblMainProgramTitle.Width = Math.Max( 1, titleRight - m_lblMainProgramTitle.Left );
				int programLeft = m_lblMainProgramTitle.Right + gap;
				m_lblMainProgram.SetBounds( programLeft, m_lblMainProgram.Top,
					Math.Max( 1, m_lblSpeedValue.Right - programLeft ), m_lblMainProgram.Height );
			}
			finally {
				m_ApplyingLayout = false;
			}
		}

		ResponsiveLayout CreateVerticalLayout( out Size designSize )
		{
			Panel[] cards = { m_pnlCoordinates, m_pnlHeight, m_pnlMachining };
			int width = 0;
			foreach( Panel card in cards ) {
				width = Math.Max( width, card.Width );
			}
			int top = ThemeMetrics.PANEL_MARGIN;
			foreach( Panel card in cards ) {
				ResponsiveLayout contents = new ResponsiveLayout( card );
				card.SetBounds( ThemeMetrics.PANEL_MARGIN, top, width, card.Height );
				contents.Apply( card );
				SortedDictionary<int, List<Control>> rows = new SortedDictionary<int, List<Control>>();
				foreach( Control control in card.Controls ) {
					if( !rows.TryGetValue( control.Top, out List<Control> row ) ) {
						row = new List<Control>();
						rows.Add( control.Top, row );
					}
					row.Add( control );
				}
				// rows keep the read-out row grid of the landscape layout from the first content row;
				// card actions in the title bar stay there
				bool isTitled = !string.IsNullOrEmpty( card.Text );
				int rowTop = isTitled ? ThemeMetrics.SECTION_CONTENT_TOP : ThemeMetrics.SECTION_PADDING;
				int rowBottom = rowTop;
				foreach( KeyValuePair<int, List<Control>> rowEntry in rows ) {
					if( isTitled && rowEntry.Key < ThemeMetrics.SECTION_TITLE_HEIGHT ) {
						continue;
					}
					foreach( Control control in rowEntry.Value ) {
						control.SetBounds( control.Left, rowTop, control.Width, ThemeMetrics.INFO_ROW_HEIGHT );
					}
					rowBottom = rowTop + ThemeMetrics.INFO_ROW_HEIGHT;
					rowTop += ThemeMetrics.INFO_ROW_PITCH;
				}
				card.Height = rowBottom + ThemeMetrics.SECTION_PADDING;
				top += card.Height + ThemeMetrics.SECTION_GAP;
			}

			// the last card is followed by the panel margin instead of a card gap
			designSize = new Size( width + ThemeMetrics.PANEL_MARGIN * 2,
				top - ThemeMetrics.SECTION_GAP + ThemeMetrics.PANEL_MARGIN );
			return new ResponsiveLayout( this, designSize );
		}

		void AlignVerticalColumns()
		{
			Label[] units = {
				m_lblXUnit, m_lblYUnit, m_lblZUnit, m_lblBUnit, m_lblCUnit,
				m_lblCapacitanceUnit, m_lblHeightUnit, m_lblFollowHeightUnit, m_lblActualHeightUnit,
				m_lblSpeedUnit, m_lblPowerUnit, m_lblFrequencyUnit, m_lblDutyCycleUnit, m_lblFocusUnit
			};
			int valueLeft = 0;
			int valueRight = 0;
			int cardWidth = int.MaxValue;
			foreach( Label value in m_Values.Values ) {
				valueLeft = Math.Max( valueLeft, value.Left );
				valueRight = Math.Max( valueRight, value.Right );
				cardWidth = Math.Min( cardWidth, value.Parent.ClientSize.Width );
			}
			int unitWidth = 0;
			foreach( Label unit in units ) {
				unitWidth = Math.Max( unitWidth, TextRenderer.MeasureText( unit.Text, unit.Font ).Width );
			}
			int gap = ThemeMetrics.CONTROL_GAP;
			int rightPadding = ThemeMetrics.SECTION_PADDING;
			valueRight = Math.Min( valueRight, cardWidth - rightPadding - gap - unitWidth );
			foreach( Label value in m_Values.Values ) {
				value.SetBounds( valueLeft, value.Top, Math.Max( 1, valueRight - valueLeft ), value.Height );
			}
			foreach( Label unit in units ) {
				unit.SetBounds( valueRight + gap, unit.Top, unitWidth, unit.Height );
			}
			foreach( Label axis in m_AxisColors.Keys ) {
				axis.Left = m_lblCapacitanceTitle.Left + AXIS_MARKER_OFFSET;
			}
		}

		internal event Action CoordinateModeClicked;

		internal void SetRotaryAxisNames( string first, string second )
		{
			m_lblBTitle.Text = first ?? "—";
			m_lblCTitle.Text = second ?? "—";
		}

		internal void SetRotaryAxisRoles( CncAxisRole? first, CncAxisRole? second )
		{
			m_FirstAxisRole = first;
			m_SecondAxisRole = second;
			m_lblBNote.Text = GetAxisRoleText( first );
			m_lblCNote.Text = GetAxisRoleText( second );
		}

		string GetAxisRoleText( CncAxisRole? role )
		{
			switch( role ) {
				case CncAxisRole.Head:
					return Language.TryGetString( "AxisRole_Head", AXIS_ROLE_HEAD );
				case CncAxisRole.Table:
					return Language.TryGetString( "AxisRole_Table", AXIS_ROLE_TABLE );
				default:
					return "—";
			}
		}

		internal void SetCoordinateMode( bool machineCoordinates )
		{
			m_MachineCoordinates = machineCoordinates;
			m_btnCoordinates.Text = machineCoordinates
				? Language.TryGetString( "CoordinateMode_Machine", MACHINE_COORDINATES )
				: Language.TryGetString( "CoordinateMode_Program", PROGRAM_COORDINATES );
		}

		internal void SetValue( CNCInfoField field, string text )
		{
			m_Values[ field ].Text = text;
		}

		internal void SetMainProgram( string programName )
		{
			m_lblMainProgram.Text = string.IsNullOrWhiteSpace( programName ) ? "—" : programName;
			m_ttpMainProgram.SetToolTip( m_lblMainProgram, programName );
		}

		internal void SetMachiningStatus( CncStatus? status )
		{
			m_MachiningStatus = status;
			string text;
			TextRole role;
			switch( status ) {
				case CncStatus.NOTREADY:
					text = Language.TryGetString( "Status_NotReady", STATUS_NOT_READY );
					role = TextRole.Error;
					break;
				case CncStatus.READY:
					text = Language.TryGetString( "Status_Ready", STATUS_READY );
					role = TextRole.Success;
					break;
				case CncStatus.START:
				case CncStatus.EXECUTE:
					text = Language.TryGetString( "Status_Machining", STATUS_MACHINING );
					role = TextRole.Warning;
					break;
				case CncStatus.FEEDHOLD:
					text = Language.TryGetString( "Status_Paused", STATUS_PAUSED );
					role = TextRole.Warning;
					break;
				case CncStatus.STOP:
					text = Language.TryGetString( "Status_Alarm", STATUS_ALARM );
					role = TextRole.Error;
					break;
				case CncStatus.RECORD:
					text = Language.TryGetString( "Status_Record", STATUS_RECORD );
					role = TextRole.Warning;
					break;
				default:
					text = "—";
					role = TextRole.Secondary;
					break;
			}
			m_lblMachiningStatus.Text = text;
			m_lblMachiningStatus.Role = role;
		}

		void OnCoordinatesPaint( object sender, PaintEventArgs e )
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			foreach( var axis in m_AxisColors ) {
				using( SolidBrush brush = new SolidBrush( axis.Value ) ) {
					e.Graphics.FillEllipse( brush, axis.Key.Left - AXIS_MARKER_OFFSET, axis.Key.Top + ( axis.Key.Height - 7 ) / 2F, 7, 7 );
				}
			}
		}

		readonly Dictionary<CNCInfoField, Label> m_Values;
		readonly Dictionary<Label, Color> m_AxisColors;

		// data colors of the X / Y / Z / B / C axis markers
		static readonly Color X_AXIS_COLOR = Color.FromArgb( 224, 90, 90 );
		static readonly Color Y_AXIS_COLOR = Color.FromArgb( 155, 225, 93 );
		static readonly Color Z_AXIS_COLOR = Color.FromArgb( 26, 26, 217 );
		static readonly Color B_AXIS_COLOR = Color.FromArgb( 106, 27, 154 );
		static readonly Color C_AXIS_COLOR = Color.FromArgb( 230, 149, 0 );
		readonly ResponsiveLayout m_Layout;
		readonly ResponsiveLayout m_VerticalLayout;
		readonly Size m_VerticalDesignSize;
		readonly Size m_DesignSize;
		bool m_IsVertical;
		bool m_ApplyingLayout;
		CncAxisRole? m_FirstAxisRole = CncAxisRole.Head;
		CncAxisRole? m_SecondAxisRole = CncAxisRole.Table;
		CncStatus? m_MachiningStatus;
		bool m_MachineCoordinates;

		const int AXIS_MARKER_OFFSET = 12;

		const string AXIS_HEADER = "軸";
		const string COORDINATE_HEADER = "座標";
		const string HEIGHT_GROUP_TITLE = "調高軸資訊";
		const string CAPACITANCE = "電容";
		const string HEIGHT_POSITION = "調高軸座標";
		const string FOLLOW_HEIGHT = "跟隨高度";
		const string ACTUAL_HEIGHT = "實際高度";
		const string MACHINING_TITLE = "加工狀態";
		const string MAIN_PROGRAM_TITLE = "加工檔名";
		const string FEED_SPEED = "進給速度";
		const string POWER = "功率";
		const string FREQUENCY = "頻率";
		const string DUTY_CYCLE = "占空比";
		const string FOCUS_POSITION = "焦點位置";
		const string MACHINING_TIME = "加工時間";
		const string AXIS_ROLE_HEAD = "擺頭";
		const string AXIS_ROLE_TABLE = "轉台";
		const string MACHINE_COORDINATES = "機械座標 ⇄";
		const string PROGRAM_COORDINATES = "程式座標 ⇄";
		const string STATUS_NOT_READY = "未就緒";
		const string STATUS_READY = "就緒";
		const string STATUS_MACHINING = "加工中";
		const string STATUS_PAUSED = "暫停";
		const string STATUS_ALARM = "警報";
		const string STATUS_RECORD = "紀錄";
	}

	internal enum CNCInfoField
	{
		X,
		Y,
		Z,
		B,
		C,
		Capacitance,
		HeightAxisPosition,
		FollowHeight,
		ActualHeight,
		FeedSpeed,
		Power,
		Frequency,
		DutyCycle,
		FocusPosition,
		MachiningTime
	}
}
