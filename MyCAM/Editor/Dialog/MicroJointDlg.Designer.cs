namespace MyCAM.Editor
{
	partial class MicroJointDlg
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
			this.m_secLength = new CommonUI.Controls.SectionPanel();
			this.m_lblLength = new System.Windows.Forms.Label();
			this.m_tbxMicroJointLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_btnClearAll = new CommonUI.Controls.ThemedButton();
			this.m_btnDelete = new CommonUI.Controls.ThemedButton();
			this.m_btnCancel = new CommonUI.Controls.ThemedButton();
			this.m_secLength.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxMicroJointLength)).BeginInit();
			this.SuspendLayout();
			//
			// m_secLength
			//
			this.m_secLength.Controls.Add(this.m_lblLength);
			this.m_secLength.Controls.Add(this.m_tbxMicroJointLength);
			this.m_secLength.Location = new System.Drawing.Point(12, 12);
			this.m_secLength.Name = "m_secLength";
			this.m_secLength.Size = new System.Drawing.Size(296, 43);
			this.m_secLength.TabIndex = 0;
			//
			// m_lblLength
			//
			this.m_lblLength.AutoSize = true;
			this.m_lblLength.Location = new System.Drawing.Point(10, 14);
			this.m_lblLength.Name = "m_lblLength";
			this.m_lblLength.Size = new System.Drawing.Size(55, 16);
			this.m_lblLength.TabIndex = 0;
			this.m_lblLength.Text = "微連長度";
			//
			// m_tbxMicroJointLength
			//
			this.m_tbxMicroJointLength.DecimalPlaces = 3;
			this.m_tbxMicroJointLength.Minimum = 0.001m;
			this.m_tbxMicroJointLength.Maximum = 99999;
			this.m_tbxMicroJointLength.Increment = 0.1m;
			this.m_tbxMicroJointLength.Location = new System.Drawing.Point(100, 10);
			this.m_tbxMicroJointLength.Name = "m_tbxMicroJointLength";
			this.m_tbxMicroJointLength.Size = new System.Drawing.Size(150, 23);
			this.m_tbxMicroJointLength.TabIndex = 1;
			this.m_tbxMicroJointLength.DebouncedValueChanged += new System.EventHandler(this.m_tbxMicroJointLength_DebouncedValueChanged);
			//
			// m_btnClearAll
			//
			this.m_btnClearAll.Kind = CommonUI.Controls.ButtonKind.Danger;
			this.m_btnClearAll.Location = new System.Drawing.Point(12, 71);
			this.m_btnClearAll.Name = "m_btnClearAll";
			this.m_btnClearAll.Size = new System.Drawing.Size(104, 28);
			this.m_btnClearAll.TabIndex = 1;
			this.m_btnClearAll.Text = "刪除所有微連";
			this.m_btnClearAll.Click += new System.EventHandler(this.m_btnClearAll_Click);
			//
			// m_btnDelete
			//
			this.m_btnDelete.Kind = CommonUI.Controls.ButtonKind.Danger;
			this.m_btnDelete.Location = new System.Drawing.Point(124, 71);
			this.m_btnDelete.Name = "m_btnDelete";
			this.m_btnDelete.Size = new System.Drawing.Size(80, 28);
			this.m_btnDelete.TabIndex = 2;
			this.m_btnDelete.Text = "刪除";
			this.m_btnDelete.Click += new System.EventHandler(this.m_btnDelete_Click);
			//
			// m_btnCancel
			//
			this.m_btnCancel.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnCancel.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnCancel.Location = new System.Drawing.Point(228, 71);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(80, 28);
			this.m_btnCancel.TabIndex = 3;
			this.m_btnCancel.Text = "確定";
			this.m_btnCancel.Click += new System.EventHandler(this.m_btnCancel_Click);
			//
			// MicroJointDlg
			//
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(320, 111);
			this.Controls.Add(this.m_secLength);
			this.Controls.Add(this.m_btnClearAll);
			this.Controls.Add(this.m_btnDelete);
			this.Controls.Add(this.m_btnCancel);
			this.Name = "MicroJointDlg";
			this.Text = "編輯微連";
			this.m_secLength.ResumeLayout(false);
			this.m_secLength.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxMicroJointLength)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_secLength;
		private System.Windows.Forms.Label m_lblLength;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxMicroJointLength;
		private CommonUI.Controls.ThemedButton m_btnClearAll;
		private CommonUI.Controls.ThemedButton m_btnDelete;
		private CommonUI.Controls.ThemedButton m_btnCancel;
	}
}
