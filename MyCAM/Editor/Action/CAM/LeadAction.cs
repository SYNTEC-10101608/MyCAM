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
			: base( dataManager )
		{
			if( pathIDList == null || pathIDList.Count == 0 ) {
				throw new ArgumentNullException( "LeadAction constructing argument pathIDList null or empty" );
			}
			m_PathIDList = pathIDList;

			foreach( string ID in m_PathIDList ) {
				m_CraftDataList.Add( ( dataManager.ObjectMap[ ID ] as PathObject ).CraftData );
			}

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

		List<CraftData> m_CraftDataList = new List<CraftData>();
		List<LeadData> m_BackupLeadParamList;
		List<string> m_PathIDList;
	}
}
