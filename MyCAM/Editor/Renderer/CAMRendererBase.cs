using MyCAM.CacheInfo;
using MyCAM.Data;
using MyCAM.Data.PathObjectFolder;
using OCCViewer;

namespace MyCAM.Editor.Renderer
{
	/// <summary>
	/// Base class for CAM renderers providing common functionality
	/// </summary>
	internal abstract class CAMRendererBase : ICAMRenderer
	{
		protected readonly Viewer m_Viewer;
		protected readonly DataManager m_DataManager;
		protected bool m_IsShow = true;

		protected CAMRendererBase( Viewer viewer, DataManager dataManager )
		{
			m_Viewer = viewer;
			m_DataManager = dataManager;
		}

		public abstract void Show( bool bUpdate = false );
		public abstract void Remove( bool bUpdate = false );

		public void SetShow( bool isShow )
		{
			m_IsShow = isShow;
		}

		public void UpdateView()
		{
			m_Viewer.UpdateView();
		}

		protected bool GetContourCacheInfoByID( string szPathID, out ContourCacheInfo contourCacheInfo )
		{
			contourCacheInfo = null;
			if( string.IsNullOrEmpty( szPathID )
				|| !m_DataManager.ObjectMap.ContainsKey( szPathID )
				|| m_DataManager.ObjectMap[ szPathID ] == null
				|| m_DataManager.ObjectMap[ szPathID ].ObjectType != ObjectType.Path ) {
				return false;
			}
			PathObject pathObject = m_DataManager.ObjectMap[ szPathID ] as PathObject;
			if( pathObject.PathType != PathType.Contour ) {
				return false;
			}
			contourCacheInfo = ( pathObject as ContourPathObject ).ContourCacheInfo;
			return contourCacheInfo != null;
		}

		protected bool GetStartPointCache( string szPathID, out IStartPointCache startPointCache )
		{
			startPointCache = null;
			if( string.IsNullOrEmpty( szPathID )
				|| !m_DataManager.ObjectMap.ContainsKey( szPathID )
				|| m_DataManager.ObjectMap[ szPathID ] == null
				|| m_DataManager.ObjectMap[ szPathID ].ObjectType != ObjectType.Path ) {
				return false;
			}

			PathObject pathObject = m_DataManager.GetPathObjectDictionary()[ szPathID ];
			switch( pathObject.PathType ) {
				case PathType.Circle:
					if( pathObject is CirclePathObject circlePathObject ) {
						startPointCache = circlePathObject.CircleCacheInfo;
						return true;
					}
					return false;
				case PathType.Rectangle:
					if( pathObject is RectanglePathObject rectanglePathObject ) {
						startPointCache = rectanglePathObject.RectangleCacheInfo;
						return true;
					}
					return false;
				case PathType.Runway:
					if( pathObject is RunwayPathObject runwayPathObject ) {
						startPointCache = runwayPathObject.RunwayCacheInfo;
						return true;
					}
					return false;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					if( pathObject is PolygonPathObject polygonPathObject ) {
						startPointCache = polygonPathObject.PolygonCacheInfo;
						return true;
					}
					return false;
				case PathType.Contour:
				default:
					if( pathObject is ContourPathObject contourPathObject ) {
						startPointCache = contourPathObject.ContourCacheInfo;
						return true;
					}
					return false;
			}
		}

		protected bool GetLeadCache( string szPathID, out ILeadCache leadCache )
		{
			leadCache = null;
			if( string.IsNullOrEmpty( szPathID )
				|| !m_DataManager.ObjectMap.ContainsKey( szPathID )
				|| m_DataManager.ObjectMap[ szPathID ] == null
				|| m_DataManager.ObjectMap[ szPathID ].ObjectType != ObjectType.Path ) {
				return false;
			}

			PathObject pathObject = m_DataManager.GetPathObjectDictionary()[ szPathID ];
			switch( pathObject.PathType ) {
				case PathType.Circle:
					if( pathObject is CirclePathObject circlePathObject ) {
						leadCache = circlePathObject.CircleCacheInfo;
						return true;
					}
					return false;
				case PathType.Rectangle:
					if( pathObject is RectanglePathObject rectanglePathObject ) {
						leadCache = rectanglePathObject.RectangleCacheInfo;
						return true;
					}
					return false;
				case PathType.Runway:
					if( pathObject is RunwayPathObject runwayPathObject ) {
						leadCache = runwayPathObject.RunwayCacheInfo;
						return true;
					}
					return false;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					if( pathObject is PolygonPathObject polygonPathObject ) {
						leadCache = polygonPathObject.PolygonCacheInfo;
						return true;
					}
					return false;
				case PathType.Contour:
				default:
					if( pathObject is ContourPathObject contourPathObject ) {
						leadCache = contourPathObject.ContourCacheInfo;
						return true;
					}
					return false;
			}
		}

