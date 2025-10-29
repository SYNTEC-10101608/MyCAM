using MyCAM.App;
using MyCAM.Data;
using System;

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
			EntryAndExitDlg entryAndExitDlg = new EntryAndExitDlg( m_DataManager.EntryAndExitData.Clone() );
			entryAndExitDlg.Confirm += ConfirmEntryAndExitParam;
			entryAndExitDlg.Preview += PreviewEntryAndExitParam;
			entryAndExitDlg.Cancel += CancelEntryAndExitParam;
			entryAndExitDlg.Show( MyApp.MainForm );
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
			End();
		}

		void CancelEntryAndExitParam()
		{
			SetEntryAndExitParam( m_BackupData );
			PropertyChanged?.Invoke();
			End();
		}

		void SetEntryAndExitParam( EntryAndExitData data )
		{
			m_DataManager.EntryAndExitData = data;
		}

		EntryAndExitData m_BackupData;
	}
}
