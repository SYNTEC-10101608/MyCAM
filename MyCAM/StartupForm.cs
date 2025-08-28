using System;
using System.Windows.Forms;
using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor;
using OCC.AIS;
using OCC.Geom;
using OCC.gp;
using OCC.Quantity;
using OCCViewer;

namespace MyCAM
{
	public partial class StartupForm : Form
	{
		public StartupForm()
		{
			// app
			MyApp.MainForm = this;
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
			m_CADManager = new DataManager();

			// CAD Editor
			m_CADEditor = new CADEditor( m_Viewer, m_TreeView, m_CADManager, m_ViewManager );
			m_CADEditor.EditStart();
			m_CADEditor.AxisTransformActionStausChanged += OnAxisTransformActionStausChanged;

			// CAM Editor
			m_CAMEditor = new CAMEditor( m_Viewer, m_TreeView, m_CADManager, m_ViewManager );
			m_CAMEditor.LeadActionStatusChange += OnLeadSettingActionStatusChange;
			m_CAMEditor.CurrentPathWithLead += OnPathWithLead;
			m_CAMEditor.CurrentPathIsClosed += OnPathIsClose;
			m_CAMEditor.OverCutActionStatusChange += OnOverCutActionStatusChange;

			// init menu strip
			m_msCAM.Enabled = false;
		}

		// view properties
		Viewer m_Viewer;
		ViewManager m_ViewManager;

		// data manager
		DataManager m_CADManager;

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

		void m_tsmiAddPoint_PointCenter_Click( object sender, EventArgs e )
		{
			m_CADEditor.AddPoint( AddPointType.TwoVertexMidPoint );
		}

		void m_tsmiAddLine_TwoVertexConnect_Click( object sender, EventArgs e )
		{
			m_CADEditor.AddLine( AddLineType.TwoVertexConnectLine );
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
			m_CADEditor.ApplyManualTransform( EConstraintType.Plane );
		}

		void m_tsmiPlanePar_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( EConstraintType.PlaneParallel );
		}

		void m_tsmiAxial_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( EConstraintType.Axial );
		}

		void m_tsmiAxialPar_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( EConstraintType.AxialParallel );
		}

		void m_tsmiPointCoincide_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( EConstraintType.Point );
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

		// axis transform
		void m_tsmiAxisTransform_Click( object sender, EventArgs e )
		{
			m_CADEditor.StartAxisTransform();
		}

		void OnAxisTransformActionStausChanged( EFunctionStatus status )
		{
			if( status == EFunctionStatus.Open ) {
				m_msCAD.Enabled = false;
			}
			else if( status == EFunctionStatus.Close ) {
				m_msCAD.Enabled = true;
			}
		}

		// go to CAM editor
		void m_tsmiCAM_Click( object sender, EventArgs e )
		{
			m_msCAM.Enabled = true;
			m_msCAD.Enabled = false;
			m_CADEditor.EditEnd();
			m_CAMEditor.EditStart();
		}

		void m_tsmiCADOK_Click( object sender, EventArgs e )
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

		// remove path
		void m_tsmiRemovePath_Click( object sender, EventArgs e )
		{
			m_CAMEditor.RemovePath();
		}

		// CAM property
		void m_tsmiStartPoint_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetStartPoint();
		}

		void m_tsmiReverse_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetReverse();
		}

		void m_tsmiOverCut_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetOverCut();
		}

		void m_tsmiLeadSetting_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetLeadLine();
		}

		void m_tsmiChangeLeadDirection_Click( object sender, EventArgs e )
		{
			m_CAMEditor.ChangeLeadDirection();
		}

		// tool vector
		void m_tsmiToolVec_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetToolVec();
		}

		void m_tsmiToolVecReverse_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetToolVecReverse();
		}

		// sort
		void m_tsmiMoveUp_Click( object sender, EventArgs e )
		{
			m_CAMEditor.MoveProcess( true );
		}

		void m_tsmiMoveDown_Click( object sender, EventArgs e )
		{
			m_CAMEditor.MoveProcess( false );
		}

		void m_tsmiAutoSort_Click( object sender, EventArgs e )
		{
			m_CAMEditor.AutoSortProcess();
		}

		// back to CAD editor
		void m_tsmiBackToCAD_Click( object sender, EventArgs e )
		{
			m_msCAM.Enabled = false;
			m_msCAD.Enabled = true;
			m_CAMEditor.EditEnd();
			m_CADEditor.EditStart();
		}

		// convert NC
		void m_tsmiCAMOK_Click( object sender, EventArgs e )
		{
			m_CAMEditor.ConvertNC();
		}

		#region UI action 

		void OnLeadSettingActionStatusChange( EFunctionStatus actionStatus )
		{
			if( actionStatus == EFunctionStatus.Open ) {
				m_msCAM.Enabled = false;
				return;
			}
			m_msCAM.Enabled = true;
		}

		void OnOverCutActionStatusChange( EFunctionStatus actionStatus )
		{
			if( actionStatus == EFunctionStatus.Open ) {
				m_msCAM.Enabled = false;
				return;
			}
			m_msCAM.Enabled = true;
		}

		void OnPathWithLead( bool isPathWithLead )
		{
			m_tsmiChangeLeadDirection.Enabled = isPathWithLead;
		}

		void OnPathIsClose( bool isClosePath )
		{
			m_tsmiStartPoint.Enabled = isClosePath;
			m_tsmiSetLead.Enabled = isClosePath;
			m_tsmiOverCut.Enabled = isClosePath;
		}

		#endregion
	}
}
