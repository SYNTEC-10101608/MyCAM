using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.Data;
using OCC.gp;

namespace MyCAM.Helper
{
	public enum GeometryEdgeType
	{
		Line,
		Arc,
		Curve,
		Unknown
	}

	public class GeometrySegment
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

	internal class GeometrySegmentHelper
	{
		const double POINT_TOLERENCE = 1e-6;
		const double ABSOLUTE_TANGENT_TOLERENCE = 5.0;
		const double RELATIVE_TANGENT_TOLERENCE = 0.2;
		const double ABSOLUTE_CURVATURE_TOLERENCE = 0.01;
		const double RELATIVE_CURVATURE_TOLERENCE = 0.1;

		const double ABSOLUTE_LINE_DISTANCE_TOLERANCE = 0.01;
		const double RELATIVE_LINE_DISTANCE_TOLERANCE = 0.1;
		const double ABSOLUTE_ARC_DISTANCE_TOLERANCE = 0.01;
		const double RELATIVE_ARC_DISTANCE_TOLERANCE = 0.1;

		public static List<GeometrySegment> BuildGeometrySegments( List<gp_Pnt> points, bool isClosed )
		{
			List<GeometrySegment> segments = new List<GeometrySegment>();
			if( points == null || points.Count == 0 ) {
				return segments;
			}

			int pointCount = points.Count;
			if( pointCount == 0 ) {
				return segments;
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

			// validate segment metrics
			foreach( GeometrySegment segment in segments ) {
				if( segment == null ||
					!( ( segment.PathLength >= 0.0 ) && ( segment.PathLength <= double.MaxValue ) ) ||
					!( ( segment.AverageCurvature >= 0.0 ) && ( segment.AverageCurvature <= double.MaxValue ) ) ||
					!( ( segment.CurvatureVariation >= 0.0 ) && ( segment.CurvatureVariation <= double.MaxValue ) ) ) {
					return new List<GeometrySegment>();
				}
			}

			return segments;
		}

		public static List<GeometrySegment> BuildGeometrySegments( List<CAMPoint> camPointList )
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

			return BuildGeometrySegments( points, isClosed );
		}

		// STEP 1 calculate G1 and G2 continuity

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
			double absoluteTangentToleranceAngle = ABSOLUTE_TANGENT_TOLERENCE * Math.PI / 180.0;
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

				if( ( incoming.SquareMagnitude() > POINT_TOLERENCE * POINT_TOLERENCE ) && ( outgoing.SquareMagnitude() > POINT_TOLERENCE * POINT_TOLERENCE ) ) {
					turningAngles[ i ] = incoming.Angle( outgoing );
				}
			}

