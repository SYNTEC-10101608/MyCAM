using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Renderer;
using OCC.AIS;
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
			if( viewer == null || treeView == null || viewManager == null || pathIndexAction == null ) {
				throw new ArgumentNullException( "ContourEditAction constructing argument null" );
			}
			if( string.IsNullOrEmpty( pathID ) ) {
				throw new ArgumentException( "ContourEditAction constructing argument pathID invalid" );
			}
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;
			m_PathIndexAction = pathIndexAction;
			m_CurrentPathID = pathID;

			if( !DataGettingHelper.GetCraftDataByID( pathID, out m_CraftData ) ) {
				throw new ArgumentException( "Cannot get CraftData by pathID: " + pathID );
			}
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

			// start path index action
			m_PathIndexAction.Start();
			m_PathIndexAction.SelectionChange += OnPathIndexChanged;

			// create dialog
			m_Dlg = new ContourEditDlg();
			m_Dlg.AddEditIndex += OnAddEditIndex;
			m_Dlg.RemoveEditIndex += OnRemoveEditIndex;
			m_Dlg.DisplacementChanged += OnDisplacementChanged;
			m_Dlg.Cancel += End;

			// default to first point
			OnSelectedIndexChanged( 0 );
			m_Dlg.Show( MyApp.MainForm );

			// activate point selection for initial path
			ActivatePointSelection();
		}

		public override void End()
		{
			DeactivatePointSelection();

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

		void ActivatePointSelection()
		{
			m_PathIndexAction.ExcludeFromSelection( m_CurrentPathID );
			CreatePointIndexAction( m_CurrentPathID );

			// show edit point marks
			m_EditPointRenderer?.Remove( true );
			m_EditPointRenderer = new EditPointRenderer( m_Viewer, m_CurrentPathID );
			m_EditPointRenderer.Show( true );
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

			// validate new path (contour only)
			if( !DataGettingHelper.GetPathType( newPathID, out PathType pathType ) || pathType != PathType.Contour ) {
				return;
			}
			if( !DataGettingHelper.GetCraftDataByID( newPathID, out CraftData newCraftData ) ) {
				return;
			}

			// cleanup old state
			DeactivatePointSelection();

			// switch to new path
			m_CurrentPathID = newPathID;
			m_CraftData = newCraftData;
			m_nSelectIndex = NULL_SELECT_INDEX;
			m_Param = null;

			// activate point selection for new path
			ActivatePointSelection();

			// default to first point
			OnSelectedIndexChanged( 0 );
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

		void OnPointIndexChanged( int nSelectIndex, TopoDS_Shape selectedVertex )
		{
			if( nSelectIndex == m_nSelectIndex ) {
				return;
			}
			OnSelectedIndexChanged( nSelectIndex );
		}

		void OnSelectedIndexChanged( int nSelectIndex )
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

		// renderer
		EditPointRenderer m_EditPointRenderer = null;

		// edit param
		int m_nSelectIndex = NULL_SELECT_INDEX;
		ContourEditParam m_Param = null;
		CraftData m_CraftData;
		string m_CurrentPathID;

		// dlg
		ContourEditDlg m_Dlg = null;

		// interactive highlight
		AIS_Shape m_HighLightPoint = null;

		// constants
		const int NULL_SELECT_INDEX = -999;

		// composition references
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;
		SelectPathAction m_PathIndexAction;
		IndexSelectAction m_PointIndexAction;
	}
}
