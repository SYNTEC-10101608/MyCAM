namespace MyCAM.Editor.Dialog
{
    partial class GlassDXFImportDlg
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
			this.m_lblRadius = new System.Windows.Forms.Label();
			this.m_tbxRadius = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblHeight = new System.Windows.Forms.Label();
			this.m_tbxHeight = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxRadius)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxHeight)).BeginInit();
			this.SuspendLayout();
			// 
			// m_lblRadius
			// 
			this.m_lblRadius.AutoSize = true;
			this.m_lblRadius.Location = new System.Drawing.Point(27, 25);
			this.m_lblRadius.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblRadius.Name = "m_lblRadius";
			this.m_lblRadius.Size = new System.Drawing.Size(127, 15);
			this.m_lblRadius.TabIndex = 0;
			this.m_lblRadius.Text = "圓球半徑 (R | mm):";
			// 
			// m_tbxRadius
			// 
			this.m_tbxRadius.DecimalPlaces = 3;
			this.m_tbxRadius.Location = new System.Drawing.Point(213, 22);
			this.m_tbxRadius.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_tbxRadius.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.m_tbxRadius.Name = "m_tbxRadius";
			this.m_tbxRadius.Size = new System.Drawing.Size(133, 25);
			this.m_tbxRadius.TabIndex = 1;
			this.m_tbxRadius.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
			// 
			// m_lblHeight
			// 
			this.m_lblHeight.AutoSize = true;
			this.m_lblHeight.Location = new System.Drawing.Point(27, 75);
			this.m_lblHeight.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblHeight.Name = "m_lblHeight";
			this.m_lblHeight.Size = new System.Drawing.Size(128, 15);
			this.m_lblHeight.TabIndex = 2;
			this.m_lblHeight.Text = "曲面高度 (H | mm):";
			// 
			// m_tbxHeight
			// 
			this.m_tbxHeight.DecimalPlaces = 3;
			this.m_tbxHeight.Location = new System.Drawing.Point(213, 72);
			this.m_tbxHeight.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_tbxHeight.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_tbxHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.m_tbxHeight.Name = "m_tbxHeight";
			this.m_tbxHeight.Size = new System.Drawing.Size(133, 25);
			this.m_tbxHeight.TabIndex = 3;
			this.m_tbxHeight.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			// 
			// m_btnOK
			// 
			this.m_btnOK.Location = new System.Drawing.Point(67, 138);
			this.m_btnOK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(107, 38);
			this.m_btnOK.TabIndex = 4;
			this.m_btnOK.Text = "確定";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.BtnOK_Click);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Location = new System.Drawing.Point(213, 138);
			this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(107, 38);
			this.m_btnCancel.TabIndex = 5;
			this.m_btnCancel.Text = "取消";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// GlassDXFImportDlg
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(379, 201);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_tbxHeight);
			this.Controls.Add(this.m_lblHeight);
			this.Controls.Add(this.m_tbxRadius);
			this.Controls.Add(this.m_lblRadius);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GlassDXFImportDlg";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "玻璃曲面投影";
			((System.ComponentModel.ISupportInitialize)(this.m_tbxRadius)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxHeight)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label m_lblRadius;
        private MyCAM.Editor.DebouncedNumericUpDown m_tbxRadius;
        private System.Windows.Forms.Label m_lblHeight;
        private MyCAM.Editor.DebouncedNumericUpDown m_tbxHeight;
        private System.Windows.Forms.Button m_btnOK;
        private System.Windows.Forms.Button m_btnCancel;
    }
}
