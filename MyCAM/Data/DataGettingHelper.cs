using MyCAM.App;
using MyCAM.CacheInfo;
using MyCAM.Data.PathObjectFolder;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public static class DataGettingHelper
	{
		static DataManager m_DataManager;

		internal static void Initialize( DataManager dataManager )
		{
			m_DataManager = dataManager;
		}

		public static bool GetGeomDataByID( string pathID, out IGeomData geomData )
		{
			geomData = null;
			Dictionary<string, PathObject> pathObjectList = m_DataManager.GetPathObjectDictionary();
			if( pathObjectList.TryGetValue( pathID, out PathObject pathObject ) ) {
				switch( pathObject.PathType ) {
					case PathType.Circle:
						if( pathObject is CirclePathObject circlePathObject ) {
							geomData = circlePathObject.CircleGeomData;
							return true;
						}
						return false;
					case PathType.Rectangle:
						if( pathObject is RectanglePathObject rectanglePathObject ) {
							geomData = rectanglePathObject.RectangleGeomData;
							return true;
						}
						return false;
					case PathType.Runway:
						if( pathObject is RunwayPathObject runwayPathObject ) {
							geomData = runwayPathObject.RunwayGeomData;
							return true;
						}
						return false;
					case PathType.Triangle:
					case PathType.Square:
					case PathType.Pentagon:
					case PathType.Hexagon:
						if( pathObject is PolygonPathObject polygonPathObject ) {
							geomData = polygonPathObject.PolygonGeomData;
							return true;
						}
						return false;
					case PathType.Contour:
					default:
						if( pathObject is ContourPathObject contourPathObject ) {
							geomData = contourPathObject.ContourGeomData;
							return true;
						}
						return false;
				}
			}
			return false;
		}

		public static bool GetCraftDataByID( string szPathID, out CraftData craftData )
		{
			craftData = null;
			if( string.IsNullOrEmpty( szPathID )
				|| m_DataManager.GetPathObjectDictionary().ContainsKey( szPathID ) == false
				|| m_DataManager.GetPathObjectDictionary()[ szPathID ] == null
				|| m_DataManager.GetPathObjectDictionary()[ szPathID ].CraftData == null ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]所選路徑資料異常，請重新選擇", MyApp.NoticeType.Hint );
				return false;
			}

			if( m_DataManager.GetPathObjectDictionary()[ szPathID ].PathType == PathType.Contour ) {
				craftData = m_DataManager.GetPathObjectDictionary()[ szPathID ].CraftData;
			}
			return true;
		}

		public static bool GetContourPathObject( PathObject pathObject, out ContourPathObject contourPathObj )
		{
			contourPathObj = null;
			switch( pathObject.PathType ) {
				case PathType.Circle:
					if( pathObject is CirclePathObject circlePathObject ) {
						contourPathObj = circlePathObject.ContourPathObject;
						return true;
					}
					return false;
				case PathType.Rectangle:
					if( pathObject is RectanglePathObject rectanglePathObject ) {
						contourPathObj = rectanglePathObject.ContourPathObject;
						return true;
					}
					return false;
				case PathType.Runway:
					if( pathObject is RunwayPathObject runwayPathObject ) {
						contourPathObj = runwayPathObject.ContourPathObject;
						return true;
					}
					return false;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					if( pathObject is PolygonPathObject polygonPathObject ) {
						contourPathObj = polygonPathObject.ContourPathObject;
						return true;
					}
					return false;
				case PathType.Contour:
				default:
					if( pathObject is ContourPathObject contourPathObject ) {
						contourPathObj = contourPathObject;
						return true;
					}
					return false;
			}
		}

		public static bool GetProcessPathStartEndCacheByID( string pathID, out IProcessPathStartEndCache processPathStartEndCache )
		{
			processPathStartEndCache = null;
			PathObject pathObject = m_DataManager.GetPathObjectDictionary()[ pathID ];
			switch( pathObject.PathType ) {
				case PathType.Circle:
					if( pathObject is CirclePathObject circlePathObject ) {
						processPathStartEndCache = circlePathObject.CircleCacheInfo;
						return true;
					}
					return false;
				case PathType.Rectangle:
					if( pathObject is RectanglePathObject rectanglePathObject ) {
						processPathStartEndCache = rectanglePathObject.RectangleCacheInfo;
						return true;
					}
					return false;
				case PathType.Runway:
					if( pathObject is RunwayPathObject runwayPathObject ) {
						processPathStartEndCache = runwayPathObject.RunwayCacheInfo;
						return true;
					}
					return false;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					if( pathObject is PolygonPathObject polygonPathObject ) {
						processPathStartEndCache = polygonPathObject.PolygonCacheInfo;
						return true;
					}
					return false;
				case PathType.Contour:
				default:
					if( pathObject is ContourPathObject contourPathObject ) {
						processPathStartEndCache = contourPathObject.ContourCacheInfo;
						return true;
					}
					return false;
			}
		}
	}
}
