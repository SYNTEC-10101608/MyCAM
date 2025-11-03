using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.Data;
using OCC.gp;

namespace MyCAM.Helper
{
	internal static class BuildCAMSegmentHelper
	{
		public static bool BuildCAMSegment( CAMData camdata, out List<ICAMSegmentElement> camSegmentList )
		{
			camSegmentList = new List<ICAMSegmentElement>();
			if( camdata.CADSegmentList == null || camdata.CADSegmentList.Count == 0 ) {
				return false;
			}

			// forward arrangement
			camSegmentList = GetOrderedSegment( camdata );
			if( camdata.IsReverse ) {
				ReverseCAMSegmentList( ref camSegmentList );
			}
			return true;
		}

		public static CAMPoint GetCAMPoint( CADPoint cadPoint, bool isToolVecReverse )
		{
			if( cadPoint == null ) {
				return null;
			}
			gp_Dir toolDir = isToolVecReverse ? cadPoint.NormalVec_1st.Reversed() : cadPoint.NormalVec_1st;
			CAMPoint camPoint = new CAMPoint( cadPoint, toolDir );
			return camPoint;
		}

		public static gp_Vec GetModifyToolVecByMap(CAMData camData, (int,int) nTargetPnt)
		{
			CADPoint targetCADPnt = camData.CADSegmentList[ nTargetPnt.Item1 ].PointList[ nTargetPnt.Item2 ];
			CAMPoint targetCAMPnt = GetCAMPoint( targetCADPnt, camData.IsToolVecReverse );
			camData.ToolVecModifyMap_New.TryGetValue( nTargetPnt, out var value1 );
			gp_Vec startVec = GetVecFromAB( targetCAMPnt,
				value1.Item1 * Math.PI / 180,
				value1.Item2 * Math.PI / 180
				);
			return startVec;
		}

		public static gp_Vec GetVecFromAB( CAMPoint camPoint, double dRA_rad, double dRB_rad )
		{
			// TDOD: RA == 0 || RB == 0
			if( dRA_rad == 0 && dRB_rad == 0 ) {
				return new gp_Vec( camPoint.ToolVec );
			}

			// get the x, y, z direction
			gp_Dir x = camPoint.CADPoint.TangentVec;
			gp_Dir z = camPoint.CADPoint.NormalVec_1st;
			gp_Dir y = z.Crossed( x );

			// X:Y:Z = tanA:tanB:1
			double X = 0;
			double Y = 0;
			double Z = 0;
			if( dRA_rad == 0 ) {
				X = 0;
				Z = 1;
			}
			else {
				X = dRA_rad < 0 ? -1 : 1;
				Z = X / Math.Tan( dRA_rad );
			}
			Y = Z * Math.Tan( dRB_rad );
			gp_Dir dir1 = new gp_Dir( x.XYZ() * X + y.XYZ() * Y + z.XYZ() * Z );
			return new gp_Vec( dir1.XYZ() );
		}
		static List<ICAMSegmentElement> GetOrderedSegment( CAMData camdata )
		{
			List<ICAMSegmentElement> camSegmentList = new List<ICAMSegmentElement>();
			(int, int) startPointIndex = camdata.NewStartPoint;

			// segment - line
			if( camdata.CADSegmentList[ startPointIndex.Item1 ].ContourType == EContourType.Line ) {

				// separate start index segment
				List<LineCAMSegment> startSegmentSeparatedList = SeparateCADLineSegmentToCAMSegment(
																(LineCADSegment)camdata.CADSegmentList[ startPointIndex.Item1 ], startPointIndex.Item2, camdata.IsToolVecReverse );
				// down cast
				List<ICAMSegmentElement> startSegmentSeparatedListTransformed = startSegmentSeparatedList.Cast<ICAMSegmentElement>().ToList();

				// add start segment
				camSegmentList.Add( startSegmentSeparatedList[ 0 ] );

				// add the rest segments
				camSegmentList.AddRange( BuildOrderedCAMListAfterStartSegment( camdata, startSegmentSeparatedListTransformed ) );
			}

			// segment - arc
			else {
				List<ArcCAMSegment> startSegmentSeparatedList = SeparateCADArcSegmentToCAMSegment(
																(ArcCADSegment)camdata.CADSegmentList[ startPointIndex.Item1 ], startPointIndex.Item2, camdata.IsToolVecReverse );
				List<ICAMSegmentElement> startSegmentSeparatedListTransformed = startSegmentSeparatedList.Cast<ICAMSegmentElement>().ToList();
				camSegmentList.Add( startSegmentSeparatedList[ 0 ] );
				camSegmentList.AddRange( BuildOrderedCAMListAfterStartSegment( camdata, startSegmentSeparatedListTransformed ) );
			}
			return camSegmentList;
		}

