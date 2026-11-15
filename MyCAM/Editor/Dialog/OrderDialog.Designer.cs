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
			this.m_gbxOrderMode = new CommonUI.Controls.SectionPanel();
			this.m_rbSequentialOrder = new System.Windows.Forms.RadioButton();
			this.m_rbPathSwap = new System.Windows.Forms.RadioButton();
			this.m_gbxContent = new CommonUI.Controls.SectionPanel();
			this.m_lblSequentialDesc = new System.Windows.Forms.Label();
			this.m_btnMoveUp = new CommonUI.Controls.ThemedButton();
			this.m_btnMoveDown = new CommonUI.Controls.ThemedButton();
			this.m_btnMoveToTop = new CommonUI.Controls.ThemedButton();
			this.m_btnApply = new CommonUI.Controls.ThemedButton();
			this.m_gbxOrderMode.SuspendLayout();
			this.m_gbxContent.SuspendLayout();
			this.SuspendLayout();
			//
			// m_gbxOrderMode
			//
			this.m_gbxOrderMode.Controls.Add(this.m_rbSequentialOrder);
			this.m_gbxOrderMode.Controls.Add(this.m_rbPathSwap);
			this.m_gbxOrderMode.Location = new System.Drawing.Point(12, 12);
			this.m_gbxOrderMode.Name = "m_gbxOrderMode";
			this.m_gbxOrderMode.Size = new System.Drawing.Size(296, 99);
			this.m_gbxOrderMode.TabIndex = 0;
			this.m_gbxOrderMode.Text = "排序模式";
			//
			// m_rbSequentialOrder
			//
			this.m_rbSequentialOrder.AutoSize = true;
			this.m_rbSequentialOrder.Location = new System.Drawing.Point(10, 38);
			this.m_rbSequentialOrder.Name = "m_rbSequentialOrder";
			this.m_rbSequentialOrder.Size = new System.Drawing.Size(73, 20);
			this.m_rbSequentialOrder.TabIndex = 0;
			this.m_rbSequentialOrder.TabStop = true;
			this.m_rbSequentialOrder.Text = "依次排序";
			this.m_rbSequentialOrder.CheckedChanged += new System.EventHandler(this.m_rbSequentialOrder_CheckedChanged);
			//
			// m_rbPathSwap
			//
			this.m_rbPathSwap.AutoSize = true;
			this.m_rbPathSwap.Location = new System.Drawing.Point(10, 68);
			this.m_rbPathSwap.Name = "m_rbPathSwap";
			this.m_rbPathSwap.Size = new System.Drawing.Size(73, 20);
			this.m_rbPathSwap.TabIndex = 1;
			this.m_rbPathSwap.TabStop = true;
			this.m_rbPathSwap.Text = "路徑置換";
			this.m_rbPathSwap.CheckedChanged += new System.EventHandler(this.m_rbPathSwap_CheckedChanged);
			//
			// m_gbxContent
			//
			this.m_gbxContent.Controls.Add(this.m_lblSequentialDesc);
			this.m_gbxContent.Controls.Add(this.m_btnMoveUp);
			this.m_gbxContent.Controls.Add(this.m_btnMoveDown);
			this.m_gbxContent.Location = new System.Drawing.Point(12, 119);
			this.m_gbxContent.Name = "m_gbxContent";
			this.m_gbxContent.Size = new System.Drawing.Size(296, 99);
			this.m_gbxContent.TabIndex = 1;
			this.m_gbxContent.Text = "設定";
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
			this.Controls.Add(this.m_gbxOrderMode);
			this.Controls.Add(this.m_gbxContent);
			this.Controls.Add(this.m_btnMoveToTop);
			this.Controls.Add(this.m_btnApply);
			this.Name = "OrderDialog";
			this.Text = "路徑排序";
			this.m_gbxOrderMode.ResumeLayout(false);
			this.m_gbxOrderMode.PerformLayout();
			this.m_gbxContent.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_gbxOrderMode;
		private System.Windows.Forms.RadioButton m_rbSequentialOrder;
		private System.Windows.Forms.RadioButton m_rbPathSwap;
		private CommonUI.Controls.SectionPanel m_gbxContent;
		private System.Windows.Forms.Label m_lblSequentialDesc;
		private CommonUI.Controls.ThemedButton m_btnMoveUp;
		private CommonUI.Controls.ThemedButton m_btnMoveDown;
		private CommonUI.Controls.ThemedButton m_btnMoveToTop;
		private CommonUI.Controls.ThemedButton m_btnApply;
	}
}
