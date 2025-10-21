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
		protected IndexSelectAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager,
			CAMData camData )
			: base( dataManager, viewer, treeView, viewManager )
		{
			if( camData == null ) {
				throw new ArgumentNullException( "PathIndexSelectAction constructing argument camData null" );
			}
			m_CAMData = camData;
			m_VertexMap = new TopTools_DataMapOfShapeInteger();
			m_VertexMap_int_int = new TopTools_DataMapOfShapeIntPair();
			MakeSelectPoint_New();
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

		protected (int, int) GetSelectIndex( out TopoDS_Shape selectedShape )
		{
			selectedShape = new TopoDS_Shape();
			m_Viewer.Select();
			m_Viewer.GetAISContext().InitSelected();
			if( !m_Viewer.GetAISContext().MoreSelected() ) {
				return (-1, -1);
			}
			selectedShape = m_Viewer.GetAISContext().SelectedShape();
			if( selectedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				return (-1, -1);
			}
			TopoDS_Vertex selectedVertex = TopoDS.ToVertex( selectedShape );
			if( m_VertexMap_int_int.IsBound( selectedVertex ) == false ) {
				return (-1, -1);
			}
			return m_VertexMap_int_int.FindPair( selectedVertex );
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

		void MakeSelectPoint_New()
		{
			// build wire from cad points
			BRepBuilderAPI_MakePolygon polygonMaker = new BRepBuilderAPI_MakePolygon();
			int nCount = 0;
			for( int i = 0; i < m_CAMData.CADSegmentList.Count; i++ ) {
				for( int j = 0; j < m_CAMData.CADSegmentList[ i ].PointList.Count; j++ ) {

					// 後面幾段的起點不用畫
					if( j == 0 && i != 0 ) {
						continue;
					}
					BRepBuilderAPI_MakeVertex vertexMaker = new BRepBuilderAPI_MakeVertex( m_CAMData.CADSegmentList[ i ].PointList[ j ].Point );
					polygonMaker.Add( vertexMaker.Vertex() );
					nCount++;
					m_VertexMap_int_int.Bind( vertexMaker.Vertex(), i, j );
				}
			}
			if( polygonMaker.IsDone() == false ) {
				return;
			}
			Console.WriteLine( nCount );
			TopoDS_Wire wire = polygonMaker.Wire();
			m_SelectedPointAIS = new AIS_Shape( wire );

			// set invisible
			m_SelectedPointAIS.SetWidth( 1e-3 );
		}

		protected CAMData m_CAMData;

		// map point on view to index on CAMData
		protected TopTools_DataMapOfShapeInteger m_VertexMap;
		TopTools_DataMapOfShapeIntPair m_VertexMap_int_int;
		protected AIS_Shape m_SelectedPointAIS;

		// flag to check is pause mode or not
		protected bool IsPausedSelectMode = false;
	}
}


public class TopTools_DataMapOfShapeIntPair
{
	readonly TopTools_DataMapOfShapeInteger m_ShapeMap = new TopTools_DataMapOfShapeInteger();
	readonly Dictionary<int, (int SegmentIndex, int PointIndex)> m_IDToPair = new Dictionary<int, (int, int)>();
	readonly Dictionary<(int, int), int> m_PairToID = new Dictionary<(int, int), int>();

	int m_CurrentID = 1;


	// 綁定一個 Shape 對應一組 (int,int)
	public void Bind( TopoDS_Shape shape, int segmentIndex, int pointIndex )
	{
		if( m_ShapeMap.IsBound( shape ) ) {
			throw new InvalidOperationException( "已經綁訂過了" );
		}


		if( m_PairToID.ContainsKey( (segmentIndex, pointIndex) ) ) {
			throw new InvalidOperationException( $"第{segmentIndex}段 第{pointIndex}Index已經綁訂過了" );
		}


		int UniqueID = m_CurrentID++;
		m_ShapeMap.Bind( shape, UniqueID );
		m_IDToPair[ UniqueID ] = (segmentIndex, pointIndex);
		m_PairToID[ (segmentIndex, pointIndex) ] = UniqueID;
	}

	public (int segmentIndex, int pointIndex) FindPair( TopoDS_Shape shape )
	{
		if( !m_ShapeMap.IsBound( shape ) ) {
			throw new KeyNotFoundException( "Shape not bound." );
		}

		int ID = m_ShapeMap.Find( shape );
		return m_IDToPair[ ID ];
	}

	// 判斷這個點是否已綁定
	public bool IsBound( TopoDS_Shape shape ) => m_ShapeMap.IsBound( shape );


	/// 移除指定 Shape 的綁定
	public void UnBind( TopoDS_Shape shape )
	{
		if( !m_ShapeMap.IsBound( shape ) ) {
			return;
		}

		int id = m_ShapeMap.Find( shape );
		(int, int) pair = m_IDToPair[ id ];

		m_ShapeMap.UnBind( shape );
		m_IDToPair.Remove( id );
		m_PairToID.Remove( pair );
	}

	// 清空所有綁定
	public void Clear()
	{
		m_ShapeMap.Clear();
		m_IDToPair.Clear();
		m_PairToID.Clear();
		m_CurrentID = 1;
	}
}
