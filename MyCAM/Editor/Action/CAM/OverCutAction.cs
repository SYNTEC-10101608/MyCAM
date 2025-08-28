using System;
using System.Windows.Forms;
using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using OCCViewer;

namespace MyCAM.Editor
{
	internal class OverCutAction : DefaultAction
	{
		public Action<EFunctionStatus> OverCutActionStatusChange;

		public OverCutAction( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager,
		CAMData camData, ESelectObjectType type )
		: base( viewer, treeView, cadManager, viewManager, type )
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

		public override void Start()
		{
			base.Start();
			OverCutActionStatusChange?.Invoke( EFunctionStatus.Open );
			//// disable tree view
			m_TreeView.Enabled = false;
		}

		public override void End()
		{
			// enable tree view
			m_TreeView.Enabled = true;
			OverCutActionStatusChange?.Invoke( EFunctionStatus.Close );
			base.End();
		}


		public Action PropertyChanged;

		CAMData m_CAMData;
	}
}
