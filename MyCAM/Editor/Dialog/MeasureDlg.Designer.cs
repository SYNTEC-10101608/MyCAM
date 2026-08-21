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
            this.m_lblMethod = new System.Windows.Forms.Label();
            this.m_cmbMethod = new System.Windows.Forms.ComboBox();
            this.m_lblGuide = new System.Windows.Forms.Label();
            this.m_lblResult = new System.Windows.Forms.Label();
            this.m_txtResult = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // m_lblMethod
            // 
            this.m_lblMethod.AutoSize = true;
            this.m_lblMethod.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lblMethod.Location = new System.Drawing.Point(20, 25);
            this.m_lblMethod.Name = "m_lblMethod";
            this.m_lblMethod.Size = new System.Drawing.Size(72, 17);
            this.m_lblMethod.TabIndex = 0;
            this.m_lblMethod.Text = "秖代よΑ";
            // 
            // m_cmbMethod
            // 
            this.m_cmbMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cmbMethod.Font = new System.Drawing.Font("Arial", 9F);
            this.m_cmbMethod.FormattingEnabled = true;
            this.m_cmbMethod.Location = new System.Drawing.Point(129, 22);
            this.m_cmbMethod.Name = "m_cmbMethod";
            this.m_cmbMethod.Size = new System.Drawing.Size(160, 25);
            this.m_cmbMethod.TabIndex = 1;
            this.m_cmbMethod.SelectedIndexChanged += new System.EventHandler(this.m_cmbMethod_SelectedIndexChanged);
            // 
            // m_lblGuide
            // 
            this.m_lblGuide.Font = new System.Drawing.Font("Arial", 9F);
            this.m_lblGuide.ForeColor = System.Drawing.Color.Gray;
            this.m_lblGuide.Location = new System.Drawing.Point(20, 57);
            this.m_lblGuide.Name = "m_lblGuide";
            this.m_lblGuide.Size = new System.Drawing.Size(269, 40);
            this.m_lblGuide.TabIndex = 2;
            this.m_lblGuide.Text = "叫匡拒秖代よΑ";
            // 
            // m_lblResult
            // 
            this.m_lblResult.AutoSize = true;
            this.m_lblResult.Font = new System.Drawing.Font("Arial", 9F);
            this.m_lblResult.Location = new System.Drawing.Point(20, 105);
            this.m_lblResult.Name = "m_lblResult";
            this.m_lblResult.Size = new System.Drawing.Size(72, 17);
            this.m_lblResult.TabIndex = 3;
            this.m_lblResult.Text = "秖代挡狦";
            // 
            // m_txtResult
            // 
            this.m_txtResult.Font = new System.Drawing.Font("Arial", 9F);
            this.m_txtResult.Location = new System.Drawing.Point(20, 128);
            this.m_txtResult.Multiline = true;
            this.m_txtResult.Name = "m_txtResult";
            this.m_txtResult.ReadOnly = true;
            this.m_txtResult.Size = new System.Drawing.Size(269, 70);
            this.m_txtResult.TabIndex = 4;
            // 
            // MeasureDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(310, 220);
            this.Controls.Add(this.m_lblMethod);
            this.Controls.Add(this.m_cmbMethod);
            this.Controls.Add(this.m_lblGuide);
            this.Controls.Add(this.m_lblResult);
            this.Controls.Add(this.m_txtResult);
            this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MeasureDlg";
            this.Text = "秖代";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label m_lblMethod;
        private System.Windows.Forms.ComboBox m_cmbMethod;
        private System.Windows.Forms.Label m_lblGuide;
        private System.Windows.Forms.Label m_lblResult;
        private System.Windows.Forms.TextBox m_txtResult;
    }
}
