using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor;
using MyCAM.FileManager;
using MyCAM.Post;
using OCC.AIS;
using OCC.Geom;
using OCC.gp;
using OCC.Quantity;
using OCCViewer;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace MyCAM
{
	public partial class StartupForm : Form
	{
		public StartupForm()
		{
#if !DEBUG
			if( FALicenseChecker.LicenseChecker.IsLicenseActivated() == false ) {
				MessageBox.Show( "Authorization process is incorrect", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				Environment.Exit( 0 );
			}
#endif

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

			// CAD Manager
			if( GetMachineDataSuccess( out MachineData machineData ) ) {
				m_CADManager = new DataManager( machineData );
			}
			else {
				// get machine data fail, machine will be null(can't use)
				m_CADManager = new DataManager();
			}

			// CAD Editor
			m_CADEditor = new CADEditor( m_Viewer, m_TreeView, m_CADManager, m_ViewManager );
			m_CADEditor.EditStart();
			m_CADEditor.AxisTransformActionStausChanged += OnAxisTransformActionStausChanged;

			// CAM Editor
			m_CAMEditor = new CAMEditor( m_Viewer, m_TreeView, m_CADManager, m_ViewManager );
			m_CAMEditor.OverCutActionStatusChange += OnOverCutActionStatusChange;
			m_CAMEditor.LeadActionStatusChange += OnLeadSettingActionStatusChange;
			m_CAMEditor.PathPropertyChanged = OnCAMPathPropertyChanged;
			m_CAMEditor.TraversePrarmSettingActionStausChanged += OnTraverseSettingActionStausChanged;

			// simu editor
			m_SimuEditor = new SimuEditor( m_Viewer, m_TreeView, m_CADManager, m_ViewManager );

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
		SimuEditor m_SimuEditor;

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

		void m_tsmiOpenProjectFile_Click( object sender, EventArgs e )
		{
			m_CADEditor.ImportProjectFile();
		}

		void m_tsmiSaveProjectFile_Click( object sender, EventArgs e )
		{
			m_CADEditor.SaveProjectFile();
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

		// post parameter setting
		void m_tsmiTraverseParamSetting_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SeTraverseParam();
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
			NCWriter writer = new NCWriter( m_CADManager.GetCAMDataList(), m_CADManager.MachineData );
			writer.Convert();

			// simulation
			// m_CAMEditor.EditEnd();
			// m_SimuEditor.EditStart();
		}

		#region UI action 

		void OnLeadSettingActionStatusChange( EActionStatus actionStatus )
		{
			if( actionStatus == EActionStatus.Start ) {
				m_msCAM.Enabled = false;
				return;
			}
			m_msCAM.Enabled = true;
		}

		void OnOverCutActionStatusChange( EActionStatus actionStatus )
		{
			if( actionStatus == EActionStatus.Start ) {
				m_msCAM.Enabled = false;
				return;
			}
			m_msCAM.Enabled = true;
		}

		void OnCAMPathPropertyChanged( bool isClosePath, bool isPathWithLead )
		{
			OnPathIsCloseChanged( isClosePath );
			OnPathLeadChanged( isPathWithLead );
		}

		void OnPathIsCloseChanged( bool isClosePath )
		{
			m_tsmiStartPoint.Enabled = isClosePath;
			m_tsmiSetLead.Enabled = isClosePath;
			m_tsmiOverCut.Enabled = isClosePath;
		}

		void OnPathLeadChanged( bool isPathWithLead )
		{
			m_tsmiChangeLeadDirection.Enabled = isPathWithLead;
		}

		void OnTraverseSettingActionStausChanged( EActionStatus actionStatus )
		{
			if( actionStatus == EActionStatus.Start ) {
				m_msCAM.Enabled = false;
				return;
			}
			m_msCAM.Enabled = true;
		}

		void OnAxisTransformActionStausChanged( EActionStatus actionStatus )
		{
			if( actionStatus == EActionStatus.Start ) {
				m_msCAD.Enabled = false;
				return;
			}
			m_msCAD.Enabled = true;
		}
		#endregion

		#region Get machine data

		bool GetMachineDataSuccess( out MachineData machineData )
		{
			machineData = null;

			// get exe directory
			string exeDir = AppDomain.CurrentDomain.BaseDirectory;
			string filePath = Path.Combine( exeDir, "MachineData.mac" );
			if( !File.Exists( filePath ) ) {
				Console.WriteLine( $"檔案不存在" );
				return false;
			}
			try {
				XmlSerializer serializer = new XmlSerializer( typeof( MachineDataDTOContainer ) );

				// get DTO from xml file
				using( FileStream fileStream = new FileStream( filePath, FileMode.Open ) ) {
					MachineDataDTOContainer machineDataDTO = (MachineDataDTOContainer)serializer.Deserialize( fileStream );

					// turn back to MachineData
					machineData = MachineDataDTOManager.ToMachineData( machineDataDTO );
					return true;
				}
			}
			catch( Exception ex ) {
				Console.WriteLine( $"讀取專案檔案失敗：\n{ex.Message}", "錯誤" );
				return false;
			}
		}

		void SaveMachineData()
		{
			// default machine data
			MixTypeMachineData machineData = new MixTypeMachineData();
			machineData.ToolDirection = ToolDirection.Z;
			machineData.MasterRotaryAxis = RotaryAxis.Y;
			machineData.SlaveRotaryAxis = RotaryAxis.Z;
			machineData.MasterRotaryDirection = RotaryDirection.RightHand;
			machineData.SlaveRotaryDirection = RotaryDirection.LeftHand;
			machineData.MasterTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			machineData.SlaveTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			machineData.ToolLength = 2.0;
			machineData.ToolToMasterVec = new gp_Vec( 0, 101.2, 169.48 );
			machineData.MCSToSlaveVec = new gp_Vec( 40.81, -384.80, -665.67 );

			// get exe directory
			string exeDir = AppDomain.CurrentDomain.BaseDirectory;
			string filePath = Path.Combine( exeDir, "MachineData.mac" );

			// turn MachineData to DTOContainer
			MachineDataDTOContainer machinDataDTOContainer = MachineDataDTOManager.ToDTOContainer( machineData );
			if( machinDataDTOContainer == null ) {
				throw new ArgumentNullException( "MachineData 轉換成 DTOContainer 失敗。" );
			}
			XmlSerializer serializer = new XmlSerializer( typeof( MachineDataDTOContainer ) );

			// remove comment xmlns:xsd and xmlns:xsi
			XmlSerializerNamespaces xmlNameSpace = new XmlSerializerNamespaces();
			xmlNameSpace.Add( "", "" );

			// write DTO to xml file
			using( FileStream fileStream = new FileStream( filePath, FileMode.Create ) ) {
				serializer.Serialize( fileStream, machinDataDTOContainer, xmlNameSpace );
			}
		}

		#endregion
	}
}
