using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor
{
	internal class ExternalAxisAction : EditCAMActionBase
	{
		public ExternalAxisAction( DataManager dataManager, List<string> pathIDList )
			: base( dataManager, pathIDList )
		{
			m_ExternalAxisBackupList = new List<double>();
			foreach( var craftData in m_CraftDataList ) {
				if( craftData == null ) {
					throw new ArgumentNullException( "ExternalAxisAction constructing argument craftData null item" );
				}
				m_ExternalAxisBackupList.Add( craftData.ExternalAxis_deg );
			}
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.ExternalAxis;
			}
		}

		public override void Start()
		{
			base.Start();
			m_Dlg = new ExternalAxisDlg( m_ExternalAxisBackupList[ 0 ] );
			m_Dlg.Preview = PreviewExternalAxis;
			m_Dlg.Confirm = ConfirmExternalAxis;
			m_Dlg.Cancel = CancelExternalAxis;
			PropertyChanged?.Invoke( m_PathIDList );
			m_Dlg.Show( MyApp.MainForm );
		}

		public override void End()
		{
			if( !m_IsConfirmed ) {
				RestoreBackupExternalAxis();
				PropertyChanged?.Invoke( m_PathIDList );
			}
			if( m_Dlg != null ) {
				m_Dlg.Preview = null;
				m_Dlg.Confirm = null;
				m_Dlg.Cancel = null;
				m_Dlg.Close();
				m_Dlg = null;
			}
			base.End();
		}

		void PreviewExternalAxis( double externalAxis_deg )
		{
			SetExternalAxis( externalAxis_deg );
			PropertyChanged?.Invoke( m_PathIDList );
		}

		void ConfirmExternalAxis( double externalAxis_deg )
		{
			SetExternalAxis( externalAxis_deg );
			PropertyChanged?.Invoke( m_PathIDList );
			m_IsConfirmed = true;
			End();
		}

		void CancelExternalAxis()
		{
			End();
		}

		void SetExternalAxis( double externalAxis_deg )
		{
			foreach( var craftData in m_CraftDataList ) {
				craftData.ExternalAxis_deg = externalAxis_deg;
			}
		}

		void RestoreBackupExternalAxis()
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].ExternalAxis_deg = m_ExternalAxisBackupList[ i ];
			}
		}

		readonly List<double> m_ExternalAxisBackupList;
		bool m_IsConfirmed = false;
		ExternalAxisDlg m_Dlg;
	}
}
