namespace MyCAM.Editor.Dialog
{
	partial class PathEditDlg
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
			this.m_pnlMove = new CommonUI.Controls.SectionPanel();
			this.m_lblRefCoordType = new System.Windows.Forms.Label();
			this.m_cmbRefCoordType = new System.Windows.Forms.ComboBox();
			this.m_lblTotalDiff = new System.Windows.Forms.Label();
			this.m_lblX = new System.Windows.Forms.Label();
			this.m_numX = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblTotalDX = new System.Windows.Forms.Label();
			this.m_lblY = new System.Windows.Forms.Label();
			this.m_numY = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblTotalDY = new System.Windows.Forms.Label();
			this.m_lblZ = new System.Windows.Forms.Label();
			this.m_numZ = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblTotalDZ = new System.Windows.Forms.Label();
			this.m_btnReset = new CommonUI.Controls.ThemedButton();
			this.m_btnClear = new CommonUI.Controls.ThemedButton();
			this.m_btnConfirm = new CommonUI.Controls.ThemedButton();
			this.m_pnlMove.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numZ)).BeginInit();
			this.SuspendLayout();
			//
			// m_pnlMove
			//
			this.m_pnlMove.Controls.Add(this.m_lblRefCoordType);
			this.m_pnlMove.Controls.Add(this.m_cmbRefCoordType);
			this.m_pnlMove.Controls.Add(this.m_lblTotalDiff);
			this.m_pnlMove.Controls.Add(this.m_lblX);
			this.m_pnlMove.Controls.Add(this.m_numX);
			this.m_pnlMove.Controls.Add(this.m_lblTotalDX);
			this.m_pnlMove.Controls.Add(this.m_lblY);
			this.m_pnlMove.Controls.Add(this.m_numY);
			this.m_pnlMove.Controls.Add(this.m_lblTotalDY);
			this.m_pnlMove.Controls.Add(this.m_lblZ);
			this.m_pnlMove.Controls.Add(this.m_numZ);
			this.m_pnlMove.Controls.Add(this.m_lblTotalDZ);
			this.m_pnlMove.Location = new System.Drawing.Point(12, 12);
			this.m_pnlMove.Name = "m_pnlMove";
			this.m_pnlMove.Size = new System.Drawing.Size(336, 159);
			this.m_pnlMove.TabIndex = 0;
			this.m_pnlMove.Text = "平移";
			//
			// m_lblRefCoordType
			//
			this.m_lblRefCoordType.AutoSize = true;
			this.m_lblRefCoordType.Location = new System.Drawing.Point(10, 40);
			this.m_lblRefCoordType.Name = "m_lblRefCoordType";
			this.m_lblRefCoordType.Size = new System.Drawing.Size(43, 16);
			this.m_lblRefCoordType.TabIndex = 0;
			this.m_lblRefCoordType.Text = "座標系";
			//
			// m_cmbRefCoordType
			//
			this.m_cmbRefCoordType.FormattingEnabled = true;
			this.m_cmbRefCoordType.Items.AddRange(new object[] {
            "區域坐標系",
            "大地坐標系"});
			this.m_cmbRefCoordType.Location = new System.Drawing.Point(100, 36);
			this.m_cmbRefCoordType.Name = "m_cmbRefCoordType";
			this.m_cmbRefCoordType.Size = new System.Drawing.Size(130, 24);
			this.m_cmbRefCoordType.TabIndex = 1;
			this.m_cmbRefCoordType.SelectedIndexChanged += new System.EventHandler(this.m_cmbRefCoordType_SelectedIndexChanged);
			//
			// m_lblTotalDiff
			//
			this.m_lblTotalDiff.AutoSize = true;
			this.m_lblTotalDiff.Location = new System.Drawing.Point(246, 40);
			this.m_lblTotalDiff.Name = "m_lblTotalDiff";
			this.m_lblTotalDiff.Size = new System.Drawing.Size(43, 16);
			this.m_lblTotalDiff.TabIndex = 2;
			this.m_lblTotalDiff.Text = "總偏移";
			//
			// m_lblX
			//
			this.m_lblX.AutoSize = true;
			this.m_lblX.Location = new System.Drawing.Point(10, 70);
			this.m_lblX.Name = "m_lblX";
			this.m_lblX.Size = new System.Drawing.Size(39, 16);
			this.m_lblX.TabIndex = 3;
			this.m_lblX.Text = "X偏移";
			//
			// m_numX
			//
			this.m_numX.DecimalPlaces = 3;
			this.m_numX.Location = new System.Drawing.Point(100, 66);
			this.m_numX.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_numX.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_numX.Name = "m_numX";
			this.m_numX.RestrictInvalidCharacters = false;
			this.m_numX.Size = new System.Drawing.Size(130, 23);
			this.m_numX.TabIndex = 4;
			this.m_numX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.m_numX.DebouncedValueChanged += new System.EventHandler(this.m_numX_DebouncedValueChanged);
			//
			// m_lblTotalDX
			//
			this.m_lblTotalDX.AutoSize = true;
			this.m_lblTotalDX.Location = new System.Drawing.Point(246, 70);
			this.m_lblTotalDX.Name = "m_lblTotalDX";
			this.m_lblTotalDX.Size = new System.Drawing.Size(38, 16);
			this.m_lblTotalDX.TabIndex = 5;
			this.m_lblTotalDX.Text = "0.000";
			//
			// m_lblY
			//
			this.m_lblY.AutoSize = true;
			this.m_lblY.Location = new System.Drawing.Point(10, 100);
			this.m_lblY.Name = "m_lblY";
			this.m_lblY.Size = new System.Drawing.Size(39, 16);
			this.m_lblY.TabIndex = 6;
			this.m_lblY.Text = "Y偏移";
			//
			// m_numY
			//
			this.m_numY.DecimalPlaces = 3;
			this.m_numY.Location = new System.Drawing.Point(100, 96);
			this.m_numY.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_numY.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_numY.Name = "m_numY";
			this.m_numY.RestrictInvalidCharacters = false;
			this.m_numY.Size = new System.Drawing.Size(130, 23);
			this.m_numY.TabIndex = 7;
			this.m_numY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.m_numY.DebouncedValueChanged += new System.EventHandler(this.m_numY_DebouncedValueChanged);
			//
			// m_lblTotalDY
			//
			this.m_lblTotalDY.AutoSize = true;
			this.m_lblTotalDY.Location = new System.Drawing.Point(246, 100);
			this.m_lblTotalDY.Name = "m_lblTotalDY";
			this.m_lblTotalDY.Size = new System.Drawing.Size(38, 16);
			this.m_lblTotalDY.TabIndex = 8;
			this.m_lblTotalDY.Text = "0.000";
			//
			// m_lblZ
			//
			this.m_lblZ.AutoSize = true;
			this.m_lblZ.Location = new System.Drawing.Point(10, 130);
			this.m_lblZ.Name = "m_lblZ";
			this.m_lblZ.Size = new System.Drawing.Size(39, 16);
			this.m_lblZ.TabIndex = 9;
			this.m_lblZ.Text = "Z偏移";
			//
			// m_numZ
			//
			this.m_numZ.DecimalPlaces = 3;
			this.m_numZ.Location = new System.Drawing.Point(100, 126);
			this.m_numZ.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_numZ.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_numZ.Name = "m_numZ";
			this.m_numZ.RestrictInvalidCharacters = false;
			this.m_numZ.Size = new System.Drawing.Size(130, 23);
			this.m_numZ.TabIndex = 10;
			this.m_numZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.m_numZ.DebouncedValueChanged += new System.EventHandler(this.m_numZ_DebouncedValueChanged);
			//
			// m_lblTotalDZ
			//
			this.m_lblTotalDZ.AutoSize = true;
			this.m_lblTotalDZ.Location = new System.Drawing.Point(246, 130);
			this.m_lblTotalDZ.Name = "m_lblTotalDZ";
			this.m_lblTotalDZ.Size = new System.Drawing.Size(38, 16);
			this.m_lblTotalDZ.TabIndex = 11;
			this.m_lblTotalDZ.Text = "0.000";
			//
			// m_btnReset
			//
			this.m_btnReset.CommonText = CommonUI.Controls.CommonButtonText.Reset;
			this.m_btnReset.Location = new System.Drawing.Point(12, 187);
			this.m_btnReset.Name = "m_btnReset";
			this.m_btnReset.Size = new System.Drawing.Size(80, 28);
			this.m_btnReset.TabIndex = 1;
			this.m_btnReset.Text = "重設";
			this.m_btnReset.Click += new System.EventHandler(this.m_btnReset_Click);
			//
			// m_btnClear
			//
			this.m_btnClear.Location = new System.Drawing.Point(100, 187);
			this.m_btnClear.Name = "m_btnClear";
			this.m_btnClear.Size = new System.Drawing.Size(80, 28);
			this.m_btnClear.TabIndex = 2;
			this.m_btnClear.Text = "清除";
			this.m_btnClear.Click += new System.EventHandler(this.m_btnClear_Click);
			//
			// m_btnConfirm
			//
			this.m_btnConfirm.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnConfirm.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnConfirm.Location = new System.Drawing.Point(268, 187);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(80, 28);
			this.m_btnConfirm.TabIndex = 3;
			this.m_btnConfirm.Text = "確定";
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			//
			// PathEditDlg
			//
			this.ClientSize = new System.Drawing.Size(360, 227);
			this.Controls.Add(this.m_pnlMove);
			this.Controls.Add(this.m_btnReset);
			this.Controls.Add(this.m_btnClear);
			this.Controls.Add(this.m_btnConfirm);
			this.Name = "PathEditDlg";
			this.Text = "路徑編輯";
			this.Shown += new System.EventHandler(this.PathEditDlg_Shown);
			this.m_pnlMove.ResumeLayout(false);
			this.m_pnlMove.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numZ)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlMove;
		private System.Windows.Forms.Label m_lblRefCoordType;
		private System.Windows.Forms.ComboBox m_cmbRefCoordType;
		private System.Windows.Forms.Label m_lblTotalDiff;
		private System.Windows.Forms.Label m_lblX;
		private CommonUI.Controls.DebouncedNumericUpDown m_numX;
		private System.Windows.Forms.Label m_lblTotalDX;
		private System.Windows.Forms.Label m_lblY;
		private CommonUI.Controls.DebouncedNumericUpDown m_numY;
		private System.Windows.Forms.Label m_lblTotalDY;
		private System.Windows.Forms.Label m_lblZ;
		private CommonUI.Controls.DebouncedNumericUpDown m_numZ;
		private System.Windows.Forms.Label m_lblTotalDZ;
		private CommonUI.Controls.ThemedButton m_btnReset;
		private CommonUI.Controls.ThemedButton m_btnClear;
		private CommonUI.Controls.ThemedButton m_btnConfirm;
	}
}
