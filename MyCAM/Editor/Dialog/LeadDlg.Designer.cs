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
			this.m_LeadInAngleUnit = new System.Windows.Forms.Label();
			this.m_tbxLeadInAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblLeadInAngle = new System.Windows.Forms.Label();
			this.m_tbxLeadInArcLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_LeadInLegnthUnit = new System.Windows.Forms.Label();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_lblLeadInLength = new System.Windows.Forms.Label();
			this.m_chkFlip = new System.Windows.Forms.CheckBox();
			this.m_lblLeadOutLength = new System.Windows.Forms.Label();
			this.m_LeadOutAngleUnit = new System.Windows.Forms.Label();
			this.m_tbxLeadOutArcLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_LeadOutLegnthUnit = new System.Windows.Forms.Label();
			this.m_tbxLeadOutAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblLeadOutAngle = new System.Windows.Forms.Label();
			this.m_lblLeadIn = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.m_tbxLeadInStraightLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.m_tbxLeadOutStraightLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadInAngle)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadInArcLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadOutArcLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadOutAngle)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadInStraightLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadOutStraightLength)).BeginInit();
			this.SuspendLayout();
			//
			// m_LeadInAngleUnit
			//
			this.m_LeadInAngleUnit.AutoSize = true;
			this.m_LeadInAngleUnit.Location = new System.Drawing.Point(231, 144);
			this.m_LeadInAngleUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_LeadInAngleUnit.Name = "m_LeadInAngleUnit";
			this.m_LeadInAngleUnit.Size = new System.Drawing.Size(11, 15);
			this.m_LeadInAngleUnit.TabIndex = 5;
			this.m_LeadInAngleUnit.Text = "°";
			//
			// m_tbxLeadInAngle
			//
			this.m_tbxLeadInAngle.DecimalPlaces = 3;
			this.m_tbxLeadInAngle.Location = new System.Drawing.Point(133, 138);
			this.m_tbxLeadInAngle.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_tbxLeadInAngle.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
			this.m_tbxLeadInAngle.Name = "m_tbxLeadInAngle";
			this.m_tbxLeadInAngle.Size = new System.Drawing.Size(84, 25);
			this.m_tbxLeadInAngle.TabIndex = 3;
			this.m_tbxLeadInAngle.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadIn_DebouncedValueChanged);
			//
			// m_lblLeadInAngle
			//
			this.m_lblLeadInAngle.AutoSize = true;
			this.m_lblLeadInAngle.Location = new System.Drawing.Point(27, 144);
			this.m_lblLeadInAngle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblLeadInAngle.Name = "m_lblLeadInAngle";
			this.m_lblLeadInAngle.Size = new System.Drawing.Size(67, 15);
			this.m_lblLeadInAngle.TabIndex = 1;
			this.m_lblLeadInAngle.Text = "引線夾角";
			//
			// m_tbxLeadInArcLength
			//
			this.m_tbxLeadInArcLength.DecimalPlaces = 3;
			this.m_tbxLeadInArcLength.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.m_tbxLeadInArcLength.Location = new System.Drawing.Point(133, 94);
			this.m_tbxLeadInArcLength.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_tbxLeadInArcLength.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_tbxLeadInArcLength.Name = "m_tbxLeadInArcLength";
			this.m_tbxLeadInArcLength.Size = new System.Drawing.Size(84, 25);
			this.m_tbxLeadInArcLength.TabIndex = 2;
			this.m_tbxLeadInArcLength.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadIn_DebouncedValueChanged);
			//
			// m_LeadInLegnthUnit
			//
			this.m_LeadInLegnthUnit.AutoSize = true;
			this.m_LeadInLegnthUnit.Location = new System.Drawing.Point(231, 100);
			this.m_LeadInLegnthUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_LeadInLegnthUnit.Name = "m_LeadInLegnthUnit";
			this.m_LeadInLegnthUnit.Size = new System.Drawing.Size(29, 15);
			this.m_LeadInLegnthUnit.TabIndex = 4;
			this.m_LeadInLegnthUnit.Text = "mm";
			//
			// m_btnOK
			//
			this.m_btnOK.Location = new System.Drawing.Point(199, 379);
			this.m_btnOK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(92, 29);
			this.m_btnOK.TabIndex = 6;
			this.m_btnOK.Text = "OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
			//
			// m_lblLeadInLength
			//
			this.m_lblLeadInLength.AutoSize = true;
			this.m_lblLeadInLength.Location = new System.Drawing.Point(27, 100);
			this.m_lblLeadInLength.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblLeadInLength.Name = "m_lblLeadInLength";
			this.m_lblLeadInLength.Size = new System.Drawing.Size(67, 15);
			this.m_lblLeadInLength.TabIndex = 7;
			this.m_lblLeadInLength.Text = "圓弧長度";
			//
			// m_chkFlip
			//
			this.m_chkFlip.AutoSize = true;
			this.m_chkFlip.Location = new System.Drawing.Point(23, 384);
			this.m_chkFlip.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_chkFlip.Name = "m_chkFlip";
			this.m_chkFlip.Size = new System.Drawing.Size(89, 19);
			this.m_chkFlip.TabIndex = 8;
			this.m_chkFlip.Text = "引線反向";
			this.m_chkFlip.UseVisualStyleBackColor = true;
			this.m_chkFlip.CheckedChanged += new System.EventHandler(this.m_chkFlip_CheckedChanged);
			//
			// m_lblLeadOutLength
			//
			this.m_lblLeadOutLength.AutoSize = true;
			this.m_lblLeadOutLength.Location = new System.Drawing.Point(27, 288);
			this.m_lblLeadOutLength.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblLeadOutLength.Name = "m_lblLeadOutLength";
			this.m_lblLeadOutLength.Size = new System.Drawing.Size(67, 15);
			this.m_lblLeadOutLength.TabIndex = 15;
			this.m_lblLeadOutLength.Text = "圓弧長度";
			//
			// m_LeadOutAngleUnit
			//
			this.m_LeadOutAngleUnit.AutoSize = true;
			this.m_LeadOutAngleUnit.Location = new System.Drawing.Point(231, 331);
			this.m_LeadOutAngleUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_LeadOutAngleUnit.Name = "m_LeadOutAngleUnit";
			this.m_LeadOutAngleUnit.Size = new System.Drawing.Size(11, 15);
			this.m_LeadOutAngleUnit.TabIndex = 14;
			this.m_LeadOutAngleUnit.Text = "°";
			//
			// m_tbxLeadOutArcLength
			//
			this.m_tbxLeadOutArcLength.DecimalPlaces = 3;
			this.m_tbxLeadOutArcLength.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.m_tbxLeadOutArcLength.Location = new System.Drawing.Point(133, 281);
			this.m_tbxLeadOutArcLength.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_tbxLeadOutArcLength.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_tbxLeadOutArcLength.Name = "m_tbxLeadOutArcLength";
			this.m_tbxLeadOutArcLength.Size = new System.Drawing.Size(84, 25);
			this.m_tbxLeadOutArcLength.TabIndex = 11;
			this.m_tbxLeadOutArcLength.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadOut_DebouncedValueChanged);
			//
			// m_LeadOutLegnthUnit
			//
			this.m_LeadOutLegnthUnit.AutoSize = true;
			this.m_LeadOutLegnthUnit.Location = new System.Drawing.Point(229, 288);
			this.m_LeadOutLegnthUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_LeadOutLegnthUnit.Name = "m_LeadOutLegnthUnit";
			this.m_LeadOutLegnthUnit.Size = new System.Drawing.Size(29, 15);
			this.m_LeadOutLegnthUnit.TabIndex = 13;
			this.m_LeadOutLegnthUnit.Text = "mm";
			//
			// m_tbxLeadOutAngle
			//
			this.m_tbxLeadOutAngle.DecimalPlaces = 3;
			this.m_tbxLeadOutAngle.Location = new System.Drawing.Point(133, 331);
			this.m_tbxLeadOutAngle.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_tbxLeadOutAngle.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
			this.m_tbxLeadOutAngle.Name = "m_tbxLeadOutAngle";
			this.m_tbxLeadOutAngle.Size = new System.Drawing.Size(84, 25);
			this.m_tbxLeadOutAngle.TabIndex = 12;
			this.m_tbxLeadOutAngle.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadOut_DebouncedValueChanged);
			//
			// m_lblLeadOutAngle
			//
			this.m_lblLeadOutAngle.AutoSize = true;
			this.m_lblLeadOutAngle.Location = new System.Drawing.Point(27, 331);
			this.m_lblLeadOutAngle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblLeadOutAngle.Name = "m_lblLeadOutAngle";
			this.m_lblLeadOutAngle.Size = new System.Drawing.Size(67, 15);
			this.m_lblLeadOutAngle.TabIndex = 10;
			this.m_lblLeadOutAngle.Text = "引線夾角";
			//
			// m_lblLeadIn
			//
			this.m_lblLeadIn.AutoSize = true;
			this.m_lblLeadIn.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.m_lblLeadIn.Location = new System.Drawing.Point(16, 12);
			this.m_lblLeadIn.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblLeadIn.Name = "m_lblLeadIn";
			this.m_lblLeadIn.Size = new System.Drawing.Size(69, 20);
			this.m_lblLeadIn.TabIndex = 16;
			this.m_lblLeadIn.Text = "引入線";
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(27, 56);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(67, 15);
			this.label1.TabIndex = 19;
			this.label1.Text = "直線長度";
			//
			// m_tbxLeadInStraightLength
			//
			this.m_tbxLeadInStraightLength.DecimalPlaces = 3;
			this.m_tbxLeadInStraightLength.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.m_tbxLeadInStraightLength.Location = new System.Drawing.Point(133, 50);
			this.m_tbxLeadInStraightLength.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_tbxLeadInStraightLength.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_tbxLeadInStraightLength.Name = "m_tbxLeadInStraightLength";
			this.m_tbxLeadInStraightLength.Size = new System.Drawing.Size(84, 25);
			this.m_tbxLeadInStraightLength.TabIndex = 17;
			this.m_tbxLeadInStraightLength.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadIn_DebouncedValueChanged);
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(229, 54);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(29, 15);
			this.label2.TabIndex = 18;
			this.label2.Text = "mm";
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.label3.Location = new System.Drawing.Point(16, 200);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(69, 20);
			this.label3.TabIndex = 20;
			this.label3.Text = "引出線";
			//
			// label4
			//
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(27, 244);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(67, 15);
			this.label4.TabIndex = 23;
			this.label4.Text = "直線長度";
			//
			// m_tbxLeadOutStraightLength
			//
			this.m_tbxLeadOutStraightLength.DecimalPlaces = 3;
			this.m_tbxLeadOutStraightLength.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.m_tbxLeadOutStraightLength.Location = new System.Drawing.Point(133, 238);
			this.m_tbxLeadOutStraightLength.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_tbxLeadOutStraightLength.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_tbxLeadOutStraightLength.Name = "m_tbxLeadOutStraightLength";
			this.m_tbxLeadOutStraightLength.Size = new System.Drawing.Size(84, 25);
			this.m_tbxLeadOutStraightLength.TabIndex = 21;
			this.m_tbxLeadOutStraightLength.DebouncedValueChanged += new System.EventHandler(this.m_tbxLeadOut_DebouncedValueChanged);
			//
			// label5
			//
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(229, 244);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(29, 15);
			this.label5.TabIndex = 22;
			this.label5.Text = "mm";
			//
			// LeadDlg
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(307, 420);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.m_tbxLeadOutStraightLength);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.m_tbxLeadInStraightLength);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.m_lblLeadIn);
			this.Controls.Add(this.m_lblLeadOutLength);
			this.Controls.Add(this.m_LeadOutAngleUnit);
			this.Controls.Add(this.m_tbxLeadOutArcLength);
			this.Controls.Add(this.m_LeadOutLegnthUnit);
			this.Controls.Add(this.m_tbxLeadOutAngle);
			this.Controls.Add(this.m_lblLeadOutAngle);
			this.Controls.Add(this.m_chkFlip);
			this.Controls.Add(this.m_lblLeadInLength);
			this.Controls.Add(this.m_LeadInAngleUnit);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_tbxLeadInArcLength);
			this.Controls.Add(this.m_LeadInLegnthUnit);
			this.Controls.Add(this.m_tbxLeadInAngle);
			this.Controls.Add(this.m_lblLeadInAngle);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LeadDlg";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "引線";
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadInAngle)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadInArcLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadOutArcLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadOutAngle)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadInStraightLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxLeadOutStraightLength)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label m_lblLeadInAngle;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxLeadInAngle;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxLeadInArcLength;
		private System.Windows.Forms.Label m_LeadInLegnthUnit;
		private System.Windows.Forms.Label m_LeadInAngleUnit;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Label m_lblLeadInLength;
		private System.Windows.Forms.CheckBox m_chkFlip;
		private System.Windows.Forms.Label m_lblLeadOutLength;
		private System.Windows.Forms.Label m_LeadOutAngleUnit;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxLeadOutArcLength;
		private System.Windows.Forms.Label m_LeadOutLegnthUnit;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxLeadOutAngle;
		private System.Windows.Forms.Label m_lblLeadOutAngle;
		private System.Windows.Forms.Label m_lblLeadIn;
		private System.Windows.Forms.Label label1;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxLeadInStraightLength;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxLeadOutStraightLength;
		private System.Windows.Forms.Label label5;
	}
}
