using System;
using System.Windows.Forms;

namespace CAMEdit
{
	public partial class ToolVecForm : Form
	{
		public ToolVecForm( double angleA_deg, double angleB_deg )
		{
			InitializeComponent();

			// initialize textbox
			m_tbxAngleA.Text = angleA_deg.ToString();
			m_tbxAngleB.Text = angleB_deg.ToString();
		}

		public void GetAngleValue( out double angleA_deg, out double angleB_deg )
		{
			angleA_deg = m_AngleA_deg;
			angleB_deg = m_AngleB_deg;
		}

		void m_btnOK_Click( object sender, EventArgs e )
		{
			// get value from textbox
			if( !double.TryParse( m_tbxAngleA.Text, out m_AngleA_deg )
					|| !double.TryParse( m_tbxAngleB.Text, out m_AngleB_deg ) ) {
				MessageBox.Show( "invalid string" );
				return;
			}
			if( m_AngleA_deg <= -180 || m_AngleA_deg >= 180 ||
			   m_AngleB_deg <= -180 || m_AngleB_deg >= 180 ) {
				MessageBox.Show( "angle must be in range (-180, 180)" );
				return;
			}
			DialogResult = DialogResult.OK;
			Close();
		}

		double m_AngleA_deg;
		double m_AngleB_deg;
	}
}
