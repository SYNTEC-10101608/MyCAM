using System;
using System.Windows.Forms;

namespace FALicenseChecker
{
	public partial class FrmLicenseActivation : Form
	{
		static readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "FrmLicenseActivation" );

		public FrmLicenseActivation()
		{
			InitializeComponent();

			// designer-time text is kept as fallback when lookup misses
			this.Text = m_LanguageManager.TryGetString( "FrmLicenseActivation_Title", this.Text );
			m_lblHint.Text = m_LanguageManager.TryGetString( "FrmLicenseActivation_Hint", m_lblHint.Text );
			m_btnActivate.Text = m_LanguageManager.TryGetString( "FrmLicenseActivation_ActivateButton", m_btnActivate.Text );
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
