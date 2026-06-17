using MyCAM.App;
using MyCAM.Data;
using OCC.AIS;
using OCCViewer;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor
{
	internal class SewPartAction : EditActionBase
	{
		public SewPartAction( DataManager dataManager, Viewer viewer, ViewManager viewManager, List<string> szPartIDList )
			: base( dataManager )
		{
			if( viewer == null || viewManager == null || szPartIDList == null || szPartIDList.Count == 0 ) {
				throw new ArgumentNullException( "SewPartAction constructing argument null" );
			}
			m_Viewer = viewer;
			m_ViewManager = viewManager;
			m_PartIDList = szPartIDList;
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.SewPart;
			}
		}

		public override void Start()
		{
			SewPartDlg dlg = new SewPartDlg( 1e-3 );
			dlg.Confirm += ( double sewTol ) =>
			{
				SewPart( sewTol );
				End();
			};
			dlg.Cancel += () =>
			{
				End();
			};
			dlg.Show( MyApp.MainForm );
		}

		void SewPart( double dSewTol )
		{
			foreach( string szPartID in m_PartIDList ) {

				// sew the part
				if( !DataGettingHelper.GetSewableObject( szPartID, out ISewableObject sewable ) ) {
					continue;
				}
				sewable.SewShape( dSewTol );

				// put the sewed shape back
				m_ViewManager.ChangePartShape( szPartID, sewable.Shape );
			}
			m_Viewer.UpdateView();
		}

		List<string> m_PartIDList;
		Viewer m_Viewer;
		ViewManager m_ViewManager;
	}
}
