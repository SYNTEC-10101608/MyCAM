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
			this.m_txbSewTol = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbSewTol)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 124F));
			this.tableLayoutPanel1.Controls.Add(this.m_lblSewTol, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_txbSewTol, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_btnOK, 2, 1);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(13, 10);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(359, 104);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// m_lblSewTol
			// 
			this.m_lblSewTol.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblSewTol.AutoSize = true;
			this.m_lblSewTol.Location = new System.Drawing.Point(4, 18);
			this.m_lblSewTol.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblSewTol.Name = "m_lblSewTol";
			this.m_lblSewTol.Size = new System.Drawing.Size(109, 15);
			this.m_lblSewTol.TabIndex = 0;
			this.m_lblSewTol.Text = "縫補精度:";
			// 
			// m_txbSewTol
			// 
			this.m_txbSewTol.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.m_txbSewTol.DecimalPlaces = 3;
			this.m_txbSewTol.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.m_txbSewTol.Location = new System.Drawing.Point(121, 13);
			this.m_txbSewTol.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_txbSewTol.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_txbSewTol.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.m_txbSewTol.Name = "m_txbSewTol";
			this.m_txbSewTol.Size = new System.Drawing.Size(109, 25);
			this.m_txbSewTol.TabIndex = 1;
			this.m_txbSewTol.Value = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			// 
			// m_btnOK
			// 
			this.m_btnOK.Location = new System.Drawing.Point(238, 56);
			this.m_btnOK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(105, 42);
			this.m_btnOK.TabIndex = 2;
			this.m_btnOK.Text = "OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
			// 
			// SewPartDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(387, 122);
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SewPartDlg";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "縫補設置";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbSewTol)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label m_lblSewTol;
		private MyCAM.Editor.DebouncedNumericUpDown m_txbSewTol;
		private System.Windows.Forms.Button m_btnOK;
	}
}
