using System;
using System.Collections.Generic;
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
		protected IndexSelectAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, string pathID )
			: base( dataManager, viewer, treeView, viewManager )
		{
			if( string.IsNullOrEmpty( pathID ) ) {
				throw new ArgumentNullException( "IndexSelectAction constructing argument null" );
			}
			m_PathID = pathID;

			if( !m_DataManager.ObjectMap.ContainsKey( m_PathID ) ) {
				throw new ArgumentException( "IndexSelectAction constructing argument invalid" );
			}
			m_ContourPathObject = (ContourPathObject)m_DataManager.ObjectMap[ m_PathID ];
			m_VertexMap = new TopTools_DataMapOfShapeSegmentIndex();
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

		protected SegmentPointIndex GetSelectIndex( out TopoDS_Shape selectedShape )
		{
			selectedShape = new TopoDS_Shape();
			m_Viewer.Select();
			m_Viewer.GetAISContext().InitSelected();
			if( !m_Viewer.GetAISContext().MoreSelected() ) {
				return new SegmentPointIndex( -1, -1 );
			}
			selectedShape = m_Viewer.GetAISContext().SelectedShape();
			if( selectedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				return new SegmentPointIndex( -1, -1 );
			}
			TopoDS_Vertex selectedVertex = TopoDS.ToVertex( selectedShape );
			if( m_VertexMap.IsBound( selectedVertex ) == false ) {
				return new SegmentPointIndex( -1, -1 );
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
			int nCount = 0;
			List<ICADSegmentElement> cadSegmentList = m_ContourPathObject.CADSegmentList;

			// each segment
			for( int i = 0; i < cadSegmentList.Count; i++ ) {

				// each point on the segment
				for( int j = 0; j < cadSegmentList[ i ].PointList.Count; j++ ) {

					// each segment start point do not need to draw
					if( j == 0 ) {
						continue;
					}
					BRepBuilderAPI_MakeVertex vertexMaker = new BRepBuilderAPI_MakeVertex( cadSegmentList[ i ].PointList[ j ].Point );
					polygonMaker.Add( vertexMaker.Vertex() );
					nCount++;
					m_VertexMap.Bind( vertexMaker.Vertex(), new SegmentPointIndex( i, j ) );

				}
			}
			if( polygonMaker.IsDone() == false ) {
				return;
			}
			TopoDS_Wire wire = polygonMaker.Wire();
			m_SelectedPointAIS = new AIS_Shape( wire );

			// set invisible
			m_SelectedPointAIS.SetWidth( 1e-3 );
		}

		protected string m_PathID;
		protected ContourPathObject m_ContourPathObject;

		// map point on view to index on CAMData
		protected TopTools_DataMapOfShapeSegmentIndex m_VertexMap;
		protected AIS_Shape m_SelectedPointAIS;

		// flag to check is pause mode or not
		protected bool IsPausedSelectMode = false;
	}

	public class TopTools_DataMapOfShapeSegmentIndex
	{
		readonly TopTools_DataMapOfShapeInteger m_ShapeMap = new TopTools_DataMapOfShapeInteger();
		readonly Dictionary<int, SegmentPointIndex> m_IDToPair = new Dictionary<int, SegmentPointIndex>();
		readonly Dictionary<SegmentPointIndex, int> m_PairToID = new Dictionary<SegmentPointIndex, int>();

		int m_CurrentID = 1;

		public void Bind( TopoDS_Shape shape, SegmentPointIndex segmentPoint )
		{
			if( m_ShapeMap.IsBound( shape ) ) {
				throw new InvalidOperationException( "Already bounded" );
			}


			if( m_PairToID.ContainsKey( segmentPoint ) ) {
				throw new InvalidOperationException( $"第{segmentPoint.SegIdx}段 第{segmentPoint.PntIdx}Index已經綁訂過了" );
			}

			int UniqueID = m_CurrentID++;
			m_ShapeMap.Bind( shape, UniqueID );
			m_IDToPair[ UniqueID ] = segmentPoint;
			m_PairToID[ segmentPoint ] = UniqueID;
		}

		public SegmentPointIndex Find( TopoDS_Shape shape )
		{
			if( !m_ShapeMap.IsBound( shape ) ) {
				throw new KeyNotFoundException( "Shape not bound." );
			}

			int ID = m_ShapeMap.Find( shape );
			return m_IDToPair[ ID ];
		}

		public bool IsBound( TopoDS_Shape shape ) => m_ShapeMap.IsBound( shape );

		public void UnBind( TopoDS_Shape shape )
		{
			if( !m_ShapeMap.IsBound( shape ) ) {
				return;
			}

			int id = m_ShapeMap.Find( shape );
			SegmentPointIndex pair = m_IDToPair[ id ];

			m_ShapeMap.UnBind( shape );
			m_IDToPair.Remove( id );
			m_PairToID.Remove( pair );
		}
		public void Clear()
		{
			m_ShapeMap.Clear();
			m_IDToPair.Clear();
			m_PairToID.Clear();
			m_CurrentID = 1;
		}
	}
}
