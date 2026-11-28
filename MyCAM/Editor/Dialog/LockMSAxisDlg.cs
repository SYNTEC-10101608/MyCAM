using System;
using System.Collections.Generic;
using CommonUI.Theme;
using MyCAM.Data;
using MyCAM.Helper;

namespace MyCAM.Editor
{
	internal partial class LockMSAxisDlg : EditDialogBase<object>
	{
		public event Action<bool> OnNormalMode;
		public Action<bool> FlipRotaryAxis;
		public event Action<bool> checkLockMasterChanged;
		public event Action<bool> checkLockSlaveChanged;
		public event Action<double> MasterChanged;
		public event Action<double> SlaveChanged;
		public Action<EMachiningDir> MachiningDirChanged;

		public LockMSAxisDlg( RotaryAxisConfig config, FiveAxisType machineType )
		{
			InitializeComponent();
			ApplyLanguage();
			m_RotaryAxisConfig = config;
			m_IsSpindle = machineType == FiveAxisType.Spindle;
			m_lblMachiningDirHint.Visible = m_IsSpindle;
			InitLocalizedTexts();
			BuildMachiningDirItems();
		}

		#region Set UI for external api

		public void SwitchTypeAsNormal( bool isNormal )
		{
			m_IsUpdatingModeUI = true;
			try {
				if( isNormal ) {
					m_rdoDefault.Checked = true;
				}
				else {
					m_rdoLockAB.Checked = true;
				}
			}
			finally {
				m_IsUpdatingModeUI = false;
			}
			UpdateModeUI( isNormal );
		}

		public void CheckLockMSArea( bool isLockMaster, bool isLockSlave, bool isSingleSelect = false )
		{
			m_IsUpdatingModeUI = true;
			try {
				m_chkLockMaster.Checked = isLockMaster;
				m_chkLockSlave.Checked = isLockSlave;
			}
			finally {
				m_IsUpdatingModeUI = false;
			}
			EnableLockMArea( isLockMaster );
			EnableLockSArea( isLockSlave, isSingleSelect );
		}

		public void EnableLockMArea( bool enableMaster )
		{
			enableMaster = enableMaster && m_RotaryAxisConfig.Variant != MixTypeVariant.SlaveOnly;
			m_pnlMasterSettings.Enabled = enableMaster;
			m_lblMasterAngle.Enabled = enableMaster;
			m_numMaster.Enabled = enableMaster;
		}

		public void EnableLockSArea( bool enableSlave, bool isSingleSelect = false )
		{
			enableSlave = enableSlave && m_RotaryAxisConfig.Variant != MixTypeVariant.MasterOnly;
			m_pnlSlaveSettings.Enabled = enableSlave;
			m_lblSlaveAngle.Enabled = enableSlave;
			m_numSlave.Enabled = enableSlave;
			m_pnlRotaryDirection.Enabled = enableSlave;
			m_pnlMachiningDir.Enabled = enableSlave && !m_IsSpindle;

			if( isSingleSelect == false ) {
				m_pnlRotaryDirection.Enabled = false;
			}
		}

		public void ResetMachiningDir( EMachiningDir dir )
		{
			if( m_cmbMachiningDir.Items.Count == 0 ) {
				return;
			}
			int nIndex = m_MachiningDirList.IndexOf( dir );
			if( nIndex < 0 || nIndex >= m_cmbMachiningDir.Items.Count ) {
				return;
			}
			m_IsUpdatingMachiningDirUI = true;
			m_cmbMachiningDir.SelectedIndex = nIndex;
			m_IsUpdatingMachiningDirUI = false;
		}

		public void ResetMSAngle( double master_deg, double slave_deg, bool isMasterAssigned = false, bool isSlaveAssigned = false )
		{
			m_IsUpdatingMSAngleUI = true;
			try {
				m_numMaster.SilentSetValue = (decimal)master_deg;
				m_numSlave.SilentSetValue = (decimal)slave_deg;
				ShowMAngleAsAssignMode( isMasterAssigned );
				ShowSAngleAsAssignMode( isSlaveAssigned );
			}
			finally {
				m_IsUpdatingMSAngleUI = false;
			}
		}

