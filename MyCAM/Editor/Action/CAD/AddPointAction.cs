using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MyCAM.App;
using MyCAM.Data;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;

namespace MyCAM.Editor
{
	internal enum AddPointType
	{
		CircArcCenter = 0,
		EdgeMidPoint = 1,
		TwoVertexMidPoint = 2,
		BoundingBoxCenter = 3,
	}

	internal class AddPointAction : KeyMouseActionBase
	{
		public AddPointAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager,
			AddPointType addPointType )
			: base( dataManager, viewer, treeView, viewManager )
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
			foreach( var partID in m_DataManager.PartIDList ) {
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
			m_ViewManager.DeactiveAll();
			base.End();
		}

		public override EditActionType ActionType
		{
			get
			{
				switch( m_AddPointType ) {
					case AddPointType.CircArcCenter:
						return EditActionType.AddPoint_CircArcCenter;
					case AddPointType.EdgeMidPoint:
						return EditActionType.AddPoint_EdgeMidPoint;
					default:
						return EditActionType.AddPoint_TwoVertexMidPoint;
				}
			}
		}

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			if( e.Button != MouseButtons.Left ) {
				return;
			}

			// get selection edge or vertex
			m_Viewer.Select( AIS_SelectionScheme.AIS_SelectionScheme_Add );
			if( ( m_AddPointType == AddPointType.TwoVertexMidPoint && m_Viewer.GetAISContext().NbSelected() != 2 )
				|| m_Viewer.GetAISContext().NbSelected() == 0 ) {
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

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		bool AddCircArcCenter( TopoDS_Edge edge )
		{
			bool isValidCircle = GeometryTool.IsCircularArc( edge, out gp_Pnt center, out _, out _ );
			if( !isValidCircle ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "NotCircularArc", NOT_CIRCULAR_ARC ), MyApp.NoticeType.Warning );
				return false;
			}
			AddToManager( center );
			return true;
		}

		bool AddEdgeMidPoint( TopoDS_Edge edge )
		{
			bool isValidEdge = GeometryTool.GetEdgeMidPoint( edge, out gp_Pnt midPoint );
			if( !isValidEdge ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "InvalidEdge", INVALID_EDGE ), MyApp.NoticeType.Warning );
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
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "InvalidPoint", INVALID_POINT ), MyApp.NoticeType.Warning );
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
			m_DataManager.AddReferenceFeature( vertex );
		}

		AddPointType m_AddPointType;

		const string NOT_CIRCULAR_ARC = "[操作提醒]選取的邊不是圓弧";
		const string INVALID_EDGE = "[操作提醒]選取的邊無效";
		const string INVALID_POINT = "[操作提醒]選取的點位無效";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "AddPointAction" );
	}
}
