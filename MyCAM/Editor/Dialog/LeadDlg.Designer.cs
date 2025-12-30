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
			this.m_tbxLeadInAngle = new System.Windows.Forms.TextBox();
			this.m_lblLeadInAngle = new System.Windows.Forms.Label();
			this.m_tbxLeadInArcLength = new System.Windows.Forms.TextBox();
			this.m_LeadInLegnthUnit = new System.Windows.Forms.Label();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_lblLeadInLength = new System.Windows.Forms.Label();
			this.m_chkFlip = new System.Windows.Forms.CheckBox();
			this.m_lblLeadOutLength = new System.Windows.Forms.Label();
			this.m_LeadOutAngleUnit = new System.Windows.Forms.Label();
			this.m_tbxLeadOutArcLength = new System.Windows.Forms.TextBox();
			this.m_LeadOutLegnthUnit = new System.Windows.Forms.Label();
			this.m_tbxLeadOutAngle = new System.Windows.Forms.TextBox();
			this.m_lblLeadOutAngle = new System.Windows.Forms.Label();
			this.m_lblLeadIn = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.m_tbxLeadInStraightLength = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.m_tbxLeadOutStraightLength = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// m_LeadInAngleUnit
			// 
			this.m_LeadInAngleUnit.AutoSize = true;
			this.m_LeadInAngleUnit.Location = new System.Drawing.Point(173, 115);
			this.m_LeadInAngleUnit.Name = "m_LeadInAngleUnit";
			this.m_LeadInAngleUnit.Size = new System.Drawing.Size(8, 12);
			this.m_LeadInAngleUnit.TabIndex = 5;
			this.m_LeadInAngleUnit.Text = "°";
			// 
			// m_tbxLeadInAngle
			// 
			this.m_tbxLeadInAngle.Location = new System.Drawing.Point(100, 110);
			this.m_tbxLeadInAngle.Name = "m_tbxLeadInAngle";
			this.m_tbxLeadInAngle.Size = new System.Drawing.Size(63, 22);
			this.m_tbxLeadInAngle.TabIndex = 3;
			this.m_tbxLeadInAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxLeadInAngle_KeyDown);
			this.m_tbxLeadInAngle.Leave += new System.EventHandler(this.m_tbxLeadInAngle_Leave);
			// 
			// m_lblLeadInAngle
			// 
			this.m_lblLeadInAngle.AutoSize = true;
			this.m_lblLeadInAngle.Location = new System.Drawing.Point(20, 115);
			this.m_lblLeadInAngle.Name = "m_lblLeadInAngle";
			this.m_lblLeadInAngle.Size = new System.Drawing.Size(53, 12);
			this.m_lblLeadInAngle.TabIndex = 1;
			this.m_lblLeadInAngle.Text = "引線夾角";
			// 
			// m_tbxLeadInArcLength
			// 
			this.m_tbxLeadInArcLength.Location = new System.Drawing.Point(100, 75);
			this.m_tbxLeadInArcLength.Name = "m_tbxLeadInArcLength";
			this.m_tbxLeadInArcLength.Size = new System.Drawing.Size(63, 22);
			this.m_tbxLeadInArcLength.TabIndex = 2;
			this.m_tbxLeadInArcLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxLeadInArcLength_KeyDown);
			this.m_tbxLeadInArcLength.Leave += new System.EventHandler(this.m_tbxLeadInArcLength_Leave);
			// 
			// m_LeadInLegnthUnit
			// 
			this.m_LeadInLegnthUnit.AutoSize = true;
			this.m_LeadInLegnthUnit.Location = new System.Drawing.Point(173, 80);
			this.m_LeadInLegnthUnit.Name = "m_LeadInLegnthUnit";
			this.m_LeadInLegnthUnit.Size = new System.Drawing.Size(23, 12);
			this.m_LeadInLegnthUnit.TabIndex = 4;
			this.m_LeadInLegnthUnit.Text = "mm";
			// 
			// m_btnOK
			// 
			this.m_btnOK.Location = new System.Drawing.Point(149, 303);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(69, 23);
			this.m_btnOK.TabIndex = 6;
			this.m_btnOK.Text = "OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
			// 
			// m_lblLeadInLength
			// 
			this.m_lblLeadInLength.AutoSize = true;
			this.m_lblLeadInLength.Location = new System.Drawing.Point(20, 80);
			this.m_lblLeadInLength.Name = "m_lblLeadInLength";
			this.m_lblLeadInLength.Size = new System.Drawing.Size(53, 12);
			this.m_lblLeadInLength.TabIndex = 7;
			this.m_lblLeadInLength.Text = "圓弧長度";
			// 
			// m_chkFlip
			// 
			this.m_chkFlip.AutoSize = true;
			this.m_chkFlip.Location = new System.Drawing.Point(17, 307);
			this.m_chkFlip.Name = "m_chkFlip";
			this.m_chkFlip.Size = new System.Drawing.Size(72, 16);
			this.m_chkFlip.TabIndex = 8;
			this.m_chkFlip.Text = "引線反向";
			this.m_chkFlip.UseVisualStyleBackColor = true;
			this.m_chkFlip.CheckedChanged += new System.EventHandler(this.m_chkFlip_CheckedChanged);
			// 
			// m_lblLeadOutLength
			// 
			this.m_lblLeadOutLength.AutoSize = true;
			this.m_lblLeadOutLength.Location = new System.Drawing.Point(20, 230);
			this.m_lblLeadOutLength.Name = "m_lblLeadOutLength";
			this.m_lblLeadOutLength.Size = new System.Drawing.Size(53, 12);
			this.m_lblLeadOutLength.TabIndex = 15;
			this.m_lblLeadOutLength.Text = "圓弧長度";
			// 
			// m_LeadOutAngleUnit
			// 
			this.m_LeadOutAngleUnit.AutoSize = true;
			this.m_LeadOutAngleUnit.Location = new System.Drawing.Point(173, 265);
			this.m_LeadOutAngleUnit.Name = "m_LeadOutAngleUnit";
			this.m_LeadOutAngleUnit.Size = new System.Drawing.Size(8, 12);
			this.m_LeadOutAngleUnit.TabIndex = 14;
			this.m_LeadOutAngleUnit.Text = "°";
			// 
			// m_tbxLeadOutArcLength
			// 
			this.m_tbxLeadOutArcLength.Location = new System.Drawing.Point(100, 225);
			this.m_tbxLeadOutArcLength.Name = "m_tbxLeadOutArcLength";
			this.m_tbxLeadOutArcLength.Size = new System.Drawing.Size(63, 22);
			this.m_tbxLeadOutArcLength.TabIndex = 11;
			this.m_tbxLeadOutArcLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxLeadOutArcLength_KeyDown);
			this.m_tbxLeadOutArcLength.Leave += new System.EventHandler(this.m_tbxLeadOutArcLength_Leave);
			// 
			// m_LeadOutLegnthUnit
			// 
			this.m_LeadOutLegnthUnit.AutoSize = true;
			this.m_LeadOutLegnthUnit.Location = new System.Drawing.Point(172, 230);
			this.m_LeadOutLegnthUnit.Name = "m_LeadOutLegnthUnit";
			this.m_LeadOutLegnthUnit.Size = new System.Drawing.Size(23, 12);
			this.m_LeadOutLegnthUnit.TabIndex = 13;
			this.m_LeadOutLegnthUnit.Text = "mm";
			// 
			// m_tbxLeadOutAngle
			// 
			this.m_tbxLeadOutAngle.Location = new System.Drawing.Point(100, 265);
			this.m_tbxLeadOutAngle.Name = "m_tbxLeadOutAngle";
			this.m_tbxLeadOutAngle.Size = new System.Drawing.Size(63, 22);
			this.m_tbxLeadOutAngle.TabIndex = 12;
			this.m_tbxLeadOutAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxLeadOutAngle_KeyDown);
			this.m_tbxLeadOutAngle.Leave += new System.EventHandler(this.m_tbxLeadOutAngle_Leave);
			// 
			// m_lblLeadOutAngle
			// 
			this.m_lblLeadOutAngle.AutoSize = true;
			this.m_lblLeadOutAngle.Location = new System.Drawing.Point(20, 265);
			this.m_lblLeadOutAngle.Name = "m_lblLeadOutAngle";
			this.m_lblLeadOutAngle.Size = new System.Drawing.Size(53, 12);
			this.m_lblLeadOutAngle.TabIndex = 10;
			this.m_lblLeadOutAngle.Text = "引線夾角";
			// 
			// m_lblLeadIn
			// 
			this.m_lblLeadIn.AutoSize = true;
			this.m_lblLeadIn.Font = new System.Drawing.Font("PMingLiU", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.m_lblLeadIn.Location = new System.Drawing.Point(12, 10);
			this.m_lblLeadIn.Name = "m_lblLeadIn";
			this.m_lblLeadIn.Size = new System.Drawing.Size(55, 16);
			this.m_lblLeadIn.TabIndex = 16;
			this.m_lblLeadIn.Text = "引入線";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(20, 45);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(53, 12);
			this.label1.TabIndex = 19;
			this.label1.Text = "直線長度";
			// 
			// m_tbxLeadInStraightLength
			// 
			this.m_tbxLeadInStraightLength.Location = new System.Drawing.Point(100, 40);
			this.m_tbxLeadInStraightLength.Name = "m_tbxLeadInStraightLength";
			this.m_tbxLeadInStraightLength.Size = new System.Drawing.Size(63, 22);
			this.m_tbxLeadInStraightLength.TabIndex = 17;
			this.m_tbxLeadInStraightLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxLeadInStraightLength_KeyDown);
			this.m_tbxLeadInStraightLength.Leave += new System.EventHandler(this.m_tbxLeadInStraightLength_Leave);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(172, 43);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(23, 12);
			this.label2.TabIndex = 18;
			this.label2.Text = "mm";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("PMingLiU", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.label3.Location = new System.Drawing.Point(12, 160);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(55, 16);
			this.label3.TabIndex = 20;
			this.label3.Text = "引出線";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(20, 195);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(53, 12);
			this.label4.TabIndex = 23;
			this.label4.Text = "直線長度";
			// 
			// m_tbxLeadOutStraightLength
			// 
			this.m_tbxLeadOutStraightLength.Location = new System.Drawing.Point(100, 190);
			this.m_tbxLeadOutStraightLength.Name = "m_tbxLeadOutStraightLength";
			this.m_tbxLeadOutStraightLength.Size = new System.Drawing.Size(63, 22);
			this.m_tbxLeadOutStraightLength.TabIndex = 21;
			this.m_tbxLeadOutStraightLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxLeadOutStraightLength_KeyDown);
			this.m_tbxLeadOutStraightLength.Leave += new System.EventHandler(this.m_tbxLeadOutStraightLength_Leave);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(172, 195);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(23, 12);
			this.label5.TabIndex = 22;
			this.label5.Text = "mm";
			// 
			// LeadDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(230, 336);
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
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LeadDlg";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "引線";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label m_lblLeadInAngle;
		private System.Windows.Forms.TextBox m_tbxLeadInAngle;
		private System.Windows.Forms.TextBox m_tbxLeadInArcLength;
		private System.Windows.Forms.Label m_LeadInLegnthUnit;
		private System.Windows.Forms.Label m_LeadInAngleUnit;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Label m_lblLeadInLength;
		private System.Windows.Forms.CheckBox m_chkFlip;
		private System.Windows.Forms.Label m_lblLeadOutLength;
		private System.Windows.Forms.Label m_LeadOutAngleUnit;
		private System.Windows.Forms.TextBox m_tbxLeadOutArcLength;
		private System.Windows.Forms.Label m_LeadOutLegnthUnit;
		private System.Windows.Forms.TextBox m_tbxLeadOutAngle;
		private System.Windows.Forms.Label m_lblLeadOutAngle;
		private System.Windows.Forms.Label m_lblLeadIn;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox m_tbxLeadInStraightLength;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox m_tbxLeadOutStraightLength;
		private System.Windows.Forms.Label label5;
	}
}