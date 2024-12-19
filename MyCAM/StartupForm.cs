using ImportExport;
using OCC.AIS;
using OCC.Graphic3d;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM
{
	public partial class StartupForm : Form
	{
		public StartupForm()
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

			// viewer action
			m_panViewer.MouseDown += ViewerMouseDown;
			m_panViewer.PreviewKeyDown += ViewerKeyDown;

			// init menu
			m_tsmiExtractFace.Enabled = false;
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();

		// import model
		TopoDS_Shape m_ModelShape = null;

		// import model
		void m_tsmiImportBRep_Click( object sender, EventArgs e )
		{
			ImportModel( ModelFormat.BREP );
		}

		void m_tsmiImportStep_Click( object sender, EventArgs e )
		{
			ImportModel( ModelFormat.STEP );
		}

		void m_tsmiImportIges_Click( object sender, EventArgs e )
		{
			ImportModel( ModelFormat.IGES );
		}

		void ImportModel( ModelFormat format )
		{
			ImportHandler.ImportModel( format, out TopoDS_Shape theShape );
			if( theShape == null ) {
				MessageBox.Show( ToString() + "Error: Import Model" );
				return;
			}

			// sew the shape
			theShape = Sew.SewShape( new List<TopoDS_Shape>() { theShape } );
			m_ModelShape = theShape;
			ShowModel( theShape );
			m_tsmiExtractFace.Enabled = true;
		}

		void ShowModel( TopoDS_Shape theShape )
		{
			// create AIS_Shape
			AIS_Shape aisShape = new AIS_Shape( theShape );
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
			m_panViewer.Focus();
		}

		// extract face
		void m_tsmiExtractFace_Click( object sender, EventArgs e )
		{
			List<TopoDS_Face> extractedFaceList = GetSelectedFace();
			if( extractedFaceList.Count == 0 ) {
				return;
			}
			CAMEditModel model = new CAMEditModel();
			if( model.Init( m_ModelShape, extractedFaceList ) == false ) {
				return;
			}
			CAMEditForm camEditForm = new CAMEditForm();
			if( camEditForm.Init( model ) == false ) {
				return;
			}
			camEditForm.ShowDialog();
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
				m_OCCViewer.GetAISContext().ClearSelected( true );
			}
		}
	}
}
