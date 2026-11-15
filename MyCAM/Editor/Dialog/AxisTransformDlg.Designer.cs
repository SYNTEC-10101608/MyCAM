namespace MyCAM.Editor.Dialog
{
	partial class AxisTransformDlg
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
			this.m_grpDisplacement = new CommonUI.Controls.SectionPanel();
			this.m_lblStepDis = new System.Windows.Forms.Label();
			this.m_txbStepDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblX = new CommonUI.Controls.ThemedLabel();
			this.m_nudX = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblY = new CommonUI.Controls.ThemedLabel();
			this.m_nudY = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblZ = new CommonUI.Controls.ThemedLabel();
			this.m_nudZ = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_grpRotation = new CommonUI.Controls.SectionPanel();
			this.m_lblStapAngle = new System.Windows.Forms.Label();
			this.m_txbStepAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblRx = new CommonUI.Controls.ThemedLabel();
			this.m_nudRx = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblRy = new CommonUI.Controls.ThemedLabel();
			this.m_nudRy = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblRz = new CommonUI.Controls.ThemedLabel();
			this.m_nudRz = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_btnReset = new CommonUI.Controls.ThemedButton();
			this.m_btnCancel = new CommonUI.Controls.ThemedButton();
			this.m_btnOK = new CommonUI.Controls.ThemedButton();
			this.m_grpDisplacement.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbStepDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudZ)).BeginInit();
			this.m_grpRotation.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbStepAngle)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudRx)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudRy)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudRz)).BeginInit();
			this.SuspendLayout();
			//
			// m_grpDisplacement
			//
			this.m_grpDisplacement.Controls.Add(this.m_lblStepDis);
			this.m_grpDisplacement.Controls.Add(this.m_txbStepDistance);
			this.m_grpDisplacement.Controls.Add(this.m_lblX);
			this.m_grpDisplacement.Controls.Add(this.m_nudX);
			this.m_grpDisplacement.Controls.Add(this.m_lblY);
			this.m_grpDisplacement.Controls.Add(this.m_nudY);
			this.m_grpDisplacement.Controls.Add(this.m_lblZ);
			this.m_grpDisplacement.Controls.Add(this.m_nudZ);
			this.m_grpDisplacement.Location = new System.Drawing.Point(12, 12);
			this.m_grpDisplacement.Name = "m_grpDisplacement";
			this.m_grpDisplacement.Size = new System.Drawing.Size(296, 159);
			this.m_grpDisplacement.TabIndex = 0;
			this.m_grpDisplacement.Text = "平移";
			//
			// m_lblStepDis
			//
			this.m_lblStepDis.AutoSize = true;
			this.m_lblStepDis.Location = new System.Drawing.Point(10, 40);
			this.m_lblStepDis.Name = "m_lblStepDis";
			this.m_lblStepDis.Size = new System.Drawing.Size(55, 16);
			this.m_lblStepDis.TabIndex = 0;
			this.m_lblStepDis.Text = "步進距離";
			//
			// m_txbStepDistance
			//
			this.m_txbStepDistance.DecimalPlaces = 3;
			this.m_txbStepDistance.Location = new System.Drawing.Point(100, 36);
			this.m_txbStepDistance.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_txbStepDistance.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.m_txbStepDistance.Name = "m_txbStepDistance";
			this.m_txbStepDistance.Size = new System.Drawing.Size(150, 23);
			this.m_txbStepDistance.TabIndex = 1;
			this.m_txbStepDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbStepDistance_KeyDown);
			this.m_txbStepDistance.Leave += new System.EventHandler(this.m_txbStepDistance_Leave);
			//
			// m_lblX
			//
			this.m_lblX.AutoSize = true;
			this.m_lblX.Location = new System.Drawing.Point(10, 70);
			this.m_lblX.Name = "m_lblX";
			this.m_lblX.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblX.Size = new System.Drawing.Size(15, 16);
			this.m_lblX.TabIndex = 2;
			this.m_lblX.Text = "X";
			//
			// m_nudX
			//
			this.m_nudX.Location = new System.Drawing.Point(100, 66);
			this.m_nudX.Name = "m_nudX";
			this.m_nudX.Size = new System.Drawing.Size(150, 23);
			this.m_nudX.TabIndex = 3;
			this.m_nudX.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_nudX_MouseWheel);
			//
			// m_lblY
			//
			this.m_lblY.AutoSize = true;
			this.m_lblY.Location = new System.Drawing.Point(10, 100);
			this.m_lblY.Name = "m_lblY";
			this.m_lblY.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblY.Size = new System.Drawing.Size(15, 16);
			this.m_lblY.TabIndex = 4;
			this.m_lblY.Text = "Y";
			//
			// m_nudY
			//
			this.m_nudY.Location = new System.Drawing.Point(100, 96);
			this.m_nudY.Name = "m_nudY";
			this.m_nudY.Size = new System.Drawing.Size(150, 23);
			this.m_nudY.TabIndex = 5;
			this.m_nudY.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_nudY_MouseWheel);
			//
			// m_lblZ
			//
			this.m_lblZ.AutoSize = true;
			this.m_lblZ.Location = new System.Drawing.Point(10, 130);
			this.m_lblZ.Name = "m_lblZ";
			this.m_lblZ.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblZ.Size = new System.Drawing.Size(15, 16);
			this.m_lblZ.TabIndex = 6;
			this.m_lblZ.Text = "Z";
			//
			// m_nudZ
			//
			this.m_nudZ.Location = new System.Drawing.Point(100, 126);
			this.m_nudZ.Name = "m_nudZ";
			this.m_nudZ.Size = new System.Drawing.Size(150, 23);
			this.m_nudZ.TabIndex = 7;
			this.m_nudZ.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_nudZ_MouseWheel);
			//
			// m_grpRotation
			//
			this.m_grpRotation.Controls.Add(this.m_lblStapAngle);
			this.m_grpRotation.Controls.Add(this.m_txbStepAngle);
			this.m_grpRotation.Controls.Add(this.m_lblRx);
			this.m_grpRotation.Controls.Add(this.m_nudRx);
			this.m_grpRotation.Controls.Add(this.m_lblRy);
			this.m_grpRotation.Controls.Add(this.m_nudRy);
			this.m_grpRotation.Controls.Add(this.m_lblRz);
			this.m_grpRotation.Controls.Add(this.m_nudRz);
			this.m_grpRotation.Location = new System.Drawing.Point(12, 179);
			this.m_grpRotation.Name = "m_grpRotation";
			this.m_grpRotation.Size = new System.Drawing.Size(296, 159);
			this.m_grpRotation.TabIndex = 1;
			this.m_grpRotation.Text = "旋轉";
			//
			// m_lblStapAngle
			//
			this.m_lblStapAngle.AutoSize = true;
			this.m_lblStapAngle.Location = new System.Drawing.Point(10, 40);
			this.m_lblStapAngle.Name = "m_lblStapAngle";
			this.m_lblStapAngle.Size = new System.Drawing.Size(55, 16);
			this.m_lblStapAngle.TabIndex = 0;
			this.m_lblStapAngle.Text = "步進角度";
			//
			// m_txbStepAngle
			//
			this.m_txbStepAngle.DecimalPlaces = 3;
			this.m_txbStepAngle.Location = new System.Drawing.Point(100, 36);
			this.m_txbStepAngle.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_txbStepAngle.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.m_txbStepAngle.Name = "m_txbStepAngle";
			this.m_txbStepAngle.Size = new System.Drawing.Size(150, 23);
			this.m_txbStepAngle.TabIndex = 1;
			this.m_txbStepAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbStepAngle_KeyDown);
			this.m_txbStepAngle.Leave += new System.EventHandler(this.m_txbStepAngle_Leave);
			//
			// m_lblRx
			//
			this.m_lblRx.AutoSize = true;
			this.m_lblRx.Location = new System.Drawing.Point(10, 70);
			this.m_lblRx.Name = "m_lblRx";
			this.m_lblRx.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblRx.Size = new System.Drawing.Size(24, 16);
			this.m_lblRx.TabIndex = 2;
			this.m_lblRx.Text = "RX";
			//
			// m_nudRx
			//
			this.m_nudRx.Location = new System.Drawing.Point(100, 66);
			this.m_nudRx.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
			this.m_nudRx.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
			this.m_nudRx.Name = "m_nudRx";
			this.m_nudRx.Size = new System.Drawing.Size(150, 23);
			this.m_nudRx.TabIndex = 3;
			this.m_nudRx.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_nudRx_MouseWheel);
			//
			// m_lblRy
			//
			this.m_lblRy.AutoSize = true;
			this.m_lblRy.Location = new System.Drawing.Point(10, 100);
			this.m_lblRy.Name = "m_lblRy";
			this.m_lblRy.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblRy.Size = new System.Drawing.Size(24, 16);
			this.m_lblRy.TabIndex = 4;
			this.m_lblRy.Text = "RY";
			//
			// m_nudRy
			//
			this.m_nudRy.Location = new System.Drawing.Point(100, 96);
			this.m_nudRy.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
			this.m_nudRy.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
			this.m_nudRy.Name = "m_nudRy";
			this.m_nudRy.Size = new System.Drawing.Size(150, 23);
			this.m_nudRy.TabIndex = 5;
			this.m_nudRy.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_nudRy_MouseWheel);
			//
			// m_lblRz
			//
			this.m_lblRz.AutoSize = true;
			this.m_lblRz.Location = new System.Drawing.Point(10, 130);
			this.m_lblRz.Name = "m_lblRz";
			this.m_lblRz.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblRz.Size = new System.Drawing.Size(24, 16);
			this.m_lblRz.TabIndex = 6;
			this.m_lblRz.Text = "RZ";
			//
			// m_nudRz
			//
			this.m_nudRz.Location = new System.Drawing.Point(100, 126);
			this.m_nudRz.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
			this.m_nudRz.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
			this.m_nudRz.Name = "m_nudRz";
			this.m_nudRz.Size = new System.Drawing.Size(150, 23);
			this.m_nudRz.TabIndex = 7;
			this.m_nudRz.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_nudRz_MouseWheel);
			//
			// m_btnReset
			//
			this.m_btnReset.CommonText = CommonUI.Controls.CommonButtonText.Reset;
			this.m_btnReset.Location = new System.Drawing.Point(12, 354);
			this.m_btnReset.Name = "m_btnReset";
			this.m_btnReset.Size = new System.Drawing.Size(80, 28);
			this.m_btnReset.TabIndex = 2;
			this.m_btnReset.Text = "重設";
			this.m_btnReset.Click += new System.EventHandler(this.m_btnReset_Click);
			//
			// m_btnCancel
			//
			this.m_btnCancel.CommonText = CommonUI.Controls.CommonButtonText.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(140, 354);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(80, 28);
			this.m_btnCancel.TabIndex = 3;
			this.m_btnCancel.Text = "取消";
			this.m_btnCancel.Click += new System.EventHandler(this.m_btnCancel_Click);
			//
			// m_btnOK
			//
			this.m_btnOK.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnOK.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnOK.Location = new System.Drawing.Point(228, 354);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(80, 28);
			this.m_btnOK.TabIndex = 4;
			this.m_btnOK.Text = "確定";
			this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
			//
			// AxisTransformDlg
			//
			this.ClientSize = new System.Drawing.Size(320, 394);
			this.Controls.Add(this.m_grpDisplacement);
			this.Controls.Add(this.m_grpRotation);
			this.Controls.Add(this.m_btnReset);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.Name = "AxisTransformDlg";
			this.Text = "軸向轉換";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AxisTransformDlg_FormClosing);
			this.m_grpDisplacement.ResumeLayout(false);
			this.m_grpDisplacement.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbStepDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudZ)).EndInit();
			this.m_grpRotation.ResumeLayout(false);
			this.m_grpRotation.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbStepAngle)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudRx)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudRy)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudRz)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_grpDisplacement;
		private System.Windows.Forms.Label m_lblStepDis;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbStepDistance;
		private CommonUI.Controls.ThemedLabel m_lblX;
		private CommonUI.Controls.DebouncedNumericUpDown m_nudX;
		private CommonUI.Controls.ThemedLabel m_lblY;
		private CommonUI.Controls.DebouncedNumericUpDown m_nudY;
		private CommonUI.Controls.ThemedLabel m_lblZ;
		private CommonUI.Controls.DebouncedNumericUpDown m_nudZ;
		private CommonUI.Controls.SectionPanel m_grpRotation;
		private System.Windows.Forms.Label m_lblStapAngle;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbStepAngle;
		private CommonUI.Controls.ThemedLabel m_lblRx;
		private CommonUI.Controls.DebouncedNumericUpDown m_nudRx;
		private CommonUI.Controls.ThemedLabel m_lblRy;
		private CommonUI.Controls.DebouncedNumericUpDown m_nudRy;
		private CommonUI.Controls.ThemedLabel m_lblRz;
		private CommonUI.Controls.DebouncedNumericUpDown m_nudRz;
		private CommonUI.Controls.ThemedButton m_btnReset;
		private CommonUI.Controls.ThemedButton m_btnCancel;
		private CommonUI.Controls.ThemedButton m_btnOK;
	}
}
