using DataStructure;
using System;
using System.Windows.Forms;

namespace ProcessEdit
{
	public partial class TraverseForm : Form
	{
		public TraverseForm( TraverseProcessData traverseProcessData )
		{
			InitializeComponent();

			// set data
			m_dX = traverseProcessData.Point_MCS.X();
			m_dY = traverseProcessData.Point_MCS.Y();
			m_dZ = traverseProcessData.Point_MCS.Z();

			// init text box
			m_tbxX.Text = m_dX.ToString( "F3" );
			m_tbxY.Text = m_dY.ToString( "F3" );
			m_tbxZ.Text = m_dZ.ToString( "F3" );
			m_tbxXR.Text = m_dZR.ToString( "F3" );
			m_tbxYR.Text = m_dYR.ToString( "F3" );
			m_tbxZR.Text = m_dZR.ToString( "F3" );
		}

		public double X => m_dX;
		public double Y => m_dY;
		public double Z => m_dZ;
		public double XR => m_dXR;
		public double YR => m_dYR;
		public double ZR => m_dZR;

		TraverseProcessData m_TraverseProcessData;

		// data
		double m_dX = 0;
		double m_dY = 0;
		double m_dZ = 0;
		double m_dXR = 0;
		double m_dYR = 0;
		double m_dZR = 0;

		void m_btnOK_Click( object sender, EventArgs e )
		{
			// traverse process data
			m_dX = double.TryParse( m_tbxX.Text, out double dX ) ? dX : m_dX;
			m_dY = double.TryParse( m_tbxY.Text, out double dY ) ? dY : m_dY;
			m_dZ = double.TryParse( m_tbxZ.Text, out double dZ ) ? dZ : m_dZ;
			m_dXR = double.TryParse( m_tbxXR.Text, out double dXR ) ? dXR : m_dXR;
			m_dYR = double.TryParse( m_tbxYR.Text, out double dYR ) ? dYR : m_dYR;
			m_dZR = double.TryParse( m_tbxZR.Text, out double dZR ) ? dZR : m_dZR;
			DialogResult = DialogResult.OK;
		}
	}
}
