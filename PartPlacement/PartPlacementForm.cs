using OCC.AIS;
using OCC.BRepPrimAPI;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Quantity;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Windows.Forms;

namespace PartPlacement
{
	public partial class PartPlacementForm : Form
	{
		public Action<gp_Trsf> PlaceOK;

		public PartPlacementForm( TopoDS_Shape partShape )
		{
			InitializeComponent();

			// create the viewer
			bool bSucess = m_OCCViewer.InitViewer( m_panViewer );
			if( !bSucess ) {
				MessageBox.Show( ToString() + "Init Error: Init Viewer" );
				return;
			}
			Controls.Add( m_panViewer );
			m_panViewer.Dock = DockStyle.Fill;
			m_OCCViewer.UpdateView();

			// show part
			m_RawPartShape = partShape;
			if( m_RawPartShape == null ) {
				return;
			}
			RefreshViewer();

			// viewer action
			m_panViewer.MouseDown += ViewerMouseDown;
			m_panViewer.MouseUp += ViewerMouseUp;
			m_panViewer.PreviewKeyDown += ViewerKeyDown;
			m_panViewer.MouseMove += ViewerMouseMove;
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();

		// raw part
		TopoDS_Shape m_RawPartShape;

		// transform
		AIS_Manipulator m_Manipulator = new AIS_Manipulator();

		void ShowMachineFrame()
		{
			// make a cone for cutter
			BRepPrimAPI_MakeCone makeCone = new BRepPrimAPI_MakeCone( 0, 10, 50 );
			AIS_Shape cutterAIS = new AIS_Shape( makeCone.Shape() );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			cutterAIS.SetMaterial( aspect );
			cutterAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			cutterAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			m_OCCViewer.GetAISContext().Display( cutterAIS, false );
			m_OCCViewer.GetAISContext().Deactivate( cutterAIS );
		}

		void ShowPart()
		{
			// create part AIS_Shape
			AIS_Shape partAIS = new AIS_Shape( m_RawPartShape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			partAIS.SetMaterial( aspect );
			partAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );

			// display the shape
			m_OCCViewer.GetAISContext().Display( partAIS, false );
			m_OCCViewer.GetAISContext().Deactivate( partAIS );

			// create manipulator
			m_Manipulator.SetPart( AIS_ManipulatorMode.AIS_MM_Scaling, false );
			m_Manipulator.Attach( partAIS );
			m_Manipulator.SetModeActivationOnDetection( true );
		}

		void RefreshViewer()
		{
			m_OCCViewer.GetAISContext().RemoveAll( false );
			ShowPart();
			ShowMachineFrame();
			m_OCCViewer.UpdateView();
			m_OCCViewer.AxoView();
			m_OCCViewer.ZoomAllView();
		}

		void m_tsmiOK_Click( object sender, EventArgs e )
		{
			PlaceOK?.Invoke( m_Manipulator.LocalTransformation() );
		}

		// viewer action
		void ViewerMouseDown( object sender, MouseEventArgs e )
		{
			m_panViewer.Focus();
			if( e.Button == MouseButtons.Left ) {
				if( m_Manipulator.HasActiveMode() ) {
					m_Manipulator.StartTransform( e.X, e.Y, m_OCCViewer.GetView() );
					m_Manipulator.SetModeActivationOnDetection( false );
				}
			}
		}

		void ViewerMouseUp( object sender, MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {
				if( m_Manipulator.HasActiveMode() ) {
					m_Manipulator.StopTransform( true );
					m_Manipulator.SetModeActivationOnDetection( true );
				}
			}
		}

		void ViewerMouseMove( object sender, MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {
				if( m_Manipulator.HasActiveMode() ) {
					m_Manipulator.Transform( e.X, e.Y, m_OCCViewer.GetView() );
					m_OCCViewer.UpdateView();
				}
			}
		}

		void ViewerKeyDown( object sender, PreviewKeyDownEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
			}
		}
	}
}
