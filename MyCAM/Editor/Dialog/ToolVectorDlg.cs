using MyCAM.App;
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
		public Action<double, double> MSAngleChanged;
		public Action<double, double> ABAngleChanged;
		public Action<EToolVecInterpolateType> TypeChanged;
		public Action AddEditIndex;
		public Action RemoveEditIndex;
		public Action SwitchStartEnd;
		public Action<bool> MoveIndex;
		public Action<bool> ToStartOrEnd;
		public Action<bool> FlipRotaryAxis;

		public ToolVectorDlg( EToolVecInterpolateType type, ToolVecParam param, bool isPathReverse, RotaryAxisConfig config )
		{
			// struct would not be null
			InitializeComponent();
			m_IsPathRevese = isPathReverse;
			ResetType( type );
			ResetToolVecParam( param );

			// update rotary axis name
			m_RotaryAxisConfig = config;
			m_lblMaster.Text = m_RotaryAxisConfig.MasterName;
			m_lblSlave.Text = m_RotaryAxisConfig.SlaveName;
			if( m_RotaryAxisConfig.RotaryAxis == ETypeOfRotaryAxis.Master ) {
				m_btnRotaryPos.Text = m_RotaryAxisConfig.MasterName + " +";
				m_btnRotaryNeg.Text = m_RotaryAxisConfig.MasterName + " -";
			}
			else {
				m_btnRotaryPos.Text = m_RotaryAxisConfig.SlaveName + " +";
				m_btnRotaryNeg.Text = m_RotaryAxisConfig.SlaveName + " -";
			}
		}

		public void ResetType( EToolVecInterpolateType type )
		{
			bSuppressTypeChangedEvent = true;

			// update modify type
			switch( type ) {
				case EToolVecInterpolateType.VectorInterpolation:
					m_rbtVecSpace.Checked = true;
					break;
				case EToolVecInterpolateType.TiltAngleInterpolation:
					m_rbtTilt.Checked = true;
					break;
				default:
					m_rbtNormal.Checked = true;
					break;
			}
			bSuppressTypeChangedEvent = false;
		}

		public void ResetToolVecParam( ToolVecParam toolVecParam )
		{
			bSuppressValueChangedEvent = true;
			m_ToolVecParam = toolVecParam;

			// no selected index param, disable edit UI
			if( m_ToolVecParam == null ) {
				m_gbxIndexParam.Enabled = false;
				return;
			}

			// update index edit UI
			m_gbxIndexParam.Enabled = true;
			m_tbxAngleA.Text = m_IsPathRevese ? ( -m_ToolVecParam.AngleA_deg ).ToString( "F3" ) : m_ToolVecParam.AngleA_deg.ToString( "F3" );
			m_tbxAngleB.Text = m_IsPathRevese ? ( -m_ToolVecParam.AngleB_deg ).ToString( "F3" ) : m_ToolVecParam.AngleB_deg.ToString( "F3" );
			m_tbxMaster.Text = m_ToolVecParam.Master_deg.ToString( "F3" );
			m_tbxSlave.Text = m_ToolVecParam.Slave_deg.ToString( "F3" );
			m_btnRemove.Enabled = m_ToolVecParam.IsModified;
			m_btnAdd.Enabled = !m_ToolVecParam.IsModified;
			bSuppressValueChangedEvent = false;
		}

		public void EnableStartEndSwitch( bool enable, bool start )
		{
			m_btnSwitchStartEnd.Visible = enable;
			m_lblStartOrEnd.Visible = enable;
			m_lblStartOrEnd.Text = start ? "當前位置：起點" : "當前位置：終點";
		}


		// UI event - interpolate type changed
		void m_rbtNormal_CheckedChanged( object sender, EventArgs e )
		{
			RaiseTypeChanged( EToolVecInterpolateType.Normal );
		}

		void m_rbtVecSpace_CheckedChanged( object sender, EventArgs e )
		{
			RaiseTypeChanged( EToolVecInterpolateType.VectorInterpolation );
		}

		void m_rbtTilt_CheckedChanged( object sender, EventArgs e )
		{
			RaiseTypeChanged( EToolVecInterpolateType.TiltAngleInterpolation );
		}

		void RaiseTypeChanged( EToolVecInterpolateType type )
		{
			if( bSuppressTypeChangedEvent ) {
				return;
			}
			TypeChanged?.Invoke( type );
		}

		// UI event - Index param value changed
		void m_tbxAngleA_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				HandleABAngleChanged();
			}
		}

		void m_tbxAngleB_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				HandleABAngleChanged();
			}
		}

		void m_tbxAngleA_Leave( object sender, EventArgs e )
		{
			HandleABAngleChanged();
		}

		void m_tbxAngleB_Leave( object sender, EventArgs e )
		{
			HandleABAngleChanged();
		}

		void m_tbxMaster_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				HandleMSAngleChanged();
			}
		}

		void m_tbxSlave_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				HandleMSAngleChanged();
			}
		}

		void m_tbxMaster_Leave( object sender, EventArgs e )
		{
			HandleMSAngleChanged();
		}

		void m_tbxSlave_Leave( object sender, EventArgs e )
		{
			HandleMSAngleChanged();
		}

		void HandleABAngleChanged()
		{
			if( bSuppressValueChangedEvent ) {
				return;
			}

			// Get AB angles from dialog
			if( !GetABAngleFromDialog( out double angleA_deg, out double angleB_deg ) ) {
				return;
			}
			if( angleA_deg == m_ToolVecParam.AngleA_deg && angleB_deg == m_ToolVecParam.AngleB_deg ) {
				return;
			}
			ABAngleChanged?.Invoke( angleA_deg, angleB_deg );
			ResetToolVecParam( m_ToolVecParam );
		}

		void HandleMSAngleChanged()
		{
			if( bSuppressValueChangedEvent ) {
				return;
			}

			// Get MS angles from dialog
			if( !GetMSAngleFromDialog( out double master_deg, out double slave_deg ) ) {
				return;
			}
			if( master_deg == m_ToolVecParam.Master_deg && slave_deg == m_ToolVecParam.Slave_deg ) {
				return;
			}
			MSAngleChanged?.Invoke( master_deg, slave_deg );
			ResetToolVecParam( m_ToolVecParam );
		}

		bool GetABAngleFromDialog( out double angleA_deg, out double angleB_deg )
		{
			// Parse AB angles from textboxes
			angleA_deg = 0;
			angleB_deg = 0;
			if( !double.TryParse( m_tbxAngleA.Text, out angleA_deg ) ||
				!double.TryParse( m_tbxAngleB.Text, out angleB_deg ) ) {
				MyApp.Logger.ShowOnLogPanel( "無效字串", MyApp.NoticeType.Warning );
				return false;
			}

			// Convert to actual angles (considering path reverse)
			angleA_deg = m_IsPathRevese ? -angleA_deg : angleA_deg;
			angleB_deg = m_IsPathRevese ? -angleB_deg : angleB_deg;
			return true;
		}

		bool GetMSAngleFromDialog( out double master_deg, out double slave_deg )
		{
			// Parse MS angles from textboxes
			master_deg = 0;
			slave_deg = 0;
			if( !double.TryParse( m_tbxMaster.Text, out master_deg ) ||
				!double.TryParse( m_tbxSlave.Text, out slave_deg ) ) {
				MyApp.Logger.ShowOnLogPanel( "無效字串", MyApp.NoticeType.Warning );
				return false;
			}
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
		}

		void m_btnRotaryNeg_Click( object sender, EventArgs e )
		{
			FlipRotaryAxis?.Invoke( false );
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

		bool bSuppressTypeChangedEvent = false;
		bool bSuppressValueChangedEvent = false;
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

		void MoveToNextTick( object sender, EventArgs e )
		{
			MoveIndex?.Invoke( true );
		}

		void MoveToLastTick( object sender, EventArgs e )
		{
			MoveIndex?.Invoke( false );
		}
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

		public ToolVecParam( double angleA_deg = 0.0, double angleB_deg = 0.0, double master_deg = 0.0, double slave_deg = 0.0, bool isModified = false )
		{
			AngleA_deg = angleA_deg;
			AngleB_deg = angleB_deg;
			Master_deg = master_deg;
			Slave_deg = slave_deg;
			IsModified = isModified;
		}
	}

	// to determin which is the rotating one
	public enum ETypeOfRotaryAxis
	{
		Master,
		Slave,
	}

	public class RotaryAxisConfig
	{
		public ETypeOfRotaryAxis RotaryAxis
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
