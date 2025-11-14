using MyCAM.App;
using MyCAM.Data;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor
{
	internal class TraverseAction : EditCAMActionBase
	{
		public TraverseAction( DataManager dataManager, List<string> pathIDList )
			: base( dataManager, pathIDList )
		{
			// checked in base constructor
			// when user cancel the traverse setting, need to turn path back
			m_BackupTraverseDataList = new List<TraverseData>();
			foreach( var camData in m_CraftDataList ) {
				if( camData == null ) {
					throw new ArgumentNullException( "TraverseAction constructing argument craftData contains null ContourCacheInfo" );
				}
				if( camData.TraverseData == null ) {
					m_BackupTraverseDataList.Add( new TraverseData() );
				}
				else {
					m_BackupTraverseDataList.Add( camData.TraverseData.Clone() );
				}
			}
		}

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
			PropertyChanged?.Invoke( m_PathIDList );
			traverseParamSettingFrom.Confirm += ConfirmTraverseParam;
			traverseParamSettingFrom.Preview += PreviewTraverseParam;
			traverseParamSettingFrom.Cancel += CancelTraverseParam;
			traverseParamSettingFrom.Show( MyApp.MainForm );
		}

		void PreviewTraverseParam( TraverseData data )
		{
			SetTraverseParam( data );
			PropertyChanged?.Invoke( m_PathIDList );
		}

		void ConfirmTraverseParam( TraverseData data )
		{
			SetTraverseParam( data );
			PropertyChanged?.Invoke( m_PathIDList );
			End();
		}

		void CancelTraverseParam()
		{
			RestoreBackupTraverseData();
			PropertyChanged?.Invoke( m_PathIDList );
			End();
		}

		void SetTraverseParam( TraverseData data )
		{
			foreach( var camData in m_CraftDataList ) {
				camData.TraverseData = data.Clone();
			}
		}

		void RestoreBackupTraverseData()
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].TraverseData = m_BackupTraverseDataList[ i ];
			}
		}

		List<TraverseData> m_BackupTraverseDataList;
	}
}
