using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor
{
	internal class LeadAction : EditActionBase
	{
		public LeadAction( DataManager dataManager, List<CraftData> craftDataList )
			: base( dataManager )
		{
			if( craftDataList == null || craftDataList.Count == 0 ) {
				throw new ArgumentNullException( "LeadAction constructing argument craftDataList null or empty" );
			}
			m_CraftDataList = craftDataList;

			// when user cancel the lead setting, need to turn path back
			m_BackupLeadParamList = new List<LeadData>();
			foreach( var camData in m_CraftDataList ) {
				if( camData == null ) {
					throw new ArgumentNullException( "LeadAction constructing argument craftDataList contains null ContourCacheInfo" );
				}
				if( camData.LeadLineParam == null ) {
					m_BackupLeadParamList.Add( new LeadData() );
				}
				else {
					m_BackupLeadParamList.Add( camData.LeadLineParam.Clone() );
				}
			}
		}

		public Action PropertyChanged; // isConfirm, isHasLead

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

			// TODO: check all CAMData has same lead param or not
			LeadDlg leadDialog = new LeadDlg( m_BackupLeadParamList[ 0 ].Clone() );
			PropertyChanged?.Invoke();

			// preview
			leadDialog.Preview += ( leadData ) =>
			{
				SetLeadParamForAll( leadData );
				PropertyChanged?.Invoke();
			};

			// confirm
			leadDialog.Confirm += ( leadData ) =>
			{
				SetLeadParamForAll( leadData );
				PropertyChanged?.Invoke();
				End();
			};

			// cancel
			leadDialog.Cancel += () =>
			{
				RestoreBackupLeadParams();
				PropertyChanged?.Invoke();
				End();
			};
			leadDialog.Show( MyApp.MainForm );
		}

		void SetLeadParamForAll( LeadData leadData )
		{
			foreach( var camData in m_CraftDataList ) {
				camData.LeadLineParam = leadData.Clone();
			}
		}

		void RestoreBackupLeadParams()
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].LeadLineParam = m_BackupLeadParamList[ i ].Clone();
			}
		}

		List<CraftData> m_CraftDataList;
		List<LeadData> m_BackupLeadParamList;
	}
}
