using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Renderer;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class ContourEditAction : EditActionBase
	{
		public ContourEditAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, string pathID, SelectPathAction pathIndexAction )
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
				return EditActionType.ContourEdit;
			}
		}

		public Action<List<string>> PropertyChanged;

		public override void Start()
		{
			base.Start();

			// setup initial path
			SetupPath( m_CurrentPathID );

			// create dialog
			m_Dlg = new ContourEditDlg();
			m_Dlg.AddEditIndex += OnAddEditIndex;
			m_Dlg.RemoveEditIndex += OnRemoveEditIndex;
			m_Dlg.DisplacementChanged += OnDisplacementChanged;
			m_Dlg.Cancel += End;
			m_Dlg.Show( MyApp.MainForm );

			// start path index action
			m_PathIndexAction.Start();
			m_PathIndexAction.SelectionChange += OnPathIndexChanged;

			// activate point selection for initial path
			ActivatePointSelection();
		}

		public override void End()
		{
			DeactivatePointSelection( m_CurrentPathID );

			// cleanup path index action
			m_PathIndexAction.SelectionChange -= OnPathIndexChanged;
			m_PathIndexAction.End();

			m_EditPointRenderer?.Remove( true );

			// close dialog
			if( m_Dlg != null ) {
				m_Dlg.Cancel -= End;
				m_Dlg.Close();
				m_Dlg = null;
			}

			base.End();
		}

		void SetupPath( string pathID )
		{
			if( string.IsNullOrEmpty( pathID ) ) {
				End();
				return;
			}

			// validate new path (contour only)
			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) || pathType != PathType.Contour ) {
				MyApp.Logger.ShowOnLogPanel( $"路徑 {pathID} 不是輪廓路徑，無法編輯", MyApp.NoticeType.Warning );
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

			// show edit point marks
			m_EditPointRenderer?.Remove( true );
			m_EditPointRenderer = new EditPointRenderer( m_Viewer, m_CurrentPathID );
			m_EditPointRenderer.Show( true );
		}

		void ActivatePointSelection()
		{
			m_PathIndexAction.ExcludeFromSelection( m_CurrentPathID );
			CreatePointIndexAction( m_CurrentPathID );

			m_nSelectIndex = NULL_SELECT_INDEX;
			m_Param = null;

			OnSelectedPointIndexChanged( 0 );
		}

		void DeactivatePointSelection( string szOldPathID )
		{
			m_nSelectIndex = NULL_SELECT_INDEX;
			m_Param = null;

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

		void OnPointIndexChanged( int nSelectIndex, TopoDS_Shape selectedVertex )
		{
			if( nSelectIndex == m_nSelectIndex ) {
				return;
			}
			OnSelectedPointIndexChanged( nSelectIndex );
		}

		void OnSelectedPointIndexChanged( int nSelectIndex )
		{
			if( m_nSelectIndex == nSelectIndex ) {
				return;
			}
			m_nSelectIndex = nSelectIndex;

			// should not happen, but just in case
			if( nSelectIndex == NULL_SELECT_INDEX ) {
				m_Param = null;
				m_Dlg.ResetParam( null );
				return;
			}

			// refresh dialog
			GetParamFormIndex( nSelectIndex, out double dx, out double dy, out double dz, out bool isModified );
			m_Param = new ContourEditParam( dx, dy, dz, isModified );
			m_Dlg.ResetParam( m_Param );
		}

		void OnAddEditIndex()
		{
			if( m_nSelectIndex == NULL_SELECT_INDEX ) {
				return;
			}
			if( m_Param != null ) {
				m_Param.IsModified = true;
			}

			// commit to data with current (zero) displacement
			double dx = m_Param?.DX ?? 0;
			double dy = m_Param?.DY ?? 0;
			double dz = m_Param?.DZ ?? 0;
			m_CraftData.SetContourEditPoint( m_nSelectIndex, dx, dy, dz );
			PropertyChanged?.Invoke( new List<string>() { m_CurrentPathID } );
			m_EditPointRenderer?.Show( true );

			// refresh dialog
			m_Dlg.ResetParam( m_Param );
		}

		void OnRemoveEditIndex()
		{
			if( m_nSelectIndex == NULL_SELECT_INDEX ) {
				return;
			}
			if( m_Param != null ) {
				m_Param.IsModified = false;
				m_Param.DX = 0;
				m_Param.DY = 0;
				m_Param.DZ = 0;
			}
			m_CraftData.RemoveContourEditPoint( m_nSelectIndex );
			PropertyChanged?.Invoke( new List<string>() { m_CurrentPathID } );
			m_EditPointRenderer?.Show( true );

			// refresh dialog
			m_Dlg.ResetParam( m_Param );
		}

		void OnDisplacementChanged( double dx, double dy, double dz )
		{
			if( m_nSelectIndex == NULL_SELECT_INDEX || m_Param == null ) {
				return;
			}
			m_Param.DX = dx;
			m_Param.DY = dy;
			m_Param.DZ = dz;
			m_CraftData.SetContourEditPoint( m_nSelectIndex, dx, dy, dz );
			PropertyChanged?.Invoke( new List<string>() { m_CurrentPathID } );
			m_EditPointRenderer?.Show( true );

			// refresh dialog
			m_Dlg.ResetParam( m_Param );
		}

		void GetParamFormIndex( int nSelectIndex, out double dx, out double dy, out double dz, out bool isModified )
		{
			dx = 0;
			dy = 0;
			dz = 0;
			isModified = false;
			if( nSelectIndex == NULL_SELECT_INDEX ) {
				return;
			}
			isModified = m_CraftData.ContourEditMap.ContainsKey( nSelectIndex );
			if( isModified ) {
				ContourEditData data = m_CraftData.ContourEditMap[ nSelectIndex ];
				dx = data.DX;
				dy = data.DY;
				dz = data.DZ;
			}
			return;
		}

		// path index control
		string m_CurrentPathID;
		CraftData m_CraftData;

		// point index control
		int m_nSelectIndex = NULL_SELECT_INDEX;
		ContourEditParam m_Param = null;

		// action utility
		EditPointRenderer m_EditPointRenderer = null;
		ContourEditDlg m_Dlg = null;

		// null point index as -999, -1 is used for closed point index
		const int NULL_SELECT_INDEX = -999;

		// composition references
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;
		SelectPathAction m_PathIndexAction;
		IndexSelectAction m_PointIndexAction;
	}
}
