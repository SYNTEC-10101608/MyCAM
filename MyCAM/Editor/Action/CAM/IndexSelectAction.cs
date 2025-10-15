using System;
using System.Windows.Forms;
using MyCAM.Data;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.TopAbs;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using OCCViewer;

namespace MyCAM.Editor
{
	internal abstract class IndexSelectAction : KeyMouseActionBase
	{
		protected IndexSelectAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager,
			CAMData camData )
			: base( dataManager, viewer, treeView, viewManager )
		{
			if( camData == null ) {
				throw new ArgumentNullException( "PathIndexSelectAction constructing argument camData null" );
			}
			m_CAMData = camData;
			m_VertexMap = new TopTools_DataMapOfShapeInteger();
			MakeSelectPoint();
		}

		public override void Start()
		{
			base.Start();

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// disable tree view
			m_TreeView.Enabled = false;

			// deactivate all
			foreach( ViewObject viewObject in m_ViewManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Deactivate( viewObject.AISHandle );
			}

			// show select point
			ShowSelectPoint();
		}

		public override void End()
		{
			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// enable tree view
			m_TreeView.Enabled = true;

			// hide select point
			HideSelectPoint();
			base.End();
		}

		public void Pause()
		{
			// stop active mode
			HideSelectPoint();
			IsPausedSelectMode = true;
		}

		public void Resume()
		{
			// open active mode
			ShowSelectPoint();
			IsPausedSelectMode = false;
		}

		protected int GetSelectIndex( out TopoDS_Shape selectedShape )
		{
			selectedShape = new TopoDS_Shape();
			m_Viewer.Select();
			m_Viewer.GetAISContext().InitSelected();
			if( !m_Viewer.GetAISContext().MoreSelected() ) {
				return -1;
			}
			selectedShape = m_Viewer.GetAISContext().SelectedShape();
			if( selectedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				return -1;
			}
			TopoDS_Vertex selectedVertex = TopoDS.ToVertex( selectedShape );
			if( m_VertexMap.IsBound( selectedVertex ) == false ) {
				return -1;
			}
			return m_VertexMap.Find( selectedVertex );
		}

		protected void ShowSelectPoint()
		{
			m_Viewer.GetAISContext().Display( m_SelectedPointAIS, true );
			m_Viewer.GetAISContext().Activate( m_SelectedPointAIS, (int)AISActiveMode.Vertex );
		}

		protected void HideSelectPoint()
		{
			m_Viewer.GetAISContext().Remove( m_SelectedPointAIS, true );
		}

		void MakeSelectPoint()
		{
			// build wire from cad points
			BRepBuilderAPI_MakePolygon polygonMaker = new BRepBuilderAPI_MakePolygon();

			// add points to the polygon
			for( int i = 0; i < m_CAMData.CADPointList.Count; i++ ) {
				BRepBuilderAPI_MakeVertex vertexMaker = new BRepBuilderAPI_MakeVertex( m_CAMData.CADPointList[ i ].Point );
				polygonMaker.Add( vertexMaker.Vertex() );
				m_VertexMap.Bind( vertexMaker.Vertex(), i );
			}
			if( polygonMaker.IsDone() == false ) {
				return;
			}
			TopoDS_Wire wire = polygonMaker.Wire();
			m_SelectedPointAIS = new AIS_Shape( wire );

			// set invisible
			m_SelectedPointAIS.SetWidth( 1e-3 );
		}

		protected CAMData m_CAMData;

		// map point on view to index on CAMData
		protected TopTools_DataMapOfShapeInteger m_VertexMap;
		protected AIS_Shape m_SelectedPointAIS;

		// flag to check is pause mode or not
		protected bool IsPausedSelectMode = false;
	}
}
