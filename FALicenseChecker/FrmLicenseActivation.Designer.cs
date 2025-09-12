namespace FALicenseChecker
{
	partial class FrmLicenseActivation
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
			this.m_btnActivate = new System.Windows.Forms.Button();
			this.m_lblHint = new System.Windows.Forms.Label();
			this.m_txbCode = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// m_btnActivate
			// 
			this.m_btnActivate.Location = new System.Drawing.Point(529, 239);
			this.m_btnActivate.Name = "m_btnActivate";
			this.m_btnActivate.Size = new System.Drawing.Size(75, 23);
			this.m_btnActivate.TabIndex = 0;
			this.m_btnActivate.Text = "Activate";
			this.m_btnActivate.UseVisualStyleBackColor = true;
			this.m_btnActivate.Click += new System.EventHandler(this.m_btnActivate_Click);
			// 
			// m_lblHint
			// 
			this.m_lblHint.AutoSize = true;
			this.m_lblHint.Location = new System.Drawing.Point(23, 9);
			this.m_lblHint.Name = "m_lblHint";
			this.m_lblHint.Size = new System.Drawing.Size(119, 15);
			this.m_lblHint.TabIndex = 1;
			this.m_lblHint.Text = "Enter License Code:";
			// 
			// m_txbCode
			// 
			this.m_txbCode.Location = new System.Drawing.Point(26, 31);
			this.m_txbCode.Multiline = true;
			this.m_txbCode.Name = "m_txbCode";
			this.m_txbCode.Size = new System.Drawing.Size(487, 231);
			this.m_txbCode.TabIndex = 1;
			// 
			// FrmLicenseActivation
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(623, 285);
			this.Controls.Add(this.m_txbCode);
			this.Controls.Add(this.m_lblHint);
			this.Controls.Add(this.m_btnActivate);
			this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FrmLicenseActivation";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "License Activation";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_btnActivate;
		private System.Windows.Forms.Label m_lblHint;
		private System.Windows.Forms.TextBox m_txbCode;
	}
}