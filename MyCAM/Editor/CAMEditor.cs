using MyCAM.Data;
using OCC.AIS;
using OCC.BRepPrimAPI;
using OCC.Geom;
using OCC.gp;
using OCC.Graphic3d;
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
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class CAMEditor
	{
		public CAMEditor( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager )
		{
			if( viewer == null || treeView == null || cadManager == null || viewManager == null ) {
				throw new ArgumentNullException( "CAMEditor consturcting argument null." );
			}

			// data manager
			m_CADManager = cadManager;
			m_CADManager.PathAdded += OnPathAdded;

			// user interface
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;

			// default action
			m_DefaultAction = new DefaultAction( m_Viewer, m_TreeView, m_CADManager, m_ViewManager, ESelectObjectType.Path );
		}

		// data manager
		DataManager m_CADManager;

		// user interface
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;

		// action
		IEditorAction m_DefaultAction;
		IEditorAction m_CurrentAction;

		// for viewer resource handle
		List<AIS_Line> m_ToolVecAISList = new List<AIS_Line>(); // need refresh, no need activate
		List<AIS_Shape> m_OrientationAISList = new List<AIS_Shape>(); // need refresh, no need activate
		List<AIS_TextLabel> m_IndexList = new List<AIS_TextLabel>(); // need refresh, no need activate
		enum EvecType
		{
			ToolVec,
			TangentVec,
			NormalVec,
		}
		int m_nGap = 1;

		// editor
		public void EditStart()
		{
			// init tree
			m_TreeView.Nodes.Add( m_ViewManager.PathNode );

			// start default action
			m_CurrentAction = m_DefaultAction;
			m_DefaultAction.Start();
		}

		public void EditEnd()
		{
		}

		// APIs
		public void StartSelectFace()
		{
			SelectFaceAction action = new SelectFaceAction( m_Viewer, m_TreeView, m_CADManager, m_ViewManager );
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
			m_CADManager.AddPath( pathWireList, edgeFaceMap );
		}

		public void StartSelectPath_Manual()
		{
			// get selected face group from select face action
			if( m_CurrentAction.ActionType != EditActionType.SelectFace ) {
				return;
			}
			List<TopoDS_Shape> selectedFaceGroupList = ( (SelectFaceAction)m_CurrentAction ).GetResult();

			// end all actions if no face is selected
			if( selectedFaceGroupList.Count == 0 ) {
				m_CurrentAction.End();
				return;
			}
			SelectPathAction action = new SelectPathAction( m_Viewer, m_TreeView, m_CADManager, m_ViewManager, selectedFaceGroupList );
			StartEditAction( action );
		}

		public void EndSelectPath_Manual()
		{
			if( m_CurrentAction.ActionType != EditActionType.SelectPath ) {
				return;
			}
			( (SelectPathAction)m_CurrentAction ).SelectDone();
		}

		public void SetStartPoint()
		{
		}

		public void SetReverse()
		{
			if( m_CurrentAction.ActionType != EditActionType.Default ) {
				return;
			}
			string szPathID = GetSelectedPathID();
			if( string.IsNullOrEmpty( szPathID ) || !m_CADManager.ShapeDataMap.ContainsKey( szPathID ) ) {
				return;
			}
			PathData pathData = (PathData)m_CADManager.ShapeDataMap[ szPathID ];

			// toggle reverse state
			pathData.CAMData.IsReverse = !pathData.CAMData.IsReverse;
			ShowCAMData();
		}

		public void SetToolVec()
		{
		}

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
				AIS_Shape aisShape = ViewHelper.CreatePathAIS( m_CADManager.ShapeDataMap[ szID ].Shape );
				m_ViewManager.ViewObjectMap.Add( szID, new ViewObject( aisShape ) );
				m_Viewer.GetAISContext().Display( aisShape, false ); // this will also activate
			}

			// update tree view and viewer
			m_ViewManager.PathNode.ExpandAll();
			ShowCAMData();
			m_Viewer.UpdateView();
		}

		// view
		void ShowCAMData()
		{
			ShowToolVec();
			ShowOrientation();
			ShowIndex();
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
			foreach( CAMData camData in GetCAMDataList() ) {
				List<CAMPoint> filteredPath = camData.CAMPointList;
				for( int i = 0; i < filteredPath.Count; i++ ) {
					if( IsKeyToolVecIndex( i, camData, out bool bHL ) ) {
						CAMPoint camPoint = filteredPath[ i ];
						AIS_Line toolVecAIS = GetVecAIS( camPoint.CADPoint.Point, camPoint.ToolVec, EvecType.ToolVec );
						if( bHL ) {
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

		void ShowOrientation()
		{
			// clear the previous orientation
			foreach( AIS_Shape orientationAIS in m_OrientationAISList ) {
				m_Viewer.GetAISContext().Remove( orientationAIS, false );
			}
			m_OrientationAISList.Clear();

			// build orientation
			foreach( CAMData camData in GetCAMDataList() ) {
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

		void ShowIndex()
		{
			// clear the previous text label
			foreach( AIS_TextLabel textLabel in m_IndexList ) {
				m_Viewer.GetAISContext().Remove( textLabel, false );
			}
			m_IndexList.Clear();

			// create text label
			int nCurrentIndex = 0;
			foreach( CAMData camData in GetCAMDataList() ) {
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

		bool IsKeyToolVecIndex( int index, CAMData camData, out bool isToolVecMod )
		{
			// map CAD and CAM point index
			int modifiedIndex = camData.IsReverse
				? ( camData.CAMPointList.Count - 1 - index + camData.StartPoint ) % camData.CAMPointList.Count
				: ( index + camData.StartPoint ) % camData.CAMPointList.Count;

			// need highlight if the index is modified index
			isToolVecMod = camData.GetToolVecModifyIndex().Contains( modifiedIndex );

			// need to show if the index is key index
			bool isEdgeStart = camData.EdgeStartIndex.Contains( modifiedIndex );
			return index % m_nGap == 0 || isEdgeStart || isToolVecMod;
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

		List<CAMData> GetCAMDataList()
		{
			List<CAMData> camDataList = new List<CAMData>();
			foreach( string pathID in m_CADManager.PathIDList ) {
				camDataList.Add( ( (PathData)m_CADManager.ShapeDataMap[ pathID ] ).CAMData );
			}
			return camDataList;
		}

		// methods
		string GetSelectedPathID()
		{
			TreeNode selectedNode = m_TreeView.SelectedNode;
			if( selectedNode == null || string.IsNullOrEmpty( selectedNode.Text ) ) {
				return string.Empty;
			}
			return selectedNode.Text;
		}

		// edit actions
		void StartEditAction( IEditorAction action )
		{
			// to prevent from non-necessary default action start
			m_IsNextAction = true;

			// end the current action
			m_CurrentAction.End();
			m_IsNextAction = false;

			// start the action
			m_CurrentAction = action;
			m_CurrentAction.Start();
			m_CurrentAction.EndAction += OnEditActionEnd;
		}

		void OnEditActionEnd( IEditorAction action )
		{
			// start default action if all edit actions are done
			if( !m_IsNextAction ) {
				m_CurrentAction = m_DefaultAction;
				m_CurrentAction.Start();
			}
		}

		bool m_IsNextAction = false;
	}
}
