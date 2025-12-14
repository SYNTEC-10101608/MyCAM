using MyCAM.Data;

namespace MyCAM.PathCache
{
	internal class TraverseDataCache : ProcessPathStartEndCache, ITraverseDataCache
	{
		public TraverseDataCache( IPathCache pathCache, CraftData craftData )
			: base( pathCache, craftData )
		{
		}

		public TraverseData TraverseData
		{
			get
			{
				return m_CraftData?.TraverseData ?? new TraverseData();
			}
		}
	}
}
