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
			this.m_pnlIndexParam = new System.Windows.Forms.Panel();
			this.m_pnlToolVecInfo = new CommonUI.Controls.SectionPanel();
			this.m_lblADeg = new CommonUI.Controls.ThemedLabel();
			this.m_numAngleA = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblBDeg = new CommonUI.Controls.ThemedLabel();
			this.m_numAngleB = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblMaster = new CommonUI.Controls.ThemedLabel();
			this.m_numMaster = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblSlave = new CommonUI.Controls.ThemedLabel();
			this.m_numSlave = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_btnRotaryPos = new CommonUI.Controls.ThemedButton();
			this.m_btnRotaryNeg = new CommonUI.Controls.ThemedButton();
			this.m_lblInterpolateType = new CommonUI.Controls.ThemedLabel();
			this.m_cmbInterpolateType = new System.Windows.Forms.ComboBox();
			this.m_pnlQuickSet = new CommonUI.Controls.SectionPanel();
			this.m_btnZDir = new CommonUI.Controls.ThemedButton();
			this.m_btnKeep = new CommonUI.Controls.ThemedButton();
			this.m_btnRevert = new CommonUI.Controls.ThemedButton();
			this.m_pnlSetting = new CommonUI.Controls.SectionPanel();
			this.m_btnAdd = new CommonUI.Controls.ThemedButton();
			this.m_btnRemove = new CommonUI.Controls.ThemedButton();
			this.m_btnClear = new CommonUI.Controls.ThemedButton();
			this.m_pnlMovePnt = new CommonUI.Controls.SectionPanel();
			this.m_lblStartPnt = new CommonUI.Controls.ThemedLabel();
			this.m_btnToStart = new CommonUI.Controls.ThemedButton();
			this.m_btnToEnd = new CommonUI.Controls.ThemedButton();
			this.m_pnlStartEndSwitch = new System.Windows.Forms.Panel();
			this.m_lblStartEnd = new CommonUI.Controls.ThemedLabel();
			this.m_btnSwitchStartEnd = new CommonUI.Controls.ThemedButton();
			this.m_lblStartOrEnd = new CommonUI.Controls.ThemedLabel();
			this.m_lblPnt = new CommonUI.Controls.ThemedLabel();
			this.m_btnPrev = new CommonUI.Controls.ThemedButton();
			this.m_btnNext = new CommonUI.Controls.ThemedButton();
			this.m_lblCtrlPnt = new CommonUI.Controls.ThemedLabel();
			this.m_btnPreCtrlPnt = new CommonUI.Controls.ThemedButton();
			this.m_btnNextCtrlPnt = new CommonUI.Controls.ThemedButton();
			this.m_pnlColorNormal = new System.Windows.Forms.Panel();
			this.m_lblLegendNormal = new CommonUI.Controls.ThemedLabel();
			this.m_pnlColorRotary = new System.Windows.Forms.Panel();
			this.m_lblLegendRotary = new CommonUI.Controls.ThemedLabel();
			this.m_pnlColorBevel = new System.Windows.Forms.Panel();
			this.m_lblLegendBevel = new CommonUI.Controls.ThemedLabel();
			this.m_pnlColorMaster = new System.Windows.Forms.Panel();
			this.m_lblLegendMaster = new CommonUI.Controls.ThemedLabel();
			this.m_pnlColorSlave = new System.Windows.Forms.Panel();
			this.m_lblLegendSlave = new CommonUI.Controls.ThemedLabel();
			this.m_pnlIndexParam.SuspendLayout();
			this.m_pnlToolVecInfo.SuspendLayout();
			this.m_pnlQuickSet.SuspendLayout();
			this.m_pnlSetting.SuspendLayout();
			this.m_pnlMovePnt.SuspendLayout();
			this.m_pnlStartEndSwitch.SuspendLayout();
			this.m_pnlColorNormal.SuspendLayout();
			this.m_pnlColorRotary.SuspendLayout();
			this.m_pnlColorBevel.SuspendLayout();
			this.m_pnlColorMaster.SuspendLayout();
			this.m_pnlColorSlave.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numAngleA)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numAngleB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMaster)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numSlave)).BeginInit();
			this.SuspendLayout();
			// 
			// m_pnlIndexParam
			// 
			this.m_pnlIndexParam.Controls.Add(this.m_pnlToolVecInfo);
			this.m_pnlIndexParam.Controls.Add(this.m_pnlQuickSet);
			this.m_pnlIndexParam.Controls.Add(this.m_pnlSetting);
			this.m_pnlIndexParam.Location = new System.Drawing.Point(12, 12);
			this.m_pnlIndexParam.Name = "m_pnlIndexParam";
			this.m_pnlIndexParam.Size = new System.Drawing.Size(452, 373);
			this.m_pnlIndexParam.TabIndex = 0;
			// 
			// m_pnlToolVecInfo
			// 
			this.m_pnlToolVecInfo.Controls.Add(this.m_lblADeg);
			this.m_pnlToolVecInfo.Controls.Add(this.m_numAngleA);
			this.m_pnlToolVecInfo.Controls.Add(this.m_lblBDeg);
			this.m_pnlToolVecInfo.Controls.Add(this.m_numAngleB);
			this.m_pnlToolVecInfo.Controls.Add(this.m_lblMaster);
			this.m_pnlToolVecInfo.Controls.Add(this.m_numMaster);
			this.m_pnlToolVecInfo.Controls.Add(this.m_lblSlave);
			this.m_pnlToolVecInfo.Controls.Add(this.m_numSlave);
			this.m_pnlToolVecInfo.Controls.Add(this.m_btnRotaryPos);
			this.m_pnlToolVecInfo.Controls.Add(this.m_btnRotaryNeg);
			this.m_pnlToolVecInfo.Controls.Add(this.m_lblInterpolateType);
			this.m_pnlToolVecInfo.Controls.Add(this.m_cmbInterpolateType);
			this.m_pnlToolVecInfo.Controls.Add(this.m_pnlColorNormal);
			this.m_pnlToolVecInfo.Controls.Add(this.m_lblLegendNormal);
			this.m_pnlToolVecInfo.Controls.Add(this.m_pnlColorRotary);
			this.m_pnlToolVecInfo.Controls.Add(this.m_lblLegendRotary);
			this.m_pnlToolVecInfo.Controls.Add(this.m_pnlColorBevel);
			this.m_pnlToolVecInfo.Controls.Add(this.m_lblLegendBevel);
			this.m_pnlToolVecInfo.Controls.Add(this.m_pnlColorMaster);
			this.m_pnlToolVecInfo.Controls.Add(this.m_lblLegendMaster);
			this.m_pnlToolVecInfo.Controls.Add(this.m_pnlColorSlave);
			this.m_pnlToolVecInfo.Controls.Add(this.m_lblLegendSlave);
			this.m_pnlToolVecInfo.Location = new System.Drawing.Point(0, 0);
			this.m_pnlToolVecInfo.Name = "m_pnlToolVecInfo";
			this.m_pnlToolVecInfo.Size = new System.Drawing.Size(452, 219);
			this.m_pnlToolVecInfo.TabIndex = 0;
			this.m_pnlToolVecInfo.Text = "單點姿態資訊";
			// 
			// m_lblADeg
			// 
			this.m_lblADeg.AutoSize = true;
			this.m_lblADeg.Location = new System.Drawing.Point(10, 40);
			this.m_lblADeg.Name = "m_lblADeg";
			this.m_lblADeg.Size = new System.Drawing.Size(43, 16);
			this.m_lblADeg.TabIndex = 0;
			this.m_lblADeg.Text = "前傾角";
			// 
			// m_numAngleA
			// 
			this.m_numAngleA.DecimalPlaces = 3;
			this.m_numAngleA.Location = new System.Drawing.Point(100, 36);
			this.m_numAngleA.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numAngleA.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_numAngleA.Name = "m_numAngleA";
			this.m_numAngleA.RestrictInvalidCharacters = false;
			this.m_numAngleA.Size = new System.Drawing.Size(120, 23);
			this.m_numAngleA.TabIndex = 1;
			this.m_numAngleA.DebouncedValueChanged += new System.EventHandler(this.m_numAngleA_DebouncedValueChanged);
			// 
			// m_lblBDeg
			// 
			this.m_lblBDeg.AutoSize = true;
			this.m_lblBDeg.Location = new System.Drawing.Point(236, 40);
			this.m_lblBDeg.Name = "m_lblBDeg";
			this.m_lblBDeg.Size = new System.Drawing.Size(43, 16);
			this.m_lblBDeg.TabIndex = 2;
			this.m_lblBDeg.Text = "側傾角";
			// 
			// m_numAngleB
			// 
			this.m_numAngleB.DecimalPlaces = 3;
			this.m_numAngleB.Location = new System.Drawing.Point(322, 36);
			this.m_numAngleB.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numAngleB.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_numAngleB.Name = "m_numAngleB";
			this.m_numAngleB.RestrictInvalidCharacters = false;
			this.m_numAngleB.Size = new System.Drawing.Size(120, 23);
			this.m_numAngleB.TabIndex = 3;
			this.m_numAngleB.DebouncedValueChanged += new System.EventHandler(this.m_numAngleB_DebouncedValueChanged);
			// 
			// m_lblMaster
			// 
			this.m_lblMaster.AutoSize = true;
			this.m_lblMaster.Location = new System.Drawing.Point(10, 70);
			this.m_lblMaster.Name = "m_lblMaster";
			this.m_lblMaster.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblMaster.Size = new System.Drawing.Size(15, 16);
			this.m_lblMaster.TabIndex = 4;
			this.m_lblMaster.Text = "C";
			// 
			// m_numMaster
			// 
			this.m_numMaster.DecimalPlaces = 3;
			this.m_numMaster.Location = new System.Drawing.Point(100, 66);
			this.m_numMaster.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numMaster.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_numMaster.Name = "m_numMaster";
			this.m_numMaster.RestrictInvalidCharacters = false;
			this.m_numMaster.Size = new System.Drawing.Size(120, 23);
			this.m_numMaster.TabIndex = 5;
			this.m_numMaster.DebouncedValueChanged += new System.EventHandler(this.m_numMaster_DebouncedValueChanged);
			// 
			// m_lblSlave
			// 
			this.m_lblSlave.AutoSize = true;
			this.m_lblSlave.Location = new System.Drawing.Point(236, 70);
			this.m_lblSlave.Name = "m_lblSlave";
			this.m_lblSlave.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblSlave.Size = new System.Drawing.Size(15, 16);
			this.m_lblSlave.TabIndex = 6;
			this.m_lblSlave.Text = "A";
			// 
			// m_numSlave
			// 
			this.m_numSlave.DecimalPlaces = 3;
			this.m_numSlave.Location = new System.Drawing.Point(322, 66);
			this.m_numSlave.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numSlave.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_numSlave.Name = "m_numSlave";
			this.m_numSlave.RestrictInvalidCharacters = false;
			this.m_numSlave.Size = new System.Drawing.Size(120, 23);
			this.m_numSlave.TabIndex = 7;
			this.m_numSlave.DebouncedValueChanged += new System.EventHandler(this.m_numSlave_DebouncedValueChanged);
			// 
			// m_btnRotaryPos
			// 
			this.m_btnRotaryPos.Location = new System.Drawing.Point(100, 96);
			this.m_btnRotaryPos.Name = "m_btnRotaryPos";
			this.m_btnRotaryPos.Size = new System.Drawing.Size(120, 23);
			this.m_btnRotaryPos.TabIndex = 8;
			this.m_btnRotaryPos.Text = "C+";
			this.m_btnRotaryPos.Click += new System.EventHandler(this.m_btnRotaryPos_Click);
			// 
			// m_btnRotaryNeg
			// 
			this.m_btnRotaryNeg.Location = new System.Drawing.Point(322, 96);
			this.m_btnRotaryNeg.Name = "m_btnRotaryNeg";
			this.m_btnRotaryNeg.Size = new System.Drawing.Size(120, 23);
			this.m_btnRotaryNeg.TabIndex = 9;
			this.m_btnRotaryNeg.Text = "C-";
			this.m_btnRotaryNeg.Click += new System.EventHandler(this.m_btnRotaryNeg_Click);
			// 
			// m_lblInterpolateType
			// 
			this.m_lblInterpolateType.AutoSize = true;
			this.m_lblInterpolateType.Location = new System.Drawing.Point(10, 130);
			this.m_lblInterpolateType.Name = "m_lblInterpolateType";
			this.m_lblInterpolateType.Size = new System.Drawing.Size(55, 16);
			this.m_lblInterpolateType.TabIndex = 10;
			this.m_lblInterpolateType.Text = "插補方案";
			// 
			// m_cmbInterpolateType
			// 
			this.m_cmbInterpolateType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbInterpolateType.FormattingEnabled = true;
			this.m_cmbInterpolateType.Location = new System.Drawing.Point(100, 126);
			this.m_cmbInterpolateType.Name = "m_cmbInterpolateType";
			this.m_cmbInterpolateType.Size = new System.Drawing.Size(120, 24);
			this.m_cmbInterpolateType.TabIndex = 11;
			this.m_cmbInterpolateType.Items.AddRange(new object[] { "維持默認", "旋轉軸平滑", "傾角切坡口", "從軸平滑", "主軸平滑"});
			this.m_cmbInterpolateType.SelectedIndexChanged += new System.EventHandler(this.m_cmbInterpolateType_SelectedIndexChanged);
			// 
			// m_pnlQuickSet
			// 
			this.m_pnlQuickSet.Controls.Add(this.m_btnZDir);
			this.m_pnlQuickSet.Controls.Add(this.m_btnKeep);
			this.m_pnlQuickSet.Controls.Add(this.m_btnRevert);
			this.m_pnlQuickSet.Location = new System.Drawing.Point(0, 227);
			this.m_pnlQuickSet.Name = "m_pnlQuickSet";
			this.m_pnlQuickSet.Size = new System.Drawing.Size(452, 69);
			this.m_pnlQuickSet.TabIndex = 1;
			this.m_pnlQuickSet.Text = "姿態方向快捷鍵";
			// 
			// m_btnZDir
			// 
			this.m_btnZDir.Location = new System.Drawing.Point(10, 36);
			this.m_btnZDir.Name = "m_btnZDir";
			this.m_btnZDir.Size = new System.Drawing.Size(100, 23);
			this.m_btnZDir.TabIndex = 0;
			this.m_btnZDir.Text = "朝Z方向";
			this.m_btnZDir.Click += new System.EventHandler(this.m_btnZDir_Click);
			// 
			// m_btnKeep
			// 
			this.m_btnKeep.Location = new System.Drawing.Point(118, 36);
			this.m_btnKeep.Name = "m_btnKeep";
			this.m_btnKeep.Size = new System.Drawing.Size(100, 23);
			this.m_btnKeep.TabIndex = 1;
			this.m_btnKeep.Text = "保持前姿態";
			this.m_btnKeep.Click += new System.EventHandler(this.m_btnKeep_Click);
			// 
			// m_btnRevert
			// 
			this.m_btnRevert.Location = new System.Drawing.Point(226, 36);
			this.m_btnRevert.Name = "m_btnRevert";
			this.m_btnRevert.Size = new System.Drawing.Size(100, 23);
			this.m_btnRevert.TabIndex = 2;
			this.m_btnRevert.Text = "原始";
			this.m_btnRevert.Click += new System.EventHandler(this.m_btnRevert_Click);
			//
			// m_pnlSetting
			//
			this.m_pnlSetting.Controls.Add(this.m_btnAdd);
			this.m_pnlSetting.Controls.Add(this.m_btnRemove);
			this.m_pnlSetting.Controls.Add(this.m_btnClear);
			this.m_pnlSetting.Location = new System.Drawing.Point(0, 304);
			this.m_pnlSetting.Name = "m_pnlSetting";
			this.m_pnlSetting.Size = new System.Drawing.Size(452, 69);
			this.m_pnlSetting.TabIndex = 2;
			this.m_pnlSetting.Text = "姿態設置";
			// 
			// m_btnAdd
			// 
			this.m_btnAdd.Location = new System.Drawing.Point(10, 36);
			this.m_btnAdd.Name = "m_btnAdd";
			this.m_btnAdd.Size = new System.Drawing.Size(100, 23);
			this.m_btnAdd.TabIndex = 0;
			this.m_btnAdd.Text = "新增";
			this.m_btnAdd.Click += new System.EventHandler(this.m_btnAdd_Click);
			// 
			// m_btnRemove
			// 
			this.m_btnRemove.Kind = CommonUI.Controls.ButtonKind.Danger;
			this.m_btnRemove.Location = new System.Drawing.Point(118, 36);
			this.m_btnRemove.Name = "m_btnRemove";
			this.m_btnRemove.Size = new System.Drawing.Size(100, 23);
			this.m_btnRemove.TabIndex = 1;
			this.m_btnRemove.Text = "刪除";
			this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
			// 
			// m_btnClear
			// 
			this.m_btnClear.Kind = CommonUI.Controls.ButtonKind.Danger;
			this.m_btnClear.Location = new System.Drawing.Point(226, 36);
			this.m_btnClear.Name = "m_btnClear";
			this.m_btnClear.Size = new System.Drawing.Size(140, 23);
			this.m_btnClear.TabIndex = 2;
			this.m_btnClear.Text = "清除所有控制點";
			this.m_btnClear.Click += new System.EventHandler(this.m_btnClear_Click);
			//
			// m_pnlMovePnt
			//
			this.m_pnlMovePnt.Controls.Add(this.m_lblStartPnt);
			this.m_pnlMovePnt.Controls.Add(this.m_btnToStart);
			this.m_pnlMovePnt.Controls.Add(this.m_btnToEnd);
			this.m_pnlMovePnt.Controls.Add(this.m_pnlStartEndSwitch);
			this.m_pnlMovePnt.Controls.Add(this.m_lblPnt);
			this.m_pnlMovePnt.Controls.Add(this.m_btnPrev);
			this.m_pnlMovePnt.Controls.Add(this.m_btnNext);
			this.m_pnlMovePnt.Controls.Add(this.m_lblCtrlPnt);
			this.m_pnlMovePnt.Controls.Add(this.m_btnPreCtrlPnt);
			this.m_pnlMovePnt.Controls.Add(this.m_btnNextCtrlPnt);
			this.m_pnlMovePnt.Location = new System.Drawing.Point(12, 393);
			this.m_pnlMovePnt.Name = "m_pnlMovePnt";
			this.m_pnlMovePnt.Size = new System.Drawing.Size(452, 159);
			this.m_pnlMovePnt.TabIndex = 1;
			this.m_pnlMovePnt.Text = "控制點移動";
			//
			// m_lblStartPnt
			//
			this.m_lblStartPnt.AutoSize = true;
			this.m_lblStartPnt.Location = new System.Drawing.Point(10, 40);
			this.m_lblStartPnt.Name = "m_lblStartPnt";
			this.m_lblStartPnt.Size = new System.Drawing.Size(67, 16);
			this.m_lblStartPnt.TabIndex = 0;
			this.m_lblStartPnt.Text = "起終點轉換";
			//
			// m_btnToStart
			//
			this.m_btnToStart.Location = new System.Drawing.Point(100, 36);
			this.m_btnToStart.Name = "m_btnToStart";
			this.m_btnToStart.Size = new System.Drawing.Size(100, 23);
			this.m_btnToStart.TabIndex = 1;
			this.m_btnToStart.Text = "至起點";
			this.m_btnToStart.Click += new System.EventHandler(this.m_btnToStart_Click);
			//
			// m_btnToEnd
			//
			this.m_btnToEnd.Location = new System.Drawing.Point(208, 36);
			this.m_btnToEnd.Name = "m_btnToEnd";
			this.m_btnToEnd.Size = new System.Drawing.Size(100, 23);
			this.m_btnToEnd.TabIndex = 2;
			this.m_btnToEnd.Text = "至終點";
			this.m_btnToEnd.Click += new System.EventHandler(this.m_btnToEnd_Click);
			//
			// m_pnlStartEndSwitch
			//
			this.m_pnlStartEndSwitch.Controls.Add(this.m_lblStartEnd);
			this.m_pnlStartEndSwitch.Controls.Add(this.m_btnSwitchStartEnd);
			this.m_pnlStartEndSwitch.Controls.Add(this.m_lblStartOrEnd);
			this.m_pnlStartEndSwitch.Location = new System.Drawing.Point(10, 66);
			this.m_pnlStartEndSwitch.Name = "m_pnlStartEndSwitch";
			this.m_pnlStartEndSwitch.Size = new System.Drawing.Size(432, 23);
			this.m_pnlStartEndSwitch.TabIndex = 3;
			//
			// m_lblStartEnd
			//
			this.m_lblStartEnd.AutoSize = true;
			this.m_lblStartEnd.Location = new System.Drawing.Point(0, 4);
			this.m_lblStartEnd.Name = "m_lblStartEnd";
			this.m_lblStartEnd.Size = new System.Drawing.Size(67, 16);
			this.m_lblStartEnd.TabIndex = 0;
			this.m_lblStartEnd.Text = "起終點切換";
			//
			// m_btnSwitchStartEnd
			//
			this.m_btnSwitchStartEnd.Location = new System.Drawing.Point(90, 0);
			this.m_btnSwitchStartEnd.Name = "m_btnSwitchStartEnd";
			this.m_btnSwitchStartEnd.Size = new System.Drawing.Size(100, 23);
			this.m_btnSwitchStartEnd.TabIndex = 1;
			this.m_btnSwitchStartEnd.Text = "起/終";
			this.m_btnSwitchStartEnd.Click += new System.EventHandler(this.m_btnSwitchStartEnd_Click);
			//
			// m_lblStartOrEnd
			//
			this.m_lblStartOrEnd.AutoSize = true;
			this.m_lblStartOrEnd.Location = new System.Drawing.Point(198, 4);
			this.m_lblStartOrEnd.Name = "m_lblStartOrEnd";
			this.m_lblStartOrEnd.Size = new System.Drawing.Size(67, 16);
			this.m_lblStartOrEnd.TabIndex = 2;
			this.m_lblStartOrEnd.Text = "當前位置：";
			//
			// m_lblPnt
			//
			this.m_lblPnt.AutoSize = true;
			this.m_lblPnt.Location = new System.Drawing.Point(10, 100);
			this.m_lblPnt.Name = "m_lblPnt";
			this.m_lblPnt.Size = new System.Drawing.Size(67, 16);
			this.m_lblPnt.TabIndex = 4;
			this.m_lblPnt.Text = "前後點轉換";
			//
			// m_btnPrev
			//
			this.m_btnPrev.Location = new System.Drawing.Point(100, 96);
			this.m_btnPrev.Name = "m_btnPrev";
			this.m_btnPrev.Size = new System.Drawing.Size(100, 23);
			this.m_btnPrev.TabIndex = 5;
			this.m_btnPrev.Text = "前一點";
			this.m_btnPrev.MouseDown += new System.Windows.Forms.MouseEventHandler(this.m_btnPrev_MouseDown);
			this.m_btnPrev.MouseUp += new System.Windows.Forms.MouseEventHandler(this.m_btnPrev_MouseUp);
			// 
			// m_btnNext
			// 
			this.m_btnNext.Location = new System.Drawing.Point(208, 96);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(100, 23);
			this.m_btnNext.TabIndex = 6;
			this.m_btnNext.Text = "後一點";
			this.m_btnNext.MouseDown += new System.Windows.Forms.MouseEventHandler(this.m_btnNext_MouseDown);
			this.m_btnNext.MouseUp += new System.Windows.Forms.MouseEventHandler(this.m_btnNext_MouseUp);
			//
			// m_lblCtrlPnt
			//
			this.m_lblCtrlPnt.AutoSize = true;
			this.m_lblCtrlPnt.Location = new System.Drawing.Point(10, 130);
			this.m_lblCtrlPnt.Name = "m_lblCtrlPnt";
			this.m_lblCtrlPnt.Size = new System.Drawing.Size(67, 16);
			this.m_lblCtrlPnt.TabIndex = 7;
			this.m_lblCtrlPnt.Text = "控制點轉換";
			//
			// m_btnPreCtrlPnt
			//
			this.m_btnPreCtrlPnt.Location = new System.Drawing.Point(100, 126);
			this.m_btnPreCtrlPnt.Name = "m_btnPreCtrlPnt";
			this.m_btnPreCtrlPnt.Size = new System.Drawing.Size(100, 23);
			this.m_btnPreCtrlPnt.TabIndex = 8;
			this.m_btnPreCtrlPnt.Text = "前控制點";
			this.m_btnPreCtrlPnt.Click += new System.EventHandler(this.m_btnPreCtrlPnt_Click);
			// 
			// m_btnNextCtrlPnt
			// 
			this.m_btnNextCtrlPnt.Location = new System.Drawing.Point(208, 126);
			this.m_btnNextCtrlPnt.Name = "m_btnNextCtrlPnt";
			this.m_btnNextCtrlPnt.Size = new System.Drawing.Size(100, 23);
			this.m_btnNextCtrlPnt.TabIndex = 9;
			this.m_btnNextCtrlPnt.Text = "後控制點";
			this.m_btnNextCtrlPnt.Click += new System.EventHandler(this.m_btnNextCtrlPnt_Click);
			//
			// m_pnlColorNormal
			//
			this.m_pnlColorNormal.BackColor = System.Drawing.Color.Blue;
			this.m_pnlColorNormal.Location = new System.Drawing.Point(100, 159);
			this.m_pnlColorNormal.Name = "m_pnlColorNormal";
			this.m_pnlColorNormal.Size = new System.Drawing.Size(16, 16);
			this.m_pnlColorNormal.TabIndex = 12;
			//
			// m_lblLegendNormal
			//
			this.m_lblLegendNormal.AutoSize = true;
			this.m_lblLegendNormal.Location = new System.Drawing.Point(122, 160);
			this.m_lblLegendNormal.Name = "m_lblLegendNormal";
			this.m_lblLegendNormal.Size = new System.Drawing.Size(31, 16);
			this.m_lblLegendNormal.TabIndex = 13;
			this.m_lblLegendNormal.Text = "維持";
			//
			// m_pnlColorRotary
			//
			this.m_pnlColorRotary.BackColor = System.Drawing.Color.Orange;
			this.m_pnlColorRotary.Location = new System.Drawing.Point(214, 159);
			this.m_pnlColorRotary.Name = "m_pnlColorRotary";
			this.m_pnlColorRotary.Size = new System.Drawing.Size(16, 16);
			this.m_pnlColorRotary.TabIndex = 14;
			//
			// m_lblLegendRotary
			//
			this.m_lblLegendRotary.AutoSize = true;
			this.m_lblLegendRotary.Location = new System.Drawing.Point(236, 160);
			this.m_lblLegendRotary.Name = "m_lblLegendRotary";
			this.m_lblLegendRotary.Size = new System.Drawing.Size(79, 16);
			this.m_lblLegendRotary.TabIndex = 15;
			this.m_lblLegendRotary.Text = "旋轉軸平滑";
			//
			// m_pnlColorBevel
			//
			this.m_pnlColorBevel.BackColor = System.Drawing.Color.Cyan;
			this.m_pnlColorBevel.Location = new System.Drawing.Point(328, 159);
			this.m_pnlColorBevel.Name = "m_pnlColorBevel";
			this.m_pnlColorBevel.Size = new System.Drawing.Size(16, 16);
			this.m_pnlColorBevel.TabIndex = 16;
			//
			// m_lblLegendBevel
			//
			this.m_lblLegendBevel.AutoSize = true;
			this.m_lblLegendBevel.Location = new System.Drawing.Point(350, 160);
			this.m_lblLegendBevel.Name = "m_lblLegendBevel";
			this.m_lblLegendBevel.Size = new System.Drawing.Size(79, 16);
			this.m_lblLegendBevel.TabIndex = 17;
			this.m_lblLegendBevel.Text = "傾角切坡口";
			//
			// m_pnlColorMaster
			//
			this.m_pnlColorMaster.BackColor = System.Drawing.Color.Magenta;
			this.m_pnlColorMaster.Location = new System.Drawing.Point(100, 189);
			this.m_pnlColorMaster.Name = "m_pnlColorMaster";
			this.m_pnlColorMaster.Size = new System.Drawing.Size(16, 16);
			this.m_pnlColorMaster.TabIndex = 18;
			//
			// m_lblLegendMaster
			//
			this.m_lblLegendMaster.AutoSize = true;
			this.m_lblLegendMaster.Location = new System.Drawing.Point(122, 190);
			this.m_lblLegendMaster.Name = "m_lblLegendMaster";
			this.m_lblLegendMaster.Size = new System.Drawing.Size(31, 16);
			this.m_lblLegendMaster.TabIndex = 19;
			this.m_lblLegendMaster.Text = "主軸";
			//
			// m_pnlColorSlave
			//
			this.m_pnlColorSlave.BackColor = System.Drawing.Color.Yellow;
			this.m_pnlColorSlave.Location = new System.Drawing.Point(214, 189);
			this.m_pnlColorSlave.Name = "m_pnlColorSlave";
			this.m_pnlColorSlave.Size = new System.Drawing.Size(16, 16);
			this.m_pnlColorSlave.TabIndex = 20;
			//
			// m_lblLegendSlave
			//
			this.m_lblLegendSlave.AutoSize = true;
			this.m_lblLegendSlave.Location = new System.Drawing.Point(236, 190);
			this.m_lblLegendSlave.Name = "m_lblLegendSlave";
			this.m_lblLegendSlave.Size = new System.Drawing.Size(31, 16);
			this.m_lblLegendSlave.TabIndex = 21;
			this.m_lblLegendSlave.Text = "從軸";
			//
			// ToolVectorDlg
			//
			this.ClientSize = new System.Drawing.Size(476, 564);
			this.Controls.Add(this.m_pnlIndexParam);
			this.Controls.Add(this.m_pnlMovePnt);
			this.Name = "ToolVectorDlg";
			this.Text = "飛邊姿態";
			this.m_pnlIndexParam.ResumeLayout(false);
			this.m_pnlToolVecInfo.ResumeLayout(false);
			this.m_pnlToolVecInfo.PerformLayout();
			this.m_pnlQuickSet.ResumeLayout(false);
			this.m_pnlSetting.ResumeLayout(false);
			this.m_pnlMovePnt.ResumeLayout(false);
			this.m_pnlMovePnt.PerformLayout();
			this.m_pnlStartEndSwitch.ResumeLayout(false);
			this.m_pnlStartEndSwitch.PerformLayout();
			this.m_pnlColorNormal.ResumeLayout(false);
			this.m_pnlColorRotary.ResumeLayout(false);
			this.m_pnlColorBevel.ResumeLayout(false);
			this.m_pnlColorMaster.ResumeLayout(false);
			this.m_pnlColorSlave.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_numAngleA)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numAngleB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMaster)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numSlave)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel m_pnlIndexParam;
		private CommonUI.Controls.SectionPanel m_pnlToolVecInfo;
		private CommonUI.Controls.ThemedLabel m_lblADeg;
		private CommonUI.Controls.DebouncedNumericUpDown m_numAngleA;
		private CommonUI.Controls.ThemedLabel m_lblBDeg;
		private CommonUI.Controls.DebouncedNumericUpDown m_numAngleB;
		private CommonUI.Controls.ThemedLabel m_lblMaster;
		private CommonUI.Controls.DebouncedNumericUpDown m_numMaster;
		private CommonUI.Controls.ThemedLabel m_lblSlave;
		private CommonUI.Controls.DebouncedNumericUpDown m_numSlave;
		private CommonUI.Controls.ThemedButton m_btnRotaryPos;
		private CommonUI.Controls.ThemedButton m_btnRotaryNeg;
		private CommonUI.Controls.ThemedLabel m_lblInterpolateType;
		private System.Windows.Forms.ComboBox m_cmbInterpolateType;
		private CommonUI.Controls.SectionPanel m_pnlQuickSet;
		private CommonUI.Controls.ThemedButton m_btnZDir;
		private CommonUI.Controls.ThemedButton m_btnKeep;
		private CommonUI.Controls.ThemedButton m_btnRevert;
		private CommonUI.Controls.SectionPanel m_pnlSetting;
		private CommonUI.Controls.ThemedButton m_btnAdd;
		private CommonUI.Controls.ThemedButton m_btnRemove;
		private CommonUI.Controls.ThemedButton m_btnClear;
		private CommonUI.Controls.SectionPanel m_pnlMovePnt;
		private CommonUI.Controls.ThemedLabel m_lblStartPnt;
		private CommonUI.Controls.ThemedButton m_btnToStart;
		private CommonUI.Controls.ThemedButton m_btnToEnd;
		private System.Windows.Forms.Panel m_pnlStartEndSwitch;
		private CommonUI.Controls.ThemedLabel m_lblStartEnd;
		private CommonUI.Controls.ThemedButton m_btnSwitchStartEnd;
		private CommonUI.Controls.ThemedLabel m_lblStartOrEnd;
		private CommonUI.Controls.ThemedLabel m_lblPnt;
		private CommonUI.Controls.ThemedButton m_btnPrev;
		private CommonUI.Controls.ThemedButton m_btnNext;
		private CommonUI.Controls.ThemedLabel m_lblCtrlPnt;
		private CommonUI.Controls.ThemedButton m_btnPreCtrlPnt;
		private CommonUI.Controls.ThemedButton m_btnNextCtrlPnt;
		private System.Windows.Forms.Panel m_pnlColorNormal;
		private CommonUI.Controls.ThemedLabel m_lblLegendNormal;
		private System.Windows.Forms.Panel m_pnlColorRotary;
		private CommonUI.Controls.ThemedLabel m_lblLegendRotary;
		private System.Windows.Forms.Panel m_pnlColorBevel;
		private CommonUI.Controls.ThemedLabel m_lblLegendBevel;
		private System.Windows.Forms.Panel m_pnlColorMaster;
		private CommonUI.Controls.ThemedLabel m_lblLegendMaster;
		private System.Windows.Forms.Panel m_pnlColorSlave;
		private CommonUI.Controls.ThemedLabel m_lblLegendSlave;
	}
}
