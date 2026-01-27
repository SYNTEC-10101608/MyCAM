using System;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
{
	public partial class ConnectDlg : Form
	{
		public ConnectDlg()
		{
			InitializeComponent();
		}

		public event Action<string> ConfirmEvent;

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			string ip = m_txbIP.Text?.Trim();
			if( string.IsNullOrEmpty( ip ) || !IsValidIP( ip ) ) {
				m_lblTips.Text = "輸入格式錯誤，請以IPv4格式輸入";
				m_lblTips.ForeColor = System.Drawing.Color.Red;
				return;
			}

			ConfirmEvent.Invoke( ip );
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
	}
}
