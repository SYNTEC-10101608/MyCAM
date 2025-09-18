using MyCAM.Data;
using OCC.AIS;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class AddLineAction : EditActionBase
	{
		public AddLineAction( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager,
			AddLineType addLineType )
			: base( viewer, treeView, cadManager, viewManager )
		{
			m_AddLineType = addLineType;
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
				m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ partID ].AISHandle, (int)AISActiveMode.Vertex );
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
				return EditActionType.AddLine;
			}
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			if( e.Button != MouseButtons.Left ) {
				return;
			}
			m_Viewer.GetAISContext().SelectDetected( AIS_SelectionScheme.AIS_SelectionScheme_Add );
			if( m_Viewer.GetAISContext().NbSelected() != 2 ) {
				m_Viewer.UpdateView();
				return;
			}
			List<TopoDS_Shape> selectedShapeList = new List<TopoDS_Shape>();
			m_Viewer.GetAISContext().InitSelected();
			while( m_Viewer.GetAISContext().MoreSelected() ) {
				var shape = m_Viewer.GetAISContext().SelectedShape();
				if( shape.IsNull() || shape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
					return;
				}
				selectedShapeList.Add( m_Viewer.GetAISContext().SelectedShape() );
				m_Viewer.GetAISContext().NextSelected();
			}
			TopoDS_Vertex v1 = TopoDS.ToVertex( selectedShapeList.First() );
			TopoDS_Vertex v2 = TopoDS.ToVertex( selectedShapeList.Last() );
			if( v1.IsEqual( v2 ) ) {
				return;
			}
			bool isAdded = AddLine2PointsConnectLine( v1, v2 );
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

		bool AddLine2PointsConnectLine( TopoDS_Vertex vertex1, TopoDS_Vertex vertex2 )
		{
			if( vertex1 == null || vertex2 == null ) {
				return false;
			}
			bool isValidPoint = GeometryTool.CreateLineFromTwoVertex( vertex1, vertex2, out TopoDS_Edge edge );
			if( !isValidPoint ) {
				MessageBox.Show( "Invalid Point" );
				return false;
			}
			m_CADManager.AddReferenceFeature( edge );
			return true;
		}

		AddLineType m_AddLineType;
	}
}
