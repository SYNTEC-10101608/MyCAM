using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.Data;
using OCC.gp;

namespace MyCAM.Helper
{
	internal class PatternRecognizeHelper
	{
		const double POINT_TOLERENCE = 1e-6;
		const double ABSOLUTE_TANGENT_TOLERENCE_DEGREE = 5.0;
		const double RELATIVE_TANGENT_TOLERENCE = 0.2;
		const double ABSOLUTE_LINE_DISTANCE_TOLERANCE = 0.01;
		const double RELATIVE_LINE_DISTANCE_TOLERANCE = 0.1;

		const double ABSOLUTE_PLANAR_TOLERANCE = 0.01;
		const double RELATIVE_PLANAR_TOLERANCE = 0.2;

		interface IShapeRecognizer
		{
			PathType PathType { get; }

			bool IsMatch( IReadOnlyList<gp_Pnt> points, IReadOnlyList<GeometrySegment> segments );
		}

		static readonly IShapeRecognizer[] s_ShapeRecognizers =
		{
			new CircleRecognizer(),
			new RunwayRecognizer(),
			new RegularPolygonRecognizer( 3, PathType.Triangle ),
			new RegularPolygonRecognizer( 4, PathType.Square ),
			new RegularPolygonRecognizer( 5, PathType.Pentagon ),
			new RegularPolygonRecognizer( 6, PathType.Hexagon ),
			new RectangleRecognizer()
		};

		public static bool TryRecognizeShape( IReadOnlyList<gp_Pnt> points, IReadOnlyList<GeometrySegment> segments, bool isClosed, out PathType pathType )
		{
			pathType = PathType.Contour;
			if( !isClosed || !IsClosedContour( points, segments ) ) {
				return false;
			}

			foreach( IShapeRecognizer recognizer in s_ShapeRecognizers ) {
				if( recognizer.IsMatch( points, segments ) ) {
					pathType = recognizer.PathType;
					return true;
				}
			}

			return false;
		}

		internal sealed class ShapeRecognitionResult
		{
			public PathType PathType { get; }

			public double Radius { get; }

			public double Length { get; }

			public double Width { get; }

			public double CornerRadius { get; }

			public int Sides { get; }

			public double SideLength { get; }

			public double RotatedAngle_deg { get; }

			public ShapeRecognitionResult( PathType pathType, double radius, double length, double width,
				double cornerRadius, int sides, double sideLength, double rotatedAngle_deg )
			{
				PathType = pathType;
				Radius = radius;
				Length = length;
				Width = width;
				CornerRadius = cornerRadius;
				Sides = sides;
				SideLength = sideLength;
				RotatedAngle_deg = rotatedAngle_deg;
			}
		}

		public static bool TryAnalyzeShape( List<CAMPoint> camPointList, gp_Ax3 coordinate, out ShapeRecognitionResult result )
		{
			result = null;

			if( !TryProjectToCoordinatePlane( camPointList, coordinate,
				out List<gp_Pnt> projectedPoints, out bool isClosed ) ) {
				return false;
			}

			List<GeometrySegment> segments = GeometrySegmentHelper.BuildGeometrySegments( projectedPoints, isClosed );
			return TryAnalyzeShape( coordinate, projectedPoints, segments, isClosed, out result );
		}

