using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor
{
	internal class OverCutAction : EditCAMActionBase
	{
		public OverCutAction( DataManager dataManager, List<string> pathIDList )
		: base( dataManager )
		{
			if( pathIDList == null || pathIDList.Count == 0 ) {
				throw new ArgumentNullException( "LeadAction constructing argument pathIDList null or empty" );
			}
			m_PathIDList = pathIDList;

			foreach( string ID in m_PathIDList ) {
				m_CraftDataList.Add( ( m_DataManager.ObjectMap[ ID ] as PathObject ).CraftData );
			}

			m_dOverCutBackupList = new List<double>();
			foreach( var craftData in m_CraftDataList ) {
				if( craftData == null ) {
					throw new ArgumentNullException( "OverCutAction constructing argument craftData null item" );
				}
				m_dOverCutBackupList.Add( craftData.OverCutLength );
			}
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.OverCut;
			}
		}

		public override void Start()
		{
			base.Start();

			// TODO: check all CAMData have same over cut length?
			OverCutDlg overCutForm = new OverCutDlg( m_dOverCutBackupList[ 0 ] );
			PropertyChanged?.Invoke( m_PathIDList );

			// preview
			overCutForm.Preview += ( overCutLength ) =>
			{
				SetOverCutLength( overCutLength );
				PropertyChanged?.Invoke( m_PathIDList );
			};

			// confirm
			overCutForm.Confirm += ( overCutLength ) =>
			{
				SetOverCutLength( overCutLength );
				PropertyChanged?.Invoke( m_PathIDList );
				End();
			};

			// cancel
			overCutForm.Cancel += () =>
			{
				RestoreBackupOverCutLength();
				PropertyChanged?.Invoke( m_PathIDList );
				End();
			};
			overCutForm.Show( MyApp.MainForm );
		}

		void SetOverCutLength( double dOverCutLength )
		{
			foreach( var camData in m_CraftDataList ) {
				camData.OverCutLength = dOverCutLength;
			}
		}

		void RestoreBackupOverCutLength()
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].OverCutLength = m_dOverCutBackupList[ i ];
			}
		}

		readonly List<CraftData> m_CraftDataList;
		readonly List<double> m_dOverCutBackupList;
		List<string> m_PathIDList;
	}
}
