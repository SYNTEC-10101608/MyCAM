using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MyCAM.App;
using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.Post;
using OCC.AIS;
using OCC.Aspect;
using OCC.BRepBuilderAPI;
using OCC.BRepPrimAPI;
using OCC.GC;
using OCC.Geom;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Prs3d;
using OCC.Quantity;
using OCC.ShapeAnalysis;
using OCC.TCollection;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using OCCViewer;

namespace MyCAM.Editor
{
	internal class CAMEditor : EditorBase
	{
		// to notice main form
		public Action<bool, bool> PathPropertyChanged; // isClosed, hasLead
		public Action<EditActionType, EActionStatus> RaiseCAMActionStatusChange;

		// action with dialog need to disable main form
		// because other action enterance might close this action, but without closing this form
		public Action<EActionStatus> RaiseWithDlgActionStatusChange;

		public CAMEditor( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
			m_DataManager.PathAdded += OnPathAdded;

			// default action is select object action
			m_DefaultAction = new SelectObjectAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, ESelectObjectType.Path );
			( m_DefaultAction as SelectObjectAction ).TreeSelectionChange += OnTreeSelectionChange;
			( m_DefaultAction as SelectObjectAction ).RemovePath += RemovePath;
			( m_DefaultAction as SelectObjectAction ).PathOrderMove += MoveProcess;
		}

		// for viewer resource handle
		List<AIS_Line> m_ToolVecAISList = new List<AIS_Line>(); // need refresh, no need activate
		List<AIS_Shape> m_OrientationAISList = new List<AIS_Shape>(); // need refresh, no need activate
		List<AIS_TextLabel> m_IndexList = new List<AIS_TextLabel>(); // need refresh, no need activate
		List<AIS_Line> m_LeadAISList = new List<AIS_Line>(); // need refresh, no need activate
		List<AIS_Shape> m_LeadOrientationAISList = new List<AIS_Shape>(); // need refresh, no need activate
		List<AIS_Line> m_OverCutAISList = new List<AIS_Line>(); // need refresh, no need activate
		List<AIS_Line> m_TraverseAISList = new List<AIS_Line>(); // need refresh, no need activate
		List<AIS_Shape> m_FrogLeapAISList = new List<AIS_Shape>(); // need refresh, no need activate

		enum EvecType
		{
			ToolVec,
			TangentVec,
			NormalVec,
		}

		// CAM data show options
		bool m_ShowToolVec = true;
		bool m_ShowOrder = true;
		bool m_ShowOrientation = true;
		bool m_ShowTraversePath = true;

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

