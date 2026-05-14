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
			this.m_gbxOrderMode = new System.Windows.Forms.GroupBox();
			this.m_rbPathSwap = new System.Windows.Forms.RadioButton();
			this.m_rbSequentialOrder = new System.Windows.Forms.RadioButton();
			this.m_gbxContent = new System.Windows.Forms.GroupBox();
			this.m_lblSequentialDesc = new System.Windows.Forms.Label();
			this.m_btnMoveUp = new System.Windows.Forms.Button();
			this.m_btnMoveDown = new System.Windows.Forms.Button();
			this.m_btnMoveToTop = new System.Windows.Forms.Button();
			this.m_btnApply = new System.Windows.Forms.Button();
			this.m_gbxOrderMode.SuspendLayout();
			this.m_gbxContent.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_gbxOrderMode
			// 
			this.m_gbxOrderMode.Controls.Add(this.m_rbPathSwap);
			this.m_gbxOrderMode.Controls.Add(this.m_rbSequentialOrder);
			this.m_gbxOrderMode.Location = new System.Drawing.Point(12, 12);
			this.m_gbxOrderMode.Margin = new System.Windows.Forms.Padding(4);
			this.m_gbxOrderMode.Name = "m_gbxOrderMode";
			this.m_gbxOrderMode.Padding = new System.Windows.Forms.Padding(7, 8, 7, 8);
			this.m_gbxOrderMode.Size = new System.Drawing.Size(280, 90);
			this.m_gbxOrderMode.TabIndex = 0;
			this.m_gbxOrderMode.TabStop = false;
			this.m_gbxOrderMode.Text = "排序模式";
			// 
			// m_rbPathSwap
			// 
			this.m_rbPathSwap.AutoSize = true;
			this.m_rbPathSwap.Location = new System.Drawing.Point(20, 56);
			this.m_rbPathSwap.Margin = new System.Windows.Forms.Padding(4);
			this.m_rbPathSwap.Name = "m_rbPathSwap";
			this.m_rbPathSwap.Size = new System.Drawing.Size(88, 19);
			this.m_rbPathSwap.TabIndex = 1;
			this.m_rbPathSwap.TabStop = true;
			this.m_rbPathSwap.Text = "路徑置換";
			this.m_rbPathSwap.UseVisualStyleBackColor = true;
			this.m_rbPathSwap.CheckedChanged += new System.EventHandler(this.m_rbPathSwap_CheckedChanged);
			// 
			// m_rbSequentialOrder
			// 
			this.m_rbSequentialOrder.AutoSize = true;
			this.m_rbSequentialOrder.Location = new System.Drawing.Point(20, 28);
			this.m_rbSequentialOrder.Margin = new System.Windows.Forms.Padding(4);
			this.m_rbSequentialOrder.Name = "m_rbSequentialOrder";
			this.m_rbSequentialOrder.Size = new System.Drawing.Size(88, 19);
			this.m_rbSequentialOrder.TabIndex = 0;
			this.m_rbSequentialOrder.TabStop = true;
			this.m_rbSequentialOrder.Text = "依次排序";
			this.m_rbSequentialOrder.UseVisualStyleBackColor = true;
			this.m_rbSequentialOrder.CheckedChanged += new System.EventHandler(this.m_rbSequentialOrder_CheckedChanged);
			// 
			// m_gbxContent
			// 
			this.m_gbxContent.Controls.Add(this.m_lblSequentialDesc);
			this.m_gbxContent.Controls.Add(this.m_btnMoveUp);
			this.m_gbxContent.Controls.Add(this.m_btnMoveDown);
			this.m_gbxContent.Location = new System.Drawing.Point(12, 110);
			this.m_gbxContent.Margin = new System.Windows.Forms.Padding(4);
			this.m_gbxContent.Name = "m_gbxContent";
			this.m_gbxContent.Padding = new System.Windows.Forms.Padding(7, 8, 7, 8);
			this.m_gbxContent.Size = new System.Drawing.Size(280, 140);
			this.m_gbxContent.TabIndex = 1;
			this.m_gbxContent.TabStop = false;
			this.m_gbxContent.Text = "設定";
			// 
			// m_lblSequentialDesc
			// 
			this.m_lblSequentialDesc.ForeColor = System.Drawing.Color.Black;
			this.m_lblSequentialDesc.Location = new System.Drawing.Point(11, 39);
			this.m_lblSequentialDesc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblSequentialDesc.Name = "m_lblSequentialDesc";
			this.m_lblSequentialDesc.Size = new System.Drawing.Size(250, 75);
			this.m_lblSequentialDesc.TabIndex = 0;
			this.m_lblSequentialDesc.Text = "選擇路徑後，按下\"Enter\"\r\n\r\n這條路徑會排列在當前🚩旗標路徑之後。";
			// 
			// m_btnMoveUp
			// 
			this.m_btnMoveUp.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_btnMoveUp.Location = new System.Drawing.Point(20, 28);
			this.m_btnMoveUp.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnMoveUp.Name = "m_btnMoveUp";
			this.m_btnMoveUp.Size = new System.Drawing.Size(240, 35);
			this.m_btnMoveUp.TabIndex = 1;
			this.m_btnMoveUp.Text = "▲ 往前移動（順序變小）";
			this.m_btnMoveUp.UseVisualStyleBackColor = true;
			this.m_btnMoveUp.Visible = false;
			this.m_btnMoveUp.Click += new System.EventHandler(this.m_btnMoveUp_Click);
			// 
			// m_btnMoveDown
			// 
			this.m_btnMoveDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_btnMoveDown.Location = new System.Drawing.Point(20, 71);
			this.m_btnMoveDown.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnMoveDown.Name = "m_btnMoveDown";
			this.m_btnMoveDown.Size = new System.Drawing.Size(240, 35);
			this.m_btnMoveDown.TabIndex = 2;
			this.m_btnMoveDown.Text = "▼ 往後移動（順序變大）";
			this.m_btnMoveDown.UseVisualStyleBackColor = true;
			this.m_btnMoveDown.Visible = false;
			this.m_btnMoveDown.Click += new System.EventHandler(this.m_btnMoveDown_Click);
			// 
			// m_btnMoveToTop
			// 
			this.m_btnMoveToTop.Location = new System.Drawing.Point(124, 266);
			this.m_btnMoveToTop.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnMoveToTop.Name = "m_btnMoveToTop";
			this.m_btnMoveToTop.Size = new System.Drawing.Size(80, 35);
			this.m_btnMoveToTop.TabIndex = 2;
			this.m_btnMoveToTop.Text = "置頂";
			this.m_btnMoveToTop.UseVisualStyleBackColor = true;
			this.m_btnMoveToTop.Click += new System.EventHandler(this.m_btnMoveToTop_Click);
			// 
			// m_btnApply
			// 
			this.m_btnApply.Location = new System.Drawing.Point(212, 266);
			this.m_btnApply.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnApply.Name = "m_btnApply";
			this.m_btnApply.Size = new System.Drawing.Size(80, 35);
			this.m_btnApply.TabIndex = 3;
			this.m_btnApply.Text = "確定";
			this.m_btnApply.UseVisualStyleBackColor = true;
			this.m_btnApply.Click += new System.EventHandler(this.m_btnApply_Click);
			// 
			// OrderDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(304, 313);
			this.Controls.Add(this.m_btnMoveToTop);
			this.Controls.Add(this.m_btnApply);
			this.Controls.Add(this.m_gbxContent);
			this.Controls.Add(this.m_gbxOrderMode);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "OrderDialog";
			this.Padding = new System.Windows.Forms.Padding(9);
			this.Text = "路徑排序";
			this.TopMost = true;
			this.m_gbxOrderMode.ResumeLayout(false);
			this.m_gbxOrderMode.PerformLayout();
			this.m_gbxContent.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		System.Windows.Forms.GroupBox m_gbxOrderMode;
		System.Windows.Forms.RadioButton m_rbSequentialOrder;
		System.Windows.Forms.RadioButton m_rbPathSwap;
		System.Windows.Forms.GroupBox m_gbxContent;
		System.Windows.Forms.Label m_lblSequentialDesc;
		System.Windows.Forms.Button m_btnMoveUp;
		System.Windows.Forms.Button m_btnMoveDown;
		System.Windows.Forms.Button m_btnMoveToTop;
		System.Windows.Forms.Button m_btnApply;
	}
}
