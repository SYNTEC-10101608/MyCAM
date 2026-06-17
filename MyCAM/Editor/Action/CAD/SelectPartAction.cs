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
			m_ViewManager.ActiveParts( m_DataManager.PartIDList );
		}

		protected override void DeactivateObject()
		{
			m_ViewManager.DeactiveParts( m_DataManager.PartIDList );
		}
	}
}
