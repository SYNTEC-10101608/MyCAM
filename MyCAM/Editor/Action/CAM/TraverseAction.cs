using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Renderer;
using MyCAM.Helper;
using MyCAM.PathCache;
using OCC.gp;
using OCCViewer;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor
{
	internal class TraverseAction : EditCAMActionBase
	{
		public TraverseAction( DataManager dataManager, List<string> pathIDList, Viewer viewer )
			: base( dataManager, pathIDList )
		{
			m_Viewer = viewer;

			// checked in base constructor
			// when user cancel the traverse setting, need to turn path back
			m_BackupTraverseDataList = new List<TraverseData>();
			foreach( var craftData in m_CraftDataList ) {
				if( craftData == null ) {
					throw new ArgumentNullException( "TraverseAction constructing argument craftData contains null craftData" );
				}
				if( craftData.TraverseData == null ) {
					m_BackupTraverseDataList.Add( new TraverseData() );
				}
				else {
					m_BackupTraverseDataList.Add( craftData.TraverseData.Clone() );
				}
			}
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.SetTraverse;
			}
		}

		public override void Start()
		{
			base.Start();

			// TODO: check all CAMData has same traverse param or not
			m_Dlg = new TraverseDlg( m_BackupTraverseDataList[ 0 ].Clone() );
			m_Dlg.Confirm = ConfirmTraverseData;
			m_Dlg.Preview = PreviewTraverseData;
			m_Dlg.Cancel = CancelTraverseData;
			PropertyChanged?.Invoke( m_PathIDList );
			m_Dlg.Show( MyApp.MainForm );
		}

		public override void End()
		{
			if( m_IsConfirmed == false ) {
				RestoreBackupTraverseDatas();
			}
			RemoveSafeBoundaryVisualization();
			PropertyChanged?.Invoke( m_PathIDList );
			if( m_Dlg != null ) {
				m_Dlg.Preview = null;
				m_Dlg.Confirm = null;
				m_Dlg.Cancel = null;
				m_Dlg.Close();
				m_Dlg = null;
			}
			base.End();
		}

		void PreviewTraverseData( TraverseData data )
		{
			SetTraverseData( data );

			RemoveSafeBoundaryVisualization();

			if( data.TraverseMode != TraverseData.TraverseType.FrogLeap ) {
				DrawSafeBoundaryVisualization( data );
			}

			PropertyChanged?.Invoke( m_PathIDList );
		}

		void ConfirmTraverseData( TraverseData data )
		{
			SetTraverseData( data );
			m_IsConfirmed = true;
			End();
		}

		void CancelTraverseData()
		{
			End();
		}

		void SetTraverseData( TraverseData data )
		{
			foreach( var camData in m_CraftDataList ) {
				camData.TraverseData = data.Clone();
			}
		}

		void RestoreBackupTraverseDatas()
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].TraverseData = m_BackupTraverseDataList[ i ].Clone();
			}
		}

		void RemoveSafeBoundaryVisualization()
		{
			m_ShapeRenderer?.Remove();
			m_ShapeRenderer = null;
			m_ShapeRendererMode = null;
		}

		void DrawSafeBoundaryVisualization( TraverseData data )
		{
			gp_Ax3 axes = data.TraverseMode == TraverseData.TraverseType.SafePlane
				? null : new gp_Ax3( new gp_Pnt( 0, 0, 0 ), TraverseHelper.GetSafeBoundaryAxis() );
			List<TraversePathResult> results = CollectTraverseResults( data, axes?.Direction() );

			switch( data.TraverseMode ) {
				case TraverseData.TraverseType.SafePlane:
					if( !TraverseHelper.TryGetPlanePreviewBounds( results,
						out double minX, out double maxX, out double minY, out double maxY ) ) {
						return;
					}
					PlaneShape3DRenderer plane = new PlaneShape3DRenderer( m_Viewer );
					plane.SetGeometry( data.SafePlaneDistance, minX, maxX, minY, maxY );
					m_ShapeRenderer = plane;
					break;

				case TraverseData.TraverseType.SafeCylinder:
				case TraverseData.TraverseType.SafeSphere:
					bool isSphere = data.TraverseMode == TraverseData.TraverseType.SafeSphere;
					double radius = isSphere ? data.SafeSphereRadius : data.SafeCylinderRadius;

					if( !TraverseHelper.TryGetRadialPreviewBounds( results, axes, radius, isSphere,
						out double minU, out double maxU, out double minV, out double maxV ) ) {
						return;
					}
					RadialShape3DRendererBase radial = new RadialShape3DRendererBase( m_Viewer, isSphere );

					radial.SetGeometry( axes, radius, minU, maxU, minV, maxV );
					m_ShapeRenderer = radial;

					break;
			}
			m_ShapeRendererMode = data.TraverseMode;
			m_ShapeRenderer.Show();
		}

		List<TraversePathResult> CollectTraverseResults( TraverseData data, gp_Dir boundaryAxis )
		{
			var results = new List<TraversePathResult>();
			foreach( string pathID in m_PathIDList ) {
				int pathIndex = m_DataManager.PathIDList.IndexOf( pathID );
				if( pathIndex <= 0 ) {
					continue;
				}
				string previousPathID = m_DataManager.PathIDList[ pathIndex - 1 ];
				if( TraverseHelper.TryCalculateTraversePoints( CacheHelper.GetProcessEndPoint( previousPathID ),
					CacheHelper.GetProcessStartPoint( pathID ), data, boundaryAxis, out TraversePathResult result ) ) {
					results.Add( result );
				}
			}
			return results;
		}

		Viewer m_Viewer;
		Shape3DRendererBase m_ShapeRenderer;
		TraverseData.TraverseType? m_ShapeRendererMode;
		List<TraverseData> m_BackupTraverseDataList;
		TraverseDlg m_Dlg;
		bool m_IsConfirmed = false;
	}
}
