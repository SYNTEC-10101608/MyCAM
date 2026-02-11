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
			this.m_gbxInterpolation = new System.Windows.Forms.GroupBox();
			this.m_btnNext = new System.Windows.Forms.Button();
			this.m_btnPrev = new System.Windows.Forms.Button();
			this.m_tbInterType = new System.Windows.Forms.TableLayoutPanel();
			this.m_pnlTiltColor = new System.Windows.Forms.Panel();
			this.m_rbtNormal = new System.Windows.Forms.RadioButton();
			this.m_rbtVecSpace = new System.Windows.Forms.RadioButton();
			this.m_rbtTilt = new System.Windows.Forms.RadioButton();
			this.m_lblVecSpaceHint = new System.Windows.Forms.Label();
			this.m_TiltHint = new System.Windows.Forms.Label();
			this.m_lblNormalHint = new System.Windows.Forms.Label();
			this.m_pnlVecSpaceColor = new System.Windows.Forms.Panel();
			this.m_pnlNormalColor = new System.Windows.Forms.Panel();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.m_gbxIndexParam = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.m_btnRotaryNeg = new System.Windows.Forms.Button();
			this.m_lblADeg = new System.Windows.Forms.Label();
			this.m_lblBDeg = new System.Windows.Forms.Label();
			this.m_lblMaster = new System.Windows.Forms.Label();
			this.m_lblSlave = new System.Windows.Forms.Label();
			this.m_tbxSlave = new System.Windows.Forms.TextBox();
			this.m_tbxAngleA = new System.Windows.Forms.TextBox();
			this.m_btnRotaryPos = new System.Windows.Forms.Button();
			this.m_tbxAngleB = new System.Windows.Forms.TextBox();
			this.m_tbxMaster = new System.Windows.Forms.TextBox();
			this.m_btnAdd = new System.Windows.Forms.Button();
			this.m_btnRemove = new System.Windows.Forms.Button();
			this.m_btnKeep = new System.Windows.Forms.Button();
			this.m_btnZDir = new System.Windows.Forms.Button();
			this.m_btnRevert = new System.Windows.Forms.Button();
			this.m_btnSwitchStartEnd = new System.Windows.Forms.Button();
			this.m_gbxInterpolation.SuspendLayout();
			this.m_tbInterType.SuspendLayout();
			this.m_gbxIndexParam.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_gbxInterpolation
			// 
			this.m_gbxInterpolation.Controls.Add(this.m_btnNext);
			this.m_gbxInterpolation.Controls.Add(this.m_btnPrev);
			this.m_gbxInterpolation.Controls.Add(this.m_tbInterType);
			this.m_gbxInterpolation.Controls.Add(this.splitter1);
			this.m_gbxInterpolation.Controls.Add(this.m_gbxIndexParam);
			this.m_gbxInterpolation.Location = new System.Drawing.Point(10, 11);
			this.m_gbxInterpolation.Name = "m_gbxInterpolation";
			this.m_gbxInterpolation.Size = new System.Drawing.Size(392, 234);
			this.m_gbxInterpolation.TabIndex = 0;
			this.m_gbxInterpolation.TabStop = false;
			this.m_gbxInterpolation.Text = "插值方案";
			// 
			// m_btnNext
			// 
			this.m_btnNext.Location = new System.Drawing.Point(133, 196);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(40, 23);
			this.m_btnNext.TabIndex = 7;
			this.m_btnNext.Text = "->";
			this.m_btnNext.UseVisualStyleBackColor = true;
			this.m_btnNext.Click += new System.EventHandler(this.m_btnNext_Click);
			// 
			// m_btnPrev
			// 
			this.m_btnPrev.Location = new System.Drawing.Point(10, 196);
			this.m_btnPrev.Name = "m_btnPrev";
			this.m_btnPrev.Size = new System.Drawing.Size(40, 23);
			this.m_btnPrev.TabIndex = 6;
			this.m_btnPrev.Text = "<-";
			this.m_btnPrev.UseVisualStyleBackColor = true;
			this.m_btnPrev.Click += new System.EventHandler(this.m_btnPrev_Click);
			// 
			// m_tbInterType
			// 
			this.m_tbInterType.ColumnCount = 2;
			this.m_tbInterType.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.m_tbInterType.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.m_tbInterType.Controls.Add(this.m_pnlTiltColor, 1, 5);
			this.m_tbInterType.Controls.Add(this.m_rbtNormal, 0, 0);
			this.m_tbInterType.Controls.Add(this.m_rbtVecSpace, 0, 2);
			this.m_tbInterType.Controls.Add(this.m_rbtTilt, 0, 4);
			this.m_tbInterType.Controls.Add(this.m_lblVecSpaceHint, 0, 3);
			this.m_tbInterType.Controls.Add(this.m_TiltHint, 0, 5);
			this.m_tbInterType.Controls.Add(this.m_lblNormalHint, 0, 1);
			this.m_tbInterType.Controls.Add(this.m_pnlVecSpaceColor, 1, 3);
			this.m_tbInterType.Controls.Add(this.m_pnlNormalColor, 1, 1);
			this.m_tbInterType.Location = new System.Drawing.Point(10, 34);
			this.m_tbInterType.Name = "m_tbInterType";
			this.m_tbInterType.RowCount = 6;
			this.m_tbInterType.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.m_tbInterType.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.m_tbInterType.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.m_tbInterType.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.m_tbInterType.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.m_tbInterType.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.m_tbInterType.Size = new System.Drawing.Size(163, 156);
			this.m_tbInterType.TabIndex = 5;
			// 
			// m_pnlTiltColor
			// 
			this.m_pnlTiltColor.BackColor = System.Drawing.Color.Cyan;
			this.m_pnlTiltColor.Location = new System.Drawing.Point(123, 128);
			this.m_pnlTiltColor.Name = "m_pnlTiltColor";
			this.m_pnlTiltColor.Size = new System.Drawing.Size(14, 16);
			this.m_pnlTiltColor.TabIndex = 11;
			// 
			// m_rbtNormal
			// 
			this.m_rbtNormal.AutoSize = true;
			this.m_rbtNormal.Location = new System.Drawing.Point(3, 3);
			this.m_rbtNormal.Name = "m_rbtNormal";
			this.m_rbtNormal.Size = new System.Drawing.Size(106, 16);
			this.m_rbtNormal.TabIndex = 0;
			this.m_rbtNormal.TabStop = true;
			this.m_rbtNormal.Text = "曲面法向 (默認)";
			this.m_rbtNormal.UseVisualStyleBackColor = true;
			this.m_rbtNormal.CheckedChanged += new System.EventHandler(this.m_rbtNormal_CheckedChanged);
			// 
			// m_rbtVecSpace
			// 
			this.m_rbtVecSpace.AutoSize = true;
			this.m_rbtVecSpace.Location = new System.Drawing.Point(3, 53);
			this.m_rbtVecSpace.Name = "m_rbtVecSpace";
			this.m_rbtVecSpace.Size = new System.Drawing.Size(83, 16);
			this.m_rbtVecSpace.TabIndex = 1;
			this.m_rbtVecSpace.TabStop = true;
			this.m_rbtVecSpace.Text = "旋轉軸插值";
			this.m_rbtVecSpace.UseVisualStyleBackColor = true;
			this.m_rbtVecSpace.CheckedChanged += new System.EventHandler(this.m_rbtVecSpace_CheckedChanged);
			// 
			// m_rbtTilt
			// 
			this.m_rbtTilt.AutoSize = true;
			this.m_rbtTilt.Location = new System.Drawing.Point(3, 103);
			this.m_rbtTilt.Name = "m_rbtTilt";
			this.m_rbtTilt.Size = new System.Drawing.Size(71, 16);
			this.m_rbtTilt.TabIndex = 9;
			this.m_rbtTilt.TabStop = true;
			this.m_rbtTilt.Text = "傾角插值";
			this.m_rbtTilt.UseVisualStyleBackColor = true;
			this.m_rbtTilt.CheckedChanged += new System.EventHandler(this.m_rbtTilt_CheckedChanged);
			// 
			// m_lblVecSpaceHint
			// 
			this.m_lblVecSpaceHint.AutoSize = true;
			this.m_lblVecSpaceHint.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.m_lblVecSpaceHint.Location = new System.Drawing.Point(3, 75);
			this.m_lblVecSpaceHint.Name = "m_lblVecSpaceHint";
			this.m_lblVecSpaceHint.Size = new System.Drawing.Size(77, 12);
			this.m_lblVecSpaceHint.TabIndex = 8;
			this.m_lblVecSpaceHint.Text = "編修姿態呈現";
			// 
			// m_TiltHint
			// 
			this.m_TiltHint.AutoSize = true;
			this.m_TiltHint.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.m_TiltHint.Location = new System.Drawing.Point(3, 125);
			this.m_TiltHint.Name = "m_TiltHint";
			this.m_TiltHint.Size = new System.Drawing.Size(77, 12);
			this.m_TiltHint.TabIndex = 7;
			this.m_TiltHint.Text = "編修姿態呈現";
			// 
			// m_lblNormalHint
			// 
			this.m_lblNormalHint.AutoSize = true;
			this.m_lblNormalHint.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.m_lblNormalHint.Location = new System.Drawing.Point(3, 25);
			this.m_lblNormalHint.Name = "m_lblNormalHint";
			this.m_lblNormalHint.Size = new System.Drawing.Size(77, 12);
			this.m_lblNormalHint.TabIndex = 10;
			this.m_lblNormalHint.Text = "編修姿態呈現";
			// 
			// m_pnlVecSpaceColor
			// 
			this.m_pnlVecSpaceColor.BackColor = System.Drawing.Color.Orange;
			this.m_pnlVecSpaceColor.Location = new System.Drawing.Point(123, 78);
			this.m_pnlVecSpaceColor.Name = "m_pnlVecSpaceColor";
			this.m_pnlVecSpaceColor.Size = new System.Drawing.Size(14, 16);
			this.m_pnlVecSpaceColor.TabIndex = 7;
			// 
			// m_pnlNormalColor
			// 
			this.m_pnlNormalColor.BackColor = System.Drawing.Color.Red;
			this.m_pnlNormalColor.Location = new System.Drawing.Point(123, 28);
			this.m_pnlNormalColor.Name = "m_pnlNormalColor";
			this.m_pnlNormalColor.Size = new System.Drawing.Size(14, 16);
			this.m_pnlNormalColor.TabIndex = 6;
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(3, 18);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(1, 213);
			this.splitter1.TabIndex = 4;
			this.splitter1.TabStop = false;
			// 
			// m_gbxIndexParam
			// 
			this.m_gbxIndexParam.Controls.Add(this.tableLayoutPanel1);
			this.m_gbxIndexParam.Location = new System.Drawing.Point(179, 18);
			this.m_gbxIndexParam.Name = "m_gbxIndexParam";
			this.m_gbxIndexParam.Size = new System.Drawing.Size(206, 182);
			this.m_gbxIndexParam.TabIndex = 3;
			this.m_gbxIndexParam.TabStop = false;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanel1.Controls.Add(this.m_btnRotaryNeg, 2, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_lblADeg, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_lblBDeg, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_lblMaster, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_lblSlave, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_tbxSlave, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_tbxAngleA, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_btnRotaryPos, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_tbxAngleB, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_tbxMaster, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_btnAdd, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.m_btnRemove, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.m_btnKeep, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.m_btnZDir, 1, 5);
			this.tableLayoutPanel1.Controls.Add(this.m_btnRevert, 2, 5);
			this.tableLayoutPanel1.Controls.Add(this.m_btnSwitchStartEnd, 2, 4);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 16);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 9;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(185, 156);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// m_btnRotaryNeg
			// 
			this.m_btnRotaryNeg.Location = new System.Drawing.Point(123, 78);
			this.m_btnRotaryNeg.Name = "m_btnRotaryNeg";
			this.m_btnRotaryNeg.Size = new System.Drawing.Size(54, 19);
			this.m_btnRotaryNeg.TabIndex = 18;
			this.m_btnRotaryNeg.Text = "C-";
			this.m_btnRotaryNeg.UseVisualStyleBackColor = true;
			this.m_btnRotaryNeg.Click += new System.EventHandler(this.m_btnRotaryNeg_Click);
			// 
			// m_lblADeg
			// 
			this.m_lblADeg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblADeg.AutoSize = true;
			this.m_lblADeg.Location = new System.Drawing.Point(2, 0);
			this.m_lblADeg.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.m_lblADeg.Name = "m_lblADeg";
			this.m_lblADeg.Size = new System.Drawing.Size(41, 25);
			this.m_lblADeg.TabIndex = 1;
			this.m_lblADeg.Text = "前傾角";
			// 
			// m_lblBDeg
			// 
			this.m_lblBDeg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblBDeg.AutoSize = true;
			this.m_lblBDeg.Location = new System.Drawing.Point(2, 25);
			this.m_lblBDeg.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.m_lblBDeg.Name = "m_lblBDeg";
			this.m_lblBDeg.Size = new System.Drawing.Size(41, 25);
			this.m_lblBDeg.TabIndex = 4;
			this.m_lblBDeg.Text = "側傾角";
			// 
			// m_lblMaster
			// 
			this.m_lblMaster.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblMaster.AutoSize = true;
			this.m_lblMaster.Location = new System.Drawing.Point(2, 50);
			this.m_lblMaster.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.m_lblMaster.Name = "m_lblMaster";
			this.m_lblMaster.Size = new System.Drawing.Size(13, 25);
			this.m_lblMaster.TabIndex = 13;
			this.m_lblMaster.Text = "C";
			// 
			// m_lblSlave
			// 
			this.m_lblSlave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblSlave.AutoSize = true;
			this.m_lblSlave.Location = new System.Drawing.Point(2, 75);
			this.m_lblSlave.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.m_lblSlave.Name = "m_lblSlave";
			this.m_lblSlave.Size = new System.Drawing.Size(13, 25);
			this.m_lblSlave.TabIndex = 16;
			this.m_lblSlave.Text = "A";
			// 
			// m_tbxSlave
			// 
			this.m_tbxSlave.Location = new System.Drawing.Point(62, 77);
			this.m_tbxSlave.Margin = new System.Windows.Forms.Padding(2);
			this.m_tbxSlave.Name = "m_tbxSlave";
			this.m_tbxSlave.Size = new System.Drawing.Size(56, 22);
			this.m_tbxSlave.TabIndex = 14;
			this.m_tbxSlave.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxSlave_KeyDown);
			this.m_tbxSlave.Leave += new System.EventHandler(this.m_tbxSlave_Leave);
			// 
			// m_tbxAngleA
			// 
			this.m_tbxAngleA.Location = new System.Drawing.Point(62, 2);
			this.m_tbxAngleA.Margin = new System.Windows.Forms.Padding(2);
			this.m_tbxAngleA.Name = "m_tbxAngleA";
			this.m_tbxAngleA.Size = new System.Drawing.Size(56, 22);
			this.m_tbxAngleA.TabIndex = 4;
			this.m_tbxAngleA.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxAngleA_KeyDown);
			this.m_tbxAngleA.Leave += new System.EventHandler(this.m_tbxAngleA_Leave);
			// 
			// m_btnRotaryPos
			// 
			this.m_btnRotaryPos.Location = new System.Drawing.Point(123, 53);
			this.m_btnRotaryPos.Name = "m_btnRotaryPos";
			this.m_btnRotaryPos.Size = new System.Drawing.Size(54, 19);
			this.m_btnRotaryPos.TabIndex = 17;
			this.m_btnRotaryPos.Text = "C+";
			this.m_btnRotaryPos.UseVisualStyleBackColor = true;
			this.m_btnRotaryPos.Click += new System.EventHandler(this.m_btnRotaryPos_Click);
			// 
			// m_tbxAngleB
			// 
			this.m_tbxAngleB.Location = new System.Drawing.Point(62, 27);
			this.m_tbxAngleB.Margin = new System.Windows.Forms.Padding(2);
			this.m_tbxAngleB.Name = "m_tbxAngleB";
			this.m_tbxAngleB.Size = new System.Drawing.Size(56, 22);
			this.m_tbxAngleB.TabIndex = 4;
			this.m_tbxAngleB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxAngleB_KeyDown);
			this.m_tbxAngleB.Leave += new System.EventHandler(this.m_tbxAngleB_Leave);
			// 
			// m_tbxMaster
			// 
			this.m_tbxMaster.Location = new System.Drawing.Point(62, 52);
			this.m_tbxMaster.Margin = new System.Windows.Forms.Padding(2);
			this.m_tbxMaster.Name = "m_tbxMaster";
			this.m_tbxMaster.Size = new System.Drawing.Size(56, 22);
			this.m_tbxMaster.TabIndex = 15;
			this.m_tbxMaster.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxMaster_KeyDown);
			this.m_tbxMaster.Leave += new System.EventHandler(this.m_tbxMaster_Leave);
			// 
			// m_btnAdd
			// 
			this.m_btnAdd.Location = new System.Drawing.Point(3, 103);
			this.m_btnAdd.Name = "m_btnAdd";
			this.m_btnAdd.Size = new System.Drawing.Size(54, 19);
			this.m_btnAdd.TabIndex = 13;
			this.m_btnAdd.Text = "新增";
			this.m_btnAdd.UseVisualStyleBackColor = true;
			this.m_btnAdd.Click += new System.EventHandler(this.m_btnAdd_Click);
			// 
			// m_btnRemove
			// 
			this.m_btnRemove.Location = new System.Drawing.Point(63, 103);
			this.m_btnRemove.Name = "m_btnRemove";
			this.m_btnRemove.Size = new System.Drawing.Size(54, 19);
			this.m_btnRemove.TabIndex = 10;
			this.m_btnRemove.Text = "刪除";
			this.m_btnRemove.UseVisualStyleBackColor = true;
			this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
			// 
			// m_btnKeep
			// 
			this.m_btnKeep.Location = new System.Drawing.Point(3, 128);
			this.m_btnKeep.Name = "m_btnKeep";
			this.m_btnKeep.Size = new System.Drawing.Size(54, 19);
			this.m_btnKeep.TabIndex = 11;
			this.m_btnKeep.Text = "保持";
			this.m_btnKeep.UseVisualStyleBackColor = true;
			this.m_btnKeep.Click += new System.EventHandler(this.m_btnKeep_Click);
			// 
			// m_btnZDir
			// 
			this.m_btnZDir.Location = new System.Drawing.Point(63, 128);
			this.m_btnZDir.Name = "m_btnZDir";
			this.m_btnZDir.Size = new System.Drawing.Size(54, 19);
			this.m_btnZDir.TabIndex = 12;
			this.m_btnZDir.Text = "Z 方向";
			this.m_btnZDir.UseVisualStyleBackColor = true;
			this.m_btnZDir.Click += new System.EventHandler(this.m_btnZDir_Click);
			// 
			// m_btnRevert
			// 
			this.m_btnRevert.Location = new System.Drawing.Point(123, 128);
			this.m_btnRevert.Name = "m_btnRevert";
			this.m_btnRevert.Size = new System.Drawing.Size(54, 19);
			this.m_btnRevert.TabIndex = 12;
			this.m_btnRevert.Text = "原始";
			this.m_btnRevert.UseVisualStyleBackColor = true;
			this.m_btnRevert.Click += new System.EventHandler(this.m_btnRevert_Click);
			// 
			// m_btnSwitchStartEnd
			// 
			this.m_btnSwitchStartEnd.Location = new System.Drawing.Point(123, 103);
			this.m_btnSwitchStartEnd.Name = "m_btnSwitchStartEnd";
			this.m_btnSwitchStartEnd.Size = new System.Drawing.Size(54, 19);
			this.m_btnSwitchStartEnd.TabIndex = 19;
			this.m_btnSwitchStartEnd.Text = "起 / 終";
			this.m_btnSwitchStartEnd.UseVisualStyleBackColor = true;
			this.m_btnSwitchStartEnd.Click += new System.EventHandler(this.m_btnSwitchStartEnd_Click);
			// 
			// ToolVectorDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(412, 257);
			this.Controls.Add(this.m_gbxInterpolation);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ToolVectorDlg";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "姿態";
			this.m_gbxInterpolation.ResumeLayout(false);
			this.m_tbInterType.ResumeLayout(false);
			this.m_tbInterType.PerformLayout();
			this.m_gbxIndexParam.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox m_gbxInterpolation;
		private System.Windows.Forms.GroupBox m_gbxIndexParam;
		private System.Windows.Forms.Button m_btnKeep;
		private System.Windows.Forms.Button m_btnZDir;
		private System.Windows.Forms.TextBox m_tbxAngleB;
		private System.Windows.Forms.TextBox m_tbxAngleA;
		private System.Windows.Forms.Label m_lblBDeg;
		private System.Windows.Forms.Label m_lblADeg;
		private System.Windows.Forms.RadioButton m_rbtVecSpace;
		private System.Windows.Forms.RadioButton m_rbtNormal;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Button m_btnRemove;
		private System.Windows.Forms.Panel m_pnlNormalColor;
		private System.Windows.Forms.Panel m_pnlVecSpaceColor;
		private System.Windows.Forms.Label m_TiltHint;
		private System.Windows.Forms.Label m_lblVecSpaceHint;
		private System.Windows.Forms.TextBox m_tbxSlave;
		private System.Windows.Forms.TextBox m_tbxMaster;
		private System.Windows.Forms.Label m_lblSlave;
		private System.Windows.Forms.Label m_lblMaster;
		private System.Windows.Forms.Panel m_pnlTiltColor;
		private System.Windows.Forms.Label m_lblNormalHint;
		private System.Windows.Forms.RadioButton m_rbtTilt;
		private System.Windows.Forms.Button m_btnRotaryNeg;
		private System.Windows.Forms.Button m_btnRotaryPos;
		private System.Windows.Forms.TableLayoutPanel m_tbInterType;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button m_btnAdd;
		private System.Windows.Forms.Button m_btnRevert;
		private System.Windows.Forms.Button m_btnSwitchStartEnd;
		private System.Windows.Forms.Button m_btnNext;
		private System.Windows.Forms.Button m_btnPrev;
	}
}