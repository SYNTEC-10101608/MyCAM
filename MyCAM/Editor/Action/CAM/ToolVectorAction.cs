﻿using System;
using System.Windows.Forms;
using MyCAM.App;
using MyCAM.Data;
using OCC.AIS;
using OCC.Aspect;
using OCC.Prs3d;
using OCC.Quantity;
using OCC.TopoDS;
using OCCViewer;

namespace MyCAM.Editor
{
	internal class ToolVectorAction : IndexSelectAction
	{
		public ToolVectorAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager,
			CAMData camData )
			: base( dataManager, viewer, treeView, viewManager, camData )
		{
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.ToolVec;
			}
		}

		public Action PropertyChanged;
		public Action<EActionStatus> RaiseEditingToolVecDlg;

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			// editing tool vector, do not allow other operation
			if( IsPausedSelectMode ) {
				return;
			}
			if( e.Button != MouseButtons.Left ) {
				return;
			}
			int nIndex = GetSelectIndex( out TopoDS_Shape selectedVertex );
			if( nIndex == -1 ) {
				return;
			}

			// modify tool vector
			bool isModified = m_CAMData.GetToolVecModify( nIndex, out double angleA_deg, out double angleB_deg );
			ToolVecParam toolVecParam = new ToolVecParam( isModified, angleA_deg, angleB_deg );

			// back up old data
			m_BackupToolVecParam = new ToolVecParam( isModified, angleA_deg, angleB_deg );
			ToolVectorDlg toolVecForm = new ToolVectorDlg( toolVecParam );
			toolVecForm.Preview += ( ToolVec ) =>
			{
				SetToolVecParam( nIndex, ToolVec );
				PropertyChanged?.Invoke();
			};
			toolVecForm.Confirm += ( ToolVec ) =>
			{
				SetToolVecParam( nIndex, ToolVec );
				SetToolVecDone();
			};
			toolVecForm.Cancel += () =>
			{
				SetToolVecParam( nIndex, m_BackupToolVecParam );
				SetToolVecDone();
			};

			// when editing a point lock the main form
			RaiseEditingToolVecDlg?.Invoke( EActionStatus.Start );

			// when editing a point, cannot select other points but still show selected point
			LockSelectedVertexHighLight( selectedVertex );
			toolVecForm.Show( MyApp.MainForm );
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			// editing tool vector, do not allow other operation
			if( IsPausedSelectMode ) {
				return;
			}
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		void LockSelectedVertexHighLight( TopoDS_Shape selectedVertex )
		{
			if( selectedVertex == null || selectedVertex.IsNull()) {
				return;
			}

			// show selected vertex on viewer, because pause select mode woud clear select
			DrawVertexOnViewer( selectedVertex );
			Pause();
		}

		void UnlockSelectedVertexHighLight()
		{
			// remove selected vertex which show on viewer
			if( m_KeepedHighLightPoint != null ) {

				// true means update viewer
				m_Viewer.GetAISContext().Remove( m_KeepedHighLightPoint, true );
				m_KeepedHighLightPoint = null;
			}
			Resume();
		}

		void SetToolVecDone()
		{
			// unlock main form
			RaiseEditingToolVecDlg?.Invoke( EActionStatus.End );

			// unlock viewer
			UnlockSelectedVertexHighLight();
			PropertyChanged?.Invoke();
		}

		void SetToolVecParam( int VecIndex, ToolVecParam toolVecParam )
		{
			if( !toolVecParam.IsModified ) {
				m_CAMData.RemoveToolVecModify( VecIndex );
				return;
			}
			m_CAMData.SetToolVecModify( VecIndex, toolVecParam.AngleA_deg, toolVecParam.AngleB_deg );
		}

		void DrawVertexOnViewer( TopoDS_Shape selectedVertex )
		{
			if( selectedVertex != null ) {
				m_KeepedHighLightPoint = new AIS_Shape( selectedVertex );

				// set pointAspect
				Prs3d_PointAspect pointAspect = new Prs3d_PointAspect(
					Aspect_TypeOfMarker.Aspect_TOM_BALL,
					new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ),
					3.0f
				);

				// set high light attribute
				m_KeepedHighLightPoint.Attributes().SetPointAspect( pointAspect );

				// refresh viewer
				m_Viewer.GetAISContext().Display( m_KeepedHighLightPoint, true );
			}
		}

		ToolVecParam m_BackupToolVecParam;

		// to storage which vertex keep show high light point on viewer
		AIS_Shape m_KeepedHighLightPoint = null;
	}
}
