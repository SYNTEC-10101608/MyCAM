using MyCAM.CAD;
using MyCAM.CAM;
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

			// this is to keep highlighted selected node when tree view looses focus
			m_TreeView.HideSelection = false;

			// show G54
			ShowG54Trihedron();

			// view manager
			m_ViewManager = new ViewManager();

			// CAD manager
			m_CADManager = new CADManager();

			// CAD Editor
			m_CADEditor = new CADEditor( m_Viewer, m_TreeView, m_CADManager, m_ViewManager );
			m_CADEditor.EditStart();

			// CAM Editor
			m_CAMEditor = new CAMEditor( m_Viewer, m_TreeView, m_CADManager, m_ViewManager );

			// init menu strip
			m_msCAM.Enabled = false;
		}

		// view properties
		Viewer m_Viewer;
		ViewManager m_ViewManager;

		// data manager
		CADManager m_CADManager;

		// editors
		CADEditor m_CADEditor;
		CAMEditor m_CAMEditor;

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

		void m_tsmiTransformOK_Click( object sender, EventArgs e )
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

		// go to CAM editor
		void m_tsmiCAM_Click( object sender, EventArgs e )
		{
			m_msCAM.Enabled = true;
			m_msCAD.Enabled = false;
			m_CADEditor.EditEnd();
			m_CAMEditor.EditStart();
		}

		// add path
		void m_tsmiAddPath_Click( object sender, EventArgs e )
		{
			m_msCAD_SelectFace.Visible = true;
			m_msCAM.Enabled = false;
			m_CAMEditor.StartSelectFace();
		}

		void m_tsmiSelectD1ContFace_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SelectD1ContFace();
		}

		void m_tsmiSelPath_FreeBound_Click( object sender, EventArgs e )
		{
			m_msCAD_SelectFace.Visible = false;
			m_msCAM.Enabled = true;
			m_CAMEditor.SelectPath_FreeBound();
		}

		void m_tsmiSelPath_Manual_Click( object sender, EventArgs e )
		{
			m_msCAD_ManualSelectPath.Visible = true;
			m_msCAD_SelectFace.Visible = false;
			m_CAMEditor.StartSelectPath_Manual();
		}

		void m_tsmiManualSelectPathOK_Click( object sender, EventArgs e )
		{
			m_msCAD_ManualSelectPath.Visible = false;
			m_msCAM.Enabled = true;
			m_CAMEditor.EndSelectPath_Manual();
		}
	}
}
