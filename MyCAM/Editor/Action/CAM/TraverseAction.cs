using MyCAM.App;
using MyCAM.Data;
using System;

namespace MyCAM.Editor
{
	internal class TraverseAction : EditActionBase
	{
		public TraverseAction( DataManager dataManager, CAMData camData )
			: base( dataManager )
		{
			if( camData == null ) {
				throw new ArgumentNullException( "TraverseAction constructing argument camData null" );
			}
			m_CAMData = camData;

			// when user cancel the traverse setting, need to turn path back
			m_BackupTraverseData = m_CAMData.TraverseData.Clone();
		}

		public Action PropertyChanged;

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
			TraverseDlg traverseParamSettingFrom = new TraverseDlg( m_CAMData.TraverseData.Clone() );
			traverseParamSettingFrom.Confirm += ConfirmTraverseParam;
			traverseParamSettingFrom.Preview += PreviewTraverseParam;
			traverseParamSettingFrom.Cancel += CancelTraverseParam;
			traverseParamSettingFrom.Show( MyApp.MainForm );
		}

		void PreviewTraverseParam( TraverseData data )
		{
			SetTraverseParam( data );
			PropertyChanged?.Invoke();
		}

		void ConfirmTraverseParam( TraverseData data )
		{
			SetTraverseParam( data );
			PropertyChanged?.Invoke();
			End();
		}

		void CancelTraverseParam()
		{
			SetTraverseParam( m_BackupTraverseData );
			PropertyChanged?.Invoke();
			End();
		}

		void SetTraverseParam( TraverseData data )
		{
			m_CAMData.TraverseData = data;
		}

		CAMData m_CAMData;
		TraverseData m_BackupTraverseData;
	}
}
