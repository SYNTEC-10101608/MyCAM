using MyCAM.App;
using MyCAM.Data;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class MicroJointAction : EditActionBase
	{
		public MicroJointAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, string pathID, SelectPathAction pathIndexAction )
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
				return EditActionType.MicroJoint;
			}
		}

		public Action<List<string>> PropertyChanged;

		public override void Start()
		{
			base.Start();

			// setup initial path
			SetupPath( m_CurrentPathID );

			// create dialog
			m_Dialog = new MicroJointDlg();
			m_Dialog.ClearAllAction = OnClearAllClicked;
			m_Dialog.DeleteAction = OnDeleteClicked;
			m_Dialog.LengthChangedAction = OnLengthChanged;
			m_Dialog.Cancel += End;
			m_Dialog.Show( MyApp.MainForm );

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

			// this end is triiger by other editor will need to close Dlg
			if( m_Dialog != null && !m_Dialog.IsDisposed ) {

				// avoid other event cause error after dialog closed
				m_Dialog.Cancel -= End;
				m_Dialog.Close();
				m_Dialog.Dispose();
				m_Dialog = null;
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
		}

		void ActivatePointSelection()
		{
			m_PathIndexAction.ExcludeFromSelection( m_CurrentPathID );
			CreatePointIndexAction( m_CurrentPathID );
			m_CurrentSelectIndex = DEFAULT_UnselectIdx;
			DisableDlgDeleteButton( true );
			ChangeClearAllState();
		}

		void DeactivatePointSelection( string szOldPathID )
		{
			m_CurrentSelectIndex = DEFAULT_UnselectIdx;

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
			m_CurrentSelectIndex = nIndex;
			m_Viewer.GetAISContext().ClearSelected( true );

			// check if the selected point is already a micro joint start point
			if( m_CraftData.MicroJointStartIdxMap.ContainsKey( nIndex ) ) {
				double currentLength = m_CraftData.MicroJointStartIdxMap[ nIndex ];
				if( m_Dialog != null && !m_Dialog.IsDisposed ) {
					m_Dialog.SetMicroJointLength( currentLength );
				}
			}
			else {
				double newLength = m_Dialog?.GetMicroJointLength() ?? DEFAULT_Length;
				m_CraftData.AddMicroJointStartIdx( nIndex, newLength );
				PropertyChanged?.Invoke( new List<string>() { m_CurrentPathID } );
			}

			// this pnt is micro joint start pnt
			DisableDlgDeleteButton( false );
			ChangeClearAllState();
		}

		const int DEFAULT_UnselectIdx = -1;
		const double DEFAULT_Length = 2;

		// path index control
		string m_CurrentPathID;
		CraftData m_CraftData;

		// point index control
		int m_CurrentSelectIndex = DEFAULT_UnselectIdx;

		// action utility
		MicroJointDlg m_Dialog;

		// composition references
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;
		SelectPathAction m_PathIndexAction;
		IndexSelectAction m_PointIndexAction;

		#region UI event

		void OnClearAllClicked()
		{
			m_CraftData.ClearMicroJointStartIdx();
			PropertyChanged?.Invoke( new List<string>() { m_CurrentPathID } );
			DisableDlgDeleteButton( true );
			ChangeClearAllState();
		}

		void OnDeleteClicked()
		{
			if( m_CurrentSelectIndex != DEFAULT_UnselectIdx && m_CraftData.MicroJointStartIdxMap.ContainsKey( m_CurrentSelectIndex ) ) {
				m_CraftData.RemoveMicroJointStartIdx( m_CurrentSelectIndex );
				PropertyChanged?.Invoke( new List<string>() { m_CurrentPathID } );
				DisableDlgDeleteButton( true );
				ChangeClearAllState();
			}
		}

		void OnLengthChanged( double newLength )
		{
			if( m_CurrentSelectIndex != DEFAULT_UnselectIdx && m_CraftData.MicroJointStartIdxMap.ContainsKey( m_CurrentSelectIndex ) ) {
				m_CraftData.AddMicroJointStartIdx( m_CurrentSelectIndex, newLength );
				PropertyChanged?.Invoke( new List<string>() { m_CurrentPathID } );
			}
		}

		#endregion

		#region UI Protection

		void ChangeClearAllState()
		{
			// this path do not have micro joint
			if( m_CraftData.MicroJointStartIdxMap.Count == 0 ) {

				// can't click clear all
				m_Dialog.DisableClearAllButton( true );
				return;
			}
			m_Dialog.DisableClearAllButton( false );
		}

		void DisableDlgDeleteButton( bool bNeedDisable )
		{
			m_Dialog.DisableDeleteButton( bNeedDisable );
		}

		#endregion
	}
}

