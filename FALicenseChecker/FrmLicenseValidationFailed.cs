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
			CADCAMVersionMismatch,
			CheckerVersionMismatch,
			LicenseExpired
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
					lblHint.Text = m_LanguageManager.TryGetString( "Hint_LicenseCodeInvalid", HINT_LicenseCodeInvalid );
					break;
				case EHintID.LicenseFileInvalid:
					lblHint.Text = m_LanguageManager.TryGetString( "Hint_LicenseFileInvalid", HINT_LicenseFileInvalid );
					break;
				case EHintID.MachineMismatch:
					lblHint.Text = m_LanguageManager.TryGetString( "Hint_MachineMismatch", HINT_MachineMismatch );
					break;
				case EHintID.CADCAMVersionMismatch:
					lblHint.Text = m_LanguageManager.TryGetString( "Hint_VersionMismatch", HINT_VersionMismatch );
					break;
				case EHintID.CheckerVersionMismatch:
					lblHint.Text = m_LanguageManager.TryGetString( "Hint_CheckerVersionMismatch", HINT_CheckerVersionMismatch );
					break;
				case EHintID.LicenseExpired:
					lblHint.Text = m_LanguageManager.TryGetString( "Hint_LicenseExpired", HINT_LicenseExpired );
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

		const string HINT_LicenseCodeInvalid = "授權碼無效或不被識別，請確認是否輸入正確或聯絡支援";
		const string HINT_LicenseFileInvalid = "授權資料已損毀或遭竄改，無法完成驗證";
		const string HINT_MachineMismatch = "此授權已綁定至其他電腦，偵測到本機硬體識別與授權不符";
		const string HINT_VersionMismatch = "當前軟體版本不符合授權要求";
		const string HINT_CheckerVersionMismatch = "授權檔版號不符";
		const string HINT_LicenseExpired = "授權已過期，請重新申請或聯絡支援";

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "FrmLicenseValidationFailed" );
	}
}
