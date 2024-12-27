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
			m_tbxX_Part.Text = m_dX_Part.ToString( "F3" );
			m_tbxY_Part.Text = m_dY_Part.ToString( "F3" );
			m_tbxZ_Part.Text = m_dZ_Part.ToString( "F3" );
			m_tbxXR_Part.Text = m_dXR_Part.ToString( "F3" );
			m_tbxYR_Part.Text = m_dYR_Part.ToString( "F3" );
			m_tbxZR_Part.Text = m_dZR_Part.ToString( "F3" );
			m_tbxX_G54.Text = m_dX_G54.ToString( "F3" );
			m_tbxY_G54.Text = m_dY_G54.ToString( "F3" );
			m_tbxZ_G54.Text = m_dZ_G54.ToString( "F3" );
			m_tbxXR_G54.Text = m_dXR_G54.ToString( "F3" );
			m_tbxYR_G54.Text = m_dYR_G54.ToString( "F3" );
			m_tbxZR_G54.Text = m_dZR_G54.ToString( "F3" );
		}

		public gp_Trsf TrsfPart => m_TrsfPart;
		public gp_Trsf TrsfG54 => m_TrsfG54;

		// part transform param
		double m_dX_Part = 0;
		double m_dY_Part = -80;
		double m_dZ_Part = -25;
		double m_dXR_Part = 0;
		double m_dYR_Part = 0;
		double m_dZR_Part = 0;

		// G54 transform param
		double m_dX_G54 = 0;
		double m_dY_G54 = -10;
		double m_dZ_G54 = -10;
		double m_dXR_G54 = 0;
		double m_dYR_G54 = 0;
		double m_dZR_G54 = 0;

		// transform matrix
		gp_Trsf m_TrsfPart = new gp_Trsf();
		gp_Trsf m_TrsfG54 = new gp_Trsf();

		void m_btnOK_Click( object sender, EventArgs e )
		{
			// part transform param
			m_dX_Part = double.TryParse( m_tbxX_Part.Text, out double dX_Part ) ? dX_Part : m_dX_Part;
			m_dY_Part = double.TryParse( m_tbxY_Part.Text, out double dY_Part ) ? dY_Part : m_dY_Part;
			m_dZ_Part = double.TryParse( m_tbxZ_Part.Text, out double dZ_Part ) ? dZ_Part : m_dZ_Part;
			m_dXR_Part = double.TryParse( m_tbxXR_Part.Text, out double dXR_Part ) ? dXR_Part : m_dXR_Part;
			m_dYR_Part = double.TryParse( m_tbxYR_Part.Text, out double dYR_Part ) ? dYR_Part : m_dYR_Part;
			m_dZR_Part = double.TryParse( m_tbxZR_Part.Text, out double dZR_Part ) ? dZR_Part : m_dZR_Part;

			// convert into radian
			double dXR_Part_Rad = m_dXR_Part * Math.PI / 180;
			double dYR_Part_Rad = m_dYR_Part * Math.PI / 180;
			double dZR_Part_Rad = m_dZR_Part * Math.PI / 180;

			// make part transform matrix
			m_TrsfPart = TransformTool.GetCoordTrsf( m_dX_Part, m_dY_Part, m_dZ_Part, dXR_Part_Rad, dYR_Part_Rad, dZR_Part_Rad );

			// G54 transform param
			m_dX_G54 = double.TryParse( m_tbxX_G54.Text, out double dX_G54 ) ? dX_G54 : m_dX_G54;
			m_dY_G54 = double.TryParse( m_tbxY_G54.Text, out double dY_G54 ) ? dY_G54 : m_dY_G54;
			m_dZ_G54 = double.TryParse( m_tbxZ_G54.Text, out double dZ_G54 ) ? dZ_G54 : m_dZ_G54;
			m_dXR_G54 = double.TryParse( m_tbxXR_G54.Text, out double dXR_G54 ) ? dXR_G54 : m_dXR_G54;
			m_dYR_G54 = double.TryParse( m_tbxYR_G54.Text, out double dYR_G54 ) ? dYR_G54 : m_dYR_G54;
			m_dZR_G54 = double.TryParse( m_tbxZR_G54.Text, out double dZR_G54 ) ? dZR_G54 : m_dZR_G54;

			// convert into radian
			double dXR_G54_Rad = m_dXR_G54 * Math.PI / 180;
			double dYR_G54_Rad = m_dYR_G54 * Math.PI / 180;
			double dZR_G54_Rad = m_dZR_G54 * Math.PI / 180;

			// make G54 transform matrix
			m_TrsfG54 = TransformTool.GetCoordTrsf( m_dX_G54, m_dY_G54, m_dZ_G54, dXR_G54_Rad, dYR_G54_Rad, dZR_G54_Rad );

			DialogResult = DialogResult.OK;
		}
	}
}
