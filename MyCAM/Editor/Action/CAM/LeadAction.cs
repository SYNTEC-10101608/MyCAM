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
			for( int i = 0; i < m_PathIDList.Count; i++ ) {
				if( m_DataManager.ObjectMap.TryGetValue( m_PathIDList[ i ], out IObject obj ) ) {
					if( obj is PathObject pathObj && pathObj is StdPatternObjectBase ) {
						m_IsStdPattern = true;
					}
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
			m_LeadDialog = new LeadDlg( m_BackupLeadDataList[ 0 ].Clone() );
			if( m_IsStdPattern ) {
				m_LeadDialog.IsStdPattern?.Invoke( false );
			}
			m_LeadDialog.Preview = PreviewLeadData;
			m_LeadDialog.Confirm = ConfirmLeadData;
			m_LeadDialog.Cancel  = CancelLeadData;
			PropertyChanged?.Invoke( m_PathIDList );
			m_LeadDialog.Show( MyApp.MainForm );
		}

		public override void End()
		{
			if( !m_IsConfirmed ) {
				RestoreBackupLeadDatas();
			}
			if( m_LeadDialog != null ) {
				m_LeadDialog.Preview = null;
				m_LeadDialog.Confirm = null;
				m_LeadDialog.Cancel  = null;
				m_LeadDialog.Close();
				m_LeadDialog = null;
			}
			base.End();
		}

		void PreviewLeadData( LeadData leadData )
		{
			SetLeadDataForAll( leadData );
			PropertyChanged?.Invoke( m_PathIDList );
		}

		void ConfirmLeadData( LeadData leadData )
		{
			SetLeadDataForAll( leadData );
			PropertyChanged?.Invoke( m_PathIDList );
			m_IsConfirmed = true;
			End();
		}

		void CancelLeadData()
		{
			End();
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
			PropertyChanged?.Invoke( m_PathIDList );
		}

		bool m_IsStdPattern = false;
		bool m_IsConfirmed = false;
		List<LeadData> m_BackupLeadDataList;
		LeadDlg m_LeadDialog;
	}
}
