namespace MyCAM.Editor
{
	partial class ToolVectorDlg
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
			this.m_gpInterpolation = new System.Windows.Forms.GroupBox();
			this.m_lblVecSpaceHint = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.m_pnlTiltColot = new System.Windows.Forms.Panel();
			this.m_TiltAngleHint = new System.Windows.Forms.Label();
			this.m_lblFixDirHint = new System.Windows.Forms.Label();
			this.m_pnlFixedDirColor = new System.Windows.Forms.Panel();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.m_gbxParam = new System.Windows.Forms.GroupBox();
			this.m_btnKeep = new System.Windows.Forms.Button();
			this.m_btnZDir = new System.Windows.Forms.Button();
			this.m_tbxAngleB = new System.Windows.Forms.TextBox();
			this.m_tbxAngleA = new System.Windows.Forms.TextBox();
			this.m_lblBDeg = new System.Windows.Forms.Label();
			this.m_lblADeg = new System.Windows.Forms.Label();
			this.m_rbtFixedDir = new System.Windows.Forms.RadioButton();
			this.m_rbtTiltAngleCase = new System.Windows.Forms.RadioButton();
			this.m_rbtVecSpaceCase = new System.Windows.Forms.RadioButton();
			this.m_btnRemove = new System.Windows.Forms.Button();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_gpInterpolation.SuspendLayout();
			this.m_gbxParam.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_gpInterpolation
			// 
			this.m_gpInterpolation.Controls.Add(this.m_lblVecSpaceHint);
			this.m_gpInterpolation.Controls.Add(this.panel1);
			this.m_gpInterpolation.Controls.Add(this.m_pnlTiltColot);
			this.m_gpInterpolation.Controls.Add(this.m_TiltAngleHint);
			this.m_gpInterpolation.Controls.Add(this.m_lblFixDirHint);
			this.m_gpInterpolation.Controls.Add(this.m_pnlFixedDirColor);
			this.m_gpInterpolation.Controls.Add(this.splitter1);
			this.m_gpInterpolation.Controls.Add(this.m_gbxParam);
			this.m_gpInterpolation.Controls.Add(this.m_rbtFixedDir);
			this.m_gpInterpolation.Controls.Add(this.m_rbtTiltAngleCase);
			this.m_gpInterpolation.Controls.Add(this.m_rbtVecSpaceCase);
			this.m_gpInterpolation.Location = new System.Drawing.Point(13, 14);
			this.m_gpInterpolation.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_gpInterpolation.Name = "m_gpInterpolation";
			this.m_gpInterpolation.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_gpInterpolation.Size = new System.Drawing.Size(387, 205);
			this.m_gpInterpolation.TabIndex = 0;
			this.m_gpInterpolation.TabStop = false;
			this.m_gpInterpolation.Text = "插值方案";
			// 
			// m_lblVecSpaceHint
			// 
			this.m_lblVecSpaceHint.AutoSize = true;
			this.m_lblVecSpaceHint.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.m_lblVecSpaceHint.Location = new System.Drawing.Point(44, 48);
			this.m_lblVecSpaceHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblVecSpaceHint.Name = "m_lblVecSpaceHint";
			this.m_lblVecSpaceHint.Size = new System.Drawing.Size(97, 15);
			this.m_lblVecSpaceHint.TabIndex = 8;
			this.m_lblVecSpaceHint.Text = "編修姿態呈現";
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Red;
			this.panel1.Location = new System.Drawing.Point(148, 45);
			this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(19, 20);
			this.panel1.TabIndex = 6;
			// 
			// m_pnlTiltColot
			// 
			this.m_pnlTiltColot.BackColor = System.Drawing.Color.Orange;
			this.m_pnlTiltColot.Location = new System.Drawing.Point(148, 96);
			this.m_pnlTiltColot.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_pnlTiltColot.Name = "m_pnlTiltColot";
			this.m_pnlTiltColot.Size = new System.Drawing.Size(19, 20);
			this.m_pnlTiltColot.TabIndex = 7;
			this.m_pnlTiltColot.Visible = false;
			// 
			// m_TiltAngleHint
			// 
			this.m_TiltAngleHint.AutoSize = true;
			this.m_TiltAngleHint.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.m_TiltAngleHint.Location = new System.Drawing.Point(44, 100);
			this.m_TiltAngleHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_TiltAngleHint.Name = "m_TiltAngleHint";
			this.m_TiltAngleHint.Size = new System.Drawing.Size(97, 15);
			this.m_TiltAngleHint.TabIndex = 7;
			this.m_TiltAngleHint.Text = "編修姿態呈現";
			this.m_TiltAngleHint.Visible = false;
			// 
			// m_lblFixDirHint
			// 
			this.m_lblFixDirHint.AutoSize = true;
			this.m_lblFixDirHint.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.m_lblFixDirHint.Location = new System.Drawing.Point(44, 158);
			this.m_lblFixDirHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblFixDirHint.Name = "m_lblFixDirHint";
			this.m_lblFixDirHint.Size = new System.Drawing.Size(97, 15);
			this.m_lblFixDirHint.TabIndex = 6;
			this.m_lblFixDirHint.Text = "路徑姿態呈現";
			this.m_lblFixDirHint.Visible = false;
			// 
			// m_pnlFixedDirColor
			// 
			this.m_pnlFixedDirColor.BackColor = System.Drawing.Color.SkyBlue;
			this.m_pnlFixedDirColor.Location = new System.Drawing.Point(148, 154);
			this.m_pnlFixedDirColor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_pnlFixedDirColor.Name = "m_pnlFixedDirColor";
			this.m_pnlFixedDirColor.Size = new System.Drawing.Size(19, 20);
			this.m_pnlFixedDirColor.TabIndex = 5;
			this.m_pnlFixedDirColor.Visible = false;
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(4, 22);
			this.splitter1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(1, 179);
			this.splitter1.TabIndex = 4;
			this.splitter1.TabStop = false;
			// 
			// m_gbxParam
			// 
			this.m_gbxParam.Controls.Add(this.m_btnKeep);
			this.m_gbxParam.Controls.Add(this.m_btnZDir);
			this.m_gbxParam.Controls.Add(this.m_tbxAngleB);
			this.m_gbxParam.Controls.Add(this.m_tbxAngleA);
			this.m_gbxParam.Controls.Add(this.m_lblBDeg);
			this.m_gbxParam.Controls.Add(this.m_lblADeg);
			this.m_gbxParam.Location = new System.Drawing.Point(229, 22);
			this.m_gbxParam.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_gbxParam.Name = "m_gbxParam";
			this.m_gbxParam.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_gbxParam.Size = new System.Drawing.Size(157, 174);
			this.m_gbxParam.TabIndex = 3;
			this.m_gbxParam.TabStop = false;
			// 
			// m_btnKeep
			// 
			this.m_btnKeep.Location = new System.Drawing.Point(8, 94);
			this.m_btnKeep.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_btnKeep.Name = "m_btnKeep";
			this.m_btnKeep.Size = new System.Drawing.Size(136, 34);
			this.m_btnKeep.TabIndex = 11;
			this.m_btnKeep.Text = "保持前編修姿態";
			this.m_btnKeep.UseVisualStyleBackColor = true;
			this.m_btnKeep.Visible = false;
			this.m_btnKeep.Click += new System.EventHandler(this.m_btnKeep_Click);
			// 
			// m_btnZDir
			// 
			this.m_btnZDir.Location = new System.Drawing.Point(9, 135);
			this.m_btnZDir.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_btnZDir.Name = "m_btnZDir";
			this.m_btnZDir.Size = new System.Drawing.Size(136, 29);
			this.m_btnZDir.TabIndex = 12;
			this.m_btnZDir.Text = "朝Z方向";
			this.m_btnZDir.UseVisualStyleBackColor = true;
			this.m_btnZDir.Visible = false;
			this.m_btnZDir.Click += new System.EventHandler(this.m_btnZDir_Click);
			// 
			// m_tbxAngleB
			// 
			this.m_tbxAngleB.Location = new System.Drawing.Point(67, 55);
			this.m_tbxAngleB.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.m_tbxAngleB.Name = "m_tbxAngleB";
			this.m_tbxAngleB.Size = new System.Drawing.Size(63, 25);
			this.m_tbxAngleB.TabIndex = 4;
			this.m_tbxAngleB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxAngleB_KeyDown);
			this.m_tbxAngleB.Leave += new System.EventHandler(this.m_tbxAngleB_Leave);
			// 
			// m_tbxAngleA
			// 
			this.m_tbxAngleA.Location = new System.Drawing.Point(67, 19);
			this.m_tbxAngleA.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.m_tbxAngleA.Name = "m_tbxAngleA";
			this.m_tbxAngleA.Size = new System.Drawing.Size(63, 25);
			this.m_tbxAngleA.TabIndex = 4;
			this.m_tbxAngleA.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxAngleA_KeyDown);
			this.m_tbxAngleA.Leave += new System.EventHandler(this.m_tbxAngleA_Leave);
			// 
			// m_lblBDeg
			// 
			this.m_lblBDeg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblBDeg.AutoSize = true;
			this.m_lblBDeg.Location = new System.Drawing.Point(7, 59);
			this.m_lblBDeg.Name = "m_lblBDeg";
			this.m_lblBDeg.Size = new System.Drawing.Size(17, 15);
			this.m_lblBDeg.TabIndex = 4;
			this.m_lblBDeg.Text = "A";
			// 
			// m_lblADeg
			// 
			this.m_lblADeg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblADeg.AutoSize = true;
			this.m_lblADeg.Location = new System.Drawing.Point(7, 22);
			this.m_lblADeg.Name = "m_lblADeg";
			this.m_lblADeg.Size = new System.Drawing.Size(16, 15);
			this.m_lblADeg.TabIndex = 1;
			this.m_lblADeg.Text = "C";
			// 
			// m_rbtFixedDir
			// 
			this.m_rbtFixedDir.AutoSize = true;
			this.m_rbtFixedDir.Location = new System.Drawing.Point(24, 130);
			this.m_rbtFixedDir.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_rbtFixedDir.Name = "m_rbtFixedDir";
			this.m_rbtFixedDir.Size = new System.Drawing.Size(103, 19);
			this.m_rbtFixedDir.TabIndex = 2;
			this.m_rbtFixedDir.TabStop = true;
			this.m_rbtFixedDir.Text = "定姿態插值";
			this.m_rbtFixedDir.UseVisualStyleBackColor = true;
			this.m_rbtFixedDir.Visible = false;
			this.m_rbtFixedDir.CheckedChanged += new System.EventHandler(this.m_rbtFixedDir_CheckedChanged);
			// 
			// m_rbtTiltAngleCase
			// 
			this.m_rbtTiltAngleCase.AutoSize = true;
			this.m_rbtTiltAngleCase.Location = new System.Drawing.Point(24, 76);
			this.m_rbtTiltAngleCase.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_rbtTiltAngleCase.Name = "m_rbtTiltAngleCase";
			this.m_rbtTiltAngleCase.Size = new System.Drawing.Size(88, 19);
			this.m_rbtTiltAngleCase.TabIndex = 1;
			this.m_rbtTiltAngleCase.TabStop = true;
			this.m_rbtTiltAngleCase.Text = "傾角插值";
			this.m_rbtTiltAngleCase.UseVisualStyleBackColor = true;
			this.m_rbtTiltAngleCase.Visible = false;
			this.m_rbtTiltAngleCase.CheckedChanged += new System.EventHandler(this.m_rbtTiltAngleCase_CheckedChanged);
			// 
			// m_rbtVecSpaceCase
			// 
			this.m_rbtVecSpaceCase.AutoSize = true;
			this.m_rbtVecSpaceCase.Location = new System.Drawing.Point(24, 26);
			this.m_rbtVecSpaceCase.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_rbtVecSpaceCase.Name = "m_rbtVecSpaceCase";
			this.m_rbtVecSpaceCase.Size = new System.Drawing.Size(88, 19);
			this.m_rbtVecSpaceCase.TabIndex = 0;
			this.m_rbtVecSpaceCase.TabStop = true;
			this.m_rbtVecSpaceCase.Text = "向量插值";
			this.m_rbtVecSpaceCase.UseVisualStyleBackColor = true;
			this.m_rbtVecSpaceCase.CheckedChanged += new System.EventHandler(this.m_rbtVecSpaceCase_CheckedChanged);
			// 
			// m_btnRemove
			// 
			this.m_btnRemove.Location = new System.Drawing.Point(237, 224);
			this.m_btnRemove.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_btnRemove.Name = "m_btnRemove";
			this.m_btnRemove.Size = new System.Drawing.Size(77, 38);
			this.m_btnRemove.TabIndex = 10;
			this.m_btnRemove.Text = "刪除";
			this.m_btnRemove.UseVisualStyleBackColor = true;
			this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
			// 
			// m_btnOK
			// 
			this.m_btnOK.Location = new System.Drawing.Point(323, 230);
			this.m_btnOK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(77, 31);
			this.m_btnOK.TabIndex = 11;
			this.m_btnOK.Text = "OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
			// 
			// ToolVectorDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(411, 264);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_btnRemove);
			this.Controls.Add(this.m_gpInterpolation);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ToolVectorDlg";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "姿態";
			this.m_gpInterpolation.ResumeLayout(false);
			this.m_gpInterpolation.PerformLayout();
			this.m_gbxParam.ResumeLayout(false);
			this.m_gbxParam.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox m_gpInterpolation;
		private System.Windows.Forms.GroupBox m_gbxParam;
		private System.Windows.Forms.Button m_btnKeep;
		private System.Windows.Forms.Button m_btnZDir;
		private System.Windows.Forms.TextBox m_tbxAngleB;
		private System.Windows.Forms.TextBox m_tbxAngleA;
		private System.Windows.Forms.Label m_lblBDeg;
		private System.Windows.Forms.Label m_lblADeg;
		private System.Windows.Forms.RadioButton m_rbtFixedDir;
		private System.Windows.Forms.RadioButton m_rbtTiltAngleCase;
		private System.Windows.Forms.RadioButton m_rbtVecSpaceCase;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Button m_btnRemove;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Label m_lblFixDirHint;
		private System.Windows.Forms.Panel m_pnlFixedDirColor;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel m_pnlTiltColot;
		private System.Windows.Forms.Label m_TiltAngleHint;
		private System.Windows.Forms.Label m_lblVecSpaceHint;
	}
}