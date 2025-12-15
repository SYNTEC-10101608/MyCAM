using MyCAM.Data;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	internal class LeadCache : ILeadCache
	{
		private readonly IPathCache m_PathCache;
		private readonly CraftData m_CraftData;

		public LeadCache( IPathCache pathCache, CraftData craftData )
		{
			m_PathCache = pathCache;
			m_CraftData = craftData;
		}

		public IReadOnlyList<IProcessPoint> LeadInCAMPointList
		{
			get
			{
				return m_PathCache.LeadInPointList.Cast<IProcessPoint>().ToList();
			}
		}

		public IReadOnlyList<IProcessPoint> LeadOutCAMPointList
		{
			get
			{
				return m_PathCache.LeadOutPointList.Cast<IProcessPoint>().ToList();
			}
		}

		public LeadData LeadData
		{
			get
			{
				return m_CraftData.LeadData;
			}
		}
	}
}
