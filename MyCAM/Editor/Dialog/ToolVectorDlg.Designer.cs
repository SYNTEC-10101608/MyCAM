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
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.m_gbxParam = new System.Windows.Forms.GroupBox();
			this.m_tbxSlave = new System.Windows.Forms.TextBox();
			this.m_tbxMaster = new System.Windows.Forms.TextBox();
			this.m_lblSlave = new System.Windows.Forms.Label();
			this.m_lblMaster = new System.Windows.Forms.Label();
			this.m_btnKeep = new System.Windows.Forms.Button();
			this.m_btnZDir = new System.Windows.Forms.Button();
			this.m_tbxAngleB = new System.Windows.Forms.TextBox();
			this.m_tbxAngleA = new System.Windows.Forms.TextBox();
			this.m_lblBDeg = new System.Windows.Forms.Label();
			this.m_lblADeg = new System.Windows.Forms.Label();
			this.m_rbtVecSpace = new System.Windows.Forms.RadioButton();
			this.m_rbtInit = new System.Windows.Forms.RadioButton();
			this.m_btnRemove = new System.Windows.Forms.Button();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.m_rbtTiltAngleSpace = new System.Windows.Forms.RadioButton();
			this.m_btnMasterPos = new System.Windows.Forms.Button();
			this.m_btnMasterNeg = new System.Windows.Forms.Button();
			this.m_gpInterpolation.SuspendLayout();
			this.m_gbxParam.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_gpInterpolation
			// 
			this.m_gpInterpolation.Controls.Add(this.panel2);
			this.m_gpInterpolation.Controls.Add(this.label1);
			this.m_gpInterpolation.Controls.Add(this.m_rbtTiltAngleSpace);
			this.m_gpInterpolation.Controls.Add(this.m_lblVecSpaceHint);
			this.m_gpInterpolation.Controls.Add(this.panel1);
			this.m_gpInterpolation.Controls.Add(this.m_pnlTiltColot);
			this.m_gpInterpolation.Controls.Add(this.m_TiltAngleHint);
			this.m_gpInterpolation.Controls.Add(this.splitter1);
			this.m_gpInterpolation.Controls.Add(this.m_gbxParam);
			this.m_gpInterpolation.Controls.Add(this.m_rbtVecSpace);
			this.m_gpInterpolation.Controls.Add(this.m_rbtInit);
			this.m_gpInterpolation.Location = new System.Drawing.Point(10, 11);
			this.m_gpInterpolation.Name = "m_gpInterpolation";
			this.m_gpInterpolation.Size = new System.Drawing.Size(359, 222);
			this.m_gpInterpolation.TabIndex = 0;
			this.m_gpInterpolation.TabStop = false;
			this.m_gpInterpolation.Text = "插值方案";
			// 
			// m_lblVecSpaceHint
			// 
			this.m_lblVecSpaceHint.AutoSize = true;
			this.m_lblVecSpaceHint.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.m_lblVecSpaceHint.Location = new System.Drawing.Point(33, 38);
			this.m_lblVecSpaceHint.Name = "m_lblVecSpaceHint";
			this.m_lblVecSpaceHint.Size = new System.Drawing.Size(77, 12);
			this.m_lblVecSpaceHint.TabIndex = 8;
			this.m_lblVecSpaceHint.Text = "編修姿態呈現";
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Red;
			this.panel1.Location = new System.Drawing.Point(111, 36);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(14, 16);
			this.panel1.TabIndex = 6;
			// 
			// m_pnlTiltColot
			// 
			this.m_pnlTiltColot.BackColor = System.Drawing.Color.Orange;
			this.m_pnlTiltColot.Location = new System.Drawing.Point(111, 77);
			this.m_pnlTiltColot.Name = "m_pnlTiltColot";
			this.m_pnlTiltColot.Size = new System.Drawing.Size(14, 16);
			this.m_pnlTiltColot.TabIndex = 7;
			// 
			// m_TiltAngleHint
			// 
			this.m_TiltAngleHint.AutoSize = true;
			this.m_TiltAngleHint.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.m_TiltAngleHint.Location = new System.Drawing.Point(33, 80);
			this.m_TiltAngleHint.Name = "m_TiltAngleHint";
			this.m_TiltAngleHint.Size = new System.Drawing.Size(77, 12);
			this.m_TiltAngleHint.TabIndex = 7;
			this.m_TiltAngleHint.Text = "編修姿態呈現";
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(3, 18);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(1, 201);
			this.splitter1.TabIndex = 4;
			this.splitter1.TabStop = false;
			// 
			// m_gbxParam
			// 
			this.m_gbxParam.Controls.Add(this.m_btnMasterNeg);
			this.m_gbxParam.Controls.Add(this.m_btnMasterPos);
			this.m_gbxParam.Controls.Add(this.m_tbxSlave);
			this.m_gbxParam.Controls.Add(this.m_tbxMaster);
			this.m_gbxParam.Controls.Add(this.m_lblSlave);
			this.m_gbxParam.Controls.Add(this.m_lblMaster);
			this.m_gbxParam.Controls.Add(this.m_btnKeep);
			this.m_gbxParam.Controls.Add(this.m_btnZDir);
			this.m_gbxParam.Controls.Add(this.m_tbxAngleB);
			this.m_gbxParam.Controls.Add(this.m_tbxAngleA);
			this.m_gbxParam.Controls.Add(this.m_lblBDeg);
			this.m_gbxParam.Controls.Add(this.m_lblADeg);
			this.m_gbxParam.Location = new System.Drawing.Point(172, 18);
			this.m_gbxParam.Name = "m_gbxParam";
			this.m_gbxParam.Size = new System.Drawing.Size(180, 198);
			this.m_gbxParam.TabIndex = 3;
			this.m_gbxParam.TabStop = false;
			// 
			// m_tbxSlave
			// 
			this.m_tbxSlave.Location = new System.Drawing.Point(50, 102);
			this.m_tbxSlave.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.m_tbxSlave.Name = "m_tbxSlave";
			this.m_tbxSlave.Size = new System.Drawing.Size(48, 22);
			this.m_tbxSlave.TabIndex = 14;
			this.m_tbxSlave.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxSlave_KeyDown);
			this.m_tbxSlave.Leave += new System.EventHandler(this.m_tbxSlave_Leave);
			// 
			// m_tbxMaster
			// 
			this.m_tbxMaster.Location = new System.Drawing.Point(50, 74);
			this.m_tbxMaster.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.m_tbxMaster.Name = "m_tbxMaster";
			this.m_tbxMaster.Size = new System.Drawing.Size(48, 22);
			this.m_tbxMaster.TabIndex = 15;
			this.m_tbxMaster.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxMaster_KeyDown);
			this.m_tbxMaster.Leave += new System.EventHandler(this.m_tbxMaster_Leave);
			// 
			// m_lblSlave
			// 
			this.m_lblSlave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblSlave.AutoSize = true;
			this.m_lblSlave.Location = new System.Drawing.Point(5, 106);
			this.m_lblSlave.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.m_lblSlave.Name = "m_lblSlave";
			this.m_lblSlave.Size = new System.Drawing.Size(13, 12);
			this.m_lblSlave.TabIndex = 16;
			this.m_lblSlave.Text = "A";
			// 
			// m_lblMaster
			// 
			this.m_lblMaster.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblMaster.AutoSize = true;
			this.m_lblMaster.Location = new System.Drawing.Point(5, 76);
			this.m_lblMaster.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.m_lblMaster.Name = "m_lblMaster";
			this.m_lblMaster.Size = new System.Drawing.Size(13, 12);
			this.m_lblMaster.TabIndex = 13;
			this.m_lblMaster.Text = "C";
			// 
			// m_btnKeep
			// 
			this.m_btnKeep.Location = new System.Drawing.Point(6, 134);
			this.m_btnKeep.Name = "m_btnKeep";
			this.m_btnKeep.Size = new System.Drawing.Size(102, 27);
			this.m_btnKeep.TabIndex = 11;
			this.m_btnKeep.Text = "保持前編修姿態";
			this.m_btnKeep.UseVisualStyleBackColor = true;
			this.m_btnKeep.Click += new System.EventHandler(this.m_btnKeep_Click);
			// 
			// m_btnZDir
			// 
			this.m_btnZDir.Location = new System.Drawing.Point(6, 168);
			this.m_btnZDir.Name = "m_btnZDir";
			this.m_btnZDir.Size = new System.Drawing.Size(102, 23);
			this.m_btnZDir.TabIndex = 12;
			this.m_btnZDir.Text = "朝Z方向";
			this.m_btnZDir.UseVisualStyleBackColor = true;
			this.m_btnZDir.Click += new System.EventHandler(this.m_btnZDir_Click);
			// 
			// m_tbxAngleB
			// 
			this.m_tbxAngleB.Location = new System.Drawing.Point(50, 44);
			this.m_tbxAngleB.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.m_tbxAngleB.Name = "m_tbxAngleB";
			this.m_tbxAngleB.Size = new System.Drawing.Size(48, 22);
			this.m_tbxAngleB.TabIndex = 4;
			this.m_tbxAngleB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxAngleB_KeyDown);
			this.m_tbxAngleB.Leave += new System.EventHandler(this.m_tbxAngleB_Leave);
			// 
			// m_tbxAngleA
			// 
			this.m_tbxAngleA.Location = new System.Drawing.Point(50, 15);
			this.m_tbxAngleA.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.m_tbxAngleA.Name = "m_tbxAngleA";
			this.m_tbxAngleA.Size = new System.Drawing.Size(48, 22);
			this.m_tbxAngleA.TabIndex = 4;
			this.m_tbxAngleA.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxAngleA_KeyDown);
			this.m_tbxAngleA.Leave += new System.EventHandler(this.m_tbxAngleA_Leave);
			// 
			// m_lblBDeg
			// 
			this.m_lblBDeg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblBDeg.AutoSize = true;
			this.m_lblBDeg.Location = new System.Drawing.Point(5, 47);
			this.m_lblBDeg.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.m_lblBDeg.Name = "m_lblBDeg";
			this.m_lblBDeg.Size = new System.Drawing.Size(41, 12);
			this.m_lblBDeg.TabIndex = 4;
			this.m_lblBDeg.Text = "側傾角";
			// 
			// m_lblADeg
			// 
			this.m_lblADeg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblADeg.AutoSize = true;
			this.m_lblADeg.Location = new System.Drawing.Point(5, 18);
			this.m_lblADeg.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.m_lblADeg.Name = "m_lblADeg";
			this.m_lblADeg.Size = new System.Drawing.Size(41, 12);
			this.m_lblADeg.TabIndex = 1;
			this.m_lblADeg.Text = "前傾角";
			// 
			// m_rbtVecSpace
			// 
			this.m_rbtVecSpace.AutoSize = true;
			this.m_rbtVecSpace.Location = new System.Drawing.Point(18, 61);
			this.m_rbtVecSpace.Name = "m_rbtVecSpace";
			this.m_rbtVecSpace.Size = new System.Drawing.Size(83, 16);
			this.m_rbtVecSpace.TabIndex = 1;
			this.m_rbtVecSpace.TabStop = true;
			this.m_rbtVecSpace.Text = "旋轉軸插值";
			this.m_rbtVecSpace.UseVisualStyleBackColor = true;
			this.m_rbtVecSpace.CheckedChanged += new System.EventHandler(this.m_rbtTiltAngleCase_CheckedChanged);
			// 
			// m_rbtInit
			// 
			this.m_rbtInit.AutoSize = true;
			this.m_rbtInit.Location = new System.Drawing.Point(18, 21);
			this.m_rbtInit.Name = "m_rbtInit";
			this.m_rbtInit.Size = new System.Drawing.Size(106, 16);
			this.m_rbtInit.TabIndex = 0;
			this.m_rbtInit.TabStop = true;
			this.m_rbtInit.Text = "曲面法向 (默認)";
			this.m_rbtInit.UseVisualStyleBackColor = true;
			this.m_rbtInit.CheckedChanged += new System.EventHandler(this.m_rbtVecSpaceCase_CheckedChanged);
			// 
			// m_btnRemove
			// 
			this.m_btnRemove.Location = new System.Drawing.Point(248, 239);
			this.m_btnRemove.Name = "m_btnRemove";
			this.m_btnRemove.Size = new System.Drawing.Size(58, 30);
			this.m_btnRemove.TabIndex = 10;
			this.m_btnRemove.Text = "刪除";
			this.m_btnRemove.UseVisualStyleBackColor = true;
			this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
			// 
			// m_btnOK
			// 
			this.m_btnOK.Location = new System.Drawing.Point(312, 242);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(58, 25);
			this.m_btnOK.TabIndex = 11;
			this.m_btnOK.Text = "OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.Color.Cyan;
			this.panel2.Location = new System.Drawing.Point(111, 119);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(14, 16);
			this.panel2.TabIndex = 11;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.label1.Location = new System.Drawing.Point(33, 122);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(77, 12);
			this.label1.TabIndex = 10;
			this.label1.Text = "編修姿態呈現";
			// 
			// m_rbtTiltAngleCase
			// 
			this.m_rbtTiltAngleSpace.AutoSize = true;
			this.m_rbtTiltAngleSpace.Location = new System.Drawing.Point(18, 103);
			this.m_rbtTiltAngleSpace.Name = "m_rbtTiltAngleCase";
			this.m_rbtTiltAngleSpace.Size = new System.Drawing.Size(71, 16);
			this.m_rbtTiltAngleSpace.TabIndex = 9;
			this.m_rbtTiltAngleSpace.TabStop = true;
			this.m_rbtTiltAngleSpace.Text = "傾角插值";
			this.m_rbtTiltAngleSpace.UseVisualStyleBackColor = true;
			// 
			// m_btnMasterPos
			// 
			this.m_btnMasterPos.Location = new System.Drawing.Point(114, 134);
			this.m_btnMasterPos.Name = "m_btnMasterPos";
			this.m_btnMasterPos.Size = new System.Drawing.Size(56, 27);
			this.m_btnMasterPos.TabIndex = 17;
			this.m_btnMasterPos.Text = "C+";
			this.m_btnMasterPos.UseVisualStyleBackColor = true;
			// 
			// m_btnMasterNeg
			// 
			this.m_btnMasterNeg.Location = new System.Drawing.Point(114, 164);
			this.m_btnMasterNeg.Name = "m_btnMasterNeg";
			this.m_btnMasterNeg.Size = new System.Drawing.Size(56, 27);
			this.m_btnMasterNeg.TabIndex = 18;
			this.m_btnMasterNeg.Text = "C-";
			this.m_btnMasterNeg.UseVisualStyleBackColor = true;
			// 
			// ToolVectorDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(381, 274);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_btnRemove);
			this.Controls.Add(this.m_gpInterpolation);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
		private System.Windows.Forms.RadioButton m_rbtVecSpace;
		private System.Windows.Forms.RadioButton m_rbtInit;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Button m_btnRemove;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel m_pnlTiltColot;
		private System.Windows.Forms.Label m_TiltAngleHint;
		private System.Windows.Forms.Label m_lblVecSpaceHint;
		private System.Windows.Forms.TextBox m_tbxSlave;
		private System.Windows.Forms.TextBox m_tbxMaster;
		private System.Windows.Forms.Label m_lblSlave;
		private System.Windows.Forms.Label m_lblMaster;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton m_rbtTiltAngleSpace;
		private System.Windows.Forms.Button m_btnMasterNeg;
		private System.Windows.Forms.Button m_btnMasterPos;
	}
}