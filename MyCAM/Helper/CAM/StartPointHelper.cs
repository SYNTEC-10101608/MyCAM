using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper.CAM
{
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

		#region Gemotry Edge Classification

		enum GeometryEdgeType
		{
			Line,
			Arc,
			Curve,
			Unknown
		}

		class GeometrySegment
		{
			public int StartCAMIndex { get; }

			public int EndCAMIndex { get; }

			public int MiddleCAMIndex
			{
				get
				{
					if( StartCAMIndex <= EndCAMIndex ) {
						return ( StartCAMIndex + EndCAMIndex ) / 2;
					}
					else {
						return ( ( StartCAMIndex + OrignPathCount + EndCAMIndex ) / 2 ) % OrignPathCount;
					}
				}
			}

			public int OrignPathCount { get; }

			public GeometryEdgeType Type { get; }

			public double PathLength { get; }

			public double AverageCurvature { get; }

			public double CurvatureVariation { get; }

			public GeometrySegment( int startCAMIndex, int endCAMIndex, int orignPathCount, GeometryEdgeType type,
				double pathLength, double averageCurvature, double curvatureVariation )
			{
				StartCAMIndex = startCAMIndex;
				EndCAMIndex = endCAMIndex;
				OrignPathCount = orignPathCount;
				Type = type;
				PathLength = pathLength;
				AverageCurvature = averageCurvature;
				CurvatureVariation = curvatureVariation;
			}
		}

		public static int ChooseLongestSegmentMiddlePoint( List<CAMPoint> camPointList )
		{
			List<GeometrySegment> segments = BuildGeometrySegments( camPointList );
			int bestGroupCount = SortGeometrySegments( segments );

			// return 0 if no valid segments
			if( bestGroupCount == 0 ) {
				return 0;
			}

			// select by InitPathIndex in the best group
			int selectedIndex = segments.First().MiddleCAMIndex;
			for( int i = 1; i < bestGroupCount; ++i ) {
				int candidateIndex = segments[ i ].MiddleCAMIndex;
				if( camPointList[ candidateIndex ].InitPathIndex < camPointList[ selectedIndex ].InitPathIndex ) {
					selectedIndex = candidateIndex;
				}
			}
			return selectedIndex;
		}

		static List<GeometrySegment> BuildGeometrySegments( List<CAMPoint> camPointList )
		{
			List<GeometrySegment> segments = new List<GeometrySegment>();
			if( camPointList == null || camPointList.Count == 0 ) {
				return segments;
			}

			bool isClosed = SpecialPointIndex.IsClosedPointIndex( camPointList[ camPointList.Count - 1 ].InitPathIndex );

			// closed contour: the end CAMPoint is same as the start CAMPoint
			int pointCount = isClosed ? ( camPointList.Count - 1 ) : camPointList.Count;
			if( pointCount == 0 ) {
				return segments;
			}

			List<gp_Pnt> points = new List<gp_Pnt>( pointCount );
			for( int i = 0; i < pointCount; ++i ) {
				points.Add( camPointList[ i ].Point );
			}

			// closed contour: one edge per point
			int edgeCount = isClosed ? pointCount : ( pointCount - 1 );
			double[] edgeLengths = new double[ edgeCount ];
			for( int i = 0; i < edgeLengths.Length; ++i ) {
				edgeLengths[ i ] = points[ i ].Distance( points[ ( i + 1 ) % pointCount ] );
			}

			List<int> breakIndices = FindBreakPoints( points, isClosed );
			if( breakIndices.Count == 0 ) {

				// closed segement: the end point is same as the start point
				int segmentEndIndex = isClosed ? 0 : ( pointCount - 1 );
				segments.Add( AnalyzeSegment( points, 0, segmentEndIndex, edgeLengths ) );
				return segments;
			}

			if( isClosed ) {
				for( int i = 0; i < breakIndices.Count; ++i ) {
					int nextIndex = ( i + 1 ) % breakIndices.Count;
					segments.Add( AnalyzeSegment( points, breakIndices[ i ], breakIndices[ nextIndex ], edgeLengths ) );
				}
			}
			else {
				int startIndex = 0;
				foreach( int endIndex in breakIndices ) {
					if( endIndex > startIndex ) {
						segments.Add( AnalyzeSegment( points, startIndex, endIndex, edgeLengths ) );
					}
					startIndex = endIndex;
				}
				if( startIndex < pointCount - 1 ) {
					segments.Add( AnalyzeSegment( points, startIndex, ( pointCount - 1 ), edgeLengths ) );
				}
			}

			return segments;
		}

		// STEP 1 calculate G1 and G2 continuity

		const double POINT_TOLERENCE = 1e-6;
		const double TANGENT_TOLERENCE_DEGREE = 1.0;
		const double ABSOLUTE_CURVATURE_TOLERENCE = 1e-4;
		const double RELATIVE_CURVATURE_TOLERENCE = 0.10;

		static List<int> FindBreakPoints( List<gp_Pnt> points, bool isClosed )
		{
			// calculate G1 need at laeast 3 points, G2 need at least 5 points
			if( points == null || points.Count < 3 ) {
				return new List<int>();
			}

			List<int> g1BreakIndices = FindG1BreakIndices( points, isClosed );
			List<int> g2BreakIndices = FindG2BreakIndices( points, isClosed, g1BreakIndices );

			return g1BreakIndices
				.Concat( g2BreakIndices )
				.Distinct()
				.OrderBy( index => index )
				.ToList();
		}

		static List<int> FindG1BreakIndices( List<gp_Pnt> points, bool isClosed )
		{
			if( points == null || points.Count < 3 ) {
				return new List<int>();
			}

			List<int> breakIndices = new List<int>();
			int pointCount = points.Count;
			double tangentToleranceAngle = TANGENT_TOLERENCE_DEGREE * Math.PI / 180.0;
			double[] turningAngles = new double[ pointCount ];
			double[] strengths = new double[ pointCount ];

			// closed: iterate [0, pointCount), not closed: iterate [1, pointCount - 1)
			int startIndex = isClosed ? 0 : 1;
			int endIndex = isClosed ? pointCount : ( pointCount - 1 );

			for( int i = startIndex; i < endIndex; ++i ) {
				gp_Pnt previous = points[ WrapIndex( i - 1, pointCount ) ];
				gp_Pnt current = points[ i ];
				gp_Pnt next = points[ WrapIndex( i + 1, pointCount ) ];

				gp_Vec incoming = new gp_Vec( previous, current );
				gp_Vec outgoing = new gp_Vec( current, next );

				if( ( incoming.Magnitude() > POINT_TOLERENCE ) && ( outgoing.Magnitude() > POINT_TOLERENCE ) ) {
					turningAngles[ i ] = incoming.Angle( outgoing );
				}
			}

			// find candidates
			for( int i = startIndex; i < endIndex; ++i ) {
				int previous1 = WrapIndex( i - 1, pointCount );
				int next1 = WrapIndex( i + 1, pointCount );
				double surroundingAngle = Math.Max( turningAngles[ previous1 ], turningAngles[ next1 ] );
				double angleIncrease = turningAngles[ i ] - surroundingAngle;

				if( angleIncrease > tangentToleranceAngle ) {
					strengths[ i ] = angleIncrease / tangentToleranceAngle;
				}
			}

			// find final G1 break points
			for( int i = startIndex; i < endIndex; ++i ) {
				int previous1 = WrapIndex( i - 1, pointCount );
				int next1 = WrapIndex( i + 1, pointCount );

				if( ( strengths[ i ] > 1.0 ) && ( strengths[ i ] >= strengths[ previous1 ] ) && ( strengths[ i ] > strengths[ next1 ] ) ) {
					breakIndices.Add( i );
				}
			}

			return breakIndices;
		}

		static List<int> FindG2BreakIndices( List<gp_Pnt> points, bool isClosed, List<int> g1BreakIndices )
		{
			if( points == null || points.Count < 5 ) {
				return new List<int>();
			}

			List<int> breakIndices = new List<int>();
			int pointCount = points.Count;
			double[] strengths = new double[ pointCount ];
			bool[] g1Breaks = new bool[ pointCount ];

			foreach( int breakIndex in g1BreakIndices ?? new List<int>() ) {
				if( ( breakIndex >= 0 ) && ( breakIndex < pointCount ) ) {
					g1Breaks[ breakIndex ] = true;
				}
			}

			// closed: iterate [0, pointCount), not closed: iterate [2, pointCount - 2)
			int startIndex = isClosed ? 0 : 2;
			int endIndex = isClosed ? pointCount : ( pointCount - 2 );

			// find candidates	
			for( int i = startIndex; i < endIndex; ++i ) {
				int previous2 = WrapIndex( i - 2, pointCount );
				int previous1 = WrapIndex( i - 1, pointCount );
				int next1 = WrapIndex( i + 1, pointCount );
				int next2 = WrapIndex( i + 2, pointCount );

				if( g1Breaks[ previous1 ] || g1Breaks[ i ] || g1Breaks[ next1 ] ) {
					continue;
				}

				bool hasLeftGeometry = TryGetEndpointGeometry( points[ previous2 ], points[ previous1 ], points[ i ],
					false, POINT_TOLERENCE, out gp_Vec leftTangent, out gp_Vec leftCurvature );

				bool hasRightGeometry = TryGetEndpointGeometry( points[ i ], points[ next1 ], points[ next2 ],
					true, POINT_TOLERENCE, out gp_Vec rightTangent, out gp_Vec rightCurvature );

				if( !hasLeftGeometry || !hasRightGeometry ) {
					continue;
				}

				gp_Vec curvatureDifference = new gp_Vec(
					rightCurvature.X() - leftCurvature.X(),
					rightCurvature.Y() - leftCurvature.Y(),
					rightCurvature.Z() - leftCurvature.Z() );

				double maximumCurvature = Math.Max( leftCurvature.Magnitude(), rightCurvature.Magnitude() );

				double allowedDifference = ABSOLUTE_CURVATURE_TOLERENCE + RELATIVE_CURVATURE_TOLERENCE * maximumCurvature;

				strengths[ i ] = curvatureDifference.Magnitude() / allowedDifference;
			}

			// find final G2 break points
			for( int i = startIndex; i < endIndex; ++i ) {
				int previous1 = WrapIndex( i - 1, pointCount );
				int next1 = WrapIndex( i + 1, pointCount );

				if( ( strengths[ i ] > 1.0 ) && ( strengths[ i ] >= strengths[ previous1 ] ) && ( strengths[ i ] > strengths[ next1 ] ) ) {
					breakIndices.Add( i );
				}
			}

			return breakIndices;
		}

		static bool TryGetEndpointGeometry( gp_Pnt point0, gp_Pnt point1, gp_Pnt point2, bool evaluateAtStart, double tolerance, out gp_Vec tangent, out gp_Vec curvature )
		{
			tangent = new gp_Vec( 0.0, 0.0, 0.0 );
			curvature = new gp_Vec( 0.0, 0.0, 0.0 );

			gp_Vec firstSegment = new gp_Vec( point0, point1 );
			gp_Vec secondSegment = new gp_Vec( point1, point2 );

			if( ( firstSegment.Magnitude() <= tolerance ) || ( secondSegment.Magnitude() <= tolerance ) ) {
				return false;
			}

			gp_Vec chord01 = new gp_Vec( point0, point1 );
			gp_Vec chord02 = new gp_Vec( point0, point2 );
			gp_Vec planeNormal = chord01.Crossed( chord02 );

			double normalSquared = planeNormal.SquareMagnitude();

			if( normalSquared <= ( tolerance * tolerance ) ) {
				tangent = evaluateAtStart ? firstSegment : secondSegment;
				return true;
			}

			gp_Pnt center = CalculateCircumcenter( point0, chord01, chord02, planeNormal );

			gp_Pnt endpoint = evaluateAtStart ? point0 : point2;
			gp_Vec centerToEndpoint = new gp_Vec( center, endpoint );
			double radiusSquared = centerToEndpoint.SquareMagnitude();

			if( radiusSquared <= ( tolerance * tolerance ) ) {
				return false;
			}

			curvature = new gp_Vec(
				-centerToEndpoint.X() / radiusSquared,
				-centerToEndpoint.Y() / radiusSquared,
				-centerToEndpoint.Z() / radiusSquared );

			tangent = planeNormal.Crossed( centerToEndpoint );

			return tangent.Magnitude() > tolerance;
		}

		static gp_Pnt CalculateCircumcenter( gp_Pnt origin, gp_Vec chord01, gp_Vec chord02, gp_Vec planeNormal )
		{
			double normalSquared = planeNormal.SquareMagnitude();
			double denominator = 2.0 * normalSquared;

			gp_Vec centerOffset1 = chord02.Crossed( planeNormal ).Multiplied( chord01.SquareMagnitude() );
			gp_Vec centerOffset2 = planeNormal.Crossed( chord01 ).Multiplied( chord02.SquareMagnitude() );

			return new gp_Pnt(
				origin.X() + ( centerOffset1.X() + centerOffset2.X() ) / denominator,
				origin.Y() + ( centerOffset1.Y() + centerOffset2.Y() ) / denominator,
				origin.Z() + ( centerOffset1.Z() + centerOffset2.Z() ) / denominator );
		}

		static int WrapIndex( int index, int count )
		{
			index %= count;
			return index < 0 ? index + count : index;
		}

		// STEP 2 classify segments type and calculate segement metrics

		const double LINE_DISTANCE_TOLERANCE = 0.01;
		const double ARC_DISTANCE_TOLERANCE = 0.01;

		static GeometrySegment AnalyzeSegment( List<gp_Pnt> points, int startIndex, int endIndex, double[] edgeLengths )
		{
			int edgeCount = ( startIndex < endIndex ) ? ( endIndex - startIndex ) : ( points.Count - startIndex + endIndex );
			GeometryEdgeType type = ClassifyGeometrySegment( points, startIndex, edgeCount );
			CalculateSegmentMetrics( points, startIndex, edgeCount, edgeLengths,
				out double pathLength, out double averageCurvature, out double curvatureVariation );

			return new GeometrySegment( startIndex, endIndex, points.Count, type, pathLength, averageCurvature, curvatureVariation );
		}

		static GeometryEdgeType ClassifyGeometrySegment( List<gp_Pnt> points, int startIndex, int edgeCount )
		{
			if( edgeCount < 1 ) {
				return GeometryEdgeType.Unknown;
			}

			if( !TryGetFarthestPointPair( points, startIndex, edgeCount, out gp_Pnt first, out gp_Pnt second ) ) {
				return GeometryEdgeType.Unknown;
			}

			gp_Vec baseVector = new gp_Vec( first, second );
			double baseLength = baseVector.Magnitude();
			if( baseLength <= POINT_TOLERENCE ) {
				return GeometryEdgeType.Curve;
			}

			bool isLine = true;
			for( int offset = 0; offset <= edgeCount; ++offset ) {
				gp_Vec toPoint = new gp_Vec( first, points[ ( startIndex + offset ) % points.Count ] );
				if( ( baseVector.Crossed( toPoint ).Magnitude() / baseLength ) > LINE_DISTANCE_TOLERANCE ) {
					isLine = false;
					break;
				}
			}
			if( isLine ) {
				return GeometryEdgeType.Line;
			}

			if( ( edgeCount < 2 ) || !TryCreateReferenceCircle( points, startIndex, edgeCount, first, baseVector, baseLength,
				out gp_Pnt center, out gp_Vec planeNormal, out double radius ) ) {
				return GeometryEdgeType.Curve;
			}

			double normalMagnitude = planeNormal.Magnitude();
			for( int offset = 0; offset <= edgeCount; ++offset ) {
				gp_Vec fromCenter = new gp_Vec( center, points[ ( startIndex + offset ) % points.Count ] );
				double planeDistance = Math.Abs( fromCenter.Dot( planeNormal ) ) / normalMagnitude;
				if( planeDistance > ARC_DISTANCE_TOLERANCE ) {
					return GeometryEdgeType.Curve;
				}
				double radiusError = Math.Abs( fromCenter.Magnitude() - radius );
				if( radiusError > ARC_DISTANCE_TOLERANCE ) {
					return GeometryEdgeType.Curve;
				}
			}

			return GeometryEdgeType.Arc;
		}

		static bool TryGetFarthestPointPair( List<gp_Pnt> points, int startIndex, int edgeCount, out gp_Pnt first, out gp_Pnt second )
		{
			first = null;
			second = null;
			gp_Pnt seed = points[ startIndex ];
			double maximumSquareDistance = 0.0;

			// First scan: seed -> second
			for( int offset = 0; offset <= edgeCount; ++offset ) {
				gp_Pnt candidate = points[ ( startIndex + offset ) % points.Count ];
				double squareDistance = seed.SquareDistance( candidate );

				if( squareDistance > maximumSquareDistance ) {
					maximumSquareDistance = squareDistance;
					second = candidate;
				}
			}

			if( second == null || maximumSquareDistance <= POINT_TOLERENCE * POINT_TOLERENCE ) {
				return false;
			}

			// Second scan: second -> first
			maximumSquareDistance = 0.0;
			for( int offset = 0; offset <= edgeCount; ++offset ) {
				gp_Pnt candidate = points[ ( startIndex + offset ) % points.Count ];
				double squareDistance = second.SquareDistance( candidate );

				if( squareDistance > maximumSquareDistance ) {
					maximumSquareDistance = squareDistance;
					first = candidate;
				}
			}

			return first != null && maximumSquareDistance > POINT_TOLERENCE * POINT_TOLERENCE;
		}

		static bool TryCreateReferenceCircle( List<gp_Pnt> points, int startIndex, int edgeCount, gp_Pnt first, gp_Vec baseVector, double baseLength,
			out gp_Pnt center, out gp_Vec planeNormal, out double radius )
		{
			center = null;
			planeNormal = null;
			radius = 0.0;
			gp_Pnt third = null;
			double maximumDistance = 0.0;

			for( int offset = 0; offset <= edgeCount; ++offset ) {
				gp_Pnt candidate = points[ ( startIndex + offset ) % points.Count ];
				gp_Vec toCandidate = new gp_Vec( first, candidate );
				double distance = baseVector.Crossed( toCandidate ).Magnitude() / baseLength;
				if( distance > maximumDistance ) {
					maximumDistance = distance;
					third = candidate;
				}
			}

			if( ( third == null ) || ( maximumDistance <= ARC_DISTANCE_TOLERANCE ) ) {
				return false;
			}

			gp_Vec chord02 = new gp_Vec( first, third );
			planeNormal = baseVector.Crossed( chord02 );
			double normalSquared = planeNormal.SquareMagnitude();
			if( normalSquared <= ( POINT_TOLERENCE * POINT_TOLERENCE ) ) {
				return false;
			}

			center = CalculateCircumcenter( first, baseVector, chord02, planeNormal );
			radius = center.Distance( first );

			return radius > ARC_DISTANCE_TOLERANCE;
		}

		static void CalculateSegmentMetrics( List<gp_Pnt> points, int startIndex, int edgeCount, double[] edgeLengths,
			out double pathLength, out double averageCurvature, out double curvatureVariation )
		{
			pathLength = 0.0;
			averageCurvature = 0.0;
			curvatureVariation = 0.0;
			if( edgeLengths.Length == 0 ) {
				return;
			}

			for( int offset = 0; offset < edgeCount; ++offset ) {
				pathLength += edgeLengths[ ( startIndex + offset ) % points.Count ];
			}
			if( edgeCount < 2 || ( pathLength <= POINT_TOLERENCE ) ) {
				return;
			}

			double accumulatedLength = 0.0;
			double curvatureSum = 0.0;
			int curvatureCount = 0;
			double previousCurvature = 0.0;
			double previousPosition = 0.0;
			double weightedSquareSum = 0.0;
			double totalWeight = 0.0;

			for( int offset = 1; offset < edgeCount; ++offset ) {
				int previousIndex = ( startIndex + offset - 1 ) % points.Count;
				int currentIndex = ( startIndex + offset ) % points.Count;
				int nextIndex = ( startIndex + offset + 1 ) % points.Count;
				double length01 = edgeLengths[ previousIndex ];
				double length12 = edgeLengths[ currentIndex ];
				accumulatedLength += length01;

				gp_Vec side02 = new gp_Vec( points[ previousIndex ], points[ nextIndex ] );
				double length02 = side02.Magnitude();
				if( ( length01 <= POINT_TOLERENCE ) || ( length12 <= POINT_TOLERENCE ) || ( length02 <= POINT_TOLERENCE ) ) {
					continue;
				}

				gp_Vec side01 = new gp_Vec( points[ previousIndex ], points[ currentIndex ] );
				double doubleArea = side01.Crossed( side02 ).Magnitude();
				double curvature = 2.0 * doubleArea / ( length01 * length12 * length02 );
				if( curvatureCount > 0 ) {
					double deltaLength = accumulatedLength - previousPosition;
					if( deltaLength > POINT_TOLERENCE ) {
						double curvatureRate = ( curvature - previousCurvature ) / deltaLength;
						weightedSquareSum += curvatureRate * curvatureRate * deltaLength;
						totalWeight += deltaLength;
					}
				}

				curvatureSum += curvature;
				curvatureCount++;
				previousCurvature = curvature;
				previousPosition = accumulatedLength;
			}

			if( curvatureCount > 0 ) {
				averageCurvature = curvatureSum / curvatureCount;
			}
			if( totalWeight > POINT_TOLERENCE ) {
				curvatureVariation = Math.Sqrt( weightedSquareSum / totalWeight );
			}
		}

		// STEP 3 sort the priority

		const double ABSOLUTE_PATH_LENGTH_GROUP_TOLERENCE = 0.01;
		const double RELATIVE_PATH_LENGTH_GROUP_TOLERENCE = 0.05;
		const double ABSOLUTE_AVERAGE_CURVATURE_GROUP_TOLERANCE = 1e-4;
		const double RELATIVE_AVERAGE_CURVATURE_GROUP_TOLERANCE = 0.05;
		const double ABSOLUTE_CURVATURE_VARIATION_GROUP_TOLERANCE = 1e-4;
		const double RELATIVE_CURVATURE_VARIATION_GROUP_TOLERANCE = 0.05;

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
