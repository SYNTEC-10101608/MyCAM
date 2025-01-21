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
	public enum Format
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

		// import part
		TopoDS_Shape m_PartShape = null;

		void m_tsmiImportBRep_Click( object sender, EventArgs e )
		{
			DoImport( Format.BREP );
		}

		void m_tsmiImportStep_Click( object sender, EventArgs e )
		{
			DoImport( Format.STEP );
		}

		void m_tsmiImportIges_Click( object sender, EventArgs e )
		{
			DoImport( Format.IGES );
		}

		void m_tsmiOK_Click( object sender, EventArgs e )
		{
			if( m_PartShape == null ) {
				return;
			}
			ImportOK?.Invoke( m_PartShape );
		}

		void DoImport( Format format )
		{
			OpenFileDialog openDialog = new OpenFileDialog();

			// file dialog filter
			string filter = "";
			switch( format ) {
				case Format.BREP:
					filter = "BREP Files (*.brep *.rle)|*.brep; *.rle";
					break;
				case Format.STEP:
					filter = "STEP Files (*.stp *.step)|*.stp; *.step";
					break;
				case Format.IGES:
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
				case Format.BREP:
					Reader = new XSControl_Reader();
					break;
				case Format.STEP:
					Reader = new STEPControl_Reader();
					break;
				case Format.IGES:
					Reader = new IGESControl_Reader();
					break;
				default:
					Reader = new XSControl_Reader();
					break;
			}
			IFSelect_ReturnStatus status = Reader.ReadFile( szFileName );

			// check the status
			if( status != IFSelect_ReturnStatus.IFSelect_RetDone ) {
				MessageBox.Show( ToString() + "Error: Import" );
				return;
			}
			Reader.TransferRoots();

			// prevent from empty shape or null shape
			if( Reader.NbShapes() == 0 ) {
				MessageBox.Show( ToString() + "Error: Import" );
				return;
			}
			TopoDS_Shape theShape = Reader.OneShape();
			if( theShape == null ) {
				MessageBox.Show( ToString() + "Error: Import" );
				return;
			}

			// sew the shape
			theShape = ShapeTool.SewShape( new List<TopoDS_Shape>() { theShape } );

			// show the part
			m_PartShape = theShape;
			ShowPart();

			// enable the OK button
			m_tsmiOK.Enabled = true;
		}

		void ShowPart()
		{
			// create AIS_Shape
			AIS_Shape aisShape = new AIS_Shape( m_PartShape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			aisShape.SetMaterial( aspect );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );

			// display the shape
			m_OCCViewer.GetAISContext().RemoveAll( false );
			m_OCCViewer.GetAISContext().Display( aisShape, true );
			m_OCCViewer.GetAISContext().Deactivate( aisShape );
			m_OCCViewer.AxoView();
			m_OCCViewer.ZoomAllView();
		}
	}
}
