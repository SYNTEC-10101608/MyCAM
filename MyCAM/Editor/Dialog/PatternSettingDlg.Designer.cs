namespace MyCAM.Editor.Dialog
{
	partial class PatternSettingDlg
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
			this.m_secPathType = new CommonUI.Controls.SectionPanel();
			this.m_lblPathType = new System.Windows.Forms.Label();
			this.m_cmbPathType = new System.Windows.Forms.ComboBox();
			this.m_btnAutoRecognize = new CommonUI.Controls.ThemedButton();
			this.m_chkCoordReverse = new System.Windows.Forms.CheckBox();
			this.m_secPatternParam = new CommonUI.Controls.SectionPanel();
			this.m_panelCircle = new System.Windows.Forms.Panel();
			this.label6 = new System.Windows.Forms.Label();
			this.m_txbCircleDiameter = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label7 = new System.Windows.Forms.Label();
			this.m_txbCircleRotatedAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_panelRectangle = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.m_txbRecLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.m_txbRecWidth = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.m_txbRecCornerRadius = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.m_txbRecRotatedAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_panelRunway = new System.Windows.Forms.Panel();
			this.label15 = new System.Windows.Forms.Label();
			this.m_txbRunwayLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label14 = new System.Windows.Forms.Label();
			this.m_txbRunwayWidth = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label12 = new System.Windows.Forms.Label();
			this.m_txbRunwayRotatedAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_panelPolygon = new System.Windows.Forms.Panel();
			this.label13 = new System.Windows.Forms.Label();
			this.m_txbPolygonSideLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label8 = new System.Windows.Forms.Label();
			this.m_txbPolygonCornerRadius = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label11 = new System.Windows.Forms.Label();
			this.m_txbPolygonRotatedAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_btnConfirm = new CommonUI.Controls.ThemedButton();
			this.m_secPathType.SuspendLayout();
			this.m_secPatternParam.SuspendLayout();
			this.m_panelCircle.SuspendLayout();
			this.m_panelRectangle.SuspendLayout();
			this.m_panelRunway.SuspendLayout();
			this.m_panelPolygon.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbCircleDiameter)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbCircleRotatedAngle)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRecLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRecWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRecCornerRadius)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRecRotatedAngle)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRunwayLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRunwayWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRunwayRotatedAngle)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbPolygonSideLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbPolygonCornerRadius)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbPolygonRotatedAngle)).BeginInit();
			this.SuspendLayout();
			// 
			// m_secPathType
			// 
			this.m_secPathType.Controls.Add(this.m_lblPathType);
			this.m_secPathType.Controls.Add(this.m_cmbPathType);
			this.m_secPathType.Controls.Add(this.m_btnAutoRecognize);
			this.m_secPathType.Controls.Add(this.m_chkCoordReverse);
			this.m_secPathType.Location = new System.Drawing.Point(12, 12);
			this.m_secPathType.Name = "m_secPathType";
			this.m_secPathType.Size = new System.Drawing.Size(356, 73);
			this.m_secPathType.TabIndex = 0;
			// 
			// m_lblPathType
			// 
			this.m_lblPathType.AutoSize = true;
			this.m_lblPathType.Location = new System.Drawing.Point(10, 14);
			this.m_lblPathType.Name = "m_lblPathType";
			this.m_lblPathType.Size = new System.Drawing.Size(55, 16);
			this.m_lblPathType.TabIndex = 0;
			this.m_lblPathType.Text = "路徑類型";
			// 
			// m_cmbPathType
			// 
			this.m_cmbPathType.FormattingEnabled = true;
			this.m_cmbPathType.Location = new System.Drawing.Point(100, 10);
			this.m_cmbPathType.Name = "m_cmbPathType";
			this.m_cmbPathType.Size = new System.Drawing.Size(150, 24);
			this.m_cmbPathType.TabIndex = 1;
			this.m_cmbPathType.Items.AddRange(new object[] { "輪廓", "圓形", "矩形", "跑道形", "三角形", "正方形", "五邊形", "六邊形"});
			this.m_cmbPathType.SelectedIndexChanged += new System.EventHandler(this.m_cmbPathType_SelectedIndexChanged);
			// 
			// m_btnAutoRecognize
			// 
			this.m_btnAutoRecognize.Location = new System.Drawing.Point(256, 10);
			this.m_btnAutoRecognize.Name = "m_btnAutoRecognize";
			this.m_btnAutoRecognize.Size = new System.Drawing.Size(90, 23);
			this.m_btnAutoRecognize.TabIndex = 2;
			this.m_btnAutoRecognize.Text = "自動識別";
			this.m_btnAutoRecognize.Click += new System.EventHandler(this.m_btnAutoRecognize_Click);
			// 
			// m_chkCoordReverse
			// 
			this.m_chkCoordReverse.AutoSize = true;
			this.m_chkCoordReverse.Location = new System.Drawing.Point(10, 42);
			this.m_chkCoordReverse.Name = "m_chkCoordReverse";
			this.m_chkCoordReverse.Size = new System.Drawing.Size(74, 20);
			this.m_chkCoordReverse.TabIndex = 3;
			this.m_chkCoordReverse.Text = "座標反轉";
			this.m_chkCoordReverse.CheckedChanged += new System.EventHandler(this.m_chkCoordReverse_CheckedChanged);
			// 
			// m_secPatternParam
			// 
			this.m_secPatternParam.Controls.Add(this.m_panelCircle);
			this.m_secPatternParam.Controls.Add(this.m_panelRectangle);
			this.m_secPatternParam.Controls.Add(this.m_panelRunway);
			this.m_secPatternParam.Controls.Add(this.m_panelPolygon);
			this.m_secPatternParam.Location = new System.Drawing.Point(12, 93);
			this.m_secPatternParam.Name = "m_secPatternParam";
			this.m_secPatternParam.Size = new System.Drawing.Size(356, 133);
			this.m_secPatternParam.TabIndex = 1;
			// 
			// m_panelCircle
			// 
			this.m_panelCircle.Controls.Add(this.label6);
			this.m_panelCircle.Controls.Add(this.m_txbCircleDiameter);
			this.m_panelCircle.Controls.Add(this.label7);
			this.m_panelCircle.Controls.Add(this.m_txbCircleRotatedAngle);
			this.m_panelCircle.Location = new System.Drawing.Point(10, 10);
			this.m_panelCircle.Name = "m_panelCircle";
			this.m_panelCircle.Size = new System.Drawing.Size(336, 113);
			this.m_panelCircle.TabIndex = 0;
			this.m_panelCircle.Visible = false;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(0, 4);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(31, 16);
			this.label6.TabIndex = 0;
			this.label6.Text = "直徑";
			// 
			// m_txbCircleDiameter
			// 
			this.m_txbCircleDiameter.DecimalPlaces = 3;
			this.m_txbCircleDiameter.Location = new System.Drawing.Point(100, 0);
			this.m_txbCircleDiameter.Name = "m_txbCircleDiameter";
			this.m_txbCircleDiameter.Size = new System.Drawing.Size(150, 23);
			this.m_txbCircleDiameter.TabIndex = 1;
			this.m_txbCircleDiameter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbCircleDiameter_KeyDown);
			this.m_txbCircleDiameter.Leave += new System.EventHandler(this.m_txbCircleDiameter_Leave);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(0, 34);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(55, 16);
			this.label7.TabIndex = 2;
			this.label7.Text = "旋轉角度";
			// 
			// m_txbCircleRotatedAngle
			// 
			this.m_txbCircleRotatedAngle.DecimalPlaces = 3;
			this.m_txbCircleRotatedAngle.Location = new System.Drawing.Point(100, 30);
			this.m_txbCircleRotatedAngle.Name = "m_txbCircleRotatedAngle";
			this.m_txbCircleRotatedAngle.Size = new System.Drawing.Size(150, 23);
			this.m_txbCircleRotatedAngle.TabIndex = 3;
			this.m_txbCircleRotatedAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbCircleRotatedAngle_KeyDown);
			this.m_txbCircleRotatedAngle.Leave += new System.EventHandler(this.m_txbCircleRotatedAngle_Leave);
			// 
			// m_panelRectangle
			// 
			this.m_panelRectangle.Controls.Add(this.label2);
			this.m_panelRectangle.Controls.Add(this.m_txbRecLength);
			this.m_panelRectangle.Controls.Add(this.label3);
			this.m_panelRectangle.Controls.Add(this.m_txbRecWidth);
			this.m_panelRectangle.Controls.Add(this.label4);
			this.m_panelRectangle.Controls.Add(this.m_txbRecCornerRadius);
			this.m_panelRectangle.Controls.Add(this.label5);
			this.m_panelRectangle.Controls.Add(this.m_txbRecRotatedAngle);
			this.m_panelRectangle.Location = new System.Drawing.Point(10, 10);
			this.m_panelRectangle.Name = "m_panelRectangle";
			this.m_panelRectangle.Size = new System.Drawing.Size(336, 113);
			this.m_panelRectangle.TabIndex = 1;
			this.m_panelRectangle.Visible = false;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(0, 4);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(31, 16);
			this.label2.TabIndex = 0;
			this.label2.Text = "長度";
			// 
			// m_txbRecLength
			// 
			this.m_txbRecLength.DecimalPlaces = 3;
			this.m_txbRecLength.Location = new System.Drawing.Point(100, 0);
			this.m_txbRecLength.Name = "m_txbRecLength";
			this.m_txbRecLength.Size = new System.Drawing.Size(150, 23);
			this.m_txbRecLength.TabIndex = 1;
			this.m_txbRecLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbRecLength_KeyDown);
			this.m_txbRecLength.Leave += new System.EventHandler(this.m_txbRecLength_Leave);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(0, 34);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(31, 16);
			this.label3.TabIndex = 2;
			this.label3.Text = "寬度";
			// 
			// m_txbRecWidth
			// 
			this.m_txbRecWidth.DecimalPlaces = 3;
			this.m_txbRecWidth.Location = new System.Drawing.Point(100, 30);
			this.m_txbRecWidth.Name = "m_txbRecWidth";
			this.m_txbRecWidth.Size = new System.Drawing.Size(150, 23);
			this.m_txbRecWidth.TabIndex = 3;
			this.m_txbRecWidth.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbRecWidth_KeyDown);
			this.m_txbRecWidth.Leave += new System.EventHandler(this.m_txbRecWidth_Leave);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(0, 64);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(55, 16);
			this.label4.TabIndex = 4;
			this.label4.Text = "圓角半徑";
			// 
			// m_txbRecCornerRadius
			// 
			this.m_txbRecCornerRadius.DecimalPlaces = 3;
			this.m_txbRecCornerRadius.Location = new System.Drawing.Point(100, 60);
			this.m_txbRecCornerRadius.Name = "m_txbRecCornerRadius";
			this.m_txbRecCornerRadius.Size = new System.Drawing.Size(150, 23);
			this.m_txbRecCornerRadius.TabIndex = 5;
			this.m_txbRecCornerRadius.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbRecCornerRadius_KeyDown);
			this.m_txbRecCornerRadius.Leave += new System.EventHandler(this.m_txbRecCornerRadius_Leave);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(0, 94);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(55, 16);
			this.label5.TabIndex = 6;
			this.label5.Text = "旋轉角度";
			// 
			// m_txbRecRotatedAngle
			// 
			this.m_txbRecRotatedAngle.DecimalPlaces = 3;
			this.m_txbRecRotatedAngle.Location = new System.Drawing.Point(100, 90);
			this.m_txbRecRotatedAngle.Name = "m_txbRecRotatedAngle";
			this.m_txbRecRotatedAngle.Size = new System.Drawing.Size(150, 23);
			this.m_txbRecRotatedAngle.TabIndex = 7;
			this.m_txbRecRotatedAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbRecRotatedAngle_KeyDown);
			this.m_txbRecRotatedAngle.Leave += new System.EventHandler(this.m_txbRecRotatedAngle_Leave);
			// 
			// m_panelRunway
			// 
			this.m_panelRunway.Controls.Add(this.label15);
			this.m_panelRunway.Controls.Add(this.m_txbRunwayLength);
			this.m_panelRunway.Controls.Add(this.label14);
			this.m_panelRunway.Controls.Add(this.m_txbRunwayWidth);
			this.m_panelRunway.Controls.Add(this.label12);
			this.m_panelRunway.Controls.Add(this.m_txbRunwayRotatedAngle);
			this.m_panelRunway.Location = new System.Drawing.Point(10, 10);
			this.m_panelRunway.Name = "m_panelRunway";
			this.m_panelRunway.Size = new System.Drawing.Size(336, 113);
			this.m_panelRunway.TabIndex = 2;
			this.m_panelRunway.Visible = false;
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(0, 4);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(31, 16);
			this.label15.TabIndex = 0;
			this.label15.Text = "長度";
			// 
			// m_txbRunwayLength
			// 
			this.m_txbRunwayLength.DecimalPlaces = 3;
			this.m_txbRunwayLength.Location = new System.Drawing.Point(100, 0);
			this.m_txbRunwayLength.Name = "m_txbRunwayLength";
			this.m_txbRunwayLength.Size = new System.Drawing.Size(150, 23);
			this.m_txbRunwayLength.TabIndex = 1;
			this.m_txbRunwayLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbRunwayLength_KeyDown);
			this.m_txbRunwayLength.Leave += new System.EventHandler(this.m_txbRunwayLength_Leave);
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(0, 34);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(31, 16);
			this.label14.TabIndex = 2;
			this.label14.Text = "寬度";
			// 
			// m_txbRunwayWidth
			// 
			this.m_txbRunwayWidth.DecimalPlaces = 3;
			this.m_txbRunwayWidth.Location = new System.Drawing.Point(100, 30);
			this.m_txbRunwayWidth.Name = "m_txbRunwayWidth";
			this.m_txbRunwayWidth.Size = new System.Drawing.Size(150, 23);
			this.m_txbRunwayWidth.TabIndex = 3;
			this.m_txbRunwayWidth.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbRunwayWidth_KeyDown);
			this.m_txbRunwayWidth.Leave += new System.EventHandler(this.m_txbRunwayWidth_Leave);
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(0, 64);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(55, 16);
			this.label12.TabIndex = 4;
			this.label12.Text = "旋轉角度";
			// 
			// m_txbRunwayRotatedAngle
			// 
			this.m_txbRunwayRotatedAngle.DecimalPlaces = 3;
			this.m_txbRunwayRotatedAngle.Location = new System.Drawing.Point(100, 60);
			this.m_txbRunwayRotatedAngle.Name = "m_txbRunwayRotatedAngle";
			this.m_txbRunwayRotatedAngle.Size = new System.Drawing.Size(150, 23);
			this.m_txbRunwayRotatedAngle.TabIndex = 5;
			this.m_txbRunwayRotatedAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbRunwayRotatedAngle_KeyDown);
			this.m_txbRunwayRotatedAngle.Leave += new System.EventHandler(this.m_txbRunwayRotatedAngle_Leave);
			// 
			// m_panelPolygon
			// 
			this.m_panelPolygon.Controls.Add(this.label13);
			this.m_panelPolygon.Controls.Add(this.m_txbPolygonSideLength);
			this.m_panelPolygon.Controls.Add(this.label8);
			this.m_panelPolygon.Controls.Add(this.m_txbPolygonCornerRadius);
			this.m_panelPolygon.Controls.Add(this.label11);
			this.m_panelPolygon.Controls.Add(this.m_txbPolygonRotatedAngle);
			this.m_panelPolygon.Location = new System.Drawing.Point(10, 10);
			this.m_panelPolygon.Name = "m_panelPolygon";
			this.m_panelPolygon.Size = new System.Drawing.Size(336, 113);
			this.m_panelPolygon.TabIndex = 3;
			this.m_panelPolygon.Visible = false;
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(0, 4);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(31, 16);
			this.label13.TabIndex = 0;
			this.label13.Text = "邊長";
			// 
			// m_txbPolygonSideLength
			// 
			this.m_txbPolygonSideLength.DecimalPlaces = 3;
			this.m_txbPolygonSideLength.Location = new System.Drawing.Point(100, 0);
			this.m_txbPolygonSideLength.Name = "m_txbPolygonSideLength";
			this.m_txbPolygonSideLength.Size = new System.Drawing.Size(150, 23);
			this.m_txbPolygonSideLength.TabIndex = 1;
			this.m_txbPolygonSideLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbPolygonSideLength_KeyDown);
			this.m_txbPolygonSideLength.Leave += new System.EventHandler(this.m_txbPolygonSideLength_Leave);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(0, 34);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(55, 16);
			this.label8.TabIndex = 2;
			this.label8.Text = "圓角半徑";
			// 
			// m_txbPolygonCornerRadius
			// 
			this.m_txbPolygonCornerRadius.DecimalPlaces = 3;
			this.m_txbPolygonCornerRadius.Location = new System.Drawing.Point(100, 30);
			this.m_txbPolygonCornerRadius.Name = "m_txbPolygonCornerRadius";
			this.m_txbPolygonCornerRadius.Size = new System.Drawing.Size(150, 23);
			this.m_txbPolygonCornerRadius.TabIndex = 3;
			this.m_txbPolygonCornerRadius.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbPolygonCornerRadius_KeyDown);
			this.m_txbPolygonCornerRadius.Leave += new System.EventHandler(this.m_txbPolygonCornerRadius_Leave);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(0, 64);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(55, 16);
			this.label11.TabIndex = 4;
			this.label11.Text = "旋轉角度";
			// 
			// m_txbPolygonRotatedAngle
			// 
			this.m_txbPolygonRotatedAngle.DecimalPlaces = 3;
			this.m_txbPolygonRotatedAngle.Location = new System.Drawing.Point(100, 60);
			this.m_txbPolygonRotatedAngle.Name = "m_txbPolygonRotatedAngle";
			this.m_txbPolygonRotatedAngle.Size = new System.Drawing.Size(150, 23);
			this.m_txbPolygonRotatedAngle.TabIndex = 5;
			this.m_txbPolygonRotatedAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbPolygonRotatedAngle_KeyDown);
			this.m_txbPolygonRotatedAngle.Leave += new System.EventHandler(this.m_txbPolygonRotatedAngle_Leave);
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnConfirm.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnConfirm.Location = new System.Drawing.Point(288, 242);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(80, 28);
			this.m_btnConfirm.TabIndex = 2;
			this.m_btnConfirm.Text = "確定";
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// PatternSettingDlg
			// 
			this.ClientSize = new System.Drawing.Size(380, 282);
			this.Controls.Add(this.m_secPathType);
			this.Controls.Add(this.m_secPatternParam);
			this.Controls.Add(this.m_btnConfirm);
			this.Name = "PatternSettingDlg";
			this.Text = "標準圖元設定";
			this.m_secPathType.ResumeLayout(false);
			this.m_secPathType.PerformLayout();
			this.m_secPatternParam.ResumeLayout(false);
			this.m_panelCircle.ResumeLayout(false);
			this.m_panelCircle.PerformLayout();
			this.m_panelRectangle.ResumeLayout(false);
			this.m_panelRectangle.PerformLayout();
			this.m_panelRunway.ResumeLayout(false);
			this.m_panelRunway.PerformLayout();
			this.m_panelPolygon.ResumeLayout(false);
			this.m_panelPolygon.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbCircleDiameter)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbCircleRotatedAngle)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRecLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRecWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRecCornerRadius)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRecRotatedAngle)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRunwayLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRunwayWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRunwayRotatedAngle)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbPolygonSideLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbPolygonCornerRadius)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbPolygonRotatedAngle)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_secPathType;
		private System.Windows.Forms.Label m_lblPathType;
		private System.Windows.Forms.ComboBox m_cmbPathType;
		private CommonUI.Controls.ThemedButton m_btnAutoRecognize;
		private System.Windows.Forms.CheckBox m_chkCoordReverse;
		private CommonUI.Controls.SectionPanel m_secPatternParam;
		private System.Windows.Forms.Panel m_panelCircle;
		private System.Windows.Forms.Label label6;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbCircleDiameter;
		private System.Windows.Forms.Label label7;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbCircleRotatedAngle;
		private System.Windows.Forms.Panel m_panelRectangle;
		private System.Windows.Forms.Label label2;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbRecLength;
		private System.Windows.Forms.Label label3;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbRecWidth;
		private System.Windows.Forms.Label label4;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbRecCornerRadius;
		private System.Windows.Forms.Label label5;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbRecRotatedAngle;
		private System.Windows.Forms.Panel m_panelRunway;
		private System.Windows.Forms.Label label15;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbRunwayLength;
		private System.Windows.Forms.Label label14;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbRunwayWidth;
		private System.Windows.Forms.Label label12;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbRunwayRotatedAngle;
		private System.Windows.Forms.Panel m_panelPolygon;
		private System.Windows.Forms.Label label13;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbPolygonSideLength;
		private System.Windows.Forms.Label label8;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbPolygonCornerRadius;
		private System.Windows.Forms.Label label11;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbPolygonRotatedAngle;
		private CommonUI.Controls.ThemedButton m_btnConfirm;
	}
}
