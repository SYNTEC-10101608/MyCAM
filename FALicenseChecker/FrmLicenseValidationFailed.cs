using System.Windows.Forms;

namespace FALicenseChecker
{
	public partial class FrmLicenseValidationFailed : Form
	{
		public FrmLicenseValidationFailed()
		{
			InitializeComponent();
			ApplyResourcesToUI();
		}

		public enum EButtonShowMode
		{
			Retry = 0,
			RestartActivation,
		}

		public enum EHintID
		{
			LicenseCodeInvalid,
			LicenseFileInvalid,
			MachineMismatch,
			VersionMismatch,
		}

		public DialogResult ShowDialog( EButtonShowMode mode, EHintID hint )
		{
			// apply action button text; designer-time "Restart Activation" / "Retry" is kept as fallback when lookup misses
			switch( mode ) {
				case EButtonShowMode.Retry:
					m_btnAction.Text = m_LanguageManager.TryGetString( "ButtonText_Retry", m_btnAction.Text );
					break;
				case EButtonShowMode.RestartActivation:
					m_btnAction.Text = m_LanguageManager.TryGetString( "ButtonText_RestartActivation", m_btnAction.Text );
					break;
				default:
					break;
			}

			// apply hint label text; designer-time English hint is kept as fallback when lookup misses
			switch( hint ) {
				case EHintID.LicenseCodeInvalid:
					lblHint.Text = m_LanguageManager.TryGetString( "Hint_LicenseCodeInvalid", lblHint.Text );
					break;
				case EHintID.LicenseFileInvalid:
					lblHint.Text = m_LanguageManager.TryGetString( "Hint_LicenseFileInvalid", lblHint.Text );
					break;
				case EHintID.MachineMismatch:
					lblHint.Text = m_LanguageManager.TryGetString( "Hint_MachineMismatch", lblHint.Text );
					break;
				case EHintID.VersionMismatch:
					lblHint.Text = m_LanguageManager.TryGetString( "Hint_VersionMismatch", lblHint.Text );
					break;
				default:
					break;
			}
			return ShowDialog();
		}

		void ApplyResourcesToUI()
		{
			// form title and exit button; designer-time text is kept as fallback when lookup misses
			this.Text = m_LanguageManager.TryGetString( "FormTitle", this.Text );
			m_btnExit.Text = m_LanguageManager.TryGetString( "m_btnExit", m_btnExit.Text );
		}

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "FrmLicenseValidationFailed" );
	}
}
