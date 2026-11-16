namespace MyCAM.Editor.Dialog
{
	partial class ConnectDlg
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
			this.m_pnlConnect = new CommonUI.Controls.SectionPanel();
			this.m_lblIP = new System.Windows.Forms.Label();
			this.m_txtIP = new System.Windows.Forms.TextBox();
			this.m_lblTips = new CommonUI.Controls.ThemedLabel();
			this.m_btnConfirm = new CommonUI.Controls.ThemedButton();
			this.m_pnlConnect.SuspendLayout();
			this.SuspendLayout();
			//
			// m_pnlConnect
			//
			this.m_pnlConnect.Controls.Add(this.m_lblIP);
			this.m_pnlConnect.Controls.Add(this.m_txtIP);
			this.m_pnlConnect.Controls.Add(this.m_lblTips);
			this.m_pnlConnect.Location = new System.Drawing.Point(12, 12);
			this.m_pnlConnect.Name = "m_pnlConnect";
			this.m_pnlConnect.Size = new System.Drawing.Size(296, 73);
			this.m_pnlConnect.TabIndex = 0;
			//
			// m_lblIP
			//
			this.m_lblIP.AutoSize = true;
			this.m_lblIP.Location = new System.Drawing.Point(10, 14);
			this.m_lblIP.Name = "m_lblIP";
			this.m_lblIP.Size = new System.Drawing.Size(27, 16);
			this.m_lblIP.TabIndex = 0;
			this.m_lblIP.Text = "IP：";
			//
			// m_txtIP
			//
			this.m_txtIP.Location = new System.Drawing.Point(50, 10);
			this.m_txtIP.Name = "m_txtIP";
			this.m_txtIP.Size = new System.Drawing.Size(150, 23);
			this.m_txtIP.TabIndex = 1;
			//
			// m_lblTips
			//
			this.m_lblTips.Location = new System.Drawing.Point(50, 44);
			this.m_lblTips.Name = "m_lblTips";
			this.m_lblTips.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblTips.Size = new System.Drawing.Size(236, 19);
			this.m_lblTips.TabIndex = 2;
			this.m_lblTips.Text = "請以IPv4格式輸入";
			//
			// m_btnConfirm
			//
			this.m_btnConfirm.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnConfirm.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnConfirm.Location = new System.Drawing.Point(228, 101);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(80, 28);
			this.m_btnConfirm.TabIndex = 1;
			this.m_btnConfirm.Text = "確定";
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			//
			// ConnectDlg
			//
			this.ClientSize = new System.Drawing.Size(320, 141);
			this.Controls.Add(this.m_pnlConnect);
			this.Controls.Add(this.m_btnConfirm);
			this.Name = "ConnectDlg";
			this.Text = "連線設定";
			this.m_pnlConnect.ResumeLayout(false);
			this.m_pnlConnect.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlConnect;
		private System.Windows.Forms.Label m_lblIP;
		private System.Windows.Forms.TextBox m_txtIP;
		private CommonUI.Controls.ThemedLabel m_lblTips;
		private CommonUI.Controls.ThemedButton m_btnConfirm;
	}
}
