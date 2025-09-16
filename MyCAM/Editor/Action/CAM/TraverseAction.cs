using MyCAM.App;
using MyCAM.Data;
using System;

namespace MyCAM.Editor
{
	internal class TraverseAction : DialogActionBase
	{
		public TraverseAction( DataManager cadManager, ViewManager viewManager )
			: base( cadManager, viewManager )
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
			TraverseParamSettingForm traverseParamSettingFrom = new TraverseParamSettingForm( m_CADManager.TraverseData.Clone() );
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
			m_CADManager.TraverseData = data;
			PropertyChanged?.Invoke();
			End();
		}

		void CancelTraverseParam()
		{
			SetTraverseParam( m_CADManager.TraverseData );
			PropertyChanged?.Invoke();
			End();
		}

		void SetTraverseParam( TraverseData data )
		{
			foreach( CAMData camData in m_CADManager.GetCAMDataList() ) {
				camData.TraverseData = data.Clone();
			}
		}
	}
}
