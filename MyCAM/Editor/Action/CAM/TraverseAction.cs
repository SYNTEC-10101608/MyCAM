using MyCAM.App;
using MyCAM.Data;
using System;

namespace MyCAM.Editor
{
	internal class TraverseAction : EditActionBase
	{
		public TraverseAction( DataManager dataManager )
			: base( dataManager )
		{
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
			TraverseDlg traverseParamSettingFrom = new TraverseDlg( m_DataManager.TraverseData.Clone() );
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
			m_DataManager.TraverseData = data;
			PropertyChanged?.Invoke();
			End();
		}

		void CancelTraverseParam()
		{
			SetTraverseParam( m_DataManager.TraverseData );
			PropertyChanged?.Invoke();
			End();
		}

		void SetTraverseParam( TraverseData data )
		{
			foreach( CAMData camData in m_DataManager.GetCAMDataList() ) {
				camData.TraverseData = data.Clone();
			}
		}
	}
}
