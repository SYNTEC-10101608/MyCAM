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
			this.m_flpnlRadioButton = new System.Windows.Forms.Panel();
			this.m_rbtnTraverseStart = new System.Windows.Forms.RadioButton();
			this.m_rbtnTraverseEnd = new System.Windows.Forms.RadioButton();
			this.m_rbtnPathStart = new System.Windows.Forms.RadioButton();
			this.m_rbtnPathEnd = new System.Windows.Forms.RadioButton();
			this.m_rbtnControlPoint = new System.Windows.Forms.RadioButton();
			this.m_flpnlToStartOrEnd = new System.Windows.Forms.Panel();
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
			this.m_flpnlRadioButton.SuspendLayout();
			this.m_flpnlToStartOrEnd.SuspendLayout();
			this.SuspendLayout();
			//
			// m_pnlExtendedGCode
			//
			this.m_pnlExtendedGCode.Controls.Add(this.m_lblInsertType);
			this.m_pnlExtendedGCode.Controls.Add(this.m_flpnlRadioButton);
			this.m_pnlExtendedGCode.Controls.Add(this.m_flpnlToStartOrEnd);
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
			// m_flpnlRadioButton
			//
			this.m_flpnlRadioButton.Controls.Add(this.m_rbtnTraverseStart);
			this.m_flpnlRadioButton.Controls.Add(this.m_rbtnTraverseEnd);
			this.m_flpnlRadioButton.Controls.Add(this.m_rbtnPathStart);
			this.m_flpnlRadioButton.Controls.Add(this.m_rbtnPathEnd);
			this.m_flpnlRadioButton.Controls.Add(this.m_rbtnControlPoint);
			this.m_flpnlRadioButton.Location = new System.Drawing.Point(10, 40);
			this.m_flpnlRadioButton.Name = "m_flpnlRadioButton";
			this.m_flpnlRadioButton.Size = new System.Drawing.Size(456, 23);
			this.m_flpnlRadioButton.TabIndex = 1;
			//
			// m_rbtnTraverseStart
			//
			this.m_rbtnTraverseStart.AutoSize = true;
			this.m_rbtnTraverseStart.Location = new System.Drawing.Point(0, 2);
			this.m_rbtnTraverseStart.Name = "m_rbtnTraverseStart";
			this.m_rbtnTraverseStart.Size = new System.Drawing.Size(73, 20);
			this.m_rbtnTraverseStart.TabIndex = 0;
			this.m_rbtnTraverseStart.TabStop = true;
			this.m_rbtnTraverseStart.Text = "空移開始";
			this.m_rbtnTraverseStart.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedhanged);
			//
			// m_rbtnTraverseEnd
			//
			this.m_rbtnTraverseEnd.AutoSize = true;
			this.m_rbtnTraverseEnd.Location = new System.Drawing.Point(90, 2);
			this.m_rbtnTraverseEnd.Name = "m_rbtnTraverseEnd";
			this.m_rbtnTraverseEnd.Size = new System.Drawing.Size(73, 20);
			this.m_rbtnTraverseEnd.TabIndex = 1;
			this.m_rbtnTraverseEnd.TabStop = true;
			this.m_rbtnTraverseEnd.Text = "空移結束";
			this.m_rbtnTraverseEnd.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedhanged);
			//
			// m_rbtnPathStart
			//
			this.m_rbtnPathStart.AutoSize = true;
			this.m_rbtnPathStart.Location = new System.Drawing.Point(180, 2);
			this.m_rbtnPathStart.Name = "m_rbtnPathStart";
			this.m_rbtnPathStart.Size = new System.Drawing.Size(73, 20);
			this.m_rbtnPathStart.TabIndex = 2;
			this.m_rbtnPathStart.TabStop = true;
			this.m_rbtnPathStart.Text = "路徑開始";
			this.m_rbtnPathStart.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedhanged);
			//
			// m_rbtnPathEnd
			//
			this.m_rbtnPathEnd.AutoSize = true;
			this.m_rbtnPathEnd.Location = new System.Drawing.Point(270, 2);
			this.m_rbtnPathEnd.Name = "m_rbtnPathEnd";
			this.m_rbtnPathEnd.Size = new System.Drawing.Size(73, 20);
			this.m_rbtnPathEnd.TabIndex = 3;
			this.m_rbtnPathEnd.TabStop = true;
			this.m_rbtnPathEnd.Text = "路徑結束";
			this.m_rbtnPathEnd.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedhanged);
			//
			// m_rbtnControlPoint
			//
			this.m_rbtnControlPoint.AutoSize = true;
			this.m_rbtnControlPoint.Location = new System.Drawing.Point(360, 2);
			this.m_rbtnControlPoint.Name = "m_rbtnControlPoint";
			this.m_rbtnControlPoint.Size = new System.Drawing.Size(61, 20);
			this.m_rbtnControlPoint.TabIndex = 4;
			this.m_rbtnControlPoint.TabStop = true;
			this.m_rbtnControlPoint.Text = "控制點";
			this.m_rbtnControlPoint.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedhanged);
			//
			// m_flpnlToStartOrEnd
			//
			this.m_flpnlToStartOrEnd.Controls.Add(this.m_lblToStartOrEnd);
			this.m_flpnlToStartOrEnd.Controls.Add(this.m_btnToStartOrEnd);
			this.m_flpnlToStartOrEnd.Controls.Add(this.m_lblIsStartOrEnd);
			this.m_flpnlToStartOrEnd.Location = new System.Drawing.Point(10, 70);
			this.m_flpnlToStartOrEnd.Name = "m_flpnlToStartOrEnd";
			this.m_flpnlToStartOrEnd.Size = new System.Drawing.Size(456, 23);
			this.m_flpnlToStartOrEnd.TabIndex = 2;
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
			this.m_flpnlRadioButton.ResumeLayout(false);
			this.m_flpnlRadioButton.PerformLayout();
			this.m_flpnlToStartOrEnd.ResumeLayout(false);
			this.m_flpnlToStartOrEnd.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlExtendedGCode;
		private System.Windows.Forms.Label m_lblInsertType;
		private System.Windows.Forms.Panel m_flpnlRadioButton;
		private System.Windows.Forms.RadioButton m_rbtnTraverseStart;
		private System.Windows.Forms.RadioButton m_rbtnTraverseEnd;
		private System.Windows.Forms.RadioButton m_rbtnPathStart;
		private System.Windows.Forms.RadioButton m_rbtnPathEnd;
		private System.Windows.Forms.RadioButton m_rbtnControlPoint;
		private System.Windows.Forms.Panel m_flpnlToStartOrEnd;
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
