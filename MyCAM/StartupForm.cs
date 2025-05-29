using MyCAM.CAD;
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
			m_Viewer = new Viewer();
			bool bSucess = m_Viewer.InitViewer( m_panViewer );
			if( !bSucess ) {
				MessageBox.Show( ToString() + "Init Error: Init Viewer" );
				return;
			}
			m_Viewer.UpdateView();

			// CAD Editor
			m_CADEditor = new CADEditor( m_Viewer, m_TreeView );
		}

		// app properties
		Viewer m_Viewer;

		// CAD properties
		CADEditor m_CADEditor;

		// import part
		void m_tsmiImportBRep_Click( object sender, EventArgs e )
		{
			m_CADEditor.ImportFile( FileFormat.BREP );
		}

		void m_tsmiImportStep_Click( object sender, EventArgs e )
		{
			m_CADEditor.ImportFile( FileFormat.STEP );
		}

		void m_tsmiImportIges_Click( object sender, EventArgs e )
		{
			m_CADEditor.ImportFile( FileFormat.IGES );
		}
	}
}
