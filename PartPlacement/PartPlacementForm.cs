using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.BRepPrimAPI;
using OCC.Geom;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Quantity;
using OCC.TopoDS;
using OCCTool;
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
			m_panViewer.PreviewKeyDown += ViewerKeyDown;
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();

		// raw part
		TopoDS_Shape m_RawPartShape;

		// transform param
		TransformParamForm m_TransformParamForm = new TransformParamForm();

		// TODO: the machine frame does not need to refresh
		void ShowMachineFrame()
		{
			// make a cone for cutter
			BRepPrimAPI_MakeCone makeCone = new BRepPrimAPI_MakeCone( 0, 10, 50 );
			AIS_Shape cutterAIS = new AIS_Shape( makeCone.Shape() );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			cutterAIS.SetMaterial( aspect );
			cutterAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			cutterAIS.SetDisplayMode( (int)AISDisplayMode.AIS_Shaded );
			m_OCCViewer.GetAISContext().Display( cutterAIS, false );
			m_OCCViewer.GetAISContext().Deactivate( cutterAIS );
		}

		void ShowPart()
		{
			// transform the part
			BRepBuilderAPI_Transform partTransform = new BRepBuilderAPI_Transform( m_RawPartShape, m_TransformParamForm.TrsfPart, true );

			// create part AIS_Shape
			AIS_Shape partAIS = new AIS_Shape( partTransform.Shape() );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			partAIS.SetMaterial( aspect );
			partAIS.SetDisplayMode( (int)AISDisplayMode.AIS_Shaded );

			// get G54 from translation part of part transform
			gp_Ax2 G54 = new gp_Ax2( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ) );
			G54.Translate( new gp_Vec( m_TransformParamForm.TrsfPart.TranslationPart() ) );

			// make a Trihedron indicate G54
			AIS_Trihedron aisTrihedron = new AIS_Trihedron( new Geom_Axis2Placement( G54 ) );
			aisTrihedron.SetSize( 10 );

			// display the shape
			m_OCCViewer.GetAISContext().Display( partAIS, false );
			m_OCCViewer.GetAISContext().Deactivate( partAIS );
			m_OCCViewer.GetAISContext().Display( aisTrihedron, false );
			m_OCCViewer.GetAISContext().Deactivate( aisTrihedron );
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

		void m_tsmiSetPart_Click( object sender, EventArgs e )
		{
			m_TransformParamForm.ShowDialog();
			if( m_TransformParamForm.DialogResult != DialogResult.OK ) {
				return;
			}
			RefreshViewer();
		}

		void m_tsmiOK_Click( object sender, EventArgs e )
		{
			PlaceOK?.Invoke( m_TransformParamForm.TrsfPart );
		}

		// viewer action
		void ViewerMouseDown( object sender, MouseEventArgs e )
		{
			m_panViewer.Focus();
			if( e.Button == MouseButtons.Left ) {
			}
		}

		void ViewerKeyDown( object sender, PreviewKeyDownEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
			}
		}
	}
}
