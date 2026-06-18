namespace MyCAM.Editor.Dialog
{
	partial class AutoSortPathDlg
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

			// dispose preview images
			if( disposing ) {
				m_imgExtrusionFirst_FromTop_Counterclockwise?.Dispose();
				m_imgExtrusionFirst_FromBottom_Counterclockwise?.Dispose();
				m_imgRotationFirst_FromTop_Clockwise?.Dispose();
				m_imgRotationFirst_FromTop_Counterclockwise?.Dispose();
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
			this.m_lblSortMethod = new System.Windows.Forms.Label();
			this.m_cmbSortMethod = new System.Windows.Forms.ComboBox();
			this.m_lblPathHint = new System.Windows.Forms.Label();
			this.m_lblSelectedPath = new System.Windows.Forms.Label();
			this.m_lblCylinderAxis = new System.Windows.Forms.Label();
			this.m_cmbCylinderAxis = new System.Windows.Forms.ComboBox();
			this.m_lblExtrusionDir = new System.Windows.Forms.Label();
			this.m_cmbExtrusionDir = new System.Windows.Forms.ComboBox();
			this.m_lblRotationDir = new System.Windows.Forms.Label();
			this.m_cmbRotationDir = new System.Windows.Forms.ComboBox();
			this.m_lblExtrusionBandwidth = new System.Windows.Forms.Label();
			this.m_nudExtrusionBandwidth = new System.Windows.Forms.NumericUpDown();
			this.m_lblExtrusionUnit = new System.Windows.Forms.Label();
			this.m_lblRotationBandwidth = new System.Windows.Forms.Label();
			this.m_nudRotationBandwidth = new System.Windows.Forms.NumericUpDown();
			this.m_lblRotationUnit = new System.Windows.Forms.Label();
			this.m_picPreview = new System.Windows.Forms.PictureBox();
			this.m_btnConfirm = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.m_nudExtrusionBandwidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudRotationBandwidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picPreview)).BeginInit();
			this.SuspendLayout();
			// 
			// m_lblSortMethod
			// 
			this.m_lblSortMethod.AutoSize = true;
			this.m_lblSortMethod.Location = new System.Drawing.Point( 19, 21 );
			this.m_lblSortMethod.Name = "m_lblSortMethod";
			this.m_lblSortMethod.Size = new System.Drawing.Size( 56, 12 );
			this.m_lblSortMethod.TabIndex = 0;
			this.m_lblSortMethod.Text = "排序方式:";
			// 
			// m_cmbSortMethod
			// 
			this.m_cmbSortMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbSortMethod.FormattingEnabled = true;
			this.m_cmbSortMethod.Location = new System.Drawing.Point( 88, 18 );
			this.m_cmbSortMethod.Name = "m_cmbSortMethod";
			this.m_cmbSortMethod.Size = new System.Drawing.Size( 160, 20 );
			this.m_cmbSortMethod.TabIndex = 1;
			this.m_cmbSortMethod.SelectedIndexChanged += new System.EventHandler( this.m_cmbSortMethod_SelectedIndexChanged );
			// 
			// m_lblPathHint
			// 
			this.m_lblPathHint.AutoSize = true;
			this.m_lblPathHint.Location = new System.Drawing.Point( 19, 52 );
			this.m_lblPathHint.Name = "m_lblPathHint";
			this.m_lblPathHint.Size = new System.Drawing.Size( 56, 12 );
			this.m_lblPathHint.TabIndex = 2;
			this.m_lblPathHint.Text = "起始路徑:";
			// 
			// m_lblSelectedPath
			// 
			this.m_lblSelectedPath.AutoSize = true;
			this.m_lblSelectedPath.ForeColor = System.Drawing.Color.Red;
			this.m_lblSelectedPath.Location = new System.Drawing.Point( 88, 52 );
			this.m_lblSelectedPath.Name = "m_lblSelectedPath";
			this.m_lblSelectedPath.Size = new System.Drawing.Size( 35, 12 );
			this.m_lblSelectedPath.TabIndex = 3;
			this.m_lblSelectedPath.Text = "未選取";
			// 
			// m_lblCylinderAxis
			// 
			this.m_lblCylinderAxis.AutoSize = true;
			this.m_lblCylinderAxis.Location = new System.Drawing.Point( 19, 52 );
			this.m_lblCylinderAxis.Name = "m_lblCylinderAxis";
			this.m_lblCylinderAxis.Size = new System.Drawing.Size( 56, 12 );
			this.m_lblCylinderAxis.TabIndex = 16;
			this.m_lblCylinderAxis.Text = "圓管軸向:";
			this.m_lblCylinderAxis.Visible = false;
			// 
			// m_cmbCylinderAxis
			// 
			this.m_cmbCylinderAxis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbCylinderAxis.FormattingEnabled = true;
			this.m_cmbCylinderAxis.Location = new System.Drawing.Point( 100, 49 );
			this.m_cmbCylinderAxis.Name = "m_cmbCylinderAxis";
			this.m_cmbCylinderAxis.Size = new System.Drawing.Size( 148, 20 );
			this.m_cmbCylinderAxis.TabIndex = 17;
			this.m_cmbCylinderAxis.Visible = false;
			// 
			// m_lblExtrusionDir
			// 
			this.m_lblExtrusionDir.AutoSize = true;
			this.m_lblExtrusionDir.Location = new System.Drawing.Point( 19, 80 );
			this.m_lblExtrusionDir.Name = "m_lblExtrusionDir";
			this.m_lblExtrusionDir.Size = new System.Drawing.Size( 68, 12 );
			this.m_lblExtrusionDir.TabIndex = 4;
			this.m_lblExtrusionDir.Text = "拉伸軸方向:";
			this.m_lblExtrusionDir.Visible = false;
			// 
			// m_cmbExtrusionDir
			// 
			this.m_cmbExtrusionDir.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbExtrusionDir.FormattingEnabled = true;
			this.m_cmbExtrusionDir.Location = new System.Drawing.Point( 100, 77 );
			this.m_cmbExtrusionDir.Name = "m_cmbExtrusionDir";
			this.m_cmbExtrusionDir.Size = new System.Drawing.Size( 148, 20 );
			this.m_cmbExtrusionDir.TabIndex = 5;
			this.m_cmbExtrusionDir.Visible = false;
			this.m_cmbExtrusionDir.SelectedIndexChanged += new System.EventHandler( this.m_cmbExtrusionDir_SelectedIndexChanged );
			// 
			// m_lblRotationDir
			// 
			this.m_lblRotationDir.AutoSize = true;
			this.m_lblRotationDir.Location = new System.Drawing.Point( 19, 80 );
			this.m_lblRotationDir.Name = "m_lblRotationDir";
			this.m_lblRotationDir.Size = new System.Drawing.Size( 68, 12 );
			this.m_lblRotationDir.TabIndex = 6;
			this.m_lblRotationDir.Text = "旋轉軸方向:";
			this.m_lblRotationDir.Visible = false;
			// 
			// m_cmbRotationDir
			// 
			this.m_cmbRotationDir.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbRotationDir.FormattingEnabled = true;
			this.m_cmbRotationDir.Location = new System.Drawing.Point( 100, 77 );
			this.m_cmbRotationDir.Name = "m_cmbRotationDir";
			this.m_cmbRotationDir.Size = new System.Drawing.Size( 148, 20 );
			this.m_cmbRotationDir.TabIndex = 7;
			this.m_cmbRotationDir.Visible = false;
			this.m_cmbRotationDir.SelectedIndexChanged += new System.EventHandler( this.m_cmbRotationDir_SelectedIndexChanged );
			// 
			// m_lblExtrusionBandwidth
			// 
			this.m_lblExtrusionBandwidth.AutoSize = true;
			this.m_lblExtrusionBandwidth.Location = new System.Drawing.Point( 19, 108 );
			this.m_lblExtrusionBandwidth.Name = "m_lblExtrusionBandwidth";
			this.m_lblExtrusionBandwidth.Size = new System.Drawing.Size( 56, 12 );
			this.m_lblExtrusionBandwidth.TabIndex = 10;
			this.m_lblExtrusionBandwidth.Text = "分組帶寬:";
			this.m_lblExtrusionBandwidth.Visible = false;
			// 
			// m_nudExtrusionBandwidth
			// 
			this.m_nudExtrusionBandwidth.DecimalPlaces = 1;
			this.m_nudExtrusionBandwidth.Increment = new decimal( new int[] { 5, 0, 0, 65536 } );
			this.m_nudExtrusionBandwidth.Location = new System.Drawing.Point( 100, 105 );
			this.m_nudExtrusionBandwidth.Maximum = new decimal( new int[] { 1000, 0, 0, 0 } );
			this.m_nudExtrusionBandwidth.Minimum = new decimal( new int[] { 1, 0, 0, 65536 } );
			this.m_nudExtrusionBandwidth.Name = "m_nudExtrusionBandwidth";
			this.m_nudExtrusionBandwidth.Size = new System.Drawing.Size( 100, 22 );
			this.m_nudExtrusionBandwidth.TabIndex = 11;
			this.m_nudExtrusionBandwidth.Value = new decimal( new int[] { 100, 0, 0, 65536 } );
			this.m_nudExtrusionBandwidth.Visible = false;
			// 
			// m_lblExtrusionUnit
			// 
			this.m_lblExtrusionUnit.AutoSize = true;
			this.m_lblExtrusionUnit.Location = new System.Drawing.Point( 206, 108 );
			this.m_lblExtrusionUnit.Name = "m_lblExtrusionUnit";
			this.m_lblExtrusionUnit.Size = new System.Drawing.Size( 23, 12 );
			this.m_lblExtrusionUnit.TabIndex = 12;
			this.m_lblExtrusionUnit.Text = "mm";
			this.m_lblExtrusionUnit.Visible = false;
			// 
			// m_lblRotationBandwidth
			// 
			this.m_lblRotationBandwidth.AutoSize = true;
			this.m_lblRotationBandwidth.Location = new System.Drawing.Point( 19, 108 );
			this.m_lblRotationBandwidth.Name = "m_lblRotationBandwidth";
			this.m_lblRotationBandwidth.Size = new System.Drawing.Size( 56, 12 );
			this.m_lblRotationBandwidth.TabIndex = 13;
			this.m_lblRotationBandwidth.Text = "分組帶寬:";
			this.m_lblRotationBandwidth.Visible = false;
			// 
			// m_nudRotationBandwidth
			// 
			this.m_nudRotationBandwidth.DecimalPlaces = 1;
			this.m_nudRotationBandwidth.Increment = new decimal( new int[] { 5, 0, 0, 65536 } );
			this.m_nudRotationBandwidth.Location = new System.Drawing.Point( 100, 105 );
			this.m_nudRotationBandwidth.Maximum = new decimal( new int[] { 180, 0, 0, 0 } );
			this.m_nudRotationBandwidth.Minimum = new decimal( new int[] { 1, 0, 0, 65536 } );
			this.m_nudRotationBandwidth.Name = "m_nudRotationBandwidth";
			this.m_nudRotationBandwidth.Size = new System.Drawing.Size( 100, 22 );
			this.m_nudRotationBandwidth.TabIndex = 14;
			this.m_nudRotationBandwidth.Value = new decimal( new int[] { 30, 0, 0, 65536 } );
			this.m_nudRotationBandwidth.Visible = false;
			// 
			// m_lblRotationUnit
			// 
			this.m_lblRotationUnit.AutoSize = true;
			this.m_lblRotationUnit.Location = new System.Drawing.Point( 206, 108 );
			this.m_lblRotationUnit.Name = "m_lblRotationUnit";
			this.m_lblRotationUnit.Size = new System.Drawing.Size( 11, 12 );
			this.m_lblRotationUnit.TabIndex = 15;
			this.m_lblRotationUnit.Text = "度";
			this.m_lblRotationUnit.Visible = false;
			// 
			// m_picPreview
			// 
			this.m_picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.m_picPreview.Location = new System.Drawing.Point( 19, 136 );
			this.m_picPreview.Name = "m_picPreview";
			this.m_picPreview.Size = new System.Drawing.Size( 229, 337 );
			this.m_picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.m_picPreview.TabIndex = 8;
			this.m_picPreview.TabStop = false;
			this.m_picPreview.Visible = false;
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Enabled = false;
			this.m_btnConfirm.Location = new System.Drawing.Point( 184, 485 );
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size( 64, 24 );
			this.m_btnConfirm.TabIndex = 9;
			this.m_btnConfirm.Text = "確定";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler( this.m_btnConfirm_Click );
			// 
			// AutoSortPathDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 12F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 265, 520 );
			this.Controls.Add( this.m_btnConfirm );
			this.Controls.Add( this.m_lblRotationUnit );
			this.Controls.Add( this.m_nudRotationBandwidth );
			this.Controls.Add( this.m_lblRotationBandwidth );
			this.Controls.Add( this.m_lblExtrusionUnit );
			this.Controls.Add( this.m_nudExtrusionBandwidth );
			this.Controls.Add( this.m_lblExtrusionBandwidth );
			this.Controls.Add( this.m_picPreview );
			this.Controls.Add( this.m_cmbRotationDir );
			this.Controls.Add( this.m_lblRotationDir );
			this.Controls.Add( this.m_cmbExtrusionDir );
			this.Controls.Add( this.m_lblExtrusionDir );
			this.Controls.Add( this.m_cmbCylinderAxis );
			this.Controls.Add( this.m_lblCylinderAxis );
			this.Controls.Add( this.m_lblSelectedPath );
			this.Controls.Add( this.m_lblPathHint );
			this.Controls.Add( this.m_cmbSortMethod );
			this.Controls.Add( this.m_lblSortMethod );
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AutoSortPathDlg";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "自動排序";
			((System.ComponentModel.ISupportInitialize)(this.m_nudExtrusionBandwidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_nudRotationBandwidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picPreview)).EndInit();
			this.TopMost = true;
			this.ResumeLayout( false );
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label m_lblSortMethod;
		private System.Windows.Forms.ComboBox m_cmbSortMethod;
		private System.Windows.Forms.Label m_lblPathHint;
		private System.Windows.Forms.Label m_lblSelectedPath;
		private System.Windows.Forms.Label m_lblCylinderAxis;
		private System.Windows.Forms.ComboBox m_cmbCylinderAxis;
		private System.Windows.Forms.Label m_lblExtrusionDir;
		private System.Windows.Forms.ComboBox m_cmbExtrusionDir;
		private System.Windows.Forms.Label m_lblRotationDir;
		private System.Windows.Forms.ComboBox m_cmbRotationDir;
		private System.Windows.Forms.Label m_lblExtrusionBandwidth;
		private System.Windows.Forms.NumericUpDown m_nudExtrusionBandwidth;
		private System.Windows.Forms.Label m_lblExtrusionUnit;
		private System.Windows.Forms.Label m_lblRotationBandwidth;
		private System.Windows.Forms.NumericUpDown m_nudRotationBandwidth;
		private System.Windows.Forms.Label m_lblRotationUnit;
		private System.Windows.Forms.PictureBox m_picPreview;
		private System.Windows.Forms.Button m_btnConfirm;
	}
}
