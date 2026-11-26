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
			if( disposing ) {
				m_CAMEditor?.EndAction( resumeDefaultAction: false );
				if( m_DataManager != null ) {
					m_DataManager.PartChanged -= UpdateThreePointCalibrationEntry;
					m_DataManager.PartRemoved -= OnCalibrationPartsRemoved;
				}
			}
			if( disposing && m_CNCUIManager != null ) {
				m_CNCUIManager.Notice -= OnCNCNotice;
				m_CNCUIManager.ControllerRequested -= ShowController;
				m_CNCUIManager.ThreePointCalibrationRequested -= ShowThreePointCalibration;
				m_CNCUIManager.AlarmsChanged -= OnCNCAlarmsChanged;
				m_CNCUIManager.ConnectionChanged -= OnCNCConnectionChanged;
				m_CNCUIManager.Dispose();
			}
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
			this.m_msMode = new System.Windows.Forms.MenuStrip();
			this.m_tsmiFile = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiCAD = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiCAM = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiSimulation = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiCNC = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiSetting = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiCNCConnection = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiController = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiLanguage = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsCAMFunction = new System.Windows.Forms.ToolStrip();
			this.m_tsbAddPath = new System.Windows.Forms.ToolStripButton();
			this.m_tsbDeletePath = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAutoOrder = new System.Windows.Forms.ToolStripButton();
			this.m_tsbManualOrder = new System.Windows.Forms.ToolStripButton();
			this.m_tsbStartPoint = new System.Windows.Forms.ToolStripButton();
			this.m_tsbReverse = new System.Windows.Forms.ToolStripButton();
			this.m_tsbEntryAndExitSetting = new System.Windows.Forms.ToolStripButton();
			this.m_tsbSetTraverseData = new System.Windows.Forms.ToolStripButton();
			this.m_tsbSetLead = new System.Windows.Forms.ToolStripButton();
			this.m_tsbOverCut = new System.Windows.Forms.ToolStripButton();
			this.m_tsbMicroJoint = new System.Windows.Forms.ToolStripButton();
			this.m_tsbRCorner = new System.Windows.Forms.ToolStripButton();
			this.m_tsbExtendedGCode = new System.Windows.Forms.ToolStripButton();
			this.m_tsbToolVec = new System.Windows.Forms.ToolStripButton();
			this.m_tsbFlyCut = new System.Windows.Forms.ToolStripButton();
			this.m_tsbFixedToolVec = new System.Windows.Forms.ToolStripButton();
			this.m_tsbToolVecReverse = new System.Windows.Forms.ToolStripButton();
			this.m_tsbOptimizeIK = new System.Windows.Forms.ToolStripButton();
			this.m_tsbPatternSetting = new System.Windows.Forms.ToolStripButton();
			this.m_tsbPathEdit = new System.Windows.Forms.ToolStripButton();
			this.m_tsbPathCompensation = new System.Windows.Forms.ToolStripButton();
			this.m_tsbContourEdit = new System.Windows.Forms.ToolStripButton();
			this.m_tsbMeasureCAM = new System.Windows.Forms.ToolStripButton();
			this.m_tsMachiningFunction = new System.Windows.Forms.ToolStrip();
			this.m_tsbSetOffset = new System.Windows.Forms.ToolStripButton();
			this.m_tsbCalibrationPnt = new System.Windows.Forms.ToolStripButton();
			this.m_tsbProcessSettings = new System.Windows.Forms.ToolStripButton();
			this.m_tsbConvertNC = new System.Windows.Forms.ToolStripButton();
			this.m_pnlToolBar = new System.Windows.Forms.Panel();
			this.m_tsSimuFunc = new System.Windows.Forms.ToolStrip();
			this.m_tsbPlay = new System.Windows.Forms.ToolStripButton();
			this.m_tsbPause = new System.Windows.Forms.ToolStripButton();
			this.m_tsbStop = new System.Windows.Forms.ToolStripButton();
			this.m_tsbPreviousPath = new System.Windows.Forms.ToolStripButton();
			this.m_tsbNextPath = new System.Windows.Forms.ToolStripButton();
			this.m_tsbPreCollision = new System.Windows.Forms.ToolStripButton();
			this.m_tsbNextCol = new System.Windows.Forms.ToolStripButton();
			this.m_tsbPrePoint = new System.Windows.Forms.ToolStripButton();
			this.m_tsbNextPoint = new System.Windows.Forms.ToolStripButton();
			this.m_tsbSpeedUp = new System.Windows.Forms.ToolStripButton();
			this.m_tsbSlowDown = new System.Windows.Forms.ToolStripButton();
			this.m_tsbCheck = new System.Windows.Forms.ToolStripButton();
			this.m_tsFileFunction = new System.Windows.Forms.ToolStrip();
			this.m_tsbImport3DFile = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddRevolution = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddTube = new System.Windows.Forms.ToolStripButton();
			this.m_tsbReadProjectFile = new System.Windows.Forms.ToolStripButton();
			this.m_tsbSaveProjectFile = new System.Windows.Forms.ToolStripButton();
			this.m_tsCADFunction = new System.Windows.Forms.ToolStrip();
			this.m_tsbSewPart = new System.Windows.Forms.ToolStripButton();
			this.m_tsbDesignSupportBracket = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddPoint_BoundingBoxCenter = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddPoint_EdgeCenter = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddPoint_PointCenter = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddPoint_AcrCenter = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddLine_TwoVertexConnect = new System.Windows.Forms.ToolStripButton();
			this.m_tsbWireDiscreteVertex = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAxisTransform = new System.Windows.Forms.ToolStripButton();
			this.m_tsbManualTransform = new System.Windows.Forms.ToolStripButton();
			this.m_tsb3PntTransform = new System.Windows.Forms.ToolStripButton();
			this.m_tsbMeasureCAD = new System.Windows.Forms.ToolStripButton();
			this.m_pnlCNCVertical = new System.Windows.Forms.Panel();
			this.m_pnlVerticalAlarm = new System.Windows.Forms.Panel();
			this.m_pnlTreeView = new System.Windows.Forms.Panel();
			this.m_pnlLog = new System.Windows.Forms.Panel();
			this.m_pnlMessages = new System.Windows.Forms.Panel();
			this.m_pnlViewer = new System.Windows.Forms.Panel();
			this.m_tsLayer = new System.Windows.Forms.ToolStrip();
			this.m_tsbLayer1 = new System.Windows.Forms.ToolStripButton();
			this.m_tsbLayer2 = new System.Windows.Forms.ToolStripButton();
			this.m_tsbLayer3 = new System.Windows.Forms.ToolStripButton();
			this.m_tsbLayer4 = new System.Windows.Forms.ToolStripButton();
			this.m_tsbLayer5 = new System.Windows.Forms.ToolStripButton();
			this.m_tsbLayer6 = new System.Windows.Forms.ToolStripButton();
			this.m_tsbLayer7 = new System.Windows.Forms.ToolStripButton();
			this.m_tsbLayer8 = new System.Windows.Forms.ToolStripButton();
			this.m_tsbLayer9 = new System.Windows.Forms.ToolStripButton();
			this.m_tsbLayer10 = new System.Windows.Forms.ToolStripButton();
			this.m_tsbLayerNone = new System.Windows.Forms.ToolStripButton();
			this.m_tsbNeedProcess = new System.Windows.Forms.ToolStripButton();
			this.m_tsViewerToolBar = new System.Windows.Forms.ToolStrip();
			this.m_tsbAXO = new System.Windows.Forms.ToolStripButton();
			this.m_tsbXPos = new System.Windows.Forms.ToolStripButton();
			this.m_tsbXNeg = new System.Windows.Forms.ToolStripButton();
			this.m_tsbYPos = new System.Windows.Forms.ToolStripButton();
			this.m_tsbYNeg = new System.Windows.Forms.ToolStripButton();
			this.m_tsbZPos = new System.Windows.Forms.ToolStripButton();
			this.m_tsbZNeg = new System.Windows.Forms.ToolStripButton();
			this.m_tssViewerToolBar = new System.Windows.Forms.ToolStripSeparator();
			this.m_tsbShowVec = new System.Windows.Forms.ToolStripButton();
			this.m_tsbShowOrder = new System.Windows.Forms.ToolStripButton();
			this.m_tsbShowOrientation = new System.Windows.Forms.ToolStripButton();
			this.m_tsbShowTraverse = new System.Windows.Forms.ToolStripButton();
			this.m_tsbShowMarker = new System.Windows.Forms.ToolStripButton();
			this.m_pnlCNC = new System.Windows.Forms.Panel();
			this.m_pnlCNCInfo = new System.Windows.Forms.Panel();
			this.m_pnlBottom = new System.Windows.Forms.Panel();
			this.m_pnlBackGround = new System.Windows.Forms.Panel();
			this.m_msMode.SuspendLayout();
			this.m_tsCAMFunction.SuspendLayout();
			this.m_tsMachiningFunction.SuspendLayout();
			this.m_pnlToolBar.SuspendLayout();
			this.m_tsSimuFunc.SuspendLayout();
			this.m_tsFileFunction.SuspendLayout();
			this.m_tsCADFunction.SuspendLayout();
			this.m_pnlCNCVertical.SuspendLayout();
			this.m_pnlMessages.SuspendLayout();
			this.m_pnlViewer.SuspendLayout();
			this.m_tsLayer.SuspendLayout();
			this.m_tsViewerToolBar.SuspendLayout();
			this.m_pnlBottom.SuspendLayout();
			this.m_pnlBackGround.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_msMode
			// 
			this.m_msMode.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.m_msMode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiFile,
            this.m_tsmiCAD,
            this.m_tsmiCAM,
            this.m_tsmiSimulation,
            this.m_tsmiCNC,
            this.m_tsmiSetting});
			this.m_msMode.Location = new System.Drawing.Point(0, 0);
			this.m_msMode.Name = "m_msMode";
			this.m_msMode.Size = new System.Drawing.Size(1920, 24);
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
			// m_tsmiSimulation
			// 
			this.m_tsmiSimulation.Name = "m_tsmiSimulation";
			this.m_tsmiSimulation.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiSimulation.Text = "模擬";
			this.m_tsmiSimulation.Click += new System.EventHandler(this.m_tsmiSimulation_Click);
			// 
			// m_tsmiCNC
			// 
			this.m_tsmiCNC.Enabled = false;
			this.m_tsmiCNC.Name = "m_tsmiCNC";
			this.m_tsmiCNC.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiCNC.Text = "加工";
			this.m_tsmiCNC.Click += new System.EventHandler(this.m_tsmiCNC_Click);
			// 
			// m_tsmiSetting
			// 
			this.m_tsmiSetting.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsmiCNCConnection,
            this.m_tsmiController,
            this.m_tsmiLanguage});
			this.m_tsmiSetting.Name = "m_tsmiSetting";
			this.m_tsmiSetting.Size = new System.Drawing.Size(43, 20);
			this.m_tsmiSetting.Text = "設定";
			// 
			// m_tsmiCNCConnection
			// 
			this.m_tsmiCNCConnection.Name = "m_tsmiCNCConnection";
			this.m_tsmiCNCConnection.Size = new System.Drawing.Size(158, 22);
			this.m_tsmiCNCConnection.Text = "CNC連線設定";
			this.m_tsmiCNCConnection.Click += new System.EventHandler(this.m_tsmiCNCConnection_Click);
			// 
			// m_tsmiController
			// 
			this.m_tsmiController.Name = "m_tsmiController";
			this.m_tsmiController.Size = new System.Drawing.Size(158, 22);
			this.m_tsmiController.Text = "控制器進階設定";
			this.m_tsmiController.Click += new System.EventHandler(this.m_tsmiController_Click);
			// 
			// m_tsmiLanguage
			// 
			this.m_tsmiLanguage.Name = "m_tsmiLanguage";
			this.m_tsmiLanguage.Size = new System.Drawing.Size(158, 22);
			this.m_tsmiLanguage.Text = "語系切換";
			// 
			// m_tsCAMFunction
			// 
			this.m_tsCAMFunction.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsCAMFunction.ImageScalingSize = new System.Drawing.Size(40, 40);
			this.m_tsCAMFunction.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbAddPath,
            this.m_tsbDeletePath,
            this.m_tsbAutoOrder,
            this.m_tsbManualOrder,
            this.m_tsbStartPoint,
            this.m_tsbReverse,
            this.m_tsbEntryAndExitSetting,
            this.m_tsbSetTraverseData,
            this.m_tsbSetLead,
            this.m_tsbOverCut,
            this.m_tsbMicroJoint,
            this.m_tsbRCorner,
            this.m_tsbExtendedGCode,
            this.m_tsbToolVec,
            this.m_tsbFlyCut,
            this.m_tsbFixedToolVec,
            this.m_tsbToolVecReverse,
            this.m_tsbOptimizeIK,
            this.m_tsbPatternSetting,
            this.m_tsbPathEdit,
            this.m_tsbPathCompensation,
            this.m_tsbContourEdit,
            this.m_tsbMeasureCAM});
			this.m_tsCAMFunction.Location = new System.Drawing.Point(0, 0);
			this.m_tsCAMFunction.Name = "m_tsCAMFunction";
			this.m_tsCAMFunction.Size = new System.Drawing.Size(1920, 75);
			this.m_tsCAMFunction.TabIndex = 2;
			this.m_tsCAMFunction.Text = "toolStrip1";
			// 
			// m_tsbAddPath
			// 
			this.m_tsbAddPath.Image = global::MyCAM.Properties.Resources.CAM_AddPath;
			this.m_tsbAddPath.Name = "m_tsbAddPath";
			this.m_tsbAddPath.Size = new System.Drawing.Size(59, 72);
			this.m_tsbAddPath.Text = "新增路徑";
			this.m_tsbAddPath.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAddPath.ToolTipText = "選擇新增路徑的方式";
			this.m_tsbAddPath.Click += new System.EventHandler(this.m_tsbAddPath_Click);
			// 
			// m_tsbDeletePath
			// 
			this.m_tsbDeletePath.Image = global::MyCAM.Properties.Resources.CAM_DeletePath;
			this.m_tsbDeletePath.Name = "m_tsbDeletePath";
			this.m_tsbDeletePath.Size = new System.Drawing.Size(59, 72);
			this.m_tsbDeletePath.Text = "刪除路徑";
			this.m_tsbDeletePath.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbDeletePath.ToolTipText = "刪除選取的路徑";
			this.m_tsbDeletePath.Click += new System.EventHandler(this.m_tsbDeletePath_Click);
			// 
			// m_tsbAutoOrder
			// 
			this.m_tsbAutoOrder.Image = global::MyCAM.Properties.Resources.CAM_AutoOrder;
			this.m_tsbAutoOrder.Name = "m_tsbAutoOrder";
			this.m_tsbAutoOrder.Size = new System.Drawing.Size(59, 72);
			this.m_tsbAutoOrder.Text = "自動排序";
			this.m_tsbAutoOrder.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAutoOrder.ToolTipText = "選擇自動排序路徑的方式";
			this.m_tsbAutoOrder.Click += new System.EventHandler(this.m_tsbAutoOrder_Click);
			// 
			// m_tsbManualOrder
			// 
			this.m_tsbManualOrder.Image = global::MyCAM.Properties.Resources.CAM_ManualOrder;
			this.m_tsbManualOrder.Name = "m_tsbManualOrder";
			this.m_tsbManualOrder.Size = new System.Drawing.Size(59, 72);
			this.m_tsbManualOrder.Text = "手動排序";
			this.m_tsbManualOrder.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbManualOrder.ToolTipText = "手動排序路徑";
			this.m_tsbManualOrder.Click += new System.EventHandler(this.m_tsbManualOrder_Click);
			// 
			// m_tsbStartPoint
			// 
			this.m_tsbStartPoint.Image = global::MyCAM.Properties.Resources.CAM_StartPointSetting;
			this.m_tsbStartPoint.Name = "m_tsbStartPoint";
			this.m_tsbStartPoint.Size = new System.Drawing.Size(44, 72);
			this.m_tsbStartPoint.Text = "起點";
			this.m_tsbStartPoint.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbStartPoint.ToolTipText = "選擇路徑的起點";
			this.m_tsbStartPoint.Click += new System.EventHandler(this.m_tsbStartPoint_Click);
			// 
			// m_tsbReverse
			// 
			this.m_tsbReverse.Image = global::MyCAM.Properties.Resources.CAM_Reverse;
			this.m_tsbReverse.Name = "m_tsbReverse";
			this.m_tsbReverse.Size = new System.Drawing.Size(44, 72);
			this.m_tsbReverse.Text = "反向";
			this.m_tsbReverse.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbReverse.ToolTipText = "反向選擇的路徑方向";
			this.m_tsbReverse.Click += new System.EventHandler(this.m_tsbReverse_Click);
			// 
			// m_tsbEntryAndExitSetting
			// 
			this.m_tsbEntryAndExitSetting.Image = global::MyCAM.Properties.Resources.CAM_EntryAndExit;
			this.m_tsbEntryAndExitSetting.Name = "m_tsbEntryAndExitSetting";
			this.m_tsbEntryAndExitSetting.Size = new System.Drawing.Size(64, 72);
			this.m_tsbEntryAndExitSetting.Text = "進入/退出";
			this.m_tsbEntryAndExitSetting.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbEntryAndExitSetting.ToolTipText = "調整進入和退出的距離";
			this.m_tsbEntryAndExitSetting.Click += new System.EventHandler(this.m_tsbEntryAndExitSetting_Click);
			// 
			// m_tsbSetTraverseData
			// 
			this.m_tsbSetTraverseData.Image = global::MyCAM.Properties.Resources.CAM_TraverseParam;
			this.m_tsbSetTraverseData.Name = "m_tsbSetTraverseData";
			this.m_tsbSetTraverseData.Size = new System.Drawing.Size(44, 72);
			this.m_tsbSetTraverseData.Text = "空移";
			this.m_tsbSetTraverseData.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbSetTraverseData.ToolTipText = "調整選擇路徑的空移方式";
			this.m_tsbSetTraverseData.Click += new System.EventHandler(this.m_tsbSetTraverseData_Click);
			// 
			// m_tsbSetLead
			// 
			this.m_tsbSetLead.Image = global::MyCAM.Properties.Resources.CAM_Lead;
			this.m_tsbSetLead.Name = "m_tsbSetLead";
			this.m_tsbSetLead.Size = new System.Drawing.Size(44, 72);
			this.m_tsbSetLead.Text = "引線";
			this.m_tsbSetLead.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbSetLead.ToolTipText = "設定選擇路徑的引線參數";
			this.m_tsbSetLead.Click += new System.EventHandler(this.m_tsbSetLead_Click);
			// 
			// m_tsbOverCut
			// 
			this.m_tsbOverCut.Image = global::MyCAM.Properties.Resources.CAM_Overcut;
			this.m_tsbOverCut.Name = "m_tsbOverCut";
			this.m_tsbOverCut.Size = new System.Drawing.Size(44, 72);
			this.m_tsbOverCut.Text = "過切";
			this.m_tsbOverCut.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbOverCut.ToolTipText = "設定選擇路徑的過切";
			this.m_tsbOverCut.Click += new System.EventHandler(this.m_tsbOverCut_Click);
			// 
			// m_tsbMicroJoint
			// 
			this.m_tsbMicroJoint.Image = global::MyCAM.Properties.Resources.CAM_MicroJoint;
			this.m_tsbMicroJoint.Name = "m_tsbMicroJoint";
			this.m_tsbMicroJoint.Size = new System.Drawing.Size(44, 72);
			this.m_tsbMicroJoint.Text = "微連";
			this.m_tsbMicroJoint.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbMicroJoint.ToolTipText = "設定選擇路徑的微連";
			this.m_tsbMicroJoint.Click += new System.EventHandler(this.m_tsbMicroJoint_Click);
			// 
			// m_tsbRCorner
			// 
			this.m_tsbRCorner.Image = global::MyCAM.Properties.Resources.CAM_RCorner;
			this.m_tsbRCorner.Name = "m_tsbRCorner";
			this.m_tsbRCorner.Size = new System.Drawing.Size(44, 72);
			this.m_tsbRCorner.Text = "R角";
			this.m_tsbRCorner.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbRCorner.ToolTipText = "設定選擇路徑的R角工藝";
			this.m_tsbRCorner.Click += new System.EventHandler(this.m_tsbRCorner_Click);
			// 
			// m_tsbExtendedGCode
			// 
			this.m_tsbExtendedGCode.Image = global::MyCAM.Properties.Resources.CAM_ExtendedGCode;
			this.m_tsbExtendedGCode.Name = "m_tsbExtendedGCode";
			this.m_tsbExtendedGCode.Size = new System.Drawing.Size(56, 72);
			this.m_tsbExtendedGCode.Text = "擴充G碼";
			this.m_tsbExtendedGCode.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbExtendedGCode.ToolTipText = "在選擇路徑的指定位置插入擴充G碼";
			this.m_tsbExtendedGCode.Click += new System.EventHandler(this.m_tsbExtendedGCode_Click);
			// 
			// m_tsbToolVec
			// 
			this.m_tsbToolVec.Image = global::MyCAM.Properties.Resources.CAM_ToolVec;
			this.m_tsbToolVec.Name = "m_tsbToolVec";
			this.m_tsbToolVec.Size = new System.Drawing.Size(59, 72);
			this.m_tsbToolVec.Text = "飛邊姿態";
			this.m_tsbToolVec.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbToolVec.ToolTipText = "調整飛邊路徑的姿態";
			this.m_tsbToolVec.Click += new System.EventHandler(this.m_tsbToolVec_Click);
			// 
			// m_tsbFlyCut
			// 
			this.m_tsbFlyCut.Image = global::MyCAM.Properties.Resources.CAM_FlyCut;
			this.m_tsbFlyCut.Name = "m_tsbFlyCut";
			this.m_tsbFlyCut.Size = new System.Drawing.Size(44, 72);
			this.m_tsbFlyCut.Text = "飛切";
			this.m_tsbFlyCut.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbFlyCut.ToolTipText = "將一組路徑的空移方式轉為飛切";
			this.m_tsbFlyCut.Click += new System.EventHandler(this.m_tsbFlyCut_Click);
			// 
			// m_tsbFixedToolVec
			// 
			this.m_tsbFixedToolVec.Image = global::MyCAM.Properties.Resources.CAM_FixedToolVec;
			this.m_tsbFixedToolVec.Name = "m_tsbFixedToolVec";
			this.m_tsbFixedToolVec.Size = new System.Drawing.Size(47, 72);
			this.m_tsbFixedToolVec.Text = "孔姿態";
			this.m_tsbFixedToolVec.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbFixedToolVec.ToolTipText = "調整孔路徑的姿態";
			this.m_tsbFixedToolVec.Click += new System.EventHandler(this.m_tsbFixedToolVec_Click);
			// 
			// m_tsbToolVecReverse
			// 
			this.m_tsbToolVecReverse.Image = global::MyCAM.Properties.Resources.CAM_ToolVecReverse;
			this.m_tsbToolVecReverse.Name = "m_tsbToolVecReverse";
			this.m_tsbToolVecReverse.Size = new System.Drawing.Size(59, 72);
			this.m_tsbToolVecReverse.Text = "姿態反向";
			this.m_tsbToolVecReverse.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbToolVecReverse.ToolTipText = "反向選擇路徑的姿態方向";
			this.m_tsbToolVecReverse.Click += new System.EventHandler(this.m_tsbTooVecReverse_Click);
			// 
			// m_tsbOptimizeIK
			// 
			this.m_tsbOptimizeIK.Image = global::MyCAM.Properties.Resources.CAM_OptimizeIK;
			this.m_tsbOptimizeIK.Name = "m_tsbOptimizeIK";
			this.m_tsbOptimizeIK.Size = new System.Drawing.Size(59, 72);
			this.m_tsbOptimizeIK.Text = "優化軸向";
			this.m_tsbOptimizeIK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbOptimizeIK.ToolTipText = "優化所有路徑之間的旋轉軸角度";
			this.m_tsbOptimizeIK.Click += new System.EventHandler(this.m_tsbOptimizeIK_Click);
			// 
			// m_tsbPatternSetting
			// 
			this.m_tsbPatternSetting.Image = global::MyCAM.Properties.Resources.CAM_StandardPattern;
			this.m_tsbPatternSetting.Name = "m_tsbPatternSetting";
			this.m_tsbPatternSetting.Size = new System.Drawing.Size(59, 72);
			this.m_tsbPatternSetting.Text = "標準圖元";
			this.m_tsbPatternSetting.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbPatternSetting.ToolTipText = "設定選擇路徑為標準圖元";
			this.m_tsbPatternSetting.Click += new System.EventHandler(this.m_tsbPatternSetting_Click);
			// 
			// m_tsbPathEdit
			// 
			this.m_tsbPathEdit.Image = global::MyCAM.Properties.Resources.CAM_PathEdit;
			this.m_tsbPathEdit.Name = "m_tsbPathEdit";
			this.m_tsbPathEdit.Size = new System.Drawing.Size(59, 72);
			this.m_tsbPathEdit.Text = "位置補償";
			this.m_tsbPathEdit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbPathEdit.ToolTipText = "設定選擇路徑的位置補償量";
			this.m_tsbPathEdit.Click += new System.EventHandler(this.m_tsbPathEdit_Click);
			// 
			// m_tsbPathCompensation
			// 
			this.m_tsbPathCompensation.Image = global::MyCAM.Properties.Resources.CAM_PathCompensate;
			this.m_tsbPathCompensation.Name = "m_tsbPathCompensation";
			this.m_tsbPathCompensation.Size = new System.Drawing.Size(59, 72);
			this.m_tsbPathCompensation.Text = "尺寸補償";
			this.m_tsbPathCompensation.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbPathCompensation.ToolTipText = "設定選擇路徑的尺寸補償量";
			this.m_tsbPathCompensation.Click += new System.EventHandler(this.m_tsbPathCompensation_Click);
			// 
			// m_tsbContourEdit
			// 
			this.m_tsbContourEdit.Image = global::MyCAM.Properties.Resources.CAM_ContourEdit;
			this.m_tsbContourEdit.Name = "m_tsbContourEdit";
			this.m_tsbContourEdit.Size = new System.Drawing.Size(59, 72);
			this.m_tsbContourEdit.Text = "輪廓編輯";
			this.m_tsbContourEdit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbContourEdit.ToolTipText = "調整選擇路徑的局部輪廓";
			this.m_tsbContourEdit.Click += new System.EventHandler(this.m_tsbContourEdit_Click);
			// 
			// m_tsbMeasureCAM
			// 
			this.m_tsbMeasureCAM.Image = global::MyCAM.Properties.Resources.CAM_Measure;
			this.m_tsbMeasureCAM.Name = "m_tsbMeasureCAM";
			this.m_tsbMeasureCAM.Size = new System.Drawing.Size(44, 72);
			this.m_tsbMeasureCAM.Text = "量測";
			this.m_tsbMeasureCAM.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbMeasureCAM.ToolTipText = "量測距離或角度";
			this.m_tsbMeasureCAM.Click += new System.EventHandler(this.m_tsbMeasureCAM_Click);
			// 
			// m_tsMachiningFunction
			// 
			this.m_tsMachiningFunction.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsMachiningFunction.ImageScalingSize = new System.Drawing.Size(40, 40);
			this.m_tsMachiningFunction.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbSetOffset,
            this.m_tsbCalibrationPnt,
            this.m_tsbProcessSettings,
            this.m_tsbConvertNC});
			this.m_tsMachiningFunction.Location = new System.Drawing.Point(0, 0);
			this.m_tsMachiningFunction.Name = "m_tsMachiningFunction";
			this.m_tsMachiningFunction.Size = new System.Drawing.Size(1920, 75);
			this.m_tsMachiningFunction.TabIndex = 4;
			this.m_tsMachiningFunction.Text = "加工";
			// 
			// m_tsbSetOffset
			// 
			this.m_tsbSetOffset.Image = global::MyCAM.Properties.Resources.CAM_SetOffset;
			this.m_tsbSetOffset.Name = "m_tsbSetOffset";
			this.m_tsbSetOffset.Size = new System.Drawing.Size(59, 72);
			this.m_tsbSetOffset.Text = "設定偏置";
			this.m_tsbSetOffset.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbSetOffset.ToolTipText = "設定程式座標偏置";
			this.m_tsbSetOffset.Click += new System.EventHandler(this.m_tsbSetOffset_Click);
			// 
			// m_tsbCalibrationPnt
			// 
			this.m_tsbCalibrationPnt.Enabled = false;
			this.m_tsbCalibrationPnt.Image = global::MyCAM.Properties.Resources.CAM_Calibration;
			this.m_tsbCalibrationPnt.Name = "m_tsbCalibrationPnt";
			this.m_tsbCalibrationPnt.Size = new System.Drawing.Size(59, 72);
			this.m_tsbCalibrationPnt.Text = "三點校正";
			this.m_tsbCalibrationPnt.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbCalibrationPnt.ToolTipText = "設定加工檔的三點校正資訊";
			this.m_tsbCalibrationPnt.Click += new System.EventHandler(this.m_tsbCalibrationPnt_Click);
			// 
			// m_tsbProcessSettings
			// 
			this.m_tsbProcessSettings.Image = global::MyCAM.Properties.Resources.CAM_ProcessSetting;
			this.m_tsbProcessSettings.Name = "m_tsbProcessSettings";
			this.m_tsbProcessSettings.Size = new System.Drawing.Size(59, 72);
			this.m_tsbProcessSettings.Text = "工藝設置";
			this.m_tsbProcessSettings.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbProcessSettings.ToolTipText = "開啟控制器工藝設置";
			this.m_tsbProcessSettings.Click += new System.EventHandler(this.m_tsbProcessSettings_Click);
			// 
			// m_tsbConvertNC
			// 
			this.m_tsbConvertNC.Image = global::MyCAM.Properties.Resources.CAM_NCFile;
			this.m_tsbConvertNC.Name = "m_tsbConvertNC";
			this.m_tsbConvertNC.Size = new System.Drawing.Size(53, 72);
			this.m_tsbConvertNC.Text = "轉出NC";
			this.m_tsbConvertNC.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbConvertNC.ToolTipText = "轉出NC加工檔";
			this.m_tsbConvertNC.Click += new System.EventHandler(this.m_tsbConvertNC_Click);
			// 
			// m_pnlToolBar
			// 
			this.m_pnlToolBar.Controls.Add(this.m_tsCAMFunction);
			this.m_pnlToolBar.Controls.Add(this.m_tsMachiningFunction);
			this.m_pnlToolBar.Controls.Add(this.m_tsSimuFunc);
			this.m_pnlToolBar.Controls.Add(this.m_tsFileFunction);
			this.m_pnlToolBar.Controls.Add(this.m_tsCADFunction);
			this.m_pnlToolBar.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_pnlToolBar.Location = new System.Drawing.Point(0, 24);
			this.m_pnlToolBar.Name = "m_pnlToolBar";
			this.m_pnlToolBar.Size = new System.Drawing.Size(1920, 75);
			this.m_pnlToolBar.TabIndex = 5;
			// 
			// m_tsSimuFunc
			// 
			this.m_tsSimuFunc.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsSimuFunc.ImageScalingSize = new System.Drawing.Size(40, 40);
			this.m_tsSimuFunc.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbPlay,
            this.m_tsbPause,
            this.m_tsbStop,
            this.m_tsbPreviousPath,
            this.m_tsbNextPath,
            this.m_tsbPreCollision,
            this.m_tsbNextCol,
            this.m_tsbPrePoint,
            this.m_tsbNextPoint,
            this.m_tsbSpeedUp,
            this.m_tsbSlowDown,
            this.m_tsbCheck});
			this.m_tsSimuFunc.Location = new System.Drawing.Point(0, 0);
			this.m_tsSimuFunc.Name = "m_tsSimuFunc";
			this.m_tsSimuFunc.Size = new System.Drawing.Size(1920, 75);
			this.m_tsSimuFunc.TabIndex = 5;
			this.m_tsSimuFunc.Text = "模擬功能";
			// 
			// m_tsbPlay
			// 
			this.m_tsbPlay.Image = global::MyCAM.Properties.Resources.Simu_Play;
			this.m_tsbPlay.Name = "m_tsbPlay";
			this.m_tsbPlay.Size = new System.Drawing.Size(44, 72);
			this.m_tsbPlay.Text = "播放";
			this.m_tsbPlay.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbPlay.ToolTipText = "播放模擬";
			this.m_tsbPlay.Click += new System.EventHandler(this.m_tsbPlay_Click);
			// 
			// m_tsbPause
			// 
			this.m_tsbPause.Image = global::MyCAM.Properties.Resources.Simu_Pause;
			this.m_tsbPause.Name = "m_tsbPause";
			this.m_tsbPause.Size = new System.Drawing.Size(44, 72);
			this.m_tsbPause.Text = "暫停";
			this.m_tsbPause.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbPause.ToolTipText = "暫停模擬";
			this.m_tsbPause.Click += new System.EventHandler(this.m_tsbPause_Click);
			// 
			// m_tsbStop
			// 
			this.m_tsbStop.Image = global::MyCAM.Properties.Resources.Simu_Stop;
			this.m_tsbStop.Name = "m_tsbStop";
			this.m_tsbStop.Size = new System.Drawing.Size(44, 72);
			this.m_tsbStop.Text = "停止";
			this.m_tsbStop.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbStop.ToolTipText = "停止模擬並回到模擬起點";
			this.m_tsbStop.Click += new System.EventHandler(this.m_tsbStop_Click);
			// 
			// m_tsbPreviousPath
			// 
			this.m_tsbPreviousPath.Image = global::MyCAM.Properties.Resources.Simu_PreviousPath;
			this.m_tsbPreviousPath.Name = "m_tsbPreviousPath";
			this.m_tsbPreviousPath.Size = new System.Drawing.Size(47, 72);
			this.m_tsbPreviousPath.Text = "前路徑";
			this.m_tsbPreviousPath.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbPreviousPath.ToolTipText = "跳往前一路徑";
			this.m_tsbPreviousPath.Click += new System.EventHandler(this.m_tsbPreviousPath_Click);
			// 
			// m_tsbNextPath
			// 
			this.m_tsbNextPath.Image = global::MyCAM.Properties.Resources.Simu_NextPath;
			this.m_tsbNextPath.Name = "m_tsbNextPath";
			this.m_tsbNextPath.Size = new System.Drawing.Size(47, 72);
			this.m_tsbNextPath.Text = "後路徑";
			this.m_tsbNextPath.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbNextPath.ToolTipText = "跳往後一路徑";
			this.m_tsbNextPath.Click += new System.EventHandler(this.m_tsbNextPath_Click);
			// 
			// m_tsbPreCollision
			// 
			this.m_tsbPreCollision.Image = global::MyCAM.Properties.Resources.Simu_PreviousCol;
			this.m_tsbPreCollision.Name = "m_tsbPreCollision";
			this.m_tsbPreCollision.Size = new System.Drawing.Size(59, 72);
			this.m_tsbPreCollision.Text = "前碰撞點";
			this.m_tsbPreCollision.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbPreCollision.ToolTipText = "跳往前一碰撞點";
			this.m_tsbPreCollision.Click += new System.EventHandler(this.m_tsbPreCollision_Click);
			// 
			// m_tsbNextCol
			// 
			this.m_tsbNextCol.Image = global::MyCAM.Properties.Resources.Simu_NextCol;
			this.m_tsbNextCol.Name = "m_tsbNextCol";
			this.m_tsbNextCol.Size = new System.Drawing.Size(59, 72);
			this.m_tsbNextCol.Text = "後碰撞點";
			this.m_tsbNextCol.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbNextCol.ToolTipText = "跳往後一碰撞點";
			this.m_tsbNextCol.Click += new System.EventHandler(this.m_tsbNextCol_Click);
			// 
			// m_tsbPrePoint
			// 
			this.m_tsbPrePoint.Image = global::MyCAM.Properties.Resources.Simu_PrePoint;
			this.m_tsbPrePoint.Name = "m_tsbPrePoint";
			this.m_tsbPrePoint.Size = new System.Drawing.Size(47, 72);
			this.m_tsbPrePoint.Text = "上一步";
			this.m_tsbPrePoint.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbPrePoint.ToolTipText = "步進到前一步";
			this.m_tsbPrePoint.Click += new System.EventHandler(this.m_tsbPrePoint_Click);
			// 
			// m_tsbNextPoint
			// 
			this.m_tsbNextPoint.Image = global::MyCAM.Properties.Resources.Simu_NextPoint;
			this.m_tsbNextPoint.Name = "m_tsbNextPoint";
			this.m_tsbNextPoint.Size = new System.Drawing.Size(47, 72);
			this.m_tsbNextPoint.Text = "下一步";
			this.m_tsbNextPoint.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbNextPoint.ToolTipText = "步進到後一步";
			this.m_tsbNextPoint.Click += new System.EventHandler(this.m_tsbNextPoint_Click);
			// 
			// m_tsbSpeedUp
			// 
			this.m_tsbSpeedUp.Image = global::MyCAM.Properties.Resources.Simu_SpeedUp;
			this.m_tsbSpeedUp.Name = "m_tsbSpeedUp";
			this.m_tsbSpeedUp.Size = new System.Drawing.Size(44, 72);
			this.m_tsbSpeedUp.Text = "加速";
			this.m_tsbSpeedUp.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbSpeedUp.ToolTipText = "加快模擬速度";
			this.m_tsbSpeedUp.Click += new System.EventHandler(this.m_tsbSpeedUp_Click);
			// 
			// m_tsbSlowDown
			// 
			this.m_tsbSlowDown.Image = global::MyCAM.Properties.Resources.Simu_SlowDown;
			this.m_tsbSlowDown.Name = "m_tsbSlowDown";
			this.m_tsbSlowDown.Size = new System.Drawing.Size(44, 72);
			this.m_tsbSlowDown.Text = "減速";
			this.m_tsbSlowDown.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbSlowDown.ToolTipText = "減慢模擬速度";
			this.m_tsbSlowDown.Click += new System.EventHandler(this.m_tsbSlowDown_Click);
			// 
			// m_tsbCheck
			// 
			this.m_tsbCheck.Image = global::MyCAM.Properties.Resources.Simu_Check;
			this.m_tsbCheck.Name = "m_tsbCheck";
			this.m_tsbCheck.Size = new System.Drawing.Size(59, 72);
			this.m_tsbCheck.Text = "碰撞檢查";
			this.m_tsbCheck.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbCheck.ToolTipText = "檢查是否有碰撞";
			this.m_tsbCheck.Click += new System.EventHandler(this.m_tsbCheck_Click);
			// 
			// m_tsFileFunction
			// 
			this.m_tsFileFunction.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsFileFunction.ImageScalingSize = new System.Drawing.Size(40, 40);
			this.m_tsFileFunction.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbImport3DFile,
            this.m_tsbAddRevolution,
            this.m_tsbAddTube,
            this.m_tsbReadProjectFile,
            this.m_tsbSaveProjectFile});
			this.m_tsFileFunction.Location = new System.Drawing.Point(0, 0);
			this.m_tsFileFunction.Name = "m_tsFileFunction";
			this.m_tsFileFunction.Size = new System.Drawing.Size(1920, 75);
			this.m_tsFileFunction.TabIndex = 4;
			this.m_tsFileFunction.Text = "匯入3D模型";
			// 
			// m_tsbImport3DFile
			// 
			this.m_tsbImport3DFile.Image = global::MyCAM.Properties.Resources.File_Import3DFile;
			this.m_tsbImport3DFile.Name = "m_tsbImport3DFile";
			this.m_tsbImport3DFile.Size = new System.Drawing.Size(51, 72);
			this.m_tsbImport3DFile.Text = "3D模型";
			this.m_tsbImport3DFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbImport3DFile.ToolTipText = "導入3D模型零件";
			this.m_tsbImport3DFile.Click += new System.EventHandler(this.m_tsbImport3DFile_Click);
			// 
			// m_tsbAddRevolution
			// 
			this.m_tsbAddRevolution.Image = global::MyCAM.Properties.Resources.CAD_Revolution;
			this.m_tsbAddRevolution.Name = "m_tsbAddRevolution";
			this.m_tsbAddRevolution.Size = new System.Drawing.Size(47, 72);
			this.m_tsbAddRevolution.Text = "迴轉體";
			this.m_tsbAddRevolution.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAddRevolution.ToolTipText = "導入迴轉體零件";
			this.m_tsbAddRevolution.Click += new System.EventHandler(this.m_tsbAddRevolution_Click);
			// 
			// m_tsbAddTube
			// 
			this.m_tsbAddTube.Image = global::MyCAM.Properties.Resources.CAD_AddTube;
			this.m_tsbAddTube.Name = "m_tsbAddTube";
			this.m_tsbAddTube.Size = new System.Drawing.Size(59, 72);
			this.m_tsbAddTube.Text = "設計管件";
			this.m_tsbAddTube.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAddTube.ToolTipText = "設計迴轉體零件";
			this.m_tsbAddTube.Click += new System.EventHandler(this.m_tsbAddTube_Click);
			// 
			// m_tsbReadProjectFile
			// 
			this.m_tsbReadProjectFile.Image = global::MyCAM.Properties.Resources.File_ReadProjectFile;
			this.m_tsbReadProjectFile.Name = "m_tsbReadProjectFile";
			this.m_tsbReadProjectFile.Size = new System.Drawing.Size(59, 72);
			this.m_tsbReadProjectFile.Text = "匯入專案";
			this.m_tsbReadProjectFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbReadProjectFile.ToolTipText = "匯入專案項目";
			this.m_tsbReadProjectFile.Click += new System.EventHandler(this.m_tsbImportProjectFile_Click);
			// 
			// m_tsbSaveProjectFile
			// 
			this.m_tsbSaveProjectFile.Image = global::MyCAM.Properties.Resources.File_SaveProjectFile;
			this.m_tsbSaveProjectFile.Name = "m_tsbSaveProjectFile";
			this.m_tsbSaveProjectFile.Size = new System.Drawing.Size(59, 72);
			this.m_tsbSaveProjectFile.Text = "儲存專案";
			this.m_tsbSaveProjectFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbSaveProjectFile.ToolTipText = "儲存專案項目";
			this.m_tsbSaveProjectFile.Click += new System.EventHandler(this.m_tsbSaveProjectFile_Click);
			// 
			// m_tsCADFunction
			// 
			this.m_tsCADFunction.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsCADFunction.ImageScalingSize = new System.Drawing.Size(40, 40);
			this.m_tsCADFunction.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbSewPart,
            this.m_tsbDesignSupportBracket,
            this.m_tsbAddPoint_BoundingBoxCenter,
            this.m_tsbAddPoint_EdgeCenter,
            this.m_tsbAddPoint_PointCenter,
            this.m_tsbAddPoint_AcrCenter,
            this.m_tsbAddLine_TwoVertexConnect,
            this.m_tsbWireDiscreteVertex,
            this.m_tsbAxisTransform,
            this.m_tsbManualTransform,
            this.m_tsb3PntTransform,
            this.m_tsbMeasureCAD});
			this.m_tsCADFunction.Location = new System.Drawing.Point(0, 0);
			this.m_tsCADFunction.Name = "m_tsCADFunction";
			this.m_tsCADFunction.Size = new System.Drawing.Size(1920, 75);
			this.m_tsCADFunction.TabIndex = 3;
			this.m_tsCADFunction.Text = "CAD功能";
			// 
			// m_tsbSewPart
			// 
			this.m_tsbSewPart.Image = global::MyCAM.Properties.Resources.CAD_SewPart;
			this.m_tsbSewPart.Name = "m_tsbSewPart";
			this.m_tsbSewPart.Size = new System.Drawing.Size(59, 72);
			this.m_tsbSewPart.Text = "模型縫補";
			this.m_tsbSewPart.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbSewPart.ToolTipText = "對零件進行縫隙縫補";
			this.m_tsbSewPart.Click += new System.EventHandler(this.m_tsbSewPart_Click);
			// 
			// m_tsbDesignSupportBracket
			// 
			this.m_tsbDesignSupportBracket.Image = global::MyCAM.Properties.Resources.CAD_SupportBracket;
			this.m_tsbDesignSupportBracket.Name = "m_tsbDesignSupportBracket";
			this.m_tsbDesignSupportBracket.Size = new System.Drawing.Size(59, 72);
			this.m_tsbDesignSupportBracket.Text = "設計夾具";
			this.m_tsbDesignSupportBracket.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbDesignSupportBracket.ToolTipText = "設計異形蓋件的簡易支架夾具";
			this.m_tsbDesignSupportBracket.Click += new System.EventHandler(this.m_tsbDesignSupportBracket_Click);
			// 
			// m_tsbAddPoint_BoundingBoxCenter
			// 
			this.m_tsbAddPoint_BoundingBoxCenter.Image = global::MyCAM.Properties.Resources.CAD_BoundingBoxCenter;
			this.m_tsbAddPoint_BoundingBoxCenter.Name = "m_tsbAddPoint_BoundingBoxCenter";
			this.m_tsbAddPoint_BoundingBoxCenter.Size = new System.Drawing.Size(71, 72);
			this.m_tsbAddPoint_BoundingBoxCenter.Text = "包圍盒中心";
			this.m_tsbAddPoint_BoundingBoxCenter.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAddPoint_BoundingBoxCenter.ToolTipText = "在包圍盒中心生成點，並將零件依照包圍盒中心移動到座標原點";
			this.m_tsbAddPoint_BoundingBoxCenter.Click += new System.EventHandler(this.m_tsbAddPoint_BoundingBoxCenter_Click);
			// 
			// m_tsbAddPoint_EdgeCenter
			// 
			this.m_tsbAddPoint_EdgeCenter.Image = global::MyCAM.Properties.Resources.CAD_EdgeCenter;
			this.m_tsbAddPoint_EdgeCenter.Name = "m_tsbAddPoint_EdgeCenter";
			this.m_tsbAddPoint_EdgeCenter.Size = new System.Drawing.Size(59, 72);
			this.m_tsbAddPoint_EdgeCenter.Text = "線段中心";
			this.m_tsbAddPoint_EdgeCenter.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAddPoint_EdgeCenter.ToolTipText = "生成所選線段的中心點";
			this.m_tsbAddPoint_EdgeCenter.Click += new System.EventHandler(this.m_tsbAddPoint_EdgeCenter_Click);
			// 
			// m_tsbAddPoint_PointCenter
			// 
			this.m_tsbAddPoint_PointCenter.Image = global::MyCAM.Properties.Resources.CAD_PointCenter;
			this.m_tsbAddPoint_PointCenter.Name = "m_tsbAddPoint_PointCenter";
			this.m_tsbAddPoint_PointCenter.Size = new System.Drawing.Size(59, 72);
			this.m_tsbAddPoint_PointCenter.Text = "兩點中心";
			this.m_tsbAddPoint_PointCenter.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAddPoint_PointCenter.ToolTipText = "生成所選兩點的中心點";
			this.m_tsbAddPoint_PointCenter.Click += new System.EventHandler(this.m_tsbAddPoint_PointCenter_Click);
			// 
			// m_tsbAddPoint_AcrCenter
			// 
			this.m_tsbAddPoint_AcrCenter.Image = global::MyCAM.Properties.Resources.CAD_AcrCenter;
			this.m_tsbAddPoint_AcrCenter.Name = "m_tsbAddPoint_AcrCenter";
			this.m_tsbAddPoint_AcrCenter.Size = new System.Drawing.Size(59, 72);
			this.m_tsbAddPoint_AcrCenter.Text = "圓弧中心";
			this.m_tsbAddPoint_AcrCenter.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAddPoint_AcrCenter.ToolTipText = "生成所選圓弧的中心點";
			this.m_tsbAddPoint_AcrCenter.Click += new System.EventHandler(this.m_tsbAddPoint_AcrCenter_Click);
			// 
			// m_tsbAddLine_TwoVertexConnect
			// 
			this.m_tsbAddLine_TwoVertexConnect.Image = global::MyCAM.Properties.Resources.CAD_TwoVertexConnect;
			this.m_tsbAddLine_TwoVertexConnect.Name = "m_tsbAddLine_TwoVertexConnect";
			this.m_tsbAddLine_TwoVertexConnect.Size = new System.Drawing.Size(59, 72);
			this.m_tsbAddLine_TwoVertexConnect.Text = "兩點連線";
			this.m_tsbAddLine_TwoVertexConnect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAddLine_TwoVertexConnect.ToolTipText = "生成所選兩點的直線";
			this.m_tsbAddLine_TwoVertexConnect.Click += new System.EventHandler(this.m_tsbAddLine_TwoVertexConnect_Click);
			// 
			// m_tsbWireDiscreteVertex
			// 
			this.m_tsbWireDiscreteVertex.Image = global::MyCAM.Properties.Resources.CAD_GenerateVertex;
			this.m_tsbWireDiscreteVertex.Name = "m_tsbWireDiscreteVertex";
			this.m_tsbWireDiscreteVertex.Size = new System.Drawing.Size(59, 72);
			this.m_tsbWireDiscreteVertex.Text = "建特徵點";
			this.m_tsbWireDiscreteVertex.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbWireDiscreteVertex.ToolTipText = "選取零件的輪廓後，並在輪廓線條上產生特徵點";
			this.m_tsbWireDiscreteVertex.Click += new System.EventHandler(this.m_tsbWireDiscreteVertex_Click);
			// 
			// m_tsbAxisTransform
			// 
			this.m_tsbAxisTransform.Image = global::MyCAM.Properties.Resources.CAD_AxisTrans;
			this.m_tsbAxisTransform.Name = "m_tsbAxisTransform";
			this.m_tsbAxisTransform.Size = new System.Drawing.Size(59, 72);
			this.m_tsbAxisTransform.Text = "軸向轉換";
			this.m_tsbAxisTransform.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAxisTransform.ToolTipText = "對零件進行偏移和旋轉";
			this.m_tsbAxisTransform.Click += new System.EventHandler(this.m_tsbAxisTransform_Click);
			// 
			// m_tsbManualTransform
			// 
			this.m_tsbManualTransform.Image = global::MyCAM.Properties.Resources.CAD_ManualTransform;
			this.m_tsbManualTransform.Name = "m_tsbManualTransform";
			this.m_tsbManualTransform.Size = new System.Drawing.Size(59, 72);
			this.m_tsbManualTransform.Text = "工件對齊";
			this.m_tsbManualTransform.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbManualTransform.ToolTipText = "將零件和座標軸進行對齊";
			this.m_tsbManualTransform.Click += new System.EventHandler(this.m_tsbManualTransform_Click);
			// 
			// m_tsb3PntTransform
			// 
			this.m_tsb3PntTransform.Image = global::MyCAM.Properties.Resources.CAM_ThreePntTrans;
			this.m_tsb3PntTransform.Name = "m_tsb3PntTransform";
			this.m_tsb3PntTransform.Size = new System.Drawing.Size(59, 72);
			this.m_tsb3PntTransform.Text = "三點轉換";
			this.m_tsb3PntTransform.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsb3PntTransform.ToolTipText = "選取三點後，依照三點座標放置工件";
			this.m_tsb3PntTransform.Click += new System.EventHandler(this.m_tsb3PntTransform_Click);
			// 
			// m_tsbMeasureCAD
			// 
			this.m_tsbMeasureCAD.Image = global::MyCAM.Properties.Resources.CAM_Measure;
			this.m_tsbMeasureCAD.Name = "m_tsbMeasureCAD";
			this.m_tsbMeasureCAD.Size = new System.Drawing.Size(44, 72);
			this.m_tsbMeasureCAD.Text = "量測";
			this.m_tsbMeasureCAD.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbMeasureCAD.ToolTipText = "量測距離或角度";
			this.m_tsbMeasureCAD.Click += new System.EventHandler(this.m_tsbMeasureCAD_Click);
			// 
			// m_pnlCNCVertical
			// 
			this.m_pnlCNCVertical.Controls.Add(this.m_pnlVerticalAlarm);
			this.m_pnlCNCVertical.Location = new System.Drawing.Point(0, 0);
			this.m_pnlCNCVertical.Name = "m_pnlCNCVertical";
			this.m_pnlCNCVertical.Size = new System.Drawing.Size(0, 0);
			this.m_pnlCNCVertical.TabIndex = 4;
			// 
			// m_pnlVerticalAlarm
			// 
			this.m_pnlVerticalAlarm.Location = new System.Drawing.Point(0, 0);
			this.m_pnlVerticalAlarm.Name = "m_pnlVerticalAlarm";
			this.m_pnlVerticalAlarm.Size = new System.Drawing.Size(0, 0);
			this.m_pnlVerticalAlarm.TabIndex = 0;
			// 
			// m_pnlTreeView
			// 
			this.m_pnlTreeView.Location = new System.Drawing.Point(4, 4);
			this.m_pnlTreeView.Name = "m_pnlTreeView";
			this.m_pnlTreeView.Size = new System.Drawing.Size(346, 661);
			this.m_pnlTreeView.TabIndex = 0;
			// 
			// m_pnlLog
			// 
			this.m_pnlLog.AutoScroll = true;
			this.m_pnlLog.Location = new System.Drawing.Point(0, 0);
			this.m_pnlLog.Name = "m_pnlLog";
			this.m_pnlLog.Size = new System.Drawing.Size(371, 233);
			this.m_pnlLog.TabIndex = 7;
			// 
			// m_pnlMessages
			// 
			this.m_pnlMessages.Controls.Add(this.m_pnlLog);
			this.m_pnlMessages.Location = new System.Drawing.Point(4, 4);
			this.m_pnlMessages.Name = "m_pnlMessages";
			this.m_pnlMessages.Size = new System.Drawing.Size(371, 233);
			this.m_pnlMessages.TabIndex = 0;
			this.m_pnlMessages.SizeChanged += new System.EventHandler(this.OnMessagesSizeChanged);
			// 
			// m_pnlViewer
			// 
			this.m_pnlViewer.Controls.Add(this.m_tsLayer);
			this.m_pnlViewer.Controls.Add(this.m_tsViewerToolBar);
			this.m_pnlViewer.Location = new System.Drawing.Point(358, 4);
			this.m_pnlViewer.Name = "m_pnlViewer";
			this.m_pnlViewer.Size = new System.Drawing.Size(1250, 661);
			this.m_pnlViewer.TabIndex = 1;
			// 
			// m_tsLayer
			// 
			this.m_tsLayer.Dock = System.Windows.Forms.DockStyle.Right;
			this.m_tsLayer.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.m_tsLayer.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.m_tsLayer.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbLayer1,
            this.m_tsbLayer2,
            this.m_tsbLayer3,
            this.m_tsbLayer4,
            this.m_tsbLayer5,
            this.m_tsbLayer6,
            this.m_tsbLayer7,
            this.m_tsbLayer8,
            this.m_tsbLayer9,
            this.m_tsbLayer10,
            this.m_tsbLayerNone,
            this.m_tsbNeedProcess});
			this.m_tsLayer.Location = new System.Drawing.Point(1214, 0);
			this.m_tsLayer.Name = "m_tsLayer";
			this.m_tsLayer.Size = new System.Drawing.Size(25, 658);
			this.m_tsLayer.TabIndex = 3;
			this.m_tsLayer.Text = "toolStrip1";
			// 
			// m_tsbLayer1
			// 
			this.m_tsbLayer1.AutoSize = false;
			this.m_tsbLayer1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer1.Name = "m_tsbLayer1";
			this.m_tsbLayer1.Size = new System.Drawing.Size(24, 24);
			this.m_tsbLayer1.Text = "1";
			this.m_tsbLayer1.Click += new System.EventHandler(this.m_tsbLayer1_Click);
			// 
			// m_tsbLayer2
			// 
			this.m_tsbLayer2.AutoSize = false;
			this.m_tsbLayer2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer2.Name = "m_tsbLayer2";
			this.m_tsbLayer2.Size = new System.Drawing.Size(24, 24);
			this.m_tsbLayer2.Text = "2";
			this.m_tsbLayer2.Click += new System.EventHandler(this.m_tsbLayer2_Click);
			// 
			// m_tsbLayer3
			// 
			this.m_tsbLayer3.AutoSize = false;
			this.m_tsbLayer3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer3.Name = "m_tsbLayer3";
			this.m_tsbLayer3.Size = new System.Drawing.Size(24, 24);
			this.m_tsbLayer3.Text = "3";
			this.m_tsbLayer3.Click += new System.EventHandler(this.m_tsbLayer3_Click);
			// 
			// m_tsbLayer4
			// 
			this.m_tsbLayer4.AutoSize = false;
			this.m_tsbLayer4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer4.Name = "m_tsbLayer4";
			this.m_tsbLayer4.Size = new System.Drawing.Size(24, 24);
			this.m_tsbLayer4.Text = "4";
			this.m_tsbLayer4.Click += new System.EventHandler(this.m_tsbLayer4_Click);
			// 
			// m_tsbLayer5
			// 
			this.m_tsbLayer5.AutoSize = false;
			this.m_tsbLayer5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer5.Name = "m_tsbLayer5";
			this.m_tsbLayer5.Size = new System.Drawing.Size(24, 24);
			this.m_tsbLayer5.Text = "5";
			this.m_tsbLayer5.Click += new System.EventHandler(this.m_tsbLayer5_Click);
			// 
			// m_tsbLayer6
			// 
			this.m_tsbLayer6.AutoSize = false;
			this.m_tsbLayer6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer6.Name = "m_tsbLayer6";
			this.m_tsbLayer6.Size = new System.Drawing.Size(24, 24);
			this.m_tsbLayer6.Text = "6";
			this.m_tsbLayer6.Click += new System.EventHandler(this.m_tsbLayer6_Click);
			// 
			// m_tsbLayer7
			// 
			this.m_tsbLayer7.AutoSize = false;
			this.m_tsbLayer7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer7.Name = "m_tsbLayer7";
			this.m_tsbLayer7.Size = new System.Drawing.Size(24, 24);
			this.m_tsbLayer7.Text = "7";
			this.m_tsbLayer7.Click += new System.EventHandler(this.m_tsbLayer7_Click);
			// 
			// m_tsbLayer8
			// 
			this.m_tsbLayer8.AutoSize = false;
			this.m_tsbLayer8.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer8.Name = "m_tsbLayer8";
			this.m_tsbLayer8.Size = new System.Drawing.Size(24, 24);
			this.m_tsbLayer8.Text = "8";
			this.m_tsbLayer8.Click += new System.EventHandler(this.m_tsbLayer8_Click);
			// 
			// m_tsbLayer9
			// 
			this.m_tsbLayer9.AutoSize = false;
			this.m_tsbLayer9.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer9.Name = "m_tsbLayer9";
			this.m_tsbLayer9.Size = new System.Drawing.Size(24, 24);
			this.m_tsbLayer9.Text = "9";
			this.m_tsbLayer9.Click += new System.EventHandler(this.m_tsbLayer9_Click);
			// 
			// m_tsbLayer10
			// 
			this.m_tsbLayer10.AutoSize = false;
			this.m_tsbLayer10.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer10.Name = "m_tsbLayer10";
			this.m_tsbLayer10.Size = new System.Drawing.Size(24, 24);
			this.m_tsbLayer10.Text = "10";
			this.m_tsbLayer10.Click += new System.EventHandler(this.m_tsbLayer10_Click);
			// 
			// m_tsbLayerNone
			// 
			this.m_tsbLayerNone.AutoSize = false;
			this.m_tsbLayerNone.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbLayerNone.Image = global::MyCAM.Properties.Resources.CAM_NoMachinePath;
			this.m_tsbLayerNone.Name = "m_tsbLayerNone";
			this.m_tsbLayerNone.Size = new System.Drawing.Size(24, 24);
			this.m_tsbLayerNone.Text = "不加工";
			this.m_tsbLayerNone.Click += new System.EventHandler(this.m_tsbLayerNone_Click);
			// 
			// m_tsbNeedProcess
			// 
			this.m_tsbNeedProcess.AutoSize = false;
			this.m_tsbNeedProcess.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbNeedProcess.Image = global::MyCAM.Properties.Resources.CAM_MachinePath;
			this.m_tsbNeedProcess.Name = "m_tsbNeedProcess";
			this.m_tsbNeedProcess.Size = new System.Drawing.Size(24, 24);
			this.m_tsbNeedProcess.Text = "加工";
			this.m_tsbNeedProcess.Click += new System.EventHandler(this.m_tsbNeedProcess_Click);
			// 
			// m_tsViewerToolBar
			// 
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
            this.m_tssViewerToolBar,
            this.m_tsbShowVec,
            this.m_tsbShowOrder,
            this.m_tsbShowOrientation,
            this.m_tsbShowTraverse,
            this.m_tsbShowMarker});
			this.m_tsViewerToolBar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.m_tsViewerToolBar.Location = new System.Drawing.Point(0, 0);
			this.m_tsViewerToolBar.Name = "m_tsViewerToolBar";
			this.m_tsViewerToolBar.Size = new System.Drawing.Size(25, 658);
			this.m_tsViewerToolBar.TabIndex = 2;
			this.m_tsViewerToolBar.Text = "toolStrip1";
			// 
			// m_tsbAXO
			// 
			this.m_tsbAXO.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAXO.Image = global::MyCAM.Properties.Resources.View_Axo;
			this.m_tsbAXO.Name = "m_tsbAXO";
			this.m_tsbAXO.Size = new System.Drawing.Size(22, 24);
			this.m_tsbAXO.Text = "AXO";
			this.m_tsbAXO.ToolTipText = "AXO";
			this.m_tsbAXO.Click += new System.EventHandler(this.m_tsbAXO_Click);
			// 
			// m_tsbXPos
			// 
			this.m_tsbXPos.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbXPos.Image = global::MyCAM.Properties.Resources.View_Right_View;
			this.m_tsbXPos.Name = "m_tsbXPos";
			this.m_tsbXPos.Size = new System.Drawing.Size(22, 24);
			this.m_tsbXPos.Text = "X+";
			this.m_tsbXPos.ToolTipText = "X+";
			this.m_tsbXPos.Click += new System.EventHandler(this.m_tsbXPos_Click);
			// 
			// m_tsbXNeg
			// 
			this.m_tsbXNeg.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbXNeg.Image = global::MyCAM.Properties.Resources.View_LeftView;
			this.m_tsbXNeg.Name = "m_tsbXNeg";
			this.m_tsbXNeg.Size = new System.Drawing.Size(22, 24);
			this.m_tsbXNeg.Text = "X-";
			this.m_tsbXNeg.ToolTipText = "X-";
			this.m_tsbXNeg.Click += new System.EventHandler(this.m_tsbXNeg_Click);
			// 
			// m_tsbYPos
			// 
			this.m_tsbYPos.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbYPos.Image = global::MyCAM.Properties.Resources.View_MainView;
			this.m_tsbYPos.Name = "m_tsbYPos";
			this.m_tsbYPos.Size = new System.Drawing.Size(22, 24);
			this.m_tsbYPos.Text = "Y+";
			this.m_tsbYPos.ToolTipText = "Y+";
			this.m_tsbYPos.Click += new System.EventHandler(this.m_tsbYPos_Click);
			// 
			// m_tsbYNeg
			// 
			this.m_tsbYNeg.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbYNeg.Image = global::MyCAM.Properties.Resources.View_BackView;
			this.m_tsbYNeg.Name = "m_tsbYNeg";
			this.m_tsbYNeg.Size = new System.Drawing.Size(22, 24);
			this.m_tsbYNeg.Text = "Y-";
			this.m_tsbYNeg.ToolTipText = "Y-";
			this.m_tsbYNeg.Click += new System.EventHandler(this.m_tsbYNeg_Click);
			// 
			// m_tsbZPos
			// 
			this.m_tsbZPos.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbZPos.Image = global::MyCAM.Properties.Resources.View_TopView;
			this.m_tsbZPos.Name = "m_tsbZPos";
			this.m_tsbZPos.Size = new System.Drawing.Size(22, 24);
			this.m_tsbZPos.Text = "Z+";
			this.m_tsbZPos.ToolTipText = "Z+";
			this.m_tsbZPos.Click += new System.EventHandler(this.m_tsbZPos_Click);
			// 
			// m_tsbZNeg
			// 
			this.m_tsbZNeg.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbZNeg.Image = global::MyCAM.Properties.Resources.View_BottomView;
			this.m_tsbZNeg.Name = "m_tsbZNeg";
			this.m_tsbZNeg.Size = new System.Drawing.Size(22, 24);
			this.m_tsbZNeg.Text = "Z-";
			this.m_tsbZNeg.ToolTipText = "Z-";
			this.m_tsbZNeg.Click += new System.EventHandler(this.m_tsbZNeg_Click);
			// 
			// m_tssViewerToolBar
			// 
			this.m_tssViewerToolBar.Name = "m_tssViewerToolBar";
			this.m_tssViewerToolBar.Size = new System.Drawing.Size(22, 6);
			// 
			// m_tsbShowVec
			// 
			this.m_tsbShowVec.Checked = true;
			this.m_tsbShowVec.CheckOnClick = true;
			this.m_tsbShowVec.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tsbShowVec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbShowVec.Image = global::MyCAM.Properties.Resources.CAM_ToolVec;
			this.m_tsbShowVec.Name = "m_tsbShowVec";
			this.m_tsbShowVec.Size = new System.Drawing.Size(22, 24);
			this.m_tsbShowVec.CheckedChanged += new System.EventHandler(this.m_tsbShowVec_CheckedChanged);
			// 
			// m_tsbShowOrder
			// 
			this.m_tsbShowOrder.Checked = true;
			this.m_tsbShowOrder.CheckOnClick = true;
			this.m_tsbShowOrder.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tsbShowOrder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbShowOrder.Image = global::MyCAM.Properties.Resources.CAM_AutoOrder;
			this.m_tsbShowOrder.Name = "m_tsbShowOrder";
			this.m_tsbShowOrder.Size = new System.Drawing.Size(22, 24);
			this.m_tsbShowOrder.CheckedChanged += new System.EventHandler(this.m_tsbShowOrder_CheckedChanged);
			// 
			// m_tsbShowOrientation
			// 
			this.m_tsbShowOrientation.Checked = true;
			this.m_tsbShowOrientation.CheckOnClick = true;
			this.m_tsbShowOrientation.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tsbShowOrientation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbShowOrientation.Image = global::MyCAM.Properties.Resources.CAM_Reverse;
			this.m_tsbShowOrientation.Name = "m_tsbShowOrientation";
			this.m_tsbShowOrientation.Size = new System.Drawing.Size(22, 24);
			this.m_tsbShowOrientation.CheckedChanged += new System.EventHandler(this.m_tsbShowOrientation_CheckedChanged);
			// 
			// m_tsbShowTraverse
			// 
			this.m_tsbShowTraverse.Checked = true;
			this.m_tsbShowTraverse.CheckOnClick = true;
			this.m_tsbShowTraverse.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tsbShowTraverse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbShowTraverse.Image = global::MyCAM.Properties.Resources.CAM_TraverseParam;
			this.m_tsbShowTraverse.Name = "m_tsbShowTraverse";
			this.m_tsbShowTraverse.Size = new System.Drawing.Size(22, 24);
			this.m_tsbShowTraverse.CheckedChanged += new System.EventHandler(this.m_tsbShowTraverse_CheckedChanged);
			// 
			// m_tsbShowMarker
			// 
			this.m_tsbShowMarker.Checked = true;
			this.m_tsbShowMarker.CheckOnClick = true;
			this.m_tsbShowMarker.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tsbShowMarker.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbShowMarker.Image = global::MyCAM.Properties.Resources.CAM_ExtendedGCode;
			this.m_tsbShowMarker.Name = "m_tsbShowMarker";
			this.m_tsbShowMarker.Size = new System.Drawing.Size(22, 24);
			this.m_tsbShowMarker.CheckedChanged += new System.EventHandler(this.m_tsbShowMarker_CheckedChanged);
			// 
			// m_pnlCNC
			// 
			this.m_pnlCNC.Location = new System.Drawing.Point(1608, -4);
			this.m_pnlCNC.Name = "m_pnlCNC";
			this.m_pnlCNC.Size = new System.Drawing.Size(316, 918);
			this.m_pnlCNC.TabIndex = 8;
			this.m_pnlCNC.Visible = false;
			// 
			// m_pnlCNCInfo
			// 
			this.m_pnlCNCInfo.Location = new System.Drawing.Point(375, -4);
			this.m_pnlCNCInfo.Name = "m_pnlCNCInfo";
			this.m_pnlCNCInfo.Size = new System.Drawing.Size(1241, 249);
			this.m_pnlCNCInfo.TabIndex = 9;
			this.m_pnlCNCInfo.Visible = false;
			// 
			// m_pnlBottom
			// 
			this.m_pnlBottom.Controls.Add(this.m_pnlMessages);
			this.m_pnlBottom.Controls.Add(this.m_pnlCNCInfo);
			this.m_pnlBottom.Location = new System.Drawing.Point(0, 669);
			this.m_pnlBottom.Name = "m_pnlBottom";
			this.m_pnlBottom.Size = new System.Drawing.Size(1612, 241);
			this.m_pnlBottom.TabIndex = 10;
			// 
			// m_pnlBackGround
			// 
			this.m_pnlBackGround.Controls.Add(this.m_pnlCNCVertical);
			this.m_pnlBackGround.Controls.Add(this.m_pnlTreeView);
			this.m_pnlBackGround.Controls.Add(this.m_pnlBottom);
			this.m_pnlBackGround.Controls.Add(this.m_pnlViewer);
			this.m_pnlBackGround.Controls.Add(this.m_pnlCNC);
			this.m_pnlBackGround.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_pnlBackGround.Location = new System.Drawing.Point(0, 99);
			this.m_pnlBackGround.Name = "m_pnlBackGround";
			this.m_pnlBackGround.Size = new System.Drawing.Size(1920, 910);
			this.m_pnlBackGround.TabIndex = 9;
			// 
			// StartupForm
			// 
			this.AutoScroll = true;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(1920, 1009);
			this.Controls.Add(this.m_pnlBackGround);
			this.Controls.Add(this.m_pnlToolBar);
			this.Controls.Add(this.m_msMode);
			this.Icon = global::MyCAM.Properties.Resources.AppIcon;
			this.MainMenuStrip = this.m_msMode;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "StartupForm";
			this.Text = "新代雷射五軸切割";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StartupForm_FormClosing);
			this.Resize += new System.EventHandler(this.StartupForm_Resize);
			this.m_msMode.ResumeLayout(false);
			this.m_msMode.PerformLayout();
			this.m_tsCAMFunction.ResumeLayout(false);
			this.m_tsCAMFunction.PerformLayout();
			this.m_tsMachiningFunction.ResumeLayout(false);
			this.m_tsMachiningFunction.PerformLayout();
			this.m_pnlToolBar.ResumeLayout(false);
			this.m_pnlToolBar.PerformLayout();
			this.m_tsSimuFunc.ResumeLayout(false);
			this.m_tsSimuFunc.PerformLayout();
			this.m_tsFileFunction.ResumeLayout(false);
			this.m_tsFileFunction.PerformLayout();
			this.m_tsCADFunction.ResumeLayout(false);
			this.m_tsCADFunction.PerformLayout();
			this.m_pnlCNCVertical.ResumeLayout(false);
			this.m_pnlMessages.ResumeLayout(false);
			this.m_pnlViewer.ResumeLayout(false);
			this.m_pnlViewer.PerformLayout();
			this.m_tsLayer.ResumeLayout(false);
			this.m_tsLayer.PerformLayout();
			this.m_tsViewerToolBar.ResumeLayout(false);
			this.m_tsViewerToolBar.PerformLayout();
			this.m_pnlBottom.ResumeLayout(false);
			this.m_pnlBackGround.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.MenuStrip m_msMode;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiFile;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiCAD;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiCAM;
		private System.Windows.Forms.ToolStrip m_tsCAMFunction;
		private System.Windows.Forms.ToolStrip m_tsMachiningFunction;
		private System.Windows.Forms.ToolStripButton m_tsbSetOffset;
		private System.Windows.Forms.ToolStripButton m_tsbProcessSettings;
		private System.Windows.Forms.ToolStripButton m_tsbAddPath;
		private System.Windows.Forms.ToolStripButton m_tsbDeletePath;
		private System.Windows.Forms.ToolStripButton m_tsbStartPoint;
		private System.Windows.Forms.ToolStripButton m_tsbReverse;
		private System.Windows.Forms.ToolStripButton m_tsbSetLead;
		private System.Windows.Forms.ToolStripButton m_tsbOverCut;
		private System.Windows.Forms.ToolStripButton m_tsbToolVecReverse;
		private System.Windows.Forms.ToolStripButton m_tsbToolVec;
		private System.Windows.Forms.ToolStripButton m_tsbAutoOrder;
		private System.Windows.Forms.Panel m_pnlToolBar;
		private System.Windows.Forms.ToolStrip m_tsCADFunction;
		private System.Windows.Forms.ToolStripButton m_tsbSetTraverseData;
		private System.Windows.Forms.ToolStripButton m_tsbFlyCut;
		private System.Windows.Forms.ToolStripButton m_tsbConvertNC;
		private System.Windows.Forms.ToolStripButton m_tsbMeasureCAM;
		private System.Windows.Forms.ToolStripButton m_tsbAddPoint_AcrCenter;
		private System.Windows.Forms.ToolStripButton m_tsbAddPoint_EdgeCenter;
		private System.Windows.Forms.ToolStripButton m_tsbAddPoint_PointCenter;
		private System.Windows.Forms.ToolStripButton m_tsbAddPoint_BoundingBoxCenter;
		private System.Windows.Forms.ToolStripButton m_tsbAddLine_TwoVertexConnect;
		private System.Windows.Forms.ToolStripButton m_tsbManualTransform;
		private System.Windows.Forms.ToolStripButton m_tsb3PntTransform;
		private System.Windows.Forms.ToolStripButton m_tsbMeasureCAD;
		private System.Windows.Forms.ToolStripButton m_tsbWireDiscreteVertex;
		private System.Windows.Forms.ToolStripButton m_tsbAxisTransform;
		private System.Windows.Forms.ToolStrip m_tsFileFunction;
		private System.Windows.Forms.ToolStripButton m_tsbImport3DFile;
		private System.Windows.Forms.ToolStripButton m_tsbReadProjectFile;
		private System.Windows.Forms.ToolStripButton m_tsbSaveProjectFile;
		private System.Windows.Forms.ToolStripButton m_tsbEntryAndExitSetting;
		private System.Windows.Forms.ToolStripButton m_tsbSewPart;
		private System.Windows.Forms.ToolStripButton m_tsbDesignSupportBracket;
		private System.Windows.Forms.ToolStripButton m_tsbPatternSetting;
		private System.Windows.Forms.ToolStripButton m_tsbFixedToolVec;
		private System.Windows.Forms.Panel m_pnlCNCVertical;
		private System.Windows.Forms.Panel m_pnlVerticalAlarm;
		private System.Windows.Forms.Panel m_pnlTreeView;
		private System.Windows.Forms.Panel m_pnlLog;
		private System.Windows.Forms.Panel m_pnlMessages;
		private System.Windows.Forms.Panel m_pnlViewer;
		private System.Windows.Forms.ToolStrip m_tsViewerToolBar;
		private System.Windows.Forms.Panel m_pnlCNC;
		private System.Windows.Forms.Panel m_pnlCNCInfo;
		private System.Windows.Forms.Panel m_pnlBottom;
		private System.Windows.Forms.ToolStripButton m_tsbAXO;
		private System.Windows.Forms.ToolStripButton m_tsbXPos;
		private System.Windows.Forms.ToolStripButton m_tsbXNeg;
		private System.Windows.Forms.ToolStripButton m_tsbYPos;
		private System.Windows.Forms.ToolStripButton m_tsbYNeg;
		private System.Windows.Forms.ToolStripButton m_tsbZPos;
		private System.Windows.Forms.ToolStripButton m_tsbZNeg;
		private System.Windows.Forms.ToolStripSeparator m_tssViewerToolBar;
		private System.Windows.Forms.ToolStripButton m_tsbShowVec;
		private System.Windows.Forms.ToolStripButton m_tsbShowOrder;
		private System.Windows.Forms.ToolStripButton m_tsbShowOrientation;
		private System.Windows.Forms.ToolStripButton m_tsbShowTraverse;
		private System.Windows.Forms.ToolStripButton m_tsbShowMarker;
		private System.Windows.Forms.Panel m_pnlBackGround;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiSetting;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiCNCConnection;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiSimulation;
		private System.Windows.Forms.ToolStrip m_tsSimuFunc;
		private System.Windows.Forms.ToolStripButton m_tsbPlay;
		private System.Windows.Forms.ToolStripButton m_tsbPause;
		private System.Windows.Forms.ToolStripButton m_tsbStop;
		private System.Windows.Forms.ToolStripButton m_tsbPreviousPath;
		private System.Windows.Forms.ToolStripButton m_tsbNextPath;
		private System.Windows.Forms.ToolStripButton m_tsbPreCollision;
		private System.Windows.Forms.ToolStripButton m_tsbNextCol;
		private System.Windows.Forms.ToolStripButton m_tsbSpeedUp;
		private System.Windows.Forms.ToolStripButton m_tsbSlowDown;
		private System.Windows.Forms.ToolStripButton m_tsbCheck;
		private System.Windows.Forms.ToolStripButton m_tsbPathEdit;
		private System.Windows.Forms.ToolStripButton m_tsbPathCompensation;
		private System.Windows.Forms.ToolStripButton m_tsbContourEdit;
		private System.Windows.Forms.ToolStripButton m_tsbPrePoint;
		private System.Windows.Forms.ToolStripButton m_tsbNextPoint;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiController;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiCNC;
		private System.Windows.Forms.ToolStrip m_tsLayer;
		private System.Windows.Forms.ToolStripButton m_tsbLayer1;
		private System.Windows.Forms.ToolStripButton m_tsbLayer2;
		private System.Windows.Forms.ToolStripButton m_tsbLayer3;
		private System.Windows.Forms.ToolStripButton m_tsbLayer4;
		private System.Windows.Forms.ToolStripButton m_tsbLayer5;
		private System.Windows.Forms.ToolStripButton m_tsbLayer6;
		private System.Windows.Forms.ToolStripButton m_tsbLayer7;
		private System.Windows.Forms.ToolStripButton m_tsbLayer8;
		private System.Windows.Forms.ToolStripButton m_tsbLayer9;
		private System.Windows.Forms.ToolStripButton m_tsbLayer10;
		private System.Windows.Forms.ToolStripButton m_tsbCalibrationPnt;
		private System.Windows.Forms.ToolStripButton m_tsbOptimizeIK;
		private System.Windows.Forms.ToolStripButton m_tsbMicroJoint;
		private System.Windows.Forms.ToolStripButton m_tsbRCorner;
		private System.Windows.Forms.ToolStripButton m_tsbExtendedGCode;
		private System.Windows.Forms.ToolStripButton m_tsbManualOrder;
		private System.Windows.Forms.ToolStripButton m_tsbAddTube;
		private System.Windows.Forms.ToolStripButton m_tsbAddRevolution;
		private System.Windows.Forms.ToolStripMenuItem m_tsmiLanguage;
		private System.Windows.Forms.ToolStripButton m_tsbLayerNone;
		private System.Windows.Forms.ToolStripButton m_tsbNeedProcess;
	}
}
