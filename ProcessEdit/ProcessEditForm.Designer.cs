namespace ProcessEdit
{
	partial class ProcessEditForm
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
			this.m_tsmiOK = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiMoveUp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiMoveDown = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAddTraverse = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiRemove = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiMoveUp,
            this.m_tsmiMoveDown,
            this.m_tsmiAddTraverse,
            this.m_tsmiEdit,
            this.m_tsmiRemove,
            this.m_tsmiOK});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(584, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
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
			// m_tsmiAddTraverse
			// 
			this.m_tsmiAddTraverse.Name = "m_tsmiAddTraverse";
			this.m_tsmiAddTraverse.Size = new System.Drawing.Size(79, 20);
			this.m_tsmiAddTraverse.Text = "新增避讓點";
			this.m_tsmiAddTraverse.Click += new System.EventHandler(this.m_tsmiAddTraverse_Click);
			// 
			// m_tsmiEdit
			// 
			this.m_tsmiEdit.Name = "m_tsmiEdit";
			this.m_tsmiEdit.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiEdit.Text = "編輯";
			this.m_tsmiEdit.Click += new System.EventHandler(this.m_tsmiEdit_Click);
			// 
			// m_tsmiRemove
			// 
			this.m_tsmiRemove.Name = "m_tsmiRemove";
			this.m_tsmiRemove.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiRemove.Text = "刪除";
			this.m_tsmiRemove.Click += new System.EventHandler(this.m_tsmiRemove_Click);
			// 
			// ProcessEditForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(584, 561);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "ProcessEditForm";
			this.Text = "工序編輯";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiOK;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiMoveUp;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiMoveDown;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAddTraverse;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiEdit;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiRemove;
	}
}

