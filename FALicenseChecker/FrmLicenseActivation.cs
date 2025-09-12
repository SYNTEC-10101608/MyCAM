using System;
using System.Windows.Forms;

namespace FALicenseChecker
{
	public partial class FrmLicenseActivation : Form
	{
		public FrmLicenseActivation()
		{
			InitializeComponent();
			Text = "產品授權";
			m_lblHint.Text = "輸入授權碼";
			m_btnActivate.Text = "啟用";
		}

		public string LicenseCode
		{
			get
			{
				return m_txbCode.Text.Trim();
			}
			set
			{
				m_txbCode.Text = value;
			}
		}

		void m_btnActivate_Click( object sender, EventArgs e )
		{
			DialogResult = DialogResult.OK;
		}
	}
}
