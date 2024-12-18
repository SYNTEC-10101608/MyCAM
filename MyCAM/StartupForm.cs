using ImportExport;
using OCC.AIS;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Windows.Forms;

namespace MyCAM
{
	public partial class StartupForm : Form
	{
		public StartupForm()
		{
			InitializeComponent();

			// create the viewer
			bool bSucess = m_viewer.InitViewer( m_panViewer );
			if( !bSucess ) {
				MessageBox.Show( "Failed to initialize the viewer" );
				return;
			}
			Controls.Add( m_panViewer );
			m_panViewer.Dock = DockStyle.Fill;
		}

		Panel m_panViewer = new Panel();
		Viewer m_viewer = new Viewer();

		void m_tsmiImportBRep_Click( object sender, EventArgs e )
		{
			ImportHandler.ImportModel( ModelFormat.BREP, out TopoDS_Shape theShape );
			ShowImportedModel( theShape );
		}

		void m_tsmiImportStep_Click( object sender, EventArgs e )
		{
			ImportHandler.ImportModel( ModelFormat.STEP, out TopoDS_Shape theShape );
			ShowImportedModel( theShape );
		}

		void m_tsmiImportIges_Click( object sender, EventArgs e )
		{
			ImportHandler.ImportModel( ModelFormat.IGES, out TopoDS_Shape theShape );
			ShowImportedModel( theShape );
		}

		void ShowImportedModel( TopoDS_Shape theShape )
		{
			if( theShape == null ) {
				MessageBox.Show( "Failed to import the model" );
				return;
			}

			// create AIS_Shape
			AIS_Shape aisShape = new AIS_Shape( theShape );

			// display the shape
			m_viewer.GetAISContext().Display( aisShape, true );
		}
	}
}
