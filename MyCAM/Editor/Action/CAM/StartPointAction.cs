using MyCAM.App;
using MyCAM.Data;
using MyCAM.Helper;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class StartPointAction : EditActionBase
	{
		public StartPointAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, string pathID, SelectPathAction pathIndexAction )
			: base( dataManager )
		{
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;

			// for path index control
			m_PathIndexAction = pathIndexAction;
			m_CurrentPathID = pathID;
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.StartPoint;
			}
		}

		public Action<List<string>> PropertyChanged;

		public override void Start()
		{
			base.Start();

			// setup initial path
			SetupPath( m_CurrentPathID );

			// register ESC key
			m_Viewer.KeyDown += OnViewerKeyDown;

			// start path index action
			m_PathIndexAction.Start();
			m_PathIndexAction.SelectionChange += OnPathIndexChanged;

			// activate point selection for initial path
			ActivatePointSelection();
		}

		public override void End()
		{
			DeactivatePointSelection( m_CurrentPathID );

			// unregister ESC key
			m_Viewer.KeyDown -= OnViewerKeyDown;

			// cleanup path index action
			m_PathIndexAction.SelectionChange -= OnPathIndexChanged;
			m_PathIndexAction.End();

			base.End();
		}

		void OnViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		void SetupPath( string pathID )
		{
			if( string.IsNullOrEmpty( pathID ) ) {
				End();
				return;
			}

			// init data for current path
			m_CurrentPathID = pathID;
			if( !DataGettingHelper.GetCraftDataByID( pathID, out m_CraftData ) ) {
				MyApp.Logger.ShowOnLogPanel( $"無法獲取路徑 {pathID} 的加工資訊", MyApp.NoticeType.Warning );
				End();
				return;
			}
		}

		void ActivatePointSelection()
		{
			m_PathIndexAction.ExcludeFromSelection( m_CurrentPathID );
			CreatePointIndexAction( m_CurrentPathID );
		}

		void DeactivatePointSelection( string szOldPathID )
		{
			DestroyPointIndexAction();
			m_PathIndexAction.RestoreFromExclusion( szOldPathID );
		}

		void CreatePointIndexAction( string pathID )
		{
			m_PointIndexAction = new IndexSelectAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, pathID );
			m_PointIndexAction.IndexChanged += OnPointIndexChanged;
			m_PointIndexAction.Start();
		}

		void DestroyPointIndexAction()
		{
			if( m_PointIndexAction != null ) {
				m_PointIndexAction.IndexChanged -= OnPointIndexChanged;
				m_PointIndexAction.End();
				m_PointIndexAction = null;
			}
		}

		void OnPathIndexChanged()
		{
			List<string> selectedIDs = m_PathIndexAction.GetSelectedIDs();
			if( selectedIDs.Count != 1 ) {
				return;
			}
			string newPathID = selectedIDs.First();
			if( newPathID == m_CurrentPathID ) {
				return;
			}
			string szOldPathID = m_CurrentPathID;
			SetupPath( newPathID );

			// cleanup old state
			DeactivatePointSelection( szOldPathID );

			// activate point selection for new path
			ActivatePointSelection();
		}

		void OnPointIndexChanged( int nIndex, TopoDS_Shape selectedVertex )
		{
			m_CraftData.StartPointIndex = nIndex;

			// update coupler craft data
			if( m_DataManager.ObjectMap[ m_CurrentPathID ] is StdPatternObjectBase stdPatternObject && stdPatternObject.GeomData != null ) {
				StdPatternCraftCoupler stdPatternCraftCoupler = new StdPatternCraftCoupler();
				stdPatternCraftCoupler.HandleCouplerCraftForStartPoint( ref m_CraftData, stdPatternObject.GeomData );
			}
			PropertyChanged?.Invoke( new List<string>() { m_CurrentPathID } );
			m_Viewer.GetAISContext().ClearSelected( true );
		}

		// path index control
		string m_CurrentPathID;
		CraftData m_CraftData;

		// composition references
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;
		SelectPathAction m_PathIndexAction;
		IndexSelectAction m_PointIndexAction;
	}
}