		static bool TryProjectToCoordinatePlane( IReadOnlyList<CAMPoint> camPointList, gp_Ax3 coordinate, out List<gp_Pnt> projectedPoints, out bool isClosed )
		{
			projectedPoints = new List<gp_Pnt>();
			isClosed = false;

			if( camPointList == null || camPointList.Count == 0 || coordinate == null ) {
				return false;
			}

			isClosed = SpecialPointIndex.IsClosedPointIndex(
				camPointList[ camPointList.Count - 1 ].InitPathIndex );

			int pointCount = isClosed ? ( camPointList.Count - 1 ) : camPointList.Count;
			if( pointCount < 2 ) {
				return false;
			}

			gp_Pnt origin = coordinate.Location();
			gp_Vec xAxis = new gp_Vec( coordinate.XDirection() );
			gp_Vec yAxis = new gp_Vec( coordinate.YDirection() );
			gp_Vec normal = new gp_Vec( coordinate.Direction() );

			double minX = double.PositiveInfinity;
			double maxX = double.NegativeInfinity;
			double minY = double.PositiveInfinity;
			double maxY = double.NegativeInfinity;
			double maxPlaneDistance = 0.0;

			for( int index = 0; index < camPointList.Count; ++index ) {
				gp_Vec offset = new gp_Vec( origin, camPointList[ index ].Point );
				double planeDistance = Math.Abs( offset.Dot( normal ) );
				if( double.IsNaN( planeDistance ) || double.IsInfinity( planeDistance ) ) {
					return false;
				}
				maxPlaneDistance = Math.Max( maxPlaneDistance, planeDistance );

				if( index == pointCount ) {
					continue;
				}

				double x = offset.Dot( xAxis );
				double y = offset.Dot( yAxis );
				if( double.IsNaN( x ) || double.IsInfinity( x ) ||
					double.IsNaN( y ) || double.IsInfinity( y ) ) {
					return false;
				}

				minX = Math.Min( minX, x );
				maxX = Math.Max( maxX, x );
				minY = Math.Min( minY, y );
				maxY = Math.Max( maxY, y );

				gp_Vec projectedOffset = xAxis.Multiplied( x );
				projectedOffset.Add( yAxis.Multiplied( y ) );
				gp_Pnt projectedPoint = origin.Translated( projectedOffset );

				if( projectedPoints.Count == 0 ||
					projectedPoints[ projectedPoints.Count - 1 ].Distance( projectedPoint ) > POINT_TOLERENCE ) {
					projectedPoints.Add( projectedPoint );
				}
			}

			double width = maxX - minX;
			double height = maxY - minY;
			double size = Math.Sqrt( width * width + height * height );
			if( double.IsNaN( size ) || double.IsInfinity( size ) ) {
				return false;
			}

			double tolerance = Math.Max( ABSOLUTE_PLANAR_TOLERANCE, RELATIVE_PLANAR_TOLERANCE * size );

			return projectedPoints.Count >= ( isClosed ? 3 : 2 ) &&
				maxPlaneDistance <= tolerance;
		}

		public static bool TryAnalyzeShape( gp_Ax3 coordinate, IReadOnlyList<gp_Pnt> points, IReadOnlyList<GeometrySegment> segments, bool isClosed,
			out ShapeRecognitionResult result )
		{
			result = null;
			if( coordinate == null || !TryRecognizeShape( points, segments, isClosed, out PathType pathType ) ) {
				return false;
			}

			List<GeometrySegment> lineSegments = GetLineSegments( segments );
			double cornerRadius = HasArcSegments( segments ) ? GetAverageArcRadius( segments ) : 0.0;
			if( HasArcSegments( segments ) && cornerRadius <= POINT_TOLERENCE ) {
				return false;
			}

			switch( pathType ) {
				case PathType.Circle:
					result = new ShapeRecognitionResult( pathType, cornerRadius, 0.0, 0.0, 0.0, 0, 0.0, 0.0 );
					return true;

				case PathType.Rectangle:
					return TryCreateRectangleResult( coordinate, points, pathType, lineSegments, cornerRadius, out result );

				case PathType.Runway:
					return TryCreateRunwayResult( coordinate, points, pathType, lineSegments, cornerRadius, out result );

				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					return TryCreateRegularPolygonResult( coordinate, points, pathType, lineSegments, cornerRadius, out result );

				default:
					return false;
			}
		}

		static bool TryCreateRectangleResult( gp_Ax3 coordinate, IReadOnlyList<gp_Pnt> points, PathType pathType, IReadOnlyList<GeometrySegment> lineSegments,
			double cornerRadius, out ShapeRecognitionResult result )
		{
			result = null;
			if( lineSegments.Count != 4 ) {
				return false;
			}

			double length = lineSegments.Max( segment => segment.PathLength ) + 2.0 * cornerRadius;
			double width = lineSegments.Min( segment => segment.PathLength ) + 2.0 * cornerRadius;
			double angle = GetLineAngleDegrees( coordinate, points, lineSegments.OrderByDescending( segment => segment.PathLength ).First(), 180.0 );
			result = new ShapeRecognitionResult( pathType, 0.0, length, width, cornerRadius, 0, 0.0, angle );
			return true;
		}

