namespace MyCAM.Editor
{
	partial class ExternalAxisDlg
	{
		void InitializeComponent()
		{
			m_lblHint = new System.Windows.Forms.Label();
			m_lblExt1 = new System.Windows.Forms.Label();
			m_lblExt2 = new System.Windows.Forms.Label();
			m_txbExt1 = new System.Windows.Forms.TextBox();
			m_txbExt2 = new System.Windows.Forms.TextBox();
			m_lblPreviewPath = new System.Windows.Forms.Label();
			m_cbxPreviewPath = new System.Windows.Forms.ComboBox();
			m_lblError = new System.Windows.Forms.Label();
			m_btnPreview = new System.Windows.Forms.Button();
			m_btnConfirm = new System.Windows.Forms.Button();
			m_btnCancel = new System.Windows.Forms.Button();
			SuspendLayout();
			m_lblHint.Location = new System.Drawing.Point( 12, 12 );
			m_lblHint.Size = new System.Drawing.Size( 396, 48 );
			m_lblHint.Text = "角度套用至所有選取路徑；留白保留各路徑原值。\r\n預覽僅顯示下方指定路徑的加工起點姿態。";
			m_lblExt1.Location = new System.Drawing.Point( 12, 70 );
			m_lblExt1.Size = new System.Drawing.Size( 120, 24 );
			m_lblExt1.Text = "Ext1 (deg)";
			m_lblExt2.Location = new System.Drawing.Point( 12, 104 );
			m_lblExt2.Size = new System.Drawing.Size( 120, 24 );
			m_lblExt2.Text = "Ext2 (deg)";
			m_txbExt1.Location = new System.Drawing.Point( 140, 66 );
			m_txbExt1.Size = new System.Drawing.Size( 268, 25 );
			m_txbExt1.TabIndex = 0;
			m_txbExt2.Location = new System.Drawing.Point( 140, 100 );
			m_txbExt2.Size = new System.Drawing.Size( 268, 25 );
			m_txbExt2.TabIndex = 1;
			m_lblPreviewPath.Location = new System.Drawing.Point( 12, 140 );
			m_lblPreviewPath.Size = new System.Drawing.Size( 120, 24 );
			m_lblPreviewPath.Text = "預覽路徑";
			m_cbxPreviewPath.Location = new System.Drawing.Point( 140, 136 );
			m_cbxPreviewPath.Size = new System.Drawing.Size( 268, 25 );
			m_cbxPreviewPath.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			m_cbxPreviewPath.TabIndex = 2;
			m_lblError.Location = new System.Drawing.Point( 12, 174 );
			m_lblError.Size = new System.Drawing.Size( 396, 46 );
			m_lblError.ForeColor = System.Drawing.Color.Firebrick;
			m_btnPreview.Location = new System.Drawing.Point( 114, 226 );
			m_btnPreview.Size = new System.Drawing.Size( 94, 30 );
			m_btnPreview.Text = "預覽";
			m_btnPreview.TabIndex = 3;
			m_btnPreview.Click += m_btnPreview_Click;
			m_btnConfirm.Location = new System.Drawing.Point( 214, 226 );
			m_btnConfirm.Size = new System.Drawing.Size( 94, 30 );
			m_btnConfirm.Text = "確定";
			m_btnConfirm.TabIndex = 4;
			m_btnConfirm.Click += m_btnConfirm_Click;
			m_btnCancel.Location = new System.Drawing.Point( 314, 226 );
			m_btnCancel.Size = new System.Drawing.Size( 94, 30 );
			m_btnCancel.Text = "取消";
			m_btnCancel.TabIndex = 5;
			m_btnCancel.Click += m_btnCancel_Click;
			AcceptButton = m_btnConfirm;
			CancelButton = m_btnCancel;
			AutoScaleDimensions = new System.Drawing.SizeF( 8F, 15F );
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size( 420, 270 );
			Controls.AddRange( new System.Windows.Forms.Control[] { m_lblHint, m_lblExt1, m_lblExt2, m_txbExt1, m_txbExt2,
				m_lblPreviewPath, m_cbxPreviewPath, m_lblError, m_btnPreview, m_btnConfirm, m_btnCancel } );
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
		System.Windows.Forms.TextBox m_txbExt1;
		System.Windows.Forms.TextBox m_txbExt2;
		System.Windows.Forms.Label m_lblPreviewPath;
		System.Windows.Forms.ComboBox m_cbxPreviewPath;
		System.Windows.Forms.Label m_lblError;
		System.Windows.Forms.Button m_btnPreview;
		System.Windows.Forms.Button m_btnConfirm;
		System.Windows.Forms.Button m_btnCancel;
	}
}
