using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using System;

namespace MyCAM.Editor
{
	internal class LeadAction : EditActionBase
	{
		public LeadAction( DataManager dataManager, CAMData camData )
			: base( dataManager )
		{
			if( camData == null ) {
				throw new ArgumentNullException( "LeadSettingAction constructing argument camData null" );
			}
			m_CAMData = camData;

			// when user cancel the lead setting, need to turn path back
			m_BackupLeadParam = m_CAMData.LeadLineParam.Clone();
		}

		public Action<bool, bool> PropertyChanged; // isConfirm, isHasLead

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
			LeadDlg leadDialog = new LeadDlg( m_CAMData.LeadLineParam.Clone() );

			// preview
			leadDialog.Preview += ( leadData ) =>
			{
				SetLeadParam( leadData );
				PropertyChanged?.Invoke( false, false ); // the second param is not used here
			};

			// confirm
			leadDialog.Confirm += ( leadData ) =>
			{
				SetLeadParam( leadData );
				PropertyChanged?.Invoke( true, m_CAMData.IsHasLead );
				End();
			};

			// cancel
			leadDialog.Cancel += () =>
			{
				SetLeadParam( m_BackupLeadParam.Clone() );
				PropertyChanged?.Invoke( false, false ); // the second param is not used here
				End();
			};
			leadDialog.Show( MyApp.MainForm );
		}



		void SetLeadParam( LeadData leadData )
		{
			m_CAMData.LeadLineParam = leadData.Clone();
		}

		CAMData m_CAMData;
		LeadData m_BackupLeadParam;
	}
}
