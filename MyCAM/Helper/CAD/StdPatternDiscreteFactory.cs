using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	internal static class StdPatternDiscreteFactory
	{
		#region Public API

		public static List<CADPoint> DiscretizeCircle( double diameter, gp_Trsf transformation )
		{
			if( diameter <= 0 ) {
				return new List<CADPoint>();
			}

			double radius = diameter / 2.0;
			return GenerateCirclePath( radius, transformation );
		}

		public static List<CADPoint> DiscretizeRectangle( double width, double length, double cornerRadius, gp_Trsf transformation )
		{
			if( width <= 0 || length <= 0 || cornerRadius < 0 ) {
				return new List<CADPoint>();
			}

			double maxCornerRadius = Math.Min( width, length ) / 2.0 * 0.9;
			if( cornerRadius > maxCornerRadius ) {
				cornerRadius = maxCornerRadius;
			}

			if( cornerRadius <= DEFAULT_PRECISION_MIN_ERROR ) {
				return GenerateSharpRectanglePath( width, length, transformation );
			}
			else {
				return GenerateRoundedRectanglePath( width, length, cornerRadius, transformation );
			}
		}

		public static List<CADPoint> DiscretizeRunway( double length, double width, gp_Trsf transformation )
		{
			if( length <= 0 || width <= 0 || length < width ) {
				return new List<CADPoint>();
			}

			return GenerateRunwayPath( length, width, transformation );
		}

		public static List<CADPoint> DiscretizePolygon( int sides, double sideLength, double cornerRadius, gp_Trsf transformation )
		{
			if( sides < 3 || sideLength <= 0 || cornerRadius < 0 ) {
				return new List<CADPoint>();
			}

			double circumradius = sideLength / ( 2.0 * Math.Sin( Math.PI / sides ) );
			double maxCornerRadius = CalculateMaxCornerRadius( sides, sideLength );
			if( cornerRadius > maxCornerRadius ) {
				cornerRadius = maxCornerRadius;
			}

			double angleOffset = GetPolygonAngleOffset( sides );

			if( cornerRadius <= DEFAULT_PRECISION_MIN_ERROR ) {
				return GenerateSharpPolygonPath( sides, circumradius, angleOffset, transformation );
			}
			else {
				return GenerateRoundedPolygonPath( sides, sideLength, cornerRadius, angleOffset, transformation, circumradius );
			}
		}

		#endregion

		#region Circle Path Generation

		static List<CADPoint> GenerateCirclePath( double radius, gp_Trsf transformation )
		{
			const int DEFAULT_MIN_CIRCLE_SEGMENTS = 8;

			List<CADPoint> resultList = new List<CADPoint>();
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );

			double circumference = 2.0 * Math.PI * radius;
			int segmentsByLength = Math.Max( DEFAULT_MIN_CIRCLE_SEGMENTS, (int)Math.Ceiling( circumference / DEFAULT_PRECISION_MAX_LENGTH ) );

			int segmentsByDeflection = int.MaxValue;
			if( radius > DISCRETE_MAX_DEFLECTION ) {
				double cosValue = 1.0 - ( DISCRETE_MAX_DEFLECTION / radius );
				if( cosValue >= -1.0 && cosValue <= 1.0 ) {
					double angleStep_rad = 2.0 * Math.Acos( cosValue );
					segmentsByDeflection = Math.Max( DEFAULT_MIN_CIRCLE_SEGMENTS, (int)Math.Ceiling( 2.0 * Math.PI / angleStep_rad ) );
				}
			}

			int segments = Math.Max( segmentsByLength, segmentsByDeflection );
			double startAngle_rad = 0.0;

			for( int i = 0; i < segments; i++ ) {
				double t = (double)i / segments;
				double angle_rad = startAngle_rad - t * 2.0 * Math.PI;
				gp_Pnt circlePoint = new gp_Pnt( radius * Math.Cos( angle_rad ), radius * Math.Sin( angle_rad ), 0 );

				gp_Dir tangent = DiscreteUtility.GetClockwiseTangent( angle_rad );
				gp_Dir normal = DiscreteUtility.GetRadialNormal( new gp_Pnt( 0, 0, 0 ), circlePoint, radius );

				resultList.Add( CreateCADPoint( circlePoint, toolVec, normal, tangent, transformation ) );
			}

			return resultList;
		}

		#endregion

		#region Rectangle Path Generation

		static List<CADPoint> GenerateSharpRectanglePath( double width, double length, gp_Trsf transformation )
		{
			List<CADPoint> resultList = new List<CADPoint>();
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );
			double halfWidth = width / 2.0;
			double halfLength = length / 2.0;

			List<gp_Pnt> vertices = new List<gp_Pnt>
			{
				new gp_Pnt( halfLength, halfWidth, 0 ),
				new gp_Pnt( halfLength, -halfWidth, 0 ),
				new gp_Pnt( -halfLength, -halfWidth, 0 ),
				new gp_Pnt( -halfLength, halfWidth, 0 )
			};
			vertices.Add( vertices[ 0 ] );

			for( int i = 0; i < vertices.Count - 1; i++ ) {
				AddStraightEdgePoints( resultList, vertices[ i ], vertices[ i + 1 ], toolVec, transformation );
			}

			return resultList;
		}

		static List<CADPoint> GenerateRoundedRectanglePath( double width, double length, double cornerRadius, gp_Trsf transformation )
		{
			List<CADPoint> resultList = new List<CADPoint>();
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );
			double halfWidth = width / 2.0;
			double halfLength = length / 2.0;

			if( cornerRadius * 2 > width || cornerRadius * 2 > length ) {
				return GenerateSharpRectanglePath( width, length, transformation );
			}

			gp_Pnt topEdgeStart = new gp_Pnt( halfLength - cornerRadius, halfWidth, 0 );
			gp_Pnt topEdgeEnd = new gp_Pnt( -halfLength + cornerRadius, halfWidth, 0 );
			gp_Pnt rightEdgeStart = new gp_Pnt( halfLength, halfWidth - cornerRadius, 0 );
			gp_Pnt rightEdgeEnd = new gp_Pnt( halfLength, -halfWidth + cornerRadius, 0 );
			gp_Pnt bottomEdgeStart = new gp_Pnt( -halfLength + cornerRadius, -halfWidth, 0 );
			gp_Pnt bottomEdgeEnd = new gp_Pnt( halfLength - cornerRadius, -halfWidth, 0 );
			gp_Pnt leftEdgeStart = new gp_Pnt( -halfLength, -halfWidth + cornerRadius, 0 );
			gp_Pnt leftEdgeEnd = new gp_Pnt( -halfLength, halfWidth - cornerRadius, 0 );

			gp_Pnt topRightCorner = new gp_Pnt( halfLength - cornerRadius, halfWidth - cornerRadius, 0 );
			gp_Pnt bottomRightCorner = new gp_Pnt( halfLength - cornerRadius, -halfWidth + cornerRadius, 0 );
			gp_Pnt bottomLeftCorner = new gp_Pnt( -halfLength + cornerRadius, -halfWidth + cornerRadius, 0 );
			gp_Pnt topLeftCorner = new gp_Pnt( -halfLength + cornerRadius, halfWidth - cornerRadius, 0 );

			AddStraightEdgePoints( resultList, rightEdgeStart, rightEdgeEnd, toolVec, transformation );
			AddRectangleCornerArc( resultList, rightEdgeEnd, bottomEdgeEnd, bottomRightCorner, cornerRadius, toolVec, transformation );
			AddStraightEdgePoints( resultList, bottomEdgeEnd, bottomEdgeStart, toolVec, transformation );
			AddRectangleCornerArc( resultList, bottomEdgeStart, leftEdgeStart, bottomLeftCorner, cornerRadius, toolVec, transformation );
			AddStraightEdgePoints( resultList, leftEdgeStart, leftEdgeEnd, toolVec, transformation );
			AddRectangleCornerArc( resultList, leftEdgeEnd, topEdgeEnd, topLeftCorner, cornerRadius, toolVec, transformation );
			AddStraightEdgePoints( resultList, topEdgeEnd, topEdgeStart, toolVec, transformation );
			AddRectangleCornerArc( resultList, topEdgeStart, rightEdgeStart, topRightCorner, cornerRadius, toolVec, transformation );

			return resultList;
		}

		static void AddRectangleCornerArc( List<CADPoint> resultList, gp_Pnt beginPoint, gp_Pnt endPoint, gp_Pnt cornerCenter, double radius, gp_Dir toolVec, gp_Trsf transformation )
		{
			double startAngle_rad = Math.Atan2( beginPoint.Y() - cornerCenter.Y(), beginPoint.X() - cornerCenter.X() );
			double endAngle_rad = Math.Atan2( endPoint.Y() - cornerCenter.Y(), endPoint.X() - cornerCenter.X() );
			double angleDiff = DiscreteUtility.GetClockwiseAngleDiff( startAngle_rad, endAngle_rad );

			double arcLength = Math.Abs( angleDiff ) * radius;
			int segmentsByLength = Math.Max( 1, (int)Math.Ceiling( arcLength / DEFAULT_PRECISION_MAX_LENGTH ) );

			int segmentsByDeflection = int.MaxValue;
			if( radius > DISCRETE_MAX_DEFLECTION ) {
				double cosValue = 1.0 - ( DISCRETE_MAX_DEFLECTION / radius );
				if( cosValue >= -1.0 && cosValue <= 1.0 ) {
					double angleStepForDeflection = 2.0 * Math.Acos( cosValue );
					segmentsByDeflection = Math.Max( 1, (int)Math.Ceiling( Math.Abs( angleDiff ) / angleStepForDeflection ) );
				}
			}

			int segments = Math.Max( segmentsByLength, segmentsByDeflection );

			for( int j = 0; j < segments; j++ ) {
				double t = (double)j / segments;
				double angle_rad = startAngle_rad + t * angleDiff;

				gp_Pnt arcPoint = new gp_Pnt(
					cornerCenter.X() + radius * Math.Cos( angle_rad ),
					cornerCenter.Y() + radius * Math.Sin( angle_rad ),
					0
				);

				gp_Dir arcTangent = DiscreteUtility.GetClockwiseTangent( angle_rad );
				gp_Dir arcNormal = DiscreteUtility.GetRadialNormal( cornerCenter, arcPoint, radius );

				resultList.Add( CreateCADPoint( arcPoint, toolVec, arcNormal, arcTangent, transformation ) );
			}
		}

		#endregion

		#region Runway Path Generation

		static List<CADPoint> GenerateRunwayPath( double length, double width, gp_Trsf transformation )
		{
			List<CADPoint> resultList = new List<CADPoint>();
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );
			double radius = width / 2.0;
			double straightLength = length - width;
			double halfStraight = straightLength / 2.0;

			gp_Pnt rightSemicircleCenter = new gp_Pnt( halfStraight, 0, 0 );
			gp_Pnt leftSemicircleCenter = new gp_Pnt( -halfStraight, 0, 0 );

			AddSemicircle( resultList, rightSemicircleCenter, radius, Math.PI / 2.0, -Math.PI / 2.0, toolVec, transformation );

			gp_Pnt bottomStart = new gp_Pnt( halfStraight, -radius, 0 );
			gp_Pnt bottomEnd = new gp_Pnt( -halfStraight, -radius, 0 );
			AddStraightEdgePoints( resultList, bottomStart, bottomEnd, toolVec, transformation );

			AddSemicircle( resultList, leftSemicircleCenter, radius, -Math.PI / 2.0, Math.PI / 2.0, toolVec, transformation );

			gp_Pnt topStart = new gp_Pnt( -halfStraight, radius, 0 );
			gp_Pnt topEnd = new gp_Pnt( halfStraight, radius, 0 );
			AddStraightEdgePoints( resultList, topStart, topEnd, toolVec, transformation );

			return resultList;
		}

		static void AddSemicircle( List<CADPoint> resultList, gp_Pnt center, double radius, double startAngle_rad, double endAngle_rad, gp_Dir toolVec, gp_Trsf transformation )
		{
			double angleDiff = DiscreteUtility.GetClockwiseAngleDiff( startAngle_rad, endAngle_rad );

			double arcLength = Math.Abs( angleDiff ) * radius;
			int segmentsByLength = Math.Max( 1, (int)Math.Ceiling( arcLength / DEFAULT_PRECISION_MAX_LENGTH ) );

			int segmentsByDeflection = int.MaxValue;
			if( radius > DISCRETE_MAX_DEFLECTION ) {
				double cosValue = 1.0 - ( DISCRETE_MAX_DEFLECTION / radius );
				if( cosValue >= -1.0 && cosValue <= 1.0 ) {
					double angleStepForDeflection = 2.0 * Math.Acos( cosValue );
					segmentsByDeflection = Math.Max( 1, (int)Math.Ceiling( Math.Abs( angleDiff ) / angleStepForDeflection ) );
				}
			}

			int segments = Math.Max( segmentsByLength, segmentsByDeflection );

			for( int j = 0; j < segments; j++ ) {
				double t = (double)j / segments;
				double angle_rad = startAngle_rad + t * angleDiff;

				gp_Pnt arcPoint = new gp_Pnt(
					center.X() + radius * Math.Cos( angle_rad ),
					center.Y() + radius * Math.Sin( angle_rad ),
					0
				);

				gp_Dir arcTangent = DiscreteUtility.GetClockwiseTangent( angle_rad );
				gp_Dir arcNormal = DiscreteUtility.GetRadialNormal( center, arcPoint, radius );

				resultList.Add( CreateCADPoint( arcPoint, toolVec, arcNormal, arcTangent, transformation ) );
			}
		}

		#endregion

		#region Polygon Path Generation

		static List<CADPoint> GenerateSharpPolygonPath( int sides, double circumradius, double angleOffset, gp_Trsf transformation )
		{
			List<CADPoint> resultList = new List<CADPoint>();
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );
			double angleStep_rad = 2.0 * Math.PI / sides;

			List<gp_Pnt> vertices = new List<gp_Pnt>();

			for( int i = 0; i < sides; i++ ) {
				double angle_rad = angleOffset - i * angleStep_rad;
				gp_Pnt vertex = new gp_Pnt(
					circumradius * Math.Cos( angle_rad ),
					circumradius * Math.Sin( angle_rad ),
					0
				);
				vertices.Add( vertex );
			}

			vertices.Add( vertices[ 0 ] );

			for( int i = 0; i < vertices.Count - 1; i++ ) {
				AddStraightEdgePoints( resultList, vertices[ i ], vertices[ i + 1 ], toolVec, transformation );
			}

			return resultList;
		}

		static List<CADPoint> GenerateRoundedPolygonPath( int sides, double sideLength, double cornerRadius, double angleOffset, gp_Trsf transformation, double circumradius )
		{
			List<CADPoint> resultList = new List<CADPoint>();
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );
			double angleStep_rad = 2.0 * Math.PI / sides;

			double interiorAngle_rad = ( sides - 2 ) * Math.PI / sides;
			double halfAngle_rad = interiorAngle_rad / 2.0;
			double tanHalfAngle_rad = Math.Tan( halfAngle_rad );
			double sinHalfAngle_rad = Math.Sin( halfAngle_rad );

			if( tanHalfAngle_rad < DEFAULT_PRECISION_MIN_ERROR || sinHalfAngle_rad < DEFAULT_PRECISION_MIN_ERROR ) {
				return GenerateSharpPolygonPath( sides, circumradius, angleOffset, transformation );
			}

			double tangentDistance = cornerRadius / tanHalfAngle_rad;
			double centerDistance = cornerRadius / sinHalfAngle_rad;

			List<gp_Pnt> vertices = new List<gp_Pnt>();
			for( int i = 0; i < sides; i++ ) {
				double angle_rad = angleOffset - i * angleStep_rad;
				vertices.Add( new gp_Pnt(
					circumradius * Math.Cos( angle_rad ),
					circumradius * Math.Sin( angle_rad ),
					0
				) );
			}

			double edgeLength = vertices[ 0 ].Distance( vertices[ 1 ] );
			if( tangentDistance * 2 > edgeLength ) {
				return GenerateSharpPolygonPath( sides, circumradius, angleOffset, transformation );
			}

			List<gp_Pnt> edgeStarts = new List<gp_Pnt>();
			List<gp_Pnt> edgeEnds = new List<gp_Pnt>();

			for( int i = 0; i < sides; i++ ) {
				gp_Pnt currentVertex = vertices[ i ];
				gp_Pnt nextVertex = vertices[ ( i + 1 ) % sides ];

				gp_Vec edgeVec = new gp_Vec( currentVertex, nextVertex );
				if( edgeVec.Magnitude() <= DEFAULT_PRECISION_MIN_ERROR ) {
					return GenerateSharpPolygonPath( sides, circumradius, angleOffset, transformation );
				}

				edgeVec.Normalize();

				gp_Pnt edgeStart = new gp_Pnt(
					currentVertex.X() + edgeVec.X() * tangentDistance,
					currentVertex.Y() + edgeVec.Y() * tangentDistance,
					currentVertex.Z()
				);
				edgeStarts.Add( edgeStart );

				gp_Pnt edgeEnd = new gp_Pnt(
					nextVertex.X() - edgeVec.X() * tangentDistance,
					nextVertex.Y() - edgeVec.Y() * tangentDistance,
					nextVertex.Z()
				);
				edgeEnds.Add( edgeEnd );

				if( edgeStart.Distance( edgeEnd ) <= DEFAULT_PRECISION_MIN_ERROR ) {
					return GenerateSharpPolygonPath( sides, circumradius, angleOffset, transformation );
				}
			}

			for( int i = 0; i < sides; i++ ) {
				AddStraightEdgePoints( resultList, edgeStarts[ i ], edgeEnds[ i ], toolVec, transformation );

				int nextVertexIndex = ( i + 1 ) % sides;
				AddCornerArc( resultList, edgeEnds[ i ], edgeStarts[ nextVertexIndex ], vertices[ nextVertexIndex ],
					cornerRadius, centerDistance, toolVec, transformation );
			}

			return resultList;
		}

		static void AddCornerArc( List<CADPoint> resultList, gp_Pnt beginPoint, gp_Pnt endPoint, gp_Pnt vertex, double radius, double centerDistance, gp_Dir toolVec, gp_Trsf transformation )
		{
			gp_Vec vec1 = new gp_Vec( vertex, beginPoint );
			gp_Vec vec2 = new gp_Vec( vertex, endPoint );

			if( vec1.Magnitude() < DEFAULT_PRECISION_MIN_ERROR || vec2.Magnitude() < DEFAULT_PRECISION_MIN_ERROR ) {
				return;
			}

			vec1.Normalize();
			vec2.Normalize();

			gp_Vec bisector = vec1.Added( vec2 );
			if( bisector.Magnitude() < DEFAULT_PRECISION_MIN_ERROR ) {
				return;
			}
			bisector.Normalize();

			gp_Pnt arcCenter = new gp_Pnt(
				vertex.X() + bisector.X() * centerDistance,
				vertex.Y() + bisector.Y() * centerDistance,
				vertex.Z()
			);

			double dist1 = arcCenter.Distance( beginPoint );
			double dist2 = arcCenter.Distance( endPoint );
			if( Math.Abs( dist1 - radius ) > DEFAULT_PRECISION_MIN_ERROR || Math.Abs( dist2 - radius ) > DEFAULT_PRECISION_MIN_ERROR ) {
				return;
			}

			double startAngle_rad = Math.Atan2( beginPoint.Y() - arcCenter.Y(), beginPoint.X() - arcCenter.X() );
			double endAngle_rad = Math.Atan2( endPoint.Y() - arcCenter.Y(), endPoint.X() - arcCenter.X() );
			double angleDiff = DiscreteUtility.GetClockwiseAngleDiff( startAngle_rad, endAngle_rad );

			double arcLength = Math.Abs( angleDiff ) * radius;
			int segmentsByLength = Math.Max( 1, (int)Math.Ceiling( arcLength / DEFAULT_PRECISION_MAX_LENGTH ) );

			int segmentsByDeflection = int.MaxValue;
			if( radius > DISCRETE_MAX_DEFLECTION ) {
				double cosValue = 1.0 - ( DISCRETE_MAX_DEFLECTION / radius );
				if( cosValue >= -1.0 && cosValue <= 1.0 ) {
					double angleStepForDeflection = 2.0 * Math.Acos( cosValue );
					segmentsByDeflection = Math.Max( 1, (int)Math.Ceiling( Math.Abs( angleDiff ) / angleStepForDeflection ) );
				}
			}

			int segments = Math.Max( segmentsByLength, segmentsByDeflection );

			for( int j = 0; j < segments; j++ ) {
				double t = (double)j / segments;
				double angle_rad = startAngle_rad + t * angleDiff;

				gp_Pnt arcPoint = new gp_Pnt(
					arcCenter.X() + radius * Math.Cos( angle_rad ),
					arcCenter.Y() + radius * Math.Sin( angle_rad ),
					0
				);

				gp_Dir arcTangent = DiscreteUtility.GetClockwiseTangent( angle_rad );
				gp_Dir arcNormal = DiscreteUtility.GetRadialNormal( arcCenter, arcPoint, radius );

				resultList.Add( CreateCADPoint( arcPoint, toolVec, arcNormal, arcTangent, transformation ) );
			}
		}

		#endregion

		#region Common Helper Methods

		static void AddStraightEdgePoints( List<CADPoint> resultList, gp_Pnt startPnt, gp_Pnt endPnt, gp_Dir toolVec, gp_Trsf transformation )
		{
			gp_Vec edgeVec = new gp_Vec( startPnt, endPnt );
			double edgeLength = edgeVec.Magnitude();

			if( edgeLength <= DEFAULT_PRECISION_MIN_ERROR ) {
				return;
			}

			edgeVec.Normalize();
			gp_Dir tangent = new gp_Dir( edgeVec );
			gp_Dir normal = DiscreteUtility.GetEdgeNormal( edgeVec );

			int segments = Math.Max( 1, (int)Math.Ceiling( edgeLength / DEFAULT_PRECISION_MAX_LENGTH ) );

			for( int j = 0; j < segments; j++ ) {
				double t = (double)j / segments;
				gp_Pnt edgePoint = new gp_Pnt(
					startPnt.X() + t * ( endPnt.X() - startPnt.X() ),
					startPnt.Y() + t * ( endPnt.Y() - startPnt.Y() ),
					0
				);

				resultList.Add( CreateCADPoint( edgePoint, toolVec, normal, tangent, transformation ) );
			}
		}

		static CADPoint CreateCADPoint( gp_Pnt localPoint, gp_Dir localToolVec, gp_Dir localNormal, gp_Dir localTangent, gp_Trsf transformation )
		{
			return new CADPoint(
				localPoint.Transformed( transformation ),
				localToolVec.Transformed( transformation ),
				localNormal.Transformed( transformation ),
				localTangent.Transformed( transformation )
			);
		}

		static double CalculateMaxCornerRadius( int sides, double sideLength )
		{
			double interiorAngle_rad = ( sides - 2 ) * Math.PI / sides;
			double halfAngle_rad = interiorAngle_rad / 2.0;
			double tanHalfAngle_rad = Math.Tan( halfAngle_rad );

			if( tanHalfAngle_rad < DEFAULT_PRECISION_MIN_ERROR ) {
				return 0;
			}

			double maxTangentDistance = sideLength / 2.0;
			double maxRadius = maxTangentDistance * tanHalfAngle_rad;

			return maxRadius * 0.9;
		}

		static double GetPolygonAngleOffset( int sides )
		{
			switch( sides ) {
				case 3:
					return Math.PI / 2.0;
				case 4:
					return Math.PI / 4.0;
				case 5:
					return Math.PI / 2.0;
				case 6:
					return 0.0;
				default:
					return 0.0;
			}
		}

		#endregion

		const double DEFAULT_PRECISION_MIN_ERROR = 0.001;
		const double DISCRETE_MAX_DEFLECTION = 0.01;
		const double DEFAULT_PRECISION_MAX_LENGTH = 1.0;
	}

	public static class DiscreteUtility
	{
		public const double VECTOR_OPPOSITE_TOLERANCE = 0.001;
		public const double PATH_MATH_TOLERANCE = 0.001;

		public static void InterpolateVecBetween2Point( IOrientationPoint currentCAMPoint, IOrientationPoint nextCAMPoint, gp_Pnt point,
			out gp_Dir toolDir, out gp_Dir tangentDir )
		{
			toolDir = currentCAMPoint.ToolVec;
			tangentDir = currentCAMPoint.TangentVec;

			// get current and next tool vector
			gp_Vec currentVec = new gp_Vec( currentCAMPoint.ToolVec );
			gp_Vec nextVec = new gp_Vec( nextCAMPoint.ToolVec );

			// get current and next tangent vector
			gp_Vec currentTangentVec = new gp_Vec( currentCAMPoint.TangentVec );
			gp_Vec nextTangentVec = new gp_Vec( nextCAMPoint.TangentVec );

			// calculate new point percentage
			double dDistanceOfCAMPath2Point = currentCAMPoint.Point.Distance( nextCAMPoint.Point );
			double dDistanceBetweenCurrentPoint2NewPoint = currentCAMPoint.Point.Distance( point );

			// two point overlap
			if( dDistanceOfCAMPath2Point <= PATH_MATH_TOLERANCE ) {
				return;
			}
			double interpolatePercent = dDistanceBetweenCurrentPoint2NewPoint / dDistanceOfCAMPath2Point;

			// get new point dir
			toolDir = InterpolateVecBetween2Vec( currentVec, nextVec, interpolatePercent );
			tangentDir = InterpolateVecBetween2Vec( currentTangentVec, nextTangentVec, interpolatePercent );
		}

		public static gp_Dir InterpolateVecBetween2Vec( gp_Vec currentVec, gp_Vec nextVec, double interpolatePercent )
		{
			// this case is unsolvable, so just return current vec
			if( currentVec.IsOpposite( nextVec, VECTOR_OPPOSITE_TOLERANCE ) ) {
				return new gp_Dir( currentVec.XYZ() );
			}

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( currentVec, nextVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );

			// calculate new point attitude
			gp_Quaternion q = new gp_Quaternion();
			slerp.Interpolate( interpolatePercent, ref q );
			gp_Trsf trsf = new gp_Trsf();
			trsf.SetRotation( q );
			gp_Dir resultDir = new gp_Dir( currentVec.Transformed( trsf ) );
			return resultDir;
		}

		public static IOrientationPoint CreateOrientationPoint( gp_Pnt localPoint, gp_Dir localToolVec, gp_Dir localNormal, gp_Dir localTangent, gp_Trsf transformation )
		{
			CADPoint cadPoint = new CADPoint(
				localPoint.Transformed( transformation ),
				localToolVec.Transformed( transformation ),
				localNormal.Transformed( transformation ),
				localTangent.Transformed( transformation )
			);

			CAMPoint camPoint = new CAMPoint( cadPoint );
			return camPoint;
		}

		public static gp_Dir GetClockwiseTangent( double angle )
		{
			return new gp_Dir( -Math.Sin( angle ), Math.Cos( angle ), 0 );
		}

		public static gp_Dir GetRadialNormal( gp_Pnt center, gp_Pnt point, double radius )
		{
			return new gp_Dir(
				( center.X() - point.X() ) / radius,
				( center.Y() - point.Y() ) / radius,
				0
			);
		}

		public static gp_Dir GetEdgeNormal( gp_Vec edgeVec )
		{
			return new gp_Dir( edgeVec.Y(), -edgeVec.X(), 0 );
		}

		public static double GetClockwiseAngleDiff( double startAngle, double endAngle )
		{
			double angleDiff = endAngle - startAngle;
			if( angleDiff > Math.PI ) {
				angleDiff -= 2 * Math.PI;
			}
			else if( angleDiff < -Math.PI ) {
				angleDiff += 2 * Math.PI;
			}
			if( angleDiff > 0 ) {
				angleDiff -= 2 * Math.PI;
			}
			return angleDiff;
		}

		public static gp_Trsf CreateCoordTransformation( gp_Ax3 refCoord )
		{
			gp_Ax3 targetCoordSystem = new gp_Ax3(
				refCoord.Location(),
				refCoord.Direction(),
				refCoord.XDirection()
			);
			gp_Trsf transformation = new gp_Trsf();
			transformation.SetTransformation( targetCoordSystem, new gp_Ax3() );
			return transformation;
		}
	}
}
