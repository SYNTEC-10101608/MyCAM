using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using MyCAM.Editor.Renderer;
using OCC.BRep;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class CalibrationREFAction : KeyMouseActionBase
	{
		public CalibrationREFAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
			if( DataGettingHelper.GetREFPnt( out CalibrationData calibrationData ) ) {
				m_BackupData = calibrationData;
			}
			else {
				m_BackupData = new CalibrationData();
			}
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.CalibrationREF;
			}
		}

		public override void Start()
		{
			base.Start();

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );
			m_LastSelectPntIndex = DEFAULT_Select_Index;

			// disable tree view
			m_TreeView.Enabled = false;

			m_REFPntRender = new REFPointRenderer( m_Viewer, m_DataManager );
			m_Dlg = new CalibrationREFDlg( m_BackupData.Clone() );

			// delete point will trigger change action state
			m_Dlg.DeletePoint = ChangeActionState;
			m_Dlg.DeleteAllPoints = DeleteAllPoints;

			// to get key down event for delete point by keyboard
			m_Dlg.DeleteKeyDown += ViewerKeyDown;
			m_Dlg.Confirm += Confirm;
			m_Dlg.Cancel += CancelParamSetting;
			m_Dlg.Preview += PreviewREFPointResult;
			m_Dlg.Show( MyApp.MainForm );

			// set edit state beacuse different state with different UI protection
			SetInitEditState();

			// set current calibration data to renderer for preview
			RefreshREFAisInfo();
		}

		public override void End()
		{
			// clear selection
			m_Viewer.GetAISContext().ClearSelected( false );

			// enable tree view
			m_TreeView.Enabled = true;

			// deactivate
			DeactiveShapeVertexSelectMode();
			RemoveREFAIsInfo();
			base.End();
		}

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			if( e.Button != MouseButtons.Left ) {
				return;
			}

			// get selected point
			m_Viewer.Select();
			bool bSelected = GetSelectPoint( out gp_Pnt selectedPoint );
			if( !bSelected ) {
				return;
			}

			// at this state only REF point can be slecet
			if( ActionState == EActionStage.Done ) {
				m_LastSelectPntIndex = m_REFPntRender.GetSelectIndex();
				return;
			}
			switch( ActionState ) {
				case EActionStage.P1:
					ClickForNewPnt1( selectedPoint );
					break;
				case EActionStage.P2:
					ClickForNewPnt2( selectedPoint );
					break;
				case EActionStage.P3:
					ClickForNewPnt3( selectedPoint );
					break;
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
			if( e.KeyCode == Keys.Delete ) {
				if( m_LastSelectPntIndex == DEFAULT_Select_Index ) {
					return;
				}
				if( m_LastSelectPntIndex == (int)EActionStage.P1 ) {
					m_Dlg.DeletPnt1();
					return;
				}
				if( m_LastSelectPntIndex == (int)EActionStage.P2 ) {
					m_Dlg.DeletPnt2();
					return;
				}
				if( m_LastSelectPntIndex == (int)EActionStage.P3 ) {
					m_Dlg.DeletPnt3();
					return;
				}
			}
		}

		int m_LastSelectPntIndex = DEFAULT_Select_Index;
		const int DEFAULT_Select_Index = 0;

		enum EActionStage
		{
			P1 = 1,
			P2 = 2,
			P3 = 3,
			Done = 4,
		}

		REFPointRenderer m_REFPntRender;

		// set stage by ActionState, don't set this variable directly, because set stage will call UI projection to update UI state
		EActionStage m_ActionStage;

		// different stage means different UI protection, so set action stage will call UI projection to update UI state
		EActionStage ActionState
		{
			get
			{
				return m_ActionStage;
			}
			set
			{
				m_ActionStage = value;
				UIProection();

			}
		}

		CalibrationData m_BackupData = null;
		CalibrationREFDlg m_Dlg = null;

		#region UI init setting

		void SetInitEditState()
		{
			if( m_BackupData.IsBeenSet == false ) {

				// start from click point 1
				ActionState = EActionStage.P1;
			}
			else {
				ActionState = EActionStage.Done;
			}
		}

		#endregion

		#region catch event from calbration dialog

		void PreviewREFPointResult( CalibrationData calibration )
		{
			m_DataManager.CalibrationData = calibration;
			RefreshREFAisInfo();
		}

		void Confirm( CalibrationData calibration )
		{
			m_DataManager.CalibrationData = calibration;

			// UI refresh will be called in End()
			End();
		}

		void CancelParamSetting()
		{
			m_DataManager.CalibrationData = m_BackupData;
			End();
		}

		void DeleteAllPoints()
		{
			RefreshREFAisInfo();
			ActionState = EActionStage.P1;
			ActiveShapeVertexSelectMode();
		}

		void ChangeActionState( int nIndex = (int)EActionStage.Done )
		{
			switch( nIndex ) {
				case (int)EActionStage.P1:
					ActionState = EActionStage.P1;
					break;
				case (int)EActionStage.P2:
					ActionState = EActionStage.P2;
					break;
				case (int)EActionStage.P3:
					ActionState = EActionStage.P3;
					break;
				default:
					ActionState = EActionStage.Done;
					break;
			}
		}

		#endregion

		bool GetSelectPoint( out gp_Pnt point )
		{
			point = new gp_Pnt();
			m_Viewer.GetAISContext().InitSelected();
			if( !m_Viewer.GetAISContext().MoreSelected() ) {
				return false;
			}
			TopoDS_Shape selectedShape = m_Viewer.GetAISContext().SelectedShape();
			if( selectedShape == null || selectedShape.IsNull() ) {
				return false;
			}
			if( selectedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				return false;
			}
			TopoDS_Vertex vertex = TopoDS.ToVertex( selectedShape );
			point = BRep_Tool.Pnt( vertex );
			return true;
		}

		void ClickForNewPnt1( gp_Pnt selectPnt )
		{
			m_Dlg.SetPnt1( selectPnt );
			StopVertexSelection();

			// all point has been set, stop selection
			if( ActionState == EActionStage.Done ) {
				return;
			}

			// go to select point 2
			ChangeActionState( (int)EActionStage.P2 );
		}

		void ClickForNewPnt2( gp_Pnt selectPnt )
		{
			m_Dlg.SetPnt2( selectPnt );

			// all point has been set, stop selection
			StopVertexSelection();
			if( ActionState == EActionStage.Done ) {
				return;
			}

			// go to select point 3
			ChangeActionState( (int)EActionStage.P3 );
		}

		void ClickForNewPnt3( gp_Pnt selectPnt )
		{
			m_Dlg.SetPnt3( selectPnt );
			ChangeActionState();
		}

		#region Select Mode Change

		void UIProection()
		{
			m_Dlg.ChangeUIState( (int)ActionState );

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( false );

			if( ActionState == EActionStage.Done ) {
				DeactiveShapeVertexSelectMode();
			}
			else {
				ActiveShapeVertexSelectMode();
			}
			m_Viewer.UpdateView();
		}

		void ActiveShapeVertexSelectMode()
		{
			foreach( var partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ partID ].AISHandle, (int)AISActiveMode.Vertex );
			}
		}

		void DeactiveShapeVertexSelectMode()
		{
			foreach( var partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				m_Viewer.GetAISContext().Deactivate( m_ViewManager.ViewObjectMap[ partID ].AISHandle, (int)AISActiveMode.Vertex );
			}
		}

		// when add new ref pnt, need to call this to check
		void StopVertexSelection()
		{
			bool isGetSuccess = DataGettingHelper.GetREFPnt( out CalibrationData calibrationData );
			if( !isGetSuccess ) {
				return;
			}
			if( calibrationData.Ref_Pnt1 != null && calibrationData.Ref_Pnt2 != null && calibrationData.Ref_Pnt3 != null ) {
				ActionState = EActionStage.Done;
			}
		}

		#endregion

		#region Refresh UI

		void RemoveREFAIsInfo()
		{
			m_REFPntRender.Remove();
			m_Viewer.UpdateView();
		}

		void RefreshREFAisInfo()
		{
			m_REFPntRender.Show();
			m_Viewer.UpdateView();
		}

		#endregion
	}
}
