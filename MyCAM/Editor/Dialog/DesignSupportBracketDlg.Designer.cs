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
			this.m_pnlSelectFace = new System.Windows.Forms.Panel();
			this.m_secSelectFace = new CommonUI.Controls.SectionPanel();
			this.m_lblHint = new System.Windows.Forms.Label();
			this.m_btnNext = new CommonUI.Controls.ThemedButton();
			this.m_pnlInputParameters = new System.Windows.Forms.Panel();
			this.m_secBase = new CommonUI.Controls.SectionPanel();
			this.m_lblXExtension = new System.Windows.Forms.Label();
			this.m_txbXExtension = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblXExtensionUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblYExtension = new System.Windows.Forms.Label();
			this.m_txbYExtension = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblYExtensionUnit = new CommonUI.Controls.ThemedLabel();
			this.m_secBracket = new CommonUI.Controls.SectionPanel();
			this.m_lblXPositionsTitle = new CommonUI.Controls.ThemedLabel();
			this.m_lblYPositionsTitle = new CommonUI.Controls.ThemedLabel();
			this.m_btnResetPositions = new CommonUI.Controls.ThemedButton();
			this.m_pnlXPositions = new System.Windows.Forms.Panel();
			this.m_pnlYPositions = new System.Windows.Forms.Panel();
			this.m_btnAddXPosition = new CommonUI.Controls.ThemedButton();
			this.m_btnAddYPosition = new CommonUI.Controls.ThemedButton();
			this.m_secBracketHeight = new CommonUI.Controls.SectionPanel();
			this.m_lblZExtension = new System.Windows.Forms.Label();
			this.m_txbZExtension = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblZExtensionUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblRibHeight = new System.Windows.Forms.Label();
			this.m_txbRibHeight = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblRibHeightUnit = new CommonUI.Controls.ThemedLabel();
			this.m_secCrossFit = new CommonUI.Controls.SectionPanel();
			this.m_lblEngagementDepth = new System.Windows.Forms.Label();
			this.m_txbEngagementDepth = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblEngagementDepthUnit = new CommonUI.Controls.ThemedLabel();
			this.m_picCrossGap = new System.Windows.Forms.PictureBox();
			this.m_secBracketCount = new CommonUI.Controls.SectionPanel();
			this.m_lblXPinCount = new System.Windows.Forms.Label();
			this.m_txbXPinCount = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblYPinCount = new System.Windows.Forms.Label();
			this.m_txbYPinCount = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_secBottomPinSize = new CommonUI.Controls.SectionPanel();
			this.m_lblPinLength = new System.Windows.Forms.Label();
			this.m_txbPinLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblPinLengthUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblWallThickness = new System.Windows.Forms.Label();
			this.m_txbWallThickness = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblWallThicknessUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblBaseThickness = new System.Windows.Forms.Label();
			this.m_txbBaseThickness = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblBaseThicknessUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblFilletRadius = new System.Windows.Forms.Label();
			this.m_txbFilletRadius = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblFilletRadiusUnit = new CommonUI.Controls.ThemedLabel();
			this.m_picPinDimension = new System.Windows.Forms.PictureBox();
			this.m_btnBack = new CommonUI.Controls.ThemedButton();
			this.m_btnGenerate = new CommonUI.Controls.ThemedButton();
			this.m_pnlSaveDxf = new System.Windows.Forms.Panel();
			this.m_secSaveDxf = new CommonUI.Controls.SectionPanel();
			this.m_lblSaveDxfHint = new System.Windows.Forms.Label();
			this.m_txbSavePath = new System.Windows.Forms.TextBox();
			this.m_btnBrowseSavePath = new CommonUI.Controls.ThemedButton();
			this.m_btnSkipSave = new CommonUI.Controls.ThemedButton();
			this.m_btnSaveDxf = new CommonUI.Controls.ThemedButton();
			this.m_pnlSelectFace.SuspendLayout();
			this.m_secSelectFace.SuspendLayout();
			this.m_pnlInputParameters.SuspendLayout();
			this.m_secBase.SuspendLayout();
			this.m_secBracket.SuspendLayout();
			this.m_pnlXPositions.SuspendLayout();
			this.m_pnlYPositions.SuspendLayout();
			this.m_secBracketHeight.SuspendLayout();
			this.m_secCrossFit.SuspendLayout();
			this.m_secBracketCount.SuspendLayout();
			this.m_secBottomPinSize.SuspendLayout();
			this.m_pnlSaveDxf.SuspendLayout();
			this.m_secSaveDxf.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbXExtension)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbYExtension)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbZExtension)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRibHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbEngagementDepth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picCrossGap)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbXPinCount)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbYPinCount)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbPinLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbWallThickness)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbBaseThickness)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbFilletRadius)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picPinDimension)).BeginInit();
			this.SuspendLayout();
			// 
			// m_pnlSelectFace
			// 
			this.m_pnlSelectFace.Controls.Add(this.m_secSelectFace);
			this.m_pnlSelectFace.Controls.Add(this.m_btnNext);
			this.m_pnlSelectFace.Location = new System.Drawing.Point(12, 12);
			this.m_pnlSelectFace.Name = "m_pnlSelectFace";
			this.m_pnlSelectFace.Size = new System.Drawing.Size(356, 108);
			this.m_pnlSelectFace.TabIndex = 0;
			// 
			// m_secSelectFace
			// 
			this.m_secSelectFace.Controls.Add(this.m_lblHint);
			this.m_secSelectFace.Location = new System.Drawing.Point(0, 0);
			this.m_secSelectFace.Name = "m_secSelectFace";
			this.m_secSelectFace.Size = new System.Drawing.Size(356, 64);
			this.m_secSelectFace.TabIndex = 0;
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
			// m_pnlInputParameters
			// 
			this.m_pnlInputParameters.Controls.Add(this.m_secBase);
			this.m_pnlInputParameters.Controls.Add(this.m_secBracket);
			this.m_pnlInputParameters.Controls.Add(this.m_secBracketHeight);
			this.m_pnlInputParameters.Controls.Add(this.m_secCrossFit);
			this.m_pnlInputParameters.Controls.Add(this.m_secBracketCount);
			this.m_pnlInputParameters.Controls.Add(this.m_secBottomPinSize);
			this.m_pnlInputParameters.Controls.Add(this.m_btnBack);
			this.m_pnlInputParameters.Controls.Add(this.m_btnGenerate);
			this.m_pnlInputParameters.Location = new System.Drawing.Point(12, 12);
			this.m_pnlInputParameters.Name = "m_pnlInputParameters";
			this.m_pnlInputParameters.Size = new System.Drawing.Size(560, 943);
			this.m_pnlInputParameters.TabIndex = 1;
			// 
			// m_secBase
			// 
			this.m_secBase.Controls.Add(this.m_lblXExtension);
			this.m_secBase.Controls.Add(this.m_txbXExtension);
			this.m_secBase.Controls.Add(this.m_lblXExtensionUnit);
			this.m_secBase.Controls.Add(this.m_lblYExtension);
			this.m_secBase.Controls.Add(this.m_txbYExtension);
			this.m_secBase.Controls.Add(this.m_lblYExtensionUnit);
			this.m_secBase.Location = new System.Drawing.Point(0, 0);
			this.m_secBase.Name = "m_secBase";
			this.m_secBase.Size = new System.Drawing.Size(560, 99);
			this.m_secBase.TabIndex = 0;
			this.m_secBase.Text = "底部大小";
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
			// m_txbXExtension
			// 
			this.m_txbXExtension.DecimalPlaces = 1;
			this.m_txbXExtension.Location = new System.Drawing.Point(150, 36);
			this.m_txbXExtension.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_txbXExtension.Name = "m_txbXExtension";
			this.m_txbXExtension.Size = new System.Drawing.Size(100, 23);
			this.m_txbXExtension.TabIndex = 1;
			this.m_txbXExtension.Value = new decimal(new int[] { 50, 0, 0, 0});
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
			// m_txbYExtension
			// 
			this.m_txbYExtension.DecimalPlaces = 1;
			this.m_txbYExtension.Location = new System.Drawing.Point(150, 66);
			this.m_txbYExtension.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_txbYExtension.Name = "m_txbYExtension";
			this.m_txbYExtension.Size = new System.Drawing.Size(100, 23);
			this.m_txbYExtension.TabIndex = 4;
			this.m_txbYExtension.Value = new decimal(new int[] { 50, 0, 0, 0});
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
			// m_secBracket
			// 
			this.m_secBracket.Controls.Add(this.m_lblXPositionsTitle);
			this.m_secBracket.Controls.Add(this.m_lblYPositionsTitle);
			this.m_secBracket.Controls.Add(this.m_btnResetPositions);
			this.m_secBracket.Controls.Add(this.m_pnlXPositions);
			this.m_secBracket.Controls.Add(this.m_pnlYPositions);
			this.m_secBracket.Controls.Add(this.m_btnAddXPosition);
			this.m_secBracket.Controls.Add(this.m_btnAddYPosition);
			this.m_secBracket.Location = new System.Drawing.Point(0, 107);
			this.m_secBracket.Name = "m_secBracket";
			this.m_secBracket.Size = new System.Drawing.Size(560, 249);
			this.m_secBracket.TabIndex = 1;
			this.m_secBracket.Text = "支架位置";
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
			// m_secBracketHeight
			// 
			this.m_secBracketHeight.Controls.Add(this.m_lblZExtension);
			this.m_secBracketHeight.Controls.Add(this.m_txbZExtension);
			this.m_secBracketHeight.Controls.Add(this.m_lblZExtensionUnit);
			this.m_secBracketHeight.Controls.Add(this.m_lblRibHeight);
			this.m_secBracketHeight.Controls.Add(this.m_txbRibHeight);
			this.m_secBracketHeight.Controls.Add(this.m_lblRibHeightUnit);
			this.m_secBracketHeight.Location = new System.Drawing.Point(0, 364);
			this.m_secBracketHeight.Name = "m_secBracketHeight";
			this.m_secBracketHeight.Size = new System.Drawing.Size(560, 99);
			this.m_secBracketHeight.TabIndex = 2;
			this.m_secBracketHeight.Text = "支架高度";
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
			// m_txbZExtension
			// 
			this.m_txbZExtension.DecimalPlaces = 1;
			this.m_txbZExtension.Location = new System.Drawing.Point(150, 36);
			this.m_txbZExtension.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_txbZExtension.Name = "m_txbZExtension";
			this.m_txbZExtension.Size = new System.Drawing.Size(100, 23);
			this.m_txbZExtension.TabIndex = 1;
			this.m_txbZExtension.Value = new decimal(new int[] { 100, 0, 0, 0});
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
			// m_txbRibHeight
			// 
			this.m_txbRibHeight.DecimalPlaces = 1;
			this.m_txbRibHeight.Location = new System.Drawing.Point(150, 66);
			this.m_txbRibHeight.Minimum = new decimal(new int[] { 1, 0, 0, 0});
			this.m_txbRibHeight.Name = "m_txbRibHeight";
			this.m_txbRibHeight.Size = new System.Drawing.Size(100, 23);
			this.m_txbRibHeight.TabIndex = 4;
			this.m_txbRibHeight.Value = new decimal(new int[] { 50, 0, 0, 0});
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
			// m_secCrossFit
			// 
			this.m_secCrossFit.Controls.Add(this.m_lblEngagementDepth);
			this.m_secCrossFit.Controls.Add(this.m_txbEngagementDepth);
			this.m_secCrossFit.Controls.Add(this.m_lblEngagementDepthUnit);
			this.m_secCrossFit.Controls.Add(this.m_picCrossGap);
			this.m_secCrossFit.Location = new System.Drawing.Point(0, 471);
			this.m_secCrossFit.Name = "m_secCrossFit";
			this.m_secCrossFit.Size = new System.Drawing.Size(560, 154);
			this.m_secCrossFit.TabIndex = 3;
			this.m_secCrossFit.Text = "咬合設計";
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
			// m_txbEngagementDepth
			// 
			this.m_txbEngagementDepth.DecimalPlaces = 1;
			this.m_txbEngagementDepth.Location = new System.Drawing.Point(150, 36);
			this.m_txbEngagementDepth.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_txbEngagementDepth.Name = "m_txbEngagementDepth";
			this.m_txbEngagementDepth.Size = new System.Drawing.Size(100, 23);
			this.m_txbEngagementDepth.TabIndex = 1;
			this.m_txbEngagementDepth.Value = new decimal(new int[] { 5, 0, 0, 0});
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
			// m_secBracketCount
			// 
			this.m_secBracketCount.Controls.Add(this.m_lblXPinCount);
			this.m_secBracketCount.Controls.Add(this.m_txbXPinCount);
			this.m_secBracketCount.Controls.Add(this.m_lblYPinCount);
			this.m_secBracketCount.Controls.Add(this.m_txbYPinCount);
			this.m_secBracketCount.Location = new System.Drawing.Point(0, 633);
			this.m_secBracketCount.Name = "m_secBracketCount";
			this.m_secBracketCount.Size = new System.Drawing.Size(560, 99);
			this.m_secBracketCount.TabIndex = 4;
			this.m_secBracketCount.Text = "底部拼接數量";
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
			// m_txbXPinCount
			// 
			this.m_txbXPinCount.DecimalPlaces = 0;
			this.m_txbXPinCount.Location = new System.Drawing.Point(150, 36);
			this.m_txbXPinCount.Minimum = new decimal(new int[] { 1, 0, 0, 0});
			this.m_txbXPinCount.Name = "m_txbXPinCount";
			this.m_txbXPinCount.Size = new System.Drawing.Size(100, 23);
			this.m_txbXPinCount.TabIndex = 1;
			this.m_txbXPinCount.Value = new decimal(new int[] { 8, 0, 0, 0});
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
			// m_txbYPinCount
			// 
			this.m_txbYPinCount.DecimalPlaces = 0;
			this.m_txbYPinCount.Location = new System.Drawing.Point(150, 66);
			this.m_txbYPinCount.Minimum = new decimal(new int[] { 1, 0, 0, 0});
			this.m_txbYPinCount.Name = "m_txbYPinCount";
			this.m_txbYPinCount.Size = new System.Drawing.Size(100, 23);
			this.m_txbYPinCount.TabIndex = 3;
			this.m_txbYPinCount.Value = new decimal(new int[] { 2, 0, 0, 0});
			// 
			// m_secBottomPinSize
			// 
			this.m_secBottomPinSize.Controls.Add(this.m_lblPinLength);
			this.m_secBottomPinSize.Controls.Add(this.m_txbPinLength);
			this.m_secBottomPinSize.Controls.Add(this.m_lblPinLengthUnit);
			this.m_secBottomPinSize.Controls.Add(this.m_lblWallThickness);
			this.m_secBottomPinSize.Controls.Add(this.m_txbWallThickness);
			this.m_secBottomPinSize.Controls.Add(this.m_lblWallThicknessUnit);
			this.m_secBottomPinSize.Controls.Add(this.m_lblBaseThickness);
			this.m_secBottomPinSize.Controls.Add(this.m_txbBaseThickness);
			this.m_secBottomPinSize.Controls.Add(this.m_lblBaseThicknessUnit);
			this.m_secBottomPinSize.Controls.Add(this.m_lblFilletRadius);
			this.m_secBottomPinSize.Controls.Add(this.m_txbFilletRadius);
			this.m_secBottomPinSize.Controls.Add(this.m_lblFilletRadiusUnit);
			this.m_secBottomPinSize.Controls.Add(this.m_picPinDimension);
			this.m_secBottomPinSize.Location = new System.Drawing.Point(0, 740);
			this.m_secBottomPinSize.Name = "m_secBottomPinSize";
			this.m_secBottomPinSize.Size = new System.Drawing.Size(560, 159);
			this.m_secBottomPinSize.TabIndex = 5;
			this.m_secBottomPinSize.Text = "拼接尺寸";
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
			// m_txbPinLength
			// 
			this.m_txbPinLength.DecimalPlaces = 1;
			this.m_txbPinLength.Location = new System.Drawing.Point(150, 36);
			this.m_txbPinLength.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_txbPinLength.Name = "m_txbPinLength";
			this.m_txbPinLength.Size = new System.Drawing.Size(100, 23);
			this.m_txbPinLength.TabIndex = 1;
			this.m_txbPinLength.Value = new decimal(new int[] { 20, 0, 0, 0});
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
			this.m_lblWallThickness.TabIndex = 3;
			this.m_lblWallThickness.Text = "拼接寬度(C支架厚度)";
			// 
			// m_txbWallThickness
			// 
			this.m_txbWallThickness.DecimalPlaces = 1;
			this.m_txbWallThickness.Location = new System.Drawing.Point(150, 66);
			this.m_txbWallThickness.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_txbWallThickness.Name = "m_txbWallThickness";
			this.m_txbWallThickness.Size = new System.Drawing.Size(100, 23);
			this.m_txbWallThickness.TabIndex = 4;
			this.m_txbWallThickness.Value = new decimal(new int[] { 3, 0, 0, 0});
			// 
			// m_lblWallThicknessUnit
			// 
			this.m_lblWallThicknessUnit.AutoSize = true;
			this.m_lblWallThicknessUnit.Location = new System.Drawing.Point(256, 70);
			this.m_lblWallThicknessUnit.Name = "m_lblWallThicknessUnit";
			this.m_lblWallThicknessUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblWallThicknessUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblWallThicknessUnit.TabIndex = 5;
			this.m_lblWallThicknessUnit.Text = "mm";
			// 
			// m_lblBaseThickness
			// 
			this.m_lblBaseThickness.AutoSize = true;
			this.m_lblBaseThickness.Location = new System.Drawing.Point(10, 100);
			this.m_lblBaseThickness.Name = "m_lblBaseThickness";
			this.m_lblBaseThickness.Size = new System.Drawing.Size(120, 16);
			this.m_lblBaseThickness.TabIndex = 6;
			this.m_lblBaseThickness.Text = "拼接高度(D底部厚度)";
			// 
			// m_txbBaseThickness
			// 
			this.m_txbBaseThickness.DecimalPlaces = 1;
			this.m_txbBaseThickness.Location = new System.Drawing.Point(150, 96);
			this.m_txbBaseThickness.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_txbBaseThickness.Name = "m_txbBaseThickness";
			this.m_txbBaseThickness.Size = new System.Drawing.Size(100, 23);
			this.m_txbBaseThickness.TabIndex = 7;
			this.m_txbBaseThickness.Value = new decimal(new int[] { 5, 0, 0, 0});
			// 
			// m_lblBaseThicknessUnit
			// 
			this.m_lblBaseThicknessUnit.AutoSize = true;
			this.m_lblBaseThicknessUnit.Location = new System.Drawing.Point(256, 100);
			this.m_lblBaseThicknessUnit.Name = "m_lblBaseThicknessUnit";
			this.m_lblBaseThicknessUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblBaseThicknessUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblBaseThicknessUnit.TabIndex = 8;
			this.m_lblBaseThicknessUnit.Text = "mm";
			// 
			// m_lblFilletRadius
			// 
			this.m_lblFilletRadius.AutoSize = true;
			this.m_lblFilletRadius.Location = new System.Drawing.Point(10, 130);
			this.m_lblFilletRadius.Name = "m_lblFilletRadius";
			this.m_lblFilletRadius.Size = new System.Drawing.Size(120, 16);
			this.m_lblFilletRadius.TabIndex = 9;
			this.m_lblFilletRadius.Text = "導圓角半徑";
			// 
			// m_txbFilletRadius
			// 
			this.m_txbFilletRadius.DecimalPlaces = 1;
			this.m_txbFilletRadius.Location = new System.Drawing.Point(150, 126);
			this.m_txbFilletRadius.Minimum = new decimal(new int[] { 0, 0, 0, 0});
			this.m_txbFilletRadius.Name = "m_txbFilletRadius";
			this.m_txbFilletRadius.Size = new System.Drawing.Size(100, 23);
			this.m_txbFilletRadius.TabIndex = 10;
			this.m_txbFilletRadius.Value = new decimal(new int[] { 1, 0, 0, 0});
			// 
			// m_lblFilletRadiusUnit
			// 
			this.m_lblFilletRadiusUnit.AutoSize = true;
			this.m_lblFilletRadiusUnit.Location = new System.Drawing.Point(256, 130);
			this.m_lblFilletRadiusUnit.Name = "m_lblFilletRadiusUnit";
			this.m_lblFilletRadiusUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblFilletRadiusUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblFilletRadiusUnit.TabIndex = 11;
			this.m_lblFilletRadiusUnit.Text = "mm";
			// 
			// m_picPinDimension
			// 
			this.m_picPinDimension.Location = new System.Drawing.Point(350, 36);
			this.m_picPinDimension.Name = "m_picPinDimension";
			this.m_picPinDimension.Size = new System.Drawing.Size(200, 108);
			this.m_picPinDimension.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.m_picPinDimension.TabIndex = 12;
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
			// m_pnlSaveDxf
			// 
			this.m_pnlSaveDxf.Controls.Add(this.m_secSaveDxf);
			this.m_pnlSaveDxf.Controls.Add(this.m_btnSkipSave);
			this.m_pnlSaveDxf.Controls.Add(this.m_btnSaveDxf);
			this.m_pnlSaveDxf.Location = new System.Drawing.Point(12, 12);
			this.m_pnlSaveDxf.Name = "m_pnlSaveDxf";
			this.m_pnlSaveDxf.Size = new System.Drawing.Size(356, 117);
			this.m_pnlSaveDxf.TabIndex = 2;
			// 
			// m_secSaveDxf
			// 
			this.m_secSaveDxf.Controls.Add(this.m_lblSaveDxfHint);
			this.m_secSaveDxf.Controls.Add(this.m_txbSavePath);
			this.m_secSaveDxf.Controls.Add(this.m_btnBrowseSavePath);
			this.m_secSaveDxf.Location = new System.Drawing.Point(0, 0);
			this.m_secSaveDxf.Name = "m_secSaveDxf";
			this.m_secSaveDxf.Size = new System.Drawing.Size(356, 73);
			this.m_secSaveDxf.TabIndex = 0;
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
			// m_txbSavePath
			// 
			this.m_txbSavePath.Location = new System.Drawing.Point(10, 40);
			this.m_txbSavePath.Name = "m_txbSavePath";
			this.m_txbSavePath.ReadOnly = true;
			this.m_txbSavePath.Size = new System.Drawing.Size(250, 23);
			this.m_txbSavePath.TabIndex = 1;
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
			this.Controls.Add(this.m_pnlSelectFace);
			this.Controls.Add(this.m_pnlInputParameters);
			this.Controls.Add(this.m_pnlSaveDxf);
			this.Name = "DesignSupportBracketDlg";
			this.Text = "設計夾具";
			this.m_pnlSelectFace.ResumeLayout(false);
			this.m_secSelectFace.ResumeLayout(false);
			this.m_pnlInputParameters.ResumeLayout(false);
			this.m_secBase.ResumeLayout(false);
			this.m_secBase.PerformLayout();
			this.m_secBracket.ResumeLayout(false);
			this.m_secBracket.PerformLayout();
			this.m_pnlXPositions.ResumeLayout(false);
			this.m_pnlYPositions.ResumeLayout(false);
			this.m_secBracketHeight.ResumeLayout(false);
			this.m_secBracketHeight.PerformLayout();
			this.m_secCrossFit.ResumeLayout(false);
			this.m_secCrossFit.PerformLayout();
			this.m_secBracketCount.ResumeLayout(false);
			this.m_secBracketCount.PerformLayout();
			this.m_secBottomPinSize.ResumeLayout(false);
			this.m_secBottomPinSize.PerformLayout();
			this.m_pnlSaveDxf.ResumeLayout(false);
			this.m_secSaveDxf.ResumeLayout(false);
			this.m_secSaveDxf.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbXExtension)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbYExtension)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbZExtension)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRibHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbEngagementDepth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picCrossGap)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbXPinCount)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbYPinCount)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbPinLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbWallThickness)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbBaseThickness)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbFilletRadius)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picPinDimension)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel m_pnlSelectFace;
		private CommonUI.Controls.SectionPanel m_secSelectFace;
		private System.Windows.Forms.Label m_lblHint;
		private CommonUI.Controls.ThemedButton m_btnNext;
		private System.Windows.Forms.Panel m_pnlInputParameters;
		private CommonUI.Controls.SectionPanel m_secBase;
		private System.Windows.Forms.Label m_lblXExtension;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbXExtension;
		private CommonUI.Controls.ThemedLabel m_lblXExtensionUnit;
		private System.Windows.Forms.Label m_lblYExtension;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbYExtension;
		private CommonUI.Controls.ThemedLabel m_lblYExtensionUnit;
		private CommonUI.Controls.SectionPanel m_secBracket;
		private CommonUI.Controls.ThemedLabel m_lblXPositionsTitle;
		private CommonUI.Controls.ThemedLabel m_lblYPositionsTitle;
		private CommonUI.Controls.ThemedButton m_btnResetPositions;
		private System.Windows.Forms.Panel m_pnlXPositions;
		private System.Windows.Forms.Panel m_pnlYPositions;
		private CommonUI.Controls.ThemedButton m_btnAddXPosition;
		private CommonUI.Controls.ThemedButton m_btnAddYPosition;
		private CommonUI.Controls.SectionPanel m_secBracketHeight;
		private System.Windows.Forms.Label m_lblZExtension;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbZExtension;
		private CommonUI.Controls.ThemedLabel m_lblZExtensionUnit;
		private System.Windows.Forms.Label m_lblRibHeight;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbRibHeight;
		private CommonUI.Controls.ThemedLabel m_lblRibHeightUnit;
		private CommonUI.Controls.SectionPanel m_secCrossFit;
		private System.Windows.Forms.Label m_lblEngagementDepth;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbEngagementDepth;
		private CommonUI.Controls.ThemedLabel m_lblEngagementDepthUnit;
		private System.Windows.Forms.PictureBox m_picCrossGap;
		private CommonUI.Controls.SectionPanel m_secBracketCount;
		private System.Windows.Forms.Label m_lblXPinCount;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbXPinCount;
		private System.Windows.Forms.Label m_lblYPinCount;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbYPinCount;
		private CommonUI.Controls.SectionPanel m_secBottomPinSize;
		private System.Windows.Forms.Label m_lblPinLength;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbPinLength;
		private CommonUI.Controls.ThemedLabel m_lblPinLengthUnit;
		private System.Windows.Forms.Label m_lblWallThickness;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbWallThickness;
		private CommonUI.Controls.ThemedLabel m_lblWallThicknessUnit;
		private System.Windows.Forms.Label m_lblBaseThickness;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbBaseThickness;
		private CommonUI.Controls.ThemedLabel m_lblBaseThicknessUnit;
		private System.Windows.Forms.Label m_lblFilletRadius;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbFilletRadius;
		private CommonUI.Controls.ThemedLabel m_lblFilletRadiusUnit;
		private System.Windows.Forms.PictureBox m_picPinDimension;
		private CommonUI.Controls.ThemedButton m_btnBack;
		private CommonUI.Controls.ThemedButton m_btnGenerate;
		private System.Windows.Forms.Panel m_pnlSaveDxf;
		private CommonUI.Controls.SectionPanel m_secSaveDxf;
		private System.Windows.Forms.Label m_lblSaveDxfHint;
		private System.Windows.Forms.TextBox m_txbSavePath;
		private CommonUI.Controls.ThemedButton m_btnBrowseSavePath;
		private CommonUI.Controls.ThemedButton m_btnSkipSave;
		private CommonUI.Controls.ThemedButton m_btnSaveDxf;
	}
}
