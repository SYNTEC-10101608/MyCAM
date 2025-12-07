using MyCAM.CacheInfo;
using MyCAM.Data;
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

		protected bool GetMainPathStartPnt( string szPathID, out IStartPnt pointGettable )
		{
			pointGettable = null;
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
						pointGettable = circlePathObject.CircleCacheInfo;
						return true;
					}
					return false;
				case PathType.Rectangle:
					if( pathObject is RectanglePathObject rectanglePathObject ) {
						pointGettable = rectanglePathObject.RectangleCacheInfo;
						return true;
					}
					return false;
				case PathType.Contour:
				default:
					if( pathObject is ContourPathObject contourPathObject ) {
						pointGettable = contourPathObject.ContourCacheInfo;
						return true;
					}
					return false;
			}
		}
	}
}
