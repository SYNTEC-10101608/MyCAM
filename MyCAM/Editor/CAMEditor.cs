using MyCAM.App;
using MyCAM.CacheInfo;
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
using System;
using System.Collections.Generic;
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
			IsReverseEditable = true;
			IsOverCutEditable = true;
			IsLeadLineEditable = true;
			IsToolVecEditable = true;
			IsToolVecReverseEditable = true;
			IsTraverseEditable = true;
			IsMoveProcessEditable = true;
			IsAutoOrderEditable = true;
		}

		public bool IsStartPointEditable;
		public bool IsReverseEditable;
		public bool IsOverCutEditable;
		public bool IsLeadLineEditable;
		public bool IsToolVecEditable;
		public bool IsToolVecReverseEditable;
		public bool IsTraverseEditable;
		public bool IsMoveProcessEditable;
		public bool IsAutoOrderEditable;
	}

	internal class CAMEditor : EditorBase
	{
		// to notice main form
		public Action<EditableInfo> PathPropertyChanged;
		public Action<EditActionType, EActionStatus> RaiseCAMActionStatusChange;

		// action with dialog need to disable main form
		// because other action enterance might close this action, but without closing this form
		public Action<EActionStatus> RaiseWithDlgActionStatusChange;

		public CAMEditor( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
			m_DataManager.PathAdded += OnPathAdded;

			// default action is select object action
			m_DefaultAction = new SelectPathAction_( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			( m_DefaultAction as SelectPathAction_ ).SelectionChange += OnPathSelectionChange;
			( m_DefaultAction as SelectPathAction_ ).RemovePath += RemovePath;
			( m_DefaultAction as SelectPathAction_ ).PathOrderMove += MoveProcess;
		}

		// for viewer resource handle
		Dictionary<string, List<AIS_Line>> m_ToolVecAISDict = new Dictionary<string, List<AIS_Line>>();
		Dictionary<string, AIS_Shape> m_OrientationAISList = new Dictionary<string, AIS_Shape>(); // need refresh, no need activate
		Dictionary<string, List<AIS_Line>> m_LeadAISDict = new Dictionary<string, List<AIS_Line>>(); // need refresh, no need activate
		Dictionary<string, List<AIS_Shape>> m_LeadOrientationAISDict = new Dictionary<string, List<AIS_Shape>>(); // need refresh, no need activate
		Dictionary<string, List<AIS_Line>> m_OverCutAISDict = new Dictionary<string, List<AIS_Line>>(); // need refresh, no need activate

		// this three need to consider standard pattern, so they are only lists now
		List<AIS_TextLabel> m_IndexList = new List<AIS_TextLabel>(); // need refresh, no need activate
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
			HideAllCAMData();
		}

		// view API

		public void SetShowToolVec( bool isShowToolVec )
		{
			m_ShowToolVec = isShowToolVec;
			ShowAllCAMData();
		}

		public void SetShowOrientation( bool isShowOrientation )
		{
			m_ShowOrientation = isShowOrientation;
			ShowAllCAMData();
		}

		public void SetShowOrder( bool isShowOrder )
		{
			m_ShowOrder = isShowOrder;
			ShowAllCAMData();
		}

		public void SetShowTraversePath( bool isShowTraversePath )
		{
			m_ShowTraversePath = isShowTraversePath;
			ShowAllCAMData();
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
			ValidateBeforeOneShotEdit( out List<string> szPathIDList, true );
			foreach( string szPathID in szPathIDList ) {

				// remove from data manager
				m_DataManager.RemovePath( szPathID );

				// remove from viewer
				if( m_ViewManager.ViewObjectMap.ContainsKey( szPathID ) ) {
					m_Viewer.GetAISContext().Remove( m_ViewManager.ViewObjectMap[ szPathID ].AISHandle, false );
					m_ViewManager.ViewObjectMap.Remove( szPathID );
				}

				// remove from tree view
				if( m_ViewManager.TreeNodeMap.ContainsKey( szPathID ) ) {
					TreeNode node = m_ViewManager.TreeNodeMap[ szPathID ];
					m_TreeView.Nodes.Remove( node );
					m_ViewManager.TreeNodeMap.Remove( szPathID );
				}
			}

			// clear selection after remove path
			m_DefaultAction.ClearSelection();
			RemoveCAMData( szPathIDList );
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
			ValidateBeforeOneShotEdit( out List<string> szPathIDList, true );
			foreach( string szPathID in szPathIDList ) {
				if( !GetCraftDataByID( m_DataManager, szPathID, out CraftData craftData ) ) {
					continue;
				}

				// toggle reverse state
				craftData.IsReverse = !craftData.IsReverse;
				// To-do：update CacheInfo in CAMPoint
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
			// one shot edit, multi edit supported
			ValidateBeforeOneShotEdit( out List<string> szPathIDList, true );
			foreach( string szPathID in szPathIDList ) {
				if( !GetCraftDataByID( m_DataManager, szPathID, out CraftData craftData ) ) {
					continue;
				}

				// toggle reverse state
				craftData.IsToolVecReverse = !craftData.IsToolVecReverse;
			}
			ShowCAMData( szPathIDList );
		}

		public void SetTraverseParam()
		{
			// action edit, multi edit supported
			if( IsSameAction( EditActionType.SetTraverseParam ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			TraverseAction action = new TraverseAction( m_DataManager, szPathIDList );
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

		#endregion

		// sort API
		public void MoveProcess( bool bUp )
		{
			// one shot edit, no multi edit supported
			ValidateBeforeOneShotEdit( out List<string> szPathIDList, false );
			string szPathID = szPathIDList[ 0 ];
			int nIndex = m_DataManager.PathIDList.IndexOf( szPathID );

			// check boundary
			if( nIndex < 0 || nIndex > m_DataManager.PathIDList.Count - 1
				|| bUp && nIndex == 0
				|| !bUp && nIndex == m_DataManager.PathIDList.Count - 1 ) {
				return;
			}

			// move process
			m_DataManager.PathIDList.RemoveAt( nIndex );
			if( bUp ) {
				m_DataManager.PathIDList.Insert( nIndex - 1, szPathID );
			}
			else {
				m_DataManager.PathIDList.Insert( nIndex + 1, szPathID );
			}
			ShowAllCAMData();
		}

		public void AutoSortProcess()
		{
			// one shot edit, no multi edit supported
			ValidateBeforeOneShotEdit( out List<string> szPathIDList, false );
			string szStartPathID = szPathIDList[ 0 ];

			// get start point
			if( !GetCacheInfoByID( m_DataManager, szStartPathID, out ICacheInfo cacheInfo ) ) {
				return;
			}
			gp_Pnt currentPoint = cacheInfo.GetProcessStartPoint().CADPoint.Point;

			// init data manager
			List<string> pathIDList = new List<string>( m_DataManager.PathIDList );
			m_DataManager.PathIDList.Clear();
			m_DataManager.PathIDList.Add( szStartPathID );

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
					if( !GetCacheInfoByID( m_DataManager, pathIDList[ i ], out ICacheInfo nextCacheInfo ) ) {
						continue;
					}
					gp_Pnt nextStartPoint = nextCacheInfo.GetProcessStartPoint().CADPoint.Point;
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
			ShowAllCAMData();
		}

		// convert NC
		public void ConverNC()
		{
			// stop current action
			EndActionIfNotDefault();
			NCWriter writer = new NCWriter( m_DataManager );
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
					continue;
				}

				// add a new node to the tree view
				TreeNode node = new TreeNode( szID );
				m_ViewManager.PathNode.Nodes.Add( node );
				m_ViewManager.TreeNodeMap.Add( szID, node );

				// add a new shape to the viewer
				AIS_Shape aisShape = ViewHelper.CreatePathAIS( m_DataManager.ObjectMap[ szID ].Shape, 3.0 );
				m_ViewManager.ViewObjectMap.Add( szID, new ViewObject( aisShape ) );
				m_Viewer.GetAISContext().Display( aisShape, false ); // this will also activate
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
				editableInfo.IsMoveProcessEditable = false;
				editableInfo.IsAutoOrderEditable = false;
			}

			// closed path editable only: start point, overcut, lead line, change lead dir
			foreach( string szPathID in szPathIDList ) {
				if( !GetCacheInfoByID( m_DataManager, szPathID, out ICacheInfo cacheInfo ) ) {
					continue;
				}
				if( !( cacheInfo as ContourCacheInfo ).IsClosed ) {
					editableInfo.IsStartPointEditable = false;
					editableInfo.IsOverCutEditable = false;
					editableInfo.IsLeadLineEditable = false;
					break;
				}
			}
			PathPropertyChanged?.Invoke( editableInfo );
		}

		#region Show CAM

		void ShowAllCAMData()
		{
			// take all path IDs
			List<string> pathIDList = m_DataManager.PathIDList;
			ShowToolVec( pathIDList );
			ShowOrientation( pathIDList );
			ShowIndex();
			ShowOverCut( pathIDList );
			ShowLeadLine( pathIDList );
			ShowLeadOrientation( pathIDList );
			ShowTraversalPath();
			m_Viewer.UpdateView();
		}

		void ShowCAMData( List<string> pathIDList )
		{
			ShowToolVec( pathIDList );
			ShowOrientation( pathIDList );
			ShowIndex();
			ShowOverCut( pathIDList );
			ShowLeadLine( pathIDList );
			ShowLeadOrientation( pathIDList );
			ShowTraversalPath();
			m_Viewer.UpdateView();
		}

		void RemoveCAMData( List<string> pathIDList )
		{
			RomoveToolVec( pathIDList );
			RomoveLeadLine( pathIDList );
			RomoveOrientation( pathIDList );
			RomoveIndex();
			RomoveOverCut( pathIDList );
			RomoveLeadOrientation( pathIDList );
			ShowTraversalPath();
			m_Viewer.UpdateView();
		}

		void RomoveToolVec( List<string> pathIDList )
		{
			// clear the previous tool vec
			foreach( string szPathID in pathIDList ) {
				if( m_ToolVecAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> toolVecAISList = m_ToolVecAISDict[ szPathID ];
					foreach( AIS_Line toolVecAIS in toolVecAISList ) {
						m_Viewer.GetAISContext().Remove( toolVecAIS, false );
					}
					m_ToolVecAISDict[ szPathID ].Clear();
					m_ToolVecAISDict.Remove( szPathID );
				}
			}
		}

		void ShowToolVec( List<string> pathIDList )
		{
			RomoveToolVec( pathIDList );

			// no need to show
			if( m_ShowToolVec == false ) {
				return;
			}

			// build tool vec
			foreach( string szPathID in pathIDList ) {
				if( GetCacheInfoByID( m_DataManager, szPathID, out ICacheInfo cacheInfo ) == false || cacheInfo.PathType != PathType.Contour ) {
					continue;
				}
				ContourCacheInfo contourCacheInfo = (ContourCacheInfo)cacheInfo;
				List<AIS_Line> toolVecAISList = new List<AIS_Line>();
				m_ToolVecAISDict.Add( szPathID, toolVecAISList );

				for( int i = 0; i < contourCacheInfo.CAMPointList.Count; i++ ) {
					CAMPoint camPoint = contourCacheInfo.CAMPointList[ i ];
					AIS_Line toolVecAIS = GetVecAIS( camPoint.CADPoint.Point, camPoint.ToolVec, EvecType.ToolVec );
					if( IsModifiedToolVecIndex( i, contourCacheInfo, contourCacheInfo.CAMPointList.Select( point => camPoint.CADPoint ).ToList() ) ) {
						toolVecAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
						toolVecAIS.SetWidth( 4 );
					}
					toolVecAISList.Add( toolVecAIS );
				}
			}

			// display the tool vec
			foreach( string szPathID in pathIDList ) {
				if( m_ToolVecAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> toolVecAISList = m_ToolVecAISDict[ szPathID ];
					foreach( AIS_Line toolVecAIS in toolVecAISList ) {
						m_Viewer.GetAISContext().Display( toolVecAIS, false );
						m_Viewer.GetAISContext().Deactivate( toolVecAIS );
					}
				}
			}
		}

		void RomoveLeadLine( List<string> pathIDList )
		{
			// clear the previous tool vec
			foreach( string szPathID in pathIDList ) {
				if( m_LeadAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> leadAISList = m_LeadAISDict[ szPathID ];
					foreach( AIS_Line leadAIS in leadAISList ) {
						m_Viewer.GetAISContext().Remove( leadAIS, false );
					}
					m_LeadAISDict[ szPathID ].Clear();
					m_LeadAISDict.Remove( szPathID );
				}
			}
		}

		void ShowLeadLine( List<string> pathIDList )
		{
			RomoveLeadLine( pathIDList );
			foreach( string szPathID in pathIDList ) {
				if( GetCacheInfoByID( m_DataManager, szPathID, out ICacheInfo cacheInfo ) == false
					|| cacheInfo.PathType != PathType.Contour ) {
					continue;
				}
				ContourCacheInfo contourCacheInfo = (ContourCacheInfo)cacheInfo;
				LeadData leadData = contourCacheInfo.GetPathLeadData();
				if( leadData.LeadIn.Type != LeadLineType.None ) {
					List<AIS_Line> leadAISList = new List<AIS_Line>();
					m_LeadAISDict.Add( szPathID, leadAISList );
					for( int i = 0; i < contourCacheInfo.LeadInCAMPointList.Count - 1; i++ ) {
						CAMPoint currentCAMPoint = contourCacheInfo.LeadInCAMPointList[ i ];
						CAMPoint nextCAMPoint = contourCacheInfo.LeadInCAMPointList[ i + 1 ];
						AIS_Line LeadAISLine = GetLineAIS( currentCAMPoint.CADPoint.Point, nextCAMPoint.CADPoint.Point, Quantity_NameOfColor.Quantity_NOC_GREENYELLOW );
						leadAISList.Add( LeadAISLine );
					}
				}

				if( leadData.LeadOut.Type != LeadLineType.None ) {
					List<AIS_Line> leadAISList;
					if( m_LeadAISDict.ContainsKey( szPathID ) ) {
						leadAISList = m_LeadAISDict[ szPathID ];
					}
					else {
						leadAISList = new List<AIS_Line>();
						m_LeadAISDict.Add( szPathID, leadAISList );
					}
					for( int i = 0; i < contourCacheInfo.LeadOutCAMPointList.Count - 1; i++ ) {
						CAMPoint currentCAMPoint = contourCacheInfo.LeadOutCAMPointList[ i ];
						CAMPoint nextCAMPoint = contourCacheInfo.LeadOutCAMPointList[ i + 1 ];
						AIS_Line LeadAISLine = GetLineAIS( currentCAMPoint.CADPoint.Point, nextCAMPoint.CADPoint.Point, Quantity_NameOfColor.Quantity_NOC_GREENYELLOW );
						leadAISList.Add( LeadAISLine );
					}
				}
			}

			// display the lead line
			foreach( string szPathID in pathIDList ) {
				if( m_LeadAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> leadAISList = m_LeadAISDict[ szPathID ];
					foreach( AIS_Line leadAIS in leadAISList ) {
						m_Viewer.GetAISContext().Display( leadAIS, false );
						m_Viewer.GetAISContext().Deactivate( leadAIS );
					}
				}
			}
		}

		void RomoveOrientation( List<string> pathIDList )
		{
			// clear the previous orientation
			foreach( string szPathID in pathIDList ) {
				if( m_OrientationAISList.ContainsKey( szPathID ) ) {
					m_Viewer.GetAISContext().Remove( m_OrientationAISList[ szPathID ], false );
					m_OrientationAISList.Remove( szPathID );
				}
			}
		}

		void ShowOrientation( List<string> pathIDList )
		{
			RomoveOrientation( pathIDList );

			// no need to show
			if( m_ShowOrientation == false ) {
				return;
			}

			foreach( string szPathID in pathIDList ) {
				if( GetCacheInfoByID( m_DataManager, szPathID, out ICacheInfo cacheInfo ) == false
					|| cacheInfo.PathType != PathType.Contour ) {
					continue;
				}
				ContourCacheInfo contourCacheInfo = (ContourCacheInfo)cacheInfo;
				gp_Pnt showPoint = contourCacheInfo.CAMPointList[ 0 ].CADPoint.Point;
				gp_Dir orientationDir = new gp_Dir( contourCacheInfo.CAMPointList[ 0 ].CADPoint.TangentVec.XYZ() );
				if( contourCacheInfo.GetPathIsReverse() ) {
					orientationDir.Reverse();
				}
				AIS_Shape orientationAIS = GetOrientationAIS( showPoint, orientationDir );
				m_OrientationAISList.Add( szPathID, orientationAIS );
			}

			// display the orientation
			foreach( string szPathID in pathIDList ) {
				if( m_OrientationAISList.ContainsKey( szPathID ) ) {
					m_Viewer.GetAISContext().Display( m_OrientationAISList[ szPathID ], false );
					m_Viewer.GetAISContext().Deactivate( m_OrientationAISList[ szPathID ] );
				}
			}
		}

		void RomoveLeadOrientation( List<string> pathIDList )
		{
			// clear the previous orientation
			foreach( string szPathID in pathIDList ) {
				if( m_LeadOrientationAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Shape> orientationAISList = m_LeadOrientationAISDict[ szPathID ];
					foreach( AIS_Shape orientationAIS in orientationAISList ) {
						m_Viewer.GetAISContext().Remove( orientationAIS, false );
					}
					m_LeadOrientationAISDict[ szPathID ].Clear();
					m_LeadOrientationAISDict.Remove( szPathID );
				}
			}
		}

		void ShowLeadOrientation( List<string> pathIDList )
		{
			RomoveLeadOrientation( pathIDList );
			foreach( string szPathID in pathIDList ) {
				if( GetCacheInfoByID( m_DataManager, szPathID, out ICacheInfo cacheInfo ) == false
					|| cacheInfo.PathType != PathType.Contour ) {
					continue;
				}
				ContourCacheInfo contourCacheInfo = (ContourCacheInfo)cacheInfo;
				LeadData leadData = contourCacheInfo.GetPathLeadData();
				if( leadData.LeadIn.Type != LeadLineType.None ) {
					List<AIS_Shape> orientationAISList = new List<AIS_Shape>();
					m_LeadOrientationAISDict.Add( szPathID, orientationAISList );

					if( contourCacheInfo.LeadInCAMPointList.Count == 0 ) {
						break;
					}
					gp_Pnt leadInStartPoint = contourCacheInfo.LeadInCAMPointList.First().CADPoint.Point;
					gp_Dir leadInOrientationDir = new gp_Dir( contourCacheInfo.LeadInCAMPointList.First().CADPoint.TangentVec.XYZ() );
					AIS_Shape orientationAIS = GetOrientationAIS( leadInStartPoint, leadInOrientationDir );
					orientationAISList.Add( orientationAIS );
				}

				// path with lead out
				if( leadData.LeadOut.Type != LeadLineType.None ) {

					List<AIS_Shape> orientationAISList;
					if( m_LeadOrientationAISDict.ContainsKey( szPathID ) ) {
						orientationAISList = m_LeadOrientationAISDict[ szPathID ];
					}
					else {
						orientationAISList = new List<AIS_Shape>();
						m_LeadOrientationAISDict.Add( szPathID, orientationAISList );
					}

					if( contourCacheInfo.LeadOutCAMPointList.Count == 0 ) {
						break;
					}
					gp_Pnt leadOutEndPoint = contourCacheInfo.LeadOutCAMPointList.Last().CADPoint.Point;
					gp_Dir leadOutOrientationDir = new gp_Dir( contourCacheInfo.LeadOutCAMPointList.Last().CADPoint.TangentVec.XYZ() );
					AIS_Shape orientationAIS = GetOrientationAIS( leadOutEndPoint, leadOutOrientationDir );
					orientationAISList.Add( orientationAIS );
				}
			}

			// display the orientation
			foreach( string szPathID in pathIDList ) {
				if( m_LeadOrientationAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Shape> orientationAISList = m_LeadOrientationAISDict[ szPathID ];
					foreach( AIS_Shape orientationAIS in orientationAISList ) {
						m_Viewer.GetAISContext().Display( orientationAIS, false );
						m_Viewer.GetAISContext().Deactivate( orientationAIS );
					}
				}
			}
		}

		void RomoveIndex()
		{
			foreach( AIS_TextLabel textLabel in m_IndexList ) {
				m_Viewer.GetAISContext().Remove( textLabel, false );
			}
			m_IndexList.Clear();
		}

		void ShowIndex()
		{
			RomoveIndex();

			// no need to show
			if( m_ShowOrder == false ) {
				return;
			}

			// create text label
			int nCurrentIndex = 0;
			foreach( ContourCacheInfo cacheInfo in m_DataManager.GetContourCacheInfoList() ) {
				gp_Pnt location = cacheInfo.CAMPointList[ 0 ].CADPoint.Point;
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

		void RomoveOverCut( List<string> pathIDList )
		{
			// clear the previous tool vec
			foreach( string szPathID in pathIDList ) {
				if( m_OverCutAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> overcutAISList = m_OverCutAISDict[ szPathID ];
					foreach( AIS_Line overcutAIS in overcutAISList ) {
						m_Viewer.GetAISContext().Remove( overcutAIS, false );
					}
					m_OverCutAISDict[ szPathID ].Clear();
					m_OverCutAISDict.Remove( szPathID );
				}
			}
		}

		void ShowOverCut( List<string> pathIDList )
		{
			RomoveOverCut( pathIDList );
			foreach( string szPathID in pathIDList ) {
				if( GetCacheInfoByID( m_DataManager, szPathID, out ICacheInfo cacheInfo ) == false
					|| cacheInfo.PathType != PathType.Contour ) {
					continue;
				}
				ContourCacheInfo contourCacheInfo = (ContourCacheInfo)cacheInfo;
				List<AIS_Line> overcutAISList = new List<AIS_Line>();
				m_OverCutAISDict.Add( szPathID, overcutAISList );
				if( contourCacheInfo.GetPathOverCutLength() > 0 ) {
					for( int i = 0; i < contourCacheInfo.OverCutCAMPointList.Count - 1; i++ ) {
						AIS_Line overCutAISLine = GetLineAIS( contourCacheInfo.OverCutCAMPointList[ i ].CADPoint.Point, contourCacheInfo.OverCutCAMPointList[ i + 1 ].CADPoint.Point, Quantity_NameOfColor.Quantity_NOC_DEEPPINK );
						overcutAISList.Add( overCutAISLine );
					}
				}
			}

			// display the tool vec
			foreach( string szPathID in pathIDList ) {
				if( m_OverCutAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> overcutAISList = m_OverCutAISDict[ szPathID ];
					foreach( AIS_Line overcutAIS in overcutAISList ) {
						m_Viewer.GetAISContext().Display( overcutAIS, false );
						m_Viewer.GetAISContext().Deactivate( overcutAIS );
					}
				}
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


			for( int i = 1; i < m_DataManager.PathIDList.Count; i++ ) {
				ContourCacheInfo previousCacheInfo = ( m_DataManager.ObjectMap[ m_DataManager.PathIDList[ i - 1 ] ] as ContourPathObject ).ContourCacheInfo;
				ContourCacheInfo currentCacheInfo = ( m_DataManager.ObjectMap[ m_DataManager.PathIDList[ i ] ] as ContourPathObject ).ContourCacheInfo;
				CraftData currentCraftData = ( m_DataManager.ObjectMap[ m_DataManager.PathIDList[ i ] ] as ContourPathObject ).CraftData;

				// p1: end of previous path
				// p2: lift up point of previous path
				// p3: frog leap middle point (if frog leap)
				// p4: cut down point of current path
				// p5: start of current path
				ITraversePoint p1 = previousCacheInfo.GetProcessEndPoint();
				ITraversePoint p2 = TraverseHelper.GetCutDownOrLiftUpPoint( previousCacheInfo.GetProcessEndPoint(), currentCraftData.TraverseData.LiftUpDistance );
				ITraversePoint p4 = TraverseHelper.GetCutDownOrLiftUpPoint( currentCacheInfo.GetProcessStartPoint(), currentCraftData.TraverseData.CutDownDistance );
				ITraversePoint p5 = currentCacheInfo.GetProcessStartPoint();

				// lift up
				if( currentCraftData.TraverseData.LiftUpDistance > 0 && p1 != null && p2 != null ) {
					AddOneLinearTraverse( p1.Point, p2.Point );
				}

				// frog leap
				if( currentCraftData.TraverseData.FrogLeapDistance > 0 && p2 != null && p4 != null ) {
					ITraversePoint p3 = TraverseHelper.GetFrogLeapMiddlePoint( p2, p4, currentCraftData.TraverseData.FrogLeapDistance );
					if( p3 != null ) {
						GC_MakeArcOfCircle makeCircle = new GC_MakeArcOfCircle( p2.Point, p3.Point, p4.Point );
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
							AddOneLinearTraverse( p2.Point, p4.Point );
						}
					}
					else {
						// fallback to normal traverse line
						AddOneLinearTraverse( p2.Point, p4.Point );
					}
				}

				// normal traverse
				else if( p2 != null && p4 != null ) {
					AddOneLinearTraverse( p2.Point, p4.Point );
				}

				// cut down
				if( currentCraftData.TraverseData.CutDownDistance > 0 && p4 != null && p5 != null ) {
					AddOneLinearTraverse( p4.Point, p5.Point );
				}
			}

			// entry
			if( m_DataManager.EntryAndExitData.EntryDistance > 0 && m_DataManager.PathIDList.Count != 0 ) {
				if( GetCacheInfoByID( m_DataManager, m_DataManager.PathIDList.First(), out ICacheInfo cacheInfo ) == false ) {
					return;
				}
				ITraversePoint firstPathStartPoint = cacheInfo.GetProcessStartPoint();
				ITraversePoint entryPoint = TraverseHelper.GetCutDownOrLiftUpPoint( firstPathStartPoint.Clone(), m_DataManager.EntryAndExitData.EntryDistance );
				if( firstPathStartPoint != null && entryPoint != null ) {
					AddOneLinearTraverse( entryPoint.Point, firstPathStartPoint.Point );
				}
			}

			// exit
			if( m_DataManager.EntryAndExitData.ExitDistance > 0 && m_DataManager.PathIDList.Count != 0 ) {
				if( GetCacheInfoByID( m_DataManager, m_DataManager.PathIDList.Last(), out ICacheInfo cacheInfo ) == false ) {
					return;
				}
				ITraversePoint lastPathEndPoint = cacheInfo.GetProcessEndPoint();
				ITraversePoint exitPoint = TraverseHelper.GetCutDownOrLiftUpPoint( lastPathEndPoint.Clone(), m_DataManager.EntryAndExitData.ExitDistance );
				if( lastPathEndPoint != null && exitPoint != null ) {
					AddOneLinearTraverse( lastPathEndPoint.Point, exitPoint.Point );
				}
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

		void HideAllCAMData()
		{
			// hide tool vec
			foreach( List<AIS_Line> toolVecAIS in m_ToolVecAISDict.Values ) {
				foreach( AIS_Line lineAIS in toolVecAIS ) {
					m_Viewer.GetAISContext().Remove( lineAIS, false );
				}
			}

			// hide orientation
			foreach( AIS_Shape orientationAIS in m_OrientationAISList.Values ) {
				m_Viewer.GetAISContext().Remove( orientationAIS, false );
			}

			// hide index
			foreach( AIS_TextLabel textLabel in m_IndexList ) {
				m_Viewer.GetAISContext().Remove( textLabel, false );
			}

			// hide over cut
			foreach( List<AIS_Line> overCutAISList in m_OverCutAISDict.Values ) {
				foreach( AIS_Line overCutAIS in overCutAISList ) {
					m_Viewer.GetAISContext().Remove( overCutAIS, false );
				}
			}

			// hide lead 
			foreach( List<AIS_Line> leadAISList in m_LeadAISDict.Values ) {
				foreach( AIS_Line leadAIS in leadAISList ) {
					m_Viewer.GetAISContext().Remove( leadAIS, false );
				}
			}

			// hide traverse path
			foreach( AIS_Line traverseAIS in m_TraverseAISList ) {
				m_Viewer.GetAISContext().Remove( traverseAIS, false );
			}
			foreach( AIS_Shape frogLeapAIS in m_FrogLeapAISList ) {
				m_Viewer.GetAISContext().Remove( frogLeapAIS, false );
			}

			// hide lead orientation
			foreach( List<AIS_Shape> orientationAISList in m_LeadOrientationAISDict.Values ) {
				foreach( AIS_Shape orientationAIS in orientationAISList ) {
					m_Viewer.GetAISContext().Remove( orientationAIS, false );
				}
			}
			m_Viewer.UpdateView();
		}

		#endregion

		// methods
		bool IsModifiedToolVecIndex( int index, ContourCacheInfo cacheInfo, List<CADPoint> cadPointList )
		{
			// map CAD and CAM point index
			int nLength = cadPointList.Count;
			int modifiedIndex = cacheInfo.GetPathIsReverse()
				? ( nLength - ( cacheInfo.IsClosed ? 0 : 1 ) - index + cacheInfo.GetPathStartPointIndex() ) % nLength
				: ( index + cacheInfo.GetPathStartPointIndex() ) % nLength;

			// need highlight if the index is modified index
			return cacheInfo.GetToolVecModifyIndex().Contains( modifiedIndex );
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

		List<string> GetSelectedIDList()
		{
			return m_DefaultAction.GetSelectedIDs();
		}

		void AddOneLinearTraverse( gp_Pnt startPnt, gp_Pnt endPnt )
		{
			AIS_Line traverseAIS = GetLineAIS( startPnt, endPnt, Quantity_NameOfColor.Quantity_NOC_RED, 1, 1, true );
			m_TraverseAISList.Add( traverseAIS );
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

		bool GetCraftDataByID( DataManager dataManager, string szPathID, out CraftData craftData )
		{
			craftData = null;
			if( string.IsNullOrEmpty( szPathID )
				|| dataManager.ObjectMap.ContainsKey( szPathID ) == false
				|| dataManager.ObjectMap[ szPathID ] == null
				|| !( dataManager.ObjectMap[ szPathID ] is PathObject )
				|| ( (PathObject)dataManager.ObjectMap[ szPathID ] ).CraftData == null ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]所選路徑資料異常，請重新選擇", MyApp.NoticeType.Hint );
				return false;
			}

			if( ( (PathObject)dataManager.ObjectMap[ szPathID ] ).PathType == PathType.Contour ) {
				craftData = ( (ContourPathObject)dataManager.ObjectMap[ szPathID ] ).CraftData;
			}
			return true;
		}

		bool GetCacheInfoByID( DataManager dataManager, string szPathID, out ICacheInfo cacheInfo )
		{
			cacheInfo = null;
			if( string.IsNullOrEmpty( szPathID )
				|| dataManager.ObjectMap.ContainsKey( szPathID ) == false
				|| dataManager.ObjectMap[ szPathID ] == null ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]所選路徑資料異常，請重新選擇", MyApp.NoticeType.Hint );
				return false;
			}
			if( ( (PathObject)dataManager.ObjectMap[ szPathID ] ).PathType == PathType.Contour ) {
				cacheInfo = ( (ContourPathObject)dataManager.ObjectMap[ szPathID ] ).ContourCacheInfo;
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
