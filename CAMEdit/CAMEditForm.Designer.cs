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
			this.m_tsmiOffset = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiLead = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiToolVec = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiOK = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiMoveUp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiMoveDown = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiStartPoint,
            this.m_tsmiReverse,
            this.m_tsmiOffset,
            this.m_tsmiToolVec,
            this.m_tsmiLead,
            this.m_tsmiMoveUp,
            this.m_tsmiMoveDown,
            this.m_tsmiOK});
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
			this.m_tsmiLead.Visible = false;
			this.m_tsmiLead.Click += new System.EventHandler(this.m_tsmiLead_Click);
			// 
			// m_tsmiToolVec
			// 
			this.m_tsmiToolVec.Name = "m_tsmiToolVec";
			this.m_tsmiToolVec.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiToolVec.Text = "姿態";
			this.m_tsmiToolVec.Click += new System.EventHandler(this.m_tsmiToolVec_Click);
			// 
			// m_tsmiOK
			// 
			this.m_tsmiOK.Name = "m_tsmiOK";
			this.m_tsmiOK.Size = new System.Drawing.Size(36, 20);
			this.m_tsmiOK.Text = "OK";
			this.m_tsmiOK.Click += new System.EventHandler(this.m_tsmiOK_Click);
			// 
			// m_tsmiMoveUp
			// 
			this.m_tsmiMoveUp.Name = "m_tsmiMoveUp";
			this.m_tsmiMoveUp.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiMoveUp.Text = "上移";
			this.m_tsmiMoveUp.Click += new System.EventHandler(this.m_tsmiMoveUp_Click);
			// 
			// m_tsmiMoveDown
			// 
			this.m_tsmiMoveDown.Name = "m_tsmiMoveDown";
			this.m_tsmiMoveDown.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiMoveDown.Text = "下移";
			this.m_tsmiMoveDown.Click += new System.EventHandler(this.m_tsmiMoveDown_Click);
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
		private System.Windows.Forms.ToolStripMenuItem m_tsmiOK;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiStartPoint;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiReverse;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiOffset;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiLead;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiToolVec;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiMoveUp;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiMoveDown;
	}
}