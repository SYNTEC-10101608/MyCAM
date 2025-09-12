namespace FALicenseChecker
{
	partial class FrmLicenseValidationFailed
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
			this.m_btnAction = new System.Windows.Forms.Button();
			this.m_btnExit = new System.Windows.Forms.Button();
			this.lblHint = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// m_btnAction
			// 
			this.m_btnAction.DialogResult = System.Windows.Forms.DialogResult.Retry;
			this.m_btnAction.Location = new System.Drawing.Point(13, 54);
			this.m_btnAction.Name = "m_btnAction";
			this.m_btnAction.Size = new System.Drawing.Size(135, 35);
			this.m_btnAction.TabIndex = 0;
			this.m_btnAction.Text = "Restart Activation";
			this.m_btnAction.UseVisualStyleBackColor = true;
			// 
			// m_btnExit
			// 
			this.m_btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnExit.Location = new System.Drawing.Point(154, 54);
			this.m_btnExit.Name = "m_btnExit";
			this.m_btnExit.Size = new System.Drawing.Size(135, 35);
			this.m_btnExit.TabIndex = 1;
			this.m_btnExit.Text = "Exit";
			this.m_btnExit.UseVisualStyleBackColor = true;
			// 
			// lblHint
			// 
			this.lblHint.Location = new System.Drawing.Point(12, 9);
			this.lblHint.Name = "lblHint";
			this.lblHint.Size = new System.Drawing.Size(277, 42);
			this.lblHint.TabIndex = 2;
			this.lblHint.Text = "The license key is invalid or not recognized. Please verify the entry or contact " +
    "support.";
			// 
			// FrmLicenseValidationFailed
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(301, 99);
			this.Controls.Add(this.lblHint);
			this.Controls.Add(this.m_btnExit);
			this.Controls.Add(this.m_btnAction);
			this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FrmLicenseValidationFailed";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "License Validation Failed";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button m_btnAction;
		private System.Windows.Forms.Button m_btnExit;
		private System.Windows.Forms.Label lblHint;
	}
}