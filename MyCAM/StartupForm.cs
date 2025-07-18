using MyCAM.CAD;
using OCC.AIS;
using OCC.Geom;
using OCC.gp;
using OCC.Quantity;
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
			ShowG54Trihedron();
			m_CADEditor.StartProcess += StartProcess;
			m_CADEditor.EndProcess += EndProcess;
		}

		// app properties
		Viewer m_Viewer;

		// CAD properties
		CADEditor m_CADEditor;

		void ShowG54Trihedron()
		{
			gp_Ax2 ax2 = new gp_Ax2();
			AIS_Trihedron trihedron = new AIS_Trihedron( new Geom_Axis2Placement( ax2 ) );
			trihedron.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY ) );
			trihedron.SetSize( 100.0 );
			trihedron.SetAxisColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY ) );
			trihedron.SetTextColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY ) );
			m_Viewer.GetAISContext().Display( trihedron, false );
			m_Viewer.GetAISContext().Deactivate( trihedron );
		}

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

		// select path
		void m_tsmiSelPath_AllFace_Click( object sender, EventArgs e )
		{
			m_CADEditor.SelectPath_AllFace();
		}

		void m_tsmiSelPath_SelFace_Click( object sender, EventArgs e )
		{

		}

		void m_tsmiSelPath_Manual_Click( object sender, EventArgs e )
		{
			m_msCAD_ManualSelectPath.Visible = true;
			m_msCAD.Enabled = false;
			m_CADEditor.StartSelectPath_Manual();
		}

		void m_tsmiManualSelectPathOK_Click( object sender, EventArgs e )
		{
			m_msCAD_ManualSelectPath.Visible = false;
			m_msCAD.Enabled = true;
			m_CADEditor.EndSelectPath_Manual();
		}

		// go to CAM
		void m_tsmiCADOK_Click( object sender, EventArgs e )
		{
			m_CADEditor.GoToCAM();
		}

		// process
		void StartProcess()
		{
			this.Enabled = false;
		}

		void EndProcess()
		{
			this.Enabled = true;
		}
	}
}
