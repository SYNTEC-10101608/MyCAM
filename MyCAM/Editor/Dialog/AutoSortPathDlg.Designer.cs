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
			this.m_pnlSort = new CommonUI.Controls.SectionPanel();
			this.m_lblSortMethod = new System.Windows.Forms.Label();
			this.m_cmbSortMethod = new System.Windows.Forms.ComboBox();
			this.m_lblPathHint = new System.Windows.Forms.Label();
			this.m_lblSelectedPath = new CommonUI.Controls.ThemedLabel();
			this.m_lblCylinderAxis = new System.Windows.Forms.Label();
			this.m_cmbCylinderAxis = new System.Windows.Forms.ComboBox();
			this.m_lblExtrusionDir = new System.Windows.Forms.Label();
			this.m_cmbExtrusionDir = new System.Windows.Forms.ComboBox();
			this.m_lblRotationDir = new System.Windows.Forms.Label();
			this.m_cmbRotationDir = new System.Windows.Forms.ComboBox();
			this.m_lblExtrusionBandwidth = new System.Windows.Forms.Label();
			this.m_numExtrusionBandwidth = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblExtrusionUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblRotationBandwidth = new System.Windows.Forms.Label();
			this.m_numRotationBandwidth = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblRotationUnit = new CommonUI.Controls.ThemedLabel();
			this.m_picPreview = new System.Windows.Forms.PictureBox();
			this.m_btnConfirm = new CommonUI.Controls.ThemedButton();
			this.m_pnlSort.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numExtrusionBandwidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRotationBandwidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picPreview)).BeginInit();
			this.SuspendLayout();
			//
			// m_pnlSort
			//
			this.m_pnlSort.Controls.Add(this.m_lblSortMethod);
			this.m_pnlSort.Controls.Add(this.m_cmbSortMethod);
			this.m_pnlSort.Controls.Add(this.m_lblPathHint);
			this.m_pnlSort.Controls.Add(this.m_lblSelectedPath);
			this.m_pnlSort.Controls.Add(this.m_lblCylinderAxis);
			this.m_pnlSort.Controls.Add(this.m_cmbCylinderAxis);
			this.m_pnlSort.Controls.Add(this.m_lblExtrusionDir);
			this.m_pnlSort.Controls.Add(this.m_cmbExtrusionDir);
			this.m_pnlSort.Controls.Add(this.m_lblRotationDir);
			this.m_pnlSort.Controls.Add(this.m_cmbRotationDir);
			this.m_pnlSort.Controls.Add(this.m_lblExtrusionBandwidth);
			this.m_pnlSort.Controls.Add(this.m_numExtrusionBandwidth);
			this.m_pnlSort.Controls.Add(this.m_lblExtrusionUnit);
			this.m_pnlSort.Controls.Add(this.m_lblRotationBandwidth);
			this.m_pnlSort.Controls.Add(this.m_numRotationBandwidth);
			this.m_pnlSort.Controls.Add(this.m_lblRotationUnit);
			this.m_pnlSort.Controls.Add(this.m_picPreview);
			this.m_pnlSort.Location = new System.Drawing.Point(12, 12);
			this.m_pnlSort.Name = "m_pnlSort";
			this.m_pnlSort.Size = new System.Drawing.Size(296, 477);
			this.m_pnlSort.TabIndex = 0;
			//
			// m_lblSortMethod
			//
			this.m_lblSortMethod.AutoSize = true;
			this.m_lblSortMethod.Location = new System.Drawing.Point(10, 14);
			this.m_lblSortMethod.Name = "m_lblSortMethod";
			this.m_lblSortMethod.Size = new System.Drawing.Size(58, 16);
			this.m_lblSortMethod.TabIndex = 0;
			this.m_lblSortMethod.Text = "排序方式:";
			//
			// m_cmbSortMethod
			//
			this.m_cmbSortMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbSortMethod.FormattingEnabled = true;
			this.m_cmbSortMethod.Location = new System.Drawing.Point(100, 10);
			this.m_cmbSortMethod.Name = "m_cmbSortMethod";
			this.m_cmbSortMethod.Size = new System.Drawing.Size(186, 24);
			this.m_cmbSortMethod.TabIndex = 1;
			this.m_cmbSortMethod.SelectedIndexChanged += new System.EventHandler(this.m_cmbSortMethod_SelectedIndexChanged);
			//
			// m_lblPathHint
			//
			this.m_lblPathHint.AutoSize = true;
			this.m_lblPathHint.Location = new System.Drawing.Point(10, 44);
			this.m_lblPathHint.Name = "m_lblPathHint";
			this.m_lblPathHint.Size = new System.Drawing.Size(58, 16);
			this.m_lblPathHint.TabIndex = 2;
			this.m_lblPathHint.Text = "起始路徑:";
			//
			// m_lblSelectedPath
			//
			this.m_lblSelectedPath.AutoSize = true;
			this.m_lblSelectedPath.Location = new System.Drawing.Point(100, 44);
			this.m_lblSelectedPath.Name = "m_lblSelectedPath";
			this.m_lblSelectedPath.Role = CommonUI.Theme.TextRole.Error;
			this.m_lblSelectedPath.Size = new System.Drawing.Size(43, 16);
			this.m_lblSelectedPath.TabIndex = 4;
			this.m_lblSelectedPath.Text = "未選取";
			//
			// m_lblCylinderAxis
			//
			this.m_lblCylinderAxis.AutoSize = true;
			this.m_lblCylinderAxis.Location = new System.Drawing.Point(10, 44);
			this.m_lblCylinderAxis.Name = "m_lblCylinderAxis";
			this.m_lblCylinderAxis.Size = new System.Drawing.Size(58, 16);
			this.m_lblCylinderAxis.TabIndex = 3;
			this.m_lblCylinderAxis.Text = "圓管軸向:";
			this.m_lblCylinderAxis.Visible = false;
			//
			// m_cmbCylinderAxis
			//
			this.m_cmbCylinderAxis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbCylinderAxis.FormattingEnabled = true;
			this.m_cmbCylinderAxis.Location = new System.Drawing.Point(100, 40);
			this.m_cmbCylinderAxis.Name = "m_cmbCylinderAxis";
			this.m_cmbCylinderAxis.Size = new System.Drawing.Size(186, 24);
			this.m_cmbCylinderAxis.TabIndex = 5;
			this.m_cmbCylinderAxis.Visible = false;
			//
			// m_lblExtrusionDir
			//
			this.m_lblExtrusionDir.AutoSize = true;
			this.m_lblExtrusionDir.Location = new System.Drawing.Point(10, 74);
			this.m_lblExtrusionDir.Name = "m_lblExtrusionDir";
			this.m_lblExtrusionDir.Size = new System.Drawing.Size(70, 16);
			this.m_lblExtrusionDir.TabIndex = 7;
			this.m_lblExtrusionDir.Text = "拉伸軸方向:";
			this.m_lblExtrusionDir.Visible = false;
			//
			// m_cmbExtrusionDir
			//
			this.m_cmbExtrusionDir.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbExtrusionDir.FormattingEnabled = true;
			this.m_cmbExtrusionDir.Location = new System.Drawing.Point(100, 70);
			this.m_cmbExtrusionDir.Name = "m_cmbExtrusionDir";
			this.m_cmbExtrusionDir.Size = new System.Drawing.Size(186, 24);
			this.m_cmbExtrusionDir.TabIndex = 8;
			this.m_cmbExtrusionDir.Visible = false;
			this.m_cmbExtrusionDir.SelectedIndexChanged += new System.EventHandler(this.m_cmbExtrusionDir_SelectedIndexChanged);
			//
			// m_lblRotationDir
			//
			this.m_lblRotationDir.AutoSize = true;
			this.m_lblRotationDir.Location = new System.Drawing.Point(10, 74);
			this.m_lblRotationDir.Name = "m_lblRotationDir";
			this.m_lblRotationDir.Size = new System.Drawing.Size(70, 16);
			this.m_lblRotationDir.TabIndex = 6;
			this.m_lblRotationDir.Text = "旋轉軸方向:";
			this.m_lblRotationDir.Visible = false;
			//
			// m_cmbRotationDir
			//
			this.m_cmbRotationDir.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbRotationDir.FormattingEnabled = true;
			this.m_cmbRotationDir.Location = new System.Drawing.Point(100, 70);
			this.m_cmbRotationDir.Name = "m_cmbRotationDir";
			this.m_cmbRotationDir.Size = new System.Drawing.Size(186, 24);
			this.m_cmbRotationDir.TabIndex = 9;
			this.m_cmbRotationDir.Visible = false;
			this.m_cmbRotationDir.SelectedIndexChanged += new System.EventHandler(this.m_cmbRotationDir_SelectedIndexChanged);
			//
			// m_lblExtrusionBandwidth
			//
			this.m_lblExtrusionBandwidth.AutoSize = true;
			this.m_lblExtrusionBandwidth.Location = new System.Drawing.Point(10, 104);
			this.m_lblExtrusionBandwidth.Name = "m_lblExtrusionBandwidth";
			this.m_lblExtrusionBandwidth.Size = new System.Drawing.Size(58, 16);
			this.m_lblExtrusionBandwidth.TabIndex = 11;
			this.m_lblExtrusionBandwidth.Text = "分組帶寬:";
			this.m_lblExtrusionBandwidth.Visible = false;
			//
			// m_numExtrusionBandwidth
			//
			this.m_numExtrusionBandwidth.DecimalPlaces = 1;
			this.m_numExtrusionBandwidth.Increment = new decimal( new int[] { 5, 0, 0, 65536 } );
			this.m_numExtrusionBandwidth.Location = new System.Drawing.Point(100, 100);
			this.m_numExtrusionBandwidth.Maximum = new decimal( new int[] { 1000, 0, 0, 0 } );
			this.m_numExtrusionBandwidth.Minimum = new decimal( new int[] { 1, 0, 0, 65536 } );
			this.m_numExtrusionBandwidth.Name = "m_numExtrusionBandwidth";
			this.m_numExtrusionBandwidth.Size = new System.Drawing.Size(100, 23);
			this.m_numExtrusionBandwidth.TabIndex = 13;
			this.m_numExtrusionBandwidth.Value = new decimal( new int[] { 100, 0, 0, 65536 } );
			this.m_numExtrusionBandwidth.Visible = false;
			//
			// m_lblExtrusionUnit
			//
			this.m_lblExtrusionUnit.AutoSize = true;
			this.m_lblExtrusionUnit.Location = new System.Drawing.Point(206, 104);
			this.m_lblExtrusionUnit.Name = "m_lblExtrusionUnit";
			this.m_lblExtrusionUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblExtrusionUnit.Size = new System.Drawing.Size(20, 16);
			this.m_lblExtrusionUnit.TabIndex = 14;
			this.m_lblExtrusionUnit.Text = "mm";
			this.m_lblExtrusionUnit.Visible = false;
			//
			// m_lblRotationBandwidth
			//
			this.m_lblRotationBandwidth.AutoSize = true;
			this.m_lblRotationBandwidth.Location = new System.Drawing.Point(10, 104);
			this.m_lblRotationBandwidth.Name = "m_lblRotationBandwidth";
			this.m_lblRotationBandwidth.Size = new System.Drawing.Size(58, 16);
			this.m_lblRotationBandwidth.TabIndex = 10;
			this.m_lblRotationBandwidth.Text = "分組帶寬:";
			this.m_lblRotationBandwidth.Visible = false;
			//
			// m_numRotationBandwidth
			//
			this.m_numRotationBandwidth.DecimalPlaces = 1;
			this.m_numRotationBandwidth.Increment = new decimal( new int[] { 5, 0, 0, 65536 } );
			this.m_numRotationBandwidth.Location = new System.Drawing.Point(100, 100);
			this.m_numRotationBandwidth.Maximum = new decimal( new int[] { 180, 0, 0, 0 } );
			this.m_numRotationBandwidth.Minimum = new decimal( new int[] { 1, 0, 0, 65536 } );
			this.m_numRotationBandwidth.Name = "m_numRotationBandwidth";
			this.m_numRotationBandwidth.Size = new System.Drawing.Size(100, 23);
			this.m_numRotationBandwidth.TabIndex = 12;
			this.m_numRotationBandwidth.Value = new decimal( new int[] { 30, 0, 0, 65536 } );
			this.m_numRotationBandwidth.Visible = false;
			//
			// m_lblRotationUnit
			//
			this.m_lblRotationUnit.AutoSize = true;
			this.m_lblRotationUnit.Location = new System.Drawing.Point(206, 104);
			this.m_lblRotationUnit.Name = "m_lblRotationUnit";
			this.m_lblRotationUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblRotationUnit.Size = new System.Drawing.Size(19, 16);
			this.m_lblRotationUnit.TabIndex = 15;
			this.m_lblRotationUnit.Text = "度";
			this.m_lblRotationUnit.Visible = false;
			//
			// m_picPreview
			//
			this.m_picPreview.Location = new System.Drawing.Point(10, 130);
			this.m_picPreview.Name = "m_picPreview";
			this.m_picPreview.Size = new System.Drawing.Size(276, 337);
			this.m_picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.m_picPreview.TabIndex = 16;
			this.m_picPreview.TabStop = false;
			this.m_picPreview.Visible = false;
			//
			// m_btnConfirm
			//
			this.m_btnConfirm.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnConfirm.Enabled = false;
			this.m_btnConfirm.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnConfirm.Location = new System.Drawing.Point(228, 505);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(80, 28);
			this.m_btnConfirm.TabIndex = 1;
			this.m_btnConfirm.Text = "確定";
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			//
			// AutoSortPathDlg
			//
			this.ClientSize = new System.Drawing.Size(320, 545);
			this.Controls.Add(this.m_pnlSort);
			this.Controls.Add(this.m_btnConfirm);
			this.Name = "AutoSortPathDlg";
			this.Text = "自動排序";
			this.m_pnlSort.ResumeLayout(false);
			this.m_pnlSort.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numExtrusionBandwidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRotationBandwidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picPreview)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlSort;
		private System.Windows.Forms.Label m_lblSortMethod;
		private System.Windows.Forms.ComboBox m_cmbSortMethod;
		private System.Windows.Forms.Label m_lblPathHint;
		private CommonUI.Controls.ThemedLabel m_lblSelectedPath;
		private System.Windows.Forms.Label m_lblCylinderAxis;
		private System.Windows.Forms.ComboBox m_cmbCylinderAxis;
		private System.Windows.Forms.Label m_lblExtrusionDir;
		private System.Windows.Forms.ComboBox m_cmbExtrusionDir;
		private System.Windows.Forms.Label m_lblRotationDir;
		private System.Windows.Forms.ComboBox m_cmbRotationDir;
		private System.Windows.Forms.Label m_lblExtrusionBandwidth;
		private CommonUI.Controls.DebouncedNumericUpDown m_numExtrusionBandwidth;
		private CommonUI.Controls.ThemedLabel m_lblExtrusionUnit;
		private System.Windows.Forms.Label m_lblRotationBandwidth;
		private CommonUI.Controls.DebouncedNumericUpDown m_numRotationBandwidth;
		private CommonUI.Controls.ThemedLabel m_lblRotationUnit;
		private System.Windows.Forms.PictureBox m_picPreview;
		private CommonUI.Controls.ThemedButton m_btnConfirm;
	}
}
