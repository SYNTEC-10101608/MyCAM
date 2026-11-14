using System.Collections.Generic;
using System.Linq;
using MyCAM.PathCache;

namespace MyCAM.Data
{
	public class FlyCutGroupPathObject : GroupPathObject
	{
		public FlyCutGroupPathObject( string szUID, List<ContourPathObject> pathList ) : base( szUID )
		{
			PathList = new List<PathObject>( pathList );
			m_CraftData = new CraftData();
			m_OriginalCraftData = pathList.ToDictionary( path => path, path => path.CraftData.Clone() );
			foreach( ContourPathObject contour in pathList ) {
				contour.SetCraftData( contour.CraftData.Clone() );
			}
			m_FlyCutCache = new FlyCutGroupPathCache( pathList, m_CraftData );
		}

		public FlyCutGroupPathObject( string szUID, List<ContourPathObject> pathList,
			CraftData craftData, Dictionary<ContourPathObject, CraftData> originalCraftData ) : base( szUID )
		{
			PathList = new List<PathObject>( pathList );

			m_CraftData = craftData;
			m_OriginalCraftData = originalCraftData;

			m_FlyCutCache = new FlyCutGroupPathCache( PathList.OfType<ContourPathObject>().ToList(), m_CraftData );
		}

		internal Dictionary<ContourPathObject, CraftData> GetOriginalCraftDataSnapshots()
		{
			return m_OriginalCraftData?.ToDictionary( pair => pair.Key, pair => pair.Value.Clone() );
		}

		public override PathType PathType
		{
			get
			{
				return PathType.FlyCutGroup;
			}
		}

		public override void Dispose()
		{
			if( m_IsDisposed ) {
				return;
			}
			m_FlyCutCache.Dispose();
			if( m_OriginalCraftData != null ) {
				foreach( var entry in m_OriginalCraftData ) {
					entry.Key.SetCraftData( entry.Value.Clone() );
				}
			}
			m_IsDisposed = true;
		}

		public FlyCutGroupPathCache FlyCutCache
		{
			get
			{
				return m_FlyCutCache;
			}
		}

		FlyCutGroupPathCache m_FlyCutCache;
		readonly Dictionary<ContourPathObject, CraftData> m_OriginalCraftData;
		bool m_IsDisposed;
	}
}
