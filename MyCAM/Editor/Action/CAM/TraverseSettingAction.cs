using MyCAM.App;
using MyCAM.Data;
using System;

namespace MyCAM.Editor
{
	internal class TraverseSettingAction : DialogActionBase
	{
		public TraverseSettingAction( DataManager cadManager, ViewManager viewManager )
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
			TraverseParamSettingForm traverseParamSettingFrom = new TraverseParamSettingForm( m_CADManager.TraverseData );
			traverseParamSettingFrom.OnConfirm += ConfirmTraverseParam;
			traverseParamSettingFrom.Preview += PreviewTraverseParam;
			traverseParamSettingFrom.OnCancel += CancelTraverseParam;
			traverseParamSettingFrom.Show( MyApp.MainForm );
		}

		public override void End()
		{
			base.End();
		}

		void ConfirmTraverseParam( TraverseData data )
		{
			SetTraverseParam( data );
			m_CADManager.TraverseData = data;
			PropertyChanged?.Invoke();
			End();
		}

		void PreviewTraverseParam( TraverseData data )
		{
			SetTraverseParam( data );
			PropertyChanged?.Invoke();
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
