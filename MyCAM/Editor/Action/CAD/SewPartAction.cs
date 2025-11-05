using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
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
			dlg.ShowDialog( MyApp.MainForm );
		}

		void SewPart( double dSewTol )
		{
			foreach( string szPartID in m_PartIDList ) {

				// sew the part
				if( !m_DataManager.PartIDList.Contains( szPartID ) || !m_DataManager.ShapeDataMap.ContainsKey( szPartID ) ) {
					continue;
				}
				ShapeData partData = m_DataManager.ShapeDataMap[ szPartID ];
				partData.SewShape( dSewTol );

				// update the viewer
				if( !m_ViewManager.ViewObjectMap.ContainsKey( szPartID ) ) {
					continue;
				}
				AIS_Shape partAIS = AIS_Shape.DownCast( m_ViewManager.ViewObjectMap[ szPartID ].AISHandle );
				if( partAIS != null && !partAIS.IsNull() ) {
					partAIS.SetShape( m_DataManager.ShapeDataMap[ szPartID ].Shape );
					m_Viewer.GetAISContext().Redisplay( partAIS, false );
				}
			}
			m_Viewer.UpdateView();
		}

		List<string> m_PartIDList;
		Viewer m_Viewer;
		ViewManager m_ViewManager;
	}
}
