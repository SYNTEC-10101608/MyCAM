namespace ImportModel
{
	partial class ImportModelForm
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
			this.m_tsmiImportFile = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiImportBRep = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiImportStep = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiImportIges = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiOK = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiImportFile,
            this.m_tsmiOK});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(584, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// m_tsmiImportFile
			// 
			this.m_tsmiImportFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiImportBRep,
            this.m_tsmiImportStep,
            this.m_tsmiImportIges});
			this.m_tsmiImportFile.Name = "m_tsmiImportFile";
			this.m_tsmiImportFile.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiImportFile.Text = "導入";
			// 
			// m_tsmiImportBRep
			// 
			this.m_tsmiImportBRep.Name = "m_tsmiImportBRep";
			this.m_tsmiImportBRep.Size = new System.Drawing.Size(104, 22);
			this.m_tsmiImportBRep.Text = "BRep";
			this.m_tsmiImportBRep.Click += new System.EventHandler(this.m_tsmiImportBRep_Click);
			// 
			// m_tsmiImportStep
			// 
			this.m_tsmiImportStep.Name = "m_tsmiImportStep";
			this.m_tsmiImportStep.Size = new System.Drawing.Size(104, 22);
			this.m_tsmiImportStep.Text = "Step";
			this.m_tsmiImportStep.Click += new System.EventHandler(this.m_tsmiImportStep_Click);
			// 
			// m_tsmiImportIges
			// 
			this.m_tsmiImportIges.Name = "m_tsmiImportIges";
			this.m_tsmiImportIges.Size = new System.Drawing.Size(104, 22);
			this.m_tsmiImportIges.Text = "Iges";
			this.m_tsmiImportIges.Click += new System.EventHandler(this.m_tsmiImportIges_Click);
			// 
			// m_tsmiOK
			// 
			this.m_tsmiOK.Name = "m_tsmiOK";
			this.m_tsmiOK.Size = new System.Drawing.Size(36, 20);
			this.m_tsmiOK.Text = "OK";
			this.m_tsmiOK.Click += new System.EventHandler(this.m_tsmiOK_Click);
			// 
			// ImportModelForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(584, 561);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "ImportModelForm";
			this.Text = "導入圖檔";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiImportFile;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiImportBRep;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiImportIges;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiImportStep;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiOK;
	}
}

