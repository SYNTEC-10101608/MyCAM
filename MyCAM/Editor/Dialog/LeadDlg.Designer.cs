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
			this.m_pnlLeadIn = new CommonUI.Controls.SectionPanel();
			this.m_lblLeadInLineLength = new System.Windows.Forms.Label();
			this.m_numLeadInStraightLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblLeadInLineLengthUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblLeadInLength = new System.Windows.Forms.Label();
			this.m_numLeadInArcLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblLeadInLengthUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblLeadInAngle = new System.Windows.Forms.Label();
			this.m_numLeadInAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblLeadInAngleUnit = new CommonUI.Controls.ThemedLabel();
			this.m_pnlLeadOut = new CommonUI.Controls.SectionPanel();
			this.m_lblLeadOutLineLength = new System.Windows.Forms.Label();
			this.m_numLeadOutStraightLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblLeadOutLineLengthUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblLeadOutLength = new System.Windows.Forms.Label();
			this.m_numLeadOutArcLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblLeadOutLengthUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblLeadOutAngle = new System.Windows.Forms.Label();
			this.m_numLeadOutAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblLeadOutAngleUnit = new CommonUI.Controls.ThemedLabel();
			this.m_pnlOption = new CommonUI.Controls.SectionPanel();
			this.m_chkFlip = new System.Windows.Forms.CheckBox();
			this.m_btnOK = new CommonUI.Controls.ThemedButton();
			this.m_pnlLeadIn.SuspendLayout();
			this.m_pnlLeadOut.SuspendLayout();
			this.m_pnlOption.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numLeadInStraightLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numLeadInArcLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numLeadInAngle)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numLeadOutStraightLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numLeadOutArcLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numLeadOutAngle)).BeginInit();
			this.SuspendLayout();
			// 
			// m_pnlLeadIn
			// 
			this.m_pnlLeadIn.Controls.Add(this.m_lblLeadInLineLength);
			this.m_pnlLeadIn.Controls.Add(this.m_numLeadInStraightLength);
			this.m_pnlLeadIn.Controls.Add(this.m_lblLeadInLineLengthUnit);
			this.m_pnlLeadIn.Controls.Add(this.m_lblLeadInLength);
			this.m_pnlLeadIn.Controls.Add(this.m_numLeadInArcLength);
			this.m_pnlLeadIn.Controls.Add(this.m_lblLeadInLengthUnit);
			this.m_pnlLeadIn.Controls.Add(this.m_lblLeadInAngle);
			this.m_pnlLeadIn.Controls.Add(this.m_numLeadInAngle);
			this.m_pnlLeadIn.Controls.Add(this.m_lblLeadInAngleUnit);
			this.m_pnlLeadIn.Location = new System.Drawing.Point(12, 12);
			this.m_pnlLeadIn.Name = "m_pnlLeadIn";
			this.m_pnlLeadIn.Size = new System.Drawing.Size(296, 129);
			this.m_pnlLeadIn.TabIndex = 0;
			this.m_pnlLeadIn.Text = "進入引線";
			// 
			// m_lblLeadInLineLength
			// 
			this.m_lblLeadInLineLength.AutoSize = true;
			this.m_lblLeadInLineLength.Location = new System.Drawing.Point(10, 40);
			this.m_lblLeadInLineLength.Name = "m_lblLeadInLineLength";
			this.m_lblLeadInLineLength.Size = new System.Drawing.Size(55, 16);
			this.m_lblLeadInLineLength.TabIndex = 0;
			this.m_lblLeadInLineLength.Text = "直線長度";
			// 
			// m_numLeadInStraightLength
			// 
			this.m_numLeadInStraightLength.DecimalPlaces = 3;
			this.m_numLeadInStraightLength.Increment = new decimal(new int[] { 5, 0, 0, 65536});
			this.m_numLeadInStraightLength.Location = new System.Drawing.Point(100, 36);
			this.m_numLeadInStraightLength.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numLeadInStraightLength.Name = "m_numLeadInStraightLength";
			this.m_numLeadInStraightLength.Size = new System.Drawing.Size(150, 23);
			this.m_numLeadInStraightLength.TabIndex = 1;
			this.m_numLeadInStraightLength.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadIn_DebouncedValueChanged);
			// 
			// m_lblLeadInLineLengthUnit
			// 
			this.m_lblLeadInLineLengthUnit.AutoSize = true;
			this.m_lblLeadInLineLengthUnit.Location = new System.Drawing.Point(256, 40);
			this.m_lblLeadInLineLengthUnit.Name = "m_lblLeadInLineLengthUnit";
			this.m_lblLeadInLineLengthUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblLeadInLineLengthUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblLeadInLineLengthUnit.TabIndex = 2;
			this.m_lblLeadInLineLengthUnit.Text = "mm";
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
			// m_numLeadInArcLength
			// 
			this.m_numLeadInArcLength.DecimalPlaces = 3;
			this.m_numLeadInArcLength.Increment = new decimal(new int[] { 5, 0, 0, 65536});
			this.m_numLeadInArcLength.Location = new System.Drawing.Point(100, 66);
			this.m_numLeadInArcLength.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numLeadInArcLength.Name = "m_numLeadInArcLength";
			this.m_numLeadInArcLength.Size = new System.Drawing.Size(150, 23);
			this.m_numLeadInArcLength.TabIndex = 4;
			this.m_numLeadInArcLength.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadIn_DebouncedValueChanged);
			// 
			// m_lblLeadInLengthUnit
			// 
			this.m_lblLeadInLengthUnit.AutoSize = true;
			this.m_lblLeadInLengthUnit.Location = new System.Drawing.Point(256, 70);
			this.m_lblLeadInLengthUnit.Name = "m_lblLeadInLengthUnit";
			this.m_lblLeadInLengthUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblLeadInLengthUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblLeadInLengthUnit.TabIndex = 5;
			this.m_lblLeadInLengthUnit.Text = "mm";
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
			// m_numLeadInAngle
			// 
			this.m_numLeadInAngle.DecimalPlaces = 3;
			this.m_numLeadInAngle.Location = new System.Drawing.Point(100, 96);
			this.m_numLeadInAngle.Maximum = new decimal(new int[] { 180, 0, 0, 0});
			this.m_numLeadInAngle.Name = "m_numLeadInAngle";
			this.m_numLeadInAngle.Size = new System.Drawing.Size(150, 23);
			this.m_numLeadInAngle.TabIndex = 7;
			this.m_numLeadInAngle.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadIn_DebouncedValueChanged);
			// 
			// m_lblLeadInAngleUnit
			// 
			this.m_lblLeadInAngleUnit.AutoSize = true;
			this.m_lblLeadInAngleUnit.Location = new System.Drawing.Point(256, 100);
			this.m_lblLeadInAngleUnit.Name = "m_lblLeadInAngleUnit";
			this.m_lblLeadInAngleUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblLeadInAngleUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblLeadInAngleUnit.TabIndex = 8;
			this.m_lblLeadInAngleUnit.Text = "°";
			// 
			// m_pnlLeadOut
			// 
			this.m_pnlLeadOut.Controls.Add(this.m_lblLeadOutLineLength);
			this.m_pnlLeadOut.Controls.Add(this.m_numLeadOutStraightLength);
			this.m_pnlLeadOut.Controls.Add(this.m_lblLeadOutLineLengthUnit);
			this.m_pnlLeadOut.Controls.Add(this.m_lblLeadOutLength);
			this.m_pnlLeadOut.Controls.Add(this.m_numLeadOutArcLength);
			this.m_pnlLeadOut.Controls.Add(this.m_lblLeadOutLengthUnit);
			this.m_pnlLeadOut.Controls.Add(this.m_lblLeadOutAngle);
			this.m_pnlLeadOut.Controls.Add(this.m_numLeadOutAngle);
			this.m_pnlLeadOut.Controls.Add(this.m_lblLeadOutAngleUnit);
			this.m_pnlLeadOut.Location = new System.Drawing.Point(12, 149);
			this.m_pnlLeadOut.Name = "m_pnlLeadOut";
			this.m_pnlLeadOut.Size = new System.Drawing.Size(296, 129);
			this.m_pnlLeadOut.TabIndex = 1;
			this.m_pnlLeadOut.Text = "退出引線";
			// 
			// m_lblLeadOutLineLength
			// 
			this.m_lblLeadOutLineLength.AutoSize = true;
			this.m_lblLeadOutLineLength.Location = new System.Drawing.Point(10, 40);
			this.m_lblLeadOutLineLength.Name = "m_lblLeadOutLineLength";
			this.m_lblLeadOutLineLength.Size = new System.Drawing.Size(55, 16);
			this.m_lblLeadOutLineLength.TabIndex = 0;
			this.m_lblLeadOutLineLength.Text = "直線長度";
			// 
			// m_numLeadOutStraightLength
			// 
			this.m_numLeadOutStraightLength.DecimalPlaces = 3;
			this.m_numLeadOutStraightLength.Increment = new decimal(new int[] { 5, 0, 0, 65536});
			this.m_numLeadOutStraightLength.Location = new System.Drawing.Point(100, 36);
			this.m_numLeadOutStraightLength.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numLeadOutStraightLength.Name = "m_numLeadOutStraightLength";
			this.m_numLeadOutStraightLength.Size = new System.Drawing.Size(150, 23);
			this.m_numLeadOutStraightLength.TabIndex = 1;
			this.m_numLeadOutStraightLength.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadOut_DebouncedValueChanged);
			// 
			// m_lblLeadOutLineLengthUnit
			// 
			this.m_lblLeadOutLineLengthUnit.AutoSize = true;
			this.m_lblLeadOutLineLengthUnit.Location = new System.Drawing.Point(256, 40);
			this.m_lblLeadOutLineLengthUnit.Name = "m_lblLeadOutLineLengthUnit";
			this.m_lblLeadOutLineLengthUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblLeadOutLineLengthUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblLeadOutLineLengthUnit.TabIndex = 2;
			this.m_lblLeadOutLineLengthUnit.Text = "mm";
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
			// m_numLeadOutArcLength
			// 
			this.m_numLeadOutArcLength.DecimalPlaces = 3;
			this.m_numLeadOutArcLength.Increment = new decimal(new int[] { 5, 0, 0, 65536});
			this.m_numLeadOutArcLength.Location = new System.Drawing.Point(100, 66);
			this.m_numLeadOutArcLength.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numLeadOutArcLength.Name = "m_numLeadOutArcLength";
			this.m_numLeadOutArcLength.Size = new System.Drawing.Size(150, 23);
			this.m_numLeadOutArcLength.TabIndex = 4;
			this.m_numLeadOutArcLength.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadOut_DebouncedValueChanged);
			// 
			// m_lblLeadOutLengthUnit
			// 
			this.m_lblLeadOutLengthUnit.AutoSize = true;
			this.m_lblLeadOutLengthUnit.Location = new System.Drawing.Point(256, 70);
			this.m_lblLeadOutLengthUnit.Name = "m_lblLeadOutLengthUnit";
			this.m_lblLeadOutLengthUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblLeadOutLengthUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblLeadOutLengthUnit.TabIndex = 5;
			this.m_lblLeadOutLengthUnit.Text = "mm";
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
			// m_numLeadOutAngle
			// 
			this.m_numLeadOutAngle.DecimalPlaces = 3;
			this.m_numLeadOutAngle.Location = new System.Drawing.Point(100, 96);
			this.m_numLeadOutAngle.Maximum = new decimal(new int[] { 180, 0, 0, 0});
			this.m_numLeadOutAngle.Name = "m_numLeadOutAngle";
			this.m_numLeadOutAngle.Size = new System.Drawing.Size(150, 23);
			this.m_numLeadOutAngle.TabIndex = 7;
			this.m_numLeadOutAngle.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadOut_DebouncedValueChanged);
			// 
			// m_lblLeadOutAngleUnit
			// 
			this.m_lblLeadOutAngleUnit.AutoSize = true;
			this.m_lblLeadOutAngleUnit.Location = new System.Drawing.Point(256, 100);
			this.m_lblLeadOutAngleUnit.Name = "m_lblLeadOutAngleUnit";
			this.m_lblLeadOutAngleUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblLeadOutAngleUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblLeadOutAngleUnit.TabIndex = 8;
			this.m_lblLeadOutAngleUnit.Text = "°";
			// 
			// m_pnlOption
			// 
			this.m_pnlOption.Controls.Add(this.m_chkFlip);
			this.m_pnlOption.Location = new System.Drawing.Point(12, 286);
			this.m_pnlOption.Name = "m_pnlOption";
			this.m_pnlOption.Size = new System.Drawing.Size(296, 43);
			this.m_pnlOption.TabIndex = 2;
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
			this.Controls.Add(this.m_pnlLeadIn);
			this.Controls.Add(this.m_pnlLeadOut);
			this.Controls.Add(this.m_pnlOption);
			this.Controls.Add(this.m_btnOK);
			this.Name = "LeadDlg";
			this.Text = "引線設置";
			this.m_pnlLeadIn.ResumeLayout(false);
			this.m_pnlLeadIn.PerformLayout();
			this.m_pnlLeadOut.ResumeLayout(false);
			this.m_pnlLeadOut.PerformLayout();
			this.m_pnlOption.ResumeLayout(false);
			this.m_pnlOption.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numLeadInStraightLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numLeadInArcLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numLeadInAngle)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numLeadOutStraightLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numLeadOutArcLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numLeadOutAngle)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlLeadIn;
		private System.Windows.Forms.Label m_lblLeadInLineLength;
		private CommonUI.Controls.DebouncedNumericUpDown m_numLeadInStraightLength;
		private CommonUI.Controls.ThemedLabel m_lblLeadInLineLengthUnit;
		private System.Windows.Forms.Label m_lblLeadInLength;
		private CommonUI.Controls.DebouncedNumericUpDown m_numLeadInArcLength;
		private CommonUI.Controls.ThemedLabel m_lblLeadInLengthUnit;
		private System.Windows.Forms.Label m_lblLeadInAngle;
		private CommonUI.Controls.DebouncedNumericUpDown m_numLeadInAngle;
		private CommonUI.Controls.ThemedLabel m_lblLeadInAngleUnit;
		private CommonUI.Controls.SectionPanel m_pnlLeadOut;
		private System.Windows.Forms.Label m_lblLeadOutLineLength;
		private CommonUI.Controls.DebouncedNumericUpDown m_numLeadOutStraightLength;
		private CommonUI.Controls.ThemedLabel m_lblLeadOutLineLengthUnit;
		private System.Windows.Forms.Label m_lblLeadOutLength;
		private CommonUI.Controls.DebouncedNumericUpDown m_numLeadOutArcLength;
		private CommonUI.Controls.ThemedLabel m_lblLeadOutLengthUnit;
		private System.Windows.Forms.Label m_lblLeadOutAngle;
		private CommonUI.Controls.DebouncedNumericUpDown m_numLeadOutAngle;
		private CommonUI.Controls.ThemedLabel m_lblLeadOutAngleUnit;
		private CommonUI.Controls.SectionPanel m_pnlOption;
		private System.Windows.Forms.CheckBox m_chkFlip;
		private CommonUI.Controls.ThemedButton m_btnOK;
	}
}
