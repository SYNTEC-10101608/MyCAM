using MyCAM.App;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	public partial class SewPartDlg : EditDialogBase<double>
	{
		public SewPartDlg( double dSewTolerance )
		{
			InitializeComponent();
			m_txbSewTol.Text = dSewTolerance.ToString();
		}

		void m_btnOK_Click( object sender, EventArgs e )
		{
			ValidateAndRaiseConfirm();
		}

		void m_txbSewTol_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				ValidateAndRaiseConfirm();
			}
		}

		void ValidateAndRaiseConfirm()
		{
			if( double.TryParse( m_txbSewTol.Text, out double dTol ) && dTol > 0 && dTol < double.MaxValue ) {
				RaiseConfirm( dTol );
			}
			else {
				MyApp.Logger.ShowOnLogPanel( "無效字串", MyApp.NoticeType.Warning );
			}
		}
	}
}
