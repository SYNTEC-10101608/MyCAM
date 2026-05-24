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
			if( viewer == null || treeView == null || viewManager == null || pathIndexAction == null ) {
				throw new ArgumentNullException( "MicroJointAction constructing argument null" );
			}
			if( string.IsNullOrEmpty( pathID ) ) {
				throw new ArgumentException( "MicroJointAction constructing argument pathID invalid" );
			}
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;
			m_PathIndexAction = pathIndexAction;
			m_CurrentPathID = pathID;

			m_PathIDList = new List<string>() { pathID };
			if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
				throw new ArgumentException( "MicroJointAction constructing argument pathID invalid path ID" );
			}
			m_CraftData = craftData;
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

			// start path index action
			m_PathIndexAction.Start();
			m_PathIndexAction.SelectionChange += OnPathIndexChanged;

			m_Dialog = new MicroJointDlg();
			m_Dialog.ClearAllAction = OnClearAllClicked;
			m_Dialog.DeleteAction = OnDeleteClicked;
			m_Dialog.LengthChangedAction = OnLengthChanged;
			m_Dialog.Cancel += End;
			m_Dialog.Show( MyApp.MainForm );

			// change dlg button state
			DisableDlgDeleteButton( true );
			ChangeClearAllState();

			// activate point selection for initial path
			ActivatePointSelection();
		}

		public override void End()
		{
			DeactivatePointSelection();

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

		void ActivatePointSelection()
		{
			m_PathIndexAction.ExcludeFromSelection( m_CurrentPathID );
			CreatePointIndexAction( m_CurrentPathID );
			m_CurrentSelectIndex = DEFAULT_UnselectIdx;
			DisableDlgDeleteButton( true );
			ChangeClearAllState();
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
				PropertyChanged?.Invoke( m_PathIDList );
			}

			// this pnt is micro joint start pnt
			DisableDlgDeleteButton( false );
			ChangeClearAllState();
		}

		const int DEFAULT_UnselectIdx = -1;
		const double DEFAULT_Length = 2;
		MicroJointDlg m_Dialog;
		int m_CurrentSelectIndex = DEFAULT_UnselectIdx;
		CraftData m_CraftData;
		List<string> m_PathIDList;
		string m_CurrentPathID;

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
			PropertyChanged?.Invoke( m_PathIDList );
			DisableDlgDeleteButton( true );
			ChangeClearAllState();
		}

		void OnDeleteClicked()
		{
			if( m_CurrentSelectIndex != DEFAULT_UnselectIdx && m_CraftData.MicroJointStartIdxMap.ContainsKey( m_CurrentSelectIndex ) ) {
				m_CraftData.RemoveMicroJointStartIdx( m_CurrentSelectIndex );
				PropertyChanged?.Invoke( m_PathIDList );
				DisableDlgDeleteButton( true );
				ChangeClearAllState();
			}
		}

		void OnLengthChanged( double newLength )
		{
			if( m_CurrentSelectIndex != DEFAULT_UnselectIdx && m_CraftData.MicroJointStartIdxMap.ContainsKey( m_CurrentSelectIndex ) ) {
				m_CraftData.AddMicroJointStartIdx( m_CurrentSelectIndex, newLength );
				PropertyChanged?.Invoke( m_PathIDList );
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

