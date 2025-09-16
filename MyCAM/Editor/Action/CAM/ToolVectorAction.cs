using System;
using System.Windows.Forms;
using MyCAM.App;
using MyCAM.Data;
using OCCViewer;

namespace MyCAM.Editor
{
	internal class ToolVectorAction : IndexSelectAction
	{
		public ToolVectorAction( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager,
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
			bool isModified = m_CAMData.GetToolVecModify( nIndex, out double angleA_deg, out double angleB_deg );
			ToolVecForm form = new ToolVecForm( isModified, angleA_deg, angleB_deg );
			form.RemoveEditData = () =>
			{
				// for remove edited data
				m_CAMData.RemoveToolVecModify( nIndex );
				PropertyChanged?.Invoke();
				m_Viewer.GetAISContext().ClearSelected( true );
			};
			DialogResult result = form.ShowDialog( MyApp.MainForm );
			if( result != DialogResult.OK ) {
				m_Viewer.GetAISContext().ClearSelected( true );
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

		protected override void TreeViewAfterSelect( object sender, TreeViewEventArgs e )
		{
			// do nothing
		}

		protected override void TreeViewKeyDown( object sender, KeyEventArgs e )
		{
			// do nothing
		}
	}
}
