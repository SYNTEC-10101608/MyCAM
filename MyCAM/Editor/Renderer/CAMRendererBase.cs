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

		bool TryGetPathObject( string szPathID, out PathObject pathObject )
		{
			pathObject = null;
			if( string.IsNullOrEmpty( szPathID )
				|| !DataGettingHelper.GetPathObjectDictionary().ContainsKey( szPathID )
				|| DataGettingHelper.GetPathObjectDictionary()[ szPathID ] == null ) {
				return false;
			}

			pathObject = m_DataManager.ObjectMap[ szPathID ] as PathObject;
			return pathObject != null;
		}

		protected bool GetMainPathStartPointCache( string szPathID, out IMainPathStartPointCache mainPathStartPoint )
		{
			mainPathStartPoint = null;
			if( !TryGetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}

			return PathCacheProvider.TryGetMainPathStartPointCache( pathObject, out mainPathStartPoint );
		}

		protected bool GetLeadCache( string szPathID, out ILeadCache leadCache )
		{
			leadCache = null;
			if( !TryGetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}

			return PathCacheProvider.TryGetLeadCache( pathObject, out leadCache );
		}

		protected bool GetPathReverseCache( string szPathID, out IPathReverseCache pathReverseCache )
		{
			pathReverseCache = null;
			if( !TryGetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}

			return PathCacheProvider.TryGetPathReverseCache( pathObject, out pathReverseCache );
		}

		protected bool GetToolVecCache( string szPathID, out IToolVecCache toolVecCache )
		{
			toolVecCache = null;
			if( !TryGetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}

			return PathCacheProvider.TryGetToolVecCache( pathObject, out toolVecCache );
		}

		protected bool GetOverCutCache( string szPathID, out IOverCutCache overCutCache )
		{
			overCutCache = null;
			if( !TryGetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}

			return PathCacheProvider.TryGetOverCutCache( pathObject, out overCutCache );
		}
	}
}
