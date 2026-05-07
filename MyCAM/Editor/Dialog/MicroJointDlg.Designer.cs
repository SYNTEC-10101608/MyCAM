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
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_lblLength = new System.Windows.Forms.Label();
			this.m_tbxMicroJointLength = new System.Windows.Forms.TextBox();
			this.m_btnClearAll = new System.Windows.Forms.Button();
			this.m_btnDelete = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Location = new System.Drawing.Point(231, 67);
			this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(84, 33);
			this.m_btnCancel.TabIndex = 2;
			this.m_btnCancel.Text = "完成";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.m_btnCancel_Click);
			// 
			// m_lblLength
			// 
			this.m_lblLength.AutoSize = true;
			this.m_lblLength.Location = new System.Drawing.Point(17, 32);
			this.m_lblLength.Name = "m_lblLength";
			this.m_lblLength.Size = new System.Drawing.Size(67, 15);
			this.m_lblLength.TabIndex = 3;
			this.m_lblLength.Text = "微連長度";
			// 
			// m_tbxMicroJointLength
			// 
			this.m_tbxMicroJointLength.Location = new System.Drawing.Point(90, 29);
			this.m_tbxMicroJointLength.Name = "m_tbxMicroJointLength";
			this.m_tbxMicroJointLength.Size = new System.Drawing.Size(100, 25);
			this.m_tbxMicroJointLength.TabIndex = 4;
			this.m_tbxMicroJointLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnTextBoxKeyDown);
			this.m_tbxMicroJointLength.Leave += new System.EventHandler(this.OnTextBoxLeave);
			// 
			// m_btnClearAll
			// 
			this.m_btnClearAll.Location = new System.Drawing.Point(13, 67);
			this.m_btnClearAll.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnClearAll.Name = "m_btnClearAll";
			this.m_btnClearAll.Size = new System.Drawing.Size(118, 33);
			this.m_btnClearAll.TabIndex = 5;
			this.m_btnClearAll.Text = "刪除所有微連";
			this.m_btnClearAll.UseVisualStyleBackColor = true;
			this.m_btnClearAll.Click += new System.EventHandler(this.m_btnClearAll_Click);
			// 
			// m_btnDelete
			// 
			this.m_btnDelete.Location = new System.Drawing.Point(139, 67);
			this.m_btnDelete.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnDelete.Name = "m_btnDelete";
			this.m_btnDelete.Size = new System.Drawing.Size(84, 33);
			this.m_btnDelete.TabIndex = 6;
			this.m_btnDelete.Text = "刪除";
			this.m_btnDelete.UseVisualStyleBackColor = true;
			this.m_btnDelete.Click += new System.EventHandler(this.m_btnDelete_Click);
			// 
			// MicroJointDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(318, 113);
			this.Controls.Add(this.m_btnDelete);
			this.Controls.Add(this.m_btnClearAll);
			this.Controls.Add(this.m_tbxMicroJointLength);
			this.Controls.Add(this.m_lblLength);
			this.Controls.Add(this.m_btnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "MicroJointDlg";
			this.Padding = new System.Windows.Forms.Padding(9);
			this.Text = "編輯微連";
			this.TopMost = true;
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Label m_lblLength;
		private System.Windows.Forms.TextBox m_tbxMicroJointLength;
		private System.Windows.Forms.Button m_btnClearAll;
		private System.Windows.Forms.Button m_btnDelete;
	}
}
