namespace MyCAM.Editor.Dialog
{
	partial class AddPathDlg
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
			this.m_grpStep1 = new CommonUI.Controls.SectionPanel();
			this.m_rdoAutoHat = new System.Windows.Forms.RadioButton();
			this.m_rdoAutoTube = new System.Windows.Forms.RadioButton();
			this.m_rdoManual = new System.Windows.Forms.RadioButton();
			this.m_grpStep3 = new CommonUI.Controls.SectionPanel();
			this.m_rdoAutoFromFace = new System.Windows.Forms.RadioButton();
			this.m_rdoManualWire = new System.Windows.Forms.RadioButton();
			this.m_secStepHint = new CommonUI.Controls.SectionPanel();
			this.m_lblStepDescription = new CommonUI.Controls.ThemedLabel();
			this.m_lblStepHints = new CommonUI.Controls.ThemedLabel();
			this.m_secOptions = new CommonUI.Controls.SectionPanel();
			this.m_chkShowAdvanced = new System.Windows.Forms.CheckBox();
			this.m_lblError = new CommonUI.Controls.ThemedLabel();
			this.m_grpAdvanced = new CommonUI.Controls.SectionPanel();
			this.m_lblD1Threshold = new System.Windows.Forms.Label();
			this.m_txbD1Threshold = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblD1Unit = new CommonUI.Controls.ThemedLabel();
			this.m_lblDiscreteMode = new System.Windows.Forms.Label();
			this.m_cmbDiscreteMode = new System.Windows.Forms.ComboBox();
			this.m_lblMaxChordError = new System.Windows.Forms.Label();
			this.m_txbMaxChordError = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblChordUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblMaxLineLength = new System.Windows.Forms.Label();
			this.m_txbMaxLineLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblLineUnit = new CommonUI.Controls.ThemedLabel();
			this.m_btnNext = new CommonUI.Controls.ThemedButton();
			this.m_btnConfirm = new CommonUI.Controls.ThemedButton();
			this.m_grpStep1.SuspendLayout();
			this.m_grpStep3.SuspendLayout();
			this.m_secStepHint.SuspendLayout();
			this.m_secOptions.SuspendLayout();
			this.m_grpAdvanced.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbD1Threshold)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbMaxChordError)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbMaxLineLength)).BeginInit();
			this.SuspendLayout();
			//
			// m_grpStep1
			//
			this.m_grpStep1.Controls.Add(this.m_rdoAutoHat);
			this.m_grpStep1.Controls.Add(this.m_rdoAutoTube);
			this.m_grpStep1.Controls.Add(this.m_rdoManual);
			this.m_grpStep1.Location = new System.Drawing.Point(12, 12);
			this.m_grpStep1.Name = "m_grpStep1";
			this.m_grpStep1.Size = new System.Drawing.Size(356, 129);
			this.m_grpStep1.TabIndex = 0;
			this.m_grpStep1.Text = "路徑生成方式";
			//
			// m_rdoAutoHat
			//
			this.m_rdoAutoHat.AutoSize = true;
			this.m_rdoAutoHat.Checked = true;
			this.m_rdoAutoHat.Location = new System.Drawing.Point(10, 38);
			this.m_rdoAutoHat.Name = "m_rdoAutoHat";
			this.m_rdoAutoHat.Size = new System.Drawing.Size(146, 20);
			this.m_rdoAutoHat.TabIndex = 0;
			this.m_rdoAutoHat.TabStop = true;
			this.m_rdoAutoHat.Text = "自動生成路徑-異形蓋件";
			this.m_rdoAutoHat.CheckedChanged += new System.EventHandler(this.OnStep1RadioChanged);
			//
			// m_rdoAutoTube
			//
			this.m_rdoAutoTube.AutoSize = true;
			this.m_rdoAutoTube.Location = new System.Drawing.Point(10, 68);
			this.m_rdoAutoTube.Name = "m_rdoAutoTube";
			this.m_rdoAutoTube.Size = new System.Drawing.Size(134, 20);
			this.m_rdoAutoTube.TabIndex = 1;
			this.m_rdoAutoTube.Text = "自動生成路徑-管拉件";
			this.m_rdoAutoTube.CheckedChanged += new System.EventHandler(this.OnStep1RadioChanged);
			//
			// m_rdoManual
			//
			this.m_rdoManual.AutoSize = true;
			this.m_rdoManual.Location = new System.Drawing.Point(10, 98);
			this.m_rdoManual.Name = "m_rdoManual";
			this.m_rdoManual.Size = new System.Drawing.Size(97, 20);
			this.m_rdoManual.TabIndex = 2;
			this.m_rdoManual.Text = "手動生成路徑";
			this.m_rdoManual.CheckedChanged += new System.EventHandler(this.OnStep1RadioChanged);
			//
			// m_grpStep3
			//
			this.m_grpStep3.Controls.Add(this.m_rdoAutoFromFace);
			this.m_grpStep3.Controls.Add(this.m_rdoManualWire);
			this.m_grpStep3.Location = new System.Drawing.Point(12, 12);
			this.m_grpStep3.Name = "m_grpStep3";
			this.m_grpStep3.Size = new System.Drawing.Size(356, 129);
			this.m_grpStep3.TabIndex = 1;
			this.m_grpStep3.Text = "路徑生成方式";
			this.m_grpStep3.Visible = false;
			//
			// m_rdoAutoFromFace
			//
			this.m_rdoAutoFromFace.AutoSize = true;
			this.m_rdoAutoFromFace.Checked = true;
			this.m_rdoAutoFromFace.Location = new System.Drawing.Point(10, 38);
			this.m_rdoAutoFromFace.Name = "m_rdoAutoFromFace";
			this.m_rdoAutoFromFace.Size = new System.Drawing.Size(97, 20);
			this.m_rdoAutoFromFace.TabIndex = 0;
			this.m_rdoAutoFromFace.TabStop = true;
			this.m_rdoAutoFromFace.Text = "自動生成路徑";
			this.m_rdoAutoFromFace.CheckedChanged += new System.EventHandler(this.OnStep3RadioChanged);
			//
			// m_rdoManualWire
			//
			this.m_rdoManualWire.AutoSize = true;
			this.m_rdoManualWire.Location = new System.Drawing.Point(10, 68);
			this.m_rdoManualWire.Name = "m_rdoManualWire";
			this.m_rdoManualWire.Size = new System.Drawing.Size(97, 20);
			this.m_rdoManualWire.TabIndex = 1;
			this.m_rdoManualWire.Text = "手動選擇路徑";
			this.m_rdoManualWire.CheckedChanged += new System.EventHandler(this.OnStep3RadioChanged);
			//
			// m_secStepHint
			//
			this.m_secStepHint.Controls.Add(this.m_lblStepDescription);
			this.m_secStepHint.Controls.Add(this.m_lblStepHints);
			this.m_secStepHint.Location = new System.Drawing.Point(12, 12);
			this.m_secStepHint.Name = "m_secStepHint";
			this.m_secStepHint.Size = new System.Drawing.Size(356, 129);
			this.m_secStepHint.TabIndex = 2;
			this.m_secStepHint.Visible = false;
			//
			// m_lblStepDescription
			//
			this.m_lblStepDescription.Location = new System.Drawing.Point(10, 14);
			this.m_lblStepDescription.Name = "m_lblStepDescription";
			this.m_lblStepDescription.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblStepDescription.Size = new System.Drawing.Size(336, 19);
			this.m_lblStepDescription.TabIndex = 0;
			//
			// m_lblStepHints
			//
			this.m_lblStepHints.Location = new System.Drawing.Point(10, 44);
			this.m_lblStepHints.Name = "m_lblStepHints";
			this.m_lblStepHints.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblStepHints.Size = new System.Drawing.Size(336, 49);
			this.m_lblStepHints.TabIndex = 1;
			this.m_lblStepHints.Visible = false;
			//
			// m_secOptions
			//
			this.m_secOptions.Controls.Add(this.m_chkShowAdvanced);
			this.m_secOptions.Controls.Add(this.m_lblError);
			this.m_secOptions.Location = new System.Drawing.Point(12, 149);
			this.m_secOptions.Name = "m_secOptions";
			this.m_secOptions.Size = new System.Drawing.Size(356, 103);
			this.m_secOptions.TabIndex = 3;
			//
			// m_chkShowAdvanced
			//
			this.m_chkShowAdvanced.AutoSize = true;
			this.m_chkShowAdvanced.Location = new System.Drawing.Point(10, 12);
			this.m_chkShowAdvanced.Name = "m_chkShowAdvanced";
			this.m_chkShowAdvanced.Size = new System.Drawing.Size(74, 20);
			this.m_chkShowAdvanced.TabIndex = 0;
			this.m_chkShowAdvanced.Text = "進階參數";
			this.m_chkShowAdvanced.CheckedChanged += new System.EventHandler(this.m_chkShowAdvanced_CheckedChanged);
			//
			// m_lblError
			//
			this.m_lblError.Location = new System.Drawing.Point(10, 44);
			this.m_lblError.Name = "m_lblError";
			this.m_lblError.Role = CommonUI.Theme.TextRole.Error;
			this.m_lblError.Size = new System.Drawing.Size(336, 49);
			this.m_lblError.TabIndex = 1;
			this.m_lblError.Visible = false;
			//
			// m_grpAdvanced
			//
			this.m_grpAdvanced.Controls.Add(this.m_lblD1Threshold);
			this.m_grpAdvanced.Controls.Add(this.m_txbD1Threshold);
			this.m_grpAdvanced.Controls.Add(this.m_lblD1Unit);
			this.m_grpAdvanced.Controls.Add(this.m_lblDiscreteMode);
			this.m_grpAdvanced.Controls.Add(this.m_cmbDiscreteMode);
			this.m_grpAdvanced.Controls.Add(this.m_lblMaxChordError);
			this.m_grpAdvanced.Controls.Add(this.m_txbMaxChordError);
			this.m_grpAdvanced.Controls.Add(this.m_lblChordUnit);
			this.m_grpAdvanced.Controls.Add(this.m_lblMaxLineLength);
			this.m_grpAdvanced.Controls.Add(this.m_txbMaxLineLength);
			this.m_grpAdvanced.Controls.Add(this.m_lblLineUnit);
			this.m_grpAdvanced.Location = new System.Drawing.Point(12, 260);
			this.m_grpAdvanced.Name = "m_grpAdvanced";
			this.m_grpAdvanced.Size = new System.Drawing.Size(356, 159);
			this.m_grpAdvanced.TabIndex = 4;
			this.m_grpAdvanced.Text = "進階參數";
			this.m_grpAdvanced.Visible = false;
			//
			// m_lblD1Threshold
			//
			this.m_lblD1Threshold.AutoSize = true;
			this.m_lblD1Threshold.Location = new System.Drawing.Point(10, 40);
			this.m_lblD1Threshold.Name = "m_lblD1Threshold";
			this.m_lblD1Threshold.Size = new System.Drawing.Size(77, 16);
			this.m_lblD1Threshold.TabIndex = 0;
			this.m_lblD1Threshold.Text = "D1 連續閥值:";
			//
			// m_txbD1Threshold
			//
			this.m_txbD1Threshold.DecimalPlaces = 3;
			this.m_txbD1Threshold.Location = new System.Drawing.Point(130, 36);
			this.m_txbD1Threshold.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
			this.m_txbD1Threshold.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.m_txbD1Threshold.Name = "m_txbD1Threshold";
			this.m_txbD1Threshold.Size = new System.Drawing.Size(150, 23);
			this.m_txbD1Threshold.TabIndex = 1;
			this.m_txbD1Threshold.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			//
			// m_lblD1Unit
			//
			this.m_lblD1Unit.AutoSize = true;
			this.m_lblD1Unit.Location = new System.Drawing.Point(286, 40);
			this.m_lblD1Unit.Name = "m_lblD1Unit";
			this.m_lblD1Unit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblD1Unit.Size = new System.Drawing.Size(26, 16);
			this.m_lblD1Unit.TabIndex = 2;
			this.m_lblD1Unit.Text = "deg";
			//
			// m_lblDiscreteMode
			//
			this.m_lblDiscreteMode.AutoSize = true;
			this.m_lblDiscreteMode.Location = new System.Drawing.Point(10, 70);
			this.m_lblDiscreteMode.Name = "m_lblDiscreteMode";
			this.m_lblDiscreteMode.Size = new System.Drawing.Size(58, 16);
			this.m_lblDiscreteMode.TabIndex = 3;
			this.m_lblDiscreteMode.Text = "離散方式:";
			//
			// m_cmbDiscreteMode
			//
			this.m_cmbDiscreteMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbDiscreteMode.FormattingEnabled = true;
			this.m_cmbDiscreteMode.Location = new System.Drawing.Point(130, 66);
			this.m_cmbDiscreteMode.Name = "m_cmbDiscreteMode";
			this.m_cmbDiscreteMode.Size = new System.Drawing.Size(150, 24);
			this.m_cmbDiscreteMode.TabIndex = 4;
			//
			// m_lblMaxChordError
			//
			this.m_lblMaxChordError.AutoSize = true;
			this.m_lblMaxChordError.Location = new System.Drawing.Point(10, 100);
			this.m_lblMaxChordError.Name = "m_lblMaxChordError";
			this.m_lblMaxChordError.Size = new System.Drawing.Size(58, 16);
			this.m_lblMaxChordError.TabIndex = 5;
			this.m_lblMaxChordError.Text = "弦高誤差:";
			//
			// m_txbMaxChordError
			//
			this.m_txbMaxChordError.DecimalPlaces = 4;
			this.m_txbMaxChordError.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.m_txbMaxChordError.Location = new System.Drawing.Point(130, 96);
			this.m_txbMaxChordError.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_txbMaxChordError.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            262144});
			this.m_txbMaxChordError.Name = "m_txbMaxChordError";
			this.m_txbMaxChordError.Size = new System.Drawing.Size(150, 23);
			this.m_txbMaxChordError.TabIndex = 6;
			this.m_txbMaxChordError.Value = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			//
			// m_lblChordUnit
			//
			this.m_lblChordUnit.AutoSize = true;
			this.m_lblChordUnit.Location = new System.Drawing.Point(286, 100);
			this.m_lblChordUnit.Name = "m_lblChordUnit";
			this.m_lblChordUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblChordUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblChordUnit.TabIndex = 7;
			this.m_lblChordUnit.Text = "mm";
			//
			// m_lblMaxLineLength
			//
			this.m_lblMaxLineLength.AutoSize = true;
			this.m_lblMaxLineLength.Location = new System.Drawing.Point(10, 130);
			this.m_lblMaxLineLength.Name = "m_lblMaxLineLength";
			this.m_lblMaxLineLength.Size = new System.Drawing.Size(82, 16);
			this.m_lblMaxLineLength.TabIndex = 8;
			this.m_lblMaxLineLength.Text = "直線離散距離:";
			//
			// m_txbMaxLineLength
			//
			this.m_txbMaxLineLength.DecimalPlaces = 3;
			this.m_txbMaxLineLength.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.m_txbMaxLineLength.Location = new System.Drawing.Point(130, 126);
			this.m_txbMaxLineLength.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_txbMaxLineLength.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.m_txbMaxLineLength.Name = "m_txbMaxLineLength";
			this.m_txbMaxLineLength.Size = new System.Drawing.Size(150, 23);
			this.m_txbMaxLineLength.TabIndex = 9;
			this.m_txbMaxLineLength.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			//
			// m_lblLineUnit
			//
			this.m_lblLineUnit.AutoSize = true;
			this.m_lblLineUnit.Location = new System.Drawing.Point(286, 130);
			this.m_lblLineUnit.Name = "m_lblLineUnit";
			this.m_lblLineUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblLineUnit.Size = new System.Drawing.Size(26, 16);
			this.m_lblLineUnit.TabIndex = 10;
			this.m_lblLineUnit.Text = "mm";
			//
			// m_btnNext
			//
			this.m_btnNext.Location = new System.Drawing.Point(180, 435);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(80, 28);
			this.m_btnNext.TabIndex = 5;
			this.m_btnNext.Text = "下一步";
			this.m_btnNext.Click += new System.EventHandler(this.m_btnNext_Click);
			//
			// m_btnConfirm
			//
			this.m_btnConfirm.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnConfirm.Location = new System.Drawing.Point(268, 435);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(100, 28);
			this.m_btnConfirm.TabIndex = 6;
			this.m_btnConfirm.Text = "生成路徑";
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			//
			// AddPathDlg
			//
			this.ClientSize = new System.Drawing.Size(380, 475);
			this.Controls.Add(this.m_grpStep1);
			this.Controls.Add(this.m_grpStep3);
			this.Controls.Add(this.m_secStepHint);
			this.Controls.Add(this.m_secOptions);
			this.Controls.Add(this.m_grpAdvanced);
			this.Controls.Add(this.m_btnNext);
			this.Controls.Add(this.m_btnConfirm);
			this.Name = "AddPathDlg";
			this.Text = "新增路徑";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AddPathDlg_FormClosing);
			this.m_grpStep1.ResumeLayout(false);
			this.m_grpStep1.PerformLayout();
			this.m_grpStep3.ResumeLayout(false);
			this.m_grpStep3.PerformLayout();
			this.m_secStepHint.ResumeLayout(false);
			this.m_secOptions.ResumeLayout(false);
			this.m_secOptions.PerformLayout();
			this.m_grpAdvanced.ResumeLayout(false);
			this.m_grpAdvanced.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbD1Threshold)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbMaxChordError)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbMaxLineLength)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_grpStep1;
		private System.Windows.Forms.RadioButton m_rdoAutoHat;
		private System.Windows.Forms.RadioButton m_rdoAutoTube;
		private System.Windows.Forms.RadioButton m_rdoManual;
		private CommonUI.Controls.SectionPanel m_grpStep3;
		private System.Windows.Forms.RadioButton m_rdoAutoFromFace;
		private System.Windows.Forms.RadioButton m_rdoManualWire;
		private CommonUI.Controls.SectionPanel m_secStepHint;
		private CommonUI.Controls.ThemedLabel m_lblStepDescription;
		private CommonUI.Controls.ThemedLabel m_lblStepHints;
		private CommonUI.Controls.SectionPanel m_secOptions;
		private System.Windows.Forms.CheckBox m_chkShowAdvanced;
		private CommonUI.Controls.ThemedLabel m_lblError;
		private CommonUI.Controls.SectionPanel m_grpAdvanced;
		private System.Windows.Forms.Label m_lblD1Threshold;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbD1Threshold;
		private CommonUI.Controls.ThemedLabel m_lblD1Unit;
		private System.Windows.Forms.Label m_lblDiscreteMode;
		private System.Windows.Forms.ComboBox m_cmbDiscreteMode;
		private System.Windows.Forms.Label m_lblMaxChordError;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbMaxChordError;
		private CommonUI.Controls.ThemedLabel m_lblChordUnit;
		private System.Windows.Forms.Label m_lblMaxLineLength;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbMaxLineLength;
		private CommonUI.Controls.ThemedLabel m_lblLineUnit;
		private CommonUI.Controls.ThemedButton m_btnNext;
		private CommonUI.Controls.ThemedButton m_btnConfirm;
	}
}
