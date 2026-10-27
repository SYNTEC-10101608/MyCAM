namespace MyCAM.Editor
{
	partial class TraverseDlg
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.m_rbtnFrogLeap = new System.Windows.Forms.RadioButton();
			this.m_rbtnSafePlane = new System.Windows.Forms.RadioButton();
			this.m_rbtnSafeCylinder = new System.Windows.Forms.RadioButton();
			this.m_rbtnSafeSphere = new System.Windows.Forms.RadioButton();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblLiftUpDistance = new System.Windows.Forms.Label();
			this.m_NumericUpDownLiftUpDistance = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_NumericUpDownCutDownDistance = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_NumericUpDownFollowSafeDistance = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblFollowSaveDistance = new System.Windows.Forms.Label();
			this.m_lblCutDownDistance = new System.Windows.Forms.Label();
			this.m_lblLiftUpDistanceUnit = new System.Windows.Forms.Label();
			this.m_lblCutDownDistanceUnit = new System.Windows.Forms.Label();
			this.m_lblFollowSaveDistanceUnit = new System.Windows.Forms.Label();
			this.m_gbxTraverse = new System.Windows.Forms.GroupBox();
			this.m_pnlFrogLeap = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblFrogLeapDistance = new System.Windows.Forms.Label();
			this.m_NumericUpDownFrogLeapDistance = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblFrogLeapDistanceUnit = new System.Windows.Forms.Label();
			this.m_pnlSafePlane = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblSafePlaneDistance = new System.Windows.Forms.Label();
			this.m_NumericUpDownSafePlaneDistance = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblSafePlaneDistanceUnit = new System.Windows.Forms.Label();
			this.m_pnlSafeCylinder = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblSafeCylinderRadius = new System.Windows.Forms.Label();
			this.m_NumericUpDownSafeCylinderRadius = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblSafeCylinderRadiusUnit = new System.Windows.Forms.Label();
			this.m_pnlSafeSphere = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblSafeSphereRadius = new System.Windows.Forms.Label();
			this.m_NumericUpDownSafeSphereRadius = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblSafeSphereRadiusUnit = new System.Windows.Forms.Label();
			this.m_btnConfirm = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownLiftUpDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownCutDownDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownFollowSafeDistance)).BeginInit();
			this.m_gbxTraverse.SuspendLayout();
			this.m_pnlFrogLeap.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownFrogLeapDistance)).BeginInit();
			this.m_pnlSafePlane.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownSafePlaneDistance)).BeginInit();
			this.m_pnlSafeCylinder.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownSafeCylinderRadius)).BeginInit();
			this.m_pnlSafeSphere.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownSafeSphereRadius)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_gbxTraverse, 0, 2);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 108F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(367, 231);
			this.tableLayoutPanel1.TabIndex = 39;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Controls.Add(this.m_rbtnFrogLeap);
			this.flowLayoutPanel1.Controls.Add(this.m_rbtnSafePlane);
			this.flowLayoutPanel1.Controls.Add(this.m_rbtnSafeCylinder);
			this.flowLayoutPanel1.Controls.Add(this.m_rbtnSafeSphere);
			this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 111);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(361, 26);
			this.flowLayoutPanel1.TabIndex = 0;
			// 
			// m_rbtnFrogLeap
			// 
			this.m_rbtnFrogLeap.AutoSize = true;
			this.m_rbtnFrogLeap.Location = new System.Drawing.Point(3, 3);
			this.m_rbtnFrogLeap.Name = "m_rbtnFrogLeap";
			this.m_rbtnFrogLeap.Size = new System.Drawing.Size(60, 23);
			this.m_rbtnFrogLeap.TabIndex = 0;
			this.m_rbtnFrogLeap.TabStop = true;
			this.m_rbtnFrogLeap.Text = "蛙跳";
			this.m_rbtnFrogLeap.UseVisualStyleBackColor = true;
			this.m_rbtnFrogLeap.CheckedChanged += new System.EventHandler(this.TraverseMode_CheckChanged);
			// 
			// m_rbtnSafePlane
			// 
			this.m_rbtnSafePlane.AutoSize = true;
			this.m_rbtnSafePlane.Location = new System.Drawing.Point(69, 3);
			this.m_rbtnSafePlane.Name = "m_rbtnSafePlane";
			this.m_rbtnSafePlane.Size = new System.Drawing.Size(90, 23);
			this.m_rbtnSafePlane.TabIndex = 1;
			this.m_rbtnSafePlane.TabStop = true;
			this.m_rbtnSafePlane.Text = "安全平面";
			this.m_rbtnSafePlane.UseVisualStyleBackColor = true;
			this.m_rbtnSafePlane.CheckedChanged += new System.EventHandler(this.TraverseMode_CheckChanged);
			// 
			// m_rbtnSafeCylinder
			// 
			this.m_rbtnSafeCylinder.AutoSize = true;
			this.m_rbtnSafeCylinder.Location = new System.Drawing.Point(165, 3);
			this.m_rbtnSafeCylinder.Name = "m_rbtnSafeCylinder";
			this.m_rbtnSafeCylinder.Size = new System.Drawing.Size(90, 23);
			this.m_rbtnSafeCylinder.TabIndex = 2;
			this.m_rbtnSafeCylinder.TabStop = true;
			this.m_rbtnSafeCylinder.Text = "安全圓柱";
			this.m_rbtnSafeCylinder.UseVisualStyleBackColor = true;
			this.m_rbtnSafeCylinder.CheckedChanged += new System.EventHandler(this.TraverseMode_CheckChanged);
			// 
			// m_rbtnSafeSphere
			// 
			this.m_rbtnSafeSphere.AutoSize = true;
			this.m_rbtnSafeSphere.Location = new System.Drawing.Point(261, 3);
			this.m_rbtnSafeSphere.Name = "m_rbtnSafeSphere";
			this.m_rbtnSafeSphere.Size = new System.Drawing.Size(90, 23);
			this.m_rbtnSafeSphere.TabIndex = 3;
			this.m_rbtnSafeSphere.TabStop = true;
			this.m_rbtnSafeSphere.Text = "安全圓球";
			this.m_rbtnSafeSphere.UseVisualStyleBackColor = true;
			this.m_rbtnSafeSphere.CheckedChanged += new System.EventHandler(this.TraverseMode_CheckChanged);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.m_lblLiftUpDistance, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.m_NumericUpDownLiftUpDistance, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.m_NumericUpDownCutDownDistance, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.m_NumericUpDownFollowSafeDistance, 1, 2);
			this.tableLayoutPanel2.Controls.Add(this.m_lblFollowSaveDistance, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.m_lblCutDownDistance, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.m_lblLiftUpDistanceUnit, 2, 0);
			this.tableLayoutPanel2.Controls.Add(this.m_lblCutDownDistanceUnit, 2, 1);
			this.tableLayoutPanel2.Controls.Add(this.m_lblFollowSaveDistanceUnit, 2, 2);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(361, 100);
			this.tableLayoutPanel2.TabIndex = 1;
			// 
			// m_lblLiftUpDistance
			// 
			this.m_lblLiftUpDistance.AutoSize = true;
			this.m_lblLiftUpDistance.Location = new System.Drawing.Point(6, 0);
			this.m_lblLiftUpDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblLiftUpDistance.Name = "m_lblLiftUpDistance";
			this.m_lblLiftUpDistance.Size = new System.Drawing.Size(69, 19);
			this.m_lblLiftUpDistance.TabIndex = 26;
			this.m_lblLiftUpDistance.Text = "抬刀距離";
			// 
			// m_NumericUpDownLiftUpDistance
			// 
			this.m_NumericUpDownLiftUpDistance.DecimalPlaces = 3;
			this.m_NumericUpDownLiftUpDistance.Location = new System.Drawing.Point(123, 3);
			this.m_NumericUpDownLiftUpDistance.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_NumericUpDownLiftUpDistance.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_NumericUpDownLiftUpDistance.Name = "m_NumericUpDownLiftUpDistance";
			this.m_NumericUpDownLiftUpDistance.RestrictInvalidCharacters = false;
			this.m_NumericUpDownLiftUpDistance.Size = new System.Drawing.Size(92, 27);
			this.m_NumericUpDownLiftUpDistance.TabIndex = 35;
			this.m_NumericUpDownLiftUpDistance.DebouncedValueChanged += new System.EventHandler(this.m_NumericUpDownLiftUpDistance_DebouncedValueChanged);
			this.m_NumericUpDownLiftUpDistance.ValueChanged += new System.EventHandler(this.m_NumericUpDownCutDownDistance_ValueChanged);
			// 
			// m_NumericUpDownCutDownDistance
			// 
			this.m_NumericUpDownCutDownDistance.DecimalPlaces = 3;
			this.m_NumericUpDownCutDownDistance.Location = new System.Drawing.Point(123, 35);
			this.m_NumericUpDownCutDownDistance.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_NumericUpDownCutDownDistance.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_NumericUpDownCutDownDistance.Name = "m_NumericUpDownCutDownDistance";
			this.m_NumericUpDownCutDownDistance.RestrictInvalidCharacters = false;
			this.m_NumericUpDownCutDownDistance.Size = new System.Drawing.Size(92, 27);
			this.m_NumericUpDownCutDownDistance.TabIndex = 36;
			this.m_NumericUpDownCutDownDistance.DebouncedValueChanged += new System.EventHandler(this.m_NumericUpDownCutDownDistance_DebouncedValueChanged);
			// 
			// m_NumericUpDownFollowSafeDistance
			// 
			this.m_NumericUpDownFollowSafeDistance.DecimalPlaces = 3;
			this.m_NumericUpDownFollowSafeDistance.Location = new System.Drawing.Point(123, 67);
			this.m_NumericUpDownFollowSafeDistance.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_NumericUpDownFollowSafeDistance.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_NumericUpDownFollowSafeDistance.Name = "m_NumericUpDownFollowSafeDistance";
			this.m_NumericUpDownFollowSafeDistance.RestrictInvalidCharacters = false;
			this.m_NumericUpDownFollowSafeDistance.Size = new System.Drawing.Size(92, 27);
			this.m_NumericUpDownFollowSafeDistance.TabIndex = 37;
			this.m_NumericUpDownFollowSafeDistance.DebouncedValueChanged += new System.EventHandler(this.m_NumericUpDownFollowSafeDistance_DebouncedValueChanged);
			// 
			// m_lblFollowSaveDistance
			// 
			this.m_lblFollowSaveDistance.AutoSize = true;
			this.m_lblFollowSaveDistance.Location = new System.Drawing.Point(6, 64);
			this.m_lblFollowSaveDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblFollowSaveDistance.Name = "m_lblFollowSaveDistance";
			this.m_lblFollowSaveDistance.Size = new System.Drawing.Size(99, 19);
			this.m_lblFollowSaveDistance.TabIndex = 20;
			this.m_lblFollowSaveDistance.Text = "隨動安全距離";
			// 
			// m_lblCutDownDistance
			// 
			this.m_lblCutDownDistance.AutoSize = true;
			this.m_lblCutDownDistance.Location = new System.Drawing.Point(6, 32);
			this.m_lblCutDownDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblCutDownDistance.Name = "m_lblCutDownDistance";
			this.m_lblCutDownDistance.Size = new System.Drawing.Size(69, 19);
			this.m_lblCutDownDistance.TabIndex = 21;
			this.m_lblCutDownDistance.Text = "下刀距離";
			// 
			// m_lblLiftUpDistanceUnit
			// 
			this.m_lblLiftUpDistanceUnit.AutoSize = true;
			this.m_lblLiftUpDistanceUnit.Location = new System.Drawing.Point(244, 0);
			this.m_lblLiftUpDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblLiftUpDistanceUnit.Name = "m_lblLiftUpDistanceUnit";
			this.m_lblLiftUpDistanceUnit.Size = new System.Drawing.Size(37, 19);
			this.m_lblLiftUpDistanceUnit.TabIndex = 24;
			this.m_lblLiftUpDistanceUnit.Text = "mm";
			// 
			// m_lblCutDownDistanceUnit
			// 
			this.m_lblCutDownDistanceUnit.AutoSize = true;
			this.m_lblCutDownDistanceUnit.Location = new System.Drawing.Point(244, 32);
			this.m_lblCutDownDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblCutDownDistanceUnit.Name = "m_lblCutDownDistanceUnit";
			this.m_lblCutDownDistanceUnit.Size = new System.Drawing.Size(37, 19);
			this.m_lblCutDownDistanceUnit.TabIndex = 23;
			this.m_lblCutDownDistanceUnit.Text = "mm";
			// 
			// m_lblFollowSaveDistanceUnit
			// 
			this.m_lblFollowSaveDistanceUnit.AutoSize = true;
			this.m_lblFollowSaveDistanceUnit.Location = new System.Drawing.Point(244, 64);
			this.m_lblFollowSaveDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblFollowSaveDistanceUnit.Name = "m_lblFollowSaveDistanceUnit";
			this.m_lblFollowSaveDistanceUnit.Size = new System.Drawing.Size(37, 19);
			this.m_lblFollowSaveDistanceUnit.TabIndex = 22;
			this.m_lblFollowSaveDistanceUnit.Text = "mm";
			// 
			// m_gbxTraverse
			// 
			this.m_gbxTraverse.Controls.Add(this.m_pnlFrogLeap);
			this.m_gbxTraverse.Controls.Add(this.m_pnlSafePlane);
			this.m_gbxTraverse.Controls.Add(this.m_pnlSafeCylinder);
			this.m_gbxTraverse.Controls.Add(this.m_pnlSafeSphere);
			this.m_gbxTraverse.Location = new System.Drawing.Point(3, 147);
			this.m_gbxTraverse.Name = "m_gbxTraverse";
			this.m_gbxTraverse.Size = new System.Drawing.Size(361, 79);
			this.m_gbxTraverse.TabIndex = 40;
			this.m_gbxTraverse.TabStop = false;
			this.m_gbxTraverse.Text = "空移參數";
			// 
			// m_pnlFrogLeap
			// 
			this.m_pnlFrogLeap.ColumnCount = 3;
			this.m_pnlFrogLeap.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.m_pnlFrogLeap.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.m_pnlFrogLeap.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.m_pnlFrogLeap.Controls.Add(this.m_lblFrogLeapDistance, 0, 0);
			this.m_pnlFrogLeap.Controls.Add(this.m_NumericUpDownFrogLeapDistance, 1, 0);
			this.m_pnlFrogLeap.Controls.Add(this.m_lblFrogLeapDistanceUnit, 2, 0);
			this.m_pnlFrogLeap.Location = new System.Drawing.Point(10, 26);
			this.m_pnlFrogLeap.Name = "m_pnlFrogLeap";
			this.m_pnlFrogLeap.RowCount = 1;
			this.m_pnlFrogLeap.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_pnlFrogLeap.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.m_pnlFrogLeap.Size = new System.Drawing.Size(341, 46);
			this.m_pnlFrogLeap.TabIndex = 41;
			// 
			// m_lblFrogLeapDistance
			// 
			this.m_lblFrogLeapDistance.AutoSize = true;
			this.m_lblFrogLeapDistance.Location = new System.Drawing.Point(6, 0);
			this.m_lblFrogLeapDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblFrogLeapDistance.Name = "m_lblFrogLeapDistance";
			this.m_lblFrogLeapDistance.Size = new System.Drawing.Size(69, 19);
			this.m_lblFrogLeapDistance.TabIndex = 19;
			this.m_lblFrogLeapDistance.Text = "蛙跳距離";
			// 
			// m_NumericUpDownFrogLeapDistance
			// 
			this.m_NumericUpDownFrogLeapDistance.DecimalPlaces = 3;
			this.m_NumericUpDownFrogLeapDistance.Location = new System.Drawing.Point(116, 3);
			this.m_NumericUpDownFrogLeapDistance.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_NumericUpDownFrogLeapDistance.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_NumericUpDownFrogLeapDistance.Name = "m_NumericUpDownFrogLeapDistance";
			this.m_NumericUpDownFrogLeapDistance.RestrictInvalidCharacters = false;
			this.m_NumericUpDownFrogLeapDistance.Size = new System.Drawing.Size(83, 27);
			this.m_NumericUpDownFrogLeapDistance.TabIndex = 38;
			this.m_NumericUpDownFrogLeapDistance.DebouncedValueChanged += new System.EventHandler(this.m_NumericUpDownFrogLeapDistance_DebouncedValueChanged);
			// 
			// m_lblFrogLeapDistanceUnit
			// 
			this.m_lblFrogLeapDistanceUnit.AutoSize = true;
			this.m_lblFrogLeapDistanceUnit.Location = new System.Drawing.Point(230, 0);
			this.m_lblFrogLeapDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblFrogLeapDistanceUnit.Name = "m_lblFrogLeapDistanceUnit";
			this.m_lblFrogLeapDistanceUnit.Size = new System.Drawing.Size(37, 19);
			this.m_lblFrogLeapDistanceUnit.TabIndex = 25;
			this.m_lblFrogLeapDistanceUnit.Text = "mm";
			// 
			// m_pnlSafePlane
			// 
			this.m_pnlSafePlane.ColumnCount = 3;
			this.m_pnlSafePlane.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.m_pnlSafePlane.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.m_pnlSafePlane.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.m_pnlSafePlane.Controls.Add(this.m_lblSafePlaneDistance, 0, 0);
			this.m_pnlSafePlane.Controls.Add(this.m_NumericUpDownSafePlaneDistance, 1, 0);
			this.m_pnlSafePlane.Controls.Add(this.m_lblSafePlaneDistanceUnit, 2, 0);
			this.m_pnlSafePlane.Location = new System.Drawing.Point(10, 26);
			this.m_pnlSafePlane.Name = "m_pnlSafePlane";
			this.m_pnlSafePlane.RowCount = 1;
			this.m_pnlSafePlane.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_pnlSafePlane.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.m_pnlSafePlane.Size = new System.Drawing.Size(341, 46);
			this.m_pnlSafePlane.TabIndex = 42;
			// 
			// m_lblSafePlaneDistance
			// 
			this.m_lblSafePlaneDistance.AutoSize = true;
			this.m_lblSafePlaneDistance.Location = new System.Drawing.Point(3, 0);
			this.m_lblSafePlaneDistance.Name = "m_lblSafePlaneDistance";
			this.m_lblSafePlaneDistance.Size = new System.Drawing.Size(78, 19);
			this.m_lblSafePlaneDistance.TabIndex = 31;
			this.m_lblSafePlaneDistance.Text = "Z平面位置";
			// 
			// m_NumericUpDownSafePlaneDistance
			// 
			this.m_NumericUpDownSafePlaneDistance.DecimalPlaces = 3;
			this.m_NumericUpDownSafePlaneDistance.Location = new System.Drawing.Point(116, 3);
			this.m_NumericUpDownSafePlaneDistance.Maximum = new decimal(new int[] {
            1410065407,
            2,
            0,
            0});
			this.m_NumericUpDownSafePlaneDistance.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_NumericUpDownSafePlaneDistance.Name = "m_NumericUpDownSafePlaneDistance";
			this.m_NumericUpDownSafePlaneDistance.RestrictInvalidCharacters = false;
			this.m_NumericUpDownSafePlaneDistance.Size = new System.Drawing.Size(107, 27);
			this.m_NumericUpDownSafePlaneDistance.TabIndex = 34;
			this.m_NumericUpDownSafePlaneDistance.DebouncedValueChanged += new System.EventHandler(this.m_NumericUpDownSafePlaneDistance_DebouncedValueChanged);
			// 
			// m_lblSafePlaneDistanceUnit
			// 
			this.m_lblSafePlaneDistanceUnit.AutoSize = true;
			this.m_lblSafePlaneDistanceUnit.Location = new System.Drawing.Point(230, 0);
			this.m_lblSafePlaneDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblSafePlaneDistanceUnit.Name = "m_lblSafePlaneDistanceUnit";
			this.m_lblSafePlaneDistanceUnit.Size = new System.Drawing.Size(37, 19);
			this.m_lblSafePlaneDistanceUnit.TabIndex = 33;
			this.m_lblSafePlaneDistanceUnit.Text = "mm";
			// 
			// m_pnlSafeCylinder
			// 
			this.m_pnlSafeCylinder.ColumnCount = 3;
			this.m_pnlSafeCylinder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.m_pnlSafeCylinder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.m_pnlSafeCylinder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.m_pnlSafeCylinder.Controls.Add(this.m_lblSafeCylinderRadius, 0, 0);
			this.m_pnlSafeCylinder.Controls.Add(this.m_NumericUpDownSafeCylinderRadius, 1, 0);
			this.m_pnlSafeCylinder.Controls.Add(this.m_lblSafeCylinderRadiusUnit, 2, 0);
			this.m_pnlSafeCylinder.Location = new System.Drawing.Point(10, 26);
			this.m_pnlSafeCylinder.Name = "m_pnlSafeCylinder";
			this.m_pnlSafeCylinder.RowCount = 1;
			this.m_pnlSafeCylinder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_pnlSafeCylinder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.m_pnlSafeCylinder.Size = new System.Drawing.Size(341, 46);
			this.m_pnlSafeCylinder.TabIndex = 42;
			// 
			// m_lblSafeCylinderRadius
			// 
			this.m_lblSafeCylinderRadius.AutoSize = true;
			this.m_lblSafeCylinderRadius.Location = new System.Drawing.Point(6, 0);
			this.m_lblSafeCylinderRadius.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblSafeCylinderRadius.Name = "m_lblSafeCylinderRadius";
			this.m_lblSafeCylinderRadius.Size = new System.Drawing.Size(69, 19);
			this.m_lblSafeCylinderRadius.TabIndex = 19;
			this.m_lblSafeCylinderRadius.Text = "圓柱半徑";
			// 
			// m_NumericUpDownSafeCylinderRadius
			// 
			this.m_NumericUpDownSafeCylinderRadius.DecimalPlaces = 3;
			this.m_NumericUpDownSafeCylinderRadius.Location = new System.Drawing.Point(116, 3);
			this.m_NumericUpDownSafeCylinderRadius.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_NumericUpDownSafeCylinderRadius.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_NumericUpDownSafeCylinderRadius.Name = "m_NumericUpDownSafeCylinderRadius";
			this.m_NumericUpDownSafeCylinderRadius.RestrictInvalidCharacters = false;
			this.m_NumericUpDownSafeCylinderRadius.Size = new System.Drawing.Size(92, 27);
			this.m_NumericUpDownSafeCylinderRadius.TabIndex = 38;
			this.m_NumericUpDownSafeCylinderRadius.DebouncedValueChanged += new System.EventHandler(this.m_NumericUpDownSafeCylinderRadius_DebouncedValueChanged);
			// 
			// m_lblSafeCylinderRadiusUnit
			// 
			this.m_lblSafeCylinderRadiusUnit.AutoSize = true;
			this.m_lblSafeCylinderRadiusUnit.Location = new System.Drawing.Point(230, 0);
			this.m_lblSafeCylinderRadiusUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblSafeCylinderRadiusUnit.Name = "m_lblSafeCylinderRadiusUnit";
			this.m_lblSafeCylinderRadiusUnit.Size = new System.Drawing.Size(37, 19);
			this.m_lblSafeCylinderRadiusUnit.TabIndex = 25;
			this.m_lblSafeCylinderRadiusUnit.Text = "mm";
			// 
			// m_pnlSafeSphere
			// 
			this.m_pnlSafeSphere.ColumnCount = 3;
			this.m_pnlSafeSphere.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.m_pnlSafeSphere.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.m_pnlSafeSphere.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.m_pnlSafeSphere.Controls.Add(this.m_lblSafeSphereRadius, 0, 0);
			this.m_pnlSafeSphere.Controls.Add(this.m_NumericUpDownSafeSphereRadius, 1, 0);
			this.m_pnlSafeSphere.Controls.Add(this.m_lblSafeSphereRadiusUnit, 2, 0);
			this.m_pnlSafeSphere.Location = new System.Drawing.Point(10, 26);
			this.m_pnlSafeSphere.Name = "m_pnlSafeSphere";
			this.m_pnlSafeSphere.RowCount = 1;
			this.m_pnlSafeSphere.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_pnlSafeSphere.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.m_pnlSafeSphere.Size = new System.Drawing.Size(341, 46);
			this.m_pnlSafeSphere.TabIndex = 43;
			// 
			// m_lblSafeSphereRadius
			// 
			this.m_lblSafeSphereRadius.AutoSize = true;
			this.m_lblSafeSphereRadius.Location = new System.Drawing.Point(6, 0);
			this.m_lblSafeSphereRadius.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblSafeSphereRadius.Name = "m_lblSafeSphereRadius";
			this.m_lblSafeSphereRadius.Size = new System.Drawing.Size(54, 19);
			this.m_lblSafeSphereRadius.TabIndex = 19;
			this.m_lblSafeSphereRadius.Text = "球半徑";
			// 
			// m_NumericUpDownSafeSphereRadius
			// 
			this.m_NumericUpDownSafeSphereRadius.DecimalPlaces = 3;
			this.m_NumericUpDownSafeSphereRadius.Location = new System.Drawing.Point(116, 3);
			this.m_NumericUpDownSafeSphereRadius.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_NumericUpDownSafeSphereRadius.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_NumericUpDownSafeSphereRadius.Name = "m_NumericUpDownSafeSphereRadius";
			this.m_NumericUpDownSafeSphereRadius.RestrictInvalidCharacters = false;
			this.m_NumericUpDownSafeSphereRadius.Size = new System.Drawing.Size(92, 27);
			this.m_NumericUpDownSafeSphereRadius.TabIndex = 38;
			this.m_NumericUpDownSafeSphereRadius.DebouncedValueChanged += new System.EventHandler(this.m_NumericUpDownSafeSphereRadius_DebouncedValueChanged);
			// 
			// m_lblSafeSphereRadiusUnit
			// 
			this.m_lblSafeSphereRadiusUnit.AutoSize = true;
			this.m_lblSafeSphereRadiusUnit.Location = new System.Drawing.Point(230, 0);
			this.m_lblSafeSphereRadiusUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblSafeSphereRadiusUnit.Name = "m_lblSafeSphereRadiusUnit";
			this.m_lblSafeSphereRadiusUnit.Size = new System.Drawing.Size(37, 19);
			this.m_lblSafeSphereRadiusUnit.TabIndex = 25;
			this.m_lblSafeSphereRadiusUnit.Text = "mm";
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Location = new System.Drawing.Point(308, 251);
			this.m_btnConfirm.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(71, 29);
			this.m_btnConfirm.TabIndex = 18;
			this.m_btnConfirm.Text = "確認";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// TraverseDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(388, 289);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.m_btnConfirm);
			this.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "TraverseDlg";
			this.Text = "空移參數設定";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownLiftUpDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownCutDownDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownFollowSafeDistance)).EndInit();
			this.m_gbxTraverse.ResumeLayout(false);
			this.m_pnlFrogLeap.ResumeLayout(false);
			this.m_pnlFrogLeap.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownFrogLeapDistance)).EndInit();
			this.m_pnlSafePlane.ResumeLayout(false);
			this.m_pnlSafePlane.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownSafePlaneDistance)).EndInit();
			this.m_pnlSafeCylinder.ResumeLayout(false);
			this.m_pnlSafeCylinder.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownSafeCylinderRadius)).EndInit();
			this.m_pnlSafeSphere.ResumeLayout(false);
			this.m_pnlSafeSphere.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownSafeSphereRadius)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label m_lblLiftUpDistance;
		private System.Windows.Forms.Label m_lblCutDownDistance;
		private System.Windows.Forms.Label m_lblFollowSaveDistance;
		private System.Windows.Forms.Label m_lblFrogLeapDistance;
		private System.Windows.Forms.Label m_lblFollowSaveDistanceUnit;
		private System.Windows.Forms.Label m_lblCutDownDistanceUnit;
		private System.Windows.Forms.Label m_lblLiftUpDistanceUnit;
		private System.Windows.Forms.Label m_lblFrogLeapDistanceUnit;
		private System.Windows.Forms.Label m_lblSafePlaneDistance;
		private System.Windows.Forms.Label m_lblSafePlaneDistanceUnit;
		private System.Windows.Forms.Button m_btnConfirm;
		private MyCAM.Editor.DebouncedNumericUpDown m_NumericUpDownSafePlaneDistance;
		private MyCAM.Editor.DebouncedNumericUpDown m_NumericUpDownLiftUpDistance;
		private MyCAM.Editor.DebouncedNumericUpDown m_NumericUpDownCutDownDistance;
		private MyCAM.Editor.DebouncedNumericUpDown m_NumericUpDownFollowSafeDistance;
		private MyCAM.Editor.DebouncedNumericUpDown m_NumericUpDownFrogLeapDistance;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.RadioButton m_rbtnFrogLeap;
		private System.Windows.Forms.RadioButton m_rbtnSafePlane;
		private System.Windows.Forms.RadioButton m_rbtnSafeCylinder;
		private System.Windows.Forms.RadioButton m_rbtnSafeSphere;
		private System.Windows.Forms.GroupBox m_gbxTraverse;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.TableLayoutPanel m_pnlFrogLeap;
		private System.Windows.Forms.TableLayoutPanel m_pnlSafePlane;
		private System.Windows.Forms.TableLayoutPanel m_pnlSafeCylinder;
		private System.Windows.Forms.Label m_lblSafeCylinderRadius;
		private DebouncedNumericUpDown m_NumericUpDownSafeCylinderRadius;
		private System.Windows.Forms.Label m_lblSafeCylinderRadiusUnit;
		private System.Windows.Forms.TableLayoutPanel m_pnlSafeSphere;
		private System.Windows.Forms.Label m_lblSafeSphereRadius;
		private DebouncedNumericUpDown m_NumericUpDownSafeSphereRadius;
		private System.Windows.Forms.Label m_lblSafeSphereRadiusUnit;
	}
}
