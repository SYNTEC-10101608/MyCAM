namespace CAMEdit
{
	partial class CAMEditForm
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
			this.m_tsmiStartPoint = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiReverse = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiToolVec = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiTV_Default = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiTV_Intersecting = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiTV_Z = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiOffset = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiLead = new System.Windows.Forms.ToolStripMenuItem();
			this.m_btnOK = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiStartPoint,
            this.m_tsmiReverse,
            this.m_tsmiToolVec,
            this.m_tsmiOffset,
            this.m_tsmiLead,
            this.m_btnOK});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(584, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// m_tsmiStartPoint
			// 
			this.m_tsmiStartPoint.Name = "m_tsmiStartPoint";
			this.m_tsmiStartPoint.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiStartPoint.Text = "起點";
			this.m_tsmiStartPoint.Click += new System.EventHandler(this.m_tsmiStartPoint_Click);
			// 
			// m_tsmiReverse
			// 
			this.m_tsmiReverse.Name = "m_tsmiReverse";
			this.m_tsmiReverse.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiReverse.Text = "反向";
			this.m_tsmiReverse.Click += new System.EventHandler(this.m_tsmiReverse_Click);
			// 
			// m_tsmiToolVec
			// 
			this.m_tsmiToolVec.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiTV_Default,
            this.m_tsmiTV_Intersecting,
            this.m_tsmiTV_Z});
			this.m_tsmiToolVec.Name = "m_tsmiToolVec";
			this.m_tsmiToolVec.Size = new System.Drawing.Size(67, 20);
			this.m_tsmiToolVec.Text = "刀具向量";
			// 
			// m_tsmiTV_Default
			// 
			this.m_tsmiTV_Default.Name = "m_tsmiTV_Default";
			this.m_tsmiTV_Default.Size = new System.Drawing.Size(180, 22);
			this.m_tsmiTV_Default.Text = "默認";
			this.m_tsmiTV_Default.Click += new System.EventHandler(this.m_tsmiTV_Default_Click);
			// 
			// m_tsmiTV_Intersecting
			// 
			this.m_tsmiTV_Intersecting.Name = "m_tsmiTV_Intersecting";
			this.m_tsmiTV_Intersecting.Size = new System.Drawing.Size(180, 22);
			this.m_tsmiTV_Intersecting.Text = "相貫線 (beta)";
			this.m_tsmiTV_Intersecting.Click += new System.EventHandler(this.m_tsmiTV_Intersecting_Click);
			// 
			// m_tsmiTV_Z
			// 
			this.m_tsmiTV_Z.Name = "m_tsmiTV_Z";
			this.m_tsmiTV_Z.Size = new System.Drawing.Size(180, 22);
			this.m_tsmiTV_Z.Text = "Z方向";
			this.m_tsmiTV_Z.Click += new System.EventHandler(this.m_tsmiTV_Z_Click);
			// 
			// m_tsmiOffset
			// 
			this.m_tsmiOffset.Name = "m_tsmiOffset";
			this.m_tsmiOffset.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiOffset.Text = "補償";
			this.m_tsmiOffset.Click += new System.EventHandler(this.m_tsmiOffset_Click);
			// 
			// m_tsmiLead
			// 
			this.m_tsmiLead.Name = "m_tsmiLead";
			this.m_tsmiLead.Size = new System.Drawing.Size(55, 20);
			this.m_tsmiLead.Text = "引刀線";
			this.m_tsmiLead.Click += new System.EventHandler(this.m_tsmiLead_Click);
			// 
			// m_btnOK
			// 
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(36, 20);
			this.m_btnOK.Text = "OK";
			this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
			// 
			// CAMEditForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(584, 561);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "CAMEditForm";
			this.Text = "CAM編修";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem m_btnOK;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiStartPoint;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiReverse;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiToolVec;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiTV_Default;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiTV_Intersecting;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiTV_Z;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiOffset;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiLead;
	}
}