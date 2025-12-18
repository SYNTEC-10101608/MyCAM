namespace MyCAM.PathCache
{
	internal class StdPatternOverCutRestrictionCache : IStdPatternOverCutMaxinumCache
	{
		private readonly IStdPatternCache m_StdPatternCache;

		public StdPatternOverCutRestrictionCache( IStdPatternCache stdPatternCache )
		{
			m_StdPatternCache = stdPatternCache;
		}

		public double GetMaxinumOverCutLength()
		{
			return m_StdPatternCache.MaxOverCutLength;
		}
	}
}
