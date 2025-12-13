using MyCAM.Data;
using System.Linq;

namespace MyCAM.PathCache
{
	internal class OrientationCache : IOrientationCache
	{
		private readonly IPathCache m_PathCache;
		private readonly CraftData m_CraftData;

		public OrientationCache( IPathCache pathCache, CraftData craftData )
		{
			m_PathCache = pathCache;
			m_CraftData = craftData;
		}

		public IOrientationPoint LeadInStartPoint
		{
			get
			{
				if( m_PathCache.LeadInPointList.Count > 0 && m_CraftData.LeadData.LeadIn.Length > 0 ) {
					return m_PathCache.LeadInPointList.First();
				}
				return null;
			}
		}

		public IOrientationPoint LeadOutEndPoint
		{
			get
			{
				if( m_PathCache.LeadOutPointList.Count > 0 && m_CraftData.LeadData.LeadOut.Length > 0 ) {
					return m_PathCache.LeadOutPointList.Last();
				}
				return null;
			}
		}

		public IOrientationPoint MainPathStartPoint
		{
			get
			{
				if( m_PathCache.MainPathPointList.Count > 0 ) {
					return m_PathCache.MainPathPointList.First();
				}
				return null;
			}
		}
	}
}
