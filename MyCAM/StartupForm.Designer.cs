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
			this.m_tsmiTransform = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiManualTransform = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmi3PointTransform = new System.Windows.Forms.ToolStripMenuItem();
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
			this.m_tsmiTransformOK = new System.Windows.Forms.ToolStripMenuItem();
			this.m_msCAD_SelectFace = new System.Windows.Forms.MenuStrip();
			this.m_tsmiSelectD1ContFace = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiSelectPath = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiSelPath_FreeBound = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiSelPath_Manual = new System.Windows.Forms.ToolStripMenuItem();
			this.m_msCAD_ManualSelectPath = new System.Windows.Forms.MenuStrip();
			this.m_tsmiManualSelectPathOK = new System.Windows.Forms.ToolStripMenuItem();
			this.m_msMain = new System.Windows.Forms.MenuStrip();
			this.m_tsmiFile = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiCAM = new System.Windows.Forms.ToolStripMenuItem();
			this.m_msCAM = new System.Windows.Forms.MenuStrip();
			this.m_tsmiAddPath = new System.Windows.Forms.ToolStripMenuItem();
			this.m_msCAD.SuspendLayout();
			this.m_panTreeView.SuspendLayout();
			this.m_panBackGround.SuspendLayout();
			this.m_msCAD_Transform.SuspendLayout();
			this.m_msCAD_SelectFace.SuspendLayout();
			this.m_msCAD_ManualSelectPath.SuspendLayout();
			this.m_msMain.SuspendLayout();
			this.m_msCAM.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_msCAD
			// 
			this.m_msCAD.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiImport,
            this.m_tsmiAddFeature,
            this.m_tsmiTransform});
			this.m_msCAD.Location = new System.Drawing.Point(0, 24);
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
			// m_tsmiTransform
			// 
			this.m_tsmiTransform.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiManualTransform,
            this.m_tsmi3PointTransform});
			this.m_tsmiTransform.Name = "m_tsmiTransform";
			this.m_tsmiTransform.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiTransform.Text = "轉換";
			// 
			// m_tsmiManualTransform
			// 
			this.m_tsmiManualTransform.Name = "m_tsmiManualTransform";
			this.m_tsmiManualTransform.Size = new System.Drawing.Size(122, 22);
			this.m_tsmiManualTransform.Text = "手動轉換";
			this.m_tsmiManualTransform.Click += new System.EventHandler(this.m_tsmiManualTransform_Click);
			// 
			// m_tsmi3PointTransform
			// 
			this.m_tsmi3PointTransform.Name = "m_tsmi3PointTransform";
			this.m_tsmi3PointTransform.Size = new System.Drawing.Size(122, 22);
			this.m_tsmi3PointTransform.Text = "三點轉換";
			this.m_tsmi3PointTransform.Click += new System.EventHandler(this.m_tsmi3PointTransform_Click);
			// 
			// m_panViewer
			// 
			this.m_panViewer.Dock = System.Windows.Forms.DockStyle.Right;
			this.m_panViewer.Location = new System.Drawing.Point(284, 0);
			this.m_panViewer.Name = "m_panViewer";
			this.m_panViewer.Size = new System.Drawing.Size(900, 689);
			this.m_panViewer.TabIndex = 1;
			// 
			// m_panTreeView
			// 
			this.m_panTreeView.Controls.Add(this.m_TreeView);
			this.m_panTreeView.Dock = System.Windows.Forms.DockStyle.Left;
			this.m_panTreeView.Location = new System.Drawing.Point(0, 0);
			this.m_panTreeView.Name = "m_panTreeView";
			this.m_panTreeView.Size = new System.Drawing.Size(300, 689);
			this.m_panTreeView.TabIndex = 1;
			// 
			// m_TreeView
			// 
			this.m_TreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_TreeView.Location = new System.Drawing.Point(0, 0);
			this.m_TreeView.Name = "m_TreeView";
			this.m_TreeView.Size = new System.Drawing.Size(300, 689);
			this.m_TreeView.TabIndex = 1;
			// 
			// m_panBackGround
			// 
			this.m_panBackGround.Controls.Add(this.m_panTreeView);
			this.m_panBackGround.Controls.Add(this.m_panViewer);
			this.m_panBackGround.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panBackGround.Location = new System.Drawing.Point(0, 72);
			this.m_panBackGround.Name = "m_panBackGround";
			this.m_panBackGround.Size = new System.Drawing.Size(1184, 689);
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
            this.m_tsmiTransformOK});
			this.m_msCAD_Transform.Location = new System.Drawing.Point(0, 72);
			this.m_msCAD_Transform.Name = "m_msCAD_Transform";
			this.m_msCAD_Transform.Size = new System.Drawing.Size(1184, 24);
			this.m_msCAD_Transform.TabIndex = 1;
			this.m_msCAD_Transform.Text = "menuStrip2";
			this.m_msCAD_Transform.Visible = false;
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
			// m_tsmiTransformOK
			// 
			this.m_tsmiTransformOK.Name = "m_tsmiTransformOK";
			this.m_tsmiTransformOK.Size = new System.Drawing.Size(36, 20);
			this.m_tsmiTransformOK.Text = "OK";
			this.m_tsmiTransformOK.Click += new System.EventHandler(this.m_tsmiTransformOK_Click);
			// 
			// m_msCAD_SelectFace
			// 
			this.m_msCAD_SelectFace.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiSelectD1ContFace,
            this.m_tsmiSelectPath});
			this.m_msCAD_SelectFace.Location = new System.Drawing.Point(0, 72);
			this.m_msCAD_SelectFace.Name = "m_msCAD_SelectFace";
			this.m_msCAD_SelectFace.Size = new System.Drawing.Size(1184, 24);
			this.m_msCAD_SelectFace.TabIndex = 0;
			this.m_msCAD_SelectFace.Text = "menuStrip1";
			this.m_msCAD_SelectFace.Visible = false;
			// 
			// m_tsmiSelectD1ContFace
			// 
			this.m_tsmiSelectD1ContFace.Name = "m_tsmiSelectD1ContFace";
			this.m_tsmiSelectD1ContFace.Size = new System.Drawing.Size(62, 20);
			this.m_tsmiSelectD1ContFace.Text = "D1 連續";
			this.m_tsmiSelectD1ContFace.Click += new System.EventHandler(this.m_tsmiSelectD1ContFace_Click);
			// 
			// m_tsmiSelectPath
			// 
			this.m_tsmiSelectPath.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiSelPath_FreeBound,
            this.m_tsmiSelPath_Manual});
			this.m_tsmiSelectPath.Name = "m_tsmiSelectPath";
			this.m_tsmiSelectPath.Size = new System.Drawing.Size(67, 20);
			this.m_tsmiSelectPath.Text = "選取路徑";
			// 
			// m_tsmiSelPath_FreeBound
			// 
			this.m_tsmiSelPath_FreeBound.Name = "m_tsmiSelPath_FreeBound";
			this.m_tsmiSelPath_FreeBound.Size = new System.Drawing.Size(134, 22);
			this.m_tsmiSelPath_FreeBound.Text = "所有面邊界";
			this.m_tsmiSelPath_FreeBound.Click += new System.EventHandler(this.m_tsmiSelPath_FreeBound_Click);
			// 
			// m_tsmiSelPath_Manual
			// 
			this.m_tsmiSelPath_Manual.Name = "m_tsmiSelPath_Manual";
			this.m_tsmiSelPath_Manual.Size = new System.Drawing.Size(134, 22);
			this.m_tsmiSelPath_Manual.Text = "手動選取";
			this.m_tsmiSelPath_Manual.Click += new System.EventHandler(this.m_tsmiSelPath_Manual_Click);
			// 
			// m_msCAD_ManualSelectPath
			// 
			this.m_msCAD_ManualSelectPath.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiManualSelectPathOK});
			this.m_msCAD_ManualSelectPath.Location = new System.Drawing.Point(0, 72);
			this.m_msCAD_ManualSelectPath.Name = "m_msCAD_ManualSelectPath";
			this.m_msCAD_ManualSelectPath.Size = new System.Drawing.Size(1184, 24);
			this.m_msCAD_ManualSelectPath.TabIndex = 1;
			this.m_msCAD_ManualSelectPath.Text = "menuStrip1";
			this.m_msCAD_ManualSelectPath.Visible = false;
			// 
			// m_tsmiManualSelectPathOK
			// 
			this.m_tsmiManualSelectPathOK.Name = "m_tsmiManualSelectPathOK";
			this.m_tsmiManualSelectPathOK.Size = new System.Drawing.Size(36, 20);
			this.m_tsmiManualSelectPathOK.Text = "OK";
			this.m_tsmiManualSelectPathOK.Click += new System.EventHandler(this.m_tsmiManualSelectPathOK_Click);
			// 
			// m_msMain
			// 
			this.m_msMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiFile,
            this.m_tsmiCAM});
			this.m_msMain.Location = new System.Drawing.Point(0, 0);
			this.m_msMain.Name = "m_msMain";
			this.m_msMain.Size = new System.Drawing.Size(1184, 24);
			this.m_msMain.TabIndex = 1;
			this.m_msMain.Text = "menuStrip1";
			// 
			// m_tsmiFile
			// 
			this.m_tsmiFile.Name = "m_tsmiFile";
			this.m_tsmiFile.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiFile.Text = "檔案";
			// 
			// m_tsmiCAM
			// 
			this.m_tsmiCAM.Name = "m_tsmiCAM";
			this.m_tsmiCAM.Size = new System.Drawing.Size(47, 20);
			this.m_tsmiCAM.Text = "CAM";
			this.m_tsmiCAM.Click += new System.EventHandler(this.m_tsmiCAM_Click);
			// 
			// m_msCAM
			// 
			this.m_msCAM.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiAddPath});
			this.m_msCAM.Location = new System.Drawing.Point(0, 48);
			this.m_msCAM.Name = "m_msCAM";
			this.m_msCAM.Size = new System.Drawing.Size(1184, 24);
			this.m_msCAM.TabIndex = 1;
			this.m_msCAM.Text = "menuStrip1";
			// 
			// m_tsmiAddPath
			// 
			this.m_tsmiAddPath.Name = "m_tsmiAddPath";
			this.m_tsmiAddPath.Size = new System.Drawing.Size(67, 20);
			this.m_tsmiAddPath.Text = "新增路徑";
			this.m_tsmiAddPath.Click += new System.EventHandler(this.m_tsmiAddPath_Click);
			// 
			// StartupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(1184, 761);
			this.Controls.Add(this.m_panBackGround);
			this.Controls.Add(this.m_msCAD_SelectFace);
			this.Controls.Add(this.m_msCAD_ManualSelectPath);
			this.Controls.Add(this.m_msCAD_Transform);
			this.Controls.Add(this.m_msCAM);
			this.Controls.Add(this.m_msCAD);
			this.Controls.Add(this.m_msMain);
			this.MainMenuStrip = this.m_msCAD;
			this.Name = "StartupForm";
			this.Text = "五軸切";
			this.m_msCAD.ResumeLayout(false);
			this.m_msCAD.PerformLayout();
			this.m_panTreeView.ResumeLayout(false);
			this.m_panBackGround.ResumeLayout(false);
			this.m_msCAD_Transform.ResumeLayout(false);
			this.m_msCAD_Transform.PerformLayout();
			this.m_msCAD_SelectFace.ResumeLayout(false);
			this.m_msCAD_SelectFace.PerformLayout();
			this.m_msCAD_ManualSelectPath.ResumeLayout(false);
			this.m_msCAD_ManualSelectPath.PerformLayout();
			this.m_msMain.ResumeLayout(false);
			this.m_msMain.PerformLayout();
			this.m_msCAM.ResumeLayout(false);
			this.m_msCAM.PerformLayout();
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
		private System.Windows.Forms.MenuStrip m_msCAD_Transform;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiPlane;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiPlane_R;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiPlanePar;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiPlanePar_R;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAxial;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAxial_R;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAxialPar;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAxialPar_R;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiTransformOK;
		private System.Windows.Forms.MenuStrip m_msCAD_SelectFace;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiSelectD1ContFace;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiManualTransform;
		private System.Windows.Forms.ToolStripMenuItem m_tsmi3PointTransform;
		private System.Windows.Forms.MenuStrip m_msCAD_ManualSelectPath;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiManualSelectPathOK;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiSelectPath;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiSelPath_FreeBound;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiSelPath_Manual;
		private System.Windows.Forms.MenuStrip m_msMain;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiFile;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiCAM;
		private System.Windows.Forms.MenuStrip m_msCAM;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAddPath;
	}
}

