using System;

namespace MyCAM.Editor
{
	public partial class SewPartDlg : EditDialogBase<double>
	{
		public SewPartDlg( double dSewTolerance )
		{
			InitializeComponent();
			m_numSewTol.Value = (decimal)dSewTolerance;
		}

		void m_btnOK_Click( object sender, EventArgs e )
		{
			double dTol = (double)m_numSewTol.Value;
			RaiseConfirm( dTol );
		}
	}
}
