using MyCAM.Data;
using OCC.AIS;
using OCC.Aspect;
using OCC.BRepAdaptor;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.Prs3d;
using OCC.Quantity;
using OCC.TopAbs;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using OCCViewer;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class WireDiscreteVertexAction : KeyMouseActionBase
	{
		public WireDiscreteVertexAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
			m_DiscretePoints = new List<gp_Pnt>();
			m_VertexMap = new TopTools_DataMapOfShapeInteger();
			m_UsedPointIndices = new HashSet<int>();
		}

		public override EditActionType ActionType
		{
			get { return EditActionType.WireDiscreteVertex; }
		}

		public override void Start()
		{
			base.Start();
			m_Viewer.GetAISContext().ClearSelected( true );
			m_TreeView.Enabled = false;

			foreach( var partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ partID ].AISHandle, (int)AISActiveMode.Edge );
			}
		}

		public override void End()
		{
			ClearDiscretePoints();
			m_Viewer.GetAISContext().ClearSelected( false );
			m_Viewer.UpdateView();
			m_TreeView.Enabled = true;
			m_ViewManager.DeactiveAll();
			base.End();
		}

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			if( e.Button != MouseButtons.Left ) {
				return;
			}

			TopoDS_Shape selectedShape = GetSelectedShape();
			if( selectedShape == null || selectedShape.IsNull() ) {
				return;
			}

			if( TryHandleDiscretePointClick( selectedShape ) ) {
				return;
			}

			// try to handle click on edge to create new discrete points
			TryHandleEdgeClick( selectedShape );
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		TopoDS_Shape GetSelectedShape()
		{
			m_Viewer.Select( AIS_SelectionScheme.AIS_SelectionScheme_Replace );
			if( m_Viewer.GetAISContext().NbSelected() == 0 ) {
				return null;
			}

			m_Viewer.GetAISContext().InitSelected();
			if( !m_Viewer.GetAISContext().MoreSelected() ) {
				return null;
			}

			return m_Viewer.GetAISContext().SelectedShape();
		}

		bool TryHandleDiscretePointClick( TopoDS_Shape selectedShape )
		{
			if( m_DiscretePointAIS == null || selectedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				return false;
			}

			int? index = GetClickedDiscretePointIndex( TopoDS.ToVertex( selectedShape ) );
			if( index == null ) {
				return false;
			}

			// avoid creating duplicate feature points for the same discrete point
			if( !m_UsedPointIndices.Add( index.Value ) ) {
				return true;
			}

			CreateVertexAtPoint( m_DiscretePoints[ index.Value ] );
			return true;
		}

		bool TryHandleEdgeClick( TopoDS_Shape selectedShape )
		{
			if( selectedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_EDGE ) {
				return false;
			}

			TopoDS_Edge selectedEdge = TopoDS.ToEdge( selectedShape );

			// skip only if the same edge is already discretized and still displayed
			bool sameEdge = m_CurrentEdge != null && !m_CurrentEdge.IsNull() && selectedEdge.IsSame( m_CurrentEdge );
			if( sameEdge && m_DiscretePointAIS != null ) {
				return false;
			}

			ClearDiscretePoints();
			m_CurrentEdge = selectedEdge;
			DiscretizeEdge( selectedEdge );
			return true;
		}

		void DiscretizeEdge( TopoDS_Edge edge )
		{
			CalculateDiscretePoints( edge );
			CreateSelectedEdgeHighlight();
			CreateDiscretePointVisualization();
		}

		void CalculateDiscretePoints( TopoDS_Edge edge )
		{
			BRepAdaptor_Curve curveAdaptor = new BRepAdaptor_Curve( edge );
			List<double> paramList = GeometryTool.DiscretizeEdgeParameters( curveAdaptor, edge, DISCRETE_MAX_DEFLECTION, DISCRETE_MAX_LENGTH );

			foreach( double param in paramList ) {
				AddPointIfNotDuplicate( curveAdaptor.Value( param ) );
			}
		}

		void AddPointIfNotDuplicate( gp_Pnt point )
		{
			if( m_DiscretePoints.Count > 0 ) {
				gp_Pnt lastPoint = m_DiscretePoints[ m_DiscretePoints.Count - 1 ];
				if( lastPoint.Distance( point ) < POINT_TOLERANCE ) {
					return;
				}
			}
			m_DiscretePoints.Add( point );
		}

		void CreateSelectedEdgeHighlight()
		{
			if( m_CurrentEdge == null || m_CurrentEdge.IsNull() ) {
				return;
			}

			// green highlight for selected edge
			m_SelectedEdgeAIS = new AIS_Shape( m_CurrentEdge );
			m_SelectedEdgeAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ) );
			m_SelectedEdgeAIS.SetWidth( SELECT_EDGE_WIDTH );

			m_Viewer.GetAISContext().Display( m_SelectedEdgeAIS, false );
			m_Viewer.GetAISContext().Deactivate( m_SelectedEdgeAIS );
		}

		void CreateDiscretePointVisualization()
		{
			if( m_DiscretePoints.Count == 0 ) {
				return;
			}

			// create a vertex for each discrete point and store the mapping from vertex to index
			List<TopoDS_Shape> vertexList = new List<TopoDS_Shape>();
			for( int i = 0; i < m_DiscretePoints.Count; i++ ) {
				TopoDS_Vertex vertex = new BRepBuilderAPI_MakeVertex( m_DiscretePoints[ i ] ).Vertex();
				vertexList.Add( vertex );
				m_VertexMap.Bind( vertex, i );
			}

			// pack vertices into a compound so they are selectable without drawing any connecting line
			TopoDS_Shape compound = ShapeTool.MakeCompound( vertexList );
			m_DiscretePointAIS = new AIS_Shape( compound );

			// use a screen-space ball marker: fixed pixel size regardless of zoom, no extra 3D geometry needed
			Prs3d_Drawer drawer = m_DiscretePointAIS.Attributes();
			drawer.SetPointAspect( new Prs3d_PointAspect(
				Aspect_TypeOfMarker.Aspect_TOM_BALL,
				new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ),
				DISCRETE_POINT_MARKER_SIZE ) );
			m_DiscretePointAIS.SetAttributes( drawer );

			m_Viewer.GetAISContext().Display( m_DiscretePointAIS, false );
			m_Viewer.GetAISContext().Activate( m_DiscretePointAIS, (int)AISActiveMode.Vertex );

			m_Viewer.UpdateView();
		}

		int? GetClickedDiscretePointIndex( TopoDS_Vertex selectedVertex )
		{
			if( selectedVertex == null || selectedVertex.IsNull() ) {
				return null;
			}

			// must check IsBound before Find (Find throws exception if not bound)
			if( !m_VertexMap.IsBound( selectedVertex ) ) {
				return null;
			}

			return m_VertexMap.Find( selectedVertex );
		}

		void CreateVertexAtPoint( gp_Pnt point )
		{
			BRepBuilderAPI_MakeVertex makeVertex = new BRepBuilderAPI_MakeVertex( point );
			if( !makeVertex.IsDone() ) {
				return;
			}
			TopoDS_Vertex vertex = makeVertex.Vertex();
			m_DataManager.AddReferenceFeature( vertex );
		}

		void ClearDiscretePoints()
		{
			m_DiscretePoints.Clear();
			m_VertexMap.Clear();
			m_UsedPointIndices.Clear();

			// remove selected edge AIS
			if( m_SelectedEdgeAIS != null ) {
				m_Viewer.GetAISContext().Remove( m_SelectedEdgeAIS, false );
				m_SelectedEdgeAIS = null;
			}

			// remove discrete point AIS
			if( m_DiscretePointAIS != null ) {
				m_Viewer.GetAISContext().Remove( m_DiscretePointAIS, false );
				m_DiscretePointAIS = null;
			}

			m_CurrentEdge = null;
		}

		TopoDS_Edge m_CurrentEdge;
		List<gp_Pnt> m_DiscretePoints;
		TopTools_DataMapOfShapeInteger m_VertexMap;  // vertex → index
		HashSet<int> m_UsedPointIndices;  // indices already turned into feature points
		AIS_Shape m_SelectedEdgeAIS;
		AIS_Shape m_DiscretePointAIS;

		const double DISCRETE_MAX_DEFLECTION = 0.05;  // max chord height error (mm)
		const double DISCRETE_MAX_LENGTH = 0.5;  // max segment length (mm)
		const double POINT_TOLERANCE = 1e-3;  // min distance (mm)

		const double SELECT_EDGE_WIDTH = 3.0;
		const double DISCRETE_POINT_MARKER_SIZE = 1.0;    // fixed on-screen marker size (px), independent of zoom
	}
}
