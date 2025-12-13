using MyCAM.Data;

namespace MyCAM.PathCache
{
	internal class StdPatternRefPointCache : IStdPatternRefPointCache
	{
		private readonly IStdPatternCache m_StdPatternCache;

		public StdPatternRefPointCache( IStdPatternCache stdPatternCache )
		{
			m_StdPatternCache = stdPatternCache;
		}

		public IProcessPoint GetProcessRefPoint()
		{
			return m_StdPatternCache.RefPoint;
		}
	}
}
