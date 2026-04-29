using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor;
using MyCAM.Editor.Dialog;
using MyCAM.Helper;
using OCC.AIS;
using OCC.Geom;
using OCC.gp;
using OCC.Quantity;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using VNCFormsApp;

namespace MyCAM
{
	public partial class StartupForm : Form
	{
		public Action<bool> RaiseShowVecStatusChange;
		public Action<bool> RaiseShowOrderStatusChange;
		public Action<bool> RaiseShowOrientStatusChange;
		public Action<bool> RaiseShowTraverseStatusChange;

		public StartupForm()
		{
#if !DEBUG
			if( FALicenseChecker.LicenseChecker.IsLicenseActivated() == false ) {
				MyApp.Logger.ShowOnLogPanel( "授權流程不正確", MyApp.NoticeType.Error );
				Environment.Exit( 0 );
			}
#endif

			// need to init befor myapp because myapp will use LogPanel
			InitializeComponent();

			// app
			MyApp.MainForm = this;
			UIListSetting();

			// create the viewer
			m_Viewer = new Viewer();
			bool bSucess = m_Viewer.InitViewer( m_panViewer );
			if( !bSucess ) {
				MyApp.Logger.ShowOnLogPanel( "初始化Viewer錯誤，請檢查套件是否安裝齊全", MyApp.NoticeType.Error );
				return;
			}
			m_Viewer.UpdateView();

			// multi select tree view
			m_TreeView = new MultiSelectTreeView();
			m_TreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			m_TreeView.Location = new System.Drawing.Point( 0, 0 );
			m_TreeView.Name = "m_TreeView";
			m_TreeView.TabIndex = 0;
			m_TreeView.ForeColor = System.Drawing.Color.Black;
			m_panTreeView.Controls.Add( this.m_TreeView );

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
				MyApp.Logger.ShowOnLogPanel( "使用默認機構專案檔", MyApp.NoticeType.Warning );
			}

			// get machine mesh by type, so data manager need to be builded already
			bool isGetSuccess = GetMachineMesh( m_DataManager.MachineData.FiveAxisType, out MachineMeshes machineMeshes );
			if( isGetSuccess ) {
				m_DataManager.MachineMeshes = machineMeshes;
			}
			DataGettingHelper.Initialize( m_DataManager );

			// CAD Editor
			m_CADEditor = new CADEditor( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			m_CADEditor.RaiseCADActionStatusChange += OnCADActionStatusChange;

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

			// simu editor
			m_SimuEditor = new SimuEditor( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			m_SimuEditor.ReadStlSuccess = OnMachineStlReadSuccess;
			m_SimuEditor.Init();

			// start with CAD editor
			SwitchEditor( EEditorType.CAD );
			DefaultUISetting();

			// Get cust post data
			MyApp.SetCustPost();
		}

		public Panel GetLogPanel
		{
			get
			{
				return m_pnlLog;
			}
		}

		protected override void WndProc( ref Message m )
		{
			base.WndProc( ref m );
			if( m.Msg == WINDOWS_Drag_Event ) {
				AdjustToCurrentScreen();
			}
		}

		const int WINDOWS_Drag_Event = 0x0232;

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

		// UI list
		Dictionary<EUIStatus, List<Control>> m_UIStatusDic;
		List<ToolStripContainer> m_ToolStripLevelList;

		// UI color
		readonly Color DEFAULT_BtnColor = SystemColors.Control;
		readonly Color ON_ButtonColor = Color.FromArgb( 233, 180, 159 );

		enum EUIStatus
		{
			File,
			CAD,
			CAM,
			Simulation,

			// cad function	
			ManualTransForm,

			// cam function
			AddPath,
			ManualSelectPath,
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

		void m_tsbImportProjectFile_Click( object sender, EventArgs e )
		{
			m_CADEditor.ImportProjectFile();
		}

		void m_tsbSaveProjectFile_Click( object sender, EventArgs e )
		{
			m_CADEditor.SaveProjectFile();
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
			m_CADEditor.StartManaulTransform();
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
			m_CAMEditor.StartSelectFace();
		}

		void m_tsAutoFindAlienatedWorkPieceBoundary_Click( object sender, EventArgs e )
		{
			m_CAMEditor.AutoFindAlienatedWorkPieceFaceAndSelectFreeBound();
		}

		void m_tsAutoFindStretchedWorkPieceBoundary_Click( object sender, EventArgs e )
		{
			m_CAMEditor.AutoFindStretchedWorkPieceFaceAndSelectFreeBound();
		}

		void m_tsbSelectD1ContFace_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SelectD1ContFace();
		}

		void m_tsbSelPath_FreeBound_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SelectPath_FreeBound();
		}

