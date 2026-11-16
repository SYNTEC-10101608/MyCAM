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
			this.m_pnlPathType = new CommonUI.Controls.SectionPanel();
			this.m_lblPathType = new System.Windows.Forms.Label();
			this.m_cmbPathType = new System.Windows.Forms.ComboBox();
			this.m_btnAutoRecognize = new CommonUI.Controls.ThemedButton();
			this.m_chkCoordReverse = new System.Windows.Forms.CheckBox();
			this.m_pnlPatternParam = new CommonUI.Controls.SectionPanel();
			this.m_pnlCircle = new System.Windows.Forms.Panel();
			this.m_lblCircleDiameter = new System.Windows.Forms.Label();
			this.m_numCircleDiameter = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblCircleRotateAngle = new System.Windows.Forms.Label();
			this.m_numCircleRotatedAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_pnlRectangle = new System.Windows.Forms.Panel();
			this.m_lblRectLength = new System.Windows.Forms.Label();
			this.m_numRecLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblRectWidth = new System.Windows.Forms.Label();
			this.m_numRecWidth = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblRectFilletRadius = new System.Windows.Forms.Label();
			this.m_numRecCornerRadius = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblRectRotateAngle = new System.Windows.Forms.Label();
			this.m_numRecRotatedAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_pnlRunway = new System.Windows.Forms.Panel();
			this.m_lblRunwayLength = new System.Windows.Forms.Label();
			this.m_numRunwayLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblRunwayWidth = new System.Windows.Forms.Label();
			this.m_numRunwayWidth = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblRunwayRotateAngle = new System.Windows.Forms.Label();
			this.m_numRunwayRotatedAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_pnlPolygon = new System.Windows.Forms.Panel();
			this.m_lblPolygonSideLength = new System.Windows.Forms.Label();
			this.m_numPolygonSideLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblPolygonFilletRadius = new System.Windows.Forms.Label();
			this.m_numPolygonCornerRadius = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblPolygonRotateAngle = new System.Windows.Forms.Label();
			this.m_numPolygonRotatedAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_btnConfirm = new CommonUI.Controls.ThemedButton();
			this.m_pnlPathType.SuspendLayout();
			this.m_pnlPatternParam.SuspendLayout();
			this.m_pnlCircle.SuspendLayout();
			this.m_pnlRectangle.SuspendLayout();
			this.m_pnlRunway.SuspendLayout();
			this.m_pnlPolygon.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numCircleDiameter)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numCircleRotatedAngle)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRecLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRecWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRecCornerRadius)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRecRotatedAngle)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRunwayLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRunwayWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRunwayRotatedAngle)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numPolygonSideLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numPolygonCornerRadius)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numPolygonRotatedAngle)).BeginInit();
			this.SuspendLayout();
			// 
			// m_pnlPathType
			// 
			this.m_pnlPathType.Controls.Add(this.m_lblPathType);
			this.m_pnlPathType.Controls.Add(this.m_cmbPathType);
			this.m_pnlPathType.Controls.Add(this.m_btnAutoRecognize);
			this.m_pnlPathType.Controls.Add(this.m_chkCoordReverse);
			this.m_pnlPathType.Location = new System.Drawing.Point(12, 12);
			this.m_pnlPathType.Name = "m_pnlPathType";
			this.m_pnlPathType.Size = new System.Drawing.Size(356, 73);
			this.m_pnlPathType.TabIndex = 0;
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
			// m_pnlPatternParam
			// 
			this.m_pnlPatternParam.Controls.Add(this.m_pnlCircle);
			this.m_pnlPatternParam.Controls.Add(this.m_pnlRectangle);
			this.m_pnlPatternParam.Controls.Add(this.m_pnlRunway);
			this.m_pnlPatternParam.Controls.Add(this.m_pnlPolygon);
			this.m_pnlPatternParam.Location = new System.Drawing.Point(12, 93);
			this.m_pnlPatternParam.Name = "m_pnlPatternParam";
			this.m_pnlPatternParam.Size = new System.Drawing.Size(356, 133);
			this.m_pnlPatternParam.TabIndex = 1;
			// 
			// m_pnlCircle
			// 
			this.m_pnlCircle.Controls.Add(this.m_lblCircleDiameter);
			this.m_pnlCircle.Controls.Add(this.m_numCircleDiameter);
			this.m_pnlCircle.Controls.Add(this.m_lblCircleRotateAngle);
			this.m_pnlCircle.Controls.Add(this.m_numCircleRotatedAngle);
			this.m_pnlCircle.Location = new System.Drawing.Point(10, 10);
			this.m_pnlCircle.Name = "m_pnlCircle";
			this.m_pnlCircle.Size = new System.Drawing.Size(336, 113);
			this.m_pnlCircle.TabIndex = 0;
			this.m_pnlCircle.Visible = false;
			// 
			// m_lblCircleDiameter
			// 
			this.m_lblCircleDiameter.AutoSize = true;
			this.m_lblCircleDiameter.Location = new System.Drawing.Point(0, 4);
			this.m_lblCircleDiameter.Name = "m_lblCircleDiameter";
			this.m_lblCircleDiameter.Size = new System.Drawing.Size(31, 16);
			this.m_lblCircleDiameter.TabIndex = 0;
			this.m_lblCircleDiameter.Text = "直徑";
			// 
			// m_numCircleDiameter
			// 
			this.m_numCircleDiameter.DecimalPlaces = 3;
			this.m_numCircleDiameter.Location = new System.Drawing.Point(100, 0);
			this.m_numCircleDiameter.Name = "m_numCircleDiameter";
			this.m_numCircleDiameter.Size = new System.Drawing.Size(150, 23);
			this.m_numCircleDiameter.TabIndex = 1;
			this.m_numCircleDiameter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_numCircleDiameter_KeyDown);
			this.m_numCircleDiameter.Leave += new System.EventHandler(this.m_numCircleDiameter_Leave);
			// 
			// m_lblCircleRotateAngle
			// 
			this.m_lblCircleRotateAngle.AutoSize = true;
			this.m_lblCircleRotateAngle.Location = new System.Drawing.Point(0, 34);
			this.m_lblCircleRotateAngle.Name = "m_lblCircleRotateAngle";
			this.m_lblCircleRotateAngle.Size = new System.Drawing.Size(55, 16);
			this.m_lblCircleRotateAngle.TabIndex = 2;
			this.m_lblCircleRotateAngle.Text = "旋轉角度";
			// 
			// m_numCircleRotatedAngle
			// 
			this.m_numCircleRotatedAngle.DecimalPlaces = 3;
			this.m_numCircleRotatedAngle.Location = new System.Drawing.Point(100, 30);
			this.m_numCircleRotatedAngle.Name = "m_numCircleRotatedAngle";
			this.m_numCircleRotatedAngle.Size = new System.Drawing.Size(150, 23);
			this.m_numCircleRotatedAngle.TabIndex = 3;
			this.m_numCircleRotatedAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_numCircleRotatedAngle_KeyDown);
			this.m_numCircleRotatedAngle.Leave += new System.EventHandler(this.m_numCircleRotatedAngle_Leave);
			// 
			// m_pnlRectangle
			// 
			this.m_pnlRectangle.Controls.Add(this.m_lblRectLength);
			this.m_pnlRectangle.Controls.Add(this.m_numRecLength);
			this.m_pnlRectangle.Controls.Add(this.m_lblRectWidth);
			this.m_pnlRectangle.Controls.Add(this.m_numRecWidth);
			this.m_pnlRectangle.Controls.Add(this.m_lblRectFilletRadius);
			this.m_pnlRectangle.Controls.Add(this.m_numRecCornerRadius);
			this.m_pnlRectangle.Controls.Add(this.m_lblRectRotateAngle);
			this.m_pnlRectangle.Controls.Add(this.m_numRecRotatedAngle);
			this.m_pnlRectangle.Location = new System.Drawing.Point(10, 10);
			this.m_pnlRectangle.Name = "m_pnlRectangle";
			this.m_pnlRectangle.Size = new System.Drawing.Size(336, 113);
			this.m_pnlRectangle.TabIndex = 1;
			this.m_pnlRectangle.Visible = false;
			// 
			// m_lblRectLength
			// 
			this.m_lblRectLength.AutoSize = true;
			this.m_lblRectLength.Location = new System.Drawing.Point(0, 4);
			this.m_lblRectLength.Name = "m_lblRectLength";
			this.m_lblRectLength.Size = new System.Drawing.Size(31, 16);
			this.m_lblRectLength.TabIndex = 0;
			this.m_lblRectLength.Text = "長度";
			// 
			// m_numRecLength
			// 
			this.m_numRecLength.DecimalPlaces = 3;
			this.m_numRecLength.Location = new System.Drawing.Point(100, 0);
			this.m_numRecLength.Name = "m_numRecLength";
			this.m_numRecLength.Size = new System.Drawing.Size(150, 23);
			this.m_numRecLength.TabIndex = 1;
			this.m_numRecLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_numRecLength_KeyDown);
			this.m_numRecLength.Leave += new System.EventHandler(this.m_numRecLength_Leave);
			// 
			// m_lblRectWidth
			// 
			this.m_lblRectWidth.AutoSize = true;
			this.m_lblRectWidth.Location = new System.Drawing.Point(0, 34);
			this.m_lblRectWidth.Name = "m_lblRectWidth";
			this.m_lblRectWidth.Size = new System.Drawing.Size(31, 16);
			this.m_lblRectWidth.TabIndex = 2;
			this.m_lblRectWidth.Text = "寬度";
			// 
			// m_numRecWidth
			// 
			this.m_numRecWidth.DecimalPlaces = 3;
			this.m_numRecWidth.Location = new System.Drawing.Point(100, 30);
			this.m_numRecWidth.Name = "m_numRecWidth";
			this.m_numRecWidth.Size = new System.Drawing.Size(150, 23);
			this.m_numRecWidth.TabIndex = 3;
			this.m_numRecWidth.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_numRecWidth_KeyDown);
			this.m_numRecWidth.Leave += new System.EventHandler(this.m_numRecWidth_Leave);
			// 
			// m_lblRectFilletRadius
			// 
			this.m_lblRectFilletRadius.AutoSize = true;
			this.m_lblRectFilletRadius.Location = new System.Drawing.Point(0, 64);
			this.m_lblRectFilletRadius.Name = "m_lblRectFilletRadius";
			this.m_lblRectFilletRadius.Size = new System.Drawing.Size(55, 16);
			this.m_lblRectFilletRadius.TabIndex = 4;
			this.m_lblRectFilletRadius.Text = "圓角半徑";
			// 
			// m_numRecCornerRadius
			// 
			this.m_numRecCornerRadius.DecimalPlaces = 3;
			this.m_numRecCornerRadius.Location = new System.Drawing.Point(100, 60);
			this.m_numRecCornerRadius.Name = "m_numRecCornerRadius";
			this.m_numRecCornerRadius.Size = new System.Drawing.Size(150, 23);
			this.m_numRecCornerRadius.TabIndex = 5;
			this.m_numRecCornerRadius.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_numRecCornerRadius_KeyDown);
			this.m_numRecCornerRadius.Leave += new System.EventHandler(this.m_numRecCornerRadius_Leave);
			// 
			// m_lblRectRotateAngle
			// 
			this.m_lblRectRotateAngle.AutoSize = true;
			this.m_lblRectRotateAngle.Location = new System.Drawing.Point(0, 94);
			this.m_lblRectRotateAngle.Name = "m_lblRectRotateAngle";
			this.m_lblRectRotateAngle.Size = new System.Drawing.Size(55, 16);
			this.m_lblRectRotateAngle.TabIndex = 6;
			this.m_lblRectRotateAngle.Text = "旋轉角度";
			// 
			// m_numRecRotatedAngle
			// 
			this.m_numRecRotatedAngle.DecimalPlaces = 3;
			this.m_numRecRotatedAngle.Location = new System.Drawing.Point(100, 90);
			this.m_numRecRotatedAngle.Name = "m_numRecRotatedAngle";
			this.m_numRecRotatedAngle.Size = new System.Drawing.Size(150, 23);
			this.m_numRecRotatedAngle.TabIndex = 7;
			this.m_numRecRotatedAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_numRecRotatedAngle_KeyDown);
			this.m_numRecRotatedAngle.Leave += new System.EventHandler(this.m_numRecRotatedAngle_Leave);
			// 
			// m_pnlRunway
			// 
			this.m_pnlRunway.Controls.Add(this.m_lblRunwayLength);
			this.m_pnlRunway.Controls.Add(this.m_numRunwayLength);
			this.m_pnlRunway.Controls.Add(this.m_lblRunwayWidth);
			this.m_pnlRunway.Controls.Add(this.m_numRunwayWidth);
			this.m_pnlRunway.Controls.Add(this.m_lblRunwayRotateAngle);
			this.m_pnlRunway.Controls.Add(this.m_numRunwayRotatedAngle);
			this.m_pnlRunway.Location = new System.Drawing.Point(10, 10);
			this.m_pnlRunway.Name = "m_pnlRunway";
			this.m_pnlRunway.Size = new System.Drawing.Size(336, 113);
			this.m_pnlRunway.TabIndex = 2;
			this.m_pnlRunway.Visible = false;
			// 
			// m_lblRunwayLength
			// 
			this.m_lblRunwayLength.AutoSize = true;
			this.m_lblRunwayLength.Location = new System.Drawing.Point(0, 4);
			this.m_lblRunwayLength.Name = "m_lblRunwayLength";
			this.m_lblRunwayLength.Size = new System.Drawing.Size(31, 16);
			this.m_lblRunwayLength.TabIndex = 0;
			this.m_lblRunwayLength.Text = "長度";
			// 
			// m_numRunwayLength
			// 
			this.m_numRunwayLength.DecimalPlaces = 3;
			this.m_numRunwayLength.Location = new System.Drawing.Point(100, 0);
			this.m_numRunwayLength.Name = "m_numRunwayLength";
			this.m_numRunwayLength.Size = new System.Drawing.Size(150, 23);
			this.m_numRunwayLength.TabIndex = 1;
			this.m_numRunwayLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_numRunwayLength_KeyDown);
			this.m_numRunwayLength.Leave += new System.EventHandler(this.m_numRunwayLength_Leave);
			// 
			// m_lblRunwayWidth
			// 
			this.m_lblRunwayWidth.AutoSize = true;
			this.m_lblRunwayWidth.Location = new System.Drawing.Point(0, 34);
			this.m_lblRunwayWidth.Name = "m_lblRunwayWidth";
			this.m_lblRunwayWidth.Size = new System.Drawing.Size(31, 16);
			this.m_lblRunwayWidth.TabIndex = 2;
			this.m_lblRunwayWidth.Text = "寬度";
			// 
			// m_numRunwayWidth
			// 
			this.m_numRunwayWidth.DecimalPlaces = 3;
			this.m_numRunwayWidth.Location = new System.Drawing.Point(100, 30);
			this.m_numRunwayWidth.Name = "m_numRunwayWidth";
			this.m_numRunwayWidth.Size = new System.Drawing.Size(150, 23);
			this.m_numRunwayWidth.TabIndex = 3;
			this.m_numRunwayWidth.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_numRunwayWidth_KeyDown);
			this.m_numRunwayWidth.Leave += new System.EventHandler(this.m_numRunwayWidth_Leave);
			// 
			// m_lblRunwayRotateAngle
			// 
			this.m_lblRunwayRotateAngle.AutoSize = true;
			this.m_lblRunwayRotateAngle.Location = new System.Drawing.Point(0, 64);
			this.m_lblRunwayRotateAngle.Name = "m_lblRunwayRotateAngle";
			this.m_lblRunwayRotateAngle.Size = new System.Drawing.Size(55, 16);
			this.m_lblRunwayRotateAngle.TabIndex = 4;
			this.m_lblRunwayRotateAngle.Text = "旋轉角度";
			// 
			// m_numRunwayRotatedAngle
			// 
			this.m_numRunwayRotatedAngle.DecimalPlaces = 3;
			this.m_numRunwayRotatedAngle.Location = new System.Drawing.Point(100, 60);
			this.m_numRunwayRotatedAngle.Name = "m_numRunwayRotatedAngle";
			this.m_numRunwayRotatedAngle.Size = new System.Drawing.Size(150, 23);
			this.m_numRunwayRotatedAngle.TabIndex = 5;
			this.m_numRunwayRotatedAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_numRunwayRotatedAngle_KeyDown);
			this.m_numRunwayRotatedAngle.Leave += new System.EventHandler(this.m_numRunwayRotatedAngle_Leave);
			// 
			// m_pnlPolygon
			// 
			this.m_pnlPolygon.Controls.Add(this.m_lblPolygonSideLength);
			this.m_pnlPolygon.Controls.Add(this.m_numPolygonSideLength);
			this.m_pnlPolygon.Controls.Add(this.m_lblPolygonFilletRadius);
			this.m_pnlPolygon.Controls.Add(this.m_numPolygonCornerRadius);
			this.m_pnlPolygon.Controls.Add(this.m_lblPolygonRotateAngle);
			this.m_pnlPolygon.Controls.Add(this.m_numPolygonRotatedAngle);
			this.m_pnlPolygon.Location = new System.Drawing.Point(10, 10);
			this.m_pnlPolygon.Name = "m_pnlPolygon";
			this.m_pnlPolygon.Size = new System.Drawing.Size(336, 113);
			this.m_pnlPolygon.TabIndex = 3;
			this.m_pnlPolygon.Visible = false;
			// 
			// m_lblPolygonSideLength
			// 
			this.m_lblPolygonSideLength.AutoSize = true;
			this.m_lblPolygonSideLength.Location = new System.Drawing.Point(0, 4);
			this.m_lblPolygonSideLength.Name = "m_lblPolygonSideLength";
			this.m_lblPolygonSideLength.Size = new System.Drawing.Size(31, 16);
			this.m_lblPolygonSideLength.TabIndex = 0;
			this.m_lblPolygonSideLength.Text = "邊長";
			// 
			// m_numPolygonSideLength
			// 
			this.m_numPolygonSideLength.DecimalPlaces = 3;
			this.m_numPolygonSideLength.Location = new System.Drawing.Point(100, 0);
			this.m_numPolygonSideLength.Name = "m_numPolygonSideLength";
			this.m_numPolygonSideLength.Size = new System.Drawing.Size(150, 23);
			this.m_numPolygonSideLength.TabIndex = 1;
			this.m_numPolygonSideLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_numPolygonSideLength_KeyDown);
			this.m_numPolygonSideLength.Leave += new System.EventHandler(this.m_numPolygonSideLength_Leave);
			// 
			// m_lblPolygonFilletRadius
			// 
			this.m_lblPolygonFilletRadius.AutoSize = true;
			this.m_lblPolygonFilletRadius.Location = new System.Drawing.Point(0, 34);
			this.m_lblPolygonFilletRadius.Name = "m_lblPolygonFilletRadius";
			this.m_lblPolygonFilletRadius.Size = new System.Drawing.Size(55, 16);
			this.m_lblPolygonFilletRadius.TabIndex = 2;
			this.m_lblPolygonFilletRadius.Text = "圓角半徑";
			// 
			// m_numPolygonCornerRadius
			// 
			this.m_numPolygonCornerRadius.DecimalPlaces = 3;
			this.m_numPolygonCornerRadius.Location = new System.Drawing.Point(100, 30);
			this.m_numPolygonCornerRadius.Name = "m_numPolygonCornerRadius";
			this.m_numPolygonCornerRadius.Size = new System.Drawing.Size(150, 23);
			this.m_numPolygonCornerRadius.TabIndex = 3;
			this.m_numPolygonCornerRadius.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_numPolygonCornerRadius_KeyDown);
			this.m_numPolygonCornerRadius.Leave += new System.EventHandler(this.m_numPolygonCornerRadius_Leave);
			// 
			// m_lblPolygonRotateAngle
			// 
			this.m_lblPolygonRotateAngle.AutoSize = true;
			this.m_lblPolygonRotateAngle.Location = new System.Drawing.Point(0, 64);
			this.m_lblPolygonRotateAngle.Name = "m_lblPolygonRotateAngle";
			this.m_lblPolygonRotateAngle.Size = new System.Drawing.Size(55, 16);
			this.m_lblPolygonRotateAngle.TabIndex = 4;
			this.m_lblPolygonRotateAngle.Text = "旋轉角度";
			// 
			// m_numPolygonRotatedAngle
			// 
			this.m_numPolygonRotatedAngle.DecimalPlaces = 3;
			this.m_numPolygonRotatedAngle.Location = new System.Drawing.Point(100, 60);
			this.m_numPolygonRotatedAngle.Name = "m_numPolygonRotatedAngle";
			this.m_numPolygonRotatedAngle.Size = new System.Drawing.Size(150, 23);
			this.m_numPolygonRotatedAngle.TabIndex = 5;
			this.m_numPolygonRotatedAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_numPolygonRotatedAngle_KeyDown);
			this.m_numPolygonRotatedAngle.Leave += new System.EventHandler(this.m_numPolygonRotatedAngle_Leave);
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
			this.Controls.Add(this.m_pnlPathType);
			this.Controls.Add(this.m_pnlPatternParam);
			this.Controls.Add(this.m_btnConfirm);
			this.Name = "PatternSettingDlg";
			this.Text = "標準圖元設定";
			this.m_pnlPathType.ResumeLayout(false);
			this.m_pnlPathType.PerformLayout();
			this.m_pnlPatternParam.ResumeLayout(false);
			this.m_pnlCircle.ResumeLayout(false);
			this.m_pnlCircle.PerformLayout();
			this.m_pnlRectangle.ResumeLayout(false);
			this.m_pnlRectangle.PerformLayout();
			this.m_pnlRunway.ResumeLayout(false);
			this.m_pnlRunway.PerformLayout();
			this.m_pnlPolygon.ResumeLayout(false);
			this.m_pnlPolygon.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numCircleDiameter)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numCircleRotatedAngle)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRecLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRecWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRecCornerRadius)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRecRotatedAngle)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRunwayLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRunwayWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRunwayRotatedAngle)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numPolygonSideLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numPolygonCornerRadius)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numPolygonRotatedAngle)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlPathType;
		private System.Windows.Forms.Label m_lblPathType;
		private System.Windows.Forms.ComboBox m_cmbPathType;
		private CommonUI.Controls.ThemedButton m_btnAutoRecognize;
		private System.Windows.Forms.CheckBox m_chkCoordReverse;
		private CommonUI.Controls.SectionPanel m_pnlPatternParam;
		private System.Windows.Forms.Panel m_pnlCircle;
		private System.Windows.Forms.Label m_lblCircleDiameter;
		private CommonUI.Controls.DebouncedNumericUpDown m_numCircleDiameter;
		private System.Windows.Forms.Label m_lblCircleRotateAngle;
		private CommonUI.Controls.DebouncedNumericUpDown m_numCircleRotatedAngle;
		private System.Windows.Forms.Panel m_pnlRectangle;
		private System.Windows.Forms.Label m_lblRectLength;
		private CommonUI.Controls.DebouncedNumericUpDown m_numRecLength;
		private System.Windows.Forms.Label m_lblRectWidth;
		private CommonUI.Controls.DebouncedNumericUpDown m_numRecWidth;
		private System.Windows.Forms.Label m_lblRectFilletRadius;
		private CommonUI.Controls.DebouncedNumericUpDown m_numRecCornerRadius;
		private System.Windows.Forms.Label m_lblRectRotateAngle;
		private CommonUI.Controls.DebouncedNumericUpDown m_numRecRotatedAngle;
		private System.Windows.Forms.Panel m_pnlRunway;
		private System.Windows.Forms.Label m_lblRunwayLength;
		private CommonUI.Controls.DebouncedNumericUpDown m_numRunwayLength;
		private System.Windows.Forms.Label m_lblRunwayWidth;
		private CommonUI.Controls.DebouncedNumericUpDown m_numRunwayWidth;
		private System.Windows.Forms.Label m_lblRunwayRotateAngle;
		private CommonUI.Controls.DebouncedNumericUpDown m_numRunwayRotatedAngle;
		private System.Windows.Forms.Panel m_pnlPolygon;
		private System.Windows.Forms.Label m_lblPolygonSideLength;
		private CommonUI.Controls.DebouncedNumericUpDown m_numPolygonSideLength;
		private System.Windows.Forms.Label m_lblPolygonFilletRadius;
		private CommonUI.Controls.DebouncedNumericUpDown m_numPolygonCornerRadius;
		private System.Windows.Forms.Label m_lblPolygonRotateAngle;
		private CommonUI.Controls.DebouncedNumericUpDown m_numPolygonRotatedAngle;
		private CommonUI.Controls.ThemedButton m_btnConfirm;
	}
}
