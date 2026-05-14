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
		protected bool m_IsPauseRefreshAndHide = false;
		protected bool m_IsPauseRefresh = false;

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

		public virtual void SetPauseRefreshAndHide( bool isPause )
		{
			m_IsPauseRefreshAndHide = isPause;
			m_IsPauseRefresh = isPause;
		}

		public virtual void SetPauseRefresh( bool isPause )
		{
			m_IsPauseRefresh = isPause;
		}

		public void UpdateView()
		{
			m_Viewer.UpdateView();
		}
	}
}
