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
				throw new ArgumentNullException( "CADEditor consturcting argument null." );
			}

			// CAD manager
			m_CAMManager = new CAMManager();

			// user interface
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_TreeView.Nodes.Add( m_CAMManager.PathNode );
		}

		// user interface
		Viewer m_Viewer;
		TreeView m_TreeView;

		// CAD manager
		CAMManager m_CAMManager;
	}
}
