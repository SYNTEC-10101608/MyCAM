namespace MyCAM.Editor
{
	partial class OrderDialog
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
			this.m_pnlOrderMode = new CommonUI.Controls.SectionPanel();
			this.m_rdoSequentialOrder = new System.Windows.Forms.RadioButton();
			this.m_rdoPathSwap = new System.Windows.Forms.RadioButton();
			this.m_pnlContent = new CommonUI.Controls.SectionPanel();
			this.m_lblSequentialDesc = new System.Windows.Forms.Label();
			this.m_btnMoveUp = new CommonUI.Controls.ThemedButton();
			this.m_btnMoveDown = new CommonUI.Controls.ThemedButton();
			this.m_btnMoveToTop = new CommonUI.Controls.ThemedButton();
			this.m_btnApply = new CommonUI.Controls.ThemedButton();
			this.m_pnlOrderMode.SuspendLayout();
			this.m_pnlContent.SuspendLayout();
			this.SuspendLayout();
			//
			// m_pnlOrderMode
			//
			this.m_pnlOrderMode.Controls.Add(this.m_rdoSequentialOrder);
			this.m_pnlOrderMode.Controls.Add(this.m_rdoPathSwap);
			this.m_pnlOrderMode.Location = new System.Drawing.Point(12, 12);
			this.m_pnlOrderMode.Name = "m_pnlOrderMode";
			this.m_pnlOrderMode.Size = new System.Drawing.Size(296, 99);
			this.m_pnlOrderMode.TabIndex = 0;
			this.m_pnlOrderMode.Text = "排序模式";
			//
			// m_rdoSequentialOrder
			//
			this.m_rdoSequentialOrder.AutoSize = true;
			this.m_rdoSequentialOrder.Location = new System.Drawing.Point(10, 38);
			this.m_rdoSequentialOrder.Name = "m_rdoSequentialOrder";
			this.m_rdoSequentialOrder.Size = new System.Drawing.Size(73, 20);
			this.m_rdoSequentialOrder.TabIndex = 0;
			this.m_rdoSequentialOrder.TabStop = true;
			this.m_rdoSequentialOrder.Text = "依次排序";
			this.m_rdoSequentialOrder.CheckedChanged += new System.EventHandler(this.m_rdoSequentialOrder_CheckedChanged);
			//
			// m_rdoPathSwap
			//
			this.m_rdoPathSwap.AutoSize = true;
			this.m_rdoPathSwap.Location = new System.Drawing.Point(10, 68);
			this.m_rdoPathSwap.Name = "m_rdoPathSwap";
			this.m_rdoPathSwap.Size = new System.Drawing.Size(73, 20);
			this.m_rdoPathSwap.TabIndex = 1;
			this.m_rdoPathSwap.TabStop = true;
			this.m_rdoPathSwap.Text = "路徑置換";
			this.m_rdoPathSwap.CheckedChanged += new System.EventHandler(this.m_rdoPathSwap_CheckedChanged);
			//
			// m_pnlContent
			//
			this.m_pnlContent.Controls.Add(this.m_lblSequentialDesc);
			this.m_pnlContent.Controls.Add(this.m_btnMoveUp);
			this.m_pnlContent.Controls.Add(this.m_btnMoveDown);
			this.m_pnlContent.Location = new System.Drawing.Point(12, 119);
			this.m_pnlContent.Name = "m_pnlContent";
			this.m_pnlContent.Size = new System.Drawing.Size(296, 99);
			this.m_pnlContent.TabIndex = 1;
			this.m_pnlContent.Text = "設定";
			//
			// m_lblSequentialDesc
			//
			this.m_lblSequentialDesc.Location = new System.Drawing.Point(10, 40);
			this.m_lblSequentialDesc.Name = "m_lblSequentialDesc";
			this.m_lblSequentialDesc.Size = new System.Drawing.Size(276, 49);
			this.m_lblSequentialDesc.TabIndex = 0;
			this.m_lblSequentialDesc.Text = "選擇路徑後，按下 Enter\r\n\r\n這條路徑會排列在當前旗標路徑之後。";
			//
			// m_btnMoveUp
			//
			this.m_btnMoveUp.Location = new System.Drawing.Point(10, 36);
			this.m_btnMoveUp.Name = "m_btnMoveUp";
			this.m_btnMoveUp.Size = new System.Drawing.Size(276, 23);
			this.m_btnMoveUp.TabIndex = 1;
			this.m_btnMoveUp.Text = "往前移動（順序變小）";
			this.m_btnMoveUp.Visible = false;
			this.m_btnMoveUp.Click += new System.EventHandler(this.m_btnMoveUp_Click);
			//
			// m_btnMoveDown
			//
			this.m_btnMoveDown.Location = new System.Drawing.Point(10, 66);
			this.m_btnMoveDown.Name = "m_btnMoveDown";
			this.m_btnMoveDown.Size = new System.Drawing.Size(276, 23);
			this.m_btnMoveDown.TabIndex = 2;
			this.m_btnMoveDown.Text = "往後移動（順序變大）";
			this.m_btnMoveDown.Visible = false;
			this.m_btnMoveDown.Click += new System.EventHandler(this.m_btnMoveDown_Click);
			//
			// m_btnMoveToTop
			//
			this.m_btnMoveToTop.Location = new System.Drawing.Point(12, 234);
			this.m_btnMoveToTop.Name = "m_btnMoveToTop";
			this.m_btnMoveToTop.Size = new System.Drawing.Size(80, 28);
			this.m_btnMoveToTop.TabIndex = 2;
			this.m_btnMoveToTop.Text = "置頂";
			this.m_btnMoveToTop.Click += new System.EventHandler(this.m_btnMoveToTop_Click);
			//
			// m_btnApply
			//
			this.m_btnApply.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnApply.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnApply.Location = new System.Drawing.Point(228, 234);
			this.m_btnApply.Name = "m_btnApply";
			this.m_btnApply.Size = new System.Drawing.Size(80, 28);
			this.m_btnApply.TabIndex = 3;
			this.m_btnApply.Text = "確定";
			this.m_btnApply.Click += new System.EventHandler(this.m_btnApply_Click);
			//
			// OrderDialog
			//
			this.ClientSize = new System.Drawing.Size(320, 274);
			this.Controls.Add(this.m_pnlOrderMode);
			this.Controls.Add(this.m_pnlContent);
			this.Controls.Add(this.m_btnMoveToTop);
			this.Controls.Add(this.m_btnApply);
			this.Name = "OrderDialog";
			this.Text = "路徑排序";
			this.m_pnlOrderMode.ResumeLayout(false);
			this.m_pnlOrderMode.PerformLayout();
			this.m_pnlContent.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlOrderMode;
		private System.Windows.Forms.RadioButton m_rdoSequentialOrder;
		private System.Windows.Forms.RadioButton m_rdoPathSwap;
		private CommonUI.Controls.SectionPanel m_pnlContent;
		private System.Windows.Forms.Label m_lblSequentialDesc;
		private CommonUI.Controls.ThemedButton m_btnMoveUp;
		private CommonUI.Controls.ThemedButton m_btnMoveDown;
		private CommonUI.Controls.ThemedButton m_btnMoveToTop;
		private CommonUI.Controls.ThemedButton m_btnApply;
	}
}
