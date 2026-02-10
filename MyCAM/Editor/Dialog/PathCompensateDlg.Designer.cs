namespace MyCAM.Editor.Dialog
{
	partial class PathCompensateDlg
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
			this.m_lblCompensate = new System.Windows.Forms.Label();
			this.m_NumUpDownCompensate = new System.Windows.Forms.NumericUpDown();
			this.m_btnConfirm = new System.Windows.Forms.Button();
			this.m_btnClear = new System.Windows.Forms.Button();
			this.m_btnReset = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.m_NumUpDownCompensate)).BeginInit();
			this.SuspendLayout();
			// 
			// m_lblCompensate
			// 
			this.m_lblCompensate.AutoSize = true;
			this.m_lblCompensate.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblCompensate.Location = new System.Drawing.Point(12, 26);
			this.m_lblCompensate.Name = "m_lblCompensate";
			this.m_lblCompensate.Size = new System.Drawing.Size(87, 15);
			this.m_lblCompensate.TabIndex = 0;
			this.m_lblCompensate.Text = "補償距離(增量)";
			// 
			// m_NumUpDownCompensate
			// 
			this.m_NumUpDownCompensate.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_NumUpDownCompensate.Location = new System.Drawing.Point(105, 24);
			this.m_NumUpDownCompensate.Name = "m_NumUpDownCompensate";
			this.m_NumUpDownCompensate.Size = new System.Drawing.Size(146, 21);
			this.m_NumUpDownCompensate.TabIndex = 1;
			this.m_NumUpDownCompensate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.m_NumUpDownCompensate.Click += new System.EventHandler(this.m_NumUpDownCompensate_Click);
			this.m_NumUpDownCompensate.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_NumUpDownCompensate_KeyDown);
			this.m_NumUpDownCompensate.Leave += new System.EventHandler(this.m_NumUpDownCompensate_Leave);
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Location = new System.Drawing.Point(176, 65);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(75, 23);
			this.m_btnConfirm.TabIndex = 2;
			this.m_btnConfirm.Text = "確認";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// m_btnClear
			// 
			this.m_btnClear.Location = new System.Drawing.Point(96, 65);
			this.m_btnClear.Name = "m_btnClear";
			this.m_btnClear.Size = new System.Drawing.Size(75, 23);
			this.m_btnClear.TabIndex = 3;
			this.m_btnClear.Text = "清除";
			this.m_btnClear.UseVisualStyleBackColor = true;
			this.m_btnClear.Click += new System.EventHandler(this.m_btnClear_Click);
			// 
			// m_btnReset
			// 
			this.m_btnReset.Location = new System.Drawing.Point(15, 65);
			this.m_btnReset.Name = "m_btnReset";
			this.m_btnReset.Size = new System.Drawing.Size(75, 23);
			this.m_btnReset.TabIndex = 4;
			this.m_btnReset.Text = "重置";
			this.m_btnReset.UseVisualStyleBackColor = true;
			this.m_btnReset.Click += new System.EventHandler(this.m_btnReset_Click);
			// 
			// PathCompensateDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(274, 112);
			this.Controls.Add(this.m_btnReset);
			this.Controls.Add(this.m_btnClear);
			this.Controls.Add(this.m_btnConfirm);
			this.Controls.Add(this.m_NumUpDownCompensate);
			this.Controls.Add(this.m_lblCompensate);
			this.Name = "PathCompensateDlg";
			this.Text = "路徑補償";
			((System.ComponentModel.ISupportInitialize)(this.m_NumUpDownCompensate)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label m_lblCompensate;
		private System.Windows.Forms.NumericUpDown m_NumUpDownCompensate;
		private System.Windows.Forms.Button m_btnConfirm;
		private System.Windows.Forms.Button m_btnClear;
		private System.Windows.Forms.Button m_btnReset;
	}
}