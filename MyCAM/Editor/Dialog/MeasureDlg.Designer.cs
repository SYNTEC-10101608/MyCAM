namespace MyCAM.Editor
{
	partial class MeasureDlg
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
			this.m_pnlMeasure = new CommonUI.Controls.SectionPanel();
			this.m_lblMethod = new CommonUI.Controls.ThemedLabel();
			this.m_cmbMethod = new System.Windows.Forms.ComboBox();
			this.m_lblGuide = new CommonUI.Controls.ThemedLabel();
			this.m_lblResult = new CommonUI.Controls.ThemedLabel();
			this.m_txtResult = new System.Windows.Forms.TextBox();
			this.m_pnlMeasure.SuspendLayout();
			this.SuspendLayout();
			//
			// m_pnlMeasure
			//
			this.m_pnlMeasure.Controls.Add(this.m_lblMethod);
			this.m_pnlMeasure.Controls.Add(this.m_cmbMethod);
			this.m_pnlMeasure.Controls.Add(this.m_lblGuide);
			this.m_pnlMeasure.Controls.Add(this.m_lblResult);
			this.m_pnlMeasure.Controls.Add(this.m_txtResult);
			this.m_pnlMeasure.Location = new System.Drawing.Point(12, 12);
			this.m_pnlMeasure.Name = "m_pnlMeasure";
			this.m_pnlMeasure.Size = new System.Drawing.Size(296, 210);
			this.m_pnlMeasure.TabIndex = 0;
			//
			// m_lblMethod
			//
			this.m_lblMethod.AutoSize = true;
			this.m_lblMethod.Location = new System.Drawing.Point(10, 14);
			this.m_lblMethod.Name = "m_lblMethod";
			this.m_lblMethod.Size = new System.Drawing.Size(55, 16);
			this.m_lblMethod.TabIndex = 0;
			this.m_lblMethod.Text = "量測方式";
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
			this.m_lblGuide.Size = new System.Drawing.Size(276, 49);
			this.m_lblGuide.TabIndex = 2;
			this.m_lblGuide.Text = "請先選擇量測方式";
			//
			// m_lblResult
			//
			this.m_lblResult.AutoSize = true;
			this.m_lblResult.Location = new System.Drawing.Point(10, 104);
			this.m_lblResult.Name = "m_lblResult";
			this.m_lblResult.Size = new System.Drawing.Size(55, 16);
			this.m_lblResult.TabIndex = 3;
			this.m_lblResult.Text = "量測結果";
			//
			// m_txtResult
			//
			this.m_txtResult.Location = new System.Drawing.Point(10, 130);
			this.m_txtResult.Multiline = true;
			this.m_txtResult.Name = "m_txtResult";
			this.m_txtResult.ReadOnly = true;
			this.m_txtResult.Size = new System.Drawing.Size(276, 70);
			this.m_txtResult.TabIndex = 4;
			//
			// MeasureDlg
			//
			this.ClientSize = new System.Drawing.Size(320, 234);
			this.Controls.Add(this.m_pnlMeasure);
			this.Name = "MeasureDlg";
			this.Text = "量測";
			this.m_pnlMeasure.ResumeLayout(false);
			this.m_pnlMeasure.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlMeasure;
		private CommonUI.Controls.ThemedLabel m_lblMethod;
		private System.Windows.Forms.ComboBox m_cmbMethod;
		private CommonUI.Controls.ThemedLabel m_lblGuide;
		private CommonUI.Controls.ThemedLabel m_lblResult;
		private System.Windows.Forms.TextBox m_txtResult;
	}
}
