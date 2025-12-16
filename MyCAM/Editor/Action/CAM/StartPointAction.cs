using MyCAM.Data;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class StartPointAction : IndexSelectAction
	{
		public StartPointAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, string pathID )
			: base( dataManager, viewer, treeView, viewManager, pathID )
		{
			// checked in base constructor
			m_PathIDList = new List<string>() { pathID };
			m_CraftData = ( m_DataManager.ObjectMap[ m_PathID ] as PathObject ).CraftData;
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
			Console.WriteLine( "nIndex：" + nIndex );
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
