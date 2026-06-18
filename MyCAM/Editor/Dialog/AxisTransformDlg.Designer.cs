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
			this.m_lblX = new System.Windows.Forms.Label();
			this.m_lblY = new System.Windows.Forms.Label();
			this.m_lblZ = new System.Windows.Forms.Label();
			this.m_lblRx = new System.Windows.Forms.Label();
			this.m_lblRy = new System.Windows.Forms.Label();
			this.m_lblRz = new System.Windows.Forms.Label();
			this.m_nudX = new System.Windows.Forms.NumericUpDown();
			this.m_nudY = new System.Windows.Forms.NumericUpDown();
			this.m_nudRx = new System.Windows.Forms.NumericUpDown();
			this.m_nudZ = new System.Windows.Forms.NumericUpDown();
			this.m_nudRy = new System.Windows.Forms.NumericUpDown();
			this.m_nudRz = new System.Windows.Forms.NumericUpDown();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnReset = new System.Windows.Forms.Button();
			this.m_lblStepDis = new System.Windows.Forms.Label();
			this.m_grpDisplacement = new System.Windows.Forms.GroupBox();
			this.m_txbStepDistance = new System.Windows.Forms.TextBox();
			this.m_grpRotation = new System.Windows.Forms.GroupBox();
			this.m_txbStepAngle = new System.Windows.Forms.TextBox();
			this.m_lblStapAngle = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_nudX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudRx)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudZ)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudRy)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudRz)).BeginInit();
			this.m_grpDisplacement.SuspendLayout();
			this.m_grpRotation.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_lblX
			// 
			this.m_lblX.AutoSize = true;
			this.m_lblX.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblX.Location = new System.Drawing.Point(15, 95);
			this.m_lblX.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblX.Name = "m_lblX";
			this.m_lblX.Size = new System.Drawing.Size(27, 27);
			this.m_lblX.TabIndex = 2;
			this.m_lblX.Text = "X";
			// 
			// m_lblY
			// 
			this.m_lblY.AutoSize = true;
			this.m_lblY.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblY.Location = new System.Drawing.Point(15, 154);
			this.m_lblY.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblY.Name = "m_lblY";
			this.m_lblY.Size = new System.Drawing.Size(28, 27);
			this.m_lblY.TabIndex = 3;
			this.m_lblY.Text = "Y";
			// 
			// m_lblZ
			// 
			this.m_lblZ.AutoSize = true;
			this.m_lblZ.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblZ.Location = new System.Drawing.Point(15, 215);
			this.m_lblZ.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblZ.Name = "m_lblZ";
			this.m_lblZ.Size = new System.Drawing.Size(27, 27);
			this.m_lblZ.TabIndex = 4;
			this.m_lblZ.Text = "Z";
			// 
			// m_lblRx
			// 
			this.m_lblRx.AutoSize = true;
			this.m_lblRx.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblRx.Location = new System.Drawing.Point(26, 95);
			this.m_lblRx.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblRx.Name = "m_lblRx";
			this.m_lblRx.Size = new System.Drawing.Size(44, 27);
			this.m_lblRx.TabIndex = 5;
			this.m_lblRx.Text = "RX";
			// 
			// m_lblRy
			// 
			this.m_lblRy.AutoSize = true;
			this.m_lblRy.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblRy.Location = new System.Drawing.Point(26, 154);
			this.m_lblRy.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblRy.Name = "m_lblRy";
			this.m_lblRy.Size = new System.Drawing.Size(45, 27);
			this.m_lblRy.TabIndex = 6;
			this.m_lblRy.Text = "RY";
			// 
			// m_lblRz
			// 
			this.m_lblRz.AutoSize = true;
			this.m_lblRz.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblRz.Location = new System.Drawing.Point(26, 215);
			this.m_lblRz.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblRz.Name = "m_lblRz";
			this.m_lblRz.Size = new System.Drawing.Size(44, 27);
			this.m_lblRz.TabIndex = 7;
			this.m_lblRz.Text = "RZ";
			// 
			// m_nudX
			// 
			this.m_nudX.Location = new System.Drawing.Point(59, 95);
			this.m_nudX.Margin = new System.Windows.Forms.Padding(4);
			this.m_nudX.Name = "m_nudX";
			this.m_nudX.Size = new System.Drawing.Size(110, 30);
			this.m_nudX.TabIndex = 8;
			this.m_nudX.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_nudX_MouseWheel);
			// 
			// m_nudY
			// 
			this.m_nudY.Location = new System.Drawing.Point(59, 154);
			this.m_nudY.Margin = new System.Windows.Forms.Padding(4);
			this.m_nudY.Name = "m_nudY";
			this.m_nudY.Size = new System.Drawing.Size(110, 30);
			this.m_nudY.TabIndex = 9;
			this.m_nudY.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_nudY_MouseWheel);
			// 
			// m_nudRx
			// 
			this.m_nudRx.Location = new System.Drawing.Point(76, 95);
			this.m_nudRx.Margin = new System.Windows.Forms.Padding(4);
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
			this.m_nudRx.Size = new System.Drawing.Size(110, 30);
			this.m_nudRx.TabIndex = 10;
			this.m_nudRx.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_nudRx_MouseWheel);
			// 
			// m_nudZ
			// 
			this.m_nudZ.Location = new System.Drawing.Point(59, 215);
			this.m_nudZ.Margin = new System.Windows.Forms.Padding(4);
			this.m_nudZ.Name = "m_nudZ";
			this.m_nudZ.Size = new System.Drawing.Size(110, 30);
			this.m_nudZ.TabIndex = 11;
			this.m_nudZ.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_nudZ_MouseWheel);
			// 
			// m_nudRy
			// 
			this.m_nudRy.Location = new System.Drawing.Point(76, 154);
			this.m_nudRy.Margin = new System.Windows.Forms.Padding(4);
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
			this.m_nudRy.Size = new System.Drawing.Size(110, 30);
			this.m_nudRy.TabIndex = 12;
			this.m_nudRy.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_nudRy_MouseWheel);
			// 
			// m_nudRz
			// 
			this.m_nudRz.Location = new System.Drawing.Point(76, 215);
			this.m_nudRz.Margin = new System.Windows.Forms.Padding(4);
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
			this.m_nudRz.Size = new System.Drawing.Size(110, 30);
			this.m_nudRz.TabIndex = 13;
			this.m_nudRz.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.m_nudRz_MouseWheel);
			// 
			// m_btnOK
			// 
			this.m_btnOK.Location = new System.Drawing.Point(387, 287);
			this.m_btnOK.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(78, 29);
			this.m_btnOK.TabIndex = 24;
			this.m_btnOK.Text = "確認";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Location = new System.Drawing.Point(473, 287);
			this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(78, 29);
			this.m_btnCancel.TabIndex = 25;
			this.m_btnCancel.Text = "取消";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.m_btnCancel_Click);
			// 
			// m_btnReset
			// 
			this.m_btnReset.Location = new System.Drawing.Point(15, 287);
			this.m_btnReset.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnReset.Name = "m_btnReset";
			this.m_btnReset.Size = new System.Drawing.Size(78, 29);
			this.m_btnReset.TabIndex = 26;
			this.m_btnReset.Text = "重設";
			this.m_btnReset.UseVisualStyleBackColor = true;
			this.m_btnReset.Click += new System.EventHandler(this.m_btnReset_Click);
			// 
			// m_lblStepDis
			// 
			this.m_lblStepDis.AutoSize = true;
			this.m_lblStepDis.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblStepDis.Location = new System.Drawing.Point(9, 42);
			this.m_lblStepDis.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblStepDis.Name = "m_lblStepDis";
			this.m_lblStepDis.Size = new System.Drawing.Size(81, 19);
			this.m_lblStepDis.TabIndex = 21;
			this.m_lblStepDis.Text = "步進距離";
			// 
			// m_grpDisplacement
			// 
			this.m_grpDisplacement.Controls.Add(this.m_txbStepDistance);
			this.m_grpDisplacement.Controls.Add(this.m_lblStepDis);
			this.m_grpDisplacement.Controls.Add(this.m_nudZ);
			this.m_grpDisplacement.Controls.Add(this.m_nudY);
			this.m_grpDisplacement.Controls.Add(this.m_lblX);
			this.m_grpDisplacement.Controls.Add(this.m_nudX);
			this.m_grpDisplacement.Controls.Add(this.m_lblZ);
			this.m_grpDisplacement.Controls.Add(this.m_lblY);
			this.m_grpDisplacement.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_grpDisplacement.Location = new System.Drawing.Point(12, 12);
			this.m_grpDisplacement.Name = "m_grpDisplacement";
			this.m_grpDisplacement.Size = new System.Drawing.Size(259, 268);
			this.m_grpDisplacement.TabIndex = 22;
			this.m_grpDisplacement.TabStop = false;
			this.m_grpDisplacement.Text = "平移";
			// 
			// m_txbStepDistance
			// 
			this.m_txbStepDistance.Location = new System.Drawing.Point(83, 37);
			this.m_txbStepDistance.Name = "m_txbStepDistance";
			this.m_txbStepDistance.Size = new System.Drawing.Size(100, 30);
			this.m_txbStepDistance.TabIndex = 22;
			this.m_txbStepDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbStepDistance_KeyDown);
			this.m_txbStepDistance.Leave += new System.EventHandler(this.m_txbStepDistance_Leave);
			// 
			// m_grpRotation
			// 
			this.m_grpRotation.Controls.Add(this.m_txbStepAngle);
			this.m_grpRotation.Controls.Add(this.m_lblStapAngle);
			this.m_grpRotation.Controls.Add(this.m_lblRx);
			this.m_grpRotation.Controls.Add(this.m_lblRy);
			this.m_grpRotation.Controls.Add(this.m_lblRz);
			this.m_grpRotation.Controls.Add(this.m_nudRx);
			this.m_grpRotation.Controls.Add(this.m_nudRy);
			this.m_grpRotation.Controls.Add(this.m_nudRz);
			this.m_grpRotation.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_grpRotation.Location = new System.Drawing.Point(288, 12);
			this.m_grpRotation.Name = "m_grpRotation";
			this.m_grpRotation.Size = new System.Drawing.Size(271, 268);
			this.m_grpRotation.TabIndex = 23;
			this.m_grpRotation.TabStop = false;
			this.m_grpRotation.Text = "旋轉";
			// 
			// m_txbStepAngle
			// 
			this.m_txbStepAngle.Location = new System.Drawing.Point(111, 40);
			this.m_txbStepAngle.Name = "m_txbStepAngle";
			this.m_txbStepAngle.Size = new System.Drawing.Size(100, 30);
			this.m_txbStepAngle.TabIndex = 24;
			this.m_txbStepAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbStepAngle_KeyDown);
			this.m_txbStepAngle.Leave += new System.EventHandler(this.m_txbStepAngle_Leave);
			// 
			// m_lblStapAngle
			// 
			this.m_lblStapAngle.AutoSize = true;
			this.m_lblStapAngle.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblStapAngle.Location = new System.Drawing.Point(26, 45);
			this.m_lblStapAngle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblStapAngle.Name = "m_lblStapAngle";
			this.m_lblStapAngle.Size = new System.Drawing.Size(81, 19);
			this.m_lblStapAngle.TabIndex = 23;
			this.m_lblStapAngle.Text = "步進角度";
			// 
			// AxisTransformDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(585, 328);
			this.Controls.Add(this.m_grpRotation);
			this.Controls.Add(this.m_grpDisplacement);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnReset);
			this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "AxisTransformDlg";
			this.Text = "軸向轉換";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AxisTransformDlg_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.m_nudX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudRx)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudZ)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudRy)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudRz)).EndInit();
			this.m_grpDisplacement.ResumeLayout(false);
			this.m_grpDisplacement.PerformLayout();
			this.m_grpRotation.ResumeLayout(false);
			this.m_grpRotation.PerformLayout();
			this.ResumeLayout(false);

		}

		private void M_nudRx_MouseWheel( object sender, System.Windows.Forms.MouseEventArgs e )
		{
			throw new System.NotImplementedException();
		}

		#endregion
		private System.Windows.Forms.Label m_lblX;
		private System.Windows.Forms.Label m_lblY;
		private System.Windows.Forms.Label m_lblZ;
		private System.Windows.Forms.Label m_lblRx;
		private System.Windows.Forms.Label m_lblRy;
		private System.Windows.Forms.Label m_lblRz;
		private System.Windows.Forms.NumericUpDown m_nudX;
		private System.Windows.Forms.NumericUpDown m_nudY;
		private System.Windows.Forms.NumericUpDown m_nudRx;
		private System.Windows.Forms.NumericUpDown m_nudZ;
		private System.Windows.Forms.NumericUpDown m_nudRy;
		private System.Windows.Forms.NumericUpDown m_nudRz;
		private System.Windows.Forms.Label m_lblStepDis;
		private System.Windows.Forms.GroupBox m_grpDisplacement;
		private System.Windows.Forms.GroupBox m_grpRotation;
		private System.Windows.Forms.TextBox m_txbStepDistance;
		private System.Windows.Forms.TextBox m_txbStepAngle;
		private System.Windows.Forms.Label m_lblStapAngle;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnReset;
	}
}
