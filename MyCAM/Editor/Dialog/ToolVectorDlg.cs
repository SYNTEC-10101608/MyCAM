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
			ApplyResourcesToUI();
			UpdateInterpolateTypeNames();
		}

		void ApplyResourcesToUI()
		{
			this.Text = m_LanguageManager.TryGetString( "ToolVectorDlg", this.Text );
			m_lblOnePntToolVecAdjust.Text = m_LanguageManager.TryGetString( "m_lblOnePntToolVecAdjust", m_lblOnePntToolVecAdjust.Text );
			m_lblADeg.Text = m_LanguageManager.TryGetString( "m_lblADeg", m_lblADeg.Text );
			m_lblBDeg.Text = m_LanguageManager.TryGetString( "m_lblBDeg", m_lblBDeg.Text );
			m.Text = m_LanguageManager.TryGetString( "m", m.Text );
			m_lblToolDirQuickSetBtn.Text = m_LanguageManager.TryGetString( "m_lblToolDirQuickSetBtn", m_lblToolDirQuickSetBtn.Text );
			m_btnZDir.Text = m_LanguageManager.TryGetString( "m_btnZDir", m_btnZDir.Text );
			m_btnKeep.Text = m_LanguageManager.TryGetString( "m_btnKeep", m_btnKeep.Text );
			m_btnRevert.Text = m_LanguageManager.TryGetString( "m_btnRevert", m_btnRevert.Text );
			m_lblStartEnd.Text = m_LanguageManager.TryGetString( "m_lblStartEnd", m_lblStartEnd.Text );
			m_btnSwitchStartEnd.Text = m_LanguageManager.TryGetString( "m_btnSwitchStartEnd", m_btnSwitchStartEnd.Text );
			m_lblSetBtn.Text = m_LanguageManager.TryGetString( "m_lblSetBtn", m_lblSetBtn.Text );
			m_btnAdd.Text = m_LanguageManager.TryGetString( "m_btnAdd", m_btnAdd.Text );
			m_btnRemove.Text = m_LanguageManager.TryGetString( "m_btnRemove", m_btnRemove.Text );
			m_btnClear.Text = m_LanguageManager.TryGetString( "m_btnClear", m_btnClear.Text );
			m_gpbStartPnt.Text = m_LanguageManager.TryGetString( "m_gpbStartPnt", m_gpbStartPnt.Text );
			m_btnToStart.Text = m_LanguageManager.TryGetString( "m_btnToStart", m_btnToStart.Text );
			m_gpbPnt.Text = m_LanguageManager.TryGetString( "m_gpbPnt", m_gpbPnt.Text );
			m_btnPrev.Text = m_LanguageManager.TryGetString( "m_btnPrev", m_btnPrev.Text );
			m_btnNext.Text = m_LanguageManager.TryGetString( "m_btnNext", m_btnNext.Text );
			m_btnToEnd.Text = m_LanguageManager.TryGetString( "m_btnToEnd", m_btnToEnd.Text );
			m_gpbCtrlPnt.Text = m_LanguageManager.TryGetString( "m_gpbCtrlPnt", m_gpbCtrlPnt.Text );
			m_btnPreCtrlPnt.Text = m_LanguageManager.TryGetString( "m_btnPreCtrlPnt", m_btnPreCtrlPnt.Text );
			m_btnNextCtrlPnt.Text = m_LanguageManager.TryGetString( "m_btnNextCtrlPnt", m_btnNextCtrlPnt.Text );
			m_gbxColorLegend.Text = m_LanguageManager.TryGetString( "m_gbxColorLegend", m_gbxColorLegend.Text );
			m_lblLegendNormal.Text = m_LanguageManager.TryGetString( "m_lblLegendNormal", m_lblLegendNormal.Text );
			m_lblLegendRotary.Text = m_LanguageManager.TryGetString( "m_lblLegendRotary", m_lblLegendRotary.Text );
			m_lblLegendBevel.Text = m_LanguageManager.TryGetString( "m_lblLegendBevel", m_lblLegendBevel.Text );
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

			m_lblStartOrEnd.Text = start ? m_LanguageManager.TryGetString( "m_lblStartOrEnd_Start", START_OR_END_START ) : m_LanguageManager.TryGetString( "m_lblStartOrEnd_End", START_OR_END_END );
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
			string masterName = m_RotaryAxisConfig.MasterName;
			string slaveName = m_RotaryAxisConfig.SlaveName;

			// Ensure we have enough items in the combo box
			while( m_cbxInterpolateType.Items.Count < 5 ) {
				m_cbxInterpolateType.Items.Add( "" );
			}

			// index 0~2: fixed types, update via language manager
			m_cbxInterpolateType.Items[ (int)EToolVecInterpolateType.Normal ] =
				m_LanguageManager.TryGetString( "InterpolateType_Normal", INTERPOLATE_TYPE_NORMAL );
			m_cbxInterpolateType.Items[ (int)EToolVecInterpolateType.VectorInterpolation ] =
				m_LanguageManager.TryGetString( "InterpolateType_Rotary", INTERPOLATE_TYPE_ROTARY );
			m_cbxInterpolateType.Items[ (int)EToolVecInterpolateType.TiltAngleInterpolation ] =
				m_LanguageManager.TryGetString( "InterpolateType_Bevel", INTERPOLATE_TYPE_BEVEL );

			string szAxisSmooth = m_LanguageManager.TryGetString( "ToolVectorDlg_AxisSmooth", START_OR_END_END );

			// index 3: MasterNormalSlaveInterpolation
			m_cbxInterpolateType.Items[ (int)EToolVecInterpolateType.MasterNormalSlaveInterpolation ] =
				 slaveName + szAxisSmooth;

			// index 4: MasterInterpolationSlaveNormal
			m_cbxInterpolateType.Items[ (int)EToolVecInterpolateType.MasterInterpolationSlaveNormal ] =
				masterName + szAxisSmooth;

			// update color legend labels
			m_lblLegendSlave.Text = slaveName + szAxisSmooth;
			m_lblLegendMaster.Text = masterName + szAxisSmooth;
		}

		const double DOUBLE_COMPARISON_EPSILON = 1e-6;
		const string INTERPOLATE_TYPE_NORMAL = "維持默認";
		const string INTERPOLATE_TYPE_ROTARY = "旋轉軸平滑";
		const string INTERPOLATE_TYPE_BEVEL = "傾角切坡口";
		const string START_OR_END_START = "當前位置：起點";
		const string START_OR_END_END = "當前位置：終點";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ToolVectorDlg" );
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
