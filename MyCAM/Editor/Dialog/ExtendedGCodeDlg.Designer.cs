namespace MyCAM.Editor
{
    partial class ExtendedGCodeDlg
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
			this.m_btnDone = new System.Windows.Forms.Button();
			this.m_pnlExtendedGCode = new System.Windows.Forms.TableLayoutPanel();
			this.m_txtGCode = new System.Windows.Forms.TextBox();
			this.m_flpnlGCode = new System.Windows.Forms.FlowLayoutPanel();
			this.m_lblGCode = new System.Windows.Forms.Label();
			this.m_lblIsMixData = new System.Windows.Forms.Label();
			this.m_lblInsertType = new System.Windows.Forms.Label();
			this.m_flpnlRadioButton = new System.Windows.Forms.FlowLayoutPanel();
			this.m_rbtnTraverseStart = new System.Windows.Forms.RadioButton();
			this.m_rbtnTraverseEnd = new System.Windows.Forms.RadioButton();
			this.m_rbtnPathStart = new System.Windows.Forms.RadioButton();
			this.m_rbtnPathEnd = new System.Windows.Forms.RadioButton();
			this.m_rbtnControlPoint = new System.Windows.Forms.RadioButton();
			this.m_flpnlButton = new System.Windows.Forms.FlowLayoutPanel();
			this.m_btnDelete = new System.Windows.Forms.Button();
			this.m_btnAdd = new System.Windows.Forms.Button();
			this.m_flpnlToStartOrEnd = new System.Windows.Forms.FlowLayoutPanel();
			this.m_lblToStartOrEnd = new System.Windows.Forms.Label();
			this.m_btnToStartOrEnd = new System.Windows.Forms.Button();
			this.m_lblIsStartOrEnd = new System.Windows.Forms.Label();
			this.m_pnlExtendedGCode.SuspendLayout();
			this.m_flpnlGCode.SuspendLayout();
			this.m_flpnlRadioButton.SuspendLayout();
			this.m_flpnlButton.SuspendLayout();
			this.m_flpnlToStartOrEnd.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_btnDone
			// 
			this.m_btnDone.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnDone.Location = new System.Drawing.Point(495, 4);
			this.m_btnDone.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnDone.Name = "m_btnDone";
			this.m_btnDone.Size = new System.Drawing.Size(84, 33);
			this.m_btnDone.TabIndex = 2;
			this.m_btnDone.Text = "完成";
			this.m_btnDone.UseVisualStyleBackColor = true;
			this.m_btnDone.Click += new System.EventHandler(this.m_btnDone_Click);
			// 
			// m_pnlExtendedGCode
			// 
			this.m_pnlExtendedGCode.ColumnCount = 1;
			this.m_pnlExtendedGCode.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 589F));
			this.m_pnlExtendedGCode.Controls.Add(this.m_txtGCode, 0, 4);
			this.m_pnlExtendedGCode.Controls.Add(this.m_flpnlGCode, 0, 3);
			this.m_pnlExtendedGCode.Controls.Add(this.m_lblInsertType, 0, 0);
			this.m_pnlExtendedGCode.Controls.Add(this.m_flpnlRadioButton, 0, 1);
			this.m_pnlExtendedGCode.Controls.Add(this.m_flpnlButton, 0, 5);
			this.m_pnlExtendedGCode.Controls.Add(this.m_flpnlToStartOrEnd, 0, 2);
			this.m_pnlExtendedGCode.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_pnlExtendedGCode.Location = new System.Drawing.Point(9, 9);
			this.m_pnlExtendedGCode.Name = "m_pnlExtendedGCode";
			this.m_pnlExtendedGCode.RowCount = 6;
			this.m_pnlExtendedGCode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.m_pnlExtendedGCode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.m_pnlExtendedGCode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
			this.m_pnlExtendedGCode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.m_pnlExtendedGCode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_pnlExtendedGCode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
			this.m_pnlExtendedGCode.Size = new System.Drawing.Size(589, 371);
			this.m_pnlExtendedGCode.TabIndex = 10;
			// 
			// m_txtGCode
			// 
			this.m_txtGCode.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_txtGCode.Location = new System.Drawing.Point(3, 163);
			this.m_txtGCode.Multiline = true;
			this.m_txtGCode.Name = "m_txtGCode";
			this.m_txtGCode.Size = new System.Drawing.Size(583, 150);
			this.m_txtGCode.TabIndex = 11;
			this.m_txtGCode.TextChanged += new System.EventHandler(this.m_txtGCode_TextChanged);
			this.m_txtGCode.Leave += new System.EventHandler(this.m_txtGCode_Leave);
			// 
			// m_flpnlGCode
			// 
			this.m_flpnlGCode.Controls.Add(this.m_lblGCode);
			this.m_flpnlGCode.Controls.Add(this.m_lblIsMixData);
			this.m_flpnlGCode.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_flpnlGCode.Location = new System.Drawing.Point(3, 128);
			this.m_flpnlGCode.Name = "m_flpnlGCode";
			this.m_flpnlGCode.Size = new System.Drawing.Size(583, 29);
			this.m_flpnlGCode.TabIndex = 11;
			// 
			// m_lblGCode
			// 
			this.m_lblGCode.AutoSize = true;
			this.m_lblGCode.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_lblGCode.Location = new System.Drawing.Point(3, 0);
			this.m_lblGCode.Name = "m_lblGCode";
			this.m_lblGCode.Size = new System.Drawing.Size(32, 15);
			this.m_lblGCode.TabIndex = 5;
			this.m_lblGCode.Text = "G碼";
			this.m_lblGCode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_lblIsMixData
			// 
			this.m_lblIsMixData.AutoSize = true;
			this.m_lblIsMixData.ForeColor = System.Drawing.Color.Red;
			this.m_lblIsMixData.Location = new System.Drawing.Point(41, 0);
			this.m_lblIsMixData.Name = "m_lblIsMixData";
			this.m_lblIsMixData.Size = new System.Drawing.Size(152, 15);
			this.m_lblIsMixData.TabIndex = 6;
			this.m_lblIsMixData.Text = "所選路徑的G碼不一致";
			// 
			// m_lblInsertType
			// 
			this.m_lblInsertType.AutoSize = true;
			this.m_lblInsertType.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_lblInsertType.Location = new System.Drawing.Point(3, 0);
			this.m_lblInsertType.Name = "m_lblInsertType";
			this.m_lblInsertType.Size = new System.Drawing.Size(583, 35);
			this.m_lblInsertType.TabIndex = 3;
			this.m_lblInsertType.Text = "插入位置";
			this.m_lblInsertType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_flpnlRadioButton
			// 
			this.m_flpnlRadioButton.Controls.Add(this.m_rbtnTraverseStart);
			this.m_flpnlRadioButton.Controls.Add(this.m_rbtnTraverseEnd);
			this.m_flpnlRadioButton.Controls.Add(this.m_rbtnPathStart);
			this.m_flpnlRadioButton.Controls.Add(this.m_rbtnPathEnd);
			this.m_flpnlRadioButton.Controls.Add(this.m_rbtnControlPoint);
			this.m_flpnlRadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_flpnlRadioButton.Location = new System.Drawing.Point(3, 38);
			this.m_flpnlRadioButton.Name = "m_flpnlRadioButton";
			this.m_flpnlRadioButton.Size = new System.Drawing.Size(583, 29);
			this.m_flpnlRadioButton.TabIndex = 4;
			// 
			// m_rbtnTraverseStart
			// 
			this.m_rbtnTraverseStart.AutoSize = true;
			this.m_rbtnTraverseStart.Location = new System.Drawing.Point(3, 3);
			this.m_rbtnTraverseStart.Name = "m_rbtnTraverseStart";
			this.m_rbtnTraverseStart.Size = new System.Drawing.Size(88, 19);
			this.m_rbtnTraverseStart.TabIndex = 5;
			this.m_rbtnTraverseStart.TabStop = true;
			this.m_rbtnTraverseStart.Text = "空移開始";
			this.m_rbtnTraverseStart.UseVisualStyleBackColor = true;
			this.m_rbtnTraverseStart.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedhanged);
			// 
			// m_rbtnTraverseEnd
			// 
			this.m_rbtnTraverseEnd.AutoSize = true;
			this.m_rbtnTraverseEnd.Location = new System.Drawing.Point(97, 3);
			this.m_rbtnTraverseEnd.Name = "m_rbtnTraverseEnd";
			this.m_rbtnTraverseEnd.Size = new System.Drawing.Size(88, 19);
			this.m_rbtnTraverseEnd.TabIndex = 6;
			this.m_rbtnTraverseEnd.TabStop = true;
			this.m_rbtnTraverseEnd.Text = "空移結束";
			this.m_rbtnTraverseEnd.UseVisualStyleBackColor = true;
			this.m_rbtnTraverseEnd.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedhanged);
			// 
			// m_rbtnPathStart
			// 
			this.m_rbtnPathStart.AutoSize = true;
			this.m_rbtnPathStart.Location = new System.Drawing.Point(191, 3);
			this.m_rbtnPathStart.Name = "m_rbtnPathStart";
			this.m_rbtnPathStart.Size = new System.Drawing.Size(88, 19);
			this.m_rbtnPathStart.TabIndex = 7;
			this.m_rbtnPathStart.TabStop = true;
			this.m_rbtnPathStart.Text = "路徑開始";
			this.m_rbtnPathStart.UseVisualStyleBackColor = true;
			this.m_rbtnPathStart.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedhanged);
			// 
			// m_rbtnPathEnd
			// 
			this.m_rbtnPathEnd.AutoSize = true;
			this.m_rbtnPathEnd.Location = new System.Drawing.Point(285, 3);
			this.m_rbtnPathEnd.Name = "m_rbtnPathEnd";
			this.m_rbtnPathEnd.Size = new System.Drawing.Size(88, 19);
			this.m_rbtnPathEnd.TabIndex = 8;
			this.m_rbtnPathEnd.TabStop = true;
			this.m_rbtnPathEnd.Text = "路徑結束";
			this.m_rbtnPathEnd.UseVisualStyleBackColor = true;
			this.m_rbtnPathEnd.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedhanged);
			// 
			// m_rbtnControlPoint
			// 
			this.m_rbtnControlPoint.AutoSize = true;
			this.m_rbtnControlPoint.Location = new System.Drawing.Point(379, 3);
			this.m_rbtnControlPoint.Name = "m_rbtnControlPoint";
			this.m_rbtnControlPoint.Size = new System.Drawing.Size(73, 19);
			this.m_rbtnControlPoint.TabIndex = 9;
			this.m_rbtnControlPoint.TabStop = true;
			this.m_rbtnControlPoint.Text = "控制點";
			this.m_rbtnControlPoint.UseVisualStyleBackColor = true;
			this.m_rbtnControlPoint.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedhanged);
			// 
			// m_flpnlButton
			// 
			this.m_flpnlButton.AutoScroll = true;
			this.m_flpnlButton.Controls.Add(this.m_btnDone);
			this.m_flpnlButton.Controls.Add(this.m_btnDelete);
			this.m_flpnlButton.Controls.Add(this.m_btnAdd);
			this.m_flpnlButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_flpnlButton.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.m_flpnlButton.Location = new System.Drawing.Point(3, 319);
			this.m_flpnlButton.Name = "m_flpnlButton";
			this.m_flpnlButton.Size = new System.Drawing.Size(583, 49);
			this.m_flpnlButton.TabIndex = 5;
			// 
			// m_btnDelete
			// 
			this.m_btnDelete.Location = new System.Drawing.Point(404, 3);
			this.m_btnDelete.Name = "m_btnDelete";
			this.m_btnDelete.Size = new System.Drawing.Size(84, 33);
			this.m_btnDelete.TabIndex = 13;
			this.m_btnDelete.Text = "刪除";
			this.m_btnDelete.UseVisualStyleBackColor = true;
			this.m_btnDelete.Click += new System.EventHandler(this.m_btnDelete_Click);
			// 
			// m_btnAdd
			// 
			this.m_btnAdd.Location = new System.Drawing.Point(314, 3);
			this.m_btnAdd.Name = "m_btnAdd";
			this.m_btnAdd.Size = new System.Drawing.Size(84, 33);
			this.m_btnAdd.TabIndex = 12;
			this.m_btnAdd.Text = "新增";
			this.m_btnAdd.UseVisualStyleBackColor = true;
			this.m_btnAdd.Click += new System.EventHandler(this.m_btnAdd_Click);
			// 
			// m_flpnlToStartOrEnd
			// 
			this.m_flpnlToStartOrEnd.Controls.Add(this.m_lblToStartOrEnd);
			this.m_flpnlToStartOrEnd.Controls.Add(this.m_btnToStartOrEnd);
			this.m_flpnlToStartOrEnd.Controls.Add(this.m_lblIsStartOrEnd);
			this.m_flpnlToStartOrEnd.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_flpnlToStartOrEnd.Location = new System.Drawing.Point(3, 73);
			this.m_flpnlToStartOrEnd.Name = "m_flpnlToStartOrEnd";
			this.m_flpnlToStartOrEnd.Size = new System.Drawing.Size(583, 49);
			this.m_flpnlToStartOrEnd.TabIndex = 12;
			// 
			// m_lblToStartOrEnd
			// 
			this.m_lblToStartOrEnd.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblToStartOrEnd.AutoSize = true;
			this.m_lblToStartOrEnd.Location = new System.Drawing.Point(3, 12);
			this.m_lblToStartOrEnd.Name = "m_lblToStartOrEnd";
			this.m_lblToStartOrEnd.Size = new System.Drawing.Size(82, 15);
			this.m_lblToStartOrEnd.TabIndex = 2;
			this.m_lblToStartOrEnd.Text = "起終點切換";
			// 
			// m_btnToStartOrEnd
			// 
			this.m_btnToStartOrEnd.Location = new System.Drawing.Point(91, 3);
			this.m_btnToStartOrEnd.Name = "m_btnToStartOrEnd";
			this.m_btnToStartOrEnd.Size = new System.Drawing.Size(84, 33);
			this.m_btnToStartOrEnd.TabIndex = 3;
			this.m_btnToStartOrEnd.Text = "起／終點";
			this.m_btnToStartOrEnd.UseVisualStyleBackColor = true;
			this.m_btnToStartOrEnd.Click += new System.EventHandler(this.m_btnToStartOrEnd_Click);
			// 
			// m_lblIsStartOrEnd
			// 
			this.m_lblIsStartOrEnd.AutoSize = true;
			this.m_lblIsStartOrEnd.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_lblIsStartOrEnd.Location = new System.Drawing.Point(181, 0);
			this.m_lblIsStartOrEnd.Name = "m_lblIsStartOrEnd";
			this.m_lblIsStartOrEnd.Size = new System.Drawing.Size(112, 39);
			this.m_lblIsStartOrEnd.TabIndex = 4;
			this.m_lblIsStartOrEnd.Text = "目前位置：起點";
			this.m_lblIsStartOrEnd.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ExtendedGCodeDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnDone;
			this.ClientSize = new System.Drawing.Size(607, 389);
			this.Controls.Add(this.m_pnlExtendedGCode);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "ExtendedGCodeDlg";
			this.Padding = new System.Windows.Forms.Padding(9);
			this.Text = "編輯Ｇ碼";
			this.Deactivate += new System.EventHandler(this.ExtendedGCodeDlg_Deactivate);
			this.m_pnlExtendedGCode.ResumeLayout(false);
			this.m_pnlExtendedGCode.PerformLayout();
			this.m_flpnlGCode.ResumeLayout(false);
			this.m_flpnlGCode.PerformLayout();
			this.m_flpnlRadioButton.ResumeLayout(false);
			this.m_flpnlRadioButton.PerformLayout();
			this.m_flpnlButton.ResumeLayout(false);
			this.m_flpnlToStartOrEnd.ResumeLayout(false);
			this.m_flpnlToStartOrEnd.PerformLayout();
			this.ResumeLayout(false);

        }

		#endregion
		private System.Windows.Forms.Button m_btnDone;
		private System.Windows.Forms.Label m_lblInsertType;
		private System.Windows.Forms.RadioButton m_rbtnTraverseStart;
		private System.Windows.Forms.RadioButton m_rbtnTraverseEnd;
		private System.Windows.Forms.RadioButton m_rbtnPathStart;
		private System.Windows.Forms.RadioButton m_rbtnPathEnd;
		private System.Windows.Forms.RadioButton m_rbtnControlPoint;
		private System.Windows.Forms.TableLayoutPanel m_pnlExtendedGCode;
		private System.Windows.Forms.TextBox m_txtGCode;
		private System.Windows.Forms.FlowLayoutPanel m_flpnlRadioButton;
		private System.Windows.Forms.FlowLayoutPanel m_flpnlButton;
		private System.Windows.Forms.Button m_btnAdd;
		private System.Windows.Forms.Button m_btnDelete;
		private System.Windows.Forms.FlowLayoutPanel m_flpnlToStartOrEnd;
		private System.Windows.Forms.Label m_lblToStartOrEnd;
		private System.Windows.Forms.Button m_btnToStartOrEnd;
		private System.Windows.Forms.Label m_lblIsStartOrEnd;
		private System.Windows.Forms.FlowLayoutPanel m_flpnlGCode;
		private System.Windows.Forms.Label m_lblGCode;
		private System.Windows.Forms.Label m_lblIsMixData;
	}
}
