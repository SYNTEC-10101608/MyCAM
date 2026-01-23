using MyCAM.App;
using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.gp;
using OCC.Quantity;
using OCC.TCollection;
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
			TraverseDlg traverseDataFrom = new TraverseDlg( m_BackupTraverseDataList[ 0 ].Clone() );
			PropertyChanged?.Invoke( m_PathIDList );
			traverseDataFrom.Confirm += ConfirmTraverseData;
			traverseDataFrom.Preview += PreviewTraverseData;
			traverseDataFrom.Cancel += CancelTraverseData;
			traverseDataFrom.Show( MyApp.MainForm );
		}

		void PreviewTraverseData( TraverseData data )
		{
			SetTraverseData( data );

			// Remove previous safe plane visualization
			RemoveSafePlaneVisualization();

			// show safe plane visualization
			if( data.IsSafePlaneEnable ) {
				DrawSafePlaneVisualization( data );
			}

			PropertyChanged?.Invoke( m_PathIDList );
		}

		void ConfirmTraverseData( TraverseData data )
		{
			SetTraverseData( data );
			RemoveSafePlaneVisualization();
			PropertyChanged?.Invoke( m_PathIDList );
			End();
		}

		void CancelTraverseData()
		{
			RestoreBackupTraverseDatas();
			RemoveSafePlaneVisualization();
			PropertyChanged?.Invoke( m_PathIDList );
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

		#region Safe Plane Visualization

		void RemoveSafePlaneVisualization()
		{
			if( m_SafePlaneAIS != null ) {
				m_Viewer.GetAISContext().Remove( m_SafePlaneAIS, false );
				m_SafePlaneLine = null;
			}
			if( m_SafePlaneArrow != null ) {
				m_Viewer.GetAISContext().Remove( m_SafePlaneArrow, false );
				m_SafePlaneArrow = null;
			}
			if( m_SafePlaneLine != null ) {
				m_Viewer.GetAISContext().Remove( m_SafePlaneLine, false );
				m_SafePlaneLine = null;
			}
			if( m_SafePlaneText != null ) {
				m_Viewer.GetAISContext().Remove( m_SafePlaneText, false );
				m_SafePlaneText = null;
			}

			m_Viewer.UpdateView();
		}

		void DrawSafePlaneVisualization( TraverseData currentData )
		{
			(List<gp_Pnt> projectionPoints, List<double> safePlaneDistances) = CollectProjectionPointsAndSafePlaneDistances( m_PathIDList );
			if( safePlaneDistances.Count == 0 || projectionPoints.Count == 0 ) {
				return;
			}

			double finalSafePlaneZ = DecideFinalSafePlaneZ( safePlaneDistances );
			DrawUnifiedSafePlane( projectionPoints, finalSafePlaneZ );
			AddOriginToPlaneArrowAndDistance( finalSafePlaneZ );
			m_Viewer.UpdateView();
		}

		(List<gp_Pnt> projectionPoints, List<double> safePlaneDistances) CollectProjectionPointsAndSafePlaneDistances( List<string> pathIDList )
		{
			List<gp_Pnt> projectionPoints = new List<gp_Pnt>();
			List<double> safePlaneDistances = new List<double>();

			foreach( string currentPathID in pathIDList ) {
				string previousPathID = GetPreviousPathIDInAllPaths( currentPathID );
				if( previousPathID == null ) {
					continue;
				}

				TraverseData traverseData = GetTraverseData( currentPathID );
				if( !traverseData.IsSafePlaneEnable ) {
					continue;
				}

				double safePlaneZ = (double)traverseData.SafePlaneDistance;
				safePlaneDistances.Add( safePlaneZ );

				if( TryGetProjectedPoints( previousPathID, currentPathID, safePlaneZ, traverseData, out gp_Pnt p8, out gp_Pnt p9 ) ) {
					projectionPoints.Add( p8 );
					projectionPoints.Add( p9 );
				}
			}

			return (projectionPoints, safePlaneDistances);
		}

		string GetPreviousPathIDInAllPaths( string currentPathID )
		{
			int indexInAllPaths = m_DataManager.PathIDList.IndexOf( currentPathID );
			if( indexInAllPaths > 0 ) {
				return m_DataManager.PathIDList[ indexInAllPaths - 1 ];
			}
			return null;
		}

		bool TryGetProjectedPoints( string previousPathID, string currentPathID, double safePlaneZ, TraverseData traverseData, out gp_Pnt outP6, out gp_Pnt outP7 )
		{
			outP6 = null;
			outP7 = null;

			IProcessPoint previousEndPoint = GetProcessEndPoint( previousPathID );
			IProcessPoint currentStartPoint = GetProcessStartPoint( currentPathID );

			if( !TraverseHelper.TryCalculateTraversePoints( previousEndPoint, currentStartPoint, traverseData, out TraverseHelper.TraversePathResult result ) ) {
				return false;
			}
			outP6 = result.SafePlaneLiftUpProjPoint;
			outP7 = result.SafePlaneCutDownProjPoint;
			return true;
		}

		double DecideFinalSafePlaneZ( List<double> safePlaneDistances )
		{
			double tolorance = 0.001;
			if( safePlaneDistances == null || safePlaneDistances.Count == 0 )
				return TraverseData.SAFE_PLANE_DISTANCE;

			double firstDistance = safePlaneDistances[ 0 ];
			for( int i = 1; i < safePlaneDistances.Count; i++ ) {
				if( Math.Abs( safePlaneDistances[ i ] - firstDistance ) > tolorance ) {

					// not same safe plane distance, use default
					return TraverseData.SAFE_PLANE_DISTANCE;
				}
			}
			return firstDistance;
		}

		void DrawUnifiedSafePlane( List<gp_Pnt> projectionPoints, double safePlaneZ )
		{
			if( projectionPoints == null || projectionPoints.Count == 0 ) {
				return;
			}

			var (minX, maxX, minY, maxY) = ComputeBounds( projectionPoints );
			ExpandBounds( ref minX, ref maxX, ref minY, ref maxY, minMargin: 10.0, marginFactor: 0.2 );
			m_SafePlaneAIS = DrawHelper.CreatePlaneAIS( minX, maxX, minY, maxY, safePlaneZ );
			if( m_SafePlaneAIS != null ) {
				m_Viewer.GetAISContext().Display( m_SafePlaneAIS, false );
				m_Viewer.GetAISContext().Deactivate( m_SafePlaneAIS );
			}
		}

		(double minX, double maxX, double minY, double maxY) ComputeBounds( List<gp_Pnt> points )
		{
			double minX = points[ 0 ].X();
			double maxX = minX;
			double minY = points[ 0 ].Y();
			double maxY = minY;

			for( int i = 1; i < points.Count; i++ ) {
				var p = points[ i ];
				minX = Math.Min( minX, p.X() );
				maxX = Math.Max( maxX, p.X() );
				minY = Math.Min( minY, p.Y() );
				maxY = Math.Max( maxY, p.Y() );
			}

			maxX = maxX < 0 ? 0 : maxX;
			maxY = maxY < 0 ? 0 : maxY;
			minX = minX > 0 ? 0 : minX;
			minY = minY > 0 ? 0 : minY;

			return (minX, maxX, minY, maxY);
		}

		void ExpandBounds( ref double minX, ref double maxX, ref double minY, ref double maxY, double minMargin, double marginFactor )
		{
			double rangeX = maxX - minX;
			double rangeY = maxY - minY;
			double margin = Math.Max( rangeX, rangeY ) * marginFactor;
			if( margin < minMargin ) {
				margin = minMargin;
			}
			minX -= margin;
			maxX += margin;
			minY -= margin;
			maxY += margin;
		}

		void AddOriginToPlaneArrowAndDistance( double safePlaneZ )
		{
			gp_Pnt origin = new gp_Pnt( 0, 0, 0 );
			gp_Pnt planePoint = new gp_Pnt( 0, 0, safePlaneZ );

			m_SafePlaneLine = DrawHelper.GetLineAIS( origin, planePoint, Quantity_NameOfColor.Quantity_NOC_ORANGE, 2, false );
			m_Viewer.GetAISContext().Display( m_SafePlaneLine, false );
			m_Viewer.GetAISContext().Deactivate( m_SafePlaneLine );

			AddArrowHead( origin, planePoint );
			AddDistanceText( origin, planePoint, safePlaneZ );
		}

		void AddArrowHead( gp_Pnt startPoint, gp_Pnt endPoint )
		{
			gp_Vec direction = new gp_Vec( startPoint, endPoint );
			double length = direction.Magnitude();
			if( length < 1e-6 )
				return;
			direction.Normalize();
			gp_Pnt arrowStartPoint = new gp_Pnt( endPoint.X(), endPoint.Y(), endPoint.Z() - direction.Z() * arrowHeight );

			m_SafePlaneArrow = DrawHelper.GetOrientationAIS( arrowStartPoint, new gp_Dir( direction ), arrowRadius, arrowHeight, Quantity_NameOfColor.Quantity_NOC_ORANGE, AIS_DisplayMode.AIS_Shaded );
			m_SafePlaneArrow.SetWidth( 3 );
			m_Viewer.GetAISContext().Display( m_SafePlaneArrow, false );
			m_Viewer.GetAISContext().Deactivate( m_SafePlaneArrow );
		}

		void AddDistanceText( gp_Pnt startPoint, gp_Pnt endPoint, double distance )
		{
			gp_Pnt textPosition = new gp_Pnt(
				endPoint.X(),
				endPoint.Y(),
				( endPoint.Z() + 5 )
			);
			string distanceText = string.Format( "Z={0:F2}", distance );

			m_SafePlaneText = new AIS_TextLabel();
			m_SafePlaneText.SetText( new TCollection_ExtendedString( distanceText ) );
			m_SafePlaneText.SetPosition( textPosition );
			m_SafePlaneText.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BROWN ) );
			m_SafePlaneText.SetHeight( 15 );

			m_Viewer.GetAISContext().Display( m_SafePlaneText, false );
			m_Viewer.GetAISContext().Deactivate( m_SafePlaneText );
		}

		IProcessPoint GetProcessStartPoint( string pathID )
		{
			if( !PathCacheProvider.TryGetTraverseDataCache( pathID, out ITraverseDataCache traverseDataCache ) ) {
				return null;
			}
			return traverseDataCache.GetProcessStartPoint();
		}

		IProcessPoint GetProcessEndPoint( string pathID )
		{
			if( !PathCacheProvider.TryGetTraverseDataCache( pathID, out ITraverseDataCache traverseDataCache ) ) {
				return null;
			}
			return traverseDataCache.GetProcessEndPoint();
		}

		TraverseData GetTraverseData( string pathID )
		{
			if( !PathCacheProvider.TryGetTraverseDataCache( pathID, out ITraverseDataCache traverseDataCache ) ) {
				return new TraverseData();
			}
			return traverseDataCache.TraverseData;
		}

		#endregion

		AIS_Shape m_SafePlaneAIS = null;
		AIS_Shape m_SafePlaneArrow = null;
		AIS_Line m_SafePlaneLine = null;
		AIS_TextLabel m_SafePlaneText = null;
		Viewer m_Viewer;
		double arrowHeight = 5.0;
		double arrowRadius = 2.0;
		List<TraverseData> m_BackupTraverseDataList;
	}
}
