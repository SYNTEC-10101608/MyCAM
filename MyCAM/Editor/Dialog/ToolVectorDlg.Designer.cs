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
			this.m_gbxIndexParam = new System.Windows.Forms.Panel();
			this.m_secToolVecInfo = new CommonUI.Controls.SectionPanel();
			this.m_lblADeg = new System.Windows.Forms.Label();
			this.m_tbxAngleA = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblBDeg = new System.Windows.Forms.Label();
			this.m_tbxAngleB = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblMaster = new CommonUI.Controls.ThemedLabel();
			this.m_tbxMaster = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblSlave = new CommonUI.Controls.ThemedLabel();
			this.m_tbxSlave = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_btnRotaryPos = new CommonUI.Controls.ThemedButton();
			this.m_btnRotaryNeg = new CommonUI.Controls.ThemedButton();
			this.m_lblInterpolateType = new System.Windows.Forms.Label();
			this.m_cbxInterpolateType = new System.Windows.Forms.ComboBox();
			this.m_secQuickSet = new CommonUI.Controls.SectionPanel();
			this.m_btnZDir = new CommonUI.Controls.ThemedButton();
			this.m_btnKeep = new CommonUI.Controls.ThemedButton();
			this.m_btnRevert = new CommonUI.Controls.ThemedButton();
			this.m_lblStartEnd = new System.Windows.Forms.Label();
			this.m_btnSwitchStartEnd = new CommonUI.Controls.ThemedButton();
			this.m_lblStartOrEnd = new System.Windows.Forms.Label();
			this.m_secSetting = new CommonUI.Controls.SectionPanel();
			this.m_btnAdd = new CommonUI.Controls.ThemedButton();
			this.m_btnRemove = new CommonUI.Controls.ThemedButton();
			this.m_btnClear = new CommonUI.Controls.ThemedButton();
			this.m_gpbStartPnt = new CommonUI.Controls.SectionPanel();
			this.m_btnToStart = new CommonUI.Controls.ThemedButton();
			this.m_btnToEnd = new CommonUI.Controls.ThemedButton();
			this.m_gpbPnt = new CommonUI.Controls.SectionPanel();
			this.m_btnPrev = new CommonUI.Controls.ThemedButton();
			this.m_btnNext = new CommonUI.Controls.ThemedButton();
			this.m_gpbCtrlPnt = new CommonUI.Controls.SectionPanel();
			this.m_btnPreCtrlPnt = new CommonUI.Controls.ThemedButton();
			this.m_btnNextCtrlPnt = new CommonUI.Controls.ThemedButton();
			this.m_gbxColorLegend = new CommonUI.Controls.SectionPanel();
			this.m_pnlColorNormal = new System.Windows.Forms.Panel();
			this.m_lblLegendNormal = new System.Windows.Forms.Label();
			this.m_pnlColorRotary = new System.Windows.Forms.Panel();
			this.m_lblLegendRotary = new System.Windows.Forms.Label();
			this.m_pnlColorBevel = new System.Windows.Forms.Panel();
			this.m_lblLegendBevel = new System.Windows.Forms.Label();
			this.m_pnlColorMaster = new System.Windows.Forms.Panel();
			this.m_lblLegendMaster = new System.Windows.Forms.Label();
			this.m_pnlColorSlave = new System.Windows.Forms.Panel();
			this.m_lblLegendSlave = new System.Windows.Forms.Label();
			this.m_gbxIndexParam.SuspendLayout();
			this.m_secToolVecInfo.SuspendLayout();
			this.m_secQuickSet.SuspendLayout();
			this.m_secSetting.SuspendLayout();
			this.m_gpbStartPnt.SuspendLayout();
			this.m_gpbPnt.SuspendLayout();
			this.m_gpbCtrlPnt.SuspendLayout();
			this.m_gbxColorLegend.SuspendLayout();
			this.m_pnlColorNormal.SuspendLayout();
			this.m_pnlColorRotary.SuspendLayout();
			this.m_pnlColorBevel.SuspendLayout();
			this.m_pnlColorMaster.SuspendLayout();
			this.m_pnlColorSlave.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxAngleA)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxAngleB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxMaster)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxSlave)).BeginInit();
			this.SuspendLayout();
			// 
			// m_gbxIndexParam
			// 
			this.m_gbxIndexParam.Controls.Add(this.m_secToolVecInfo);
			this.m_gbxIndexParam.Controls.Add(this.m_secQuickSet);
			this.m_gbxIndexParam.Controls.Add(this.m_secSetting);
			this.m_gbxIndexParam.Location = new System.Drawing.Point(12, 12);
			this.m_gbxIndexParam.Name = "m_gbxIndexParam";
			this.m_gbxIndexParam.Size = new System.Drawing.Size(452, 343);
			this.m_gbxIndexParam.TabIndex = 0;
			// 
			// m_secToolVecInfo
			// 
			this.m_secToolVecInfo.Controls.Add(this.m_lblADeg);
			this.m_secToolVecInfo.Controls.Add(this.m_tbxAngleA);
			this.m_secToolVecInfo.Controls.Add(this.m_lblBDeg);
			this.m_secToolVecInfo.Controls.Add(this.m_tbxAngleB);
			this.m_secToolVecInfo.Controls.Add(this.m_lblMaster);
			this.m_secToolVecInfo.Controls.Add(this.m_tbxMaster);
			this.m_secToolVecInfo.Controls.Add(this.m_lblSlave);
			this.m_secToolVecInfo.Controls.Add(this.m_tbxSlave);
			this.m_secToolVecInfo.Controls.Add(this.m_btnRotaryPos);
			this.m_secToolVecInfo.Controls.Add(this.m_btnRotaryNeg);
			this.m_secToolVecInfo.Controls.Add(this.m_lblInterpolateType);
			this.m_secToolVecInfo.Controls.Add(this.m_cbxInterpolateType);
			this.m_secToolVecInfo.Location = new System.Drawing.Point(0, 0);
			this.m_secToolVecInfo.Name = "m_secToolVecInfo";
			this.m_secToolVecInfo.Size = new System.Drawing.Size(452, 159);
			this.m_secToolVecInfo.TabIndex = 0;
			this.m_secToolVecInfo.Text = "單點姿態資訊";
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
			// m_tbxAngleA
			// 
			this.m_tbxAngleA.DecimalPlaces = 3;
			this.m_tbxAngleA.Location = new System.Drawing.Point(100, 36);
			this.m_tbxAngleA.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_tbxAngleA.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_tbxAngleA.Name = "m_tbxAngleA";
			this.m_tbxAngleA.RestrictInvalidCharacters = false;
			this.m_tbxAngleA.Size = new System.Drawing.Size(120, 23);
			this.m_tbxAngleA.TabIndex = 1;
			this.m_tbxAngleA.DebouncedValueChanged += new System.EventHandler(this.m_tbxAngleA_DebouncedValueChanged);
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
			// m_tbxAngleB
			// 
			this.m_tbxAngleB.DecimalPlaces = 3;
			this.m_tbxAngleB.Location = new System.Drawing.Point(322, 36);
			this.m_tbxAngleB.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_tbxAngleB.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_tbxAngleB.Name = "m_tbxAngleB";
			this.m_tbxAngleB.RestrictInvalidCharacters = false;
			this.m_tbxAngleB.Size = new System.Drawing.Size(120, 23);
			this.m_tbxAngleB.TabIndex = 3;
			this.m_tbxAngleB.DebouncedValueChanged += new System.EventHandler(this.m_tbxAngleB_DebouncedValueChanged);
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
			// m_tbxMaster
			// 
			this.m_tbxMaster.DecimalPlaces = 3;
			this.m_tbxMaster.Location = new System.Drawing.Point(100, 66);
			this.m_tbxMaster.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_tbxMaster.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_tbxMaster.Name = "m_tbxMaster";
			this.m_tbxMaster.RestrictInvalidCharacters = false;
			this.m_tbxMaster.Size = new System.Drawing.Size(120, 23);
			this.m_tbxMaster.TabIndex = 5;
			this.m_tbxMaster.DebouncedValueChanged += new System.EventHandler(this.m_tbxMaster_DebouncedValueChanged);
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
			// m_tbxSlave
			// 
			this.m_tbxSlave.DecimalPlaces = 3;
			this.m_tbxSlave.Location = new System.Drawing.Point(322, 66);
			this.m_tbxSlave.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_tbxSlave.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_tbxSlave.Name = "m_tbxSlave";
			this.m_tbxSlave.RestrictInvalidCharacters = false;
			this.m_tbxSlave.Size = new System.Drawing.Size(120, 23);
			this.m_tbxSlave.TabIndex = 7;
			this.m_tbxSlave.DebouncedValueChanged += new System.EventHandler(this.m_tbxSlave_DebouncedValueChanged);
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
			// m_cbxInterpolateType
			// 
			this.m_cbxInterpolateType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cbxInterpolateType.FormattingEnabled = true;
			this.m_cbxInterpolateType.Location = new System.Drawing.Point(100, 126);
			this.m_cbxInterpolateType.Name = "m_cbxInterpolateType";
			this.m_cbxInterpolateType.Size = new System.Drawing.Size(120, 24);
			this.m_cbxInterpolateType.TabIndex = 11;
			this.m_cbxInterpolateType.Items.AddRange(new object[] { "維持默認", "旋轉軸平滑", "傾角切坡口", "從軸平滑", "主軸平滑"});
			this.m_cbxInterpolateType.SelectedIndexChanged += new System.EventHandler(this.m_cbxInterpolateType_SelectedIndexChanged);
			// 
			// m_secQuickSet
			// 
			this.m_secQuickSet.Controls.Add(this.m_btnZDir);
			this.m_secQuickSet.Controls.Add(this.m_btnKeep);
			this.m_secQuickSet.Controls.Add(this.m_btnRevert);
			this.m_secQuickSet.Controls.Add(this.m_lblStartEnd);
			this.m_secQuickSet.Controls.Add(this.m_btnSwitchStartEnd);
			this.m_secQuickSet.Controls.Add(this.m_lblStartOrEnd);
			this.m_secQuickSet.Location = new System.Drawing.Point(0, 167);
			this.m_secQuickSet.Name = "m_secQuickSet";
			this.m_secQuickSet.Size = new System.Drawing.Size(452, 99);
			this.m_secQuickSet.TabIndex = 1;
			this.m_secQuickSet.Text = "姿態方向快捷鍵";
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
			// m_lblStartEnd
			// 
			this.m_lblStartEnd.AutoSize = true;
			this.m_lblStartEnd.Location = new System.Drawing.Point(10, 70);
			this.m_lblStartEnd.Name = "m_lblStartEnd";
			this.m_lblStartEnd.Size = new System.Drawing.Size(67, 16);
			this.m_lblStartEnd.TabIndex = 3;
			this.m_lblStartEnd.Text = "起終點切換";
			// 
			// m_btnSwitchStartEnd
			// 
			this.m_btnSwitchStartEnd.Location = new System.Drawing.Point(100, 66);
			this.m_btnSwitchStartEnd.Name = "m_btnSwitchStartEnd";
			this.m_btnSwitchStartEnd.Size = new System.Drawing.Size(80, 23);
			this.m_btnSwitchStartEnd.TabIndex = 4;
			this.m_btnSwitchStartEnd.Text = "起/終";
			this.m_btnSwitchStartEnd.Click += new System.EventHandler(this.m_btnSwitchStartEnd_Click);
			// 
			// m_lblStartOrEnd
			// 
			this.m_lblStartOrEnd.AutoSize = true;
			this.m_lblStartOrEnd.Location = new System.Drawing.Point(190, 70);
			this.m_lblStartOrEnd.Name = "m_lblStartOrEnd";
			this.m_lblStartOrEnd.Size = new System.Drawing.Size(67, 16);
			this.m_lblStartOrEnd.TabIndex = 5;
			this.m_lblStartOrEnd.Text = "當前位置：";
			// 
			// m_secSetting
			// 
			this.m_secSetting.Controls.Add(this.m_btnAdd);
			this.m_secSetting.Controls.Add(this.m_btnRemove);
			this.m_secSetting.Controls.Add(this.m_btnClear);
			this.m_secSetting.Location = new System.Drawing.Point(0, 274);
			this.m_secSetting.Name = "m_secSetting";
			this.m_secSetting.Size = new System.Drawing.Size(452, 69);
			this.m_secSetting.TabIndex = 2;
			this.m_secSetting.Text = "姿態設置";
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
			// m_gpbStartPnt
			// 
			this.m_gpbStartPnt.Controls.Add(this.m_btnToStart);
			this.m_gpbStartPnt.Controls.Add(this.m_btnToEnd);
			this.m_gpbStartPnt.Location = new System.Drawing.Point(12, 363);
			this.m_gpbStartPnt.Name = "m_gpbStartPnt";
			this.m_gpbStartPnt.Size = new System.Drawing.Size(452, 69);
			this.m_gpbStartPnt.TabIndex = 1;
			this.m_gpbStartPnt.Text = "起終點轉換";
			// 
			// m_btnToStart
			// 
			this.m_btnToStart.Location = new System.Drawing.Point(10, 36);
			this.m_btnToStart.Name = "m_btnToStart";
			this.m_btnToStart.Size = new System.Drawing.Size(100, 23);
			this.m_btnToStart.TabIndex = 0;
			this.m_btnToStart.Text = "至起點";
			this.m_btnToStart.Click += new System.EventHandler(this.m_btnToStart_Click);
			// 
			// m_btnToEnd
			// 
			this.m_btnToEnd.Location = new System.Drawing.Point(118, 36);
			this.m_btnToEnd.Name = "m_btnToEnd";
			this.m_btnToEnd.Size = new System.Drawing.Size(100, 23);
			this.m_btnToEnd.TabIndex = 1;
			this.m_btnToEnd.Text = "至終點";
			this.m_btnToEnd.Click += new System.EventHandler(this.m_btnToEnd_Click);
			// 
			// m_gpbPnt
			// 
			this.m_gpbPnt.Controls.Add(this.m_btnPrev);
			this.m_gpbPnt.Controls.Add(this.m_btnNext);
			this.m_gpbPnt.Location = new System.Drawing.Point(12, 440);
			this.m_gpbPnt.Name = "m_gpbPnt";
			this.m_gpbPnt.Size = new System.Drawing.Size(452, 69);
			this.m_gpbPnt.TabIndex = 2;
			this.m_gpbPnt.Text = "前後點轉換";
			// 
			// m_btnPrev
			// 
			this.m_btnPrev.Location = new System.Drawing.Point(10, 36);
			this.m_btnPrev.Name = "m_btnPrev";
			this.m_btnPrev.Size = new System.Drawing.Size(100, 23);
			this.m_btnPrev.TabIndex = 0;
			this.m_btnPrev.Text = "前一點";
			this.m_btnPrev.MouseDown += new System.Windows.Forms.MouseEventHandler(this.m_btnPrev_MouseDown);
			this.m_btnPrev.MouseUp += new System.Windows.Forms.MouseEventHandler(this.m_btnPrev_MouseUp);
			// 
			// m_btnNext
			// 
			this.m_btnNext.Location = new System.Drawing.Point(118, 36);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(100, 23);
			this.m_btnNext.TabIndex = 1;
			this.m_btnNext.Text = "後一點";
			this.m_btnNext.MouseDown += new System.Windows.Forms.MouseEventHandler(this.m_btnNext_MouseDown);
			this.m_btnNext.MouseUp += new System.Windows.Forms.MouseEventHandler(this.m_btnNext_MouseUp);
			// 
			// m_gpbCtrlPnt
			// 
			this.m_gpbCtrlPnt.Controls.Add(this.m_btnPreCtrlPnt);
			this.m_gpbCtrlPnt.Controls.Add(this.m_btnNextCtrlPnt);
			this.m_gpbCtrlPnt.Location = new System.Drawing.Point(12, 517);
			this.m_gpbCtrlPnt.Name = "m_gpbCtrlPnt";
			this.m_gpbCtrlPnt.Size = new System.Drawing.Size(452, 69);
			this.m_gpbCtrlPnt.TabIndex = 3;
			this.m_gpbCtrlPnt.Text = "控制點轉換";
			// 
			// m_btnPreCtrlPnt
			// 
			this.m_btnPreCtrlPnt.Location = new System.Drawing.Point(10, 36);
			this.m_btnPreCtrlPnt.Name = "m_btnPreCtrlPnt";
			this.m_btnPreCtrlPnt.Size = new System.Drawing.Size(100, 23);
			this.m_btnPreCtrlPnt.TabIndex = 0;
			this.m_btnPreCtrlPnt.Text = "前控制點";
			this.m_btnPreCtrlPnt.Click += new System.EventHandler(this.m_btnPreCtrlPnt_Click);
			// 
			// m_btnNextCtrlPnt
			// 
			this.m_btnNextCtrlPnt.Location = new System.Drawing.Point(118, 36);
			this.m_btnNextCtrlPnt.Name = "m_btnNextCtrlPnt";
			this.m_btnNextCtrlPnt.Size = new System.Drawing.Size(100, 23);
			this.m_btnNextCtrlPnt.TabIndex = 1;
			this.m_btnNextCtrlPnt.Text = "後控制點";
			this.m_btnNextCtrlPnt.Click += new System.EventHandler(this.m_btnNextCtrlPnt_Click);
			// 
			// m_gbxColorLegend
			// 
			this.m_gbxColorLegend.Controls.Add(this.m_pnlColorNormal);
			this.m_gbxColorLegend.Controls.Add(this.m_lblLegendNormal);
			this.m_gbxColorLegend.Controls.Add(this.m_pnlColorRotary);
			this.m_gbxColorLegend.Controls.Add(this.m_lblLegendRotary);
			this.m_gbxColorLegend.Controls.Add(this.m_pnlColorBevel);
			this.m_gbxColorLegend.Controls.Add(this.m_lblLegendBevel);
			this.m_gbxColorLegend.Controls.Add(this.m_pnlColorMaster);
			this.m_gbxColorLegend.Controls.Add(this.m_lblLegendMaster);
			this.m_gbxColorLegend.Controls.Add(this.m_pnlColorSlave);
			this.m_gbxColorLegend.Controls.Add(this.m_lblLegendSlave);
			this.m_gbxColorLegend.Location = new System.Drawing.Point(12, 594);
			this.m_gbxColorLegend.Name = "m_gbxColorLegend";
			this.m_gbxColorLegend.Size = new System.Drawing.Size(452, 99);
			this.m_gbxColorLegend.TabIndex = 4;
			this.m_gbxColorLegend.Text = "插值方案";
			// 
			// m_pnlColorNormal
			// 
			this.m_pnlColorNormal.BackColor = System.Drawing.Color.Blue;
			this.m_pnlColorNormal.Location = new System.Drawing.Point(10, 39);
			this.m_pnlColorNormal.Name = "m_pnlColorNormal";
			this.m_pnlColorNormal.Size = new System.Drawing.Size(16, 16);
			this.m_pnlColorNormal.TabIndex = 0;
			// 
			// m_lblLegendNormal
			// 
			this.m_lblLegendNormal.AutoSize = true;
			this.m_lblLegendNormal.Location = new System.Drawing.Point(32, 40);
			this.m_lblLegendNormal.Name = "m_lblLegendNormal";
			this.m_lblLegendNormal.Size = new System.Drawing.Size(31, 16);
			this.m_lblLegendNormal.TabIndex = 1;
			this.m_lblLegendNormal.Text = "維持";
			// 
			// m_pnlColorRotary
			// 
			this.m_pnlColorRotary.BackColor = System.Drawing.Color.Orange;
			this.m_pnlColorRotary.Location = new System.Drawing.Point(160, 39);
			this.m_pnlColorRotary.Name = "m_pnlColorRotary";
			this.m_pnlColorRotary.Size = new System.Drawing.Size(16, 16);
			this.m_pnlColorRotary.TabIndex = 2;
			// 
			// m_lblLegendRotary
			// 
			this.m_lblLegendRotary.AutoSize = true;
			this.m_lblLegendRotary.Location = new System.Drawing.Point(182, 40);
			this.m_lblLegendRotary.Name = "m_lblLegendRotary";
			this.m_lblLegendRotary.Size = new System.Drawing.Size(79, 16);
			this.m_lblLegendRotary.TabIndex = 3;
			this.m_lblLegendRotary.Text = "旋轉軸平滑";
			// 
			// m_pnlColorBevel
			// 
			this.m_pnlColorBevel.BackColor = System.Drawing.Color.Cyan;
			this.m_pnlColorBevel.Location = new System.Drawing.Point(310, 39);
			this.m_pnlColorBevel.Name = "m_pnlColorBevel";
			this.m_pnlColorBevel.Size = new System.Drawing.Size(16, 16);
			this.m_pnlColorBevel.TabIndex = 4;
			// 
			// m_lblLegendBevel
			// 
			this.m_lblLegendBevel.AutoSize = true;
			this.m_lblLegendBevel.Location = new System.Drawing.Point(332, 40);
			this.m_lblLegendBevel.Name = "m_lblLegendBevel";
			this.m_lblLegendBevel.Size = new System.Drawing.Size(79, 16);
			this.m_lblLegendBevel.TabIndex = 5;
			this.m_lblLegendBevel.Text = "傾角切坡口";
			// 
			// m_pnlColorMaster
			// 
			this.m_pnlColorMaster.BackColor = System.Drawing.Color.Magenta;
			this.m_pnlColorMaster.Location = new System.Drawing.Point(10, 69);
			this.m_pnlColorMaster.Name = "m_pnlColorMaster";
			this.m_pnlColorMaster.Size = new System.Drawing.Size(16, 16);
			this.m_pnlColorMaster.TabIndex = 6;
			// 
			// m_lblLegendMaster
			// 
			this.m_lblLegendMaster.AutoSize = true;
			this.m_lblLegendMaster.Location = new System.Drawing.Point(32, 70);
			this.m_lblLegendMaster.Name = "m_lblLegendMaster";
			this.m_lblLegendMaster.Size = new System.Drawing.Size(31, 16);
			this.m_lblLegendMaster.TabIndex = 7;
			this.m_lblLegendMaster.Text = "主軸";
			// 
			// m_pnlColorSlave
			// 
			this.m_pnlColorSlave.BackColor = System.Drawing.Color.Yellow;
			this.m_pnlColorSlave.Location = new System.Drawing.Point(160, 69);
			this.m_pnlColorSlave.Name = "m_pnlColorSlave";
			this.m_pnlColorSlave.Size = new System.Drawing.Size(16, 16);
			this.m_pnlColorSlave.TabIndex = 8;
			// 
			// m_lblLegendSlave
			// 
			this.m_lblLegendSlave.AutoSize = true;
			this.m_lblLegendSlave.Location = new System.Drawing.Point(182, 70);
			this.m_lblLegendSlave.Name = "m_lblLegendSlave";
			this.m_lblLegendSlave.Size = new System.Drawing.Size(31, 16);
			this.m_lblLegendSlave.TabIndex = 9;
			this.m_lblLegendSlave.Text = "從軸";
			// 
			// ToolVectorDlg
			// 
			this.ClientSize = new System.Drawing.Size(476, 705);
			this.Controls.Add(this.m_gbxIndexParam);
			this.Controls.Add(this.m_gpbStartPnt);
			this.Controls.Add(this.m_gpbPnt);
			this.Controls.Add(this.m_gpbCtrlPnt);
			this.Controls.Add(this.m_gbxColorLegend);
			this.Name = "ToolVectorDlg";
			this.Text = "飛邊姿態";
			this.m_gbxIndexParam.ResumeLayout(false);
			this.m_secToolVecInfo.ResumeLayout(false);
			this.m_secToolVecInfo.PerformLayout();
			this.m_secQuickSet.ResumeLayout(false);
			this.m_secQuickSet.PerformLayout();
			this.m_secSetting.ResumeLayout(false);
			this.m_gpbStartPnt.ResumeLayout(false);
			this.m_gpbPnt.ResumeLayout(false);
			this.m_gpbCtrlPnt.ResumeLayout(false);
			this.m_gbxColorLegend.ResumeLayout(false);
			this.m_gbxColorLegend.PerformLayout();
			this.m_pnlColorNormal.ResumeLayout(false);
			this.m_pnlColorRotary.ResumeLayout(false);
			this.m_pnlColorBevel.ResumeLayout(false);
			this.m_pnlColorMaster.ResumeLayout(false);
			this.m_pnlColorSlave.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_tbxAngleA)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxAngleB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxMaster)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxSlave)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel m_gbxIndexParam;
		private CommonUI.Controls.SectionPanel m_secToolVecInfo;
		private System.Windows.Forms.Label m_lblADeg;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxAngleA;
		private System.Windows.Forms.Label m_lblBDeg;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxAngleB;
		private CommonUI.Controls.ThemedLabel m_lblMaster;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxMaster;
		private CommonUI.Controls.ThemedLabel m_lblSlave;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxSlave;
		private CommonUI.Controls.ThemedButton m_btnRotaryPos;
		private CommonUI.Controls.ThemedButton m_btnRotaryNeg;
		private System.Windows.Forms.Label m_lblInterpolateType;
		private System.Windows.Forms.ComboBox m_cbxInterpolateType;
		private CommonUI.Controls.SectionPanel m_secQuickSet;
		private CommonUI.Controls.ThemedButton m_btnZDir;
		private CommonUI.Controls.ThemedButton m_btnKeep;
		private CommonUI.Controls.ThemedButton m_btnRevert;
		private System.Windows.Forms.Label m_lblStartEnd;
		private CommonUI.Controls.ThemedButton m_btnSwitchStartEnd;
		private System.Windows.Forms.Label m_lblStartOrEnd;
		private CommonUI.Controls.SectionPanel m_secSetting;
		private CommonUI.Controls.ThemedButton m_btnAdd;
		private CommonUI.Controls.ThemedButton m_btnRemove;
		private CommonUI.Controls.ThemedButton m_btnClear;
		private CommonUI.Controls.SectionPanel m_gpbStartPnt;
		private CommonUI.Controls.ThemedButton m_btnToStart;
		private CommonUI.Controls.ThemedButton m_btnToEnd;
		private CommonUI.Controls.SectionPanel m_gpbPnt;
		private CommonUI.Controls.ThemedButton m_btnPrev;
		private CommonUI.Controls.ThemedButton m_btnNext;
		private CommonUI.Controls.SectionPanel m_gpbCtrlPnt;
		private CommonUI.Controls.ThemedButton m_btnPreCtrlPnt;
		private CommonUI.Controls.ThemedButton m_btnNextCtrlPnt;
		private CommonUI.Controls.SectionPanel m_gbxColorLegend;
		private System.Windows.Forms.Panel m_pnlColorNormal;
		private System.Windows.Forms.Label m_lblLegendNormal;
		private System.Windows.Forms.Panel m_pnlColorRotary;
		private System.Windows.Forms.Label m_lblLegendRotary;
		private System.Windows.Forms.Panel m_pnlColorBevel;
		private System.Windows.Forms.Label m_lblLegendBevel;
		private System.Windows.Forms.Panel m_pnlColorMaster;
		private System.Windows.Forms.Label m_lblLegendMaster;
		private System.Windows.Forms.Panel m_pnlColorSlave;
		private System.Windows.Forms.Label m_lblLegendSlave;
	}
}
