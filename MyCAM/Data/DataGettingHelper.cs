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

		public static bool GetPathObject( string szPathID, out PathObject pathObject )
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
				|| !( m_DataManager.ObjectMap[ szPathID ] is PathObject _pathObject ) ) {
				return false;
			}

			pathObject = _pathObject;
			return pathObject != null;
		}

		public static bool GetPathType( string szPathID, out PathType pathType )
		{
			pathType = PathType.Contour;
			if( !GetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}
			pathType = pathObject.PathType;
			return true;
		}

		public static bool GetGeomDataByID( string pathID, out IGeomData geomData )
		{
			geomData = null;
			if( !GetPathObject( pathID, out PathObject pathObject ) ) {
				return false;
			}

			// use unified GeomData property from StandardPatternBasedPathObject
			if( pathObject is StdPatternObjectBase standardPatternPathObject ) {
				geomData = standardPatternPathObject.GeomData;
			}
			else if( pathObject is ContourPathObject contourPathObject ) {
				geomData = contourPathObject.ContourGeomData;
			}
			return geomData != null;
		}

		public static bool GetCraftDataByID( string szPathID, out CraftData craftData )
		{
			craftData = null;
			if( !GetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}
			craftData = pathObject.CraftData;
			return craftData != null;
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
			return shape != null;
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
			return transformable != null;
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
			return sewable != null;
		}
	}
}
