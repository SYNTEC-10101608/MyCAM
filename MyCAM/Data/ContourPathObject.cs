using OCC.TopoDS;
using System.Collections.Generic;

namespace MyCAM.Data
{
	internal class ContourPathObject : PathObject
	{
		public ContourPathObject( string szUID, TopoDS_Shape shapeData, List<PathEdge5D> pathDataList, CraftData craftData )
			: base( szUID, shapeData, PathType.Contour )
		{
			m_PathEdge5DList = pathDataList;
			m_CraftData = craftData;
			m_ContourCacheInfo = new ContourCacheInfo( szUID, m_PathEdge5DList, craftData );
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

		List<PathEdge5D> m_PathEdge5DList;
		CraftData m_CraftData = new CraftData();
		ContourCacheInfo m_ContourCacheInfo;
	}
}