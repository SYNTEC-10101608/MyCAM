using MyCAM.Data;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	internal class StartPointRendererCache : IStartPointRendererCache
	{
		private readonly IPathCache m_PathCache;

		public StartPointRendererCache( IPathCache pathCache )
		{
			m_PathCache = pathCache;
		}

		// TODO¡Gthis method not only for renderer, it alse be used for post module.
		public IProcessPoint GetStartCAMPoint()
		{
			if( m_PathCache.StartPointList.Count > 0 ) {
				return m_PathCache.StartPointList.First();
			}
			return null;
		}

		public IReadOnlyList<IProcessPoint> StartPointList
		{
			get
			{
				return m_PathCache.StartPointList.Cast<IProcessPoint>().ToList();
			}
		}
	}
}
