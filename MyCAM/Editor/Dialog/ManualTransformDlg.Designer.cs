namespace MyCAM.Editor
{
	partial class ManualTransformDlg
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
			this.m_lblAxisPlane = new System.Windows.Forms.Label();
			this.m_cmbAxisPlane = new System.Windows.Forms.ComboBox();
			this.m_lblMoveStatus = new System.Windows.Forms.Label();
			this.m_lblGuide = new System.Windows.Forms.Label();
			this.m_lblError = new System.Windows.Forms.Label();
			this.m_btnConfirm = new System.Windows.Forms.Button();
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
			this.m_lblMethod.Text = "對齊方式";
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
			// m_lblAxisPlane
			// 
			this.m_lblAxisPlane.AutoSize = true;
			this.m_lblAxisPlane.Font = new System.Drawing.Font("Arial", 9F);
			this.m_lblAxisPlane.Location = new System.Drawing.Point(20, 89);
			this.m_lblAxisPlane.Name = "m_lblAxisPlane";
			this.m_lblAxisPlane.Size = new System.Drawing.Size(56, 17);
			this.m_lblAxisPlane.TabIndex = 6;
			this.m_lblAxisPlane.Text = "對齊點";
			this.m_lblAxisPlane.Visible = false;
			// 
			// m_cmbAxisPlane
			// 
			this.m_cmbAxisPlane.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbAxisPlane.Font = new System.Drawing.Font("Arial", 9F);
			this.m_cmbAxisPlane.FormattingEnabled = true;
			this.m_cmbAxisPlane.Location = new System.Drawing.Point(129, 86);
			this.m_cmbAxisPlane.Name = "m_cmbAxisPlane";
			this.m_cmbAxisPlane.Size = new System.Drawing.Size(160, 25);
			this.m_cmbAxisPlane.TabIndex = 7;
			this.m_cmbAxisPlane.Visible = false;
			this.m_cmbAxisPlane.SelectedIndexChanged += new System.EventHandler(this.m_cmbAxisPlane_SelectedIndexChanged);
			// 
			// m_lblMoveStatus
			// 
			this.m_lblMoveStatus.AutoSize = true;
			this.m_lblMoveStatus.Font = new System.Drawing.Font("Arial", 9F);
			this.m_lblMoveStatus.ForeColor = System.Drawing.Color.Gray;
			this.m_lblMoveStatus.Location = new System.Drawing.Point(21, 117);
			this.m_lblMoveStatus.Name = "m_lblMoveStatus";
			this.m_lblMoveStatus.Size = new System.Drawing.Size(54, 17);
			this.m_lblMoveStatus.TabIndex = 5;
			this.m_lblMoveStatus.Text = "✘ 工件";
			// 
			// m_lblGuide
			// 
			this.m_lblGuide.Font = new System.Drawing.Font("Arial", 9F);
			this.m_lblGuide.ForeColor = System.Drawing.Color.Gray;
			this.m_lblGuide.Location = new System.Drawing.Point(20, 57);
			this.m_lblGuide.Name = "m_lblGuide";
			this.m_lblGuide.Size = new System.Drawing.Size(269, 23);
			this.m_lblGuide.TabIndex = 2;
			this.m_lblGuide.Text = "請先選擇對齊方式";
			// 
			// m_lblError
			// 
			this.m_lblError.AutoSize = false;
			this.m_lblError.Font = new System.Drawing.Font( "Arial", 9F );
			this.m_lblError.ForeColor = System.Drawing.Color.Red;
			this.m_lblError.Location = new System.Drawing.Point( 20, 140 );
			this.m_lblError.Name = "m_lblError";
			this.m_lblError.Size = new System.Drawing.Size( 269, 17 );
			this.m_lblError.TabIndex = 8;
			this.m_lblError.Text = string.Empty;
			this.m_lblError.Visible = false;
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Enabled = false;
			this.m_btnConfirm.Font = new System.Drawing.Font("Arial", 9F);
			this.m_btnConfirm.Location = new System.Drawing.Point(219, 163);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(71, 29);
			this.m_btnConfirm.TabIndex = 3;
			this.m_btnConfirm.Text = "OK";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// ManualTransformDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 23F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(310, 205);
			this.Controls.Add(this.m_lblMethod);
			this.Controls.Add(this.m_cmbMethod);
			this.Controls.Add(this.m_lblAxisPlane);
			this.Controls.Add(this.m_cmbAxisPlane);
			this.Controls.Add(this.m_lblMoveStatus);
			this.Controls.Add(this.m_lblGuide);
			this.Controls.Add( this.m_lblError );
			this.Controls.Add(this.m_btnConfirm);
			this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ManualTransformDlg";
			this.Text = "工件對齊";
			this.TopMost = true;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label m_lblMethod;
		private System.Windows.Forms.ComboBox m_cmbMethod;
		private System.Windows.Forms.Label m_lblAxisPlane;
		private System.Windows.Forms.ComboBox m_cmbAxisPlane;
		private System.Windows.Forms.Label m_lblMoveStatus;
		private System.Windows.Forms.Label m_lblGuide;
		private System.Windows.Forms.Label m_lblError;
		private System.Windows.Forms.Button m_btnConfirm;
	}
}
