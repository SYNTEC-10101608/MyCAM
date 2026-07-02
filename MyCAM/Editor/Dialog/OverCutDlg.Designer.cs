namespace MyCAM.Editor.Dialog
{
	partial class OverCutDlg
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
			this.m_lblOverCutLength = new System.Windows.Forms.Label();
			this.m_tbxOverCutLength = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_OverCutUnit = new System.Windows.Forms.Label();
			this.m_btnSure = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxOverCutLength)).BeginInit();
			this.SuspendLayout();
			// 
			// m_lblOverCutLength
			// 
			this.m_lblOverCutLength.AutoSize = true;
			this.m_lblOverCutLength.Location = new System.Drawing.Point(25, 26);
			this.m_lblOverCutLength.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblOverCutLength.Name = "m_lblOverCutLength";
			this.m_lblOverCutLength.Size = new System.Drawing.Size(71, 15);
			this.m_lblOverCutLength.TabIndex = 0;
			this.m_lblOverCutLength.Text = "過切長度:";
			// 
			// m_tbxOverCutLength
			// 
			this.m_tbxOverCutLength.DecimalPlaces = 3;
			this.m_tbxOverCutLength.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.m_tbxOverCutLength.Location = new System.Drawing.Point(117, 22);
			this.m_tbxOverCutLength.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_tbxOverCutLength.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_tbxOverCutLength.Name = "m_tbxOverCutLength";
			this.m_tbxOverCutLength.Size = new System.Drawing.Size(123, 25);
			this.m_tbxOverCutLength.TabIndex = 1;
			this.m_tbxOverCutLength.DebouncedValueChanged += new System.EventHandler(this.m_tbxOverCutLength_DebouncedValueChanged);
			// 
			// m_OverCutUnit
			// 
			this.m_OverCutUnit.AutoSize = true;
			this.m_OverCutUnit.Location = new System.Drawing.Point(260, 26);
			this.m_OverCutUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_OverCutUnit.Name = "m_OverCutUnit";
			this.m_OverCutUnit.Size = new System.Drawing.Size(29, 15);
			this.m_OverCutUnit.TabIndex = 2;
			this.m_OverCutUnit.Text = "mm";
			// 
			// m_btnSure
			// 
			this.m_btnSure.Location = new System.Drawing.Point(251, 65);
			this.m_btnSure.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_btnSure.Name = "m_btnSure";
			this.m_btnSure.Size = new System.Drawing.Size(85, 30);
			this.m_btnSure.TabIndex = 3;
			this.m_btnSure.Text = "OK";
			this.m_btnSure.UseVisualStyleBackColor = true;
			this.m_btnSure.Click += new System.EventHandler(this.m_btnSure_Click);
			// 
			// OverCutDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(353, 118);
			this.Controls.Add(this.m_btnSure);
			this.Controls.Add(this.m_OverCutUnit);
			this.Controls.Add(this.m_tbxOverCutLength);
			this.Controls.Add(this.m_lblOverCutLength);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.Name = "OverCutDlg";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "過切";
			((System.ComponentModel.ISupportInitialize)(this.m_tbxOverCutLength)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label m_lblOverCutLength;
		private MyCAM.Editor.DebouncedNumericUpDown m_tbxOverCutLength;
		private System.Windows.Forms.Label m_OverCutUnit;
		private System.Windows.Forms.Button m_btnSure;
	}
}
