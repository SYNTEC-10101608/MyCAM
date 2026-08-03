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
			this.m_lblStepDescription = new System.Windows.Forms.Label();
			this.m_grpStep1 = new System.Windows.Forms.GroupBox();
			this.m_rdoAutoHat = new System.Windows.Forms.RadioButton();
			this.m_rdoAutoTube = new System.Windows.Forms.RadioButton();
			this.m_rdoManual = new System.Windows.Forms.RadioButton();
			this.m_grpStep3 = new System.Windows.Forms.GroupBox();
			this.m_rdoAutoFromFace = new System.Windows.Forms.RadioButton();
			this.m_rdoManualWire = new System.Windows.Forms.RadioButton();
			this.m_lblError = new System.Windows.Forms.Label();
			this.m_btnNext = new System.Windows.Forms.Button();
			this.m_btnConfirm = new System.Windows.Forms.Button();
			this.m_chkShowAdvanced = new System.Windows.Forms.CheckBox();
			this.m_grpAdvanced = new System.Windows.Forms.GroupBox();
			this.m_lblLineUnit = new System.Windows.Forms.Label();
			this.m_lblChordUnit = new System.Windows.Forms.Label();
			this.m_lblD1Unit = new System.Windows.Forms.Label();
			this.m_txbMaxLineLength = new System.Windows.Forms.TextBox();
			this.m_lblMaxLineLength = new System.Windows.Forms.Label();
			this.m_txbMaxChordError = new System.Windows.Forms.TextBox();
			this.m_lblMaxChordError = new System.Windows.Forms.Label();
			this.m_cmbDiscreteMode = new System.Windows.Forms.ComboBox();
			this.m_lblDiscreteMode = new System.Windows.Forms.Label();
			this.m_txbD1Threshold = new System.Windows.Forms.TextBox();
			this.m_lblD1Threshold = new System.Windows.Forms.Label();
			this.m_lblStepHints = new System.Windows.Forms.Label();
			this.m_grpStep1.SuspendLayout();
			this.m_grpStep3.SuspendLayout();
			this.m_grpAdvanced.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_lblStepDescription
			// 
			this.m_lblStepDescription.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold);
			this.m_lblStepDescription.Location = new System.Drawing.Point(20, 12);
			this.m_lblStepDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblStepDescription.Name = "m_lblStepDescription";
			this.m_lblStepDescription.Size = new System.Drawing.Size(360, 25);
			this.m_lblStepDescription.TabIndex = 0;
			// 
			// m_grpStep1
			// 
			this.m_grpStep1.Controls.Add(this.m_rdoAutoHat);
			this.m_grpStep1.Controls.Add(this.m_rdoAutoTube);
			this.m_grpStep1.Controls.Add(this.m_rdoManual);
			this.m_grpStep1.Location = new System.Drawing.Point(20, 12);
			this.m_grpStep1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_grpStep1.Name = "m_grpStep1";
			this.m_grpStep1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_grpStep1.Size = new System.Drawing.Size(360, 119);
			this.m_grpStep1.TabIndex = 1;
			this.m_grpStep1.TabStop = false;
			this.m_grpStep1.Text = "路徑生成方式";
			// 
			// m_rdoAutoHat
			// 
			this.m_rdoAutoHat.AutoSize = true;
			this.m_rdoAutoHat.Checked = true;
			this.m_rdoAutoHat.Location = new System.Drawing.Point(13, 25);
			this.m_rdoAutoHat.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_rdoAutoHat.Name = "m_rdoAutoHat";
			this.m_rdoAutoHat.Size = new System.Drawing.Size(183, 19);
			this.m_rdoAutoHat.TabIndex = 0;
			this.m_rdoAutoHat.TabStop = true;
			this.m_rdoAutoHat.Text = "自動生成路徑-異形蓋件";
			this.m_rdoAutoHat.UseVisualStyleBackColor = true;
			this.m_rdoAutoHat.CheckedChanged += new System.EventHandler(this.OnStep1RadioChanged);
			// 
			// m_rdoAutoTube
			// 
			this.m_rdoAutoTube.AutoSize = true;
			this.m_rdoAutoTube.Location = new System.Drawing.Point(13, 52);
			this.m_rdoAutoTube.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_rdoAutoTube.Name = "m_rdoAutoTube";
			this.m_rdoAutoTube.Size = new System.Drawing.Size(168, 19);
			this.m_rdoAutoTube.TabIndex = 1;
			this.m_rdoAutoTube.Text = "自動生成路徑-管拉件";
			this.m_rdoAutoTube.UseVisualStyleBackColor = true;
			this.m_rdoAutoTube.CheckedChanged += new System.EventHandler(this.OnStep1RadioChanged);
			// 
			// m_rdoManual
			// 
			this.m_rdoManual.AutoSize = true;
			this.m_rdoManual.Location = new System.Drawing.Point(13, 80);
			this.m_rdoManual.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_rdoManual.Name = "m_rdoManual";
			this.m_rdoManual.Size = new System.Drawing.Size(118, 19);
			this.m_rdoManual.TabIndex = 2;
			this.m_rdoManual.Text = "手動生成路徑";
			this.m_rdoManual.UseVisualStyleBackColor = true;
			this.m_rdoManual.CheckedChanged += new System.EventHandler(this.OnStep1RadioChanged);
			// 
			// m_grpStep3
			// 
			this.m_grpStep3.Controls.Add(this.m_rdoAutoFromFace);
			this.m_grpStep3.Controls.Add(this.m_rdoManualWire);
			this.m_grpStep3.Location = new System.Drawing.Point(20, 12);
			this.m_grpStep3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_grpStep3.Name = "m_grpStep3";
			this.m_grpStep3.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_grpStep3.Size = new System.Drawing.Size(360, 88);
			this.m_grpStep3.TabIndex = 2;
			this.m_grpStep3.TabStop = false;
			this.m_grpStep3.Text = "路徑生成方式";
			this.m_grpStep3.Visible = false;
			// 
			// m_rdoAutoFromFace
			// 
			this.m_rdoAutoFromFace.AutoSize = true;
			this.m_rdoAutoFromFace.Checked = true;
			this.m_rdoAutoFromFace.Location = new System.Drawing.Point(13, 25);
			this.m_rdoAutoFromFace.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_rdoAutoFromFace.Name = "m_rdoAutoFromFace";
			this.m_rdoAutoFromFace.Size = new System.Drawing.Size(118, 19);
			this.m_rdoAutoFromFace.TabIndex = 0;
			this.m_rdoAutoFromFace.TabStop = true;
			this.m_rdoAutoFromFace.Text = "自動生成路徑";
			this.m_rdoAutoFromFace.UseVisualStyleBackColor = true;
			this.m_rdoAutoFromFace.CheckedChanged += new System.EventHandler(this.OnStep3RadioChanged);
			// 
			// m_rdoManualWire
			// 
			this.m_rdoManualWire.AutoSize = true;
			this.m_rdoManualWire.Location = new System.Drawing.Point(13, 52);
			this.m_rdoManualWire.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_rdoManualWire.Name = "m_rdoManualWire";
			this.m_rdoManualWire.Size = new System.Drawing.Size(118, 19);
			this.m_rdoManualWire.TabIndex = 1;
			this.m_rdoManualWire.Text = "手動選擇路徑";
			this.m_rdoManualWire.UseVisualStyleBackColor = true;
			this.m_rdoManualWire.CheckedChanged += new System.EventHandler(this.OnStep3RadioChanged);
			// 
			// m_lblError
			// 
			this.m_lblError.ForeColor = System.Drawing.Color.Red;
			this.m_lblError.Location = new System.Drawing.Point(20, 142);
			this.m_lblError.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblError.Name = "m_lblError";
			this.m_lblError.Size = new System.Drawing.Size(360, 34);
			this.m_lblError.TabIndex = 3;
			this.m_lblError.Visible = false;
			// 
			// m_btnNext
			// 
			this.m_btnNext.Location = new System.Drawing.Point(160, 179);
			this.m_btnNext.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(107, 34);
			this.m_btnNext.TabIndex = 4;
			this.m_btnNext.Text = "下一步";
			this.m_btnNext.UseVisualStyleBackColor = true;
			this.m_btnNext.Click += new System.EventHandler(this.m_btnNext_Click);
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Location = new System.Drawing.Point(280, 179);
			this.m_btnConfirm.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(100, 34);
			this.m_btnConfirm.TabIndex = 5;
			this.m_btnConfirm.Text = "生成路徑";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// m_chkShowAdvanced
			// 
			this.m_chkShowAdvanced.AutoSize = true;
			this.m_chkShowAdvanced.Location = new System.Drawing.Point(26, 188);
			this.m_chkShowAdvanced.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_chkShowAdvanced.Name = "m_chkShowAdvanced";
			this.m_chkShowAdvanced.Size = new System.Drawing.Size(89, 19);
			this.m_chkShowAdvanced.TabIndex = 6;
			this.m_chkShowAdvanced.Text = "進階選項";
			this.m_chkShowAdvanced.UseVisualStyleBackColor = true;
			this.m_chkShowAdvanced.CheckedChanged += new System.EventHandler(this.m_chkShowAdvanced_CheckedChanged);
			// 
			// m_grpAdvanced
			// 
			this.m_grpAdvanced.Controls.Add(this.m_lblLineUnit);
			this.m_grpAdvanced.Controls.Add(this.m_lblChordUnit);
			this.m_grpAdvanced.Controls.Add(this.m_lblD1Unit);
			this.m_grpAdvanced.Controls.Add(this.m_txbMaxLineLength);
			this.m_grpAdvanced.Controls.Add(this.m_lblMaxLineLength);
			this.m_grpAdvanced.Controls.Add(this.m_txbMaxChordError);
			this.m_grpAdvanced.Controls.Add(this.m_lblMaxChordError);
			this.m_grpAdvanced.Controls.Add(this.m_cmbDiscreteMode);
			this.m_grpAdvanced.Controls.Add(this.m_lblDiscreteMode);
			this.m_grpAdvanced.Controls.Add(this.m_txbD1Threshold);
			this.m_grpAdvanced.Controls.Add(this.m_lblD1Threshold);
			this.m_grpAdvanced.Location = new System.Drawing.Point(20, 300);
			this.m_grpAdvanced.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_grpAdvanced.Name = "m_grpAdvanced";
			this.m_grpAdvanced.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_grpAdvanced.Size = new System.Drawing.Size(360, 181);
			this.m_grpAdvanced.TabIndex = 7;
			this.m_grpAdvanced.TabStop = false;
			this.m_grpAdvanced.Visible = false;
			// 
			// m_lblLineUnit
			// 
			this.m_lblLineUnit.AutoSize = true;
			this.m_lblLineUnit.Location = new System.Drawing.Point(307, 140);
			this.m_lblLineUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblLineUnit.Name = "m_lblLineUnit";
			this.m_lblLineUnit.Size = new System.Drawing.Size(29, 15);
			this.m_lblLineUnit.TabIndex = 10;
			this.m_lblLineUnit.Text = "mm";
			// 
			// m_lblChordUnit
			// 
			this.m_lblChordUnit.AutoSize = true;
			this.m_lblChordUnit.Location = new System.Drawing.Point(307, 102);
			this.m_lblChordUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblChordUnit.Name = "m_lblChordUnit";
			this.m_lblChordUnit.Size = new System.Drawing.Size(29, 15);
			this.m_lblChordUnit.TabIndex = 7;
			this.m_lblChordUnit.Text = "mm";
			// 
			// m_lblD1Unit
			// 
			this.m_lblD1Unit.AutoSize = true;
			this.m_lblD1Unit.Location = new System.Drawing.Point(307, 28);
			this.m_lblD1Unit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblD1Unit.Name = "m_lblD1Unit";
			this.m_lblD1Unit.Size = new System.Drawing.Size(22, 15);
			this.m_lblD1Unit.TabIndex = 2;
			this.m_lblD1Unit.Text = "度";
			// 
			// m_txbMaxLineLength
			// 
			this.m_txbMaxLineLength.Location = new System.Drawing.Point(140, 136);
			this.m_txbMaxLineLength.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_txbMaxLineLength.Name = "m_txbMaxLineLength";
			this.m_txbMaxLineLength.Size = new System.Drawing.Size(145, 25);
			this.m_txbMaxLineLength.TabIndex = 9;
			this.m_txbMaxLineLength.Text = "1.0";
			// 
			// m_lblMaxLineLength
			// 
			this.m_lblMaxLineLength.AutoSize = true;
			this.m_lblMaxLineLength.Location = new System.Drawing.Point(13, 140);
			this.m_lblMaxLineLength.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblMaxLineLength.Name = "m_lblMaxLineLength";
			this.m_lblMaxLineLength.Size = new System.Drawing.Size(142, 15);
			this.m_lblMaxLineLength.TabIndex = 8;
			this.m_lblMaxLineLength.Text = "最長直線離散距離：";
			// 
			// m_txbMaxChordError
			// 
			this.m_txbMaxChordError.Location = new System.Drawing.Point(140, 99);
			this.m_txbMaxChordError.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_txbMaxChordError.Name = "m_txbMaxChordError";
			this.m_txbMaxChordError.Size = new System.Drawing.Size(145, 25);
			this.m_txbMaxChordError.TabIndex = 6;
			this.m_txbMaxChordError.Text = "0.01";
			// 
			// m_lblMaxChordError
			// 
			this.m_lblMaxChordError.AutoSize = true;
			this.m_lblMaxChordError.Location = new System.Drawing.Point(13, 102);
			this.m_lblMaxChordError.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblMaxChordError.Name = "m_lblMaxChordError";
			this.m_lblMaxChordError.Size = new System.Drawing.Size(112, 15);
			this.m_lblMaxChordError.TabIndex = 5;
			this.m_lblMaxChordError.Text = "最大弦高誤差：";
			// 
			// m_cmbDiscreteMode
			// 
			this.m_cmbDiscreteMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbDiscreteMode.FormattingEnabled = true;
			this.m_cmbDiscreteMode.Location = new System.Drawing.Point(140, 61);
			this.m_cmbDiscreteMode.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_cmbDiscreteMode.Name = "m_cmbDiscreteMode";
			this.m_cmbDiscreteMode.Size = new System.Drawing.Size(145, 23);
			this.m_cmbDiscreteMode.TabIndex = 4;
			// 
			// m_lblDiscreteMode
			// 
			this.m_lblDiscreteMode.AutoSize = true;
			this.m_lblDiscreteMode.Location = new System.Drawing.Point(13, 65);
			this.m_lblDiscreteMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblDiscreteMode.Name = "m_lblDiscreteMode";
			this.m_lblDiscreteMode.Size = new System.Drawing.Size(82, 15);
			this.m_lblDiscreteMode.TabIndex = 3;
			this.m_lblDiscreteMode.Text = "離散方式：";
			// 
			// m_txbD1Threshold
			// 
			this.m_txbD1Threshold.Location = new System.Drawing.Point(140, 24);
			this.m_txbD1Threshold.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_txbD1Threshold.Name = "m_txbD1Threshold";
			this.m_txbD1Threshold.Size = new System.Drawing.Size(145, 25);
			this.m_txbD1Threshold.TabIndex = 1;
			this.m_txbD1Threshold.Text = "5";
			// 
			// m_lblD1Threshold
			// 
			this.m_lblD1Threshold.AutoSize = true;
			this.m_lblD1Threshold.Location = new System.Drawing.Point(13, 28);
			this.m_lblD1Threshold.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblD1Threshold.Name = "m_lblD1Threshold";
			this.m_lblD1Threshold.Size = new System.Drawing.Size(103, 15);
			this.m_lblD1Threshold.TabIndex = 0;
			this.m_lblD1Threshold.Text = "D1 連續閥值：";
			// 
			// m_lblStepHints
			// 
			this.m_lblStepHints.ForeColor = System.Drawing.Color.Blue;
			this.m_lblStepHints.Location = new System.Drawing.Point(20, 40);
			this.m_lblStepHints.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblStepHints.Name = "m_lblStepHints";
			this.m_lblStepHints.Size = new System.Drawing.Size(360, 44);
			this.m_lblStepHints.TabIndex = 10;
			this.m_lblStepHints.Visible = false;
			// 
			// AddPathDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(400, 237);
			this.Controls.Add(this.m_grpStep1);
			this.Controls.Add(this.m_grpStep3);
			this.Controls.Add(this.m_lblStepDescription);
			this.Controls.Add(this.m_lblStepHints);
			this.Controls.Add(this.m_lblError);
			this.Controls.Add(this.m_btnNext);
			this.Controls.Add(this.m_btnConfirm);
			this.Controls.Add(this.m_chkShowAdvanced);
			this.Controls.Add(this.m_grpAdvanced);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AddPathDlg";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "新增路徑";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AddPathDlg_FormClosing);
			this.m_grpStep1.ResumeLayout(false);
			this.m_grpStep1.PerformLayout();
			this.m_grpStep3.ResumeLayout(false);
			this.m_grpStep3.PerformLayout();
			this.m_grpAdvanced.ResumeLayout(false);
			this.m_grpAdvanced.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label m_lblStepDescription;
        private System.Windows.Forms.GroupBox m_grpStep1;
        private System.Windows.Forms.RadioButton m_rdoAutoHat;
        private System.Windows.Forms.RadioButton m_rdoAutoTube;
        private System.Windows.Forms.RadioButton m_rdoManual;
        private System.Windows.Forms.GroupBox m_grpStep3;
        private System.Windows.Forms.RadioButton m_rdoAutoFromFace;
        private System.Windows.Forms.RadioButton m_rdoManualWire;
        private System.Windows.Forms.Label m_lblError;
        private System.Windows.Forms.Button m_btnNext;
        private System.Windows.Forms.Button m_btnConfirm;
        private System.Windows.Forms.CheckBox m_chkShowAdvanced;
        private System.Windows.Forms.GroupBox m_grpAdvanced;
        private System.Windows.Forms.TextBox m_txbMaxLineLength;
        private System.Windows.Forms.Label m_lblMaxLineLength;
        private System.Windows.Forms.TextBox m_txbMaxChordError;
        private System.Windows.Forms.Label m_lblMaxChordError;
        private System.Windows.Forms.ComboBox m_cmbDiscreteMode;
        private System.Windows.Forms.Label m_lblDiscreteMode;
        private System.Windows.Forms.TextBox m_txbD1Threshold;
        private System.Windows.Forms.Label m_lblD1Threshold;
        private System.Windows.Forms.Label m_lblD1Unit;
        private System.Windows.Forms.Label m_lblChordUnit;
        private System.Windows.Forms.Label m_lblLineUnit;
		private System.Windows.Forms.Label m_lblStepHints;
	}
}
