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
			m_txbSewTol.Value = (decimal)dSewTolerance;
		}

		void m_btnOK_Click( object sender, EventArgs e )
		{
			double dTol = (double)m_txbSewTol.Value;
			RaiseConfirm( dTol );
		}
	}
}