			// find candidates
			for( int i = startIndex; i < endIndex; ++i ) {
				int previous1 = WrapIndex( i - 1, pointCount );
				int next1 = WrapIndex( i + 1, pointCount );
				double surroundingAngle = Math.Max( turningAngles[ previous1 ], turningAngles[ next1 ] );
				double angleIncrease = turningAngles[ i ] - surroundingAngle;
				double tangentToleranceAngle = absoluteTangentToleranceAngle + RELATIVE_TANGENT_TOLERENCE * surroundingAngle;

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
			double toleranceSquared = tolerance * tolerance;

			if( firstSegment.SquareMagnitude() <= toleranceSquared || secondSegment.SquareMagnitude() <= toleranceSquared ) {
				return false;
			}

			gp_Vec chord02 = new gp_Vec( point0, point2 );
			double chordLengthSquared = chord02.SquareMagnitude();
			if( chordLengthSquared <= toleranceSquared ) {
				return false;
			}

			gp_Vec planeNormal = firstSegment.Crossed( chord02 );
			double distanceToChordSquared = planeNormal.SquareMagnitude() / chordLengthSquared;
			if( distanceToChordSquared <= toleranceSquared ) {
				tangent = evaluateAtStart ? firstSegment : secondSegment;
				return true;
			}

			gp_Pnt center = CalculateCircumcenter( point0, firstSegment, chord02, planeNormal );
			gp_Pnt endpoint = evaluateAtStart ? point0 : point2;
			gp_Vec centerToEndpoint = new gp_Vec( center, endpoint );
			double radiusSquared = centerToEndpoint.SquareMagnitude();

			if( radiusSquared <= toleranceSquared ) {
				return false;
			}

			curvature = new gp_Vec(
				-centerToEndpoint.X() / radiusSquared,
				-centerToEndpoint.Y() / radiusSquared,
				-centerToEndpoint.Z() / radiusSquared );

			tangent = planeNormal.Crossed( centerToEndpoint );
			return true;
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
			double baseLengthSquared = baseVector.SquareMagnitude();
			if( baseLengthSquared <= POINT_TOLERENCE * POINT_TOLERENCE ) {
				return GeometryEdgeType.Curve;
			}

			double lineDistanceTolerance = ABSOLUTE_LINE_DISTANCE_TOLERANCE +
				RELATIVE_LINE_DISTANCE_TOLERANCE * Math.Sqrt( baseLengthSquared );
			bool isLine = true;
			for( int offset = 0; offset <= edgeCount; ++offset ) {
				gp_Vec toPoint = new gp_Vec( first, points[ ( startIndex + offset ) % points.Count ] );
				if( ( baseVector.Crossed( toPoint ).SquareMagnitude() / baseLengthSquared ) > lineDistanceTolerance * lineDistanceTolerance ) {
					isLine = false;
					break;
				}
			}
			if( isLine ) {
				return GeometryEdgeType.Line;
			}

			if( ( edgeCount < 2 ) || !TryCreateReferenceCircle( points, startIndex, edgeCount, first, baseVector, baseLengthSquared,
				out gp_Pnt center, out gp_Vec planeNormal, out double radius ) ) {
				return GeometryEdgeType.Curve;
			}

			double normalSquared = planeNormal.SquareMagnitude();
			double arcDistanceTolerance = ABSOLUTE_ARC_DISTANCE_TOLERANCE +
				RELATIVE_ARC_DISTANCE_TOLERANCE * radius;
			for( int offset = 0; offset <= edgeCount; ++offset ) {
				gp_Vec fromCenter = new gp_Vec( center, points[ ( startIndex + offset ) % points.Count ] );
				double normalProjection = fromCenter.Dot( planeNormal );
				double planeDistanceSquared = normalProjection * normalProjection / normalSquared;
				if( planeDistanceSquared > arcDistanceTolerance * arcDistanceTolerance ) {
					return GeometryEdgeType.Curve;
				}
				double radiusError = Math.Abs( fromCenter.Magnitude() - radius );
				if( radiusError > arcDistanceTolerance ) {
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

		static bool TryCreateReferenceCircle( List<gp_Pnt> points, int startIndex, int edgeCount, gp_Pnt first, gp_Vec baseVector, double baseLengthSquared,
			out gp_Pnt center, out gp_Vec planeNormal, out double radius )
		{
			center = null;
			planeNormal = null;
			radius = 0.0;
			gp_Pnt third = null;
			double maximumDistanceSquared = 0.0;

			for( int offset = 0; offset <= edgeCount; ++offset ) {
				gp_Pnt candidate = points[ ( startIndex + offset ) % points.Count ];
				gp_Vec toCandidate = new gp_Vec( first, candidate );
				double distanceSquared = baseVector.Crossed( toCandidate ).SquareMagnitude() / baseLengthSquared;
				if( distanceSquared > maximumDistanceSquared ) {
					maximumDistanceSquared = distanceSquared;
					third = candidate;
				}
			}

			if( ( third == null ) || ( maximumDistanceSquared <= ABSOLUTE_ARC_DISTANCE_TOLERANCE * ABSOLUTE_ARC_DISTANCE_TOLERANCE ) ) {
				return false;
			}

			gp_Vec chord02 = new gp_Vec( first, third );
			planeNormal = baseVector.Crossed( chord02 );
			double normalSquared = planeNormal.SquareMagnitude();
			if( normalSquared / baseLengthSquared <= POINT_TOLERENCE * POINT_TOLERENCE ) {
				return false;
			}

			center = CalculateCircumcenter( first, baseVector, chord02, planeNormal );
			radius = center.Distance( first );

			return radius > ABSOLUTE_ARC_DISTANCE_TOLERANCE;
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
	}
}