		void m_tsbManualSelectPathOK_Click( object sender, EventArgs e )
		{
			m_CAMEditor.EndSelectPath_Manual();
		}

		void m_tsbSelPath_Manual_Click( object sender, EventArgs e )
		{
			m_CAMEditor.StartSelectPath_Manual();
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
			m_CAMEditor.SetFixedToolVec();
		}

		void m_tsbOptimizeIK_Click( object sender, EventArgs e )
		{
			m_CAMEditor.AutoOptimizeIKContinuity();
		}

		// sort
		void m_tsbMoveUp_Click( object sender, EventArgs e )
		{
			m_CAMEditor.MoveProcess( true );
		}

		void m_tsbMoveDown_Click( object sender, EventArgs e )
		{
			m_CAMEditor.MoveProcess( false );
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

		void m_BtnContourEdit_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetContourEdit();
		}

		void m_tsbPathCompensation_Click( object sender, EventArgs e )
		{
			m_CAMEditor.SetPathCompensation();
		}

		// convert NC
		void m_tsbConvertNC_Click( object sender, EventArgs e )
		{
			m_CAMEditor.ConverNC();
			// simulation
			// m_CAMEditor.EditEnd();
			// m_SimuEditor.EditStart();
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
				}
				return;
			}

			// action start, need to relay out
			if( action == EditActionType.ManualTransform ) {
				RefreshToolStripLayout( EUIStatus.ManualTransForm );
			}
			else {
				RefreshToolStripLayout( EUIStatus.CAD );
			}

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
			}
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
					case EditActionType.SelectFace:
						m_tsbAddPath.BackColor = DEFAULT_BtnColor;
						break;
					case EditActionType.SelectPath:
						m_tsbSelectD1ContFace.Enabled = true;
						m_tsbSelPath_FreeBound.Enabled = true;
						m_tsbAddPath.BackColor = DEFAULT_BtnColor;
						m_tsbSelPath_Manual.BackColor = DEFAULT_BtnColor;
						break;
					case EditActionType.StartPoint:
						m_tsbStartPoint.BackColor = DEFAULT_BtnColor;
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
					case EditActionType.SetEntryAndExitParam:
						m_tsbEntryAndExitSetting.BackColor = DEFAULT_BtnColor;
						OnCAMDlgActionStatusChange( actionStatus );
						break;
					default:
						break;
				}
				return;
			}

			// action start, need to relay out
			if( action == EditActionType.SelectFace ) {
				RefreshToolStripLayout( EUIStatus.AddPath );
			}
			else if( action == EditActionType.SelectPath ) {
				RefreshToolStripLayout( EUIStatus.ManualSelectPath );
			}
			else {
				RefreshToolStripLayout( EUIStatus.CAM );
			}
			switch( action ) {
				case EditActionType.SelectFace:
					m_tsbAddPath.BackColor = ON_ButtonColor;
					break;
				case EditActionType.SelectPath:

					// add path icon still need to light up
					m_tsbAddPath.BackColor = ON_ButtonColor;
					m_tsbSelPath_Manual.BackColor = ON_ButtonColor;
					m_tsbSelectD1ContFace.Enabled = false;
					m_tsbSelPath_FreeBound.Enabled = false;
					break;
				case EditActionType.StartPoint:
					m_tsbStartPoint.BackColor = ON_ButtonColor;
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
				case EditActionType.SetEntryAndExitParam:
					m_tsbEntryAndExitSetting.BackColor = ON_ButtonColor;
					OnCAMDlgActionStatusChange( actionStatus );
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
			m_tsbMoveUp.Enabled = editableInfo.IsMoveProcessEditable;
			m_tsbMoveDown.Enabled = editableInfo.IsMoveProcessEditable;
			m_tsbAutoOrder.Enabled = editableInfo.IsAutoOrderEditable;
			m_tsbReverse.Enabled = editableInfo.IsPathReverseEditable;
			m_tsbPathCompensation.Enabled = editableInfo.IsPathCompensateEditable;
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
			if( !m_UIStatusDic.ContainsKey( uiStatus ) || m_UIStatusDic[ uiStatus ] == null ) {
				return;
			}

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
			if( m_panBackGround.Visible ) {
				Controls.SetChildIndex( m_panBackGround, currentIndex++ );
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
			foreach( ToolStripButton btn in m_tsCAMFunction.Items ) {
				btn.Enabled = true;
			}
		}

		void VisibleCAMQuickToolBarButtons( bool isVisible )
		{
			m_tsbShowVec.Visible = isVisible;
			m_tsbShowOrder.Visible = isVisible;
			m_tsbShowOrientation.Visible = isVisible;
			m_tsbShowTraverse.Visible = isVisible;
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
				{ EUIStatus.Simulation, new List<Control>() { m_tsSimuFunc } },

				// cad function
				{ EUIStatus.ManualTransForm, new List<Control>(){m_tsCADFunction, m_tsManualTrans } },
				
				// cam function
				{ EUIStatus.AddPath, new List<Control>(){ m_tsCAMFunction, m_tsAddPathSubFunc } },
				{ EUIStatus.ManualSelectPath, new List<Control>(){ m_tsCAMFunction, m_tsAddPathSubFunc, m_tsSelectPath } }
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
						MyApp.Logger.ShowOnLogPanel( "機構參數節點為空,使用默認機構參數", MyApp.NoticeType.Warning );
						return false;
					case EMachineDataLoadStatus.NullFile:
						MyApp.Logger.ShowOnLogPanel( "機構檔案為空,使用默認機構參數", MyApp.NoticeType.Warning );
						return false;
					case EMachineDataLoadStatus.InvalidPrValue:
						MyApp.Logger.ShowOnLogPanel( $"機構參數錯誤(Pr:{nErrorPrIndex}),使用默認機構參數", MyApp.NoticeType.Warning );
						return false;
					case EMachineDataLoadStatus.TreeInvalid:
						MyApp.Logger.ShowOnLogPanel( "機構樹結構錯誤,使用默認機構樹", MyApp.NoticeType.Warning );
						return true;
					default:
						return true;
				}
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( $"讀取機構檔案失敗：\n{ex.Message},使用默認機構參數", MyApp.NoticeType.Error );
				return false;
			}
		}

		bool GetMachineMesh( FiveAxisType fiveAxis, out MachineMeshes machineMeshes )
		{
			string exeDir = AppDomain.CurrentDomain.BaseDirectory;
			string szFolderName;
			switch( fiveAxis ) {
				case FiveAxisType.Table:
					szFolderName = Path.Combine( exeDir, "Table" );
					break;
				case FiveAxisType.Mix:
					szFolderName = Path.Combine( exeDir, "Mix" );
					break;
				case FiveAxisType.Spindle:
				default:
					szFolderName = Path.Combine( exeDir, "Spindle" );
					break;
			}
			bool isGetMesh = ReadMachineMeshHelper.LoadMachineMeshes( szFolderName, out machineMeshes );
			return isGetMesh;
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
			MachineTreeNode ToolNode = new MachineTreeNode( MachineComponentType.Laser );
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
				MyApp.Logger.ShowOnLogPanel( $"儲存機構檔案失敗：\n{ex.Message}", MyApp.NoticeType.Error );
			}
		}

		#endregion

		#region VNC Connection

		bool SetAndDoConnect()
		{
			bool isConnectSuccess = false;

			// Create new ConnectDlg
			ConnectDlg ConnectDlg = new ConnectDlg();

			// Handle form closed event to clear reference
			ConnectDlg.FormClosed += ( s, args ) =>
			{
				ConnectDlg = null;
			};

			// Handle confirm event
			ConnectDlg.ConfirmEvent += ( ip ) =>
			{
				CleanupVNCConnections();
				MyApp.CNCIP = null;
				VNCUserControl newVNCControl = new VNCUserControl( ip );
				MyApp.VNCScreen = newVNCControl;
				ConnectDlg.Close();
				switch( newVNCControl.ConnectionResult ) {
					case VNCConnectionResult.Success:
						MyApp.CNCIP = ip;
						isConnectSuccess = true;
						break;
					case VNCConnectionResult.FileNotFound:
						MessageBox.Show( MyApp.MainForm, "找不到VNC執行檔，請確認VNC執行檔與程式執行檔在同一資料夾下", "外部程式調用錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error );
						break;
					case VNCConnectionResult.ConnectionFailed:
						MessageBox.Show( MyApp.MainForm, "無法連線到指定IP位置", "連線錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error );
						break;
					case VNCConnectionResult.OtherError:
						MessageBox.Show( MyApp.MainForm, "VNC連線過程發生未知錯誤", "連線錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error );
						break;
				}
			};

			// Show the dialog and set focus
			ConnectDlg.ShowDialog( MyApp.MainForm );
			return isConnectSuccess;
		}

		void m_tsbVNCConnection_Click( object sender, EventArgs e )
		{
			bool isConnectSuccess = SetAndDoConnect();

			// if connect success and in vertical screen, show vnc screen
			if( isConnectSuccess && m_panVNC.Visible ) {
				MyApp.VNCScreen.ScreenResizeByParentSize( m_panVNC.Width, m_panVNC.Height, out int nVNCStartX, out int nVNCStartY );
				MyApp.VNCScreen.Location = new Point( nVNCStartX, nVNCStartY );
				m_panVNC.Controls.Add( MyApp.VNCScreen );
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

		void SetVerticalScreenMode( bool isVertical )
		{
			if( isVertical ) {
				m_panVNC.Visible = true;
				m_tsbController.Visible = false;
				m_panVNC.Controls.Add( MyApp.VNCScreen );
			}

			// hide VNC screen and show controller entrance when in horizontal screen
			else {
				m_panVNC.Visible = false;
				m_tsbController.Visible = true;
				m_panVNC.Controls.Remove( MyApp.VNCScreen );
			}
		}

		void StartupForm_Resize( object sender, EventArgs e )
		{
			bool isVertical = Height > Width;
			if( isVertical ) {
				SetVerticalScreenMode( isVertical );

				// CADCAM
				m_panBackGround.RowStyles[ 0 ].SizeType = SizeType.Percent;
				m_panBackGround.RowStyles[ 0 ].Height = 47;

				// Log
				m_panBackGround.RowStyles[ 1 ].SizeType = SizeType.Percent;
				m_panBackGround.RowStyles[ 1 ].Height = 6;

				// VNC
				m_panBackGround.RowStyles[ 2 ].SizeType = SizeType.Percent;
				m_panBackGround.RowStyles[ 2 ].Height = 47;

				// resize VNC and reset position 
				if( m_panVNC.Controls.Count != 0 ) {
					( (VNCUserControl)m_panVNC.Controls[ 0 ] ).ScreenResizeByParentSize( m_panVNC.Width, m_panVNC.Height, out int nVNCStartX, out int nVNCStartY );
					m_panVNC.Controls[ 0 ].Location = new Point( nVNCStartX, nVNCStartY );
				}
				return;
			}
			else {
				SetVerticalScreenMode( isVertical );

				// VNC
				m_panBackGround.RowStyles[ 2 ].SizeType = SizeType.Absolute;
				m_panBackGround.RowStyles[ 2 ].Height = 0;

				// CADCAM
				m_panBackGround.RowStyles[ 0 ].SizeType = SizeType.Percent;
				m_panBackGround.RowStyles[ 0 ].Height = 90;

				// Log
				m_panBackGround.RowStyles[ 1 ].SizeType = SizeType.Percent;
				m_panBackGround.RowStyles[ 1 ].Height = 10;
			}
		}

		void StartupForm_FormClosing( object sender, FormClosingEventArgs e )
		{
			CleanupVNCConnections();
		}

		#endregion

		#region Simulation mode

		void m_tsmiSimulation_Click( object sender, EventArgs e )
		{
			RefreshToolStripLayout( EUIStatus.Simulation );

			// not in cam editing viewer tool bar cam buttons can't be used
			VisibleCAMQuickToolBarButtons( false );
			VisibleTechLayerButtons( false );

			// refresh main form UI layout, before start simu editor because start simu editor will trigger show machine
			SwitchEditor( EEditorType.Simulation );
		}

		void m_btnPlay_Click( object sender, EventArgs e )
		{
			m_SimuEditor.PlaySimulation();
		}

		void m_btnPause_Click( object sender, EventArgs e )
		{
			m_SimuEditor.PauseSimulation();
		}

		void m_btnStop_Click( object sender, EventArgs e )
		{
			m_SimuEditor.StopSimulation();
		}

		#endregion

		void m_btnPreviousPath_Click( object sender, EventArgs e )
		{
			m_SimuEditor.PlayPrePath();
		}

		void m_btnNextPath_Click( object sender, EventArgs e )
		{
			m_SimuEditor.PlayNextPath();
		}

		void m_btnPreCollision_Click( object sender, EventArgs e )
		{
			m_SimuEditor.MoveToPreCollision();
		}

		void m_btnNextCol_Click( object sender, EventArgs e )
		{
			m_SimuEditor.MoveToNextCollision();
		}

		void m_btnSpeedUp_Click( object sender, EventArgs e )
		{
			m_SimuEditor.SpeedUp();
		}

		void m_btnSlowDonw_Click( object sender, EventArgs e )
		{
			m_SimuEditor.SlowDown();
		}

		void m_btnCheck_Click( object sender, EventArgs e )
		{
			m_SimuEditor.CheckExitCollision();
		}

		void m_btnPrePoint_Click( object sender, EventArgs e )
		{
			m_SimuEditor.PrePoint();
		}

		void m_btnNextPoint_Click( object sender, EventArgs e )
		{
			m_SimuEditor.NextPoint();
		}

		void m_tsbController_Click( object sender, EventArgs e )
		{
			bool isConnectSuccess = true;
			if( string.IsNullOrWhiteSpace( MyApp.CNCIP ) ) {
				isConnectSuccess = SetAndDoConnect();
			}
			if( isConnectSuccess == false ) {
				return;
			}
			ControllerDlg controllerDlg = new ControllerDlg();

			// show dialog at center of main form
			controllerDlg.StartPosition = FormStartPosition.Manual;
			controllerDlg.Location = MyApp.CalculateDialogCenterLocation( controllerDlg );
			controllerDlg.PutVNCOnDlg();
			controllerDlg.ShowDialog( MyApp.MainForm );
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
			m_CAMEditor.SetCalibrationREF();
		}
	}
}
