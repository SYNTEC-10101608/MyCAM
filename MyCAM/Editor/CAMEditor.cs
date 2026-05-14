using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Renderer;
using MyCAM.Helper;
using MyCAM.PathCache;
using MyCAM.Post;
using OCC.AIS;
using OCC.gp;
using OCC.ShapeAnalysis;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCC.V3d;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class EditableInfo
	{
		public EditableInfo()
		{
			// all set true
			IsStartPointEditable = true;
			IsPathReverseEditable = true;
			IsOverCutEditable = true;
			IsLeadLineEditable = true;
			IsToolVecEditable = true;
			IsToolVecReverseEditable = true;
			IsFixedToolVecEditable = true;
			IsTraverseEditable = true;
			IsAutoOrderEditable = true;
			IsPathCompensateEditable = true;
		}

		public bool IsStartPointEditable;
		public bool IsPathReverseEditable;
		public bool IsOverCutEditable;
		public bool IsLeadLineEditable;
		public bool IsToolVecEditable;
		public bool IsToolVecReverseEditable;
		public bool IsFixedToolVecEditable;
		public bool IsTraverseEditable;
		public bool IsAutoOrderEditable;
		public bool IsPathCompensateEditable;
	}

	internal class CAMEditor : EditorBase
	{
		// to notice main form
		public Action<EditableInfo> PathPropertyChanged;
		public Action<EditActionType, EActionStatus> RaiseCAMActionStatusChange;
		public Action<PathType> PathShapeTypeChanged;

		// action with dialog need to disable main form
		// because other action enterance might close this action, but without closing this form
		public Action<EActionStatus> RaiseWithDlgActionStatusChange;

		public CAMEditor( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
			m_DataManager.PathAdded += OnPathAdded;

			// default action is select object action
			m_DefaultAction = new SelectPathAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			( m_DefaultAction as SelectPathAction ).SelectionChange += OnPathSelectionChange;
			( m_DefaultAction as SelectPathAction ).RemovePath += RemovePath;

			// Initialize renderers
			m_IndexRenderer = new IndexRenderer( m_Viewer, m_DataManager );
			m_OrientationRenderer = new OrientationRenderer( m_Viewer, m_DataManager );
			m_CraftRenderer = new CraftRenderer( m_Viewer, m_DataManager );
			m_ToolVecRenderer = new ToolVecRenderer( m_Viewer, m_DataManager );
			m_TraverseRenderer = new TraverseRenderer( m_Viewer, m_DataManager );
			m_MainPathRenderer = new PathRenderer( m_Viewer, m_ViewManager, m_DataManager );
			m_MachineRender = new MachineRender( m_Viewer, m_DataManager );
		}

		public const string PATH_NODE_PREFIX = "Path_";
		public const int PATH_NODE_PERFIX_LENGTH = 5;

		// Renderers
		IndexRenderer m_IndexRenderer;
		OrientationRenderer m_OrientationRenderer;
		CraftRenderer m_CraftRenderer;
		ToolVecRenderer m_ToolVecRenderer;
		TraverseRenderer m_TraverseRenderer;
		PathRenderer m_MainPathRenderer;
		MachineRender m_MachineRender;

		// editor
		public override EEditorType Type
		{
			get
			{
				return EEditorType.CAM;
			}
		}

		public override void EditStart()
		{
			base.EditStart();

			// init tree
			m_TreeView.Nodes.Add( m_ViewManager.PathNode );
			m_ViewManager.PathNode.ExpandAll();
			ShowAllCAMData();
		}

		public override void EditEnd()
		{
			base.EditEnd();

			// clear tree
			m_TreeView.Nodes.Clear();

			// clear viewer
			foreach( var pathID in m_DataManager.PathIDList ) {
				AIS_InteractiveObject obj = m_ViewManager.ViewObjectMap[ pathID ].AISHandle;
				m_Viewer.GetAISContext().Remove( obj, false );
			}
			RemoveAllCAMData();
		}

		// view API
		public void SetShowToolVec( bool isShowToolVec )
		{
			m_ToolVecRenderer.SetShow( isShowToolVec );
			m_ToolVecRenderer.Show( true );
		}

		public void SetShowOrientation( bool isShowOrientation )
		{
			m_OrientationRenderer.SetShow( isShowOrientation );
			m_OrientationRenderer.Show( true );
		}

		public void SetShowOrder( bool isShowOrder )
		{
			m_IndexRenderer.SetShow( isShowOrder );
			m_IndexRenderer.Show( true );
		}

		public void SetShowTraversePath( bool isShowTraversePath )
		{
			m_TraverseRenderer.SetShow( isShowTraversePath );
			m_TraverseRenderer.Show( true );
		}

		// add/ remove path API

		public void StartSelectFace()
		{
			// with on/off button, so end current action
			if( IsSameAction( EditActionType.SelectFace ) || IsSameAction( EditActionType.SelectPath ) ) {
				m_CurrentAction?.End();
				return;
			}
			SelectFaceAction action = new SelectFaceAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			StartEditAction( action );
		}

		// Auto find top face along Z axis, then process D1 continuous faces and free boundaries
		public void AutoFindAlienatedWorkPieceFaceAndSelectFreeBound()
		{
			// Guard check - ensure workpiece exists
			if( m_DataManager.PartIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先新增工件", MyApp.NoticeType.Hint );
				EndActionIfNotDefault();
				return;
			}

			BoundingBox bbox = GetVisibleWorkpieceBBox();
			if( bbox == null ) {
				return;
			}

			List<gp_Ax1> AlienatedStrategies = GeometryTool.BuildRayAxesFromBBox( new gp_Dir( 0, 0, 1 ), bbox );
			AutoFindOutesetFaceAndAddPath( AlienatedStrategies );
		}

		// Auto find outermost face along X/Y axis, then process D1 continuous faces and free boundaries
		public void AutoFindStretchedWorkPieceFaceAndSelectFreeBound()
		{
			// Guard check - ensure workpiece exists
			if( m_DataManager.PartIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先新增工件", MyApp.NoticeType.Hint );
				EndActionIfNotDefault();
				return;
			}

			BoundingBox bbox = GetVisibleWorkpieceBBox();
			if( bbox == null ) {
				return;
			}

			// Build strategies along +X then +Y, interleaved by priority
			// (center along X, center along Y, then corner midpoints for both)
			List<gp_Ax1> strategiesX = GeometryTool.BuildRayAxesFromBBox( new gp_Dir( 1, 0, 0 ), bbox );
			List<gp_Ax1> strategiesY = GeometryTool.BuildRayAxesFromBBox( new gp_Dir( 0, 1, 0 ), bbox );

			List<gp_Ax1> stretchedStrategies = new List<gp_Ax1>();
			int maxCount = Math.Max( strategiesX.Count, strategiesY.Count );
			for( int i = 0; i < maxCount; i++ ) {
				if( i < strategiesX.Count ) {
					stretchedStrategies.Add( strategiesX[ i ] );
				}
				if( i < strategiesY.Count ) {
					stretchedStrategies.Add( strategiesY[ i ] );
				}
			}
			AutoFindOutesetFaceAndAddPath( stretchedStrategies );
		}

		public void SelectD1ContFace()
		{
			if( m_CurrentAction.ActionType != EditActionType.SelectFace ) {
				return;
			}
			( (SelectFaceAction)m_CurrentAction ).SelectD1ContFace();
		}

		public void SelectPath_FreeBound()
		{
			// get selected face group from select face action
			if( m_CurrentAction.ActionType != EditActionType.SelectFace ) {
				return;
			}
			List<TopoDS_Shape> selectedFaceGroupList = ( (SelectFaceAction)m_CurrentAction ).GetResult();
			m_CurrentAction.End();

			// get path from free boundaries
			if( selectedFaceGroupList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先選擇面", MyApp.NoticeType.Hint );
				return;
			}

			// Extract free boundaries and add paths
			AddPathsFromFaceGroup( selectedFaceGroupList );
		}

		public void StartSelectPath_Manual()
		{
			// with on/off button, so can end current action first
			if( IsSameAction( EditActionType.SelectPath ) || m_CurrentAction.ActionType != EditActionType.SelectFace ) {
				m_CurrentAction.End();
				return;
			}

			// previous action is select face action
			List<TopoDS_Shape> selectedFaceGroupList = ( (SelectFaceAction)m_CurrentAction ).GetResult();

			// end all actions if no face is selected
			if( selectedFaceGroupList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先選擇面", MyApp.NoticeType.Hint );
				m_CurrentAction.End();
				return;
			}
			SelectWireAction action = new SelectWireAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, selectedFaceGroupList );
			StartEditAction( action );
		}

		// Shared: extract free boundaries from face group and add paths to data manager
		void AddPathsFromFaceGroup( List<TopoDS_Shape> faceGroupList )
		{
			List<TopoDS_Wire> pathWireList = new List<TopoDS_Wire>();
			TopTools_IndexedDataMapOfShapeListOfShape edgeFaceMap = new TopTools_IndexedDataMapOfShapeListOfShape();

			foreach( TopoDS_Shape oneFace in faceGroupList ) {
				ShapeAnalysis_FreeBounds freeBounds = new ShapeAnalysis_FreeBounds( oneFace );

				// Add to map
				TopExp.MapShapesAndAncestors(
					oneFace, TopAbs_ShapeEnum.TopAbs_EDGE,
					TopAbs_ShapeEnum.TopAbs_FACE, ref edgeFaceMap );

				// Get all closed wires
				TopExp_Explorer wireExp = new TopExp_Explorer(
					freeBounds.GetClosedWires(), TopAbs_ShapeEnum.TopAbs_WIRE );
				while( wireExp.More() ) {
					pathWireList.Add( TopoDS.ToWire( wireExp.Current() ) );
					wireExp.Next();
				}
			}

			if( pathWireList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]找不到封閉邊界", MyApp.NoticeType.Hint );
				return;
			}
			m_DataManager.AddPath( pathWireList, edgeFaceMap );
		}

		public void EndSelectPath_Manual()
		{
			if( m_CurrentAction.ActionType != EditActionType.SelectPath ) {
				return;
			}
			( (SelectWireAction)m_CurrentAction ).SelectDone();
		}

		public void RemovePath()
		{
			// stop current action
			if( !ValidateBeforeOneShotEdit( out List<string> szPathIDList, true ) ) {
				return;
			}

			foreach( var item in szPathIDList ) {
				string szPathID = item;
				int originalIndex = m_DataManager.PathIDList.IndexOf( szPathID );

				// remove from data manager
				m_DataManager.RemovePath( szPathID );

				// remove from viewer
				if( m_ViewManager.ViewObjectMap.ContainsKey( szPathID ) ) {
					m_Viewer.GetAISContext().Remove( m_ViewManager.ViewObjectMap[ szPathID ].AISHandle, false );
					m_ViewManager.ViewObjectMap.Remove( szPathID );
				}
			}

			// Rebuild tree nodes with correct indices after removal
			RebuildTreeNodes();

			// clear selection after remove path
			m_DefaultAction.ClearSelection();
			RemovePathCAMData( szPathIDList );
		}

		#region Set CAM

		public void SetStartPoint()
		{
			// action edit, no multi edit supported
			if( IsSameAction( EditActionType.StartPoint ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, false ) ) {
				return;
			}

			StartPointAction action = new StartPointAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, szPathIDList.First() );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetReverse()
		{
			// one shot edit, muti edit supported
			if( !ValidateBeforeOneShotEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			foreach( string szPathID in szPathIDList ) {
				if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData ) ) {
					continue;
				}

				// toggle reverse state
				craftData.IsPathReverse = !craftData.IsPathReverse;
			}
			ShowCAMData( szPathIDList );
		}

		public void SetOverCut()
		{
			// action edit, multi edit supported
			if( IsSameAction( EditActionType.OverCut ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			OverCutAction action = new OverCutAction( m_DataManager, szPathIDList );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetLeadLine()
		{
			// action edit, multi edit supported
			if( IsSameAction( EditActionType.SetLead ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			LeadAction action = new LeadAction( m_DataManager, szPathIDList );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetToolVec()
		{
			// action edit, no multi edit supported
			if( IsSameAction( EditActionType.ToolVec ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, false ) ) {
				return;
			}
			ToolVectorAction action = new ToolVectorAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, szPathIDList.First() );

			// register before init, cause init will change select node to start point, machine need to translate
			action.RaiseTrans += SetTrans;
			action.RaiseActionStart += ToolVecActionStartIO;

			// trigger select on star point and translate machine
			StartEditAction( action );
		}

		void ToolVecActionStartIO( bool isStart )
		{
			if( isStart ) {
				m_MachineRender.Show();
				m_MainPathRenderer.SetPauseRefreshAndHide( true );
				m_ToolVecRenderer.SetPauseRefreshAndHide( true );
				m_TraverseRenderer.SetPauseRefreshAndHide( true );
			}
			else {
				m_MainPathRenderer.SetPauseRefreshAndHide( false );
				m_ToolVecRenderer.SetPauseRefreshAndHide( false );
				m_TraverseRenderer.SetPauseRefreshAndHide( false );
				m_MachineRender.Remove();
				m_ToolVecRenderer.Reset();
				m_TraverseRenderer.Reset();
				m_OrientationRenderer.Reset();
				m_IndexRenderer.Reset();
				m_MainPathRenderer.Reset();
				m_CraftRenderer.Reset();
				SetWorkPieceDisplayTransform( new gp_Trsf() );
			}
			m_Viewer.UpdateView();
		}

		void SetWorkPieceDisplayTransform( gp_Trsf trsf = null, bool isUnLockViewerUpdate = true )
		{
			if( trsf == null ) {
				return;
			}

			// backup previous transformation for camera update
			gp_Trsf prevTrsf = null;
			if( m_DataManager.PartIDList.Count > 0 ) {
				prevTrsf = m_ViewManager.ViewObjectMap[ m_DataManager.PartIDList.First() ]?.AISHandle?.LocalTransformation();
			}
			if( prevTrsf == null ) {
				prevTrsf = new gp_Trsf();
			}

			// transform the workpiece display
			foreach( string ID in m_DataManager.PartIDList ) {
				m_ViewManager.ViewObjectMap[ ID ]?.AISHandle?.SetLocalTransformation( trsf );
			}

			// apply camera follow
			V3d_View view = m_Viewer.GetView();
			if( view == null ) {
				return;
			}

			// calculate relative transformation (delta)
			// deltaTrsf = newTrsf * oldTrsf^-1
			gp_Trsf inversePrevious = prevTrsf.Inverted();
			gp_Trsf deltaTrsf = trsf.Multiplied( inversePrevious );

			// get current camera parameters
			double eyeX = 0, eyeY = 0, eyeZ = 0;
			double atX = 0, atY = 0, atZ = 0;
			double upX = 0, upY = 0, upZ = 0;

			view.Eye( ref eyeX, ref eyeY, ref eyeZ );
			view.At( ref atX, ref atY, ref atZ );
			view.Up( ref upX, ref upY, ref upZ );

			// apply only the delta transformation to camera
			gp_Pnt oldEye = new gp_Pnt( eyeX, eyeY, eyeZ );
			gp_Pnt oldAt = new gp_Pnt( atX, atY, atZ );
			gp_Dir oldUp = new gp_Dir( upX, upY, upZ );

			gp_Pnt newEye = oldEye.Transformed( deltaTrsf );
			gp_Pnt newAt = oldAt.Transformed( deltaTrsf );
			gp_Dir newUp = oldUp.Transformed( deltaTrsf );

			// disable immediate view update to prevent flickering
			bool previousImmediateUpdate = view.SetImmediateUpdate( false );

			// apply new camera parameters
			view.SetEye( newEye.X(), newEye.Y(), newEye.Z() );
			view.SetAt( newAt.X(), newAt.Y(), newAt.Z() );
			view.SetUp( newUp.X(), newUp.Y(), newUp.Z() );

			if( isUnLockViewerUpdate ) {
				// restore immediate update and update view once
				view.SetImmediateUpdate( previousImmediateUpdate );
			}
		}

		public void SetToolVecReverse()
		{
			// one shot edit, multi edit supported
			if( !ValidateBeforeOneShotEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			foreach( string szPathID in szPathIDList ) {
				if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData ) ) {
					continue;
				}

				// toggle reverse state
				craftData.IsToolVecReverse = !craftData.IsToolVecReverse;
			}
			ShowCAMData( szPathIDList );
		}

		// TODO: this is temp version
		public void SetFixedToolVec()
		{
			// one shot edit, multi edit supported
			if( !ValidateBeforeOneShotEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			foreach( string szPathID in szPathIDList ) {
				if( !DataGettingHelper.GetPathType( szPathID, out PathType pathType )
					|| pathType != PathType.Contour ) {
					continue;
				}
				if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData )
					|| !DataGettingHelper.GetGeomDataByID( szPathID, out IGeomData geomData ) ) {
					continue;
				}

				// clear all tool vec modify data
				craftData.ClearToolVecModify();
				craftData.StartPntToolVecData = new StartPntToolVecParam();

				// get start point
				ToolVecActionDataHandler dataHandler = new ToolVecActionDataHandler( szPathID );
				ISetToolVecPoint startPoint = dataHandler.GetPointByCADIndex( dataHandler.GetStartPointCADIndex() );

				// get averge tool vec
				gp_Dir avgDir = ( geomData as ContourGeomData ).RefCenterDir.Direction();

				// get ms angle
				Tuple<double, double> avgMS_deg = ToolVecHelper.GetMSAngleFromToolVec( avgDir, startPoint );

				// get ab angle
				Tuple<double, double> avgAB_deg = ToolVecHelper.GetABAngleFromToolVec( avgDir, startPoint );

				ToolVecAngleData Angle = new ToolVecAngleData()
				{
					RA_deg = avgAB_deg.Item1,
					RB_deg = avgAB_deg.Item2,
					Master_deg = avgMS_deg.Item1,
					Slave_deg = avgMS_deg.Item2
				};
				if( craftData.IsPathReverse ) {
					craftData.StartPntToolVecData.StartPnt = new ToolVecModifyData( Angle, EToolVecInterpolateType.VectorInterpolation );
				}
				else {
					craftData.StartPntToolVecData.StartPnt = new ToolVecModifyData( Angle, EToolVecInterpolateType.Normal );
					craftData.StartPntToolVecData.EndPnt.InterpolateType = EToolVecInterpolateType.VectorInterpolation;
				}
			}
			ShowCAMData( szPathIDList );
		}

		public void SetTraverseData()
		{
			// action edit, multi edit supported
			if( IsSameAction( EditActionType.SetTraverse ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			TraverseAction action = new TraverseAction( m_DataManager, szPathIDList, m_Viewer );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetEntryAndExitParam()
		{
			// action edit, not depends on path
			if( IsSameAction( EditActionType.SetEntryAndExitParam ) ) {
				m_CurrentAction.End();
				return;
			}
			EntryAndExitAction action = new EntryAndExitAction( m_DataManager );
			action.PropertyChanged += ShowAllCAMData;
			StartEditAction( action );
		}

		public void SetStandardPattern()
		{
			if( IsSameAction( EditActionType.SetPattern ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, true ) ) {
				return;
			}

			PatternAction action = new PatternAction( m_DataManager, m_Viewer, m_ViewManager, szPathIDList );
			action.PropertyChanged += OnPathShapeTypeChange;
			StartEditAction( action );
		}

		public void SetPathEdit()
		{
			if( IsSameAction( EditActionType.PathEdit ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, true ) ) {
				return;
			}

			PathEditAction action = new PathEditAction( m_DataManager, szPathIDList, m_Viewer );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetPathCompensation()
		{
			if( IsSameAction( EditActionType.PathCompensation ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			PathCompensationAction action = new PathCompensationAction( m_DataManager, szPathIDList );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetContourEdit()
		{
			// action edit, no multi edit supported
			if( IsSameAction( EditActionType.ContourEdit ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, false ) ) {
				return;
			}
			string szPathID = szPathIDList[ 0 ];
			if( !DataGettingHelper.GetPathType( szPathID, out PathType pathType )
				|| pathType != PathType.Contour ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]此功能僅支援輪廓路徑", MyApp.NoticeType.Hint );
				return;
			}
			ContourEditAction action = new ContourEditAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, szPathID );
			action.PropertyChanged += () => ShowCAMData( new List<string>() { szPathID } );
			StartEditAction( action );
		}

		public void SetPathLayer( int nTechLayer )
		{
			// one shot edit, multi edit supported
			if( !ValidateBeforeOneShotEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			foreach( string szPathID in szPathIDList ) {
				if( !DataGettingHelper.GetPathType( szPathID, out PathType pathType ) ) {
					continue;
				}
				if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData ) ) {
					continue;
				}
				craftData.TechLayer = nTechLayer;
			}
			m_MainPathRenderer.Show( szPathIDList );
		}

		public void SetCalibrationREF()
		{
			if( m_DataManager.PartIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先新增工件", MyApp.NoticeType.Hint );
				return;
			}

			if( IsSameAction( EditActionType.CalibrationREF ) ) {
				m_CurrentAction.End();
				return;
			}
			CalibrationREFAction action = new CalibrationREFAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			StartEditAction( action );
		}

		public void SetMicroJoint()
		{
			if( IsSameAction( EditActionType.MicroJoint ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, false ) ) {
				return;
			}
			MicroJointAction action = new MicroJointAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, szPathIDList.First() );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		#endregion

		// sort API
		public void SetManualOrder()
		{
			if( IsSameAction( EditActionType.ManualOrder ) ) {
				m_CurrentAction.End();
				return;
			}
			EndActionIfNotDefault();
			ManualOrderAction action = new ManualOrderAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			action.ShowOrderData += ShowAllCAMData;
			action.RaiseActionStart += ManualStartIO;

			StartEditAction( action );
		}

		void ManualStartIO( bool isStart )
		{
			if( isStart ) {
				m_ToolVecRenderer.SetPauseRefreshAndHide( true );
				m_MainPathRenderer.SetPauseRefresh( true );
				m_TraverseRenderer.SetPauseRefreshAndHide( true );
				m_CraftRenderer.SetPauseRefreshAndHide( true );
				m_OrientationRenderer.SetPauseRefreshAndHide( true );
			}
			else {

				// re-display all data
				m_ToolVecRenderer.SetPauseRefreshAndHide( false );
				m_MainPathRenderer.SetPauseRefresh( false );
				m_TraverseRenderer.SetPauseRefreshAndHide( false );
				m_CraftRenderer.SetPauseRefreshAndHide( false );
				m_OrientationRenderer.SetPauseRefreshAndHide( false );

				// traverse will change during order change
				m_TraverseRenderer.Show();
			}
			m_Viewer.UpdateView();
		}

		// TODO: is it making sense to use cache here?
		public void AutoSortProcess()
		{
			// one shot edit, no multi edit supported
			if( !ValidateBeforeOneShotEdit( out List<string> szPathIDList, false ) ) {
				return;
			}
			string szStartPathID = szPathIDList[ 0 ];

			// get start point
			gp_Pnt currentPoint = CacheHelper.GetProcessStartPoint( szStartPathID ).Point;
			if( currentPoint == null ) {
				return;
			}

			// init data manager
			List<string> pathIDList = new List<string>( m_DataManager.PathIDList );
			List<string> newPathIDList = new List<string> { szStartPathID };

			// visited path recorded container
			bool[] visited = new bool[ pathIDList.Count ];
			int startIdx = pathIDList.IndexOf( szStartPathID );
			visited[ startIdx ] = true;
			int visitedCount = 1;
			while( visitedCount < pathIDList.Count ) {
				double minDistanceSq = double.MaxValue;
				int nearestIdx = -1;
				gp_Pnt nearestPoint = null;
				for( int i = 0; i < pathIDList.Count; i++ ) {
					if( visited[ i ] ) {
						continue;
					}
					gp_Pnt nextStartPoint = CacheHelper.GetProcessStartPoint( pathIDList[ i ] ).Point;
					double distanceSq;
					if( nextStartPoint == null ) {
						distanceSq = double.MaxValue;
					}
					else {
						distanceSq = currentPoint.SquareDistance( nextStartPoint );
					}
					if( distanceSq < minDistanceSq ) {
						minDistanceSq = distanceSq;
						nearestPoint = nextStartPoint;
						nearestIdx = i;
					}
				}
				if( nearestIdx != -1 ) {
					currentPoint = nearestPoint;
					visited[ nearestIdx ] = true;
					visitedCount++;
					newPathIDList.Add( pathIDList[ nearestIdx ] );
				}
				else {
					break;
				}
			}
			m_DataManager.PathIDList.Clear();
			m_DataManager.PathIDList.AddRange( newPathIDList );

			// select focus on first path
			if( m_DefaultAction is SelectPathAction selectAction ) {
				selectAction.ClearSelection();
				selectAction.SelectPathByID( m_DataManager.PathIDList.First() );
			}
			ShowAllCAMData();
		}

		public void AutoOptimizeIKContinuity()
		{
			// one-shot action, no path selection required
			EndActionIfNotDefault();

			List<string> pathIDList = m_DataManager.PathIDList;
			if( pathIDList.Count == 0 ) {
				return;
			}

			double prevMaster_rad = 0;
			double prevSlave_rad = 0;

			foreach( string szPathID in pathIDList ) {
				if( !DataGettingHelper.GetPathType( szPathID, out PathType pathType )
					|| pathType != PathType.Contour ) {
					// skip non-contour but keep previous MS for next contour path
					continue;
				}
				if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData ) ) {
					continue;
				}

				// write previous path end MS as initial value for this path's IK solve
				craftData.InitMaster_rad = prevMaster_rad;
				craftData.InitSlave_rad = prevSlave_rad;

				// trigger cache rebuild
				craftData.CAMFactorChanged?.Invoke();

				// read this path's end MS for next path
				if( CacheHelper.GetMainPathEndMS( szPathID, out double endMaster_rad, out double endSlave_rad ) ) {
					prevMaster_rad = endMaster_rad;
					prevSlave_rad = endSlave_rad;
				}
			}
			ShowAllCAMData();
			MyApp.Logger.ShowOnLogPanel( "[操作完成]已完成起點旋轉軸自動優化", MyApp.NoticeType.Hint );
		}

		// convert NC
		public void ConverNC()
		{
			// stop current action
			EndActionIfNotDefault();
			const string NC_FILE_DEFAULT_NAME = "FiveAxisProgram.nc";
			using( SaveFileDialog saveFileDialog = new SaveFileDialog() ) {
				saveFileDialog.Filter = "NC Files (*.nc)|*.nc|All Files (*.*)|*.*";
				saveFileDialog.DefaultExt = "nc";
				saveFileDialog.FileName = NC_FILE_DEFAULT_NAME;
				saveFileDialog.Title = "儲存 NC 檔案";
				saveFileDialog.RestoreDirectory = true;

				if( saveFileDialog.ShowDialog() != DialogResult.OK ) {
					return;
				}

				string szSaveFilePath = saveFileDialog.FileName;
				string szFileName = Path.GetFileName( szSaveFilePath );
				string szTempFilePath = Path.Combine( Path.GetTempPath(), NC_FILE_DEFAULT_NAME );
				NCWriter writer = new NCWriter( m_DataManager );
				bool bNCCreateSuccess = writer.ConvertSuccess( szFileName, szTempFilePath, out string szCreateErrorMessage );

				if( !bNCCreateSuccess ) {
					MyApp.Logger.ShowOnLogPanel( $"[NC轉出狀態]轉出NC失敗: {szCreateErrorMessage}\n" + "[檔案傳輸狀態]傳輸NC檔案失敗", MyApp.NoticeType.Error );

					if( File.Exists( szTempFilePath ) ) {
						try {
							File.Delete( szTempFilePath );
						}
						catch {

							// this would not happen often, but still need to handle
							MyApp.Logger.ShowOnLogPanel( $"[清理臨時檔案] 刪除失敗: {szTempFilePath}", MyApp.NoticeType.Warning );
						}
					}
					return;
				}

				try {
					File.Copy( szTempFilePath, szSaveFilePath, true );
					File.Delete( szTempFilePath );
				}
				catch( Exception ex ) {
					MyApp.Logger.ShowOnLogPanel( $"[NC轉出狀態]儲存檔案失敗: {ex.Message}", MyApp.NoticeType.Error );
					if( File.Exists( szTempFilePath ) ) {
						try {
							File.Delete( szTempFilePath );
						}
						catch {

							// this would not happen often, but still need to handle
							MyApp.Logger.ShowOnLogPanel( $"[清理臨時檔案] 刪除失敗: {szTempFilePath}", MyApp.NoticeType.Warning );
						}
					}
					return;
				}

				// transmit file if CNC IP is set
				if( string.IsNullOrWhiteSpace( MyApp.CNCIP ) ) {
					MyApp.Logger.ShowOnLogPanel( "[NC轉出狀態]成功轉出NC\n" + "[檔案傳輸狀態]未與控制器進行連線，檔案無法傳輸", MyApp.NoticeType.Warning, true );
				}
				else {
					bool bTransmitSuccess = FTPTransmission.FileTransmit( szSaveFilePath, MyApp.CNCIP, szFileName, out string szTransmitErrorMessage );

					if( bTransmitSuccess ) {
						MyApp.Logger.ShowOnLogPanel( "[NC轉出狀態]成功轉出NC\n" + "[檔案傳輸狀態]傳輸NC檔案成功", MyApp.NoticeType.Hint, true );
					}
					else {
						MyApp.Logger.ShowOnLogPanel( "[NC轉出狀態]成功轉出NC\n" + $"[檔案傳輸狀態]傳輸NC檔案失敗：{szTransmitErrorMessage}", MyApp.NoticeType.Warning, true );
					}
				}
			}
		}

		// path added
		void OnPathAdded( List<string> newPathIDs )
		{
			foreach( string szID in newPathIDs ) {
				if( string.IsNullOrEmpty( szID ) ) {
					continue;
				}

				// add a new node to the tree view
				int nodeIndex = m_DataManager.PathIDList.IndexOf( szID ) + 1; // 1 based index
				if( nodeIndex == -1 ) {
					continue;
				}
				string szNodeID = PATH_NODE_PREFIX + nodeIndex.ToString();
				TreeNode node = new TreeNode( szNodeID );
				m_ViewManager.PathNode.Nodes.Add( node );
				m_ViewManager.TreeNodeMap.Add( szNodeID, node );
			}

			// update tree view and viewer
			m_ViewManager.PathNode.ExpandAll();
			ShowAllCAMData();
			m_Viewer.UpdateView();
		}

		// selection changed
		void OnPathSelectionChange()
		{
			if( m_CurrentAction.ActionType != EditActionType.SelectObject ) {
				return;
			}
			List<string> szPathIDList = GetSelectedIDList();
			EditableInfo editableInfo = new EditableInfo();

			// no multi edit: start point, tool vec, sort
			if( szPathIDList.Count > 1 ) {
				editableInfo.IsStartPointEditable = false;
				editableInfo.IsToolVecEditable = false;
				editableInfo.IsAutoOrderEditable = false;
			}

			// closed path editable only: start point, overcut, lead line, change lead dir
			foreach( string szPathID in szPathIDList ) {
				if( !DataGettingHelper.GetGeomDataByID( szPathID, out IGeomData geomData ) ) {
					continue;
				}
				if( !geomData.IsClosed ) {
					editableInfo.IsStartPointEditable = false;
					editableInfo.IsOverCutEditable = false;
					editableInfo.IsLeadLineEditable = false;
					break;
				}

				if( geomData.PathType != PathType.Contour ) {
					editableInfo.IsPathReverseEditable = false;
					editableInfo.IsToolVecEditable = false;
					editableInfo.IsToolVecReverseEditable = false;
					editableInfo.IsFixedToolVecEditable = false;
				}

				if( geomData.PathType == PathType.Contour ) {
					editableInfo.IsPathCompensateEditable = false;
				}
			}
			PathPropertyChanged?.Invoke( editableInfo );
		}

		void OnPathShapeTypeChange( PathType type, List<string> szPathIDList )
		{
			ShowCAMData( szPathIDList );
			PathShapeTypeChanged?.Invoke( type );
		}

		#region Show CAM

		void ShowAllCAMData()
		{
			// take all path IDs
			List<string> pathIDList = m_DataManager.PathIDList;
			ShowCAMData( pathIDList );
		}

		void ShowCAMData( List<string> pathIDList )
		{
			m_MainPathRenderer.Show( pathIDList );
			m_ToolVecRenderer.Show( pathIDList );
			m_OrientationRenderer.Show( pathIDList );
			m_IndexRenderer.Show();
			m_CraftRenderer.Show( pathIDList );
			m_TraverseRenderer.Show( pathIDList );
			m_Viewer.UpdateView();
		}

		void ShowTransedCAMData( gp_Trsf trsf )
		{
			// draw with translated location
			m_MainPathRenderer.ShowTrans( trsf );
			m_ToolVecRenderer.ShowTrans( trsf );
			m_IndexRenderer.ShowTrans( trsf );

			// do not need to redraw ,just need trsf
			m_OrientationRenderer.Trans( trsf );
			m_CraftRenderer.Trans( trsf );
			m_TraverseRenderer.Trans( trsf );
		}

		void SetTrans( Dictionary<MachineComponentType, List<gp_Trsf>> transMap, bool isUnLockViewerUpdate = true )
		{
			// update camera FIRST before setting any transformations
			gp_Trsf workPieceTrsf = transMap[ MachineComponentType.WorkPiece ].Last();

			// show
			m_MachineRender.ShowToolVecEditResult( transMap );
			ShowTransedCAMData( workPieceTrsf );
			SetWorkPieceDisplayTransform( workPieceTrsf, isUnLockViewerUpdate );
			m_Viewer.UpdateView();
		}

		void RemoveAllCAMData()
		{
			List<string> pathIDList = m_DataManager.PathIDList;
			RemoveCAMData( pathIDList );
		}

		void RemoveCAMData( List<string> pathIDList )
		{
			m_MainPathRenderer.Remove();
			m_ToolVecRenderer.Remove( pathIDList );
			m_CraftRenderer.Remove( pathIDList );
			m_OrientationRenderer.Remove( pathIDList );
			m_IndexRenderer.Remove();
			m_TraverseRenderer.Remove( pathIDList );
			m_Viewer.UpdateView();
		}

		void RemovePathCAMData( List<string> pathIDList )
		{
			m_MainPathRenderer.Remove( pathIDList );
			m_ToolVecRenderer.Remove( pathIDList );
			m_CraftRenderer.Remove( pathIDList );
			m_OrientationRenderer.Remove( pathIDList );
			m_TraverseRenderer.Remove( pathIDList );

			// those info are not recorded on each path, need to refresh all not remove
			m_IndexRenderer.Show();
			m_TraverseRenderer.Show();
			m_Viewer.UpdateView();
		}

		#endregion

		// methods
		List<string> GetSelectedIDList()
		{
			return m_DefaultAction.GetSelectedIDs();
		}

		bool IsSameAction( EditActionType newActionType )
		{
			if( m_CurrentAction != null && m_CurrentAction.ActionType == newActionType ) {
				return true;
			}
			return false;
		}

		bool ValidateBeforeActionEdit( out List<string> szIDList, bool isMutiEditable )
		{
			szIDList = GetSelectedIDList();

			// nothing selected
			if( szIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先選擇路徑", MyApp.NoticeType.Hint );
				EndActionIfNotDefault();
				return false;
			}

			// multiple selected but not allowed
			else if( isMutiEditable == false && szIDList.Count > 1 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]只能選擇一個路徑進行編輯", MyApp.NoticeType.Hint );
				EndActionIfNotDefault();
				return false;
			}
			return true;
		}

		bool ValidateBeforeOneShotEdit( out List<string> szIDList, bool isMutiEditable )
		{
			EndActionIfNotDefault();
			szIDList = GetSelectedIDList();

			// nothing selected
			if( szIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先選擇路徑", MyApp.NoticeType.Hint );
				return false;
			}

			// multiple selected but not allowed
			else if( isMutiEditable == false && szIDList.Count > 1 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]只能選擇一個路徑進行編輯", MyApp.NoticeType.Hint );
				return false;
			}
			return true;
		}

		void RebuildTreeNodes()
		{
			// Clear existing tree nodes and mappings
			m_ViewManager.PathNode.Nodes.Clear();

			// Remove old path node mappings
			var keysToRemove = m_ViewManager.TreeNodeMap.Keys
				.Where( key => key.StartsWith( PATH_NODE_PREFIX ) )
				.ToList();

			foreach( var key in keysToRemove ) {
				m_ViewManager.TreeNodeMap.Remove( key );
			}

			// Rebuild tree nodes with correct indices
			for( int i = 0; i < m_DataManager.PathIDList.Count; i++ ) {
				string pathID = m_DataManager.PathIDList[ i ];
				string szNodeID = PATH_NODE_PREFIX + ( i + 1 ).ToString(); // 1-based index

				TreeNode node = new TreeNode( szNodeID );
				m_ViewManager.PathNode.Nodes.Add( node );
				m_ViewManager.TreeNodeMap.Add( szNodeID, node );
			}

			// Expand all nodes
			m_ViewManager.PathNode.ExpandAll();
		}
		// edit actions
		protected override void OnEditActionStart( IEditorAction action )
		{
			base.OnEditActionStart( action );
			if( action.ActionType == EditActionType.OverCut
				|| action.ActionType == EditActionType.SetLead
				|| action.ActionType == EditActionType.SetTraverse
				|| action.ActionType == EditActionType.SetPattern
				|| action.ActionType == EditActionType.PathEdit
				|| action.ActionType == EditActionType.ToolVec
				|| action.ActionType == EditActionType.MicroJoint
				|| action.ActionType == EditActionType.ContourEdit ) {

				// lock main form
				m_TreeView.Enabled = false;
				RaiseWithDlgActionStatusChange?.Invoke( EActionStatus.Start );
			}

			// chnage display
			RaiseCAMActionStatusChange( action.ActionType, EActionStatus.Start );
		}

		protected override void OnEditActionEnd( IEditorAction action )
		{
			// these action will show dialog, need to lock ui
			if( action.ActionType == EditActionType.OverCut
				|| action.ActionType == EditActionType.SetLead
				|| action.ActionType == EditActionType.SetTraverse
				|| action.ActionType == EditActionType.SetPattern
				|| action.ActionType == EditActionType.PathEdit
				|| action.ActionType == EditActionType.ToolVec
				|| action.ActionType == EditActionType.MicroJoint
				|| action.ActionType == EditActionType.ContourEdit ) {

				// unlock main form
				m_TreeView.Enabled = true;
				RaiseWithDlgActionStatusChange?.Invoke( EActionStatus.End );
			}
			RaiseCAMActionStatusChange?.Invoke( action.ActionType, EActionStatus.End );
			base.OnEditActionEnd( action );
		}

		// Shared: auto find outermost face by ray strategies, then find D1 faces and add free boundary paths
		void AutoFindOutesetFaceAndAddPath( List<gp_Ax1> strategies )
		{
			// Step 1: For each strategy, collect bbox-filtered face candidates and find the outermost intersected face
			TopoDS_Face outerTargetFace = null;
			foreach( gp_Ax1 strategy in strategies ) {
				List<TopoDS_Face> bboxFilteredFaces = GetVisibleFaceCandidatesByRay( strategy );
				if( bboxFilteredFaces.Count == 0 ) {
					continue;
				}
				if( GeometryTool.FindOutermostFaceAlongPrincipalAxis( bboxFilteredFaces, strategy.Location(), strategy.Direction(), out outerTargetFace ) ) {
					break;
				}
			}

			if( outerTargetFace == null ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]找不到相交的面", MyApp.NoticeType.Hint );
				EndActionIfNotDefault();
				return;
			}

			// Step 2: Borrow SelectFaceAction's calculation to find D1 continuous faces from the outermost face
			// Note: Does not start interactive mode, only uses the geometry calculation
			SelectFaceAction faceSearchAction = new SelectFaceAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			List<TopoDS_Face> d1ContinuousFaceList = faceSearchAction.FindD1ContFaces( outerTargetFace );
			if( d1ContinuousFaceList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]找不到 D1 連續面", MyApp.NoticeType.Hint );
				EndActionIfNotDefault();
				return;
			}

			// Step 3: Convert D1 continuous faces into sewn shape groups
			List<TopoDS_Shape> sewedFaceGroupList = faceSearchAction.GetResultFromFaces( d1ContinuousFaceList );
			if( sewedFaceGroupList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]面群組建立失敗", MyApp.NoticeType.Hint );
				EndActionIfNotDefault();
				return;
			}

			m_CurrentAction.End();

			// Step 4: Extract free boundaries from face groups and add as paths
			AddPathsFromFaceGroup( sewedFaceGroupList );
		}

		// Collect visible faces only from parts whose bounding box intersects the given ray.
		// This avoids face-level processing on parts that cannot possibly intersect.
		List<TopoDS_Face> GetVisibleFaceCandidatesByRay( gp_Ax1 strategy )
		{
			List<TopoDS_Face> faceList = new List<TopoDS_Face>();
			foreach( string partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				if( DataGettingHelper.GetShapeObject( partID, out IShapeObject shapeObject ) == false ) {
					continue;
				}

				// Part-level BBox pre-filter: skip entire part if ray cannot intersect
				BoundingBox partBBox = new BoundingBox( shapeObject.Shape );
				if( GeometryTool.RayIntersectsBBox( strategy.Location(), strategy.Direction(), partBBox ) == false ) {
					continue;
				}

				// Collect all faces from this part
				TopExp_Explorer exp = new TopExp_Explorer(
					shapeObject.Shape, TopAbs_ShapeEnum.TopAbs_FACE );
				for( ; exp.More(); exp.Next() ) {
					faceList.Add( TopoDS.ToFace( exp.Current() ) );
				}
			}
			return faceList;
		}

		// Build compound of all visible workpieces and return its bounding box.
		// Returns null if no visible workpiece exists.
		BoundingBox GetVisibleWorkpieceBBox()
		{
			List<TopoDS_Shape> shapeList = new List<TopoDS_Shape>();
			foreach( string partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				if( DataGettingHelper.GetShapeObject( partID, out IShapeObject shapeObject ) == false ) {
					continue;
				}
				shapeList.Add( shapeObject.Shape );
			}
			if( shapeList.Count == 0 ) {
				return null;
			}
			TopoDS_Shape compound = ShapeTool.MakeCompound( shapeList );
			return new BoundingBox( compound );
		}
	}
}
