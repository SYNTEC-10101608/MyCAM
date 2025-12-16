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
			m_BackupLeadDataList = new List<LeadData>();
			foreach( var craftData in m_CraftDataList ) {
				if( craftData == null ) {
					throw new ArgumentNullException( "LeadAction constructing argument craftDataList contains null craftData" );
				}
				if( craftData.LeadData == null ) {
					m_BackupLeadDataList.Add( new LeadData() );
				}
				else {
					m_BackupLeadDataList.Add( craftData.LeadData.Clone() );
				}
			}
			if( m_DataManager.ObjectMap.TryGetValue( m_PathIDList[ 0 ], out IObject obj ) ) {
				if( obj is PathObject pathObj && pathObj is StdPatternObjectBase ) {
					m_IsStdPattern = true;
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
			LeadDlg leadDialog = new LeadDlg( m_BackupLeadDataList[ 0 ].Clone() );
			if( m_IsStdPattern ) {
				leadDialog.IsStdPattern?.Invoke( false );
			}
			PropertyChanged?.Invoke( m_PathIDList );

			// preview
			leadDialog.Preview += ( leadData ) =>
			{
				SetLeadDataForAll( leadData );
				PropertyChanged?.Invoke( m_PathIDList );
			};

			// confirm
			leadDialog.Confirm += ( leadData ) =>
			{
				SetLeadDataForAll( leadData );
				PropertyChanged?.Invoke( m_PathIDList );
				End();
			};

			// cancel
			leadDialog.Cancel += () =>
			{
				RestoreBackupLeadDatas();
				PropertyChanged?.Invoke( m_PathIDList );
				End();
			};
			leadDialog.Show( MyApp.MainForm );
		}

		void SetLeadDataForAll( LeadData leadData )
		{
			foreach( var camData in m_CraftDataList ) {
				camData.LeadData = leadData.Clone();
			}
		}

		void RestoreBackupLeadDatas()
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].LeadData = m_BackupLeadDataList[ i ].Clone();
			}
		}

		bool m_IsStdPattern = false;
		List<LeadData> m_BackupLeadDataList;
	}
}
