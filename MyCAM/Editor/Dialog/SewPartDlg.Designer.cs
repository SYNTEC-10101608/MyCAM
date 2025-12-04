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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblSewTol = new System.Windows.Forms.Label();
			this.m_txbSewTol = new System.Windows.Forms.TextBox();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93F));
			this.tableLayoutPanel1.Controls.Add(this.m_lblSewTol, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_txbSewTol, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_btnOK, 2, 1);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(10, 8);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(269, 83);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// m_lblSewTol
			// 
			this.m_lblSewTol.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblSewTol.AutoSize = true;
			this.m_lblSewTol.Location = new System.Drawing.Point(3, 14);
			this.m_lblSewTol.Name = "m_lblSewTol";
			this.m_lblSewTol.Size = new System.Drawing.Size(82, 12);
			this.m_lblSewTol.TabIndex = 0;
			this.m_lblSewTol.Text = "縫補精度:";
			// 
			// m_txbSewTol
			// 
			this.m_txbSewTol.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.m_txbSewTol.Location = new System.Drawing.Point(91, 9);
			this.m_txbSewTol.Name = "m_txbSewTol";
			this.m_txbSewTol.Size = new System.Drawing.Size(82, 22);
			this.m_txbSewTol.TabIndex = 1;
			this.m_txbSewTol.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbSewTol_KeyDown);
			// 
			// m_btnOK
			// 
			this.m_btnOK.Location = new System.Drawing.Point(179, 44);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(79, 34);
			this.m_btnOK.TabIndex = 2;
			this.m_btnOK.Text = "OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
			// 
			// SewPartDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(290, 98);
			this.Controls.Add(this.tableLayoutPanel1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SewPartDlg";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "縫補設置";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label m_lblSewTol;
		private System.Windows.Forms.TextBox m_txbSewTol;
		private System.Windows.Forms.Button m_btnOK;
	}
}