		public void ShowMAngleAsAssignMode( bool isAssigned )
		{
			ThemeText.SetRole( isAssigned ? TextRole.Body : TextRole.Secondary, m_numMaster );
			m_IsUpdatingMSAngleUI = true;
			try {
				if( isAssigned ) {
					m_rdoMasterManual.Checked = true;
				}
				else {
					m_rdoMasterAuto.Checked = true;
				}
			}
			finally {
				m_IsUpdatingMSAngleUI = false;
			}
		}

		public void ShowSAngleAsAssignMode( bool isAssigned )
		{
			ThemeText.SetRole( isAssigned ? TextRole.Body : TextRole.Secondary, m_numSlave );
			m_IsUpdatingMSAngleUI = true;
			try {
				if( isAssigned ) {
					m_rdoSlaveManual.Checked = true;
				}
				else {
					m_rdoSlaveAuto.Checked = true;
				}
			}
			finally {
				m_IsUpdatingMSAngleUI = false;
			}
		}

		public void GetMSAngle( out double master_deg, out double slave_deg )
		{
			master_deg = (double)m_numMaster.Value;
			slave_deg = (double)m_numSlave.Value;
		}

		#endregion

		#region UI Event

		void m_rdoSlaveAuto_CheckedChanged( object sender, EventArgs e )
		{
			if( m_rdoSlaveAuto.Checked && !m_IsUpdatingMSAngleUI ) {
				NoticeSNeedBeSetAsAUTO();
			}
		}

		void m_rdoSlaveManual_CheckedChanged( object sender, EventArgs e )
		{
			if( m_rdoSlaveManual.Checked ) {
				m_numSlave_ValueChanged( sender, e );
			}
		}

		void m_numSlave_ValueChanged( object sender, EventArgs e )
		{
			if( m_IsUpdatingMSAngleUI ) {
				return;
			}
			if( m_chkLockSlave.Checked ) {
				SlaveChanged?.Invoke( (double)m_numSlave.Value );
				return;
			}
		}

		void m_rdoMasterAuto_CheckedChanged( object sender, EventArgs e )
		{
			if( !m_rdoMasterAuto.Checked ) {
				return;
			}
			NoticeMNeedBeSetAsAUTO();
		}

		void m_rdoMasterManual_CheckedChanged( object sender, EventArgs e )
		{
			if( m_rdoMasterManual.Checked ) {
				m_numMaster_ValueChanged( sender, e );
			}
		}

		void m_numMaster_ValueChanged( object sender, EventArgs e )
		{
			if( m_IsUpdatingMSAngleUI ) {
				return;
			}

			// set assignMS
			if( m_chkLockMaster.Checked ) {
				MasterChanged?.Invoke( (double)m_numMaster.Value );
				return;
			}
		}

		void m_chkLockSlave_CheckedChanged( object sender, EventArgs e )
		{
			NoticeSNeedBeSetAsAUTO();
		}

		void m_chkLockMaster_CheckedChanged( object sender, EventArgs e )
		{
			NoticeMNeedBeSetAsAUTO();
		}

		void m_rdoDefault_CheckedChanged( object sender, EventArgs e )
		{
			if( m_IsUpdatingModeUI ) {
				return;
			}
			UpdateModeUI( m_rdoDefault.Checked );
			if( m_rdoDefault.Checked ) {
				OnNormalMode?.Invoke( true );
				return;
			}
			OnNormalMode?.Invoke( false );
		}

		void m_btnRotaryPos_Click( object sender, EventArgs e )
		{
			FlipRotaryAxis?.Invoke( true );
		}

		void m_btnRotaryNeg_Click( object sender, EventArgs e )
		{
			FlipRotaryAxis?.Invoke( false );
		}

		void m_cmbMachiningDir_SelectedIndexChanged( object sender, EventArgs e )
		{
			if( m_IsUpdatingMachiningDirUI ) {
				return;
			}
			int nIndex = m_cmbMachiningDir.SelectedIndex;
			if( nIndex < 0 || nIndex >= m_MachiningDirList.Count ) {
				return;
			}
			MachiningDirChanged?.Invoke( m_MachiningDirList[ nIndex ] );
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			Close();
		}

