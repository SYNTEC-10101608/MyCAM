using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor
{
	internal class OverCutAction : EditActionBase
	{
		public OverCutAction( DataManager dataManager, List<CraftData> craftDataList )
		: base( dataManager )
		{
			if( craftDataList == null || craftDataList.Count == 0 ) {
				throw new ArgumentNullException( "OverCutAction constructing argument camData list null or empty" );
			}
			m_CraftDataList = craftDataList;
			m_dOverCutBackupList = new List<double>();
			foreach( var camData in m_CraftDataList ) {
				if( camData == null ) {
					throw new ArgumentNullException( "OverCutAction constructing argument camData null item" );
				}
				m_dOverCutBackupList.Add( camData.OverCutLength );
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
			PropertyChanged?.Invoke();

			// preview
			overCutForm.Preview += ( overCutLength ) =>
			{
				SetOverCutLength( overCutLength );
				PropertyChanged?.Invoke();
			};

			// confirm
			overCutForm.Confirm += ( overCutLength ) =>
			{
				SetOverCutLength( overCutLength );
				PropertyChanged?.Invoke();
				End();
			};

			// cancel
			overCutForm.Cancel += () =>
			{
				RestoreBackupOverCutLength();
				PropertyChanged?.Invoke();
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

		public Action PropertyChanged;
		readonly List<CraftData> m_CraftDataList;
		readonly List<double> m_dOverCutBackupList;
	}
}
