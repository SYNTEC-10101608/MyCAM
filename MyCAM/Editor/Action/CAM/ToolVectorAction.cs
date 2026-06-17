using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Renderer;
using MyCAM.Helper;
using MyCAM.PathCache;
using MyCAM.Post;
using OCC.AIS;
using OCC.Aspect;
using OCC.gp;
using OCC.Prs3d;
using OCC.Quantity;
using OCC.TopoDS;
using OCC.V3d;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class ToolVectorAction : EditActionBase
	{
		public ToolVectorAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, string pathID, SelectPathAction pathIndexAction )
			: base( dataManager )
		{
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;

			// for path index control
			m_PathIndexAction = pathIndexAction;
			m_CurrentPathID = pathID;

			// for edit action
			m_RotaryAxisConfig = CreateRotaryAxisConfig();
			m_CoordIconRenderer = new CoordIconRenderer( viewer, dataManager );
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.ToolVec;
			}
		}

		public Action<Dictionary<MachineComponentType, List<gp_Trsf>>, bool> RaiseTrans;
		public Action<bool> RaiseActionStart;

		public override void Start()
		{
			base.Start();

			// we have a original path index
			SetupPath( m_CurrentPathID );

			// init dialog
			m_ToolVecDlg = new ToolVectorDlg( m_InterpolateType, ref m_ToolVecParam, m_CraftData.IsPathReverse, m_RotaryAxisConfig );
			m_ToolVecDlg.SetKeep += () => OnSetKeep();
			m_ToolVecDlg.SetZdir += () => OnSetZDir();
			m_ToolVecDlg.SetRevert += () => OnSetRevert();
			m_ToolVecDlg.ABAngleChanged = ( angleA, angleB ) => OnABAngleChanged( angleA, angleB );
			m_ToolVecDlg.MSAngleChanged = ( master, slave ) => OnMSAngleChanged( master, slave );
			m_ToolVecDlg.TypeChanged += ( type ) => OnTypeChanged( type );
			m_ToolVecDlg.AddEditIndex += () => OnAddEditIndex();
			m_ToolVecDlg.RemoveEditIndex += () => OnRemoveEditIndex();
			m_ToolVecDlg.SwitchStartEnd += () => OnSwitchStartEnd();
			m_ToolVecDlg.MoveIndex += ( isNext ) => OnMoveIndex( isNext );
			m_ToolVecDlg.MoveCtrlPnt += ( isNext ) => OnMoveToCtrlPnt( isNext );
			m_ToolVecDlg.ToStartOrEnd += ( toStart ) => OnToStartOrEnd( toStart );
			m_ToolVecDlg.FlipRotaryAxis += ( isPositive ) => OnFlipRotaryAxis( isPositive );
			m_ToolVecDlg.Clear += () => OnClearCtrlPnt();
			m_ToolVecDlg.EnableStartEndSwitch( false, false );
			m_ToolVecDlg.Cancel += End;
			m_ToolVecDlg.Show( MyApp.MainForm );

			// draw new trihedron for G54 must before change to start point
			// because change to start point will trigger coord trasform
			bool isGetMachineData = DataGettingHelper.GetMachineData( out MachineData machineData );
			if( isGetMachineData ) {
				gp_Pnt position = new gp_Pnt( machineData.SimulationOffset.x, machineData.SimulationOffset.y, machineData.SimulationOffset.z );
				m_CoordIconRenderer.Show( position );
			}
			else {
				m_CoordIconRenderer.Show();
			}

			// start path index action
			m_PathIndexAction.Start();
			m_PathIndexAction.SelectionChange += OnPathIndexChanged;

			// start point index action, this will enter the start point
			ActivatePointSelection();

			// show machine
			RaiseActionStart?.Invoke( true );
		}

		public override void End()
		{
			// end point index action
			DeactivatePointSelection( m_CurrentPathID );

			// end path index action// cleanup path index action
			m_PathIndexAction.SelectionChange -= OnPathIndexChanged;
			m_PathIndexAction.End();

			// clear render
			m_CoordIconRenderer.Remove(); // global
			m_ToolVecEditRender.Remove(); // path

			RaiseActionStart?.Invoke( false );
			base.End();
		}

		void SetupPath( string pathID )
		{
			if( string.IsNullOrEmpty( pathID ) ) {
				End();
				return;
			}

			// init data for current path
			m_CurrentPathID = pathID;
			if( !DataGettingHelper.GetCraftDataByID( pathID, out m_CraftData ) ) {
				MyApp.Logger.ShowOnLogPanel( $"無法獲取路徑 {pathID} 的加工資訊", MyApp.NoticeType.Warning );
				End();
				return;
			}
			m_DataHandler = new ToolVecActionDataHandler( pathID );

			// init render for current path
			List<string> pathIDList = new List<string>() { pathID };
			m_ToolVecEditRender?.Remove();
			m_ToolVecEditRender = new ToolVecEditRender( m_Viewer, m_DataManager, pathIDList );
			m_ToolVecEditRender.Show( pathIDList );
		}

		void ActivatePointSelection()
		{
			m_PathIndexAction.ExcludeFromSelection( m_CurrentPathID );
			m_ViewManager.ErasePath( m_CurrentPathID );
			CreatePointIndexAction( m_CurrentPathID );

			m_nPointIndex = NULL_POINT_INDEX;
			m_SelectedPoint = null;
			m_ToolVecParam = null;

			int nStartPntIndex = m_DataHandler.GetStartPointCADIndex();
			OnSelectedPointIndexChanged( nStartPntIndex );
		}

		void DeactivatePointSelection( string szOldPathID )
		{
			UnlockSelectedVertexHighLight();

			m_nPointIndex = NULL_POINT_INDEX;
			m_SelectedPoint = null;
			m_ToolVecParam = null;

			DestroyPointIndexAction();
			m_PathIndexAction.RestoreFromExclusion( szOldPathID );
			m_ViewManager.DisplayPath( szOldPathID );
		}

		void CreatePointIndexAction( string pathID )
		{
			m_PointIndexAction = new IndexSelectAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, pathID );
			m_PointIndexAction.IndexChanged += OnPointIndexChanged;
			m_PointIndexAction.Start();
		}

		void DestroyPointIndexAction()
		{
			if( m_PointIndexAction != null ) {
				m_PointIndexAction.IndexChanged -= OnPointIndexChanged;
				m_PointIndexAction.End();
				m_PointIndexAction = null;
			}
		}

		// path switching
		void OnPathIndexChanged()
		{
			List<string> selectedIDs = m_PathIndexAction.GetSelectedIDs();
			if( selectedIDs.Count != 1 ) {
				return;
			}
			string newPathID = selectedIDs.First();
			if( newPathID == m_CurrentPathID ) {
				return;
			}
			string szOldPathID = m_CurrentPathID;
			SetupPath( newPathID );

			// cleanup old state
			DeactivatePointSelection( szOldPathID );

			// activate point selection for new path
			ActivatePointSelection();
		}

		void OnPointIndexChanged( int nSelectIndex, TopoDS_Shape selectedVertex )
		{
			if( nSelectIndex == m_nPointIndex ) {
				return;
			}
			UnlockSelectedVertexHighLight();
			LockSelectedVertexHighLight( selectedVertex );
			OnSelectedPointIndexChanged( nSelectIndex );
		}

		// UI Setting
		void OnSelectedPointIndexChanged( int nSelectIndex )
		{
			m_IsUpdatingUI = true;
			if( m_nPointIndex == nSelectIndex ) {
				m_IsUpdatingUI = false;
				return;
			}
			m_nPointIndex = nSelectIndex;

			// no select
			if( nSelectIndex == NULL_POINT_INDEX ) {
				ResetToolVecState();
				UIProtection();
				m_IsUpdatingUI = false;
				return;
			}

			// with select
			m_SelectedPoint = m_DataHandler.GetPointByCADIndex( m_nPointIndex );

			// check if point is valid
			if( m_SelectedPoint == null ) {
				ResetToolVecState();
				UIProtection();
				m_IsUpdatingUI = false;
				return;
			}

			// determine if this is start or end point
			UpdateStartEndPointFlags();

			// create tool vector parameter based on point type
			m_ToolVecParam = CreateToolVecParamForSelectedPoint();

			// update dialog
			m_ToolVecDlg.ResetToolVecParam( m_ToolVecParam );
			UIProtection();
			RefreshSimuResult();
			m_IsUpdatingUI = false;
		}

		void ResetToolVecState()
		{
			m_SelectedPoint = null;
			m_ToolVecParam = null;
			m_IsStartPnt = false;
			m_IsEndPnt = false;
			m_ToolVecDlg.ResetToolVecParam( m_ToolVecParam );
		}

		void UpdateStartEndPointFlags()
		{
			m_IsStartPnt = m_nPointIndex == m_DataHandler.GetStartPointCADIndex();
			if( m_DataHandler.IsClosed() ) {
				m_IsEndPnt = m_nPointIndex == CLOSED_POINT_INDEX;
			}
			else {
				m_IsEndPnt = m_nPointIndex == m_DataHandler.GetEndPointCADIndex();
			}
		}

		ToolVecParam CreateToolVecParamForSelectedPoint()
		{
			// try to get modified start/end point data
			if( m_IsStartPnt ) {
				return TryCreateToolVecParamFromModifiedPoint( true );
			}
			else if( m_IsEndPnt ) {
				return TryCreateToolVecParamFromModifiedPoint( false );
			}
			else {
				// normal control point
				return CreateToolVecParamFromDataHandler();
			}
		}

		ToolVecParam TryCreateToolVecParamFromModifiedPoint( bool isStartIndex )
		{
			if( m_CraftData.IsStartPntModified( isStartIndex, out ToolVecAngleData toolVecAngleData, out _ ) ) {
				return new ToolVecParam(
					toolVecAngleData.RA_deg,
					toolVecAngleData.RB_deg,
					toolVecAngleData.Master_deg,
					toolVecAngleData.Slave_deg,
					true // mark this point is modify pnt
				);
			}
			else {
				return CreateToolVecParamFromDataHandler();
			}
		}

		ToolVecParam CreateToolVecParamFromDataHandler()
		{
			bool isModify = m_DataHandler.GetToolVecModify( m_nPointIndex, out double angleA_deg, out double angleB_deg, out double master_deg, out double slave_deg );
			return new ToolVecParam( angleA_deg, angleB_deg, master_deg, slave_deg, isModify );
		}

		void LockSelectedVertexHighLight( TopoDS_Shape selectedVertex )
		{
			if( selectedVertex == null || selectedVertex.IsNull() ) {
				return;
			}

			// show selected vertex on viewer, because pause select mode woud clear select
			DrawVertexOnViewer( selectedVertex );
		}

		void UnlockSelectedVertexHighLight()
		{
			// remove selected vertex which show on viewer
			if( m_KeepedHighLightPoint != null ) {

				// true means update viewer
				m_Viewer.GetAISContext().Remove( m_KeepedHighLightPoint, false );
				m_KeepedHighLightPoint = null;
			}
		}

		void DrawVertexOnViewer( TopoDS_Shape selectedVertex )
		{
			if( selectedVertex != null ) {
				m_KeepedHighLightPoint = new AIS_Shape( selectedVertex );

				// set pointAspect
				Prs3d_PointAspect pointAspect = new Prs3d_PointAspect(
					Aspect_TypeOfMarker.Aspect_TOM_BALL,
					new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ),
					3.0f
				);

				// set high light attribute
				m_KeepedHighLightPoint.Attributes().SetPointAspect( pointAspect );

				// refresh viewer
				m_Viewer.GetAISContext().Display( m_KeepedHighLightPoint, false );
			}
		}

		// dialog event
		void OnSetRevert()
		{
			if( m_SelectedPoint == null ) {
				return;
			}

			// point exists, get original MS angles
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.Master_deg = m_SelectedPoint.InitMaster_rad * 180.0 / Math.PI;
				m_ToolVecParam.Slave_deg = m_SelectedPoint.InitSlave_rad * 180.0 / Math.PI;
				m_ToolVecParam.AngleA_deg = 0;
				m_ToolVecParam.AngleB_deg = 0;
			}
			SetToolVecParamAndPreview();
		}

		void OnSetKeep()
		{
			if( m_nPointIndex == NULL_POINT_INDEX ) {
				return;
			}
			ISetToolVecPoint preCtrlPoint = m_DataHandler.GetPreCtrlPoint( m_nPointIndex );
			if( preCtrlPoint == null ) {
				return;
			}

			// Calculate AB angles from previous control point MS angles
			if( m_SelectedPoint == null ) {
				return;
			}
			Tuple<double, double> abAngles_deg =
				ToolVecHelper.GetABAngleFromMSAngle( preCtrlPoint.ModMaster_rad * 180.0 / Math.PI,
														preCtrlPoint.ModSlave_rad * 180.0 / Math.PI,
														m_SelectedPoint );

			// check angle range
			if( !CheckABAngleRange( abAngles_deg.Item1, abAngles_deg.Item2 ) ) {
				return;
			}

			// valid result, trigger update
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.Master_deg = preCtrlPoint.ModMaster_rad * 180.0 / Math.PI;
				m_ToolVecParam.Slave_deg = preCtrlPoint.ModSlave_rad * 180.0 / Math.PI;
				m_ToolVecParam.AngleA_deg = abAngles_deg.Item1;
				m_ToolVecParam.AngleB_deg = abAngles_deg.Item2;
			}
			SetToolVecParamAndPreview();
		}

		void OnSetZDir()
		{
			// calculate MS angles from Z direction
			if( m_SelectedPoint == null ) {
				return;
			}
			Tuple<double, double> msAngles_deg = ToolVecHelper.GetMSAngleFromToolVec( new gp_Dir( 0, 0, 1 ), m_SelectedPoint );

			// calculate AB angles from MS angles
			Tuple<double, double> abAngles_deg = ToolVecHelper.GetABAngleFromMSAngle( msAngles_deg.Item1, msAngles_deg.Item2, m_SelectedPoint );

			// check angle range
			if( !CheckABAngleRange( abAngles_deg.Item1, abAngles_deg.Item2 ) ) {
				return;
			}

			// valid result, trigger update
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.Master_deg = msAngles_deg.Item1;
				m_ToolVecParam.Slave_deg = msAngles_deg.Item2;
				m_ToolVecParam.AngleA_deg = abAngles_deg.Item1;
				m_ToolVecParam.AngleB_deg = abAngles_deg.Item2;
			}
			SetToolVecParamAndPreview();
		}

		void OnABAngleChanged( double angleA_deg, double angleB_deg )
		{
			// check angle range
			if( !CheckABAngleRange( angleA_deg, angleB_deg ) ) {
				return;
			}
			if( !IsValidToolVecState() ) {
				return;
			}
			Tuple<double, double> msAngle_deg = ToolVecHelper.GetMSAngleFromABAngle( angleA_deg, angleB_deg, m_SelectedPoint );

			// valid result, trigger update
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.Master_deg = msAngle_deg.Item1;
				m_ToolVecParam.Slave_deg = msAngle_deg.Item2;
				m_ToolVecParam.AngleA_deg = angleA_deg;
				m_ToolVecParam.AngleB_deg = angleB_deg;
			}
			SetToolVecParamAndPreview();
		}

		void OnMSAngleChanged( double master_deg, double slave_deg )
		{
			if( !IsValidToolVecState() ) {
				return;
			}
			Tuple<double, double> abAngle_deg = ToolVecHelper.GetABAngleFromMSAngle( master_deg, slave_deg, m_SelectedPoint );

			// check angle range
			if( !CheckABAngleRange( abAngle_deg.Item1, abAngle_deg.Item2 ) ) {
				return;
			}

			// valid result, trigger update
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.AngleA_deg = abAngle_deg.Item1;
				m_ToolVecParam.AngleB_deg = abAngle_deg.Item2;
				m_ToolVecParam.Master_deg = master_deg;
				m_ToolVecParam.Slave_deg = slave_deg;
			}
			SetToolVecParamAndPreview();
		}

		void OnFlipRotaryAxis( bool isPositive )
		{
			if( !IsValidToolVecState() ) {
				return;
			}
			Tuple<double, double> rotated = ToolVecHelper.FlipRotaryAxis( m_ToolVecParam.Master_deg, m_ToolVecParam.Slave_deg, isPositive );
			OnMSAngleChanged( rotated.Item1, rotated.Item2 );
		}

		void SetInterpolationMode( int nCurrentIdx, EToolVecInterpolateType interpolateType )
		{
			bool isGetNextModfiyIndex = m_DataHandler.FindNextCtrlPntIdx( nCurrentIdx, out int nNextIdx );
			if( isGetNextModfiyIndex ) {
				m_CraftData.SetInterpolationTypeAtPnt( nNextIdx, interpolateType );
			}
			else {
				if( m_CraftData.IsPathReverse ) {

					// first region is record on first pnt (in reverse case this region type is recorded at preidx)
					m_CraftData.SetInterpolationTypeAtStartPnt( true, interpolateType );
				}
				else {

					// last region is record on end pnt
					m_CraftData.SetInterpolationTypeAtStartPnt( false, interpolateType );
				}
			}
		}

		void OnTypeChanged( EToolVecInterpolateType type )
		{
			// this even is trigger by UI refresh, do not have to set any value
			if( m_IsUpdatingUI ) {
				return;
			}
			m_InterpolateType = type;
			SetInterpolationMode( m_nPointIndex, m_InterpolateType );

			// trigger viewer refresh
			RefreshSimuResult();

			// update dialog, cause change type might change is point param
			bool isModify = m_DataHandler.GetToolVecModify( m_nPointIndex, out double angleA_deg, out double angleB_deg, out double master_deg, out double slave_deg );
			m_ToolVecParam = new ToolVecParam( angleA_deg, angleB_deg, master_deg, slave_deg, isModify );
			m_ToolVecDlg.ResetToolVecParam( m_ToolVecParam );
			UIProtection();
		}

		void OnAddEditIndex()
		{
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.IsModified = true;
				SetToolVecParamAndPreview( true );
			}
		}

		void OnRemoveEditIndex()
		{
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.IsModified = false;
				SetToolVecParamAndPreview( true );
			}
		}

		void OnSwitchStartEnd()
		{
			if( m_DataHandler.IsClosed() ) {
				// at start index, switch to end index
				if( m_nPointIndex == m_DataHandler.GetStartPointCADIndex() ) {
					OnSelectedPointIndexChanged( CLOSED_POINT_INDEX );
				}

				// at end index, switch to start index
				else if( m_nPointIndex == CLOSED_POINT_INDEX ) {
					OnSelectedPointIndexChanged( m_DataHandler.GetStartPointCADIndex() );
				}
				else {
					// do nothing
				}
			}
			else {
				// at start index, switch to end index
				if( m_nPointIndex == m_DataHandler.GetStartPointCADIndex() ) {
					OnSelectedPointIndexChanged( m_DataHandler.GetEndPointCADIndex() );
				}

				// at end index, switch to start index
				else if( m_nPointIndex == m_DataHandler.GetEndPointCADIndex() ) {
					OnSelectedPointIndexChanged( m_DataHandler.GetStartPointCADIndex() );
				}
				else {
					// do nothing
				}
			}

		}

		void OnMoveIndex( bool isNext )
		{
			int newIndex = m_DataHandler.GetPrevOrNextCADIndex( isNext, m_nPointIndex );
			OnSelectedPointIndexChanged( newIndex );
		}

		void OnMoveToCtrlPnt( bool isNext )
		{
			if( isNext ) {
				FindNextCtrlPnt();
			}
			else {
				FindPreCtrlPnt();
			}
		}

		void OnToStartOrEnd( bool toStart )
		{
			if( m_DataHandler.IsClosed() ) {
				OnSelectedPointIndexChanged( toStart ? m_DataHandler.GetStartPointCADIndex() : CLOSED_POINT_INDEX );
			}

			// for open path, just use 0 and last index of CAD
			else {
				OnSelectedPointIndexChanged( toStart ? m_DataHandler.GetStartPointCADIndex() : m_DataHandler.GetEndPointCADIndex() );
			}
		}

		void OnClearCtrlPnt()
		{
			m_CraftData.ClearToolVecModify();

			// create tool vector parameter based on point type
			m_ToolVecParam = CreateToolVecParamForSelectedPoint();

			// update dialog
			m_ToolVecDlg.ResetToolVecParam( m_ToolVecParam );
			UIProtection();
			RefreshSimuResult();
		}

		bool CheckABAngleRange( double angleA_deg, double angleB_deg )
		{
			if( angleA_deg < MIN_TiltAngle || angleA_deg > MAX_TiltAngle ||
				angleB_deg < MIN_TiltAngle || angleB_deg > MAX_TiltAngle ) {
				MyApp.Logger.ShowOnLogPanel( "傾角值過大，角度必須在 -90~+90 範圍內", MyApp.NoticeType.Warning );
				return false;
			}
			return true;
		}

		bool IsValidToolVecState()
		{
			return m_ToolVecParam != null && m_SelectedPoint != null;
		}

		bool ValidateFrameTransformMap( Dictionary<MachineComponentType, List<gp_Trsf>> frameTransformMap )
		{
			if( frameTransformMap == null
				|| !frameTransformMap.ContainsKey( MachineComponentType.WorkPiece )
				|| frameTransformMap[ MachineComponentType.WorkPiece ] == null
				|| frameTransformMap[ MachineComponentType.WorkPiece ].Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "無法順利模擬該點姿態", MyApp.NoticeType.Warning );
				return false;
			}
			return true;
		}

		ToolVecModifyData CreateToolVecModifyData( EToolVecInterpolateType interpolateType )
		{
			return new ToolVecModifyData()
			{
				AngleData = new ToolVecAngleData()
				{
					RA_deg = m_ToolVecParam.AngleA_deg,
					RB_deg = m_ToolVecParam.AngleB_deg,
					Master_deg = m_ToolVecParam.Master_deg,
					Slave_deg = m_ToolVecParam.Slave_deg
				},
				InterpolateType = interpolateType
			};
		}

		void UIProtection()
		{
			m_ToolVecDlg.EnableStartEndSwitch( m_IsStartPnt || m_IsEndPnt, m_IsStartPnt );

			if( m_CraftData.StartPntToolVecData == null ) {
				m_CraftData.StartPntToolVecData = new StartPntToolVecParam();
			}

			// it is start or end point and is modify pnt
			if( m_IsStartPnt && m_CraftData.StartPntToolVecData.StartPnt.AngleData != null ||
				m_IsEndPnt && m_CraftData.StartPntToolVecData.EndPnt.AngleData != null ) {
				m_ToolVecDlg.UIControlProtection( true, true );
				return;
			}

			// it is start or end point butnot modify pnt
			if( m_IsStartPnt && m_CraftData.StartPntToolVecData.StartPnt.AngleData == null ||
				m_IsEndPnt && m_CraftData.StartPntToolVecData.EndPnt.AngleData == null ) {
				m_ToolVecDlg.UIControlProtection( false, true );
				return;
			}

			// cbx need to be lock
			if( m_SelectedPoint != null && m_SelectedPoint.IsToolVecModPoint ) {
				m_ToolVecDlg.UIControlProtection( true );
				return;
			}

			// get this region interpolate type to show on combobox
			EToolVecInterpolateType interpolateType = GetNextModifyIndexInterpolate();
			m_ToolVecDlg.UIControlProtection( false, false, interpolateType );
		}

		#region Set tool vector param

		void SetToolVecParamAndPreview( bool bForceUpdate = false )
		{
			if( m_ToolVecParam == null ) {
				return;
			}
			if( m_CraftData.StartPntToolVecData == null ) {
				m_CraftData.StartPntToolVecData = new StartPntToolVecParam();
			}
			SetCtrlPntAngleData();

			if( m_ToolVecParam.IsModified ) {
				RefreshSimuResult();
			}
			else {
				// trigger viewer refresh
				RefreshSimuResult( false );
			}

			// update cache point
			if( m_SelectedPoint != null && m_nPointIndex != NULL_POINT_INDEX ) {
				m_SelectedPoint = m_DataHandler.GetPointByCADIndex( m_nPointIndex );
			}
			UIProtection();
		}

		void SetCtrlPntAngleData()
		{
			if( m_IsStartPnt ) {
				ModifyStartPnt();
				return;
			}
			if( m_IsEndPnt ) {
				ModifyEndPnt();
				return;
			}

			// normal point
			SetNormalCtrlPnt();
		}

		void ModifyStartPnt()
		{
			// remove start point ctrl
			if( m_ToolVecParam.IsModified == false ) {
				EToolVecInterpolateType type = m_CraftData.StartPntToolVecData.StartPnt.InterpolateType;
				m_CraftData.StartPntToolVecData.StartPnt = new ToolVecModifyData( type );
				return;
			}

			// add or change start point ctrl
			ToolVecModifyData startPntData = CreateToolVecModifyData( m_CraftData.StartPntToolVecData.StartPnt.InterpolateType );
			m_CraftData.StartPntToolVecData.StartPnt = startPntData;
		}

		void ModifyEndPnt()
		{
			// remove end point ctrl
			if( m_ToolVecParam.IsModified == false ) {
				EToolVecInterpolateType type = m_CraftData.StartPntToolVecData.EndPnt.InterpolateType;
				m_CraftData.StartPntToolVecData.EndPnt = new ToolVecModifyData( type );
				return;
			}

			// add or change end point ctrl
			ToolVecModifyData endPntData = CreateToolVecModifyData( m_CraftData.StartPntToolVecData.EndPnt.InterpolateType );
			m_CraftData.StartPntToolVecData.EndPnt = endPntData;
		}

		void SetNormalCtrlPnt()
		{
			// remove point
			if( !m_ToolVecParam.IsModified ) {
				RemoveCtrlPnt();
				return;
			}

			// change pnt data
			if( m_CraftData.ToolVecModifyMap.ContainsKey( m_nPointIndex ) ) {
				ChangeCtrlPntParam();
			}

			// add new point
			else {
				AddCtrlPnt();
			}
		}

		void RemoveCtrlPnt()
		{
			if( m_CraftData.ToolVecModifyMap.ContainsKey( m_nPointIndex ) ) {
				if( m_CraftData.IsPathReverse == false ) {
					bool isFoundNext = m_DataHandler.FindNextCtrlPntIdx( m_nPointIndex, out int nNextIdx );
					if( isFoundNext ) {
						m_CraftData.RemoveCtrlPntAndTransInterpolation( m_nPointIndex, nNextIdx );
					}
					else {
						EToolVecInterpolateType removedType = m_CraftData.ToolVecModifyMap[ m_nPointIndex ].InterpolateType;
						m_CraftData.SetInterpolationTypeAtStartPnt( false, removedType );
						m_CraftData.RemoveCtrlPnt( m_nPointIndex );
					}
				}
				else {
					// this region interpolate type in revese case is record at pre index
					// remove this index means pre region will include this region, so type do not have to change )
					m_CraftData.RemoveCtrlPnt( m_nPointIndex );
				}
			}
		}

		void ChangeCtrlPntParam()
		{
			// just change param, no need to change interpolate type (ctrl pnt can't modify type, UI already protected)
			m_CraftData.ChangeToolVecModify( m_nPointIndex,
				m_ToolVecParam.AngleA_deg, m_ToolVecParam.AngleB_deg, m_ToolVecParam.Master_deg, m_ToolVecParam.Slave_deg );
		}

		void AddCtrlPnt()
		{
			// find next modified point index
			EToolVecInterpolateType interpolateType = GetNextModifyIndexInterpolate();

			// set modify data
			m_CraftData.AddToolVecModify( m_nPointIndex,
				m_ToolVecParam.AngleA_deg, m_ToolVecParam.AngleB_deg, m_ToolVecParam.Master_deg, m_ToolVecParam.Slave_deg, interpolateType );
		}

		#endregion

		#region Get ctrl pnt info

		EToolVecInterpolateType GetNextModifyIndexInterpolate()
		{
			bool isFound = m_DataHandler.FindNextCtrlPntIdx( m_nPointIndex, out int nNextIdx );
			if( !isFound ) {
				if( m_CraftData.StartPntToolVecData == null ) {
					return EToolVecInterpolateType.Normal;
				}
				if( m_CraftData.IsPathReverse ) {
					return m_CraftData.StartPntToolVecData.StartPnt.InterpolateType;
				}
				else {
					return m_CraftData.StartPntToolVecData.EndPnt.InterpolateType;
				}
			}
			else {
				if( m_CraftData.ToolVecModifyMap.TryGetValue( nNextIdx, out ToolVecModifyData data ) && data != null ) {
					return data.InterpolateType;
				}
				return EToolVecInterpolateType.Normal;
			}
		}

		void FindNextCtrlPnt()
		{
			// already at end point
			if( m_DataHandler.IsClosed() && m_nPointIndex == CLOSED_POINT_INDEX ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]沒有下個控制點", MyApp.NoticeType.Hint );
				return;
			}

			bool isReverse = m_CraftData.IsPathReverse;
			bool isFound = isReverse
				? m_DataHandler.FindPreCtrlPntIdx( m_nPointIndex, out int targetIdx )
				: m_DataHandler.FindNextCtrlPntIdx( m_nPointIndex, out targetIdx );

			// if no control point found, check if end point is a control point
			if( !isFound && m_CraftData.StartPntToolVecData.EndPnt.AngleData != null ) {
				isFound = true;
				targetIdx = CLOSED_POINT_INDEX;
			}
			if( isFound ) {
				OnSelectedPointIndexChanged( targetIdx );
			}
			else {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]沒有下個控制點", MyApp.NoticeType.Hint );
			}
		}

		void FindPreCtrlPnt()
		{
			// already at start point
			if( m_DataHandler.IsClosed() && m_nPointIndex == m_DataHandler.GetStartPointCADIndex() ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]沒有前個控制點", MyApp.NoticeType.Hint );
				return;
			}

			int currentIdx = ( m_nPointIndex == CLOSED_POINT_INDEX ) ? m_DataHandler.GetStartPointCADIndex() : m_nPointIndex;
			bool isReverse = m_CraftData.IsPathReverse;
			bool isFound = isReverse
				? m_DataHandler.FindNextCtrlPntIdx( currentIdx, out int targetIdx )
				: m_DataHandler.FindPreCtrlPntIdx( currentIdx, out targetIdx );

			// if no control point found, check if start point is a control point
			if( !isFound && m_CraftData.StartPntToolVecData.StartPnt.AngleData != null ) {
				isFound = true;
				targetIdx = m_DataHandler.GetStartPointCADIndex();
			}
			if( isFound ) {
				OnSelectedPointIndexChanged( targetIdx );
			}
			else {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]沒有前個控制點", MyApp.NoticeType.Hint );
			}
		}

		#endregion

		#region Update UI

		void RefreshSimuResult( bool isEditModifyPnt = true )
		{
			bool isGetMachineData = DataGettingHelper.GetMachineData( out MachineData machineData );
			if( !isGetMachineData ) {
				MyApp.Logger.ShowOnLogPanel( "無法獲得機構資訊", MyApp.NoticeType.Warning );
				return;
			}


			// if false means just want to refresh viewer by current point transform
			if( isEditModifyPnt ) {

				// trigger rebuild cam point list
				m_SelectedPoint = m_DataHandler.GetPointByCADIndex( m_nPointIndex );
			}
			bool isCalSuccess = CalSimuTranfResult( machineData, out Dictionary<MachineComponentType, List<gp_Trsf>> frameTransformMap, isEditModifyPnt );
			if( !isCalSuccess ) {
				MyApp.Logger.ShowOnLogPanel( "無法順利模擬該點姿態", MyApp.NoticeType.Warning );
				return;
			}
			if( !ValidateFrameTransformMap( frameTransformMap ) ) {
				return;
			}
			// output vertex is the shape of high light
			m_PointIndexAction.TranfAndRebuildMap( frameTransformMap[ MachineComponentType.WorkPiece ].Last(), m_nPointIndex, out TopoDS_Shape vertexhighlight );

			// remove old hight light
			UnlockSelectedVertexHighLight();

			// re high light
			LockSelectedVertexHighLight( vertexhighlight );

			// trihedron also need to change according to workpiece
			m_CoordIconRenderer.Trans( frameTransformMap[ MachineComponentType.WorkPiece ].Last() );

			// false means pause the viewer, because m_ToolVecEditRender will cause viewer flash
			RaiseTrans?.Invoke( frameTransformMap, false );
			m_ToolVecEditRender.ShowTrans( frameTransformMap[ MachineComponentType.WorkPiece ].Last() );
			EnableViewImmediateUpdate();
			m_Viewer.UpdateView();
		}

		void EnableViewImmediateUpdate()
		{
			V3d_View view = m_Viewer.GetView();
			view.SetImmediateUpdate( true );
		}

		bool CalSimuTranfResult( MachineData machineData, out Dictionary<MachineComponentType, List<gp_Trsf>> frameTransformMap, bool isEditModifyPnt )
		{
			frameTransformMap = new Dictionary<MachineComponentType, List<gp_Trsf>>();
			if( m_SelectedPoint == null ) {
				return false;
			}
			if( machineData == null ) {
				return false;
			}

			// set default offset
			gp_Vec simuOffset = machineData.SimulationOffset ?? new gp_Vec();
			PostPoint G54pnt;

			// get the real IK solved result to preview
			if( isEditModifyPnt ) {
				G54pnt = new PostPoint()
				{
					X = m_SelectedPoint.Point.x,
					Y = m_SelectedPoint.Point.y,
					Z = m_SelectedPoint.Point.z,
					Master = m_SelectedPoint.ModMaster_rad,
					Slave = m_SelectedPoint.ModSlave_rad,
				};
			}

			// user want to know this unmodify point vector
			else {
				G54pnt = new PostPoint()
				{
					X = m_SelectedPoint.Point.x,
					Y = m_SelectedPoint.Point.y,
					Z = m_SelectedPoint.Point.z,
					Master = m_ToolVecParam.Master_deg * Math.PI / 180.0,
					Slave = m_ToolVecParam.Slave_deg * Math.PI / 180.0
				};
			}


			// create PostSolver
			PostSolver postSolver = new PostSolver( m_DataManager.MachineData );
			bool calSuccess = SimulationHelper.BuildFKPostPnt( postSolver, G54pnt, out PostPoint FKpnt, simuOffset );
			if( !calSuccess ) {
				MyApp.Logger.ShowOnLogPanel( "無法計算出雷射頭位置", MyApp.NoticeType.Warning );
				return false;
			}
			// init frame transform map
			frameTransformMap[ MachineComponentType.XAxis ] = new List<gp_Trsf>();
			frameTransformMap[ MachineComponentType.YAxis ] = new List<gp_Trsf>();
			frameTransformMap[ MachineComponentType.ZAxis ] = new List<gp_Trsf>();
			frameTransformMap[ MachineComponentType.Master ] = new List<gp_Trsf>();
			frameTransformMap[ MachineComponentType.Slave ] = new List<gp_Trsf>();
			frameTransformMap[ MachineComponentType.Laser ] = new List<gp_Trsf>();
			frameTransformMap[ MachineComponentType.WorkPiece ] = new List<gp_Trsf>();
			SimulationHelper.FKToFrameTranfResult( FKpnt, machineData.SimulationOffset, m_DataManager.WorkPieceChain, machineData, m_DataManager.MachineChainListMap, ref frameTransformMap );
			return true;
		}

		#endregion

		// rotary axis config
		RotaryAxisConfig CreateRotaryAxisConfig()
		{
			if( !DataGettingHelper.GetMachineData( out MachineData machineData ) ) {
				throw new InvalidOperationException( "Cannot get machine data" );
			}
			RotaryAxisConfig config = new RotaryAxisConfig();
			config.MasterName = ConvertRotaryAxisName( machineData.MasterRotaryAxis );
			config.SlaveName = ConvertRotaryAxisName( machineData.SlaveRotaryAxis );
			if( machineData.FiveAxisType == FiveAxisType.Table || machineData.FiveAxisType == FiveAxisType.Mix ) {
				config.RotaryAxisName = config.SlaveName;
			}
			else {
				config.RotaryAxisName = config.MasterName;
			}
			return config;
		}

		string ConvertRotaryAxisName( RotaryAxis axis )
		{
			switch( axis ) {
				case RotaryAxis.X:
					return "A";
				case RotaryAxis.Y:
					return "B";
				case RotaryAxis.Z:
					return "C";
				default:
					return "";
			}
		}

		// path index param
		string m_CurrentPathID;
		ToolVecActionDataHandler m_DataHandler = null;
		CraftData m_CraftData = null;

		// point index param
		int m_nPointIndex = NULL_POINT_INDEX;
		ToolVecParam m_ToolVecParam = null;
		ISetToolVecPoint m_SelectedPoint = null;

		// global param
		EToolVecInterpolateType m_InterpolateType = EToolVecInterpolateType.Normal;
		RotaryAxisConfig m_RotaryAxisConfig = null;

		// action data
		AIS_Shape m_KeepedHighLightPoint = null;
		ToolVectorDlg m_ToolVecDlg = null;

		// angle limit
		public const double MAX_TiltAngle = 90.0;
		public const double MIN_TiltAngle = -90.0;

		// null point index as -999, -1 is used for closed point index
		const int NULL_POINT_INDEX = -999;
		const int CLOSED_POINT_INDEX = -1;

		// renderer
		CoordIconRenderer m_CoordIconRenderer; // global
		ToolVecEditRender m_ToolVecEditRender; // path

		// start or end point flag
		bool m_IsStartPnt;
		bool m_IsEndPnt;

		// composition references
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;
		SelectPathAction m_PathIndexAction;
		IndexSelectAction m_PointIndexAction;

		// flag to stop event raise to trigger onproperty change
		bool m_IsUpdatingUI = false;
	}

	class ToolVecActionDataHandler
	{
		public ToolVecActionDataHandler( string szPathID )
		{
			// check ID is a contour path
			if( !DataGettingHelper.GetPathType( szPathID, out PathType pathType )
				|| pathType != PathType.Contour ) {
				throw new ArgumentException( "PathID is not a contour path: " + szPathID );
			}

			// get craft data by pathID
			if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData ) ) {
				throw new ArgumentException( "Cannot get CraftData by pathID: " + szPathID );
			}
			m_CraftData = craftData;

			// get cache by pathID
			if( !DataGettingHelper.GetContourCacheByID( szPathID, out ContourCache pathCache ) ) {
				throw new ArgumentException( "Cannot get ContourCache by pathID: " + szPathID );
			}
			m_PathCache = pathCache;

			// get geom data by pathID
			if( !DataGettingHelper.GetGeomDataByID( szPathID, out IGeomData geomData ) ) {
				throw new ArgumentException( "Cannot get GeomData by pathID: " + szPathID );
			}
			m_GeomData = geomData as ContourGeomData;
		}

		public bool GetToolVecModify( int cadIndex, out double dRA_deg, out double dRB_deg, out double master_deg, out double slave_deg )
		{
			if( m_CraftData.ToolVecModifyMap.TryGetValue( cadIndex, out ToolVecModifyData data ) &&
				data != null &&
				data.AngleData != null ) {
				dRA_deg = data.AngleData.RA_deg;
				dRB_deg = data.AngleData.RB_deg;
				master_deg = data.AngleData.Master_deg;
				slave_deg = data.AngleData.Slave_deg;
				return true;
			}
			else {
				dRA_deg = 0;
				dRB_deg = 0;

				// get CAM map index
				int camIndex = 0;
				if( cadIndex == CLOSED_POINT_INDEX ) {
					camIndex = m_PathCache.MainPathPointList.Count - 1;
				}
				else if( m_PathCache.CADToCAMIndexMap.ContainsKey( cadIndex ) ) {
					camIndex = m_PathCache.CADToCAMIndexMap[ cadIndex ];
				}
				else {
					master_deg = 0;
					slave_deg = 0;
					return false;
				}

				// get master and slave from cache
				if( camIndex >= 0 && camIndex < m_PathCache.MainPathPointList.Count ) {
					master_deg = m_PathCache.MainPathPointList[ camIndex ].ModMaster_rad * 180.0 / Math.PI;
					slave_deg = m_PathCache.MainPathPointList[ camIndex ].ModSlave_rad * 180.0 / Math.PI;

					// get AB angles from master and slave
					Tuple<double, double> abAngles = ToolVecHelper.GetABAngleFromMSAngle( master_deg, slave_deg, m_PathCache.MainPathPointList[ camIndex ] );
					dRA_deg = abAngles.Item1;
					dRB_deg = abAngles.Item2;
				}
				else {
					master_deg = 0;
					slave_deg = 0;
				}
				return false;
			}
		}

		public ISetToolVecPoint GetPointByCADIndex( int cadIndex )
		{
			if( m_PathCache.CADToCAMIndexMap.ContainsKey( cadIndex ) ) {
				int camIndex = m_PathCache.CADToCAMIndexMap[ cadIndex ];
				if( camIndex >= 0 && camIndex < m_PathCache.MainPathPointList.Count ) {
					return m_PathCache.MainPathPointList[ camIndex ];
				}
			}
			else if( cadIndex == CLOSED_POINT_INDEX ) {
				return m_PathCache.MainPathPointList.Last();
			}
			return null;
		}

		public bool IsClosed()
		{
			return m_GeomData.IsClosed;
		}

		public int GetStartPointCADIndex()
		{
			if( m_PathCache.MainPathPointList.Count > 0 ) {
				return m_PathCache.MainPathPointList[ 0 ].InitPathIndex;
			}
			return NULL_POINT_INDEX;
		}

		public int GetEndPointCADIndex()
		{
			if( m_PathCache.MainPathPointList.Count > 0 ) {
				return m_PathCache.MainPathPointList.Last().InitPathIndex;
			}
			return NULL_POINT_INDEX;
		}

		public ISetToolVecPoint GetPreCtrlPoint( int cadIndex )
		{
			int camIndex = 0;
			if( m_PathCache.CADToCAMIndexMap.ContainsKey( cadIndex ) ) {
				camIndex = m_PathCache.CADToCAMIndexMap[ cadIndex ];
			}
			else if( cadIndex == CLOSED_POINT_INDEX ) {
				camIndex = m_PathCache.MainPathPointList.Count - 1;
			}
			else {
				return null;
			}

			// find previous control point
			for( int i = camIndex - 1; i >= 0; i-- ) {
				if( m_PathCache.MainPathPointList[ i ].IsToolVecModPoint ) {
					return m_PathCache.MainPathPointList[ i ];
				}
			}
			return null;
		}

		public int GetPrevOrNextCADIndex( bool isNext, int currentCADIndex )
		{
			// convert to cam index
			int camIndex = 0;
			if( m_PathCache.CADToCAMIndexMap.ContainsKey( currentCADIndex ) ) {
				camIndex = m_PathCache.CADToCAMIndexMap[ currentCADIndex ];
			}
			else if( currentCADIndex == CLOSED_POINT_INDEX ) {
				camIndex = m_PathCache.MainPathPointList.Count - 1;
			}
			else {
				return NULL_POINT_INDEX;
			}

			// cam index++ or cam index--, skip generated points (InitPathIndex == OFFSET_GENERATED_INDEX)
			int count = m_PathCache.MainPathPointList.Count;
			int maxSteps = count;
			int step = 0;
			do {
				if( isNext ) {
					camIndex++;
				}
				else {
					camIndex--;
				}
				step++;

				// clamp
				if( camIndex < 0 ) {
					camIndex = 0;
					break;
				}
				else if( camIndex >= count ) {
					camIndex = count - 1;
					break;
				}
			}
			while( step < maxSteps &&
				m_PathCache.MainPathPointList[ camIndex ].InitPathIndex == ContourOffsetHelper.OFFSET_GENERATED_INDEX );

			// convert back to cad index
			if( IsClosed() && camIndex == m_PathCache.MainPathPointList.Count - 1 ) {

				// at the end point of closed path, return closed point index
				return CLOSED_POINT_INDEX;
			}
			else {
				return m_PathCache.MainPathPointList[ camIndex ].InitPathIndex;
			}
		}

		public bool FindNextCtrlPntIdx( int currentIdx, out int nextIdx )
		{
			int StartPntIdx = m_CraftData.StartPointIndex;

			// find the smallest key that is greater than the removed key
			nextIdx = -1;
			bool found = false;

			if( currentIdx >= StartPntIdx ) {

				// find the smallest key that is greater than currentIdx till the end
				foreach( int k in m_CraftData.ToolVecModifyMap.Keys ) {
					if( !m_PathCache.CADToCAMIndexMap.ContainsKey( k ) ) {
						continue;
					}
					if( k > currentIdx ) {
						nextIdx = k;
						found = true;
						break;
					}
				}

				// cant find, then find the smallest key that is smaller than start point index
				if( found == false ) {
					foreach( int k in m_CraftData.ToolVecModifyMap.Keys ) {
						if( !m_PathCache.CADToCAMIndexMap.ContainsKey( k ) ) {
							continue;
						}
						if( k > StartPntIdx ) {
							break;
						}
						if( k < currentIdx ) {
							nextIdx = k;
							found = true;
							break;
						}
					}
				}
			}
			else {

				// find the smallest key that is greater than currentIdx till the start point index
				foreach( int k in m_CraftData.ToolVecModifyMap.Keys ) {
					if( !m_PathCache.CADToCAMIndexMap.ContainsKey( k ) ) {
						continue;
					}
					if( k > StartPntIdx ) {
						break;
					}
					if( k > currentIdx ) {
						nextIdx = k;
						found = true;
						break;
					}
				}
			}
			return found;
		}

		public bool FindPreCtrlPntIdx( int currentIdx, out int preIdx )
		{
			int StartPntIdx = m_CraftData.StartPointIndex;

			// find the greatest key that is smaller than the removed key
			preIdx = -1;
			bool found = false;

			if( currentIdx > StartPntIdx ) {

				// find the greatest key that is smaller than currentIdx till the end
				foreach( int k in m_CraftData.ToolVecModifyMap.Keys ) {
					if( !m_PathCache.CADToCAMIndexMap.ContainsKey( k ) ) {
						continue;
					}
					if( k < StartPntIdx ) {
						continue;
					}
					if( k < currentIdx ) {
						preIdx = k;
						found = true;
					}
				}
			}
			else {

				// find the greatest key that is smaller than currentIdx
				foreach( int k in m_CraftData.ToolVecModifyMap.Keys ) {
					if( !m_PathCache.CADToCAMIndexMap.ContainsKey( k ) ) {
						continue;
					}
					if( k > StartPntIdx ) {
						break;
					}
					if( k < currentIdx ) {
						preIdx = k;
						found = true;
					}
				}

				// cant find, then find the greatest key that is greater than start point index
				if( found == false ) {
					foreach( int k in m_CraftData.ToolVecModifyMap.Keys ) {
						if( !m_PathCache.CADToCAMIndexMap.ContainsKey( k ) ) {
							continue;
						}
						if( k < StartPntIdx ) {
							continue;
						}
						preIdx = k;
						found = true;
					}
				}
			}
			return found;
		}

		readonly CraftData m_CraftData;
		readonly ContourCache m_PathCache;
		readonly ContourGeomData m_GeomData;
		const int NULL_POINT_INDEX = -999;
		const int CLOSED_POINT_INDEX = -1;
	}
}
