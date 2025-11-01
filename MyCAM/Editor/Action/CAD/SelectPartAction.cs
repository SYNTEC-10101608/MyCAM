using MyCAM.Data;
using OCCViewer;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class SelectPartAction : SelectObjectAction
	{
		public SelectPartAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
		}

		protected override void ActivateObject()
		{
			foreach( var partID in m_DataManager.PartIDList ) {
				m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ partID ].AISHandle );
			}
		}

		protected override void DeactivateObject()
		{
			foreach( var partID in m_DataManager.PartIDList ) {
				m_Viewer.GetAISContext().Deactivate( m_ViewManager.ViewObjectMap[ partID ].AISHandle );
			}
		}
	}
}