		void m_btnCancel_Click( object sender, EventArgs e )
		{
			Close();
		}

		#endregion

		#region UI Setting

		// texts that contain the axis names of the machine
		void InitLocalizedTexts()
		{
			m_pnlRotaryDirection.Text = string.Format(
				m_LanguageManager.TryGetString( "Format_RotaryDirection", "{0}軸方向調整" ),
				m_RotaryAxisConfig.RotaryAxisName );
			m_chkLockSlave.Text = string.Format( m_LanguageManager.TryGetString( "Format_LockSlave", "鎖{0}軸" ),
				m_RotaryAxisConfig.SlaveName );
			m_lblSlaveAngle.Text = string.Format( m_LanguageManager.TryGetString( "Format_SlaveAngle", "{0}軸角度" ),
				m_RotaryAxisConfig.SlaveName );
			m_chkLockMaster.Text = string.Format( m_LanguageManager.TryGetString( "Format_LockMaster", "鎖{0}軸" ),
				m_RotaryAxisConfig.MasterName );
			m_lblMasterAngle.Text = string.Format(
				m_LanguageManager.TryGetString( "Format_MasterAngle", "{0}軸角度" ),
				m_RotaryAxisConfig.MasterName );
			m_btnRotaryPos.Text = m_RotaryAxisConfig.RotaryAxisName + " +";
			m_btnRotaryNeg.Text = m_RotaryAxisConfig.RotaryAxisName + " -";
		}

		void BuildMachiningDirItems()
		{
			m_IsUpdatingMachiningDirUI = true;
			m_cmbMachiningDir.Items.Clear();
			foreach( EMachiningDir dir in m_MachiningDirList ) {
				string szName = ( dir == EMachiningDir.None )
					? m_LanguageManager.TryGetString( "MachiningDir_None", "無" )
					: RotaryAxisHelper.ConvertMachiningDirName( dir, m_RotaryAxisConfig.RotaryAxisType );
				m_cmbMachiningDir.Items.Add( szName );
			}
			m_cmbMachiningDir.SelectedIndex = 0;
			m_IsUpdatingMachiningDirUI = false;
		}

		void UpdateModeUI( bool isNormal )
		{
			m_pnlLockAB.Enabled = !isNormal;
			m_chkLockMaster.Enabled = m_RotaryAxisConfig.Variant != MixTypeVariant.SlaveOnly;
			m_chkLockSlave.Enabled = m_RotaryAxisConfig.Variant != MixTypeVariant.MasterOnly;
		}

		#endregion

		void NoticeSNeedBeSetAsAUTO()
		{
			if( m_IsUpdatingModeUI || m_IsUpdatingMSAngleUI ) {
				return;
			}
			if( m_chkLockSlave.Checked ) {
				checkLockSlaveChanged?.Invoke( true );
				return;
			}
			checkLockSlaveChanged?.Invoke( false );
		}

		void NoticeMNeedBeSetAsAUTO()
		{
			if( m_IsUpdatingModeUI || m_IsUpdatingMSAngleUI ) {
				return;
			}
			if( m_chkLockMaster.Checked ) {
				checkLockMasterChanged?.Invoke( true );
				return;
			}
			checkLockMasterChanged?.Invoke( false );
		}

		readonly LanguageManager.LanguageManager m_LanguageManager =
			new LanguageManager.LanguageManager( "LockMSAxisDlg" );

		RotaryAxisConfig m_RotaryAxisConfig;
		readonly bool m_IsSpindle;

		bool m_IsUpdatingModeUI = false;
		bool m_IsUpdatingMachiningDirUI = false;

		// to stop MS Change evnet
		bool m_IsUpdatingMSAngleUI = false;


		// defines the combo box order
		readonly List<EMachiningDir> m_MachiningDirList = new List<EMachiningDir>
		{
			EMachiningDir.None,
			EMachiningDir.RefAxisPositive,
			EMachiningDir.CrossAxisPositive,
			EMachiningDir.RefAxisNegative,
			EMachiningDir.CrossAxisNegative
		};
	}
}
