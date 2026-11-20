namespace MyCAM.Editor.Dialog
{
	partial class GlassDXFImportDlg
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
			this.m_pnlParam = new CommonUI.Controls.SectionPanel();
			this.m_lblRadius = new CommonUI.Controls.ThemedLabel();
			this.m_numRadius = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblHeight = new CommonUI.Controls.ThemedLabel();
			this.m_numHeight = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_btnCancel = new CommonUI.Controls.ThemedButton();
			this.m_btnOK = new CommonUI.Controls.ThemedButton();
			this.m_pnlParam.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numRadius)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numHeight)).BeginInit();
			this.SuspendLayout();
			//
			// m_pnlParam
			//
			this.m_pnlParam.Controls.Add(this.m_lblRadius);
			this.m_pnlParam.Controls.Add(this.m_numRadius);
			this.m_pnlParam.Controls.Add(this.m_lblHeight);
			this.m_pnlParam.Controls.Add(this.m_numHeight);
			this.m_pnlParam.Location = new System.Drawing.Point(12, 12);
			this.m_pnlParam.Name = "m_pnlParam";
			this.m_pnlParam.Size = new System.Drawing.Size(296, 73);
			this.m_pnlParam.TabIndex = 0;
			//
			// m_lblRadius
			//
			this.m_lblRadius.AutoSize = true;
			this.m_lblRadius.Location = new System.Drawing.Point(10, 14);
			this.m_lblRadius.Name = "m_lblRadius";
			this.m_lblRadius.Size = new System.Drawing.Size(74, 16);
			this.m_lblRadius.TabIndex = 0;
			this.m_lblRadius.Text = "圓球半徑(R)";
			//
			// m_numRadius
			//
			this.m_numRadius.DecimalPlaces = 3;
			this.m_numRadius.Location = new System.Drawing.Point(110, 10);
			this.m_numRadius.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.m_numRadius.Name = "m_numRadius";
			this.m_numRadius.Size = new System.Drawing.Size(150, 23);
			this.m_numRadius.TabIndex = 1;
			this.m_numRadius.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
			//
			// m_lblHeight
			//
			this.m_lblHeight.AutoSize = true;
			this.m_lblHeight.Location = new System.Drawing.Point(10, 44);
			this.m_lblHeight.Name = "m_lblHeight";
			this.m_lblHeight.Size = new System.Drawing.Size(74, 16);
			this.m_lblHeight.TabIndex = 2;
			this.m_lblHeight.Text = "曲面高度(H)";
			//
			// m_numHeight
			//
			this.m_numHeight.DecimalPlaces = 3;
			this.m_numHeight.Location = new System.Drawing.Point(110, 40);
			this.m_numHeight.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_numHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.m_numHeight.Name = "m_numHeight";
			this.m_numHeight.Size = new System.Drawing.Size(150, 23);
			this.m_numHeight.TabIndex = 3;
			this.m_numHeight.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			//
			// m_btnCancel
			//
			this.m_btnCancel.CommonText = CommonUI.Controls.CommonButtonText.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(140, 101);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(80, 28);
			this.m_btnCancel.TabIndex = 1;
			this.m_btnCancel.Text = "取消";
			this.m_btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			//
			// m_btnOK
			//
			this.m_btnOK.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnOK.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnOK.Location = new System.Drawing.Point(228, 101);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(80, 28);
			this.m_btnOK.TabIndex = 2;
			this.m_btnOK.Text = "確定";
			this.m_btnOK.Click += new System.EventHandler(this.BtnOK_Click);
			//
			// GlassDXFImportDlg
			//
			this.AcceptButton = this.m_btnOK;
			this.ClientSize = new System.Drawing.Size(320, 141);
			this.Controls.Add(this.m_pnlParam);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.Name = "GlassDXFImportDlg";
			this.Text = "玻璃DXF匯入";
			this.m_pnlParam.ResumeLayout(false);
			this.m_pnlParam.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numRadius)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numHeight)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlParam;
		private CommonUI.Controls.ThemedLabel m_lblRadius;
		private CommonUI.Controls.DebouncedNumericUpDown m_numRadius;
		private CommonUI.Controls.ThemedLabel m_lblHeight;
		private CommonUI.Controls.DebouncedNumericUpDown m_numHeight;
		private CommonUI.Controls.ThemedButton m_btnCancel;
		private CommonUI.Controls.ThemedButton m_btnOK;
	}
}
