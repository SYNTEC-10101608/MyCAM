using MyCAM.Data;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class StartPointAction : IndexSelectAction
	{
		public StartPointAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, List<string> pathIDList )
			: base( dataManager, viewer, treeView, viewManager, pathIDList.First() )
		{
			m_CraftData = ( m_DataManager.ObjectMap[ m_PathID ] as ContourPathObject ).CraftData;
			m_PathIDList = pathIDList;
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.StartPoint;
			}
		}

		public Action<List<string>> PropertyChanged;

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			if( e.Button != MouseButtons.Left ) {
				return;
			}
			int nIndex = GetSelectIndex( out _ );
			if( nIndex == -1 ) {
				return;
			}
			m_CraftData.StartPointIndex = nIndex;
			PropertyChanged?.Invoke( m_PathIDList );
			m_Viewer.GetAISContext().ClearSelected( true );
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		CraftData m_CraftData;
		List<string> m_PathIDList;
	}
}
