using MyCAM.Data;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	internal class OverCutCache : IOverCutCache
	{
		private readonly IPathCache m_PathCache;
		private readonly CraftData m_CraftData;

		public OverCutCache( IPathCache pathCache, CraftData craftData )
		{
			m_PathCache = pathCache;
			m_CraftData = craftData;
		}

		public IReadOnlyList<IProcessPoint> OverCutCAMPointList
		{
			get
			{
				return m_PathCache.OverCutPointList.Cast<IProcessPoint>().ToList();
			}
		}

		public double OverCutLength
		{
			get
			{
				return m_CraftData.OverCutLength;
			}
		}
	}
}
