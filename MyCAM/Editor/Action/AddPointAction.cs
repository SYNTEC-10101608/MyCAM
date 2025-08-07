using MyCAM.Data;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class AddPointAction : CADACtionBase
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
				m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ partID ].AISHandle, (int)AISActiveMode.Edge );
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
				m_Viewer.GetAISContext().Deactivate();
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

			// get selection edge
			m_Viewer.GetAISContext().SelectDetected();
			m_Viewer.GetAISContext().InitSelected();
			if( !m_Viewer.GetAISContext().MoreSelected() ) {
				return;
			}
			TopoDS_Shape selectedShape = m_Viewer.GetAISContext().SelectedShape();

			// validate the edge
			if( selectedShape == null || selectedShape.IsNull() ) {
				return;
			}
			if( selectedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_EDGE ) {
				return;
			}
			TopoDS_Edge edge = TopoDS.ToEdge( selectedShape );

			// add the point
			bool isAdded = false;
			if( m_AddPointType == AddPointType.CircArcCenter ) {
				isAdded = AddCircArcCenter( edge );
			}
			else if( m_AddPointType == AddPointType.EdgeMidPoint ) {
				isAdded = AddEdgeMidPoint( edge );
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
