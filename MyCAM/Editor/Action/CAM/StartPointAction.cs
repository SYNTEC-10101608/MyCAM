using MyCAM.Data;
using OCCViewer;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class StartPointAction : IndexSelectAction
	{
		public StartPointAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager,
			CAMData camData )
			: base( dataManager, viewer, treeView, viewManager, camData )
		{
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.StartPoint;
			}
		}

		public Action PropertyChanged;

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			if( e.Button != MouseButtons.Left ) {
				return;
			}
			int nIndex = GetSelectIndex(out _);
			if( nIndex == -1 ) {
				return;
			}
			m_CAMData.StartPoint = nIndex;
			PropertyChanged?.Invoke();
			m_Viewer.GetAISContext().ClearSelected( true );
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}
	}
}
