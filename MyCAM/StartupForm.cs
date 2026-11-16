using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using CommonUI.Controls;
using CommonUI.Forms;
using CommonUI.Theme;
using CncManager;
using CncUI;
using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor;
using MyCAM.Editor.Dialog;
using MyCAM.Helper;
using MyCAM.Helper.FormShow;
using MyCAM.Layout;
using OCC.AIS;
using OCC.Geom;
using OCC.gp;
using OCC.Quantity;
using OCC.TopoDS;
using OCCViewer;
using TpCADCore;
using TpCADUI;
using VNCFormsApp;

namespace MyCAM
{
	public partial class StartupForm : MainWindowBase
	{
		public Action<bool> RaiseShowVecStatusChange;
		public Action<bool> RaiseShowOrderStatusChange;
		public Action<bool> RaiseShowOrientStatusChange;
		public Action<bool> RaiseShowTraverseStatusChange;
		public Action<bool> RaiseShowMarkerStatusChange;

		public StartupForm()
		{
			m_LanguageManager = new LanguageManager.LanguageManager( "StartupForm" );

#if !DEBUG
			if( FALicenseChecker.LicenseChecker.IsLicenseActivated() == false ) {
				Environment.Exit( 0 );
			}
#endif

			// need to init befor myapp because myapp will use LogPanel
			InitializeComponent();
			m_MainLayout = new MainLayout( m_pnlBackGround, m_pnlBottom, m_pnlCNC, m_pnlCNCInfo,
				m_pnlMessages, m_pnlLog, m_pnlCNCVertical, m_pnlVerticalAlarm, m_tsmiCNC, m_tsmiController );
			m_pnlBackGround.SizeChanged += OnMainLayoutSizeChanged;
			DialogPlacement.AreaProvider = () => m_MainLayout.GetDialogArea( this );

			// app
			MyApp.MainForm = this;
			UIListSetting();
			InitCNCModule();

			// create the viewer
			m_Viewer = new Viewer();
			bool bSucess = m_Viewer.InitViewer( m_pnlViewer );
			if( !bSucess ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Log_ViewerInitFailed", LOG_VIEWER_INIT_FAILED ), MyApp.NoticeType.Error );
				return;
			}
			m_Viewer.UpdateView();

			// multi select tree view
			m_TreeView = new MultiSelectTreeView();
			m_TreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			m_TreeView.Location = new System.Drawing.Point( 0, 0 );
			m_TreeView.Name = "m_TreeView";
			m_TreeView.TabIndex = 0;
			m_pnlTreeView.Controls.Add( this.m_TreeView );

			// show G54
			ShowG54Trihedron();

			// view manager
			m_ViewManager = new ViewManager( m_Viewer );

			// CAD Manager
			if( GetMachineDataSuccess( out FiveAxisMachineData machineData ) ) {
				m_DataManager = new DataManager
				{
					MachineData = machineData
				};
			}
			else {
				// get machine data fail, machine will be null(can't use)
				m_DataManager = new DataManager();
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Log_UseDefaultMachineProject", LOG_USE_DEFAULT_MACHINE_PROJECT ), MyApp.NoticeType.Warning );
			}

			// get machine mesh by type, so data manager need to be builded already
			bool isGetSuccess = GetMachineMesh( m_DataManager.MachineData, out MachineMeshes machineMeshes );
			if( isGetSuccess ) {
				m_DataManager.MachineMeshes = machineMeshes;
			}
			DataGettingHelper.Initialize( m_DataManager );
			m_DataManager.PartChanged += UpdateThreePointCalibrationEntry;
			m_DataManager.PartRemoved += OnCalibrationPartsRemoved;
			UpdateThreePointCalibrationEntry();

			// CAD Editor
			m_CADEditor = new CADEditor( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			m_CADEditor.RaiseCADActionStatusChange += OnCADActionStatusChange;
			m_CADEditor.RaiseFileImported += OnProjectFileImported;

			// CAM Editor
			m_CAMEditor = new CAMEditor( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			m_CAMEditor.PathPropertyChanged = OnCAMPathPropertyChanged;
			m_CAMEditor.RaiseCAMActionStatusChange += OnCAMActionStatusChange;
			m_CAMEditor.RaiseWithDlgActionStatusChange += OnCAMDlgActionStatusChange;
			m_CAMEditor.PathShapeTypeChanged = OnPathShapeTypeChange;
			RaiseShowVecStatusChange += m_CAMEditor.SetShowToolVec;
			RaiseShowOrderStatusChange += m_CAMEditor.SetShowOrder;
			RaiseShowOrientStatusChange += m_CAMEditor.SetShowOrientation;
			RaiseShowTraverseStatusChange += m_CAMEditor.SetShowTraversePath;
			RaiseShowMarkerStatusChange += m_CAMEditor.SetShowMarker;

			// simu editor
			m_SimuEditor = new SimuEditor( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			m_SimuEditor.ReadStlSuccess = OnMachineStlReadSuccess;
			m_SimuEditor.Init();

			// start with CAD editor
			SwitchEditor( EEditorType.CAD );
			DefaultUISetting();

			// Get cust post data
			MyApp.SetCustPost();

			// init language switch menu
			InitLanguageMenu();

			// apply language resources to UI
			ApplyResourcesToUI();
		}

		public Panel GetLogPanel
		{
			get
			{
				return m_pnlLog;
			}
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );
			m_CNCUIManager?.Init();
			UpdateMainLayout();
		}

		void InitCNCModule()
		{
			MachiningPreparation preparation = new MachiningPreparation( () => m_CAMEditor );
			m_CNCUIManager = new CNCUIManager( CncClient.Instance,
				MyApp.AppSetting.ApplyCNCIP, preparation.PrepareAsync,
				DialogShowHelper.ShowCentered );
			m_CNCUIManager.Notice += OnCNCNotice;
			m_CNCUIManager.ControllerRequested += ShowController;
			m_CNCUIManager.ThreePointCalibrationRequested += ShowThreePointCalibration;
			m_CNCUIManager.AlarmsChanged += OnCNCAlarmsChanged;
			m_CNCUIManager.ConnectionChanged += OnCNCConnectionChanged;
			OnCNCConnectionChanged( m_CNCUIManager.IsConnected );
			UpdateMainLayout();
		}

		void OnCNCConnectionChanged( bool connected )
		{
			if( IsDisposed || Disposing ) {
				return;
			}
			m_tsbSetOffset.Enabled = connected;
			m_tsbProcessSettings.Enabled = connected;
			if( connected && ( Height > Width || m_CurrentUIStatus == EUIStatus.Machining ) && !m_CNCUIManager.HasPanels ) {
				ShowCNCPanels( false );
			}
		}

		void OnCNCAlarmsChanged()
		{
			if( IsDisposed || Disposing ) {
				return;
			}
			UpdateAlarmLayout();
		}

		void OnMessagesSizeChanged( object sender, EventArgs e )
		{
			UpdateAlarmLayout();
		}

		void UpdateAlarmLayout()
		{
			if( IsDisposed || Disposing ) {
				return;
			}
			m_MainLayout?.UpdateMessages( Size, m_CNCUIManager?.AlarmPanel, m_CNCUIManager?.HasAlarmContent == true );
		}

		void m_tsmiCNC_Click( object sender, EventArgs e )
		{
			ShowCNCPanels();
		}

		void ShowCNCPanels( bool activateMachining = true )
		{
			if( m_CNCUIManager == null ) {
				return;
			}
			if( activateMachining ) {
				SwitchEditor( EEditorType.CAM, false );
			}
			if( m_CNCUIManager.HasPanels ) {
				if( activateMachining ) {
					RefreshToolStripLayout( EUIStatus.Machining );
				}
				return;
			}
			m_pnlBackGround.SuspendLayout();
			try {
				if( !m_CNCUIManager.CreatePanels() ) {
					if( activateMachining ) {
						RefreshToolStripLayout( EUIStatus.Machining );
						VisibleCAMQuickToolBarButtons( false );
						VisibleTechLayerButtons( false );
					}
					return;
				}
				m_CNCUIManager.Panel.Visible = false;
				m_CNCUIManager.InfoPanel.Visible = false;
				m_CNCUIManager.Panel.Dock = DockStyle.Fill;
				m_pnlCNC.Controls.Add( m_CNCUIManager.Panel );
				m_CNCUIManager.InfoPanel.Dock = DockStyle.Fill;
				m_pnlCNCInfo.Controls.Add( m_CNCUIManager.InfoPanel );
				UpdateMainLayout();
				if( activateMachining ) {
					RefreshToolStripLayout( EUIStatus.Machining );
					VisibleCAMQuickToolBarButtons( false );
					VisibleTechLayerButtons( false );
				}
			}
			finally {
				m_pnlBackGround.ResumeLayout( true );
			}
			m_MainLayout.FitPanels( m_CNCUIManager.Panel, m_CNCUIManager.InfoPanel );
			m_CNCUIManager.Panel.Visible = true;
			m_CNCUIManager.InfoPanel.Visible = true;
		}

		void CloseCNCPanels()
		{
			if( m_CurrentEditor == m_CAMEditor ) {
				m_CAMEditor?.EndAction();
			}

			if( Height > Width ) {
				m_CNCUIManager?.CloseOffsetDialog();
			}
			else {
				m_CNCUIManager?.ClosePanels();
			}
			UpdateMainLayout();
		}

		void UpdateMainLayout()
		{
			if( IsDisposed || Disposing ) {
				return;
			}
			if( m_CNCUIManager?.InfoPanel != null ) {
				m_CNCUIManager.InfoPanel.VerticalLayout = Height > Width;
			}
			m_MainLayout?.Update( Size, m_CNCUIManager != null, m_CurrentUIStatus == EUIStatus.Machining,
				CNCPanel.PREFERRED_SIZE,
				m_CNCUIManager?.Panel, m_CNCUIManager?.InfoPanel,
				m_CNCUIManager?.AlarmPanel, m_CNCUIManager?.HasAlarmContent == true );
		}

		void OnMainLayoutSizeChanged( object sender, EventArgs e )
		{
			UpdateMainLayout();
		}

		void OnCNCNotice( string message, bool isError )
		{
			MyApp.Logger.ShowOnLogPanel( message, isError ? MyApp.NoticeType.Warning : MyApp.NoticeType.Hint );
		}

		protected override void WndProc( ref Message m )
		{
			// prevent dragging main form when CNCDlg is open
			const int NON_Client_Area_Button_Down = 0xA1;
			const int HIT_Title_Bar = 2;

			// preventing form from being dragged when cnc dlg is showed
			if( m.Msg == NON_Client_Area_Button_Down && m.WParam.ToInt32() == HIT_Title_Bar ) {
				if( m_CNCDlg != null && !m_CNCDlg.IsDisposed ) {

					return;
				}
			}

			base.WndProc( ref m );
			if( m.Msg == WINDOWS_Drag_Event ) {
				AdjustToCurrentScreen();
			}
		}

		const int WINDOWS_Drag_Event = 0x0232;

		// language
		readonly LanguageManager.LanguageManager m_LanguageManager;
		const string CUSTOM_Root_Folder_Name = "Custom";
		const string MACHINE_Data_File_Name = "MachineData.mac";

		// view properties
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;

		// data manager
		DataManager m_DataManager;

		// editors
		CADEditor m_CADEditor;
		CAMEditor m_CAMEditor;
		SimuEditor m_SimuEditor;
		IEditor m_CurrentEditor;

		// controller dialog
		CNCDlg m_CNCDlg;
		CNCUIManager m_CNCUIManager;
		bool m_CNCShutdownPending;
		bool m_CNCShutdownComplete;

		// UI list
		readonly MainLayout m_MainLayout;
		Dictionary<EUIStatus, List<Control>> m_UIStatusDic;
		List<ToolStripContainer> m_ToolStripLevelList;
		EUIStatus m_CurrentUIStatus = EUIStatus.File;

		// UI color
		readonly Color DEFAULT_BtnColor = Color.Empty;
		readonly Color ON_ButtonColor = ThemeColors.ACCENT_TINT_STRONG;

		const string SOFTWARE_Version = "22661.0.3 (0.41.0)";
		const string SOFTWARE_Name = "飛切特別版 " + SOFTWARE_Version;

		string m_currentImportedFileName = string.Empty;

		const string LOG_LICENSE_INVALID = "授權流程不正確";
		const string LOG_VIEWER_INIT_FAILED = "顯示器初始化失敗";
		const string LOG_USE_DEFAULT_MACHINE_PROJECT = "使用默認機構專案檔";
		const string LOG_MACHINE_NODE_NULL = "機構參數節點為空,使用默認機構參數";
		const string LOG_MACHINE_FILE_NULL = "機構檔案為空,使用默認機構參數";
		const string LOG_MACHINE_PARAM_ERROR = "機構參數錯誤(Pr:{0}),使用默認機構參數";
		const string LOG_MACHINE_TREE_INVALID = "機構樹結構錯誤,使用默認機構樹";
		const string LOG_MACHINE_FILE_READ_FAILED = "讀取機構檔案失敗：\n{0},使用默認機構參數";
		const string LOG_MACHINE_FILE_SAVE_FAILED = "儲存機構檔案失敗：\n{0}";
		const string LOG_VNC_FILE_NOT_FOUND = "找不到VNC執行檔，請確認VNC執行檔與程式執行檔在同一資料夾下";
		const string LOG_VNC_CONNECT_FAILED = "無法連線到指定IP位置";
		const string LOG_VNC_UNKNOWN_ERROR = "VNC連線過程發生未知錯誤";
		const string LOG_VNC_CONNECTION_ERROR = "VNC 連線失敗：{0}";
		const string LOG_CNC_CONNECTION_ERROR = "CNC 連線流程失敗：{0}";
		const string LOG_CNC_SHUTDOWN_UNCONFIRMED = "CNC 關閉未確認：{0}";
		const string LOG_SHUTDOWN_FAILED = "關閉流程失敗：{0}";
		const string LOG_CNC_CONNECT_FAILED = "CNC 連線失敗";
		const string LOG_CNC_CONNECT_SUCCESS = "CNC 連線成功";
		const string LOG_CNC_RECONNECTING = "CNC 重新連線中...";
		const string LOG_CNC_RECONNECTED = "CNC 重新連線成功";
		const string LOG_CNC_RECONNECT_FAILED = "CNC 重新連線失敗，請檢查網路或 CNC 狀態";
		const string LOG_CNC_PLEASE_CONNECT_FIRST = "尚未和CNC連線，請先進行CNC連線";
		const string MSG_SAVE_BEFORE_EXIT_TITLE = "存檔確認";
		const string MSG_SAVE_BEFORE_EXIT_WITH_DATA = "是否要在關閉前存檔？";

		enum EUIStatus
		{
			File,
			CAD,
			CAM,
			Simulation,
			Machining,
		}

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

		void m_tsmiFile_Click( object sender, EventArgs e )
		{
			SwitchEditor( EEditorType.CAD );
			RefreshToolStripLayout( EUIStatus.File );

			// not in cam editing viewer tool bar cam buttons can't be used
			VisibleCAMQuickToolBarButtons( false );
			VisibleTechLayerButtons( false );
		}

		// back to CAD editor
		void m_tsmiCAD_Click( object sender, EventArgs e )
		{
			SwitchEditor( EEditorType.CAD );
			RefreshToolStripLayout( EUIStatus.CAD );

			// not in cam editing viewer tool bar cam buttons can't be used
			VisibleCAMQuickToolBarButtons( false );
			VisibleTechLayerButtons( false );
		}

		// go to CAM editor
		void m_tsmiCAM_Click( object sender, EventArgs e )
		{
			SwitchEditor( EEditorType.CAM );
			RefreshToolStripLayout( EUIStatus.CAM );

			// ex CAM edtior might lock some entrance
			EnableAllCAMEnterance();

			// during cam editing viewer tool bar cam buttons can be used
			VisibleCAMQuickToolBarButtons( true );
			VisibleTechLayerButtons( true );
		}

		// import part
		void m_tsbImport3DFile_Click( object sender, EventArgs e )
		{
			m_CADEditor.Import3DFile();
		}

		void m_tsbAddRevolution_Click( object sender, EventArgs e )
		{
			m_CADEditor.ImportRingPart();
		}

		void m_tsbImportProjectFile_Click( object sender, EventArgs e )
		{
			m_CADEditor.ImportProjectFile();
		}

		void m_tsbSaveProjectFile_Click( object sender, EventArgs e )
		{
			m_CADEditor.SaveProjectFile();
		}

		void m_tsbAddTube_Click( object sender, EventArgs e )
		{
			CADEditMainForm form = new CADEditMainForm();
			form.SetCadMapToModify( null );
			form.CreateNewTube += CreateTube;
			form.StartPosition = FormStartPosition.Manual;
			form.Location = new Point( this.Location.X, this.Location.Y );
			form.Size = new Size( this.Width, this.Height );
			form.ShowDialog( this );
		}

		void CreateTube( TopoDS_Shape rawTubeShape, CADFeatureParamMap cadMap, bool isModifying, ref bool isCancelImport, ref bool bImportSuccess )
		{
			isCancelImport = false;
			bImportSuccess = true;
			m_CADEditor.ImportTube( rawTubeShape );
		}

		// sew part
		void m_tsbSewPart_Click( object sender, EventArgs e )
		{
			m_CADEditor.SewPart();
		}

		// add feature
		private void m_tsbAddLine_TwoVertexConnect_Click( object sender, EventArgs e )
		{
			m_CADEditor.AddLine( AddLineType.TwoVertexConnectLine );
		}

		void m_tsbAddPoint_AcrCenter_Click( object sender, EventArgs e )
		{
			m_CADEditor.AddPoint( AddPointType.CircArcCenter );

		}

		void m_tsbAddPoint_EdgeCenter_Click( object sender, EventArgs e )
		{
			m_CADEditor.AddPoint( AddPointType.EdgeMidPoint );
		}

		void m_tsbAddPoint_PointCenter_Click( object sender, EventArgs e )
		{
			m_CADEditor.AddPoint( AddPointType.TwoVertexMidPoint );
		}

		void m_tsbAddPoint_BoundingBoxCenter_Click( object sender, EventArgs e )
		{
			m_CADEditor.AddPoint( AddPointType.BoundingBoxCenter );
		}

		// manual transform
		void m_tsbManualTransform_Click( object sender, EventArgs e )
		{
			m_CADEditor.StartManualTransform();
		}

		void m_tsbManualTransPlane_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( ETrsfConstraintType.Plane );
		}

		void m_tsbManualTransPlanePar_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( ETrsfConstraintType.PlaneParallel );
		}

