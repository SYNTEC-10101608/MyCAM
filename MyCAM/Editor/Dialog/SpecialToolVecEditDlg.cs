using MyCAM.Data;
using System;

namespace MyCAM.Editor
{
	internal partial class SpecialToolVecEditDlg : EditDialogBase<object>
	{
		public Action<double, double> MSAngleChanged;
		public Action<ESpecialToolVecType> TypeChanged;
		public Action<bool> FlipRotaryAxis;
		public Action RotateToFixDir;

		public SpecialToolVecEditDlg( RotaryAxisConfig config )
		{
			InitializeComponent();
			m_RotaryAxisConfig = config;
			ApplyResourcesToUI();
		}

		public void SetSinglePathUI( SpecialToolVecData data )
		{
			SwitchType( data.Type );
			SetUIPermissions( data.Type, false );
			SetParam( data );
		}

		public void SwitchType( ESpecialToolVecType type )
		{
			m_IsUpdatingTypeUI = true;
			switch( type ) {
				case ESpecialToolVecType.FixDir:
					m_rbtnFixDir.Checked = true;
					break;
				case ESpecialToolVecType.AssignMS:
					m_rbtnAssignMS.Checked = true;
					break;
				case ESpecialToolVecType.ZDir:
					m_rbtnZ.Checked = true;
					break;
				case ESpecialToolVecType.Normal:
				default:
					m_rbtNormal.Checked = true;
					break;
			}
			m_IsUpdatingTypeUI = false;
		}

		public void SetUIPermissions( ESpecialToolVecType type, bool isNeedToChangeDefaultValue = true )
		{
			bool isAssignMS = type == ESpecialToolVecType.AssignMS;
			m_gbxMSDeg.Enabled = isAssignMS;
			m_gbxMSDeg.Visible = isAssignMS;
			if( m_gbxMSDeg.Enabled == false ) {
				SetMSAngleSilent( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
			}
		}

		#region Set textbox value silently

		public void SetMSAngleSilent( double master_deg, double slave_deg )
		{
			m_dbtnMaster.SilentSetValue = (decimal)master_deg;
			m_dbtnSlave.SilentSetValue = (decimal)slave_deg;
		}

		#endregion

		#region UI Setting

		void ApplyResourcesToUI()
		{
			this.Text = m_LanguageManager.TryGetString( "FormTitle", this.Text );
			m_gbxType.Text = m_LanguageManager.TryGetString( "m_gbxType", m_gbxType.Text );
			m_rbtNormal.Text = m_LanguageManager.TryGetString( "m_rbtNormal", m_rbtNormal.Text );
			m_rbtnFixDir.Text = m_LanguageManager.TryGetString( "m_rbtnFixDir", m_rbtnFixDir.Text );
			m_rbtnZ.Text = m_LanguageManager.TryGetString( "m_rbtnZ", m_rbtnZ.Text );
			m_rbtnAssignMS.Text = m_LanguageManager.TryGetString( "m_rbtnAssignMS", m_rbtnAssignMS.Text );
			m_gbxMSDeg.Text = m_LanguageManager.TryGetString( "m_gbxMSDeg", m_gbxMSDeg.Text );
			m_gbxCRotate.Text = m_LanguageManager.TryGetString( "m_gbxCRotate", m_gbxCRotate.Text );
			m_gbxSwitchDir.Text = m_LanguageManager.TryGetString( "m_gbxSwitchDir", m_gbxSwitchDir.Text );
			m_btnZ.Text = m_LanguageManager.TryGetString( "m_btnZ", m_btnZ.Text );
			m_btnFixDir.Text = m_LanguageManager.TryGetString( "m_btnFixDir", m_btnFixDir.Text );
			m_btnConfirm.Text = m_LanguageManager.TryGetString( "m_btnConfirm", m_btnConfirm.Text );

			// dynamic labels depend on RotaryAxisConfig
			string szMDegFormat = m_LanguageManager.TryGetString( "m_lblMDeg_Format", "{0}軸旋轉角度" );
			string szSDegFormat = m_LanguageManager.TryGetString( "m_lblSDeg_Format", "{0}軸旋轉角度" );
			m_lblMDeg.Text = string.Format( szMDegFormat, m_RotaryAxisConfig.MasterName );
			m_lblSDeg.Text = string.Format( szSDegFormat, m_RotaryAxisConfig.SlaveName );
			m_btnRotaryPos.Text = m_RotaryAxisConfig.RotaryAxisName + " +";
			m_btnRotaryNeg.Text = m_RotaryAxisConfig.RotaryAxisName + " -";
		}

		#endregion

		const double DOUBLE_COMPARISON_EPSILON = 1e-6;
		SpecialToolVecData m_ToolVecData;
		RotaryAxisConfig m_RotaryAxisConfig;
		const double DEFAULT_Angle_Deg = 0;
		bool m_IsUpdatingTypeUI = false;
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "SpecialToolVecEditDlg" );

		void SetParam( SpecialToolVecData data )
		{
			SetMSAngleSilent( DEFAULT_Angle_Deg, DEFAULT_Angle_Deg );
			if( data.Type != ESpecialToolVecType.AssignMS ) {
				return;
			}
			SetMSAngleSilent( data.MasterAngle_deg, data.SlaveAngle_deg );
		}

