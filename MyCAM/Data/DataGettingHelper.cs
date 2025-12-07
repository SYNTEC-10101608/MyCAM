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

		public static IGeomData GetGeomDataByID( string pathID )
		{
			Dictionary<string, PathObject> pathObjectList = m_DataManager.GetPathObjectDictionary();
			if( pathObjectList.TryGetValue( pathID, out PathObject pathObject ) ) {
				switch( pathObject.PathType ) {
					case PathType.Circle:
						return ( pathObject as CirclePathObject ).CircleGeomData;
					case PathType.Rectangle:
						return ( pathObject as RectanglePathObject ).RectangleGeomData;
					case PathType.Runway:
						return ( pathObject as RunwayPathObject ).RunwayGeomData;
					case PathType.Triangle:
					case PathType.Square:
					case PathType.Pentagon:
					case PathType.Hexagon:
						return ( pathObject as PolygonPathObject ).PolygonGeomData;
					case PathType.Contour:
					default:
						return ( pathObject as ContourPathObject ).ContourGeomData;
				}
			}
			return null;
		}
	}
}
