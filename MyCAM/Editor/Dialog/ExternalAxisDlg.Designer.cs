namespace MyCAM.Editor
{
	partial class ExternalAxisDlg
	{
		void InitializeComponent()
		{
			m_lblHint = new System.Windows.Forms.Label();
			m_lblExt1 = new System.Windows.Forms.Label();
			m_lblExt2 = new System.Windows.Forms.Label();
			m_dbtnExt1 = new DebouncedNumericUpDown();
			m_dbtnExt2 = new DebouncedNumericUpDown();
			m_lblError = new System.Windows.Forms.Label();
			m_btnConfirm = new System.Windows.Forms.Button();
			m_btnCancel = new System.Windows.Forms.Button();
			SuspendLayout();
			m_lblHint.Location = new System.Drawing.Point( 12, 12 );
			m_lblHint.Size = new System.Drawing.Size( 396, 48 );
			m_lblHint.Text = "可切換單選／多選路徑；角度變更即套用。\r\nEnter 預覽；確定、取消或關閉皆保留已套用角度。";
			m_lblExt1.Location = new System.Drawing.Point( 12, 70 );
			m_lblExt1.Size = new System.Drawing.Size( 120, 24 );
			m_lblExt1.Text = "Ext1 (deg)";
			m_lblExt2.Location = new System.Drawing.Point( 12, 104 );
			m_lblExt2.Size = new System.Drawing.Size( 120, 24 );
			m_lblExt2.Text = "Ext2 (deg)";
			m_dbtnExt1.Location = new System.Drawing.Point( 140, 66 );
			m_dbtnExt1.Size = new System.Drawing.Size( 268, 25 );
			m_dbtnExt1.TabIndex = 0;
			m_dbtnExt2.Location = new System.Drawing.Point( 140, 100 );
			m_dbtnExt2.Size = new System.Drawing.Size( 268, 25 );
			m_dbtnExt2.TabIndex = 1;
			m_lblError.Location = new System.Drawing.Point( 12, 136 );
			m_lblError.Size = new System.Drawing.Size( 396, 46 );
			m_lblError.ForeColor = System.Drawing.Color.Firebrick;
			m_btnConfirm.Location = new System.Drawing.Point( 214, 188 );
			m_btnConfirm.Size = new System.Drawing.Size( 94, 30 );
			m_btnConfirm.Text = "確定";
			m_btnConfirm.TabIndex = 2;
			m_btnConfirm.Click += m_btnConfirm_Click;
			m_btnCancel.Location = new System.Drawing.Point( 314, 188 );
			m_btnCancel.Size = new System.Drawing.Size( 94, 30 );
			m_btnCancel.Text = "取消";
			m_btnCancel.TabIndex = 3;
			m_btnCancel.Click += m_btnCancel_Click;
			CancelButton = m_btnCancel;
			AutoScaleDimensions = new System.Drawing.SizeF( 8F, 15F );
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size( 420, 232 );
			Controls.AddRange( new System.Windows.Forms.Control[] { m_lblHint, m_lblExt1, m_lblExt2, m_dbtnExt1, m_dbtnExt2,
				m_lblError, m_btnConfirm, m_btnCancel } );
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "ExternalAxisDlg";
			Text = "變位機編程";
			ResumeLayout( false );
			PerformLayout();
		}

		System.Windows.Forms.Label m_lblHint;
		System.Windows.Forms.Label m_lblExt1;
		System.Windows.Forms.Label m_lblExt2;
		DebouncedNumericUpDown m_dbtnExt1;
		DebouncedNumericUpDown m_dbtnExt2;
		System.Windows.Forms.Label m_lblError;
		System.Windows.Forms.Button m_btnConfirm;
		System.Windows.Forms.Button m_btnCancel;
	}
}
