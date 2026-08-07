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
			if( !SetupPath( m_CurrentPathID ) ) {
				End();
				return;
			}

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

		bool SetupPath( string pathID )
		{
			if( string.IsNullOrEmpty( pathID ) ) {
				return false;
			}

			// check is closed path
			if( !DataGettingHelper.GetGeomDataByID( pathID, out IGeomData geomData ) ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetPathGeomInfo", CANNOT_GET_PATH_GEOM_INFO ), pathID ), MyApp.NoticeType.Warning );
				return false;
			}
			if( !geomData.IsClosed ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "PathNotClosed", PATH_NOT_CLOSED ), pathID ), MyApp.NoticeType.Warning );
				return false;
			}

			// init data for current path
			m_CurrentPathID = pathID;
			if( !DataGettingHelper.GetCraftDataByID( pathID, out m_CraftData ) ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetPathCraftInfo", CANNOT_GET_PATH_CRAFT_INFO ), pathID ), MyApp.NoticeType.Warning );
				return false;
			}
			return true;
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
			if( !SetupPath( newPathID ) ) {
				return;
			}

			// cleanup old state
			DeactivatePointSelection( szOldPathID );

			// activate point selection for new path
			ActivatePointSelection();
		}

		void OnPointIndexChanged( int nIndex, TopoDS_Shape selectedVertex )
		{
			m_CraftData.SetStartPntIndex( nIndex );

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

		const string CANNOT_GET_PATH_CRAFT_INFO = "無法獲取路徑 {0} 的加工資訊";
		const string CANNOT_GET_PATH_GEOM_INFO = "無法獲取路徑 {0} 的幾何資訊";
		const string PATH_NOT_CLOSED = "路徑 {0} 為非封閉路徑，無法設定起點";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "StartPointAction" );
	}
}
