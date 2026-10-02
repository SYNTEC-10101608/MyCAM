using MyCAM.Data;
using MyCAM.Helper;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor
{
	internal partial class SpecialToolVecEditDlg : EditDialogBase<object>
	{
		public Action<double, double> MSAngleChanged;
		public Action<ESpecialToolVecType> TypeChanged;
		public Action<bool> FlipRotaryAxis;
		public Action RotateToFixDir;
		public Action RotateToZDir;
		public Action<EMachiningDir> MachiningDirChanged;

		public SpecialToolVecEditDlg( RotaryAxisConfig config )
		{
			InitializeComponent();
			m_RotaryAxisConfig = config;

			// single rotary axis machine has fewer degrees of freedom, adjust UI
			if( m_RotaryAxisConfig.IsSingleAxis ) {
				ApplySingleAxisUI();
			}
			ApplyResourcesToUI();
			BuildMachiningDirItems();
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
				case ESpecialToolVecType.LockMaster:
					m_rbtnLockMaster.Checked = true;
					break;
				case ESpecialToolVecType.LockSlave:
					m_rbtnLockSlave.Checked = true;
					break;
				case ESpecialToolVecType.Normal:
				default:
					m_rbtNormal.Checked = true;
					break;
			}
			m_IsUpdatingTypeUI = false;
		}

		// the assigned machining direction is only meaningful for the ZDir type
		public void ResetMachiningDir( EMachiningDir dir )
		{
			if( m_cbxMachiningDir.Items.Count == 0 ) {
				return;
			}
			int nIndex = m_MachiningDirList.IndexOf( dir );
			if( nIndex < 0 || nIndex >= m_cbxMachiningDir.Items.Count ) {
				return;
			}
			m_IsUpdatingMachiningDirUI = true;
			m_cbxMachiningDir.SelectedIndex = nIndex;
			m_IsUpdatingMachiningDirUI = false;
		}

		public void SetUIPermissions( bool enableMSDeg, bool enableSwitchDir, bool enableMachiningDir )
		{
			m_gbxMSDeg.Enabled = enableMSDeg;
			m_gbxSwitchDir.Enabled = enableSwitchDir;
			m_lblMachiningDir.Enabled = enableMachiningDir;
			m_cbxMachiningDir.Enabled = enableMachiningDir;

			// single-axis protection takes precedence over general UI permissions
			if( m_RotaryAxisConfig.IsSingleAxis ) {
				ApplySingleAxisUI();
			}
		}

		// show the hint under the combobox when the machining direction is not supported by the spindle type machine
		public void SetMachiningDirHintVisible( bool isVisible )
		{
			m_lblMachiningDirHint.Visible = isVisible;
		}

		#region Set textbox value silently

		public void ResetMSAngle( double master_deg, double slave_deg )
		{
			m_dbtnMaster.SilentSetValue = (decimal)master_deg;
			m_dbtnSlave.SilentSetValue = (decimal)slave_deg;
		}

		#endregion

		#region UI Setting

		public bool GetMSAngleFromDialog( out double master_deg, out double slave_deg )
		{
			master_deg = (double)m_dbtnMaster.Value;
			slave_deg = (double)m_dbtnSlave.Value;

			// single rotary axis machine: force the locked axis to 0, prevent legacy data from leaking in
			if( m_RotaryAxisConfig.Variant == MixTypeVariant.MasterOnly ) {
				slave_deg = 0;
			}
			else if( m_RotaryAxisConfig.Variant == MixTypeVariant.SlaveOnly ) {
				master_deg = 0;
			}
			return true;
		}

		void ApplyResourcesToUI()
		{
			this.Text = m_LanguageManager.TryGetString( "FormTitle", this.Text );
			m_gbxType.Text = m_LanguageManager.TryGetString( "m_gbxType", m_gbxType.Text );
			m_rbtNormal.Text = m_LanguageManager.TryGetString( "m_rbtNormal", m_rbtNormal.Text );
			m_rbtnFixDir.Text = m_LanguageManager.TryGetString( "m_rbtnFixDir", m_rbtnFixDir.Text );
			m_rbtnAssignMS.Text = m_LanguageManager.TryGetString( "m_rbtnAssignMS", m_rbtnAssignMS.Text );
			m_gbxMSDeg.Text = m_LanguageManager.TryGetString( "m_gbxMSDeg", m_gbxMSDeg.Text );
			m_gbxCRotate.Text = m_LanguageManager.TryGetString( "m_gbxCRotate", m_gbxCRotate.Text );
			m_gbxSwitchDir.Text = m_LanguageManager.TryGetString( "m_gbxSwitchDir", m_gbxSwitchDir.Text );
			m_btnFixDir.Text = m_LanguageManager.TryGetString( "m_btnFixDir", m_btnFixDir.Text );
			m_btnConfirm.Text = m_LanguageManager.TryGetString( "m_btnConfirm", m_btnConfirm.Text );
			m_gbxMachiningDir.Text = m_LanguageManager.TryGetString( "m_gbxMachiningDir", m_gbxMachiningDir.Text );
			m_lblMachiningDir.Text = m_LanguageManager.TryGetString( "m_lblMachiningDir", m_lblMachiningDir.Text );
			m_lblMachiningDirHint.Text = m_LanguageManager.TryGetString( "m_lblMachiningDirHint", m_lblMachiningDirHint.Text );

			// dynamic labels depend on RotaryAxisConfig
			string szMDegFormat = m_LanguageManager.TryGetString( "m_lblMDeg_Format", "{0}軸旋轉角度" );
			string szSDegFormat = m_LanguageManager.TryGetString( "m_lblSDeg_Format", "{0}軸旋轉角度" );
			m_lblMDeg.Text = string.Format( szMDegFormat, m_RotaryAxisConfig.MasterName );
			m_lblSDeg.Text = string.Format( szSDegFormat, m_RotaryAxisConfig.SlaveName );
			string szLockMasterFormat = m_LanguageManager.TryGetString( "m_rbtnLockMaster", m_rbtnLockMaster.Text );
			string szLockSlaveFormat = m_LanguageManager.TryGetString( "m_rbtnLockSlave", m_rbtnLockSlave.Text );
			m_rbtnLockMaster.Text = string.Format( szLockMasterFormat, m_RotaryAxisConfig.MasterName );
			m_rbtnLockSlave.Text = string.Format( szLockSlaveFormat, m_RotaryAxisConfig.SlaveName );
			m_btnRotaryPos.Text = m_RotaryAxisConfig.RotaryAxisName + " +";
			m_btnRotaryNeg.Text = m_RotaryAxisConfig.RotaryAxisName + " -";

			// set rotary axis name
			string szRotaryDirName = RotaryAxisHelper.ConvertRotaryAxisDirName( m_RotaryAxisConfig.RotaryAxisType );
			string szrbtZFormat = m_LanguageManager.TryGetString( "m_rbtnZ", m_rbtnZ.Text );
			string szBtnZFormat = m_LanguageManager.TryGetString( "m_btnZ", m_btnZ.Text );
			m_rbtnZ.Text = string.Format( szrbtZFormat, szRotaryDirName );
			m_btnZ.Text = string.Format( szBtnZFormat, szRotaryDirName );
		}

		// the RefAxis and CrossAxis name depend on the rotary axis, so build the items after the config is ready
		void BuildMachiningDirItems()
		{
			m_IsUpdatingMachiningDirUI = true;
			m_cbxMachiningDir.Items.Clear();
			foreach( EMachiningDir dir in m_MachiningDirList ) {
				string szName = ( dir == EMachiningDir.None )
					? m_LanguageManager.TryGetString( "MachiningDir_None", "無" )
					: RotaryAxisHelper.ConvertMachiningDirName( dir, m_RotaryAxisConfig.RotaryAxisType );
				m_cbxMachiningDir.Items.Add( szName );
			}
			m_cbxMachiningDir.SelectedIndex = 0;
			m_IsUpdatingMachiningDirUI = false;
		}

		void m_cbxMachiningDir_SelectedIndexChanged( object sender, EventArgs e )
		{
			if( m_IsUpdatingMachiningDirUI ) {
				return;
			}
			int nIndex = m_cbxMachiningDir.SelectedIndex;
			if( nIndex < 0 || nIndex >= m_MachiningDirList.Count ) {
				return;
			}
			MachiningDirChanged?.Invoke( m_MachiningDirList[ nIndex ] );
		}

		#endregion

		const double DOUBLE_COMPARISON_EPSILON = 1e-3;
		SpecialToolVecData m_ToolVecData;
		RotaryAxisConfig m_RotaryAxisConfig;
		bool m_IsUpdatingTypeUI = false;
		bool m_IsUpdatingMachiningDirUI = false;

		// defines the combo box order
		readonly List<EMachiningDir> m_MachiningDirList = new List<EMachiningDir>
		{
			EMachiningDir.None,
			EMachiningDir.RefAxisPositive,
			EMachiningDir.CrossAxisPositive,
			EMachiningDir.RefAxisNegative,
			EMachiningDir.CrossAxisNegative
		};
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "SpecialToolVecEditDlg" );

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

		void m_rbtnLockMaster_CheckedChanged( object sender, EventArgs e )
		{
			// only process when this radio button is checked (not unchecked)
			if( !m_rbtnLockMaster.Checked || m_IsUpdatingTypeUI ) {
				return;
			}
			m_IsUpdatingTypeUI = true;
			SetAsLockMasterMode();
			TypeChange();
			m_IsUpdatingTypeUI = false;
		}

		void m_rbtnLockSlave_CheckedChanged( object sender, EventArgs e )
		{
			// only process when this radio button is checked (not unchecked)
			if( !m_rbtnLockSlave.Checked || m_IsUpdatingTypeUI ) {
				return;
			}
			m_IsUpdatingTypeUI = true;
			SetAsLockSlaveMode();
			TypeChange();
			m_IsUpdatingTypeUI = false;
		}

		#region UI protection

		void ApplySingleAxisUI()
		{
			// disable the locked axis, it does not physically exist
			if( m_RotaryAxisConfig.Variant == MixTypeVariant.MasterOnly ) {
				m_lblSDeg.Enabled = false;
				m_dbtnSlave.Enabled = false;
				m_rbtnLockSlave.Enabled = false;
			}
			else if( m_RotaryAxisConfig.Variant == MixTypeVariant.SlaveOnly ) {
				m_lblMDeg.Enabled = false;
				m_dbtnMaster.Enabled = false;
				m_rbtnLockMaster.Enabled = false;
			}

			// single axis machine has only one IK solution, disable flip controls
			m_gbxCRotate.Enabled = false;
			m_gbxMachiningDir.Enabled = false;
		}

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
				case ESpecialToolVecType.LockMaster:
					m_rbtnLockMaster.Checked = true;
					break;
				case ESpecialToolVecType.LockSlave:
					m_rbtnLockSlave.Checked = true;
					break;
			}
		}

		void SetAsDefaultMode()
		{
			CheckRadioButton( ESpecialToolVecType.Normal );
		}

		void SetAsFixDirMode()
		{
			CheckRadioButton( ESpecialToolVecType.FixDir );
		}

		void SetAsAssignMSMode()
		{
			CheckRadioButton( ESpecialToolVecType.AssignMS );
		}

		void SetAsZDirMode()
		{
			CheckRadioButton( ESpecialToolVecType.ZDir );
		}

		void SetAsLockMasterMode()
		{
			CheckRadioButton( ESpecialToolVecType.LockMaster );
		}

		void SetAsLockSlaveMode()
		{
			CheckRadioButton( ESpecialToolVecType.LockSlave );
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
			else if( m_rbtnLockMaster.Checked ) {
				TypeChanged?.Invoke( ESpecialToolVecType.LockMaster );
			}
			else if( m_rbtnLockSlave.Checked ) {
				TypeChanged?.Invoke( ESpecialToolVecType.LockSlave );
			}
			else {
				TypeChanged?.Invoke( ESpecialToolVecType.Normal );
			}
		}

		#endregion

		#region Data synchronization

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
			RotateToZDir?.Invoke();
		}

		void m_btnFixDir_Click( object sender, EventArgs e )
		{
			RotateToFixDir?.Invoke();
		}
	}
}
