namespace MyCAM.Editor
{
	partial class ManualTransformDlg
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
			this.m_pnlAlign = new CommonUI.Controls.SectionPanel();
			this.m_lblMethod = new CommonUI.Controls.ThemedLabel();
			this.m_cmbMethod = new System.Windows.Forms.ComboBox();
			this.m_lblGuide = new CommonUI.Controls.ThemedLabel();
			this.m_lblAxisPlane = new CommonUI.Controls.ThemedLabel();
			this.m_cmbAxisPlane = new System.Windows.Forms.ComboBox();
			this.m_lblMoveStatus = new CommonUI.Controls.ThemedLabel();
			this.m_lblError = new CommonUI.Controls.ThemedLabel();
			this.m_btnConfirm = new CommonUI.Controls.ThemedButton();
			this.m_pnlAlign.SuspendLayout();
			this.SuspendLayout();
			//
			// m_pnlAlign
			//
			this.m_pnlAlign.Controls.Add(this.m_lblMethod);
			this.m_pnlAlign.Controls.Add(this.m_cmbMethod);
			this.m_pnlAlign.Controls.Add(this.m_lblGuide);
			this.m_pnlAlign.Controls.Add(this.m_lblAxisPlane);
			this.m_pnlAlign.Controls.Add(this.m_cmbAxisPlane);
			this.m_pnlAlign.Controls.Add(this.m_lblMoveStatus);
			this.m_pnlAlign.Controls.Add(this.m_lblError);
			this.m_pnlAlign.Location = new System.Drawing.Point(12, 12);
			this.m_pnlAlign.Name = "m_pnlAlign";
			this.m_pnlAlign.Size = new System.Drawing.Size(296, 193);
			this.m_pnlAlign.TabIndex = 0;
			//
			// m_lblMethod
			//
			this.m_lblMethod.AutoSize = true;
			this.m_lblMethod.Location = new System.Drawing.Point(10, 14);
			this.m_lblMethod.Name = "m_lblMethod";
			this.m_lblMethod.Size = new System.Drawing.Size(55, 16);
			this.m_lblMethod.TabIndex = 0;
			this.m_lblMethod.Text = "對齊方式";
			//
			// m_cmbMethod
			//
			this.m_cmbMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbMethod.FormattingEnabled = true;
			this.m_cmbMethod.Location = new System.Drawing.Point(100, 10);
			this.m_cmbMethod.Name = "m_cmbMethod";
			this.m_cmbMethod.Size = new System.Drawing.Size(186, 24);
			this.m_cmbMethod.TabIndex = 1;
			this.m_cmbMethod.SelectedIndexChanged += new System.EventHandler(this.m_cmbMethod_SelectedIndexChanged);
			//
			// m_lblGuide
			//
			this.m_lblGuide.Location = new System.Drawing.Point(10, 44);
			this.m_lblGuide.Name = "m_lblGuide";
			this.m_lblGuide.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblGuide.Size = new System.Drawing.Size(276, 19);
			this.m_lblGuide.TabIndex = 2;
			this.m_lblGuide.Text = "請先選擇對齊方式";
			//
			// m_lblAxisPlane
			//
			this.m_lblAxisPlane.AutoSize = true;
			this.m_lblAxisPlane.Location = new System.Drawing.Point(10, 74);
			this.m_lblAxisPlane.Name = "m_lblAxisPlane";
			this.m_lblAxisPlane.Size = new System.Drawing.Size(43, 16);
			this.m_lblAxisPlane.TabIndex = 3;
			this.m_lblAxisPlane.Text = "對齊點";
			this.m_lblAxisPlane.Visible = false;
			//
			// m_cmbAxisPlane
			//
			this.m_cmbAxisPlane.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbAxisPlane.FormattingEnabled = true;
			this.m_cmbAxisPlane.Location = new System.Drawing.Point(100, 70);
			this.m_cmbAxisPlane.Name = "m_cmbAxisPlane";
			this.m_cmbAxisPlane.Size = new System.Drawing.Size(186, 24);
			this.m_cmbAxisPlane.TabIndex = 4;
			this.m_cmbAxisPlane.Visible = false;
			this.m_cmbAxisPlane.SelectedIndexChanged += new System.EventHandler(this.m_cmbAxisPlane_SelectedIndexChanged);
			//
			// m_lblMoveStatus
			//
			this.m_lblMoveStatus.AutoSize = true;
			this.m_lblMoveStatus.Location = new System.Drawing.Point(10, 104);
			this.m_lblMoveStatus.Name = "m_lblMoveStatus";
			this.m_lblMoveStatus.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblMoveStatus.Size = new System.Drawing.Size(42, 16);
			this.m_lblMoveStatus.TabIndex = 5;
			this.m_lblMoveStatus.Text = "✘ 工件";
			//
			// m_lblError
			//
			this.m_lblError.Location = new System.Drawing.Point(10, 134);
			this.m_lblError.Name = "m_lblError";
			this.m_lblError.Role = CommonUI.Theme.TextRole.Error;
			this.m_lblError.Size = new System.Drawing.Size(276, 49);
			this.m_lblError.TabIndex = 6;
			this.m_lblError.Visible = false;
			//
			// m_btnConfirm
			//
			this.m_btnConfirm.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnConfirm.Enabled = false;
			this.m_btnConfirm.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnConfirm.Location = new System.Drawing.Point(228, 221);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(80, 28);
			this.m_btnConfirm.TabIndex = 1;
			this.m_btnConfirm.Text = "確定";
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			//
			// ManualTransformDlg
			//
			this.ClientSize = new System.Drawing.Size(320, 261);
			this.Controls.Add(this.m_pnlAlign);
			this.Controls.Add(this.m_btnConfirm);
			this.Name = "ManualTransformDlg";
			this.Text = "工件對齊";
			this.m_pnlAlign.ResumeLayout(false);
			this.m_pnlAlign.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlAlign;
		private CommonUI.Controls.ThemedLabel m_lblMethod;
		private System.Windows.Forms.ComboBox m_cmbMethod;
		private CommonUI.Controls.ThemedLabel m_lblGuide;
		private CommonUI.Controls.ThemedLabel m_lblAxisPlane;
		private System.Windows.Forms.ComboBox m_cmbAxisPlane;
		private CommonUI.Controls.ThemedLabel m_lblMoveStatus;
		private CommonUI.Controls.ThemedLabel m_lblError;
		private CommonUI.Controls.ThemedButton m_btnConfirm;
	}
}
