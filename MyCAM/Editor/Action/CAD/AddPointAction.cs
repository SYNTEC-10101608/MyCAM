using MyCAM.Data;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class AddPointAction : EditActionBase
	{
		public AddPointAction( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager,
			AddPointType addPointType )
			: base( viewer, treeView, cadManager, viewManager )
		{
			m_AddPointType = addPointType;
		}

		public override void Start()
		{
			base.Start();

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// disable tree view
			m_TreeView.Enabled = false;

			// activate edge slection mode
			foreach( var partID in m_CADManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				if( m_AddPointType == AddPointType.TwoVertexMidPoint ) {
					m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ partID ].AISHandle, (int)AISActiveMode.Vertex );
				}
				else if( m_AddPointType == AddPointType.EdgeMidPoint || m_AddPointType == AddPointType.CircArcCenter ) {
					m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ partID ].AISHandle, (int)AISActiveMode.Edge );
				}
			}
		}

		public override void End()
		{
			// clear selection
			m_Viewer.GetAISContext().ClearSelected( false );
			m_Viewer.UpdateView();

			// enable tree view
			m_TreeView.Enabled = true;

			// deactivate
			foreach( ViewObject viewObject in m_ViewManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Deactivate( viewObject.AISHandle );
			}
			base.End();
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.AddPoint;
			}
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			if( e.Button != MouseButtons.Left ) {
				return;
			}

			// get selection edge or vertex
			m_Viewer.GetAISContext().SelectDetected( AIS_SelectionScheme.AIS_SelectionScheme_Add );
			if( ( m_AddPointType == AddPointType.TwoVertexMidPoint && m_Viewer.GetAISContext().NbSelected() != 2 )
				|| m_Viewer.GetAISContext().NbSelected() == 0 ) {
				m_Viewer.UpdateView();
				return;
			}
			List<TopoDS_Shape> selectedShapeList = new List<TopoDS_Shape>();
			m_Viewer.GetAISContext().InitSelected();
			while( m_Viewer.GetAISContext().MoreSelected() ) {
				var shape = m_Viewer.GetAISContext().SelectedShape();
				if( shape.IsNull() ) {
					return;
				}
				selectedShapeList.Add( m_Viewer.GetAISContext().SelectedShape() );
				m_Viewer.GetAISContext().NextSelected();
			}

			// add the point
			bool isAdded = false;
			if( m_AddPointType == AddPointType.CircArcCenter || m_AddPointType == AddPointType.EdgeMidPoint ) {
				TopoDS_Shape theShape = selectedShapeList.First();
				if( theShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_EDGE ) {
					return;
				}
				TopoDS_Edge edge = TopoDS.ToEdge( selectedShapeList.First() );
				if( m_AddPointType == AddPointType.CircArcCenter ) {
					isAdded = AddCircArcCenter( edge );
				}
				else if( m_AddPointType == AddPointType.EdgeMidPoint ) {
					isAdded = AddEdgeMidPoint( edge );
				}
			}
			else if( m_AddPointType == AddPointType.TwoVertexMidPoint ) {
				TopoDS_Shape theShape1 = selectedShapeList.First();
				TopoDS_Shape theShape2 = selectedShapeList.Last();
				if( theShape1.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX || theShape2.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
					return;
				}
				TopoDS_Vertex v1 = TopoDS.ToVertex( selectedShapeList.First() );
				TopoDS_Vertex v2 = TopoDS.ToVertex( selectedShapeList.Last() );
				if( v1.IsEqual( v2 ) ) {
					return;
				}
				isAdded = AddTwoVertexMidPoint( v1, v2 );
			}

			// end action if the point is added
			if( isAdded ) {
				End();
			}
		}

		protected override void ViewerMouseMove( MouseEventArgs e )
		{
			// do nothing
		}

		protected override void ViewerMouseUp( MouseEventArgs e )
		{
			// do nothing
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		protected override void TreeViewAfterSelect( object sender, TreeViewEventArgs e )
		{
			// do nothing
		}

		protected override void TreeViewKeyDown( object sender, KeyEventArgs e )
		{
			// do nothing
		}

		bool AddCircArcCenter( TopoDS_Edge edge )
		{
			bool isValidCircle = GeometryTool.IsCircularArc( edge, out gp_Pnt center, out _, out _ );
			if( !isValidCircle ) {
				MessageBox.Show( "Bad Arc" );
				return false;
			}
			AddToManager( center );
			return true;
		}

		bool AddEdgeMidPoint( TopoDS_Edge edge )
		{
			bool isValidEdge = GeometryTool.GetEdgeMidPoint( edge, out gp_Pnt midPoint );
			if( !isValidEdge ) {
				MessageBox.Show( "Bad Edge" );
				return false;
			}
			AddToManager( midPoint );
			return true;
		}

		bool AddTwoVertexMidPoint( TopoDS_Vertex vertex1, TopoDS_Vertex vertex2 )
		{
			if( vertex1 == null || vertex2 == null ) {
				return false;
			}
			bool isValidPoint = GeometryTool.GetTwoVertexMidPoint( vertex1, vertex2, out gp_Pnt midPoint );
			if( !isValidPoint ) {
				MessageBox.Show( "Valid Point" );
				return false;
			}
			AddToManager( midPoint );
			return true;
		}

		void AddToManager( gp_Pnt pointToAdd )
		{
			// create the vertex to add
			BRepBuilderAPI_MakeVertex makeVertex = new BRepBuilderAPI_MakeVertex( pointToAdd );
			if( !makeVertex.IsDone() ) {
				return;
			}
			TopoDS_Vertex vertex = makeVertex.Vertex();
			m_CADManager.AddReferenceFeature( vertex );
		}

		AddPointType m_AddPointType;
	}
}
