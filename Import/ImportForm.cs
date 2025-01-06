using OCC.AIS;
using OCC.Graphic3d;
using OCC.IFSelect;
using OCC.IGESControl;
using OCC.STEPControl;
using OCC.TopoDS;
using OCC.XSControl;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Import
{
	public enum ModelFormat
	{
		BREP = 0,
		STEP = 1,
		IGES = 2
	}

	public partial class ImportForm : Form
	{
		public Action<TopoDS_Shape> ImportOK;

		public ImportForm()
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

			// menu items
			m_tsmiOK.Enabled = false;
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();

		// import model
		TopoDS_Shape m_ModelShape = null;

		void m_tsmiImportBRep_Click( object sender, EventArgs e )
		{
			DoImport( ModelFormat.BREP );
		}

		void m_tsmiImportStep_Click( object sender, EventArgs e )
		{
			DoImport( ModelFormat.STEP );
		}

		void m_tsmiImportIges_Click( object sender, EventArgs e )
		{
			DoImport( ModelFormat.IGES );
		}

		void m_tsmiOK_Click( object sender, EventArgs e )
		{
			ImportOK?.Invoke( m_ModelShape );
		}

		void DoImport( ModelFormat format )
		{
			OpenFileDialog openDialog = new OpenFileDialog();

			// file dialog filter
			string filter = "";
			switch( format ) {
				case ModelFormat.BREP:
					filter = "BREP Files (*.brep *.rle)|*.brep; *.rle";
					break;
				case ModelFormat.STEP:
					filter = "STEP Files (*.stp *.step)|*.stp; *.step";
					break;
				case ModelFormat.IGES:
					filter = "IGES Files (*.igs *.iges)|*.igs; *.iges";
					break;
				default:
					break;
			}
			openDialog.Filter = filter + "|All files (*.*)|*.*";

			// show file dialog
			if( openDialog.ShowDialog() != DialogResult.OK ) {
				return;
			}

			// get the file name
			string szFileName = openDialog.FileName;
			if( string.IsNullOrEmpty( szFileName ) ) {
				return;
			}

			// read the file
			XSControl_Reader Reader;
			switch( format ) {
				case ModelFormat.BREP:
					Reader = new XSControl_Reader();
					break;
				case ModelFormat.STEP:
					Reader = new STEPControl_Reader();
					break;
				case ModelFormat.IGES:
					Reader = new IGESControl_Reader();
					break;
				default:
					Reader = new XSControl_Reader();
					break;
			}
			IFSelect_ReturnStatus status = Reader.ReadFile( szFileName );

			// check the status
			if( status != IFSelect_ReturnStatus.IFSelect_RetDone ) {
				MessageBox.Show( ToString() + "Error: Import Model" );
				return;
			}
			Reader.TransferRoots();

			// prevent from empty shape or null shape
			if( Reader.NbShapes() == 0 ) {
				MessageBox.Show( ToString() + "Error: Import Model" );
				return;
			}
			TopoDS_Shape theShape = Reader.OneShape();
			if( theShape == null ) {
				MessageBox.Show( ToString() + "Error: Import Model" );
				return;
			}

			// sew the shape
			theShape = ShapeTool.SewShape( new List<TopoDS_Shape>() { theShape } );

			// show the model
			m_ModelShape = theShape;
			ShowModel();

			// enable the OK button
			m_tsmiOK.Enabled = true;
		}

		void ShowModel()
		{
			// create AIS_Shape
			AIS_Shape aisShape = new AIS_Shape( m_ModelShape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			aisShape.SetMaterial( aspect );
			aisShape.SetDisplayMode( (int)AISDisplayMode.AIS_Shaded );

			// display the shape
			m_OCCViewer.GetAISContext().RemoveAll( false );
			m_OCCViewer.GetAISContext().Display( aisShape, true );
			m_OCCViewer.AxoView();
			m_OCCViewer.ZoomAllView();
		}
	}
}
