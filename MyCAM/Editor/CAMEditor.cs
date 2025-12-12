using MyCAM.App;
using MyCAM.CacheInfo;
using MyCAM.Data;
using MyCAM.Editor.Renderer;
using MyCAM.Post;
using OCC.AIS;
using OCC.gp;
using OCC.ShapeAnalysis;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
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

			// Initialize renderers
			m_IndexRenderer = new IndexRenderer( m_Viewer, m_DataManager );
			m_OrientationRenderer = new OrientationRenderer( m_Viewer, m_DataManager );
			m_CraftRenderer = new CraftRenderer( m_Viewer, m_DataManager );
			m_ToolVecRenderer = new ToolVecRenderer( m_Viewer, m_DataManager );
			m_TraverseRenderer = new TraverseRenderer( m_Viewer, m_DataManager );
		}

		public const string PATH_NODE_PREFIX = "Path_";
		public const int PATH_NODE_PERFIX_LENGTH = 5;

		// Renderers
		IndexRenderer m_IndexRenderer;
		OrientationRenderer m_OrientationRenderer;
		CraftRenderer m_CraftRenderer;
		ToolVecRenderer m_ToolVecRenderer;
		TraverseRenderer m_TraverseRenderer;

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
			int newIndex;
			if( bUp ) {
				newIndex = nIndex - 1;
				m_DataManager.PathIDList.Insert( newIndex, szPathID );
			}
			else {
				newIndex = nIndex + 1;
				m_DataManager.PathIDList.Insert( newIndex, szPathID );
			}

			// tree view select moved node
			string newNodeID = PATH_NODE_PREFIX + ( newIndex + 1 ).ToString();
			if( m_ViewManager.TreeNodeMap.ContainsKey( newNodeID ) ) {
				TreeNode nodeToSelect = m_ViewManager.TreeNodeMap[ newNodeID ];
				m_TreeView.SelectedNode = nodeToSelect;

				if( m_DefaultAction is SelectPathAction_ selectAction ) {
					selectAction.ClearSelection();
					selectAction.SelectPathByID( szPathID );
				}
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
			gp_Pnt currentPoint = cacheInfo.GetProcessStartPoint().Point;

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
					gp_Pnt nextStartPoint = nextCacheInfo.GetProcessStartPoint().Point;
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

			// select focus on first path
			if( m_DefaultAction is SelectPathAction_ selectAction ) {
				selectAction.ClearSelection();
				selectAction.SelectPathByID( m_DataManager.PathIDList.First() );
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
				int nodeIndex = m_DataManager.PathIDList.IndexOf( szID ) + 1; // 1 based index
				if( nodeIndex == -1 ) {
					continue;
				}
				string szNodeID = PATH_NODE_PREFIX + nodeIndex.ToString();
				TreeNode node = new TreeNode( szNodeID );
				m_ViewManager.PathNode.Nodes.Add( node );
				m_ViewManager.TreeNodeMap.Add( szNodeID, node );

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
			ShowCAMData( pathIDList );
			}

		void ShowCAMData( List<string> pathIDList )
		{
			m_ToolVecRenderer.Show( pathIDList );
			m_OrientationRenderer.Show( pathIDList );
			m_IndexRenderer.Show();
			m_CraftRenderer.Show( pathIDList );
			m_TraverseRenderer.Show();
			m_Viewer.UpdateView();
			}

		void RemoveAllCAMData()
		{
			List<string> pathIDList = m_DataManager.PathIDList;
			RemoveCAMData( pathIDList );
			}

		void RemoveCAMData( List<string> pathIDList )
		{
			m_ToolVecRenderer.Remove( pathIDList );
			m_CraftRenderer.Remove( pathIDList );
			m_OrientationRenderer.Remove( pathIDList );
			m_IndexRenderer.Remove();
			m_TraverseRenderer.Remove();
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
