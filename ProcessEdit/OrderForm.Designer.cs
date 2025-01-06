namespace ProcessEdit
{
	partial class OrderForm
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
			this.m_msOrder = new System.Windows.Forms.MenuStrip();
			this.m_tsmiSiftUp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiSiftDown = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tvBrowser = new System.Windows.Forms.TreeView();
			this.m_tsmiAddTraverse = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAddTraverseBefore = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAddTraverseAfter = new System.Windows.Forms.ToolStripMenuItem();
			this.m_msOrder.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_msOrder
			// 
			this.m_msOrder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiSiftUp,
            this.m_tsmiSiftDown,
            this.m_tsmiAddTraverse});
			this.m_msOrder.Location = new System.Drawing.Point(0, 0);
			this.m_msOrder.Name = "m_msOrder";
			this.m_msOrder.Size = new System.Drawing.Size(284, 24);
			this.m_msOrder.TabIndex = 2;
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
			// m_tvBrowser
			// 
			this.m_tvBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tvBrowser.Location = new System.Drawing.Point(0, 24);
			this.m_tvBrowser.Name = "m_tvBrowser";
			this.m_tvBrowser.Size = new System.Drawing.Size(284, 237);
			this.m_tvBrowser.TabIndex = 3;
			// 
			// m_tsmiAddTraverse
			// 
			this.m_tsmiAddTraverse.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiAddTraverseBefore,
            this.m_tsmiAddTraverseAfter});
			this.m_tsmiAddTraverse.Name = "m_tsmiAddTraverse";
			this.m_tsmiAddTraverse.Size = new System.Drawing.Size(79, 20);
			this.m_tsmiAddTraverse.Text = "新增安全點";
			// 
			// m_tsmiAddTraverseBefore
			// 
			this.m_tsmiAddTraverseBefore.Name = "m_tsmiAddTraverseBefore";
			this.m_tsmiAddTraverseBefore.Size = new System.Drawing.Size(180, 22);
			this.m_tsmiAddTraverseBefore.Text = "向上";
			this.m_tsmiAddTraverseBefore.Click += new System.EventHandler(this.m_tsmiAddTraverseBefore_Click);
			// 
			// m_tsmiAddTraverseAfter
			// 
			this.m_tsmiAddTraverseAfter.Name = "m_tsmiAddTraverseAfter";
			this.m_tsmiAddTraverseAfter.Size = new System.Drawing.Size(180, 22);
			this.m_tsmiAddTraverseAfter.Text = "向下";
			this.m_tsmiAddTraverseAfter.Click += new System.EventHandler(this.m_tsmiAddTraverseAfter_Click);
			// 
			// OrderForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Controls.Add(this.m_tvBrowser);
			this.Controls.Add(this.m_msOrder);
			this.Name = "OrderForm";
			this.Text = "排序";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OrderForm_FormClosing);
			this.m_msOrder.ResumeLayout(false);
			this.m_msOrder.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip m_msOrder;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiSiftUp;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiSiftDown;
		private System.Windows.Forms.TreeView m_tvBrowser;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAddTraverse;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAddTraverseBefore;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAddTraverseAfter;
	}
}