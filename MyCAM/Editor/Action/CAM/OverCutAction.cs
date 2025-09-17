using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using System;

namespace MyCAM.Editor
{
	internal class OverCutAction : EditActionBase
	{
		public OverCutAction( DataManager cadManager, CAMData camData )
		: base( cadManager )
		{
			if( camData == null ) {
				throw new ArgumentNullException( "OverCutAction constructing argument camData null" );
			}
			m_CAMData = camData;
			m_dOverCutBackup = m_CAMData.OverCutLength;
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
			OverCutDlg overCutForm = new OverCutDlg( m_CAMData.OverCutLength );

			// preview
			overCutForm.Preview += ( overCutLength ) =>
			{
				m_CAMData.OverCutLength = overCutLength;
				PropertyChanged?.Invoke();
			};

			// confirm
			overCutForm.Confirm += ( overCutLength ) =>
			{
				m_CAMData.OverCutLength = overCutLength;
				PropertyChanged?.Invoke();
				End();
			};

			// cancel
			overCutForm.Cancel += () =>
			{
				m_CAMData.OverCutLength = m_dOverCutBackup;
				PropertyChanged?.Invoke();
				End();
			};
			overCutForm.Show( MyApp.MainForm );
		}

		public Action PropertyChanged;
		readonly CAMData m_CAMData;
		readonly double m_dOverCutBackup;
	}
}