		void m_btnCancel_Click( object sender, EventArgs e )
		{
			Cancel?.Invoke();
			Close();
		}

		void m_rbtnAssignMS_CheckedChanged( object sender, EventArgs e )
		{
			// only process when this radio button is checked (not unchecked)
			if( !m_rbtnAssignMS.Checked || m_IsUpdatingTypeUI ) {
				return;
			}
			m_IsUpdatingTypeUI = true;
			SetAsAssignMSMode();
			TypeChange();
			m_IsUpdatingTypeUI = false;
		}

		void m_rbtNormal_CheckedChanged( object sender, EventArgs e )
		{
			// only process when this radio button is checked (not unchecked)
			if( !m_rbtNormal.Checked || m_IsUpdatingTypeUI ) {
				return;
			}
			m_IsUpdatingTypeUI = true;
			SetAsDefaultMode();
			TypeChange();
			m_IsUpdatingTypeUI = false;
		}


		void m_rbtnZ_CheckedChanged( object sender, EventArgs e )
		{
			if( !m_rbtnZ.Checked || m_IsUpdatingTypeUI ) {
				return;
			}
			m_IsUpdatingTypeUI = true;
			SetAsZDirMode();
			TypeChange();
			m_IsUpdatingTypeUI = false;
		}

		void m_rbtnFixDir_CheckedChanged( object sender, EventArgs e )
		{
			// only process when this radio button is checked (not unchecked)
			if( !m_rbtnFixDir.Checked || m_IsUpdatingTypeUI ) {
				return;
			}
			m_IsUpdatingTypeUI = true;
			SetAsFixDirMode();
			TypeChange();
			m_IsUpdatingTypeUI = false;
		}

		#region UI protection m_btnSet_Click

		void CheckRadioButton( ESpecialToolVecType type )
		{
			switch( type ) {
				case ESpecialToolVecType.Normal:
					m_rbtNormal.Checked = true;
					break;
				case ESpecialToolVecType.FixDir:
					m_rbtnFixDir.Checked = true;
					break;
				case ESpecialToolVecType.AssignMS:
					m_rbtnAssignMS.Checked = true;
					break;
			}
		}

		void SetAsDefaultMode()
		{
			CheckRadioButton( ESpecialToolVecType.Normal );
			SetUIPermissions( ESpecialToolVecType.Normal );
		}

		void SetAsFixDirMode()
		{
			CheckRadioButton( ESpecialToolVecType.FixDir );
			SetUIPermissions( ESpecialToolVecType.FixDir );
		}

		void SetAsAssignMSMode()
		{
			CheckRadioButton( ESpecialToolVecType.AssignMS );
			SetUIPermissions( ESpecialToolVecType.AssignMS );
		}

		void SetAsZDirMode()
		{
			CheckRadioButton( ESpecialToolVecType.ZDir );
			SetUIPermissions( ESpecialToolVecType.ZDir );
		}

		void TypeChange()
		{
			if( m_rbtnFixDir.Checked ) {
				TypeChanged?.Invoke( ESpecialToolVecType.FixDir );
			}
			else if( m_rbtnAssignMS.Checked ) {
				TypeChanged?.Invoke( ESpecialToolVecType.AssignMS );
			}
			else if( m_rbtnZ.Checked ) {
				TypeChanged?.Invoke( ESpecialToolVecType.ZDir );
			}
			else {
				TypeChanged?.Invoke( ESpecialToolVecType.Normal );
			}
		}


		#endregion

		#region Data synchronization

		bool GetMSAngleFromDialog( out double master_deg, out double slave_deg )
		{
			master_deg = (double)m_dbtnMaster.Value;
			slave_deg = (double)m_dbtnSlave.Value;
			return true;
		}

		void HandleMSAngleChanged()
		{
			if( !GetMSAngleFromDialog( out double master_deg, out double slave_deg ) ) {
				return;
			}

			// avoid repeated trigger
			if( m_ToolVecData != null &&
				Math.Abs( master_deg - m_ToolVecData.MasterAngle_deg ) <= DOUBLE_COMPARISON_EPSILON &&
				Math.Abs( slave_deg - m_ToolVecData.SlaveAngle_deg ) <= DOUBLE_COMPARISON_EPSILON ) {
				return;
			}
			MSAngleChanged?.Invoke( master_deg, slave_deg );
		}

		#endregion

		#region Value changed

		void m_dbtnMaster_DebouncedValueChanged( object sender, EventArgs e )
		{
			HandleMSAngleChanged();
		}

		void m_dbtnSlave_DebouncedValueChanged( object sender, EventArgs e )
		{
			HandleMSAngleChanged();
		}


		#endregion

		void m_btnRotaryPos_Click( object sender, EventArgs e )
		{
			FlipRotaryAxis?.Invoke( true );
		}

		void m_btnRotaryNeg_Click( object sender, EventArgs e )
		{
			FlipRotaryAxis?.Invoke( false );
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			Close();
		}

		void m_btnZ_Click( object sender, EventArgs e )
		{
			TypeChanged?.Invoke( ESpecialToolVecType.AssignMS );
		}

		void m_btnFixDir_Click( object sender, EventArgs e )
		{
			RotateToFixDir?.Invoke();
		}
	}
}
