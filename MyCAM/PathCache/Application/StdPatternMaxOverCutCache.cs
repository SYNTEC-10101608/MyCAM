namespace MyCAM.PathCache
{
	internal class StdPatternMaxOverCutCache : IStdPatternMaxOverCutCache
	{
		private readonly IStdPatternCache m_StdPatternCache;

		public StdPatternMaxOverCutCache( IStdPatternCache stdPatternCache )
		{
			m_StdPatternCache = stdPatternCache;
		}

		public double GetMaxOverCutLength()
		{
			return m_StdPatternCache.MaxOverCutLength;
		}
	}
}
