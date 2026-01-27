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
			this.m_txbIP = new System.Windows.Forms.TextBox();
			this.m_lblIP = new System.Windows.Forms.Label();
			this.m_btnConfirm = new System.Windows.Forms.Button();
			this.m_lblTips = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// m_txbIP
			// 
			this.m_txbIP.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_txbIP.Location = new System.Drawing.Point(83, 20);
			this.m_txbIP.Name = "m_txbIP";
			this.m_txbIP.Size = new System.Drawing.Size(154, 21);
			this.m_txbIP.TabIndex = 0;
			// 
			// m_lblIP
			// 
			this.m_lblIP.AutoSize = true;
			this.m_lblIP.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblIP.Location = new System.Drawing.Point(39, 20);
			this.m_lblIP.Name = "m_lblIP";
			this.m_lblIP.Size = new System.Drawing.Size(38, 18);
			this.m_lblIP.TabIndex = 1;
			this.m_lblIP.Text = "IP：";
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_btnConfirm.Location = new System.Drawing.Point(248, 68);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(75, 23);
			this.m_btnConfirm.TabIndex = 2;
			this.m_btnConfirm.Text = "確認";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// m_lblTips
			// 
			this.m_lblTips.AutoSize = true;
			this.m_lblTips.Location = new System.Drawing.Point(83, 48);
			this.m_lblTips.Name = "m_lblTips";
			this.m_lblTips.Size = new System.Drawing.Size(99, 12);
			this.m_lblTips.TabIndex = 3;
			this.m_lblTips.Text = "請以IPv4格式輸入";
			// 
			// ConnectDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(335, 103);
			this.Controls.Add(this.m_lblTips);
			this.Controls.Add(this.m_btnConfirm);
			this.Controls.Add(this.m_lblIP);
			this.Controls.Add(this.m_txbIP);
			this.Name = "ConnectDlg";
			this.Text = "連線設定";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox m_txbIP;
		private System.Windows.Forms.Label m_lblIP;
		private System.Windows.Forms.Button m_btnConfirm;
		private System.Windows.Forms.Label m_lblTips;
	}
}