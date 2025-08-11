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
			this.m_tsmiAddPoint_PointCenter = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAddLine = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAddLine_TwoVertexConnect = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiTransform = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiManualTransform = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmi3PointTransform = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiManipulatorTransform = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiCADOK = new System.Windows.Forms.ToolStripMenuItem();
			this.m_panViewer = new System.Windows.Forms.Panel();
			this.m_panTreeView = new System.Windows.Forms.Panel();
			this.m_TreeView = new System.Windows.Forms.TreeView();
			this.m_panBackGround = new System.Windows.Forms.Panel();
			this.m_msCAD_Transform = new System.Windows.Forms.MenuStrip();
			this.m_tsmiPlane = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiPlanePar = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAxial = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAxialPar = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiPointCoincide = new System.Windows.Forms.ToolStripMenuItem();
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
			this.m_tsmiRemovePath = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiStartPoint = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiReverse = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiSetLead = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiChangeLeadDirection = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiLeadSetting = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiOverCut = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiToolVecReverse = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiToolVec = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiMoveUp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiMoveDown = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiAutoSort = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiBackToCAD = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiCAMOK = new System.Windows.Forms.ToolStripMenuItem();
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
			this.m_msCAD.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.m_msCAD.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiImport,
            this.m_tsmiAddFeature,
            this.m_tsmiTransform,
            this.m_tsmiCADOK});
			this.m_msCAD.Location = new System.Drawing.Point(0, 0);
			this.m_msCAD.Name = "m_msCAD";
			this.m_msCAD.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
			this.m_msCAD.Size = new System.Drawing.Size(1579, 27);
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
			this.m_tsmiImport.Size = new System.Drawing.Size(53, 24);
			this.m_tsmiImport.Text = "導入";
			// 
			// m_tsmiImportBRep
			// 
			this.m_tsmiImportBRep.Name = "m_tsmiImportBRep";
			this.m_tsmiImportBRep.Size = new System.Drawing.Size(128, 26);
			this.m_tsmiImportBRep.Text = "BRep";
			this.m_tsmiImportBRep.Click += new System.EventHandler(this.m_tsmiImportBRep_Click);
			// 
			// m_tsmiImportStep
			// 
			this.m_tsmiImportStep.Name = "m_tsmiImportStep";
			this.m_tsmiImportStep.Size = new System.Drawing.Size(128, 26);
			this.m_tsmiImportStep.Text = "Step";
			this.m_tsmiImportStep.Click += new System.EventHandler(this.m_tsmiImportStep_Click);
			// 
			// m_tsmiImportIges
			// 
			this.m_tsmiImportIges.Name = "m_tsmiImportIges";
			this.m_tsmiImportIges.Size = new System.Drawing.Size(128, 26);
			this.m_tsmiImportIges.Text = "Iges";
			this.m_tsmiImportIges.Click += new System.EventHandler(this.m_tsmiImportIges_Click);
			// 
			// m_tsmiAddFeature
			// 
			this.m_tsmiAddFeature.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiAddPoint,
            this.m_tsmiAddLine});
			this.m_tsmiAddFeature.Name = "m_tsmiAddFeature";
			this.m_tsmiAddFeature.Size = new System.Drawing.Size(83, 24);
			this.m_tsmiAddFeature.Text = "建立特徵";
			// 
			// m_tsmiAddPoint
			// 
			this.m_tsmiAddPoint.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiAddPoint_AcrCenter,
            this.m_tsmiAddPoint_EdgeCenter,
            this.m_tsmiAddPoint_PointCenter});
			this.m_tsmiAddPoint.Name = "m_tsmiAddPoint";
			this.m_tsmiAddPoint.Size = new System.Drawing.Size(137, 26);
			this.m_tsmiAddPoint.Text = "參考點";
			// 
			// m_tsmiAddPoint_AcrCenter
			// 
			this.m_tsmiAddPoint_AcrCenter.Name = "m_tsmiAddPoint_AcrCenter";
			this.m_tsmiAddPoint_AcrCenter.Size = new System.Drawing.Size(152, 26);
			this.m_tsmiAddPoint_AcrCenter.Text = "圓弧中心";
			this.m_tsmiAddPoint_AcrCenter.Click += new System.EventHandler(this.m_tsmiAddPoint_AcrCenter_Click);
			// 
			// m_tsmiAddPoint_EdgeCenter
			// 
			this.m_tsmiAddPoint_EdgeCenter.Name = "m_tsmiAddPoint_EdgeCenter";
			this.m_tsmiAddPoint_EdgeCenter.Size = new System.Drawing.Size(152, 26);
			this.m_tsmiAddPoint_EdgeCenter.Text = "線段中心";
			this.m_tsmiAddPoint_EdgeCenter.Click += new System.EventHandler(this.m_tsmiAddPoint_EdgeCenter_Click);
			// 
			// m_tsmiAddPoint_PointCenter
			// 
			this.m_tsmiAddPoint_PointCenter.Name = "m_tsmiAddPoint_PointCenter";
			this.m_tsmiAddPoint_PointCenter.Size = new System.Drawing.Size(152, 26);
			this.m_tsmiAddPoint_PointCenter.Text = "兩點中心";
			this.m_tsmiAddPoint_PointCenter.Click += new System.EventHandler(this.m_tsmiAddPoint_PointCenter_Click);
			// 
			// m_tsmiAddLine
			// 
			this.m_tsmiAddLine.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiAddLine_TwoVertexConnect});
			this.m_tsmiAddLine.Name = "m_tsmiAddLine";
			this.m_tsmiAddLine.Size = new System.Drawing.Size(137, 26);
			this.m_tsmiAddLine.Text = "參考線";
			// 
			// m_tsmiAddLine_TwoVertexConnect
			// 
			this.m_tsmiAddLine_TwoVertexConnect.Name = "m_tsmiAddLine_TwoVertexConnect";
			this.m_tsmiAddLine_TwoVertexConnect.Size = new System.Drawing.Size(152, 26);
			this.m_tsmiAddLine_TwoVertexConnect.Text = "兩點連線";
			this.m_tsmiAddLine_TwoVertexConnect.Click += new System.EventHandler(this.m_tsmiAddLine_TwoVertexConnect_Click);
			// 
			// m_tsmiTransform
			// 
			this.m_tsmiTransform.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiManualTransform,
            this.m_tsmi3PointTransform,
            this.m_tsmiManipulatorTransform});
			this.m_tsmiTransform.Name = "m_tsmiTransform";
			this.m_tsmiTransform.Size = new System.Drawing.Size(53, 24);
			this.m_tsmiTransform.Text = "轉換";
			// 
			// m_tsmiManualTransform
			// 
			this.m_tsmiManualTransform.Name = "m_tsmiManualTransform";
			this.m_tsmiManualTransform.Size = new System.Drawing.Size(152, 26);
			this.m_tsmiManualTransform.Text = "手動轉換";
			this.m_tsmiManualTransform.Click += new System.EventHandler(this.m_tsmiManualTransform_Click);
			// 
			// m_tsmi3PointTransform
			// 
			this.m_tsmi3PointTransform.Name = "m_tsmi3PointTransform";
			this.m_tsmi3PointTransform.Size = new System.Drawing.Size(152, 26);
			this.m_tsmi3PointTransform.Text = "三點轉換";
			this.m_tsmi3PointTransform.Click += new System.EventHandler(this.m_tsmi3PointTransform_Click);
			// 
			// m_tsmiManipulatorTransform
			// 
			this.m_tsmiManipulatorTransform.Name = "m_tsmiManipulatorTransform";
			this.m_tsmiManipulatorTransform.Size = new System.Drawing.Size(152, 26);
			this.m_tsmiManipulatorTransform.Text = "軸向轉換";
			this.m_tsmiManipulatorTransform.Click += new System.EventHandler(this.m_tsmiAxisTransform_Click);
			// 
			// m_tsmiCADOK
			// 
			this.m_tsmiCADOK.Name = "m_tsmiCADOK";
			this.m_tsmiCADOK.Size = new System.Drawing.Size(44, 24);
			this.m_tsmiCADOK.Text = "OK";
			this.m_tsmiCADOK.Click += new System.EventHandler(this.m_tsmiCADOK_Click);
			// 
			// m_panViewer
			// 
			this.m_panViewer.Dock = System.Windows.Forms.DockStyle.Right;
			this.m_panViewer.Location = new System.Drawing.Point(379, 0);
			this.m_panViewer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_panViewer.Name = "m_panViewer";
			this.m_panViewer.Size = new System.Drawing.Size(1200, 897);
			this.m_panViewer.TabIndex = 1;
			// 
			// m_panTreeView
			// 
			this.m_panTreeView.Controls.Add(this.m_TreeView);
			this.m_panTreeView.Dock = System.Windows.Forms.DockStyle.Left;
			this.m_panTreeView.Location = new System.Drawing.Point(0, 0);
			this.m_panTreeView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_panTreeView.Name = "m_panTreeView";
			this.m_panTreeView.Size = new System.Drawing.Size(400, 897);
			this.m_panTreeView.TabIndex = 1;
			// 
			// m_TreeView
			// 
			this.m_TreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_TreeView.Location = new System.Drawing.Point(0, 0);
			this.m_TreeView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_TreeView.Name = "m_TreeView";
			this.m_TreeView.Size = new System.Drawing.Size(400, 897);
			this.m_TreeView.TabIndex = 1;
			// 
			// m_panBackGround
			// 
			this.m_panBackGround.Controls.Add(this.m_panTreeView);
			this.m_panBackGround.Controls.Add(this.m_panViewer);
			this.m_panBackGround.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panBackGround.Location = new System.Drawing.Point(0, 54);
			this.m_panBackGround.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_panBackGround.Name = "m_panBackGround";
			this.m_panBackGround.Size = new System.Drawing.Size(1579, 897);
			this.m_panBackGround.TabIndex = 0;
			// 
			// m_msCAD_Transform
			// 
			this.m_msCAD_Transform.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.m_msCAD_Transform.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiPlane,
            this.m_tsmiPlanePar,
            this.m_tsmiAxial,
            this.m_tsmiAxialPar,
            this.m_tsmiPointCoincide,
            this.m_tsmiTransformOK});
			this.m_msCAD_Transform.Location = new System.Drawing.Point(0, 54);
			this.m_msCAD_Transform.Name = "m_msCAD_Transform";
			this.m_msCAD_Transform.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
			this.m_msCAD_Transform.Size = new System.Drawing.Size(1579, 28);
			this.m_msCAD_Transform.TabIndex = 1;
			this.m_msCAD_Transform.Text = "menuStrip2";
			this.m_msCAD_Transform.Visible = false;
			// 
			// m_tsmiPlane
			// 
			this.m_tsmiPlane.Name = "m_tsmiPlane";
			this.m_tsmiPlane.Size = new System.Drawing.Size(68, 26);
			this.m_tsmiPlane.Text = "面重合";
			this.m_tsmiPlane.Click += new System.EventHandler(this.m_tsmiPlane_Click);
			// 
			// m_tsmiPlanePar
			// 
			this.m_tsmiPlanePar.Name = "m_tsmiPlanePar";
			this.m_tsmiPlanePar.Size = new System.Drawing.Size(68, 26);
			this.m_tsmiPlanePar.Text = "面平行";
			this.m_tsmiPlanePar.Click += new System.EventHandler(this.m_tsmiPlanePar_Click);
			// 
			// m_tsmiAxial
			// 
			this.m_tsmiAxial.Name = "m_tsmiAxial";
			this.m_tsmiAxial.Size = new System.Drawing.Size(68, 26);
			this.m_tsmiAxial.Text = "軸重合";
			this.m_tsmiAxial.Click += new System.EventHandler(this.m_tsmiAxial_Click);
			// 
			// m_tsmiAxialPar
			// 
			this.m_tsmiAxialPar.Name = "m_tsmiAxialPar";
			this.m_tsmiAxialPar.Size = new System.Drawing.Size(68, 26);
			this.m_tsmiAxialPar.Text = "軸平行";
			this.m_tsmiAxialPar.Click += new System.EventHandler(this.m_tsmiAxialPar_Click);
			// 
			// m_tsmiPointCoincide
			// 
			this.m_tsmiPointCoincide.Name = "m_tsmiPointCoincide";
			this.m_tsmiPointCoincide.Size = new System.Drawing.Size(68, 26);
			this.m_tsmiPointCoincide.Text = "點重合";
			this.m_tsmiPointCoincide.Click += new System.EventHandler(this.m_tsmiPointCoincide_Click);
			// 
			// m_tsmiTransformOK
			// 
			this.m_tsmiTransformOK.Name = "m_tsmiTransformOK";
			this.m_tsmiTransformOK.Size = new System.Drawing.Size(44, 26);
			this.m_tsmiTransformOK.Text = "OK";
			this.m_tsmiTransformOK.Click += new System.EventHandler(this.m_tsmiTransformOK_Click);
			// 
			// m_msCAD_SelectFace
			// 
			this.m_msCAD_SelectFace.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.m_msCAD_SelectFace.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiSelectD1ContFace,
            this.m_tsmiSelectPath});
			this.m_msCAD_SelectFace.Location = new System.Drawing.Point(0, 54);
			this.m_msCAD_SelectFace.Name = "m_msCAD_SelectFace";
			this.m_msCAD_SelectFace.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
			this.m_msCAD_SelectFace.Size = new System.Drawing.Size(1579, 28);
			this.m_msCAD_SelectFace.TabIndex = 0;
			this.m_msCAD_SelectFace.Text = "menuStrip1";
			this.m_msCAD_SelectFace.Visible = false;
			// 
			// m_tsmiSelectD1ContFace
			// 
			this.m_tsmiSelectD1ContFace.Name = "m_tsmiSelectD1ContFace";
			this.m_tsmiSelectD1ContFace.Size = new System.Drawing.Size(77, 24);
			this.m_tsmiSelectD1ContFace.Text = "D1 連續";
			this.m_tsmiSelectD1ContFace.Click += new System.EventHandler(this.m_tsmiSelectD1ContFace_Click);
			// 
			// m_tsmiSelectPath
			// 
			this.m_tsmiSelectPath.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiSelPath_FreeBound,
            this.m_tsmiSelPath_Manual});
			this.m_tsmiSelectPath.Name = "m_tsmiSelectPath";
			this.m_tsmiSelectPath.Size = new System.Drawing.Size(83, 24);
			this.m_tsmiSelectPath.Text = "選取路徑";
			// 
			// m_tsmiSelPath_FreeBound
			// 
			this.m_tsmiSelPath_FreeBound.Name = "m_tsmiSelPath_FreeBound";
			this.m_tsmiSelPath_FreeBound.Size = new System.Drawing.Size(167, 26);
			this.m_tsmiSelPath_FreeBound.Text = "所有面邊界";
			this.m_tsmiSelPath_FreeBound.Click += new System.EventHandler(this.m_tsmiSelPath_FreeBound_Click);
			// 
			// m_tsmiSelPath_Manual
			// 
			this.m_tsmiSelPath_Manual.Name = "m_tsmiSelPath_Manual";
			this.m_tsmiSelPath_Manual.Size = new System.Drawing.Size(167, 26);
			this.m_tsmiSelPath_Manual.Text = "手動選取";
			this.m_tsmiSelPath_Manual.Click += new System.EventHandler(this.m_tsmiSelPath_Manual_Click);
			// 
			// m_msCAD_ManualSelectPath
			// 
			this.m_msCAD_ManualSelectPath.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.m_msCAD_ManualSelectPath.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiManualSelectPathOK});
			this.m_msCAD_ManualSelectPath.Location = new System.Drawing.Point(0, 54);
			this.m_msCAD_ManualSelectPath.Name = "m_msCAD_ManualSelectPath";
			this.m_msCAD_ManualSelectPath.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
			this.m_msCAD_ManualSelectPath.Size = new System.Drawing.Size(1579, 28);
			this.m_msCAD_ManualSelectPath.TabIndex = 1;
			this.m_msCAD_ManualSelectPath.Text = "menuStrip1";
			this.m_msCAD_ManualSelectPath.Visible = false;
			// 
			// m_tsmiManualSelectPathOK
			// 
			this.m_tsmiManualSelectPathOK.Name = "m_tsmiManualSelectPathOK";
			this.m_tsmiManualSelectPathOK.Size = new System.Drawing.Size(44, 24);
			this.m_tsmiManualSelectPathOK.Text = "OK";
			this.m_tsmiManualSelectPathOK.Click += new System.EventHandler(this.m_tsmiManualSelectPathOK_Click);
			// 
			// m_msMain
			// 
			this.m_msMain.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.m_msMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiFile,
            this.m_tsmiCAM});
			this.m_msMain.Location = new System.Drawing.Point(0, 0);
			this.m_msMain.Name = "m_msMain";
			this.m_msMain.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
			this.m_msMain.Size = new System.Drawing.Size(1579, 28);
			this.m_msMain.TabIndex = 1;
			this.m_msMain.Text = "menuStrip1";
			this.m_msMain.Visible = false;
			// 
			// m_tsmiFile
			// 
			this.m_tsmiFile.Name = "m_tsmiFile";
			this.m_tsmiFile.Size = new System.Drawing.Size(53, 24);
			this.m_tsmiFile.Text = "檔案";
			// 
			// m_tsmiCAM
			// 
			this.m_tsmiCAM.Name = "m_tsmiCAM";
			this.m_tsmiCAM.Size = new System.Drawing.Size(57, 24);
			this.m_tsmiCAM.Text = "CAM";
			this.m_tsmiCAM.Click += new System.EventHandler(this.m_tsmiCAM_Click);
			// 
			// m_msCAM
			// 
			this.m_msCAM.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.m_msCAM.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiAddPath,
            this.m_tsmiRemovePath,
            this.m_tsmiStartPoint,
            this.m_tsmiReverse,
            this.m_tsmiSetLead,
            this.m_tsmiOverCut,
            this.m_tsmiToolVecReverse,
            this.m_tsmiToolVec,
            this.m_tsmiMoveUp,
            this.m_tsmiMoveDown,
            this.m_tsmiAutoSort,
            this.m_tsmiBackToCAD,
            this.m_tsmiCAMOK});
			this.m_msCAM.Location = new System.Drawing.Point(0, 27);
			this.m_msCAM.Name = "m_msCAM";
			this.m_msCAM.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
			this.m_msCAM.Size = new System.Drawing.Size(1579, 27);
			this.m_msCAM.TabIndex = 1;
			this.m_msCAM.Text = "menuStrip1";
			// 
			// m_tsmiAddPath
			// 
			this.m_tsmiAddPath.Name = "m_tsmiAddPath";
			this.m_tsmiAddPath.Size = new System.Drawing.Size(83, 23);
			this.m_tsmiAddPath.Text = "新增路徑";
			this.m_tsmiAddPath.Click += new System.EventHandler(this.m_tsmiAddPath_Click);
			// 
			// m_tsmiRemovePath
			// 
			this.m_tsmiRemovePath.Name = "m_tsmiRemovePath";
			this.m_tsmiRemovePath.Size = new System.Drawing.Size(83, 23);
			this.m_tsmiRemovePath.Text = "刪除路徑";
			this.m_tsmiRemovePath.Click += new System.EventHandler(this.m_tsmiRemovePath_Click);
			// 
			// m_tsmiStartPoint
			// 
			this.m_tsmiStartPoint.Name = "m_tsmiStartPoint";
			this.m_tsmiStartPoint.Size = new System.Drawing.Size(53, 23);
			this.m_tsmiStartPoint.Text = "起點";
			this.m_tsmiStartPoint.Click += new System.EventHandler(this.m_tsmiStartPoint_Click);
			// 
			// m_tsmiReverse
			// 
			this.m_tsmiReverse.Name = "m_tsmiReverse";
			this.m_tsmiReverse.Size = new System.Drawing.Size(53, 23);
			this.m_tsmiReverse.Text = "反向";
			this.m_tsmiReverse.Click += new System.EventHandler(this.m_tsmiReverse_Click);
			// 
			// m_tsmiSetLead
			// 
			this.m_tsmiSetLead.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiChangeLeadDirection,
            this.m_tsmiLeadSetting});
			this.m_tsmiSetLead.Name = "m_tsmiSetLead";
			this.m_tsmiSetLead.Size = new System.Drawing.Size(53, 23);
			this.m_tsmiSetLead.Text = "引線";
			// 
			// m_tsmiChangeLeadDirection
			// 
			this.m_tsmiChangeLeadDirection.Name = "m_tsmiChangeLeadDirection";
			this.m_tsmiChangeLeadDirection.Size = new System.Drawing.Size(152, 22);
			this.m_tsmiChangeLeadDirection.Text = "更換引線方向";
			this.m_tsmiChangeLeadDirection.Click += new System.EventHandler(this.m_tsmiChangeLeadDirection_Click);
			// 
			// m_tsmiLeadSetting
			// 
			this.m_tsmiLeadSetting.Name = "m_tsmiLeadSetting";
			this.m_tsmiLeadSetting.Size = new System.Drawing.Size(152, 22);
			this.m_tsmiLeadSetting.Text = "設置引線";
			this.m_tsmiLeadSetting.Click += new System.EventHandler(this.m_tsmiLeadSetting_Click);
			// 
			// m_tsmiOverCut
			// 
			this.m_tsmiOverCut.Name = "m_tsmiOverCut";
			this.m_tsmiOverCut.Size = new System.Drawing.Size(53, 23);
			this.m_tsmiOverCut.Text = "過切";
			this.m_tsmiOverCut.Click += new System.EventHandler(this.m_tsmiOverCut_Click);
			// 
			// m_tsmiToolVecReverse
			// 
			this.m_tsmiToolVecReverse.Name = "m_tsmiToolVecReverse";
			this.m_tsmiToolVecReverse.Size = new System.Drawing.Size(71, 20);
			this.m_tsmiToolVecReverse.Text = "姿態反向";
			this.m_tsmiToolVecReverse.Click += new System.EventHandler(this.m_tsmiToolVecReverse_Click);
			// 
			// m_tsmiToolVec
			// 
			this.m_tsmiToolVec.Name = "m_tsmiToolVec";
			this.m_tsmiToolVec.Size = new System.Drawing.Size(53, 23);
			this.m_tsmiToolVec.Text = "姿態";
			this.m_tsmiToolVec.Click += new System.EventHandler(this.m_tsmiToolVec_Click);
			// 
			// m_tsmiMoveUp
			// 
			this.m_tsmiMoveUp.Name = "m_tsmiMoveUp";
			this.m_tsmiMoveUp.Size = new System.Drawing.Size(53, 23);
			this.m_tsmiMoveUp.Text = "上移";
			this.m_tsmiMoveUp.Click += new System.EventHandler(this.m_tsmiMoveUp_Click);
			// 
			// m_tsmiMoveDown
			// 
			this.m_tsmiMoveDown.Name = "m_tsmiMoveDown";
			this.m_tsmiMoveDown.Size = new System.Drawing.Size(53, 23);
			this.m_tsmiMoveDown.Text = "下移";
			this.m_tsmiMoveDown.Click += new System.EventHandler(this.m_tsmiMoveDown_Click);
			// 
			// m_tsmiAutoSort
			// 
			this.m_tsmiAutoSort.Name = "m_tsmiAutoSort";
			this.m_tsmiAutoSort.Size = new System.Drawing.Size(83, 23);
			this.m_tsmiAutoSort.Text = "自動排序";
			this.m_tsmiAutoSort.Click += new System.EventHandler(this.m_tsmiAutoSort_Click);
			// 
			// m_tsmiBackToCAD
			// 
			this.m_tsmiBackToCAD.Name = "m_tsmiBackToCAD";
			this.m_tsmiBackToCAD.Size = new System.Drawing.Size(88, 23);
			this.m_tsmiBackToCAD.Text = "回到 CAD";
			this.m_tsmiBackToCAD.Click += new System.EventHandler(this.m_tsmiBackToCAD_Click);
			// 
			// m_tsmiCAMOK
			// 
			this.m_tsmiCAMOK.Name = "m_tsmiCAMOK";
			this.m_tsmiCAMOK.Size = new System.Drawing.Size(44, 23);
			this.m_tsmiCAMOK.Text = "OK";
			this.m_tsmiCAMOK.Click += new System.EventHandler(this.m_tsmiCAMOK_Click);
			// 
			// StartupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(1579, 951);
			this.Controls.Add(this.m_panBackGround);
			this.Controls.Add(this.m_msCAD_Transform);
			this.Controls.Add(this.m_msCAD_SelectFace);
			this.Controls.Add(this.m_msCAD_ManualSelectPath);
			this.Controls.Add(this.m_msCAM);
			this.Controls.Add(this.m_msCAD);
			this.Controls.Add(this.m_msMain);
			this.MainMenuStrip = this.m_msCAD;
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
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
		private System.Windows.Forms.ToolStripMenuItem m_tsmiPlanePar;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAxial;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAxialPar;
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
		private System.Windows.Forms.ToolStripMenuItem m_tsmiCADOK;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiStartPoint;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiReverse;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiSetLead;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiOverCut;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiToolVec;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiCAMOK;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiMoveUp;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiMoveDown;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAutoSort;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiRemovePath;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiBackToCAD;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiManipulatorTransform;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiPointCoincide;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAddPoint_PointCenter;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAddLine;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiAddLine_TwoVertexConnect;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiChangeLeadDirection;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiLeadSetting;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiToolVecReverse;
	}
}

