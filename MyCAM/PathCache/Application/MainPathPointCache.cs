using MyCAM.Data;
using OCC.gp;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	internal class MainPathPointCache : IMainPathCache
	{
		private readonly IPathCache m_PathCache;

		public MainPathPointCache( IPathCache pathCache )
		{
			m_PathCache = pathCache;
		}

		public IReadOnlyList<IProcessPoint> MainPathPointList
		{
			get
			{
				return m_PathCache.MainPathPointList.Cast<IProcessPoint>().ToList();
			}
		}

		public IReadOnlyList<gp_Pnt> GetMainPathGPPntList()
		{
			if( m_PathCache.MainPathCADPointList.Count > 0 ) {
				return m_PathCache.MainPathCADPointList.Select( p => p.Point ).ToList();
			}
			return null;
		}
	}
}
