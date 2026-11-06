using LanguageManager;
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

			m_CalculationInfo = TraverseHelper.BuildTraverseCalculationInfo( dataManager );

			// checked in base constructor
			// when user cancel the traverse setting, need to turn path back
			m_BackupTraverseDataList = new List<TraverseData>();
			foreach( var craftData in m_CraftDataList ) {
				if( craftData == null ) {
					throw new ArgumentNullException( "TraverseAction constructing argument craftData contains null craftData" );
				}
				TraverseData backup = ( craftData.TraverseData == null ) ? new TraverseData() : craftData.TraverseData.Clone();
				InitializeSafeRadial( backup );
				m_BackupTraverseDataList.Add( backup );
			}
		}

		void InitializeSafeRadial( TraverseData traverseData )
		{
			bool initializeCylinder = traverseData.SafeCylinderRadius <= GEOMETRY_TOLERANCE;
			bool initializeSphere = traverseData.SafeSphereRadius <= GEOMETRY_TOLERANCE;
			if( !initializeCylinder && !initializeSphere ) {
				return;
			}

			List<IShapeObject> workpieceShapes = new List<IShapeObject>();
			foreach( string partID in m_DataManager.PartIDList ) {
				if( !DataGettingHelper.GetShapeObject( partID, out IShapeObject shapeObject ) ||
					( shapeObject as PartObject )?.Role != PartRole.Workpiece ) {
					continue;
				}
				workpieceShapes.Add( shapeObject );
			}

			if( workpieceShapes.Count == 0 ) {
				return;
			}

			if( initializeCylinder ) {
				traverseData.SafeCylinderRadius = TraverseHelper.GetBoundingMaxValue( workpieceShapes, m_CalculationInfo.SafeBoundaryAxes, false );
			}
			if( initializeSphere ) {
				traverseData.SafeSphereRadius = TraverseHelper.GetBoundingMaxValue( workpieceShapes, m_CalculationInfo.SafeBoundaryAxes, true );
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
			m_IsConfirmed = CollectTraverseResults( data, m_CalculationInfo, out _ );
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
		}

		void DrawSafeBoundaryVisualization( TraverseData data )
		{
			if( !CollectTraverseResults( data, m_CalculationInfo, out List<TraversePathResult> results ) ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotBuildTraverse", CANNOT_BUILD_TRAVERSE ), MyApp.NoticeType.Warning );
				return;
			}

			switch( data.TraverseMode ) {
				case TraverseData.TraverseType.SafePlane:
					if( !TraverseHelper.TryGetPlanePreviewBounds( results, out double minX, out double maxX, out double minY, out double maxY ) ) {
						MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotBuildTraverse", CANNOT_BUILD_TRAVERSE ), MyApp.NoticeType.Warning );
					}
					PlaneShape3DRenderer plane = new PlaneShape3DRenderer( m_Viewer, m_DataManager );

					if( !plane.SetGeometry( data.SafePlaneDistance, minX, maxX, minY, maxY ) ) {
						MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotBuildTraverse", CANNOT_BUILD_TRAVERSE ), MyApp.NoticeType.Warning );
					}
					m_ShapeRenderer = plane;

					break;

				case TraverseData.TraverseType.SafeCylinder:
				case TraverseData.TraverseType.SafeSphere:
					bool isSphere = data.TraverseMode == TraverseData.TraverseType.SafeSphere;
					double radius = isSphere ? data.SafeSphereRadius : data.SafeCylinderRadius;

					if( !TraverseHelper.TryGetRadialPreviewBounds( results, m_CalculationInfo.SafeBoundaryAxes, radius, isSphere,
						out double minU, out double maxU, out double minV, out double maxV ) ) {
						MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotBuildTraverse", CANNOT_BUILD_TRAVERSE ), MyApp.NoticeType.Warning );
					}
					RadialShape3DRenderer radial = new RadialShape3DRenderer( m_Viewer, m_DataManager, isSphere );

					if( !radial.SetGeometry( m_CalculationInfo.SafeBoundaryAxes, radius, minU, maxU, minV, maxV ) ) {
						MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotBuildTraverse", CANNOT_BUILD_TRAVERSE ), MyApp.NoticeType.Warning );
					}
					m_ShapeRenderer = radial;

					break;
			}
			m_ShapeRenderer.Show();
		}

		bool CollectTraverseResults( TraverseData data, TraverseCalculationInfo calculationData, out List<TraversePathResult> traverseResults )
		{
			traverseResults = new List<TraversePathResult>();
			foreach( string pathID in m_PathIDList ) {
				int pathIndex = m_DataManager.PathIDList.IndexOf( pathID );
				if( pathIndex <= 0 ) {
					continue;
				}
				string previousPathID = m_DataManager.PathIDList[ pathIndex - 1 ];
				if( TraverseHelper.TryCalculateTraversePoints( CacheHelper.GetProcessEndPoint( previousPathID ),
					CacheHelper.GetProcessStartPoint( pathID ), data, calculationData, out TraversePathResult result ) ) {
					traverseResults.Add( result );
				}
				else {
					return false;
				}
			}

			return true;
		}

		const double GEOMETRY_TOLERANCE = 1e-6;

		Viewer m_Viewer;
		Shape3DRendererBase m_ShapeRenderer;
		List<TraverseData> m_BackupTraverseDataList;
		TraverseCalculationInfo m_CalculationInfo;
		TraverseDlg m_Dlg;
		bool m_IsConfirmed = false;

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "TraverseAction" );
		const string CANNOT_BUILD_TRAVERSE = "無法產生空移";
	}
}
