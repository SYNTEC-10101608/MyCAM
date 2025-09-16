using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using System;

namespace MyCAM.Editor
{
	internal class LeadSettingAction : DialogActionBase
	{
		public LeadSettingAction( DataManager cadManager, ViewManager viewManager, CAMData camData )
			: base( cadManager, viewManager )
		{
			if( camData == null ) {
				throw new ArgumentNullException( "PathIndexSelectAction constructing argument camData null" );
			}
			m_CAMData = camData;

			// when user cancel the lead setting, need to turn path back
			LeadData backupLeadParam = m_CAMData.LeadLineParam.Clone();

			// open lead setting form
			LeadLineForm leadDialog = new LeadLineForm( m_CAMData.LeadLineParam.Clone() );

			// preview will change viewer
			leadDialog.Preview += () =>
			{
				SetLeadParam( leadDialog );
				PropertyChanged?.Invoke( false, false ); // the second param is not used here
			};

			// get prvious lead back
			leadDialog.OnCancel += () =>
			{
				// need clone to trigger property changed event
				m_CAMData.LeadLineParam = backupLeadParam.Clone();
				PropertyChanged?.Invoke( false, false ); // the second param is not used here
				End();
			};
			leadDialog.OnComfirm += () =>
			{
				SetLeadParam( leadDialog );
				PropertyChanged?.Invoke( true, m_CAMData.IsHasLead );
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
		}

		public override void End()
		{
		}

		public Action<bool, bool> PropertyChanged; // isConfirm, isHasLead

		void SetLeadParam( LeadLineForm leadDialog )
		{
			m_CAMData.LeadLineParam = leadDialog.LeadLindParam.Clone();
		}

		CAMData m_CAMData;
	}
}
