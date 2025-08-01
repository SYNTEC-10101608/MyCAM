using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.Quantity;
using OCC.ShapeAnalysis;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using OCCViewer;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.CAD
{
	internal class SelectPathAction : CADACtionBase
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

		public SelectPathAction( Viewer viewer, TreeView treeView, CADManager cadManager, List<TopoDS_Shape> faceGroupList )
			: base( viewer, treeView, cadManager )
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

					// collect all edge
					TopExp_Explorer oneWireExp = new TopExp_Explorer( wire, TopAbs_ShapeEnum.TopAbs_EDGE );
					for( ; oneWireExp.More(); oneWireExp.Next() ) {
						TopoDS_Edge edge = TopoDS.ToEdge( oneWireExp.Current() );
						AIS_Shape aisShape = SelectViewHelper.CreateEdgeAIS( edge );
						m_EdgeAISPairList.Add( new EdgeHandle() { Edge = edge, AIS = aisShape } );
						oneFreeBoundWire.Add( edge );
					}
					if( oneFreeBoundWire.Count > 0 ) {
						m_FreeBoundWireList.Add( oneFreeBoundWire );
					}
				}
			}
		}

		public override CADActionType ActionType
		{
			get
			{
				return CADActionType.SelectPath;
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
			foreach( ViewObject viewObject in m_CADManager.ViewObjectMap.Values ) {
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
			foreach( ViewObject viewObject in m_CADManager.ViewObjectMap.Values ) {
				if( viewObject.Visible ) {
					m_Viewer.GetAISContext().Display( viewObject.AISHandle, false );
				}
			}

			// hide element for selection
			HideElement();
			m_Viewer.UpdateView();
			base.End();
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			// TODO: figure how OCCT slect work, dont do shit like this
			if( e.Button == MouseButtons.Left ) {

				// the program will crash if nothing detected, so dont use detected interactive API
				m_Viewer.Select();
				m_Viewer.GetAISContext().InitSelected();
				if( !m_Viewer.GetAISContext().MoreSelected() ) {
					return;
				}
				AIS_InteractiveObject ais = m_Viewer.GetAISContext().SelectedInteractive();
				if( ais == null || ais.IsNull() ) {
					return;
				}
				m_Viewer.GetAISContext().ClearSelected( false );

				// arrange the colors
				Quantity_Color color = new Quantity_Color();
				ais.Color( ref color );

				// toggle color
				if( color.Name() == SelectViewHelper.COLOR_DEFAULT ) {
					ais.SetColor( new Quantity_Color( SelectViewHelper.COLOR_SELECTED ) );
				}
				else {
					ais.SetColor( new Quantity_Color( SelectViewHelper.COLOR_DEFAULT ) );
				}
				ais.Attributes().SetFaceBoundaryDraw( true );
				ais.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
				m_Viewer.UpdateView();
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
		}

		public void SelectDone()
		{
			// get selected edge
			TopTools_MapOfShape extractedEdgeSet = GetSelectedEdge();
			if( extractedEdgeSet.Size() == 0 ) {
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
			m_CADManager.AddPath( pathWireList, m_EdgeFaceMap );
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
			foreach( var faceAISPair in m_EdgeAISPairList ) {
				Quantity_Color color = new Quantity_Color();
				faceAISPair.AIS.Color( ref color );
				if( color.Name() == SelectViewHelper.COLOR_SELECTED ) {
					selectedEdgeSet.Add( faceAISPair.Edge );
				}
			}
			return selectedEdgeSet;
		}

		List<TopoDS_Shape> m_FaceGroupList;
		TopTools_IndexedDataMapOfShapeListOfShape m_EdgeFaceMap;
		List<EdgeHandle> m_EdgeAISPairList;
		List<List<TopoDS_Edge>> m_FreeBoundWireList;
		List<AIS_Shape> m_FaceGroupAISList;
	}
}
