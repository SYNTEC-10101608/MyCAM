namespace PartPlacement
{
	partial class PartPlacementForm
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
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.m_tsmiSetPart = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiOK = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiSetPart,
            this.m_tsmiOK});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(584, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// m_tsmiSetPart
			// 
			this.m_tsmiSetPart.Name = "m_tsmiSetPart";
			this.m_tsmiSetPart.Size = new System.Drawing.Size(67, 20);
			this.m_tsmiSetPart.Text = "放置零件";
			this.m_tsmiSetPart.Click += new System.EventHandler(this.m_tsmiSetPart_Click);
			// 
			// m_tsmiOK
			// 
			this.m_tsmiOK.Name = "m_tsmiOK";
			this.m_tsmiOK.Size = new System.Drawing.Size(36, 20);
			this.m_tsmiOK.Text = "OK";
			this.m_tsmiOK.Click += new System.EventHandler(this.m_tsmiOK_Click);
			// 
			// PartPlacementForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(584, 561);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "PartPlacementForm";
			this.Text = "放置零件";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiSetPart;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiOK;
	}
}

