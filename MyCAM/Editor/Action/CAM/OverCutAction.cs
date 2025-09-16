using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using System;

namespace MyCAM.Editor
{
	internal class OverCutAction : DialogActionBase
	{
		public OverCutAction( DataManager cadManager, ViewManager viewManager, CAMData camData )
		: base( cadManager, viewManager )
		{
			if( camData == null ) {
				throw new ArgumentNullException( "PathIndexSelectAction constructing argument camData null" );
			}
			m_CAMData = camData;
			double dOverCutBackup = m_CAMData.OverCutLength;
			OverCutForm overCutForm = new OverCutForm( m_CAMData.OverCutLength );

			// preview will change viewer
			overCutForm.Preview += () =>
			{
				m_CAMData.OverCutLength = overCutForm.OverCutLength;
				PropertyChanged?.Invoke();
			};

			// get prvious over cut back
			overCutForm.OnCancel += () =>
			{
				m_CAMData.OverCutLength = dOverCutBackup;
				PropertyChanged?.Invoke();
				End();
			};
			overCutForm.OnComfirm += () =>
			{
				m_CAMData.OverCutLength = overCutForm.OverCutLength;
				PropertyChanged?.Invoke();
				End();
			};
			overCutForm.Show( MyApp.MainForm );
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
		}

		public override void End()
		{
		}

		public Action PropertyChanged;
		CAMData m_CAMData;
	}
}
