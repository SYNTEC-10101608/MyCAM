using OCC.gp;
using OCCTool;
using System;
using System.Windows.Forms;

namespace PartPlacement
{
	public partial class TransformParamForm : Form
	{
		public TransformParamForm()
		{
			InitializeComponent();

			// init text box
			m_tbxX_G54.Text = m_dX_G54.ToString( "F3" );
			m_tbxY_G54.Text = m_dY_G54.ToString( "F3" );
			m_tbxZ_G54.Text = m_dZ_G54.ToString( "F3" );
		}

		public gp_Trsf TrsfG54 => m_TrsfG54;

		// G54 transform param
		double m_dX_G54 = 0;
		double m_dY_G54 = -10;
		double m_dZ_G54 = -10;

		// transform matrix
		gp_Trsf m_TrsfG54 = new gp_Trsf();

		void m_btnOK_Click( object sender, EventArgs e )
		{
			// G54 transform param
			m_dX_G54 = double.TryParse( m_tbxX_G54.Text, out double dX_G54 ) ? dX_G54 : m_dX_G54;
			m_dY_G54 = double.TryParse( m_tbxY_G54.Text, out double dY_G54 ) ? dY_G54 : m_dY_G54;
			m_dZ_G54 = double.TryParse( m_tbxZ_G54.Text, out double dZ_G54 ) ? dZ_G54 : m_dZ_G54;

			// make G54 transform matrix
			m_TrsfG54 = TransformTool.GetCoordTrsf( m_dX_G54, m_dY_G54, m_dZ_G54, 0, 0, 0 );
			DialogResult = DialogResult.OK;
		}
	}
}
