namespace MyCAM.Editor
{
    partial class RCornerDlg
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
            this.m_lblStatus = new System.Windows.Forms.Label();
            this.m_btnDelete = new System.Windows.Forms.Button();
            this.m_btnSwitchStartOrEnd = new System.Windows.Forms.Button();
            this.m_btnDone = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // m_lblStatus
            //
            this.m_lblStatus.AutoSize = true;
            this.m_lblStatus.Location = new System.Drawing.Point(14, 18);
            this.m_lblStatus.Name = "m_lblStatus";
            this.m_lblStatus.Size = new System.Drawing.Size(0, 15);
            this.m_lblStatus.TabIndex = 0;
            //
            // m_btnDelete
            //
            this.m_btnDelete.Location = new System.Drawing.Point(17, 55);
            this.m_btnDelete.Margin = new System.Windows.Forms.Padding(4);
            this.m_btnDelete.Name = "m_btnDelete";
            this.m_btnDelete.Size = new System.Drawing.Size(130, 33);
            this.m_btnDelete.TabIndex = 1;
            this.m_btnDelete.Text = "刪除R角工藝";
            this.m_btnDelete.UseVisualStyleBackColor = true;
            this.m_btnDelete.Click += new System.EventHandler(this.m_btnDelete_Click);
            //
            // m_btnSwitchStartOrEnd
            //
            this.m_btnSwitchStartOrEnd.Location = new System.Drawing.Point(155, 55);
            this.m_btnSwitchStartOrEnd.Margin = new System.Windows.Forms.Padding(4);
            this.m_btnSwitchStartOrEnd.Name = "m_btnSwitchStartOrEnd";
            this.m_btnSwitchStartOrEnd.Size = new System.Drawing.Size(130, 33);
            this.m_btnSwitchStartOrEnd.TabIndex = 2;
            this.m_btnSwitchStartOrEnd.Text = "切換起終點";
            this.m_btnSwitchStartOrEnd.UseVisualStyleBackColor = true;
            this.m_btnSwitchStartOrEnd.Click += new System.EventHandler(this.m_btnSwitchStartOrEnd_Click);
            //
            // m_btnDone
            //
            this.m_btnDone.Location = new System.Drawing.Point(155, 96);
            this.m_btnDone.Margin = new System.Windows.Forms.Padding(4);
            this.m_btnDone.Name = "m_btnDone";
            this.m_btnDone.Size = new System.Drawing.Size(130, 33);
            this.m_btnDone.TabIndex = 3;
            this.m_btnDone.Text = "完成";
            this.m_btnDone.UseVisualStyleBackColor = true;
            this.m_btnDone.Click += new System.EventHandler(this.m_btnDone_Click);
            //
            // RCornerDlg
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnDone;
            this.ClientSize = new System.Drawing.Size(302, 145);
            this.Controls.Add(this.m_btnDone);
            this.Controls.Add(this.m_btnSwitchStartOrEnd);
            this.Controls.Add(this.m_btnDelete);
            this.Controls.Add(this.m_lblStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "RCornerDlg";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.Text = "編輯R角";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label m_lblStatus;
        private System.Windows.Forms.Button m_btnDelete;
        private System.Windows.Forms.Button m_btnSwitchStartOrEnd;
        private System.Windows.Forms.Button m_btnDone;
    }
}
