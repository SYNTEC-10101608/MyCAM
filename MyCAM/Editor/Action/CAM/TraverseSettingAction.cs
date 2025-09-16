using MyCAM.App;
using MyCAM.Data;
using OCCViewer;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class TraverseSettingAction : KeyMouseActionBase
	{
		public TraverseSettingAction( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager )
			: base( viewer, treeView, cadManager, viewManager )
		{
		}

		public Action PropertyChanged;

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.SetTraverseParam;
			}
		}

		public override void Start()
		{
			base.Start();
			TraverseParamSettingForm traverseParamSettingFrom = new TraverseParamSettingForm( m_CADManager.TraverseData );
			traverseParamSettingFrom.OnConfirm += ConfirmTraverseParam;
			traverseParamSettingFrom.Preview += PreviewTraverseParam;
			traverseParamSettingFrom.OnCancel += CancelTraverseParam;
			traverseParamSettingFrom.Show( MyApp.MainForm );
		}

		public override void End()
		{
			base.End();
		}

		void ConfirmTraverseParam( TraverseData data )
		{
			SetTraverseParam( data );
			m_CADManager.TraverseData = data;
			PropertyChanged?.Invoke();
			End();
		}

		void PreviewTraverseParam( TraverseData data )
		{
			SetTraverseParam( data );
			PropertyChanged?.Invoke();
		}

		void CancelTraverseParam()
		{
			SetTraverseParam( m_CADManager.TraverseData );
			PropertyChanged?.Invoke();
			End();
		}

		void SetTraverseParam( TraverseData data )
		{
			foreach( CAMData camData in m_CADManager.GetCAMDataList() ) {
				camData.TraverseData = data.Clone();
			}
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			// do nothing
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			// do nothing
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
