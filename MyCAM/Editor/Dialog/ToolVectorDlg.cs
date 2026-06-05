using MyCAM.Data;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	public partial class ToolVectorDlg : EditDialogBase<EToolVecInterpolateType>
	{
		public Action SetKeep;
		public Action SetZdir;
		public Action SetRevert;
		public Action Clear;
		public Action<double, double> MSAngleChanged;
		public Action<double, double> ABAngleChanged;
		public Action<EToolVecInterpolateType> TypeChanged;
		public Action AddEditIndex;
		public Action RemoveEditIndex;
		public Action SwitchStartEnd;
		public Action<bool> MoveIndex;
		public Action<bool> MoveCtrlPnt;
		public Action<bool> ToStartOrEnd;
		public Action<bool> FlipRotaryAxis;

		public ToolVectorDlg( EToolVecInterpolateType type, ref ToolVecParam param, bool isPathReverse, RotaryAxisConfig config )
		{
			// struct would not be null
			InitializeComponent();
			m_IsPathRevese = isPathReverse;
			ResetType();
			ResetToolVecParam( param );

			// update rotary axis name
			m_RotaryAxisConfig = config;
			m_lblMaster.Text = m_RotaryAxisConfig.MasterName;
			m_lblSlave.Text = m_RotaryAxisConfig.SlaveName;
			m_btnRotaryPos.Text = m_RotaryAxisConfig.RotaryAxisName + " +";
			m_btnRotaryNeg.Text = m_RotaryAxisConfig.RotaryAxisName + " -";

			// update interpolation type combo box with actual axis names
			UpdateInterpolateTypeNames();
		}

		public void ResetType()
		{
			m_cbxInterpolateType.SelectedIndex = (int)EToolVecInterpolateType.Normal;
		}

		public void ResetToolVecParam( ToolVecParam toolVecParam )
		{
			m_ToolVecParam = toolVecParam;

			// no selected index param, disable edit UI
			if( m_ToolVecParam == null ) {
				m_gbxIndexParam.Enabled = false;
				return;
			}

			// update index edit UI
			m_gbxIndexParam.Enabled = true;
			m_tbxAngleA.SilentSetValue = (decimal)( m_IsPathRevese ? -m_ToolVecParam.AngleA_deg : m_ToolVecParam.AngleA_deg );
			m_tbxAngleB.SilentSetValue = (decimal)( m_IsPathRevese ? -m_ToolVecParam.AngleB_deg : m_ToolVecParam.AngleB_deg );
			m_tbxMaster.SilentSetValue = (decimal)m_ToolVecParam.Master_deg;
			m_tbxSlave.SilentSetValue = (decimal)m_ToolVecParam.Slave_deg;
		}

		public void EnableStartEndSwitch( bool enable, bool start )
		{
			m_btnSwitchStartEnd.Visible = enable;
			m_lblStartOrEnd.Visible = enable;
			m_lblStartOrEnd.Text = start ? "當前位置：起點" : "當前位置：終點";
		}

		public void UIControlProtection( bool isControlPnt, bool isStartEndPnt = false, EToolVecInterpolateType interpolateType = EToolVecInterpolateType.Normal )
		{
			// is control pnt
			if( isControlPnt ) {
				m_btnAdd.Enabled = false;
				m_btnRemove.Enabled = true;
				m_cbxInterpolateType.SelectedIndex = -1;
				m_cbxInterpolateType.Enabled = false;
			}
			else {
				m_btnAdd.Enabled = true;
				m_btnRemove.Enabled = false;
				if( isStartEndPnt ) {
					m_cbxInterpolateType.SelectedIndex = -1;
					m_cbxInterpolateType.Enabled = false;
				}
				else {
					m_cbxInterpolateType.Enabled = true;
					m_cbxInterpolateType.SelectedIndex = (int)interpolateType;
				}
			}
		}


		// UI event - interpolate type changed
		void m_cbxInterpolateType_SelectedIndexChanged( object sender, EventArgs e )
		{
			if( m_cbxInterpolateType.SelectedIndex < 0 ) {
				return;
			}
			EToolVecInterpolateType selectedType = (EToolVecInterpolateType)m_cbxInterpolateType.SelectedIndex;
			TypeChanged?.Invoke( selectedType );
		}

		// UI event - Index param value changed (debounced)
		void m_tbxAngleA_DebouncedValueChanged( object sender, EventArgs e )
		{
			HandleABAngleChanged();
		}

		void m_tbxAngleB_DebouncedValueChanged( object sender, EventArgs e )
		{
			HandleABAngleChanged();
		}

		void m_tbxMaster_DebouncedValueChanged( object sender, EventArgs e )
		{
			HandleMSAngleChanged();
		}

		void m_tbxSlave_DebouncedValueChanged( object sender, EventArgs e )
		{
			HandleMSAngleChanged();
		}

		void HandleABAngleChanged()
		{
			// Get AB angles from dialog
			if( !GetABAngleFromDialog( out double angleA_deg, out double angleB_deg ) ) {
				return;
			}
			if( Math.Abs( angleA_deg - m_ToolVecParam.AngleA_deg ) <= DOUBLE_COMPARISON_EPSILON
				&& Math.Abs( angleB_deg - m_ToolVecParam.AngleB_deg ) <= DOUBLE_COMPARISON_EPSILON ) {
				return;
			}
			ABAngleChanged?.Invoke( angleA_deg, angleB_deg );
			ResetToolVecParam( m_ToolVecParam );
		}

		void HandleMSAngleChanged()
		{
			// Get MS angles from dialog
			if( !GetMSAngleFromDialog( out double master_deg, out double slave_deg ) ) {
				return;
			}
			if( Math.Abs( master_deg - m_ToolVecParam.Master_deg ) <= DOUBLE_COMPARISON_EPSILON
				&& Math.Abs( slave_deg - m_ToolVecParam.Slave_deg ) <= DOUBLE_COMPARISON_EPSILON ) {
				return;
			}
			MSAngleChanged?.Invoke( master_deg, slave_deg );
			ResetToolVecParam( m_ToolVecParam );
		}

		bool GetABAngleFromDialog( out double angleA_deg, out double angleB_deg )
		{
			angleA_deg = (double)m_tbxAngleA.Value;
			angleB_deg = (double)m_tbxAngleB.Value;

			// Convert to actual angles (considering path reverse)
			angleA_deg = m_IsPathRevese ? -angleA_deg : angleA_deg;
			angleB_deg = m_IsPathRevese ? -angleB_deg : angleB_deg;
			return true;
		}

		bool GetMSAngleFromDialog( out double master_deg, out double slave_deg )
		{
			master_deg = (double)m_tbxMaster.Value;
			slave_deg = (double)m_tbxSlave.Value;
			return true;
		}

		// UI event - Index Param button
		void m_btnKeep_Click( object sender, EventArgs e )
		{
			SetTarget( SetKeep );
		}

		void m_btnZDir_Click( object sender, EventArgs e )
		{
			SetTarget( SetZdir );
		}

		void m_btnRevert_Click( object sender, EventArgs e )
		{
			SetTarget( SetRevert );
		}

		void SetTarget( Action setTargetFunc )
		{
			setTargetFunc?.Invoke();
			ResetToolVecParam( m_ToolVecParam );
		}

		void m_btnAdd_Click( object sender, EventArgs e )
		{
			AddEditIndex?.Invoke();
			m_btnAdd.Enabled = false;
			m_btnRemove.Enabled = true;
		}

		void m_btnRemove_Click( object sender, EventArgs e )
		{
			RemoveEditIndex?.Invoke();
			m_btnAdd.Enabled = true;
			m_btnRemove.Enabled = false;
		}

		void m_btnSwitchStartEnd_Click( object sender, EventArgs e )
		{
			SwitchStartEnd?.Invoke();
		}

		void m_btnRotaryPos_Click( object sender, EventArgs e )
		{
			FlipRotaryAxis?.Invoke( true );
			ResetToolVecParam( m_ToolVecParam );
		}

		void m_btnRotaryNeg_Click( object sender, EventArgs e )
		{
			FlipRotaryAxis?.Invoke( false );
			ResetToolVecParam( m_ToolVecParam );
		}

		// index moving
		void m_btnToStart_Click( object sender, EventArgs e )
		{
			ToStartOrEnd?.Invoke( true );
		}

		void m_btnToEnd_Click( object sender, EventArgs e )
		{
			ToStartOrEnd?.Invoke( false );
		}

		bool m_IsPathRevese = false;
		ToolVecParam m_ToolVecParam;

		RotaryAxisConfig m_RotaryAxisConfig;

		Timer m_Timer;
		const int TIMER_INTERVAL = 10;

		void m_btnNext_MouseDown( object sender, MouseEventArgs e )
		{
			// initialize timer
			if( m_Timer == null ) {
				m_Timer = new Timer();
			}
			m_Timer.Tick += MoveToNextTick;
			m_Timer.Interval = TIMER_INTERVAL;
			m_Timer.Start();
		}

		void m_btnNext_MouseUp( object sender, MouseEventArgs e )
		{
			m_Timer?.Stop();
			m_Timer.Tick -= MoveToNextTick;
		}

		void m_btnPrev_MouseDown( object sender, MouseEventArgs e )
		{
			// initialize timer
			if( m_Timer == null ) {
				m_Timer = new Timer();
			}
			m_Timer.Tick += MoveToLastTick;
			m_Timer.Interval = TIMER_INTERVAL;
			m_Timer.Start();
		}

		void m_btnPrev_MouseUp( object sender, MouseEventArgs e )
		{
			m_Timer?.Stop();
			m_Timer.Tick -= MoveToLastTick;
		}

		void m_btnPreCtrlPnt_Click( object sender, EventArgs e )
		{
			MoveCtrlPnt?.Invoke( false );
		}

		void m_btnNextCtrlPnt_Click( object sender, EventArgs e )
		{
			MoveCtrlPnt?.Invoke( true );
		}

		void MoveToNextTick( object sender, EventArgs e )
		{
			MoveIndex?.Invoke( true );
		}

		void MoveToLastTick( object sender, EventArgs e )
		{
			MoveIndex?.Invoke( false );
		}

		void m_btnClear_Click( object sender, EventArgs e )
		{
			Clear?.Invoke();
		}

		void UpdateInterpolateTypeNames()
		{
			if( m_RotaryAxisConfig == null ) {
				return;
			}
			string masterName = m_RotaryAxisConfig.MasterName ?? "主軸";
			string slaveName = m_RotaryAxisConfig.SlaveName ?? "從軸";

			// index 3: MasterNormalSlaveInterpolation => "A軸法向量且C軸插值"
			if( m_cbxInterpolateType.Items.Count > (int)EToolVecInterpolateType.MasterNormalSlaveInterpolation ) {
				m_cbxInterpolateType.Items[ (int)EToolVecInterpolateType.MasterNormalSlaveInterpolation ] =
					 slaveName + "軸平滑";
			}

			// index 4: MasterInterpolationSlaveNormal => "C軸插值且A軸法向量"
			if( m_cbxInterpolateType.Items.Count > (int)EToolVecInterpolateType.MasterInterpolationSlaveNormal ) {
				m_cbxInterpolateType.Items[ (int)EToolVecInterpolateType.MasterInterpolationSlaveNormal ] =
					masterName + "軸平滑";
			}

			// update color legend labels
			m_lblLegendSlave.Text = slaveName + "軸平滑";
			m_lblLegendMaster.Text = masterName + "軸平滑";
		}

		const double DOUBLE_COMPARISON_EPSILON = 1e-6;
	}

	public class ToolVecParam
	{
		public bool IsModified
		{
			get; set;
		}

		public double AngleA_deg
		{
			get; set;
		}

		public double AngleB_deg
		{
			get; set;
		}

		public double Master_deg
		{
			get; set;
		}

		public double Slave_deg
		{
			get; set;
		}

		public EToolVecInterpolateType InterpolateType
		{
			get; set;
		}

		public ToolVecParam( double angleA_deg = 0.0, double angleB_deg = 0.0, double master_deg = 0.0, double slave_deg = 0.0, bool isModified = false, EToolVecInterpolateType interpolateType = EToolVecInterpolateType.Normal )
		{
			AngleA_deg = angleA_deg;
			AngleB_deg = angleB_deg;
			Master_deg = master_deg;
			Slave_deg = slave_deg;
			IsModified = isModified;
			InterpolateType = interpolateType;
		}
	}

	public class RotaryAxisConfig
	{
		// the one can be flipped by +-180
		public string RotaryAxisName
		{
			get; set;
		}

		public string MasterName
		{
			get; set;
		}

		public string SlaveName
		{
			get; set;
		}
	}
}
