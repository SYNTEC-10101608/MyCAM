using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using MyCAM.Helper;
using OCC.gp;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class AutoSortPathAction : EditActionBase
	{
		public AutoSortPathAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, SelectPathAction selectPathAction )
			: base( dataManager )
		{
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;
			m_SelectPathAction = selectPathAction;
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.ManualOrder;
			}
		}

		/// <summary>
		/// Raised when sort is completed. Carries the new path ID order.
		/// </summary>
		public Action<List<string>> SortCompleted;

		public override void Start()
		{
			base.Start();

			m_SelectPathAction.Start();
			m_SelectPathAction.SelectionChange += OnSelectionChanged;

			m_Dialog = new AutoSortPathDlg();
			m_Dialog.Confirm += OnConfirm;
			m_Dialog.Cancel += OnCancel;
			m_Dialog.Show( MyApp.MainForm );
		}

		public override void End()
		{
			m_SelectPathAction.SelectionChange -= OnSelectionChanged;
			m_SelectPathAction.End();
			base.End();
		}

		void OnSelectionChanged()
		{
			List<string> selectedIDs = m_SelectPathAction.GetSelectedIDs();
			string pathID = selectedIDs.Count > 0 ? selectedIDs.First() : null;
			m_Dialog?.UpdateSelectedPathStatus( pathID );
		}

		void OnConfirm( SortParams sortParams )
		{
			try {
				List<string> pathIDList = new List<string>( m_DataManager.PathIDList );
				List<string> result = null;

				switch( sortParams.Method ) {
					case SortMethod.ShortestPath:
						string startPathID = m_SelectPathAction.GetSelectedIDs().FirstOrDefault();
						result = PathSortHelper.SortByShortestPath( pathIDList, startPathID );
						break;

					case SortMethod.CylinderVertical:
						if( TryGetCylinderAxis( out gp_Dir axisDir1, out gp_Pnt axisLoc1 ) ) {
							result = PathSortHelper.SortByRotationThenExtrusion(
								pathIDList, axisDir1, axisLoc1,
								sortParams.IsRotationDescending,
								sortParams.IsExtrusionDescending );
						}
						else {
							System.Diagnostics.Debug.WriteLine( "[AutoSortPathAction] Failed to get cylinder axis for vertical sort" );
						}
						break;

					case SortMethod.CylinderHorizontal:
						if( TryGetCylinderAxis( out gp_Dir axisDir2, out gp_Pnt axisLoc2 ) ) {
							result = PathSortHelper.SortByExtrusionThenRotation(
								pathIDList, axisDir2, axisLoc2,
								sortParams.IsExtrusionDescending,
								sortParams.IsRotationDescending );
						}
						else {
							System.Diagnostics.Debug.WriteLine( "[AutoSortPathAction] Failed to get cylinder axis for horizontal sort" );
						}
						break;
				}

				if( result != null && result.Count > 0 ) {
					SortCompleted?.Invoke( result );
				}
				else {
					System.Diagnostics.Debug.WriteLine( "[AutoSortPathAction] Sort returned no results" );
				}
			}
			catch( Exception ex ) {
				System.Diagnostics.Debug.WriteLine( $"[AutoSortPathAction] OnConfirm failed: {ex.Message}\n{ex.StackTrace}" );
				System.Windows.Forms.MessageBox.Show(
					$"¸ô®|±Æ§Ç¥¢±Ñ: {ex.Message}",
					"¿ù»~",
					System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Error );
			}
			finally {
				End();
			}
		}

		void OnCancel()
		{
			End();
		}

		bool TryGetCylinderAxis( out gp_Dir axisDir, out gp_Pnt axisLocation )
		{
			axisDir = null;
			axisLocation = null;
			if( !DataGettingHelper.GetMachineData( out MachineData machineData ) ) {
				return false;
			}
			axisDir = machineData.MasterRotateDir;

			// axis location from workpiece bounding box center, same as CAMEditor
			axisLocation = new gp_Pnt( 0, 0, 0 );
			BoundingBox bbox = GetVisibleWorkpieceBBox();
			if( bbox != null ) {
				axisLocation = new gp_Pnt( bbox.XCenter, bbox.YCenter, bbox.ZCenter );
			}
			return true;
		}

		BoundingBox GetVisibleWorkpieceBBox()
		{
			List<TopoDS_Shape> shapeList = new List<TopoDS_Shape>();
			foreach( string partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				if( DataGettingHelper.GetShapeObject( partID, out IShapeObject shapeObject ) == false ) {
					continue;
				}
				shapeList.Add( shapeObject.Shape );
			}
			if( shapeList.Count == 0 ) {
				return null;
			}
			TopoDS_Shape compound = ShapeTool.MakeCompound( shapeList );
			return new BoundingBox( compound );
		}

		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;
		SelectPathAction m_SelectPathAction;
		AutoSortPathDlg m_Dialog;
	}
}
