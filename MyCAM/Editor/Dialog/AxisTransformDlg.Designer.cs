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
			this.m_pnlDisplacement = new CommonUI.Controls.SectionPanel();
			this.m_lblStepDis = new CommonUI.Controls.ThemedLabel();
			this.m_numStepDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblX = new CommonUI.Controls.ThemedLabel();
			this.m_numX = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblY = new CommonUI.Controls.ThemedLabel();
			this.m_numY = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblZ = new CommonUI.Controls.ThemedLabel();
			this.m_numZ = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_pnlRotation = new CommonUI.Controls.SectionPanel();
			this.m_lblStapAngle = new CommonUI.Controls.ThemedLabel();
			this.m_numStepAngle = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblRx = new CommonUI.Controls.ThemedLabel();
			this.m_numRx = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblRy = new CommonUI.Controls.ThemedLabel();
			this.m_numRy = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblRz = new CommonUI.Controls.ThemedLabel();
			this.m_numRz = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_btnReset = new CommonUI.Controls.ThemedButton();
			this.m_btnCancel = new CommonUI.Controls.ThemedButton();
			this.m_btnOK = new CommonUI.Controls.ThemedButton();
			this.m_pnlDisplacement.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numStepDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numZ)).BeginInit();
			this.m_pnlRotation.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numStepAngle)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRx)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRy)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRz)).BeginInit();
			this.SuspendLayout();
			//
			// m_pnlDisplacement
			//
			this.m_pnlDisplacement.Controls.Add(this.m_lblStepDis);
			this.m_pnlDisplacement.Controls.Add(this.m_numStepDistance);
			this.m_pnlDisplacement.Controls.Add(this.m_lblX);
			this.m_pnlDisplacement.Controls.Add(this.m_numX);
			this.m_pnlDisplacement.Controls.Add(this.m_lblY);
			this.m_pnlDisplacement.Controls.Add(this.m_numY);
			this.m_pnlDisplacement.Controls.Add(this.m_lblZ);
			this.m_pnlDisplacement.Controls.Add(this.m_numZ);
			this.m_pnlDisplacement.Location = new System.Drawing.Point(12, 12);
			this.m_pnlDisplacement.Name = "m_pnlDisplacement";
			this.m_pnlDisplacement.Size = new System.Drawing.Size(296, 159);
			this.m_pnlDisplacement.TabIndex = 0;
			this.m_pnlDisplacement.Text = "平移";
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
			// m_numStepDistance
			//
			this.m_numStepDistance.DecimalPlaces = 3;
			this.m_numStepDistance.Location = new System.Drawing.Point(100, 36);
			this.m_numStepDistance.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_numStepDistance.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.m_numStepDistance.Name = "m_numStepDistance";
			this.m_numStepDistance.Size = new System.Drawing.Size(150, 23);
			this.m_numStepDistance.TabIndex = 1;
			this.m_numStepDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_numStepDistance_KeyDown);
			this.m_numStepDistance.Leave += new System.EventHandler(this.m_numStepDistance_Leave);
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
			// m_numX
			//
			this.m_numX.Location = new System.Drawing.Point(100, 66);
			this.m_numX.Name = "m_numX";
			this.m_numX.Size = new System.Drawing.Size(150, 23);
			this.m_numX.TabIndex = 3;
			this.m_numX.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_numX_MouseWheel);
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
			// m_numY
			//
			this.m_numY.Location = new System.Drawing.Point(100, 96);
			this.m_numY.Name = "m_numY";
			this.m_numY.Size = new System.Drawing.Size(150, 23);
			this.m_numY.TabIndex = 5;
			this.m_numY.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_numY_MouseWheel);
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
			// m_numZ
			//
			this.m_numZ.Location = new System.Drawing.Point(100, 126);
			this.m_numZ.Name = "m_numZ";
			this.m_numZ.Size = new System.Drawing.Size(150, 23);
			this.m_numZ.TabIndex = 7;
			this.m_numZ.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_numZ_MouseWheel);
			//
			// m_pnlRotation
			//
			this.m_pnlRotation.Controls.Add(this.m_lblStapAngle);
			this.m_pnlRotation.Controls.Add(this.m_numStepAngle);
			this.m_pnlRotation.Controls.Add(this.m_lblRx);
			this.m_pnlRotation.Controls.Add(this.m_numRx);
			this.m_pnlRotation.Controls.Add(this.m_lblRy);
			this.m_pnlRotation.Controls.Add(this.m_numRy);
			this.m_pnlRotation.Controls.Add(this.m_lblRz);
			this.m_pnlRotation.Controls.Add(this.m_numRz);
			this.m_pnlRotation.Location = new System.Drawing.Point(12, 179);
			this.m_pnlRotation.Name = "m_pnlRotation";
			this.m_pnlRotation.Size = new System.Drawing.Size(296, 159);
			this.m_pnlRotation.TabIndex = 1;
			this.m_pnlRotation.Text = "旋轉";
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
			// m_numStepAngle
			//
			this.m_numStepAngle.DecimalPlaces = 3;
			this.m_numStepAngle.Location = new System.Drawing.Point(100, 36);
			this.m_numStepAngle.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_numStepAngle.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.m_numStepAngle.Name = "m_numStepAngle";
			this.m_numStepAngle.Size = new System.Drawing.Size(150, 23);
			this.m_numStepAngle.TabIndex = 1;
			this.m_numStepAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_numStepAngle_KeyDown);
			this.m_numStepAngle.Leave += new System.EventHandler(this.m_numStepAngle_Leave);
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
			// m_numRx
			//
			this.m_numRx.Location = new System.Drawing.Point(100, 66);
			this.m_numRx.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
			this.m_numRx.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
			this.m_numRx.Name = "m_numRx";
			this.m_numRx.Size = new System.Drawing.Size(150, 23);
			this.m_numRx.TabIndex = 3;
			this.m_numRx.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_numRx_MouseWheel);
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
			// m_numRy
			//
			this.m_numRy.Location = new System.Drawing.Point(100, 96);
			this.m_numRy.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
			this.m_numRy.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
			this.m_numRy.Name = "m_numRy";
			this.m_numRy.Size = new System.Drawing.Size(150, 23);
			this.m_numRy.TabIndex = 5;
			this.m_numRy.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_numRy_MouseWheel);
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
			// m_numRz
			//
			this.m_numRz.Location = new System.Drawing.Point(100, 126);
			this.m_numRz.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
			this.m_numRz.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
			this.m_numRz.Name = "m_numRz";
			this.m_numRz.Size = new System.Drawing.Size(150, 23);
			this.m_numRz.TabIndex = 7;
			this.m_numRz.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_numRz_MouseWheel);
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
			this.Controls.Add(this.m_pnlDisplacement);
			this.Controls.Add(this.m_pnlRotation);
			this.Controls.Add(this.m_btnReset);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.Name = "AxisTransformDlg";
			this.Text = "軸向轉換";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AxisTransformDlg_FormClosing);
			this.m_pnlDisplacement.ResumeLayout(false);
			this.m_pnlDisplacement.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numStepDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numZ)).EndInit();
			this.m_pnlRotation.ResumeLayout(false);
			this.m_pnlRotation.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numStepAngle)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRx)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRy)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRz)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlDisplacement;
		private CommonUI.Controls.ThemedLabel m_lblStepDis;
		private CommonUI.Controls.DebouncedNumericUpDown m_numStepDistance;
		private CommonUI.Controls.ThemedLabel m_lblX;
		private CommonUI.Controls.DebouncedNumericUpDown m_numX;
		private CommonUI.Controls.ThemedLabel m_lblY;
		private CommonUI.Controls.DebouncedNumericUpDown m_numY;
		private CommonUI.Controls.ThemedLabel m_lblZ;
		private CommonUI.Controls.DebouncedNumericUpDown m_numZ;
		private CommonUI.Controls.SectionPanel m_pnlRotation;
		private CommonUI.Controls.ThemedLabel m_lblStapAngle;
		private CommonUI.Controls.DebouncedNumericUpDown m_numStepAngle;
		private CommonUI.Controls.ThemedLabel m_lblRx;
		private CommonUI.Controls.DebouncedNumericUpDown m_numRx;
		private CommonUI.Controls.ThemedLabel m_lblRy;
		private CommonUI.Controls.DebouncedNumericUpDown m_numRy;
		private CommonUI.Controls.ThemedLabel m_lblRz;
		private CommonUI.Controls.DebouncedNumericUpDown m_numRz;
		private CommonUI.Controls.ThemedButton m_btnReset;
		private CommonUI.Controls.ThemedButton m_btnCancel;
		private CommonUI.Controls.ThemedButton m_btnOK;
	}
}
