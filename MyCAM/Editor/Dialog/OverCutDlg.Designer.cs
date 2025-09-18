namespace MyCAM.Editor.Dialog
{
	partial class OverCutDlg
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
			this.m_lblOverCutLength = new System.Windows.Forms.Label();
			this.m_tbxOverCutLength = new System.Windows.Forms.TextBox();
			this.m_OverCutUnit = new System.Windows.Forms.Label();
			this.m_btnSure = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// m_lblOverCutLength
			// 
			this.m_lblOverCutLength.AutoSize = true;
			this.m_lblOverCutLength.Location = new System.Drawing.Point(19, 21);
			this.m_lblOverCutLength.Name = "m_lblOverCutLength";
			this.m_lblOverCutLength.Size = new System.Drawing.Size(56, 12);
			this.m_lblOverCutLength.TabIndex = 0;
			this.m_lblOverCutLength.Text = "過切長度:";
			// 
			// m_tbxOverCutLength
			// 
			this.m_tbxOverCutLength.Location = new System.Drawing.Point(88, 18);
			this.m_tbxOverCutLength.Name = "m_tbxOverCutLength";
			this.m_tbxOverCutLength.Size = new System.Drawing.Size(92, 22);
			this.m_tbxOverCutLength.TabIndex = 1;
			this.m_tbxOverCutLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxOverCutLength_KeyDown);
			this.m_tbxOverCutLength.Leave += new System.EventHandler(this.m_tbxOverCutLength_Leave);
			// 
			// m_OverCutUnit
			// 
			this.m_OverCutUnit.AutoSize = true;
			this.m_OverCutUnit.Location = new System.Drawing.Point(195, 21);
			this.m_OverCutUnit.Name = "m_OverCutUnit";
			this.m_OverCutUnit.Size = new System.Drawing.Size(23, 12);
			this.m_OverCutUnit.TabIndex = 2;
			this.m_OverCutUnit.Text = "mm";
			// 
			// m_btnSure
			// 
			this.m_btnSure.Location = new System.Drawing.Point(188, 52);
			this.m_btnSure.Name = "m_btnSure";
			this.m_btnSure.Size = new System.Drawing.Size(64, 24);
			this.m_btnSure.TabIndex = 3;
			this.m_btnSure.Text = "OK";
			this.m_btnSure.UseVisualStyleBackColor = true;
			this.m_btnSure.Click += new System.EventHandler(this.m_btnSure_Click);
			// 
			// OverCutForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(265, 94);
			this.Controls.Add(this.m_btnSure);
			this.Controls.Add(this.m_OverCutUnit);
			this.Controls.Add(this.m_tbxOverCutLength);
			this.Controls.Add(this.m_lblOverCutLength);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "OverCutForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "過切";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label m_lblOverCutLength;
		private System.Windows.Forms.TextBox m_tbxOverCutLength;
		private System.Windows.Forms.Label m_OverCutUnit;
		private System.Windows.Forms.Button m_btnSure;
	}
}