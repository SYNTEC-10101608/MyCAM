namespace MyCAM.Editor
{
	partial class LockMSAxisDlg
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) ) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnConfirm = new System.Windows.Forms.Button();
			this.m_gbxLockAB = new System.Windows.Forms.GroupBox();
			this.m_chkLockSlave = new System.Windows.Forms.CheckBox();
			this.m_gbxSlaveSettings = new System.Windows.Forms.GroupBox();
			this.m_tlpSlaveAngle = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblSlaveAngle = new System.Windows.Forms.Label();
			this.m_rbtnSlaveAuto = new System.Windows.Forms.RadioButton();
			this.m_rbtnSlaveManual = new System.Windows.Forms.RadioButton();
			this.m_dbtnSlave = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_gbxRotaryDirection = new System.Windows.Forms.GroupBox();
			this.m_btnRotaryPos = new System.Windows.Forms.Button();
			this.m_btnRotaryNeg = new System.Windows.Forms.Button();
			this.m_gbxMachiningDir = new System.Windows.Forms.GroupBox();
			this.m_lblMachiningDir = new System.Windows.Forms.Label();
			this.m_cbxMachiningDir = new System.Windows.Forms.ComboBox();
			this.m_lblMachiningDirHint = new System.Windows.Forms.Label();
			this.m_chkLockMaster = new System.Windows.Forms.CheckBox();
			this.m_gbxMasterSettings = new System.Windows.Forms.GroupBox();
			this.m_tlpMasterAngle = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblMasterAngle = new System.Windows.Forms.Label();
			this.m_rbtnMasterAuto = new System.Windows.Forms.RadioButton();
			this.m_rbtnMasterManual = new System.Windows.Forms.RadioButton();
			this.m_dbtnMaster = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_gbxMode = new System.Windows.Forms.GroupBox();
			this.m_rbtnLockAB = new System.Windows.Forms.RadioButton();
			this.m_rbtnDefault = new System.Windows.Forms.RadioButton();
			this.m_lblMode = new System.Windows.Forms.Label();
			this.m_gbxLockAB.SuspendLayout();
			this.m_gbxSlaveSettings.SuspendLayout();
			this.m_tlpSlaveAngle.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dbtnSlave)).BeginInit();
			this.m_gbxRotaryDirection.SuspendLayout();
			this.m_gbxMachiningDir.SuspendLayout();
			this.m_gbxMasterSettings.SuspendLayout();
			this.m_tlpMasterAngle.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dbtnMaster)).BeginInit();
			this.m_gbxMode.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Location = new System.Drawing.Point(304, 430);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 5;
			this.m_btnCancel.Text = "取消";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.m_btnCancel_Click);
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Location = new System.Drawing.Point(223, 430);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(75, 23);
			this.m_btnConfirm.TabIndex = 4;
			this.m_btnConfirm.Text = "確認";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// m_gbxLockAB
			// 
			this.m_gbxLockAB.Controls.Add(this.m_chkLockSlave);
			this.m_gbxLockAB.Controls.Add(this.m_gbxSlaveSettings);
			this.m_gbxLockAB.Controls.Add(this.m_chkLockMaster);
			this.m_gbxLockAB.Controls.Add(this.m_gbxMasterSettings);
			this.m_gbxLockAB.Enabled = false;
			this.m_gbxLockAB.Location = new System.Drawing.Point(12, 57);
			this.m_gbxLockAB.Name = "m_gbxLockAB";
			this.m_gbxLockAB.Size = new System.Drawing.Size(363, 367);
			this.m_gbxLockAB.TabIndex = 3;
			this.m_gbxLockAB.TabStop = false;
			this.m_gbxLockAB.Text = "鎖軸切割";
			// 
			// m_chkLockSlave
			// 
			this.m_chkLockSlave.AutoSize = true;
			this.m_chkLockSlave.Enabled = false;
			this.m_chkLockSlave.Location = new System.Drawing.Point(14, 21);
			this.m_chkLockSlave.Name = "m_chkLockSlave";
			this.m_chkLockSlave.Size = new System.Drawing.Size(60, 16);
			this.m_chkLockSlave.TabIndex = 4;
			this.m_chkLockSlave.Text = "鎖轉檯";
			this.m_chkLockSlave.UseVisualStyleBackColor = true;
			this.m_chkLockSlave.CheckedChanged += new System.EventHandler(this.m_chkLockSlave_CheckedChanged);
			// 
			// m_gbxSlaveSettings
			// 
			this.m_gbxSlaveSettings.Controls.Add(this.m_tlpSlaveAngle);
			this.m_gbxSlaveSettings.Controls.Add(this.m_gbxRotaryDirection);
			this.m_gbxSlaveSettings.Controls.Add(this.m_gbxMachiningDir);
			this.m_gbxSlaveSettings.Location = new System.Drawing.Point(20, 37);
			this.m_gbxSlaveSettings.Name = "m_gbxSlaveSettings";
			this.m_gbxSlaveSettings.Size = new System.Drawing.Size(320, 216);
			this.m_gbxSlaveSettings.TabIndex = 5;
			this.m_gbxSlaveSettings.TabStop = false;
			// 
			// m_tlpSlaveAngle
			// 
			this.m_tlpSlaveAngle.ColumnCount = 3;
			this.m_tlpSlaveAngle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tlpSlaveAngle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tlpSlaveAngle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tlpSlaveAngle.Controls.Add(this.m_lblSlaveAngle, 0, 0);
			this.m_tlpSlaveAngle.Controls.Add(this.m_rbtnSlaveAuto, 1, 0);
			this.m_tlpSlaveAngle.Controls.Add(this.m_rbtnSlaveManual, 2, 0);
			this.m_tlpSlaveAngle.Controls.Add(this.m_dbtnSlave, 1, 1);
			this.m_tlpSlaveAngle.Location = new System.Drawing.Point(3, 10);
			this.m_tlpSlaveAngle.Name = "m_tlpSlaveAngle";
			this.m_tlpSlaveAngle.RowCount = 2;
			this.m_tlpSlaveAngle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_tlpSlaveAngle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_tlpSlaveAngle.Size = new System.Drawing.Size(314, 59);
			this.m_tlpSlaveAngle.TabIndex = 0;
			// 
			// m_lblSlaveAngle
			// 
			this.m_lblSlaveAngle.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblSlaveAngle.AutoSize = true;
			this.m_lblSlaveAngle.Enabled = false;
			this.m_lblSlaveAngle.Location = new System.Drawing.Point(3, 8);
			this.m_lblSlaveAngle.Name = "m_lblSlaveAngle";
			this.m_lblSlaveAngle.Size = new System.Drawing.Size(53, 12);
			this.m_lblSlaveAngle.TabIndex = 0;
			this.m_lblSlaveAngle.Text = "轉檯角度";
			// 
			// m_rbtnSlaveAuto
			// 
			this.m_rbtnSlaveAuto.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_rbtnSlaveAuto.AutoSize = true;
			this.m_rbtnSlaveAuto.Location = new System.Drawing.Point(62, 6);
			this.m_rbtnSlaveAuto.Name = "m_rbtnSlaveAuto";
			this.m_rbtnSlaveAuto.Size = new System.Drawing.Size(71, 16);
			this.m_rbtnSlaveAuto.TabIndex = 1;
			this.m_rbtnSlaveAuto.Text = "自動計算";
			this.m_rbtnSlaveAuto.UseVisualStyleBackColor = true;
			this.m_rbtnSlaveAuto.CheckedChanged += new System.EventHandler(this.m_rbtnSlaveAuto_CheckedChanged);
			// 
			// m_rbtnSlaveManual
			// 
			this.m_rbtnSlaveManual.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_rbtnSlaveManual.AutoSize = true;
			this.m_rbtnSlaveManual.Location = new System.Drawing.Point(139, 6);
			this.m_rbtnSlaveManual.Name = "m_rbtnSlaveManual";
			this.m_rbtnSlaveManual.Size = new System.Drawing.Size(71, 16);
			this.m_rbtnSlaveManual.TabIndex = 2;
			this.m_rbtnSlaveManual.Text = "手動設定";
			this.m_rbtnSlaveManual.UseVisualStyleBackColor = true;
			this.m_rbtnSlaveManual.CheckedChanged += new System.EventHandler(this.m_rbtnSlaveManual_CheckedChanged);
			// 
			// m_dbtnSlave
			// 
			this.m_dbtnSlave.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_tlpSlaveAngle.SetColumnSpan(this.m_dbtnSlave, 2);
			this.m_dbtnSlave.DecimalPlaces = 3;
			this.m_dbtnSlave.Enabled = false;
			this.m_dbtnSlave.Location = new System.Drawing.Point(62, 33);
			this.m_dbtnSlave.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_dbtnSlave.Minimum = new decimal(new int[] {
            -99999,
            0,
            0,
            -2147483648});
			this.m_dbtnSlave.Name = "m_dbtnSlave";
			this.m_dbtnSlave.RestrictInvalidCharacters = false;
			this.m_dbtnSlave.Size = new System.Drawing.Size(88, 22);
			this.m_dbtnSlave.TabIndex = 3;
			this.m_dbtnSlave.ValueChanged += new System.EventHandler(this.m_dbtnSlave_ValueChanged);
			// 
			// m_gbxRotaryDirection
			// 
			this.m_gbxRotaryDirection.Controls.Add(this.m_btnRotaryPos);
			this.m_gbxRotaryDirection.Controls.Add(this.m_btnRotaryNeg);
			this.m_gbxRotaryDirection.Enabled = false;
			this.m_gbxRotaryDirection.Location = new System.Drawing.Point(14, 75);
			this.m_gbxRotaryDirection.Name = "m_gbxRotaryDirection";
			this.m_gbxRotaryDirection.Size = new System.Drawing.Size(208, 55);
			this.m_gbxRotaryDirection.TabIndex = 7;
			this.m_gbxRotaryDirection.TabStop = false;
			this.m_gbxRotaryDirection.Text = "轉台方向調整";
			// 
			// m_btnRotaryPos
			// 
			this.m_btnRotaryPos.Location = new System.Drawing.Point(17, 21);
			this.m_btnRotaryPos.Name = "m_btnRotaryPos";
			this.m_btnRotaryPos.Size = new System.Drawing.Size(75, 23);
			this.m_btnRotaryPos.TabIndex = 0;
			this.m_btnRotaryPos.Text = "+";
			this.m_btnRotaryPos.UseVisualStyleBackColor = true;
			this.m_btnRotaryPos.Click += new System.EventHandler(this.m_btnRotaryPos_Click);
			// 
			// m_btnRotaryNeg
			// 
			this.m_btnRotaryNeg.Location = new System.Drawing.Point(112, 21);
			this.m_btnRotaryNeg.Name = "m_btnRotaryNeg";
			this.m_btnRotaryNeg.Size = new System.Drawing.Size(75, 23);
			this.m_btnRotaryNeg.TabIndex = 1;
			this.m_btnRotaryNeg.Text = "-";
			this.m_btnRotaryNeg.UseVisualStyleBackColor = true;
			this.m_btnRotaryNeg.Click += new System.EventHandler(this.m_btnRotaryNeg_Click);
			// 
			// m_gbxMachiningDir
			// 
			this.m_gbxMachiningDir.Controls.Add(this.m_lblMachiningDir);
			this.m_gbxMachiningDir.Controls.Add(this.m_cbxMachiningDir);
			this.m_gbxMachiningDir.Controls.Add(this.m_lblMachiningDirHint);
			this.m_gbxMachiningDir.Enabled = false;
			this.m_gbxMachiningDir.Location = new System.Drawing.Point(14, 136);
			this.m_gbxMachiningDir.Name = "m_gbxMachiningDir";
			this.m_gbxMachiningDir.Size = new System.Drawing.Size(208, 74);
			this.m_gbxMachiningDir.TabIndex = 8;
			this.m_gbxMachiningDir.TabStop = false;
			this.m_gbxMachiningDir.Text = "指定加工方向";
			// 
			// m_lblMachiningDir
			// 
			this.m_lblMachiningDir.AutoSize = true;
			this.m_lblMachiningDir.Location = new System.Drawing.Point(32, 26);
			this.m_lblMachiningDir.Name = "m_lblMachiningDir";
			this.m_lblMachiningDir.Size = new System.Drawing.Size(29, 12);
			this.m_lblMachiningDir.TabIndex = 0;
			this.m_lblMachiningDir.Text = "方向";
			// 
			// m_cbxMachiningDir
			// 
			this.m_cbxMachiningDir.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cbxMachiningDir.FormattingEnabled = true;
			this.m_cbxMachiningDir.Location = new System.Drawing.Point(112, 18);
			this.m_cbxMachiningDir.Name = "m_cbxMachiningDir";
			this.m_cbxMachiningDir.Size = new System.Drawing.Size(75, 20);
			this.m_cbxMachiningDir.TabIndex = 1;
			this.m_cbxMachiningDir.SelectedIndexChanged += new System.EventHandler(this.m_cbxMachiningDir_SelectedIndexChanged);
			// 
			// m_lblMachiningDirHint
			// 
			this.m_lblMachiningDirHint.ForeColor = System.Drawing.Color.Red;
			this.m_lblMachiningDirHint.Location = new System.Drawing.Point(34, 50);
			this.m_lblMachiningDirHint.Name = "m_lblMachiningDirHint";
			this.m_lblMachiningDirHint.Size = new System.Drawing.Size(165, 24);
			this.m_lblMachiningDirHint.TabIndex = 2;
			this.m_lblMachiningDirHint.Text = "Spindle Type 不支援此功能";
			this.m_lblMachiningDirHint.Visible = false;
			// 
			// m_chkLockMaster
			// 
			this.m_chkLockMaster.AutoSize = true;
			this.m_chkLockMaster.Enabled = false;
			this.m_chkLockMaster.Location = new System.Drawing.Point(14, 265);
			this.m_chkLockMaster.Name = "m_chkLockMaster";
			this.m_chkLockMaster.Size = new System.Drawing.Size(60, 16);
			this.m_chkLockMaster.TabIndex = 9;
			this.m_chkLockMaster.Text = "鎖擺頭";
			this.m_chkLockMaster.UseVisualStyleBackColor = true;
			this.m_chkLockMaster.CheckedChanged += new System.EventHandler(this.m_chkLockMaster_CheckedChanged);
			// 
			// m_gbxMasterSettings
			// 
			this.m_gbxMasterSettings.Controls.Add(this.m_tlpMasterAngle);
			this.m_gbxMasterSettings.Location = new System.Drawing.Point(20, 281);
			this.m_gbxMasterSettings.Name = "m_gbxMasterSettings";
			this.m_gbxMasterSettings.Size = new System.Drawing.Size(320, 80);
			this.m_gbxMasterSettings.TabIndex = 10;
			this.m_gbxMasterSettings.TabStop = false;
			// 
			// m_tlpMasterAngle
			// 
			this.m_tlpMasterAngle.ColumnCount = 3;
			this.m_tlpMasterAngle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tlpMasterAngle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tlpMasterAngle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tlpMasterAngle.Controls.Add(this.m_lblMasterAngle, 0, 0);
			this.m_tlpMasterAngle.Controls.Add(this.m_rbtnMasterAuto, 1, 0);
			this.m_tlpMasterAngle.Controls.Add(this.m_rbtnMasterManual, 2, 0);
			this.m_tlpMasterAngle.Controls.Add(this.m_dbtnMaster, 1, 1);
			this.m_tlpMasterAngle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tlpMasterAngle.Location = new System.Drawing.Point(3, 18);
			this.m_tlpMasterAngle.Name = "m_tlpMasterAngle";
			this.m_tlpMasterAngle.RowCount = 2;
			this.m_tlpMasterAngle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_tlpMasterAngle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_tlpMasterAngle.Size = new System.Drawing.Size(314, 59);
			this.m_tlpMasterAngle.TabIndex = 0;
			// 
			// m_lblMasterAngle
			// 
			this.m_lblMasterAngle.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblMasterAngle.AutoSize = true;
			this.m_lblMasterAngle.Enabled = false;
			this.m_lblMasterAngle.Location = new System.Drawing.Point(3, 8);
			this.m_lblMasterAngle.Name = "m_lblMasterAngle";
			this.m_lblMasterAngle.Size = new System.Drawing.Size(53, 12);
			this.m_lblMasterAngle.TabIndex = 0;
			this.m_lblMasterAngle.Text = "擺頭角度";
			// 
			// m_rbtnMasterAuto
			// 
			this.m_rbtnMasterAuto.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_rbtnMasterAuto.AutoSize = true;
			this.m_rbtnMasterAuto.Location = new System.Drawing.Point(62, 6);
			this.m_rbtnMasterAuto.Name = "m_rbtnMasterAuto";
			this.m_rbtnMasterAuto.Size = new System.Drawing.Size(71, 16);
			this.m_rbtnMasterAuto.TabIndex = 1;
			this.m_rbtnMasterAuto.Text = "自動計算";
			this.m_rbtnMasterAuto.UseVisualStyleBackColor = true;
			this.m_rbtnMasterAuto.CheckedChanged += new System.EventHandler(this.m_rbtnMasterAuto_CheckedChanged);
			// 
			// m_rbtnMasterManual
			// 
			this.m_rbtnMasterManual.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_rbtnMasterManual.AutoSize = true;
			this.m_rbtnMasterManual.Location = new System.Drawing.Point(139, 6);
			this.m_rbtnMasterManual.Name = "m_rbtnMasterManual";
			this.m_rbtnMasterManual.Size = new System.Drawing.Size(71, 16);
			this.m_rbtnMasterManual.TabIndex = 2;
			this.m_rbtnMasterManual.Text = "手動設定";
			this.m_rbtnMasterManual.UseVisualStyleBackColor = true;
			this.m_rbtnMasterManual.CheckedChanged += new System.EventHandler(this.m_rbtnMasterManual_CheckedChanged);
			// 
			// m_dbtnMaster
			// 
			this.m_dbtnMaster.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_tlpMasterAngle.SetColumnSpan(this.m_dbtnMaster, 2);
			this.m_dbtnMaster.DecimalPlaces = 3;
			this.m_dbtnMaster.Enabled = false;
			this.m_dbtnMaster.Location = new System.Drawing.Point(62, 33);
			this.m_dbtnMaster.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_dbtnMaster.Minimum = new decimal(new int[] {
            -99999,
            0,
            0,
            -2147483648});
			this.m_dbtnMaster.Name = "m_dbtnMaster";
			this.m_dbtnMaster.RestrictInvalidCharacters = false;
			this.m_dbtnMaster.Size = new System.Drawing.Size(88, 22);
			this.m_dbtnMaster.TabIndex = 3;
			this.m_dbtnMaster.ValueChanged += new System.EventHandler(this.m_dbtnMaster_ValueChanged);
			// 
			// m_gbxMode
			// 
			this.m_gbxMode.Controls.Add(this.m_rbtnLockAB);
			this.m_gbxMode.Controls.Add(this.m_rbtnDefault);
			this.m_gbxMode.Controls.Add(this.m_lblMode);
			this.m_gbxMode.Location = new System.Drawing.Point(12, 12);
			this.m_gbxMode.Name = "m_gbxMode";
			this.m_gbxMode.Size = new System.Drawing.Size(363, 37);
			this.m_gbxMode.TabIndex = 0;
			this.m_gbxMode.TabStop = false;
			// 
			// m_rbtnLockAB
			// 
			this.m_rbtnLockAB.AutoSize = true;
			this.m_rbtnLockAB.Location = new System.Drawing.Point(159, 15);
			this.m_rbtnLockAB.Name = "m_rbtnLockAB";
			this.m_rbtnLockAB.Size = new System.Drawing.Size(71, 16);
			this.m_rbtnLockAB.TabIndex = 2;
			this.m_rbtnLockAB.Text = "鎖軸切割";
			this.m_rbtnLockAB.UseVisualStyleBackColor = true;
			// 
			// m_rbtnDefault
			// 
			this.m_rbtnDefault.AutoSize = true;
			this.m_rbtnDefault.Checked = true;
			this.m_rbtnDefault.Location = new System.Drawing.Point(86, 17);
			this.m_rbtnDefault.Name = "m_rbtnDefault";
			this.m_rbtnDefault.Size = new System.Drawing.Size(47, 16);
			this.m_rbtnDefault.TabIndex = 1;
			this.m_rbtnDefault.TabStop = true;
			this.m_rbtnDefault.Text = "默認";
			this.m_rbtnDefault.UseVisualStyleBackColor = true;
			this.m_rbtnDefault.CheckedChanged += new System.EventHandler(this.m_rbtnDefault_CheckedChanged);
			// 
			// m_lblMode
			// 
			this.m_lblMode.AutoSize = true;
			this.m_lblMode.Location = new System.Drawing.Point(12, 17);
			this.m_lblMode.Name = "m_lblMode";
			this.m_lblMode.Size = new System.Drawing.Size(29, 12);
			this.m_lblMode.TabIndex = 0;
			this.m_lblMode.Text = "模式";
			// 
			// LockMSAxisDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(392, 465);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnConfirm);
			this.Controls.Add(this.m_gbxLockAB);
			this.Controls.Add(this.m_gbxMode);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LockMSAxisDlg";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.m_gbxLockAB.ResumeLayout(false);
			this.m_gbxLockAB.PerformLayout();
			this.m_gbxSlaveSettings.ResumeLayout(false);
			this.m_tlpSlaveAngle.ResumeLayout(false);
			this.m_tlpSlaveAngle.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dbtnSlave)).EndInit();
			this.m_gbxRotaryDirection.ResumeLayout(false);
			this.m_gbxMachiningDir.ResumeLayout(false);
			this.m_gbxMachiningDir.PerformLayout();
			this.m_gbxMasterSettings.ResumeLayout(false);
			this.m_tlpMasterAngle.ResumeLayout(false);
			this.m_tlpMasterAngle.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dbtnMaster)).EndInit();
			this.m_gbxMode.ResumeLayout(false);
			this.m_gbxMode.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox m_gbxMode;
		private System.Windows.Forms.Label m_lblMode;
		private System.Windows.Forms.RadioButton m_rbtnDefault;
		private System.Windows.Forms.RadioButton m_rbtnLockAB;
		private System.Windows.Forms.GroupBox m_gbxLockAB;
		private System.Windows.Forms.CheckBox m_chkLockSlave;
		private System.Windows.Forms.GroupBox m_gbxSlaveSettings;
		private System.Windows.Forms.TableLayoutPanel m_tlpSlaveAngle;
		private System.Windows.Forms.Label m_lblSlaveAngle;
		private System.Windows.Forms.RadioButton m_rbtnSlaveAuto;
		private System.Windows.Forms.RadioButton m_rbtnSlaveManual;
		private DebouncedNumericUpDown m_dbtnSlave;
		private System.Windows.Forms.GroupBox m_gbxRotaryDirection;
		private System.Windows.Forms.Button m_btnRotaryPos;
		private System.Windows.Forms.Button m_btnRotaryNeg;
		private System.Windows.Forms.GroupBox m_gbxMachiningDir;
		private System.Windows.Forms.Label m_lblMachiningDir;
		private System.Windows.Forms.ComboBox m_cbxMachiningDir;
		private System.Windows.Forms.Label m_lblMachiningDirHint;
		private System.Windows.Forms.CheckBox m_chkLockMaster;
		private System.Windows.Forms.GroupBox m_gbxMasterSettings;
		private System.Windows.Forms.TableLayoutPanel m_tlpMasterAngle;
		private System.Windows.Forms.Label m_lblMasterAngle;
		private System.Windows.Forms.RadioButton m_rbtnMasterAuto;
		private System.Windows.Forms.RadioButton m_rbtnMasterManual;
		private DebouncedNumericUpDown m_dbtnMaster;
		private System.Windows.Forms.Button m_btnConfirm;
		private System.Windows.Forms.Button m_btnCancel;
	}
}
