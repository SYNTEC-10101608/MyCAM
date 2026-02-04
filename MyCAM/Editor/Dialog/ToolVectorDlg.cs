using MyCAM.App;
using MyCAM.Data;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	public partial class ToolVectorDlg : EditDialogBase<EToolVecInterpolateType>
	{
		public Func<SolveTargetResult> SetKeep;
		public Func<SolveTargetResult> SetZdir;
		public Func<SolveTargetResult> SetRevert;
		public Func<double, double, SolveABResult> MSAngleChanged;
		public Func<double, double, SolveMSResult> ABAngleChanged;
		public Action<EToolVecInterpolateType> TypeChanged;
		public Action AddEditIndex;
		public Action RemoveEditIndex;
		public Action SwitchStartEnd;

		public ToolVectorDlg( EToolVecInterpolateType type, ToolVecParam param, bool isPathReverse )
		{
			// struct would not be null
			InitializeComponent();
			m_IsPathRevese = isPathReverse;
			ResetType( type );
			ResetToolVecParam( param );
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

		public void EnableStartEndSwitch( bool enable )
		{
			m_btnSwitchStartEnd.Visible = enable;
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
			TypeChanged( type );
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

			// Calculate MS angles from AB angles
			SolveMSResult msResult = ABAngleChanged( angleA_deg, angleB_deg );

			// update MS angles if valid
			if( msResult.IsValid ) {
				m_ToolVecParam.Master_deg = msResult.Master_deg;
				m_ToolVecParam.Slave_deg = msResult.Slave_deg;
			}

			// update dialog
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

			// Calculate AB angles from MS angles
			SolveABResult abResult = MSAngleChanged( master_deg, slave_deg );

			// update AB angles if valid
			if( abResult.IsValid ) {
				m_ToolVecParam.AngleA_deg = abResult.AngleA_deg;
				m_ToolVecParam.AngleB_deg = abResult.AngleB_deg;
			}

			// update dialog
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

		void SetTarget( Func<SolveTargetResult> setTargetFunc )
		{
			SolveTargetResult result = setTargetFunc();

			// update param if valid, if not, keep original param
			if( result.IsValid ) {
				m_ToolVecParam.AngleA_deg = result.AngleA_deg;
				m_ToolVecParam.AngleB_deg = result.AngleB_deg;
				m_ToolVecParam.Master_deg = result.Master_deg;
				m_ToolVecParam.Slave_deg = result.Slave_deg;
			}

			// update dialog
			ResetToolVecParam( m_ToolVecParam );
		}

		void m_btnAdd_Click( object sender, EventArgs e )
		{
			AddEditIndex();
			m_btnAdd.Enabled = false;
			m_btnRemove.Enabled = true;
		}

		void m_btnRemove_Click( object sender, EventArgs e )
		{
			RemoveEditIndex();
			m_btnAdd.Enabled = true;
			m_btnRemove.Enabled = false;
		}

		void m_btnSwitchStartEnd_Click( object sender, EventArgs e )
		{
			SwitchStartEnd();
		}

		bool m_IsPathRevese = false;
		ToolVecParam m_ToolVecParam;

		bool bSuppressTypeChangedEvent = false;
		bool bSuppressValueChangedEvent = false;
	}

	public struct SolveTargetResult
	{
		public bool IsValid
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

		public SolveTargetResult( bool isValid = false, double angleA_deg = 0.0, double angleB_deg = 0.0, double master_deg = 0.0, double slave_deg = 0.0 )
		{
			IsValid = isValid;
			AngleA_deg = angleA_deg;
			AngleB_deg = angleB_deg;
			Master_deg = master_deg;
			Slave_deg = slave_deg;
		}
	}

	public struct SolveABResult
	{
		public bool IsValid
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

		public SolveABResult( bool isValid = false, double angleA_deg = 0.0, double angleB_deg = 0.0 )
		{
			IsValid = isValid;
			AngleA_deg = angleA_deg;
			AngleB_deg = angleB_deg;
		}
	}

	public struct SolveMSResult
	{
		public bool IsValid
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

		public SolveMSResult( bool isValid = false, double master_deg = 0.0, double slave_deg = 0.0 )
		{
			IsValid = isValid;
			Master_deg = master_deg;
			Slave_deg = slave_deg;
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
}
