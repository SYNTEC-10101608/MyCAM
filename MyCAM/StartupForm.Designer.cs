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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartupForm));
			this.m_msMode = new System.Windows.Forms.MenuStrip();
			this.m_tsmiFile = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiCAD = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiCAM = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsCAMFunction = new System.Windows.Forms.ToolStrip();
			this.m_tsbAddPath = new System.Windows.Forms.ToolStripButton();
			this.m_tsbDeletePath = new System.Windows.Forms.ToolStripButton();
			this.m_tsbStartPoint = new System.Windows.Forms.ToolStripButton();
			this.m_tsbReverse = new System.Windows.Forms.ToolStripButton();
			this.m_tsbSetLead = new System.Windows.Forms.ToolStripButton();
			this.m_tsbFlipLead = new System.Windows.Forms.ToolStripButton();
			this.m_tsbOverCut = new System.Windows.Forms.ToolStripButton();
			this.m_tsbTooVecReverse = new System.Windows.Forms.ToolStripButton();
			this.m_tsbToolVec = new System.Windows.Forms.ToolStripButton();
			this.m_tsbMoveUp = new System.Windows.Forms.ToolStripButton();
			this.m_tsbMoveDown = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAutoOrder = new System.Windows.Forms.ToolStripButton();
			this.m_tsbTraverseParamSetting = new System.Windows.Forms.ToolStripButton();
			this.m_tsbConvertNC = new System.Windows.Forms.ToolStripButton();
			this.m_panBackGround = new System.Windows.Forms.Panel();
			this.m_tsViewerToolBar = new System.Windows.Forms.ToolStrip();
			this.m_tsbShowVec = new System.Windows.Forms.ToolStripButton();
			this.m_tsbShowOrder = new System.Windows.Forms.ToolStripButton();
			this.m_tsbShowOrientation = new System.Windows.Forms.ToolStripButton();
			this.m_tsbShowTraverse = new System.Windows.Forms.ToolStripButton();
			this.m_panViewer = new System.Windows.Forms.Panel();
			this.m_panTreeView = new System.Windows.Forms.Panel();
			this.m_TreeView = new System.Windows.Forms.TreeView();
			this.m_tscLevel1Container = new System.Windows.Forms.ToolStripContainer();
			this.m_tsFileFunction = new System.Windows.Forms.ToolStrip();
			this.m_tsbImport3DFile = new System.Windows.Forms.ToolStripButton();
			this.m_tsbReadProjectFile = new System.Windows.Forms.ToolStripButton();
			this.m_tsbSaveProjectFile = new System.Windows.Forms.ToolStripButton();
			this.m_tsCADFunction = new System.Windows.Forms.ToolStrip();
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
			this.m_tsbManualTransAxial = new System.Windows.Forms.ToolStripButton();
			this.m_tsbManualTransAxialPar = new System.Windows.Forms.ToolStripButton();
			this.m_tsbManualTransPointCoincide = new System.Windows.Forms.ToolStripButton();
			this.m_pnlLog = new System.Windows.Forms.Panel();
			this.m_tscLevel3Container = new System.Windows.Forms.ToolStripContainer();
			this.m_tsSelectPath = new System.Windows.Forms.ToolStrip();
			this.m_tsbManualSelectPathOK = new System.Windows.Forms.ToolStripButton();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
			this.m_tsbEntryAndExitSetting = new System.Windows.Forms.ToolStripButton();
			this.m_msMode.SuspendLayout();
			this.m_tsCAMFunction.SuspendLayout();
			this.m_panBackGround.SuspendLayout();
			this.m_tsViewerToolBar.SuspendLayout();
			this.m_panTreeView.SuspendLayout();
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
			this.SuspendLayout();
			// 
			// m_msMode
			// 
			this.m_msMode.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.m_msMode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiFile,
            this.m_tsmiCAD,
            this.m_tsmiCAM});
			this.m_msMode.Location = new System.Drawing.Point(0, 0);
			this.m_msMode.Name = "m_msMode";
			this.m_msMode.Size = new System.Drawing.Size(1065, 24);
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
            this.m_tsbFlipLead,
            this.m_tsbOverCut,
            this.m_tsbTooVecReverse,
            this.m_tsbToolVec,
            this.m_tsbMoveUp,
            this.m_tsbMoveDown,
            this.m_tsbAutoOrder,
            this.m_tsbTraverseParamSetting,
            this.m_tsbEntryAndExitSetting,
            this.m_tsbConvertNC});
			this.m_tsCAMFunction.Location = new System.Drawing.Point(0, 0);
			this.m_tsCAMFunction.Name = "m_tsCAMFunction";
			this.m_tsCAMFunction.Size = new System.Drawing.Size(1065, 21);
			this.m_tsCAMFunction.TabIndex = 2;
			this.m_tsCAMFunction.Text = "toolStrip1";
			// 
			// m_tsbAddPath
			// 
			this.m_tsbAddPath.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAddPath.Image = global::MyCAM.Properties.Resources.AddPath;
			this.m_tsbAddPath.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddPath.Name = "m_tsbAddPath";
			this.m_tsbAddPath.Size = new System.Drawing.Size(44, 18);
			this.m_tsbAddPath.Text = "新增路徑";
			this.m_tsbAddPath.Click += new System.EventHandler(this.m_tsbAddPath_Click);
			// 
			// m_tsbDeletePath
			// 
			this.m_tsbDeletePath.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbDeletePath.Image = global::MyCAM.Properties.Resources.DeletePath;
			this.m_tsbDeletePath.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbDeletePath.Name = "m_tsbDeletePath";
			this.m_tsbDeletePath.Size = new System.Drawing.Size(44, 18);
			this.m_tsbDeletePath.Text = "刪除路徑";
			this.m_tsbDeletePath.Click += new System.EventHandler(this.m_tsbDeletePath_Click);
			// 
			// m_tsbStartPoint
			// 
			this.m_tsbStartPoint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbStartPoint.Image = global::MyCAM.Properties.Resources.StartPointSetting;
			this.m_tsbStartPoint.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbStartPoint.Name = "m_tsbStartPoint";
			this.m_tsbStartPoint.Size = new System.Drawing.Size(44, 18);
			this.m_tsbStartPoint.Text = "起點";
			this.m_tsbStartPoint.Click += new System.EventHandler(this.m_tsbStartPoint_Click);
			// 
			// m_tsbReverse
			// 
			this.m_tsbReverse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbReverse.Image = global::MyCAM.Properties.Resources.Reverse;
			this.m_tsbReverse.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbReverse.Name = "m_tsbReverse";
			this.m_tsbReverse.Size = new System.Drawing.Size(44, 18);
			this.m_tsbReverse.Text = "反向";
			this.m_tsbReverse.Click += new System.EventHandler(this.m_tsbReverse_Click);
			// 
			// m_tsbSetLead
			// 
			this.m_tsbSetLead.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbSetLead.Image = global::MyCAM.Properties.Resources.Lead;
			this.m_tsbSetLead.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbSetLead.Name = "m_tsbSetLead";
			this.m_tsbSetLead.Size = new System.Drawing.Size(44, 18);
			this.m_tsbSetLead.Text = "引線";
			this.m_tsbSetLead.Click += new System.EventHandler(this.m_tsbSetLead_Click);
			// 
			// m_tsbFlipLead
			// 
			this.m_tsbFlipLead.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbFlipLead.Image = global::MyCAM.Properties.Resources.FlipLead;
			this.m_tsbFlipLead.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbFlipLead.Name = "m_tsbFlipLead";
			this.m_tsbFlipLead.Size = new System.Drawing.Size(44, 18);
			this.m_tsbFlipLead.Text = "更換引線方向";
			this.m_tsbFlipLead.Click += new System.EventHandler(this.m_tsbFlipLead_Click);
			// 
			// m_tsbOverCut
			// 
			this.m_tsbOverCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbOverCut.Image = global::MyCAM.Properties.Resources.Overcut;
			this.m_tsbOverCut.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbOverCut.Name = "m_tsbOverCut";
			this.m_tsbOverCut.Size = new System.Drawing.Size(44, 18);
			this.m_tsbOverCut.Text = "過切";
			this.m_tsbOverCut.Click += new System.EventHandler(this.m_tsbOverCut_Click);
			// 
			// m_tsbTooVecReverse
			// 
			this.m_tsbTooVecReverse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbTooVecReverse.Image = ((System.Drawing.Image)(resources.GetObject("m_tsbTooVecReverse.Image")));
			this.m_tsbTooVecReverse.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbTooVecReverse.Name = "m_tsbTooVecReverse";
			this.m_tsbTooVecReverse.Size = new System.Drawing.Size(44, 18);
			this.m_tsbTooVecReverse.Text = "姿態反向";
			this.m_tsbTooVecReverse.Click += new System.EventHandler(this.m_tsbTooVecReverse_Click);
			// 
			// m_tsbToolVec
			// 
			this.m_tsbToolVec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbToolVec.Image = global::MyCAM.Properties.Resources.ToolVec;
			this.m_tsbToolVec.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbToolVec.Name = "m_tsbToolVec";
			this.m_tsbToolVec.Size = new System.Drawing.Size(44, 18);
			this.m_tsbToolVec.Text = "姿態";
			this.m_tsbToolVec.Click += new System.EventHandler(this.m_tsbToolVec_Click);
			// 
			// m_tsbMoveUp
			// 
			this.m_tsbMoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbMoveUp.Image = global::MyCAM.Properties.Resources.MoveUp;
			this.m_tsbMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbMoveUp.Name = "m_tsbMoveUp";
			this.m_tsbMoveUp.Size = new System.Drawing.Size(44, 18);
			this.m_tsbMoveUp.Text = "順序往前";
			this.m_tsbMoveUp.Click += new System.EventHandler(this.m_tsbMoveUp_Click);
			// 
			// m_tsbMoveDown
			// 
			this.m_tsbMoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbMoveDown.Image = global::MyCAM.Properties.Resources.MoveDown;
			this.m_tsbMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbMoveDown.Name = "m_tsbMoveDown";
			this.m_tsbMoveDown.Size = new System.Drawing.Size(44, 18);
			this.m_tsbMoveDown.Text = "順序往後";
			this.m_tsbMoveDown.Click += new System.EventHandler(this.m_tsbMoveDown_Click);
			// 
			// m_tsbAutoOrder
			// 
			this.m_tsbAutoOrder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAutoOrder.Image = global::MyCAM.Properties.Resources.AutoOrder;
			this.m_tsbAutoOrder.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAutoOrder.Name = "m_tsbAutoOrder";
			this.m_tsbAutoOrder.Size = new System.Drawing.Size(44, 18);
			this.m_tsbAutoOrder.Text = "自動排序";
			this.m_tsbAutoOrder.Click += new System.EventHandler(this.m_tsbAutoOrder_Click);
			// 
			// m_tsbTraverseParamSetting
			// 
			this.m_tsbTraverseParamSetting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbTraverseParamSetting.Image = global::MyCAM.Properties.Resources.TraverseParam;
			this.m_tsbTraverseParamSetting.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbTraverseParamSetting.Name = "m_tsbTraverseParamSetting";
			this.m_tsbTraverseParamSetting.Size = new System.Drawing.Size(44, 18);
			this.m_tsbTraverseParamSetting.Text = "空移參數設定";
			this.m_tsbTraverseParamSetting.Click += new System.EventHandler(this.m_tsbTraverseParamSetting_Click);
			// 
			// m_tsbConvertNC
			// 
			this.m_tsbConvertNC.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbConvertNC.Image = global::MyCAM.Properties.Resources.NCFile;
			this.m_tsbConvertNC.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbConvertNC.Name = "m_tsbConvertNC";
			this.m_tsbConvertNC.Size = new System.Drawing.Size(44, 18);
			this.m_tsbConvertNC.Text = "轉出NC";
			this.m_tsbConvertNC.Click += new System.EventHandler(this.m_tsbConvertNC_Click);
			// 
			// m_panBackGround
			// 
			this.m_panBackGround.Controls.Add(this.m_tsViewerToolBar);
			this.m_panBackGround.Controls.Add(this.m_panViewer);
			this.m_panBackGround.Controls.Add(this.m_panTreeView);
			this.m_panBackGround.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panBackGround.Location = new System.Drawing.Point(0, 113);
			this.m_panBackGround.Name = "m_panBackGround";
			this.m_panBackGround.Size = new System.Drawing.Size(1065, 705);
			this.m_panBackGround.TabIndex = 4;
			// 
			// m_tsViewerToolBar
			// 
			this.m_tsViewerToolBar.BackColor = System.Drawing.SystemColors.Window;
			this.m_tsViewerToolBar.Dock = System.Windows.Forms.DockStyle.Left;
			this.m_tsViewerToolBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.m_tsViewerToolBar.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.m_tsViewerToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbShowVec,
            this.m_tsbShowOrder,
            this.m_tsbShowOrientation,
            this.m_tsbShowTraverse});
			this.m_tsViewerToolBar.Location = new System.Drawing.Point(165, 0);
			this.m_tsViewerToolBar.Name = "m_tsViewerToolBar";
			this.m_tsViewerToolBar.Size = new System.Drawing.Size(25, 705);
			this.m_tsViewerToolBar.TabIndex = 2;
			this.m_tsViewerToolBar.Text = "toolStrip1";
			// 
			// m_tsbShowVec
			// 
			this.m_tsbShowVec.Checked = true;
			this.m_tsbShowVec.CheckOnClick = true;
			this.m_tsbShowVec.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tsbShowVec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbShowVec.Image = global::MyCAM.Properties.Resources.ToolVec;
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
			this.m_tsbShowOrder.Image = global::MyCAM.Properties.Resources.AutoOrder;
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
			this.m_tsbShowOrientation.Image = global::MyCAM.Properties.Resources.Reverse;
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
			this.m_tsbShowTraverse.Image = global::MyCAM.Properties.Resources.TraverseParam;
			this.m_tsbShowTraverse.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbShowTraverse.Name = "m_tsbShowTraverse";
			this.m_tsbShowTraverse.Size = new System.Drawing.Size(22, 24);
			this.m_tsbShowTraverse.Text = "顯示抬下刀路徑";
			this.m_tsbShowTraverse.CheckedChanged += new System.EventHandler(this.m_tsbShowTraverse_CheckedChanged);
			// 
			// m_panViewer
			// 
			this.m_panViewer.BackColor = System.Drawing.SystemColors.Window;
			this.m_panViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panViewer.Location = new System.Drawing.Point(165, 0);
			this.m_panViewer.Name = "m_panViewer";
			this.m_panViewer.Size = new System.Drawing.Size(900, 705);
			this.m_panViewer.TabIndex = 1;
			// 
			// m_panTreeView
			// 
			this.m_panTreeView.BackColor = System.Drawing.SystemColors.ButtonHighlight;
			this.m_panTreeView.Controls.Add(this.m_TreeView);
			this.m_panTreeView.Dock = System.Windows.Forms.DockStyle.Left;
			this.m_panTreeView.Location = new System.Drawing.Point(0, 0);
			this.m_panTreeView.Name = "m_panTreeView";
			this.m_panTreeView.Size = new System.Drawing.Size(165, 705);
			this.m_panTreeView.TabIndex = 0;
			// 
			// m_TreeView
			// 
			this.m_TreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_TreeView.Location = new System.Drawing.Point(0, 0);
			this.m_TreeView.Name = "m_TreeView";
			this.m_TreeView.Size = new System.Drawing.Size(165, 705);
			this.m_TreeView.TabIndex = 0;
			// 
			// m_tscLevel1Container
			// 
			// 
			// m_tscLevel1Container.ContentPanel
			// 
			this.m_tscLevel1Container.ContentPanel.Controls.Add(this.m_tsCAMFunction);
			this.m_tscLevel1Container.ContentPanel.Controls.Add(this.m_tsFileFunction);
			this.m_tscLevel1Container.ContentPanel.Controls.Add(this.m_tsCADFunction);
			this.m_tscLevel1Container.ContentPanel.Size = new System.Drawing.Size(1065, 21);
			this.m_tscLevel1Container.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_tscLevel1Container.Location = new System.Drawing.Point(0, 24);
			this.m_tscLevel1Container.Name = "m_tscLevel1Container";
			this.m_tscLevel1Container.Size = new System.Drawing.Size(1065, 46);
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
			this.m_tsFileFunction.Size = new System.Drawing.Size(1065, 21);
			this.m_tsFileFunction.TabIndex = 4;
			this.m_tsFileFunction.Text = "匯入3D模型";
			// 
			// m_tsbImport3DFile
			// 
			this.m_tsbImport3DFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbImport3DFile.Image = global::MyCAM.Properties.Resources.Import3DFile;
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
            this.m_tsbAddPoint_AcrCenter,
            this.m_tsbAddPoint_EdgeCenter,
            this.m_tsbAddPoint_PointCenter,
            this.m_tsbAddLine_TwoVertexConnect,
            this.m_tsbManualTransform,
            this.m_tsb3PntTransform,
            this.m_tsbAxisTransform});
			this.m_tsCADFunction.Location = new System.Drawing.Point(0, 0);
			this.m_tsCADFunction.Name = "m_tsCADFunction";
			this.m_tsCADFunction.Size = new System.Drawing.Size(1065, 21);
			this.m_tsCADFunction.TabIndex = 3;
			this.m_tsCADFunction.Text = "CAD功能";
			// 
			// m_tsbAddPoint_AcrCenter
			// 
			this.m_tsbAddPoint_AcrCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAddPoint_AcrCenter.Image = global::MyCAM.Properties.Resources.AcrCenter;
			this.m_tsbAddPoint_AcrCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddPoint_AcrCenter.Name = "m_tsbAddPoint_AcrCenter";
			this.m_tsbAddPoint_AcrCenter.Size = new System.Drawing.Size(44, 18);
			this.m_tsbAddPoint_AcrCenter.Text = "新增圓弧中心";
			this.m_tsbAddPoint_AcrCenter.Click += new System.EventHandler(this.m_tsbAddPoint_AcrCenter_Click);
			// 
			// m_tsbAddPoint_EdgeCenter
			// 
			this.m_tsbAddPoint_EdgeCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAddPoint_EdgeCenter.Image = global::MyCAM.Properties.Resources.EdgeCenter;
			this.m_tsbAddPoint_EdgeCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddPoint_EdgeCenter.Name = "m_tsbAddPoint_EdgeCenter";
			this.m_tsbAddPoint_EdgeCenter.Size = new System.Drawing.Size(44, 18);
			this.m_tsbAddPoint_EdgeCenter.Text = "新增線段中心";
			this.m_tsbAddPoint_EdgeCenter.Click += new System.EventHandler(this.m_tsbAddPoint_EdgeCenter_Click);
			// 
			// m_tsbAddPoint_PointCenter
			// 
			this.m_tsbAddPoint_PointCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAddPoint_PointCenter.Image = global::MyCAM.Properties.Resources.PointCenter;
			this.m_tsbAddPoint_PointCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddPoint_PointCenter.Name = "m_tsbAddPoint_PointCenter";
			this.m_tsbAddPoint_PointCenter.Size = new System.Drawing.Size(44, 18);
			this.m_tsbAddPoint_PointCenter.Text = "新增兩點中心";
			this.m_tsbAddPoint_PointCenter.Click += new System.EventHandler(this.m_tsbAddPoint_PointCenter_Click);
			// 
			// m_tsbAddLine_TwoVertexConnect
			// 
			this.m_tsbAddLine_TwoVertexConnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAddLine_TwoVertexConnect.Image = global::MyCAM.Properties.Resources.TwoVertexConnect;
			this.m_tsbAddLine_TwoVertexConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddLine_TwoVertexConnect.Name = "m_tsbAddLine_TwoVertexConnect";
			this.m_tsbAddLine_TwoVertexConnect.Size = new System.Drawing.Size(44, 18);
			this.m_tsbAddLine_TwoVertexConnect.Text = "新增兩點連線";
			this.m_tsbAddLine_TwoVertexConnect.Click += new System.EventHandler(this.m_tsbAddLine_TwoVertexConnect_Click);
			// 
			// m_tsbManualTransform
			// 
			this.m_tsbManualTransform.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbManualTransform.Image = global::MyCAM.Properties.Resources.ManualTransform;
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
			this.m_tscLevel2Container.ContentPanel.Size = new System.Drawing.Size(1065, 21);
			this.m_tscLevel2Container.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_tscLevel2Container.Location = new System.Drawing.Point(0, 70);
			this.m_tscLevel2Container.Name = "m_tscLevel2Container";
			this.m_tscLevel2Container.Size = new System.Drawing.Size(1065, 21);
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
			this.m_tsAddPathSubFunc.Size = new System.Drawing.Size(1065, 21);
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
            this.m_tsbManualTransAxial,
            this.m_tsbManualTransAxialPar,
            this.m_tsbManualTransPointCoincide});
			this.m_tsManualTrans.Location = new System.Drawing.Point(0, 0);
			this.m_tsManualTrans.Name = "m_tsManualTrans";
			this.m_tsManualTrans.Size = new System.Drawing.Size(1065, 21);
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
			// m_pnlLog
			// 
			this.m_pnlLog.AutoScroll = true;
			this.m_pnlLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(90)))), ((int)(((byte)(119)))));
			this.m_pnlLog.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.m_pnlLog.Location = new System.Drawing.Point(0, 818);
			this.m_pnlLog.Name = "m_pnlLog";
			this.m_pnlLog.Size = new System.Drawing.Size(1065, 102);
			this.m_pnlLog.TabIndex = 7;
			// 
			// m_tscLevel3Container
			// 
			this.m_tscLevel3Container.BottomToolStripPanelVisible = false;
			// 
			// m_tscLevel3Container.ContentPanel
			// 
			this.m_tscLevel3Container.ContentPanel.Controls.Add(this.m_tsSelectPath);
			this.m_tscLevel3Container.ContentPanel.Size = new System.Drawing.Size(1065, 22);
			this.m_tscLevel3Container.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_tscLevel3Container.LeftToolStripPanelVisible = false;
			this.m_tscLevel3Container.Location = new System.Drawing.Point(0, 91);
			this.m_tscLevel3Container.Name = "m_tscLevel3Container";
			this.m_tscLevel3Container.RightToolStripPanelVisible = false;
			this.m_tscLevel3Container.Size = new System.Drawing.Size(1065, 22);
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
			this.m_tsSelectPath.Size = new System.Drawing.Size(1065, 22);
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
			// StartupForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(1065, 920);
			this.Controls.Add(this.m_panBackGround);
			this.Controls.Add(this.m_tscLevel3Container);
			this.Controls.Add(this.m_tscLevel2Container);
			this.Controls.Add(this.m_tscLevel1Container);
			this.Controls.Add(this.m_msMode);
			this.Controls.Add(this.m_pnlLog);
			this.MainMenuStrip = this.m_msMode;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "StartupForm";
			this.Text = "五軸切";
			this.m_msMode.ResumeLayout(false);
			this.m_msMode.PerformLayout();
			this.m_tsCAMFunction.ResumeLayout(false);
			this.m_tsCAMFunction.PerformLayout();
			this.m_panBackGround.ResumeLayout(false);
			this.m_panBackGround.PerformLayout();
			this.m_tsViewerToolBar.ResumeLayout(false);
			this.m_tsViewerToolBar.PerformLayout();
			this.m_panTreeView.ResumeLayout(false);
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
		private System.Windows.Forms.Panel m_panBackGround;
		private System.Windows.Forms.Panel m_panTreeView;
		private System.Windows.Forms.Panel m_panViewer;
		private System.Windows.Forms.TreeView m_TreeView;
		private System.Windows.Forms.ToolStripButton m_tsbDeletePath;
		private System.Windows.Forms.ToolStripButton m_tsbStartPoint;
		private System.Windows.Forms.ToolStripButton m_tsbReverse;
		private System.Windows.Forms.ToolStripButton m_tsbSetLead;
		private System.Windows.Forms.ToolStripButton m_tsbOverCut;
		private System.Windows.Forms.ToolStripButton m_tsbTooVecReverse;
		private System.Windows.Forms.ToolStripButton m_tsbToolVec;
		private System.Windows.Forms.ToolStripButton m_tsbMoveUp;
		private System.Windows.Forms.ToolStripButton m_tsbMoveDown;
		private System.Windows.Forms.ToolStripButton m_tsbAutoOrder;
		private System.Windows.Forms.ToolStripContainer m_tscLevel1Container;
		private System.Windows.Forms.ToolStripContainer m_tscLevel2Container;
		private System.Windows.Forms.ToolStrip m_tsCADFunction;
		private System.Windows.Forms.Panel m_pnlLog;
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
		private System.Windows.Forms.ToolStripButton m_tsbTraverseParamSetting;
		private System.Windows.Forms.ToolStripButton m_tsbConvertNC;
		private System.Windows.Forms.ToolStripButton m_tsbFlipLead;
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
		private System.Windows.Forms.ToolStrip m_tsViewerToolBar;
		private System.Windows.Forms.ToolStripButton m_tsbShowVec;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ToolTip toolTip2;
		private System.Windows.Forms.ToolStripButton m_tsbShowOrder;
		private System.Windows.Forms.ToolStripButton m_tsbShowOrientation;
		private System.Windows.Forms.ToolStripButton m_tsbShowTraverse;
		private System.Windows.Forms.ToolStripButton m_tsbSelPath_FreeBound;
		private System.Windows.Forms.ToolStripButton m_tsbSelPath_Manual;
		private System.Windows.Forms.ToolStripButton m_tsbEntryAndExitSetting;
	}
}

