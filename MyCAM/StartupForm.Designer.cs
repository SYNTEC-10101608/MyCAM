namespace MyCAM
{
	partial class StartupForm
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
			this.m_tsmiImport = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_tsmiImportSTEP = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiImportIGES = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiImportBRep = new System.Windows.Forms.ToolStripMenuItem();
			this.m_panView = new System.Windows.Forms.Panel();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiImport});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(584, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// m_tsmiImport
			// 
			this.m_tsmiImport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.m_tsmiImportSTEP,
            this.m_tsmiImportIGES,
            this.m_tsmiImportBRep});
			this.m_tsmiImport.Name = "m_tsmiImport";
			this.m_tsmiImport.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiImport.Text = "導入";
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(101, 6);
			// 
			// m_tsmiImportSTEP
			// 
			this.m_tsmiImportSTEP.Name = "m_tsmiImportSTEP";
			this.m_tsmiImportSTEP.Size = new System.Drawing.Size(104, 22);
			this.m_tsmiImportSTEP.Text = "STEP";
			this.m_tsmiImportSTEP.Click += new System.EventHandler(this.m_tsmiImportSTEP_Click);
			// 
			// m_tsmiImportIGES
			// 
			this.m_tsmiImportIGES.Name = "m_tsmiImportIGES";
			this.m_tsmiImportIGES.Size = new System.Drawing.Size(104, 22);
			this.m_tsmiImportIGES.Text = "IGES";
			this.m_tsmiImportIGES.Click += new System.EventHandler(this.m_tsmiImportIGES_Click);
			// 
			// m_tsmiImportBRep
			// 
			this.m_tsmiImportBRep.Name = "m_tsmiImportBRep";
			this.m_tsmiImportBRep.Size = new System.Drawing.Size(104, 22);
			this.m_tsmiImportBRep.Text = "BRep";
			this.m_tsmiImportBRep.Click += new System.EventHandler(this.m_tsmiImportBRep_Click);
			// 
			// m_panView
			// 
			this.m_panView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panView.Location = new System.Drawing.Point(0, 24);
			this.m_panView.Name = "m_panView";
			this.m_panView.Size = new System.Drawing.Size(584, 537);
			this.m_panView.TabIndex = 1;
			this.m_panView.SizeChanged += new System.EventHandler(this.m_panView_SizeChanged);
			this.m_panView.Paint += new System.Windows.Forms.PaintEventHandler(this.m_panView_Paint);
			// 
			// StartupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(584, 561);
			this.Controls.Add(this.m_panView);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "StartupForm";
			this.Text = "五軸切";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiImport;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiImportSTEP;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiImportIGES;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiImportBRep;
		private System.Windows.Forms.Panel m_panView;
	}
}

