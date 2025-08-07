using MyCAM.Data;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using OCCViewer;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class PathIndexSelectAction : EditActionBase
	{
		public PathIndexSelectAction( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager,
			CAMData camData )
			: base( viewer, treeView, cadManager, viewManager )
		{
			if( camData == null ) {
				throw new ArgumentNullException( "PathIndexSelectAction constructing argument camData null" );
			}
			m_CAMData = camData;
			m_VertexMap = new TopTools_IndexedMapOfShape();
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

		protected int GetSelectIndex()
		{
			m_Viewer.Select();
			m_Viewer.GetAISContext().InitSelected();
			TopoDS_Shape selectedShape = m_Viewer.GetAISContext().SelectedShape();
			if( selectedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				return -1;
			}
			return m_VertexMap.FindIndex( selectedShape ) - 1;
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

			// add points to the polygon, we do not add the last point
			for( int i = 0; i < m_CAMData.CADPointList.Count - 1; i++ ) {
				polygonMaker.Add( m_CAMData.CADPointList[ i ].Point );
			}
			if( polygonMaker.IsDone() == false ) {
				return;
			}
			TopoDS_Wire wire = polygonMaker.Wire();

			// TODO: this is tricky but it works well
			TopExp.MapShapes( wire, TopAbs_ShapeEnum.TopAbs_VERTEX, ref m_VertexMap );
			m_SelectedPointAIS = new AIS_Shape( wire );

			// set invisible
			m_SelectedPointAIS.SetWidth( 1e-3 );
		}

		protected CAMData m_CAMData;

		// map point on view to index on CAMData
		protected TopTools_IndexedMapOfShape m_VertexMap;
		protected AIS_Shape m_SelectedPointAIS;

	}
}
