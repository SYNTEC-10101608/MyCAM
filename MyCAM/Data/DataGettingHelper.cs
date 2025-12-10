using MyCAM.CacheInfo;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Data
{
	public static class DataGettingHelper
	{
		static DataManager m_DataManager;

		internal static void Initialize( DataManager dataManager )
		{
			m_DataManager = dataManager;
		}

		public static Dictionary<string, PathObject> GetPathObjectDictionary()
		{
			return m_DataManager.ObjectMap.Values.OfType<PathObject>().ToDictionary( obj => obj.UID );
		}

		#region GeomData and PathObject Access

		public static bool GetGeomDataByID( string pathID, out IGeomData geomData )
		{
			geomData = null;
			if( !TryGetPathObject( pathID, out PathObject pathObject ) ) {
				return false;
			}

			// Use unified GeomData property from StandardPatternBasedPathObject
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

			// Use unified ContourPathObject property from StandardPatternBasedPathObject
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

		#endregion

		#region Cache Access - Unified Pattern

		public static bool GetProcessPathStartEndCacheByID( string pathID, out IProcessPathStartEndCache processPathStartEndCache )
		{
			processPathStartEndCache = null;
			if( !TryGetPathObject( pathID, out PathObject pathObject ) ) {
				return false;
			}

			// Use unified CacheInfo property from StandardPatternBasedPathObject
			if( pathObject is StandardPatternBasedPathObject standardPatternPathObject ) {
				processPathStartEndCache = standardPatternPathObject.CacheInfo as IProcessPathStartEndCache;
				return processPathStartEndCache != null;
			}
			else if( pathObject is ContourPathObject contourPathObject ) {
				processPathStartEndCache = contourPathObject.ContourCacheInfo;
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

			// Use unified CacheInfo property from StandardPatternBasedPathObject
			if( pathObject is StandardPatternBasedPathObject standardPatternPathObject ) {
				// CacheInfo implements IStandardPatternCacheInfo which has GetProcessRefPoint
				if( standardPatternPathObject.CacheInfo is IStandardPatternCacheInfo standardCacheInfo ) {
					refPoint = standardCacheInfo.GetProcessRefPoint();
					return true;
				}
			}
			return false;
		}

		public static bool GetMainPathStartPointCache( string szPathID, out IMainPathStartPointCache mainPathStartPoint )
		{
			mainPathStartPoint = null;
			if( !TryGetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}
			return PathCacheProvider.TryGetMainPathStartPointCache( pathObject, out mainPathStartPoint );
		}

		public static bool GetLeadCache( string szPathID, out ILeadCache leadCache )
		{
			leadCache = null;
			if( !TryGetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}
			return PathCacheProvider.TryGetLeadCache( pathObject, out leadCache );
		}

		public static bool GetPathReverseCache( string szPathID, out IPathReverseCache pathReverseCache )
		{
			pathReverseCache = null;
			if( !TryGetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}
			return PathCacheProvider.TryGetPathReverseCache( pathObject, out pathReverseCache );
		}

		public static bool GetToolVecCache( string szPathID, out IToolVecCache toolVecCache )
		{
			toolVecCache = null;
			if( !TryGetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}
			return PathCacheProvider.TryGetToolVecCache( pathObject, out toolVecCache );
		}

		public static bool GetOverCutCache( string szPathID, out IOverCutCache overCutCache )
		{
			overCutCache = null;
			if( !TryGetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}
			return PathCacheProvider.TryGetOverCutCache( pathObject, out overCutCache );
		}

		#endregion

		#region Private Helper Methods

		/// <summary>
		/// Safely retrieves a PathObject by ID with null and existence checks
		/// </summary>
		/// <param name="szPathID">The path ID to retrieve</param>
		/// <param name="pathObject">The retrieved PathObject, or null if not found</param>
		/// <returns>True if the PathObject was successfully retrieved, false otherwise</returns>
		static bool TryGetPathObject( string szPathID, out PathObject pathObject )
		{
			pathObject = null;

			// Validate input
			if( string.IsNullOrEmpty( szPathID ) ) {
				return false;
			}

			// Get the dictionary once to avoid multiple calls
			Dictionary<string, PathObject> pathObjectDict = GetPathObjectDictionary();

			// Check if key exists and retrieve PathObject
			if( !pathObjectDict.TryGetValue( szPathID, out pathObject ) || pathObject == null ) {
				return false;
			}

			return true;
		}

		#endregion
	}
}
