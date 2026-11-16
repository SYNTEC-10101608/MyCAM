namespace MyCAM.Editor.Dialog
{
	partial class DesignSupportBracketDlg
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
			this.m_pnlSelectFacePage = new System.Windows.Forms.Panel();
			this.m_pnlSelectFace = new CommonUI.Controls.SectionPanel();
			this.m_lblHint = new System.Windows.Forms.Label();
			this.m_btnNext = new CommonUI.Controls.ThemedButton();
			this.m_pnlInputParametersPage = new System.Windows.Forms.Panel();
			this.m_pnlBase = new CommonUI.Controls.SectionPanel();
			this.m_lblXExtension = new System.Windows.Forms.Label();
			this.m_numXExtension = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblXExtensionUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblYExtension = new System.Windows.Forms.Label();
			this.m_numYExtension = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblYExtensionUnit = new CommonUI.Controls.ThemedLabel();
			this.m_pnlBracket = new CommonUI.Controls.SectionPanel();
			this.m_lblXPositionsTitle = new CommonUI.Controls.ThemedLabel();
			this.m_lblYPositionsTitle = new CommonUI.Controls.ThemedLabel();
			this.m_btnResetPositions = new CommonUI.Controls.ThemedButton();
			this.m_pnlXPositions = new System.Windows.Forms.Panel();
			this.m_pnlYPositions = new System.Windows.Forms.Panel();
			this.m_btnAddXPosition = new CommonUI.Controls.ThemedButton();
			this.m_btnAddYPosition = new CommonUI.Controls.ThemedButton();
			this.m_pnlBracketHeight = new CommonUI.Controls.SectionPanel();
			this.m_lblZExtension = new System.Windows.Forms.Label();
			this.m_numZExtension = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblZExtensionUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblRibHeight = new System.Windows.Forms.Label();
			this.m_numRibHeight = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblRibHeightUnit = new CommonUI.Controls.ThemedLabel();
			this.m_pnlCrossFit = new CommonUI.Controls.SectionPanel();
			this.m_lblEngagementDepth = new System.Windows.Forms.Label();
			this.m_numEngagementDepth = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblEngagementDepthUnit = new CommonUI.Controls.ThemedLabel();
			this.m_picCrossGap = new System.Windows.Forms.PictureBox();
			this.m_pnlBracketCount = new CommonUI.Controls.SectionPanel();
			this.m_lblXPinCount = new System.Windows.Forms.Label();
			this.m_numXPinCount = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblYPinCount = new System.Windows.Forms.Label();
			this.m_numYPinCount = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_pnlBottomPinSize = new CommonUI.Controls.SectionPanel();
			this.m_lblPinLength = new System.Windows.Forms.Label();
			this.m_numPinLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblPinLengthUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblWallThickness = new System.Windows.Forms.Label();
			this.m_numWallThickness = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblWallThicknessUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblBaseThickness = new System.Windows.Forms.Label();
			this.m_numBaseThickness = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblBaseThicknessUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblFilletRadius = new System.Windows.Forms.Label();
			this.m_numFilletRadius = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblFilletRadiusUnit = new CommonUI.Controls.ThemedLabel();
			this.m_picPinDimension = new System.Windows.Forms.PictureBox();
			this.m_btnBack = new CommonUI.Controls.ThemedButton();
			this.m_btnGenerate = new CommonUI.Controls.ThemedButton();
			this.m_pnlSaveDxfPage = new System.Windows.Forms.Panel();
			this.m_pnlSaveDxf = new CommonUI.Controls.SectionPanel();
			this.m_lblSaveDxfHint = new System.Windows.Forms.Label();
			this.m_txtSavePath = new System.Windows.Forms.TextBox();
			this.m_btnBrowseSavePath = new CommonUI.Controls.ThemedButton();
			this.m_btnSkipSave = new CommonUI.Controls.ThemedButton();
			this.m_btnSaveDxf = new CommonUI.Controls.ThemedButton();
			this.m_pnlSelectFacePage.SuspendLayout();
			this.m_pnlSelectFace.SuspendLayout();
			this.m_pnlInputParametersPage.SuspendLayout();
			this.m_pnlBase.SuspendLayout();
			this.m_pnlBracket.SuspendLayout();
			this.m_pnlXPositions.SuspendLayout();
			this.m_pnlYPositions.SuspendLayout();
			this.m_pnlBracketHeight.SuspendLayout();
			this.m_pnlCrossFit.SuspendLayout();
			this.m_pnlBracketCount.SuspendLayout();
			this.m_pnlBottomPinSize.SuspendLayout();
			this.m_pnlSaveDxfPage.SuspendLayout();
			this.m_pnlSaveDxf.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numXExtension)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numYExtension)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numZExtension)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRibHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numEngagementDepth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picCrossGap)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numXPinCount)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numYPinCount)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numPinLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numWallThickness)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numBaseThickness)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numFilletRadius)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picPinDimension)).BeginInit();
			this.SuspendLayout();
			// 
			// m_pnlSelectFacePage
			// 
			this.m_pnlSelectFacePage.Controls.Add(this.m_pnlSelectFace);
			this.m_pnlSelectFacePage.Controls.Add(this.m_btnNext);
			this.m_pnlSelectFacePage.Location = new System.Drawing.Point(12, 12);
			this.m_pnlSelectFacePage.Name = "m_pnlSelectFacePage";
			this.m_pnlSelectFacePage.Size = new System.Drawing.Size(356, 108);
			this.m_pnlSelectFacePage.TabIndex = 0;
			// 
			// m_pnlSelectFace
			// 
			this.m_pnlSelectFace.Controls.Add(this.m_lblHint);
			this.m_pnlSelectFace.Location = new System.Drawing.Point(0, 0);
			this.m_pnlSelectFace.Name = "m_pnlSelectFace";
			this.m_pnlSelectFace.Size = new System.Drawing.Size(356, 64);
			this.m_pnlSelectFace.TabIndex = 0;
			// 
			// m_lblHint
			// 
			this.m_lblHint.Location = new System.Drawing.Point(10, 14);
			this.m_lblHint.Name = "m_lblHint";
			this.m_lblHint.Size = new System.Drawing.Size(336, 40);
			this.m_lblHint.TabIndex = 0;
			this.m_lblHint.Text = "請點選工件上要放置夾具的底面／側面\r\n（支援 Ctrl 多選、雙擊連續面選取）";
			// 
			// m_btnNext
			// 
			this.m_btnNext.Enabled = false;
			this.m_btnNext.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnNext.Location = new System.Drawing.Point(276, 80);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(80, 28);
			this.m_btnNext.TabIndex = 1;
			this.m_btnNext.Text = "下一步";
			this.m_btnNext.Click += new System.EventHandler(this.m_btnNext_Click);
			// 
			// m_pnlInputParametersPage
			// 
			this.m_pnlInputParametersPage.Controls.Add(this.m_pnlBase);
			this.m_pnlInputParametersPage.Controls.Add(this.m_pnlBracket);
			this.m_pnlInputParametersPage.Controls.Add(this.m_pnlBracketHeight);
			this.m_pnlInputParametersPage.Controls.Add(this.m_pnlCrossFit);
			this.m_pnlInputParametersPage.Controls.Add(this.m_pnlBracketCount);
			this.m_pnlInputParametersPage.Controls.Add(this.m_pnlBottomPinSize);
			this.m_pnlInputParametersPage.Controls.Add(this.m_btnBack);
			this.m_pnlInputParametersPage.Controls.Add(this.m_btnGenerate);
			this.m_pnlInputParametersPage.Location = new System.Drawing.Point(12, 12);
			this.m_pnlInputParametersPage.Name = "m_pnlInputParametersPage";
			this.m_pnlInputParametersPage.Size = new System.Drawing.Size(560, 943);
			this.m_pnlInputParametersPage.TabIndex = 1;
			// 
			// m_pnlBase
			// 
			this.m_pnlBase.Controls.Add(this.m_lblXExtension);
			this.m_pnlBase.Controls.Add(this.m_numXExtension);
			this.m_pnlBase.Controls.Add(this.m_lblXExtensionUnit);
			this.m_pnlBase.Controls.Add(this.m_lblYExtension);
			this.m_pnlBase.Controls.Add(this.m_numYExtension);
			this.m_pnlBase.Controls.Add(this.m_lblYExtensionUnit);
			this.m_pnlBase.Location = new System.Drawing.Point(0, 0);
			this.m_pnlBase.Name = "m_pnlBase";
			this.m_pnlBase.Size = new System.Drawing.Size(560, 99);
			this.m_pnlBase.TabIndex = 0;
			this.m_pnlBase.Text = "底部大小";
			// 
			// m_lblXExtension
			// 
			this.m_lblXExtension.AutoSize = true;
			this.m_lblXExtension.Location = new System.Drawing.Point(10, 40);
			this.m_lblXExtension.Name = "m_lblXExtension";
			this.m_lblXExtension.Size = new System.Drawing.Size(120, 16);
			this.m_lblXExtension.TabIndex = 0;
			this.m_lblXExtension.Text = "X 方向延伸距離";
			// 
			// m_numXExtension
			// 
			this.m_numXExtension.DecimalPlaces = 1;
			this.m_numXExtension.Location = new System.Drawing.Point(150, 36);
			this.m_numXExtension.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_numXExtension.Name = "m_numXExtension";
			this.m_numXExtension.Size = new System.Drawing.Size(100, 23);
			this.m_numXExtension.TabIndex = 1;
			this.m_numXExtension.Value = new decimal(new int[] { 50, 0, 0, 0});
			// 
			// m_lblXExtensionUnit
			// 
			this.m_lblXExtensionUnit.AutoSize = true;
			this.m_lblXExtensionUnit.Location = new System.Drawing.Point(256, 40);
			this.m_lblXExtensionUnit.Name = "m_lblXExtensionUnit";
			this.m_lblXExtensionUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblXExtensionUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblXExtensionUnit.TabIndex = 2;
			this.m_lblXExtensionUnit.Text = "mm";
			// 
			// m_lblYExtension
			// 
			this.m_lblYExtension.AutoSize = true;
			this.m_lblYExtension.Location = new System.Drawing.Point(10, 70);
			this.m_lblYExtension.Name = "m_lblYExtension";
			this.m_lblYExtension.Size = new System.Drawing.Size(120, 16);
			this.m_lblYExtension.TabIndex = 3;
			this.m_lblYExtension.Text = "Y 方向延伸距離";
			// 
			// m_numYExtension
			// 
			this.m_numYExtension.DecimalPlaces = 1;
			this.m_numYExtension.Location = new System.Drawing.Point(150, 66);
			this.m_numYExtension.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_numYExtension.Name = "m_numYExtension";
			this.m_numYExtension.Size = new System.Drawing.Size(100, 23);
			this.m_numYExtension.TabIndex = 4;
			this.m_numYExtension.Value = new decimal(new int[] { 50, 0, 0, 0});
			// 
			// m_lblYExtensionUnit
			// 
			this.m_lblYExtensionUnit.AutoSize = true;
			this.m_lblYExtensionUnit.Location = new System.Drawing.Point(256, 70);
			this.m_lblYExtensionUnit.Name = "m_lblYExtensionUnit";
			this.m_lblYExtensionUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblYExtensionUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblYExtensionUnit.TabIndex = 5;
			this.m_lblYExtensionUnit.Text = "mm";
			// 
			// m_pnlBracket
			// 
			this.m_pnlBracket.Controls.Add(this.m_lblXPositionsTitle);
			this.m_pnlBracket.Controls.Add(this.m_lblYPositionsTitle);
			this.m_pnlBracket.Controls.Add(this.m_btnResetPositions);
			this.m_pnlBracket.Controls.Add(this.m_pnlXPositions);
			this.m_pnlBracket.Controls.Add(this.m_pnlYPositions);
			this.m_pnlBracket.Controls.Add(this.m_btnAddXPosition);
			this.m_pnlBracket.Controls.Add(this.m_btnAddYPosition);
			this.m_pnlBracket.Location = new System.Drawing.Point(0, 107);
			this.m_pnlBracket.Name = "m_pnlBracket";
			this.m_pnlBracket.Size = new System.Drawing.Size(560, 249);
			this.m_pnlBracket.TabIndex = 1;
			this.m_pnlBracket.Text = "支架位置";
			// 
			// m_lblXPositionsTitle
			// 
			this.m_lblXPositionsTitle.AutoSize = true;
			this.m_lblXPositionsTitle.Location = new System.Drawing.Point(10, 40);
			this.m_lblXPositionsTitle.Name = "m_lblXPositionsTitle";
			this.m_lblXPositionsTitle.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblXPositionsTitle.Size = new System.Drawing.Size(92, 16);
			this.m_lblXPositionsTitle.TabIndex = 0;
			this.m_lblXPositionsTitle.Text = "X 方向支架位置";
			// 
			// m_lblYPositionsTitle
			// 
			this.m_lblYPositionsTitle.AutoSize = true;
			this.m_lblYPositionsTitle.Location = new System.Drawing.Point(290, 40);
			this.m_lblYPositionsTitle.Name = "m_lblYPositionsTitle";
			this.m_lblYPositionsTitle.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblYPositionsTitle.Size = new System.Drawing.Size(92, 16);
			this.m_lblYPositionsTitle.TabIndex = 1;
			this.m_lblYPositionsTitle.Text = "Y 方向支架位置";
			// 
			// m_btnResetPositions
			// 
			this.m_btnResetPositions.Location = new System.Drawing.Point(470, 36);
			this.m_btnResetPositions.Name = "m_btnResetPositions";
			this.m_btnResetPositions.Size = new System.Drawing.Size(80, 23);
			this.m_btnResetPositions.TabIndex = 2;
			this.m_btnResetPositions.Text = "重設位置";
			this.m_btnResetPositions.Click += new System.EventHandler(this.m_btnResetPositions_Click);
			// 
			// m_pnlXPositions
			// 
			this.m_pnlXPositions.AutoScroll = true;
			this.m_pnlXPositions.Location = new System.Drawing.Point(10, 66);
			this.m_pnlXPositions.Name = "m_pnlXPositions";
			this.m_pnlXPositions.Size = new System.Drawing.Size(260, 150);
			this.m_pnlXPositions.TabIndex = 3;
			// 
			// m_pnlYPositions
			// 
			this.m_pnlYPositions.AutoScroll = true;
			this.m_pnlYPositions.Location = new System.Drawing.Point(290, 66);
			this.m_pnlYPositions.Name = "m_pnlYPositions";
			this.m_pnlYPositions.Size = new System.Drawing.Size(260, 150);
			this.m_pnlYPositions.TabIndex = 4;
			// 
			// m_btnAddXPosition
			// 
			this.m_btnAddXPosition.Location = new System.Drawing.Point(10, 216);
			this.m_btnAddXPosition.Name = "m_btnAddXPosition";
			this.m_btnAddXPosition.Size = new System.Drawing.Size(260, 23);
			this.m_btnAddXPosition.TabIndex = 5;
			this.m_btnAddXPosition.Text = "+ 新增 X 支架位置";
			this.m_btnAddXPosition.Click += new System.EventHandler(this.m_btnAddXPosition_Click);
			// 
			// m_btnAddYPosition
			// 
			this.m_btnAddYPosition.Location = new System.Drawing.Point(290, 216);
			this.m_btnAddYPosition.Name = "m_btnAddYPosition";
			this.m_btnAddYPosition.Size = new System.Drawing.Size(260, 23);
			this.m_btnAddYPosition.TabIndex = 6;
			this.m_btnAddYPosition.Text = "+ 新增 Y 支架位置";
			this.m_btnAddYPosition.Click += new System.EventHandler(this.m_btnAddYPosition_Click);
			// 
			// m_pnlBracketHeight
			// 
			this.m_pnlBracketHeight.Controls.Add(this.m_lblZExtension);
			this.m_pnlBracketHeight.Controls.Add(this.m_numZExtension);
			this.m_pnlBracketHeight.Controls.Add(this.m_lblZExtensionUnit);
			this.m_pnlBracketHeight.Controls.Add(this.m_lblRibHeight);
			this.m_pnlBracketHeight.Controls.Add(this.m_numRibHeight);
			this.m_pnlBracketHeight.Controls.Add(this.m_lblRibHeightUnit);
			this.m_pnlBracketHeight.Location = new System.Drawing.Point(0, 364);
			this.m_pnlBracketHeight.Name = "m_pnlBracketHeight";
			this.m_pnlBracketHeight.Size = new System.Drawing.Size(560, 99);
			this.m_pnlBracketHeight.TabIndex = 2;
			this.m_pnlBracketHeight.Text = "支架高度";
			// 
			// m_lblZExtension
			// 
			this.m_lblZExtension.AutoSize = true;
			this.m_lblZExtension.Location = new System.Drawing.Point(10, 40);
			this.m_lblZExtension.Name = "m_lblZExtension";
			this.m_lblZExtension.Size = new System.Drawing.Size(120, 16);
			this.m_lblZExtension.TabIndex = 0;
			this.m_lblZExtension.Text = "工件離地距離";
			// 
			// m_numZExtension
			// 
			this.m_numZExtension.DecimalPlaces = 1;
			this.m_numZExtension.Location = new System.Drawing.Point(150, 36);
			this.m_numZExtension.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_numZExtension.Name = "m_numZExtension";
			this.m_numZExtension.Size = new System.Drawing.Size(100, 23);
			this.m_numZExtension.TabIndex = 1;
			this.m_numZExtension.Value = new decimal(new int[] { 100, 0, 0, 0});
			// 
			// m_lblZExtensionUnit
			// 
			this.m_lblZExtensionUnit.AutoSize = true;
			this.m_lblZExtensionUnit.Location = new System.Drawing.Point(256, 40);
			this.m_lblZExtensionUnit.Name = "m_lblZExtensionUnit";
			this.m_lblZExtensionUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblZExtensionUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblZExtensionUnit.TabIndex = 2;
			this.m_lblZExtensionUnit.Text = "mm";
			// 
			// m_lblRibHeight
			// 
			this.m_lblRibHeight.AutoSize = true;
			this.m_lblRibHeight.Location = new System.Drawing.Point(10, 70);
			this.m_lblRibHeight.Name = "m_lblRibHeight";
			this.m_lblRibHeight.Size = new System.Drawing.Size(120, 16);
			this.m_lblRibHeight.TabIndex = 3;
			this.m_lblRibHeight.Text = "支架基礎高度";
			// 
			// m_numRibHeight
			// 
			this.m_numRibHeight.DecimalPlaces = 1;
			this.m_numRibHeight.Location = new System.Drawing.Point(150, 66);
			this.m_numRibHeight.Minimum = new decimal(new int[] { 1, 0, 0, 0});
			this.m_numRibHeight.Name = "m_numRibHeight";
			this.m_numRibHeight.Size = new System.Drawing.Size(100, 23);
			this.m_numRibHeight.TabIndex = 4;
			this.m_numRibHeight.Value = new decimal(new int[] { 50, 0, 0, 0});
			// 
			// m_lblRibHeightUnit
			// 
			this.m_lblRibHeightUnit.AutoSize = true;
			this.m_lblRibHeightUnit.Location = new System.Drawing.Point(256, 70);
			this.m_lblRibHeightUnit.Name = "m_lblRibHeightUnit";
			this.m_lblRibHeightUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblRibHeightUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblRibHeightUnit.TabIndex = 5;
			this.m_lblRibHeightUnit.Text = "mm";
			// 
			// m_pnlCrossFit
			// 
			this.m_pnlCrossFit.Controls.Add(this.m_lblEngagementDepth);
			this.m_pnlCrossFit.Controls.Add(this.m_numEngagementDepth);
			this.m_pnlCrossFit.Controls.Add(this.m_lblEngagementDepthUnit);
			this.m_pnlCrossFit.Controls.Add(this.m_picCrossGap);
			this.m_pnlCrossFit.Location = new System.Drawing.Point(0, 471);
			this.m_pnlCrossFit.Name = "m_pnlCrossFit";
			this.m_pnlCrossFit.Size = new System.Drawing.Size(560, 154);
			this.m_pnlCrossFit.TabIndex = 3;
			this.m_pnlCrossFit.Text = "咬合設計";
			// 
			// m_lblEngagementDepth
			// 
			this.m_lblEngagementDepth.AutoSize = true;
			this.m_lblEngagementDepth.Location = new System.Drawing.Point(10, 40);
			this.m_lblEngagementDepth.Name = "m_lblEngagementDepth";
			this.m_lblEngagementDepth.Size = new System.Drawing.Size(120, 16);
			this.m_lblEngagementDepth.TabIndex = 0;
			this.m_lblEngagementDepth.Text = "支架咬合深度(A)";
			// 
			// m_numEngagementDepth
			// 
			this.m_numEngagementDepth.DecimalPlaces = 1;
			this.m_numEngagementDepth.Location = new System.Drawing.Point(150, 36);
			this.m_numEngagementDepth.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_numEngagementDepth.Name = "m_numEngagementDepth";
			this.m_numEngagementDepth.Size = new System.Drawing.Size(100, 23);
			this.m_numEngagementDepth.TabIndex = 1;
			this.m_numEngagementDepth.Value = new decimal(new int[] { 5, 0, 0, 0});
			// 
			// m_lblEngagementDepthUnit
			// 
			this.m_lblEngagementDepthUnit.AutoSize = true;
			this.m_lblEngagementDepthUnit.Location = new System.Drawing.Point(256, 40);
			this.m_lblEngagementDepthUnit.Name = "m_lblEngagementDepthUnit";
			this.m_lblEngagementDepthUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblEngagementDepthUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblEngagementDepthUnit.TabIndex = 2;
			this.m_lblEngagementDepthUnit.Text = "mm";
			// 
			// m_picCrossGap
			// 
			this.m_picCrossGap.Location = new System.Drawing.Point(350, 36);
			this.m_picCrossGap.Name = "m_picCrossGap";
			this.m_picCrossGap.Size = new System.Drawing.Size(200, 108);
			this.m_picCrossGap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.m_picCrossGap.TabIndex = 3;
			this.m_picCrossGap.TabStop = false;
			// 
			// m_pnlBracketCount
			// 
			this.m_pnlBracketCount.Controls.Add(this.m_lblXPinCount);
			this.m_pnlBracketCount.Controls.Add(this.m_numXPinCount);
			this.m_pnlBracketCount.Controls.Add(this.m_lblYPinCount);
			this.m_pnlBracketCount.Controls.Add(this.m_numYPinCount);
			this.m_pnlBracketCount.Location = new System.Drawing.Point(0, 633);
			this.m_pnlBracketCount.Name = "m_pnlBracketCount";
			this.m_pnlBracketCount.Size = new System.Drawing.Size(560, 99);
			this.m_pnlBracketCount.TabIndex = 4;
			this.m_pnlBracketCount.Text = "底部拼接數量";
			// 
			// m_lblXPinCount
			// 
			this.m_lblXPinCount.AutoSize = true;
			this.m_lblXPinCount.Location = new System.Drawing.Point(10, 40);
			this.m_lblXPinCount.Name = "m_lblXPinCount";
			this.m_lblXPinCount.Size = new System.Drawing.Size(120, 16);
			this.m_lblXPinCount.TabIndex = 0;
			this.m_lblXPinCount.Text = "X 方向拼接數量";
			// 
			// m_numXPinCount
			// 
			this.m_numXPinCount.DecimalPlaces = 0;
			this.m_numXPinCount.Location = new System.Drawing.Point(150, 36);
			this.m_numXPinCount.Minimum = new decimal(new int[] { 1, 0, 0, 0});
			this.m_numXPinCount.Name = "m_numXPinCount";
			this.m_numXPinCount.Size = new System.Drawing.Size(100, 23);
			this.m_numXPinCount.TabIndex = 1;
			this.m_numXPinCount.Value = new decimal(new int[] { 8, 0, 0, 0});
			// 
			// m_lblYPinCount
			// 
			this.m_lblYPinCount.AutoSize = true;
			this.m_lblYPinCount.Location = new System.Drawing.Point(10, 70);
			this.m_lblYPinCount.Name = "m_lblYPinCount";
			this.m_lblYPinCount.Size = new System.Drawing.Size(120, 16);
			this.m_lblYPinCount.TabIndex = 2;
			this.m_lblYPinCount.Text = "Y 方向拼接數量";
			// 
			// m_numYPinCount
			// 
			this.m_numYPinCount.DecimalPlaces = 0;
			this.m_numYPinCount.Location = new System.Drawing.Point(150, 66);
			this.m_numYPinCount.Minimum = new decimal(new int[] { 1, 0, 0, 0});
			this.m_numYPinCount.Name = "m_numYPinCount";
			this.m_numYPinCount.Size = new System.Drawing.Size(100, 23);
			this.m_numYPinCount.TabIndex = 3;
			this.m_numYPinCount.Value = new decimal(new int[] { 2, 0, 0, 0});
			// 
			// m_pnlBottomPinSize
			// 
			this.m_pnlBottomPinSize.Controls.Add(this.m_lblPinLength);
			this.m_pnlBottomPinSize.Controls.Add(this.m_numPinLength);
			this.m_pnlBottomPinSize.Controls.Add(this.m_lblPinLengthUnit);
			this.m_pnlBottomPinSize.Controls.Add(this.m_lblWallThickness);
			this.m_pnlBottomPinSize.Controls.Add(this.m_numWallThickness);
			this.m_pnlBottomPinSize.Controls.Add(this.m_lblWallThicknessUnit);
			this.m_pnlBottomPinSize.Controls.Add(this.m_lblBaseThickness);
			this.m_pnlBottomPinSize.Controls.Add(this.m_numBaseThickness);
			this.m_pnlBottomPinSize.Controls.Add(this.m_lblBaseThicknessUnit);
			this.m_pnlBottomPinSize.Controls.Add(this.m_lblFilletRadius);
			this.m_pnlBottomPinSize.Controls.Add(this.m_numFilletRadius);
			this.m_pnlBottomPinSize.Controls.Add(this.m_lblFilletRadiusUnit);
			this.m_pnlBottomPinSize.Controls.Add(this.m_picPinDimension);
			this.m_pnlBottomPinSize.Location = new System.Drawing.Point(0, 740);
			this.m_pnlBottomPinSize.Name = "m_pnlBottomPinSize";
			this.m_pnlBottomPinSize.Size = new System.Drawing.Size(560, 159);
			this.m_pnlBottomPinSize.TabIndex = 5;
			this.m_pnlBottomPinSize.Text = "拼接尺寸";
			// 
			// m_lblPinLength
			// 
			this.m_lblPinLength.AutoSize = true;
			this.m_lblPinLength.Location = new System.Drawing.Point(10, 40);
			this.m_lblPinLength.Name = "m_lblPinLength";
			this.m_lblPinLength.Size = new System.Drawing.Size(120, 16);
			this.m_lblPinLength.TabIndex = 0;
			this.m_lblPinLength.Text = "拼接長度(B)";
			// 
			// m_numPinLength
			// 
			this.m_numPinLength.DecimalPlaces = 1;
			this.m_numPinLength.Location = new System.Drawing.Point(150, 36);
			this.m_numPinLength.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_numPinLength.Name = "m_numPinLength";
			this.m_numPinLength.Size = new System.Drawing.Size(100, 23);
			this.m_numPinLength.TabIndex = 1;
			this.m_numPinLength.Value = new decimal(new int[] { 20, 0, 0, 0});
			// 
			// m_lblPinLengthUnit
			// 
			this.m_lblPinLengthUnit.AutoSize = true;
			this.m_lblPinLengthUnit.Location = new System.Drawing.Point(256, 40);
			this.m_lblPinLengthUnit.Name = "m_lblPinLengthUnit";
			this.m_lblPinLengthUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblPinLengthUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblPinLengthUnit.TabIndex = 2;
			this.m_lblPinLengthUnit.Text = "mm";
			// 
			// m_lblWallThickness
			// 
			this.m_lblWallThickness.AutoSize = true;
			this.m_lblWallThickness.Location = new System.Drawing.Point(10, 70);
			this.m_lblWallThickness.Name = "m_lblWallThickness";
			this.m_lblWallThickness.Size = new System.Drawing.Size(120, 16);
			this.m_lblWallThickness.TabIndex = 4;
			this.m_lblWallThickness.Text = "拼接寬度(C支架厚度)";
			// 
			// m_numWallThickness
			// 
			this.m_numWallThickness.DecimalPlaces = 1;
			this.m_numWallThickness.Location = new System.Drawing.Point(150, 66);
			this.m_numWallThickness.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_numWallThickness.Name = "m_numWallThickness";
			this.m_numWallThickness.Size = new System.Drawing.Size(100, 23);
			this.m_numWallThickness.TabIndex = 5;
			this.m_numWallThickness.Value = new decimal(new int[] { 3, 0, 0, 0});
			// 
			// m_lblWallThicknessUnit
			// 
			this.m_lblWallThicknessUnit.AutoSize = true;
			this.m_lblWallThicknessUnit.Location = new System.Drawing.Point(256, 70);
			this.m_lblWallThicknessUnit.Name = "m_lblWallThicknessUnit";
			this.m_lblWallThicknessUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblWallThicknessUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblWallThicknessUnit.TabIndex = 6;
			this.m_lblWallThicknessUnit.Text = "mm";
			// 
			// m_lblBaseThickness
			// 
			this.m_lblBaseThickness.AutoSize = true;
			this.m_lblBaseThickness.Location = new System.Drawing.Point(10, 100);
			this.m_lblBaseThickness.Name = "m_lblBaseThickness";
			this.m_lblBaseThickness.Size = new System.Drawing.Size(120, 16);
			this.m_lblBaseThickness.TabIndex = 7;
			this.m_lblBaseThickness.Text = "拼接高度(D底部厚度)";
			// 
			// m_numBaseThickness
			// 
			this.m_numBaseThickness.DecimalPlaces = 1;
			this.m_numBaseThickness.Location = new System.Drawing.Point(150, 96);
			this.m_numBaseThickness.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_numBaseThickness.Name = "m_numBaseThickness";
			this.m_numBaseThickness.Size = new System.Drawing.Size(100, 23);
			this.m_numBaseThickness.TabIndex = 8;
			this.m_numBaseThickness.Value = new decimal(new int[] { 5, 0, 0, 0});
			// 
			// m_lblBaseThicknessUnit
			// 
			this.m_lblBaseThicknessUnit.AutoSize = true;
			this.m_lblBaseThicknessUnit.Location = new System.Drawing.Point(256, 100);
			this.m_lblBaseThicknessUnit.Name = "m_lblBaseThicknessUnit";
			this.m_lblBaseThicknessUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblBaseThicknessUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblBaseThicknessUnit.TabIndex = 9;
			this.m_lblBaseThicknessUnit.Text = "mm";
			// 
			// m_lblFilletRadius
			// 
			this.m_lblFilletRadius.AutoSize = true;
			this.m_lblFilletRadius.Location = new System.Drawing.Point(10, 130);
			this.m_lblFilletRadius.Name = "m_lblFilletRadius";
			this.m_lblFilletRadius.Size = new System.Drawing.Size(120, 16);
			this.m_lblFilletRadius.TabIndex = 10;
			this.m_lblFilletRadius.Text = "導圓角半徑";
			// 
			// m_numFilletRadius
			// 
			this.m_numFilletRadius.DecimalPlaces = 1;
			this.m_numFilletRadius.Location = new System.Drawing.Point(150, 126);
			this.m_numFilletRadius.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_numFilletRadius.Name = "m_numFilletRadius";
			this.m_numFilletRadius.Size = new System.Drawing.Size(100, 23);
			this.m_numFilletRadius.TabIndex = 11;
			this.m_numFilletRadius.Value = new decimal(new int[] { 1, 0, 0, 0});
			// 
			// m_lblFilletRadiusUnit
			// 
			this.m_lblFilletRadiusUnit.AutoSize = true;
			this.m_lblFilletRadiusUnit.Location = new System.Drawing.Point(256, 130);
			this.m_lblFilletRadiusUnit.Name = "m_lblFilletRadiusUnit";
			this.m_lblFilletRadiusUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblFilletRadiusUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblFilletRadiusUnit.TabIndex = 12;
			this.m_lblFilletRadiusUnit.Text = "mm";
			// 
			// m_picPinDimension
			// 
			this.m_picPinDimension.Location = new System.Drawing.Point(350, 36);
			this.m_picPinDimension.Name = "m_picPinDimension";
			this.m_picPinDimension.Size = new System.Drawing.Size(200, 108);
			this.m_picPinDimension.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.m_picPinDimension.TabIndex = 3;
			this.m_picPinDimension.TabStop = false;
			// 
			// m_btnBack
			// 
			this.m_btnBack.Location = new System.Drawing.Point(0, 915);
			this.m_btnBack.Name = "m_btnBack";
			this.m_btnBack.Size = new System.Drawing.Size(80, 28);
			this.m_btnBack.TabIndex = 6;
			this.m_btnBack.Text = "上一步";
			this.m_btnBack.Click += new System.EventHandler(this.m_btnBack_Click);
			// 
			// m_btnGenerate
			// 
			this.m_btnGenerate.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnGenerate.Location = new System.Drawing.Point(480, 915);
			this.m_btnGenerate.Name = "m_btnGenerate";
			this.m_btnGenerate.Size = new System.Drawing.Size(80, 28);
			this.m_btnGenerate.TabIndex = 7;
			this.m_btnGenerate.Text = "生成";
			this.m_btnGenerate.Click += new System.EventHandler(this.m_btnGenerate_Click);
			// 
			// m_pnlSaveDxfPage
			// 
			this.m_pnlSaveDxfPage.Controls.Add(this.m_pnlSaveDxf);
			this.m_pnlSaveDxfPage.Controls.Add(this.m_btnSkipSave);
			this.m_pnlSaveDxfPage.Controls.Add(this.m_btnSaveDxf);
			this.m_pnlSaveDxfPage.Location = new System.Drawing.Point(12, 12);
			this.m_pnlSaveDxfPage.Name = "m_pnlSaveDxfPage";
			this.m_pnlSaveDxfPage.Size = new System.Drawing.Size(356, 117);
			this.m_pnlSaveDxfPage.TabIndex = 2;
			// 
			// m_pnlSaveDxf
			// 
			this.m_pnlSaveDxf.Controls.Add(this.m_lblSaveDxfHint);
			this.m_pnlSaveDxf.Controls.Add(this.m_txtSavePath);
			this.m_pnlSaveDxf.Controls.Add(this.m_btnBrowseSavePath);
			this.m_pnlSaveDxf.Location = new System.Drawing.Point(0, 0);
			this.m_pnlSaveDxf.Name = "m_pnlSaveDxf";
			this.m_pnlSaveDxf.Size = new System.Drawing.Size(356, 73);
			this.m_pnlSaveDxf.TabIndex = 0;
			// 
			// m_lblSaveDxfHint
			// 
			this.m_lblSaveDxfHint.AutoSize = true;
			this.m_lblSaveDxfHint.Location = new System.Drawing.Point(10, 14);
			this.m_lblSaveDxfHint.Name = "m_lblSaveDxfHint";
			this.m_lblSaveDxfHint.Size = new System.Drawing.Size(200, 16);
			this.m_lblSaveDxfHint.TabIndex = 0;
			this.m_lblSaveDxfHint.Text = "是否要將支架輸出為 DXF 檔案？";
			// 
			// m_txtSavePath
			// 
			this.m_txtSavePath.Location = new System.Drawing.Point(10, 40);
			this.m_txtSavePath.Name = "m_txtSavePath";
			this.m_txtSavePath.ReadOnly = true;
			this.m_txtSavePath.Size = new System.Drawing.Size(250, 23);
			this.m_txtSavePath.TabIndex = 1;
			// 
			// m_btnBrowseSavePath
			// 
			this.m_btnBrowseSavePath.Location = new System.Drawing.Point(266, 40);
			this.m_btnBrowseSavePath.Name = "m_btnBrowseSavePath";
			this.m_btnBrowseSavePath.Size = new System.Drawing.Size(80, 23);
			this.m_btnBrowseSavePath.TabIndex = 2;
			this.m_btnBrowseSavePath.Text = "瀏覽...";
			this.m_btnBrowseSavePath.Click += new System.EventHandler(this.m_btnBrowseSavePath_Click);
			// 
			// m_btnSkipSave
			// 
			this.m_btnSkipSave.Location = new System.Drawing.Point(188, 89);
			this.m_btnSkipSave.Name = "m_btnSkipSave";
			this.m_btnSkipSave.Size = new System.Drawing.Size(80, 28);
			this.m_btnSkipSave.TabIndex = 1;
			this.m_btnSkipSave.Text = "略過";
			this.m_btnSkipSave.Click += new System.EventHandler(this.m_btnSkipSave_Click);
			// 
			// m_btnSaveDxf
			// 
			this.m_btnSaveDxf.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnSaveDxf.Location = new System.Drawing.Point(276, 89);
			this.m_btnSaveDxf.Name = "m_btnSaveDxf";
			this.m_btnSaveDxf.Size = new System.Drawing.Size(80, 28);
			this.m_btnSaveDxf.TabIndex = 2;
			this.m_btnSaveDxf.Text = "儲存";
			this.m_btnSaveDxf.Click += new System.EventHandler(this.m_btnSaveDxf_Click);
			// 
			// DesignSupportBracketDlg
			// 
			this.ClientSize = new System.Drawing.Size(584, 967);
			this.Controls.Add(this.m_pnlSelectFacePage);
			this.Controls.Add(this.m_pnlInputParametersPage);
			this.Controls.Add(this.m_pnlSaveDxfPage);
			this.Name = "DesignSupportBracketDlg";
			this.Text = "設計夾具";
			this.m_pnlSelectFacePage.ResumeLayout(false);
			this.m_pnlSelectFace.ResumeLayout(false);
			this.m_pnlInputParametersPage.ResumeLayout(false);
			this.m_pnlBase.ResumeLayout(false);
			this.m_pnlBase.PerformLayout();
			this.m_pnlBracket.ResumeLayout(false);
			this.m_pnlBracket.PerformLayout();
			this.m_pnlXPositions.ResumeLayout(false);
			this.m_pnlYPositions.ResumeLayout(false);
			this.m_pnlBracketHeight.ResumeLayout(false);
			this.m_pnlBracketHeight.PerformLayout();
			this.m_pnlCrossFit.ResumeLayout(false);
			this.m_pnlCrossFit.PerformLayout();
			this.m_pnlBracketCount.ResumeLayout(false);
			this.m_pnlBracketCount.PerformLayout();
			this.m_pnlBottomPinSize.ResumeLayout(false);
			this.m_pnlBottomPinSize.PerformLayout();
			this.m_pnlSaveDxfPage.ResumeLayout(false);
			this.m_pnlSaveDxf.ResumeLayout(false);
			this.m_pnlSaveDxf.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numXExtension)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numYExtension)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numZExtension)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numRibHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numEngagementDepth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picCrossGap)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numXPinCount)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numYPinCount)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numPinLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numWallThickness)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numBaseThickness)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numFilletRadius)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picPinDimension)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel m_pnlSelectFacePage;
		private CommonUI.Controls.SectionPanel m_pnlSelectFace;
		private System.Windows.Forms.Label m_lblHint;
		private CommonUI.Controls.ThemedButton m_btnNext;
		private System.Windows.Forms.Panel m_pnlInputParametersPage;
		private CommonUI.Controls.SectionPanel m_pnlBase;
		private System.Windows.Forms.Label m_lblXExtension;
		private CommonUI.Controls.DebouncedNumericUpDown m_numXExtension;
		private CommonUI.Controls.ThemedLabel m_lblXExtensionUnit;
		private System.Windows.Forms.Label m_lblYExtension;
		private CommonUI.Controls.DebouncedNumericUpDown m_numYExtension;
		private CommonUI.Controls.ThemedLabel m_lblYExtensionUnit;
		private CommonUI.Controls.SectionPanel m_pnlBracket;
		private CommonUI.Controls.ThemedLabel m_lblXPositionsTitle;
		private CommonUI.Controls.ThemedLabel m_lblYPositionsTitle;
		private CommonUI.Controls.ThemedButton m_btnResetPositions;
		private System.Windows.Forms.Panel m_pnlXPositions;
		private System.Windows.Forms.Panel m_pnlYPositions;
		private CommonUI.Controls.ThemedButton m_btnAddXPosition;
		private CommonUI.Controls.ThemedButton m_btnAddYPosition;
		private CommonUI.Controls.SectionPanel m_pnlBracketHeight;
		private System.Windows.Forms.Label m_lblZExtension;
		private CommonUI.Controls.DebouncedNumericUpDown m_numZExtension;
		private CommonUI.Controls.ThemedLabel m_lblZExtensionUnit;
		private System.Windows.Forms.Label m_lblRibHeight;
		private CommonUI.Controls.DebouncedNumericUpDown m_numRibHeight;
		private CommonUI.Controls.ThemedLabel m_lblRibHeightUnit;
		private CommonUI.Controls.SectionPanel m_pnlCrossFit;
		private System.Windows.Forms.Label m_lblEngagementDepth;
		private CommonUI.Controls.DebouncedNumericUpDown m_numEngagementDepth;
		private CommonUI.Controls.ThemedLabel m_lblEngagementDepthUnit;
		private System.Windows.Forms.PictureBox m_picCrossGap;
		private CommonUI.Controls.SectionPanel m_pnlBracketCount;
		private System.Windows.Forms.Label m_lblXPinCount;
		private CommonUI.Controls.DebouncedNumericUpDown m_numXPinCount;
		private System.Windows.Forms.Label m_lblYPinCount;
		private CommonUI.Controls.DebouncedNumericUpDown m_numYPinCount;
		private CommonUI.Controls.SectionPanel m_pnlBottomPinSize;
		private System.Windows.Forms.Label m_lblPinLength;
		private CommonUI.Controls.DebouncedNumericUpDown m_numPinLength;
		private CommonUI.Controls.ThemedLabel m_lblPinLengthUnit;
		private System.Windows.Forms.Label m_lblWallThickness;
		private CommonUI.Controls.DebouncedNumericUpDown m_numWallThickness;
		private CommonUI.Controls.ThemedLabel m_lblWallThicknessUnit;
		private System.Windows.Forms.Label m_lblBaseThickness;
		private CommonUI.Controls.DebouncedNumericUpDown m_numBaseThickness;
		private CommonUI.Controls.ThemedLabel m_lblBaseThicknessUnit;
		private System.Windows.Forms.Label m_lblFilletRadius;
		private CommonUI.Controls.DebouncedNumericUpDown m_numFilletRadius;
		private CommonUI.Controls.ThemedLabel m_lblFilletRadiusUnit;
		private System.Windows.Forms.PictureBox m_picPinDimension;
		private CommonUI.Controls.ThemedButton m_btnBack;
		private CommonUI.Controls.ThemedButton m_btnGenerate;
		private System.Windows.Forms.Panel m_pnlSaveDxfPage;
		private CommonUI.Controls.SectionPanel m_pnlSaveDxf;
		private System.Windows.Forms.Label m_lblSaveDxfHint;
		private System.Windows.Forms.TextBox m_txtSavePath;
		private CommonUI.Controls.ThemedButton m_btnBrowseSavePath;
		private CommonUI.Controls.ThemedButton m_btnSkipSave;
		private CommonUI.Controls.ThemedButton m_btnSaveDxf;
	}
}
