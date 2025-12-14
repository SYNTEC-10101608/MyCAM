using OCC.gp;
using System.Linq;

namespace MyCAM.PathCache
{
	internal class MainPathStartPointCache : IMainPathStartPointCache
	{
		private readonly IPathCache m_PathCache;

		public MainPathStartPointCache( IPathCache pathCache )
		{
			m_PathCache = pathCache;
		}

		public gp_Pnt GetMainPathStartCAMPoint()
		{
			if( m_PathCache.MainPathPointList.Count > 0 ) {
				return m_PathCache.MainPathPointList.First().Point;
			}
			return null;
		}
	}
}