		static bool TryCreateRunwayResult( gp_Ax3 coordinate, IReadOnlyList<gp_Pnt> points, PathType pathType, IReadOnlyList<GeometrySegment> lineSegments,
			double radius, out ShapeRecognitionResult result )
		{
			result = null;
			if( lineSegments.Count != 2 || radius <= POINT_TOLERENCE ) {
				return false;
			}

			double width = 2.0 * radius;
			double length = lineSegments.Average( segment => segment.PathLength ) + width;
			double angle = GetLineAngleDegrees( coordinate, points, lineSegments[ 0 ], 180.0 );
			result = new ShapeRecognitionResult( pathType, 0.0, length, width, 0.0, 0, 0.0, angle );
			return true;
		}

		static bool TryCreateRegularPolygonResult( gp_Ax3 coordinate, IReadOnlyList<gp_Pnt> points, PathType pathType, IReadOnlyList<GeometrySegment> lineSegments,
			double cornerRadius, out ShapeRecognitionResult result )
		{
			result = null;
			int sides = lineSegments.Count;
			if( sides < 3 || sides > 6 ) {
				return false;
			}

			double sideLength = lineSegments.Average( segment => segment.PathLength );
			if( cornerRadius > 0.0 ) {
				double interiorAngle = ( sides - 2.0 ) * Math.PI / sides;
				sideLength += 2.0 * cornerRadius / Math.Tan( interiorAngle / 2.0 );
			}
			double angle = GetRegularPolygonRotatedAngleDegrees( coordinate, points, lineSegments[ 0 ], sides );
			result = new ShapeRecognitionResult( pathType, 0.0, 0.0, 0.0, cornerRadius, sides, sideLength, angle );
			return true;
		}

		static bool HasArcSegments( IReadOnlyList<GeometrySegment> segments )
		{
			return segments.Any( segment => segment.Type == GeometryEdgeType.Arc );
		}

		static bool IsClosedContour( IReadOnlyList<gp_Pnt> points, IReadOnlyList<GeometrySegment> segments )
		{
			if( points == null || points.Count < 3 || segments == null || segments.Count == 0 ) {
				return false;
			}

			foreach( GeometrySegment segment in segments ) {
				if( segment == null || segment.OrignPathCount != points.Count ||
					segment.StartCAMIndex < 0 || segment.StartCAMIndex >= points.Count ||
					segment.EndCAMIndex < 0 || segment.EndCAMIndex >= points.Count ||
					points[ segment.StartCAMIndex ] == null || points[ segment.EndCAMIndex ] == null ) {
					return false;
				}
			}

			for( int i = 0; i < segments.Count; ++i ) {
				GeometrySegment current = segments[ i ];
				GeometrySegment next = segments[ ( i + 1 ) % segments.Count ];
				if( points[ current.EndCAMIndex ].Distance( points[ next.StartCAMIndex ] ) > POINT_TOLERENCE ) {
					return false;
				}
			}

			return true;
		}

		static double GetAverageArcRadius( IReadOnlyList<GeometrySegment> segments )
		{
			List<double> radii = segments
				.Where( segment => segment.Type == GeometryEdgeType.Arc && segment.AverageCurvature > POINT_TOLERENCE )
				.Select( segment => 1.0 / segment.AverageCurvature )
				.ToList();
			return radii.Count == 0 ? 0.0 : radii.Average();
		}

		static double GetLineAngleDegrees( gp_Ax3 coordinate, IReadOnlyList<gp_Pnt> points, GeometrySegment segment, double periodDegrees )
		{
			gp_Vec direction = GetDirection( points, segment );
			double x = direction.Dot( new gp_Vec( coordinate.XDirection() ) );
			double y = direction.Dot( new gp_Vec( coordinate.YDirection() ) );
			double angle = Math.Atan2( y, x ) * 180.0 / Math.PI;
			return NormalizeAngleDegrees( angle, periodDegrees );
		}

		static double NormalizeAngleDegrees( double angleDegrees, double periodDegrees )
		{
			double normalized = angleDegrees % periodDegrees;
			return normalized < 0.0 ? normalized + periodDegrees : normalized;
		}

		sealed class CircleRecognizer : IShapeRecognizer
		{
			public PathType PathType => PathType.Circle;

			public bool IsMatch( IReadOnlyList<gp_Pnt> points, IReadOnlyList<GeometrySegment> segments )
			{
				return segments.Count == 1 &&
					segments[ 0 ].Type == GeometryEdgeType.Arc &&
					points[ segments[ 0 ].StartCAMIndex ].Distance( points[ segments[ 0 ].EndCAMIndex ] ) <= POINT_TOLERENCE;
			}
		}

