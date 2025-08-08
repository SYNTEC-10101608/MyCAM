using MyCAM.Data;
using OCCViewer;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class ToolVecAction : PathIndexSelectAction
	{
		public ToolVecAction( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager,
			CAMData camData )
			: base( viewer, treeView, cadManager, viewManager, camData )
		{
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.ToolVec;
			}
		}

		public Action PropertyChanged;

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			if( e.Button != MouseButtons.Left ) {
				return;
			}
			int nIndex = GetSelectIndex();
			if( nIndex == -1 ) {
				return;
			}

			// modify tool vector
			m_CAMData.GetToolVecModify( nIndex, out double angleA_deg, out double angleB_deg );
			ToolVecForm form = new ToolVecForm( angleA_deg, angleB_deg );
			DialogResult result = form.ShowDialog();
			if( result != DialogResult.OK ) {
				return;
			}
			form.GetAngleValue( out double newAngleA_deg, out double newAnfleB_deg );
			m_CAMData.SetToolVecModify( nIndex, newAngleA_deg, newAnfleB_deg );
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
