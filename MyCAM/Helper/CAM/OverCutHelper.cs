using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
	public static class OverCutHelper
	{
		public static void SetOverCut( IReadOnlyList<IOrientationPoint> mainPointList, out List<IOrientationPoint> overCutPointList,
			double overCutLength, bool isClosed, double overCutTolerance = OVERCUT_MATH_TOLERANCE )
		{
			overCutPointList = new List<IOrientationPoint>();
			if( mainPointList.Count == 0 || overCutLength == 0 || !isClosed ) {
				return;
			}
			double dTotalOverCutLength = 0;

			// end point is the start of over cut
			overCutPointList.Add( mainPointList.Last().Clone() );
			for( int i = 0; i < mainPointList.Count - 1; i++ ) {

				// get this edge distance
				double dDistance = mainPointList[ i ].Point.Distance( mainPointList[ i + 1 ].Point );
				if( dTotalOverCutLength + dDistance < overCutLength ) {

					// still within overcut length → take next point directly
					overCutPointList.Add( mainPointList[ i + 1 ].Clone() );
					dTotalOverCutLength += dDistance;
				}
				else {

					// need to stop inside this segment
					double dRemain = overCutLength - dTotalOverCutLength;
					if( dRemain <= overCutTolerance ) {
						return;
					}

					// compute new point along segment
					gp_Pnt overCutEndPoint = GetExactOverCutEndPoint( mainPointList[ i ].Point, mainPointList[ i + 1 ].Point, dRemain );

					// interpolate tool vector
					DiscreteUtility.InterpolateVecBetween2Point( mainPointList[ i ], mainPointList[ i + 1 ], overCutEndPoint, out gp_Dir endPointToolVec, out gp_Dir endPointTangentVec );

					// create new cam point
					IOrientationPoint camPoint = BuildOverCutPoint( overCutEndPoint, endPointToolVec, endPointTangentVec );
					overCutPointList.Add( camPoint );
					return;
				}
			}
		}

		public static void SetStdPatternOverCut( gp_Ax3 refCoord, IStdPatternGeomData geomData, List<IOrientationPoint> mainPointList, double overCutLength,
			out List<IOrientationPoint> overCutPointList, double overCutTolerance = OVERCUT_MATH_TOLERANCE )
		{
			overCutPointList = new List<IOrientationPoint>();

			if( geomData == null || mainPointList == null || mainPointList.Count == 0 || overCutLength <= 0 ) {
				return;
			}

			// step 1: Discretize the geometry into orientation points
			if( !StdPatternDiscreteFactory.GetDiscreteOriPointList( refCoord, geomData, out List<IOrientationPoint> discretizedPointList ) || discretizedPointList == null || discretizedPointList.Count == 0 ) {
				return;
			}

			// step 2: Find the end point index in the discretized list
			int endPointIndex = FindClosestPointIndex( discretizedPointList, mainPointList.Last().Clone() );
			if( endPointIndex < 0 ) {
				return;
			}

			// step 3: Calculate overcut starting from end point
			// the path continues from endPointIndex onwards (wrapping around if needed)
			double dTotalOverCutLength = 0;

			// first point of overcut is the end point
			overCutPointList.Add( discretizedPointList[ endPointIndex ].Clone() );

			// traverse the path from end point onwards
			for( int i = 0; i < discretizedPointList.Count; i++ ) {
				int currentIndex = ( endPointIndex + i ) % discretizedPointList.Count;
				int nextIndex = ( endPointIndex + i + 1 ) % discretizedPointList.Count;

				// get this edge distance
				double dDistance = discretizedPointList[ currentIndex ].Point.Distance( discretizedPointList[ nextIndex ].Point );
				if( dTotalOverCutLength + dDistance < overCutLength ) {

					// still within overcut length → take next point directly
					overCutPointList.Add( discretizedPointList[ nextIndex ].Clone() );
					dTotalOverCutLength += dDistance;
				}
				else {

					// need to stop inside this segment
					double dRemain = overCutLength - dTotalOverCutLength;
					if( dRemain <= overCutTolerance ) {
						return;
					}

					// compute new point along segment
					gp_Pnt overCutEndPoint = GetExactOverCutEndPoint(
						discretizedPointList[ currentIndex ].Point,
						discretizedPointList[ nextIndex ].Point,
						dRemain
					);

					// interpolate tool vector
					DiscreteUtility.InterpolateVecBetween2Point(
						discretizedPointList[ currentIndex ],
						discretizedPointList[ nextIndex ],
						overCutEndPoint,
						out gp_Dir endPointToolVec,
						out gp_Dir endPointTangentVec
					);

					// create new cam point
					IOrientationPoint camPoint = BuildOverCutPoint( overCutEndPoint, endPointToolVec, endPointTangentVec );
					overCutPointList.Add( camPoint );
					return;
				}
			}
		}

		// TODO: I think there should be some other better place for it
		public static double GetMaxOverCutLength( IStdPatternGeomData stdPatternGeomData, int StartPointIndex )
		{
			switch( stdPatternGeomData.PathType ) {
				case PathType.Circle:
					CircleGeomData circleGeomData = stdPatternGeomData as CircleGeomData;
					if( circleGeomData != null ) {
						return GetCircleMaxOverCutLength( circleGeomData );
					}
					break;
				case PathType.Rectangle:
					RectangleGeomData rectangleGeomData = stdPatternGeomData as RectangleGeomData;
					if( rectangleGeomData != null ) {
						return GetRectangleMaxOverCutLength( rectangleGeomData, StartPointIndex );
					}
					break;
				case PathType.Runway:
					RunwayGeomData runwayGeomData = stdPatternGeomData as RunwayGeomData;
					if( runwayGeomData != null ) {
						return GetRunwayMaxOverCutLength( runwayGeomData, StartPointIndex );
					}
					break;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					PolygonGeomData polygonGeomData = stdPatternGeomData as PolygonGeomData;
					if( polygonGeomData != null ) {
						return GetPolygonMaxOverCutLength( polygonGeomData );
					}
					break;
				default:
					break;
			}
			return 0;
		}

		static int FindClosestPointIndex( List<IOrientationPoint> pointList, IOrientationPoint targetPoint )
		{
			if( pointList == null || pointList.Count == 0 || targetPoint == null ) {
				return -1;
			}

			int closestIndex = 0;
			double minDistance = pointList[ 0 ].Point.Distance( targetPoint.Point );

			for( int i = 1; i < pointList.Count; i++ ) {
				double distance = pointList[ i ].Point.Distance( targetPoint.Point );
				if( distance < minDistance ) {
					minDistance = distance;
					closestIndex = i;
				}
			}

			return closestIndex;
		}

		static IOrientationPoint BuildOverCutPoint( gp_Pnt overCutEndPoint, gp_Dir endPointToolVec, gp_Dir endPointTangentVec )
		{
			CADPoint cadPoint = new CADPoint( overCutEndPoint, endPointToolVec, endPointToolVec, endPointTangentVec );
			CAMPoint camPoint = new CAMPoint( cadPoint, endPointToolVec );
			return camPoint;
		}

		static gp_Pnt GetExactOverCutEndPoint( gp_Pnt currentPoint, gp_Pnt nextPoint, double dDistanceMoveFromOverPoint )
		{
			// from currentPoint → nextOverLengthPoint
			gp_Vec movingVec = new gp_Vec( currentPoint, nextPoint );

			// normalize to unit vector
			movingVec.Normalize();
			gp_Vec moveVec = movingVec.Multiplied( dDistanceMoveFromOverPoint );

			// shifted along the vector
			return new gp_Pnt( currentPoint.XYZ() + moveVec.XYZ() );
		}

		static double GetCircleMaxOverCutLength( CircleGeomData circleGeomData )
		{
			// Unconditionally rounded to three decimal places.
			return Math.Floor( Math.PI * circleGeomData.Diameter / 4 * 1000 ) / 1000;
		}

		static double GetRectangleMaxOverCutLength( RectangleGeomData rectangleGeomData, int startPointIndex )
		{
			if( startPointIndex != 0 ) {

				// Unconditionally rounded to three decimal places.
				return Math.Floor( ( rectangleGeomData.Length - 2 * rectangleGeomData.CornerRadius ) / 2 * 1000 ) / 1000;
			}

			// Unconditionally rounded to three decimal places.
			return Math.Floor( ( rectangleGeomData.Width - 2 * rectangleGeomData.CornerRadius ) / 2 * 1000 ) / 1000;
		}

		static double GetRunwayMaxOverCutLength( RunwayGeomData runwayGeomData, int startPointIndex )
		{
			if( startPointIndex != 0 ) {

				// Unconditionally rounded to three decimal places.
				return Math.Floor( ( runwayGeomData.Length - runwayGeomData.Width ) / 2.0 * 1000 ) / 1000;
			}

			// Unconditionally rounded to three decimal places.
			return Math.Floor( ( runwayGeomData.Width * Math.PI / 4.0 * 1000 ) ) / 1000;
		}

		static double GetPolygonMaxOverCutLength( PolygonGeomData polygonGeomData )
		{
			double sideLength = polygonGeomData.SideLength;
			double interiorAngle = ( polygonGeomData.Sides - 2 ) * Math.PI / polygonGeomData.Sides;
			double halfAngle = interiorAngle / 2.0;
			double tangentDistance = polygonGeomData.CornerRadius / Math.Tan( halfAngle );

			// Unconditionally rounded to three decimal places.
			return Math.Floor( ( sideLength - 2 * tangentDistance ) / 2 * 1000 ) / 1000;
		}

		const double OVERCUT_MATH_TOLERANCE = 0.001;
	}
}
