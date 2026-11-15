namespace MyCAM.Editor.Dialog
{
	partial class LeadDlg
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
			this.m_secLeadIn = new CommonUI.Controls.SectionPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.m_tbxLeadInStraightLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label2 = new CommonUI.Controls.ThemedLabel();
			this.m_lblLeadInLength = new System.Windows.Forms.Label();
			this.m_tbxLeadInArcLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_LeadInLegnthUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblLeadInAngle = new System.Windows.Forms.Label();
			this.m_tbxLeadInAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_LeadInAngleUnit = new CommonUI.Controls.ThemedLabel();
			this.m_secLeadOut = new CommonUI.Controls.SectionPanel();
			this.label4 = new System.Windows.Forms.Label();
			this.m_tbxLeadOutStraightLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label5 = new CommonUI.Controls.ThemedLabel();
			this.m_lblLeadOutLength = new System.Windows.Forms.Label();
			this.m_tbxLeadOutArcLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_LeadOutLegnthUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblLeadOutAngle = new System.Windows.Forms.Label();
			this.m_tbxLeadOutAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_LeadOutAngleUnit = new CommonUI.Controls.ThemedLabel();
			this.m_secOption = new CommonUI.Controls.SectionPanel();
			this.m_chkFlip = new System.Windows.Forms.CheckBox();
			this.m_btnOK = new CommonUI.Controls.ThemedButton();
			this.m_secLeadIn.SuspendLayout();
			this.m_secLeadOut.SuspendLayout();
			this.m_secOption.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadInStraightLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadInArcLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadInAngle)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadOutStraightLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadOutArcLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadOutAngle)).BeginInit();
			this.SuspendLayout();
			// 
			// m_secLeadIn
			// 
			this.m_secLeadIn.Controls.Add(this.label1);
			this.m_secLeadIn.Controls.Add(this.m_tbxLeadInStraightLength);
			this.m_secLeadIn.Controls.Add(this.label2);
			this.m_secLeadIn.Controls.Add(this.m_lblLeadInLength);
			this.m_secLeadIn.Controls.Add(this.m_tbxLeadInArcLength);
			this.m_secLeadIn.Controls.Add(this.m_LeadInLegnthUnit);
			this.m_secLeadIn.Controls.Add(this.m_lblLeadInAngle);
			this.m_secLeadIn.Controls.Add(this.m_tbxLeadInAngle);
			this.m_secLeadIn.Controls.Add(this.m_LeadInAngleUnit);
			this.m_secLeadIn.Location = new System.Drawing.Point(12, 12);
			this.m_secLeadIn.Name = "m_secLeadIn";
			this.m_secLeadIn.Size = new System.Drawing.Size(296, 129);
			this.m_secLeadIn.TabIndex = 0;
			this.m_secLeadIn.Text = "進入引線";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(10, 40);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(55, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "直線長度";
			// 
			// m_tbxLeadInStraightLength
			// 
			this.m_tbxLeadInStraightLength.DecimalPlaces = 3;
			this.m_tbxLeadInStraightLength.Increment = new decimal(new int[] { 5, 0, 0, 65536});
			this.m_tbxLeadInStraightLength.Location = new System.Drawing.Point(100, 36);
			this.m_tbxLeadInStraightLength.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_tbxLeadInStraightLength.Name = "m_tbxLeadInStraightLength";
			this.m_tbxLeadInStraightLength.Size = new System.Drawing.Size(150, 23);
			this.m_tbxLeadInStraightLength.TabIndex = 1;
			this.m_tbxLeadInStraightLength.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadIn_DebouncedValueChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(256, 40);
			this.label2.Name = "label2";
			this.label2.Role = CommonUI.Theme.TextRole.Secondary;
			this.label2.Size = new System.Drawing.Size(26, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "mm";
			// 
			// m_lblLeadInLength
			// 
			this.m_lblLeadInLength.AutoSize = true;
			this.m_lblLeadInLength.Location = new System.Drawing.Point(10, 70);
			this.m_lblLeadInLength.Name = "m_lblLeadInLength";
			this.m_lblLeadInLength.Size = new System.Drawing.Size(55, 16);
			this.m_lblLeadInLength.TabIndex = 3;
			this.m_lblLeadInLength.Text = "半徑";
			// 
			// m_tbxLeadInArcLength
			// 
			this.m_tbxLeadInArcLength.DecimalPlaces = 3;
			this.m_tbxLeadInArcLength.Increment = new decimal(new int[] { 5, 0, 0, 65536});
			this.m_tbxLeadInArcLength.Location = new System.Drawing.Point(100, 66);
			this.m_tbxLeadInArcLength.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_tbxLeadInArcLength.Name = "m_tbxLeadInArcLength";
			this.m_tbxLeadInArcLength.Size = new System.Drawing.Size(150, 23);
			this.m_tbxLeadInArcLength.TabIndex = 4;
			this.m_tbxLeadInArcLength.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadIn_DebouncedValueChanged);
			// 
			// m_LeadInLegnthUnit
			// 
			this.m_LeadInLegnthUnit.AutoSize = true;
			this.m_LeadInLegnthUnit.Location = new System.Drawing.Point(256, 70);
			this.m_LeadInLegnthUnit.Name = "m_LeadInLegnthUnit";
			this.m_LeadInLegnthUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_LeadInLegnthUnit.Size = new System.Drawing.Size(26, 16);
			this.m_LeadInLegnthUnit.TabIndex = 5;
			this.m_LeadInLegnthUnit.Text = "mm";
			// 
			// m_lblLeadInAngle
			// 
			this.m_lblLeadInAngle.AutoSize = true;
			this.m_lblLeadInAngle.Location = new System.Drawing.Point(10, 100);
			this.m_lblLeadInAngle.Name = "m_lblLeadInAngle";
			this.m_lblLeadInAngle.Size = new System.Drawing.Size(55, 16);
			this.m_lblLeadInAngle.TabIndex = 6;
			this.m_lblLeadInAngle.Text = "角度";
			// 
			// m_tbxLeadInAngle
			// 
			this.m_tbxLeadInAngle.DecimalPlaces = 3;
			this.m_tbxLeadInAngle.Location = new System.Drawing.Point(100, 96);
			this.m_tbxLeadInAngle.Maximum = new decimal(new int[] { 180, 0, 0, 0});
			this.m_tbxLeadInAngle.Name = "m_tbxLeadInAngle";
			this.m_tbxLeadInAngle.Size = new System.Drawing.Size(150, 23);
			this.m_tbxLeadInAngle.TabIndex = 7;
			this.m_tbxLeadInAngle.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadIn_DebouncedValueChanged);
			// 
			// m_LeadInAngleUnit
			// 
			this.m_LeadInAngleUnit.AutoSize = true;
			this.m_LeadInAngleUnit.Location = new System.Drawing.Point(256, 100);
			this.m_LeadInAngleUnit.Name = "m_LeadInAngleUnit";
			this.m_LeadInAngleUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_LeadInAngleUnit.Size = new System.Drawing.Size(26, 16);
			this.m_LeadInAngleUnit.TabIndex = 8;
			this.m_LeadInAngleUnit.Text = "°";
			// 
			// m_secLeadOut
			// 
			this.m_secLeadOut.Controls.Add(this.label4);
			this.m_secLeadOut.Controls.Add(this.m_tbxLeadOutStraightLength);
			this.m_secLeadOut.Controls.Add(this.label5);
			this.m_secLeadOut.Controls.Add(this.m_lblLeadOutLength);
			this.m_secLeadOut.Controls.Add(this.m_tbxLeadOutArcLength);
			this.m_secLeadOut.Controls.Add(this.m_LeadOutLegnthUnit);
			this.m_secLeadOut.Controls.Add(this.m_lblLeadOutAngle);
			this.m_secLeadOut.Controls.Add(this.m_tbxLeadOutAngle);
			this.m_secLeadOut.Controls.Add(this.m_LeadOutAngleUnit);
			this.m_secLeadOut.Location = new System.Drawing.Point(12, 149);
			this.m_secLeadOut.Name = "m_secLeadOut";
			this.m_secLeadOut.Size = new System.Drawing.Size(296, 129);
			this.m_secLeadOut.TabIndex = 1;
			this.m_secLeadOut.Text = "退出引線";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(10, 40);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(55, 16);
			this.label4.TabIndex = 0;
			this.label4.Text = "直線長度";
			// 
			// m_tbxLeadOutStraightLength
			// 
			this.m_tbxLeadOutStraightLength.DecimalPlaces = 3;
			this.m_tbxLeadOutStraightLength.Increment = new decimal(new int[] { 5, 0, 0, 65536});
			this.m_tbxLeadOutStraightLength.Location = new System.Drawing.Point(100, 36);
			this.m_tbxLeadOutStraightLength.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_tbxLeadOutStraightLength.Name = "m_tbxLeadOutStraightLength";
			this.m_tbxLeadOutStraightLength.Size = new System.Drawing.Size(150, 23);
			this.m_tbxLeadOutStraightLength.TabIndex = 1;
			this.m_tbxLeadOutStraightLength.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadOut_DebouncedValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(256, 40);
			this.label5.Name = "label5";
			this.label5.Role = CommonUI.Theme.TextRole.Secondary;
			this.label5.Size = new System.Drawing.Size(26, 16);
			this.label5.TabIndex = 2;
			this.label5.Text = "mm";
			// 
			// m_lblLeadOutLength
			// 
			this.m_lblLeadOutLength.AutoSize = true;
			this.m_lblLeadOutLength.Location = new System.Drawing.Point(10, 70);
			this.m_lblLeadOutLength.Name = "m_lblLeadOutLength";
			this.m_lblLeadOutLength.Size = new System.Drawing.Size(55, 16);
			this.m_lblLeadOutLength.TabIndex = 3;
			this.m_lblLeadOutLength.Text = "半徑";
			// 
			// m_tbxLeadOutArcLength
			// 
			this.m_tbxLeadOutArcLength.DecimalPlaces = 3;
			this.m_tbxLeadOutArcLength.Increment = new decimal(new int[] { 5, 0, 0, 65536});
			this.m_tbxLeadOutArcLength.Location = new System.Drawing.Point(100, 66);
			this.m_tbxLeadOutArcLength.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_tbxLeadOutArcLength.Name = "m_tbxLeadOutArcLength";
			this.m_tbxLeadOutArcLength.Size = new System.Drawing.Size(150, 23);
			this.m_tbxLeadOutArcLength.TabIndex = 4;
			this.m_tbxLeadOutArcLength.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadOut_DebouncedValueChanged);
			// 
			// m_LeadOutLegnthUnit
			// 
			this.m_LeadOutLegnthUnit.AutoSize = true;
			this.m_LeadOutLegnthUnit.Location = new System.Drawing.Point(256, 70);
			this.m_LeadOutLegnthUnit.Name = "m_LeadOutLegnthUnit";
			this.m_LeadOutLegnthUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_LeadOutLegnthUnit.Size = new System.Drawing.Size(26, 16);
			this.m_LeadOutLegnthUnit.TabIndex = 5;
			this.m_LeadOutLegnthUnit.Text = "mm";
			// 
			// m_lblLeadOutAngle
			// 
			this.m_lblLeadOutAngle.AutoSize = true;
			this.m_lblLeadOutAngle.Location = new System.Drawing.Point(10, 100);
			this.m_lblLeadOutAngle.Name = "m_lblLeadOutAngle";
			this.m_lblLeadOutAngle.Size = new System.Drawing.Size(55, 16);
			this.m_lblLeadOutAngle.TabIndex = 6;
			this.m_lblLeadOutAngle.Text = "角度";
			// 
			// m_tbxLeadOutAngle
			// 
			this.m_tbxLeadOutAngle.DecimalPlaces = 3;
			this.m_tbxLeadOutAngle.Location = new System.Drawing.Point(100, 96);
			this.m_tbxLeadOutAngle.Maximum = new decimal(new int[] { 180, 0, 0, 0});
			this.m_tbxLeadOutAngle.Name = "m_tbxLeadOutAngle";
			this.m_tbxLeadOutAngle.Size = new System.Drawing.Size(150, 23);
			this.m_tbxLeadOutAngle.TabIndex = 7;
			this.m_tbxLeadOutAngle.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadOut_DebouncedValueChanged);
			// 
			// m_LeadOutAngleUnit
			// 
			this.m_LeadOutAngleUnit.AutoSize = true;
			this.m_LeadOutAngleUnit.Location = new System.Drawing.Point(256, 100);
			this.m_LeadOutAngleUnit.Name = "m_LeadOutAngleUnit";
			this.m_LeadOutAngleUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_LeadOutAngleUnit.Size = new System.Drawing.Size(26, 16);
			this.m_LeadOutAngleUnit.TabIndex = 8;
			this.m_LeadOutAngleUnit.Text = "°";
			// 
			// m_secOption
			// 
			this.m_secOption.Controls.Add(this.m_chkFlip);
			this.m_secOption.Location = new System.Drawing.Point(12, 286);
			this.m_secOption.Name = "m_secOption";
			this.m_secOption.Size = new System.Drawing.Size(296, 43);
			this.m_secOption.TabIndex = 2;
			// 
			// m_chkFlip
			// 
			this.m_chkFlip.AutoSize = true;
			this.m_chkFlip.Location = new System.Drawing.Point(10, 12);
			this.m_chkFlip.Name = "m_chkFlip";
			this.m_chkFlip.Size = new System.Drawing.Size(74, 20);
			this.m_chkFlip.TabIndex = 0;
			this.m_chkFlip.Text = "引線反向";
			this.m_chkFlip.CheckedChanged += new System.EventHandler(this.m_chkFlip_CheckedChanged);
			// 
			// m_btnOK
			// 
			this.m_btnOK.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnOK.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnOK.Location = new System.Drawing.Point(228, 345);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(80, 28);
			this.m_btnOK.TabIndex = 3;
			this.m_btnOK.Text = "確定";
			this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
			// 
			// LeadDlg
			// 
			this.ClientSize = new System.Drawing.Size(320, 385);
			this.Controls.Add(this.m_secLeadIn);
			this.Controls.Add(this.m_secLeadOut);
			this.Controls.Add(this.m_secOption);
			this.Controls.Add(this.m_btnOK);
			this.Name = "LeadDlg";
			this.Text = "引線設置";
			this.m_secLeadIn.ResumeLayout(false);
			this.m_secLeadIn.PerformLayout();
			this.m_secLeadOut.ResumeLayout(false);
			this.m_secLeadOut.PerformLayout();
			this.m_secOption.ResumeLayout(false);
			this.m_secOption.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadInStraightLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadInArcLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadInAngle)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadOutStraightLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadOutArcLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadOutAngle)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_secLeadIn;
		private System.Windows.Forms.Label label1;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxLeadInStraightLength;
		private CommonUI.Controls.ThemedLabel label2;
		private System.Windows.Forms.Label m_lblLeadInLength;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxLeadInArcLength;
		private CommonUI.Controls.ThemedLabel m_LeadInLegnthUnit;
		private System.Windows.Forms.Label m_lblLeadInAngle;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxLeadInAngle;
		private CommonUI.Controls.ThemedLabel m_LeadInAngleUnit;
		private CommonUI.Controls.SectionPanel m_secLeadOut;
		private System.Windows.Forms.Label label4;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxLeadOutStraightLength;
		private CommonUI.Controls.ThemedLabel label5;
		private System.Windows.Forms.Label m_lblLeadOutLength;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxLeadOutArcLength;
		private CommonUI.Controls.ThemedLabel m_LeadOutLegnthUnit;
		private System.Windows.Forms.Label m_lblLeadOutAngle;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxLeadOutAngle;
		private CommonUI.Controls.ThemedLabel m_LeadOutAngleUnit;
		private CommonUI.Controls.SectionPanel m_secOption;
		private System.Windows.Forms.CheckBox m_chkFlip;
		private CommonUI.Controls.ThemedButton m_btnOK;
	}
}
