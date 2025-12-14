using MyCAM.Data;
using System.Collections.Generic;

namespace MyCAM.PathCache
{
	internal class PathReverseCache : IPathReverseCache
	{
		private readonly CraftData m_CraftData;

		public PathReverseCache( CraftData craftData )
		{
			m_CraftData = craftData;
		}

		public bool IsPathReverse
		{
			get
			{
				return m_CraftData.IsPathReverse;
			}
		}
	}
}
