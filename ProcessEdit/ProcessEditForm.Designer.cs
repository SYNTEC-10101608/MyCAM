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
			this.m_tsmiOrder = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiTraverse = new System.Windows.Forms.ToolStripMenuItem();
			this.m_msOrder = new System.Windows.Forms.MenuStrip();
			this.m_tsmiSiftUp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiSiftDown = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.m_msOrder.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiOK,
            this.m_tsmiOrder,
            this.m_tsmiTraverse});
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
			// m_tsmiOrder
			// 
			this.m_tsmiOrder.Name = "m_tsmiOrder";
			this.m_tsmiOrder.Size = new System.Drawing.Size(67, 20);
			this.m_tsmiOrder.Text = "工序調整";
			this.m_tsmiOrder.Click += new System.EventHandler(this.m_tsmiOrder_Click);
			// 
			// m_tsmiTraverse
			// 
			this.m_tsmiTraverse.Name = "m_tsmiTraverse";
			this.m_tsmiTraverse.Size = new System.Drawing.Size(79, 20);
			this.m_tsmiTraverse.Text = "新增安全點";
			this.m_tsmiTraverse.Click += new System.EventHandler(this.m_tsmiTraverse_Click);
			// 
			// m_msOrder
			// 
			this.m_msOrder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiSiftUp,
            this.m_tsmiSiftDown});
			this.m_msOrder.Location = new System.Drawing.Point(0, 24);
			this.m_msOrder.Name = "m_msOrder";
			this.m_msOrder.Size = new System.Drawing.Size(584, 24);
			this.m_msOrder.TabIndex = 1;
			this.m_msOrder.Text = "menuStrip2";
			// 
			// m_tsmiSiftUp
			// 
			this.m_tsmiSiftUp.Name = "m_tsmiSiftUp";
			this.m_tsmiSiftUp.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiSiftUp.Text = "上移";
			this.m_tsmiSiftUp.Click += new System.EventHandler(this.m_tsmiSiftUp_Click);
			// 
			// m_tsmiSiftDown
			// 
			this.m_tsmiSiftDown.Name = "m_tsmiSiftDown";
			this.m_tsmiSiftDown.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiSiftDown.Text = "下移";
			this.m_tsmiSiftDown.Click += new System.EventHandler(this.m_tsmiSiftDown_Click);
			// 
			// ProcessEditForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(584, 561);
			this.Controls.Add(this.m_msOrder);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "ProcessEditForm";
			this.Text = "工序編輯";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.m_msOrder.ResumeLayout(false);
			this.m_msOrder.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiOK;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiOrder;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiTraverse;
		private System.Windows.Forms.MenuStrip m_msOrder;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiSiftUp;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiSiftDown;
	}
}

