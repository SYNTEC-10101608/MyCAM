using MyCAM.Data;
using OCCViewer;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class SelectPathAction_ : SelectObjectAction
	{
		public Action RemovePath;
		public Action<bool> PathOrderMove;

		public SelectPathAction_( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
		}

		protected override void ActivateObject()
		{
			foreach( var pathID in m_DataManager.PathIDList ) {
				m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ pathID ].AISHandle );
			}
		}

		protected override void DeactivateObject()
		{
			foreach( var pathID in m_DataManager.PathIDList ) {
				m_Viewer.GetAISContext().Deactivate( m_ViewManager.ViewObjectMap[ pathID ].AISHandle );
			}
		}

		protected override void OnKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Delete ) {
				RemovePath?.Invoke();
			}
			base.OnKeyDown( e );
		}
	}
}
