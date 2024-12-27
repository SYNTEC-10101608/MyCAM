using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.BRepPrimAPI;
using OCC.Geom;
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
		public Action<TopoDS_Shape> ImportOK;

		public PartPlacementForm( TopoDS_Shape modelShape )
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

			// show machine frame
			ShowMachineFrame();

			// show model
			m_RawModelShape = modelShape;
			if( m_RawModelShape == null ) {
				return;
			}
			ShowModel();
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();

		// import model
		TopoDS_Shape m_RawModelShape;

		// transform param
		TransformParamForm m_TransformParamForm = new TransformParamForm();

		void ShowMachineFrame()
		{
			// make a box for machine frame
			const int nSize = 500;
			gp_Pnt negPole = new gp_Pnt( -nSize / 2, -nSize / 2, -nSize / 2 );
			BRepPrimAPI_MakeBox makeBox = new BRepPrimAPI_MakeBox( negPole, nSize, nSize, nSize );
			AIS_Shape machineFrameAIS = new AIS_Shape( makeBox.Shape() );
			machineFrameAIS.SetDisplayMode( 0 );

			// make a cone for cutter
			BRepPrimAPI_MakeCone makeCone = new BRepPrimAPI_MakeCone( 0, 10, 50 );
			AIS_Shape cutterAIS = new AIS_Shape( makeCone.Shape() );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			cutterAIS.SetMaterial( aspect );
			cutterAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			cutterAIS.SetDisplayMode( 1 );

			//m_OCCViewer.GetAISContext().Display( machineFrameAIS, false );
			m_OCCViewer.GetAISContext().Display( cutterAIS, false );
			m_OCCViewer.UpdateView();
		}

		void ShowModel()
		{
			// get the transform matrix
			gp_Trsf trsfPart = m_TransformParamForm.TrsfPart;
			gp_Trsf trsfG54 = m_TransformParamForm.TrsfG54;
			gp_Trsf trsfPartG54 = trsfPart.Multiplied( trsfG54 );

			// transform the model
			BRepBuilderAPI_Transform partTransform = new BRepBuilderAPI_Transform( m_RawModelShape, trsfPartG54, true );

			// create model AIS_Shape
			AIS_Shape aisShape = new AIS_Shape( partTransform.Shape() );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			aisShape.SetMaterial( aspect );
			aisShape.SetDisplayMode( 1 );

			// make G54 coordinate
			gp_Ax2 G54 = new gp_Ax2( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ) );
			G54.Transform( trsfG54 );

			// make a Trihedron indicate G54
			AIS_Trihedron aisTrihedron = new AIS_Trihedron( new Geom_Axis2Placement( G54 ) );
			aisTrihedron.SetSize( 10 );

			// display the shape
			m_OCCViewer.GetAISContext().Display( aisShape, false );
			m_OCCViewer.GetAISContext().Display( aisTrihedron, false );
			m_OCCViewer.UpdateView();
			m_OCCViewer.AxoView();
			m_OCCViewer.ZoomAllView();
		}

		void m_tsmiSetting_Click( object sender, EventArgs e )
		{
			m_TransformParamForm.ShowDialog();
			if( m_TransformParamForm.DialogResult != DialogResult.OK ) {
				return;
			}

			// refresh the viewer
			m_OCCViewer.GetAISContext().RemoveAll( false );
			ShowMachineFrame();
			ShowModel();
		}
	}
}
