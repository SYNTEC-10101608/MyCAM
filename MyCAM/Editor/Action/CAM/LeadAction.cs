using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor
{
	internal class LeadAction : EditActionBase
	{
		public LeadAction( DataManager dataManager, List<CAMData> camDataList )
			: base( dataManager )
		{
			if( camDataList == null || camDataList.Count == 0 ) {
				throw new ArgumentNullException( "LeadAction constructing argument camDataList null or empty" );
			}
			m_CAMDataList = camDataList;

			// when user cancel the lead setting, need to turn path back
			m_BackupLeadParamList = new List<LeadData>();
			foreach( var camData in m_CAMDataList ) {
				if( camData == null ) {
					throw new ArgumentNullException( "LeadAction constructing argument camDataList contains null CAMData" );
				}
				if( camData.LeadLineParam == null ) {
					m_BackupLeadParamList.Add( new LeadData() );
				}
				else {
					m_BackupLeadParamList.Add( camData.LeadLineParam.Clone() );
				}
			}
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

			// TODO: check all CAMData has same lead param or not
			LeadDlg leadDialog = new LeadDlg( new LeadData() );
			PropertyChanged?.Invoke( false, false );

			// preview
			leadDialog.Preview += ( leadData ) =>
			{
				SetLeadParamForAll( leadData );
				PropertyChanged?.Invoke( false, false ); // the second param is not used here
			};

			// confirm
			leadDialog.Confirm += ( leadData ) =>
			{
				SetLeadParamForAll( leadData );
				PropertyChanged?.Invoke( true, GetHasLeadStatus() );
				End();
			};

			// cancel
			leadDialog.Cancel += () =>
			{
				RestoreBackupLeadParams();
				PropertyChanged?.Invoke( false, false ); // the second param is not used here
				End();
			};
			leadDialog.Show( MyApp.MainForm );
		}

		void SetLeadParamForAll( LeadData leadData )
		{
			foreach( var camData in m_CAMDataList ) {
				camData.LeadLineParam = leadData.Clone();
			}
		}

		void RestoreBackupLeadParams()
		{
			for( int i = 0; i < m_CAMDataList.Count; i++ ) {
				m_CAMDataList[ i ].LeadLineParam = m_BackupLeadParamList[ i ].Clone();
			}
		}

		bool GetHasLeadStatus()
		{
			// Return false if one of the CAMData has no lead
			foreach( var camData in m_CAMDataList ) {
				if( !camData.IsHasLead ) {
					return false;
				}
			}
			return true;
		}

		List<CAMData> m_CAMDataList;
		List<LeadData> m_BackupLeadParamList;
	}
}
