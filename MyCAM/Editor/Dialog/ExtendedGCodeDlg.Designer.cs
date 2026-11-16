namespace MyCAM.Editor
{
	partial class ExtendedGCodeDlg
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
			this.m_pnlExtendedGCode = new CommonUI.Controls.SectionPanel();
			this.m_lblInsertType = new System.Windows.Forms.Label();
			this.m_pnlRadioButton = new System.Windows.Forms.Panel();
			this.m_rdoTraverseStart = new System.Windows.Forms.RadioButton();
			this.m_rdoTraverseEnd = new System.Windows.Forms.RadioButton();
			this.m_rdoPathStart = new System.Windows.Forms.RadioButton();
			this.m_rdoPathEnd = new System.Windows.Forms.RadioButton();
			this.m_rdoControlPoint = new System.Windows.Forms.RadioButton();
			this.m_pnlToStartOrEnd = new System.Windows.Forms.Panel();
			this.m_lblToStartOrEnd = new System.Windows.Forms.Label();
			this.m_btnToStartOrEnd = new CommonUI.Controls.ThemedButton();
			this.m_lblIsStartOrEnd = new System.Windows.Forms.Label();
			this.m_lblGCode = new System.Windows.Forms.Label();
			this.m_lblIsMixData = new CommonUI.Controls.ThemedLabel();
			this.m_txtGCode = new System.Windows.Forms.TextBox();
			this.m_btnAdd = new CommonUI.Controls.ThemedButton();
			this.m_btnDelete = new CommonUI.Controls.ThemedButton();
			this.m_btnDone = new CommonUI.Controls.ThemedButton();
			this.m_pnlExtendedGCode.SuspendLayout();
			this.m_pnlRadioButton.SuspendLayout();
			this.m_pnlToStartOrEnd.SuspendLayout();
			this.SuspendLayout();
			//
			// m_pnlExtendedGCode
			//
			this.m_pnlExtendedGCode.Controls.Add(this.m_lblInsertType);
			this.m_pnlExtendedGCode.Controls.Add(this.m_pnlRadioButton);
			this.m_pnlExtendedGCode.Controls.Add(this.m_pnlToStartOrEnd);
			this.m_pnlExtendedGCode.Controls.Add(this.m_lblGCode);
			this.m_pnlExtendedGCode.Controls.Add(this.m_lblIsMixData);
			this.m_pnlExtendedGCode.Controls.Add(this.m_txtGCode);
			this.m_pnlExtendedGCode.Location = new System.Drawing.Point(12, 12);
			this.m_pnlExtendedGCode.Name = "m_pnlExtendedGCode";
			this.m_pnlExtendedGCode.Size = new System.Drawing.Size(476, 290);
			this.m_pnlExtendedGCode.TabIndex = 0;
			//
			// m_lblInsertType
			//
			this.m_lblInsertType.AutoSize = true;
			this.m_lblInsertType.Location = new System.Drawing.Point(10, 14);
			this.m_lblInsertType.Name = "m_lblInsertType";
			this.m_lblInsertType.Size = new System.Drawing.Size(55, 16);
			this.m_lblInsertType.TabIndex = 0;
			this.m_lblInsertType.Text = "插入位置";
			//
			// m_pnlRadioButton
			//
			this.m_pnlRadioButton.Controls.Add(this.m_rdoTraverseStart);
			this.m_pnlRadioButton.Controls.Add(this.m_rdoTraverseEnd);
			this.m_pnlRadioButton.Controls.Add(this.m_rdoPathStart);
			this.m_pnlRadioButton.Controls.Add(this.m_rdoPathEnd);
			this.m_pnlRadioButton.Controls.Add(this.m_rdoControlPoint);
			this.m_pnlRadioButton.Location = new System.Drawing.Point(10, 40);
			this.m_pnlRadioButton.Name = "m_pnlRadioButton";
			this.m_pnlRadioButton.Size = new System.Drawing.Size(456, 23);
			this.m_pnlRadioButton.TabIndex = 1;
			//
			// m_rdoTraverseStart
			//
			this.m_rdoTraverseStart.AutoSize = true;
			this.m_rdoTraverseStart.Location = new System.Drawing.Point(0, 2);
			this.m_rdoTraverseStart.Name = "m_rdoTraverseStart";
			this.m_rdoTraverseStart.Size = new System.Drawing.Size(73, 20);
			this.m_rdoTraverseStart.TabIndex = 0;
			this.m_rdoTraverseStart.TabStop = true;
			this.m_rdoTraverseStart.Text = "空移開始";
			this.m_rdoTraverseStart.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedhanged);
			//
			// m_rdoTraverseEnd
			//
			this.m_rdoTraverseEnd.AutoSize = true;
			this.m_rdoTraverseEnd.Location = new System.Drawing.Point(90, 2);
			this.m_rdoTraverseEnd.Name = "m_rdoTraverseEnd";
			this.m_rdoTraverseEnd.Size = new System.Drawing.Size(73, 20);
			this.m_rdoTraverseEnd.TabIndex = 1;
			this.m_rdoTraverseEnd.TabStop = true;
			this.m_rdoTraverseEnd.Text = "空移結束";
			this.m_rdoTraverseEnd.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedhanged);
			//
			// m_rdoPathStart
			//
			this.m_rdoPathStart.AutoSize = true;
			this.m_rdoPathStart.Location = new System.Drawing.Point(180, 2);
			this.m_rdoPathStart.Name = "m_rdoPathStart";
			this.m_rdoPathStart.Size = new System.Drawing.Size(73, 20);
			this.m_rdoPathStart.TabIndex = 2;
			this.m_rdoPathStart.TabStop = true;
			this.m_rdoPathStart.Text = "路徑開始";
			this.m_rdoPathStart.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedhanged);
			//
			// m_rdoPathEnd
			//
			this.m_rdoPathEnd.AutoSize = true;
			this.m_rdoPathEnd.Location = new System.Drawing.Point(270, 2);
			this.m_rdoPathEnd.Name = "m_rdoPathEnd";
			this.m_rdoPathEnd.Size = new System.Drawing.Size(73, 20);
			this.m_rdoPathEnd.TabIndex = 3;
			this.m_rdoPathEnd.TabStop = true;
			this.m_rdoPathEnd.Text = "路徑結束";
			this.m_rdoPathEnd.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedhanged);
			//
			// m_rdoControlPoint
			//
			this.m_rdoControlPoint.AutoSize = true;
			this.m_rdoControlPoint.Location = new System.Drawing.Point(360, 2);
			this.m_rdoControlPoint.Name = "m_rdoControlPoint";
			this.m_rdoControlPoint.Size = new System.Drawing.Size(61, 20);
			this.m_rdoControlPoint.TabIndex = 4;
			this.m_rdoControlPoint.TabStop = true;
			this.m_rdoControlPoint.Text = "控制點";
			this.m_rdoControlPoint.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedhanged);
			//
			// m_pnlToStartOrEnd
			//
			this.m_pnlToStartOrEnd.Controls.Add(this.m_lblToStartOrEnd);
			this.m_pnlToStartOrEnd.Controls.Add(this.m_btnToStartOrEnd);
			this.m_pnlToStartOrEnd.Controls.Add(this.m_lblIsStartOrEnd);
			this.m_pnlToStartOrEnd.Location = new System.Drawing.Point(10, 70);
			this.m_pnlToStartOrEnd.Name = "m_pnlToStartOrEnd";
			this.m_pnlToStartOrEnd.Size = new System.Drawing.Size(456, 23);
			this.m_pnlToStartOrEnd.TabIndex = 2;
			//
			// m_lblToStartOrEnd
			//
			this.m_lblToStartOrEnd.AutoSize = true;
			this.m_lblToStartOrEnd.Location = new System.Drawing.Point(0, 4);
			this.m_lblToStartOrEnd.Name = "m_lblToStartOrEnd";
			this.m_lblToStartOrEnd.Size = new System.Drawing.Size(67, 16);
			this.m_lblToStartOrEnd.TabIndex = 0;
			this.m_lblToStartOrEnd.Text = "起終點切換";
			//
			// m_btnToStartOrEnd
			//
			this.m_btnToStartOrEnd.Location = new System.Drawing.Point(90, 0);
			this.m_btnToStartOrEnd.Name = "m_btnToStartOrEnd";
			this.m_btnToStartOrEnd.Size = new System.Drawing.Size(96, 23);
			this.m_btnToStartOrEnd.TabIndex = 1;
			this.m_btnToStartOrEnd.Text = "起／終點";
			this.m_btnToStartOrEnd.Click += new System.EventHandler(this.m_btnToStartOrEnd_Click);
			//
			// m_lblIsStartOrEnd
			//
			this.m_lblIsStartOrEnd.AutoSize = true;
			this.m_lblIsStartOrEnd.Location = new System.Drawing.Point(196, 4);
			this.m_lblIsStartOrEnd.Name = "m_lblIsStartOrEnd";
			this.m_lblIsStartOrEnd.Size = new System.Drawing.Size(91, 16);
			this.m_lblIsStartOrEnd.TabIndex = 2;
			this.m_lblIsStartOrEnd.Text = "目前位置：起點";
			//
			// m_lblGCode
			//
			this.m_lblGCode.AutoSize = true;
			this.m_lblGCode.Location = new System.Drawing.Point(10, 104);
			this.m_lblGCode.Name = "m_lblGCode";
			this.m_lblGCode.Size = new System.Drawing.Size(28, 16);
			this.m_lblGCode.TabIndex = 3;
			this.m_lblGCode.Text = "G碼";
			//
			// m_lblIsMixData
			//
			this.m_lblIsMixData.AutoSize = true;
			this.m_lblIsMixData.Location = new System.Drawing.Point(100, 104);
			this.m_lblIsMixData.Name = "m_lblIsMixData";
			this.m_lblIsMixData.Role = CommonUI.Theme.TextRole.Error;
			this.m_lblIsMixData.Size = new System.Drawing.Size(115, 16);
			this.m_lblIsMixData.TabIndex = 4;
			this.m_lblIsMixData.Text = "所選路徑的G碼不一致";
			//
			// m_txtGCode
			//
			this.m_txtGCode.Location = new System.Drawing.Point(10, 130);
			this.m_txtGCode.Multiline = true;
			this.m_txtGCode.Name = "m_txtGCode";
			this.m_txtGCode.Size = new System.Drawing.Size(456, 150);
			this.m_txtGCode.TabIndex = 5;
			this.m_txtGCode.TextChanged += new System.EventHandler(this.m_txtGCode_TextChanged);
			this.m_txtGCode.Leave += new System.EventHandler(this.m_txtGCode_Leave);
			//
			// m_btnAdd
			//
			this.m_btnAdd.Location = new System.Drawing.Point(12, 318);
			this.m_btnAdd.Name = "m_btnAdd";
			this.m_btnAdd.Size = new System.Drawing.Size(80, 28);
			this.m_btnAdd.TabIndex = 1;
			this.m_btnAdd.Text = "新增";
			this.m_btnAdd.Click += new System.EventHandler(this.m_btnAdd_Click);
			//
			// m_btnDelete
			//
			this.m_btnDelete.Kind = CommonUI.Controls.ButtonKind.Danger;
			this.m_btnDelete.Location = new System.Drawing.Point(100, 318);
			this.m_btnDelete.Name = "m_btnDelete";
			this.m_btnDelete.Size = new System.Drawing.Size(80, 28);
			this.m_btnDelete.TabIndex = 2;
			this.m_btnDelete.Text = "刪除";
			this.m_btnDelete.Click += new System.EventHandler(this.m_btnDelete_Click);
			//
			// m_btnDone
			//
			this.m_btnDone.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnDone.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnDone.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnDone.Location = new System.Drawing.Point(408, 318);
			this.m_btnDone.Name = "m_btnDone";
			this.m_btnDone.Size = new System.Drawing.Size(80, 28);
			this.m_btnDone.TabIndex = 3;
			this.m_btnDone.Text = "確定";
			this.m_btnDone.Click += new System.EventHandler(this.m_btnDone_Click);
			//
			// ExtendedGCodeDlg
			//
			this.CancelButton = this.m_btnDone;
			this.ClientSize = new System.Drawing.Size(500, 358);
			this.Controls.Add(this.m_pnlExtendedGCode);
			this.Controls.Add(this.m_btnAdd);
			this.Controls.Add(this.m_btnDelete);
			this.Controls.Add(this.m_btnDone);
			this.Name = "ExtendedGCodeDlg";
			this.Text = "編輯G碼";
			this.Deactivate += new System.EventHandler(this.ExtendedGCodeDlg_Deactivate);
			this.m_pnlExtendedGCode.ResumeLayout(false);
			this.m_pnlExtendedGCode.PerformLayout();
			this.m_pnlRadioButton.ResumeLayout(false);
			this.m_pnlRadioButton.PerformLayout();
			this.m_pnlToStartOrEnd.ResumeLayout(false);
			this.m_pnlToStartOrEnd.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlExtendedGCode;
		private System.Windows.Forms.Label m_lblInsertType;
		private System.Windows.Forms.Panel m_pnlRadioButton;
		private System.Windows.Forms.RadioButton m_rdoTraverseStart;
		private System.Windows.Forms.RadioButton m_rdoTraverseEnd;
		private System.Windows.Forms.RadioButton m_rdoPathStart;
		private System.Windows.Forms.RadioButton m_rdoPathEnd;
		private System.Windows.Forms.RadioButton m_rdoControlPoint;
		private System.Windows.Forms.Panel m_pnlToStartOrEnd;
		private System.Windows.Forms.Label m_lblToStartOrEnd;
		private CommonUI.Controls.ThemedButton m_btnToStartOrEnd;
		private System.Windows.Forms.Label m_lblIsStartOrEnd;
		private System.Windows.Forms.Label m_lblGCode;
		private CommonUI.Controls.ThemedLabel m_lblIsMixData;
		private System.Windows.Forms.TextBox m_txtGCode;
		private CommonUI.Controls.ThemedButton m_btnAdd;
		private CommonUI.Controls.ThemedButton m_btnDelete;
		private CommonUI.Controls.ThemedButton m_btnDone;
	}
}
