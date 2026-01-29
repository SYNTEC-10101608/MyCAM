using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper.CAM
{
	internal class StartPointHelper
	{
		public static List<CADPoint> EnsureStartPointsIncluded( List<CADPoint> discretizedPoints, List<CADPoint> startPoints, IStdPatternGeomData geomData )
		{
			if( discretizedPoints == null || discretizedPoints.Count == 0 || startPoints == null || startPoints.Count == 0 || geomData == null ) {
				return discretizedPoints;
			}

			const double POINT_MATCH_TOLERANCE = 0.001;

			foreach( CADPoint startPoint in startPoints ) {
				// find the insertion index for the start point
				int nInsertIndex = FindInsertIndex( discretizedPoints, startPoint, geomData, POINT_MATCH_TOLERANCE, out bool isMatch );

				if( isMatch ) {
					// start point already exists, no need to insert
					continue;
				}

				// insert the start point at the correct position
				if( nInsertIndex >= 0 && nInsertIndex < discretizedPoints.Count ) {
					discretizedPoints.Insert( nInsertIndex, startPoint );
				}
				else if( nInsertIndex == discretizedPoints.Count ) {
					discretizedPoints.Add( startPoint );
				}
			}

			return discretizedPoints;
		}

		#region Start Point Insertion Helper Methods

		static int FindInsertIndex( List<CADPoint> discretizedPoints, CADPoint startPoint, IStdPatternGeomData geomData, double tolerance, out bool isMatch )
		{
			isMatch = false;

			if( discretizedPoints == null || discretizedPoints.Count == 0 ) {
				return 0;
			}

			// check for exact match first
			for( int i = 0; i < discretizedPoints.Count; i++ ) {
				double distance = startPoint.Point.SquareDistance( discretizedPoints[ i ].Point );
				if( distance < tolerance ) {
					isMatch = true;
					return i;
				}
			}

			// circle uses angle-based insertion, others use segment projection
			if( geomData.PathType == PathType.Circle && geomData is CircleGeomData circleData ) {
				return GetIndexByAngle( discretizedPoints, startPoint, circleData );
			}
			else {
				return GetIndexBySegmentProj( discretizedPoints, startPoint );
			}
		}

		static int GetIndexByAngle( List<CADPoint> discretizedPoints, CADPoint startPoint, CircleGeomData circleData )
		{
			if( circleData == null || discretizedPoints.Count == 0 ) {
				return 0;
			}

			gp_Pnt center = circleData.RefCoord.Location();
			gp_Dir xDir = circleData.RefCoord.XDirection();
			gp_Dir yDir = circleData.RefCoord.YDirection();

			double startAngle = CalculateAngle( center, startPoint.Point, xDir, yDir );

			// find the insertion position based on angle
			for( int i = 0; i < discretizedPoints.Count; i++ ) {
				int nextIndex = ( i + 1 ) % discretizedPoints.Count;
				double angle1 = CalculateAngle( center, discretizedPoints[ i ].Point, xDir, yDir );
				double angle2 = CalculateAngle( center, discretizedPoints[ nextIndex ].Point, xDir, yDir );

				// handle angle wrapping (clockwise direction, angles decrease)
				if( angle1 < angle2 ) {
					// wrapping case
					if( startAngle <= angle1 || startAngle >= angle2 ) {
						return nextIndex;
					}
				}
				else {
					// normal case
					if( startAngle <= angle1 && startAngle >= angle2 ) {
						return nextIndex;
					}
				}
			}

			return GetClosestIndex( discretizedPoints, startPoint ) + 1;
		}

		static int GetIndexBySegmentProj( List<CADPoint> discretizedPoints, CADPoint startPoint )
		{
			if( discretizedPoints == null || discretizedPoints.Count == 0 ) {
				return 0;
			}

			int bestSegmentIndex = 0;
			double bestDistance = double.MaxValue;
			double bestProjectionParam = 0.0;

			// iterate through all segments
			for( int i = 0; i < discretizedPoints.Count; i++ ) {
				int nextIndex = ( i + 1 ) % discretizedPoints.Count;

				gp_Pnt p1 = discretizedPoints[ i ].Point;
				gp_Pnt p2 = discretizedPoints[ nextIndex ].Point;
				gp_Pnt target = startPoint.Point;

				// project target onto segment [p1, p2]
				double t = ProjectPointOnSegment( p1, p2, target );

				// calculate projection point
				gp_Pnt projection = InterpolatePoint( p1, p2, t );

				// calculate distance from target to projection
				double distance = target.Distance( projection );

				if( distance < bestDistance ) {
					bestDistance = distance;
					bestSegmentIndex = i;
					bestProjectionParam = t;
				}
			}

			// determine insertion index based on projection parameter
			// if t is close to 0, insert before segment start (at current index)
			// if t is close to 1, insert after segment end (at next index)
			// if t is in the middle, insert at next index (after segment start)
			const double ENDPOINT_TOLERANCE = 0.1;

			if( bestProjectionParam < ENDPOINT_TOLERANCE ) {
				// closer to segment start point
				return bestSegmentIndex;
			}
			else {
				// closer to or within segment, insert after segment start
				return ( bestSegmentIndex + 1 ) % discretizedPoints.Count;
			}
		}

		static double ProjectPointOnSegment( gp_Pnt segmentStart, gp_Pnt segmentEnd, gp_Pnt point )
		{
			gp_Vec segmentVec = new gp_Vec( segmentStart, segmentEnd );
			double segmentLength = segmentVec.SquareMagnitude();

			// degenerate segment (start == end)
			const double MIN_SEGMENT_LENGTH = 0.001;
			if( segmentLength < MIN_SEGMENT_LENGTH ) {
				return 0.0;
			}

			gp_Vec toPoint = new gp_Vec( segmentStart, point );
			double dotProduct = segmentVec.Dot( toPoint );

			// calculate unclamped parameter
			double t = dotProduct / segmentLength;

			// clamp to [0, 1] to keep projection on segment
			if( t < 0.0 ) {
				return 0.0;
			}
			if( t > 1.0 ) {
				return 1.0;
			}

			return t;
		}

		static gp_Pnt InterpolatePoint( gp_Pnt p1, gp_Pnt p2, double t )
		{
			return new gp_Pnt(
				p1.X() + t * ( p2.X() - p1.X() ),
				p1.Y() + t * ( p2.Y() - p1.Y() ),
				p1.Z() + t * ( p2.Z() - p1.Z() )
			);
		}

		static int GetClosestIndex( List<CADPoint> discretizedPoints, CADPoint startPoint )
		{
			double minDistance = double.MaxValue;
			int closestIndex = 0;

			for( int i = 0; i < discretizedPoints.Count; i++ ) {
				double dist = startPoint.Point.Distance( discretizedPoints[ i ].Point );
				if( dist < minDistance ) {
					minDistance = dist;
					closestIndex = i;
				}
			}

			return closestIndex;
		}

		static double CalculateAngle( gp_Pnt center, gp_Pnt point, gp_Dir xDir, gp_Dir yDir )
		{
			gp_Vec toPoint = new gp_Vec( center, point );
			double x = toPoint.Dot( new gp_Vec( xDir ) );
			double y = toPoint.Dot( new gp_Vec( yDir ) );
			double angle = Math.Atan2( y, x );

			// normalize to [0, 2π]
			if( angle < 0 ) {
				angle += 2.0 * Math.PI;
			}

			return angle;
		}

		#endregion
	}
}
