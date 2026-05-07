using MyCAM.App;
using MyCAM.Data;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class MicroJointAction : IndexSelectAction
	{
		public MicroJointAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, string pathID )
			: base( dataManager, viewer, treeView, viewManager, pathID )
		{
			// checked in base constructor
			m_PathIDList = new List<string>() { pathID };
			m_CraftData = ( m_DataManager.ObjectMap[ m_PathID ] as PathObject ).CraftData;
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
			m_Dialog = new MicroJointDlg();
			m_Dialog.ClearAllAction = OnClearAllClicked;
			m_Dialog.DeleteAction = OnDeleteClicked;
			m_Dialog.LengthChangedAction = OnLengthChanged;
			m_Dialog.Cancel += End;
			m_Dialog.Show( MyApp.MainForm );

			// change dlg button state
			DisableDlgDeleteButton( true );
			ChangeClearAllState();
		}

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			if( e.Button != MouseButtons.Left ) {
				return;
			}
			int? nIndex = GetSelectIndex( out _ );
			m_CurrentSelectIndex = nIndex ?? DEFAULT_UnselectIdx;
			if( nIndex == null ) {
				return;
			}
			m_Viewer.GetAISContext().ClearSelected( true );

			// check if the selected point is already a micro joint start point
			if( m_CraftData.MicroJointStartIdxMap.ContainsKey( nIndex.Value ) ) {
				double currentLength = m_CraftData.MicroJointStartIdxMap[ nIndex.Value ];
				if( m_Dialog != null && !m_Dialog.IsDisposed ) {
					m_Dialog.SetMicroJointLength( currentLength );
				}
			}
			else {
				double newLength = m_Dialog?.GetMicroJointLength() ?? DEFAULT_Length;
				m_CraftData.AddMicroJointStartIdx( nIndex.Value, newLength );
				PropertyChanged?.Invoke( m_PathIDList );
			}

			// this pnt is micro joint start pnt
			DisableDlgDeleteButton( false );
			ChangeClearAllState();
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		public override void End()
		{
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

		const int DEFAULT_UnselectIdx = -1;
		const double DEFAULT_Length = 2;
		MicroJointDlg m_Dialog;
		int m_CurrentSelectIndex = DEFAULT_UnselectIdx;
		CraftData m_CraftData;
		List<string> m_PathIDList;

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

