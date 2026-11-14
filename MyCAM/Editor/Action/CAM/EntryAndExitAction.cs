using System;
using MyCAM.App;
using MyCAM.Data;

namespace MyCAM.Editor
{
	internal class EntryAndExitAction : EditActionBase
	{
		public EntryAndExitAction( DataManager dataManager )
			: base( dataManager )
		{
			m_BackupData = dataManager.EntryAndExitData.Clone();
		}

		public Action PropertyChanged;

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.SetEntryAndExitParam;
			}
		}

		public override void Start()
		{
			base.Start();
			m_Dlg = new EntryAndExitDlg( m_DataManager.EntryAndExitData.Clone() );
			m_Dlg.Confirm = ConfirmEntryAndExitParam;
			m_Dlg.Preview = PreviewEntryAndExitParam;
			m_Dlg.Cancel = CancelEntryAndExitParam;
			m_Dlg.Show( MyApp.MainForm );
		}

		public override void End()
		{
			if( !m_IsConfirmed ) {
				RestoreBackupData();
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

		void PreviewEntryAndExitParam( EntryAndExitData data )
		{
			SetEntryAndExitParam( data );
			PropertyChanged?.Invoke();
		}

		void ConfirmEntryAndExitParam( EntryAndExitData data )
		{
			SetEntryAndExitParam( data );
			PropertyChanged?.Invoke();
			m_IsConfirmed = true;
			End();
		}

		void CancelEntryAndExitParam()
		{
			End();
		}

		void RestoreBackupData()
		{
			SetEntryAndExitParam( m_BackupData );
			PropertyChanged?.Invoke();
		}

		void SetEntryAndExitParam( EntryAndExitData data )
		{
			m_DataManager.EntryAndExitData = data;
		}

		EntryAndExitData m_BackupData;
		EntryAndExitDlg m_Dlg;
		bool m_IsConfirmed = false;
	}
}
