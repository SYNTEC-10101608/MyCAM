using MyCAM.PathCache;
using System;

namespace MyCAM.Data
{
	public static class DataGettingHelper
	{
		static DataManager m_DataManager;

		internal static void Initialize( DataManager dataManager )
		{
			if( dataManager == null ) {
				throw new NullReferenceException( "DataManager reference is null in DataGettingHelper.Initialize()" );
			}
			if( dataManager.ObjectMap == null ) {
				throw new NullReferenceException( "DataManager.ObjectMap is null in DataGettingHelper.Initialize()" );
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
			if( !m_DataManager.PathIDList.Contains( szPathID )
				|| !m_DataManager.ObjectMap.ContainsKey( szPathID )
				|| m_DataManager.ObjectMap[ szPathID ] == null
				|| !( m_DataManager.ObjectMap[ szPathID ].ObjectType == ObjectType.Path ) ) {
				return false;
			}

			pathObject = m_DataManager.ObjectMap[ szPathID ] as PathObject;
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
			if( pathObject.PathType == PathType.Contour ) {
				geomData = ( pathObject as ContourPathObject )?.GeomData;
			}
			else if( IsStdPattern( pathObject.PathType ) ) {
				geomData = ( pathObject as StdPatternObjectBase )?.GeomData;
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

		public static bool GetPathCacheByID( string szPathID, out IPathCache contourCache )
		{
			contourCache = null;
			if( !GetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}
			if( pathObject.PathType == PathType.Contour ) {
				contourCache = ( pathObject as ContourPathObject )?.ContourCache;
			}
			else if( IsStdPattern( pathObject.PathType ) ) {
				contourCache = ( pathObject as StdPatternObjectBase )?.StdPatternCache;
			}
			return contourCache != null;
		}

		public static bool GetContourCacheByID( string szPathID, out ContourCache contourCache )
		{
			contourCache = null;
			if( !GetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}
			if( pathObject.PathType != PathType.Contour ) {
				return false;
			}
			contourCache = ( pathObject as ContourPathObject )?.ContourCache;
			return contourCache != null;
		}

		public static bool GetStdPatternCacheByID( string szPathID, out StdPatternCacheBase stdPatternCache )
		{
			stdPatternCache = null;
			if( !GetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}
			if( !IsStdPattern( pathObject.PathType ) ) {
				return false;
			}
			stdPatternCache = ( pathObject as StdPatternObjectBase )?.StdPatternCache;
			return stdPatternCache != null;
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

		public static bool GetMachineData( out MachineData machineData )
		{
			machineData = m_DataManager?.MachineData;
			return machineData != null;
		}

		public static bool IsStdPattern( PathType pathType )
		{
			return pathType == PathType.Circle
				|| pathType == PathType.Rectangle
				|| pathType == PathType.Runway
				|| pathType == PathType.Triangle
				|| pathType == PathType.Square
				|| pathType == PathType.Pentagon
				|| pathType == PathType.Hexagon;
		}
	}
}
