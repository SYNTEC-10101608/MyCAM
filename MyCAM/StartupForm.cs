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
			Viewer viewer = new Viewer();
			bool bSucess = viewer.InitViewer( m_panViewer );
			if( !bSucess ) {
				MessageBox.Show( "Failed to initialize the viewer" );
				return;
			}
			Controls.Add( m_panViewer );
			m_panViewer.Dock = DockStyle.Fill;
		}

		Panel m_panViewer = new Panel();

		void m_tsmiImportBRep_Click( object sender, EventArgs e )
		{
		}

		void m_tsmiImportStep_Click( object sender, EventArgs e )
		{
		}

		void m_tsmiImportIges_Click( object sender, EventArgs e )
		{
		}
	}
}
