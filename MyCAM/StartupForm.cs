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

		// add feature
		void m_tsmiAddPoint_AcrCenter_Click( object sender, EventArgs e )
		{
			m_CADEditor.AddPoint( AddPointType.CircArcCenter );
		}

		void m_tsmiAddPoint_EdgeCenter_Click( object sender, EventArgs e )
		{
			m_CADEditor.AddPoint( AddPointType.EdgeMidPoint );
		}

		// coordinate system
		void m_tsmiCreate3PCS_Click( object sender, EventArgs e )
		{
			m_CADEditor.Create3PCoordSystem();
		}

		void m_tsmiModifyCS_Click( object sender, EventArgs e )
		{
			throw new NotImplementedException( "Modify Coordinate System is not implemented yet." );
		}

		void m_tsmiTransform_Click( object sender, EventArgs e )
		{
			m_msCAD_Transform.Visible = true;

		}
	}
}
