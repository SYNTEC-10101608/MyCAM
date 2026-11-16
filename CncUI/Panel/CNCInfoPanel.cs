using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CncManager.Definition;
using CncUI.Controls;
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
			m_grpCoordinates.Paint += OnCoordinatesPaint;
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
			// captions whose ID is the control name
			LanguageApplier.Apply( this, Language );
			SetCoordinateMode( m_MachineCoordinates );
			SetRotaryAxisRoles( m_FirstAxisRole, m_SecondAxisRole );
			SetMachiningStatus( m_MachiningStatus );
			UpdateResponsiveLayout();
		}

		public override Size GetPreferredSize( Size proposedSize )
		{
			float scale = DeviceDpi / 96F;
			return new Size( (int)Math.Ceiling( 672 * scale ), (int)Math.Ceiling( 236 * scale ) );
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
				int gap = (int)Math.Ceiling( VERTICAL_COLUMN_GAP * DeviceDpi / 96F );
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
			Panel[] cards = { m_grpCoordinates, m_grpHeight, m_grpMachining };
			int width = 0;
			foreach( Panel card in cards ) {
				width = Math.Max( width, card.Width );
			}
			int top = VERTICAL_CARD_GAP;
			foreach( Panel card in cards ) {
				ResponsiveLayout contents = new ResponsiveLayout( card );
				card.SetBounds( VERTICAL_CARD_GAP, top, width, card.Height );
				contents.Apply( card );
				SortedDictionary<int, List<Control>> rows = new SortedDictionary<int, List<Control>>();
				foreach( Control control in card.Controls ) {
					if( !rows.TryGetValue( control.Top, out List<Control> row ) ) {
						row = new List<Control>();
						rows.Add( control.Top, row );
					}
					row.Add( control );
				}
				// a titled card starts its rows below the title bar; card actions in the title bar stay there
				bool isTitled = !string.IsNullOrEmpty( card.Text );
				int rowTop = isTitled ? ThemeMetrics.SECTION_TITLE_HEIGHT : VERTICAL_CARD_GAP;
				foreach( KeyValuePair<int, List<Control>> rowEntry in rows ) {
					if( isTitled && rowEntry.Key < ThemeMetrics.SECTION_TITLE_HEIGHT ) {
						continue;
					}
					List<Control> row = rowEntry.Value;
					int rowHeight = 0;
					foreach( Control control in row ) {
						int padding = control is Button ? VERTICAL_BUTTON_PADDING : VERTICAL_ROW_PADDING;
						rowHeight = Math.Max( rowHeight, control.Font.Height + padding );
					}
					foreach( Control control in row ) {
						control.SetBounds( control.Left, rowTop, control.Width, rowHeight );
					}
					rowTop += rowHeight;
				}
				card.Height = rowTop + VERTICAL_CARD_GAP;
				top += card.Height + VERTICAL_CARD_GAP;
			}
			designSize = new Size( width + VERTICAL_CARD_GAP * 2, top );
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
			int gap = (int)Math.Ceiling( VERTICAL_COLUMN_GAP * DeviceDpi / 96F );
			int rightPadding = (int)Math.Ceiling( VERTICAL_CARD_GAP * DeviceDpi / 96F );
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

		internal void SetSecondRotaryAxisVisible( bool visible )
		{
			m_lblCTitle.Visible = visible;
			m_lblCNote.Visible = visible;
			m_lblCValue.Visible = visible;
			m_lblCUnit.Visible = visible;
			m_grpCoordinates.Invalidate();
		}

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
				if( !axis.Key.Visible ) {
					continue;
				}
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
		bool m_IsVertical;
		bool m_ApplyingLayout;
		CncAxisRole? m_FirstAxisRole = CncAxisRole.Head;
		CncAxisRole? m_SecondAxisRole = CncAxisRole.Table;
		CncStatus? m_MachiningStatus;
		bool m_MachineCoordinates;

		const int VERTICAL_CARD_GAP = 4;
		const int VERTICAL_COLUMN_GAP = 4;
		const int VERTICAL_ROW_PADDING = 2;
		const int VERTICAL_BUTTON_PADDING = 6;
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