		static void ReverseCAMSegmentList( ref List<ICAMSegmentElement> camSegmentList )
		{
			if( camSegmentList == null || camSegmentList.Count == 0 ) {
				return;
			}

			// change segment order
			camSegmentList.Reverse();

			// reverse start / end point
			for( int i = 0; i < camSegmentList.Count; i++ ) {
				if( camSegmentList[ i ].ContourType == EContourType.Line ) {
					LineCAMSegment reversedLineCAMSegment = new LineCAMSegment( camSegmentList[ i ].EndPoint, camSegmentList[ i ].StartPoint );
					camSegmentList[ i ] = reversedLineCAMSegment;
					continue;
				}
				if( camSegmentList[ i ].ContourType == EContourType.Arc ) {
					ArcCAMSegment oldSegment = (ArcCAMSegment)camSegmentList[ i ];
					ArcCAMSegment reversedArcCAMSegment = new ArcCAMSegment( oldSegment.EndPoint, oldSegment.StartPoint, oldSegment.MidPoint );
					camSegmentList[ i ] = reversedArcCAMSegment;
				}
			}
		}

		static List<LineCAMSegment> SeparateCADLineSegmentToCAMSegment( LineCADSegment lineSegment, int nStartPointIndex, bool isToolVecReverse )
		{
			if( nStartPointIndex < 0 || nStartPointIndex >= lineSegment.PointList.Count ) {
				return null;
			}
			if( nStartPointIndex == 0 ) {
				List<LineCAMSegment> singleLineSegment = new List<LineCAMSegment> {
					BuildCAMLineSegment(lineSegment.PointList,  isToolVecReverse)};
				return singleLineSegment;
			}
			LineCAMSegment endPartLineSegment = BuildCAMLineSegment( lineSegment.PointList.GetRange( 0, nStartPointIndex + 1 ), isToolVecReverse );
			LineCAMSegment startPartLineSegment = BuildCAMLineSegment( lineSegment.PointList.GetRange( nStartPointIndex, lineSegment.PointList.Count - nStartPointIndex ), isToolVecReverse );
			List<LineCAMSegment> separatedLineSegmentList = new List<LineCAMSegment>{
															startPartLineSegment,
															endPartLineSegment,};
			return separatedLineSegmentList;
		}

		static List<ArcCAMSegment> SeparateCADArcSegmentToCAMSegment( ArcCADSegment arcSegment, int startPointIndex, bool isToolVecReverse )
		{
			if( startPointIndex < 0 || startPointIndex >= arcSegment.PointList.Count ) {
				return null;
			}
			if( startPointIndex == 0 ) {
				List<ArcCAMSegment> singleArcSegmentList = new List<ArcCAMSegment> {
					BuildCAMArcSegment( arcSegment.PointList, isToolVecReverse )};
				return singleArcSegmentList;
			}

			ArcCAMSegment endPartArcSegment = BuildCAMArcSegment( arcSegment.PointList.GetRange( 0, startPointIndex + 1 ), isToolVecReverse );
			ArcCAMSegment startPartArcSegment = BuildCAMArcSegment( arcSegment.PointList.GetRange( startPointIndex, arcSegment.PointList.Count - startPointIndex ), isToolVecReverse );
			List<ArcCAMSegment> separatedArcSegmentList = new List<ArcCAMSegment>{
														endPartArcSegment,
														startPartArcSegment};
			return separatedArcSegmentList;
		}

		static List<ICAMSegmentElement> BuildOrderedCAMListAfterStartSegment( CAMData camdata, List<ICAMSegmentElement> startSegmentSeparatedList )
		{
			List<ICAMSegmentElement> cAMSegmentElements = new List<ICAMSegmentElement>();
			(int, int) startPointIndex = camdata.NewStartPoint;

			// 起點下一個到最後
			for( int i = startPointIndex.Item1 + 1; i < camdata.CADSegmentList.Count; i++ ) {
				cAMSegmentElements.Add( BuildCAMSegment( camdata.CADSegmentList[ i ], camdata.IsToolVecReverse ) );
			}

			// 第一個到起點前
			for( int i = 0; i < startPointIndex.Item1; i++ ) {
				cAMSegmentElements.Add( BuildCAMSegment( camdata.CADSegmentList[ i ], camdata.IsToolVecReverse ) );
			}

			// 起點這個segment
			if( startSegmentSeparatedList.Count > 1 ) {
				for( int i = 1; i < startSegmentSeparatedList.Count; i++ ) {
					cAMSegmentElements.Add( startSegmentSeparatedList[ i ] );
				}
			}
			return cAMSegmentElements;
		}

