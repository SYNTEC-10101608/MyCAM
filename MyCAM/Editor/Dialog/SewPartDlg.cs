using System;

namespace MyCAM.Editor
{
	public partial class SewPartDlg : EditDialogBase<double>
	{
		public SewPartDlg( double dSewTolerance )
		{
			InitializeComponent();
			m_txbSewTol.Value = (decimal)dSewTolerance;
			ApplyResourcesToUI();
		}

		void ApplyResourcesToUI()
		{
			this.Text = m_LanguageManager.TryGetString( "SewPartDlg", this.Text );
			m_lblSewTol.Text = m_LanguageManager.TryGetString( "m_lblSewTol", m_lblSewTol.Text );
			m_btnOK.Text = m_LanguageManager.TryGetString( "m_btnOK", m_btnOK.Text );
		}

		void m_btnOK_Click( object sender, EventArgs e )
		{
			double dTol = (double)m_txbSewTol.Value;
			RaiseConfirm( dTol );
		}

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "SewPartDlg" );
	}
}
