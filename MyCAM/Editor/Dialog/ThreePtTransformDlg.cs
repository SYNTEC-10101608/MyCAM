using MyCAM.App;
using OCC.gp;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	// editdialog <input> is for preview and confirm, but no need input data here
	public partial class ThreePtTransformDlg : EditDialogBase<object>
	{
		public ThreePtTransformDlg( gp_Pnt p1, gp_Pnt p2, gp_Pnt p3 )
		{
			InitializeComponent();

			// init cad textbox
			tbxCADX1.Text = p1.X().ToString( "F3" );
			tbxCADY1.Text = p1.Y().ToString( "F3" );
			tbxCADZ1.Text = p1.Z().ToString( "F3" );

			tbxCADX2.Text = p2.X().ToString( "F3" );
			tbxCADY2.Text = p2.Y().ToString( "F3" );
			tbxCADZ2.Text = p2.Z().ToString( "F3" );

			tbxCADX3.Text = p3.X().ToString( "F3" );
			tbxCADY3.Text = p3.Y().ToString( "F3" );
			tbxCADZ3.Text = p3.Z().ToString( "F3" );
		}

		public bool GetMachinePoint( out gp_Pnt p1, out gp_Pnt p2, out gp_Pnt p3 )
		{
			if( m_MachineP1 == null || m_MachineP2 == null || m_MachineP3 == null ) {
				p1 = p2 = p3 = null;
				return false;
			}
			p1 = m_MachineP1;
			p2 = m_MachineP2;
			p3 = m_MachineP3;
			return true;
		}

		void btnOK_Click( object sender, EventArgs e )
		{
			// get machine point coordinates
			if( !double.TryParse( tbxMachineX1.Text, out double x1 ) ||
				!double.TryParse( tbxMachineY1.Text, out double y1 ) ||
				!double.TryParse( tbxMachineZ1.Text, out double z1 ) ||
				!double.TryParse( tbxMachineX2.Text, out double x2 ) ||
				!double.TryParse( tbxMachineY2.Text, out double y2 ) ||
				!double.TryParse( tbxMachineZ2.Text, out double z2 ) ||
				!double.TryParse( tbxMachineX3.Text, out double x3 ) ||
				!double.TryParse( tbxMachineY3.Text, out double y3 ) ||
				!double.TryParse( tbxMachineZ3.Text, out double z3 ) ) {
				MyApp.Logger.ShowOnLogPanel( $"機械點座標無效", MyApp.NoticeType.Warning );
				return;
			}

			// construct machine points
			m_MachineP1 = new gp_Pnt( x1, y1, z1 );
			m_MachineP2 = new gp_Pnt( x2, y2, z2 );
			m_MachineP3 = new gp_Pnt( x3, y3, z3 );

			// check if machine points are coincident
			if( m_MachineP1.IsEqual( m_MachineP2, 1e-3 ) ||
				m_MachineP1.IsEqual( m_MachineP3, 1e-3 ) ||
				m_MachineP2.IsEqual( m_MachineP3, 1e-3 ) ) {
				MyApp.Logger.ShowOnLogPanel( $"機械點不能重合", MyApp.NoticeType.Warning );
				return;
			}

			// check if machine points are collinear
			gp_Vec v12 = new gp_Vec( m_MachineP1, m_MachineP2 );
			gp_Vec v13 = new gp_Vec( m_MachineP1, m_MachineP3 );
			if( v12.IsParallel( v13, 1e-3 ) ) {
				MyApp.Logger.ShowOnLogPanel( $"機械點不能共線", MyApp.NoticeType.Warning );
				return;
			}

			// dialog result is OK
			DialogResult = DialogResult.OK;
			Close();
		}

		gp_Pnt m_MachineP1;
		gp_Pnt m_MachineP2;
		gp_Pnt m_MachineP3;
	}
}