		static ICAMSegmentElement BuildCAMSegment( ICADSegmentElement cadSegment, bool isToolVecReverse )
		{
			if( cadSegment.ContourType == EContourType.Line ) {
				return BuildCAMLineSegment( cadSegment.PointList, isToolVecReverse );
			}
			else {
				return BuildCAMArcSegment( cadSegment.PointList, isToolVecReverse );
			}
		}

		static LineCAMSegment BuildCAMLineSegment( List<CADPoint> linePointList, bool isToolVecReverse )
		{
			LineCAMSegment camLineSegment = null;
			if( linePointList == null || linePointList.Count < 2 ) {
				return camLineSegment;
			}
			CAMPoint startCAMPoint = GetCAMPoint( linePointList[ 0 ], isToolVecReverse );
			CAMPoint endCAMPoint = GetCAMPoint( linePointList[ linePointList.Count - 1 ], isToolVecReverse );
			camLineSegment = new LineCAMSegment( startCAMPoint, endCAMPoint );
			return camLineSegment;
		}

		static ArcCAMSegment BuildCAMArcSegment( List<CADPoint> arcPointList, bool isToolVecReverse )
		{
			ArcCAMSegment camArcSegment = null;
			if( arcPointList == null || arcPointList.Count < 2 ) {
				return camArcSegment;
			}
			CAMPoint startCAMPoint = GetCAMPoint( arcPointList[ 0 ], isToolVecReverse );
			CAMPoint endCAMPoint = GetCAMPoint( arcPointList[ arcPointList.Count - 1 ], isToolVecReverse );

			if( arcPointList.Count % 2 != 0 ) {
				CAMPoint realMidCAMPoint = GetCAMPoint( arcPointList[ arcPointList.Count / 2 ], isToolVecReverse );
				camArcSegment = new ArcCAMSegment( startCAMPoint, endCAMPoint, realMidCAMPoint );
				return camArcSegment;
			}

			CADPoint EstimatedMidCADPoint = GetArcMidPoint( arcPointList );
			CAMPoint EstimatedMidCAMPoint = GetCAMPoint( EstimatedMidCADPoint, isToolVecReverse );
			camArcSegment = new ArcCAMSegment( startCAMPoint, endCAMPoint, EstimatedMidCAMPoint );
			return camArcSegment;
		}

		static CADPoint GetArcMidPoint( List<CADPoint> arcPointList )
		{
			CADPoint midCadPoint = null;
			if( arcPointList == null || arcPointList.Count < 2 ) {
				return midCadPoint;
			}

			// two point is really close 
			if( arcPointList.Count == 2 ) {
				gp_Pnt midPnt = new gp_Pnt(
						 ( arcPointList[ 0 ].Point.X() + arcPointList[ 1 ].Point.X() ) / 2,
						 ( arcPointList[ 0 ].Point.Y() + arcPointList[ 1 ].Point.Y() ) / 2,
						 ( arcPointList[ 0 ].Point.Z() + arcPointList[ 1 ].Point.Z() ) / 2
					);
				midCadPoint = new CADPoint( midPnt,
						AverageDir( arcPointList[ 0 ].NormalVec_1st, arcPointList[ 1 ].NormalVec_1st ),
						AverageDir( arcPointList[ 0 ].NormalVec_1st, arcPointList[ 1 ].NormalVec_1st ),
						AverageDir( arcPointList[ 0 ].TangentVec, arcPointList[ 1 ].TangentVec ) );
				return midCadPoint;
			}
			return arcPointList[ arcPointList.Count / 2 ];
		}

		static gp_Dir AverageDir( gp_Dir d1, gp_Dir d2 )
		{
			gp_Vec vector = new gp_Vec( d1 );
			vector.Add( new gp_Vec( d2 ) );

			// two directions opposite
			if( vector.Magnitude() < 1e-3 ) {
				return new gp_Dir( d1.XYZ() );
			}

			vector.Normalize();
			return new gp_Dir( vector );
		}
	}
}
