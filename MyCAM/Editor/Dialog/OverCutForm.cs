using System;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
{
	public partial class OverCutForm : Form
	{
		public Action Preview;
		public Action OnComfirm;
		public Action OnCancel;

		public OverCutForm( double overCutLength )
		{
			InitializeComponent();
			m_OverCutLength = overCutLength;
			m_tbxOverCutLength.Text = m_OverCutLength.ToString();
		}

		public double OverCutLength
		{
			get
			{
				return m_OverCutLength;
			}
		}

		double m_OverCutLength = 0;
		bool m_IsConfirmed = false;

		void m_btnSure_Click( object sender, EventArgs e )
		{
			if( IsValidParam() ) {
				m_IsConfirmed = true;
				OnComfirm?.Invoke();
				Close();
			}
		}

		void OverCutForm_FormClosing( object sender, FormClosingEventArgs e )
		{
			if( !m_IsConfirmed && e.CloseReason == CloseReason.UserClosing ) {
				OnCancel?.Invoke();
			}
		}

		void PreviewOverCutResult()
		{
			if( IsValidParam() ) {
				Preview?.Invoke();
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
	}
}
