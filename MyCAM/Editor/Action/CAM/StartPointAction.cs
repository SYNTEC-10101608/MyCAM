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
			if( viewer == null || treeView == null || viewManager == null || pathIndexAction == null ) {
				throw new ArgumentNullException( "StartPointAction constructing argument null" );
			}
			if( string.IsNullOrEmpty( pathID ) ) {
				throw new ArgumentException( "StartPointAction constructing argument pathID invalid" );
			}
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;
			m_PathIndexAction = pathIndexAction;
			m_CurrentPathID = pathID;

			m_PathIDList = new List<string>() { pathID };
			if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
				throw new ArgumentException( "StartPointAction constructing argument pathID invalid path ID" );
			}
			m_CraftData = craftData;
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

			// start path index action
			m_PathIndexAction.Start();
			m_PathIndexAction.SelectionChange += OnPathIndexChanged;

			// register ESC key
			m_Viewer.KeyDown += OnViewerKeyDown;

			// activate point selection for initial path
			ActivatePointSelection();
		}

		public override void End()
		{
			DeactivatePointSelection();

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

		void ActivatePointSelection()
		{
			m_PathIndexAction.ExcludeFromSelection( m_CurrentPathID );
			CreatePointIndexAction( m_CurrentPathID );
		}

		void DeactivatePointSelection()
		{
			DestroyPointIndexAction();
			m_PathIndexAction.RestoreFromExclusion( m_CurrentPathID );
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

			// validate new path
			if( !DataGettingHelper.GetCraftDataByID( newPathID, out CraftData newCraftData ) ) {
				return;
			}

			// cleanup old state
			DeactivatePointSelection();

			// switch to new path
			m_CurrentPathID = newPathID;
			m_CraftData = newCraftData;
			m_PathIDList = new List<string>() { newPathID };

			// activate point selection for new path
			ActivatePointSelection();
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

		void OnPointIndexChanged( int nIndex, TopoDS_Shape selectedVertex )
		{
			m_CraftData.StartPointIndex = nIndex;

			// update coupler craft data
			if( m_DataManager.ObjectMap[ m_CurrentPathID ] is StdPatternObjectBase stdPatternObject && stdPatternObject.GeomData != null ) {
				StdPatternCraftCoupler stdPatternCraftCoupler = new StdPatternCraftCoupler();
				stdPatternCraftCoupler.HandleCouplerCraftForStartPoint( ref m_CraftData, stdPatternObject.GeomData );
			}
			PropertyChanged?.Invoke( m_PathIDList );
			m_Viewer.GetAISContext().ClearSelected( true );
		}

		CraftData m_CraftData;
		List<string> m_PathIDList;
		string m_CurrentPathID;

		// composition references
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;
		SelectPathAction m_PathIndexAction;
		IndexSelectAction m_PointIndexAction;
	}
}
