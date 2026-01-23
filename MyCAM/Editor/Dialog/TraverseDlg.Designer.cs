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
			this.m_lblLiftUpDistance = new System.Windows.Forms.Label();
			this.m_lblCutDownDistance = new System.Windows.Forms.Label();
			this.m_lblFollowSaveDistance = new System.Windows.Forms.Label();
			this.m_chkSafePlane = new System.Windows.Forms.CheckBox();
			this.m_lblFrogLeapDistance = new System.Windows.Forms.Label();
			this.m_lblFollowSaveDistanceUnit = new System.Windows.Forms.Label();
			this.m_lblCutDownDistanceUnit = new System.Windows.Forms.Label();
			this.m_lblLiftUpDistanceUnit = new System.Windows.Forms.Label();
			this.m_lblFrogLeapDistanceUnit = new System.Windows.Forms.Label();
			this.m_lblSafePlaneDistance = new System.Windows.Forms.Label();
			this.m_lblSafePlaneDistanceUnit = new System.Windows.Forms.Label();
			this.m_btnConfirm = new System.Windows.Forms.Button();
			this.m_NumericUpDownSafePlaneDistance = new System.Windows.Forms.NumericUpDown();
			this.m_NumericUpDownLiftUpDistance = new System.Windows.Forms.NumericUpDown();
			this.m_NumericUpDownCutDownDistance = new System.Windows.Forms.NumericUpDown();
			this.m_NumericUpDownFollowSafeDistance = new System.Windows.Forms.NumericUpDown();
			this.m_NumericUpDownFrogLeapDistance = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownSafePlaneDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownLiftUpDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownCutDownDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownFollowSafeDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownFrogLeapDistance)).BeginInit();
			this.SuspendLayout();
			// 
			// m_lblLiftUpDistance
			// 
			this.m_lblLiftUpDistance.AutoSize = true;
			this.m_lblLiftUpDistance.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblLiftUpDistance.Location = new System.Drawing.Point(28, 25);
			this.m_lblLiftUpDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblLiftUpDistance.Name = "m_lblLiftUpDistance";
			this.m_lblLiftUpDistance.Size = new System.Drawing.Size(55, 15);
			this.m_lblLiftUpDistance.TabIndex = 26;
			this.m_lblLiftUpDistance.Text = "抬刀距離";
			// 
			// m_lblCutDownDistance
			// 
			this.m_lblCutDownDistance.AutoSize = true;
			this.m_lblCutDownDistance.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblCutDownDistance.Location = new System.Drawing.Point(28, 60);
			this.m_lblCutDownDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblCutDownDistance.Name = "m_lblCutDownDistance";
			this.m_lblCutDownDistance.Size = new System.Drawing.Size(55, 15);
			this.m_lblCutDownDistance.TabIndex = 21;
			this.m_lblCutDownDistance.Text = "下刀距離";
			// 
			// m_lblFollowSaveDistance
			// 
			this.m_lblFollowSaveDistance.AutoSize = true;
			this.m_lblFollowSaveDistance.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblFollowSaveDistance.Location = new System.Drawing.Point(28, 95);
			this.m_lblFollowSaveDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblFollowSaveDistance.Name = "m_lblFollowSaveDistance";
			this.m_lblFollowSaveDistance.Size = new System.Drawing.Size(79, 15);
			this.m_lblFollowSaveDistance.TabIndex = 20;
			this.m_lblFollowSaveDistance.Text = "隨動安全距離";
			// 
			// m_chkSafePlane
			// 
			this.m_chkSafePlane.AutoSize = true;
			this.m_chkSafePlane.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_chkSafePlane.Location = new System.Drawing.Point(28, 165);
			this.m_chkSafePlane.Name = "m_chkSafePlane";
			this.m_chkSafePlane.Size = new System.Drawing.Size(98, 19);
			this.m_chkSafePlane.TabIndex = 28;
			this.m_chkSafePlane.Text = "啟用安全平面";
			this.m_chkSafePlane.UseVisualStyleBackColor = true;
			this.m_chkSafePlane.CheckedChanged += new System.EventHandler(this.m_chkSafePlane_CheckedChanged);
			// 
			// m_lblFrogLeapDistance
			// 
			this.m_lblFrogLeapDistance.AutoSize = true;
			this.m_lblFrogLeapDistance.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblFrogLeapDistance.Location = new System.Drawing.Point(28, 130);
			this.m_lblFrogLeapDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblFrogLeapDistance.Name = "m_lblFrogLeapDistance";
			this.m_lblFrogLeapDistance.Size = new System.Drawing.Size(55, 15);
			this.m_lblFrogLeapDistance.TabIndex = 19;
			this.m_lblFrogLeapDistance.Text = "蛙跳距離";
			// 
			// m_lblFollowSaveDistanceUnit
			// 
			this.m_lblFollowSaveDistanceUnit.AutoSize = true;
			this.m_lblFollowSaveDistanceUnit.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblFollowSaveDistanceUnit.Location = new System.Drawing.Point(254, 95);
			this.m_lblFollowSaveDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblFollowSaveDistanceUnit.Name = "m_lblFollowSaveDistanceUnit";
			this.m_lblFollowSaveDistanceUnit.Size = new System.Drawing.Size(29, 15);
			this.m_lblFollowSaveDistanceUnit.TabIndex = 22;
			this.m_lblFollowSaveDistanceUnit.Text = "mm";
			// 
			// m_lblCutDownDistanceUnit
			// 
			this.m_lblCutDownDistanceUnit.AutoSize = true;
			this.m_lblCutDownDistanceUnit.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblCutDownDistanceUnit.Location = new System.Drawing.Point(254, 60);
			this.m_lblCutDownDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblCutDownDistanceUnit.Name = "m_lblCutDownDistanceUnit";
			this.m_lblCutDownDistanceUnit.Size = new System.Drawing.Size(29, 15);
			this.m_lblCutDownDistanceUnit.TabIndex = 23;
			this.m_lblCutDownDistanceUnit.Text = "mm";
			// 
			// m_lblLiftUpDistanceUnit
			// 
			this.m_lblLiftUpDistanceUnit.AutoSize = true;
			this.m_lblLiftUpDistanceUnit.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblLiftUpDistanceUnit.Location = new System.Drawing.Point(254, 25);
			this.m_lblLiftUpDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblLiftUpDistanceUnit.Name = "m_lblLiftUpDistanceUnit";
			this.m_lblLiftUpDistanceUnit.Size = new System.Drawing.Size(29, 15);
			this.m_lblLiftUpDistanceUnit.TabIndex = 24;
			this.m_lblLiftUpDistanceUnit.Text = "mm";
			// 
			// m_lblFrogLeapDistanceUnit
			// 
			this.m_lblFrogLeapDistanceUnit.AutoSize = true;
			this.m_lblFrogLeapDistanceUnit.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblFrogLeapDistanceUnit.Location = new System.Drawing.Point(254, 130);
			this.m_lblFrogLeapDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblFrogLeapDistanceUnit.Name = "m_lblFrogLeapDistanceUnit";
			this.m_lblFrogLeapDistanceUnit.Size = new System.Drawing.Size(29, 15);
			this.m_lblFrogLeapDistanceUnit.TabIndex = 25;
			this.m_lblFrogLeapDistanceUnit.Text = "mm";
			// 
			// m_lblSafePlaneDistance
			// 
			this.m_lblSafePlaneDistance.AutoSize = true;
			this.m_lblSafePlaneDistance.Font = new System.Drawing.Font("Arial", 9F);
			this.m_lblSafePlaneDistance.Location = new System.Drawing.Point(50, 200);
			this.m_lblSafePlaneDistance.Name = "m_lblSafePlaneDistance";
			this.m_lblSafePlaneDistance.Size = new System.Drawing.Size(31, 15);
			this.m_lblSafePlaneDistance.TabIndex = 31;
			this.m_lblSafePlaneDistance.Text = "距離";
			// 
			// m_lblSafePlaneDistanceUnit
			// 
			this.m_lblSafePlaneDistanceUnit.AutoSize = true;
			this.m_lblSafePlaneDistanceUnit.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblSafePlaneDistanceUnit.Location = new System.Drawing.Point(254, 200);
			this.m_lblSafePlaneDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblSafePlaneDistanceUnit.Name = "m_lblSafePlaneDistanceUnit";
			this.m_lblSafePlaneDistanceUnit.Size = new System.Drawing.Size(29, 15);
			this.m_lblSafePlaneDistanceUnit.TabIndex = 33;
			this.m_lblSafePlaneDistanceUnit.Text = "mm";
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_btnConfirm.Location = new System.Drawing.Point(212, 235);
			this.m_btnConfirm.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(71, 29);
			this.m_btnConfirm.TabIndex = 18;
			this.m_btnConfirm.Text = "確認";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// m_NumericUpDownSafePlaneDistance
			// 
			this.m_NumericUpDownSafePlaneDistance.DecimalPlaces = 3;
			this.m_NumericUpDownSafePlaneDistance.Enabled = false;
			this.m_NumericUpDownSafePlaneDistance.Font = new System.Drawing.Font("Arial", 9F);
			this.m_NumericUpDownSafePlaneDistance.Location = new System.Drawing.Point(154, 200);
			this.m_NumericUpDownSafePlaneDistance.Maximum = new decimal(new int[] {
            1410065407,
            2,
            0,
            0});
			this.m_NumericUpDownSafePlaneDistance.Name = "m_NumericUpDownSafePlaneDistance";
			this.m_NumericUpDownSafePlaneDistance.Size = new System.Drawing.Size(92, 21);
			this.m_NumericUpDownSafePlaneDistance.TabIndex = 34;
			this.m_NumericUpDownSafePlaneDistance.Click += new System.EventHandler(this.m_NumericUpDownSafePlaneDistance_Click);
			this.m_NumericUpDownSafePlaneDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_NumericUpDownSafePlaneDistance_KeyDown);
			this.m_NumericUpDownSafePlaneDistance.Leave += new System.EventHandler(this.m_NumericUpDownSafePlaneDistance_Leave);
			// 
			// m_NumericUpDownLiftUpDistance
			// 
			this.m_NumericUpDownLiftUpDistance.DecimalPlaces = 3;
			this.m_NumericUpDownLiftUpDistance.Font = new System.Drawing.Font("Arial", 9F);
			this.m_NumericUpDownLiftUpDistance.Location = new System.Drawing.Point(155, 25);
			this.m_NumericUpDownLiftUpDistance.Name = "m_NumericUpDownLiftUpDistance";
			this.m_NumericUpDownLiftUpDistance.Size = new System.Drawing.Size(92, 21);
			this.m_NumericUpDownLiftUpDistance.TabIndex = 35;
			this.m_NumericUpDownLiftUpDistance.Click += new System.EventHandler(this.m_NumericUpDownLiftUpDistance_Click);
			this.m_NumericUpDownLiftUpDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_NumericUpDownLiftUpDistance_KeyDown);
			this.m_NumericUpDownLiftUpDistance.Leave += new System.EventHandler(this.m_NumericUpDownLiftUpDistance_Leave);
			// 
			// m_NumericUpDownCutDownDistance
			// 
			this.m_NumericUpDownCutDownDistance.DecimalPlaces = 3;
			this.m_NumericUpDownCutDownDistance.Font = new System.Drawing.Font("Arial", 9F);
			this.m_NumericUpDownCutDownDistance.Location = new System.Drawing.Point(155, 60);
			this.m_NumericUpDownCutDownDistance.Name = "m_NumericUpDownCutDownDistance";
			this.m_NumericUpDownCutDownDistance.Size = new System.Drawing.Size(92, 21);
			this.m_NumericUpDownCutDownDistance.TabIndex = 36;
			this.m_NumericUpDownCutDownDistance.Click += new System.EventHandler(this.m_NumericUpDownCutDownDistance_Click);
			this.m_NumericUpDownCutDownDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_NumericUpDownCutDownDistance_KeyDown);
			this.m_NumericUpDownCutDownDistance.Leave += new System.EventHandler(this.m_NumericUpDownCutDownDistance_Leave);
			// 
			// m_NumericUpDownFollowSafeDistance
			// 
			this.m_NumericUpDownFollowSafeDistance.DecimalPlaces = 3;
			this.m_NumericUpDownFollowSafeDistance.Font = new System.Drawing.Font("Arial", 9F);
			this.m_NumericUpDownFollowSafeDistance.Location = new System.Drawing.Point(155, 95);
			this.m_NumericUpDownFollowSafeDistance.Name = "m_NumericUpDownFollowSafeDistance";
			this.m_NumericUpDownFollowSafeDistance.Size = new System.Drawing.Size(92, 21);
			this.m_NumericUpDownFollowSafeDistance.TabIndex = 37;
			this.m_NumericUpDownFollowSafeDistance.Click += new System.EventHandler(this.m_NumericUpDownFollowSafeDistance_Click);
			this.m_NumericUpDownFollowSafeDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_NumericUpDownFollowSafeDistance_KeyDown);
			this.m_NumericUpDownFollowSafeDistance.Leave += new System.EventHandler(this.m_NumericUpDownFollowSafeDistance_Leave);
			// 
			// m_NumericUpDownFrogLeapDistance
			// 
			this.m_NumericUpDownFrogLeapDistance.DecimalPlaces = 3;
			this.m_NumericUpDownFrogLeapDistance.Font = new System.Drawing.Font("Arial", 9F);
			this.m_NumericUpDownFrogLeapDistance.Location = new System.Drawing.Point(155, 130);
			this.m_NumericUpDownFrogLeapDistance.Name = "m_NumericUpDownFrogLeapDistance";
			this.m_NumericUpDownFrogLeapDistance.Size = new System.Drawing.Size(92, 21);
			this.m_NumericUpDownFrogLeapDistance.TabIndex = 38;
			this.m_NumericUpDownFrogLeapDistance.Click += new System.EventHandler(this.m_NumericUpDownFrogLeapDistance_Click);
			this.m_NumericUpDownFrogLeapDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_NumericUpDownFrogLeapDistance_KeyDown);
			this.m_NumericUpDownFrogLeapDistance.Leave += new System.EventHandler(this.m_NumericUpDownFrogLeapDistance_Leave);
			// 
			// TraverseDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(294, 276);
			this.Controls.Add(this.m_NumericUpDownFrogLeapDistance);
			this.Controls.Add(this.m_NumericUpDownFollowSafeDistance);
			this.Controls.Add(this.m_NumericUpDownCutDownDistance);
			this.Controls.Add(this.m_NumericUpDownLiftUpDistance);
			this.Controls.Add(this.m_NumericUpDownSafePlaneDistance);
			this.Controls.Add(this.m_lblLiftUpDistance);
			this.Controls.Add(this.m_lblCutDownDistance);
			this.Controls.Add(this.m_lblFollowSaveDistance);
			this.Controls.Add(this.m_chkSafePlane);
			this.Controls.Add(this.m_lblFrogLeapDistance);
			this.Controls.Add(this.m_lblFollowSaveDistanceUnit);
			this.Controls.Add(this.m_lblCutDownDistanceUnit);
			this.Controls.Add(this.m_lblLiftUpDistanceUnit);
			this.Controls.Add(this.m_lblFrogLeapDistanceUnit);
			this.Controls.Add(this.m_lblSafePlaneDistance);
			this.Controls.Add(this.m_lblSafePlaneDistanceUnit);
			this.Controls.Add(this.m_btnConfirm);
			this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "TraverseDlg";
			this.Text = "空移參數設定";
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownSafePlaneDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownLiftUpDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownCutDownDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownFollowSafeDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumericUpDownFrogLeapDistance)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label m_lblLiftUpDistance;
		private System.Windows.Forms.Label m_lblCutDownDistance;
		private System.Windows.Forms.Label m_lblFollowSaveDistance;
		private System.Windows.Forms.CheckBox m_chkSafePlane;
		private System.Windows.Forms.Label m_lblFrogLeapDistance;
		private System.Windows.Forms.Label m_lblFollowSaveDistanceUnit;
		private System.Windows.Forms.Label m_lblCutDownDistanceUnit;
		private System.Windows.Forms.Label m_lblLiftUpDistanceUnit;
		private System.Windows.Forms.Label m_lblFrogLeapDistanceUnit;
		private System.Windows.Forms.Label m_lblSafePlaneDistance;
		private System.Windows.Forms.Label m_lblSafePlaneDistanceUnit;
		private System.Windows.Forms.Button m_btnConfirm;
		private System.Windows.Forms.NumericUpDown m_NumericUpDownSafePlaneDistance;
		private System.Windows.Forms.NumericUpDown m_NumericUpDownLiftUpDistance;
		private System.Windows.Forms.NumericUpDown m_NumericUpDownCutDownDistance;
		private System.Windows.Forms.NumericUpDown m_NumericUpDownFollowSafeDistance;
		private System.Windows.Forms.NumericUpDown m_NumericUpDownFrogLeapDistance;
	}
}