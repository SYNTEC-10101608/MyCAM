namespace MyCAM.Editor
{
	partial class SewPartDlg
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
			this.m_pnlSew = new CommonUI.Controls.SectionPanel();
			this.m_lblSewTol = new CommonUI.Controls.ThemedLabel();
			this.m_numSewTol = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_btnOK = new CommonUI.Controls.ThemedButton();
			this.m_pnlSew.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numSewTol)).BeginInit();
			this.SuspendLayout();
			//
			// m_pnlSew
			//
			this.m_pnlSew.Controls.Add(this.m_lblSewTol);
			this.m_pnlSew.Controls.Add(this.m_numSewTol);
			this.m_pnlSew.Location = new System.Drawing.Point(12, 12);
			this.m_pnlSew.Name = "m_pnlSew";
			this.m_pnlSew.Size = new System.Drawing.Size(296, 43);
			this.m_pnlSew.TabIndex = 0;
			//
			// m_lblSewTol
			//
			this.m_lblSewTol.AutoSize = true;
			this.m_lblSewTol.Location = new System.Drawing.Point(10, 14);
			this.m_lblSewTol.Name = "m_lblSewTol";
			this.m_lblSewTol.Size = new System.Drawing.Size(58, 16);
			this.m_lblSewTol.TabIndex = 0;
			this.m_lblSewTol.Text = "縫補精度:";
			//
			// m_numSewTol
			//
			this.m_numSewTol.DecimalPlaces = 3;
			this.m_numSewTol.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.m_numSewTol.Location = new System.Drawing.Point(100, 10);
			this.m_numSewTol.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_numSewTol.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.m_numSewTol.Name = "m_numSewTol";
			this.m_numSewTol.Size = new System.Drawing.Size(150, 23);
			this.m_numSewTol.TabIndex = 1;
			this.m_numSewTol.Value = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			//
			// m_btnOK
			//
			this.m_btnOK.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnOK.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnOK.Location = new System.Drawing.Point(228, 71);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(80, 28);
			this.m_btnOK.TabIndex = 1;
			this.m_btnOK.Text = "確定";
			this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
			//
			// SewPartDlg
			//
			this.ClientSize = new System.Drawing.Size(320, 111);
			this.Controls.Add(this.m_pnlSew);
			this.Controls.Add(this.m_btnOK);
			this.Name = "SewPartDlg";
			this.Text = "縫補設置";
			this.m_pnlSew.ResumeLayout(false);
			this.m_pnlSew.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numSewTol)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlSew;
		private CommonUI.Controls.ThemedLabel m_lblSewTol;
		private CommonUI.Controls.DebouncedNumericUpDown m_numSewTol;
		private CommonUI.Controls.ThemedButton m_btnOK;
	}
}
