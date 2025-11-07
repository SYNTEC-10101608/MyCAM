using OCC.BRep;
using OCC.gp;
using OCC.TopExp;
using OCC.TopoDS;
using System.Collections.Generic;

namespace MyCAM.Data
{
	internal class ContourPathObject : PathObject
	{
		public ContourPathObject( DataManager dataManager, string szUID, TopoDS_Shape shapeData, List<PathEdge5D> pathDataList )
			: base( szUID, shapeData, PathType.Contour )
		{
			m_PathEdge5DList = pathDataList;

			TopoDS_Vertex startVertex = new TopoDS_Vertex();
			TopoDS_Vertex endVertex = new TopoDS_Vertex();
			TopExp.Vertices( TopoDS.ToWire( shapeData ), ref startVertex, ref endVertex );
			gp_Pnt startPoint = BRep_Tool.Pnt( TopoDS.ToVertex( startVertex ) );
			gp_Pnt endPoint = BRep_Tool.Pnt( TopoDS.ToVertex( endVertex ) );
			bool isClosed = startPoint.IsEqual( endPoint, 1e-3 );

			m_CraftData = new CraftData( szUID, isClosed );
			m_ContourCacheInfo = new ContourCacheInfo( dataManager, szUID, m_PathEdge5DList, m_CraftData );
		}

		public List<PathEdge5D> PathDataList
		{
			get
			{
				return m_PathEdge5DList;
			}
		}

		public override CraftData CraftData
		{
			get
			{
				return m_CraftData;
			}
		}

		public ContourCacheInfo ContourCacheInfo
		{
			get
			{
				return m_ContourCacheInfo;
			}
		}

		public override void DoTransform( gp_Trsf transform )
		{
			base.DoTransform( transform );
			m_ContourCacheInfo.Transform( transform );
		}

		List<PathEdge5D> m_PathEdge5DList;
		CraftData m_CraftData;
		ContourCacheInfo m_ContourCacheInfo;
	}
}