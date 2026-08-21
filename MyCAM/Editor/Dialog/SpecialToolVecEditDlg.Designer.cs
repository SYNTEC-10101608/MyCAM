namespace MyCAM.Editor
{
    partial class SpecialToolVecEditDlg
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose( bool disposing )
        {
            if( disposing && ( components != null ) ) {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
			this.m_rbtnZ = new System.Windows.Forms.RadioButton();
			this.m_rbtnFixDir = new System.Windows.Forms.RadioButton();
			this.m_gbxType = new System.Windows.Forms.GroupBox();
			this.m_rbtNormal = new System.Windows.Forms.RadioButton();
			this.m_rbtnAssignMS = new System.Windows.Forms.RadioButton();
			this.m_gbxMSDeg = new System.Windows.Forms.GroupBox();
			this.m_gbxSwitchDir = new System.Windows.Forms.GroupBox();
			this.m_btnZ = new System.Windows.Forms.Button();
			this.m_btnFixDir = new System.Windows.Forms.Button();
			this.m_gbxCRotate = new System.Windows.Forms.GroupBox();
			this.m_btnRotaryNeg = new System.Windows.Forms.Button();
			this.m_btnRotaryPos = new System.Windows.Forms.Button();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.m_dbtnSlave = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_dbtnMaster = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblSDeg = new System.Windows.Forms.Label();
			this.m_lblMDeg = new System.Windows.Forms.Label();
			this.m_btnConfirm = new System.Windows.Forms.Button();
			this.m_gbxType.SuspendLayout();
			this.m_gbxMSDeg.SuspendLayout();
			this.m_gbxSwitchDir.SuspendLayout();
			this.m_gbxCRotate.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dbtnSlave)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_dbtnMaster)).BeginInit();
			this.SuspendLayout();
			// 
			// m_rbtnZ
			// 
			this.m_rbtnZ.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_rbtnZ.AutoSize = true;
			this.m_rbtnZ.Location = new System.Drawing.Point(168, 21);
			this.m_rbtnZ.Name = "m_rbtnZ";
			this.m_rbtnZ.Size = new System.Drawing.Size(42, 16);
			this.m_rbtnZ.TabIndex = 2;
			this.m_rbtnZ.Text = "朝Z";
			this.m_rbtnZ.UseVisualStyleBackColor = true;
			this.m_rbtnZ.CheckedChanged += new System.EventHandler(this.m_rbtnZ_CheckedChanged);
			// 
			// m_rbtnFixDir
			// 
			this.m_rbtnFixDir.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_rbtnFixDir.AutoSize = true;
			this.m_rbtnFixDir.Location = new System.Drawing.Point(57, 21);
			this.m_rbtnFixDir.Name = "m_rbtnFixDir";
			this.m_rbtnFixDir.Size = new System.Drawing.Size(95, 16);
			this.m_rbtnFixDir.TabIndex = 1;
			this.m_rbtnFixDir.Text = "垂直曲面切割";
			this.m_rbtnFixDir.UseVisualStyleBackColor = true;
			this.m_rbtnFixDir.CheckedChanged += new System.EventHandler(this.m_rbtnFixDir_CheckedChanged);
			// 
			// m_gbxType
			// 
			this.m_gbxType.Controls.Add(this.m_rbtNormal);
			this.m_gbxType.Controls.Add(this.m_rbtnAssignMS);
			this.m_gbxType.Controls.Add(this.m_rbtnFixDir);
			this.m_gbxType.Controls.Add(this.m_rbtnZ);
			this.m_gbxType.Location = new System.Drawing.Point(10, 10);
			this.m_gbxType.Name = "m_gbxType";
			this.m_gbxType.Size = new System.Drawing.Size(307, 47);
			this.m_gbxType.TabIndex = 3;
			this.m_gbxType.TabStop = false;
			this.m_gbxType.Text = "孔姿態類型";
			// 
			// m_rbtNormal
			// 
			this.m_rbtNormal.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_rbtNormal.AutoSize = true;
			this.m_rbtNormal.Checked = true;
			this.m_rbtNormal.Location = new System.Drawing.Point(6, 21);
			this.m_rbtNormal.Name = "m_rbtNormal";
			this.m_rbtNormal.Size = new System.Drawing.Size(47, 16);
			this.m_rbtNormal.TabIndex = 4;
			this.m_rbtNormal.TabStop = true;
			this.m_rbtNormal.Text = "默認";
			this.m_rbtNormal.UseVisualStyleBackColor = true;
			this.m_rbtNormal.CheckedChanged += new System.EventHandler(this.m_rbtNormal_CheckedChanged);
			// 
			// m_rbtnAssignMS
			// 
			this.m_rbtnAssignMS.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_rbtnAssignMS.AutoSize = true;
			this.m_rbtnAssignMS.Location = new System.Drawing.Point(226, 21);
			this.m_rbtnAssignMS.Name = "m_rbtnAssignMS";
			this.m_rbtnAssignMS.Size = new System.Drawing.Size(71, 16);
			this.m_rbtnAssignMS.TabIndex = 3;
			this.m_rbtnAssignMS.Text = "鎖軸切割";
			this.m_rbtnAssignMS.UseVisualStyleBackColor = true;
			this.m_rbtnAssignMS.CheckedChanged += new System.EventHandler(this.m_rbtnAssignMS_CheckedChanged);
			// 
			// m_gbxMSDeg
			// 
			this.m_gbxMSDeg.Controls.Add(this.m_gbxSwitchDir);
			this.m_gbxMSDeg.Controls.Add(this.m_gbxCRotate);
			this.m_gbxMSDeg.Controls.Add(this.tableLayoutPanel2);
			this.m_gbxMSDeg.Location = new System.Drawing.Point(10, 63);
			this.m_gbxMSDeg.Name = "m_gbxMSDeg";
			this.m_gbxMSDeg.Size = new System.Drawing.Size(216, 196);
			this.m_gbxMSDeg.TabIndex = 5;
			this.m_gbxMSDeg.TabStop = false;
			this.m_gbxMSDeg.Text = "起點旋轉軸角度";
			// 
			// m_gbxSwitchDir
			// 
			this.m_gbxSwitchDir.Controls.Add(this.m_btnZ);
			this.m_gbxSwitchDir.Controls.Add(this.m_btnFixDir);
			this.m_gbxSwitchDir.Location = new System.Drawing.Point(17, 138);
			this.m_gbxSwitchDir.Name = "m_gbxSwitchDir";
			this.m_gbxSwitchDir.Size = new System.Drawing.Size(182, 46);
			this.m_gbxSwitchDir.TabIndex = 12;
			this.m_gbxSwitchDir.TabStop = false;
			this.m_gbxSwitchDir.Text = "轉至默認角度";
			// 
			// m_btnZ
			// 
			this.m_btnZ.Location = new System.Drawing.Point(7, 16);
			this.m_btnZ.Name = "m_btnZ";
			this.m_btnZ.Size = new System.Drawing.Size(63, 26);
			this.m_btnZ.TabIndex = 8;
			this.m_btnZ.Text = "轉至朝Z";
			this.m_btnZ.UseVisualStyleBackColor = true;
			this.m_btnZ.Click += new System.EventHandler(this.m_btnZ_Click);
			// 
			// m_btnFixDir
			// 
			this.m_btnFixDir.Location = new System.Drawing.Point(76, 16);
			this.m_btnFixDir.Name = "m_btnFixDir";
			this.m_btnFixDir.Size = new System.Drawing.Size(93, 26);
			this.m_btnFixDir.TabIndex = 9;
			this.m_btnFixDir.Text = "轉至垂直曲面";
			this.m_btnFixDir.UseVisualStyleBackColor = true;
			this.m_btnFixDir.Click += new System.EventHandler(this.m_btnFixDir_Click);
			// 
			// m_gbxCRotate
			// 
			this.m_gbxCRotate.Controls.Add(this.m_btnRotaryNeg);
			this.m_gbxCRotate.Controls.Add(this.m_btnRotaryPos);
			this.m_gbxCRotate.Location = new System.Drawing.Point(17, 86);
			this.m_gbxCRotate.Name = "m_gbxCRotate";
			this.m_gbxCRotate.Size = new System.Drawing.Size(169, 46);
			this.m_gbxCRotate.TabIndex = 10;
			this.m_gbxCRotate.TabStop = false;
			this.m_gbxCRotate.Text = "主軸方向調整";
			// 
			// m_btnRotaryNeg
			// 
			this.m_btnRotaryNeg.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_btnRotaryNeg.Location = new System.Drawing.Point(64, 14);
			this.m_btnRotaryNeg.Name = "m_btnRotaryNeg";
			this.m_btnRotaryNeg.Size = new System.Drawing.Size(51, 26);
			this.m_btnRotaryNeg.TabIndex = 11;
			this.m_btnRotaryNeg.Text = "C-";
			this.m_btnRotaryNeg.UseVisualStyleBackColor = true;
			this.m_btnRotaryNeg.Click += new System.EventHandler(this.m_btnRotaryNeg_Click);
			// 
			// m_btnRotaryPos
			// 
			this.m_btnRotaryPos.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_btnRotaryPos.Location = new System.Drawing.Point(7, 15);
			this.m_btnRotaryPos.Name = "m_btnRotaryPos";
			this.m_btnRotaryPos.Size = new System.Drawing.Size(51, 25);
			this.m_btnRotaryPos.TabIndex = 10;
			this.m_btnRotaryPos.Text = "C+";
			this.m_btnRotaryPos.UseVisualStyleBackColor = true;
			this.m_btnRotaryPos.Click += new System.EventHandler(this.m_btnRotaryPos_Click);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 48.9011F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 51.0989F));
			this.tableLayoutPanel2.Controls.Add(this.m_dbtnSlave, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.m_dbtnMaster, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.m_lblSDeg, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.m_lblMDeg, 0, 0);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(17, 14);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 66.66666F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(182, 66);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// m_dbtnSlave
			// 
			this.m_dbtnSlave.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_dbtnSlave.DecimalPlaces = 3;
			this.m_dbtnSlave.Location = new System.Drawing.Point(92, 33);
			this.m_dbtnSlave.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_dbtnSlave.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_dbtnSlave.Name = "m_dbtnSlave";
			this.m_dbtnSlave.RestrictInvalidCharacters = false;
			this.m_dbtnSlave.Size = new System.Drawing.Size(55, 22);
			this.m_dbtnSlave.TabIndex = 9;
			this.m_dbtnSlave.DebouncedValueChanged += new System.EventHandler(this.m_dbtnSlave_DebouncedValueChanged);
			// 
			// m_dbtnMaster
			// 
			this.m_dbtnMaster.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_dbtnMaster.DecimalPlaces = 3;
			this.m_dbtnMaster.Location = new System.Drawing.Point(3, 33);
			this.m_dbtnMaster.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_dbtnMaster.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_dbtnMaster.Name = "m_dbtnMaster";
			this.m_dbtnMaster.RestrictInvalidCharacters = false;
			this.m_dbtnMaster.Size = new System.Drawing.Size(55, 22);
			this.m_dbtnMaster.TabIndex = 8;
			this.m_dbtnMaster.DebouncedValueChanged += new System.EventHandler(this.m_dbtnMaster_DebouncedValueChanged);
			// 
			// m_lblSDeg
			// 
			this.m_lblSDeg.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblSDeg.AutoSize = true;
			this.m_lblSDeg.Location = new System.Drawing.Point(92, 5);
			this.m_lblSDeg.Name = "m_lblSDeg";
			this.m_lblSDeg.Size = new System.Drawing.Size(65, 12);
			this.m_lblSDeg.TabIndex = 6;
			this.m_lblSDeg.Text = "從動軸角度";
			// 
			// m_lblMDeg
			// 
			this.m_lblMDeg.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblMDeg.AutoSize = true;
			this.m_lblMDeg.Location = new System.Drawing.Point(3, 5);
			this.m_lblMDeg.Name = "m_lblMDeg";
			this.m_lblMDeg.Size = new System.Drawing.Size(53, 12);
			this.m_lblMDeg.TabIndex = 5;
			this.m_lblMDeg.Text = "主軸角度";
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Location = new System.Drawing.Point(244, 233);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(63, 26);
			this.m_btnConfirm.TabIndex = 7;
			this.m_btnConfirm.Text = "完成";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// SpecialToolVecEditDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(323, 267);
			this.Controls.Add(this.m_btnConfirm);
			this.Controls.Add(this.m_gbxMSDeg);
			this.Controls.Add(this.m_gbxType);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "SpecialToolVecEditDlg";
			this.Padding = new System.Windows.Forms.Padding(7);
			this.Text = "孔姿態";
			this.m_gbxType.ResumeLayout(false);
			this.m_gbxType.PerformLayout();
			this.m_gbxMSDeg.ResumeLayout(false);
			this.m_gbxSwitchDir.ResumeLayout(false);
			this.m_gbxCRotate.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dbtnSlave)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_dbtnMaster)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion
		private System.Windows.Forms.RadioButton m_rbtnZ;
		private System.Windows.Forms.RadioButton m_rbtnFixDir;
		private System.Windows.Forms.GroupBox m_gbxType;
		private System.Windows.Forms.GroupBox m_gbxMSDeg;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Label m_lblSDeg;
		private System.Windows.Forms.Label m_lblMDeg;
		private System.Windows.Forms.RadioButton m_rbtnAssignMS;
		private System.Windows.Forms.RadioButton m_rbtNormal;
		private DebouncedNumericUpDown m_dbtnSlave;
		private DebouncedNumericUpDown m_dbtnMaster;
		private System.Windows.Forms.Button m_btnRotaryNeg;
		private System.Windows.Forms.Button m_btnRotaryPos;
		private System.Windows.Forms.Button m_btnConfirm;
		private System.Windows.Forms.Button m_btnFixDir;
		private System.Windows.Forms.Button m_btnZ;
		private System.Windows.Forms.GroupBox m_gbxSwitchDir;
		private System.Windows.Forms.GroupBox m_gbxCRotate;
	}
}
