using System.Windows.Forms;

namespace FALicenseChecker
{
	public partial class FrmLicenseValidationFailed : Form
	{
		public FrmLicenseValidationFailed()
		{
			InitializeComponent();

			Text = "授權驗證失敗";
			m_btnExit.Text = "結束程式";
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
			switch( mode ) {
				case EButtonShowMode.Retry:
					m_btnAction.Text = "重新輸入";
					break;
				case EButtonShowMode.RestartActivation:
					m_btnAction.Text = "重新進行產品授權";
					break;
				default:
					break;
			}

			switch( hint ) {
				case EHintID.LicenseCodeInvalid:
					lblHint.Text = "授權碼無效或不被識別，請確認是否輸入正確或聯絡支援";
					break;
				case EHintID.LicenseFileInvalid:
					lblHint.Text = "授權資料已損毀或遭竄改，無法完成驗證";
					break;
				case EHintID.MachineMismatch:
					lblHint.Text = "此授權已綁定至其他電腦，偵測到本機硬體識別與授權不符";
					break;
				case EHintID.VersionMismatch:
					lblHint.Text = "當前軟體版本不符合授權要求";
					break;
				default:
					break;
			}
			return ShowDialog();
		}
	}
}
