using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.Data;
using OCC.gp;

namespace MyCAM.Helper
{
	public enum StartPointSelectStrategy
	{
		LongestSegmentMidpoint,
		XNegative,
		XPositive,
		YNegative,
		YPositive,
		ZNegative,
		ZPositive
	}

	internal class StartPointHelper
	{
		public static List<CADPoint> EnsureStartPointsIncluded( gp_Ax3 coordinate, List<CADPoint> discretizedPoints, List<CADPoint> startPoints, IStdPatternGeomData geomData )
		{
			if( discretizedPoints == null || discretizedPoints.Count == 0 || startPoints == null || startPoints.Count == 0 || geomData == null ) {
				return discretizedPoints;
			}

			const double POINT_MATCH_TOLERANCE = 0.001;

			foreach( CADPoint startPoint in startPoints ) {
				// find the insertion index for the start point
				int nInsertIndex = FindInsertIndex( coordinate, discretizedPoints, startPoint, geomData, POINT_MATCH_TOLERANCE, out bool isMatch );

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

		static int FindInsertIndex( gp_Ax3 coordinate, List<CADPoint> discretizedPoints, CADPoint startPoint, IStdPatternGeomData geomData, double tolerance, out bool isMatch )
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
				return GetIndexByAngle( coordinate, discretizedPoints, startPoint, circleData );
			}
			else {
				return GetIndexBySegmentProj( discretizedPoints, startPoint );
			}
		}

