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
			this.m_tbxLeadInLength = new System.Windows.Forms.TextBox();
			this.m_LeadInLegnthUnit = new System.Windows.Forms.Label();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_cbxLeadInType = new System.Windows.Forms.ComboBox();
			this.m_lblLeadInLength = new System.Windows.Forms.Label();
			this.m_chkFlip = new System.Windows.Forms.CheckBox();
			this.m_cbxLeadOutType = new System.Windows.Forms.ComboBox();
			this.m_lblLeadOutLength = new System.Windows.Forms.Label();
			this.m_LeadOutAngleUnit = new System.Windows.Forms.Label();
			this.m_tbxLeadOutLength = new System.Windows.Forms.TextBox();
			this.m_LeadOutLegnthUnit = new System.Windows.Forms.Label();
			this.m_tbxLeadOutAngle = new System.Windows.Forms.TextBox();
			this.m_lblLeadOutAngle = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// m_LeadInAngleUnit
			// 
			this.m_LeadInAngleUnit.AutoSize = true;
			this.m_LeadInAngleUnit.Location = new System.Drawing.Point(173, 76);
			this.m_LeadInAngleUnit.Name = "m_LeadInAngleUnit";
			this.m_LeadInAngleUnit.Size = new System.Drawing.Size(8, 12);
			this.m_LeadInAngleUnit.TabIndex = 5;
			this.m_LeadInAngleUnit.Text = "°";
			// 
			// m_tbxLeadInAngle
			// 
			this.m_tbxLeadInAngle.Location = new System.Drawing.Point(104, 76);
			this.m_tbxLeadInAngle.Name = "m_tbxLeadInAngle";
			this.m_tbxLeadInAngle.Size = new System.Drawing.Size(63, 22);
			this.m_tbxLeadInAngle.TabIndex = 3;
			this.m_tbxLeadInAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxLeadInAngle_KeyDown);
			this.m_tbxLeadInAngle.Leave += new System.EventHandler(this.m_tbxLeadInAngle_Leave);
			// 
			// m_lblLeadInAngle
			// 
			this.m_lblLeadInAngle.AutoSize = true;
			this.m_lblLeadInAngle.Location = new System.Drawing.Point(21, 79);
			this.m_lblLeadInAngle.Name = "m_lblLeadInAngle";
			this.m_lblLeadInAngle.Size = new System.Drawing.Size(53, 12);
			this.m_lblLeadInAngle.TabIndex = 1;
			this.m_lblLeadInAngle.Text = "引線夾角";
			// 
			// m_tbxLeadInLength
			// 
			this.m_tbxLeadInLength.Location = new System.Drawing.Point(104, 38);
			this.m_tbxLeadInLength.Name = "m_tbxLeadInLength";
			this.m_tbxLeadInLength.Size = new System.Drawing.Size(63, 22);
			this.m_tbxLeadInLength.TabIndex = 2;
			this.m_tbxLeadInLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxLeadInLength_KeyDown);
			this.m_tbxLeadInLength.Leave += new System.EventHandler(this.m_tbxLeadInLength_Leave);
			// 
			// m_LeadInLegnthUnit
			// 
			this.m_LeadInLegnthUnit.AutoSize = true;
			this.m_LeadInLegnthUnit.Location = new System.Drawing.Point(173, 41);
			this.m_LeadInLegnthUnit.Name = "m_LeadInLegnthUnit";
			this.m_LeadInLegnthUnit.Size = new System.Drawing.Size(23, 12);
			this.m_LeadInLegnthUnit.TabIndex = 4;
			this.m_LeadInLegnthUnit.Text = "mm";
			// 
			// m_btnOK
			// 
			this.m_btnOK.Location = new System.Drawing.Point(149, 235);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(69, 23);
			this.m_btnOK.TabIndex = 6;
			this.m_btnOK.Text = "OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
			// 
			// m_cbxLeadInType
			// 
			this.m_cbxLeadInType.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cbxLeadInType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cbxLeadInType.FormattingEnabled = true;
			this.m_cbxLeadInType.Items.AddRange(new object[] {
            "無引入線",
            "直線引入線",
            "圓弧引入線"});
			this.m_cbxLeadInType.Location = new System.Drawing.Point(17, 12);
			this.m_cbxLeadInType.Name = "m_cbxLeadInType";
			this.m_cbxLeadInType.Size = new System.Drawing.Size(150, 20);
			this.m_cbxLeadInType.TabIndex = 0;
			this.m_cbxLeadInType.SelectedIndexChanged += new System.EventHandler(this.m_cbxLeadInType_SelectedIndexChanged);
			// 
			// m_lblLeadInLength
			// 
			this.m_lblLeadInLength.AutoSize = true;
			this.m_lblLeadInLength.Location = new System.Drawing.Point(21, 41);
			this.m_lblLeadInLength.Name = "m_lblLeadInLength";
			this.m_lblLeadInLength.Size = new System.Drawing.Size(53, 12);
			this.m_lblLeadInLength.TabIndex = 7;
			this.m_lblLeadInLength.Text = "引線長度";
			// 
			// m_chkFlip
			// 
			this.m_chkFlip.AutoSize = true;
			this.m_chkFlip.Location = new System.Drawing.Point(17, 239);
			this.m_chkFlip.Name = "m_chkFlip";
			this.m_chkFlip.Size = new System.Drawing.Size(72, 16);
			this.m_chkFlip.TabIndex = 8;
			this.m_chkFlip.Text = "引線反向";
			this.m_chkFlip.UseVisualStyleBackColor = true;
			this.m_chkFlip.CheckedChanged += new System.EventHandler(this.m_chkFlip_CheckedChanged);
			// 
			// m_cbxLeadOutType
			// 
			this.m_cbxLeadOutType.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cbxLeadOutType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cbxLeadOutType.FormattingEnabled = true;
			this.m_cbxLeadOutType.Items.AddRange(new object[] {
            "無引出線",
            "直線引出線",
            "圓弧引出線"});
			this.m_cbxLeadOutType.Location = new System.Drawing.Point(17, 121);
			this.m_cbxLeadOutType.Name = "m_cbxLeadOutType";
			this.m_cbxLeadOutType.Size = new System.Drawing.Size(150, 20);
			this.m_cbxLeadOutType.TabIndex = 9;
			this.m_cbxLeadOutType.SelectedIndexChanged += new System.EventHandler(this.m_cbxLeadOutType_SelectedIndexChanged);
			// 
			// m_lblLeadOutLength
			// 
			this.m_lblLeadOutLength.AutoSize = true;
			this.m_lblLeadOutLength.Location = new System.Drawing.Point(21, 160);
			this.m_lblLeadOutLength.Name = "m_lblLeadOutLength";
			this.m_lblLeadOutLength.Size = new System.Drawing.Size(53, 12);
			this.m_lblLeadOutLength.TabIndex = 15;
			this.m_lblLeadOutLength.Text = "引線長度";
			// 
			// m_LeadOutAngleUnit
			// 
			this.m_LeadOutAngleUnit.AutoSize = true;
			this.m_LeadOutAngleUnit.Location = new System.Drawing.Point(173, 195);
			this.m_LeadOutAngleUnit.Name = "m_LeadOutAngleUnit";
			this.m_LeadOutAngleUnit.Size = new System.Drawing.Size(8, 12);
			this.m_LeadOutAngleUnit.TabIndex = 14;
			this.m_LeadOutAngleUnit.Text = "°";
			// 
			// m_tbxLeadOutLength
			// 
			this.m_tbxLeadOutLength.Location = new System.Drawing.Point(104, 157);
			this.m_tbxLeadOutLength.Name = "m_tbxLeadOutLength";
			this.m_tbxLeadOutLength.Size = new System.Drawing.Size(63, 22);
			this.m_tbxLeadOutLength.TabIndex = 11;
			this.m_tbxLeadOutLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxLeadOutLength_KeyDown);
			this.m_tbxLeadOutLength.Leave += new System.EventHandler(this.m_tbxLeadOutLength_Leave);
			// 
			// m_LeadOutLegnthUnit
			// 
			this.m_LeadOutLegnthUnit.AutoSize = true;
			this.m_LeadOutLegnthUnit.Location = new System.Drawing.Point(173, 160);
			this.m_LeadOutLegnthUnit.Name = "m_LeadOutLegnthUnit";
			this.m_LeadOutLegnthUnit.Size = new System.Drawing.Size(23, 12);
			this.m_LeadOutLegnthUnit.TabIndex = 13;
			this.m_LeadOutLegnthUnit.Text = "mm";
			// 
			// m_tbxLeadOutAngle
			// 
			this.m_tbxLeadOutAngle.Location = new System.Drawing.Point(104, 195);
			this.m_tbxLeadOutAngle.Name = "m_tbxLeadOutAngle";
			this.m_tbxLeadOutAngle.Size = new System.Drawing.Size(63, 22);
			this.m_tbxLeadOutAngle.TabIndex = 12;
			this.m_tbxLeadOutAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxLeadOutAngle_KeyDown);
			this.m_tbxLeadOutAngle.Leave += new System.EventHandler(this.m_tbxLeadOutAngle_Leave);
			// 
			// m_lblLeadOutAngle
			// 
			this.m_lblLeadOutAngle.AutoSize = true;
			this.m_lblLeadOutAngle.Location = new System.Drawing.Point(21, 198);
			this.m_lblLeadOutAngle.Name = "m_lblLeadOutAngle";
			this.m_lblLeadOutAngle.Size = new System.Drawing.Size(53, 12);
			this.m_lblLeadOutAngle.TabIndex = 10;
			this.m_lblLeadOutAngle.Text = "引線夾角";
			// 
			// LeadLineForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(230, 267);
			this.Controls.Add(this.m_lblLeadOutLength);
			this.Controls.Add(this.m_LeadOutAngleUnit);
			this.Controls.Add(this.m_tbxLeadOutLength);
			this.Controls.Add(this.m_LeadOutLegnthUnit);
			this.Controls.Add(this.m_tbxLeadOutAngle);
			this.Controls.Add(this.m_lblLeadOutAngle);
			this.Controls.Add(this.m_cbxLeadOutType);
			this.Controls.Add(this.m_chkFlip);
			this.Controls.Add(this.m_lblLeadInLength);
			this.Controls.Add(this.m_cbxLeadInType);
			this.Controls.Add(this.m_LeadInAngleUnit);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_tbxLeadInLength);
			this.Controls.Add(this.m_LeadInLegnthUnit);
			this.Controls.Add(this.m_tbxLeadInAngle);
			this.Controls.Add(this.m_lblLeadInAngle);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LeadLineForm";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "引線";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.ComboBox m_cbxLeadInType;
		private System.Windows.Forms.Label m_lblLeadInAngle;
		private System.Windows.Forms.TextBox m_tbxLeadInAngle;
		private System.Windows.Forms.TextBox m_tbxLeadInLength;
		private System.Windows.Forms.Label m_LeadInLegnthUnit;
		private System.Windows.Forms.Label m_LeadInAngleUnit;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Label m_lblLeadInLength;
		private System.Windows.Forms.CheckBox m_chkFlip;
		private System.Windows.Forms.ComboBox m_cbxLeadOutType;
		private System.Windows.Forms.Label m_lblLeadOutLength;
		private System.Windows.Forms.Label m_LeadOutAngleUnit;
		private System.Windows.Forms.TextBox m_tbxLeadOutLength;
		private System.Windows.Forms.Label m_LeadOutLegnthUnit;
		private System.Windows.Forms.TextBox m_tbxLeadOutAngle;
		private System.Windows.Forms.Label m_lblLeadOutAngle;
	}
}