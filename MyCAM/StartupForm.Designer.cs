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
			this.m_msCAD = new System.Windows.Forms.MenuStrip();
			this.m_tsmiImport = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiImportBRep = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiImportStep = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiImportIges = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAddFeature = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAddPoint = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAddPoint_AcrCenter = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAddPoint_EdgeCenter = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmi_Coordinate = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiCreate3PCS = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiModifyCS = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiTransform = new System.Windows.Forms.ToolStripMenuItem();
			this.m_panViewer = new System.Windows.Forms.Panel();
			this.m_panTreeView = new System.Windows.Forms.Panel();
			this.m_TreeView = new System.Windows.Forms.TreeView();
			this.m_panBackGround = new System.Windows.Forms.Panel();
			this.m_msCAD_Transform = new System.Windows.Forms.MenuStrip();
			this.m_tsmiPlane = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiPlane_R = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiPlanePar = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiPlanePar_R = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAxial = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAxial_R = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAxialPar = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAxialPar_R = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiOK = new System.Windows.Forms.ToolStripMenuItem();
			this.m_msCAD.SuspendLayout();
			this.m_panTreeView.SuspendLayout();
			this.m_panBackGround.SuspendLayout();
			this.m_msCAD_Transform.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_msCAD
			// 
			this.m_msCAD.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiImport,
            this.m_tsmiAddFeature,
            this.m_tsmi_Coordinate,
            this.m_tsmiTransform});
			this.m_msCAD.Location = new System.Drawing.Point(0, 0);
			this.m_msCAD.Name = "m_msCAD";
			this.m_msCAD.Size = new System.Drawing.Size(1184, 24);
			this.m_msCAD.TabIndex = 1;
			this.m_msCAD.Text = "menuStrip1";
			// 
			// m_tsmiImport
			// 
			this.m_tsmiImport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiImportBRep,
            this.m_tsmiImportStep,
            this.m_tsmiImportIges});
			this.m_tsmiImport.Name = "m_tsmiImport";
			this.m_tsmiImport.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiImport.Text = "導入";
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
			// m_tsmiAddFeature
			// 
			this.m_tsmiAddFeature.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiAddPoint});
			this.m_tsmiAddFeature.Name = "m_tsmiAddFeature";
			this.m_tsmiAddFeature.Size = new System.Drawing.Size(67, 20);
			this.m_tsmiAddFeature.Text = "建立特徵";
			this.m_tsmiAddFeature.Visible = false;
			// 
			// m_tsmiAddPoint
			// 
			this.m_tsmiAddPoint.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiAddPoint_AcrCenter,
            this.m_tsmiAddPoint_EdgeCenter});
			this.m_tsmiAddPoint.Name = "m_tsmiAddPoint";
			this.m_tsmiAddPoint.Size = new System.Drawing.Size(110, 22);
			this.m_tsmiAddPoint.Text = "參考點";
			// 
			// m_tsmiAddPoint_AcrCenter
			// 
			this.m_tsmiAddPoint_AcrCenter.Name = "m_tsmiAddPoint_AcrCenter";
			this.m_tsmiAddPoint_AcrCenter.Size = new System.Drawing.Size(122, 22);
			this.m_tsmiAddPoint_AcrCenter.Text = "圓弧中心";
			this.m_tsmiAddPoint_AcrCenter.Click += new System.EventHandler(this.m_tsmiAddPoint_AcrCenter_Click);
			// 
			// m_tsmiAddPoint_EdgeCenter
			// 
			this.m_tsmiAddPoint_EdgeCenter.Name = "m_tsmiAddPoint_EdgeCenter";
			this.m_tsmiAddPoint_EdgeCenter.Size = new System.Drawing.Size(122, 22);
			this.m_tsmiAddPoint_EdgeCenter.Text = "線段中心";
			this.m_tsmiAddPoint_EdgeCenter.Click += new System.EventHandler(this.m_tsmiAddPoint_EdgeCenter_Click);
			// 
			// m_tsmi_Coordinate
			// 
			this.m_tsmi_Coordinate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiCreate3PCS,
            this.m_tsmiModifyCS});
			this.m_tsmi_Coordinate.Name = "m_tsmi_Coordinate";
			this.m_tsmi_Coordinate.Size = new System.Drawing.Size(55, 20);
			this.m_tsmi_Coordinate.Text = "座標系";
			this.m_tsmi_Coordinate.Visible = false;
			// 
			// m_tsmiCreate3PCS
			// 
			this.m_tsmiCreate3PCS.Name = "m_tsmiCreate3PCS";
			this.m_tsmiCreate3PCS.Size = new System.Drawing.Size(158, 22);
			this.m_tsmiCreate3PCS.Text = "建立三點座標系";
			this.m_tsmiCreate3PCS.Click += new System.EventHandler(this.m_tsmiCreate3PCS_Click);
			// 
			// m_tsmiModifyCS
			// 
			this.m_tsmiModifyCS.Name = "m_tsmiModifyCS";
			this.m_tsmiModifyCS.Size = new System.Drawing.Size(158, 22);
			this.m_tsmiModifyCS.Text = "修改座標系";
			this.m_tsmiModifyCS.Click += new System.EventHandler(this.m_tsmiModifyCS_Click);
			// 
			// m_tsmiTransform
			// 
			this.m_tsmiTransform.Name = "m_tsmiTransform";
			this.m_tsmiTransform.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiTransform.Text = "轉換";
			this.m_tsmiTransform.Click += new System.EventHandler(this.m_tsmiTransform_Click);
			// 
			// m_panViewer
			// 
			this.m_panViewer.Dock = System.Windows.Forms.DockStyle.Right;
			this.m_panViewer.Location = new System.Drawing.Point(284, 0);
			this.m_panViewer.Name = "m_panViewer";
			this.m_panViewer.Size = new System.Drawing.Size(900, 737);
			this.m_panViewer.TabIndex = 1;
			// 
			// m_panTreeView
			// 
			this.m_panTreeView.Controls.Add(this.m_TreeView);
			this.m_panTreeView.Dock = System.Windows.Forms.DockStyle.Left;
			this.m_panTreeView.Location = new System.Drawing.Point(0, 0);
			this.m_panTreeView.Name = "m_panTreeView";
			this.m_panTreeView.Size = new System.Drawing.Size(300, 737);
			this.m_panTreeView.TabIndex = 0;
			// 
			// m_TreeView
			// 
			this.m_TreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_TreeView.Location = new System.Drawing.Point(0, 0);
			this.m_TreeView.Name = "m_TreeView";
			this.m_TreeView.Size = new System.Drawing.Size(300, 737);
			this.m_TreeView.TabIndex = 0;
			// 
			// m_panBackGround
			// 
			this.m_panBackGround.Controls.Add(this.m_panTreeView);
			this.m_panBackGround.Controls.Add(this.m_panViewer);
			this.m_panBackGround.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panBackGround.Location = new System.Drawing.Point(0, 24);
			this.m_panBackGround.Name = "m_panBackGround";
			this.m_panBackGround.Size = new System.Drawing.Size(1184, 737);
			this.m_panBackGround.TabIndex = 0;
			// 
			// m_msCAD_Transform
			// 
			this.m_msCAD_Transform.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiPlane,
            this.m_tsmiPlane_R,
            this.m_tsmiPlanePar,
            this.m_tsmiPlanePar_R,
            this.m_tsmiAxial,
            this.m_tsmiAxial_R,
            this.m_tsmiAxialPar,
            this.m_tsmiAxialPar_R,
            this.m_tsmiOK});
			this.m_msCAD_Transform.Location = new System.Drawing.Point(0, 24);
			this.m_msCAD_Transform.Name = "m_msCAD_Transform";
			this.m_msCAD_Transform.Size = new System.Drawing.Size(1184, 24);
			this.m_msCAD_Transform.TabIndex = 2;
			this.m_msCAD_Transform.Text = "menuStrip2";
			this.m_msCAD_Transform.Visible = false;
			// 
			// m_tsmiPlane
			// 
			this.m_tsmiPlane.Name = "m_tsmiPlane";
			this.m_tsmiPlane.Size = new System.Drawing.Size(55, 20);
			this.m_tsmiPlane.Text = "面重合";
			// 
			// m_tsmiPlane_R
			// 
			this.m_tsmiPlane_R.Name = "m_tsmiPlane_R";
			this.m_tsmiPlane_R.Size = new System.Drawing.Size(74, 20);
			this.m_tsmiPlane_R.Text = "面重合 (R)";
			// 
			// m_tsmiPlanePar
			// 
			this.m_tsmiPlanePar.Name = "m_tsmiPlanePar";
			this.m_tsmiPlanePar.Size = new System.Drawing.Size(55, 20);
			this.m_tsmiPlanePar.Text = "面平行";
			// 
			// m_tsmiPlanePar_R
			// 
			this.m_tsmiPlanePar_R.Name = "m_tsmiPlanePar_R";
			this.m_tsmiPlanePar_R.Size = new System.Drawing.Size(74, 20);
			this.m_tsmiPlanePar_R.Text = "面平行 (R)";
			// 
			// m_tsmiAxial
			// 
			this.m_tsmiAxial.Name = "m_tsmiAxial";
			this.m_tsmiAxial.Size = new System.Drawing.Size(55, 20);
			this.m_tsmiAxial.Text = "軸重合";
			// 
			// m_tsmiAxial_R
			// 
			this.m_tsmiAxial_R.Name = "m_tsmiAxial_R";
			this.m_tsmiAxial_R.Size = new System.Drawing.Size(74, 20);
			this.m_tsmiAxial_R.Text = "軸重合 (R)";
			// 
			// m_tsmiAxialPar
			// 
			this.m_tsmiAxialPar.Name = "m_tsmiAxialPar";
			this.m_tsmiAxialPar.Size = new System.Drawing.Size(55, 20);
			this.m_tsmiAxialPar.Text = "軸平行";
			// 
			// m_tsmiAxialPar_R
			// 
			this.m_tsmiAxialPar_R.Name = "m_tsmiAxialPar_R";
			this.m_tsmiAxialPar_R.Size = new System.Drawing.Size(74, 20);
			this.m_tsmiAxialPar_R.Text = "軸平行 (R)";
			// 
			// m_tsmiOK
			// 
			this.m_tsmiOK.Name = "m_tsmiOK";
			this.m_tsmiOK.Size = new System.Drawing.Size(36, 20);
			this.m_tsmiOK.Text = "OK";
			// 
			// StartupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(1184, 761);
			this.Controls.Add(this.m_panBackGround);
			this.Controls.Add(this.m_msCAD_Transform);
			this.Controls.Add(this.m_msCAD);
			this.MainMenuStrip = this.m_msCAD;
			this.Name = "StartupForm";
			this.Text = "五軸切";
			this.m_msCAD.ResumeLayout(false);
			this.m_msCAD.PerformLayout();
			this.m_panTreeView.ResumeLayout(false);
			this.m_panBackGround.ResumeLayout(false);
			this.m_msCAD_Transform.ResumeLayout(false);
			this.m_msCAD_Transform.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.MenuStrip m_msCAD;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiImport;
		private System.Windows.Forms.Panel m_panViewer;
		private System.Windows.Forms.Panel m_panTreeView;
		private System.Windows.Forms.Panel m_panBackGround;
		private System.Windows.Forms.TreeView m_TreeView;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiImportBRep;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiImportStep;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiImportIges;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAddFeature;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAddPoint;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAddPoint_AcrCenter;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAddPoint_EdgeCenter;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiTransform;
		private System.Windows.Forms.ToolStripMenuItem m_tsmi_Coordinate;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiCreate3PCS;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiModifyCS;
		private System.Windows.Forms.MenuStrip m_msCAD_Transform;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiPlane;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiPlane_R;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiPlanePar;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiPlanePar_R;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAxial;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAxial_R;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAxialPar;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAxialPar_R;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiOK;
	}
}

