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
			this.m_msMode = new System.Windows.Forms.MenuStrip();
			this.m_tsmiFile = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiCAD = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiCAM = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsmiSimulation = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsbSetting = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsbVNCConnection = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tsbController = new System.Windows.Forms.ToolStripMenuItem();
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
			this.m_tsbPathEdit = new System.Windows.Forms.ToolStripButton();
			this.m_BtnContourEdit = new System.Windows.Forms.ToolStripButton();
			this.m_tsbPathCompensation = new System.Windows.Forms.ToolStripButton();
			this.m_tsbCalibrationPnt = new System.Windows.Forms.ToolStripButton();
			this.m_tsbConvertNC = new System.Windows.Forms.ToolStripButton();
			this.m_tscLevel1Container = new System.Windows.Forms.ToolStripContainer();
			this.m_tsSimuFunc = new System.Windows.Forms.ToolStrip();
			this.m_btnPlay = new System.Windows.Forms.ToolStripButton();
			this.m_btnPause = new System.Windows.Forms.ToolStripButton();
			this.m_btnStop = new System.Windows.Forms.ToolStripButton();
			this.m_btnPreviousPath = new System.Windows.Forms.ToolStripButton();
			this.m_btnNextPath = new System.Windows.Forms.ToolStripButton();
			this.m_btnPreCollision = new System.Windows.Forms.ToolStripButton();
			this.m_btnNextCol = new System.Windows.Forms.ToolStripButton();
			this.m_btnPrePoint = new System.Windows.Forms.ToolStripButton();
			this.m_btnNextPoint = new System.Windows.Forms.ToolStripButton();
			this.m_btnSpeedUp = new System.Windows.Forms.ToolStripButton();
			this.m_btnSlowDonw = new System.Windows.Forms.ToolStripButton();
			this.m_btnCheck = new System.Windows.Forms.ToolStripButton();
			this.m_tsFileFunction = new System.Windows.Forms.ToolStrip();
			this.m_tsbImport3DFile = new System.Windows.Forms.ToolStripButton();
			this.m_tsbReadProjectFile = new System.Windows.Forms.ToolStripButton();
			this.m_tsbSaveProjectFile = new System.Windows.Forms.ToolStripButton();
			this.m_tsCADFunction = new System.Windows.Forms.ToolStrip();
			this.m_tsbSewPart = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddPoint_AcrCenter = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddPoint_EdgeCenter = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddPoint_PointCenter = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddPoint_BoundingBoxCenter = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAddLine_TwoVertexConnect = new System.Windows.Forms.ToolStripButton();
			this.m_tsbManualTransform = new System.Windows.Forms.ToolStripButton();
			this.m_tsb3PntTransform = new System.Windows.Forms.ToolStripButton();
			this.m_tsbAxisTransform = new System.Windows.Forms.ToolStripButton();
			this.m_tscLevel2Container = new System.Windows.Forms.ToolStripContainer();
			this.m_tsAddPathSubFunc = new System.Windows.Forms.ToolStrip();
			this.m_tsAutoFindAlienatedWorkPieceBoundary = new System.Windows.Forms.ToolStripButton();
			this.m_tsAutoFindScretchWorkPieceBoundary = new System.Windows.Forms.ToolStripButton();
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
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip( this.components );
			this.m_msMode.SuspendLayout();
			this.m_tsCAMFunction.SuspendLayout();
			this.m_tscLevel1Container.ContentPanel.SuspendLayout();
			this.m_tscLevel1Container.SuspendLayout();
			this.m_tsSimuFunc.SuspendLayout();
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
			this.m_tsLayer.SuspendLayout();
			this.m_tsViewerToolBar.SuspendLayout();
			this.m_panBackGround.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_msMode
			// 
			this.m_msMode.ImageScalingSize = new System.Drawing.Size( 20, 20 );
			this.m_msMode.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
			this.m_tsmiFile,
			this.m_tsmiCAD,
			this.m_tsmiCAM,
			this.m_tsmiSimulation,
			this.m_tsbSetting,
			this.m_tsbController} );
			this.m_msMode.Location = new System.Drawing.Point( 0, 0 );
			this.m_msMode.Name = "m_msMode";
			this.m_msMode.Size = new System.Drawing.Size( 1596, 27 );
			this.m_msMode.TabIndex = 1;
			this.m_msMode.Text = "menuStrip1";
			// 
			// m_tsmiFile
			// 
			this.m_tsmiFile.Name = "m_tsmiFile";
			this.m_tsmiFile.Size = new System.Drawing.Size( 53, 23 );
			this.m_tsmiFile.Text = "檔案";
			this.m_tsmiFile.Click += new System.EventHandler( this.m_tsmiFile_Click );
			// 
			// m_tsmiCAD
			// 
			this.m_tsmiCAD.Name = "m_tsmiCAD";
			this.m_tsmiCAD.Size = new System.Drawing.Size( 54, 23 );
			this.m_tsmiCAD.Text = "CAD";
			this.m_tsmiCAD.Click += new System.EventHandler( this.m_tsmiCAD_Click );
			// 
			// m_tsmiCAM
			// 
			this.m_tsmiCAM.Name = "m_tsmiCAM";
			this.m_tsmiCAM.Size = new System.Drawing.Size( 57, 23 );
			this.m_tsmiCAM.Text = "CAM";
			this.m_tsmiCAM.Click += new System.EventHandler( this.m_tsmiCAM_Click );
			// 
			// m_tsmiSimulation
			// 
			this.m_tsmiSimulation.Name = "m_tsmiSimulation";
			this.m_tsmiSimulation.Size = new System.Drawing.Size( 53, 23 );
			this.m_tsmiSimulation.Text = "模擬";
			this.m_tsmiSimulation.Click += new System.EventHandler( this.m_tsmiSimulation_Click );
			// 
			// m_tsbSetting
			// 
			this.m_tsbSetting.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
			this.m_tsbVNCConnection} );
			this.m_tsbSetting.Name = "m_tsbSetting";
			this.m_tsbSetting.Size = new System.Drawing.Size( 53, 23 );
			this.m_tsbSetting.Text = "設定";
			// 
			// m_tsbVNCConnection
			// 
			this.m_tsbVNCConnection.Name = "m_tsbVNCConnection";
			this.m_tsbVNCConnection.Size = new System.Drawing.Size( 184, 26 );
			this.m_tsbVNCConnection.Text = "VNC連線設定";
			this.m_tsbVNCConnection.Click += new System.EventHandler( this.m_tsbVNCConnection_Click );
			// 
			// m_tsbController
			// 
			this.m_tsbController.Name = "m_tsbController";
			this.m_tsbController.Size = new System.Drawing.Size( 68, 23 );
			this.m_tsbController.Text = "控制器";
			this.m_tsbController.Click += new System.EventHandler( this.m_tsbController_Click );
			// 
			// m_tsCAMFunction
			// 
			this.m_tsCAMFunction.BackColor = System.Drawing.SystemColors.Control;
			this.m_tsCAMFunction.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsCAMFunction.ImageScalingSize = new System.Drawing.Size( 40, 40 );
			this.m_tsCAMFunction.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
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
			this.m_tsbPathEdit,
			this.m_BtnContourEdit,
			this.m_tsbPathCompensation,
			this.m_tsbCalibrationPnt,
			this.m_tsbConvertNC} );
			this.m_tsCAMFunction.Location = new System.Drawing.Point( 0, 0 );
			this.m_tsCAMFunction.Name = "m_tsCAMFunction";
			this.m_tsCAMFunction.Size = new System.Drawing.Size( 1596, 27 );
			this.m_tsCAMFunction.TabIndex = 2;
			this.m_tsCAMFunction.Text = "toolStrip1";
			// 
			// m_tsbAddPath
			// 
			this.m_tsbAddPath.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAddPath.Image = global::MyCAM.Properties.Resources.CAM_AddPath;
			this.m_tsbAddPath.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddPath.Name = "m_tsbAddPath";
			this.m_tsbAddPath.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbAddPath.Text = "新增路徑";
			this.m_tsbAddPath.ToolTipText = "新增路徑";
			this.m_tsbAddPath.Click += new System.EventHandler( this.m_tsbAddPath_Click );
			// 
			// m_tsbDeletePath
			// 
			this.m_tsbDeletePath.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbDeletePath.Image = global::MyCAM.Properties.Resources.CAM_DeletePath;
			this.m_tsbDeletePath.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbDeletePath.Name = "m_tsbDeletePath";
			this.m_tsbDeletePath.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbDeletePath.Text = "刪除路徑";
			this.m_tsbDeletePath.ToolTipText = "刪除路徑";
			this.m_tsbDeletePath.Click += new System.EventHandler( this.m_tsbDeletePath_Click );
			// 
			// m_tsbStartPoint
			// 
			this.m_tsbStartPoint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbStartPoint.Image = global::MyCAM.Properties.Resources.CAM_StartPointSetting;
			this.m_tsbStartPoint.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbStartPoint.Name = "m_tsbStartPoint";
			this.m_tsbStartPoint.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbStartPoint.Text = "起點";
			this.m_tsbStartPoint.ToolTipText = "起點";
			this.m_tsbStartPoint.Click += new System.EventHandler( this.m_tsbStartPoint_Click );
			// 
			// m_tsbReverse
			// 
			this.m_tsbReverse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbReverse.Image = global::MyCAM.Properties.Resources.CAM_Reverse;
			this.m_tsbReverse.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbReverse.Name = "m_tsbReverse";
			this.m_tsbReverse.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbReverse.Text = "反向";
			this.m_tsbReverse.ToolTipText = "反向";
			this.m_tsbReverse.Click += new System.EventHandler( this.m_tsbReverse_Click );
			// 
			// m_tsbSetLead
			// 
			this.m_tsbSetLead.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbSetLead.Image = global::MyCAM.Properties.Resources.CAM_Lead;
			this.m_tsbSetLead.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbSetLead.Name = "m_tsbSetLead";
			this.m_tsbSetLead.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbSetLead.Text = "引線";
			this.m_tsbSetLead.ToolTipText = "引線";
			this.m_tsbSetLead.Click += new System.EventHandler( this.m_tsbSetLead_Click );
			// 
			// m_tsbOverCut
			// 
			this.m_tsbOverCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbOverCut.Image = global::MyCAM.Properties.Resources.CAM_Overcut;
			this.m_tsbOverCut.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbOverCut.Name = "m_tsbOverCut";
			this.m_tsbOverCut.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbOverCut.Text = "過切";
			this.m_tsbOverCut.ToolTipText = "過切";
			this.m_tsbOverCut.Click += new System.EventHandler( this.m_tsbOverCut_Click );
			// 
			// m_tsbToolVecReverse
			// 
			this.m_tsbToolVecReverse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbToolVecReverse.Image = global::MyCAM.Properties.Resources.CAM_ToolVecReverse;
			this.m_tsbToolVecReverse.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbToolVecReverse.Name = "m_tsbToolVecReverse";
			this.m_tsbToolVecReverse.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbToolVecReverse.Text = "姿態反向";
			this.m_tsbToolVecReverse.ToolTipText = "姿態反向";
			this.m_tsbToolVecReverse.Click += new System.EventHandler( this.m_tsbTooVecReverse_Click );
			// 
			// m_tsbToolVec
			// 
			this.m_tsbToolVec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbToolVec.Image = global::MyCAM.Properties.Resources.CAM_ToolVec;
			this.m_tsbToolVec.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbToolVec.Name = "m_tsbToolVec";
			this.m_tsbToolVec.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbToolVec.Text = "姿態";
			this.m_tsbToolVec.ToolTipText = "姿態";
			this.m_tsbToolVec.Click += new System.EventHandler( this.m_tsbToolVec_Click );
			// 
			// m_tsbFixedToolVec
			// 
			this.m_tsbFixedToolVec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbFixedToolVec.Image = global::MyCAM.Properties.Resources.CAM_FixedToolVec;
			this.m_tsbFixedToolVec.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbFixedToolVec.Name = "m_tsbFixedToolVec";
			this.m_tsbFixedToolVec.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbFixedToolVec.Text = "定姿態切割";
			this.m_tsbFixedToolVec.ToolTipText = "定姿態切割";
			this.m_tsbFixedToolVec.Click += new System.EventHandler( this.m_tsbFixedToolVec_Click );
			// 
			// m_tsbMoveUp
			// 
			this.m_tsbMoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbMoveUp.Image = global::MyCAM.Properties.Resources.CAM_MoveUp;
			this.m_tsbMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbMoveUp.Name = "m_tsbMoveUp";
			this.m_tsbMoveUp.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbMoveUp.Text = "順序往前";
			this.m_tsbMoveUp.ToolTipText = "順序往前";
			this.m_tsbMoveUp.Click += new System.EventHandler( this.m_tsbMoveUp_Click );
			// 
			// m_tsbMoveDown
			// 
			this.m_tsbMoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbMoveDown.Image = global::MyCAM.Properties.Resources.CAM_MoveDown;
			this.m_tsbMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbMoveDown.Name = "m_tsbMoveDown";
			this.m_tsbMoveDown.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbMoveDown.Text = "順序往後";
			this.m_tsbMoveDown.ToolTipText = "順序往後";
			this.m_tsbMoveDown.Click += new System.EventHandler( this.m_tsbMoveDown_Click );
			// 
			// m_tsbAutoOrder
			// 
			this.m_tsbAutoOrder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAutoOrder.Image = global::MyCAM.Properties.Resources.CAM_AutoOrder;
			this.m_tsbAutoOrder.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAutoOrder.Name = "m_tsbAutoOrder";
			this.m_tsbAutoOrder.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbAutoOrder.Text = "自動排序";
			this.m_tsbAutoOrder.ToolTipText = "自動排序";
			this.m_tsbAutoOrder.Click += new System.EventHandler( this.m_tsbAutoOrder_Click );
			// 
			// m_tsbSetTraverseData
			// 
			this.m_tsbSetTraverseData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbSetTraverseData.Image = global::MyCAM.Properties.Resources.CAM_TraverseParam;
			this.m_tsbSetTraverseData.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbSetTraverseData.Name = "m_tsbSetTraverseData";
			this.m_tsbSetTraverseData.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbSetTraverseData.Text = "空移參數設定";
			this.m_tsbSetTraverseData.ToolTipText = "空移參數設定";
			this.m_tsbSetTraverseData.Click += new System.EventHandler( this.m_tsbSetTraverseData_Click );
			// 
			// m_tsbEntryAndExitSetting
			// 
			this.m_tsbEntryAndExitSetting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbEntryAndExitSetting.Image = global::MyCAM.Properties.Resources.CAM_EntryAndExit;
			this.m_tsbEntryAndExitSetting.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbEntryAndExitSetting.Name = "m_tsbEntryAndExitSetting";
			this.m_tsbEntryAndExitSetting.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbEntryAndExitSetting.Text = "進入/退出";
			this.m_tsbEntryAndExitSetting.ToolTipText = "進入/退出";
			this.m_tsbEntryAndExitSetting.Click += new System.EventHandler( this.m_tsbEntryAndExitSetting_Click );
			// 
			// m_tsbPatternSetting
			// 
			this.m_tsbPatternSetting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbPatternSetting.Image = global::MyCAM.Properties.Resources.CAM_StandardPattern;
			this.m_tsbPatternSetting.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbPatternSetting.Name = "m_tsbPatternSetting";
			this.m_tsbPatternSetting.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbPatternSetting.Text = "設定圖元";
			this.m_tsbPatternSetting.ToolTipText = "設定圖元";
			this.m_tsbPatternSetting.Click += new System.EventHandler( this.m_tsbPatternSetting_Click );
			// 
			// m_tsbPathEdit
			// 
			this.m_tsbPathEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbPathEdit.Image = global::MyCAM.Properties.Resources.CAM_PathEdit;
			this.m_tsbPathEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbPathEdit.Name = "m_tsbPathEdit";
			this.m_tsbPathEdit.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbPathEdit.Text = "路徑編輯";
			this.m_tsbPathEdit.ToolTipText = "路徑編輯";
			this.m_tsbPathEdit.Click += new System.EventHandler( this.m_tsbPathEdit_Click );
			// 
			// m_BtnContourEdit
			// 
			this.m_BtnContourEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_BtnContourEdit.Image = global::MyCAM.Properties.Resources.CAM_ContourEdit;
			this.m_BtnContourEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_BtnContourEdit.Name = "m_BtnContourEdit";
			this.m_BtnContourEdit.Size = new System.Drawing.Size( 44, 24 );
			this.m_BtnContourEdit.Text = "輪廓編輯";
			this.m_BtnContourEdit.ToolTipText = "輪廓編輯";
			this.m_BtnContourEdit.Click += new System.EventHandler( this.m_BtnContourEdit_Click );
			// 
			// m_tsbPathCompensation
			// 
			this.m_tsbPathCompensation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbPathCompensation.Image = global::MyCAM.Properties.Resources.CAM_PathCompensate;
			this.m_tsbPathCompensation.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbPathCompensation.Name = "m_tsbPathCompensation";
			this.m_tsbPathCompensation.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbPathCompensation.Text = "路徑補償";
			this.m_tsbPathCompensation.ToolTipText = "路徑補償";
			this.m_tsbPathCompensation.Click += new System.EventHandler( this.m_tsbPathCompensation_Click );
			// 
			// m_tsbCalibrationPnt
			// 
			this.m_tsbCalibrationPnt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbCalibrationPnt.Image = global::MyCAM.Properties.Resources.CAM_Calibration;
			this.m_tsbCalibrationPnt.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbCalibrationPnt.Name = "m_tsbCalibrationPnt";
			this.m_tsbCalibrationPnt.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbCalibrationPnt.Text = "三點校正參考點";
			this.m_tsbCalibrationPnt.ToolTipText = "三點校正參考點";
			this.m_tsbCalibrationPnt.Click += new System.EventHandler( this.m_tsbCalibrationPnt_Click );
			// 
			// m_tsbConvertNC
			// 
			this.m_tsbConvertNC.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbConvertNC.Image = global::MyCAM.Properties.Resources.CAM_NCFile;
			this.m_tsbConvertNC.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbConvertNC.Name = "m_tsbConvertNC";
			this.m_tsbConvertNC.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbConvertNC.Text = "轉出NC";
			this.m_tsbConvertNC.ToolTipText = "轉出NC";
			this.m_tsbConvertNC.Click += new System.EventHandler( this.m_tsbConvertNC_Click );
			// 
			// m_tscLevel1Container
			// 
			// 
			// m_tscLevel1Container.ContentPanel
			// 
			this.m_tscLevel1Container.ContentPanel.Controls.Add( this.m_tsCADFunction );
			this.m_tscLevel1Container.ContentPanel.Controls.Add( this.m_tsCAMFunction );
			this.m_tscLevel1Container.ContentPanel.Controls.Add( this.m_tsSimuFunc );
			this.m_tscLevel1Container.ContentPanel.Controls.Add( this.m_tsFileFunction );
			this.m_tscLevel1Container.ContentPanel.Size = new System.Drawing.Size( 1596, 50 );
			this.m_tscLevel1Container.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_tscLevel1Container.Location = new System.Drawing.Point( 0, 27 );
			this.m_tscLevel1Container.Name = "m_tscLevel1Container";
			this.m_tscLevel1Container.Size = new System.Drawing.Size( 1596, 75 );
			this.m_tscLevel1Container.TabIndex = 5;
			this.m_tscLevel1Container.Text = "toolStripContainer1";
			// 
			// m_tsSimuFunc
			// 
			this.m_tsSimuFunc.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsSimuFunc.ImageScalingSize = new System.Drawing.Size( 40, 40 );
			this.m_tsSimuFunc.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
			this.m_btnPlay,
			this.m_btnPause,
			this.m_btnStop,
			this.m_btnPreviousPath,
			this.m_btnNextPath,
			this.m_btnPreCollision,
			this.m_btnNextCol,
			this.m_btnPrePoint,
			this.m_btnNextPoint,
			this.m_btnSpeedUp,
			this.m_btnSlowDonw,
			this.m_btnCheck} );
			this.m_tsSimuFunc.Location = new System.Drawing.Point( 0, 0 );
			this.m_tsSimuFunc.Name = "m_tsSimuFunc";
			this.m_tsSimuFunc.Size = new System.Drawing.Size( 1596, 27 );
			this.m_tsSimuFunc.TabIndex = 5;
			this.m_tsSimuFunc.Text = "模擬功能";
			// 
			// m_btnPlay
			// 
			this.m_btnPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_btnPlay.Image = global::MyCAM.Properties.Resources.Simu_Play;
			this.m_btnPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_btnPlay.Name = "m_btnPlay";
			this.m_btnPlay.Size = new System.Drawing.Size( 44, 24 );
			this.m_btnPlay.Text = "播放模擬";
			this.m_btnPlay.ToolTipText = "播放模擬";
			this.m_btnPlay.Click += new System.EventHandler( this.m_btnPlay_Click );
			// 
			// m_btnPause
			// 
			this.m_btnPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_btnPause.Image = global::MyCAM.Properties.Resources.Simu_Pause;
			this.m_btnPause.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_btnPause.Name = "m_btnPause";
			this.m_btnPause.Size = new System.Drawing.Size( 44, 24 );
			this.m_btnPause.Text = "暫停";
			this.m_btnPause.ToolTipText = "暫停";
			this.m_btnPause.Click += new System.EventHandler( this.m_btnPause_Click );
			// 
			// m_btnStop
			// 
			this.m_btnStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_btnStop.Image = global::MyCAM.Properties.Resources.Simu_Stop;
			this.m_btnStop.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_btnStop.Name = "m_btnStop";
			this.m_btnStop.Size = new System.Drawing.Size( 44, 24 );
			this.m_btnStop.Text = "停止";
			this.m_btnStop.ToolTipText = "停止";
			this.m_btnStop.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.m_btnStop.Click += new System.EventHandler( this.m_btnStop_Click );
			// 
			// m_btnPreviousPath
			// 
			this.m_btnPreviousPath.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_btnPreviousPath.Image = global::MyCAM.Properties.Resources.Simu_PreviousPath;
			this.m_btnPreviousPath.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_btnPreviousPath.Name = "m_btnPreviousPath";
			this.m_btnPreviousPath.Size = new System.Drawing.Size( 44, 24 );
			this.m_btnPreviousPath.Text = "模擬上一條路徑";
			this.m_btnPreviousPath.ToolTipText = "模擬上一條路徑";
			this.m_btnPreviousPath.Click += new System.EventHandler( this.m_btnPreviousPath_Click );
			// 
			// m_btnNextPath
			// 
			this.m_btnNextPath.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_btnNextPath.Image = global::MyCAM.Properties.Resources.Simu_NextPath;
			this.m_btnNextPath.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_btnNextPath.Name = "m_btnNextPath";
			this.m_btnNextPath.Size = new System.Drawing.Size( 44, 24 );
			this.m_btnNextPath.Text = "模擬下一條路徑";
			this.m_btnNextPath.ToolTipText = "模擬下一條路徑";
			this.m_btnNextPath.Click += new System.EventHandler( this.m_btnNextPath_Click );
			// 
			// m_btnPreCollision
			// 
			this.m_btnPreCollision.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_btnPreCollision.Image = global::MyCAM.Properties.Resources.Simu_PreviousCol;
			this.m_btnPreCollision.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_btnPreCollision.Name = "m_btnPreCollision";
			this.m_btnPreCollision.Size = new System.Drawing.Size( 44, 24 );
			this.m_btnPreCollision.Text = "移至上一個碰撞點";
			this.m_btnPreCollision.ToolTipText = "移至上一個碰撞點";
			this.m_btnPreCollision.Click += new System.EventHandler( this.m_btnPreCollision_Click );
			// 
			// m_btnNextCol
			// 
			this.m_btnNextCol.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_btnNextCol.Image = global::MyCAM.Properties.Resources.Simu_NextCol;
			this.m_btnNextCol.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_btnNextCol.Name = "m_btnNextCol";
			this.m_btnNextCol.Size = new System.Drawing.Size( 44, 24 );
			this.m_btnNextCol.Text = "移至下一個碰撞點";
			this.m_btnNextCol.ToolTipText = "移至下一個碰撞點";
			this.m_btnNextCol.Click += new System.EventHandler( this.m_btnNextCol_Click );
			// 
			// m_btnPrePoint
			// 
			this.m_btnPrePoint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_btnPrePoint.Image = global::MyCAM.Properties.Resources.Simu_PrePoint;
			this.m_btnPrePoint.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_btnPrePoint.Name = "m_btnPrePoint";
			this.m_btnPrePoint.Size = new System.Drawing.Size( 44, 24 );
			this.m_btnPrePoint.Text = "上一個點";
			this.m_btnPrePoint.ToolTipText = "上一個點";
			this.m_btnPrePoint.Click += new System.EventHandler( this.m_btnPrePoint_Click );
			// 
			// m_btnNextPoint
			// 
			this.m_btnNextPoint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_btnNextPoint.Image = global::MyCAM.Properties.Resources.Simu_NextPoint;
			this.m_btnNextPoint.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_btnNextPoint.Name = "m_btnNextPoint";
			this.m_btnNextPoint.Size = new System.Drawing.Size( 44, 24 );
			this.m_btnNextPoint.Text = "下一個點";
			this.m_btnNextPoint.ToolTipText = "下一個點";
			this.m_btnNextPoint.Click += new System.EventHandler( this.m_btnNextPoint_Click );
			// 
			// m_btnSpeedUp
			// 
			this.m_btnSpeedUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_btnSpeedUp.Image = global::MyCAM.Properties.Resources.Simu_SpeedUp;
			this.m_btnSpeedUp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_btnSpeedUp.Name = "m_btnSpeedUp";
			this.m_btnSpeedUp.Size = new System.Drawing.Size( 44, 24 );
			this.m_btnSpeedUp.Text = "加速";
			this.m_btnSpeedUp.ToolTipText = "加速";
			this.m_btnSpeedUp.Click += new System.EventHandler( this.m_btnSpeedUp_Click );
			// 
			// m_btnSlowDonw
			// 
			this.m_btnSlowDonw.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_btnSlowDonw.Image = global::MyCAM.Properties.Resources.Simu_SlowDown;
			this.m_btnSlowDonw.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_btnSlowDonw.Name = "m_btnSlowDonw";
			this.m_btnSlowDonw.Size = new System.Drawing.Size( 44, 24 );
			this.m_btnSlowDonw.Text = "減速";
			this.m_btnSlowDonw.ToolTipText = "減速";
			this.m_btnSlowDonw.Click += new System.EventHandler( this.m_btnSlowDonw_Click );
			// 
			// m_btnCheck
			// 
			this.m_btnCheck.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_btnCheck.Image = global::MyCAM.Properties.Resources.Simu_Check;
			this.m_btnCheck.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_btnCheck.Name = "m_btnCheck";
			this.m_btnCheck.Size = new System.Drawing.Size( 44, 24 );
			this.m_btnCheck.Text = "檢查";
			this.m_btnCheck.ToolTipText = "檢查";
			this.m_btnCheck.Click += new System.EventHandler( this.m_btnCheck_Click );
			// 
			// m_tsFileFunction
			// 
			this.m_tsFileFunction.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsFileFunction.ImageScalingSize = new System.Drawing.Size( 40, 40 );
			this.m_tsFileFunction.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
			this.m_tsbImport3DFile,
			this.m_tsbReadProjectFile,
			this.m_tsbSaveProjectFile} );
			this.m_tsFileFunction.Location = new System.Drawing.Point( 0, 0 );
			this.m_tsFileFunction.Name = "m_tsFileFunction";
			this.m_tsFileFunction.Size = new System.Drawing.Size( 1596, 27 );
			this.m_tsFileFunction.TabIndex = 4;
			this.m_tsFileFunction.Text = "匯入3D模型";
			// 
			// m_tsbImport3DFile
			// 
			this.m_tsbImport3DFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbImport3DFile.Image = global::MyCAM.Properties.Resources.File_Import3DFile;
			this.m_tsbImport3DFile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbImport3DFile.Name = "m_tsbImport3DFile";
			this.m_tsbImport3DFile.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbImport3DFile.Text = "匯入3D模型";
			this.m_tsbImport3DFile.ToolTipText = "匯入3D模型";
			this.m_tsbImport3DFile.Click += new System.EventHandler( this.m_tsbImport3DFile_Click );
			// 
			// m_tsbReadProjectFile
			// 
			this.m_tsbReadProjectFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbReadProjectFile.Image = global::MyCAM.Properties.Resources.File_ReadProjectFile;
			this.m_tsbReadProjectFile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbReadProjectFile.Name = "m_tsbReadProjectFile";
			this.m_tsbReadProjectFile.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbReadProjectFile.Text = "匯入專案檔";
			this.m_tsbReadProjectFile.ToolTipText = "匯入專案檔";
			this.m_tsbReadProjectFile.Click += new System.EventHandler( this.m_tsbImportProjectFile_Click );
			// 
			// m_tsbSaveProjectFile
			// 
			this.m_tsbSaveProjectFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbSaveProjectFile.Image = global::MyCAM.Properties.Resources.File_SaveProjectFile;
			this.m_tsbSaveProjectFile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbSaveProjectFile.Name = "m_tsbSaveProjectFile";
			this.m_tsbSaveProjectFile.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbSaveProjectFile.Text = "儲存專案檔";
			this.m_tsbSaveProjectFile.ToolTipText = "儲存專案檔";
			this.m_tsbSaveProjectFile.Click += new System.EventHandler( this.m_tsbSaveProjectFile_Click );
			// 
			// m_tsCADFunction
			// 
			this.m_tsCADFunction.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsCADFunction.ImageScalingSize = new System.Drawing.Size( 40, 40 );
			this.m_tsCADFunction.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
			this.m_tsbSewPart,
			this.m_tsbAddPoint_AcrCenter,
			this.m_tsbAddPoint_EdgeCenter,
			this.m_tsbAddPoint_PointCenter,
			this.m_tsbAddPoint_BoundingBoxCenter,
			this.m_tsbAddLine_TwoVertexConnect,
			this.m_tsbManualTransform,
			this.m_tsb3PntTransform,
			this.m_tsbAxisTransform} );
			this.m_tsCADFunction.Location = new System.Drawing.Point( 0, 0 );
			this.m_tsCADFunction.Name = "m_tsCADFunction";
			this.m_tsCADFunction.Size = new System.Drawing.Size( 1596, 27 );
			this.m_tsCADFunction.TabIndex = 3;
			this.m_tsCADFunction.Text = "CAD功能";
			// 
			// m_tsbSewPart
			// 
			this.m_tsbSewPart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
			this.m_tsbSewPart.Image = global::MyCAM.Properties.Resources.CAD_SewPart;
			this.m_tsbSewPart.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbSewPart.Name = "m_tsbSewPart";
			this.m_tsbSewPart.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbSewPart.Text = "模型縫補";
			this.m_tsbSewPart.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbSewPart.ToolTipText = "模型縫補";
			this.m_tsbSewPart.Click += new System.EventHandler( this.m_tsbSewPart_Click );
			// 
			// m_tsbAddPoint_AcrCenter
			// 
			this.m_tsbAddPoint_AcrCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
			this.m_tsbAddPoint_AcrCenter.Image = global::MyCAM.Properties.Resources.CAD_AcrCenter;
			this.m_tsbAddPoint_AcrCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddPoint_AcrCenter.Name = "m_tsbAddPoint_AcrCenter";
			this.m_tsbAddPoint_AcrCenter.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbAddPoint_AcrCenter.Text = "新增圓弧中心";
			this.m_tsbAddPoint_AcrCenter.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAddPoint_AcrCenter.ToolTipText = "新增圓弧中心";
			this.m_tsbAddPoint_AcrCenter.Click += new System.EventHandler( this.m_tsbAddPoint_AcrCenter_Click );
			// 
			// m_tsbAddPoint_EdgeCenter
			// 
			this.m_tsbAddPoint_EdgeCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
			this.m_tsbAddPoint_EdgeCenter.Image = global::MyCAM.Properties.Resources.CAD_EdgeCenter;
			this.m_tsbAddPoint_EdgeCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddPoint_EdgeCenter.Name = "m_tsbAddPoint_EdgeCenter";
			this.m_tsbAddPoint_EdgeCenter.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbAddPoint_EdgeCenter.Text = "新增線段中心";
			this.m_tsbAddPoint_EdgeCenter.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAddPoint_EdgeCenter.ToolTipText = "新增線段中心";
			this.m_tsbAddPoint_EdgeCenter.Click += new System.EventHandler( this.m_tsbAddPoint_EdgeCenter_Click );
			// 
			// m_tsbAddPoint_PointCenter
			// 
			this.m_tsbAddPoint_PointCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
			this.m_tsbAddPoint_PointCenter.Image = global::MyCAM.Properties.Resources.CAD_PointCenter;
			this.m_tsbAddPoint_PointCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddPoint_PointCenter.Name = "m_tsbAddPoint_PointCenter";
			this.m_tsbAddPoint_PointCenter.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbAddPoint_PointCenter.Text = "新增兩點中心";
			this.m_tsbAddPoint_PointCenter.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAddPoint_PointCenter.ToolTipText = "新增兩點中心";
			this.m_tsbAddPoint_PointCenter.Click += new System.EventHandler( this.m_tsbAddPoint_PointCenter_Click );
			// 
			// m_tsbAddPoint_BoundingBoxCenter
			// 
			this.m_tsbAddPoint_BoundingBoxCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
			this.m_tsbAddPoint_BoundingBoxCenter.Image = global::MyCAM.Properties.Resources.CAD_BoundingBoxCenter;
			this.m_tsbAddPoint_BoundingBoxCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddPoint_BoundingBoxCenter.Name = "m_tsbAddPoint_BoundingBoxCenter";
			this.m_tsbAddPoint_BoundingBoxCenter.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbAddPoint_BoundingBoxCenter.Text = "新增包圍盒中心";
			this.m_tsbAddPoint_BoundingBoxCenter.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAddPoint_BoundingBoxCenter.ToolTipText = "新增包圍盒中心";
			this.m_tsbAddPoint_BoundingBoxCenter.Click += new System.EventHandler( this.m_tsbAddPoint_BoundingBoxCenter_Click );
			// 
			// m_tsbAddLine_TwoVertexConnect
			// 
			this.m_tsbAddLine_TwoVertexConnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
			this.m_tsbAddLine_TwoVertexConnect.Image = global::MyCAM.Properties.Resources.CAD_TwoVertexConnect;
			this.m_tsbAddLine_TwoVertexConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAddLine_TwoVertexConnect.Name = "m_tsbAddLine_TwoVertexConnect";
			this.m_tsbAddLine_TwoVertexConnect.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbAddLine_TwoVertexConnect.Text = "新增兩點連線";
			this.m_tsbAddLine_TwoVertexConnect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAddLine_TwoVertexConnect.ToolTipText = "新增兩點連線";
			this.m_tsbAddLine_TwoVertexConnect.Click += new System.EventHandler( this.m_tsbAddLine_TwoVertexConnect_Click );
			// 
			// m_tsbManualTransform
			// 
			this.m_tsbManualTransform.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
			this.m_tsbManualTransform.Image = global::MyCAM.Properties.Resources.CAD_ManualTransform;
			this.m_tsbManualTransform.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbManualTransform.Name = "m_tsbManualTransform";
			this.m_tsbManualTransform.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbManualTransform.Text = "手動轉換";
			this.m_tsbManualTransform.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbManualTransform.ToolTipText = "手動轉換";
			this.m_tsbManualTransform.Click += new System.EventHandler( this.m_tsbManualTransform_Click );
			// 
			// m_tsb3PntTransform
			// 
			this.m_tsb3PntTransform.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
			this.m_tsb3PntTransform.Image = global::MyCAM.Properties.Resources.CAM_ThreePntTrans;
			this.m_tsb3PntTransform.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsb3PntTransform.Name = "m_tsb3PntTransform";
			this.m_tsb3PntTransform.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsb3PntTransform.Text = "三點轉換";
			this.m_tsb3PntTransform.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsb3PntTransform.ToolTipText = "三點轉換";
			this.m_tsb3PntTransform.Click += new System.EventHandler( this.m_tsb3PntTransform_Click );
			// 
			// m_tsbAxisTransform
			// 
			this.m_tsbAxisTransform.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
			this.m_tsbAxisTransform.Image = global::MyCAM.Properties.Resources.CAD_AxisTrans;
			this.m_tsbAxisTransform.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAxisTransform.Name = "m_tsbAxisTransform";
			this.m_tsbAxisTransform.Size = new System.Drawing.Size( 44, 24 );
			this.m_tsbAxisTransform.Text = "軸向轉換";
			this.m_tsbAxisTransform.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.m_tsbAxisTransform.ToolTipText = "軸向轉換";
			this.m_tsbAxisTransform.Click += new System.EventHandler( this.m_tsbAxisTransform_Click );
			// 
			// m_tscLevel2Container
			// 
			// 
			// m_tscLevel2Container.ContentPanel
			// 
			this.m_tscLevel2Container.ContentPanel.Controls.Add( this.m_tsAddPathSubFunc );
			this.m_tscLevel2Container.ContentPanel.Controls.Add( this.m_tsManualTrans );
			this.m_tscLevel2Container.ContentPanel.Size = new System.Drawing.Size( 1596, 30 );
			this.m_tscLevel2Container.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_tscLevel2Container.Location = new System.Drawing.Point( 0, 79 );
			this.m_tscLevel2Container.Name = "m_tscLevel2Container";
			this.m_tscLevel2Container.Size = new System.Drawing.Size( 1596, 30 );
			this.m_tscLevel2Container.TabIndex = 6;
			this.m_tscLevel2Container.Text = "toolStripContainer1";
			this.m_tscLevel2Container.TopToolStripPanelVisible = false;
			// 
			// m_tsAddPathSubFunc
			// 
			this.m_tsAddPathSubFunc.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsAddPathSubFunc.ImageScalingSize = new System.Drawing.Size( 24, 24 );
			this.m_tsAddPathSubFunc.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
			this.m_tsAutoFindAlienatedWorkPieceBoundary,
			this.m_tsAutoFindScretchWorkPieceBoundary,
			this.m_tsbSelectD1ContFace,
			this.m_tsbSelPath_FreeBound,
			this.m_tsbSelPath_Manual} );
			this.m_tsAddPathSubFunc.Location = new System.Drawing.Point( 0, 0 );
			this.m_tsAddPathSubFunc.Name = "m_tsAddPathSubFunc";
			this.m_tsAddPathSubFunc.Size = new System.Drawing.Size( 1596, 30 );
			this.m_tsAddPathSubFunc.TabIndex = 5;
			this.m_tsAddPathSubFunc.Text = "tsAddPathSubFunc";
			// 
			// m_tsAutoFindAlienatedWorkPieceBoundary
			// 
			this.m_tsAutoFindAlienatedWorkPieceBoundary.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsAutoFindAlienatedWorkPieceBoundary.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsAutoFindAlienatedWorkPieceBoundary.Name = "m_tsAutoFindAlienatedWorkPieceBoundary";
			this.m_tsAutoFindAlienatedWorkPieceBoundary.Size = new System.Drawing.Size( 139, 27 );
			this.m_tsAutoFindAlienatedWorkPieceBoundary.Text = "異形蓋件-生成路徑";
			this.m_tsAutoFindAlienatedWorkPieceBoundary.Click += new System.EventHandler( this.m_tsAutoFindAlienatedWorkPieceBoundary_Click );
			// 
			// m_tsAutoFindScretchWorkPieceBoundary
			// 
			this.m_tsAutoFindScretchWorkPieceBoundary.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsAutoFindScretchWorkPieceBoundary.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsAutoFindScretchWorkPieceBoundary.Name = "m_tsAutoFindScretchWorkPieceBoundary";
			this.m_tsAutoFindScretchWorkPieceBoundary.Size = new System.Drawing.Size( 124, 27 );
			this.m_tsAutoFindScretchWorkPieceBoundary.Text = "管拉件-生成路徑";
			this.m_tsAutoFindScretchWorkPieceBoundary.Click += new System.EventHandler( this.m_tsAutoFindStretchedWorkPieceBoundary_Click );
			// 
			// m_tsbSelectD1ContFace
			// 
			this.m_tsbSelectD1ContFace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbSelectD1ContFace.Name = "m_tsbSelectD1ContFace";
			this.m_tsbSelectD1ContFace.Size = new System.Drawing.Size( 67, 27 );
			this.m_tsbSelectD1ContFace.Text = "D1 連續";
			this.m_tsbSelectD1ContFace.Click += new System.EventHandler( this.m_tsbSelectD1ContFace_Click );
			// 
			// m_tsbSelPath_FreeBound
			// 
			this.m_tsbSelPath_FreeBound.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbSelPath_FreeBound.Name = "m_tsbSelPath_FreeBound";
			this.m_tsbSelPath_FreeBound.Size = new System.Drawing.Size( 88, 27 );
			this.m_tsbSelPath_FreeBound.Text = "所有面邊界";
			this.m_tsbSelPath_FreeBound.Click += new System.EventHandler( this.m_tsbSelPath_FreeBound_Click );
			// 
			// m_tsbSelPath_Manual
			// 
			this.m_tsbSelPath_Manual.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbSelPath_Manual.Name = "m_tsbSelPath_Manual";
			this.m_tsbSelPath_Manual.Size = new System.Drawing.Size( 73, 27 );
			this.m_tsbSelPath_Manual.Text = "手動選取";
			this.m_tsbSelPath_Manual.Click += new System.EventHandler( this.m_tsbSelPath_Manual_Click );
			// 
			// m_tsManualTrans
			// 
			this.m_tsManualTrans.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsManualTrans.ImageScalingSize = new System.Drawing.Size( 24, 24 );
			this.m_tsManualTrans.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
			this.m_tsbManualTransPlane,
			this.m_tsbManualTransPlanePar,
			this.m_tsbManualTransAxial,
			this.m_tsbManualTransAxialPar,
			this.m_tsbManualTransPointCoincide} );
			this.m_tsManualTrans.Location = new System.Drawing.Point( 0, 0 );
			this.m_tsManualTrans.Name = "m_tsManualTrans";
			this.m_tsManualTrans.Size = new System.Drawing.Size( 1596, 30 );
			this.m_tsManualTrans.TabIndex = 0;
			this.m_tsManualTrans.Text = "toolStrip1";
			// 
			// m_tsbManualTransPlane
			// 
			this.m_tsbManualTransPlane.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbManualTransPlane.Name = "m_tsbManualTransPlane";
			this.m_tsbManualTransPlane.RightToLeftAutoMirrorImage = true;
			this.m_tsbManualTransPlane.Size = new System.Drawing.Size( 58, 27 );
			this.m_tsbManualTransPlane.Text = "面重合";
			this.m_tsbManualTransPlane.Click += new System.EventHandler( this.m_tsbManualTransPlane_Click );
			// 
			// m_tsbManualTransPlanePar
			// 
			this.m_tsbManualTransPlanePar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbManualTransPlanePar.Name = "m_tsbManualTransPlanePar";
			this.m_tsbManualTransPlanePar.Size = new System.Drawing.Size( 58, 27 );
			this.m_tsbManualTransPlanePar.Text = "面平行";
			this.m_tsbManualTransPlanePar.Click += new System.EventHandler( this.m_tsbManualTransPlanePar_Click );
			// 
			// m_tsbManualTransAxial
			// 
			this.m_tsbManualTransAxial.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbManualTransAxial.Name = "m_tsbManualTransAxial";
			this.m_tsbManualTransAxial.Size = new System.Drawing.Size( 58, 27 );
			this.m_tsbManualTransAxial.Text = "軸重合";
			this.m_tsbManualTransAxial.Click += new System.EventHandler( this.m_tsbManualTransAxial_Click );
			// 
			// m_tsbManualTransAxialPar
			// 
			this.m_tsbManualTransAxialPar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbManualTransAxialPar.Name = "m_tsbManualTransAxialPar";
			this.m_tsbManualTransAxialPar.Size = new System.Drawing.Size( 58, 27 );
			this.m_tsbManualTransAxialPar.Text = "軸平行";
			this.m_tsbManualTransAxialPar.Click += new System.EventHandler( this.m_tsbManualTransAxialPar_Click );
			// 
			// m_tsbManualTransPointCoincide
			// 
			this.m_tsbManualTransPointCoincide.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbManualTransPointCoincide.Name = "m_tsbManualTransPointCoincide";
			this.m_tsbManualTransPointCoincide.Size = new System.Drawing.Size( 58, 27 );
			this.m_tsbManualTransPointCoincide.Text = "點重合";
			this.m_tsbManualTransPointCoincide.Click += new System.EventHandler( this.m_tsbManualTransPointCoincide_Click );
			// 
			// m_tscLevel3Container
			// 
			this.m_tscLevel3Container.BottomToolStripPanelVisible = false;
			// 
			// m_tscLevel3Container.ContentPanel
			// 
			this.m_tscLevel3Container.ContentPanel.Controls.Add( this.m_tsSelectPath );
			this.m_tscLevel3Container.ContentPanel.Size = new System.Drawing.Size( 1596, 24 );
			this.m_tscLevel3Container.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_tscLevel3Container.LeftToolStripPanelVisible = false;
			this.m_tscLevel3Container.Location = new System.Drawing.Point( 0, 109 );
			this.m_tscLevel3Container.Name = "m_tscLevel3Container";
			this.m_tscLevel3Container.RightToolStripPanelVisible = false;
			this.m_tscLevel3Container.Size = new System.Drawing.Size( 1596, 24 );
			this.m_tscLevel3Container.TabIndex = 8;
			this.m_tscLevel3Container.Text = "toolStripContainer1";
			this.m_tscLevel3Container.TopToolStripPanelVisible = false;
			// 
			// m_tsSelectPath
			// 
			this.m_tsSelectPath.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tsSelectPath.ImageScalingSize = new System.Drawing.Size( 24, 24 );
			this.m_tsSelectPath.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
			this.m_tsbManualSelectPathOK} );
			this.m_tsSelectPath.Location = new System.Drawing.Point( 0, 0 );
			this.m_tsSelectPath.Name = "m_tsSelectPath";
			this.m_tsSelectPath.Size = new System.Drawing.Size( 1596, 24 );
			this.m_tsSelectPath.TabIndex = 1;
			this.m_tsSelectPath.Text = "SelectPath";
			// 
			// m_tsbManualSelectPathOK
			// 
			this.m_tsbManualSelectPathOK.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbManualSelectPathOK.Name = "m_tsbManualSelectPathOK";
			this.m_tsbManualSelectPathOK.Size = new System.Drawing.Size( 103, 21 );
			this.m_tsbManualSelectPathOK.Text = "手動選取完成";
			this.m_tsbManualSelectPathOK.Click += new System.EventHandler( this.m_tsbManualSelectPathOK_Click );
			// 
			// m_panVNC
			// 
			this.m_panVNC.BackColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 238 ) ) ) ), ( (int)( ( (byte)( 239 ) ) ) ), ( (int)( ( (byte)( 244 ) ) ) ) );
			this.m_panBackGround.SetColumnSpan( this.m_panVNC, 2 );
			this.m_panVNC.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panVNC.Location = new System.Drawing.Point( 3, 494 );
			this.m_panVNC.Name = "m_panVNC";
			this.m_panVNC.Size = new System.Drawing.Size( 1590, 431 );
			this.m_panVNC.TabIndex = 4;
			// 
			// m_panTreeView
			// 
			this.m_panTreeView.BackColor = System.Drawing.SystemColors.ButtonHighlight;
			this.m_panTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panTreeView.Location = new System.Drawing.Point( 3, 3 );
			this.m_panTreeView.Name = "m_panTreeView";
			this.m_panTreeView.Size = new System.Drawing.Size( 233, 430 );
			this.m_panTreeView.TabIndex = 0;
			// 
			// m_pnlLog
			// 
			this.m_pnlLog.AutoScroll = true;
			this.m_pnlLog.BackColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 238 ) ) ) ), ( (int)( ( (byte)( 239 ) ) ) ), ( (int)( ( (byte)( 244 ) ) ) ) );
			this.m_panBackGround.SetColumnSpan( this.m_pnlLog, 2 );
			this.m_pnlLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_pnlLog.Font = new System.Drawing.Font( "Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
			this.m_pnlLog.Location = new System.Drawing.Point( 3, 439 );
			this.m_pnlLog.Name = "m_pnlLog";
			this.m_pnlLog.Size = new System.Drawing.Size( 1590, 49 );
			this.m_pnlLog.TabIndex = 7;
			// 
			// m_panViewer
			// 
			this.m_panViewer.BackColor = System.Drawing.SystemColors.Window;
			this.m_panViewer.Controls.Add( this.m_tsLayer );
			this.m_panViewer.Controls.Add( this.m_tsViewerToolBar );
			this.m_panViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panViewer.Location = new System.Drawing.Point( 242, 3 );
			this.m_panViewer.Name = "m_panViewer";
			this.m_panViewer.Size = new System.Drawing.Size( 1351, 430 );
			this.m_panViewer.TabIndex = 1;
			// 
			// m_tsLayer
			// 
			this.m_tsLayer.Dock = System.Windows.Forms.DockStyle.Right;
			this.m_tsLayer.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.m_tsLayer.ImageScalingSize = new System.Drawing.Size( 20, 20 );
			this.m_tsLayer.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
			this.m_tsbLayer1,
			this.m_tsbLayer2,
			this.m_tsbLayer3,
			this.m_tsbLayer4,
			this.m_tsbLayer5,
			this.m_tsbLayer6,
			this.m_tsbLayer7,
			this.m_tsbLayer8,
			this.m_tsbLayer9,
			this.m_tsbLayer10} );
			this.m_tsLayer.Location = new System.Drawing.Point( 1326, 0 );
			this.m_tsLayer.Name = "m_tsLayer";
			this.m_tsLayer.Size = new System.Drawing.Size( 25, 430 );
			this.m_tsLayer.TabIndex = 3;
			this.m_tsLayer.Text = "toolStrip1";
			// 
			// m_tsbLayer1
			// 
			this.m_tsbLayer1.AutoSize = false;
			this.m_tsbLayer1.BackColor = System.Drawing.Color.Blue;
			this.m_tsbLayer1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer1.ForeColor = System.Drawing.Color.White;
			this.m_tsbLayer1.Name = "m_tsbLayer1";
			this.m_tsbLayer1.Size = new System.Drawing.Size( 24, 24 );
			this.m_tsbLayer1.Text = "1";
			this.m_tsbLayer1.ToolTipText = "圖層1";
			this.m_tsbLayer1.Click += new System.EventHandler( this.m_tsbLayer1_Click );
			// 
			// m_tsbLayer2
			// 
			this.m_tsbLayer2.AutoSize = false;
			this.m_tsbLayer2.BackColor = System.Drawing.Color.Orange;
			this.m_tsbLayer2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer2.Name = "m_tsbLayer2";
			this.m_tsbLayer2.Size = new System.Drawing.Size( 24, 24 );
			this.m_tsbLayer2.Text = "2";
			this.m_tsbLayer2.ToolTipText = "圖層2";
			this.m_tsbLayer2.Click += new System.EventHandler( this.m_tsbLayer2_Click );
			// 
			// m_tsbLayer3
			// 
			this.m_tsbLayer3.AutoSize = false;
			this.m_tsbLayer3.BackColor = System.Drawing.Color.Purple;
			this.m_tsbLayer3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer3.ForeColor = System.Drawing.Color.White;
			this.m_tsbLayer3.Name = "m_tsbLayer3";
			this.m_tsbLayer3.Size = new System.Drawing.Size( 24, 24 );
			this.m_tsbLayer3.Text = "3";
			this.m_tsbLayer3.ToolTipText = "圖層3";
			this.m_tsbLayer3.Click += new System.EventHandler( this.m_tsbLayer3_Click );
			// 
			// m_tsbLayer4
			// 
			this.m_tsbLayer4.AutoSize = false;
			this.m_tsbLayer4.BackColor = System.Drawing.Color.Yellow;
			this.m_tsbLayer4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer4.Name = "m_tsbLayer4";
			this.m_tsbLayer4.Size = new System.Drawing.Size( 24, 24 );
			this.m_tsbLayer4.Text = "4";
			this.m_tsbLayer4.ToolTipText = "圖層4";
			this.m_tsbLayer4.Click += new System.EventHandler( this.m_tsbLayer4_Click );
			// 
			// m_tsbLayer5
			// 
			this.m_tsbLayer5.AutoSize = false;
			this.m_tsbLayer5.BackColor = System.Drawing.Color.Green;
			this.m_tsbLayer5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer5.ForeColor = System.Drawing.Color.White;
			this.m_tsbLayer5.Name = "m_tsbLayer5";
			this.m_tsbLayer5.Size = new System.Drawing.Size( 24, 24 );
			this.m_tsbLayer5.Text = "5";
			this.m_tsbLayer5.ToolTipText = "圖層5";
			this.m_tsbLayer5.Click += new System.EventHandler( this.m_tsbLayer5_Click );
			// 
			// m_tsbLayer6
			// 
			this.m_tsbLayer6.AutoSize = false;
			this.m_tsbLayer6.BackColor = System.Drawing.Color.Tomato;
			this.m_tsbLayer6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer6.Name = "m_tsbLayer6";
			this.m_tsbLayer6.Size = new System.Drawing.Size( 24, 24 );
			this.m_tsbLayer6.Text = "6";
			this.m_tsbLayer6.ToolTipText = "圖層6";
			this.m_tsbLayer6.Click += new System.EventHandler( this.m_tsbLayer6_Click );
			// 
			// m_tsbLayer7
			// 
			this.m_tsbLayer7.AutoSize = false;
			this.m_tsbLayer7.BackColor = System.Drawing.Color.GreenYellow;
			this.m_tsbLayer7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer7.Name = "m_tsbLayer7";
			this.m_tsbLayer7.Size = new System.Drawing.Size( 24, 24 );
			this.m_tsbLayer7.Text = "7";
			this.m_tsbLayer7.ToolTipText = "圖層7";
			this.m_tsbLayer7.Click += new System.EventHandler( this.m_tsbLayer7_Click );
			// 
			// m_tsbLayer8
			// 
			this.m_tsbLayer8.AutoSize = false;
			this.m_tsbLayer8.BackColor = System.Drawing.Color.Sienna;
			this.m_tsbLayer8.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer8.ForeColor = System.Drawing.Color.White;
			this.m_tsbLayer8.Name = "m_tsbLayer8";
			this.m_tsbLayer8.Size = new System.Drawing.Size( 24, 24 );
			this.m_tsbLayer8.Text = "8";
			this.m_tsbLayer8.ToolTipText = "圖層8";
			this.m_tsbLayer8.Click += new System.EventHandler( this.m_tsbLayer8_Click );
			// 
			// m_tsbLayer9
			// 
			this.m_tsbLayer9.AutoSize = false;
			this.m_tsbLayer9.BackColor = System.Drawing.Color.Fuchsia;
			this.m_tsbLayer9.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer9.Name = "m_tsbLayer9";
			this.m_tsbLayer9.Size = new System.Drawing.Size( 24, 24 );
			this.m_tsbLayer9.Text = "9";
			this.m_tsbLayer9.ToolTipText = "圖層9";
			this.m_tsbLayer9.Click += new System.EventHandler( this.m_tsbLayer9_Click );
			// 
			// m_tsbLayer10
			// 
			this.m_tsbLayer10.AutoSize = false;
			this.m_tsbLayer10.BackColor = System.Drawing.Color.Cyan;
			this.m_tsbLayer10.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbLayer10.Name = "m_tsbLayer10";
			this.m_tsbLayer10.Size = new System.Drawing.Size( 24, 24 );
			this.m_tsbLayer10.Text = "10";
			this.m_tsbLayer10.ToolTipText = "圖層10";
			this.m_tsbLayer10.Click += new System.EventHandler( this.m_tsbLayer10_Click );
			// 
			// m_tsViewerToolBar
			// 
			this.m_tsViewerToolBar.BackColor = System.Drawing.SystemColors.Window;
			this.m_tsViewerToolBar.Dock = System.Windows.Forms.DockStyle.Left;
			this.m_tsViewerToolBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.m_tsViewerToolBar.ImageScalingSize = new System.Drawing.Size( 20, 20 );
			this.m_tsViewerToolBar.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
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
			this.m_tsbShowTraverse} );
			this.m_tsViewerToolBar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.m_tsViewerToolBar.Location = new System.Drawing.Point( 0, 0 );
			this.m_tsViewerToolBar.Name = "m_tsViewerToolBar";
			this.m_tsViewerToolBar.Size = new System.Drawing.Size( 30, 430 );
			this.m_tsViewerToolBar.TabIndex = 2;
			this.m_tsViewerToolBar.Text = "toolStrip1";
			// 
			// m_tsbAXO
			// 
			this.m_tsbAXO.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbAXO.Image = global::MyCAM.Properties.Resources.View_Axo;
			this.m_tsbAXO.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbAXO.Name = "m_tsbAXO";
			this.m_tsbAXO.Size = new System.Drawing.Size( 27, 24 );
			this.m_tsbAXO.Text = "AXO";
			this.m_tsbAXO.ToolTipText = "AXO";
			this.m_tsbAXO.Click += new System.EventHandler( this.m_tsbAXO_Click );
			// 
			// m_tsbXPos
			// 
			this.m_tsbXPos.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbXPos.Image = global::MyCAM.Properties.Resources.View_Right_View;
			this.m_tsbXPos.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbXPos.Name = "m_tsbXPos";
			this.m_tsbXPos.Size = new System.Drawing.Size( 27, 24 );
			this.m_tsbXPos.Text = "X+";
			this.m_tsbXPos.ToolTipText = "X+";
			this.m_tsbXPos.Click += new System.EventHandler( this.m_tsbXPos_Click );
			// 
			// m_tsbXNeg
			// 
			this.m_tsbXNeg.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbXNeg.Image = global::MyCAM.Properties.Resources.View_LeftView;
			this.m_tsbXNeg.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbXNeg.Name = "m_tsbXNeg";
			this.m_tsbXNeg.Size = new System.Drawing.Size( 27, 24 );
			this.m_tsbXNeg.Text = "X-";
			this.m_tsbXNeg.ToolTipText = "X-";
			this.m_tsbXNeg.Click += new System.EventHandler( this.m_tsbXNeg_Click );
			// 
			// m_tsbYPos
			// 
			this.m_tsbYPos.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbYPos.Image = global::MyCAM.Properties.Resources.View_MainView;
			this.m_tsbYPos.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbYPos.Name = "m_tsbYPos";
			this.m_tsbYPos.Size = new System.Drawing.Size( 27, 24 );
			this.m_tsbYPos.Text = "Y+";
			this.m_tsbYPos.ToolTipText = "Y+";
			this.m_tsbYPos.Click += new System.EventHandler( this.m_tsbYPos_Click );
			// 
			// m_tsbYNeg
			// 
			this.m_tsbYNeg.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbYNeg.Image = global::MyCAM.Properties.Resources.View_BackView;
			this.m_tsbYNeg.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbYNeg.Name = "m_tsbYNeg";
			this.m_tsbYNeg.Size = new System.Drawing.Size( 27, 24 );
			this.m_tsbYNeg.Text = "Y-";
			this.m_tsbYNeg.ToolTipText = "Y-";
			this.m_tsbYNeg.Click += new System.EventHandler( this.m_tsbYNeg_Click );
			// 
			// m_tsbZPos
			// 
			this.m_tsbZPos.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbZPos.Image = global::MyCAM.Properties.Resources.View_TopView;
			this.m_tsbZPos.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbZPos.Name = "m_tsbZPos";
			this.m_tsbZPos.Size = new System.Drawing.Size( 27, 24 );
			this.m_tsbZPos.Text = "Z+";
			this.m_tsbZPos.ToolTipText = "Z+";
			this.m_tsbZPos.Click += new System.EventHandler( this.m_tsbZPos_Click );
			// 
			// m_tsbZNeg
			// 
			this.m_tsbZNeg.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbZNeg.Image = global::MyCAM.Properties.Resources.View_BottomView;
			this.m_tsbZNeg.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbZNeg.Name = "m_tsbZNeg";
			this.m_tsbZNeg.Size = new System.Drawing.Size( 27, 24 );
			this.m_tsbZNeg.Text = "Z-";
			this.m_tsbZNeg.ToolTipText = "Z-";
			this.m_tsbZNeg.Click += new System.EventHandler( this.m_tsbZNeg_Click );
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size( 27, 6 );
			// 
			// m_tsbShowVec
			// 
			this.m_tsbShowVec.Checked = true;
			this.m_tsbShowVec.CheckOnClick = true;
			this.m_tsbShowVec.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tsbShowVec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbShowVec.Image = global::MyCAM.Properties.Resources.CAM_ToolVec;
			this.m_tsbShowVec.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbShowVec.Name = "m_tsbShowVec";
			this.m_tsbShowVec.Size = new System.Drawing.Size( 27, 24 );
			this.m_tsbShowVec.Text = "顯示姿態";
			this.m_tsbShowVec.ToolTipText = "顯示姿態";
			this.m_tsbShowVec.CheckedChanged += new System.EventHandler( this.m_tsbShowVec_CheckedChanged );
			// 
			// m_tsbShowOrder
			// 
			this.m_tsbShowOrder.Checked = true;
			this.m_tsbShowOrder.CheckOnClick = true;
			this.m_tsbShowOrder.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tsbShowOrder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbShowOrder.Image = global::MyCAM.Properties.Resources.CAM_AutoOrder;
			this.m_tsbShowOrder.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbShowOrder.Name = "m_tsbShowOrder";
			this.m_tsbShowOrder.Size = new System.Drawing.Size( 27, 24 );
			this.m_tsbShowOrder.Text = "顯示順序";
			this.m_tsbShowOrder.ToolTipText = "顯示順序";
			this.m_tsbShowOrder.CheckedChanged += new System.EventHandler( this.m_tsbShowOrder_CheckedChanged );
			// 
			// m_tsbShowOrientation
			// 
			this.m_tsbShowOrientation.Checked = true;
			this.m_tsbShowOrientation.CheckOnClick = true;
			this.m_tsbShowOrientation.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tsbShowOrientation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbShowOrientation.Image = global::MyCAM.Properties.Resources.CAM_Reverse;
			this.m_tsbShowOrientation.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbShowOrientation.Name = "m_tsbShowOrientation";
			this.m_tsbShowOrientation.Size = new System.Drawing.Size( 27, 24 );
			this.m_tsbShowOrientation.Text = "顯示路徑方向";
			this.m_tsbShowOrientation.ToolTipText = "顯示路徑方向";
			this.m_tsbShowOrientation.CheckedChanged += new System.EventHandler( this.m_tsbShowOrientation_CheckedChanged );
			// 
			// m_tsbShowTraverse
			// 
			this.m_tsbShowTraverse.Checked = true;
			this.m_tsbShowTraverse.CheckOnClick = true;
			this.m_tsbShowTraverse.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tsbShowTraverse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbShowTraverse.Image = global::MyCAM.Properties.Resources.CAM_TraverseParam;
			this.m_tsbShowTraverse.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbShowTraverse.Name = "m_tsbShowTraverse";
			this.m_tsbShowTraverse.Size = new System.Drawing.Size( 27, 24 );
			this.m_tsbShowTraverse.Text = "顯示抬下刀路徑";
			this.m_tsbShowTraverse.ToolTipText = "顯示抬下刀路徑";
			this.m_tsbShowTraverse.CheckedChanged += new System.EventHandler( this.m_tsbShowTraverse_CheckedChanged );
			// 
			// m_panBackGround
			// 
			this.m_panBackGround.ColumnCount = 2;
			this.m_panBackGround.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle( System.Windows.Forms.SizeType.Percent, 15F ) );
			this.m_panBackGround.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle( System.Windows.Forms.SizeType.Percent, 85F ) );
			this.m_panBackGround.Controls.Add( this.m_panVNC, 0, 2 );
			this.m_panBackGround.Controls.Add( this.m_panTreeView, 0, 0 );
			this.m_panBackGround.Controls.Add( this.m_pnlLog, 0, 1 );
			this.m_panBackGround.Controls.Add( this.m_panViewer, 1, 0 );
			this.m_panBackGround.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panBackGround.Location = new System.Drawing.Point( 0, 133 );
			this.m_panBackGround.Name = "m_panBackGround";
			this.m_panBackGround.RowCount = 3;
			this.m_panBackGround.RowStyles.Add( new System.Windows.Forms.RowStyle( System.Windows.Forms.SizeType.Percent, 47F ) );
			this.m_panBackGround.RowStyles.Add( new System.Windows.Forms.RowStyle( System.Windows.Forms.SizeType.Percent, 6F ) );
			this.m_panBackGround.RowStyles.Add( new System.Windows.Forms.RowStyle( System.Windows.Forms.SizeType.Percent, 47F ) );
			this.m_panBackGround.Size = new System.Drawing.Size( 1596, 928 );
			this.m_panBackGround.TabIndex = 9;
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size( 24, 24 );
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size( 61, 4 );
			// 
			// StartupForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size( 1596, 1061 );
			this.Controls.Add( this.m_panBackGround );
			this.Controls.Add( this.m_tscLevel3Container );
			this.Controls.Add( this.m_tscLevel2Container );
			this.Controls.Add( this.m_tscLevel1Container );
			this.Controls.Add( this.m_msMode );
			this.MainMenuStrip = this.m_msMode;
			this.Margin = new System.Windows.Forms.Padding( 4 );
			this.Name = "StartupForm";
			this.Text = "五軸切";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler( this.StartupForm_FormClosing );
			this.Resize += new System.EventHandler( this.StartupForm_Resize );
			this.m_msMode.ResumeLayout( false );
			this.m_msMode.PerformLayout();
			this.m_tsCAMFunction.ResumeLayout( false );
			this.m_tsCAMFunction.PerformLayout();
			this.m_tscLevel1Container.ContentPanel.ResumeLayout( false );
			this.m_tscLevel1Container.ContentPanel.PerformLayout();
			this.m_tscLevel1Container.ResumeLayout( false );
			this.m_tscLevel1Container.PerformLayout();
			this.m_tsSimuFunc.ResumeLayout( false );
			this.m_tsSimuFunc.PerformLayout();
			this.m_tsFileFunction.ResumeLayout( false );
			this.m_tsFileFunction.PerformLayout();
			this.m_tsCADFunction.ResumeLayout( false );
			this.m_tsCADFunction.PerformLayout();
			this.m_tscLevel2Container.ContentPanel.ResumeLayout( false );
			this.m_tscLevel2Container.ContentPanel.PerformLayout();
			this.m_tscLevel2Container.ResumeLayout( false );
			this.m_tscLevel2Container.PerformLayout();
			this.m_tsAddPathSubFunc.ResumeLayout( false );
			this.m_tsAddPathSubFunc.PerformLayout();
			this.m_tsManualTrans.ResumeLayout( false );
			this.m_tsManualTrans.PerformLayout();
			this.m_tscLevel3Container.ContentPanel.ResumeLayout( false );
			this.m_tscLevel3Container.ContentPanel.PerformLayout();
			this.m_tscLevel3Container.ResumeLayout( false );
			this.m_tscLevel3Container.PerformLayout();
			this.m_tsSelectPath.ResumeLayout( false );
			this.m_tsSelectPath.PerformLayout();
			this.m_panViewer.ResumeLayout( false );
			this.m_panViewer.PerformLayout();
			this.m_tsLayer.ResumeLayout( false );
			this.m_tsLayer.PerformLayout();
			this.m_tsViewerToolBar.ResumeLayout( false );
			this.m_tsViewerToolBar.PerformLayout();
			this.m_panBackGround.ResumeLayout( false );
			this.ResumeLayout( false );
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
		private System.Windows.Forms.ToolStripButton m_tsbAddPoint_BoundingBoxCenter;
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
		private System.Windows.Forms.ToolStripMenuItem m_tsmiSimulation;
		private System.Windows.Forms.ToolStrip m_tsSimuFunc;
		private System.Windows.Forms.ToolStripButton m_btnPlay;
		private System.Windows.Forms.ToolStripButton m_btnPause;
		private System.Windows.Forms.ToolStripButton m_btnStop;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripButton m_btnPreviousPath;
		private System.Windows.Forms.ToolStripButton m_btnNextPath;
		private System.Windows.Forms.ToolStripButton m_btnPreCollision;
		private System.Windows.Forms.ToolStripButton m_btnNextCol;
		private System.Windows.Forms.ToolStripButton m_btnSpeedUp;
		private System.Windows.Forms.ToolStripButton m_btnSlowDonw;
		private System.Windows.Forms.ToolStripButton m_btnCheck;
		private System.Windows.Forms.ToolStripButton m_tsbPathEdit;
		private System.Windows.Forms.ToolStripButton m_tsbPathCompensation;
		private System.Windows.Forms.ToolStripButton m_btnPrePoint;
		private System.Windows.Forms.ToolStripButton m_btnNextPoint;
		private System.Windows.Forms.ToolStripMenuItem m_tsbController;
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
		private System.Windows.Forms.ToolStripButton m_BtnContourEdit;
		private System.Windows.Forms.ToolStripButton m_tsAutoFindAlienatedWorkPieceBoundary;
		private System.Windows.Forms.ToolStripButton m_tsAutoFindScretchWorkPieceBoundary;
	}
}