		sealed class RunwayRecognizer : IShapeRecognizer
		{
			public PathType PathType => PathType.Runway;

			public bool IsMatch( IReadOnlyList<gp_Pnt> points, IReadOnlyList<GeometrySegment> segments )
			{
				List<GeometrySegment> lineSegments = GetLineSegments( segments );
				return HasAlternatingSegmentTypes( segments, 2 ) && AreParallel( points, lineSegments[ 0 ], lineSegments[ 1 ] );
			}
		}

		sealed class RectangleRecognizer : IShapeRecognizer
		{
			public PathType PathType => PathType.Rectangle;

			public bool IsMatch( IReadOnlyList<gp_Pnt> points, IReadOnlyList<GeometrySegment> segments )
			{
				List<GeometrySegment> lineSegments = GetLineSegments( segments );
				return ( HasLineSegments( segments, 4 ) || HasAlternatingSegmentTypes( segments, 4 ) ) &&
					IsRectangle( points, lineSegments );
			}
		}

		sealed class RegularPolygonRecognizer : IShapeRecognizer
		{
			readonly int m_Sides;
			readonly PathType m_PathType;

			public RegularPolygonRecognizer( int sides, PathType pathType )
			{
				m_Sides = sides;
				m_PathType = pathType;
			}

			public PathType PathType => m_PathType;

			public bool IsMatch( IReadOnlyList<gp_Pnt> points, IReadOnlyList<GeometrySegment> segments )
			{
				if( HasLineSegments( segments, m_Sides ) ) {
					return IsRegularPolygon( points, GetLineSegments( segments ) );
				}

				return HasAlternatingSegmentTypes( segments, m_Sides ) &&
					IsRegularPolygon( points, GetLineSegments( segments ) );
			}
		}

		static bool HasLineSegments( IReadOnlyList<GeometrySegment> segments, int count )
		{
			return segments.Count == count && segments.All( segment => segment.Type == GeometryEdgeType.Line );
		}

		static bool HasAlternatingSegmentTypes( IReadOnlyList<GeometrySegment> segments, int pairCount )
		{
			if( segments.Count != pairCount * 2 ) {
				return false;
			}

			foreach( GeometryEdgeType firstType in new[] { GeometryEdgeType.Line, GeometryEdgeType.Arc } ) {
				bool isMatch = true;
				for( int i = 0; i < segments.Count; ++i ) {
					GeometryEdgeType expectedType = ( i % 2 == 0 ) ? firstType :
						( firstType == GeometryEdgeType.Line ? GeometryEdgeType.Arc : GeometryEdgeType.Line );
					if( segments[ i ].Type != expectedType ) {
						isMatch = false;
						break;
					}
				}
				if( isMatch ) {
					return true;
				}
			}

			return false;
		}

		static List<GeometrySegment> GetLineSegments( IReadOnlyList<GeometrySegment> segments )
		{
			return segments.Where( segment => segment.Type == GeometryEdgeType.Line ).ToList();
		}

		static bool IsRectangle( IReadOnlyList<gp_Pnt> points, IReadOnlyList<GeometrySegment> lineSegments )
		{
			if( lineSegments.Count != 4 ) {
				return false;
			}

			return ArePerpendicular( points, lineSegments[ 0 ], lineSegments[ 1 ] ) &&
				ArePerpendicular( points, lineSegments[ 1 ], lineSegments[ 2 ] ) &&
				ArePerpendicular( points, lineSegments[ 2 ], lineSegments[ 3 ] ) &&
				ArePerpendicular( points, lineSegments[ 3 ], lineSegments[ 0 ] ) &&
				AreParallel( points, lineSegments[ 0 ], lineSegments[ 2 ] ) &&
				AreParallel( points, lineSegments[ 1 ], lineSegments[ 3 ] );
		}

		static bool IsRegularPolygon( IReadOnlyList<gp_Pnt> points, IReadOnlyList<GeometrySegment> lineSegments )
		{
			if( lineSegments.Count < 3 || !HaveEqualLineLengths( lineSegments ) ) {
				return false;
			}

			double expectedAngle = 2.0 * Math.PI / lineSegments.Count;
			double angleTolerance = GetTangentToleranceRadians( expectedAngle );
			for( int i = 0; i < lineSegments.Count; ++i ) {
				gp_Vec current = GetDirection( points, lineSegments[ i ] );
				gp_Vec next = GetDirection( points, lineSegments[ ( i + 1 ) % lineSegments.Count ] );
				if( Math.Abs( current.Angle( next ) - expectedAngle ) > angleTolerance ) {
					return false;
				}
			}

			return true;
		}

