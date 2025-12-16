using MyCAM.Data;
using System.Linq;

namespace MyCAM.PathCache
{
	internal class ProcessPathStartEndCache : IProcessPathStartEndCache
	{
		protected readonly IPathCache m_PathCache;
		protected readonly CraftData m_CraftData;

		public ProcessPathStartEndCache( IPathCache pathCache, CraftData craftData )
		{
			m_PathCache = pathCache;
			m_CraftData = craftData;
		}

		public IProcessPoint GetProcessStartPoint()
		{
			CAMPoint camPoint = null;
			if( m_PathCache.LeadInPointList.Count > 0 && ( m_CraftData.LeadData.LeadIn.StraightLength > 0 || m_CraftData.LeadData.LeadIn.ArcLength > 0 ) ) {
				camPoint = m_PathCache.LeadInPointList.First().Clone();
			}
			else if( m_PathCache.MainPathPointList.Count > 0 ) {
				camPoint = m_PathCache.MainPathPointList.First().Clone();
			}
			return camPoint;
		}

		public IProcessPoint GetProcessEndPoint()
		{
			CAMPoint camPoint = null;
			if( m_PathCache.LeadOutPointList.Count > 0 && ( m_CraftData.LeadData.LeadOut.StraightLength > 0 || m_CraftData.LeadData.LeadOut.ArcLength > 0 ) ) {
				camPoint = m_PathCache.LeadOutPointList.Last().Clone();
			}
			else if( m_PathCache.OverCutPointList.Count > 0 && m_CraftData.OverCutLength > 0 ) {
				camPoint = m_PathCache.OverCutPointList.Last().Clone();
			}
			else if( m_PathCache.MainPathPointList.Count > 0 ) {
				camPoint = m_PathCache.MainPathPointList.Last().Clone();
			}
			return camPoint;
		}
	}
}
