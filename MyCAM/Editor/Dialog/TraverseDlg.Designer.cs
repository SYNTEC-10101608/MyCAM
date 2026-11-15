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
			this.m_secDistance = new CommonUI.Controls.SectionPanel();
			this.m_lblLiftUpDistance = new System.Windows.Forms.Label();
			this.m_NumericUpDownLiftUpDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblLiftUpDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblCutDownDistance = new System.Windows.Forms.Label();
			this.m_NumericUpDownCutDownDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblCutDownDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblFollowSaveDistance = new System.Windows.Forms.Label();
			this.m_NumericUpDownFollowSafeDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblFollowSaveDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_secTraverseMode = new CommonUI.Controls.SectionPanel();
			this.m_pnlTraverseMode = new System.Windows.Forms.Panel();
			this.m_rbtnFrogLeap = new System.Windows.Forms.RadioButton();
			this.m_rbtnSafePlane = new System.Windows.Forms.RadioButton();
			this.m_rbtnSafeCylinder = new System.Windows.Forms.RadioButton();
			this.m_rbtnSafeSphere = new System.Windows.Forms.RadioButton();
			this.m_gbxTraverse = new CommonUI.Controls.SectionPanel();
			this.m_pnlFrogLeap = new System.Windows.Forms.Panel();
			this.m_lblFrogLeapDistance = new System.Windows.Forms.Label();
			this.m_NumericUpDownFrogLeapDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblFrogLeapDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_pnlSafePlane = new System.Windows.Forms.Panel();
			this.m_lblSafePlaneDistance = new System.Windows.Forms.Label();
			this.m_NumericUpDownSafePlaneDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblSafePlaneDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_pnlSafeCylinder = new System.Windows.Forms.Panel();
			this.m_lblSafeCylinderRadius = new System.Windows.Forms.Label();
			this.m_NumericUpDownSafeCylinderRadius = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblSafeCylinderRadiusUnit = new CommonUI.Controls.ThemedLabel();
			this.m_pnlSafeSphere = new System.Windows.Forms.Panel();
			this.m_lblSafeSphereRadius = new System.Windows.Forms.Label();
			this.m_NumericUpDownSafeSphereRadius = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblSafeSphereRadiusUnit = new CommonUI.Controls.ThemedLabel();
			this.m_btnConfirm = new CommonUI.Controls.ThemedButton();
			this.m_secDistance.SuspendLayout();
			this.m_secTraverseMode.SuspendLayout();
			this.m_pnlTraverseMode.SuspendLayout();
			this.m_gbxTraverse.SuspendLayout();
			this.m_pnlFrogLeap.SuspendLayout();
			this.m_pnlSafePlane.SuspendLayout();
			this.m_pnlSafeCylinder.SuspendLayout();
			this.m_pnlSafeSphere.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownLiftUpDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownCutDownDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownFollowSafeDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownFrogLeapDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownSafePlaneDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownSafeCylinderRadius)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownSafeSphereRadius)).BeginInit();
			this.SuspendLayout();
			// 
			// m_secDistance
			// 
			this.m_secDistance.Controls.Add(this.m_lblLiftUpDistance);
			this.m_secDistance.Controls.Add(this.m_NumericUpDownLiftUpDistance);
			this.m_secDistance.Controls.Add(this.m_lblLiftUpDistanceUnit);
			this.m_secDistance.Controls.Add(this.m_lblCutDownDistance);
			this.m_secDistance.Controls.Add(this.m_NumericUpDownCutDownDistance);
			this.m_secDistance.Controls.Add(this.m_lblCutDownDistanceUnit);
			this.m_secDistance.Controls.Add(this.m_lblFollowSaveDistance);
			this.m_secDistance.Controls.Add(this.m_NumericUpDownFollowSafeDistance);
			this.m_secDistance.Controls.Add(this.m_lblFollowSaveDistanceUnit);
			this.m_secDistance.Location = new System.Drawing.Point(12, 12);
			this.m_secDistance.Name = "m_secDistance";
			this.m_secDistance.Size = new System.Drawing.Size(356, 103);
			this.m_secDistance.TabIndex = 0;
			// 
			// m_lblLiftUpDistance
			// 
			this.m_lblLiftUpDistance.AutoSize = true;
			this.m_lblLiftUpDistance.Location = new System.Drawing.Point(10, 14);
			this.m_lblLiftUpDistance.Name = "m_lblLiftUpDistance";
			this.m_lblLiftUpDistance.Size = new System.Drawing.Size(55, 16);
			this.m_lblLiftUpDistance.TabIndex = 0;
			this.m_lblLiftUpDistance.Text = "抬刀距離";
			// 
			// m_NumericUpDownLiftUpDistance
			// 
			this.m_NumericUpDownLiftUpDistance.DecimalPlaces = 3;
			this.m_NumericUpDownLiftUpDistance.Location = new System.Drawing.Point(110, 10);
			this.m_NumericUpDownLiftUpDistance.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_NumericUpDownLiftUpDistance.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_NumericUpDownLiftUpDistance.Name = "m_NumericUpDownLiftUpDistance";
			this.m_NumericUpDownLiftUpDistance.RestrictInvalidCharacters = false;
			this.m_NumericUpDownLiftUpDistance.Size = new System.Drawing.Size(150, 23);
			this.m_NumericUpDownLiftUpDistance.TabIndex = 1;
			this.m_NumericUpDownLiftUpDistance.DebouncedValueChanged += new System.EventHandler(this.m_NumericUpDownLiftUpDistance_DebouncedValueChanged);
			this.m_NumericUpDownLiftUpDistance.ValueChanged += new System.EventHandler(this.m_NumericUpDownCutDownDistance_ValueChanged);
			// 
			// m_lblLiftUpDistanceUnit
			// 
			this.m_lblLiftUpDistanceUnit.AutoSize = true;
			this.m_lblLiftUpDistanceUnit.Location = new System.Drawing.Point(266, 14);
			this.m_lblLiftUpDistanceUnit.Name = "m_lblLiftUpDistanceUnit";
			this.m_lblLiftUpDistanceUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblLiftUpDistanceUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblLiftUpDistanceUnit.TabIndex = 2;
			this.m_lblLiftUpDistanceUnit.Text = "mm";
			// 
			// m_lblCutDownDistance
			// 
			this.m_lblCutDownDistance.AutoSize = true;
			this.m_lblCutDownDistance.Location = new System.Drawing.Point(10, 44);
			this.m_lblCutDownDistance.Name = "m_lblCutDownDistance";
			this.m_lblCutDownDistance.Size = new System.Drawing.Size(55, 16);
			this.m_lblCutDownDistance.TabIndex = 3;
			this.m_lblCutDownDistance.Text = "下刀距離";
			// 
			// m_NumericUpDownCutDownDistance
			// 
			this.m_NumericUpDownCutDownDistance.DecimalPlaces = 3;
			this.m_NumericUpDownCutDownDistance.Location = new System.Drawing.Point(110, 40);
			this.m_NumericUpDownCutDownDistance.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_NumericUpDownCutDownDistance.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_NumericUpDownCutDownDistance.Name = "m_NumericUpDownCutDownDistance";
			this.m_NumericUpDownCutDownDistance.RestrictInvalidCharacters = false;
			this.m_NumericUpDownCutDownDistance.Size = new System.Drawing.Size(150, 23);
			this.m_NumericUpDownCutDownDistance.TabIndex = 4;
			this.m_NumericUpDownCutDownDistance.DebouncedValueChanged += new System.EventHandler(this.m_NumericUpDownCutDownDistance_DebouncedValueChanged);
			// 
			// m_lblCutDownDistanceUnit
			// 
			this.m_lblCutDownDistanceUnit.AutoSize = true;
			this.m_lblCutDownDistanceUnit.Location = new System.Drawing.Point(266, 44);
			this.m_lblCutDownDistanceUnit.Name = "m_lblCutDownDistanceUnit";
			this.m_lblCutDownDistanceUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblCutDownDistanceUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblCutDownDistanceUnit.TabIndex = 5;
			this.m_lblCutDownDistanceUnit.Text = "mm";
			// 
			// m_lblFollowSaveDistance
			// 
			this.m_lblFollowSaveDistance.AutoSize = true;
			this.m_lblFollowSaveDistance.Location = new System.Drawing.Point(10, 74);
			this.m_lblFollowSaveDistance.Name = "m_lblFollowSaveDistance";
			this.m_lblFollowSaveDistance.Size = new System.Drawing.Size(79, 16);
			this.m_lblFollowSaveDistance.TabIndex = 6;
			this.m_lblFollowSaveDistance.Text = "隨動安全距離";
			// 
			// m_NumericUpDownFollowSafeDistance
			// 
			this.m_NumericUpDownFollowSafeDistance.DecimalPlaces = 3;
			this.m_NumericUpDownFollowSafeDistance.Location = new System.Drawing.Point(110, 70);
			this.m_NumericUpDownFollowSafeDistance.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_NumericUpDownFollowSafeDistance.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_NumericUpDownFollowSafeDistance.Name = "m_NumericUpDownFollowSafeDistance";
			this.m_NumericUpDownFollowSafeDistance.RestrictInvalidCharacters = false;
			this.m_NumericUpDownFollowSafeDistance.Size = new System.Drawing.Size(150, 23);
			this.m_NumericUpDownFollowSafeDistance.TabIndex = 7;
			this.m_NumericUpDownFollowSafeDistance.DebouncedValueChanged += new System.EventHandler(this.m_NumericUpDownFollowSafeDistance_DebouncedValueChanged);
			// 
			// m_lblFollowSaveDistanceUnit
			// 
			this.m_lblFollowSaveDistanceUnit.AutoSize = true;
			this.m_lblFollowSaveDistanceUnit.Location = new System.Drawing.Point(266, 74);
			this.m_lblFollowSaveDistanceUnit.Name = "m_lblFollowSaveDistanceUnit";
			this.m_lblFollowSaveDistanceUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblFollowSaveDistanceUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblFollowSaveDistanceUnit.TabIndex = 8;
			this.m_lblFollowSaveDistanceUnit.Text = "mm";
			// 
			// m_secTraverseMode
			// 
			this.m_secTraverseMode.Controls.Add(this.m_pnlTraverseMode);
			this.m_secTraverseMode.Location = new System.Drawing.Point(12, 123);
			this.m_secTraverseMode.Name = "m_secTraverseMode";
			this.m_secTraverseMode.Size = new System.Drawing.Size(356, 43);
			this.m_secTraverseMode.TabIndex = 1;
			// 
			// m_pnlTraverseMode
			// 
			this.m_pnlTraverseMode.Controls.Add(this.m_rbtnFrogLeap);
			this.m_pnlTraverseMode.Controls.Add(this.m_rbtnSafePlane);
			this.m_pnlTraverseMode.Controls.Add(this.m_rbtnSafeCylinder);
			this.m_pnlTraverseMode.Controls.Add(this.m_rbtnSafeSphere);
			this.m_pnlTraverseMode.Location = new System.Drawing.Point(10, 10);
			this.m_pnlTraverseMode.Name = "m_pnlTraverseMode";
			this.m_pnlTraverseMode.Size = new System.Drawing.Size(336, 23);
			this.m_pnlTraverseMode.TabIndex = 0;
			// 
			// m_rbtnFrogLeap
			// 
			this.m_rbtnFrogLeap.AutoSize = true;
			this.m_rbtnFrogLeap.Location = new System.Drawing.Point(0, 2);
			this.m_rbtnFrogLeap.Name = "m_rbtnFrogLeap";
			this.m_rbtnFrogLeap.Size = new System.Drawing.Size(49, 20);
			this.m_rbtnFrogLeap.TabIndex = 0;
			this.m_rbtnFrogLeap.TabStop = true;
			this.m_rbtnFrogLeap.Text = "蛙跳";
			this.m_rbtnFrogLeap.CheckedChanged += new System.EventHandler(this.TraverseMode_CheckChanged);
			// 
			// m_rbtnSafePlane
			// 
			this.m_rbtnSafePlane.AutoSize = true;
			this.m_rbtnSafePlane.Location = new System.Drawing.Point(70, 2);
			this.m_rbtnSafePlane.Name = "m_rbtnSafePlane";
			this.m_rbtnSafePlane.Size = new System.Drawing.Size(73, 20);
			this.m_rbtnSafePlane.TabIndex = 1;
			this.m_rbtnSafePlane.TabStop = true;
			this.m_rbtnSafePlane.Text = "安全平面";
			this.m_rbtnSafePlane.CheckedChanged += new System.EventHandler(this.TraverseMode_CheckChanged);
			// 
			// m_rbtnSafeCylinder
			// 
			this.m_rbtnSafeCylinder.AutoSize = true;
			this.m_rbtnSafeCylinder.Location = new System.Drawing.Point(160, 2);
			this.m_rbtnSafeCylinder.Name = "m_rbtnSafeCylinder";
			this.m_rbtnSafeCylinder.Size = new System.Drawing.Size(73, 20);
			this.m_rbtnSafeCylinder.TabIndex = 2;
			this.m_rbtnSafeCylinder.TabStop = true;
			this.m_rbtnSafeCylinder.Text = "安全圓柱";
			this.m_rbtnSafeCylinder.CheckedChanged += new System.EventHandler(this.TraverseMode_CheckChanged);
			// 
			// m_rbtnSafeSphere
			// 
			this.m_rbtnSafeSphere.AutoSize = true;
			this.m_rbtnSafeSphere.Location = new System.Drawing.Point(250, 2);
			this.m_rbtnSafeSphere.Name = "m_rbtnSafeSphere";
			this.m_rbtnSafeSphere.Size = new System.Drawing.Size(73, 20);
			this.m_rbtnSafeSphere.TabIndex = 3;
			this.m_rbtnSafeSphere.TabStop = true;
			this.m_rbtnSafeSphere.Text = "安全圓球";
			this.m_rbtnSafeSphere.CheckedChanged += new System.EventHandler(this.TraverseMode_CheckChanged);
			// 
			// m_gbxTraverse
			// 
			this.m_gbxTraverse.Controls.Add(this.m_pnlFrogLeap);
			this.m_gbxTraverse.Controls.Add(this.m_pnlSafePlane);
			this.m_gbxTraverse.Controls.Add(this.m_pnlSafeCylinder);
			this.m_gbxTraverse.Controls.Add(this.m_pnlSafeSphere);
			this.m_gbxTraverse.Location = new System.Drawing.Point(12, 174);
			this.m_gbxTraverse.Name = "m_gbxTraverse";
			this.m_gbxTraverse.Size = new System.Drawing.Size(356, 69);
			this.m_gbxTraverse.TabIndex = 2;
			this.m_gbxTraverse.Text = "空移參數";
			// 
			// m_pnlFrogLeap
			// 
			this.m_pnlFrogLeap.Controls.Add(this.m_lblFrogLeapDistance);
			this.m_pnlFrogLeap.Controls.Add(this.m_NumericUpDownFrogLeapDistance);
			this.m_pnlFrogLeap.Controls.Add(this.m_lblFrogLeapDistanceUnit);
			this.m_pnlFrogLeap.Location = new System.Drawing.Point(10, 36);
			this.m_pnlFrogLeap.Name = "m_pnlFrogLeap";
			this.m_pnlFrogLeap.Size = new System.Drawing.Size(336, 23);
			this.m_pnlFrogLeap.TabIndex = 0;
			// 
			// m_lblFrogLeapDistance
			// 
			this.m_lblFrogLeapDistance.AutoSize = true;
			this.m_lblFrogLeapDistance.Location = new System.Drawing.Point(0, 4);
			this.m_lblFrogLeapDistance.Name = "m_lblFrogLeapDistance";
			this.m_lblFrogLeapDistance.Size = new System.Drawing.Size(55, 16);
			this.m_lblFrogLeapDistance.TabIndex = 0;
			this.m_lblFrogLeapDistance.Text = "蛙跳距離";
			// 
			// m_NumericUpDownFrogLeapDistance
			// 
			this.m_NumericUpDownFrogLeapDistance.DecimalPlaces = 3;
			this.m_NumericUpDownFrogLeapDistance.Location = new System.Drawing.Point(100, 0);
			this.m_NumericUpDownFrogLeapDistance.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_NumericUpDownFrogLeapDistance.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_NumericUpDownFrogLeapDistance.Name = "m_NumericUpDownFrogLeapDistance";
			this.m_NumericUpDownFrogLeapDistance.RestrictInvalidCharacters = false;
			this.m_NumericUpDownFrogLeapDistance.Size = new System.Drawing.Size(150, 23);
			this.m_NumericUpDownFrogLeapDistance.TabIndex = 1;
			this.m_NumericUpDownFrogLeapDistance.DebouncedValueChanged += new System.EventHandler(this.m_NumericUpDownFrogLeapDistance_DebouncedValueChanged);
			// 
			// m_lblFrogLeapDistanceUnit
			// 
			this.m_lblFrogLeapDistanceUnit.AutoSize = true;
			this.m_lblFrogLeapDistanceUnit.Location = new System.Drawing.Point(256, 4);
			this.m_lblFrogLeapDistanceUnit.Name = "m_lblFrogLeapDistanceUnit";
			this.m_lblFrogLeapDistanceUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblFrogLeapDistanceUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblFrogLeapDistanceUnit.TabIndex = 2;
			this.m_lblFrogLeapDistanceUnit.Text = "mm";
			// 
			// m_pnlSafePlane
			// 
			this.m_pnlSafePlane.Controls.Add(this.m_lblSafePlaneDistance);
			this.m_pnlSafePlane.Controls.Add(this.m_NumericUpDownSafePlaneDistance);
			this.m_pnlSafePlane.Controls.Add(this.m_lblSafePlaneDistanceUnit);
			this.m_pnlSafePlane.Location = new System.Drawing.Point(10, 36);
			this.m_pnlSafePlane.Name = "m_pnlSafePlane";
			this.m_pnlSafePlane.Size = new System.Drawing.Size(336, 23);
			this.m_pnlSafePlane.TabIndex = 1;
			// 
			// m_lblSafePlaneDistance
			// 
			this.m_lblSafePlaneDistance.AutoSize = true;
			this.m_lblSafePlaneDistance.Location = new System.Drawing.Point(0, 4);
			this.m_lblSafePlaneDistance.Name = "m_lblSafePlaneDistance";
			this.m_lblSafePlaneDistance.Size = new System.Drawing.Size(63, 16);
			this.m_lblSafePlaneDistance.TabIndex = 0;
			this.m_lblSafePlaneDistance.Text = "Z平面位置";
			// 
			// m_NumericUpDownSafePlaneDistance
			// 
			this.m_NumericUpDownSafePlaneDistance.DecimalPlaces = 3;
			this.m_NumericUpDownSafePlaneDistance.Location = new System.Drawing.Point(100, 0);
			this.m_NumericUpDownSafePlaneDistance.Maximum = new decimal(new int[] { 1410065407, 2, 0, 0});
			this.m_NumericUpDownSafePlaneDistance.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_NumericUpDownSafePlaneDistance.Name = "m_NumericUpDownSafePlaneDistance";
			this.m_NumericUpDownSafePlaneDistance.RestrictInvalidCharacters = false;
			this.m_NumericUpDownSafePlaneDistance.Size = new System.Drawing.Size(150, 23);
			this.m_NumericUpDownSafePlaneDistance.TabIndex = 1;
			this.m_NumericUpDownSafePlaneDistance.DebouncedValueChanged += new System.EventHandler(this.m_NumericUpDownSafePlaneDistance_DebouncedValueChanged);
			// 
			// m_lblSafePlaneDistanceUnit
			// 
			this.m_lblSafePlaneDistanceUnit.AutoSize = true;
			this.m_lblSafePlaneDistanceUnit.Location = new System.Drawing.Point(256, 4);
			this.m_lblSafePlaneDistanceUnit.Name = "m_lblSafePlaneDistanceUnit";
			this.m_lblSafePlaneDistanceUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblSafePlaneDistanceUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblSafePlaneDistanceUnit.TabIndex = 2;
			this.m_lblSafePlaneDistanceUnit.Text = "mm";
			// 
			// m_pnlSafeCylinder
			// 
			this.m_pnlSafeCylinder.Controls.Add(this.m_lblSafeCylinderRadius);
			this.m_pnlSafeCylinder.Controls.Add(this.m_NumericUpDownSafeCylinderRadius);
			this.m_pnlSafeCylinder.Controls.Add(this.m_lblSafeCylinderRadiusUnit);
			this.m_pnlSafeCylinder.Location = new System.Drawing.Point(10, 36);
			this.m_pnlSafeCylinder.Name = "m_pnlSafeCylinder";
			this.m_pnlSafeCylinder.Size = new System.Drawing.Size(336, 23);
			this.m_pnlSafeCylinder.TabIndex = 2;
			// 
			// m_lblSafeCylinderRadius
			// 
			this.m_lblSafeCylinderRadius.AutoSize = true;
			this.m_lblSafeCylinderRadius.Location = new System.Drawing.Point(0, 4);
			this.m_lblSafeCylinderRadius.Name = "m_lblSafeCylinderRadius";
			this.m_lblSafeCylinderRadius.Size = new System.Drawing.Size(55, 16);
			this.m_lblSafeCylinderRadius.TabIndex = 0;
			this.m_lblSafeCylinderRadius.Text = "圓柱半徑";
			// 
			// m_NumericUpDownSafeCylinderRadius
			// 
			this.m_NumericUpDownSafeCylinderRadius.DecimalPlaces = 3;
			this.m_NumericUpDownSafeCylinderRadius.Location = new System.Drawing.Point(100, 0);
			this.m_NumericUpDownSafeCylinderRadius.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_NumericUpDownSafeCylinderRadius.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_NumericUpDownSafeCylinderRadius.Name = "m_NumericUpDownSafeCylinderRadius";
			this.m_NumericUpDownSafeCylinderRadius.RestrictInvalidCharacters = false;
			this.m_NumericUpDownSafeCylinderRadius.Size = new System.Drawing.Size(150, 23);
			this.m_NumericUpDownSafeCylinderRadius.TabIndex = 1;
			this.m_NumericUpDownSafeCylinderRadius.DebouncedValueChanged += new System.EventHandler(this.m_NumericUpDownSafeCylinderRadius_DebouncedValueChanged);
			// 
			// m_lblSafeCylinderRadiusUnit
			// 
			this.m_lblSafeCylinderRadiusUnit.AutoSize = true;
			this.m_lblSafeCylinderRadiusUnit.Location = new System.Drawing.Point(256, 4);
			this.m_lblSafeCylinderRadiusUnit.Name = "m_lblSafeCylinderRadiusUnit";
			this.m_lblSafeCylinderRadiusUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblSafeCylinderRadiusUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblSafeCylinderRadiusUnit.TabIndex = 2;
			this.m_lblSafeCylinderRadiusUnit.Text = "mm";
			// 
			// m_pnlSafeSphere
			// 
			this.m_pnlSafeSphere.Controls.Add(this.m_lblSafeSphereRadius);
			this.m_pnlSafeSphere.Controls.Add(this.m_NumericUpDownSafeSphereRadius);
			this.m_pnlSafeSphere.Controls.Add(this.m_lblSafeSphereRadiusUnit);
			this.m_pnlSafeSphere.Location = new System.Drawing.Point(10, 36);
			this.m_pnlSafeSphere.Name = "m_pnlSafeSphere";
			this.m_pnlSafeSphere.Size = new System.Drawing.Size(336, 23);
			this.m_pnlSafeSphere.TabIndex = 3;
			// 
			// m_lblSafeSphereRadius
			// 
			this.m_lblSafeSphereRadius.AutoSize = true;
			this.m_lblSafeSphereRadius.Location = new System.Drawing.Point(0, 4);
			this.m_lblSafeSphereRadius.Name = "m_lblSafeSphereRadius";
			this.m_lblSafeSphereRadius.Size = new System.Drawing.Size(43, 16);
			this.m_lblSafeSphereRadius.TabIndex = 0;
			this.m_lblSafeSphereRadius.Text = "球半徑";
			// 
			// m_NumericUpDownSafeSphereRadius
			// 
			this.m_NumericUpDownSafeSphereRadius.DecimalPlaces = 3;
			this.m_NumericUpDownSafeSphereRadius.Location = new System.Drawing.Point(100, 0);
			this.m_NumericUpDownSafeSphereRadius.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_NumericUpDownSafeSphereRadius.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_NumericUpDownSafeSphereRadius.Name = "m_NumericUpDownSafeSphereRadius";
			this.m_NumericUpDownSafeSphereRadius.RestrictInvalidCharacters = false;
			this.m_NumericUpDownSafeSphereRadius.Size = new System.Drawing.Size(150, 23);
			this.m_NumericUpDownSafeSphereRadius.TabIndex = 1;
			this.m_NumericUpDownSafeSphereRadius.DebouncedValueChanged += new System.EventHandler(this.m_NumericUpDownSafeSphereRadius_DebouncedValueChanged);
			// 
			// m_lblSafeSphereRadiusUnit
			// 
			this.m_lblSafeSphereRadiusUnit.AutoSize = true;
			this.m_lblSafeSphereRadiusUnit.Location = new System.Drawing.Point(256, 4);
			this.m_lblSafeSphereRadiusUnit.Name = "m_lblSafeSphereRadiusUnit";
			this.m_lblSafeSphereRadiusUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblSafeSphereRadiusUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblSafeSphereRadiusUnit.TabIndex = 2;
			this.m_lblSafeSphereRadiusUnit.Text = "mm";
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnConfirm.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnConfirm.Location = new System.Drawing.Point(288, 259);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(80, 28);
			this.m_btnConfirm.TabIndex = 3;
			this.m_btnConfirm.Text = "確定";
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// TraverseDlg
			// 
			this.ClientSize = new System.Drawing.Size(380, 299);
			this.Controls.Add(this.m_secDistance);
			this.Controls.Add(this.m_secTraverseMode);
			this.Controls.Add(this.m_gbxTraverse);
			this.Controls.Add(this.m_btnConfirm);
			this.Name = "TraverseDlg";
			this.Text = "空移參數設定";
			this.m_secDistance.ResumeLayout(false);
			this.m_secDistance.PerformLayout();
			this.m_secTraverseMode.ResumeLayout(false);
			this.m_pnlTraverseMode.ResumeLayout(false);
			this.m_pnlTraverseMode.PerformLayout();
			this.m_gbxTraverse.ResumeLayout(false);
			this.m_pnlFrogLeap.ResumeLayout(false);
			this.m_pnlFrogLeap.PerformLayout();
			this.m_pnlSafePlane.ResumeLayout(false);
			this.m_pnlSafePlane.PerformLayout();
			this.m_pnlSafeCylinder.ResumeLayout(false);
			this.m_pnlSafeCylinder.PerformLayout();
			this.m_pnlSafeSphere.ResumeLayout(false);
			this.m_pnlSafeSphere.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownLiftUpDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownCutDownDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownFollowSafeDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownFrogLeapDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownSafePlaneDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownSafeCylinderRadius)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownSafeSphereRadius)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_secDistance;
		private System.Windows.Forms.Label m_lblLiftUpDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_NumericUpDownLiftUpDistance;
		private CommonUI.Controls.ThemedLabel m_lblLiftUpDistanceUnit;
		private System.Windows.Forms.Label m_lblCutDownDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_NumericUpDownCutDownDistance;
		private CommonUI.Controls.ThemedLabel m_lblCutDownDistanceUnit;
		private System.Windows.Forms.Label m_lblFollowSaveDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_NumericUpDownFollowSafeDistance;
		private CommonUI.Controls.ThemedLabel m_lblFollowSaveDistanceUnit;
		private CommonUI.Controls.SectionPanel m_secTraverseMode;
		private System.Windows.Forms.Panel m_pnlTraverseMode;
		private System.Windows.Forms.RadioButton m_rbtnFrogLeap;
		private System.Windows.Forms.RadioButton m_rbtnSafePlane;
		private System.Windows.Forms.RadioButton m_rbtnSafeCylinder;
		private System.Windows.Forms.RadioButton m_rbtnSafeSphere;
		private CommonUI.Controls.SectionPanel m_gbxTraverse;
		private System.Windows.Forms.Panel m_pnlFrogLeap;
		private System.Windows.Forms.Label m_lblFrogLeapDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_NumericUpDownFrogLeapDistance;
		private CommonUI.Controls.ThemedLabel m_lblFrogLeapDistanceUnit;
		private System.Windows.Forms.Panel m_pnlSafePlane;
		private System.Windows.Forms.Label m_lblSafePlaneDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_NumericUpDownSafePlaneDistance;
		private CommonUI.Controls.ThemedLabel m_lblSafePlaneDistanceUnit;
		private System.Windows.Forms.Panel m_pnlSafeCylinder;
		private System.Windows.Forms.Label m_lblSafeCylinderRadius;
		private CommonUI.Controls.DebouncedNumericUpDown m_NumericUpDownSafeCylinderRadius;
		private CommonUI.Controls.ThemedLabel m_lblSafeCylinderRadiusUnit;
		private System.Windows.Forms.Panel m_pnlSafeSphere;
		private System.Windows.Forms.Label m_lblSafeSphereRadius;
		private CommonUI.Controls.DebouncedNumericUpDown m_NumericUpDownSafeSphereRadius;
		private CommonUI.Controls.ThemedLabel m_lblSafeSphereRadiusUnit;
		private CommonUI.Controls.ThemedButton m_btnConfirm;
	}
}
