using System;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
{
	public partial class OverCutDlg : ActionDialogBase<double>
	{
		public OverCutDlg( double overCutLength )
		{
			InitializeComponent();
			m_OverCutLength = overCutLength;
			m_tbxOverCutLength.Text = m_OverCutLength.ToString();
		}

		void m_tbxOverCutLength_Leave( object sender, EventArgs e )
		{
			PreviewOverCutResult();
		}

		void m_tbxOverCutLength_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				PreviewOverCutResult();
			}
		}

		void m_btnSure_Click( object sender, EventArgs e )
		{
			if( IsValidParam() ) {
				RaiseConfirm( m_OverCutLength );
			}
		}

		void PreviewOverCutResult()
		{
			if( IsValidParam() ) {
				RaisePreview( m_OverCutLength );
			}
		}

		bool IsValidParam()
		{
			if( !double.TryParse( m_tbxOverCutLength.Text, out double dOverCutLength ) ) {
				MessageBox.Show( "invalid string" );
				return false;
			}
			if( dOverCutLength < 0 ) {
				MessageBox.Show( "length must longer than 0" );
				return false;
			}
			m_OverCutLength = dOverCutLength;
			return true;
		}

		double m_OverCutLength = 0;
	}
}
