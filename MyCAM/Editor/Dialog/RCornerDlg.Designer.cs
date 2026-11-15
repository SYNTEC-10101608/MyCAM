namespace MyCAM.Editor
{
	partial class RCornerDlg
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
			this.m_secStatus = new CommonUI.Controls.SectionPanel();
			this.m_lblStatus = new System.Windows.Forms.Label();
			this.m_btnDelete = new CommonUI.Controls.ThemedButton();
			this.m_btnSwitchStartOrEnd = new CommonUI.Controls.ThemedButton();
			this.m_btnDone = new CommonUI.Controls.ThemedButton();
			this.m_secStatus.SuspendLayout();
			this.SuspendLayout();
			//
			// m_secStatus
			//
			this.m_secStatus.Controls.Add(this.m_lblStatus);
			this.m_secStatus.Location = new System.Drawing.Point(12, 12);
			this.m_secStatus.Name = "m_secStatus";
			this.m_secStatus.Size = new System.Drawing.Size(296, 43);
			this.m_secStatus.TabIndex = 0;
			//
			// m_lblStatus
			//
			this.m_lblStatus.Location = new System.Drawing.Point(10, 14);
			this.m_lblStatus.Name = "m_lblStatus";
			this.m_lblStatus.Size = new System.Drawing.Size(276, 19);
			this.m_lblStatus.TabIndex = 0;
			//
			// m_btnDelete
			//
			this.m_btnDelete.Kind = CommonUI.Controls.ButtonKind.Danger;
			this.m_btnDelete.Location = new System.Drawing.Point(12, 71);
			this.m_btnDelete.Name = "m_btnDelete";
			this.m_btnDelete.Size = new System.Drawing.Size(96, 28);
			this.m_btnDelete.TabIndex = 1;
			this.m_btnDelete.Text = "刪除";
			this.m_btnDelete.Click += new System.EventHandler(this.m_btnDelete_Click);
			//
			// m_btnSwitchStartOrEnd
			//
			this.m_btnSwitchStartOrEnd.Location = new System.Drawing.Point(116, 71);
			this.m_btnSwitchStartOrEnd.Name = "m_btnSwitchStartOrEnd";
			this.m_btnSwitchStartOrEnd.Size = new System.Drawing.Size(96, 28);
			this.m_btnSwitchStartOrEnd.TabIndex = 2;
			this.m_btnSwitchStartOrEnd.Text = "切換起終點";
			this.m_btnSwitchStartOrEnd.Click += new System.EventHandler(this.m_btnSwitchStartOrEnd_Click);
			//
			// m_btnDone
			//
			this.m_btnDone.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnDone.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnDone.Location = new System.Drawing.Point(228, 71);
			this.m_btnDone.Name = "m_btnDone";
			this.m_btnDone.Size = new System.Drawing.Size(80, 28);
			this.m_btnDone.TabIndex = 3;
			this.m_btnDone.Text = "確定";
			this.m_btnDone.Click += new System.EventHandler(this.m_btnDone_Click);
			//
			// RCornerDlg
			//
			this.CancelButton = this.m_btnDone;
			this.ClientSize = new System.Drawing.Size(320, 111);
			this.Controls.Add(this.m_secStatus);
			this.Controls.Add(this.m_btnDelete);
			this.Controls.Add(this.m_btnSwitchStartOrEnd);
			this.Controls.Add(this.m_btnDone);
			this.Name = "RCornerDlg";
			this.Text = "R角";
			this.m_secStatus.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_secStatus;
		private System.Windows.Forms.Label m_lblStatus;
		private CommonUI.Controls.ThemedButton m_btnDelete;
		private CommonUI.Controls.ThemedButton m_btnSwitchStartOrEnd;
		private CommonUI.Controls.ThemedButton m_btnDone;
	}
}
