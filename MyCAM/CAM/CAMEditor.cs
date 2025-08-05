using MyCAM.CAD;
using OCCViewer;
using System;
using System.Windows.Forms;

namespace MyCAM.CAM
{
	internal class CAMEditor
	{
		public CAMEditor( Viewer viewer, TreeView treeView )
		{
			if( viewer == null || treeView == null ) {
				throw new ArgumentNullException( "CAMEditor consturcting argument null." );
			}

			// CAD manager
			m_CAMManager = new CAMManager();

			// user interface
			m_Viewer = viewer;
			m_TreeView = treeView;
		}

		// user interface
		Viewer m_Viewer;
		TreeView m_TreeView;

		// CAD manager
		CAMManager m_CAMManager;

		// editor
		public void SwitchIn()
		{
			// init tree
			m_TreeView.Nodes.Add( m_CAMManager.PathNode );

			// start default action
			//m_CurrentAction = m_DefaultAction;
			//m_DefaultAction.Start();
		}

		public void SwitchOut()
		{
		}
	}
}