		static int GetIndexByAngle( gp_Ax3 coordinate, List<CADPoint> discretizedPoints, CADPoint startPoint, CircleGeomData circleData )
		{
			if( circleData == null || discretizedPoints.Count == 0 ) {
				return 0;
			}

			gp_Pnt center = coordinate.Location();
			gp_Dir xDir = coordinate.XDirection();
			gp_Dir yDir = coordinate.YDirection();

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

		#region Auto Select

		public static bool TryChooseByDirection( List<CAMPoint> camPointList, gp_Dir searchDir, double tolerance, out int camIndex )
		{
			camIndex = -1;
			if( camPointList == null || camPointList.Count == 0
				|| searchDir == null || double.IsNaN( tolerance )
				|| double.IsInfinity( tolerance ) || tolerance < 0 ) {
				return false;
			}

			List<int> candidates = new List<int>();
			for( int index = 0; index < camPointList.Count; index++ ) {
				CAMPoint point = camPointList[ index ];
				if( point == null || point.Point == null ) {
					return false;
				}
				if( SpecialPointIndex.IsClosedPointIndex( point.InitPathIndex ) || SpecialPointIndex.IsGeneratedPointIndex( point.InitPathIndex ) ) {
					continue;
				}
				candidates.Add( index );
			}
			if( candidates.Count == 0 ) {
				return false;
			}

			candidates = FilterAxisExtreme(
				camPointList, candidates, searchDir, tolerance );
			if( candidates.Count == 0 ) {
				return false;
			}

			camIndex = candidates.Count == 1
				? candidates[ 0 ]
				: FilterCandidateGroupCenter( camPointList, candidates );
			return true;
		}

		public static bool TryChooseByPriority( List<CAMPoint> camPointList, IReadOnlyList<StartPointSelectStrategy> strategies, double tolerance, out int camIndex )
		{
			camIndex = -1;
			if( camPointList == null || strategies == null || strategies.Count() == 0 ) {
				return false;
			}

			List<int> candidateCAMIndices = new List<int>();
			for( int i = 0; i < camPointList.Count; ++i ) {
				int cadIndex = camPointList[ i ].InitPathIndex;
				if( SpecialPointIndex.IsClosedPointIndex( cadIndex ) || SpecialPointIndex.IsGeneratedPointIndex( cadIndex ) ) {
					continue;
				}
				candidateCAMIndices.Add( i );
			}
			if( candidateCAMIndices.Count == 0 ) {
				return false;
			}

			List<int> longestMidpointCAMIndices = strategies.Contains( StartPointSelectStrategy.LongestSegmentMidpoint )
				? GetLongestSegmentMidpointCAMIndices( camPointList )
				: null;

			foreach( StartPointSelectStrategy strategy in strategies ) {

				switch( strategy ) {
					case StartPointSelectStrategy.LongestSegmentMidpoint:
						candidateCAMIndices = FilterLongestSegmentMidpoints( camPointList, candidateCAMIndices, longestMidpointCAMIndices, tolerance );
						break;

					case StartPointSelectStrategy.XNegative:
						candidateCAMIndices = FilterAxisExtreme( camPointList, candidateCAMIndices, new gp_Dir( -1, 0, 0 ), tolerance );
						break;

					case StartPointSelectStrategy.XPositive:
						candidateCAMIndices = FilterAxisExtreme( camPointList, candidateCAMIndices, new gp_Dir( 1, 0, 0 ), tolerance );
						break;

					case StartPointSelectStrategy.YNegative:
						candidateCAMIndices = FilterAxisExtreme( camPointList, candidateCAMIndices, new gp_Dir( 0, -1, 0 ), tolerance );
						break;

					case StartPointSelectStrategy.YPositive:
						candidateCAMIndices = FilterAxisExtreme( camPointList, candidateCAMIndices, new gp_Dir( 0, 1, 0 ), tolerance );
						break;

					case StartPointSelectStrategy.ZNegative:
						candidateCAMIndices = FilterAxisExtreme( camPointList, candidateCAMIndices, new gp_Dir( 0, 0, -1 ), tolerance );
						break;

					case StartPointSelectStrategy.ZPositive:
						candidateCAMIndices = FilterAxisExtreme( camPointList, candidateCAMIndices, new gp_Dir( 0, 0, 1 ), tolerance );
						break;

					default:
						return false;
				}

				if( candidateCAMIndices.Count == 0 ) {
					return false;
				}
				if( candidateCAMIndices.Count == 1 ) {
					break;
				}
			}

			if( candidateCAMIndices.Count > 1 ) {
				bool hasCalculateAxisExtreme = strategies.Any( strategy =>
					strategy == StartPointSelectStrategy.XNegative ||
					strategy == StartPointSelectStrategy.XPositive ||
					strategy == StartPointSelectStrategy.YNegative ||
					strategy == StartPointSelectStrategy.YPositive ||
					strategy == StartPointSelectStrategy.ZNegative ||
					strategy == StartPointSelectStrategy.ZPositive );

				camIndex = hasCalculateAxisExtreme
					? FilterCandidateGroupCenter( camPointList, candidateCAMIndices )
					: FilterOriginalStartPoint( camPointList, candidateCAMIndices );
			}
			else {
				camIndex = candidateCAMIndices.First();
			}

			return true;
		}

		static List<int> GetLongestSegmentMidpointCAMIndices( List<CAMPoint> camPointList )
		{
			List<GeometrySegment> segments = GeometrySegmentHelper.BuildGeometrySegments( camPointList );
			int bestGroupCount = SortGeometrySegments( segments );
			return segments
				.Take( bestGroupCount )
				.Select( segment => segment.MiddleCAMIndex )
				.ToList();
		}

		static List<int> FilterLongestSegmentMidpoints( List<CAMPoint> camPointList, IReadOnlyList<int> candidateCAMIndices,
			IReadOnlyList<int> longestMidpointCAMIndices, double tolerance )
		{
			if( longestMidpointCAMIndices == null || longestMidpointCAMIndices.Count == 0 ) {
				return new List<int>();
			}

			double effectiveTolerance = Math.Max( tolerance, 0 );
			double toleranceSquared = effectiveTolerance * effectiveTolerance;

			return candidateCAMIndices.Where( candidateIndex =>
			{
				gp_Pnt candidatePoint = camPointList[ candidateIndex ].Point;

				return longestMidpointCAMIndices.Any( midpointIndex =>
				{
					gp_Pnt midpoint = camPointList[ midpointIndex ].Point;
					return candidatePoint.SquareDistance( midpoint ) <= toleranceSquared;
				} );
			} )
				.ToList();
		}

		static List<int> FilterAxisExtreme( List<CAMPoint> camPointList, IReadOnlyList<int> candidateCAMIndices, gp_Dir searchDir, double tolerance )
		{
			gp_Pnt fp = camPointList[ candidateCAMIndices[ 0 ] ].Point;
			double maximumProjection = fp.X() * searchDir.X() + fp.Y() * searchDir.Y() + fp.Z() * searchDir.Z();

			for( int i = 1; i < candidateCAMIndices.Count(); ++i ) {
				gp_Pnt p = camPointList[ candidateCAMIndices[ i ] ].Point;
				double projection = p.X() * searchDir.X() + p.Y() * searchDir.Y() + p.Z() * searchDir.Z();
				maximumProjection = Math.Max( maximumProjection, projection );
			}

			List<int> result = new List<int>();

			double effectiveTolerance = Math.Max( tolerance, 0 );

			for( int i = 0; i < candidateCAMIndices.Count(); ++i ) {
				gp_Pnt p = camPointList[ candidateCAMIndices[ i ] ].Point;
				double projection = p.X() * searchDir.X() + p.Y() * searchDir.Y() + p.Z() * searchDir.Z();

				if( ( maximumProjection - projection ) <= effectiveTolerance ) {
					result.Add( candidateCAMIndices[ i ] );
				}
			}

			return result;
		}

		static int FilterOriginalStartPoint( List<CAMPoint> camPointList, IReadOnlyList<int> candidateCAMIndices )
		{
			return candidateCAMIndices
				.OrderBy( candidateIndex => camPointList[ candidateIndex ].InitPathIndex )
				.First();
		}

		static int FilterCandidateGroupCenter( List<CAMPoint> camPointList, IReadOnlyList<int> candidateCAMIndices )
		{
			double centerX = candidateCAMIndices.Average( index => camPointList[ index ].Point.X() );
			double centerY = candidateCAMIndices.Average( index => camPointList[ index ].Point.Y() );
			double centerZ = candidateCAMIndices.Average( index => camPointList[ index ].Point.Z() );
			gp_Pnt center = new gp_Pnt( centerX, centerY, centerZ );

			return candidateCAMIndices
				.OrderBy( index => camPointList[ index ].Point.SquareDistance( center ) )
				.ThenBy( index => camPointList[ index ].InitPathIndex )
				.First();
		}

		// sort the priority
		const double ABSOLUTE_PATH_LENGTH_GROUP_TOLERENCE = 0.01;
		const double RELATIVE_PATH_LENGTH_GROUP_TOLERENCE = 0.2;
		const double ABSOLUTE_AVERAGE_CURVATURE_GROUP_TOLERANCE = 1e-4;
		const double RELATIVE_AVERAGE_CURVATURE_GROUP_TOLERANCE = 0.2;
		const double ABSOLUTE_CURVATURE_VARIATION_GROUP_TOLERANCE = 1e-4;
		const double RELATIVE_CURVATURE_VARIATION_GROUP_TOLERANCE = 0.2;

		// return the top priority group [0, bestGroupCount)
		static int SortGeometrySegments( List<GeometrySegment> segments )
		{
			if( segments == null ) {
				return 0;
			}

			// Segments must have finite, nonnegative geometry metrics
			foreach( GeometrySegment segment in segments ) {
				if( segment == null ||
					!( ( segment.PathLength >= 0.0 ) && ( segment.PathLength <= double.MaxValue ) ) ||
					!( ( segment.AverageCurvature >= 0.0 ) && ( segment.AverageCurvature <= double.MaxValue ) ) ||
					!( ( segment.CurvatureVariation >= 0.0 ) && ( segment.CurvatureVariation <= double.MaxValue ) ) ) {
					return 0;
				}
			}

			GeometrySegment[] ordered = segments.OrderBy( segment => GetTypePriority( segment.Type ) ).ToArray();
			int bestGroupCount = 0;
			int groupStart = 0;
			while( groupStart < ordered.Length ) {
				int typePriority = GetTypePriority( ordered[ groupStart ].Type );
				int groupEnd = groupStart + 1;
				while( ( groupEnd < ordered.Length ) && ( GetTypePriority( ordered[ groupEnd ].Type ) == typePriority ) ) {
					groupEnd++;
				}

				int firstGroupCount = SortSegmentMetricGroups( ordered, groupStart, groupEnd - groupStart, 0 );
				if( groupStart == 0 ) {
					bestGroupCount = firstGroupCount;
				}
				groupStart = groupEnd;
			}

			for( int i = 0; i < ordered.Length; ++i ) {
				segments[ i ] = ordered[ i ];
			}
			return bestGroupCount;
		}

		static int SortSegmentMetricGroups( GeometrySegment[] segments, int startIndex, int count, int metricIndex )
		{
			if( count <= 1 ) {
				return count;
			}

			Func<GeometrySegment, double> metric;
			double absoluteTolerance;
			double relativeTolerance;
			switch( metricIndex ) {
				case 0:
					metric = segment => segment.PathLength;
					absoluteTolerance = ABSOLUTE_PATH_LENGTH_GROUP_TOLERENCE;
					relativeTolerance = RELATIVE_PATH_LENGTH_GROUP_TOLERENCE;
					break;
				case 1:
					metric = segment => segment.AverageCurvature;
					absoluteTolerance = ABSOLUTE_AVERAGE_CURVATURE_GROUP_TOLERANCE;
					relativeTolerance = RELATIVE_AVERAGE_CURVATURE_GROUP_TOLERANCE;
					break;
				default:
					metric = segment => segment.CurvatureVariation;
					absoluteTolerance = ABSOLUTE_CURVATURE_VARIATION_GROUP_TOLERANCE;
					relativeTolerance = RELATIVE_CURVATURE_VARIATION_GROUP_TOLERANCE;
					break;
			}

			IEnumerable<GeometrySegment> range = Enumerable.Range( startIndex, count ).Select( index => segments[ index ] );
			GeometrySegment[] ordered = ( metricIndex == 0 ? range.OrderByDescending( metric ) : range.OrderBy( metric ) ).ToArray();
			Array.Copy( ordered, 0, segments, startIndex, count );

			int firstGroupCount = 0;
			int groupStart = startIndex;
			int endIndex = startIndex + count;
			while( groupStart < endIndex ) {
				double anchor = metric( segments[ groupStart ] );
				double tolerance = Math.Max( absoluteTolerance, relativeTolerance * anchor );
				int groupEnd = groupStart + 1;
				while( ( groupEnd < endIndex ) && ( Math.Abs( metric( segments[ groupEnd ] ) - anchor ) <= tolerance ) ) {
					groupEnd++;
				}

				int terminalGroupCount = ( metricIndex < 2 )
					? SortSegmentMetricGroups( segments, groupStart, groupEnd - groupStart, metricIndex + 1 )
					: ( groupEnd - groupStart );
				if( groupStart == startIndex ) {
					firstGroupCount = terminalGroupCount;
				}
				groupStart = groupEnd;
			}
			return firstGroupCount;
		}

		static int GetTypePriority( GeometryEdgeType type )
		{
			switch( type ) {
				case GeometryEdgeType.Line:
					return 0;

				case GeometryEdgeType.Arc:
					return 1;

				case GeometryEdgeType.Curve:
					return 2;

				default:
					return 3;
			}
		}

		#endregion
	}
}
