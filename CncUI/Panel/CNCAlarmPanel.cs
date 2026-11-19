using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using CommonUI.Forms;
using CommonUI.Theme;
using CncManager.Definition;

namespace CncUI
{
	public partial class CNCAlarmPanel : PanelBase
	{
		public CNCAlarmPanel()
		{
			InitializeComponent();
			ApplyResources();
		}

		internal LanguageManager.LanguageManager Language { get; } = new LanguageManager.LanguageManager( "CNCAlarmPanel" );

		public void ApplyResources()
		{
			// captions and list columns whose ID is the control name, then the texts set at runtime
			ApplyLanguage( Language );
			m_pnlAlarm.Text = m_IsCurrent
				? string.Format( Language.TryGetString( "Title_Current", CURRENT_TITLE ), m_AlarmCount )
				: Language.TryGetString( "Title_Stale", STALE_TITLE );
			switch( m_Status ) {
				case CNCAlarmStatus.Updating:
					m_lblStatus.Text = Language.TryGetString( "Status_Updating", UPDATING );
					break;
				case CNCAlarmStatus.Disconnected:
					m_lblStatus.Text = Language.TryGetString( "Status_Disconnected", DISCONNECTED );
					break;
				case CNCAlarmStatus.ReadFailed:
					m_lblStatus.Text = string.Format( Language.TryGetString( "Status_ReadFailed", READ_FAILED ), m_Error );
					break;
				default:
					m_lblStatus.Text = string.Empty;
					break;
			}
			m_lblStatus.Visible = !string.IsNullOrEmpty( m_lblStatus.Text );
			m_ttpAlarm.SetToolTip( m_lblStatus, m_lblStatus.Text );
			RefreshAlarmText();
			UpdateListBounds();
		}

		internal void SetSnapshot( IReadOnlyList<CncAlarm> alarms, bool isCurrent, CNCAlarmStatus status, string error )
		{
			m_AlarmCount = alarms.Count;
			m_IsCurrent = isCurrent;
			m_Status = status;
			m_Error = error;
			ApplyResources();
			m_dgvAlarms.SuspendLayout();
			try {
				m_dgvAlarms.Rows.Clear();
				foreach( CncAlarm alarm in alarms ) {
					string code = string.Format( CultureInfo.InvariantCulture, "{0}-{1}-{2}",
						alarm.Category.Replace( "ALARM", string.Empty ), alarm.ObjectID, alarm.AlarmID );
					int index = m_dgvAlarms.Rows.Add( code, GetAlarmText( alarm ) );
					m_dgvAlarms.Rows[ index ].Tag = alarm;
				}
				m_dgvAlarms.CellRole = isCurrent ? TextRole.Error : TextRole.Secondary;
			}
			finally {
				m_dgvAlarms.ResumeLayout();
			}
			UpdateListBounds();
		}

		internal void SetAxes( IReadOnlyList<CncAxis> axes )
		{
			m_Axes = axes;
			RefreshAlarmText();
		}

		void RefreshAlarmText()
		{
			foreach( DataGridViewRow row in m_dgvAlarms.Rows ) {
				if( row.Tag is CncAlarm alarm ) {
					row.Cells[ m_colText.Index ].Value = GetAlarmText( alarm );
				}
			}
		}

		string GetAlarmText( CncAlarm alarm )
		{
			string text = CNCAlarmTextResolver.Default.Resolve( alarm );
			string category = CNCAlarmTextResolver.GetCategory( alarm.Category ).ToUpperInvariant();
			string prefix = string.Empty;
			string number = alarm.ObjectID.ToString( CultureInfo.InvariantCulture );
			switch( category ) {
				case "MOTION":
				case "ROT":
				case "EXTSTROKELIMIT":
					if( alarm.ObjectID > 0 ) {
						string name = GetAxisName( alarm.ObjectID );
						prefix = string.IsNullOrEmpty( name )
							? string.Format( Language.TryGetString( "Format_AxisNumber", AXIS_NUMBER ), number )
							: string.Format( Language.TryGetString( "Format_AxisName", AXIS_NAME ), name );
					}
					break;
				case "SPINDLE":
					if( alarm.ObjectID > 0 ) {
						prefix = string.Format( Language.TryGetString( "Format_SpindleNumber", SPINDLE_NUMBER ), number );
					}
					break;
				case "STATION":
					if( alarm.ObjectID >= 0 ) {
						prefix = string.Format( Language.TryGetString( "Format_StationNumber", STATION_NUMBER ),
							alarm.ObjectID.ToString( "X", CultureInfo.InvariantCulture ) );
					}
					break;
				default:
					if( alarm.ObjectID > 0 && ( category == "SERIALPLCAXIS" || category.StartsWith( "DRV_", StringComparison.Ordinal ) ) ) {
						prefix = string.Format( Language.TryGetString( "Format_AxisNumber", AXIS_NUMBER ), number );
					}
					break;
			}
			return string.IsNullOrEmpty( prefix ) ? text : ( prefix + " " + text ).TrimEnd();
		}

		string GetAxisName( int id )
		{
			switch( id ) {
				case 1:
					return "X";
				case 2:
					return "Y";
				case 3:
					return "Z";
				case 4:
				case 5:
					foreach( CncAxis axis in m_Axes ) {
						if( axis.ID == id ) {
							return axis.Name;
						}
					}
					break;
			}
			return null;
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			base.OnSizeChanged( e );
			UpdateListBounds();
		}

		void UpdateListBounds()
		{
			if( m_dgvAlarms == null ) {
				return;
			}
			int top = string.IsNullOrEmpty( m_lblStatus.Text )
				? m_lblStatus.Top
				: m_lblStatus.Bottom + ThemeMetrics.CONTROL_GAP;
			m_dgvAlarms.SetBounds( ThemeMetrics.SECTION_PADDING, top,
				Math.Max( 0, m_pnlAlarm.ClientSize.Width - 2 * ThemeMetrics.SECTION_PADDING ),
				Math.Max( 0, m_pnlAlarm.ClientSize.Height - top - ThemeMetrics.SECTION_PADDING ) );
		}

		IReadOnlyList<CncAxis> m_Axes = Array.AsReadOnly( new CncAxis[ 0 ] );
		int m_AlarmCount;
		bool m_IsCurrent = true;
		CNCAlarmStatus m_Status;
		string m_Error;

		const string CURRENT_TITLE = "當前警報（{0}）";
		const string STALE_TITLE = "當前警報（未更新）";
		const string UPDATING = "正在更新警報，資料尚未確認";
		const string DISCONNECTED = "連線中斷，警報資料未更新";
		const string READ_FAILED = "警報讀取失敗，資料未更新：{0}";
		const string AXIS_NAME = "{0}軸";
		const string AXIS_NUMBER = "第{0}軸";
		const string SPINDLE_NUMBER = "第{0}主軸";
		const string STATION_NUMBER = "Station {0}h";
	}
}