		static bool HaveEqualLineLengths( IReadOnlyList<GeometrySegment> lineSegments )
		{
			double referenceLength = lineSegments[ 0 ].PathLength;
			if( referenceLength <= POINT_TOLERENCE ) {
				return false;
			}

			double lengthTolerance = ABSOLUTE_LINE_DISTANCE_TOLERANCE + RELATIVE_LINE_DISTANCE_TOLERANCE * referenceLength;
			return lineSegments.All( segment => Math.Abs( segment.PathLength - referenceLength ) <= lengthTolerance );
		}

		static bool AreParallel( IReadOnlyList<gp_Pnt> points, GeometrySegment first, GeometrySegment second )
		{
			gp_Vec firstDirection = GetDirection( points, first );
			gp_Vec secondDirection = GetDirection( points, second );
			double angleTolerance = GetTangentToleranceRadians( 0.0 );
			return ( firstDirection.Crossed( secondDirection ).Magnitude() /
				( firstDirection.Magnitude() * secondDirection.Magnitude() ) ) <= Math.Sin( angleTolerance );
		}

		static bool ArePerpendicular( IReadOnlyList<gp_Pnt> points, GeometrySegment first, GeometrySegment second )
		{
			gp_Vec firstDirection = GetDirection( points, first );
			gp_Vec secondDirection = GetDirection( points, second );
			double angleTolerance = GetTangentToleranceRadians( Math.PI / 2.0 );
			return Math.Abs( firstDirection.Dot( secondDirection ) ) /
				( firstDirection.Magnitude() * secondDirection.Magnitude() ) <= Math.Sin( angleTolerance );
		}

		static double GetTangentToleranceRadians( double referenceAngle )
		{
			return ABSOLUTE_TANGENT_TOLERENCE_DEGREE * Math.PI / 180.0 + RELATIVE_TANGENT_TOLERENCE * Math.Abs( referenceAngle );
		}

		static gp_Vec GetDirection( IReadOnlyList<gp_Pnt> points, GeometrySegment segment )
		{
			return new gp_Vec( points[ segment.StartCAMIndex ], points[ segment.EndCAMIndex ] );
		}

		static double GetRegularPolygonRotatedAngleDegrees( gp_Ax3 coordinate, IReadOnlyList<gp_Pnt> points, GeometrySegment segment, int sides )
		{
			gp_Vec direction = GetDirection( points, segment );
			gp_Pnt startPoint = points[ segment.StartCAMIndex ];
			gp_Pnt endPoint = points[ segment.EndCAMIndex ];

			gp_Pnt midpoint = new gp_Pnt(
				( startPoint.X() + endPoint.X() ) / 2.0,
				( startPoint.Y() + endPoint.Y() ) / 2.0,
				( startPoint.Z() + endPoint.Z() ) / 2.0 );

			gp_Vec centerToMidpoint = new gp_Vec( coordinate.Location(), midpoint );
			gp_Vec normal = new gp_Vec( coordinate.Direction() );

			// StdPatternDiscreteFactory builds polygons in clockwise order
			// Unifies the edge direction of input contours to prevent reversed paths from affecting angle calculations
			if( centerToMidpoint.Crossed( direction ).Dot( normal ) > POINT_TOLERENCE ) {
				direction = new gp_Vec( endPoint, startPoint );
			}

			double x = direction.Dot( new gp_Vec( coordinate.XDirection() ) );
			double y = direction.Dot( new gp_Vec( coordinate.YDirection() ) );
			double edgeAngle = Math.Atan2( y, x ) * 180.0 / Math.PI;

			return NormalizeAngleDegrees( edgeAngle - GetPolygonFirstEdgeAngleDegrees( sides ), 360.0 / sides );
		}

		static double GetPolygonFirstEdgeAngleDegrees( int sides )
		{
			double vertexAngle;
			switch( sides ) {
				case 3:
					vertexAngle = 90.0;
					break;
				case 4:
					vertexAngle = 45.0;
					break;
				case 5:
					vertexAngle = 90.0;
					break;
				case 6:
					vertexAngle = 0.0;
					break;
				default:
					return 0.0;
			}

			return vertexAngle - 180.0 / sides - 90.0;
		}
	}
}
