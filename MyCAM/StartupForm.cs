using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor;
using MyCAM.Helper;
using MyCAM.Post;
using OCC.AIS;
using OCC.Geom;
using OCC.gp;
using OCC.Quantity;
using OCCViewer;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

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
				m_DataManager = new DataManager
				{
					MachineData = machineData
				};
			}
			else {
				// get machine data fail, machine will be null(can't use)
				m_DataManager = new DataManager();
			}

			// CAD Editor
			m_CADEditor = new CADEditor( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			m_CADEditor.AxisTransformActionStausChanged += OnAxisTransformActionStausChanged;
			m_CADEditor.ManualTransformActionStausChanged += OnManualTransformStausChanged;

			// CAM Editor
			m_CAMEditor = new CAMEditor( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			m_CAMEditor.OverCutActionStatusChange += OnOverCutActionStatusChange;
			m_CAMEditor.LeadActionStatusChange += OnLeadSettingActionStatusChange;
			m_CAMEditor.PathPropertyChanged = OnCAMPathPropertyChanged;
			m_CAMEditor.TraversePrarmSettingActionStausChanged += OnTraverseSettingActionStausChanged;
			m_CAMEditor.SelectFaceActionStausChanged += OnSelectFaceActionStausChanged;
			m_CAMEditor.SelectPathActionStausChanged += OnSelectPathActionStausChanged;

			// simu editor
			m_SimuEditor = new SimuEditor( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );

			// start with CAD editor
			m_msCAD.Enabled = true;
			m_msCAM.Enabled = false;
			SwitchEditor( EEditorType.CAD );
		}

		// view properties
		Viewer m_Viewer;
		ViewManager m_ViewManager;

		// data manager
		DataManager m_DataManager;

		// editors
		CADEditor m_CADEditor;
		CAMEditor m_CAMEditor;
		SimuEditor m_SimuEditor;
		IEditor m_CurrentEditor;

		void ShowG54Trihedron()
		{
			gp_Ax2 ax2 = new gp_Ax2();
			AIS_Trihedron trihedron = new AIS_Trihedron( new Geom_Axis2Placement( ax2 ) );
			trihedron.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
			trihedron.SetSize( 100.0 );
			trihedron.SetAxisColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
			trihedron.SetTextColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
			trihedron.SetArrowColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
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

			m_CADEditor.StartManaulTransform();
		}

		void m_tsmiPlane_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( ETrsfConstraintType.Plane );
		}

		void m_tsmiPlanePar_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( ETrsfConstraintType.PlaneParallel );
		}

		void m_tsmiAxial_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( ETrsfConstraintType.Axial );
		}

		void m_tsmiAxialPar_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( ETrsfConstraintType.AxialParallel );
		}

		void m_tsmiPointCoincide_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( ETrsfConstraintType.Point );
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
			SwitchEditor( EEditorType.CAM );
		}

		// add path
		void m_tsmiAddPath_Click( object sender, EventArgs e )
		{
			m_CAMEditor.StartSelectFace();
		}

		void m_tsmiSelectD1ContFace_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SelectD1ContFace();
		}

		void m_tsmiSelPath_FreeBound_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SelectPath_FreeBound();
		}

		void m_tsmiSelPath_Manual_Click( object sender, EventArgs e )
		{
			m_CAMEditor.StartSelectPath_Manual();
		}

		void m_tsmiManualSelectPathOK_Click( object sender, EventArgs e )
		{
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
			SwitchEditor( EEditorType.CAD );
		}

		// convert NC
		void m_tsmiCAMOK_Click( object sender, EventArgs e )
		{
			NCWriter writer = new NCWriter( m_DataManager.GetCAMDataList(), m_DataManager.MachineData );
			writer.Convert();

			// simulation
			// m_CAMEditor.EditEnd();
			// m_SimuEditor.EditStart();
		}

		#region UI action 

		void OnManualTransformStausChanged( EActionStatus actionStatus )
		{
			if( actionStatus == EActionStatus.Start ) {
				m_msCAD_Transform.Visible = true;
				m_msCAD.Enabled = false;
				return;
			}
			m_msCAD_Transform.Visible = false;
			m_msCAD.Enabled = true;
		}

		void OnSelectFaceActionStausChanged( EActionStatus actionStatus )
		{
			if( actionStatus == EActionStatus.Start ) {
				m_msCAD_SelectFace.Visible = true;
				m_msCAM.Enabled = false;
				return;
			}
			m_msCAD_SelectFace.Visible = false;
			m_msCAM.Enabled = true;
		}

		void OnSelectPathActionStausChanged( EActionStatus actionStatus )
		{
			if( actionStatus == EActionStatus.Start ) {
				m_msCAD_ManualSelectPath.Visible = true;
				m_msCAD_SelectFace.Visible = false;
				m_msCAM.Enabled = false;
				return;
			}
			m_msCAD_ManualSelectPath.Visible = false;
			m_msCAM.Enabled = true;
		}

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

		// switch editor
		void SwitchEditor( EEditorType type )
		{
			// no current editor
			if( m_CurrentEditor == null ) {
				m_CurrentEditor = GetEditor( type );
				m_CurrentEditor?.EditStart();
				return;
			}
			else {
				// same editor
				if( m_CurrentEditor.Type == type ) {
					return;
				}

				// different editor
				m_CurrentEditor.EditEnd();
				m_CurrentEditor = GetEditor( type );
				m_CurrentEditor?.EditStart();
			}
		}

		IEditor GetEditor( EEditorType type )
		{
			switch( type ) {
				case EEditorType.CAD:
					return m_CADEditor;
				case EEditorType.CAM:
					return m_CAMEditor;
				case EEditorType.Simulation:
					return m_SimuEditor;
				default:
					return null;
			}
		}

		#region Get machine data

		bool GetMachineDataSuccess( out MachineData machineData )
		{
			machineData = null;

			// get exe directory
			string exeDir = AppDomain.CurrentDomain.BaseDirectory;
			string filePath = Path.Combine( exeDir, "MachineData.mac" );
			if( !File.Exists( filePath ) ) {
				return false;
			}
			try {
				// load machine data document
				XDocument machineDataDoc = XDocument.Load( filePath );

				// turn back to MachineData
				machineData = MachineDataXMLHelper.ConvertMachineDataFileToMachineData( machineDataDoc, out EMachineDataLoadStatus status, out int nErrorPrIndex );
				switch( status ) {
					case EMachineDataLoadStatus.NullMachineDataNode:
						MessageBox.Show( $"機構參數節點為空,使用默認機構參數", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error );
						return false;
					case EMachineDataLoadStatus.NullFile:
						MessageBox.Show( $"機構檔案為空,使用默認機構參數", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error );
						return false;
					case EMachineDataLoadStatus.InvalidPrValue:
						MessageBox.Show( $"機構參數錯誤(Pr:{nErrorPrIndex}),使用默認機構參數", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error );
						return false;
					case EMachineDataLoadStatus.TreeInvalid:
						MessageBox.Show( $"機構樹結構錯誤,使用默認機構樹", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error );
						return true;
					default:
						return true;
				}
			}
			catch( Exception ex ) {
				MessageBox.Show( $"讀取機構檔案失敗：\n{ex.Message},使用默認機構參數", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error );
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

			// build machine tree
			MachineTreeNode XNode = new MachineTreeNode( MachineComponentType.XAxis );
			MachineTreeNode YNode = new MachineTreeNode( MachineComponentType.YAxis );
			MachineTreeNode ZNode = new MachineTreeNode( MachineComponentType.ZAxis );
			MachineTreeNode MasterNode = new MachineTreeNode( MachineComponentType.Master );
			MachineTreeNode SlaveNode = new MachineTreeNode( MachineComponentType.Slave );
			MachineTreeNode ToolNode = new MachineTreeNode( MachineComponentType.Tool );
			MachineTreeNode WorkPieceNode = new MachineTreeNode( MachineComponentType.WorkPiece );

			machineData.RootNode.AddChild( YNode );
			YNode.AddChild( SlaveNode );
			SlaveNode.AddChild( WorkPieceNode );
			machineData.RootNode.AddChild( XNode );
			XNode.AddChild( ZNode );
			ZNode.AddChild( MasterNode );
			MasterNode.AddChild( ToolNode );


			/*
			SpindleTypeMachineData machineData = new SpindleTypeMachineData();
			machineData.ToolDirection = ToolDirection.Z;
			machineData.MasterRotaryAxis = RotaryAxis.Z;
			machineData.SlaveRotaryAxis = RotaryAxis.X;
			machineData.MasterRotaryDirection = RotaryDirection.RightHand;
			machineData.SlaveRotaryDirection = RotaryDirection.RightHand;
			machineData.MasterTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			machineData.SlaveTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			machineData.ToolLength = 2.0;
			machineData.ToolToSlaveVec = new gp_Vec( -101.20, -0.19, 169.43 );
			machineData.SlaveToMasterVec = new gp_Vec( -252.70, 0, 362.98 ) - machineData.ToolToSlaveVec;
			// build machine tree
			MachineTreeNode XNode = new MachineTreeNode( MachineComponentType.XAxis );
			MachineTreeNode YNode = new MachineTreeNode( MachineComponentType.YAxis );
			MachineTreeNode ZNode = new MachineTreeNode( MachineComponentType.ZAxis );
			MachineTreeNode MasterNode = new MachineTreeNode( MachineComponentType.Master );
			MachineTreeNode SlaveNode = new MachineTreeNode( MachineComponentType.Slave );
			MachineTreeNode ToolNode = new MachineTreeNode( MachineComponentType.Tool );
			MachineTreeNode WorkPieceNode = new MachineTreeNode( MachineComponentType.WorkPiece );
			machineData.RootNode.AddChild( XNode );
			XNode.AddChild( YNode );
			YNode.AddChild( ZNode );
			ZNode.AddChild( MasterNode );
			MasterNode.AddChild( SlaveNode );
			SlaveNode.AddChild( ToolNode );
			machineData.RootNode.AddChild( WorkPieceNode );
			*/

			/*
			TableTypeMachineData machineData = new TableTypeMachineData();
			machineData.ToolDirection = ToolDirection.Z;
			machineData.MasterRotaryAxis = RotaryAxis.Y;
			machineData.SlaveRotaryAxis = RotaryAxis.Z;
			machineData.MasterRotaryDirection = RotaryDirection.LeftHand;
			machineData.SlaveRotaryDirection = RotaryDirection.LeftHand;
			machineData.MasterTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			machineData.SlaveTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			machineData.ToolLength = 2.0;
			( machineData as TableTypeMachineData ).MCSToMasterVec = new gp_Vec( -80.51, 73.81, -129.55 );
			( machineData as TableTypeMachineData ).MasterToSlaveVec = new gp_Vec( -80.43, -71.67, -94.55 ) - ( machineData as TableTypeMachineData ).MCSToMasterVec;

			// build machine tree
			MachineTreeNode XNode = new MachineTreeNode( MachineComponentType.XAxis );
			MachineTreeNode YNode = new MachineTreeNode( MachineComponentType.YAxis );
			MachineTreeNode ZNode = new MachineTreeNode( MachineComponentType.ZAxis );
			MachineTreeNode MasterNode = new MachineTreeNode( MachineComponentType.Master );
			MachineTreeNode SlaveNode = new MachineTreeNode( MachineComponentType.Slave );
			MachineTreeNode ToolNode = new MachineTreeNode( MachineComponentType.Tool );
			MachineTreeNode WorkPieceNode = new MachineTreeNode( MachineComponentType.WorkPiece );
			machineData.RootNode.AddChild( YNode );
			YNode.AddChild( XNode );
			XNode.AddChild( MasterNode );
			MasterNode.AddChild( SlaveNode );
			SlaveNode.AddChild( WorkPieceNode );
			machineData.RootNode.AddChild( ZNode );
			ZNode.AddChild( ToolNode );
			*/

			// get exe directory
			string exeDir = AppDomain.CurrentDomain.BaseDirectory;
			string filePath = Path.Combine( exeDir, "MachineData.mac" );
			try {
				XDocument xmlDoc = MachineDataXMLHelper.ConvertMachineDataAndTree2XML( machineData );
				xmlDoc.Save( filePath );
			}
			catch( Exception ex ) {
				MessageBox.Show( $"儲存機構檔案失敗：\n{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}
	}

	#endregion
}

