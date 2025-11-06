using System.Collections.Generic;

namespace MyCAM.Data
{
	internal class ContourPathObject : CraftData
	{
		public ContourPathObject( List<PathEdge5D> pathDataList, CraftData craftData )
		{
			m_PathEdge5DList = pathDataList;
			m_CraftData = craftData;
		}

		public List<PathEdge5D> PathDataList
		{
			get
			{
				return m_PathEdge5DList;
			}
		}

		public CraftData CraftData
		{
			get
			{
				return m_CraftData;
			}
		}

		List<PathEdge5D> m_PathEdge5DList;
		CraftData m_CraftData = new CraftData();
	}
}