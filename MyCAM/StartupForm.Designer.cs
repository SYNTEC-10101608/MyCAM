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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartupForm));
			this.m_msMode = new System.Windows.Forms.MenuStrip();
			this.m_tsmiFile = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiCAD = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiCAM = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsbSetting = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsbVNCConnection = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsCAMFunction = new System.Windows.Forms.ToolStrip();
			this.m_tsbAddPath = new System.Windows.Forms.ToolStripButton();
			this.m_tsbDeletePath = new System.Windows.Forms.ToolStripButton();
			this.m_tsbStartPoint = new System.Windows.Forms.ToolStripButton();
			this.m_tsbReverse = new System.Windows.Forms.ToolStripButton();
			this.m_tsbSetLead = new System.Windows.Forms.ToolStripButton();
			this.m_tsbOverCut = new System.Windows.Forms.ToolStripButton();
			this.m_tsbToolVecReverse = new System.Windows.Forms.ToolStripButton();
			this.m_tsbToolVec = new System.Windows.Forms.ToolStripButton();
			this.m_tsbFixedToolVec = new System.Windows.Forms.ToolStripButton();
			this.m_tsbMoveUp = new System.Windows.Forms.ToolStripButton();
			this.m_tsbMoveDown = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAutoOrder = new System.Windows.Forms.ToolStripButton();
			this.m_tsbSetTraverseData = new System.Windows.Forms.ToolStripButton();
			this.m_tsbEntryAndExitSetting = new System.Windows.Forms.ToolStripButton();
			this.m_tsbPatternSetting = new System.Windows.Forms.ToolStripButton();
			this.m_tsbConvertNC = new System.Windows.Forms.ToolStripButton();
			this.m_tscLevel1Container = new System.Windows.Forms.ToolStripContainer();
			this.m_tsFileFunction = new System.Windows.Forms.ToolStrip();
			this.m_tsbImport3DFile = new System.Windows.Forms.ToolStripButton();
			this.m_tsbReadProjectFile = new System.Windows.Forms.ToolStripButton();
			this.m_tsbSaveProjectFile = new System.Windows.Forms.ToolStripButton();
			this.m_tsCADFunction = new System.Windows.Forms.ToolStrip();
			this.m_tsbSewPart = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddPoint_AcrCenter = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddPoint_EdgeCenter = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddPoint_PointCenter = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddLine_TwoVertexConnect = new System.Windows.Forms.ToolStripButton();
			this.m_tsbManualTransform = new System.Windows.Forms.ToolStripButton();
			this.m_tsb3PntTransform = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAxisTransform = new System.Windows.Forms.ToolStripButton();
			this.m_tscLevel2Container = new System.Windows.Forms.ToolStripContainer();
			this.m_tsAddPathSubFunc = new System.Windows.Forms.ToolStrip();
			this.m_tsbSelectD1ContFace = new System.Windows.Forms.ToolStripButton();
			this.m_tsbSelPath_FreeBound = new System.Windows.Forms.ToolStripButton();
			this.m_tsbSelPath_Manual = new System.Windows.Forms.ToolStripButton();
			this.m_tsManualTrans = new System.Windows.Forms.ToolStrip();
			this.m_tsbManualTransPlane = new System.Windows.Forms.ToolStripButton();
			this.m_tsbManualTransPlanePar = new System.Windows.Forms.ToolStripButton();
			this.m_tsbManualTransAxial = new System.Windows.Forms.ToolStripButton();
			this.m_tsbManualTransAxialPar = new System.Windows.Forms.ToolStripButton();
			this.m_tsbManualTransPointCoincide = new System.Windows.Forms.ToolStripButton();
			this.m_tscLevel3Container = new System.Windows.Forms.ToolStripContainer();
			this.m_tsSelectPath = new System.Windows.Forms.ToolStrip();
			this.m_tsbManualSelectPathOK = new System.Windows.Forms.ToolStripButton();
			this.m_panVNC = new System.Windows.Forms.Panel();
			this.m_panTreeView = new System.Windows.Forms.Panel();
			this.m_pnlLog = new System.Windows.Forms.Panel();
			this.m_panViewer = new System.Windows.Forms.Panel();
			this.m_tsViewerToolBar = new System.Windows.Forms.ToolStrip();
			this.m_tsbAXO = new System.Windows.Forms.ToolStripButton();
			this.m_tsbXPos = new System.Windows.Forms.ToolStripButton();
			this.m_tsbXNeg = new System.Windows.Forms.ToolStripButton();
			this.m_tsbYPos = new System.Windows.Forms.ToolStripButton();
			this.m_tsbYNeg = new System.Windows.Forms.ToolStripButton();
			this.m_tsbZPos = new System.Windows.Forms.ToolStripButton();
			this.m_tsbZNeg = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_tsbShowVec = new System.Windows.Forms.ToolStripButton();
			this.m_tsbShowOrder = new System.Windows.Forms.ToolStripButton();
			this.m_tsbShowOrientation = new System.Windows.Forms.ToolStripButton();
			this.m_tsbShowTraverse = new System.Windows.Forms.ToolStripButton();
			this.m_panBackGround = new System.Windows.Forms.TableLayoutPanel();
			this.m_msMode.SuspendLayout();
			this.m_tsCAMFunction.SuspendLayout();
			this.m_tscLevel1Container.ContentPanel.SuspendLayout();
			this.m_tscLevel1Container.SuspendLayout();
			this.m_tsFileFunction.SuspendLayout();
			this.m_tsCADFunction.SuspendLayout();
			this.m_tscLevel2Container.ContentPanel.SuspendLayout();
			this.m_tscLevel2Container.SuspendLayout();
			this.m_tsAddPathSubFunc.SuspendLayout();
			this.m_tsManualTrans.SuspendLayout();
			this.m_tscLevel3Container.ContentPanel.SuspendLayout();
			this.m_tscLevel3Container.SuspendLayout();
			this.m_tsSelectPath.SuspendLayout();
			this.m_panViewer.SuspendLayout();
			this.m_tsViewerToolBar.SuspendLayout();
			this.m_panBackGround.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_msMode
			// 
			this.m_msMode.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.m_msMode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiFile,
            this.m_tsmiCAD,
            this.m_tsmiCAM,
            this.m_tsbSetting});
			this.m_msMode.Location = new System.Drawing.Point(0, 0);
			this.m_msMode.Name = "m_msMode";
			this.m_msMode.Size = new System.Drawing.Size(1064, 24);
			this.m_msMode.TabIndex = 1;
			this.m_msMode.Text = "menuStrip1";
			// 
			// m_tsmiFile
			// 
			this.m_tsmiFile.Name = "m_tsmiFile";
			this.m_tsmiFile.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiFile.Text = "檔案";
			this.m_tsmiFile.Click += new System.EventHandler(this.m_tsmiFile_Click);
			// 
			// m_tsmiCAD
			// 
			this.m_tsmiCAD.Name = "m_tsmiCAD";
			this.m_tsmiCAD.Size = new System.Drawing.Size(44, 20);
			this.m_tsmiCAD.Text = "CAD";
			this.m_tsmiCAD.Click += new System.EventHandler(this.m_tsmiCAD_Click);
			// 
			// m_tsmiCAM
			// 
			this.m_tsmiCAM.Name = "m_tsmiCAM";
			this.m_tsmiCAM.Size = new System.Drawing.Size(47, 20);
			this.m_tsmiCAM.Text = "CAM";
			this.m_tsmiCAM.Click += new System.EventHandler(this.m_tsmiCAM_Click);
			// 
			// m_tsbSetting
			// 
			this.m_tsbSetting.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbVNCConnection});
			this.m_tsbSetting.Name = "m_tsbSetting";
			this.m_tsbSetting.Size = new System.Drawing.Size(43, 20);
			this.m_tsbSetting.Text = "設定";
			// 
			// m_tsbVNCConnection
			// 
			this.m_tsbVNCConnection.Name = "m_tsbVNCConnection";
			this.m_tsbVNCConnection.Size = new System.Drawing.Size(148, 22);
			this.m_tsbVNCConnection.Text = "VNC連線設定";
			this.m_tsbVNCConnection.Click += new System.EventHandler(this.m_tsbVNCConnection_Click);
			// 
			// m_tsCAMFunction
			// 
			this.m_tsCAMFunction.BackColor = System.Drawing.SystemColors.Control;
			this.m_tsCAMFunction.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsCAMFunction.ImageScalingSize = new System.Drawing.Size(40, 40);
			this.m_tsCAMFunction.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbAddPath,
            this.m_tsbDeletePath,
            this.m_tsbStartPoint,
            this.m_tsbReverse,
            this.m_tsbSetLead,
            this.m_tsbOverCut,
            this.m_tsbToolVecReverse,
            this.m_tsbToolVec,
            this.m_tsbFixedToolVec,
            this.m_tsbMoveUp,
            this.m_tsbMoveDown,
            this.m_tsbAutoOrder,
            this.m_tsbSetTraverseData,
            this.m_tsbEntryAndExitSetting,
            this.m_tsbPatternSetting,
            this.m_tsbConvertNC});
			this.m_tsCAMFunction.Location = new System.Drawing.Point(0, 0);
			this.m_tsCAMFunction.Name = "m_tsCAMFunction";
			this.m_tsCAMFunction.Size = new System.Drawing.Size(1064, 21);
			this.m_tsCAMFunction.TabIndex = 2;
			this.m_tsCAMFunction.Text = "toolStrip1";
			// 
			// m_tsbAddPath
			// 
			this.m_tsbAddPath.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAddPath.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbAddPath.Image")));
			this.m_tsbAddPath.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddPath.Name = "m_tsbAddPath";
			this.m_tsbAddPath.Size = new System.Drawing.Size(44, 18);
			this.m_tsbAddPath.Text = "新增路徑";
			this.m_tsbAddPath.Click += new System.EventHandler(this.m_tsbAddPath_Click);
			// 
			// m_tsbDeletePath
			// 
			this.m_tsbDeletePath.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbDeletePath.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbDeletePath.Image")));
			this.m_tsbDeletePath.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbDeletePath.Name = "m_tsbDeletePath";
			this.m_tsbDeletePath.Size = new System.Drawing.Size(44, 18);
			this.m_tsbDeletePath.Text = "刪除路徑";
			this.m_tsbDeletePath.Click += new System.EventHandler(this.m_tsbDeletePath_Click);
			// 
			// m_tsbStartPoint
			// 
			this.m_tsbStartPoint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbStartPoint.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbStartPoint.Image")));
			this.m_tsbStartPoint.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbStartPoint.Name = "m_tsbStartPoint";
			this.m_tsbStartPoint.Size = new System.Drawing.Size(44, 18);
			this.m_tsbStartPoint.Text = "起點";
			this.m_tsbStartPoint.Click += new System.EventHandler(this.m_tsbStartPoint_Click);
			// 
			// m_tsbReverse
			// 
			this.m_tsbReverse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbReverse.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbReverse.Image")));
			this.m_tsbReverse.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbReverse.Name = "m_tsbReverse";
			this.m_tsbReverse.Size = new System.Drawing.Size(44, 18);
			this.m_tsbReverse.Text = "反向";
			this.m_tsbReverse.Click += new System.EventHandler(this.m_tsbReverse_Click);
			// 
			// m_tsbSetLead
			// 
			this.m_tsbSetLead.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbSetLead.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbSetLead.Image")));
			this.m_tsbSetLead.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbSetLead.Name = "m_tsbSetLead";
			this.m_tsbSetLead.Size = new System.Drawing.Size(44, 18);
			this.m_tsbSetLead.Text = "引線";
			this.m_tsbSetLead.Click += new System.EventHandler(this.m_tsbSetLead_Click);
			// 
			// m_tsbOverCut
			// 
			this.m_tsbOverCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbOverCut.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbOverCut.Image")));
			this.m_tsbOverCut.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbOverCut.Name = "m_tsbOverCut";
			this.m_tsbOverCut.Size = new System.Drawing.Size(44, 18);
			this.m_tsbOverCut.Text = "過切";
			this.m_tsbOverCut.Click += new System.EventHandler(this.m_tsbOverCut_Click);
			// 
			// m_tsbToolVecReverse
			// 
			this.m_tsbToolVecReverse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbToolVecReverse.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbToolVecReverse.Image")));
			this.m_tsbToolVecReverse.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbToolVecReverse.Name = "m_tsbToolVecReverse";
			this.m_tsbToolVecReverse.Size = new System.Drawing.Size(44, 18);
			this.m_tsbToolVecReverse.Text = "姿態反向";
			this.m_tsbToolVecReverse.Click += new System.EventHandler(this.m_tsbTooVecReverse_Click);
			// 
			// m_tsbToolVec
			// 
			this.m_tsbToolVec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbToolVec.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbToolVec.Image")));
			this.m_tsbToolVec.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbToolVec.Name = "m_tsbToolVec";
			this.m_tsbToolVec.Size = new System.Drawing.Size(44, 18);
			this.m_tsbToolVec.Text = "姿態";
			this.m_tsbToolVec.Click += new System.EventHandler(this.m_tsbToolVec_Click);
			// 
			// m_tsbFixedToolVec
			// 
			this.m_tsbFixedToolVec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbFixedToolVec.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbFixedToolVec.Image")));
			this.m_tsbFixedToolVec.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbFixedToolVec.Name = "m_tsbFixedToolVec";
			this.m_tsbFixedToolVec.Size = new System.Drawing.Size(44, 18);
			this.m_tsbFixedToolVec.Text = "定姿態切割";
			this.m_tsbFixedToolVec.Click += new System.EventHandler(this.m_tsbFixedToolVec_Click);
			// 
			// m_tsbMoveUp
			// 
			this.m_tsbMoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbMoveUp.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbMoveUp.Image")));
			this.m_tsbMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbMoveUp.Name = "m_tsbMoveUp";
			this.m_tsbMoveUp.Size = new System.Drawing.Size(44, 18);
			this.m_tsbMoveUp.Text = "順序往前";
			this.m_tsbMoveUp.Click += new System.EventHandler(this.m_tsbMoveUp_Click);
			// 
			// m_tsbMoveDown
			// 
			this.m_tsbMoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbMoveDown.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbMoveDown.Image")));
			this.m_tsbMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbMoveDown.Name = "m_tsbMoveDown";
			this.m_tsbMoveDown.Size = new System.Drawing.Size(44, 18);
			this.m_tsbMoveDown.Text = "順序往後";
			this.m_tsbMoveDown.Click += new System.EventHandler(this.m_tsbMoveDown_Click);
			// 
			// m_tsbAutoOrder
			// 
			this.m_tsbAutoOrder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAutoOrder.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbAutoOrder.Image")));
			this.m_tsbAutoOrder.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAutoOrder.Name = "m_tsbAutoOrder";
			this.m_tsbAutoOrder.Size = new System.Drawing.Size(44, 18);
			this.m_tsbAutoOrder.Text = "自動排序";
			this.m_tsbAutoOrder.Click += new System.EventHandler(this.m_tsbAutoOrder_Click);
			// 
			// m_tsbSetTraverseData
			// 
			this.m_tsbSetTraverseData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbSetTraverseData.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbSetTraverseData.Image")));
			this.m_tsbSetTraverseData.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbSetTraverseData.Name = "m_tsbSetTraverseData";
			this.m_tsbSetTraverseData.Size = new System.Drawing.Size(44, 18);
			this.m_tsbSetTraverseData.Text = "空移參數設定";
			this.m_tsbSetTraverseData.Click += new System.EventHandler(this.m_tsbSetTraverseData_Click);
			// 
			// m_tsbEntryAndExitSetting
			// 
			this.m_tsbEntryAndExitSetting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbEntryAndExitSetting.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbEntryAndExitSetting.Image")));
			this.m_tsbEntryAndExitSetting.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbEntryAndExitSetting.Name = "m_tsbEntryAndExitSetting";
			this.m_tsbEntryAndExitSetting.Size = new System.Drawing.Size(44, 18);
			this.m_tsbEntryAndExitSetting.Text = "進入/退出";
			this.m_tsbEntryAndExitSetting.Click += new System.EventHandler(this.m_tsbEntryAndExitSetting_Click);
			// 
			// m_tsbPatternSetting
			// 
			this.m_tsbPatternSetting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbPatternSetting.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbPatternSetting.Image")));
			this.m_tsbPatternSetting.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbPatternSetting.Name = "m_tsbPatternSetting";
			this.m_tsbPatternSetting.Size = new System.Drawing.Size(44, 18);
			this.m_tsbPatternSetting.Text = "toolStripButton1";
			this.m_tsbPatternSetting.ToolTipText = "設定圖元";
			this.m_tsbPatternSetting.Click += new System.EventHandler(this.m_tsbPatternSetting_Click);
			// 
			// m_tsbConvertNC
			// 
			this.m_tsbConvertNC.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbConvertNC.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbConvertNC.Image")));
			this.m_tsbConvertNC.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbConvertNC.Name = "m_tsbConvertNC";
			this.m_tsbConvertNC.Size = new System.Drawing.Size(44, 18);
			this.m_tsbConvertNC.Text = "轉出NC";
			this.m_tsbConvertNC.Click += new System.EventHandler(this.m_tsbConvertNC_Click);
			// 
			// m_tscLevel1Container
			// 
			// 
			// m_tscLevel1Container.ContentPanel
			// 
			this.m_tscLevel1Container.ContentPanel.Controls.Add(this.m_tsFileFunction);
			this.m_tscLevel1Container.ContentPanel.Controls.Add(this.m_tsCADFunction);
			this.m_tscLevel1Container.ContentPanel.Controls.Add(this.m_tsCAMFunction);
			this.m_tscLevel1Container.ContentPanel.Size = new System.Drawing.Size(1064, 21);
			this.m_tscLevel1Container.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_tscLevel1Container.Location = new System.Drawing.Point(0, 24);
			this.m_tscLevel1Container.Name = "m_tscLevel1Container";
			this.m_tscLevel1Container.Size = new System.Drawing.Size(1064, 46);
			this.m_tscLevel1Container.TabIndex = 5;
			this.m_tscLevel1Container.Text = "toolStripContainer1";
			// 
			// m_tsFileFunction
			// 
			this.m_tsFileFunction.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsFileFunction.ImageScalingSize = new System.Drawing.Size(40, 40);
			this.m_tsFileFunction.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbImport3DFile,
            this.m_tsbReadProjectFile,
            this.m_tsbSaveProjectFile});
			this.m_tsFileFunction.Location = new System.Drawing.Point(0, 0);
			this.m_tsFileFunction.Name = "m_tsFileFunction";
			this.m_tsFileFunction.Size = new System.Drawing.Size(1064, 21);
			this.m_tsFileFunction.TabIndex = 4;
			this.m_tsFileFunction.Text = "匯入3D模型";
			// 
			// m_tsbImport3DFile
			// 
			this.m_tsbImport3DFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbImport3DFile.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbImport3DFile.Image")));
			this.m_tsbImport3DFile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbImport3DFile.Name = "m_tsbImport3DFile";
			this.m_tsbImport3DFile.Size = new System.Drawing.Size(44, 18);
			this.m_tsbImport3DFile.Text = "匯入3D模型";
			this.m_tsbImport3DFile.Click += new System.EventHandler(this.m_tsbImport3DFile_Click);
			// 
			// m_tsbReadProjectFile
			// 
			this.m_tsbReadProjectFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbReadProjectFile.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbReadProjectFile.Image")));
			this.m_tsbReadProjectFile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbReadProjectFile.Name = "m_tsbReadProjectFile";
			this.m_tsbReadProjectFile.Size = new System.Drawing.Size(44, 18);
			this.m_tsbReadProjectFile.Text = "匯入專案檔";
			this.m_tsbReadProjectFile.Click += new System.EventHandler(this.m_tsbImportProjectFile_Click);
			// 
			// m_tsbSaveProjectFile
			// 
			this.m_tsbSaveProjectFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbSaveProjectFile.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbSaveProjectFile.Image")));
			this.m_tsbSaveProjectFile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbSaveProjectFile.Name = "m_tsbSaveProjectFile";
			this.m_tsbSaveProjectFile.Size = new System.Drawing.Size(44, 18);
			this.m_tsbSaveProjectFile.Text = "儲存專案檔";
			this.m_tsbSaveProjectFile.Click += new System.EventHandler(this.m_tsbSaveProjectFile_Click);
			// 
			// m_tsCADFunction
			// 
			this.m_tsCADFunction.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsCADFunction.ImageScalingSize = new System.Drawing.Size(40, 40);
			this.m_tsCADFunction.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbSewPart,
            this.m_tsbAddPoint_AcrCenter,
            this.m_tsbAddPoint_EdgeCenter,
            this.m_tsbAddPoint_PointCenter,
            this.m_tsbAddLine_TwoVertexConnect,
            this.m_tsbManualTransform,
            this.m_tsb3PntTransform,
            this.m_tsbAxisTransform});
			this.m_tsCADFunction.Location = new System.Drawing.Point(0, 0);
			this.m_tsCADFunction.Name = "m_tsCADFunction";
			this.m_tsCADFunction.Size = new System.Drawing.Size(1064, 21);
			this.m_tsCADFunction.TabIndex = 3;
			this.m_tsCADFunction.Text = "CAD功能";
			// 
			// m_tsbSewPart
			// 
			this.m_tsbSewPart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbSewPart.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbSewPart.Image")));
			this.m_tsbSewPart.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbSewPart.Name = "m_tsbSewPart";
			this.m_tsbSewPart.Size = new System.Drawing.Size(44, 18);
			this.m_tsbSewPart.Text = "模型縫補";
			this.m_tsbSewPart.Click += new System.EventHandler(this.m_tsbSewPart_Click);
			// 
			// m_tsbAddPoint_AcrCenter
			// 
			this.m_tsbAddPoint_AcrCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAddPoint_AcrCenter.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbAddPoint_AcrCenter.Image")));
			this.m_tsbAddPoint_AcrCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddPoint_AcrCenter.Name = "m_tsbAddPoint_AcrCenter";
			this.m_tsbAddPoint_AcrCenter.Size = new System.Drawing.Size(44, 18);
			this.m_tsbAddPoint_AcrCenter.Text = "新增圓弧中心";
			this.m_tsbAddPoint_AcrCenter.Click += new System.EventHandler(this.m_tsbAddPoint_AcrCenter_Click);
			// 
			// m_tsbAddPoint_EdgeCenter
			// 
			this.m_tsbAddPoint_EdgeCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAddPoint_EdgeCenter.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbAddPoint_EdgeCenter.Image")));
			this.m_tsbAddPoint_EdgeCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddPoint_EdgeCenter.Name = "m_tsbAddPoint_EdgeCenter";
			this.m_tsbAddPoint_EdgeCenter.Size = new System.Drawing.Size(44, 18);
			this.m_tsbAddPoint_EdgeCenter.Text = "新增線段中心";
			this.m_tsbAddPoint_EdgeCenter.Click += new System.EventHandler(this.m_tsbAddPoint_EdgeCenter_Click);
			// 
			// m_tsbAddPoint_PointCenter
			// 
			this.m_tsbAddPoint_PointCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAddPoint_PointCenter.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbAddPoint_PointCenter.Image")));
			this.m_tsbAddPoint_PointCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddPoint_PointCenter.Name = "m_tsbAddPoint_PointCenter";
			this.m_tsbAddPoint_PointCenter.Size = new System.Drawing.Size(44, 18);
			this.m_tsbAddPoint_PointCenter.Text = "新增兩點中心";
			this.m_tsbAddPoint_PointCenter.Click += new System.EventHandler(this.m_tsbAddPoint_PointCenter_Click);
			// 
			// m_tsbAddLine_TwoVertexConnect
			// 
			this.m_tsbAddLine_TwoVertexConnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAddLine_TwoVertexConnect.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbAddLine_TwoVertexConnect.Image")));
			this.m_tsbAddLine_TwoVertexConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddLine_TwoVertexConnect.Name = "m_tsbAddLine_TwoVertexConnect";
			this.m_tsbAddLine_TwoVertexConnect.Size = new System.Drawing.Size(44, 18);
			this.m_tsbAddLine_TwoVertexConnect.Text = "新增兩點連線";
			this.m_tsbAddLine_TwoVertexConnect.Click += new System.EventHandler(this.m_tsbAddLine_TwoVertexConnect_Click);
			// 
			// m_tsbManualTransform
			// 
			this.m_tsbManualTransform.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbManualTransform.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbManualTransform.Image")));
			this.m_tsbManualTransform.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbManualTransform.Name = "m_tsbManualTransform";
			this.m_tsbManualTransform.Size = new System.Drawing.Size(44, 18);
			this.m_tsbManualTransform.Text = "手動轉換";
			this.m_tsbManualTransform.Click += new System.EventHandler(this.m_tsbManualTransform_Click);
			// 
			// m_tsb3PntTransform
			// 
			this.m_tsb3PntTransform.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsb3PntTransform.Image = ((System.Drawing.Image)(resources.GetObject("m_tsb3PntTransform.Image")));
			this.m_tsb3PntTransform.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsb3PntTransform.Name = "m_tsb3PntTransform";
			this.m_tsb3PntTransform.Size = new System.Drawing.Size(44, 18);
			this.m_tsb3PntTransform.Text = "三點轉換";
			this.m_tsb3PntTransform.Click += new System.EventHandler(this.m_tsb3PntTransform_Click);
			// 
			// m_tsbAxisTransform
			// 
			this.m_tsbAxisTransform.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAxisTransform.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbAxisTransform.Image")));
			this.m_tsbAxisTransform.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAxisTransform.Name = "m_tsbAxisTransform";
			this.m_tsbAxisTransform.Size = new System.Drawing.Size(44, 18);
			this.m_tsbAxisTransform.Text = "軸向轉換";
			this.m_tsbAxisTransform.Click += new System.EventHandler(this.m_tsbAxisTransform_Click);
			// 
			// m_tscLevel2Container
			// 
			// 
			// m_tscLevel2Container.ContentPanel
			// 
			this.m_tscLevel2Container.ContentPanel.Controls.Add(this.m_tsAddPathSubFunc);
			this.m_tscLevel2Container.ContentPanel.Controls.Add(this.m_tsManualTrans);
			this.m_tscLevel2Container.ContentPanel.Size = new System.Drawing.Size(1064, 21);
			this.m_tscLevel2Container.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_tscLevel2Container.Location = new System.Drawing.Point(0, 70);
			this.m_tscLevel2Container.Name = "m_tscLevel2Container";
			this.m_tscLevel2Container.Size = new System.Drawing.Size(1064, 21);
			this.m_tscLevel2Container.TabIndex = 6;
			this.m_tscLevel2Container.Text = "toolStripContainer1";
			this.m_tscLevel2Container.TopToolStripPanelVisible = false;
			// 
			// m_tsAddPathSubFunc
			// 
			this.m_tsAddPathSubFunc.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsAddPathSubFunc.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.m_tsAddPathSubFunc.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbSelectD1ContFace,
            this.m_tsbSelPath_FreeBound,
            this.m_tsbSelPath_Manual});
			this.m_tsAddPathSubFunc.Location = new System.Drawing.Point(0, 0);
			this.m_tsAddPathSubFunc.Name = "m_tsAddPathSubFunc";
			this.m_tsAddPathSubFunc.Size = new System.Drawing.Size(1064, 21);
			this.m_tsAddPathSubFunc.TabIndex = 5;
			this.m_tsAddPathSubFunc.Text = "tsAddPathSubFunc";
			// 
			// m_tsbSelectD1ContFace
			// 
			this.m_tsbSelectD1ContFace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbSelectD1ContFace.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbSelectD1ContFace.Image")));
			this.m_tsbSelectD1ContFace.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbSelectD1ContFace.Name = "m_tsbSelectD1ContFace";
			this.m_tsbSelectD1ContFace.Size = new System.Drawing.Size(54, 18);
			this.m_tsbSelectD1ContFace.Text = "D1 連續";
			this.m_tsbSelectD1ContFace.Click += new System.EventHandler(this.m_tsbSelectD1ContFace_Click);
			// 
			// m_tsbSelPath_FreeBound
			// 
			this.m_tsbSelPath_FreeBound.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbSelPath_FreeBound.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbSelPath_FreeBound.Image")));
			this.m_tsbSelPath_FreeBound.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbSelPath_FreeBound.Name = "m_tsbSelPath_FreeBound";
			this.m_tsbSelPath_FreeBound.Size = new System.Drawing.Size(71, 18);
			this.m_tsbSelPath_FreeBound.Text = "所有面邊界";
			this.m_tsbSelPath_FreeBound.Click += new System.EventHandler(this.m_tsbSelPath_FreeBound_Click);
			// 
			// m_tsbSelPath_Manual
			// 
			this.m_tsbSelPath_Manual.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbSelPath_Manual.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbSelPath_Manual.Image")));
			this.m_tsbSelPath_Manual.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbSelPath_Manual.Name = "m_tsbSelPath_Manual";
			this.m_tsbSelPath_Manual.Size = new System.Drawing.Size(59, 18);
			this.m_tsbSelPath_Manual.Text = "手動選取";
			this.m_tsbSelPath_Manual.Click += new System.EventHandler(this.m_tsbSelPath_Manual_Click);
			// 
			// m_tsManualTrans
			// 
			this.m_tsManualTrans.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsManualTrans.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.m_tsManualTrans.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbManualTransPlane,
            this.m_tsbManualTransPlanePar,
            this.m_tsbManualTransAxial,
            this.m_tsbManualTransAxialPar,
            this.m_tsbManualTransPointCoincide});
			this.m_tsManualTrans.Location = new System.Drawing.Point(0, 0);
			this.m_tsManualTrans.Name = "m_tsManualTrans";
			this.m_tsManualTrans.Size = new System.Drawing.Size(1064, 21);
			this.m_tsManualTrans.TabIndex = 0;
			this.m_tsManualTrans.Text = "toolStrip1";
			// 
			// m_tsbManualTransPlane
			// 
			this.m_tsbManualTransPlane.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbManualTransPlane.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbManualTransPlane.Image")));
			this.m_tsbManualTransPlane.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbManualTransPlane.Name = "m_tsbManualTransPlane";
			this.m_tsbManualTransPlane.RightToLeftAutoMirrorImage = true;
			this.m_tsbManualTransPlane.Size = new System.Drawing.Size(47, 18);
			this.m_tsbManualTransPlane.Text = "面重合";
			this.m_tsbManualTransPlane.Click += new System.EventHandler(this.m_tsbManualTransPlane_Click);
			// 
			// m_tsbManualTransPlanePar
			// 
			this.m_tsbManualTransPlanePar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbManualTransPlanePar.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbManualTransPlanePar.Image")));
			this.m_tsbManualTransPlanePar.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbManualTransPlanePar.Name = "m_tsbManualTransPlanePar";
			this.m_tsbManualTransPlanePar.Size = new System.Drawing.Size(47, 18);
			this.m_tsbManualTransPlanePar.Text = "面平行";
			this.m_tsbManualTransPlanePar.Click += new System.EventHandler(this.m_tsbManualTransPlanePar_Click);
			// 
			// m_tsbManualTransAxial
			// 
			this.m_tsbManualTransAxial.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbManualTransAxial.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbManualTransAxial.Image")));
			this.m_tsbManualTransAxial.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbManualTransAxial.Name = "m_tsbManualTransAxial";
			this.m_tsbManualTransAxial.Size = new System.Drawing.Size(47, 18);
			this.m_tsbManualTransAxial.Text = "軸重合";
			this.m_tsbManualTransAxial.Click += new System.EventHandler(this.m_tsbManualTransAxial_Click);
			// 
			// m_tsbManualTransAxialPar
			// 
			this.m_tsbManualTransAxialPar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbManualTransAxialPar.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbManualTransAxialPar.Image")));
			this.m_tsbManualTransAxialPar.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbManualTransAxialPar.Name = "m_tsbManualTransAxialPar";
			this.m_tsbManualTransAxialPar.Size = new System.Drawing.Size(47, 18);
			this.m_tsbManualTransAxialPar.Text = "軸平行";
			this.m_tsbManualTransAxialPar.Click += new System.EventHandler(this.m_tsbManualTransAxialPar_Click);
			// 
			// m_tsbManualTransPointCoincide
			// 
			this.m_tsbManualTransPointCoincide.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbManualTransPointCoincide.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbManualTransPointCoincide.Image")));
			this.m_tsbManualTransPointCoincide.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbManualTransPointCoincide.Name = "m_tsbManualTransPointCoincide";
			this.m_tsbManualTransPointCoincide.Size = new System.Drawing.Size(47, 18);
			this.m_tsbManualTransPointCoincide.Text = "點重合";
			this.m_tsbManualTransPointCoincide.Click += new System.EventHandler(this.m_tsbManualTransPointCoincide_Click);
			// 
			// m_tscLevel3Container
			// 
			this.m_tscLevel3Container.BottomToolStripPanelVisible = false;
			// 
			// m_tscLevel3Container.ContentPanel
			// 
			this.m_tscLevel3Container.ContentPanel.Controls.Add(this.m_tsSelectPath);
			this.m_tscLevel3Container.ContentPanel.Size = new System.Drawing.Size(1064, 22);
			this.m_tscLevel3Container.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_tscLevel3Container.LeftToolStripPanelVisible = false;
			this.m_tscLevel3Container.Location = new System.Drawing.Point(0, 91);
			this.m_tscLevel3Container.Name = "m_tscLevel3Container";
			this.m_tscLevel3Container.RightToolStripPanelVisible = false;
			this.m_tscLevel3Container.Size = new System.Drawing.Size(1064, 22);
			this.m_tscLevel3Container.TabIndex = 8;
			this.m_tscLevel3Container.Text = "toolStripContainer1";
			this.m_tscLevel3Container.TopToolStripPanelVisible = false;
			// 
			// m_tsSelectPath
			// 
			this.m_tsSelectPath.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsSelectPath.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.m_tsSelectPath.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbManualSelectPathOK});
			this.m_tsSelectPath.Location = new System.Drawing.Point(0, 0);
			this.m_tsSelectPath.Name = "m_tsSelectPath";
			this.m_tsSelectPath.Size = new System.Drawing.Size(1064, 22);
			this.m_tsSelectPath.TabIndex = 1;
			this.m_tsSelectPath.Text = "SelectPath";
			// 
			// m_tsbManualSelectPathOK
			// 
			this.m_tsbManualSelectPathOK.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbManualSelectPathOK.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbManualSelectPathOK.Image")));
			this.m_tsbManualSelectPathOK.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbManualSelectPathOK.Name = "m_tsbManualSelectPathOK";
			this.m_tsbManualSelectPathOK.Size = new System.Drawing.Size(83, 19);
			this.m_tsbManualSelectPathOK.Text = "手動選取完成";
			this.m_tsbManualSelectPathOK.Click += new System.EventHandler(this.m_tsbManualSelectPathOK_Click);
			// 
			// m_panVNC
			// 
			this.m_panVNC.BackColor = System.Drawing.SystemColors.ButtonShadow;
			this.m_panBackGround.SetColumnSpan(this.m_panVNC, 2);
			this.m_panVNC.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panVNC.Location = new System.Drawing.Point(3, 939);
			this.m_panVNC.Name = "m_panVNC";
			this.m_panVNC.Size = new System.Drawing.Size(1058, 826);
			this.m_panVNC.TabIndex = 4;
			// 
			// m_panTreeView
			// 
			this.m_panTreeView.BackColor = System.Drawing.SystemColors.ButtonHighlight;
			this.m_panTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panTreeView.Location = new System.Drawing.Point(3, 3);
			this.m_panTreeView.Name = "m_panTreeView";
			this.m_panTreeView.Size = new System.Drawing.Size(153, 824);
			this.m_panTreeView.TabIndex = 0;
			// 
			// m_pnlLog
			// 
			this.m_pnlLog.AutoScroll = true;
			this.m_pnlLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(90)))), ((int)(((byte)(119)))));
			this.m_panBackGround.SetColumnSpan(this.m_pnlLog, 2);
			this.m_pnlLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_pnlLog.Location = new System.Drawing.Point(3, 833);
			this.m_pnlLog.Name = "m_pnlLog";
			this.m_pnlLog.Size = new System.Drawing.Size(1058, 100);
			this.m_pnlLog.TabIndex = 7;
			// 
			// m_panViewer
			// 
			this.m_panViewer.BackColor = System.Drawing.SystemColors.Window;
			this.m_panViewer.Controls.Add(this.m_tsViewerToolBar);
			this.m_panViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panViewer.Location = new System.Drawing.Point(162, 3);
			this.m_panViewer.Name = "m_panViewer";
			this.m_panViewer.Size = new System.Drawing.Size(899, 824);
			this.m_panViewer.TabIndex = 1;
			// 
			// m_tsViewerToolBar
			// 
			this.m_tsViewerToolBar.BackColor = System.Drawing.SystemColors.Window;
			this.m_tsViewerToolBar.Dock = System.Windows.Forms.DockStyle.Left;
			this.m_tsViewerToolBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.m_tsViewerToolBar.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.m_tsViewerToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbAXO,
            this.m_tsbXPos,
            this.m_tsbXNeg,
            this.m_tsbYPos,
            this.m_tsbYNeg,
            this.m_tsbZPos,
            this.m_tsbZNeg,
            this.toolStripSeparator1,
            this.m_tsbShowVec,
            this.m_tsbShowOrder,
            this.m_tsbShowOrientation,
            this.m_tsbShowTraverse});
			this.m_tsViewerToolBar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.m_tsViewerToolBar.Location = new System.Drawing.Point(0, 0);
			this.m_tsViewerToolBar.Name = "m_tsViewerToolBar";
			this.m_tsViewerToolBar.Size = new System.Drawing.Size(25, 824);
			this.m_tsViewerToolBar.TabIndex = 2;
			this.m_tsViewerToolBar.Text = "toolStrip1";
			// 
			// m_tsbAXO
			// 
			this.m_tsbAXO.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAXO.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbAXO.Image")));
			this.m_tsbAXO.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAXO.Name = "m_tsbAXO";
			this.m_tsbAXO.Size = new System.Drawing.Size(22, 24);
			this.m_tsbAXO.Text = "AXO";
			this.m_tsbAXO.ToolTipText = "AXO";
			this.m_tsbAXO.Click += new System.EventHandler(this.m_tsbAXO_Click);
			// 
			// m_tsbXPos
			// 
			this.m_tsbXPos.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbXPos.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbXPos.Image")));
			this.m_tsbXPos.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbXPos.Name = "m_tsbXPos";
			this.m_tsbXPos.Size = new System.Drawing.Size(22, 24);
			this.m_tsbXPos.Text = "X+";
			this.m_tsbXPos.ToolTipText = "X+";
			this.m_tsbXPos.Click += new System.EventHandler(this.m_tsbXPos_Click);
			// 
			// m_tsbXNeg
			// 
			this.m_tsbXNeg.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbXNeg.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbXNeg.Image")));
			this.m_tsbXNeg.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbXNeg.Name = "m_tsbXNeg";
			this.m_tsbXNeg.Size = new System.Drawing.Size(22, 24);
			this.m_tsbXNeg.Text = "X-";
			this.m_tsbXNeg.ToolTipText = "X-";
			this.m_tsbXNeg.Click += new System.EventHandler(this.m_tsbXNeg_Click);
			// 
			// m_tsbYPos
			// 
			this.m_tsbYPos.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbYPos.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbYPos.Image")));
			this.m_tsbYPos.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbYPos.Name = "m_tsbYPos";
			this.m_tsbYPos.Size = new System.Drawing.Size(22, 24);
			this.m_tsbYPos.Text = "Y+";
			this.m_tsbYPos.ToolTipText = "Y+";
			this.m_tsbYPos.Click += new System.EventHandler(this.m_tsbYPos_Click);
			// 
			// m_tsbYNeg
			// 
			this.m_tsbYNeg.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbYNeg.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbYNeg.Image")));
			this.m_tsbYNeg.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbYNeg.Name = "m_tsbYNeg";
			this.m_tsbYNeg.Size = new System.Drawing.Size(22, 24);
			this.m_tsbYNeg.Text = "Y-";
			this.m_tsbYNeg.ToolTipText = "Y-";
			this.m_tsbYNeg.Click += new System.EventHandler(this.m_tsbYNeg_Click);
			// 
			// m_tsbZPos
			// 
			this.m_tsbZPos.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbZPos.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbZPos.Image")));
			this.m_tsbZPos.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbZPos.Name = "m_tsbZPos";
			this.m_tsbZPos.Size = new System.Drawing.Size(22, 24);
			this.m_tsbZPos.Text = "Z+";
			this.m_tsbZPos.ToolTipText = "Z+";
			this.m_tsbZPos.Click += new System.EventHandler(this.m_tsbZPos_Click);
			// 
			// m_tsbZNeg
			// 
			this.m_tsbZNeg.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbZNeg.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbZNeg.Image")));
			this.m_tsbZNeg.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbZNeg.Name = "m_tsbZNeg";
			this.m_tsbZNeg.Size = new System.Drawing.Size(22, 24);
			this.m_tsbZNeg.Text = "Z-";
			this.m_tsbZNeg.ToolTipText = "Z-";
			this.m_tsbZNeg.Click += new System.EventHandler(this.m_tsbZNeg_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(22, 6);
			// 
			// m_tsbShowVec
			// 
			this.m_tsbShowVec.Checked = true;
			this.m_tsbShowVec.CheckOnClick = true;
			this.m_tsbShowVec.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tsbShowVec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbShowVec.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbShowVec.Image")));
			this.m_tsbShowVec.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbShowVec.Name = "m_tsbShowVec";
			this.m_tsbShowVec.Size = new System.Drawing.Size(22, 24);
			this.m_tsbShowVec.Text = "顯示姿態";
			this.m_tsbShowVec.CheckedChanged += new System.EventHandler(this.m_tsbShowVec_CheckedChanged);
			// 
			// m_tsbShowOrder
			// 
			this.m_tsbShowOrder.Checked = true;
			this.m_tsbShowOrder.CheckOnClick = true;
			this.m_tsbShowOrder.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tsbShowOrder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbShowOrder.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbShowOrder.Image")));
			this.m_tsbShowOrder.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbShowOrder.Name = "m_tsbShowOrder";
			this.m_tsbShowOrder.Size = new System.Drawing.Size(22, 24);
			this.m_tsbShowOrder.Text = "顯示順序";
			this.m_tsbShowOrder.CheckedChanged += new System.EventHandler(this.m_tsbShowOrder_CheckedChanged);
			// 
			// m_tsbShowOrientation
			// 
			this.m_tsbShowOrientation.Checked = true;
			this.m_tsbShowOrientation.CheckOnClick = true;
			this.m_tsbShowOrientation.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tsbShowOrientation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbShowOrientation.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbShowOrientation.Image")));
			this.m_tsbShowOrientation.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbShowOrientation.Name = "m_tsbShowOrientation";
			this.m_tsbShowOrientation.Size = new System.Drawing.Size(22, 24);
			this.m_tsbShowOrientation.Text = "顯示路徑方向";
			this.m_tsbShowOrientation.CheckedChanged += new System.EventHandler(this.m_tsbShowOrientation_CheckedChanged);
			// 
			// m_tsbShowTraverse
			// 
			this.m_tsbShowTraverse.Checked = true;
			this.m_tsbShowTraverse.CheckOnClick = true;
			this.m_tsbShowTraverse.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tsbShowTraverse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbShowTraverse.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbShowTraverse.Image")));
			this.m_tsbShowTraverse.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbShowTraverse.Name = "m_tsbShowTraverse";
			this.m_tsbShowTraverse.Size = new System.Drawing.Size(22, 24);
			this.m_tsbShowTraverse.Text = "顯示抬下刀路徑";
			this.m_tsbShowTraverse.CheckedChanged += new System.EventHandler(this.m_tsbShowTraverse_CheckedChanged);
			// 
			// m_panBackGround
			// 
			this.m_panBackGround.ColumnCount = 2;
			this.m_panBackGround.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
			this.m_panBackGround.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F));
			this.m_panBackGround.Controls.Add(this.m_panVNC, 0, 2);
			this.m_panBackGround.Controls.Add(this.m_panTreeView, 0, 0);
			this.m_panBackGround.Controls.Add(this.m_pnlLog, 0, 1);
			this.m_panBackGround.Controls.Add(this.m_panViewer, 1, 0);
			this.m_panBackGround.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panBackGround.Location = new System.Drawing.Point(0, 113);
			this.m_panBackGround.Name = "m_panBackGround";
			this.m_panBackGround.RowCount = 3;
			this.m_panBackGround.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 47F));
			this.m_panBackGround.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6F));
			this.m_panBackGround.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 47F));
			this.m_panBackGround.Size = new System.Drawing.Size(1064, 1768);
			this.m_panBackGround.TabIndex = 9;
			// 
			// StartupForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(1064, 1881);
			this.Controls.Add(this.m_panBackGround);
			this.Controls.Add(this.m_tscLevel3Container);
			this.Controls.Add(this.m_tscLevel2Container);
			this.Controls.Add(this.m_tscLevel1Container);
			this.Controls.Add(this.m_msMode);
			this.MainMenuStrip = this.m_msMode;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "StartupForm";
			this.Text = "五軸切";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StartupForm_FormClosing);
			this.Resize += new System.EventHandler(this.StartupForm_Resize);
			this.m_msMode.ResumeLayout(false);
			this.m_msMode.PerformLayout();
			this.m_tsCAMFunction.ResumeLayout(false);
			this.m_tsCAMFunction.PerformLayout();
			this.m_tscLevel1Container.ContentPanel.ResumeLayout(false);
			this.m_tscLevel1Container.ContentPanel.PerformLayout();
			this.m_tscLevel1Container.ResumeLayout(false);
			this.m_tscLevel1Container.PerformLayout();
			this.m_tsFileFunction.ResumeLayout(false);
			this.m_tsFileFunction.PerformLayout();
			this.m_tsCADFunction.ResumeLayout(false);
			this.m_tsCADFunction.PerformLayout();
			this.m_tscLevel2Container.ContentPanel.ResumeLayout(false);
			this.m_tscLevel2Container.ContentPanel.PerformLayout();
			this.m_tscLevel2Container.ResumeLayout(false);
			this.m_tscLevel2Container.PerformLayout();
			this.m_tsAddPathSubFunc.ResumeLayout(false);
			this.m_tsAddPathSubFunc.PerformLayout();
			this.m_tsManualTrans.ResumeLayout(false);
			this.m_tsManualTrans.PerformLayout();
			this.m_tscLevel3Container.ContentPanel.ResumeLayout(false);
			this.m_tscLevel3Container.ContentPanel.PerformLayout();
			this.m_tscLevel3Container.ResumeLayout(false);
			this.m_tscLevel3Container.PerformLayout();
			this.m_tsSelectPath.ResumeLayout(false);
			this.m_tsSelectPath.PerformLayout();
			this.m_panViewer.ResumeLayout(false);
			this.m_panViewer.PerformLayout();
			this.m_tsViewerToolBar.ResumeLayout(false);
			this.m_tsViewerToolBar.PerformLayout();
			this.m_panBackGround.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.MenuStrip m_msMode;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiFile;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiCAD;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiCAM;
		private System.Windows.Forms.ToolStrip m_tsCAMFunction;
		private System.Windows.Forms.ToolStripButton m_tsbAddPath;
		private System.Windows.Forms.ToolStripButton m_tsbDeletePath;
		private System.Windows.Forms.ToolStripButton m_tsbStartPoint;
		private System.Windows.Forms.ToolStripButton m_tsbReverse;
		private System.Windows.Forms.ToolStripButton m_tsbSetLead;
		private System.Windows.Forms.ToolStripButton m_tsbOverCut;
		private System.Windows.Forms.ToolStripButton m_tsbToolVecReverse;
		private System.Windows.Forms.ToolStripButton m_tsbToolVec;
		private System.Windows.Forms.ToolStripButton m_tsbMoveUp;
		private System.Windows.Forms.ToolStripButton m_tsbMoveDown;
		private System.Windows.Forms.ToolStripButton m_tsbAutoOrder;
		private System.Windows.Forms.ToolStripContainer m_tscLevel1Container;
		private System.Windows.Forms.ToolStripContainer m_tscLevel2Container;
		private System.Windows.Forms.ToolStrip m_tsCADFunction;
		private System.Windows.Forms.ToolStripContainer m_tscLevel3Container;
		private System.Windows.Forms.ToolStrip m_tsManualTrans;
		private System.Windows.Forms.ToolStripButton m_tsbManualTransPlane;
		private System.Windows.Forms.ToolStripButton m_tsbManualTransAxial;
		private System.Windows.Forms.ToolStripButton m_tsbManualTransAxialPar;
		private System.Windows.Forms.ToolStripButton m_tsbManualTransPointCoincide;
		private System.Windows.Forms.ToolStrip m_tsAddPathSubFunc;
		private System.Windows.Forms.ToolStripButton m_tsbSelectD1ContFace;
		private System.Windows.Forms.ToolStrip m_tsSelectPath;
		private System.Windows.Forms.ToolStripButton m_tsbManualSelectPathOK;
		private System.Windows.Forms.ToolStripButton m_tsbSetTraverseData;
		private System.Windows.Forms.ToolStripButton m_tsbConvertNC;
		private System.Windows.Forms.ToolStripButton m_tsbAddPoint_AcrCenter;
		private System.Windows.Forms.ToolStripButton m_tsbAddPoint_EdgeCenter;
		private System.Windows.Forms.ToolStripButton m_tsbAddPoint_PointCenter;
		private System.Windows.Forms.ToolStripButton m_tsbAddLine_TwoVertexConnect;
		private System.Windows.Forms.ToolStripButton m_tsbManualTransform;
		private System.Windows.Forms.ToolStripButton m_tsb3PntTransform;
		private System.Windows.Forms.ToolStripButton m_tsbAxisTransform;
		private System.Windows.Forms.ToolStrip m_tsFileFunction;
		private System.Windows.Forms.ToolStripButton m_tsbImport3DFile;
		private System.Windows.Forms.ToolStripButton m_tsbReadProjectFile;
		private System.Windows.Forms.ToolStripButton m_tsbSaveProjectFile;
		private System.Windows.Forms.ToolStripButton m_tsbSelPath_FreeBound;
		private System.Windows.Forms.ToolStripButton m_tsbSelPath_Manual;
		private System.Windows.Forms.ToolStripButton m_tsbEntryAndExitSetting;
		private System.Windows.Forms.ToolStripButton m_tsbSewPart;
		private System.Windows.Forms.ToolStripButton m_tsbManualTransPlanePar;
		private System.Windows.Forms.ToolStripButton m_tsbPatternSetting;
		private System.Windows.Forms.ToolStripButton m_tsbFixedToolVec;
		private System.Windows.Forms.Panel m_panVNC;
		private System.Windows.Forms.Panel m_panTreeView;
		private System.Windows.Forms.Panel m_pnlLog;
		private System.Windows.Forms.Panel m_panViewer;
		private System.Windows.Forms.ToolStrip m_tsViewerToolBar;
		private System.Windows.Forms.ToolStripButton m_tsbAXO;
		private System.Windows.Forms.ToolStripButton m_tsbXPos;
		private System.Windows.Forms.ToolStripButton m_tsbXNeg;
		private System.Windows.Forms.ToolStripButton m_tsbYPos;
		private System.Windows.Forms.ToolStripButton m_tsbYNeg;
		private System.Windows.Forms.ToolStripButton m_tsbZPos;
		private System.Windows.Forms.ToolStripButton m_tsbZNeg;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton m_tsbShowVec;
		private System.Windows.Forms.ToolStripButton m_tsbShowOrder;
		private System.Windows.Forms.ToolStripButton m_tsbShowOrientation;
		private System.Windows.Forms.ToolStripButton m_tsbShowTraverse;
		private System.Windows.Forms.TableLayoutPanel m_panBackGround;
		private System.Windows.Forms.ToolStripMenuItem m_tsbSetting;
		private System.Windows.Forms.ToolStripMenuItem m_tsbVNCConnection;
	}
}

