using System;
using System.Windows.Forms;
using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using OCCViewer;

namespace MyCAM.Editor
{
	internal class LeadSettingAction : EditActionBase
	{
		public Action<EFunctionStatus> LeadActionStatusChange;
		public Action<bool> PathWithLeadLine;

		public LeadSettingAction( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager,
			CAMData camData )
			: base( viewer, treeView, cadManager, viewManager )
		{
			if( camData == null ) {
				throw new ArgumentNullException( "PathIndexSelectAction constructing argument camData null" );
			}
			m_CAMData = camData;

			// when user cancel the lead setting, need to turn path back
			LeadData backupLeadParam = m_CAMData.LeadLineParam.Clone();

			// open lead setting form
			LeadLineForm leadDialog = new LeadLineForm( m_CAMData.LeadLineParam );

			// preview will change viewer
			leadDialog.Preview += () =>
			{
				SetLeadParam( leadDialog );
				PropertyChanged?.Invoke();
			};

			// get prvious lead back
			leadDialog.OnCancel += () =>
			{
				m_CAMData.LeadLineParam = backupLeadParam;
				PropertyChanged?.Invoke();
				End();
			};
			leadDialog.OnComfirm += () =>
			{
				SetLeadParam( leadDialog );
				bool IsPathWithLead = m_CAMData.LeadLineParam.LeadIn.Type != LeadType.LeadLineType.None || m_CAMData.LeadLineParam.LeadOut.Type != LeadType.LeadLineType.None;
				PathWithLeadLine?.Invoke( IsPathWithLead );
				PropertyChanged?.Invoke();
				End();
			};
			leadDialog.Show( MyApp.MainForm );
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.SetLead;
			}
		}

		public override void Start()
		{
			base.Start();
			LeadActionStatusChange?.Invoke( EFunctionStatus.Open );

			// disable tree view
			m_TreeView.Enabled = false;
		}

		public override void End()
		{
			// enable tree view
			m_TreeView.Enabled = true;
			LeadActionStatusChange?.Invoke( EFunctionStatus.Close );
			base.End();
		}

		public Action PropertyChanged;

		void SetLeadParam( LeadLineForm leadDialog )
		{
			m_CAMData.LeadLineParam = leadDialog.LeadLindParam.Clone();
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

		CAMData m_CAMData;
	}
}