		protected bool GetPathReverseCache( string szPathID, out IPathReverseCache pathReverseCache )
		{
			pathReverseCache = null;
			if( string.IsNullOrEmpty( szPathID )
				|| !m_DataManager.ObjectMap.ContainsKey( szPathID )
				|| m_DataManager.ObjectMap[ szPathID ] == null
				|| m_DataManager.ObjectMap[ szPathID ].ObjectType != ObjectType.Path ) {
				return false;
			}

			PathObject pathObject = m_DataManager.GetPathObjectDictionary()[ szPathID ];
			switch( pathObject.PathType ) {
				case PathType.Circle:
					if( pathObject is CirclePathObject circlePathObject ) {
						pathReverseCache = circlePathObject.CircleCacheInfo;
						return true;
					}
					return false;
				case PathType.Rectangle:
					if( pathObject is RectanglePathObject rectanglePathObject ) {
						pathReverseCache = rectanglePathObject.RectangleCacheInfo;
						return true;
					}
					return false;
				case PathType.Runway:
					if( pathObject is RunwayPathObject runwayPathObject ) {
						pathReverseCache = runwayPathObject.RunwayCacheInfo;
						return true;
					}
					return false;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					if( pathObject is PolygonPathObject polygonPathObject ) {
						pathReverseCache = polygonPathObject.PolygonCacheInfo;
						return true;
					}
					return false;
				case PathType.Contour:
				default:
					if( pathObject is ContourPathObject contourPathObject ) {
						pathReverseCache = contourPathObject.ContourCacheInfo;
						return true;
					}
					return false;
			}
		}

		protected bool GetToolVecCache( string szPathID, out IToolVecCache toolVecCache )
		{
			toolVecCache = null;
			if( string.IsNullOrEmpty( szPathID )
				|| !m_DataManager.ObjectMap.ContainsKey( szPathID )
				|| m_DataManager.ObjectMap[ szPathID ] == null
				|| m_DataManager.ObjectMap[ szPathID ].ObjectType != ObjectType.Path ) {
				return false;
			}

			PathObject pathObject = m_DataManager.GetPathObjectDictionary()[ szPathID ];
			switch( pathObject.PathType ) {
				case PathType.Contour:
				default:
					if( pathObject is ContourPathObject contourPathObject ) {
						toolVecCache = contourPathObject.ContourCacheInfo;
						return true;
					}
					return false;
			}
		}

		protected bool GetOverCutCache( string szPathID, out IOverCutCache overCutCache )
		{
			overCutCache = null;
			if( string.IsNullOrEmpty( szPathID )
				|| !m_DataManager.ObjectMap.ContainsKey( szPathID )
				|| m_DataManager.ObjectMap[ szPathID ] == null
				|| m_DataManager.ObjectMap[ szPathID ].ObjectType != ObjectType.Path ) {
				return false;
			}

			PathObject pathObject = m_DataManager.GetPathObjectDictionary()[ szPathID ];
			switch( pathObject.PathType ) {
				case PathType.Circle:
					if( pathObject is CirclePathObject circlePathObject ) {
						overCutCache = circlePathObject.CircleCacheInfo;
						return true;
					}
					return false;
				case PathType.Rectangle:
					if( pathObject is RectanglePathObject rectanglePathObject ) {
						overCutCache = rectanglePathObject.RectangleCacheInfo;
						return true;
					}
					return false;
				case PathType.Runway:
					if( pathObject is RunwayPathObject runwayPathObject ) {
						overCutCache = runwayPathObject.RunwayCacheInfo;
						return true;
					}
					return false;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					if( pathObject is PolygonPathObject polygonPathObject ) {
						overCutCache = polygonPathObject.PolygonCacheInfo;
						return true;
					}
					return false;
				case PathType.Contour:
				default:
					if( pathObject is ContourPathObject contourPathObject ) {
						overCutCache = contourPathObject.ContourCacheInfo;
						return true;
					}
					return false;
			}
		}
	}
}
