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
			this.m_btnToStart = new System.Windows.Forms.Button();
			this.m_btnNext = new System.Windows.Forms.Button();
			this.m_btnPrev = new System.Windows.Forms.Button();
			this.m_gbxIndexParam = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblStartOrEnd = new System.Windows.Forms.Label();
			this.m_lblSetBtn = new System.Windows.Forms.Label();
			this.m_btnAdd = new System.Windows.Forms.Button();
			this.m_btnRemove = new System.Windows.Forms.Button();
			this.m_btnKeep = new System.Windows.Forms.Button();
			this.m_btnZDir = new System.Windows.Forms.Button();
			this.m_btnRevert = new System.Windows.Forms.Button();
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
			this.m_btnSwitchStartEnd = new System.Windows.Forms.Button();
			this.m_lblToolDirQuickSetBtn = new System.Windows.Forms.Label();
			this.m_lblStartEnd = new System.Windows.Forms.Label();
			this.m_lblOnePntToolVecAdjust = new System.Windows.Forms.Label();
			this.m = new System.Windows.Forms.Label();
			this.m_cbxInterpolateType = new System.Windows.Forms.ComboBox();
			this.m_btnToEnd = new System.Windows.Forms.Button();
			this.m_gbxIndexParam.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_btnToStart
			// 
			this.m_btnToStart.Location = new System.Drawing.Point(159, 372);
			this.m_btnToStart.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnToStart.Name = "m_btnToStart";
			this.m_btnToStart.Size = new System.Drawing.Size(85, 29);
			this.m_btnToStart.TabIndex = 8;
			this.m_btnToStart.Text = "至起點";
			this.m_btnToStart.UseVisualStyleBackColor = true;
			this.m_btnToStart.Click += new System.EventHandler(this.m_btnToStart_Click);
			// 
			// m_btnNext
			// 
			this.m_btnNext.Location = new System.Drawing.Point(342, 372);
			this.m_btnNext.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(80, 29);
			this.m_btnNext.TabIndex = 7;
			this.m_btnNext.Text = "後一點";
			this.m_btnNext.UseVisualStyleBackColor = true;
			this.m_btnNext.MouseDown += new System.Windows.Forms.MouseEventHandler(this.m_btnNext_MouseDown);
			this.m_btnNext.MouseUp += new System.Windows.Forms.MouseEventHandler(this.m_btnNext_MouseUp);
			// 
			// m_btnPrev
			// 
			this.m_btnPrev.Location = new System.Drawing.Point(252, 372);
			this.m_btnPrev.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnPrev.Name = "m_btnPrev";
			this.m_btnPrev.Size = new System.Drawing.Size(82, 29);
			this.m_btnPrev.TabIndex = 6;
			this.m_btnPrev.Text = "前一點";
			this.m_btnPrev.UseVisualStyleBackColor = true;
			this.m_btnPrev.MouseDown += new System.Windows.Forms.MouseEventHandler(this.m_btnPrev_MouseDown);
			this.m_btnPrev.MouseUp += new System.Windows.Forms.MouseEventHandler(this.m_btnPrev_MouseUp);
			// 
			// m_gbxIndexParam
			// 
			this.m_gbxIndexParam.Controls.Add(this.tableLayoutPanel1);
			this.m_gbxIndexParam.Location = new System.Drawing.Point(13, 13);
			this.m_gbxIndexParam.Margin = new System.Windows.Forms.Padding(4);
			this.m_gbxIndexParam.Name = "m_gbxIndexParam";
			this.m_gbxIndexParam.Padding = new System.Windows.Forms.Padding(4);
			this.m_gbxIndexParam.Size = new System.Drawing.Size(484, 351);
			this.m_gbxIndexParam.TabIndex = 3;
			this.m_gbxIndexParam.TabStop = false;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 132F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 109F));
			this.tableLayoutPanel1.Controls.Add(this.m_lblStartOrEnd, 2, 6);
			this.tableLayoutPanel1.Controls.Add(this.m_lblSetBtn, 0, 7);
			this.tableLayoutPanel1.Controls.Add(this.m_btnAdd, 1, 7);
			this.tableLayoutPanel1.Controls.Add(this.m_btnRemove, 2, 7);
			this.tableLayoutPanel1.Controls.Add(this.m_btnKeep, 2, 5);
			this.tableLayoutPanel1.Controls.Add(this.m_btnZDir, 1, 5);
			this.tableLayoutPanel1.Controls.Add(this.m_btnRevert, 3, 5);
			this.tableLayoutPanel1.Controls.Add(this.m_btnRotaryNeg, 3, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_lblADeg, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_lblBDeg, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_lblMaster, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_lblSlave, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_tbxSlave, 2, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_tbxAngleA, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_btnRotaryPos, 3, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_tbxAngleB, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_tbxMaster, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_btnSwitchStartEnd, 1, 6);
			this.tableLayoutPanel1.Controls.Add(this.m_lblToolDirQuickSetBtn, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.m_lblStartEnd, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.m_lblOnePntToolVecAdjust, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.m_cbxInterpolateType, 1, 4);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 20);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 10;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(451, 331);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// m_lblStartOrEnd
			// 
			this.m_lblStartOrEnd.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.m_lblStartOrEnd, 2);
			this.m_lblStartOrEnd.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_lblStartOrEnd.Location = new System.Drawing.Point(241, 240);
			this.m_lblStartOrEnd.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblStartOrEnd.Name = "m_lblStartOrEnd";
			this.m_lblStartOrEnd.Size = new System.Drawing.Size(206, 40);
			this.m_lblStartOrEnd.TabIndex = 1;
			this.m_lblStartOrEnd.Text = "當前位置：";
			this.m_lblStartOrEnd.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_lblSetBtn
			// 
			this.m_lblSetBtn.AutoSize = true;
			this.m_lblSetBtn.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_lblSetBtn.Location = new System.Drawing.Point(3, 280);
			this.m_lblSetBtn.Name = "m_lblSetBtn";
			this.m_lblSetBtn.Size = new System.Drawing.Size(126, 40);
			this.m_lblSetBtn.TabIndex = 21;
			this.m_lblSetBtn.Text = "姿態設置";
			this.m_lblSetBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// m_btnAdd
			// 
			this.m_btnAdd.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_btnAdd.Location = new System.Drawing.Point(136, 284);
			this.m_btnAdd.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnAdd.Name = "m_btnAdd";
			this.m_btnAdd.Size = new System.Drawing.Size(97, 32);
			this.m_btnAdd.TabIndex = 13;
			this.m_btnAdd.Text = "新增";
			this.m_btnAdd.UseVisualStyleBackColor = true;
			this.m_btnAdd.Click += new System.EventHandler(this.m_btnAdd_Click);
			// 
			// m_btnRemove
			// 
			this.m_btnRemove.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_btnRemove.Location = new System.Drawing.Point(241, 284);
			this.m_btnRemove.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnRemove.Name = "m_btnRemove";
			this.m_btnRemove.Size = new System.Drawing.Size(97, 32);
			this.m_btnRemove.TabIndex = 10;
			this.m_btnRemove.Text = "刪除";
			this.m_btnRemove.UseVisualStyleBackColor = true;
			this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
			// 
			// m_btnKeep
			// 
			this.m_btnKeep.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_btnKeep.Location = new System.Drawing.Point(241, 204);
			this.m_btnKeep.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnKeep.Name = "m_btnKeep";
			this.m_btnKeep.Size = new System.Drawing.Size(97, 32);
			this.m_btnKeep.TabIndex = 11;
			this.m_btnKeep.Text = "保持前姿態";
			this.m_btnKeep.UseVisualStyleBackColor = true;
			this.m_btnKeep.Click += new System.EventHandler(this.m_btnKeep_Click);
			// 
			// m_btnZDir
			// 
			this.m_btnZDir.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_btnZDir.Location = new System.Drawing.Point(136, 204);
			this.m_btnZDir.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnZDir.Name = "m_btnZDir";
			this.m_btnZDir.Size = new System.Drawing.Size(97, 32);
			this.m_btnZDir.TabIndex = 12;
			this.m_btnZDir.Text = "朝Z方向";
			this.m_btnZDir.UseVisualStyleBackColor = true;
			this.m_btnZDir.Click += new System.EventHandler(this.m_btnZDir_Click);
			// 
			// m_btnRevert
			// 
			this.m_btnRevert.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_btnRevert.Location = new System.Drawing.Point(346, 204);
			this.m_btnRevert.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnRevert.Name = "m_btnRevert";
			this.m_btnRevert.Size = new System.Drawing.Size(101, 32);
			this.m_btnRevert.TabIndex = 12;
			this.m_btnRevert.Text = "原始";
			this.m_btnRevert.UseVisualStyleBackColor = true;
			this.m_btnRevert.Click += new System.EventHandler(this.m_btnRevert_Click);
			// 
			// m_btnRotaryNeg
			// 
			this.m_btnRotaryNeg.Location = new System.Drawing.Point(346, 124);
			this.m_btnRotaryNeg.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnRotaryNeg.Name = "m_btnRotaryNeg";
			this.m_btnRotaryNeg.Size = new System.Drawing.Size(72, 23);
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
			this.m_lblADeg.Location = new System.Drawing.Point(135, 0);
			this.m_lblADeg.Name = "m_lblADeg";
			this.m_lblADeg.Size = new System.Drawing.Size(52, 40);
			this.m_lblADeg.TabIndex = 1;
			this.m_lblADeg.Text = "前傾角";
			// 
			// m_lblBDeg
			// 
			this.m_lblBDeg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblBDeg.AutoSize = true;
			this.m_lblBDeg.Location = new System.Drawing.Point(135, 40);
			this.m_lblBDeg.Name = "m_lblBDeg";
			this.m_lblBDeg.Size = new System.Drawing.Size(52, 40);
			this.m_lblBDeg.TabIndex = 4;
			this.m_lblBDeg.Text = "側傾角";
			// 
			// m_lblMaster
			// 
			this.m_lblMaster.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblMaster.AutoSize = true;
			this.m_lblMaster.Location = new System.Drawing.Point(135, 80);
			this.m_lblMaster.Name = "m_lblMaster";
			this.m_lblMaster.Size = new System.Drawing.Size(16, 40);
			this.m_lblMaster.TabIndex = 13;
			this.m_lblMaster.Text = "C";
			// 
			// m_lblSlave
			// 
			this.m_lblSlave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblSlave.AutoSize = true;
			this.m_lblSlave.Location = new System.Drawing.Point(135, 120);
			this.m_lblSlave.Name = "m_lblSlave";
			this.m_lblSlave.Size = new System.Drawing.Size(17, 40);
			this.m_lblSlave.TabIndex = 16;
			this.m_lblSlave.Text = "A";
			// 
			// m_tbxSlave
			// 
			this.m_tbxSlave.Location = new System.Drawing.Point(240, 122);
			this.m_tbxSlave.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.m_tbxSlave.Name = "m_tbxSlave";
			this.m_tbxSlave.Size = new System.Drawing.Size(73, 25);
			this.m_tbxSlave.TabIndex = 14;
			this.m_tbxSlave.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxSlave_KeyDown);
			this.m_tbxSlave.Leave += new System.EventHandler(this.m_tbxSlave_Leave);
			// 
			// m_tbxAngleA
			// 
			this.m_tbxAngleA.Location = new System.Drawing.Point(240, 2);
			this.m_tbxAngleA.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.m_tbxAngleA.Name = "m_tbxAngleA";
			this.m_tbxAngleA.Size = new System.Drawing.Size(73, 25);
			this.m_tbxAngleA.TabIndex = 4;
			this.m_tbxAngleA.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxAngleA_KeyDown);
			this.m_tbxAngleA.Leave += new System.EventHandler(this.m_tbxAngleA_Leave);
			// 
			// m_btnRotaryPos
			// 
			this.m_btnRotaryPos.Location = new System.Drawing.Point(346, 84);
			this.m_btnRotaryPos.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnRotaryPos.Name = "m_btnRotaryPos";
			this.m_btnRotaryPos.Size = new System.Drawing.Size(72, 23);
			this.m_btnRotaryPos.TabIndex = 17;
			this.m_btnRotaryPos.Text = "C+";
			this.m_btnRotaryPos.UseVisualStyleBackColor = true;
			this.m_btnRotaryPos.Click += new System.EventHandler(this.m_btnRotaryPos_Click);
			// 
			// m_tbxAngleB
			// 
			this.m_tbxAngleB.Location = new System.Drawing.Point(240, 42);
			this.m_tbxAngleB.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.m_tbxAngleB.Name = "m_tbxAngleB";
			this.m_tbxAngleB.Size = new System.Drawing.Size(73, 25);
			this.m_tbxAngleB.TabIndex = 4;
			this.m_tbxAngleB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxAngleB_KeyDown);
			this.m_tbxAngleB.Leave += new System.EventHandler(this.m_tbxAngleB_Leave);
			// 
			// m_tbxMaster
			// 
			this.m_tbxMaster.Location = new System.Drawing.Point(240, 82);
			this.m_tbxMaster.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.m_tbxMaster.Name = "m_tbxMaster";
			this.m_tbxMaster.Size = new System.Drawing.Size(73, 25);
			this.m_tbxMaster.TabIndex = 15;
			this.m_tbxMaster.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxMaster_KeyDown);
			this.m_tbxMaster.Leave += new System.EventHandler(this.m_tbxMaster_Leave);
			// 
			// m_btnSwitchStartEnd
			// 
			this.m_btnSwitchStartEnd.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_btnSwitchStartEnd.Location = new System.Drawing.Point(136, 244);
			this.m_btnSwitchStartEnd.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnSwitchStartEnd.Name = "m_btnSwitchStartEnd";
			this.m_btnSwitchStartEnd.Size = new System.Drawing.Size(97, 32);
			this.m_btnSwitchStartEnd.TabIndex = 19;
			this.m_btnSwitchStartEnd.Text = "起 / 終";
			this.m_btnSwitchStartEnd.UseVisualStyleBackColor = true;
			this.m_btnSwitchStartEnd.Click += new System.EventHandler(this.m_btnSwitchStartEnd_Click);
			// 
			// m_lblToolDirQuickSetBtn
			// 
			this.m_lblToolDirQuickSetBtn.AutoSize = true;
			this.m_lblToolDirQuickSetBtn.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_lblToolDirQuickSetBtn.Location = new System.Drawing.Point(3, 200);
			this.m_lblToolDirQuickSetBtn.Name = "m_lblToolDirQuickSetBtn";
			this.m_lblToolDirQuickSetBtn.Size = new System.Drawing.Size(126, 40);
			this.m_lblToolDirQuickSetBtn.TabIndex = 20;
			this.m_lblToolDirQuickSetBtn.Text = "姿態方向快捷鍵";
			this.m_lblToolDirQuickSetBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// m_lblStartEnd
			// 
			this.m_lblStartEnd.AutoSize = true;
			this.m_lblStartEnd.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_lblStartEnd.Location = new System.Drawing.Point(3, 240);
			this.m_lblStartEnd.Name = "m_lblStartEnd";
			this.m_lblStartEnd.Size = new System.Drawing.Size(126, 40);
			this.m_lblStartEnd.TabIndex = 22;
			this.m_lblStartEnd.Text = "起終點切換";
			this.m_lblStartEnd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// m_lblOnePntToolVecAdjust
			// 
			this.m_lblOnePntToolVecAdjust.AutoSize = true;
			this.m_lblOnePntToolVecAdjust.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_lblOnePntToolVecAdjust.Location = new System.Drawing.Point(3, 0);
			this.m_lblOnePntToolVecAdjust.Name = "m_lblOnePntToolVecAdjust";
			this.m_lblOnePntToolVecAdjust.Size = new System.Drawing.Size(126, 40);
			this.m_lblOnePntToolVecAdjust.TabIndex = 23;
			this.m_lblOnePntToolVecAdjust.Text = "單點姿態資訊";
			this.m_lblOnePntToolVecAdjust.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// m
			// 
			this.m.AutoSize = true;
			this.m.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m.Location = new System.Drawing.Point(3, 160);
			this.m.Name = "m";
			this.m.Size = new System.Drawing.Size(126, 40);
			this.m.TabIndex = 24;
			this.m.Text = "插值方案";
			this.m.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// m_cbxInterpolateType
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.m_cbxInterpolateType, 2);
			this.m_cbxInterpolateType.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_cbxInterpolateType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cbxInterpolateType.FormattingEnabled = true;
			this.m_cbxInterpolateType.Items.AddRange(new object[] {
            "曲面法向量",
            "旋轉軸插值",
            "傾角插值",
            "主軸法向量且從軸插值",
            "從軸法向量且主軸插值"});
			this.m_cbxInterpolateType.Location = new System.Drawing.Point(135, 163);
			this.m_cbxInterpolateType.Name = "m_cbxInterpolateType";
			this.m_cbxInterpolateType.Size = new System.Drawing.Size(204, 23);
			this.m_cbxInterpolateType.TabIndex = 25;
			this.m_cbxInterpolateType.SelectedIndexChanged += new System.EventHandler(this.m_cbxInterpolateType_SelectedIndexChanged);
			// 
			// m_btnToEnd
			// 
			this.m_btnToEnd.Location = new System.Drawing.Point(423, 372);
			this.m_btnToEnd.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnToEnd.Name = "m_btnToEnd";
			this.m_btnToEnd.Size = new System.Drawing.Size(74, 29);
			this.m_btnToEnd.TabIndex = 9;
			this.m_btnToEnd.Text = "至終點";
			this.m_btnToEnd.UseVisualStyleBackColor = true;
			this.m_btnToEnd.Click += new System.EventHandler(this.m_btnToEnd_Click);
			// 
			// ToolVectorDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(521, 420);
			this.Controls.Add(this.m_btnToEnd);
			this.Controls.Add(this.m_btnNext);
			this.Controls.Add(this.m_btnToStart);
			this.Controls.Add(this.m_btnPrev);
			this.Controls.Add(this.m_gbxIndexParam);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ToolVectorDlg";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "姿態";
			this.TopMost = true;
			this.m_gbxIndexParam.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.GroupBox m_gbxIndexParam;
		private System.Windows.Forms.Button m_btnKeep;
		private System.Windows.Forms.Button m_btnZDir;
		private System.Windows.Forms.TextBox m_tbxAngleB;
		private System.Windows.Forms.TextBox m_tbxAngleA;
		private System.Windows.Forms.Label m_lblBDeg;
		private System.Windows.Forms.Label m_lblADeg;
		private System.Windows.Forms.Button m_btnRemove;
		private System.Windows.Forms.TextBox m_tbxSlave;
		private System.Windows.Forms.TextBox m_tbxMaster;
		private System.Windows.Forms.Label m_lblSlave;
		private System.Windows.Forms.Label m_lblMaster;
		private System.Windows.Forms.Button m_btnRotaryNeg;
		private System.Windows.Forms.Button m_btnRotaryPos;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button m_btnAdd;
		private System.Windows.Forms.Button m_btnRevert;
		private System.Windows.Forms.Button m_btnSwitchStartEnd;
		private System.Windows.Forms.Button m_btnNext;
		private System.Windows.Forms.Button m_btnPrev;
		private System.Windows.Forms.Button m_btnToStart;
		private System.Windows.Forms.Label m_lblStartOrEnd;
		private System.Windows.Forms.Label m_lblToolDirQuickSetBtn;
		private System.Windows.Forms.Label m_lblSetBtn;
		private System.Windows.Forms.Label m_lblStartEnd;
		private System.Windows.Forms.Label m_lblOnePntToolVecAdjust;
		private System.Windows.Forms.Label m;
		private System.Windows.Forms.ComboBox m_cbxInterpolateType;
		private System.Windows.Forms.Button m_btnToEnd;
	}
}
