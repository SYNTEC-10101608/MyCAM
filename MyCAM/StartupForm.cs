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

		// manual transform
		void m_tsmiManualTransform_Click( object sender, EventArgs e )
		{
			m_msCAD_Transform.Visible = true;
			m_msCAD.Enabled = false;
			m_CADEditor.StartManaulTransform();
		}

		void m_tsmiPlane_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( EConstraintType.Plane, false );
		}

		void m_tsmiPlane_R_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( EConstraintType.Plane, true );
		}

		void m_tsmiPlanePar_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( EConstraintType.PlaneParallel, false );
		}

		void m_tsmiPlanePar_R_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( EConstraintType.PlaneParallel, true );
		}

		void m_tsmiAxial_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( EConstraintType.Axial, false );
		}

		void m_tsmiAxial_R_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( EConstraintType.Axial, true );
		}

		void m_tsmiAxialPar_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( EConstraintType.AxialParallel, false );
		}

		void m_tsmiAxialPar_R_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( EConstraintType.AxialParallel, true );
		}

		void m_tsmiOK_Click( object sender, EventArgs e )
		{
			m_msCAD_Transform.Visible = false;
			m_msCAD.Enabled = true;
			m_CADEditor.EndManualTransform();
		}

		// 3 point transform
		void m_tsmi3PointTransform_Click( object sender, EventArgs e )
		{
			m_CADEditor.ThreePointTransform();
		}

		// select face
		void m_tsmiSelectFace_Click( object sender, EventArgs e )
		{
			m_msCAD_SelectFace.Visible = true;
			m_msCAD.Enabled = false;
			m_CADEditor.StartSelectFace();
		}

		void m_tsmiSelectD1ContFace_Click( object sender, EventArgs e )
		{
			m_CADEditor.SelectD1ContFace();
		}

		void m_tsmiSelectFaceOK_Click( object sender, EventArgs e )
		{
			m_msCAD_SelectFace.Visible = false;
			m_msCAD.Enabled = true;
			m_CADEditor.EndSelectFace();
		}
	}
}
