using OCC.gp;
using System.Collections.Generic;

namespace MyCAM.PathCache
{
	internal class MainPathPointRendererCache : IMainPathRendererCache
	{
		readonly IPathCache m_PathCache;
		List<gp_Pnt> m_MainPathPointList = null;

		public MainPathPointRendererCache( IPathCache pathCache )
		{
			m_PathCache = pathCache;
			m_MainPathPointList = new List<gp_Pnt>();

			for( int i = 0; i < m_PathCache.MainPathPointList.Count; i++ ) {
				gp_Pnt point = m_PathCache.MainPathPointList[ i ].Point;
				if( point == null ) {
					continue;
				}
				m_MainPathPointList.Add( new gp_Pnt( point.X(), point.Y(), point.Z() ) );
			}
		}

		public IReadOnlyList<gp_Pnt> MainPathPointList
		{
			get
			{
				return m_MainPathPointList;
			}
		}
	}
}
