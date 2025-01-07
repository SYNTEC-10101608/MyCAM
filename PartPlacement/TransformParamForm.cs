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
		}

		public gp_Trsf TrsfPart => m_TrsfPart;

		// part transform param
		double m_dX_Part = 0;
		double m_dY_Part = -50;
		double m_dZ_Part = -25;
		double m_dXR_Part = 0;
		double m_dYR_Part = 0;
		double m_dZR_Part = 90;

		// transform matrix
		gp_Trsf m_TrsfPart = new gp_Trsf();

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
			DialogResult = DialogResult.OK;
		}
	}
}
