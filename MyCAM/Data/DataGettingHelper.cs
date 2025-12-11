using MyCAM.App;
using MyCAM.CacheInfo;

namespace MyCAM.Data
{
	public static class DataGettingHelper
	{
		static DataManager m_DataManager;

		internal static void Initialize( DataManager dataManager )
		{
			if( dataManager == null ) {
				return;
			}
			m_DataManager = dataManager;
		}

		public static bool GetGeomDataByID( string pathID, out IGeomData geomData )
		{
			geomData = null;
			if( !TryGetPathObject( pathID, out PathObject pathObject ) ) {
				return false;
			}

			// use unified GeomData property from StandardPatternBasedPathObject
			if( pathObject is StandardPatternBasedPathObject standardPatternPathObject ) {
				geomData = standardPatternPathObject.GeomData;
				return true;
			}
			else if( pathObject is ContourPathObject contourPathObject ) {
				geomData = contourPathObject.ContourGeomData;
				return true;
			}
			return false;
		}

		public static bool GetCraftDataByID( string szPathID, out CraftData craftData )
		{
			craftData = null;
			if( !TryGetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}

			if( pathObject.CraftData == null ) {
				return false;
			}

			if( pathObject.PathType == PathType.Contour ) {
				craftData = pathObject.CraftData;
			}
			return true;
		}

		public static bool GetContourPathObject( PathObject pathObject, out ContourPathObject contourPathObj )
		{
			contourPathObj = null;
			if( pathObject == null ) {
				return false;
			}

			// use unified ContourPathObject property from StandardPatternBasedPathObject
			if( pathObject is StandardPatternBasedPathObject standardPatternPathObject ) {
				contourPathObj = standardPatternPathObject.ContourPathObject;
				return true;
			}
			else if( pathObject is ContourPathObject contourPathObject ) {
				contourPathObj = contourPathObject;
				return true;
			}
			return false;
		}

		public static bool GetReferencePoint( string pathID, out IProcessPoint refPoint )
		{
			refPoint = null;
			if( !TryGetPathObject( pathID, out PathObject pathObject ) ) {
				return false;
			}

			// use unified CacheInfo property from StandardPatternBasedPathObject
			if( pathObject is StandardPatternBasedPathObject standardPatternPathObject ) {
				// cacheInfo implements IStandardPatternCacheInfo which has GetProcessRefPoint
				if( standardPatternPathObject.StandatdPatternCacheInfo is IStandardPatternRefPointCache refPointCache ) {
					refPoint = refPointCache.GetProcessRefPoint();
					return true;
				}
			}
			return false;
		}

		public static bool TryGetPathObject( string szPathID, out PathObject pathObject )
		{
			pathObject = null;

			// validate input
			if( string.IsNullOrEmpty( szPathID ) ) {
				return false;
			}

			// check if the object exists and is a PathObject
			if( m_DataManager?.ObjectMap == null
				|| !m_DataManager.ObjectMap.ContainsKey( szPathID )
				|| m_DataManager.ObjectMap[ szPathID ] == null
				|| !( m_DataManager.ObjectMap[ szPathID ] is PathObject ) ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]所選路徑資料異常，請重新選擇", MyApp.NoticeType.Hint );
				return false;
			}

			pathObject = m_DataManager.ObjectMap[ szPathID ] as PathObject;
			return true;
		}
	}
}
