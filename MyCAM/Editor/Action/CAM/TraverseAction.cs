using MyCAM.App;
using MyCAM.Data;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor
{
	internal class TraverseAction : EditActionBase
	{
		public TraverseAction( DataManager dataManager, List<CAMData> camDataList )
			: base( dataManager )
		{
			if( camDataList == null || camDataList.Count == 0 ) {
				throw new ArgumentNullException( "TraverseAction constructing argument camDataList null or empty" );
			}
			m_CAMDataList = camDataList;

			// when user cancel the traverse setting, need to turn path back
			m_BackupTraverseDataList = new List<TraverseData>();
			foreach( var camData in m_CAMDataList ) {
				if( camData == null ) {
					throw new ArgumentNullException( "TraverseAction constructing argument camDataList contains null CAMData" );
				}
				if( camData.TraverseData == null ) {
					m_BackupTraverseDataList.Add( new TraverseData() );
				}
				else {
					m_BackupTraverseDataList.Add( camData.TraverseData.Clone() );
				}
			}
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

			// TODO: check all CAMData has same traverse param or not
			TraverseDlg traverseParamSettingFrom = new TraverseDlg( m_BackupTraverseDataList[ 0 ].Clone() );
			PropertyChanged?.Invoke();
			traverseParamSettingFrom.Confirm += ConfirmTraverseParam;
			traverseParamSettingFrom.Preview += PreviewTraverseParam;
			traverseParamSettingFrom.Cancel += CancelTraverseParam;
			traverseParamSettingFrom.Show( MyApp.MainForm );
		}

		void PreviewTraverseParam( TraverseData data )
		{
			SetTraverseParam( data );
			PropertyChanged?.Invoke();
		}

		void ConfirmTraverseParam( TraverseData data )
		{
			SetTraverseParam( data );
			PropertyChanged?.Invoke();
			End();
		}

		void CancelTraverseParam()
		{
			RestoreBackupTraverseData();
			PropertyChanged?.Invoke();
			End();
		}

		void SetTraverseParam( TraverseData data )
		{
			foreach( var camData in m_CAMDataList ) {
				camData.TraverseData = data.Clone();
			}
		}

		void RestoreBackupTraverseData()
		{
			for( int i = 0; i < m_CAMDataList.Count; i++ ) {
				m_CAMDataList[ i ].TraverseData = m_BackupTraverseDataList[ i ];
			}
		}

		List<CAMData> m_CAMDataList;
		List<TraverseData> m_BackupTraverseDataList;
	}
}
