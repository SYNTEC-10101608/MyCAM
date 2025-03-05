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
			this.m_tsmiPlane = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiPlane_R = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiPlanePar = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiPlanePar_R = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiOK = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip2 = new System.Windows.Forms.MenuStrip();
			this.m_tsmiAxial = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAxial_R = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAxialPar = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAxialPar_R = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.menuStrip2.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiPlane,
            this.m_tsmiPlane_R,
            this.m_tsmiPlanePar,
            this.m_tsmiPlanePar_R,
            this.m_tsmiOK});
			this.menuStrip1.Location = new System.Drawing.Point(0, 24);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(584, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// m_tsmiPlane
			// 
			this.m_tsmiPlane.Name = "m_tsmiPlane";
			this.m_tsmiPlane.Size = new System.Drawing.Size(55, 20);
			this.m_tsmiPlane.Text = "面重合";
			this.m_tsmiPlane.Click += new System.EventHandler(this.m_tsmiPlane_Click);
			// 
			// m_tsmiPlane_R
			// 
			this.m_tsmiPlane_R.Name = "m_tsmiPlane_R";
			this.m_tsmiPlane_R.Size = new System.Drawing.Size(74, 20);
			this.m_tsmiPlane_R.Text = "面重合 (R)";
			this.m_tsmiPlane_R.Click += new System.EventHandler(this.m_tsmiPlane_R_Click);
			// 
			// m_tsmiPlanePar
			// 
			this.m_tsmiPlanePar.Name = "m_tsmiPlanePar";
			this.m_tsmiPlanePar.Size = new System.Drawing.Size(55, 20);
			this.m_tsmiPlanePar.Text = "面平行";
			this.m_tsmiPlanePar.Click += new System.EventHandler(this.m_tsmiPlanePar_Click);
			// 
			// m_tsmiPlanePar_R
			// 
			this.m_tsmiPlanePar_R.Name = "m_tsmiPlanePar_R";
			this.m_tsmiPlanePar_R.Size = new System.Drawing.Size(74, 20);
			this.m_tsmiPlanePar_R.Text = "面平行 (R)";
			this.m_tsmiPlanePar_R.Click += new System.EventHandler(this.m_tsmiPlanePar_R_Click);
			// 
			// m_tsmiOK
			// 
			this.m_tsmiOK.Name = "m_tsmiOK";
			this.m_tsmiOK.Size = new System.Drawing.Size(36, 20);
			this.m_tsmiOK.Text = "OK";
			this.m_tsmiOK.Click += new System.EventHandler(this.m_tsmiOK_Click);
			// 
			// menuStrip2
			// 
			this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiAxial,
            this.m_tsmiAxial_R,
            this.m_tsmiAxialPar,
            this.m_tsmiAxialPar_R});
			this.menuStrip2.Location = new System.Drawing.Point(0, 0);
			this.menuStrip2.Name = "menuStrip2";
			this.menuStrip2.Size = new System.Drawing.Size(584, 24);
			this.menuStrip2.TabIndex = 1;
			this.menuStrip2.Text = "menuStrip2";
			// 
			// m_tsmiAxial
			// 
			this.m_tsmiAxial.Name = "m_tsmiAxial";
			this.m_tsmiAxial.Size = new System.Drawing.Size(55, 20);
			this.m_tsmiAxial.Text = "軸重合";
			this.m_tsmiAxial.Click += new System.EventHandler(this.m_tsmiAxial_Click);
			// 
			// m_tsmiAxial_R
			// 
			this.m_tsmiAxial_R.Name = "m_tsmiAxial_R";
			this.m_tsmiAxial_R.Size = new System.Drawing.Size(74, 20);
			this.m_tsmiAxial_R.Text = "軸重合 (R)";
			this.m_tsmiAxial_R.Click += new System.EventHandler(this.m_tsmiAxial_R_Click);
			// 
			// m_tsmiAxialPar
			// 
			this.m_tsmiAxialPar.Name = "m_tsmiAxialPar";
			this.m_tsmiAxialPar.Size = new System.Drawing.Size(55, 20);
			this.m_tsmiAxialPar.Text = "軸平行";
			this.m_tsmiAxialPar.Click += new System.EventHandler(this.m_tsmiAxialPar_Click);
			// 
			// m_tsmiAxialPar_R
			// 
			this.m_tsmiAxialPar_R.Name = "m_tsmiAxialPar_R";
			this.m_tsmiAxialPar_R.Size = new System.Drawing.Size(74, 20);
			this.m_tsmiAxialPar_R.Text = "軸平行 (R)";
			this.m_tsmiAxialPar_R.Click += new System.EventHandler(this.m_tsmiAxialPar_R_Click);
			// 
			// PartPlacementForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(584, 561);
			this.Controls.Add(this.menuStrip1);
			this.Controls.Add(this.menuStrip2);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "PartPlacementForm";
			this.Text = "放置零件";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.menuStrip2.ResumeLayout(false);
			this.menuStrip2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiOK;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiPlane;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiPlane_R;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiPlanePar;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiPlanePar_R;
		private System.Windows.Forms.MenuStrip menuStrip2;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAxial;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAxial_R;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAxialPar;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAxialPar_R;
	}
}

