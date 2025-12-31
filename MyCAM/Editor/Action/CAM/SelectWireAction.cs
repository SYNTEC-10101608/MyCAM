using MyCAM.App;
using MyCAM.Data;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.ShapeAnalysis;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using OCCViewer;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class SelectWireAction : KeyMouseActionBase
	{
		class EdgeHandle
		{
			public TopoDS_Edge Edge
			{
				get; set;
			}

			public AIS_Shape AIS
			{
				get; set;
			}
		}

		public SelectWireAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager,
			List<TopoDS_Shape> faceGroupList )
			: base( dataManager, viewer, treeView, viewManager )
		{
			// get face group list from select face action
			if( faceGroupList == null || faceGroupList.Count == 0 ) {
				m_FaceGroupList = new List<TopoDS_Shape>();
			}
			else {
				m_FaceGroupList = faceGroupList;
			}

			// init data
			m_EdgeFaceMap = new TopTools_IndexedDataMapOfShapeListOfShape();
			m_EdgeAISPairList = new List<EdgeHandle>();
			m_FreeBoundWireList = new List<List<TopoDS_Edge>>();
			m_FaceGroupAISList = new List<AIS_Shape>();
			m_EdgeHandleToWireMap = new Dictionary<EdgeHandle, List<EdgeHandle>>();

			// collect all free boundary from face group
			foreach( var oneShape in m_FaceGroupList ) {

				// create face AIS
				AIS_Shape faceAIS = SelectViewHelper.CreateFaceAIS( oneShape );
				m_FaceGroupAISList.Add( faceAIS );

				// map edge and face
				TopExp.MapShapesAndAncestors( oneShape, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref m_EdgeFaceMap );

				// get free boundary on face
				ShapeAnalysis_FreeBounds sfb = new ShapeAnalysis_FreeBounds( oneShape );
				TopoDS_Shape closedWires = sfb.GetClosedWires();
				if( closedWires.IsNull() ) {
					continue;
				}

				// collect all free boundary wires
				TopExp_Explorer allWireExp = new TopExp_Explorer( closedWires, TopAbs_ShapeEnum.TopAbs_WIRE );
				for( ; allWireExp.More(); allWireExp.Next() ) {
					TopoDS_Wire wire = TopoDS.ToWire( allWireExp.Current() );
					List<TopoDS_Edge> oneFreeBoundWire = new List<TopoDS_Edge>();
					List<EdgeHandle> wireHandles = new List<EdgeHandle>();

					// collect all edge
					TopExp_Explorer oneWireExp = new TopExp_Explorer( wire, TopAbs_ShapeEnum.TopAbs_EDGE );
					for( ; oneWireExp.More(); oneWireExp.Next() ) {
						TopoDS_Edge edge = TopoDS.ToEdge( oneWireExp.Current() );
						AIS_Shape aisShape = SelectViewHelper.CreateEdgeAIS( edge );
						EdgeHandle handle = new EdgeHandle() { Edge = edge, AIS = aisShape };
						m_EdgeAISPairList.Add( handle );
						oneFreeBoundWire.Add( edge );
						wireHandles.Add( handle );
					}

					if( oneFreeBoundWire.Count > 0 ) {
						m_FreeBoundWireList.Add( oneFreeBoundWire );

						// create edge handle to wire handle map
						foreach( var handle in wireHandles ) {
							m_EdgeHandleToWireMap[ handle ] = wireHandles;
						}
					}
				}
			}
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.SelectPath;
			}
		}

		public override void Start()
		{
			base.Start();

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// disable tree view
			m_TreeView.Enabled = false;

			// hide all shape
			foreach( ViewObject viewObject in m_ViewManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Erase( viewObject.AISHandle, false );
			}

			// show element for selction
			ShowElement();
			m_Viewer.UpdateView();
		}

		public override void End()
		{
			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// enable tree view
			m_TreeView.Enabled = true;

			// show all shape
			foreach( ViewObject viewObject in m_ViewManager.ViewObjectMap.Values ) {
				if( viewObject.Visible ) {
					m_Viewer.GetAISContext().Display( viewObject.AISHandle, false );
					m_Viewer.GetAISContext().Deactivate( viewObject.AISHandle );
				}
			}

			// hide element for selection
			HideElement();
			m_Viewer.UpdateView();
			base.End();
		}

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {
				if( m_Viewer.GetAISContext().DetectedOwner().IsNull()
					|| m_Viewer.GetAISContext().DetectedOwner().HasSelectable() == false ) {
					m_Viewer.GetAISContext().ClearSelected( true );
					return;
				}
				AIS_InteractiveObject detectedObject = m_Viewer.GetAISContext().DetectedInteractive();
				if( ( Control.ModifierKeys & Keys.Control ) != Keys.Control ) {
					m_Viewer.GetAISContext().ClearSelected( false );
				}
				m_Viewer.GetAISContext().AddOrRemoveSelected( detectedObject, false );
				m_Viewer.UpdateView();
			}
		}

		protected override void ViewerMouseDoubleClick( MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {
				SelectClosedWire();
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		public void SelectClosedWire()
		{
			List<TopoDS_Edge> selectedEdges = GetSelectedEdgeList();
			if( selectedEdges.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先選擇邊", MyApp.NoticeType.Hint );
				return;
			}

			// find all selected edge handles
			List<EdgeHandle> selectedHandles = new List<EdgeHandle>();
			foreach( var selectedEdge in selectedEdges ) {
				foreach( var handle in m_EdgeAISPairList ) {
					if( handle.Edge.IsEqual( selectedEdge ) ) {
						selectedHandles.Add( handle );
						break;
					}
				}
			}

			// collect all wire handles that belong to the same wires as selected edges
			HashSet<EdgeHandle> allWireHandles = new HashSet<EdgeHandle>();
			foreach( var handle in selectedHandles ) {
				if( m_EdgeHandleToWireMap.TryGetValue( handle, out var wireHandles ) ) {
					foreach( var wireHandle in wireHandles ) {
						allWireHandles.Add( wireHandle );
					}
				}
			}

			// clear current selection
			m_Viewer.GetAISContext().ClearSelected( false );

			// select all edges that belong to the same wires
			foreach( var handle in allWireHandles ) {
				m_Viewer.GetAISContext().AddOrRemoveSelected( handle.AIS, false );
			}
			m_Viewer.UpdateView();
		}

		public void SelectDone()
		{
			// get selected edge
			TopTools_MapOfShape extractedEdgeSet = GetSelectedEdge();
			if( extractedEdgeSet.Size() == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]未選取任何路徑", MyApp.NoticeType.Hint );
				End();
				return;
			}

			// series the selected edge if possible
			List<List<TopoDS_Edge>> edgeGroupList = new List<List<TopoDS_Edge>>();
			foreach( var oneFreeBound in m_FreeBoundWireList ) {
				List<List<TopoDS_Edge>> subWire = FindSubWires( oneFreeBound, extractedEdgeSet );
				if( subWire.Count > 0 ) {
					edgeGroupList.AddRange( subWire );
				}
			}

			// make wire
			List<TopoDS_Wire> pathWireList = new List<TopoDS_Wire>();
			foreach( var oneEdgeGroup in edgeGroupList ) {
				if( oneEdgeGroup.Count == 0 ) {
					continue;
				}
				BRepBuilderAPI_MakeWire wireMaker = new BRepBuilderAPI_MakeWire();
				foreach( var oneEdge in oneEdgeGroup ) {
					wireMaker.Add( oneEdge );
					if( !wireMaker.IsDone() ) {
						break;
					}
				}
				if( wireMaker.IsDone() ) {
					pathWireList.Add( wireMaker.Wire() );
				}
			}

			// update datas
			m_DataManager.AddPath( pathWireList, m_EdgeFaceMap );
			End();
		}

		public static List<List<TopoDS_Edge>> FindSubWires( List<TopoDS_Edge> wire, TopTools_MapOfShape extractedEdgeSet )
		{
			var allSubWires = new List<List<TopoDS_Edge>>();
			int n = wire.Count;
			int i = 0;

			while( i < n ) {
				if( extractedEdgeSet.Contains( wire[ i ] ) ) {
					var oneSubWire = new List<TopoDS_Edge>();
					while( i < n && extractedEdgeSet.Contains( wire[ i ] ) ) {
						oneSubWire.Add( wire[ i ] );
						i++;
					}
					allSubWires.Add( oneSubWire );
				}
				else {
					i++;
				}
			}

			// Check for circular join: last run touches end, and first run starts at 0
			if( allSubWires.Count >= 2 &&
				extractedEdgeSet.Contains( wire[ 0 ] ) &&
				extractedEdgeSet.Contains( wire[ n - 1 ] ) ) {
				var first = allSubWires[ 0 ];
				var last = allSubWires[ allSubWires.Count - 1 ];

				// Combine last + first
				var circularWire = new List<TopoDS_Edge>( last );
				circularWire.AddRange( first );

				// Replace runs
				allSubWires[ 0 ] = circularWire;
				allSubWires.RemoveAt( allSubWires.Count - 1 );
			}

			return allSubWires;
		}

		void ShowElement()
		{
			foreach( var faceAIS in m_FaceGroupAISList ) {
				m_Viewer.GetAISContext().Display( faceAIS, false );
				m_Viewer.GetAISContext().Deactivate( faceAIS );
			}
			foreach( var edgeAISPair in m_EdgeAISPairList ) {
				m_Viewer.GetAISContext().Display( edgeAISPair.AIS, false );
				m_Viewer.GetAISContext().Activate( edgeAISPair.AIS, (int)AISActiveMode.Edge );
			}
		}

		void HideElement()
		{
			foreach( var faceAIS in m_FaceGroupAISList ) {
				m_Viewer.GetAISContext().Remove( faceAIS, false );
			}
			foreach( var edgeAISPair in m_EdgeAISPairList ) {
				m_Viewer.GetAISContext().Remove( edgeAISPair.AIS, false );
			}
		}

		TopTools_MapOfShape GetSelectedEdge()
		{
			TopTools_MapOfShape selectedEdgeSet = new TopTools_MapOfShape();
			m_Viewer.GetAISContext().InitSelected();
			while( m_Viewer.GetAISContext().MoreSelected() ) {
				var shape = m_Viewer.GetAISContext().SelectedShape();
				if( shape == null || shape.ShapeType() != TopAbs_ShapeEnum.TopAbs_EDGE ) {
					m_Viewer.GetAISContext().NextSelected();
					continue;
				}
				selectedEdgeSet.Add( TopoDS.ToEdge( shape ) );
				m_Viewer.GetAISContext().NextSelected();
			}
			return selectedEdgeSet;
		}

		List<TopoDS_Edge> GetSelectedEdgeList()
		{
			List<TopoDS_Edge> selectedEdgeList = new List<TopoDS_Edge>();
			m_Viewer.GetAISContext().InitSelected();
			while( m_Viewer.GetAISContext().MoreSelected() ) {
				var shape = m_Viewer.GetAISContext().SelectedShape();
				if( shape == null || shape.ShapeType() != TopAbs_ShapeEnum.TopAbs_EDGE ) {
					m_Viewer.GetAISContext().NextSelected();
					continue;
				}
				selectedEdgeList.Add( TopoDS.ToEdge( shape ) );
				m_Viewer.GetAISContext().NextSelected();
			}
			return selectedEdgeList;
		}

		List<TopoDS_Shape> m_FaceGroupList;
		TopTools_IndexedDataMapOfShapeListOfShape m_EdgeFaceMap;
		List<EdgeHandle> m_EdgeAISPairList;
		List<List<TopoDS_Edge>> m_FreeBoundWireList;
		List<AIS_Shape> m_FaceGroupAISList;
		Dictionary<EdgeHandle, List<EdgeHandle>> m_EdgeHandleToWireMap;
	}
}
