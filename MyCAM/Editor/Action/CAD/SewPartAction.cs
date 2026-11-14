using System;
using System.Collections.Generic;
using MyCAM.App;
using MyCAM.Data;
using OCC.AIS;
using OCCViewer;

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
			m_Dlg = new SewPartDlg( 1e-3 );
			m_Dlg.Confirm += ConfirmSewPart;
			m_Dlg.Cancel += CancelSewPart;
			m_Dlg.Show( MyApp.MainForm );
		}

		public override void End()
		{
			if( m_Dlg != null ) {
				m_Dlg.Confirm -= ConfirmSewPart;
				m_Dlg.Cancel -= CancelSewPart;
				m_Dlg.Close();
				m_Dlg = null;
			}
			base.End();
		}

		void ConfirmSewPart( double sewTol )
		{
			SewPart( sewTol );
			End();
		}

		void CancelSewPart()
		{
			End();
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
		SewPartDlg m_Dlg;
	}
}
