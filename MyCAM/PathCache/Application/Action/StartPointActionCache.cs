using MyCAM.Data;
using System.Collections.Generic;

namespace MyCAM.PathCache
{
	internal class StartPointActionCache : IStartPointActionCache
	{
		readonly IPathCache m_PathCache;

		public StartPointActionCache( IPathCache pathCache )
		{
			m_PathCache = pathCache;
		}

		// TODO：when ToolVecAction separate from StartPointAction, the type of interface in this method need to be changed.(simplify) 
		public IReadOnlyList<ISetToolVecPoint> StartPointList
		{
			get
			{
				return m_PathCache.StartPointList;
			}
		}
	}
}
