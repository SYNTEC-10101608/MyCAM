using MyCAM.App;
using OCC.gp;
using System;

namespace MyCAM.Editor
{
	public struct TransParam
	{
		public gp_Pnt MachineP1
		{
			get; set;
		}

		public gp_Pnt MachineP2
		{
			get; set;
		}

		public gp_Pnt MachineP3
		{
			get; set;
		}

		public TransParam( gp_Pnt machineP1, gp_Pnt machineP2, gp_Pnt machineP3 )
		{
			MachineP1 = machineP1;
			MachineP2 = machineP2;
			MachineP3 = machineP3;
		}
	}

	public partial class ThreePtTransformDlg : EditDialogBase<TransParam>
	{
		public ThreePtTransformDlg( TransParam transParam )
		{
			InitializeComponent();

			// init cad textbox
			tbxCADX1.Text = transParam.MachineP1.X().ToString( "F3" );
			tbxCADY1.Text = transParam.MachineP1.Y().ToString( "F3" );
			tbxCADZ1.Text = transParam.MachineP1.Z().ToString( "F3" );

			tbxCADX2.Text = transParam.MachineP2.X().ToString( "F3" );
			tbxCADY2.Text = transParam.MachineP2.Y().ToString( "F3" );
			tbxCADZ2.Text = transParam.MachineP2.Z().ToString( "F3" );

			tbxCADX3.Text = transParam.MachineP3.X().ToString( "F3" );
			tbxCADY3.Text = transParam.MachineP3.Y().ToString( "F3" );
			tbxCADZ3.Text = transParam.MachineP3.Z().ToString( "F3" );
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
			RaiseConfirm( new TransParam( m_MachineP1, m_MachineP2, m_MachineP3 ) );
			Close();
		}

		gp_Pnt m_MachineP1;
		gp_Pnt m_MachineP2;
		gp_Pnt m_MachineP3;
	}
}
