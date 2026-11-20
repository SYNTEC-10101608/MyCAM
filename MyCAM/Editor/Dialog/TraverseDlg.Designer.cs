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
			this.m_pnlDistance = new CommonUI.Controls.SectionPanel();
			this.m_lblLiftUpDistance = new CommonUI.Controls.ThemedLabel();
			this.m_numLiftUpDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblLiftUpDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblCutDownDistance = new CommonUI.Controls.ThemedLabel();
			this.m_numCutDownDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblCutDownDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblFollowSaveDistance = new CommonUI.Controls.ThemedLabel();
			this.m_numFollowSafeDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblFollowSaveDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_pnlTraverseMode = new CommonUI.Controls.SectionPanel();
			this.m_pnlTraverseModeOption = new System.Windows.Forms.Panel();
			this.m_rdoFrogLeap = new System.Windows.Forms.RadioButton();
			this.m_rdoSafePlane = new System.Windows.Forms.RadioButton();
			this.m_rdoSafeCylinder = new System.Windows.Forms.RadioButton();
			this.m_rdoSafeSphere = new System.Windows.Forms.RadioButton();
			this.m_pnlTraverse = new CommonUI.Controls.SectionPanel();
			this.m_pnlFrogLeap = new System.Windows.Forms.Panel();
			this.m_lblFrogLeapDistance = new CommonUI.Controls.ThemedLabel();
			this.m_numFrogLeapDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblFrogLeapDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_pnlSafePlane = new System.Windows.Forms.Panel();
			this.m_lblSafePlaneDistance = new CommonUI.Controls.ThemedLabel();
			this.m_numSafePlaneDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblSafePlaneDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_pnlSafeCylinder = new System.Windows.Forms.Panel();
			this.m_lblSafeCylinderRadius = new CommonUI.Controls.ThemedLabel();
			this.m_numSafeCylinderRadius = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblSafeCylinderRadiusUnit = new CommonUI.Controls.ThemedLabel();
			this.m_pnlSafeSphere = new System.Windows.Forms.Panel();
			this.m_lblSafeSphereRadius = new CommonUI.Controls.ThemedLabel();
			this.m_numSafeSphereRadius = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblSafeSphereRadiusUnit = new CommonUI.Controls.ThemedLabel();
			this.m_btnConfirm = new CommonUI.Controls.ThemedButton();
			this.m_pnlDistance.SuspendLayout();
			this.m_pnlTraverseMode.SuspendLayout();
			this.m_pnlTraverseModeOption.SuspendLayout();
			this.m_pnlTraverse.SuspendLayout();
			this.m_pnlFrogLeap.SuspendLayout();
			this.m_pnlSafePlane.SuspendLayout();
			this.m_pnlSafeCylinder.SuspendLayout();
			this.m_pnlSafeSphere.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numLiftUpDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numCutDownDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numFollowSafeDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numFrogLeapDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numSafePlaneDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numSafeCylinderRadius)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numSafeSphereRadius)).BeginInit();
			this.SuspendLayout();
			// 
			// m_pnlDistance
			// 
			this.m_pnlDistance.Controls.Add(this.m_lblLiftUpDistance);
			this.m_pnlDistance.Controls.Add(this.m_numLiftUpDistance);
			this.m_pnlDistance.Controls.Add(this.m_lblLiftUpDistanceUnit);
			this.m_pnlDistance.Controls.Add(this.m_lblCutDownDistance);
			this.m_pnlDistance.Controls.Add(this.m_numCutDownDistance);
			this.m_pnlDistance.Controls.Add(this.m_lblCutDownDistanceUnit);
			this.m_pnlDistance.Controls.Add(this.m_lblFollowSaveDistance);
			this.m_pnlDistance.Controls.Add(this.m_numFollowSafeDistance);
			this.m_pnlDistance.Controls.Add(this.m_lblFollowSaveDistanceUnit);
			this.m_pnlDistance.Location = new System.Drawing.Point(12, 12);
			this.m_pnlDistance.Name = "m_pnlDistance";
			this.m_pnlDistance.Size = new System.Drawing.Size(356, 103);
			this.m_pnlDistance.TabIndex = 0;
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
			// m_numLiftUpDistance
			// 
			this.m_numLiftUpDistance.DecimalPlaces = 3;
			this.m_numLiftUpDistance.Location = new System.Drawing.Point(110, 10);
			this.m_numLiftUpDistance.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numLiftUpDistance.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_numLiftUpDistance.Name = "m_numLiftUpDistance";
			this.m_numLiftUpDistance.RestrictInvalidCharacters = false;
			this.m_numLiftUpDistance.Size = new System.Drawing.Size(150, 23);
			this.m_numLiftUpDistance.TabIndex = 1;
			this.m_numLiftUpDistance.DebouncedValueChanged += new System.EventHandler(this.m_numLiftUpDistance_DebouncedValueChanged);
			this.m_numLiftUpDistance.ValueChanged += new System.EventHandler(this.m_numCutDownDistance_ValueChanged);
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
			// m_numCutDownDistance
			// 
			this.m_numCutDownDistance.DecimalPlaces = 3;
			this.m_numCutDownDistance.Location = new System.Drawing.Point(110, 40);
			this.m_numCutDownDistance.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numCutDownDistance.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_numCutDownDistance.Name = "m_numCutDownDistance";
			this.m_numCutDownDistance.RestrictInvalidCharacters = false;
			this.m_numCutDownDistance.Size = new System.Drawing.Size(150, 23);
			this.m_numCutDownDistance.TabIndex = 4;
			this.m_numCutDownDistance.DebouncedValueChanged += new System.EventHandler(this.m_numCutDownDistance_DebouncedValueChanged);
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
			// m_numFollowSafeDistance
			// 
			this.m_numFollowSafeDistance.DecimalPlaces = 3;
			this.m_numFollowSafeDistance.Location = new System.Drawing.Point(110, 70);
			this.m_numFollowSafeDistance.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numFollowSafeDistance.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_numFollowSafeDistance.Name = "m_numFollowSafeDistance";
			this.m_numFollowSafeDistance.RestrictInvalidCharacters = false;
			this.m_numFollowSafeDistance.Size = new System.Drawing.Size(150, 23);
			this.m_numFollowSafeDistance.TabIndex = 7;
			this.m_numFollowSafeDistance.DebouncedValueChanged += new System.EventHandler(this.m_numFollowSafeDistance_DebouncedValueChanged);
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
			// m_pnlTraverseMode
			// 
			this.m_pnlTraverseMode.Controls.Add(this.m_pnlTraverseModeOption);
			this.m_pnlTraverseMode.Location = new System.Drawing.Point(12, 123);
			this.m_pnlTraverseMode.Name = "m_pnlTraverseMode";
			this.m_pnlTraverseMode.Size = new System.Drawing.Size(356, 43);
			this.m_pnlTraverseMode.TabIndex = 1;
			// 
			// m_pnlTraverseModeOption
			// 
			this.m_pnlTraverseModeOption.Controls.Add(this.m_rdoFrogLeap);
			this.m_pnlTraverseModeOption.Controls.Add(this.m_rdoSafePlane);
			this.m_pnlTraverseModeOption.Controls.Add(this.m_rdoSafeCylinder);
			this.m_pnlTraverseModeOption.Controls.Add(this.m_rdoSafeSphere);
			this.m_pnlTraverseModeOption.Location = new System.Drawing.Point(10, 10);
			this.m_pnlTraverseModeOption.Name = "m_pnlTraverseModeOption";
			this.m_pnlTraverseModeOption.Size = new System.Drawing.Size(336, 23);
			this.m_pnlTraverseModeOption.TabIndex = 0;
			// 
			// m_rdoFrogLeap
			// 
			this.m_rdoFrogLeap.AutoSize = true;
			this.m_rdoFrogLeap.Location = new System.Drawing.Point(0, 2);
			this.m_rdoFrogLeap.Name = "m_rdoFrogLeap";
			this.m_rdoFrogLeap.Size = new System.Drawing.Size(49, 20);
			this.m_rdoFrogLeap.TabIndex = 0;
			this.m_rdoFrogLeap.TabStop = true;
			this.m_rdoFrogLeap.Text = "蛙跳";
			this.m_rdoFrogLeap.CheckedChanged += new System.EventHandler(this.TraverseMode_CheckChanged);
			// 
			// m_rdoSafePlane
			// 
			this.m_rdoSafePlane.AutoSize = true;
			this.m_rdoSafePlane.Location = new System.Drawing.Point(70, 2);
			this.m_rdoSafePlane.Name = "m_rdoSafePlane";
			this.m_rdoSafePlane.Size = new System.Drawing.Size(73, 20);
			this.m_rdoSafePlane.TabIndex = 1;
			this.m_rdoSafePlane.TabStop = true;
			this.m_rdoSafePlane.Text = "安全平面";
			this.m_rdoSafePlane.CheckedChanged += new System.EventHandler(this.TraverseMode_CheckChanged);
			// 
			// m_rdoSafeCylinder
			// 
			this.m_rdoSafeCylinder.AutoSize = true;
			this.m_rdoSafeCylinder.Location = new System.Drawing.Point(160, 2);
			this.m_rdoSafeCylinder.Name = "m_rdoSafeCylinder";
			this.m_rdoSafeCylinder.Size = new System.Drawing.Size(73, 20);
			this.m_rdoSafeCylinder.TabIndex = 2;
			this.m_rdoSafeCylinder.TabStop = true;
			this.m_rdoSafeCylinder.Text = "安全圓柱";
			this.m_rdoSafeCylinder.CheckedChanged += new System.EventHandler(this.TraverseMode_CheckChanged);
			// 
			// m_rdoSafeSphere
			// 
			this.m_rdoSafeSphere.AutoSize = true;
			this.m_rdoSafeSphere.Location = new System.Drawing.Point(250, 2);
			this.m_rdoSafeSphere.Name = "m_rdoSafeSphere";
			this.m_rdoSafeSphere.Size = new System.Drawing.Size(73, 20);
			this.m_rdoSafeSphere.TabIndex = 3;
			this.m_rdoSafeSphere.TabStop = true;
			this.m_rdoSafeSphere.Text = "安全圓球";
			this.m_rdoSafeSphere.CheckedChanged += new System.EventHandler(this.TraverseMode_CheckChanged);
			// 
			// m_pnlTraverse
			// 
			this.m_pnlTraverse.Controls.Add(this.m_pnlFrogLeap);
			this.m_pnlTraverse.Controls.Add(this.m_pnlSafePlane);
			this.m_pnlTraverse.Controls.Add(this.m_pnlSafeCylinder);
			this.m_pnlTraverse.Controls.Add(this.m_pnlSafeSphere);
			this.m_pnlTraverse.Location = new System.Drawing.Point(12, 174);
			this.m_pnlTraverse.Name = "m_pnlTraverse";
			this.m_pnlTraverse.Size = new System.Drawing.Size(356, 69);
			this.m_pnlTraverse.TabIndex = 2;
			this.m_pnlTraverse.Text = "空移參數";
			// 
			// m_pnlFrogLeap
			// 
			this.m_pnlFrogLeap.Controls.Add(this.m_lblFrogLeapDistance);
			this.m_pnlFrogLeap.Controls.Add(this.m_numFrogLeapDistance);
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
			// m_numFrogLeapDistance
			// 
			this.m_numFrogLeapDistance.DecimalPlaces = 3;
			this.m_numFrogLeapDistance.Location = new System.Drawing.Point(100, 0);
			this.m_numFrogLeapDistance.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numFrogLeapDistance.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_numFrogLeapDistance.Name = "m_numFrogLeapDistance";
			this.m_numFrogLeapDistance.RestrictInvalidCharacters = false;
			this.m_numFrogLeapDistance.Size = new System.Drawing.Size(150, 23);
			this.m_numFrogLeapDistance.TabIndex = 1;
			this.m_numFrogLeapDistance.DebouncedValueChanged += new System.EventHandler(this.m_numFrogLeapDistance_DebouncedValueChanged);
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
			this.m_pnlSafePlane.Controls.Add(this.m_numSafePlaneDistance);
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
			// m_numSafePlaneDistance
			// 
			this.m_numSafePlaneDistance.DecimalPlaces = 3;
			this.m_numSafePlaneDistance.Location = new System.Drawing.Point(100, 0);
			this.m_numSafePlaneDistance.Maximum = new decimal(new int[] { 1410065407, 2, 0, 0});
			this.m_numSafePlaneDistance.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_numSafePlaneDistance.Name = "m_numSafePlaneDistance";
			this.m_numSafePlaneDistance.RestrictInvalidCharacters = false;
			this.m_numSafePlaneDistance.Size = new System.Drawing.Size(150, 23);
			this.m_numSafePlaneDistance.TabIndex = 1;
			this.m_numSafePlaneDistance.DebouncedValueChanged += new System.EventHandler(this.m_numSafePlaneDistance_DebouncedValueChanged);
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
			this.m_pnlSafeCylinder.Controls.Add(this.m_numSafeCylinderRadius);
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
			// m_numSafeCylinderRadius
			// 
			this.m_numSafeCylinderRadius.DecimalPlaces = 3;
			this.m_numSafeCylinderRadius.Location = new System.Drawing.Point(100, 0);
			this.m_numSafeCylinderRadius.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numSafeCylinderRadius.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_numSafeCylinderRadius.Name = "m_numSafeCylinderRadius";
			this.m_numSafeCylinderRadius.RestrictInvalidCharacters = false;
			this.m_numSafeCylinderRadius.Size = new System.Drawing.Size(150, 23);
			this.m_numSafeCylinderRadius.TabIndex = 1;
			this.m_numSafeCylinderRadius.DebouncedValueChanged += new System.EventHandler(this.m_numSafeCylinderRadius_DebouncedValueChanged);
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
			this.m_pnlSafeSphere.Controls.Add(this.m_numSafeSphereRadius);
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
			// m_numSafeSphereRadius
			// 
			this.m_numSafeSphereRadius.DecimalPlaces = 3;
			this.m_numSafeSphereRadius.Location = new System.Drawing.Point(100, 0);
			this.m_numSafeSphereRadius.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numSafeSphereRadius.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_numSafeSphereRadius.Name = "m_numSafeSphereRadius";
			this.m_numSafeSphereRadius.RestrictInvalidCharacters = false;
			this.m_numSafeSphereRadius.Size = new System.Drawing.Size(150, 23);
			this.m_numSafeSphereRadius.TabIndex = 1;
			this.m_numSafeSphereRadius.DebouncedValueChanged += new System.EventHandler(this.m_numSafeSphereRadius_DebouncedValueChanged);
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
			this.Controls.Add(this.m_pnlDistance);
			this.Controls.Add(this.m_pnlTraverseMode);
			this.Controls.Add(this.m_pnlTraverse);
			this.Controls.Add(this.m_btnConfirm);
			this.Name = "TraverseDlg";
			this.Text = "空移參數設定";
			this.m_pnlDistance.ResumeLayout(false);
			this.m_pnlDistance.PerformLayout();
			this.m_pnlTraverseMode.ResumeLayout(false);
			this.m_pnlTraverseModeOption.ResumeLayout(false);
			this.m_pnlTraverseModeOption.PerformLayout();
			this.m_pnlTraverse.ResumeLayout(false);
			this.m_pnlFrogLeap.ResumeLayout(false);
			this.m_pnlFrogLeap.PerformLayout();
			this.m_pnlSafePlane.ResumeLayout(false);
			this.m_pnlSafePlane.PerformLayout();
			this.m_pnlSafeCylinder.ResumeLayout(false);
			this.m_pnlSafeCylinder.PerformLayout();
			this.m_pnlSafeSphere.ResumeLayout(false);
			this.m_pnlSafeSphere.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numLiftUpDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numCutDownDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numFollowSafeDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numFrogLeapDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numSafePlaneDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numSafeCylinderRadius)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numSafeSphereRadius)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlDistance;
		private CommonUI.Controls.ThemedLabel m_lblLiftUpDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_numLiftUpDistance;
		private CommonUI.Controls.ThemedLabel m_lblLiftUpDistanceUnit;
		private CommonUI.Controls.ThemedLabel m_lblCutDownDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_numCutDownDistance;
		private CommonUI.Controls.ThemedLabel m_lblCutDownDistanceUnit;
		private CommonUI.Controls.ThemedLabel m_lblFollowSaveDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_numFollowSafeDistance;
		private CommonUI.Controls.ThemedLabel m_lblFollowSaveDistanceUnit;
		private CommonUI.Controls.SectionPanel m_pnlTraverseMode;
		private System.Windows.Forms.Panel m_pnlTraverseModeOption;
		private System.Windows.Forms.RadioButton m_rdoFrogLeap;
		private System.Windows.Forms.RadioButton m_rdoSafePlane;
		private System.Windows.Forms.RadioButton m_rdoSafeCylinder;
		private System.Windows.Forms.RadioButton m_rdoSafeSphere;
		private CommonUI.Controls.SectionPanel m_pnlTraverse;
		private System.Windows.Forms.Panel m_pnlFrogLeap;
		private CommonUI.Controls.ThemedLabel m_lblFrogLeapDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_numFrogLeapDistance;
		private CommonUI.Controls.ThemedLabel m_lblFrogLeapDistanceUnit;
		private System.Windows.Forms.Panel m_pnlSafePlane;
		private CommonUI.Controls.ThemedLabel m_lblSafePlaneDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_numSafePlaneDistance;
		private CommonUI.Controls.ThemedLabel m_lblSafePlaneDistanceUnit;
		private System.Windows.Forms.Panel m_pnlSafeCylinder;
		private CommonUI.Controls.ThemedLabel m_lblSafeCylinderRadius;
		private CommonUI.Controls.DebouncedNumericUpDown m_numSafeCylinderRadius;
		private CommonUI.Controls.ThemedLabel m_lblSafeCylinderRadiusUnit;
		private System.Windows.Forms.Panel m_pnlSafeSphere;
		private CommonUI.Controls.ThemedLabel m_lblSafeSphereRadius;
		private CommonUI.Controls.DebouncedNumericUpDown m_numSafeSphereRadius;
		private CommonUI.Controls.ThemedLabel m_lblSafeSphereRadiusUnit;
		private CommonUI.Controls.ThemedButton m_btnConfirm;
	}
}
