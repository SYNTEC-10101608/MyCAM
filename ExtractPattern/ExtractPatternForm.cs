using OCC.AIS;
using OCC.Graphic3d;
using OCC.Prs3d;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ExtractPattern
{
	public partial class ExtractPatternForm : Form
	{
		public Action<TopoDS_Shape, List<TopoDS_Face>> ExtractOK;

		public ExtractPatternForm( TopoDS_Shape modelShape )
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

			// set AIS selction style
			Prs3d_Drawer d = m_OCCViewer.GetAISContext().HighlightStyle( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_LocalSelected );
			d.SetColor( new OCC.Quantity.Quantity_Color( OCC.Quantity.Quantity_NameOfColor.Quantity_NOC_RED ) );
			d.SetTransparency( 0.5f );
			d.SetDisplayMode( 1 );
			m_OCCViewer.GetAISContext().SetHighlightStyle( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_LocalSelected, d );

			// set model
			if( modelShape == null ) {
				return;
			}
			m_ModelShape = modelShape;
			ShowModel();

			// viewer action
			m_panViewer.MouseDown += ViewerMouseDown;
			m_panViewer.PreviewKeyDown += ViewerKeyDown;

			// TODO: panel key down does not work
			PreviewKeyDown += ViewerKeyDown;
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();

		// import model
		TopoDS_Shape m_ModelShape = null;

		void ShowModel()
		{
			// create AIS_Shape
			AIS_Shape aisShape = new AIS_Shape( m_ModelShape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			aisShape.SetMaterial( aspect );
			aisShape.SetDisplayMode( 1 );

			// display the shape
			m_OCCViewer.GetAISContext().RemoveAll( false );
			m_OCCViewer.GetAISContext().Display( aisShape, true );
			m_OCCViewer.AxoView();
			m_OCCViewer.ZoomAllView();

			// start face selection mode
			m_OCCViewer.GetAISContext().Activate( 4 /*face mode*/ );
		}

		// extract face
		void m_tsmiOK_Click( object sender, EventArgs e )
		{
			List<TopoDS_Face> extractedFaceList = GetSelectedFace();
			if( extractedFaceList.Count == 0 ) {
				return;
			}
			ExtractOK?.Invoke( m_ModelShape, extractedFaceList );
		}

		List<TopoDS_Face> GetSelectedFace()
		{
			List<TopoDS_Face> lstFace = new List<TopoDS_Face>();
			m_OCCViewer.GetAISContext().InitSelected();
			while( m_OCCViewer.GetAISContext().MoreSelected() ) {
				TopoDS_Shape theShape = m_OCCViewer.GetAISContext().SelectedShape();
				if( theShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_FACE ) {
					lstFace.Add( TopoDS.ToFace( theShape ) );
				}
				m_OCCViewer.GetAISContext().NextSelected();
			}
			return lstFace;
		}

		// viewer action
		void ViewerMouseDown( object sender, MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {

				// select the face
				m_OCCViewer.ShiftSelect();
			}
		}

		void ViewerKeyDown( object sender, PreviewKeyDownEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {

				// clear the selected face
				m_OCCViewer.GetAISContext().ClearSelected( true );
			}
		}
	}
}
