namespace MyCAM.Editor
{
	partial class LockMSAxisDlg
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) ) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.m_pnlMode = new CommonUI.Controls.SectionPanel();
			this.m_lblMode = new CommonUI.Controls.ThemedLabel();
			this.m_rdoDefault = new System.Windows.Forms.RadioButton();
			this.m_rdoLockAB = new System.Windows.Forms.RadioButton();
			this.m_pnlLockAB = new CommonUI.Controls.SectionPanel();
			this.m_chkLockSlave = new System.Windows.Forms.CheckBox();
			this.m_pnlSlaveSettings = new System.Windows.Forms.Panel();
			this.m_pnlSlaveAngle = new System.Windows.Forms.Panel();
			this.m_lblSlaveAngle = new CommonUI.Controls.ThemedLabel();
			this.m_rdoSlaveAuto = new System.Windows.Forms.RadioButton();
			this.m_rdoSlaveManual = new System.Windows.Forms.RadioButton();
			this.m_numSlave = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_pnlRotaryDirection = new CommonUI.Controls.SectionPanel();
			this.m_btnRotaryPos = new CommonUI.Controls.ThemedButton();
			this.m_btnRotaryNeg = new CommonUI.Controls.ThemedButton();
			this.m_pnlMachiningDir = new CommonUI.Controls.SectionPanel();
			this.m_lblMachiningDir = new CommonUI.Controls.ThemedLabel();
			this.m_cmbMachiningDir = new System.Windows.Forms.ComboBox();
			this.m_lblMachiningDirHint = new CommonUI.Controls.ThemedLabel();
			this.m_chkLockMaster = new System.Windows.Forms.CheckBox();
			this.m_pnlMasterSettings = new System.Windows.Forms.Panel();
			this.m_pnlMasterAngle = new System.Windows.Forms.Panel();
			this.m_lblMasterAngle = new CommonUI.Controls.ThemedLabel();
			this.m_rdoMasterAuto = new System.Windows.Forms.RadioButton();
			this.m_rdoMasterManual = new System.Windows.Forms.RadioButton();
			this.m_numMaster = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_btnCancel = new CommonUI.Controls.ThemedButton();
			this.m_btnConfirm = new CommonUI.Controls.ThemedButton();
			this.m_pnlMode.SuspendLayout();
			this.m_pnlLockAB.SuspendLayout();
			this.m_pnlSlaveSettings.SuspendLayout();
			this.m_pnlSlaveAngle.SuspendLayout();
			this.m_pnlRotaryDirection.SuspendLayout();
			this.m_pnlMachiningDir.SuspendLayout();
			this.m_pnlMasterSettings.SuspendLayout();
			this.m_pnlMasterAngle.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numSlave)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMaster)).BeginInit();
			this.SuspendLayout();
			// 
			// m_pnlMode
			// 
			this.m_pnlMode.Controls.Add(this.m_lblMode);
			this.m_pnlMode.Controls.Add(this.m_rdoDefault);
			this.m_pnlMode.Controls.Add(this.m_rdoLockAB);
			this.m_pnlMode.Location = new System.Drawing.Point(12, 12);
			this.m_pnlMode.Name = "m_pnlMode";
			this.m_pnlMode.Size = new System.Drawing.Size(416, 43);
			this.m_pnlMode.TabIndex = 0;
			// 
			// m_lblMode
			// 
			this.m_lblMode.AutoSize = true;
			this.m_lblMode.Location = new System.Drawing.Point(10, 14);
			this.m_lblMode.Name = "m_lblMode";
			this.m_lblMode.Size = new System.Drawing.Size(31, 16);
			this.m_lblMode.TabIndex = 0;
			this.m_lblMode.Text = "模式";
			// 
			// m_rdoDefault
			// 
			this.m_rdoDefault.AutoSize = true;
			this.m_rdoDefault.Checked = true;
			this.m_rdoDefault.Location = new System.Drawing.Point(100, 12);
			this.m_rdoDefault.Name = "m_rdoDefault";
			this.m_rdoDefault.Size = new System.Drawing.Size(49, 20);
			this.m_rdoDefault.TabIndex = 1;
			this.m_rdoDefault.TabStop = true;
			this.m_rdoDefault.Text = "默認";
			this.m_rdoDefault.CheckedChanged += new System.EventHandler(this.m_rdoDefault_CheckedChanged);
			// 
			// m_rdoLockAB
			// 
			this.m_rdoLockAB.AutoSize = true;
			this.m_rdoLockAB.Location = new System.Drawing.Point(190, 12);
			this.m_rdoLockAB.Name = "m_rdoLockAB";
			this.m_rdoLockAB.Size = new System.Drawing.Size(73, 20);
			this.m_rdoLockAB.TabIndex = 2;
			this.m_rdoLockAB.Text = "鎖軸切割";
			// 
			// m_pnlLockAB
			// 
			this.m_pnlLockAB.Controls.Add(this.m_chkLockSlave);
			this.m_pnlLockAB.Controls.Add(this.m_pnlSlaveSettings);
			this.m_pnlLockAB.Controls.Add(this.m_chkLockMaster);
			this.m_pnlLockAB.Controls.Add(this.m_pnlMasterSettings);
			this.m_pnlLockAB.Enabled = false;
			this.m_pnlLockAB.Location = new System.Drawing.Point(12, 63);
			this.m_pnlLockAB.Name = "m_pnlLockAB";
			this.m_pnlLockAB.Size = new System.Drawing.Size(416, 339);
			this.m_pnlLockAB.TabIndex = 1;
			this.m_pnlLockAB.Text = "鎖軸切割";
			// 
			// m_chkLockSlave
			// 
			this.m_chkLockSlave.AutoSize = true;
			this.m_chkLockSlave.Enabled = false;
			this.m_chkLockSlave.Location = new System.Drawing.Point(10, 38);
			this.m_chkLockSlave.Name = "m_chkLockSlave";
			this.m_chkLockSlave.Size = new System.Drawing.Size(61, 20);
			this.m_chkLockSlave.TabIndex = 0;
			this.m_chkLockSlave.Text = "鎖轉檯";
			this.m_chkLockSlave.CheckedChanged += new System.EventHandler(this.m_chkLockSlave_CheckedChanged);
			// 
			// m_pnlSlaveSettings
			// 
			this.m_pnlSlaveSettings.Controls.Add(this.m_pnlSlaveAngle);
			this.m_pnlSlaveSettings.Controls.Add(this.m_pnlRotaryDirection);
			this.m_pnlSlaveSettings.Controls.Add(this.m_pnlMachiningDir);
			this.m_pnlSlaveSettings.Location = new System.Drawing.Point(10, 66);
			this.m_pnlSlaveSettings.Name = "m_pnlSlaveSettings";
			this.m_pnlSlaveSettings.Size = new System.Drawing.Size(396, 159);
			this.m_pnlSlaveSettings.TabIndex = 1;
			// 
			// m_pnlSlaveAngle
			// 
			this.m_pnlSlaveAngle.Controls.Add(this.m_lblSlaveAngle);
			this.m_pnlSlaveAngle.Controls.Add(this.m_rdoSlaveAuto);
			this.m_pnlSlaveAngle.Controls.Add(this.m_rdoSlaveManual);
			this.m_pnlSlaveAngle.Controls.Add(this.m_numSlave);
			this.m_pnlSlaveAngle.Location = new System.Drawing.Point(0, 0);
			this.m_pnlSlaveAngle.Name = "m_pnlSlaveAngle";
			this.m_pnlSlaveAngle.Size = new System.Drawing.Size(396, 53);
			this.m_pnlSlaveAngle.TabIndex = 0;
			// 
			// m_lblSlaveAngle
			// 
			this.m_lblSlaveAngle.AutoSize = true;
			this.m_lblSlaveAngle.Enabled = false;
			this.m_lblSlaveAngle.Location = new System.Drawing.Point(0, 4);
			this.m_lblSlaveAngle.Name = "m_lblSlaveAngle";
			this.m_lblSlaveAngle.Size = new System.Drawing.Size(55, 16);
			this.m_lblSlaveAngle.TabIndex = 0;
			this.m_lblSlaveAngle.Text = "轉檯角度";
			// 
			// m_rdoSlaveAuto
			// 
			this.m_rdoSlaveAuto.AutoSize = true;
			this.m_rdoSlaveAuto.Location = new System.Drawing.Point(90, 2);
			this.m_rdoSlaveAuto.Name = "m_rdoSlaveAuto";
			this.m_rdoSlaveAuto.Size = new System.Drawing.Size(73, 20);
			this.m_rdoSlaveAuto.TabIndex = 1;
			this.m_rdoSlaveAuto.Text = "自動計算";
			this.m_rdoSlaveAuto.CheckedChanged += new System.EventHandler(this.m_rdoSlaveAuto_CheckedChanged);
			// 
			// m_rdoSlaveManual
			// 
			this.m_rdoSlaveManual.AutoSize = true;
			this.m_rdoSlaveManual.Location = new System.Drawing.Point(190, 2);
			this.m_rdoSlaveManual.Name = "m_rdoSlaveManual";
			this.m_rdoSlaveManual.Size = new System.Drawing.Size(73, 20);
			this.m_rdoSlaveManual.TabIndex = 2;
			this.m_rdoSlaveManual.Text = "手動設定";
			this.m_rdoSlaveManual.CheckedChanged += new System.EventHandler(this.m_rdoSlaveManual_CheckedChanged);
			// 
			// m_numSlave
			// 
			this.m_numSlave.DecimalPlaces = 3;
			this.m_numSlave.Enabled = false;
			this.m_numSlave.Location = new System.Drawing.Point(90, 30);
			this.m_numSlave.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numSlave.Minimum = new decimal(new int[] { -99999, 0, 0, -2147483648});
			this.m_numSlave.Name = "m_numSlave";
			this.m_numSlave.RestrictInvalidCharacters = false;
			this.m_numSlave.Size = new System.Drawing.Size(150, 23);
			this.m_numSlave.TabIndex = 3;
			this.m_numSlave.ValueChanged += new System.EventHandler(this.m_numSlave_ValueChanged);
			// 
			// m_pnlRotaryDirection
			// 
			this.m_pnlRotaryDirection.Controls.Add(this.m_btnRotaryPos);
			this.m_pnlRotaryDirection.Controls.Add(this.m_btnRotaryNeg);
			this.m_pnlRotaryDirection.Enabled = false;
			this.m_pnlRotaryDirection.Location = new System.Drawing.Point(0, 60);
			this.m_pnlRotaryDirection.Name = "m_pnlRotaryDirection";
			this.m_pnlRotaryDirection.Size = new System.Drawing.Size(193, 69);
			this.m_pnlRotaryDirection.TabIndex = 1;
			this.m_pnlRotaryDirection.Text = "轉台方向調整";
			// 
			// m_btnRotaryPos
			// 
			this.m_btnRotaryPos.Location = new System.Drawing.Point(10, 36);
			this.m_btnRotaryPos.Name = "m_btnRotaryPos";
			this.m_btnRotaryPos.Size = new System.Drawing.Size(80, 23);
			this.m_btnRotaryPos.TabIndex = 0;
			this.m_btnRotaryPos.Text = "+";
			this.m_btnRotaryPos.Click += new System.EventHandler(this.m_btnRotaryPos_Click);
			// 
			// m_btnRotaryNeg
			// 
			this.m_btnRotaryNeg.Location = new System.Drawing.Point(100, 36);
			this.m_btnRotaryNeg.Name = "m_btnRotaryNeg";
			this.m_btnRotaryNeg.Size = new System.Drawing.Size(80, 23);
			this.m_btnRotaryNeg.TabIndex = 1;
			this.m_btnRotaryNeg.Text = "-";
			this.m_btnRotaryNeg.Click += new System.EventHandler(this.m_btnRotaryNeg_Click);
			// 
			// m_pnlMachiningDir
			// 
			this.m_pnlMachiningDir.Controls.Add(this.m_lblMachiningDir);
			this.m_pnlMachiningDir.Controls.Add(this.m_cmbMachiningDir);
			this.m_pnlMachiningDir.Controls.Add(this.m_lblMachiningDirHint);
			this.m_pnlMachiningDir.Enabled = false;
			this.m_pnlMachiningDir.Location = new System.Drawing.Point(203, 60);
			this.m_pnlMachiningDir.Name = "m_pnlMachiningDir";
			this.m_pnlMachiningDir.Size = new System.Drawing.Size(193, 99);
			this.m_pnlMachiningDir.TabIndex = 2;
			this.m_pnlMachiningDir.Text = "指定加工方位";
			// 
			// m_lblMachiningDir
			// 
			this.m_lblMachiningDir.AutoSize = true;
			this.m_lblMachiningDir.Location = new System.Drawing.Point(10, 40);
			this.m_lblMachiningDir.Name = "m_lblMachiningDir";
			this.m_lblMachiningDir.Size = new System.Drawing.Size(31, 16);
			this.m_lblMachiningDir.TabIndex = 0;
			this.m_lblMachiningDir.Text = "方向";
			// 
			// m_cmbMachiningDir
			// 
			this.m_cmbMachiningDir.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbMachiningDir.FormattingEnabled = true;
			this.m_cmbMachiningDir.Location = new System.Drawing.Point(60, 36);
			this.m_cmbMachiningDir.Name = "m_cmbMachiningDir";
			this.m_cmbMachiningDir.Size = new System.Drawing.Size(123, 24);
			this.m_cmbMachiningDir.TabIndex = 1;
			this.m_cmbMachiningDir.SelectedIndexChanged += new System.EventHandler(this.m_cmbMachiningDir_SelectedIndexChanged);
			// 
			// m_lblMachiningDirHint
			// 
			this.m_lblMachiningDirHint.Location = new System.Drawing.Point(10, 70);
			this.m_lblMachiningDirHint.Name = "m_lblMachiningDirHint";
			this.m_lblMachiningDirHint.Role = CommonUI.Theme.TextRole.Error;
			this.m_lblMachiningDirHint.Size = new System.Drawing.Size(173, 19);
			this.m_lblMachiningDirHint.TabIndex = 2;
			this.m_lblMachiningDirHint.Text = "Spindle Type 不支援此功能";
			this.m_lblMachiningDirHint.Visible = false;
			// 
			// m_chkLockMaster
			// 
			this.m_chkLockMaster.AutoSize = true;
			this.m_chkLockMaster.Enabled = false;
			this.m_chkLockMaster.Location = new System.Drawing.Point(10, 248);
			this.m_chkLockMaster.Name = "m_chkLockMaster";
			this.m_chkLockMaster.Size = new System.Drawing.Size(61, 20);
			this.m_chkLockMaster.TabIndex = 2;
			this.m_chkLockMaster.Text = "鎖擺頭";
			this.m_chkLockMaster.CheckedChanged += new System.EventHandler(this.m_chkLockMaster_CheckedChanged);
			// 
			// m_pnlMasterSettings
			// 
			this.m_pnlMasterSettings.Controls.Add(this.m_pnlMasterAngle);
			this.m_pnlMasterSettings.Location = new System.Drawing.Point(10, 276);
			this.m_pnlMasterSettings.Name = "m_pnlMasterSettings";
			this.m_pnlMasterSettings.Size = new System.Drawing.Size(396, 53);
			this.m_pnlMasterSettings.TabIndex = 3;
			// 
			// m_pnlMasterAngle
			// 
			this.m_pnlMasterAngle.Controls.Add(this.m_lblMasterAngle);
			this.m_pnlMasterAngle.Controls.Add(this.m_rdoMasterAuto);
			this.m_pnlMasterAngle.Controls.Add(this.m_rdoMasterManual);
			this.m_pnlMasterAngle.Controls.Add(this.m_numMaster);
			this.m_pnlMasterAngle.Location = new System.Drawing.Point(0, 0);
			this.m_pnlMasterAngle.Name = "m_pnlMasterAngle";
			this.m_pnlMasterAngle.Size = new System.Drawing.Size(396, 53);
			this.m_pnlMasterAngle.TabIndex = 0;
			// 
			// m_lblMasterAngle
			// 
			this.m_lblMasterAngle.AutoSize = true;
			this.m_lblMasterAngle.Enabled = false;
			this.m_lblMasterAngle.Location = new System.Drawing.Point(0, 4);
			this.m_lblMasterAngle.Name = "m_lblMasterAngle";
			this.m_lblMasterAngle.Size = new System.Drawing.Size(55, 16);
			this.m_lblMasterAngle.TabIndex = 0;
			this.m_lblMasterAngle.Text = "擺頭角度";
			// 
			// m_rdoMasterAuto
			// 
			this.m_rdoMasterAuto.AutoSize = true;
			this.m_rdoMasterAuto.Location = new System.Drawing.Point(90, 2);
			this.m_rdoMasterAuto.Name = "m_rdoMasterAuto";
			this.m_rdoMasterAuto.Size = new System.Drawing.Size(73, 20);
			this.m_rdoMasterAuto.TabIndex = 1;
			this.m_rdoMasterAuto.Text = "自動計算";
			this.m_rdoMasterAuto.CheckedChanged += new System.EventHandler(this.m_rdoMasterAuto_CheckedChanged);
			// 
			// m_rdoMasterManual
			// 
			this.m_rdoMasterManual.AutoSize = true;
			this.m_rdoMasterManual.Location = new System.Drawing.Point(190, 2);
			this.m_rdoMasterManual.Name = "m_rdoMasterManual";
			this.m_rdoMasterManual.Size = new System.Drawing.Size(73, 20);
			this.m_rdoMasterManual.TabIndex = 2;
			this.m_rdoMasterManual.Text = "手動設定";
			this.m_rdoMasterManual.CheckedChanged += new System.EventHandler(this.m_rdoMasterManual_CheckedChanged);
			// 
			// m_numMaster
			// 
			this.m_numMaster.DecimalPlaces = 3;
			this.m_numMaster.Enabled = false;
			this.m_numMaster.Location = new System.Drawing.Point(90, 30);
			this.m_numMaster.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numMaster.Minimum = new decimal(new int[] { -99999, 0, 0, -2147483648});
			this.m_numMaster.Name = "m_numMaster";
			this.m_numMaster.RestrictInvalidCharacters = false;
			this.m_numMaster.Size = new System.Drawing.Size(150, 23);
			this.m_numMaster.TabIndex = 3;
			this.m_numMaster.ValueChanged += new System.EventHandler(this.m_numMaster_ValueChanged);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.CommonText = CommonUI.Controls.CommonButtonText.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(260, 418);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(80, 28);
			this.m_btnCancel.TabIndex = 2;
			this.m_btnCancel.Text = "取消";
			this.m_btnCancel.Click += new System.EventHandler(this.m_btnCancel_Click);
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnConfirm.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnConfirm.Location = new System.Drawing.Point(348, 418);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(80, 28);
			this.m_btnConfirm.TabIndex = 3;
			this.m_btnConfirm.Text = "確定";
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// LockMSAxisDlg
			// 
			this.ClientSize = new System.Drawing.Size(440, 458);
			this.Controls.Add(this.m_pnlMode);
			this.Controls.Add(this.m_pnlLockAB);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnConfirm);
			this.Name = "LockMSAxisDlg";
			this.Text = "鎖軸設定";
			this.m_pnlMode.ResumeLayout(false);
			this.m_pnlMode.PerformLayout();
			this.m_pnlLockAB.ResumeLayout(false);
			this.m_pnlLockAB.PerformLayout();
			this.m_pnlSlaveSettings.ResumeLayout(false);
			this.m_pnlSlaveAngle.ResumeLayout(false);
			this.m_pnlSlaveAngle.PerformLayout();
			this.m_pnlRotaryDirection.ResumeLayout(false);
			this.m_pnlMachiningDir.ResumeLayout(false);
			this.m_pnlMachiningDir.PerformLayout();
			this.m_pnlMasterSettings.ResumeLayout(false);
			this.m_pnlMasterAngle.ResumeLayout(false);
			this.m_pnlMasterAngle.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numSlave)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMaster)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlMode;
		private CommonUI.Controls.ThemedLabel m_lblMode;
		private System.Windows.Forms.RadioButton m_rdoDefault;
		private System.Windows.Forms.RadioButton m_rdoLockAB;
		private CommonUI.Controls.SectionPanel m_pnlLockAB;
		private System.Windows.Forms.CheckBox m_chkLockSlave;
		private System.Windows.Forms.Panel m_pnlSlaveSettings;
		private System.Windows.Forms.Panel m_pnlSlaveAngle;
		private CommonUI.Controls.ThemedLabel m_lblSlaveAngle;
		private System.Windows.Forms.RadioButton m_rdoSlaveAuto;
		private System.Windows.Forms.RadioButton m_rdoSlaveManual;
		private CommonUI.Controls.DebouncedNumericUpDown m_numSlave;
		private CommonUI.Controls.SectionPanel m_pnlRotaryDirection;
		private CommonUI.Controls.ThemedButton m_btnRotaryPos;
		private CommonUI.Controls.ThemedButton m_btnRotaryNeg;
		private CommonUI.Controls.SectionPanel m_pnlMachiningDir;
		private CommonUI.Controls.ThemedLabel m_lblMachiningDir;
		private System.Windows.Forms.ComboBox m_cmbMachiningDir;
		private CommonUI.Controls.ThemedLabel m_lblMachiningDirHint;
		private System.Windows.Forms.CheckBox m_chkLockMaster;
		private System.Windows.Forms.Panel m_pnlMasterSettings;
		private System.Windows.Forms.Panel m_pnlMasterAngle;
		private CommonUI.Controls.ThemedLabel m_lblMasterAngle;
		private System.Windows.Forms.RadioButton m_rdoMasterAuto;
		private System.Windows.Forms.RadioButton m_rdoMasterManual;
		private CommonUI.Controls.DebouncedNumericUpDown m_numMaster;
		private CommonUI.Controls.ThemedButton m_btnCancel;
		private CommonUI.Controls.ThemedButton m_btnConfirm;
	}
}
