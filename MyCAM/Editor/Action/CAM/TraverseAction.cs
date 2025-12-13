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
			foreach( var craftData in m_CraftDataList ) {
				if( craftData == null ) {
					throw new ArgumentNullException( "TraverseAction constructing argument craftData contains null craftData" );
				}
				if( craftData.TraverseData == null ) {
					m_BackupTraverseDataList.Add( new TraverseData() );
				}
				else {
					m_BackupTraverseDataList.Add( craftData.TraverseData.Clone() );
				}
			}
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.SetTraverse;
			}
		}

		public override void Start()
		{
			base.Start();

			// TODO: check all CAMData has same traverse param or not
			TraverseDlg traverseDataFrom = new TraverseDlg( m_BackupTraverseDataList[ 0 ].Clone() );
			PropertyChanged?.Invoke( m_PathIDList );
			traverseDataFrom.Confirm += ConfirmTraverseData;
			traverseDataFrom.Preview += PreviewTraverseData;
			traverseDataFrom.Cancel += CancelTraverseData;
			traverseDataFrom.Show( MyApp.MainForm );
		}

		void PreviewTraverseData( TraverseData data )
		{
			SetTraverseData( data );
			PropertyChanged?.Invoke( m_PathIDList );
		}

		void ConfirmTraverseData( TraverseData data )
		{
			SetTraverseData( data );
			PropertyChanged?.Invoke( m_PathIDList );
			End();
		}

		void CancelTraverseData()
		{
			RestoreBackupTraverseDatas();
			PropertyChanged?.Invoke( m_PathIDList );
			End();
		}

		void SetTraverseData( TraverseData data )
		{
			foreach( var camData in m_CraftDataList ) {
				camData.TraverseData = data.Clone();
			}
		}

		void RestoreBackupTraverseDatas()
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].TraverseData = m_BackupTraverseDataList[ i ];
			}
		}

		List<TraverseData> m_BackupTraverseDataList;
	}
}
