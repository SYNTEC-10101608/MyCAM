using MyCAM.App;
using MyCAM.PathCache;

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
			if( pathObject is StdPatternObjectBase standardPatternPathObject ) {
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
			if( pathObject is StdPatternObjectBase standardPatternPathObject ) {
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

			// use unified Cache property from StandardPatternBasedPathObject
			if( pathObject is StdPatternObjectBase standardPatternPathObject ) {

				// cache implements IStdPatternCache which has GetProcessRefPoint
				if( standardPatternPathObject.StdPatternCache is IStdPatternRefPointCache refPointCache ) {
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

		public static bool GetShapeObject( string szObjID, out IShapeObject shape )
		{
			shape = null;
			if( m_DataManager == null || string.IsNullOrEmpty( szObjID ) ) {
				return false;
			}
			if( m_DataManager.ObjectMap == null
				|| !m_DataManager.ObjectMap.ContainsKey( szObjID )
				|| m_DataManager.ObjectMap[ szObjID ] == null
				|| !( m_DataManager.ObjectMap[ szObjID ] is IShapeObject _shapeObject ) ) {
				return false;
			}
			shape = _shapeObject;
			return true;
		}

		public static bool GetTransformableObject( string szObjID, out ITransformableObject transformable )
		{
			transformable = null;
			if( m_DataManager == null || string.IsNullOrEmpty( szObjID ) ) {
				return false;
			}
			if( m_DataManager.ObjectMap == null
				|| !m_DataManager.ObjectMap.ContainsKey( szObjID )
				|| m_DataManager.ObjectMap[ szObjID ] == null
				|| !( m_DataManager.ObjectMap[ szObjID ] is ITransformableObject _transformable ) ) {
				return false;
			}
			transformable = _transformable;
			return true;
		}

		public static bool GetSewableObject( string szObjID, out ISewableObject sewable )
		{
			sewable = null;
			if( m_DataManager == null || string.IsNullOrEmpty( szObjID ) ) {
				return false;
			}
			if( m_DataManager.ObjectMap == null
				|| !m_DataManager.ObjectMap.ContainsKey( szObjID )
				|| m_DataManager.ObjectMap[ szObjID ] == null
				|| !( m_DataManager.ObjectMap[ szObjID ] is ISewableObject _sewable ) ) {
				return false;
			}
			sewable = _sewable;
			return true;
		}
	}
}
