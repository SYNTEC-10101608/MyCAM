using MyCAM.CacheInfo;
using MyCAM.Data;
using OCCViewer;
using System.Collections.Generic;

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
		public abstract void Remove();

		public void SetShow( bool isShow )
		{
			m_IsShow = isShow;
		}

		public void UpdateView()
		{
			m_Viewer.UpdateView();
		}

		/// <summary>
		/// Get cache info by path ID
		/// </summary>
		protected bool GetCacheInfoByID( string szPathID, out ICacheInfo cacheInfo )
		{
			cacheInfo = null;
			if( string.IsNullOrEmpty( szPathID )
				|| !m_DataManager.ObjectMap.ContainsKey( szPathID )
				|| m_DataManager.ObjectMap[ szPathID ] == null ) {
				return false;
			}
			if( ( m_DataManager.ObjectMap[ szPathID ] as PathObject )?.PathType == PathType.Contour ) {
				cacheInfo = ( m_DataManager.ObjectMap[ szPathID ] as ContourPathObject )?.ContourCacheInfo;
			}
			return cacheInfo != null;
		}

		/// <summary>
		/// Get contour cache info by path ID
		/// </summary>
		protected bool GetContourCacheInfoByID( string szPathID, out ContourCacheInfo contourCacheInfo )
		{
			contourCacheInfo = null;
			if( GetCacheInfoByID( szPathID, out ICacheInfo cacheInfo ) && cacheInfo.PathType == PathType.Contour ) {
				contourCacheInfo = cacheInfo as ContourCacheInfo;
				return true;
			}
			return false;
		}
	}
}
