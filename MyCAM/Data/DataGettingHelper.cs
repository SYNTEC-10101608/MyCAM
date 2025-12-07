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
					case PathType.Contour:
					default:
						return ( pathObject as ContourPathObject ).ContourGeomData;
				}
			}
			return null;
		}
	}
}
