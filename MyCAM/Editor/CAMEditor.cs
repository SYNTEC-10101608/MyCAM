using MyCAM.App;
using MyCAM.Data;
using MyCAM.Post;
using OCC.AIS;
using OCC.Aspect;
using OCC.BRepPrimAPI;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
		}

		// for viewer resource handle
		List<AIS_Line> m_ToolVecAISList = new List<AIS_Line>(); // need refresh, no need activate
		List<AIS_Shape> m_OrientationAISList = new List<AIS_Shape>(); // need refresh, no need activate
		List<AIS_TextLabel> m_IndexList = new List<AIS_TextLabel>(); // need refresh, no need activate
		List<AIS_Line> m_LeadAISList = new List<AIS_Line>(); // need refresh, no need activate
		List<AIS_Shape> m_LeadOrientationAISList = new List<AIS_Shape>(); // need refresh, no need activate
		List<AIS_Line> m_OverCutAISList = new List<AIS_Line>(); // need refresh, no need activate
		List<AIS_Line> m_TraverseAISList = new List<AIS_Line>(); // need refresh, no need activate

		enum EvecType
		{
			ToolVec,
			TangentVec,
			NormalVec,
		}

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
			// clear tree
			m_TreeView.Nodes.Clear();

			// clear viewer
			foreach( var pathID in m_DataManager.PathIDList ) {
				AIS_InteractiveObject obj = m_ViewManager.ViewObjectMap[ pathID ].AISHandle;
				m_Viewer.GetAISContext().Remove( obj, false );
			}
			HideCAMData();
			base.EditEnd();
		}

		// APIs
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

			// stop current action
			EndActionIfNotDefault();
			TraverseAction action = new TraverseAction( m_DataManager );
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
			NCWriter writer = new NCWriter( m_DataManager.GetCAMDataList(), m_DataManager.MachineData );
			bool bSuccess = writer.ConvertSuccess( out string szErrorMessage );
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
			ShowToolVec();
			ShowOrientation();
			ShowIndex();
			ShowOverCut();
			ShowLeadLine();
			ShowLeadOrientation();
			ShowTraversalPath();
			m_Viewer.UpdateView();
		}

		void ShowToolVec()
		{
			// clear the previous tool vec
			foreach( AIS_Line toolVecAIS in m_ToolVecAISList ) {
				m_Viewer.GetAISContext().Remove( toolVecAIS, false );
			}
			m_ToolVecAISList.Clear();

			// build tool vec
			foreach( CAMData camData in m_DataManager.GetCAMDataList() ) {
				for( int i = 0; i < camData.CAMPointList.Count; i++ ) {
					CAMPoint camPoint = camData.CAMPointList[ i ];
					AIS_Line toolVecAIS = GetVecAIS( camPoint.CADPoint.Point, camPoint.ToolVec, EvecType.ToolVec );
					if( IsModifiedToolVecIndex( i, camData ) ) {
						toolVecAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
						toolVecAIS.SetWidth( 4 );
					}
					m_ToolVecAISList.Add( toolVecAIS );
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
						AIS_Line LeadAISLine = GetAISLine( currentCAMPoint.CADPoint.Point, nextCAMPoint.CADPoint.Point, Quantity_NameOfColor.Quantity_NOC_GREENYELLOW );
						m_LeadAISList.Add( LeadAISLine );
					}
				}

				// draw lead out
				if( camData.LeadLineParam.LeadOut.Type != LeadLineType.None ) {
					for( int i = 0; i < camData.LeadOutCAMPointList.Count - 1; i++ ) {
						CAMPoint currentCAMPoint = camData.LeadOutCAMPointList[ i ];
						CAMPoint nextCAMPoint = camData.LeadOutCAMPointList[ i + 1 ];
						AIS_Line LeadAISLine = GetAISLine( currentCAMPoint.CADPoint.Point, nextCAMPoint.CADPoint.Point, Quantity_NameOfColor.Quantity_NOC_GREENYELLOW );
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
						AIS_Line OverCutAISLine = GetAISLine( camData.OverCutCAMPointList[ i ].CADPoint.Point, camData.OverCutCAMPointList[ i + 1 ].CADPoint.Point, Quantity_NameOfColor.Quantity_NOC_DEEPPINK );
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
			m_TraverseAISList.Clear();
			List<CAMData> camDataList = m_DataManager.GetCAMDataList();
			if( camDataList == null || camDataList.Count == 0 ) {
				return;
			}
			for( int i = 0; i < camDataList.Count; i++ ) {
				var camData = camDataList[ i ];
				if( camData == null ) {
					continue;
				}

				// tool down point list + follow safe point list
				List<CAMPoint> cutDownPointList = new List<CAMPoint>();

				// the first path, only need cut down traverse path
				if( i == 0 ) {
					cutDownPointList.AddRange( GetCutDownList( camData ) );
				}
				else {

					// not the first path, need to add from previous path lift up point
					var prevCamData = camDataList[ i - 1 ];
					if( prevCamData?.LiftUpCAMPoint != null ) {
						cutDownPointList.Add( prevCamData.LiftUpCAMPoint );
					}
					cutDownPointList.AddRange( GetCutDownList( camData ) );
				}

				// tool up point list
				List<CAMPoint> liftUpPointList = new List<CAMPoint>();
				if( camData.GetProcessEndPoint() != null ) {
					liftUpPointList.Add( camData.GetProcessEndPoint() );
				}
				if( camData.LiftUpCAMPoint != null ) {
					liftUpPointList.Add( camData.LiftUpCAMPoint );
				}

				// tool down + follow safe + tool up AIS lines
				m_TraverseAISList.AddRange( GetAISLineList( cutDownPointList, Quantity_NameOfColor.Quantity_NOC_RED, 1, 0.5, true ) );
				m_TraverseAISList.AddRange( GetAISLineList( liftUpPointList, Quantity_NameOfColor.Quantity_NOC_RED, 1, 0.5, true ) );
			}

			// Display all lines
			foreach( AIS_Line rapidTraverseAIS in m_TraverseAISList ) {
				m_Viewer.GetAISContext().Display( rapidTraverseAIS, false );
				m_Viewer.GetAISContext().Deactivate( rapidTraverseAIS );
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

			// hide lead orientation
			// hide orientation
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
		List<CAMPoint> GetCutDownList( CAMData camData )
		{
			List<CAMPoint> cutDownPointList = new List<CAMPoint>();
			if( camData.CutDownCAMPoint != null ) {
				cutDownPointList.Add( camData.CutDownCAMPoint );
			}
			if( camData.FollowSafeCAMPoint != null ) {
				cutDownPointList.Add( camData.FollowSafeCAMPoint );
			}
			if( camData.GetProcessStartPoint() != null ) {
				cutDownPointList.Add( camData.GetProcessStartPoint() );
			}
			return cutDownPointList;
		}

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

		AIS_Line GetAISLine( gp_Pnt leadStartPnt, gp_Pnt leadEndPnt, Quantity_NameOfColor color, double lineWidth = 1, double dTransparancy = 1, bool isDashLine = false )
		{
			AIS_Line lineAIS = new AIS_Line( new Geom_CartesianPoint( leadStartPnt ), new Geom_CartesianPoint( leadEndPnt ) );
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

		List<AIS_Line> GetAISLineList( List<CAMPoint> points, Quantity_NameOfColor color, double lineWidth = 1, double dTransparancy = 1, bool isDashLine = false )
		{
			List<AIS_Line> newLines = new List<AIS_Line>();
			if( points == null || points.Count < 2 ) {
				return null;
			}
			for( int i = 0; i < points.Count - 1; i++ ) {
				if( points[ i ].CADPoint.Point.IsEqual( points[ i + 1 ].CADPoint.Point, 0.001 ) ) {
					continue;
				}
				var line = GetAISLine( points[ i ].CADPoint.Point, points[ i + 1 ].CADPoint.Point, color, lineWidth, dTransparancy, isDashLine );
				newLines.Add( line );
			}
			return newLines;
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
	}
}
