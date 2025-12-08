using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor
{
	internal class LeadAction : EditCAMActionBase
	{
		public LeadAction( DataManager dataManager, List<string> pathIDList )
			: base( dataManager, pathIDList )
		{
			// checked in base constructor
			// when user cancel the lead setting, need to turn path back
			m_BackupLeadParamList = new List<LeadData>();
			foreach( var craftData in m_CraftDataList ) {
				if( craftData == null ) {
					throw new ArgumentNullException( "LeadAction constructing argument craftDataList contains null craftData" );
				}
				if( craftData.LeadLineParam == null ) {
					m_BackupLeadParamList.Add( new LeadData() );
				}
				else {
					m_BackupLeadParamList.Add( craftData.LeadLineParam.Clone() );
				}
			}
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

			// TODO: check all CAMData has same lead param or not
			LeadDlg leadDialog = new LeadDlg( m_BackupLeadParamList[ 0 ].Clone() );
			PropertyChanged?.Invoke( m_PathIDList );

			// preview
			leadDialog.Preview += ( leadData ) =>
			{
				SetLeadParamForAll( leadData );
				PropertyChanged?.Invoke( m_PathIDList );
			};

			// confirm
			leadDialog.Confirm += ( leadData ) =>
			{
				SetLeadParamForAll( leadData );
				PropertyChanged?.Invoke( m_PathIDList );
				End();
			};

			// cancel
			leadDialog.Cancel += () =>
			{
				RestoreBackupLeadParams();
				PropertyChanged?.Invoke( m_PathIDList );
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

		List<LeadData> m_BackupLeadParamList;
	}
}