		void m_tsbManualTransAxial_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( ETrsfConstraintType.Axial );
		}

		void m_tsbManualTransAxialPar_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( ETrsfConstraintType.AxialParallel );
		}

		void m_tsbManualTransPointCoincide_Click( object sender, EventArgs e )
		{
			m_CADEditor.ApplyManualTransform( ETrsfConstraintType.Point );
		}

		// measure
		void m_tsbMeasureCAD_Click( object sender, EventArgs e )
		{
			m_CADEditor.StartMeasure();
		}

		void m_tsbMeasureCAM_Click( object sender, EventArgs e )
		{
			m_CAMEditor.StartMeasure();
		}

		// wire discrete vertex
		void m_tsbWireDiscreteVertex_Click( object sender, EventArgs e )
		{
			m_CADEditor.StartWireDiscreteVertex();
		}

		// 3 point transform
		void m_tsb3PntTransform_Click( object sender, EventArgs e )
		{
			m_CADEditor.ThreePointTransform();
		}

		// axis transform
		void m_tsbAxisTransform_Click( object sender, EventArgs e )
		{
			m_CADEditor.StartAxisTransform();
		}

		// add path
		void m_tsbAddPath_Click( object sender, EventArgs e )
		{
			m_CAMEditor.StartAddPath();
		}

		void m_tsbDesignSupportBracket_Click( object sender, EventArgs e )
		{
			m_CADEditor.StartDesignSupportBracket();
		}

		// remove path
		void m_tsbDeletePath_Click( object sender, EventArgs e )
		{
			m_CAMEditor.RemovePath();
		}

		// CAM property
		void m_tsbStartPoint_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetStartPoint();
		}

		void m_tsbReverse_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetReverse();
		}

		void m_tsbSetLead_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetLeadLine();
		}

		void m_tsbOverCut_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetOverCut();
		}

		// tool vector
		void m_tsbToolVec_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetToolVec();
		}

		void m_tsbTooVecReverse_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetToolVecReverse();
		}

		void m_tsbFixedToolVec_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetFixedToolVec2();
		}

		void m_tsbOptimizeIK_Click( object sender, EventArgs e )
		{
			m_CAMEditor.AutoOptimizeIKContinuity();
		}

		// sort
		void m_tsbManualOrder_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetManualOrder();
		}

		void m_tsbAutoOrder_Click( object sender, EventArgs e )
		{
			m_CAMEditor.AutoSortProcess();
		}

		// post parameter setting
		void m_tsbSetTraverseData_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetTraverseData();
		}

		void m_tsbFlyCut_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetFlyCut();
		}

		void m_tsbEntryAndExitSetting_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetEntryAndExitParam();
		}

		void m_tsbPatternSetting_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetStandardPattern();
		}

		void m_tsbPathEdit_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetPathEdit();
		}

		void m_tsbContourEdit_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetContourEdit();
		}

		void m_tsbPathCompensation_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetPathCompensation();
		}

		// convert NC
		async void m_tsbConvertNC_Click( object sender, EventArgs e )
		{
			if( !m_tsbConvertNC.Enabled ) {
				return;
			}
			m_tsbConvertNC.Enabled = false;
			try {
				await m_CAMEditor.ConvertNCAsync();
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "NC export workflow failed: {0}", ex );
				if( !IsDisposed && !Disposing ) {
					MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "Log_NCExportError", LOG_NC_EXPORT_ERROR ), ex.Message ), MyApp.NoticeType.Warning );
				}
			}
			finally {
				if( !IsDisposed && !Disposing ) {
					m_tsbConvertNC.Enabled = true;
				}
			}
		}

		void m_tsbMicroJoint_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetMicroJoint();
		}

		// extended G Code
		void m_tsbExtendedGCode_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetExtendedGCode();
		}

		void m_tsbRCorner_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetRCorner();
		}

		#region Viewer tool bar click action

		void m_tsbShowVec_CheckedChanged( object sender, EventArgs e )
		{
			RaiseShowVecStatusChange( m_tsbShowVec.Checked );
		}

		void m_tsbShowOrder_CheckedChanged( object sender, EventArgs e )
		{
			RaiseShowOrderStatusChange( m_tsbShowOrder.Checked );
		}

		void m_tsbShowOrientation_CheckedChanged( object sender, EventArgs e )
		{
			RaiseShowOrientStatusChange( m_tsbShowOrientation.Checked );
		}

		void m_tsbShowTraverse_CheckedChanged( object sender, EventArgs e )
		{
			RaiseShowTraverseStatusChange( m_tsbShowTraverse.Checked );
		}

		void m_tsbShowMarker_CheckedChanged( object sender, EventArgs e )
		{
			RaiseShowMarkerStatusChange( m_tsbShowMarker.Checked );
		}

		void m_tsbAXO_Click( object sender, EventArgs e )
		{
			m_Viewer.AxoView();
			m_Viewer.ZoomAllView();
		}

		void m_tsbXPos_Click( object sender, EventArgs e )
		{
			m_Viewer.RightView();
			m_Viewer.ZoomAllView();
		}

		void m_tsbXNeg_Click( object sender, EventArgs e )
		{
			m_Viewer.LeftView();
			m_Viewer.ZoomAllView();
		}

		void m_tsbYPos_Click( object sender, EventArgs e )
		{
			m_Viewer.FrontView();
			m_Viewer.ZoomAllView();
		}

		void m_tsbYNeg_Click( object sender, EventArgs e )
		{
			m_Viewer.BackView();
			m_Viewer.ZoomAllView();
		}

		void m_tsbZPos_Click( object sender, EventArgs e )
		{
			m_Viewer.TopView();
			m_Viewer.ZoomAllView();
		}

		void m_tsbZNeg_Click( object sender, EventArgs e )
		{
			m_Viewer.BottomView();
			m_Viewer.ZoomAllView();
		}

		#endregion

		#region UI action

		// cad action change event
		void OnCADActionStatusChange( EditActionType action, EActionStatus actionStatus )
		{
			if( actionStatus == EActionStatus.End ) {
				RefreshToolStripLayout( EUIStatus.CAD );
				switch( action ) {
					case EditActionType.AddPoint_CircArcCenter:
						m_tsbAddPoint_AcrCenter.BackColor = DEFAULT_BtnColor;
						return;
					case EditActionType.AddPoint_EdgeMidPoint:
						m_tsbAddPoint_EdgeCenter.BackColor = DEFAULT_BtnColor;
						return;
					case EditActionType.AddPoint_TwoVertexMidPoint:
						m_tsbAddPoint_PointCenter.BackColor = DEFAULT_BtnColor;
						return;
					case EditActionType.AddLine:
						m_tsbAddLine_TwoVertexConnect.BackColor = DEFAULT_BtnColor;
						return;
					case EditActionType.AxisTransform:
						m_tsbAxisTransform.BackColor = DEFAULT_BtnColor;
						return;
					case EditActionType.ThreePtTransform:
						m_tsb3PntTransform.BackColor = DEFAULT_BtnColor;
						return;
					case EditActionType.ManualTransform:
						m_tsbManualTransform.BackColor = DEFAULT_BtnColor;
						return;
					case EditActionType.Measure:
						m_tsbMeasureCAD.BackColor = DEFAULT_BtnColor;
						return;
					case EditActionType.WireDiscreteVertex:
						m_tsbWireDiscreteVertex.BackColor = DEFAULT_BtnColor;
						return;
					case EditActionType.DesignSupportBracket:
						m_tsbDesignSupportBracket.BackColor = DEFAULT_BtnColor;
						return;
					default:
						return;
				}
			}

			RefreshToolStripLayout( EUIStatus.CAD );

			// turn on light
			switch( action ) {
				case EditActionType.AddPoint_CircArcCenter:
					m_tsbAddPoint_AcrCenter.BackColor = ON_ButtonColor;
					return;
				case EditActionType.AddPoint_EdgeMidPoint:
					m_tsbAddPoint_EdgeCenter.BackColor = ON_ButtonColor;
					return;
				case EditActionType.AddPoint_TwoVertexMidPoint:
					m_tsbAddPoint_PointCenter.BackColor = ON_ButtonColor;
					return;
				case EditActionType.AddLine:
					m_tsbAddLine_TwoVertexConnect.BackColor = ON_ButtonColor;
					return;
				case EditActionType.AxisTransform:
					m_tsbAxisTransform.BackColor = ON_ButtonColor;
					return;
				case EditActionType.ThreePtTransform:
					m_tsb3PntTransform.BackColor = ON_ButtonColor;
					return;
				case EditActionType.ManualTransform:
					m_tsbManualTransform.BackColor = ON_ButtonColor;
					return;
				case EditActionType.Measure:
					m_tsbMeasureCAD.BackColor = ON_ButtonColor;
					return;
				case EditActionType.WireDiscreteVertex:
					m_tsbWireDiscreteVertex.BackColor = ON_ButtonColor;
					return;
				case EditActionType.DesignSupportBracket:
					m_tsbDesignSupportBracket.BackColor = ON_ButtonColor;
					return;
				default:
					return;
			}
		}

		void UpdateFormTitle()
		{
			string localizedBase = string.Format(
				m_LanguageManager.TryGetString( "FormTitle", SOFTWARE_Name ),
				SOFTWARE_Version );
			Text = string.IsNullOrEmpty( m_currentImportedFileName )
				? localizedBase
				: $"{localizedBase} - {m_currentImportedFileName}";
		}

		void OnProjectFileImported( bool isSuccess, string fileName )
		{
			UpdateThreePointCalibrationEntry();
			if( isSuccess == false ) {
				return;
			}
			m_currentImportedFileName = fileName ?? string.Empty;
			UpdateFormTitle();
		}

		// cam action change event
		void OnCAMDlgActionStatusChange( EActionStatus actionStatus )
		{
			if( actionStatus == EActionStatus.Start ) {
				m_tsCAMFunction.Enabled = false;
				m_msMode.Enabled = false;
				return;
			}
			m_tsCAMFunction.Enabled = true;
			m_msMode.Enabled = true;
		}

		void OnCAMActionStatusChange( EditActionType action, EActionStatus actionStatus )
		{
			if( actionStatus == EActionStatus.End ) {

				// cam action is done, back to default cam lay out,
				RefreshToolStripLayout( EUIStatus.CAM );
				switch( action ) {
					case EditActionType.AddPath:
						m_tsbAddPath.BackColor = DEFAULT_BtnColor;
						break;
					case EditActionType.StartPoint:
						m_tsbStartPoint.BackColor = DEFAULT_BtnColor;
						break;
					case EditActionType.MicroJoint:
						m_tsbMicroJoint.BackColor = DEFAULT_BtnColor;
						break;
					case EditActionType.RCorner:
						m_tsbRCorner.BackColor = DEFAULT_BtnColor;
						break;
					case EditActionType.ExtendedGCode:
						m_tsbExtendedGCode.BackColor = DEFAULT_BtnColor;
						break;
					case EditActionType.SetLead:
						m_tsbSetLead.BackColor = DEFAULT_BtnColor;

						// unlock form
						OnCAMDlgActionStatusChange( actionStatus );
						break;
					case EditActionType.OverCut:
						m_tsbOverCut.BackColor = DEFAULT_BtnColor;
						OnCAMDlgActionStatusChange( actionStatus );
						break;
					case EditActionType.ToolVec:
						m_tsbToolVec.BackColor = DEFAULT_BtnColor;
						break;
					case EditActionType.SetTraverse:
						m_tsbSetTraverseData.BackColor = DEFAULT_BtnColor;
						OnCAMDlgActionStatusChange( actionStatus );
						break;
					case EditActionType.FlyCut:
						m_tsbFlyCut.BackColor = DEFAULT_BtnColor;
						OnCAMDlgActionStatusChange( actionStatus );
						break;
					case EditActionType.SetEntryAndExitParam:
						m_tsbEntryAndExitSetting.BackColor = DEFAULT_BtnColor;
						OnCAMDlgActionStatusChange( actionStatus );
						break;
					case EditActionType.ManualOrder:
						m_tsbManualOrder.BackColor = DEFAULT_BtnColor;
						OnCAMDlgActionStatusChange( actionStatus );
						break;
					case EditActionType.Measure:
						m_tsbMeasureCAM.BackColor = DEFAULT_BtnColor;
						break;
					case EditActionType.SpecialToolVecEdit:
						m_tsbFixedToolVec.BackColor = DEFAULT_BtnColor;
						break;
					default:
						break;
				}
				return;
			}

			// action start, need to relay out
			RefreshToolStripLayout( EUIStatus.CAM );
			switch( action ) {
				case EditActionType.AddPath:
					m_tsbAddPath.BackColor = ON_ButtonColor;
					break;
				case EditActionType.StartPoint:
					m_tsbStartPoint.BackColor = ON_ButtonColor;
					break;
				case EditActionType.MicroJoint:
					m_tsbMicroJoint.BackColor = ON_ButtonColor;
					break;
				case EditActionType.RCorner:
					m_tsbRCorner.BackColor = ON_ButtonColor;
					break;
				case EditActionType.ExtendedGCode:
					m_tsbExtendedGCode.BackColor = ON_ButtonColor;
					break;
				case EditActionType.SetLead:
					m_tsbSetLead.BackColor = ON_ButtonColor;
					// lock main from
					OnCAMDlgActionStatusChange( actionStatus );
					break;
				case EditActionType.OverCut:
					m_tsbOverCut.BackColor = ON_ButtonColor;
					// lock main from
					OnCAMDlgActionStatusChange( actionStatus );
					break;
				case EditActionType.ToolVec:
					m_tsbToolVec.BackColor = ON_ButtonColor;
					break;
				case EditActionType.SetTraverse:
					m_tsbSetTraverseData.BackColor = ON_ButtonColor;
					OnCAMDlgActionStatusChange( actionStatus );
					break;
				case EditActionType.FlyCut:
					m_tsbFlyCut.BackColor = ON_ButtonColor;
					OnCAMDlgActionStatusChange( actionStatus );
					break;
				case EditActionType.SetEntryAndExitParam:
					m_tsbEntryAndExitSetting.BackColor = ON_ButtonColor;
					OnCAMDlgActionStatusChange( actionStatus );
					break;
				case EditActionType.ManualOrder:
					m_tsbManualOrder.BackColor = ON_ButtonColor;
					OnCAMDlgActionStatusChange( actionStatus );
					break;
				case EditActionType.Measure:
					m_tsbMeasureCAM.BackColor = ON_ButtonColor;
					break;
				case EditActionType.SpecialToolVecEdit:
					m_tsbFixedToolVec.BackColor = ON_ButtonColor;
					break;
				default:
					break;
			}
		}

		// path property change event
		void OnCAMPathPropertyChanged( EditableInfo editableInfo )
		{
			m_tsbStartPoint.Enabled = editableInfo.IsStartPointEditable;
			m_tsbOverCut.Enabled = editableInfo.IsOverCutEditable;
			m_tsbSetLead.Enabled = editableInfo.IsLeadLineEditable;
			m_tsbToolVec.Enabled = editableInfo.IsToolVecEditable;
			m_tsbToolVecReverse.Enabled = editableInfo.IsToolVecReverseEditable;
			m_tsbFixedToolVec.Enabled = editableInfo.IsFixedToolVecEditable;
			m_tsbAutoOrder.Enabled = editableInfo.IsAutoOrderEditable;
			m_tsbReverse.Enabled = editableInfo.IsPathReverseEditable;
			m_tsbPathCompensation.Enabled = editableInfo.IsPathCompensateEditable;
			m_tsbMicroJoint.Enabled = editableInfo.IsMicroJointEditable;
			m_tsbRCorner.Enabled = editableInfo.IsRCornerEditable;
			m_tsbExtendedGCode.Enabled = editableInfo.IsExtendedGCodeEditable;
			m_tsbContourEdit.Enabled = editableInfo.IsContourEditable;
			m_tsbSetTraverseData.Enabled = editableInfo.IsTraverseEditable;
			m_tsbFlyCut.Enabled = editableInfo.IsFlyCutEditable;
			m_tsbPatternSetting.Enabled = editableInfo.IsStandardPatternEditable;
			m_tsbPathEdit.Enabled = editableInfo.IsPathEditEditable;
		}

		// path type change event
		void OnPathShapeTypeChange( PathType type )
		{
			if( type != PathType.Contour ) {
				m_tsbReverse.Enabled = false;
				m_tsbToolVec.Enabled = false;
				m_tsbToolVecReverse.Enabled = false;
			}
			else {
				m_tsbReverse.Enabled = true;
				m_tsbToolVec.Enabled = true;
				m_tsbToolVecReverse.Enabled = true;
			}
		}

		// ui display
		void RefreshToolStripLayout( EUIStatus uiStatus )
		{
			UpdateThreePointCalibrationEntry();
			if( Height <= Width && m_CNCUIManager != null && m_CNCUIManager.HasPanels ) {
				uiStatus = EUIStatus.Machining;
			}
			if( !m_UIStatusDic.ContainsKey( uiStatus ) || m_UIStatusDic[ uiStatus ] == null ) {
				return;
			}
			m_CurrentUIStatus = uiStatus;

			// get which tool strip need to show on container
			List<Control> ShownToolStripList = m_UIStatusDic[ uiStatus ];

			// change tool strip container visible
			int nContainerLayerToShow = ShownToolStripList.Count;
			bool bToolStirpShownCountChange = false;
			for( int i = 0; i < m_ToolStripLevelList.Count; i++ ) {

				// these container visible change
				if( m_ToolStripLevelList[ i ].Visible != ( i < nContainerLayerToShow ) ) {
					bToolStirpShownCountChange = true;
				}
				m_ToolStripLevelList[ i ].Visible = i < nContainerLayerToShow;
			}

			// change witch tool strip will be show
			for( int i = 0; i < ShownToolStripList.Count; i++ ) {

				// level 1 container always show on form
				if( i == 0 && m_ToolStripLevelList[ i ].Visible != true ) {
					m_ToolStripLevelList[ i ].Visible = true;
				}

				// change tool strip visible in this container
				List<ToolStrip> thisToolStrip = GetAllToolStrips( m_ToolStripLevelList[ i ] );
				foreach( ToolStrip toolStrip in thisToolStrip ) {
					if( toolStrip == ShownToolStripList[ i ] ) {
						if( toolStrip.Visible != true ) {
							toolStrip.Visible = true;
						}
						continue;
					}
					toolStrip.Visible = false;
				}
			}

			// visible will cause layout change need to make sure container layout order correct
			if( bToolStirpShownCountChange ) {
				ReLayOutUIZorder();
			}
			UpdateMainLayout();
		}

		List<ToolStrip> GetAllToolStrips( ToolStripContainer container )
		{
			List<ToolStrip> childToolStripList = new List<ToolStrip>();

			// container with 5 panel
			FindContainerAllToolStrips( container.TopToolStripPanel, childToolStripList );
			FindContainerAllToolStrips( container.BottomToolStripPanel, childToolStripList );
			FindContainerAllToolStrips( container.LeftToolStripPanel, childToolStripList );
			FindContainerAllToolStrips( container.RightToolStripPanel, childToolStripList );
			FindContainerAllToolStrips( container.ContentPanel, childToolStripList );
			return childToolStripList;
		}

		// each container will include several tool strips, use this to know which tool strips are belong to this container
		void FindContainerAllToolStrips( Control parent, List<ToolStrip> totalToolStripList )
		{
			foreach( Control control in parent.Controls ) {
				if( control is ToolStrip toolStrip ) {
					totalToolStripList.Add( toolStrip );
				}
				else if( control.HasChildren ) {
					FindContainerAllToolStrips( control, totalToolStripList );
				}
			}
		}

		// to make sure container disply order is level 1 -> 3
		void ReLayOutUIZorder()
		{
			int currentIndex = 0;

			// layout from bottom to top
			if( m_pnlBackGround.Visible ) {
				Controls.SetChildIndex( m_pnlBackGround, currentIndex++ );
			}
			if( m_tscLevel3Container.Visible ) {
				Controls.SetChildIndex( m_tscLevel3Container, currentIndex++ );
			}
			if( m_tscLevel2Container.Visible ) {
				Controls.SetChildIndex( m_tscLevel2Container, currentIndex++ );
			}
			if( m_tscLevel1Container.Visible ) {
				Controls.SetChildIndex( m_tscLevel1Container, currentIndex++ );
			}
			if( m_msMode.Visible ) {
				Controls.SetChildIndex( m_msMode, currentIndex++ );
			}
		}

		void EnableAllCAMEnterance()
		{
			foreach( ToolStripItem item in m_tsCAMFunction.Items ) {
				item.Enabled = true;
			}
		}

		void VisibleCAMQuickToolBarButtons( bool isVisible )
		{
			m_tsbShowVec.Visible = isVisible;
			m_tsbShowOrder.Visible = isVisible;
			m_tsbShowOrientation.Visible = isVisible;
			m_tsbShowTraverse.Visible = isVisible;
			m_tsbShowMarker.Visible = isVisible;
		}

		void VisibleTechLayerButtons( bool isVisible )
		{
			m_tsLayer.Visible = isVisible;
		}

		void OnMachineStlReadSuccess( bool isReadSuccess )
		{
			if( isReadSuccess == false ) {
				m_tsmiSimulation.Enabled = false;
			}
		}

		#endregion

		// switch editor
		void SwitchEditor( EEditorType type, bool closePanels = true )
		{
			if( closePanels ) {
				CloseCNCPanels();
			}
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

		// UI setting
		void DefaultUISetting()
		{
			// hide tool strip container
			m_tscLevel2Container.Visible = false;
			m_tscLevel3Container.Visible = false;

			// default is cad mode
			RefreshToolStripLayout( EUIStatus.File );
			VisibleCAMQuickToolBarButtons( false );
			VisibleTechLayerButtons( false );

			// fixed single border
			FormBorderStyle = FormBorderStyle.FixedSingle;
			WindowState = FormWindowState.Maximized;
		}

		// this setting is for main form to know what situation ui need to refresh as what look like
		void UIListSetting()
		{
			m_UIStatusDic = new Dictionary<EUIStatus, List<Control>>()
			{
				{ EUIStatus.File, new List<Control>() { m_tsFileFunction } },
				{ EUIStatus.CAD , new List<Control>() { m_tsCADFunction } },
				{ EUIStatus.CAM, new List<Control>() { m_tsCAMFunction } },
				{ EUIStatus.Machining, new List<Control>() { m_tsMachiningFunction } },
				{ EUIStatus.Simulation, new List<Control>() { m_tsSimuFunc } },
			};
			m_ToolStripLevelList = new List<ToolStripContainer>()
			{
				m_tscLevel1Container,
				m_tscLevel2Container,
				m_tscLevel3Container
			};
		}

		void AdjustToCurrentScreen()
		{
			WindowState = FormWindowState.Maximized;
		}

		#region Get machine data

		bool GetMachineDataSuccess( out FiveAxisMachineData machineData )
		{
			machineData = null;

			// get exe directory
			string exeDir = AppDomain.CurrentDomain.BaseDirectory;
			string filePath = Path.Combine( exeDir, CUSTOM_Root_Folder_Name, MACHINE_Data_File_Name );
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
						MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Log_MachineNodeNull", LOG_MACHINE_NODE_NULL ), MyApp.NoticeType.Warning );
						return false;
					case EMachineDataLoadStatus.NullFile:
						MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Log_MachineFileNull", LOG_MACHINE_FILE_NULL ), MyApp.NoticeType.Warning );
						return false;
					case EMachineDataLoadStatus.InvalidPrValue:
						MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "Log_MachineParamError", LOG_MACHINE_PARAM_ERROR ), nErrorPrIndex ), MyApp.NoticeType.Warning );
						return false;
					case EMachineDataLoadStatus.TreeInvalid:
						MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Log_MachineTreeInvalid", LOG_MACHINE_TREE_INVALID ), MyApp.NoticeType.Warning );
						return true;
					default:
						return true;
				}
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "Log_MachineFileReadFailed", LOG_MACHINE_FILE_READ_FAILED ), ex.Message ), MyApp.NoticeType.Error );
				return false;
			}
		}

		bool GetMachineMesh( IMachineDataBase machineData, out MachineMeshes machineMeshes )
		{
			machineMeshes = null;
			if( machineData == null ) {
				return false;
			}
			string szFolderName = GetMachineMeshFolderName( machineData );
			bool isGetMesh = ReadMachineMeshHelper.LoadMachineMeshes( szFolderName, machineData, out machineMeshes );
			return isGetMesh;
		}

		// resolve the machine mesh folder ( named after the product / five-axis type )
		string GetMachineMeshFolderName( IMachineDataBase machineData )
		{
			string exeDir = AppDomain.CurrentDomain.BaseDirectory;
			string machineConfigDir = Path.Combine( exeDir, CUSTOM_Root_Folder_Name );

			// articulated robot meshes live in a dedicated folder
			if( machineData.Product == Product.ArticRobot ) {
				return Path.Combine( machineConfigDir, Product.ArticRobot.ToString() );
			}

			// five-axis meshes live in a folder named after the five-axis type
			FiveAxisType fiveAxis = ( machineData as FiveAxisMachineData )?.FiveAxisType ?? FiveAxisType.Spindle;
			return Path.Combine( machineConfigDir, fiveAxis.ToString() );
		}

		#endregion

		#region VNC Connection

		async Task SetAndDoConnectAsync()
		{
			string ip = null;
			using( ConnectDlg connectDlg = new ConnectDlg() ) {
				connectDlg.ConfirmEvent += confirmedIP =>
				{
					ip = confirmedIP;
					connectDlg.Close();
				};
				connectDlg.ShowDialog( this );
			}
			if( ip == null ) {
				return;
			}

			MyApp.CNCIP = null;
			try {
				CleanupVNCConnections();
				VNCUserControl newVNCControl = new VNCUserControl( ip );
				MyApp.VNCScreen = newVNCControl;
				switch( newVNCControl.ConnectionResult ) {
					case VNCConnectionResult.Success:
						break;
					case VNCConnectionResult.FileNotFound:
						MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Log_VNCFileNotFound", LOG_VNC_FILE_NOT_FOUND ), MyApp.NoticeType.Error, true );
						break;
					case VNCConnectionResult.ConnectionFailed:
						MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Log_VNCConnectFailed", LOG_VNC_CONNECT_FAILED ), MyApp.NoticeType.Error, true );
						break;
					case VNCConnectionResult.OtherError:
						MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Log_VNCUnknownError", LOG_VNC_UNKNOWN_ERROR ), MyApp.NoticeType.Error, true );
						break;
				}
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "Log_VNCConnectionError", LOG_VNC_CONNECTION_ERROR ), ex.Message ), MyApp.NoticeType.Warning );
			}
			var result = await m_CNCUIManager.ConnectAsync( ip );
			MyApp.CNCIP = result.IsSuccess ? ip : null;
		}

		async void m_tsmiCNCConnection_Click( object sender, EventArgs e )
		{
			if( !m_tsmiCNCConnection.Enabled ) {
				return;
			}
			m_tsmiCNCConnection.Enabled = false;
			try {
				await SetAndDoConnectAsync();
			}
			catch( Exception ex ) {
				if( !IsDisposed && !Disposing ) {
					MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "Log_CNCConnectionError", LOG_CNC_CONNECTION_ERROR ), ex.Message ), MyApp.NoticeType.Warning );
				}
			}
			finally {
				if( !IsDisposed && !Disposing ) {
					m_tsmiCNCConnection.Enabled = true;
				}
			}
		}

		void CleanupVNCConnections()
		{
			if( MyApp.VNCScreen != null ) {
				MyApp.VNCScreen.CloseVNCProcess();
				MyApp.VNCScreen.Dispose();
				MyApp.VNCScreen = null;
			}
		}

		void StartupForm_Resize( object sender, EventArgs e )
		{
			if( IsDisposed || Disposing ) {
				return;
			}
			if( Height > Width && m_CNCUIManager?.IsConnected == true && !m_CNCUIManager.HasPanels ) {
				ShowCNCPanels( false );
			}
			else if( Height <= Width && m_CurrentUIStatus != EUIStatus.Machining && m_CNCUIManager?.HasPanels == true ) {
				CloseCNCPanels();
			}
			UpdateMainLayout();
		}

		async void StartupForm_FormClosing( object sender, FormClosingEventArgs e )
		{
			if( m_CNCShutdownComplete ) {
				return;
			}
			if( m_CNCShutdownPending ) {
				e.Cancel = true;
				return;
			}
			if( e.Cancel ) {
				return;
			}
			m_CAMEditor?.EndAction();
			// check if there are any workpieces
			if( m_DataManager != null && m_DataManager.PartIDList.Count > 0 ) {
				string message = string.Format(
					m_LanguageManager.TryGetString( "Msg_SaveBeforeExitWithData", MSG_SAVE_BEFORE_EXIT_WITH_DATA ),
					m_DataManager.PartIDList.Count );
				string title = m_LanguageManager.TryGetString( "Msg_SaveBeforeExitTitle", MSG_SAVE_BEFORE_EXIT_TITLE );

				DialogResult result = MessageBox.Show(
					this,
					message,
					title,
					MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Question );

				if( result == DialogResult.Yes ) {
					// user wants to save
					m_CADEditor.SaveProjectFile();
				}
				else if( result == DialogResult.Cancel ) {
					// user cancelled the close operation
					e.Cancel = true;
					return;
				}
				// if No, continue closing without saving
			}

			e.Cancel = true;
			m_CNCShutdownPending = true;
			Enabled = false;
			try {
				if( m_CNCUIManager != null ) {
					try {
						var result = await m_CNCUIManager.StopAsync();
						if( !result.IsSuccess ) {
							OnCNCNotice( string.Format( m_LanguageManager.TryGetString( "Log_CNCShutdownUnconfirmed", LOG_CNC_SHUTDOWN_UNCONFIRMED ), result.ErrorMessage ), true );
						}
					}
					finally {
						m_CNCUIManager.Dispose();
					}
				}
				CleanupVNCConnections();
			}
			catch( Exception ex ) {
				OnCNCNotice( string.Format( m_LanguageManager.TryGetString( "Log_ShutdownFailed", LOG_SHUTDOWN_FAILED ), ex.Message ), true );
			}
			finally {
				m_CNCShutdownComplete = true;
				if( !IsDisposed ) {
					BeginInvoke( new Action( Close ) );
				}
			}
		}

		#endregion

		#region Simulation mode

		void m_tsmiSimulation_Click( object sender, EventArgs e )
		{
			SwitchEditor( EEditorType.Simulation );
			RefreshToolStripLayout( EUIStatus.Simulation );

			// not in cam editing viewer tool bar cam buttons can't be used
			VisibleCAMQuickToolBarButtons( false );
			VisibleTechLayerButtons( false );
		}

		void m_tsbPlay_Click( object sender, EventArgs e )
		{
			m_SimuEditor.PlaySimulation();
		}

		void m_tsbPause_Click( object sender, EventArgs e )
		{
			m_SimuEditor.PauseSimulation();
		}

		void m_tsbStop_Click( object sender, EventArgs e )
		{
			m_SimuEditor.StopSimulation();
		}

		#endregion

		void m_tsbPreviousPath_Click( object sender, EventArgs e )
		{
			m_SimuEditor.PlayPrePath();
		}

		void m_tsbNextPath_Click( object sender, EventArgs e )
		{
			m_SimuEditor.PlayNextPath();
		}

		void m_tsbPreCollision_Click( object sender, EventArgs e )
		{
			m_SimuEditor.MoveToPreCollision();
		}

		void m_tsbNextCol_Click( object sender, EventArgs e )
		{
			m_SimuEditor.MoveToNextCollision();
		}

		void m_tsbSpeedUp_Click( object sender, EventArgs e )
		{
			m_SimuEditor.SpeedUp();
		}

		void m_tsbSlowDown_Click( object sender, EventArgs e )
		{
			m_SimuEditor.SlowDown();
		}

		void m_tsbCheck_Click( object sender, EventArgs e )
		{
			m_SimuEditor.CheckExitCollision();
		}

		void m_tsbPrePoint_Click( object sender, EventArgs e )
		{
			m_SimuEditor.PrePoint();
		}

		void m_tsbNextPoint_Click( object sender, EventArgs e )
		{
			m_SimuEditor.NextPoint();
		}

		void m_tsmiController_Click( object sender, EventArgs e )
		{
			ShowController();
		}

		void ShowController()
		{
			if( IsDisposed || Disposing ) {
				return;
			}
			// no valid VNC connection, ask user to connect first
			if( MyApp.VNCScreen == null || MyApp.VNCScreen.IsDisposed || !MyApp.VNCScreen.IsConnected ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Log_CNCPleaseConnectFirst", LOG_CNC_PLEASE_CONNECT_FIRST ), MyApp.NoticeType.Warning, true );
				return;
			}

			// check if CNCDlg is already open
			if( m_CNCDlg != null && !m_CNCDlg.IsDisposed ) {

				// bring existing dialog to front
				m_CNCDlg.Activate();
				m_CNCDlg.BringToFront();

				m_CNCDlg.PositionInArea( m_pnlCNCVertical.RectangleToScreen( m_pnlCNCVertical.ClientRectangle ) );
				return;
			}

			// save original main form state
			bool m_OriginalMaximizeBox = MaximizeBox;
			bool m_OriginalMinimizeBox = MinimizeBox;

			// disable maximize and minimize button
			MaximizeBox = false;
			MinimizeBox = false;
			m_CNCDlg = new CNCDlg();

			// set owner to keep z order
			m_CNCDlg.Owner = MyApp.MainForm;

			// position dialog in the lower panel area for portrait mode
			m_CNCDlg.StartPosition = FormStartPosition.Manual;
			m_CNCDlg.PositionInArea( m_pnlCNCVertical.RectangleToScreen( m_pnlCNCVertical.ClientRectangle ) );
			m_CNCDlg.PutVNCOnDlg();

			// subscribe to FormClosed event to clean up reference and restore main form state
			m_CNCDlg.FormClosed += ( s, args ) =>
			{
				// restore main form maximize and minimize buttons
				MaximizeBox = m_OriginalMaximizeBox;
				MinimizeBox = m_OriginalMinimizeBox;
				m_CNCDlg = null;
			};

			// show as modeless dialog (allows main form interaction)
			m_CNCDlg.Show( MyApp.MainForm );
		}

		void m_tsbNeedProcess_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetProcessPath( true );
		}

		void m_tsbLayerNone_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetProcessPath( false );
		}

		void m_tsbLayer1_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetPathLayer( 1 );
		}

		void m_tsbLayer2_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetPathLayer( 2 );
		}

		void m_tsbLayer3_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetPathLayer( 3 );
		}

		void m_tsbLayer4_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetPathLayer( 4 );
		}

		void m_tsbLayer5_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetPathLayer( 5 );
		}

		void m_tsbLayer6_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetPathLayer( 6 );
		}

		void m_tsbLayer7_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetPathLayer( 7 );
		}

		void m_tsbLayer8_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetPathLayer( 8 );
		}

		void m_tsbLayer9_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetPathLayer( 9 );
		}

		void m_tsbLayer10_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetPathLayer( 10 );
		}

		void m_tsbCalibrationPnt_Click( object sender, EventArgs e )
		{
			ShowThreePointCalibration();
		}

		bool HasCalibrationPart => m_DataManager != null && m_DataManager.PartIDList.Count > 0;

		void UpdateThreePointCalibrationEntry()
		{
			m_tsbCalibrationPnt.Enabled = HasCalibrationPart;
		}

		void OnCalibrationPartsRemoved( List<string> removedParts ) => UpdateThreePointCalibrationEntry();

		void ShowThreePointCalibration()
		{
			UpdateThreePointCalibrationEntry();
			if( !HasCalibrationPart ) {
				return;
			}
			ShowCNCPanels();
			if( m_CurrentEditor != m_CAMEditor ) {
				return;
			}
			try {
				m_CAMEditor.StartThreePointCalibration( this,
					m_msMode, m_tsCADFunction, m_tsCAMFunction, m_tsMachiningFunction, m_tsSimuFunc, m_tsFileFunction, m_tsLayer );
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC calibration initialization failed: {0}", ex );
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "Log_CalibrationError", LOG_CALIBRATION_ERROR ), ex.Message ), MyApp.NoticeType.Warning );
			}
		}

		void m_tsbSetOffset_Click( object sender, EventArgs e )
		{
			m_CNCUIManager?.ShowOffset( this );
		}

		void m_tsbProcessSettings_Click( object sender, EventArgs e )
		{
			m_CNCUIManager?.ShowProcessSettings();
		}

		void ApplyResourcesToUI()
		{
			// form title
			UpdateFormTitle();

			// menu bar
			m_tsmiFile.Text = m_LanguageManager.TryGetString( "m_tsmiFile", m_tsmiFile.Text );
			m_tsmiCAD.Text = m_LanguageManager.TryGetString( "m_tsmiCAD", m_tsmiCAD.Text );
			m_tsmiCAM.Text = m_LanguageManager.TryGetString( "m_tsmiCAM", m_tsmiCAM.Text );
			m_tsmiSimulation.Text = m_LanguageManager.TryGetString( "m_tsmiSimulation", m_tsmiSimulation.Text );
			m_tsmiSetting.Text = m_LanguageManager.TryGetString( "m_tsmiSetting", m_tsmiSetting.Text );
			m_tsmiCNCConnection.Text = m_LanguageManager.TryGetString( "m_tsmiCNCConnection", m_tsmiCNCConnection.Text );
			m_tsmiLanguage.Text = m_LanguageManager.TryGetString( "m_tsmiLanguage", m_tsmiLanguage.Text );
			m_tsmiController.Text = m_LanguageManager.TryGetString( "m_tsmiControllerAdvancedSettings", m_tsmiController.Text );
			m_tsmiCNC.Text = m_LanguageManager.TryGetString( "m_tsmiMachining", m_tsmiCNC.Text );
			m_CNCUIManager?.ApplyResources();

			// file toolbar
			m_tsbImport3DFile.Text = m_LanguageManager.TryGetString( "m_tsbImport3DFile", m_tsbImport3DFile.Text );
			m_tsbImport3DFile.ToolTipText = m_LanguageManager.TryGetString( "m_tsbImport3DFile_ToolTip", m_tsbImport3DFile.ToolTipText );

			m_tsbAddRevolution.Text = m_LanguageManager.TryGetString( "m_tsbAddRevolution", m_tsbAddRevolution.Text );
			m_tsbAddRevolution.ToolTipText = m_LanguageManager.TryGetString( "m_tsbAddRevolution_ToolTip", m_tsbAddRevolution.ToolTipText );

			m_tsbReadProjectFile.Text = m_LanguageManager.TryGetString( "m_tsbReadProjectFile", m_tsbReadProjectFile.Text );
			m_tsbReadProjectFile.ToolTipText = m_LanguageManager.TryGetString( "m_tsbReadProjectFile_ToolTip", m_tsbReadProjectFile.ToolTipText );

			m_tsbSaveProjectFile.Text = m_LanguageManager.TryGetString( "m_tsbSaveProjectFile", m_tsbSaveProjectFile.Text );
			m_tsbSaveProjectFile.ToolTipText = m_LanguageManager.TryGetString( "m_tsbSaveProjectFile_ToolTip", m_tsbSaveProjectFile.ToolTipText );

			m_tsbAddTube.Text = m_LanguageManager.TryGetString( "m_tsbAddTube", m_tsbAddTube.Text );
			m_tsbAddTube.ToolTipText = m_LanguageManager.TryGetString( "m_tsbAddTube_ToolTip", m_tsbAddTube.ToolTipText );

			// cad toolbar
			m_tsbSewPart.Text = m_LanguageManager.TryGetString( "m_tsbSewPart", m_tsbSewPart.Text );
			m_tsbSewPart.ToolTipText = m_LanguageManager.TryGetString( "m_tsbSewPart_ToolTip", m_tsbSewPart.ToolTipText );

			m_tsbDesignSupportBracket.Text = m_LanguageManager.TryGetString( "m_tsbDesignSupportBracket", m_tsbDesignSupportBracket.Text );
			m_tsbDesignSupportBracket.ToolTipText = m_LanguageManager.TryGetString( "m_tsbDesignSupportBracket_ToolTip", m_tsbDesignSupportBracket.ToolTipText );

			m_tsbAddPoint_AcrCenter.Text = m_LanguageManager.TryGetString( "m_tsbAddPoint_AcrCenter", m_tsbAddPoint_AcrCenter.Text );
			m_tsbAddPoint_AcrCenter.ToolTipText = m_LanguageManager.TryGetString( "m_tsbAddPoint_AcrCenter_ToolTip", m_tsbAddPoint_AcrCenter.ToolTipText );

			m_tsbAddPoint_EdgeCenter.Text = m_LanguageManager.TryGetString( "m_tsbAddPoint_EdgeCenter", m_tsbAddPoint_EdgeCenter.Text );
			m_tsbAddPoint_EdgeCenter.ToolTipText = m_LanguageManager.TryGetString( "m_tsbAddPoint_EdgeCenter_ToolTip", m_tsbAddPoint_EdgeCenter.ToolTipText );

			m_tsbAddPoint_PointCenter.Text = m_LanguageManager.TryGetString( "m_tsbAddPoint_PointCenter", m_tsbAddPoint_PointCenter.Text );
			m_tsbAddPoint_PointCenter.ToolTipText = m_LanguageManager.TryGetString( "m_tsbAddPoint_PointCenter_ToolTip", m_tsbAddPoint_PointCenter.ToolTipText );

			m_tsbAddPoint_BoundingBoxCenter.Text = m_LanguageManager.TryGetString( "m_tsbAddPoint_BoundingBoxCenter", m_tsbAddPoint_BoundingBoxCenter.Text );
			m_tsbAddPoint_BoundingBoxCenter.ToolTipText = m_LanguageManager.TryGetString( "m_tsbAddPoint_BoundingBoxCenter_ToolTip", m_tsbAddPoint_BoundingBoxCenter.ToolTipText );

			m_tsbAddLine_TwoVertexConnect.Text = m_LanguageManager.TryGetString( "m_tsbAddLine_TwoVertexConnect", m_tsbAddLine_TwoVertexConnect.Text );
			m_tsbAddLine_TwoVertexConnect.ToolTipText = m_LanguageManager.TryGetString( "m_tsbAddLine_TwoVertexConnect_ToolTip", m_tsbAddLine_TwoVertexConnect.ToolTipText );

			m_tsbManualTransform.Text = m_LanguageManager.TryGetString( "m_tsbManualTransform", m_tsbManualTransform.Text );
			m_tsbManualTransform.ToolTipText = m_LanguageManager.TryGetString( "m_tsbManualTransform_ToolTip", m_tsbManualTransform.ToolTipText );

			m_tsb3PntTransform.Text = m_LanguageManager.TryGetString( "m_tsb3PntTransform", m_tsb3PntTransform.Text );
			m_tsb3PntTransform.ToolTipText = m_LanguageManager.TryGetString( "m_tsb3PntTransform_ToolTip", m_tsb3PntTransform.ToolTipText );

			m_tsbAxisTransform.Text = m_LanguageManager.TryGetString( "m_tsbAxisTransform", m_tsbAxisTransform.Text );
			m_tsbAxisTransform.ToolTipText = m_LanguageManager.TryGetString( "m_tsbAxisTransform_ToolTip", m_tsbAxisTransform.ToolTipText );

			m_tsbMeasureCAD.Text = m_LanguageManager.TryGetString( "m_tsbMeasureCAD", m_tsbMeasureCAD.Text );
			m_tsbMeasureCAD.ToolTipText = m_LanguageManager.TryGetString( "m_tsbMeasureCAD_ToolTip", m_tsbMeasureCAD.ToolTipText );

			m_tsbWireDiscreteVertex.Text = m_LanguageManager.TryGetString( "m_tsbWireDiscreteVertex", m_tsbWireDiscreteVertex.Text );
			m_tsbWireDiscreteVertex.ToolTipText = m_LanguageManager.TryGetString( "m_tsbWireDiscreteVertex_ToolTip", m_tsbWireDiscreteVertex.ToolTipText );

			// cam toolbar
			m_tsbAddPath.Text = m_LanguageManager.TryGetString( "m_tsbAddPath", m_tsbAddPath.Text );
			m_tsbAddPath.ToolTipText = m_LanguageManager.TryGetString( "m_tsbAddPath_ToolTip", m_tsbAddPath.ToolTipText );

			m_tsbDeletePath.Text = m_LanguageManager.TryGetString( "m_tsbDeletePath", m_tsbDeletePath.Text );
			m_tsbDeletePath.ToolTipText = m_LanguageManager.TryGetString( "m_tsbDeletePath_ToolTip", m_tsbDeletePath.ToolTipText );

			m_tsbStartPoint.Text = m_LanguageManager.TryGetString( "m_tsbStartPoint", m_tsbStartPoint.Text );
			m_tsbStartPoint.ToolTipText = m_LanguageManager.TryGetString( "m_tsbStartPoint_ToolTip", m_tsbStartPoint.ToolTipText );

			m_tsbReverse.Text = m_LanguageManager.TryGetString( "m_tsbReverse", m_tsbReverse.Text );
			m_tsbReverse.ToolTipText = m_LanguageManager.TryGetString( "m_tsbReverse_ToolTip", m_tsbReverse.ToolTipText );

			m_tsbSetLead.Text = m_LanguageManager.TryGetString( "m_tsbSetLead", m_tsbSetLead.Text );
			m_tsbSetLead.ToolTipText = m_LanguageManager.TryGetString( "m_tsbSetLead_ToolTip", m_tsbSetLead.ToolTipText );

			m_tsbOverCut.Text = m_LanguageManager.TryGetString( "m_tsbOverCut", m_tsbOverCut.Text );
			m_tsbOverCut.ToolTipText = m_LanguageManager.TryGetString( "m_tsbOverCut_ToolTip", m_tsbOverCut.ToolTipText );

			m_tsbMicroJoint.Text = m_LanguageManager.TryGetString( "m_tsbMicroJoint", m_tsbMicroJoint.Text );
			m_tsbMicroJoint.ToolTipText = m_LanguageManager.TryGetString( "m_tsbMicroJoint_ToolTip", m_tsbMicroJoint.ToolTipText );

			m_tsbExtendedGCode.Text = m_LanguageManager.TryGetString( "m_tsbExtendedGCode", m_tsbExtendedGCode.Text );
			m_tsbExtendedGCode.ToolTipText = m_LanguageManager.TryGetString( "m_tsbExtendedGCode_ToolTip", m_tsbExtendedGCode.ToolTipText );

			m_tsbRCorner.Text = m_LanguageManager.TryGetString( "m_tsbRCorner", m_tsbRCorner.Text );
			m_tsbRCorner.ToolTipText = m_LanguageManager.TryGetString( "m_tsbRCorner_ToolTip", m_tsbRCorner.ToolTipText );

			m_tsbToolVecReverse.Text = m_LanguageManager.TryGetString( "m_tsbToolVecReverse", m_tsbToolVecReverse.Text );
			m_tsbToolVecReverse.ToolTipText = m_LanguageManager.TryGetString( "m_tsbToolVecReverse_ToolTip", m_tsbToolVecReverse.ToolTipText );

			m_tsbToolVec.Text = m_LanguageManager.TryGetString( "m_tsbToolVec", m_tsbToolVec.Text );
			m_tsbToolVec.ToolTipText = m_LanguageManager.TryGetString( "m_tsbToolVec_ToolTip", m_tsbToolVec.ToolTipText );

			m_tsbFixedToolVec.Text = m_LanguageManager.TryGetString( "m_tsbFixedToolVec", m_tsbFixedToolVec.Text );
			m_tsbFixedToolVec.ToolTipText = m_LanguageManager.TryGetString( "m_tsbFixedToolVec_ToolTip", m_tsbFixedToolVec.ToolTipText );

			m_tsbOptimizeIK.Text = m_LanguageManager.TryGetString( "m_tsbOptimizeIK", m_tsbOptimizeIK.Text );
			m_tsbOptimizeIK.ToolTipText = m_LanguageManager.TryGetString( "m_tsbOptimizeIK_ToolTip", m_tsbOptimizeIK.ToolTipText );

			m_tsbManualOrder.Text = m_LanguageManager.TryGetString( "m_tsbManualOrder", m_tsbManualOrder.Text );
			m_tsbManualOrder.ToolTipText = m_LanguageManager.TryGetString( "m_tsbManualOrder_ToolTip", m_tsbManualOrder.ToolTipText );

			m_tsbAutoOrder.Text = m_LanguageManager.TryGetString( "m_tsbAutoOrder", m_tsbAutoOrder.Text );
			m_tsbAutoOrder.ToolTipText = m_LanguageManager.TryGetString( "m_tsbAutoOrder_ToolTip", m_tsbAutoOrder.ToolTipText );

			m_tsbSetTraverseData.Text = m_LanguageManager.TryGetString( "m_tsbSetTraverseData", m_tsbSetTraverseData.Text );
			m_tsbSetTraverseData.ToolTipText = m_LanguageManager.TryGetString( "m_tsbSetTraverseData_ToolTip", m_tsbSetTraverseData.ToolTipText );

			m_tsbFlyCut.Text = m_LanguageManager.TryGetString( "m_tsbFlyCut", m_tsbFlyCut.Text );
			m_tsbFlyCut.ToolTipText = m_LanguageManager.TryGetString( "m_tsbFlyCut_ToolTip", m_tsbFlyCut.ToolTipText );

			m_tsbEntryAndExitSetting.Text = m_LanguageManager.TryGetString( "m_tsbEntryAndExitSetting", m_tsbEntryAndExitSetting.Text );
			m_tsbEntryAndExitSetting.ToolTipText = m_LanguageManager.TryGetString( "m_tsbEntryAndExitSetting_ToolTip", m_tsbEntryAndExitSetting.ToolTipText );

			m_tsbPatternSetting.Text = m_LanguageManager.TryGetString( "m_tsbPatternSetting", m_tsbPatternSetting.Text );
			m_tsbPatternSetting.ToolTipText = m_LanguageManager.TryGetString( "m_tsbPatternSetting_ToolTip", m_tsbPatternSetting.ToolTipText );

			m_tsbPathEdit.Text = m_LanguageManager.TryGetString( "m_tsbPathEdit", m_tsbPathEdit.Text );
			m_tsbPathEdit.ToolTipText = m_LanguageManager.TryGetString( "m_tsbPathEdit_ToolTip", m_tsbPathEdit.ToolTipText );

			m_tsbContourEdit.Text = m_LanguageManager.TryGetString( "m_BtnContourEdit", m_tsbContourEdit.Text );
			m_tsbContourEdit.ToolTipText = m_LanguageManager.TryGetString( "m_BtnContourEdit_ToolTip", m_tsbContourEdit.ToolTipText );

			m_tsbPathCompensation.Text = m_LanguageManager.TryGetString( "m_tsbPathCompensation", m_tsbPathCompensation.Text );
			m_tsbPathCompensation.ToolTipText = m_LanguageManager.TryGetString( "m_tsbPathCompensation_ToolTip", m_tsbPathCompensation.ToolTipText );

			m_tsbCalibrationPnt.Text = m_LanguageManager.TryGetString( "m_tsbCalibrationPnt", m_tsbCalibrationPnt.Text );
			m_tsbCalibrationPnt.ToolTipText = m_LanguageManager.TryGetString( "m_tsbCalibrationPnt_ToolTip", m_tsbCalibrationPnt.ToolTipText );

			m_tsbConvertNC.Text = m_LanguageManager.TryGetString( "m_tsbConvertNC", m_tsbConvertNC.Text );
			m_tsbConvertNC.ToolTipText = m_LanguageManager.TryGetString( "m_tsbConvertNC_ToolTip", m_tsbConvertNC.ToolTipText );
			m_tsbSetOffset.Text = m_LanguageManager.TryGetString( "m_tsbSetOffset", SET_OFFSET );
			m_tsbSetOffset.ToolTipText = m_LanguageManager.TryGetString( "m_tsbSetOffset_ToolTip", SET_OFFSET_TOOLTIP );
			m_tsbProcessSettings.Text = m_LanguageManager.TryGetString( "m_tsbProcessSettings", PROCESS_SETTINGS );
			m_tsbProcessSettings.ToolTipText = m_LanguageManager.TryGetString( "m_tsbProcessSettings_ToolTip", PROCESS_SETTINGS_TOOLTIP );

			m_tsbMeasureCAM.Text = m_LanguageManager.TryGetString( "m_tsbMeasureCAM", m_tsbMeasureCAM.Text );
			m_tsbMeasureCAM.ToolTipText = m_LanguageManager.TryGetString( "m_tsbMeasureCAM_ToolTip", m_tsbMeasureCAM.ToolTipText );

			// simulation toolbar
			m_tsbPlay.Text = m_LanguageManager.TryGetString( "m_tsbPlay", m_tsbPlay.Text );
			m_tsbPlay.ToolTipText = m_LanguageManager.TryGetString( "m_tsbPlay_ToolTip", m_tsbPlay.ToolTipText );

			m_tsbPause.Text = m_LanguageManager.TryGetString( "m_tsbPause", m_tsbPause.Text );
			m_tsbPause.ToolTipText = m_LanguageManager.TryGetString( "m_tsbPause_ToolTip", m_tsbPause.ToolTipText );

			m_tsbStop.Text = m_LanguageManager.TryGetString( "m_tsbStop", m_tsbStop.Text );
			m_tsbStop.ToolTipText = m_LanguageManager.TryGetString( "m_tsbStop_ToolTip", m_tsbStop.ToolTipText );

			m_tsbPreviousPath.Text = m_LanguageManager.TryGetString( "m_tsbPreviousPath", m_tsbPreviousPath.Text );
			m_tsbPreviousPath.ToolTipText = m_LanguageManager.TryGetString( "m_tsbPreviousPath_ToolTip", m_tsbPreviousPath.ToolTipText );

			m_tsbNextPath.Text = m_LanguageManager.TryGetString( "m_tsbNextPath", m_tsbNextPath.Text );
			m_tsbNextPath.ToolTipText = m_LanguageManager.TryGetString( "m_tsbNextPath_ToolTip", m_tsbNextPath.ToolTipText );

			m_tsbPreCollision.Text = m_LanguageManager.TryGetString( "m_tsbPreCollision", m_tsbPreCollision.Text );
			m_tsbPreCollision.ToolTipText = m_LanguageManager.TryGetString( "m_tsbPreCollision_ToolTip", m_tsbPreCollision.ToolTipText );

			m_tsbNextCol.Text = m_LanguageManager.TryGetString( "m_tsbNextCol", m_tsbNextCol.Text );
			m_tsbNextCol.ToolTipText = m_LanguageManager.TryGetString( "m_tsbNextCol_ToolTip", m_tsbNextCol.ToolTipText );

			m_tsbPrePoint.Text = m_LanguageManager.TryGetString( "m_tsbPrePoint", m_tsbPrePoint.Text );
			m_tsbPrePoint.ToolTipText = m_LanguageManager.TryGetString( "m_tsbPrePoint_ToolTip", m_tsbPrePoint.ToolTipText );

			m_tsbNextPoint.Text = m_LanguageManager.TryGetString( "m_tsbNextPoint", m_tsbNextPoint.Text );
			m_tsbNextPoint.ToolTipText = m_LanguageManager.TryGetString( "m_tsbNextPoint_ToolTip", m_tsbNextPoint.ToolTipText );

			m_tsbSpeedUp.Text = m_LanguageManager.TryGetString( "m_tsbSpeedUp", m_tsbSpeedUp.Text );
			m_tsbSpeedUp.ToolTipText = m_LanguageManager.TryGetString( "m_tsbSpeedUp_ToolTip", m_tsbSpeedUp.ToolTipText );

			m_tsbSlowDown.Text = m_LanguageManager.TryGetString( "m_tsbSlowDown", m_tsbSlowDown.Text );
			m_tsbSlowDown.ToolTipText = m_LanguageManager.TryGetString( "m_tsbSlowDown_ToolTip", m_tsbSlowDown.ToolTipText );

			m_tsbCheck.Text = m_LanguageManager.TryGetString( "m_tsbCheck", m_tsbCheck.Text );
			m_tsbCheck.ToolTipText = m_LanguageManager.TryGetString( "m_tsbCheck_ToolTip", m_tsbCheck.ToolTipText );

			m_tsbShowVec.ToolTipText = m_LanguageManager.TryGetString( "ShowVec", m_tsbShowVec.ToolTipText );
			m_tsbShowOrder.ToolTipText = m_LanguageManager.TryGetString( "ShowOrder", m_tsbShowOrder.ToolTipText );
			m_tsbShowOrientation.ToolTipText = m_LanguageManager.TryGetString( "ShowOrientation", m_tsbShowOrientation.ToolTipText );
			m_tsbShowTraverse.ToolTipText = m_LanguageManager.TryGetString( "ShowTraverse", m_tsbShowTraverse.ToolTipText );
			m_tsbShowMarker.ToolTipText = m_LanguageManager.TryGetString( "ShowMarker", m_tsbShowMarker.ToolTipText );

			m_tsbLayer1.ToolTipText = string.Format( m_LanguageManager.TryGetString( "Layer", LAYER ), 1 );
			m_tsbLayer2.ToolTipText = string.Format( m_LanguageManager.TryGetString( "Layer", LAYER ), 2 );
			m_tsbLayer3.ToolTipText = string.Format( m_LanguageManager.TryGetString( "Layer", LAYER ), 3 );
			m_tsbLayer4.ToolTipText = string.Format( m_LanguageManager.TryGetString( "Layer", LAYER ), 4 );
			m_tsbLayer5.ToolTipText = string.Format( m_LanguageManager.TryGetString( "Layer", LAYER ), 5 );
			m_tsbLayer6.ToolTipText = string.Format( m_LanguageManager.TryGetString( "Layer", LAYER ), 6 );
			m_tsbLayer7.ToolTipText = string.Format( m_LanguageManager.TryGetString( "Layer", LAYER ), 7 );
			m_tsbLayer8.ToolTipText = string.Format( m_LanguageManager.TryGetString( "Layer", LAYER ), 8 );
			m_tsbLayer9.ToolTipText = string.Format( m_LanguageManager.TryGetString( "Layer", LAYER ), 9 );
			m_tsbLayer10.ToolTipText = string.Format( m_LanguageManager.TryGetString( "Layer", LAYER ), 10 );
			m_tsbLayerNone.ToolTipText = m_LanguageManager.TryGetString( "m_tsbLayerNone_ToolTip", NOT_NEED_PROCESS );
			m_tsbNeedProcess.ToolTipText = m_LanguageManager.TryGetString( "m_tsbNeedProcess_ToolTip", NEED_PROCESS );
		}

		void InitLanguageMenu()
		{
			m_tsmiLanguage.DropDownItems.Clear();
			foreach( string szLanguage in LanguageManager.LanguageManager.CurrentSupportedLanguageList ) {
				string szDisplayName = GetLanguageDisplayName( szLanguage );
				ToolStripMenuItem item = new ToolStripMenuItem( szDisplayName );
				item.Tag = szLanguage;

				// capture loop variable for closure
				string szCapturedLanguage = szLanguage;
				item.Click += ( sender, e ) => OnLanguageMenuItemClick( szCapturedLanguage );
				m_tsmiLanguage.DropDownItems.Add( item );
			}
			UpdateLanguageMenuCheckMark();
		}

		string GetLanguageDisplayName( string szLanguage )
		{
			string szNativeName = LanguageManager.LanguageManager.GetNativeNameByLanguage( szLanguage );
			return string.IsNullOrEmpty( szNativeName ) ? szLanguage : szNativeName;
		}

		void OnLanguageMenuItemClick( string szLanguage )
		{
			// switch language
			LanguageManager.LanguageManager.CurrentUILanguage = szLanguage;

			// the main window and screens created from now on use the font family of the new language
			ThemeFonts.Initialize( LanguageManager.LanguageManager.CurrentUILanguage );
			ApplyThemeFont();

			// persist setting to file
			MyApp.AppSetting.ApplyUILanguage( szLanguage );

			// refresh StartupForm UI with the new language
			ApplyResourcesToUI();
			UpdateLanguageMenuCheckMark();
		}

		void UpdateLanguageMenuCheckMark()
		{
			foreach( ToolStripMenuItem item in m_tsmiLanguage.DropDownItems ) {
				string szItemLanguage = item.Tag as string;
				item.Checked = LanguageManager.LanguageManager.CompareLanguageTag(
					szItemLanguage, LanguageManager.LanguageManager.CurrentUILanguage );
			}
		}

		const string LAYER = "圖層";
		const string SET_OFFSET = "設定偏置";
		const string SET_OFFSET_TOOLTIP = "設定程式座標偏置";
		const string PROCESS_SETTINGS = "工藝設置";
		const string PROCESS_SETTINGS_TOOLTIP = "開啟控制器工藝設置";
		const string LOG_CALIBRATION_ERROR = "無法開啟三點校正：{0}";
		const string LOG_NC_EXPORT_ERROR = "NC 轉出流程失敗：{0}";
		const String NEED_PROCESS = "加工";
		const string NOT_NEED_PROCESS = "不加工";
	}
}
