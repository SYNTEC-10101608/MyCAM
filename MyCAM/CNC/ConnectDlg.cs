using System.Windows.Forms;
using CommonUI.Forms;
using MyCAM.App;

namespace MyCAM.Editor.Dialog
{
	public partial class ConnectDlg : DialogBase
	{

		public ConnectDlg()
		{
			InitializeComponent();
			m_txbIP.Text = MyApp.AppSetting.CNCIP;
			ApplyResourcesToUI();
		}

		public event System.Action<string> ConfirmEvent;

		void m_btnConfirm_Click( object sender, System.EventArgs e )
		{
			string ip = m_txbIP.Text?.Trim();
			if( string.IsNullOrEmpty( ip ) || !IsValidIP( ip ) ) {
				m_lblTips.Text = m_LanguageManager.TryGetString( "Error_InvalidIPFormat", m_lblTips.Text );
				m_lblTips.ForeColor = System.Drawing.Color.Red;
				return;
			}

			// save confirmed IP to setting file
			MyApp.AppSetting.ApplyCNCIP( ip );

			ConfirmEvent?.Invoke( ip );
		}

		bool IsValidIP( string ip )
		{
			if( string.IsNullOrWhiteSpace( ip ) ) {
				return false;
			}

			// Split the IP address by dots
			string[] parts = ip.Split( '.' );

			// IPv4 must have exactly 4 parts
			if( parts.Length != 4 ) {
				return false;
			}

			// Validate each octet
			foreach( string part in parts ) {
				// Check if the part is empty or contains non-numeric characters
				if( string.IsNullOrEmpty( part ) ) {
					return false;
				}

				// Check for leading zeros (except for "0" itself)
				if( part.Length > 1 && part[ 0 ] == '0' ) {
					return false;
				}

				// Try to parse the octet as an integer
				int octet;
				if( !int.TryParse( part, out octet ) ) {
					return false;
				}

				// Check if the octet is within valid range (0-255)
				if( octet < 0 || octet > 255 ) {
					return false;
				}
			}

			return true;
		}

		void ApplyResourcesToUI()
		{
			this.Text = m_LanguageManager.TryGetString( "FormTitle", this.Text );
			m_lblIP.Text = m_LanguageManager.TryGetString( "m_lblIP", m_lblIP.Text );
			m_btnConfirm.Text = m_LanguageManager.TryGetString( "m_btnConfirm", m_btnConfirm.Text );
			m_lblTips.Text = m_LanguageManager.TryGetString( "m_lblTips_Default", m_lblTips.Text );
		}

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ConnectDlg" );
	}
}