			// init viewer
			foreach( var pathID in m_DataManager.PathIDList ) {
				if( m_ViewManager.ViewObjectMap[ pathID ].Visible == false ) {
					continue;
				}
				AIS_InteractiveObject obj = m_ViewManager.ViewObjectMap[ pathID ].AISHandle;
				m_Viewer.GetAISContext().Display( obj, false );
			}
			ShowCAMData();
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
			HideCAMData();
		}

		// APIs

		public void SetShowToolVec( bool isShowToolVec )
		{
			m_ShowToolVec = isShowToolVec;
			ShowCAMData();
		}

		public void SetShowOrientation( bool isShowOrientation )
		{
			m_ShowOrientation = isShowOrientation;
			ShowCAMData();
		}

		public void SetShowOrder( bool isShowOrder )
		{
			m_ShowOrder = isShowOrder;
			ShowCAMData();
		}

		public void SetShowTraversePath( bool isShowTraversePath )
		{
			m_ShowTraversePath = isShowTraversePath;
			ShowCAMData();
		}

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
			List<TopoDS_Wire> pathWireList = new List<TopoDS_Wire>();
			TopTools_IndexedDataMapOfShapeListOfShape edgeFaceMap = new TopTools_IndexedDataMapOfShapeListOfShape();
			foreach( TopoDS_Shape oneFace in selectedFaceGroupList ) {
				ShapeAnalysis_FreeBounds freeBounds = new ShapeAnalysis_FreeBounds( oneFace );

				// add to map
				TopExp.MapShapesAndAncestors( oneFace, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref edgeFaceMap );

				// get all closed wires
				TopExp_Explorer wireExp = new TopExp_Explorer( freeBounds.GetClosedWires(), TopAbs_ShapeEnum.TopAbs_WIRE );
				while( wireExp.More() ) {
					pathWireList.Add( TopoDS.ToWire( wireExp.Current() ) );
					wireExp.Next();
				}
			}
			m_DataManager.AddPath( pathWireList, edgeFaceMap );
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
			SelectPathAction action = new SelectPathAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, selectedFaceGroupList );
			StartEditAction( action );
		}

		public void EndSelectPath_Manual()
		{
			if( m_CurrentAction.ActionType != EditActionType.SelectPath ) {
				return;
			}
			( (SelectPathAction)m_CurrentAction ).SelectDone();
		}

		public void RemovePath()
		{
			// stop current action
			EndActionIfNotDefault();
			string szPathID = GetSelectedPathID();
			if( string.IsNullOrEmpty( szPathID ) || !m_DataManager.ShapeDataMap.ContainsKey( szPathID ) ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先選擇路徑", MyApp.NoticeType.Hint );
				return;
			}

			// remove from data manager
			m_DataManager.RemovePath( szPathID );

			// remove from viewer
			m_Viewer.GetAISContext().Remove( m_ViewManager.ViewObjectMap[ szPathID ].AISHandle, false );
			m_ViewManager.ViewObjectMap.Remove( szPathID );

			// remove from tree view
			TreeNode node = m_ViewManager.TreeNodeMap[ szPathID ];
			m_TreeView.Nodes.Remove( node );
			m_ViewManager.TreeNodeMap.Remove( szPathID );

			ShowCAMData();
		}

		#region Set CAM

		public void SetStartPoint()
		{
			// is on/off button, so end it then return
			if( IsSameAction( EditActionType.StartPoint ) ) {
				m_CurrentAction.End();
				return;
			}
			bool isGetIDSuccess = ValidateSelectAndEndExAction( out string szPathID );
			if( isGetIDSuccess == false ) {
				return;
			}
			PathData pathData = (PathData)m_DataManager.ShapeDataMap[ szPathID ];

			// skip non-closed path
			if( !pathData.CAMData.IsClosed ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]非封閉路徑不支援修改起點", MyApp.NoticeType.Hint );
				return;
			}
			var action = new StartPointAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, pathData.CAMData );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetReverse()
		{
			// stop current action
			EndActionIfNotDefault();
			string szPathID = GetSelectedPathID();
			if( string.IsNullOrEmpty( szPathID ) || !m_DataManager.ShapeDataMap.ContainsKey( szPathID ) ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先選擇路徑", MyApp.NoticeType.Hint );
				return;
			}
			PathData pathData = (PathData)m_DataManager.ShapeDataMap[ szPathID ];

			// toggle reverse state
			pathData.CAMData.IsReverse = !pathData.CAMData.IsReverse;
			ShowCAMData();
		}

		public void SetOverCut()
		{
			// is on/off button, so end it then return
			if( IsSameAction( EditActionType.OverCut ) ) {
				m_CurrentAction.End();
				return;
			}
			bool isGetIDSuccess = ValidateSelectAndEndExAction( out string szPathID );
			if( isGetIDSuccess == false ) {
				return;
			}
			PathData pathData = (PathData)m_DataManager.ShapeDataMap[ szPathID ];
			OverCutAction action = new OverCutAction( m_DataManager, pathData.CAMData );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetLeadLine()
		{
			// is on/off button, so end it then return
			if( IsSameAction( EditActionType.SetLead ) ) {
				m_CurrentAction.End();
				return;
			}
			bool isGetIDSuccess = ValidateSelectAndEndExAction( out string szPathID );
			if( isGetIDSuccess == false ) {
				return;
			}
			PathData pathData = (PathData)m_DataManager.ShapeDataMap[ szPathID ];
			LeadAction action = new LeadAction( m_DataManager, pathData.CAMData );
			action.PropertyChanged += OnSetLeadPropertyChanged;
			StartEditAction( action );
		}

		public void ChangeLeadDirection()
		{
			// stop current action
			EndActionIfNotDefault();
			string szPathID = GetSelectedPathID();
			if( string.IsNullOrEmpty( szPathID ) || !m_DataManager.ShapeDataMap.ContainsKey( szPathID ) ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先選擇路徑", MyApp.NoticeType.Hint );
				return;
			}
			PathData pathData = (PathData)m_DataManager.ShapeDataMap[ szPathID ];

			// nothing change
			if( pathData.CAMData.IsHasLead == false ) {
				pathData.CAMData.LeadLineParam.IsChangeLeadDirection = false;
				return;
			}
			pathData.CAMData.LeadLineParam.IsChangeLeadDirection = !pathData.CAMData.LeadLineParam.IsChangeLeadDirection;

			// need clone to trigger property changed event
			pathData.CAMData.LeadLineParam = pathData.CAMData.LeadLineParam.Clone();
			ShowCAMData();
		}

		public void SetToolVec()
		{
			// is on/off button, so end it then return
			if( IsSameAction( EditActionType.ToolVec ) ) {
				m_CurrentAction.End();
				return;
			}
			bool isGetIDSuccess = ValidateSelectAndEndExAction( out string szPathID );
			if( isGetIDSuccess == false ) {
				return;
			}
			PathData pathData = (PathData)m_DataManager.ShapeDataMap[ szPathID ];
			ToolVectorAction action = new ToolVectorAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, pathData.CAMData );
			action.PropertyChanged += ShowCAMData;

			// when editing tool vec dilog show/close, need disable/enable main form
			action.RaiseEditingToolVecDlg += ( isStart ) =>
			{
				RaiseWithDlgActionStatusChange?.Invoke( isStart );
			};
			StartEditAction( action );
		}

		public void SetToolVecReverse()
		{
			// stop current action
			EndActionIfNotDefault();
			string szPathID = GetSelectedPathID();
			if( string.IsNullOrEmpty( szPathID ) || !m_DataManager.ShapeDataMap.ContainsKey( szPathID ) ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先選擇路徑", MyApp.NoticeType.Hint );
				return;
			}
			PathData pathData = (PathData)m_DataManager.ShapeDataMap[ szPathID ];

			// toggle reverse state
			pathData.CAMData.IsToolVecReverse = !pathData.CAMData.IsToolVecReverse;
			ShowCAMData();
		}

		public void SeTraverseParam()
		{
			// is on/off button, so end it then return
			if( IsSameAction( EditActionType.SetTraverseParam ) ) {
				m_CurrentAction.End();
				return;
			}
			bool isGetIDSuccess = ValidateSelectAndEndExAction( out string szPathID );
			if( isGetIDSuccess == false ) {
				return;
			}
			PathData pathData = (PathData)m_DataManager.ShapeDataMap[ szPathID ];
			TraverseAction action = new TraverseAction( m_DataManager, pathData.CAMData );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetEntryAndExitParam()
		{
			// is on/off button, so end it then return
			if( IsSameAction( EditActionType.SetEntryAndExitParam ) ) {
				m_CurrentAction.End();
				return;
			}
			EntryAndExitAction action = new EntryAndExitAction( m_DataManager );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		#endregion

		// TODO: refresh tree
		public void MoveProcess( bool bUp )
		{
			// stop current action
			EndActionIfNotDefault();
			string szPathID = GetSelectedPathID();
			if( string.IsNullOrEmpty( szPathID ) || !m_DataManager.ShapeDataMap.ContainsKey( szPathID ) ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先選擇路徑", MyApp.NoticeType.Hint );
				return;
			}
			int nIndex = m_DataManager.PathIDList.IndexOf( szPathID );
			if( nIndex < 0 || nIndex > m_DataManager.PathIDList.Count - 1
				|| bUp && nIndex == 0
				|| !bUp && nIndex == m_DataManager.PathIDList.Count - 1 ) {
				return;
			}
			m_DataManager.PathIDList.RemoveAt( nIndex );
			if( bUp ) {
				m_DataManager.PathIDList.Insert( nIndex - 1, szPathID );
			}
			else {
				m_DataManager.PathIDList.Insert( nIndex + 1, szPathID );
			}
			ShowCAMData();
		}

		// TODO: implement it and refresh tree
		public void AutoSortProcess()
		{
			EndActionIfNotDefault();
			string szPathID = GetSelectedPathID();
			if( string.IsNullOrEmpty( szPathID ) || !m_DataManager.ShapeDataMap.ContainsKey( szPathID ) ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先選擇路徑", MyApp.NoticeType.Hint );
				return;
			}
			List<string> pathIDList = new List<string>( m_DataManager.PathIDList );
			m_DataManager.PathIDList.Clear();
			m_DataManager.PathIDList.Add( szPathID );

			// get start point
			PathData pathData = (PathData)m_DataManager.ShapeDataMap[ szPathID ];
			gp_Pnt currentPoint = pathData.CAMData.GetProcessStartPoint().CADPoint.Point;

			// visited path recorded container
			bool[] visited = new bool[ pathIDList.Count ];
			int startIdx = pathIDList.IndexOf( szPathID );
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
					PathData nextPathData = (PathData)m_DataManager.ShapeDataMap[ pathIDList[ i ] ];
					gp_Pnt nextStartPoint = nextPathData.CAMData.GetProcessStartPoint().CADPoint.Point;
					double distanceSq = currentPoint.SquareDistance( nextStartPoint );
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
					m_DataManager.PathIDList.Add( pathIDList[ nearestIdx ] );
				}
				else {
					break;
				}
			}
			ShowCAMData();
		}

		public void ConverNC()
		{
			// stop current action
			EndActionIfNotDefault();
			NCWriter writer = new NCWriter( m_DataManager.GetCAMDataList(), m_DataManager.MachineData, m_DataManager.EntryAndExitData );
			bool bSuccess = writer.ConvertSuccess_New( out string szErrorMessage );
			if( bSuccess ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提示]成功轉出NC", MyApp.NoticeType.Hint );
			}
			else {
				MyApp.Logger.ShowOnLogPanel( $"轉出NC失敗: {szErrorMessage}", MyApp.NoticeType.Error );
			}
		}

		// path added
		void OnPathAdded( List<string> newPathIDs )
		{
			foreach( string szID in newPathIDs ) {
				if( string.IsNullOrEmpty( szID ) ) {
					return;
				}

				// add a new node to the tree view
				TreeNode node = new TreeNode( szID );
				m_ViewManager.PathNode.Nodes.Add( node );
				m_ViewManager.TreeNodeMap.Add( szID, node );

				// add a new shape to the viewer
				AIS_Shape aisShape = ViewHelper.CreatePathAIS( m_DataManager.ShapeDataMap[ szID ].Shape, 3.0 );
				m_ViewManager.ViewObjectMap.Add( szID, new ViewObject( aisShape ) );
				m_Viewer.GetAISContext().Display( aisShape, false ); // this will also activate
			}

			// update tree view and viewer
			m_ViewManager.PathNode.ExpandAll();
			ShowCAMData();
			m_Viewer.UpdateView();
		}

		// tree selection changed
		void OnTreeSelectionChange()
		{
			if( m_CurrentAction.ActionType != EditActionType.SelectObject ) {
				return;
			}
			string szPathID = GetSelectedPathID();
			if( string.IsNullOrEmpty( szPathID ) || !m_DataManager.ShapeDataMap.ContainsKey( szPathID ) ) {
				return;
			}
			PathData pathData = (PathData)m_DataManager.ShapeDataMap[ szPathID ];
			PathPropertyChanged?.Invoke( pathData.CAMData.IsClosed, pathData.CAMData.IsHasLead );
		}

		#region Show CAM
		void ShowCAMData()
		{
			// TODO: we dont always need to refresh such many things
			ShowToolVec_New();
			ShowOrientation_New();
			ShowIndex();
			ShowOverCut();
			ShowLeadLine();
			ShowLeadOrientation();
			ShowTraversalPath();
			m_Viewer.UpdateView();
		}

		void ShowToolVec_New()
		{
			// clear the previous tool vec
			foreach( AIS_Line toolVecAIS in m_ToolVecAISList ) {
				m_Viewer.GetAISContext().Remove( toolVecAIS, false );
			}
			m_ToolVecAISList.Clear();

			// no need to show
			if( m_ShowToolVec == false ) {
				return;
			}

			// build tool vec
			foreach( CAMData camData in m_DataManager.GetCAMDataList() ) {
				bool isGetSuccess = PathToolVec( camData, out List<(gp_Pnt point, gp_Dir toolVec, bool isModifyToolVec)> points );
				if( isGetSuccess ) {
					for( int i = 0; i < points.Count; i++ ) {
						AIS_Line toolVecAIS = GetVecAIS( points[ i ].point, points[ i ].toolVec, EvecType.ToolVec );
						if( points[ i ].isModifyToolVec ) {
							toolVecAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
							toolVecAIS.SetWidth( 4 );
						}
						m_ToolVecAISList.Add( toolVecAIS );
					}
				}

			}

			// display the tool vec
			foreach( AIS_Line toolVecAIS in m_ToolVecAISList ) {
				m_Viewer.GetAISContext().Display( toolVecAIS, false );
				m_Viewer.GetAISContext().Deactivate( toolVecAIS );
			}
		}

		void ShowLeadLine()
		{
			// clear the previous tool vec
			foreach( AIS_Line toolVecAIS in m_LeadAISList ) {
				m_Viewer.GetAISContext().Remove( toolVecAIS, false );
			}
			m_LeadAISList.Clear();

			// build lead line AIS
			foreach( CAMData camData in m_DataManager.GetCAMDataList() ) {

				// draw lead in
				if( camData.LeadLineParam.LeadIn.Type != LeadLineType.None ) {
					for( int i = 0; i < camData.LeadInCAMPointList.Count - 1; i++ ) {
						CAMPoint currentCAMPoint = camData.LeadInCAMPointList[ i ];
						CAMPoint nextCAMPoint = camData.LeadInCAMPointList[ i + 1 ];
						AIS_Line LeadAISLine = GetLineAIS( currentCAMPoint.CADPoint.Point, nextCAMPoint.CADPoint.Point, Quantity_NameOfColor.Quantity_NOC_GREENYELLOW );
						m_LeadAISList.Add( LeadAISLine );
					}
				}

				// draw lead out
				if( camData.LeadLineParam.LeadOut.Type != LeadLineType.None ) {
					for( int i = 0; i < camData.LeadOutCAMPointList.Count - 1; i++ ) {
						CAMPoint currentCAMPoint = camData.LeadOutCAMPointList[ i ];
						CAMPoint nextCAMPoint = camData.LeadOutCAMPointList[ i + 1 ];
						AIS_Line LeadAISLine = GetLineAIS( currentCAMPoint.CADPoint.Point, nextCAMPoint.CADPoint.Point, Quantity_NameOfColor.Quantity_NOC_GREENYELLOW );
						m_LeadAISList.Add( LeadAISLine );
					}
				}
			}

			// display the lead line
			foreach( AIS_Line toolVecAIS in m_LeadAISList ) {
				m_Viewer.GetAISContext().Display( toolVecAIS, false );
				m_Viewer.GetAISContext().Deactivate( toolVecAIS );
			}
		}

		void ShowOrientation()
		{
			// clear the previous orientation
			foreach( AIS_Shape orientationAIS in m_OrientationAISList ) {
				m_Viewer.GetAISContext().Remove( orientationAIS, false );
			}
			m_OrientationAISList.Clear();

			// no need to show
			if( m_ShowOrientation == false ) {
				return;
			}


			// build orientation
			foreach( CAMData camData in m_DataManager.GetCAMDataList() ) {
				gp_Pnt showPoint = camData.CAMPointList[ 0 ].CADPoint.Point;
				gp_Dir orientationDir = new gp_Dir( camData.CAMPointList[ 0 ].CADPoint.TangentVec.XYZ() );
				if( camData.IsReverse ) {
					orientationDir.Reverse();
				}
				AIS_Shape orientationAIS = GetOrientationAIS( showPoint, orientationDir );
				m_OrientationAISList.Add( orientationAIS );
			}

			// display the orientation
			foreach( AIS_Shape orientationAIS in m_OrientationAISList ) {
				m_Viewer.GetAISContext().Display( orientationAIS, false );
				m_Viewer.GetAISContext().Deactivate( orientationAIS );
			}
		}

		void ShowOrientation_New()
		{
			// clear the previous orientation
			foreach( AIS_Shape orientationAIS in m_OrientationAISList ) {
				m_Viewer.GetAISContext().Remove( orientationAIS, false );
			}
			m_OrientationAISList.Clear();

			// no need to show
			if( m_ShowOrientation == false ) {
				return;
			}

			// build orientation
			foreach( CAMData camData in m_DataManager.GetCAMDataList() ) {
				(int, int) startPoint = camData.NewStartPoint;
				gp_Pnt showPoint = camData.CADSegmentList[ startPoint.Item1 ].PointList[ startPoint.Item2 ].Point;
				gp_Dir orientationDir = new gp_Dir( camData.CADSegmentList[ startPoint.Item1 ].PointList[ startPoint.Item2 ].TangentVec.XYZ() );
				if( camData.IsReverse ) {
					orientationDir.Reverse();
				}
				AIS_Shape orientationAIS = GetOrientationAIS( showPoint, orientationDir );
				m_OrientationAISList.Add( orientationAIS );
			}

			// display the orientation
			foreach( AIS_Shape orientationAIS in m_OrientationAISList ) {
				m_Viewer.GetAISContext().Display( orientationAIS, false );
				m_Viewer.GetAISContext().Deactivate( orientationAIS );
			}
		}

		void ShowLeadOrientation()
		{
			// clear the previous orientation
			foreach( AIS_Shape orientationAIS in m_LeadOrientationAISList ) {
				m_Viewer.GetAISContext().Remove( orientationAIS, false );
			}
			m_LeadOrientationAISList.Clear();

			// build orientation
			foreach( CAMData camData in m_DataManager.GetCAMDataList() ) {

				// path with lead in
				if( camData.LeadLineParam.LeadIn.Type != LeadLineType.None ) {
					if( camData.LeadInCAMPointList.Count == 0 ) {
						break;
					}
					gp_Pnt leadInStartPoint = camData.LeadInCAMPointList.First().CADPoint.Point;
					gp_Dir leadInOrientationDir = new gp_Dir( camData.LeadInCAMPointList.First().CADPoint.TangentVec.XYZ() );
					AIS_Shape orientationAIS = GetOrientationAIS( leadInStartPoint, leadInOrientationDir );
					m_LeadOrientationAISList.Add( orientationAIS );
				}

				// path with lead out
				if( camData.LeadLineParam.LeadOut.Type != LeadLineType.None ) {
					if( camData.LeadOutCAMPointList.Count == 0 ) {
						break;
					}
					gp_Pnt leadOutEndPoint = camData.LeadOutCAMPointList.Last().CADPoint.Point;
					gp_Dir leadOutOrientationDir = new gp_Dir( camData.LeadOutCAMPointList.Last().CADPoint.TangentVec.XYZ() );
					AIS_Shape orientationAIS = GetOrientationAIS( leadOutEndPoint, leadOutOrientationDir );
					m_LeadOrientationAISList.Add( orientationAIS );
				}
			}

			// display the orientation
			foreach( AIS_Shape orientationAIS in m_LeadOrientationAISList ) {
				m_Viewer.GetAISContext().Display( orientationAIS, false );
				m_Viewer.GetAISContext().Deactivate( orientationAIS );
			}
		}

		void ShowIndex()
		{
			// clear the previous text label
			foreach( AIS_TextLabel textLabel in m_IndexList ) {
				m_Viewer.GetAISContext().Remove( textLabel, false );
			}
			m_IndexList.Clear();

			// no need to show
			if( m_ShowOrder == false ) {
				return;
			}

			// create text label
			int nCurrentIndex = 0;
			foreach( CAMData camData in m_DataManager.GetCAMDataList() ) {
				gp_Pnt location = camData.CAMPointList[ 0 ].CADPoint.Point;
				string szIndex = nCurrentIndex++.ToString();

				// create text label ais
				AIS_TextLabel textLabel = new AIS_TextLabel();
				textLabel.SetText( new TCollection_ExtendedString( szIndex ) );
				textLabel.SetPosition( location );
				textLabel.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
				textLabel.SetHeight( 20 );
				textLabel.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );
				m_IndexList.Add( textLabel );
			}

			// display text label
			foreach( AIS_TextLabel textLabel in m_IndexList ) {
				m_Viewer.GetAISContext().Display( textLabel, false );
				m_Viewer.GetAISContext().Deactivate( textLabel );
			}
		}

		void ShowOverCut()
		{
			// clear the previous tool vec
			foreach( AIS_Line overCutAIS in m_OverCutAISList ) {
				m_Viewer.GetAISContext().Remove( overCutAIS, false );
			}
			m_OverCutAISList.Clear();

			// build over cut
			foreach( CAMData camData in m_DataManager.GetCAMDataList() ) {
				if( camData.OverCutLength > 0 ) {
					for( int i = 0; i < camData.OverCutCAMPointList.Count - 1; i++ ) {
						AIS_Line OverCutAISLine = GetLineAIS( camData.OverCutCAMPointList[ i ].CADPoint.Point, camData.OverCutCAMPointList[ i + 1 ].CADPoint.Point, Quantity_NameOfColor.Quantity_NOC_DEEPPINK );
						m_OverCutAISList.Add( OverCutAISLine );
					}
				}
			}

			// display the tool vec
			foreach( AIS_Line toolVecAIS in m_OverCutAISList ) {
				m_Viewer.GetAISContext().Display( toolVecAIS, false );
				m_Viewer.GetAISContext().Deactivate( toolVecAIS );
			}
		}

		void ShowTraversalPath()
		{
			// Remove previous lines
			foreach( AIS_Line traverseAIS in m_TraverseAISList ) {
				m_Viewer.GetAISContext().Remove( traverseAIS, false );
			}
			foreach( AIS_Shape frogLeapAIS in m_FrogLeapAISList ) {
				m_Viewer.GetAISContext().Remove( frogLeapAIS, false );
			}
			m_TraverseAISList.Clear();
			m_FrogLeapAISList.Clear();

			// no need to show
			if( m_ShowTraversePath == false ) {
				return;
			}
			List<CAMData> camDataList = m_DataManager.GetCAMDataList();
			if( camDataList == null || camDataList.Count <= 1 ) {
				return;
			}
			for( int i = 1; i < camDataList.Count; i++ ) {
				CAMData previousCamData = camDataList[ i - 1 ];
				CAMData currentCamData = camDataList[ i ];

				// p1: end of previous path
				// p2: lift up point of previous path
				// p3: frog leap middle point (if frog leap)
				// p4: cut down point of current path
				// p5: start of current path
				CAMPoint p1 = previousCamData.GetProcessEndPoint();
				CAMPoint p2 = TraverseHelper.GetCutDownOrLiftUpPoint( previousCamData.GetProcessEndPoint(), currentCamData.TraverseData.LiftUpDistance );
				CAMPoint p4 = TraverseHelper.GetCutDownOrLiftUpPoint( currentCamData.GetProcessStartPoint(), currentCamData.TraverseData.CutDownDistance );
				CAMPoint p5 = currentCamData.GetProcessStartPoint();

				// lift up
				if( currentCamData.TraverseData.LiftUpDistance > 0 ) {
					AIS_Line line1 = GetLineAIS( p1.CADPoint.Point, p2.CADPoint.Point, Quantity_NameOfColor.Quantity_NOC_RED, 1, 1, true );
					m_TraverseAISList.Add( line1 );
				}

				// frog leap
				if( currentCamData.TraverseData.EnableFrogLeap && currentCamData.TraverseData.FrogLeapDistance > 0 ) {
					CAMPoint p3 = TraverseHelper.GetFrogLeapMiddlePoint( p2, p4, currentCamData.TraverseData.FrogLeapDistance );
					GC_MakeArcOfCircle makeCircle = new GC_MakeArcOfCircle( p2.CADPoint.Point, p3.CADPoint.Point, p4.CADPoint.Point );
					if( makeCircle.IsDone() ) {
						Geom_TrimmedCurve arcCurve = makeCircle.Value();
						BRepBuilderAPI_MakeEdge makeEdge = new BRepBuilderAPI_MakeEdge( arcCurve );
						AIS_Shape arcAIS = new AIS_Shape( makeEdge.Shape() );
						arcAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
						arcAIS.SetWidth( 1 );
						arcAIS.SetTransparency( 1 );
						Prs3d_LineAspect prs3D_LineAspect = new Prs3d_LineAspect( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ), Aspect_TypeOfLine.Aspect_TOL_DASH, 1 );
						arcAIS.Attributes().SetWireAspect( prs3D_LineAspect );
						m_FrogLeapAISList.Add( arcAIS );
					}
					else {
						// fallback to normal traverse line
						AIS_Line line2 = GetLineAIS( p2.CADPoint.Point, p4.CADPoint.Point, Quantity_NameOfColor.Quantity_NOC_RED, 1, 1, true );
						m_TraverseAISList.Add( line2 );
					}
				}

				// normal traverse
				else {
					AIS_Line line2 = GetLineAIS( p2.CADPoint.Point, p4.CADPoint.Point, Quantity_NameOfColor.Quantity_NOC_RED, 1, 1, true );
					m_TraverseAISList.Add( line2 );
				}

				// cut down
				if( currentCamData.TraverseData.CutDownDistance > 0 ) {
					AIS_Line line3 = GetLineAIS( p4.CADPoint.Point, p5.CADPoint.Point, Quantity_NameOfColor.Quantity_NOC_RED, 1, 1, true );
					m_TraverseAISList.Add( line3 );
				}
			}

			// entry
			CAMPoint firstPathStartPoint = camDataList.First().GetProcessStartPoint();
			CAMPoint entryPoint = TraverseHelper.GetCutDownOrLiftUpPoint( firstPathStartPoint.Clone(), m_DataManager.EntryAndExitData.EntryDistance );
			if( m_DataManager.EntryAndExitData.EntryDistance > 0 ) {
				AIS_Line entryLine = GetLineAIS( entryPoint.CADPoint.Point, firstPathStartPoint.CADPoint.Point, Quantity_NameOfColor.Quantity_NOC_RED, 1, 1, true );
				m_TraverseAISList.Insert( 0, entryLine );
			}

			// exit
			CAMPoint lastPathEndPoint = camDataList.Last().GetProcessEndPoint();
			CAMPoint exitPoint = TraverseHelper.GetCutDownOrLiftUpPoint( lastPathEndPoint.Clone(), m_DataManager.EntryAndExitData.ExitDistance );
			if( m_DataManager.EntryAndExitData.ExitDistance > 0 ) {
				AIS_Line exitLine = GetLineAIS( lastPathEndPoint.CADPoint.Point, exitPoint.CADPoint.Point, Quantity_NameOfColor.Quantity_NOC_RED, 1, 1, true );
				m_TraverseAISList.Add( exitLine );
			}

			// Display all lines
			foreach( AIS_Line rapidTraverseAIS in m_TraverseAISList ) {
				m_Viewer.GetAISContext().Display( rapidTraverseAIS, false );
				m_Viewer.GetAISContext().Deactivate( rapidTraverseAIS );
			}
			foreach( AIS_Shape frogLeapAIS in m_FrogLeapAISList ) {
				m_Viewer.GetAISContext().Display( frogLeapAIS, false );
				m_Viewer.GetAISContext().Deactivate( frogLeapAIS );
			}
		}

		void HideCAMData()
		{
			// hide tool vec
			foreach( AIS_Line toolVecAIS in m_ToolVecAISList ) {
				m_Viewer.GetAISContext().Remove( toolVecAIS, false );
			}

			// hide orientation
			foreach( AIS_Shape orientationAIS in m_OrientationAISList ) {
				m_Viewer.GetAISContext().Remove( orientationAIS, false );
			}

			// hide index
			foreach( AIS_TextLabel textLabel in m_IndexList ) {
				m_Viewer.GetAISContext().Remove( textLabel, false );
			}

			// hide over cut
			foreach( AIS_Line overCutAIS in m_OverCutAISList ) {
				m_Viewer.GetAISContext().Remove( overCutAIS, false );
			}

			// hide lead 
			foreach( AIS_Line leadAIS in m_LeadAISList ) {
				m_Viewer.GetAISContext().Remove( leadAIS, false );
			}

			// hide traverse path
			foreach( AIS_Line traverseAIS in m_TraverseAISList ) {
				m_Viewer.GetAISContext().Remove( traverseAIS, false );
			}
			foreach( AIS_Shape frogLeapAIS in m_FrogLeapAISList ) {
				m_Viewer.GetAISContext().Remove( frogLeapAIS, false );
			}

			// hide lead orientation
			foreach( AIS_Shape orientationAIS in m_LeadOrientationAISList ) {
				m_Viewer.GetAISContext().Remove( orientationAIS, false );
			}
			m_Viewer.UpdateView();
		}

		void OnSetLeadPropertyChanged( bool isConfirm, bool isHasLead )
		{
			ShowCAMData();
			if( isConfirm ) {
				PathPropertyChanged?.Invoke( ( (PathData)m_DataManager.ShapeDataMap[ GetSelectedPathID() ] ).CAMData.IsClosed, isHasLead );
			}
		}

		#endregion

		// methods
		bool IsModifiedToolVecIndex( int index, CAMData camData )
		{
			// map CAD and CAM point index
			int nLength = camData.CADPointList.Count;
			int modifiedIndex = camData.IsReverse
				? ( nLength - ( camData.IsClosed ? 0 : 1 ) - index + camData.StartPoint ) % nLength
				: ( index + camData.StartPoint ) % nLength;

			// need highlight if the index is modified index
			return camData.GetToolVecModifyIndex().Contains( modifiedIndex );
		}

		AIS_Line GetVecAIS( gp_Pnt point, gp_Dir dir, EvecType vecType )
		{
			gp_Pnt endPoint = new gp_Pnt( point.XYZ() + dir.XYZ() * 10 );
			AIS_Line lineAIS = new AIS_Line( new Geom_CartesianPoint( point ), new Geom_CartesianPoint( endPoint ) );
			switch( vecType ) {
				case EvecType.ToolVec:
					lineAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
					break;
				case EvecType.TangentVec:
					lineAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
					break;
				case EvecType.NormalVec:
					lineAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ) );
					break;
			}
			lineAIS.SetWidth( 1 );
			return lineAIS;
		}

		AIS_Line GetLineAIS( gp_Pnt startPnt, gp_Pnt endPnt, Quantity_NameOfColor color, double lineWidth = 1, double dTransparancy = 1, bool isDashLine = false )
		{
			AIS_Line lineAIS = new AIS_Line( new Geom_CartesianPoint( startPnt ), new Geom_CartesianPoint( endPnt ) );
			lineAIS.SetColor( new Quantity_Color( color ) );
			lineAIS.SetWidth( lineWidth );
			lineAIS.SetTransparency( dTransparancy );
			if( isDashLine ) {
				Prs3d_LineAspect prs3D_LineAspect = new Prs3d_LineAspect( new Quantity_Color( color ), Aspect_TypeOfLine.Aspect_TOL_DASH, lineWidth );
				lineAIS.Attributes().SetLineAspect( prs3D_LineAspect );
			}
			return lineAIS;
		}

		AIS_Shape GetOrientationAIS( gp_Pnt point, gp_Dir dir )
		{
			// draw a cone to indicate the orientation
			gp_Ax2 coneAx2 = new gp_Ax2( point, dir );
			BRepPrimAPI_MakeCone coneMaker = new BRepPrimAPI_MakeCone( coneAx2, 0.5, 0, 2 );
			AIS_Shape coneAIS = new AIS_Shape( coneMaker.Shape() );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			coneAIS.SetMaterial( aspect );
			coneAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
			coneAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			coneAIS.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );
			return coneAIS;
		}

		string GetSelectedPathID()
		{
			TreeNode selectedNode = m_TreeView.SelectedNode;
			if( selectedNode == null || string.IsNullOrEmpty( selectedNode.Text ) ) {
				return string.Empty;
			}
			return selectedNode.Text;
		}

		bool IsSameAction( EditActionType newActionType )
		{
			if( m_CurrentAction != null && m_CurrentAction.ActionType == newActionType ) {
				return true;
			}
			return false;
		}

		bool ValidateSelectAndEndExAction( out string szPathID )
		{
			szPathID = GetSelectedPathID();
			if( string.IsNullOrEmpty( szPathID ) || !m_DataManager.ShapeDataMap.ContainsKey( szPathID ) ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先選擇路徑", MyApp.NoticeType.Hint );

				// default action do not need to stop
				EndActionIfNotDefault();
				return false;
			}
			return true;
		}

		// edit actions
		protected override void OnEditActionStart( IEditorAction action )
		{
			base.OnEditActionStart( action );
			if( action.ActionType == EditActionType.OverCut ||
				action.ActionType == EditActionType.SetLead ||
				action.ActionType == EditActionType.SetTraverseParam ) {

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
			if( action.ActionType == EditActionType.OverCut ||
				action.ActionType == EditActionType.SetLead ||
				action.ActionType == EditActionType.SetTraverseParam
				) {

				// unlock main form
				m_TreeView.Enabled = true;
				RaiseWithDlgActionStatusChange?.Invoke( EActionStatus.End );
			}
			RaiseCAMActionStatusChange?.Invoke( action.ActionType, EActionStatus.End );
			base.OnEditActionEnd( action );
		}

		bool PathToolVec( CAMData camData, out List<(gp_Pnt point, gp_Dir toolVec, bool isModifiedToolVec)> points )
		{
			points = new List<(gp_Pnt point, gp_Dir toolVec, bool isModifiedToolVec)>();
			if( camData.CADSegmentList == null || camData.CADSegmentList.Count == 0 ) {
				return false;
			}

			// 沒有操縱桿
			if( camData.ToolVecModifyMap_New.Count == 0 ) {
				for( int i = 0; i < camData.CADSegmentList.Count; i++ ) {
					points.AddRange( DrawNormalToolVec( camData.CADSegmentList[ i ], camData.IsToolVecReverse ) );
				}
				return true;
			}

			// 有一根
			if( camData.ToolVecModifyMap_New.Count == 1 ) {
				(int, int) targetPointSegmentIndex = camData.ToolVecModifyMap_New.Keys.First();
				CADPoint targetPoint = camData.CADSegmentList[ targetPointSegmentIndex.Item1 ].PointList[ targetPointSegmentIndex.Item2 ];
				CAMPoint targetCAMPoint = BuildCAMSegmentHelper.GetCAMPoint( targetPoint, camData.IsToolVecReverse );
				gp_Vec newVec = GetVecFromAB( targetCAMPoint,
					camData.ToolVecModifyMap_New.Values.First().Item1 * Math.PI / 180,
					camData.ToolVecModifyMap_New.Values.First().Item2 * Math.PI / 180 );
				gp_Dir assignDir = new gp_Dir( newVec );
				points = DrawAssignToolVec( camData.CADSegmentList, assignDir );
				points.Add( (targetPoint.Point, assignDir, true) );
				return true;
			}

			// 有多根
			List<((int, int), (int, int))> interpolateIntervalList = GetInterpolateIntervalList( camData );

			// modify the tool vector
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {

				// get start and end index
				(int, int) nStartIndex = interpolateIntervalList[ i ].Item1;
				(int, int) nEndIndex = interpolateIntervalList[ i ].Item2;
				points = InterpolateToolVec( camData, nStartIndex, nEndIndex );
			}
			return true;

		}

		List<(gp_Pnt point, gp_Dir toolVec, bool isModiyToolVec)> InterpolateToolVec( CAMData camdata, (int, int) nStartIndex, (int, int) nEndIndex )
		{
			List<(gp_Pnt point, double dDistance, bool isModifyToolVec)> toolVecLocation = new List<(gp_Pnt point, double dDistance, bool isModifyToolVec)>();
			List<(gp_Pnt point, gp_Dir toolVec, bool isModiyToolVec)> points = new List<(gp_Pnt point, gp_Dir toolVecbool, bool isModiyToolVec)>();
			// 計算長度
			double dTotalLength = 0;

			// 同一個segment內
			if( nStartIndex.Item1 == nEndIndex.Item1 ) {

				if( camdata.CADSegmentList[ nStartIndex.Item1 ] is LineCADSegment lineCADSegment ) {
					toolVecLocation.AddRange( GetLine( lineCADSegment, nStartIndex.Item2, true, nEndIndex.Item2, true, ref dTotalLength ) );
				}
				if( camdata.CADSegmentList[ nStartIndex.Item1 ] is ArcCADSegment arcCADSegment ) {
					toolVecLocation.AddRange( GetArc( arcCADSegment, nStartIndex.Item2, true, nEndIndex.Item2, true, ref dTotalLength ) );
				}
			}
			else {
				// 前一段到後幾段
				if( nStartIndex.Item1 < nEndIndex.Item1 ) {

					if( camdata.CADSegmentList[ nStartIndex.Item1 ] is LineCADSegment lineCADSegment ) {
						toolVecLocation.AddRange( GetLine( lineCADSegment, nStartIndex.Item2, true, lineCADSegment.PointList.Count-1, false, ref dTotalLength ) );
					}
					if( camdata.CADSegmentList[ nStartIndex.Item1 ] is ArcCADSegment arcCADSegment ) {
						toolVecLocation.AddRange( GetArc( arcCADSegment, nStartIndex.Item2, true, arcCADSegment.PointList.Count - 1, false, ref dTotalLength ) );
					}

					
					for( int i = nStartIndex.Item1 + 1; i < nEndIndex.Item1; i++ ) {

						if( camdata.CADSegmentList[i] is LineCADSegment lineCADSegment_1 ) {
							toolVecLocation.AddRange( GetLine( lineCADSegment_1, 0, false, lineCADSegment_1.PointList.Count - 1, false, ref dTotalLength ) );
						}
						if( camdata.CADSegmentList[ i ] is ArcCADSegment arcCADSegment_1 ) {
							toolVecLocation.AddRange( GetArc( arcCADSegment_1, 0, false, arcCADSegment_1.PointList.Count - 1, false, ref dTotalLength ) );
						}
					}

					// last Segment length
					if( camdata.CADSegmentList[ nEndIndex.Item1 ] is LineCADSegment lineCADSegment_2 ) {
						toolVecLocation.AddRange( GetLine( lineCADSegment_2, 0, false, nEndIndex.Item2, true, ref dTotalLength ) );
					}
					if( camdata.CADSegmentList[ nEndIndex.Item1 ] is ArcCADSegment arcCADSegment_2 ) {
						toolVecLocation.AddRange( GetArc( arcCADSegment_2, 0, false, nEndIndex.Item2, true, ref dTotalLength ) );
					}
				}

				// 最後一段到前面
				else {

					if( camdata.CADSegmentList[ nStartIndex.Item1 ] is LineCADSegment lineCADSegment ) {
						toolVecLocation.AddRange( GetLine( lineCADSegment, nStartIndex.Item2, true, lineCADSegment.PointList.Count-1, false, ref dTotalLength ) );
					}
					if( camdata.CADSegmentList[ nStartIndex.Item1 ] is ArcCADSegment arcCADSegment ) {
						toolVecLocation.AddRange( GetArc( arcCADSegment, nStartIndex.Item2, true, arcCADSegment.PointList.Count - 1, false, ref dTotalLength ) );
					}

					for( int i = 0; i < nEndIndex.Item1; i++ ) {
						if( camdata.CADSegmentList[ i ] is LineCADSegment lineCADSegment_1 ) {
							toolVecLocation.AddRange( GetLine( lineCADSegment_1, 0, false, lineCADSegment_1.PointList.Count - 1, false, ref dTotalLength ) );
						}
						if ( camdata.CADSegmentList[ i ] is ArcCADSegment arcCADSegment_1 ) {
							toolVecLocation.AddRange( GetArc( arcCADSegment_1, 0, false, arcCADSegment_1.PointList.Count - 1, false, ref dTotalLength ) );
						}
					}

					// first Segment length
					if( camdata.CADSegmentList[ nEndIndex.Item1] is LineCADSegment lineCADSegment_2 ) {
						toolVecLocation.AddRange( GetLine( lineCADSegment_2, 0, false, nEndIndex.Item2, true, ref dTotalLength ) );
					}
					if( camdata.CADSegmentList[ nEndIndex.Item1 ] is ArcCADSegment arcCADSegment_2 ) {
						toolVecLocation.AddRange( GetArc( arcCADSegment_2, 0, false, nEndIndex.Item2, true, ref dTotalLength ) );
					}

				}
			}

			// get the start and end tool vector
			CADPoint startCADPnt = camdata.CADSegmentList[ nStartIndex.Item1 ].PointList[ nStartIndex.Item2 ];
			CAMPoint startCAMPnt = BuildCAMSegmentHelper.GetCAMPoint( startCADPnt, camdata.IsToolVecReverse );
			gp_Vec startVec = GetVecFromAB( startCAMPnt,
				camdata.ToolVecModifyMap_New[ nStartIndex ].Item1 * Math.PI / 180,
				camdata.ToolVecModifyMap_New[ nStartIndex ].Item2 * Math.PI / 180
				);
			CADPoint endCADPnt = camdata.CADSegmentList[ nEndIndex.Item1 ].PointList[ nEndIndex.Item2 ];
			CAMPoint endCAMPnt = BuildCAMSegmentHelper.GetCAMPoint( endCADPnt, camdata.IsToolVecReverse );
			gp_Vec endVec = GetVecFromAB( endCAMPnt,
				camdata.ToolVecModifyMap_New[ nEndIndex ].Item1 * Math.PI / 180,
				camdata.ToolVecModifyMap_New[ nEndIndex ].Item2 * Math.PI / 180
				);

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( startVec, endVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );
			double accumulatedDistance = 0;

			for( int i = 0; i < toolVecLocation.Count; i++ ) {

				double t = accumulatedDistance / dTotalLength;
				accumulatedDistance += toolVecLocation[ i ].dDistance;

				gp_Quaternion q = new gp_Quaternion();
				slerp.Interpolate( t, ref q );
				gp_Trsf trsf = new gp_Trsf();
				trsf.SetRotation( q );
				gp_Dir toolVecDir = new gp_Dir( startVec.Transformed( trsf ) );
				points.Add( (toolVecLocation[ i ].point, toolVecDir, toolVecLocation[ i ].isModifyToolVec) );

			}
			return points;
		}

		List<(gp_Pnt point, double dDistance, bool isModiyToolVec)> GetArc( ArcCADSegment arcCADSegment, int nStartIndex, bool IsSModify, int nEndIndex, bool IsEModify, ref double dTotalLength )
		{
			List<(gp_Pnt point, double dDistance, bool isModiyToolVec)> pointToolVecList = new List<(gp_Pnt point, double dDistance, bool isModiyToolVec)>();
			pointToolVecList.Add( (arcCADSegment.PointList[ nStartIndex ].Point, 0, IsSModify) );
			int midIndex = arcCADSegment.PointList.Count / 2;

			// 圓弧在中間
			if( nStartIndex < midIndex && nEndIndex > midIndex ) {
				double dLenghtToMid = ( midIndex - nStartIndex ) * arcCADSegment.PointSpace;
				dTotalLength += dLenghtToMid;
				pointToolVecList.Add( (arcCADSegment.MidPoint.Point, dLenghtToMid, false) );

				double dLengthToArcEnd = ( nEndIndex - midIndex ) * arcCADSegment.PointSpace;
				dTotalLength += dLengthToArcEnd;
				pointToolVecList.Add( (arcCADSegment.PointList[ nEndIndex ].Point, dLengthToArcEnd, IsEModify) );
			}
			else {
				double dLengthFromStartToArcEnd = ( nEndIndex - nStartIndex ) * arcCADSegment.PointSpace;
				dTotalLength += dLengthFromStartToArcEnd;

				// 這段結尾
				pointToolVecList.Add( (arcCADSegment.PointList[ nEndIndex ].Point, dLengthFromStartToArcEnd, IsEModify) );
			}
			return pointToolVecList;
		}

		List<(gp_Pnt point, double dDistance, bool isModiyToolVec)> GetLine( LineCADSegment lineCADSegment, int nStartIndex, bool IsSModify, int nEndIndex, bool IsEModify, ref double dTotalLength )
		{

			List<(gp_Pnt point, double dDistance, bool isModiyToolVec)> pointToolVecList = new List<(gp_Pnt point, double dDistance, bool isModiyToolVec)>();
			pointToolVecList.Add( (lineCADSegment.PointList[ nStartIndex ].Point, 0, IsSModify) );
			double dDistanceFromStartToLineEnd = ( nEndIndex - nStartIndex ) * lineCADSegment.PointSpace;
			dTotalLength += dDistanceFromStartToLineEnd;

			// 這段結尾
			pointToolVecList.Add( (lineCADSegment.PointList[nEndIndex].Point, dDistanceFromStartToLineEnd, IsEModify) );

			return pointToolVecList;
		}

		List<((int, int), (int, int))> GetInterpolateIntervalList( CAMData camData )
		{
			// sort the modify data by index
			List<(int, int)> indexInOrder = camData.ToolVecModifyMap_New.Keys.ToList();
			indexInOrder.Sort();

			List<((int, int), (int, int))> intervalList = new List<((int, int), (int, int))>();
			if( camData.IsClosed ) {

				// for closed path, the index is wrapped
				for( int i = 0; i < indexInOrder.Count; i++ ) {
					int nextIndex = ( i + 1 ) % indexInOrder.Count;
					intervalList.Add( (indexInOrder[ i ], indexInOrder[ nextIndex ]) );
				}
			}
			else {
				for( int i = 0; i < indexInOrder.Count - 1; i++ ) {
					intervalList.Add( (indexInOrder[ i ], indexInOrder[ i + 1 ]) );
				}
			}
			return intervalList;
		}

		List<(gp_Pnt point, gp_Dir toolVec, bool isModifiedToolVec)> DrawAssignToolVec( List<ICADSegmentElement> cadSegmentList, gp_Dir assignDir )
		{
			List<(gp_Pnt point, gp_Dir toolVec, bool isModifiedToolVec)> points = new List<(gp_Pnt point, gp_Dir toolVec, bool isModifiedToolVec)>();
			for( int i = 0; i < cadSegmentList.Count; i++ ) {
				switch( cadSegmentList[ i ] ) {
					case ArcCADSegment arcSegment:
						points.Add( (arcSegment.StartPoint.Point, assignDir, false) );
						points.Add( (arcSegment.EndPoint.Point, assignDir, false) );
						points.Add( (arcSegment.MidPoint.Point, assignDir, false) );
						break;
					case LineCADSegment lineSegment:
						points.Add( (lineSegment.StartPoint.Point, assignDir, false) );
						points.Add( (lineSegment.EndPoint.Point, assignDir, false) );
						break;
					default:
						return points;
				}
			}
			return points;
		}

		gp_Vec GetVecFromAB( CAMPoint camPoint, double dRA_rad, double dRB_rad )
		{
			// TDOD: RA == 0 || RB == 0
			if( dRA_rad == 0 && dRB_rad == 0 ) {
				return new gp_Vec( camPoint.ToolVec );
			}

			// get the x, y, z direction
			gp_Dir x = camPoint.CADPoint.TangentVec;
			gp_Dir z = camPoint.CADPoint.NormalVec_1st;
			gp_Dir y = z.Crossed( x );

			// X:Y:Z = tanA:tanB:1
			double X = 0;
			double Y = 0;
			double Z = 0;
			if( dRA_rad == 0 ) {
				X = 0;
				Z = 1;
			}
			else {
				X = dRA_rad < 0 ? -1 : 1;
				Z = X / Math.Tan( dRA_rad );
			}
			Y = Z * Math.Tan( dRB_rad );
			gp_Dir dir1 = new gp_Dir( x.XYZ() * X + y.XYZ() * Y + z.XYZ() * Z );
			return new gp_Vec( dir1.XYZ() );
		}


		List<(gp_Pnt point, gp_Dir toolVec, bool isModifiedToolVec)> DrawNormalToolVec( ICADSegmentElement cadSegment, bool isToolVecReverse )
		{

			List<(gp_Pnt point, gp_Dir toolVec, bool isModifiedToolVec)> points = new List<(gp_Pnt point, gp_Dir toolVec, bool isModifiedToolVec)>();

			switch( cadSegment ) {
				case LineCADSegment lineSegment:
					points.Add( (lineSegment.StartPoint.Point, lineSegment.StartPoint.NormalVec_1st, false) );
					points.Add( (lineSegment.EndPoint.Point, lineSegment.EndPoint.NormalVec_1st, false) );
					break;

				case ArcCADSegment arcSegment:
					points.Add( (arcSegment.StartPoint.Point, arcSegment.StartPoint.NormalVec_1st, false) );
					points.Add( (arcSegment.EndPoint.Point, arcSegment.EndPoint.NormalVec_1st, false) );
					points.Add( (arcSegment.MidPoint.Point, arcSegment.MidPoint.NormalVec_1st, false) );
					break;

				default:
					return points;
			}
			return points;
		}
	}
}
