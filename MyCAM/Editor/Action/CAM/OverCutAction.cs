using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor
{
	internal class OverCutAction : EditActionBase
	{
		public OverCutAction( DataManager dataManager, List<CAMData> camDataList )
		: base( dataManager )
		{
			if( camDataList == null || camDataList.Count == 0 ) {
				throw new ArgumentNullException( "OverCutAction constructing argument camData list null or empty" );
			}
			m_CAMDataList = camDataList;
			m_dOverCutBackupList = new List<double>();
			foreach( var camData in m_CAMDataList ) {
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
			OverCutDlg overCutForm = new OverCutDlg( 0 );
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
				for( int i = 0; i < m_CAMDataList.Count; i++ ) {
					m_CAMDataList[ i ].OverCutLength = m_dOverCutBackupList[ i ];
				}
				PropertyChanged?.Invoke();
				End();
			};
			overCutForm.Show( MyApp.MainForm );
		}

		void SetOverCutLength( double dOverCutLength )
		{
			foreach( var camData in m_CAMDataList ) {
				camData.OverCutLength = dOverCutLength;
			}
		}

		public Action PropertyChanged;
		readonly List<CAMData> m_CAMDataList;
		readonly List<double> m_dOverCutBackupList;
	}
}
