using System.Windows.Forms;

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
			if( disposing ) {
				m_picCrossGap?.Image?.Dispose();
				m_picPinDimension?.Image?.Dispose();
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
			this.m_lblHint = new System.Windows.Forms.Label();
			this.m_btnNext = new System.Windows.Forms.Button();
			this.m_pnlInputParameters = new System.Windows.Forms.Panel();
			this.m_lblSectionBase = new System.Windows.Forms.Label();
			this.m_lblXExtension = new System.Windows.Forms.Label();
			this.m_txbXExtension = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblXExtensionUnit = new System.Windows.Forms.Label();
			this.m_lblYExtension = new System.Windows.Forms.Label();
			this.m_txbYExtension = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblYExtensionUnit = new System.Windows.Forms.Label();
			this.m_lblSectionBracket = new System.Windows.Forms.Label();
			this.m_lblXPositionsTitle = new System.Windows.Forms.Label();
			this.m_lblYPositionsTitle = new System.Windows.Forms.Label();
			this.m_pnlXPositions = new System.Windows.Forms.Panel();
			this.m_pnlYPositions = new System.Windows.Forms.Panel();
			this.m_btnAddXPosition = new System.Windows.Forms.Button();
			this.m_btnAddYPosition = new System.Windows.Forms.Button();
			this.m_lblSectionBracketHeight = new System.Windows.Forms.Label();
			this.m_picCrossGap = new System.Windows.Forms.PictureBox();
			this.m_picPinDimension = new System.Windows.Forms.PictureBox();
			this.m_lblZExtension = new System.Windows.Forms.Label();
			this.m_txbZExtension = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblZExtensionUnit = new System.Windows.Forms.Label();
			this.m_lblRibHeight = new System.Windows.Forms.Label();
			this.m_txbRibHeight = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblRibHeightUnit = new System.Windows.Forms.Label();
			this.m_lblSectionBracketCount = new System.Windows.Forms.Label();
			this.m_lblSectionCrossFit = new System.Windows.Forms.Label();
			this.m_lblEngagementDepth = new System.Windows.Forms.Label();
			this.m_txbEngagementDepth = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblEngagementDepthUnit = new System.Windows.Forms.Label();
			this.m_lblXPinCount = new System.Windows.Forms.Label();
			this.m_txbXPinCount = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblYPinCount = new System.Windows.Forms.Label();
			this.m_txbYPinCount = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblSectionBottomPinSize = new System.Windows.Forms.Label();
			this.m_lblBaseThickness = new System.Windows.Forms.Label();
			this.m_txbBaseThickness = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblBaseThicknessUnit = new System.Windows.Forms.Label();
			this.m_lblWallThickness = new System.Windows.Forms.Label();
			this.m_txbWallThickness = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblWallThicknessUnit = new System.Windows.Forms.Label();
			this.m_lblPinLength = new System.Windows.Forms.Label();
			this.m_txbPinLength = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblPinLengthUnit = new System.Windows.Forms.Label();
			this.m_btnBack = new System.Windows.Forms.Button();
			this.m_btnGenerate = new System.Windows.Forms.Button();
			this.m_pnlSaveDxf = new System.Windows.Forms.Panel();
			this.m_lblSaveDxfHint = new System.Windows.Forms.Label();
			this.m_txbSavePath = new System.Windows.Forms.TextBox();
			this.m_btnBrowseSavePath = new System.Windows.Forms.Button();
			this.m_btnSaveDxf = new System.Windows.Forms.Button();
			this.m_btnSkipSave = new System.Windows.Forms.Button();
			this.m_pnlSelectFace.SuspendLayout();
			this.m_pnlInputParameters.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbXExtension)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbYExtension)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picCrossGap)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picPinDimension)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbZExtension)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRibHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbEngagementDepth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbXPinCount)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbYPinCount)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbPinLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbWallThickness)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbBaseThickness)).BeginInit();
			this.m_pnlSaveDxf.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_pnlSelectFace
			// 
			this.m_pnlSelectFace.Controls.Add(this.m_lblHint);
			this.m_pnlSelectFace.Controls.Add(this.m_btnNext);
			this.m_pnlSelectFace.Location = new System.Drawing.Point(9, 8);
			this.m_pnlSelectFace.Name = "m_pnlSelectFace";
			this.m_pnlSelectFace.Size = new System.Drawing.Size(306, 76);
			this.m_pnlSelectFace.TabIndex = 0;
			// 
			// m_lblHint
			// 
			this.m_lblHint.AutoSize = false;
			this.m_lblHint.Location = new System.Drawing.Point(0, 2);
			this.m_lblHint.Name = "m_lblHint";
			this.m_lblHint.Size = new System.Drawing.Size(300, 40);
			this.m_lblHint.TabIndex = 0;
			this.m_lblHint.Text = "請點選工件上要放置夾具的底面／側面\r\n（支援 Ctrl 多選、雙擊連續面選取）";
			// 
			// m_btnNext
			// 
			this.m_btnNext.Enabled = false;
			this.m_btnNext.Location = new System.Drawing.Point(203, 43);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(103, 33);
			this.m_btnNext.TabIndex = 1;
			this.m_btnNext.Text = "下一步";
			this.m_btnNext.UseVisualStyleBackColor = true;
			this.m_btnNext.Click += new System.EventHandler(this.m_btnNext_Click);
			// 
			// m_pnlInputParameters
			// 
			this.m_pnlInputParameters.Controls.Add(this.m_lblSectionBase);
			this.m_pnlInputParameters.Controls.Add(this.m_lblXExtension);
			this.m_pnlInputParameters.Controls.Add(this.m_txbXExtension);
			this.m_pnlInputParameters.Controls.Add(this.m_lblXExtensionUnit);
			this.m_pnlInputParameters.Controls.Add(this.m_lblYExtension);
			this.m_pnlInputParameters.Controls.Add(this.m_txbYExtension);
			this.m_pnlInputParameters.Controls.Add(this.m_lblYExtensionUnit);
			this.m_pnlInputParameters.Controls.Add(this.m_lblSectionBracket);
			this.m_pnlInputParameters.Controls.Add(this.m_lblXPositionsTitle);
			this.m_pnlInputParameters.Controls.Add(this.m_lblYPositionsTitle);
			this.m_pnlInputParameters.Controls.Add(this.m_pnlXPositions);
			this.m_pnlInputParameters.Controls.Add(this.m_pnlYPositions);
			this.m_pnlInputParameters.Controls.Add(this.m_btnAddXPosition);
			this.m_pnlInputParameters.Controls.Add(this.m_btnAddYPosition);
			this.m_pnlInputParameters.Controls.Add(this.m_lblSectionBracketHeight);
			this.m_pnlInputParameters.Controls.Add(this.m_picCrossGap);
			this.m_pnlInputParameters.Controls.Add(this.m_picPinDimension);
			this.m_pnlInputParameters.Controls.Add(this.m_lblZExtension);
			this.m_pnlInputParameters.Controls.Add(this.m_txbZExtension);
			this.m_pnlInputParameters.Controls.Add(this.m_lblZExtensionUnit);
			this.m_pnlInputParameters.Controls.Add(this.m_lblRibHeight);
			this.m_pnlInputParameters.Controls.Add(this.m_txbRibHeight);
			this.m_pnlInputParameters.Controls.Add(this.m_lblRibHeightUnit);
			this.m_pnlInputParameters.Controls.Add(this.m_lblSectionCrossFit);
			this.m_pnlInputParameters.Controls.Add(this.m_lblEngagementDepth);
			this.m_pnlInputParameters.Controls.Add(this.m_txbEngagementDepth);
			this.m_pnlInputParameters.Controls.Add(this.m_lblEngagementDepthUnit);
			this.m_pnlInputParameters.Controls.Add(this.m_lblSectionBracketCount);
			this.m_pnlInputParameters.Controls.Add(this.m_lblXPinCount);
			this.m_pnlInputParameters.Controls.Add(this.m_txbXPinCount);
			this.m_pnlInputParameters.Controls.Add(this.m_lblYPinCount);
			this.m_pnlInputParameters.Controls.Add(this.m_txbYPinCount);
			this.m_pnlInputParameters.Controls.Add(this.m_lblSectionBottomPinSize);
			this.m_pnlInputParameters.Controls.Add(this.m_lblBaseThickness);
			this.m_pnlInputParameters.Controls.Add(this.m_txbBaseThickness);
			this.m_pnlInputParameters.Controls.Add(this.m_lblBaseThicknessUnit);
			this.m_pnlInputParameters.Controls.Add(this.m_lblWallThickness);
			this.m_pnlInputParameters.Controls.Add(this.m_txbWallThickness);
			this.m_pnlInputParameters.Controls.Add(this.m_lblWallThicknessUnit);
			this.m_pnlInputParameters.Controls.Add(this.m_lblPinLength);
			this.m_pnlInputParameters.Controls.Add(this.m_txbPinLength);
			this.m_pnlInputParameters.Controls.Add(this.m_lblPinLengthUnit);
			this.m_pnlInputParameters.Controls.Add(this.m_btnBack);
			this.m_pnlInputParameters.Controls.Add(this.m_btnGenerate);
			this.m_pnlInputParameters.Location = new System.Drawing.Point(9, 8);
			this.m_pnlInputParameters.Name = "m_pnlInputParameters";
			this.m_pnlInputParameters.Size = new System.Drawing.Size(560, 762);
			this.m_pnlInputParameters.TabIndex = 1;
			// 
			// m_lblSectionBase
			// 
			this.m_lblSectionBase.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold);
			this.m_lblSectionBase.Location = new System.Drawing.Point(0, 10);
			this.m_lblSectionBase.Name = "m_lblSectionBase";
			this.m_lblSectionBase.Size = new System.Drawing.Size(540, 20);
			this.m_lblSectionBase.TabIndex = 0;
			this.m_lblSectionBase.Text = "底部大小";
			// 
			// m_lblXExtension
			// 
			this.m_lblXExtension.Location = new System.Drawing.Point(11, 34);
			this.m_lblXExtension.Name = "m_lblXExtension";
			this.m_lblXExtension.Size = new System.Drawing.Size(160, 15);
			this.m_lblXExtension.TabIndex = 1;
			this.m_lblXExtension.Text = "X 方向延伸距離";
			// 
			// m_txbXExtension
			// 
			this.m_txbXExtension.Location = new System.Drawing.Point(175, 32);
			this.m_txbXExtension.Name = "m_txbXExtension";
			this.m_txbXExtension.Size = new System.Drawing.Size(62, 25);
			this.m_txbXExtension.TabIndex = 2;
			this.m_txbXExtension.Minimum = new decimal(new int[] {
			0,
			0,
			0,
			0});
			this.m_txbXExtension.Value = new decimal(new int[] {
			50,
			0,
			0,
			0});
			this.m_txbXExtension.DecimalPlaces = 1;
			// 
			// m_lblXExtensionUnit
			// 
			this.m_lblXExtensionUnit.Location = new System.Drawing.Point(242, 32);
			this.m_lblXExtensionUnit.Name = "m_lblXExtensionUnit";
			this.m_lblXExtensionUnit.Size = new System.Drawing.Size(34, 25);
			this.m_lblXExtensionUnit.TabIndex = 3;
			this.m_lblXExtensionUnit.Text = "mm";
			this.m_lblXExtensionUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_lblYExtension
			// 
			this.m_lblYExtension.Location = new System.Drawing.Point(11, 60);
			this.m_lblYExtension.Name = "m_lblYExtension";
			this.m_lblYExtension.Size = new System.Drawing.Size(160, 15);
			this.m_lblYExtension.TabIndex = 4;
			this.m_lblYExtension.Text = "Y 方向延伸距離";
			// 
			// m_txbYExtension
			// 
			this.m_txbYExtension.Location = new System.Drawing.Point(175, 58);
			this.m_txbYExtension.Name = "m_txbYExtension";
			this.m_txbYExtension.Size = new System.Drawing.Size(62, 25);
			this.m_txbYExtension.TabIndex = 5;
			this.m_txbYExtension.Minimum = new decimal(new int[] {
			0,
			0,
			0,
			0});
			this.m_txbYExtension.Value = new decimal(new int[] {
			50,
			0,
			0,
			0});
			this.m_txbYExtension.DecimalPlaces = 1;
			// 
			// m_lblYExtensionUnit
			// 
			this.m_lblYExtensionUnit.Location = new System.Drawing.Point(242, 58);
			this.m_lblYExtensionUnit.Name = "m_lblYExtensionUnit";
			this.m_lblYExtensionUnit.Size = new System.Drawing.Size(34, 25);
			this.m_lblYExtensionUnit.TabIndex = 6;
			this.m_lblYExtensionUnit.Text = "mm";
			this.m_lblYExtensionUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_lblSectionBracket
			// 
			this.m_lblSectionBracket.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold);
			this.m_lblSectionBracket.Location = new System.Drawing.Point(0, 104);
			this.m_lblSectionBracket.Name = "m_lblSectionBracket";
			this.m_lblSectionBracket.Size = new System.Drawing.Size(540, 20);
			this.m_lblSectionBracket.TabIndex = 7;
			this.m_lblSectionBracket.Text = "支架位置";
			// 
			// m_lblXPositionsTitle
			// 
			this.m_lblXPositionsTitle.Location = new System.Drawing.Point(11, 128);
			this.m_lblXPositionsTitle.Name = "m_lblXPositionsTitle";
			this.m_lblXPositionsTitle.Size = new System.Drawing.Size(260, 20);
			this.m_lblXPositionsTitle.TabIndex = 8;
			this.m_lblXPositionsTitle.Text = "X 方向支架位置";
			// 
			// m_lblYPositionsTitle
			// 
			this.m_lblYPositionsTitle.Location = new System.Drawing.Point(285, 128);
			this.m_lblYPositionsTitle.Name = "m_lblYPositionsTitle";
			this.m_lblYPositionsTitle.Size = new System.Drawing.Size(260, 20);
			this.m_lblYPositionsTitle.TabIndex = 9;
			this.m_lblYPositionsTitle.Text = "Y 方向支架位置";
			// 
			// m_pnlXPositions - scrollable list, height sized to show 6 rows
			// 
			this.m_pnlXPositions.AutoScroll = true;
			this.m_pnlXPositions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.m_pnlXPositions.Location = new System.Drawing.Point(11, 150);
			this.m_pnlXPositions.Name = "m_pnlXPositions";
			this.m_pnlXPositions.Size = new System.Drawing.Size(260, 162);
			this.m_pnlXPositions.TabIndex = 10;
			// 
			// m_pnlYPositions - scrollable list, height sized to show 6 rows
			// 
			this.m_pnlYPositions.AutoScroll = true;
			this.m_pnlYPositions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.m_pnlYPositions.Location = new System.Drawing.Point(285, 150);
			this.m_pnlYPositions.Name = "m_pnlYPositions";
			this.m_pnlYPositions.Size = new System.Drawing.Size(260, 162);
			this.m_pnlYPositions.TabIndex = 11;
			// 
			// m_btnAddXPosition
			// 
			this.m_btnAddXPosition.Location = new System.Drawing.Point(11, 316);
			this.m_btnAddXPosition.Name = "m_btnAddXPosition";
			this.m_btnAddXPosition.Size = new System.Drawing.Size(260, 26);
			this.m_btnAddXPosition.TabIndex = 12;
			this.m_btnAddXPosition.Text = "+ 新增 X 支架位置";
			this.m_btnAddXPosition.UseVisualStyleBackColor = true;
			this.m_btnAddXPosition.Click += new System.EventHandler(this.m_btnAddXPosition_Click);
			// 
			// m_btnAddYPosition
			// 
			this.m_btnAddYPosition.Location = new System.Drawing.Point(285, 316);
			this.m_btnAddYPosition.Name = "m_btnAddYPosition";
			this.m_btnAddYPosition.Size = new System.Drawing.Size(260, 26);
			this.m_btnAddYPosition.TabIndex = 13;
			this.m_btnAddYPosition.Text = "+ 新增 Y 支架位置";
			this.m_btnAddYPosition.UseVisualStyleBackColor = true;
			this.m_btnAddYPosition.Click += new System.EventHandler(this.m_btnAddYPosition_Click);
			// 
			// m_lblSectionBracketHeight
			// 
			this.m_lblSectionBracketHeight.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold);
			this.m_lblSectionBracketHeight.Location = new System.Drawing.Point(0, 360);
			this.m_lblSectionBracketHeight.Name = "m_lblSectionBracketHeight";
			this.m_lblSectionBracketHeight.Size = new System.Drawing.Size(540, 20);
			this.m_lblSectionBracketHeight.TabIndex = 14;
			this.m_lblSectionBracketHeight.Text = "支架高度";
			// 
			// m_picCrossGap
			// 
			this.m_picCrossGap.Location = new System.Drawing.Point( 285, 400 );
			this.m_picCrossGap.Name = "m_picCrossGap";
			this.m_picCrossGap.Size = new System.Drawing.Size( 260, 140 );
			this.m_picCrossGap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.m_picCrossGap.TabIndex = 35;
			this.m_picCrossGap.TabStop = false;
			// 
			// m_picPinDimension
			// 
			this.m_picPinDimension.Location = new System.Drawing.Point( 285, 560 );
			this.m_picPinDimension.Name = "m_picPinDimension";
			this.m_picPinDimension.Size = new System.Drawing.Size( 260, 140 );
			this.m_picPinDimension.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.m_picPinDimension.TabIndex = 36;
			this.m_picPinDimension.TabStop = false;
			// 
			// m_lblZExtension
			// 
			this.m_lblZExtension.Location = new System.Drawing.Point(11, 384);
			this.m_lblZExtension.Name = "m_lblZExtension";
			this.m_lblZExtension.Size = new System.Drawing.Size(160, 20);
			this.m_lblZExtension.TabIndex = 15;
			this.m_lblZExtension.Text = "工件離地距離";
			// 
			// m_txbZExtension
			// 
			this.m_txbZExtension.Location = new System.Drawing.Point(175, 382);
			this.m_txbZExtension.Name = "m_txbZExtension";
			this.m_txbZExtension.Size = new System.Drawing.Size(62, 25);
			this.m_txbZExtension.TabIndex = 16;
			this.m_txbZExtension.Value = new decimal(new int[] {
			100,
			0,
			0,
			0});
			this.m_txbZExtension.DecimalPlaces = 1;
			this.m_txbZExtension.Minimum = new decimal(new int[] {
			0,
			0,
			0,
			0});
			// 
			// m_lblZExtensionUnit
			// 
			this.m_lblZExtensionUnit.Location = new System.Drawing.Point(242, 382);
			this.m_lblZExtensionUnit.Name = "m_lblZExtensionUnit";
			this.m_lblZExtensionUnit.Size = new System.Drawing.Size(34, 25);
			this.m_lblZExtensionUnit.TabIndex = 17;
			this.m_lblZExtensionUnit.Text = "mm";
			this.m_lblZExtensionUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_lblRibHeight
			// 
			this.m_lblRibHeight.Location = new System.Drawing.Point(11, 410);
			this.m_lblRibHeight.Name = "m_lblRibHeight";
			this.m_lblRibHeight.Size = new System.Drawing.Size(160, 20);
			this.m_lblRibHeight.TabIndex = 18;
			this.m_lblRibHeight.Text = "支架基礎高度";
			// 
			// m_txbRibHeight
			// 
			this.m_txbRibHeight.Location = new System.Drawing.Point(175, 408);
			this.m_txbRibHeight.Name = "m_txbRibHeight";
			this.m_txbRibHeight.Size = new System.Drawing.Size(62, 25);
			this.m_txbRibHeight.TabIndex = 19;
			this.m_txbRibHeight.Value = new decimal(new int[] {
			50,
			0,
			0,
			0});
			this.m_txbRibHeight.DecimalPlaces = 1;
			this.m_txbRibHeight.Minimum = new decimal(new int[] {
			1,
			0,
			0,
			0});
			// 
			// m_lblRibHeightUnit
			// 
			this.m_lblRibHeightUnit.Location = new System.Drawing.Point(242, 408);
			this.m_lblRibHeightUnit.Name = "m_lblRibHeightUnit";
			this.m_lblRibHeightUnit.Size = new System.Drawing.Size(34, 25);
			this.m_lblRibHeightUnit.TabIndex = 20;
			this.m_lblRibHeightUnit.Text = "mm";
			this.m_lblRibHeightUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_lblSectionCrossFit
			// 
			this.m_lblSectionCrossFit.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold);
			this.m_lblSectionCrossFit.Location = new System.Drawing.Point(0, 446);
			this.m_lblSectionCrossFit.Name = "m_lblSectionCrossFit";
			this.m_lblSectionCrossFit.Size = new System.Drawing.Size(540, 20);
			this.m_lblSectionCrossFit.TabIndex = 20;
			this.m_lblSectionCrossFit.Text = "咬合設計";
			// 
			// m_lblEngagementDepth
			// 
			this.m_lblEngagementDepth.Location = new System.Drawing.Point(11, 470);
			this.m_lblEngagementDepth.Name = "m_lblEngagementDepth";
			this.m_lblEngagementDepth.Size = new System.Drawing.Size(160, 20);
			this.m_lblEngagementDepth.TabIndex = 20;
			this.m_lblEngagementDepth.Text = "支架咬合深度(A)";
			// 
			// m_txbEngagementDepth
			// 
			this.m_txbEngagementDepth.Location = new System.Drawing.Point(175, 468);
			this.m_txbEngagementDepth.Name = "m_txbEngagementDepth";
			this.m_txbEngagementDepth.Size = new System.Drawing.Size(62, 25);
			this.m_txbEngagementDepth.TabIndex = 20;
			this.m_txbEngagementDepth.Value = new decimal(new int[] {
			5,
			0,
			0,
			0});
			this.m_txbEngagementDepth.DecimalPlaces = 1;
			this.m_txbEngagementDepth.Minimum = new decimal(new int[] {
			0,
			0,
			0,
			0});
			// 
			// m_lblEngagementDepthUnit
			// 
			this.m_lblEngagementDepthUnit.Location = new System.Drawing.Point(242, 468);
			this.m_lblEngagementDepthUnit.Name = "m_lblEngagementDepthUnit";
			this.m_lblEngagementDepthUnit.Size = new System.Drawing.Size(34, 25);
			this.m_lblEngagementDepthUnit.TabIndex = 20;
			this.m_lblEngagementDepthUnit.Text = "mm";
			this.m_lblEngagementDepthUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_lblSectionBracketCount
			// 
			this.m_lblSectionBracketCount.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold);
			this.m_lblSectionBracketCount.Location = new System.Drawing.Point(0, 506);
			this.m_lblSectionBracketCount.Name = "m_lblSectionBracketCount";
			this.m_lblSectionBracketCount.Size = new System.Drawing.Size(540, 20);
			this.m_lblSectionBracketCount.TabIndex = 21;
			this.m_lblSectionBracketCount.Text = "底部拼接數量";
			// 
			// m_lblXPinCount
			// 
			this.m_lblXPinCount.Location = new System.Drawing.Point(11, 530);
			this.m_lblXPinCount.Name = "m_lblXPinCount";
			this.m_lblXPinCount.Size = new System.Drawing.Size(160, 20);
			this.m_lblXPinCount.TabIndex = 22;
			this.m_lblXPinCount.Text = "X 方向拼接數量";
			// 
			// m_txbXPinCount
			// 
			this.m_txbXPinCount.Location = new System.Drawing.Point(175, 528);
			this.m_txbXPinCount.Name = "m_txbXPinCount";
			this.m_txbXPinCount.Size = new System.Drawing.Size(62, 25);
			this.m_txbXPinCount.TabIndex = 23;
			this.m_txbXPinCount.Value = new decimal(new int[] {
			8,
			0,
			0,
			0});
			this.m_txbXPinCount.DecimalPlaces = 0;
			this.m_txbXPinCount.Minimum = new decimal(new int[] {
			1,
			0,
			0,
			0});
			// 
			// m_lblYPinCount
			// 
			this.m_lblYPinCount.Location = new System.Drawing.Point(11, 556);
			this.m_lblYPinCount.Name = "m_lblYPinCount";
			this.m_lblYPinCount.Size = new System.Drawing.Size(160, 20);
			this.m_lblYPinCount.TabIndex = 24;
			this.m_lblYPinCount.Text = "Y 方向拼接數量";
			// 
			// m_txbYPinCount
			// 
			this.m_txbYPinCount.Location = new System.Drawing.Point(175, 554);
			this.m_txbYPinCount.Name = "m_txbYPinCount";
			this.m_txbYPinCount.Size = new System.Drawing.Size(62, 25);
			this.m_txbYPinCount.TabIndex = 25;
			this.m_txbYPinCount.Value = new decimal(new int[] {
			2,
			0,
			0,
			0});
			this.m_txbYPinCount.DecimalPlaces = 0;
			this.m_txbYPinCount.Minimum = new decimal(new int[] {
			1,
			0,
			0,
			0});
			// 
			// m_lblSectionBottomPinSize
			// 
			this.m_lblSectionBottomPinSize.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold);
			this.m_lblSectionBottomPinSize.Location = new System.Drawing.Point(0, 592);
			this.m_lblSectionBottomPinSize.Name = "m_lblSectionBottomPinSize";
			this.m_lblSectionBottomPinSize.Size = new System.Drawing.Size(540, 20);
			this.m_lblSectionBottomPinSize.TabIndex = 26;
			this.m_lblSectionBottomPinSize.Text = "拼接尺寸";
			// 
			// m_lblPinLength
			// 
			this.m_lblPinLength.Location = new System.Drawing.Point(11, 616);
			this.m_lblPinLength.Name = "m_lblPinLength";
			this.m_lblPinLength.Size = new System.Drawing.Size(160, 20);
			this.m_lblPinLength.TabIndex = 37;
			this.m_lblPinLength.Text = "拼接長度(B)";
			// 
			// m_txbPinLength
			// 
			this.m_txbPinLength.Location = new System.Drawing.Point(175, 614);
			this.m_txbPinLength.Name = "m_txbPinLength";
			this.m_txbPinLength.Size = new System.Drawing.Size(62, 25);
			this.m_txbPinLength.TabIndex = 38;
			this.m_txbPinLength.Value = new decimal(new int[] {
			20,
			0,
			0,
			0});
			this.m_txbPinLength.DecimalPlaces = 1;
			this.m_txbPinLength.Minimum = new decimal(new int[] {
			0,
			0,
			0,
			0});
			// 
			// m_lblPinLengthUnit
			// 
			this.m_lblPinLengthUnit.Location = new System.Drawing.Point(242, 614);
			this.m_lblPinLengthUnit.Name = "m_lblPinLengthUnit";
			this.m_lblPinLengthUnit.Size = new System.Drawing.Size(34, 25);
			this.m_lblPinLengthUnit.TabIndex = 39;
			this.m_lblPinLengthUnit.Text = "mm";
			this.m_lblPinLengthUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_lblWallThickness
			// 
			this.m_lblWallThickness.Location = new System.Drawing.Point(11, 642);
			this.m_lblWallThickness.Name = "m_lblWallThickness";
			this.m_lblWallThickness.Size = new System.Drawing.Size(160, 20);
			this.m_lblWallThickness.TabIndex = 31;
			this.m_lblWallThickness.Text = "拼接寬度(C支架厚度)";
			// 
			// m_txbWallThickness
			// 
			this.m_txbWallThickness.Location = new System.Drawing.Point(175, 640);
			this.m_txbWallThickness.Name = "m_txbWallThickness";
			this.m_txbWallThickness.Size = new System.Drawing.Size(62, 25);
			this.m_txbWallThickness.TabIndex = 32;
			this.m_txbWallThickness.Value = new decimal(new int[] {
			3,
			0,
			0,
			0});
			this.m_txbWallThickness.DecimalPlaces = 1;
			this.m_txbWallThickness.Minimum = new decimal(new int[] {
			0,
			0,
			0,
			0});
			// 
			// m_lblWallThicknessUnit
			// 
			this.m_lblWallThicknessUnit.Location = new System.Drawing.Point(242, 640);
			this.m_lblWallThicknessUnit.Name = "m_lblWallThicknessUnit";
			this.m_lblWallThicknessUnit.Size = new System.Drawing.Size(34, 25);
			this.m_lblWallThicknessUnit.TabIndex = 33;
			this.m_lblWallThicknessUnit.Text = "mm";
			this.m_lblWallThicknessUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_lblBaseThickness
			// 
			this.m_lblBaseThickness.Location = new System.Drawing.Point(11, 668);
			this.m_lblBaseThickness.Name = "m_lblBaseThickness";
			this.m_lblBaseThickness.Size = new System.Drawing.Size(160, 20);
			this.m_lblBaseThickness.TabIndex = 27;
			this.m_lblBaseThickness.Text = "拼接高度(D底部厚度)";
			// 
			// m_txbBaseThickness
			// 
			this.m_txbBaseThickness.Location = new System.Drawing.Point(175, 666);
			this.m_txbBaseThickness.Name = "m_txbBaseThickness";
			this.m_txbBaseThickness.Size = new System.Drawing.Size(62, 25);
			this.m_txbBaseThickness.TabIndex = 28;
			this.m_txbBaseThickness.Value = new decimal(new int[] {
			5,
			0,
			0,
			0});
			this.m_txbBaseThickness.DecimalPlaces = 1;
			this.m_txbBaseThickness.Minimum = new decimal(new int[] {
			0,
			0,
			0,
			0});
			// 
			// m_lblBaseThicknessUnit
			// 
			this.m_lblBaseThicknessUnit.Location = new System.Drawing.Point(242, 666);
			this.m_lblBaseThicknessUnit.Name = "m_lblBaseThicknessUnit";
			this.m_lblBaseThicknessUnit.Size = new System.Drawing.Size(34, 25);
			this.m_lblBaseThicknessUnit.TabIndex = 29;
			this.m_lblBaseThicknessUnit.Text = "mm";
			this.m_lblBaseThicknessUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_btnBack
			// 
			this.m_btnBack.Location = new System.Drawing.Point(11, 710);
			this.m_btnBack.Name = "m_btnBack";
			this.m_btnBack.Size = new System.Drawing.Size(114, 35);
			this.m_btnBack.TabIndex = 40;
			this.m_btnBack.Text = "上一步";
			this.m_btnBack.UseVisualStyleBackColor = true;
			this.m_btnBack.Click += new System.EventHandler(this.m_btnBack_Click);
			// 
			// m_btnGenerate
			// 
			this.m_btnGenerate.Location = new System.Drawing.Point(437, 710);
			this.m_btnGenerate.Name = "m_btnGenerate";
			this.m_btnGenerate.Size = new System.Drawing.Size(114, 35);
			this.m_btnGenerate.TabIndex = 41;
			this.m_btnGenerate.Text = "生成";
			this.m_btnGenerate.UseVisualStyleBackColor = true;
			this.m_btnGenerate.Click += new System.EventHandler(this.m_btnGenerate_Click);
			// 
			// m_pnlSaveDxf
			// 
			this.m_pnlSaveDxf.Controls.Add(this.m_lblSaveDxfHint);
			this.m_pnlSaveDxf.Controls.Add(this.m_txbSavePath);
			this.m_pnlSaveDxf.Controls.Add(this.m_btnBrowseSavePath);
			this.m_pnlSaveDxf.Controls.Add(this.m_btnSaveDxf);
			this.m_pnlSaveDxf.Controls.Add(this.m_btnSkipSave);
			this.m_pnlSaveDxf.Location = new System.Drawing.Point(9, 8);
			this.m_pnlSaveDxf.Name = "m_pnlSaveDxf";
			this.m_pnlSaveDxf.Size = new System.Drawing.Size(306, 118);
			this.m_pnlSaveDxf.TabIndex = 2;
			// 
			// m_lblSaveDxfHint
			// 
			this.m_lblSaveDxfHint.Location = new System.Drawing.Point(0, 0);
			this.m_lblSaveDxfHint.Name = "m_lblSaveDxfHint";
			this.m_lblSaveDxfHint.Size = new System.Drawing.Size(306, 30);
			this.m_lblSaveDxfHint.TabIndex = 0;
			this.m_lblSaveDxfHint.Text = "是否要將支架輸出為 DXF 檔案？";
			// 
			// m_txbSavePath
			// 
			this.m_txbSavePath.Location = new System.Drawing.Point(0, 48);
			this.m_txbSavePath.Name = "m_txbSavePath";
			this.m_txbSavePath.ReadOnly = true;
			this.m_txbSavePath.Size = new System.Drawing.Size(214, 25);
			this.m_txbSavePath.TabIndex = 1;
			// 
			// m_btnBrowseSavePath
			// 
			this.m_btnBrowseSavePath.Location = new System.Drawing.Point(221, 47);
			this.m_btnBrowseSavePath.Name = "m_btnBrowseSavePath";
			this.m_btnBrowseSavePath.Size = new System.Drawing.Size(86, 25);
			this.m_btnBrowseSavePath.TabIndex = 2;
			this.m_btnBrowseSavePath.Text = "瀏覽...";
			this.m_btnBrowseSavePath.UseVisualStyleBackColor = true;
			this.m_btnBrowseSavePath.Click += new System.EventHandler(this.m_btnBrowseSavePath_Click);
			// 
			// m_btnSaveDxf
			// 
			this.m_btnSaveDxf.Location = new System.Drawing.Point(203, 80);
			this.m_btnSaveDxf.Name = "m_btnSaveDxf";
			this.m_btnSaveDxf.Size = new System.Drawing.Size(103, 33);
			this.m_btnSaveDxf.TabIndex = 3;
			this.m_btnSaveDxf.Text = "儲存";
			this.m_btnSaveDxf.UseVisualStyleBackColor = true;
			this.m_btnSaveDxf.Click += new System.EventHandler(this.m_btnSaveDxf_Click);
			// 
			// m_btnSkipSave
			// 
			this.m_btnSkipSave.Location = new System.Drawing.Point(0, 80);
			this.m_btnSkipSave.Name = "m_btnSkipSave";
			this.m_btnSkipSave.Size = new System.Drawing.Size(103, 33);
			this.m_btnSkipSave.TabIndex = 4;
			this.m_btnSkipSave.Text = "略過";
			this.m_btnSkipSave.UseVisualStyleBackColor = true;
			this.m_btnSkipSave.Click += new System.EventHandler(this.m_btnSkipSave_Click);
			// 
			// DesignSupportBracketDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(325, 64);
			this.Controls.Add(this.m_pnlSelectFace);
			this.Controls.Add(this.m_pnlInputParameters);
			this.Controls.Add(this.m_pnlSaveDxf);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DesignSupportBracketDlg";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "設計夾具";
			this.m_pnlSelectFace.ResumeLayout(false);
			this.m_pnlSelectFace.PerformLayout();
			this.m_pnlInputParameters.ResumeLayout(false);
			this.m_pnlInputParameters.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbXExtension)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbYExtension)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picCrossGap)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picPinDimension)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbZExtension)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbRibHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbEngagementDepth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbXPinCount)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbYPinCount)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbPinLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbWallThickness)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbBaseThickness)).EndInit();
			this.m_pnlSaveDxf.ResumeLayout(false);
			this.m_pnlSaveDxf.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Panel m_pnlSelectFace;
		private System.Windows.Forms.Label m_lblHint;
		private System.Windows.Forms.Button m_btnNext;
		private System.Windows.Forms.Panel m_pnlInputParameters;
		private System.Windows.Forms.Label m_lblSectionBase;
		private System.Windows.Forms.Label m_lblXExtension;
		private MyCAM.Editor.DebouncedNumericUpDown m_txbXExtension;
		private System.Windows.Forms.Label m_lblXExtensionUnit;
		private System.Windows.Forms.Label m_lblYExtension;
		private MyCAM.Editor.DebouncedNumericUpDown m_txbYExtension;
		private System.Windows.Forms.Label m_lblYExtensionUnit;
		private System.Windows.Forms.Label m_lblSectionBracket;
		private System.Windows.Forms.Label m_lblXPositionsTitle;
		private System.Windows.Forms.Label m_lblYPositionsTitle;
		private System.Windows.Forms.Panel m_pnlXPositions;
		private System.Windows.Forms.Panel m_pnlYPositions;
		private System.Windows.Forms.Button m_btnAddXPosition;
		private System.Windows.Forms.Button m_btnAddYPosition;
		private System.Windows.Forms.Label m_lblSectionBracketHeight;
		private System.Windows.Forms.PictureBox m_picCrossGap;
		private System.Windows.Forms.PictureBox m_picPinDimension;
		private System.Windows.Forms.Label m_lblZExtension;
		private MyCAM.Editor.DebouncedNumericUpDown m_txbZExtension;
		private System.Windows.Forms.Label m_lblZExtensionUnit;
		private System.Windows.Forms.Label m_lblRibHeight;
		private MyCAM.Editor.DebouncedNumericUpDown m_txbRibHeight;
		private System.Windows.Forms.Label m_lblRibHeightUnit;
		private System.Windows.Forms.Label m_lblSectionCrossFit;
		private System.Windows.Forms.Label m_lblEngagementDepth;
		private MyCAM.Editor.DebouncedNumericUpDown m_txbEngagementDepth;
		private System.Windows.Forms.Label m_lblEngagementDepthUnit;
		private System.Windows.Forms.Label m_lblSectionBracketCount;
		private System.Windows.Forms.Label m_lblXPinCount;
		private MyCAM.Editor.DebouncedNumericUpDown m_txbXPinCount;
		private System.Windows.Forms.Label m_lblYPinCount;
		private MyCAM.Editor.DebouncedNumericUpDown m_txbYPinCount;
		private System.Windows.Forms.Label m_lblSectionBottomPinSize;
		private System.Windows.Forms.Label m_lblPinLength;
		private MyCAM.Editor.DebouncedNumericUpDown m_txbPinLength;
		private System.Windows.Forms.Label m_lblPinLengthUnit;
		private System.Windows.Forms.Label m_lblWallThickness;
		private MyCAM.Editor.DebouncedNumericUpDown m_txbWallThickness;
		private System.Windows.Forms.Label m_lblWallThicknessUnit;
		private System.Windows.Forms.Label m_lblBaseThickness;
		private MyCAM.Editor.DebouncedNumericUpDown m_txbBaseThickness;
		private System.Windows.Forms.Label m_lblBaseThicknessUnit;
		private System.Windows.Forms.Button m_btnBack;
		private System.Windows.Forms.Button m_btnGenerate;
		private System.Windows.Forms.Panel m_pnlSaveDxf;
		private System.Windows.Forms.Label m_lblSaveDxfHint;
		private System.Windows.Forms.TextBox m_txbSavePath;
		private System.Windows.Forms.Button m_btnBrowseSavePath;
		private System.Windows.Forms.Button m_btnSaveDxf;
		private System.Windows.Forms.Button m_btnSkipSave;
	}
}
