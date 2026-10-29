namespace MyCAM.Editor.Dialog
{
	partial class ExternalAxisDlg
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
			this.m_lblExternalAxis = new System.Windows.Forms.Label();
			this.m_tbxExternalAxis = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblExternalAxisUnit = new System.Windows.Forms.Label();
			this.m_btnSure = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxExternalAxis)).BeginInit();
			this.SuspendLayout();
			// 
			// m_lblExternalAxis
			// 
			this.m_lblExternalAxis.AutoSize = true;
			this.m_lblExternalAxis.Location = new System.Drawing.Point(25, 26);
			this.m_lblExternalAxis.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblExternalAxis.Name = "m_lblExternalAxis";
			this.m_lblExternalAxis.Size = new System.Drawing.Size(112, 15);
			this.m_lblExternalAxis.TabIndex = 0;
			this.m_lblExternalAxis.Text = "變位機角度(C):";
			// 
			// m_tbxExternalAxis
			// 
			this.m_tbxExternalAxis.DecimalPlaces = 3;
			this.m_tbxExternalAxis.Increment = new decimal(new int[] {
			1,
			0,
			0,
			0});
			this.m_tbxExternalAxis.Location = new System.Drawing.Point(147, 22);
			this.m_tbxExternalAxis.Margin = new System.Windows.Forms.Padding(4);
			this.m_tbxExternalAxis.Maximum = new decimal(new int[] {
			99999,
			0,
			0,
			0});
			this.m_tbxExternalAxis.Minimum = new decimal(new int[] {
			99999,
			0,
			0,
			-2147483648});
			this.m_tbxExternalAxis.Name = "m_tbxExternalAxis";
			this.m_tbxExternalAxis.Size = new System.Drawing.Size(123, 25);
			this.m_tbxExternalAxis.TabIndex = 1;
			this.m_tbxExternalAxis.DebouncedValueChanged += new System.EventHandler(this.m_tbxExternalAxis_DebouncedValueChanged);
			// 
			// m_lblExternalAxisUnit
			// 
			this.m_lblExternalAxisUnit.AutoSize = true;
			this.m_lblExternalAxisUnit.Location = new System.Drawing.Point(279, 26);
			this.m_lblExternalAxisUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblExternalAxisUnit.Name = "m_lblExternalAxisUnit";
			this.m_lblExternalAxisUnit.Size = new System.Drawing.Size(23, 15);
			this.m_lblExternalAxisUnit.TabIndex = 2;
			this.m_lblExternalAxisUnit.Text = "deg";
			// 
			// m_btnSure
			// 
			this.m_btnSure.Location = new System.Drawing.Point(251, 65);
			this.m_btnSure.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnSure.Name = "m_btnSure";
			this.m_btnSure.Size = new System.Drawing.Size(85, 30);
			this.m_btnSure.TabIndex = 3;
			this.m_btnSure.Text = "OK";
			this.m_btnSure.UseVisualStyleBackColor = true;
			this.m_btnSure.Click += new System.EventHandler(this.m_btnSure_Click);
			// 
			// ExternalAxisDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(353, 118);
			this.Controls.Add(this.m_btnSure);
			this.Controls.Add(this.m_lblExternalAxisUnit);
			this.Controls.Add(this.m_tbxExternalAxis);
			this.Controls.Add(this.m_lblExternalAxis);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "ExternalAxisDlg";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "變位機";
			((System.ComponentModel.ISupportInitialize)(this.m_tbxExternalAxis)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label m_lblExternalAxis;
		private MyCAM.Editor.DebouncedNumericUpDown m_tbxExternalAxis;
		private System.Windows.Forms.Label m_lblExternalAxisUnit;
		private System.Windows.Forms.Button m_btnSure;
	}
}
