using MyCAM.App;
using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	// TODO: I need to review this class
	internal static class StdPatternDiscreteFactory
	{
		public static bool GetDiscreteOriPointList( gp_Ax3 refCoord, IStdPatternGeomData geomData, out List<IOrientationPoint> discreteList )
		{
			discreteList = null;
			if( refCoord == null || geomData == null ) {
				return false;
			}

			if( geomData is CircleGeomData circle ) {
				return DiscretizeCircle( refCoord, circle, out discreteList );
			}
			if( geomData is RectangleGeomData rectangle ) {
				return DiscretizeRectangle( refCoord, rectangle, out discreteList );
			}
			if( geomData is RunwayGeomData runway ) {
				return DiscretizeRunway( refCoord, runway, out discreteList );
			}
			if( geomData is PolygonGeomData polygon ) {
				return DiscretizePolygon( refCoord, polygon, out discreteList );
			}

			return false;
		}

		static bool DiscretizeCircle( gp_Ax3 refCoord, CircleGeomData circleData, out List<IOrientationPoint> circleOriPointList )
		{
			circleOriPointList = null;
			if( refCoord == null || circleData == null ) {
				return false;
			}
			double diameter = circleData.Diameter;

			// validate inputs
			if( diameter <= 0 ) {
				return false;
			}
			double radius = diameter / 2.0;

			// create coordinate system transformation
			gp_Trsf transformation = DiscreteUtility.CreateCoordTransformation( refCoord );
			try {
				GenerateCirclePath( radius, transformation, out circleOriPointList );
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( $"Error occurred while generating circle path: {ex.Message}", MyApp.NoticeType.Error );
				return false;
			}

			return true;
		}

		static bool DiscretizeRectangle( gp_Ax3 refCoord, RectangleGeomData rectangleData, out List<IOrientationPoint> rectangleOriPointList )
		{
			rectangleOriPointList = null;
			if( refCoord == null || rectangleData == null ) {
				return false;
			}

			double width = rectangleData.Width;
			double length = rectangleData.Length;
			double cornerRadius = rectangleData.CornerRadius;

			// validate inputs
			if( width <= 0 || length <= 0 ) {
				return false;
			}
			if( cornerRadius < 0 ) {
				return false;
			}

			// limit corner radius to avoid overlap
			// use 90% of the max possible value as safety margin to avoid precision issues
			double maxCornerRadius = Math.Min( width, length ) / 2.0 * 0.9;
			if( cornerRadius > maxCornerRadius ) {
				cornerRadius = maxCornerRadius;
			}

			// create coordinate system transformation
			gp_Trsf transformation = DiscreteUtility.CreateCoordTransformation( refCoord );
			try {
				if( cornerRadius <= DEFAULT_PRECISION_MIN_ERROR ) {

					// sharp corners
					GenerateSharpRectanglePath( width, length, transformation, out rectangleOriPointList );
				}
				else {

					// rounded corners
					GenerateRoundedRectanglePath( width, length, cornerRadius, transformation, out rectangleOriPointList );
				}
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( $"Error occurred while generating rectangle path: {ex.Message}", MyApp.NoticeType.Error );
				return false;
			}
			return true;
		}

		static bool DiscretizeRunway( gp_Ax3 refCoord, RunwayGeomData runwayData, out List<IOrientationPoint> runwayOriPointList )
		{
			runwayOriPointList = null;
			if( refCoord == null || runwayData == null ) {
				return false;
			}

			double length = runwayData.Length;
			double width = runwayData.Width;

			// validate inputs
			if( length <= 0 || width <= 0 ) {
				return false;
			}
			if( length < width ) {
				return false;
			}

			// create coordinate system transformation
			gp_Trsf transformation = DiscreteUtility.CreateCoordTransformation( refCoord );
			try {
				GenerateRunwayPath( length, width, transformation, out runwayOriPointList );
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( $"Error occurred while generating runway path: {ex.Message}", MyApp.NoticeType.Error );
				return false;
			}
			return true;
		}

		static bool DiscretizePolygon( gp_Ax3 refCoord, PolygonGeomData polygonData, out List<IOrientationPoint> polygonOriPointList )
		{
			polygonOriPointList = null;
			if( refCoord == null || polygonData == null ) {
				return false;
			}

			int sides = polygonData.Sides;
			double sideLength = polygonData.SideLength;
			double cornerRadius = polygonData.CornerRadius;

			// validate inputs
			if( sides < 3 ) {
				return false;
			}
			if( sideLength <= 0 ) {
				return false;
			}
			if( cornerRadius < 0 ) {
				return false;
			}

			// calculate circumradius (distance from center to vertex)
			double circumradius = sideLength / ( 2.0 * Math.Sin( Math.PI / sides ) );

			// limit corner radius to avoid overlap
			double maxCornerRadius = CalculateMaxCornerRadius( sides, sideLength );
			if( cornerRadius > maxCornerRadius ) {
				cornerRadius = maxCornerRadius;
			}

			// create coordinate system transformation
			gp_Trsf transformation = DiscreteUtility.CreateCoordTransformation( refCoord );

			// calculate angle offset based on polygon type
			double angleOffset = GetPolygonAngleOffset( sides );

			try {
				if( cornerRadius <= DEFAULT_PRECISION_MIN_ERROR ) {

					// sharp corners
					GenerateSharpPolygonPath( sides, circumradius, angleOffset, transformation, out polygonOriPointList );
				}
				else {

					// rounded corners
					GenerateRoundedPolygonPath( sides, sideLength, cornerRadius, angleOffset, transformation, circumradius, out polygonOriPointList );
				}
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( $"Error occurred while generating polygon path: {ex.Message}", MyApp.NoticeType.Error );
				return false;
			}
			return true;
		}

		static void GenerateCirclePath( double radius, gp_Trsf transformation, out List<IOrientationPoint> resultList )
		{
			// Minimum segments to use for full-circle discretization
			const int DEFAULT_MIN_CIRCLE_SEGMENTS = 8;

			resultList = new List<IOrientationPoint>();
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );

			// calculate number of segments based on max length
			double circumference = 2.0 * Math.PI * radius;
			int segmentsByLength = Math.Max( DEFAULT_MIN_CIRCLE_SEGMENTS, (int)Math.Ceiling( circumference / DEFAULT_PRECISION_MAX_LENGTH ) );

			// calculate number of segments based on chord height (deflection)
			// deflection = radius * (1 - cos(angleStep/2))
			// solving for angleStep: angleStep = 2 * arccos(1 - deflection/radius)
			int segmentsByDeflection = int.MaxValue;
			if( radius > DISCRETE_MAX_DEFLECTION ) {
				double cosValue = 1.0 - ( DISCRETE_MAX_DEFLECTION / radius );
				if( cosValue >= -1.0 && cosValue <= 1.0 ) {
					double angleStep = 2.0 * Math.Acos( cosValue );
					segmentsByDeflection = Math.Max( DEFAULT_MIN_CIRCLE_SEGMENTS, (int)Math.Ceiling( 2.0 * Math.PI / angleStep ) );
				}
			}

			// use the more restrictive (larger) number of segments
			int segments = Math.Max( segmentsByLength, segmentsByDeflection );

			// start angle: 0 (can be adjusted if CircleGeomData has rotation)
			double startAngle = 0.0;

			// generate points around the circle in clockwise direction
			for( int i = 0; i < segments; i++ ) {
				double t = (double)i / segments;

				// clockwise: use negative angle progression
				double angle = startAngle - t * 2.0 * Math.PI;
				gp_Pnt circlePoint = new gp_Pnt( radius * Math.Cos( angle ), radius * Math.Sin( angle ), 0 );

				// tangent and normal directions
				gp_Dir tangent = DiscreteUtility.GetClockwiseTangent( angle );
				gp_Dir normal = DiscreteUtility.GetRadialNormal( new gp_Pnt( 0, 0, 0 ), circlePoint, radius );

				// create orientation point
				resultList.Add( DiscreteUtility.CreateOrientationPoint( circlePoint, toolVec, normal, tangent, transformation ) );
			}
		}

		static void GenerateSharpRectanglePath( double width, double length, gp_Trsf transformation, out List<IOrientationPoint> resultList )
		{
			resultList = new List<IOrientationPoint>();
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );
			double halfWidth = width / 2.0;
			double halfLength = length / 2.0;

			// define rectangle vertices in clockwise order starting from top-right
			// top-right, bottom-right, bottom-left, top-left
			List<gp_Pnt> vertices = new List<gp_Pnt>
			{
				new gp_Pnt( halfLength, halfWidth, 0 ),    // top-right
				new gp_Pnt( halfLength, -halfWidth, 0 ),   // bottom-right
				new gp_Pnt( -halfLength, -halfWidth, 0 ),  // bottom-left
				new gp_Pnt( -halfLength, halfWidth, 0 )    // top-left
			};

			// close the rectangle
			vertices.Add( vertices[ 0 ] );

			// generate points along each edge
			for( int i = 0; i < vertices.Count - 1; i++ ) {
				AddStraightEdgePoints( resultList, vertices[ i ], vertices[ i + 1 ], toolVec, transformation );
			}
		}

		static void GenerateRoundedRectanglePath( double width, double length, double cornerRadius, gp_Trsf transformation, out List<IOrientationPoint> resultList )
		{
			resultList = new List<IOrientationPoint>();
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );
			double halfWidth = width / 2.0;
			double halfLength = length / 2.0;

			// check if corner radius is valid
			if( cornerRadius * 2 > width || cornerRadius * 2 > length ) {
				// fallback to sharp corners
				GenerateSharpRectanglePath( width, length, transformation, out resultList );
				return;
			}

			// calculate edge start/end points after corner filleting
			// for each corner, we need two points: where the straight edges end/start

			// top edge: from (halfLength - cornerRadius, halfWidth) to (-halfLength + cornerRadius, halfWidth)
			gp_Pnt topEdgeStart = new gp_Pnt( halfLength - cornerRadius, halfWidth, 0 );
			gp_Pnt topEdgeEnd = new gp_Pnt( -halfLength + cornerRadius, halfWidth, 0 );

			// right edge: from (halfLength, halfWidth - cornerRadius) to (halfLength, -halfWidth + cornerRadius)
			gp_Pnt rightEdgeStart = new gp_Pnt( halfLength, halfWidth - cornerRadius, 0 );
			gp_Pnt rightEdgeEnd = new gp_Pnt( halfLength, -halfWidth + cornerRadius, 0 );

			// bottom edge: from (-halfLength + cornerRadius, -halfWidth) to (halfLength - cornerRadius, -halfWidth)
			gp_Pnt bottomEdgeStart = new gp_Pnt( -halfLength + cornerRadius, -halfWidth, 0 );
			gp_Pnt bottomEdgeEnd = new gp_Pnt( halfLength - cornerRadius, -halfWidth, 0 );

			// left edge: from (-halfLength, -halfWidth + cornerRadius) to (-halfLength, halfWidth - cornerRadius)
			gp_Pnt leftEdgeStart = new gp_Pnt( -halfLength, -halfWidth + cornerRadius, 0 );
			gp_Pnt leftEdgeEnd = new gp_Pnt( -halfLength, halfWidth - cornerRadius, 0 );

			// corner centers
			gp_Pnt topRightCorner = new gp_Pnt( halfLength - cornerRadius, halfWidth - cornerRadius, 0 );
			gp_Pnt bottomRightCorner = new gp_Pnt( halfLength - cornerRadius, -halfWidth + cornerRadius, 0 );
			gp_Pnt bottomLeftCorner = new gp_Pnt( -halfLength + cornerRadius, -halfWidth + cornerRadius, 0 );
			gp_Pnt topLeftCorner = new gp_Pnt( -halfLength + cornerRadius, halfWidth - cornerRadius, 0 );

			// generate path: straight edge + corner arc, repeated for all 4 sides
			// clockwise order: right edge, bottom-right corner, bottom edge, bottom-left corner, left edge, top-left corner, top edge, top-right corner

			// 1. Right edge (top to bottom)
			AddStraightEdgePoints( resultList, rightEdgeStart, rightEdgeEnd, toolVec, transformation );

			// 2. Bottom-right corner (90 degrees, clockwise)
			AddRectangleCornerArc( resultList, rightEdgeEnd, bottomEdgeEnd, bottomRightCorner, cornerRadius, toolVec, transformation );

			// 3. Bottom edge (right to left)
			AddStraightEdgePoints( resultList, bottomEdgeEnd, bottomEdgeStart, toolVec, transformation );

			// 4. Bottom-left corner (90 degrees, clockwise)
			AddRectangleCornerArc( resultList, bottomEdgeStart, leftEdgeStart, bottomLeftCorner, cornerRadius, toolVec, transformation );

			// 5. Left edge (bottom to top)
			AddStraightEdgePoints( resultList, leftEdgeStart, leftEdgeEnd, toolVec, transformation );

			// 6. Top-left corner (90 degrees, clockwise)
			AddRectangleCornerArc( resultList, leftEdgeEnd, topEdgeEnd, topLeftCorner, cornerRadius, toolVec, transformation );

			// 7. Top edge (left to right)
			AddStraightEdgePoints( resultList, topEdgeEnd, topEdgeStart, toolVec, transformation );

			// 8. Top-right corner (90 degrees, clockwise)
			AddRectangleCornerArc( resultList, topEdgeStart, rightEdgeStart, topRightCorner, cornerRadius, toolVec, transformation );
		}

		static void AddRectangleCornerArc( List<IOrientationPoint> resultList, gp_Pnt tangentPoint1, gp_Pnt tangentPoint2, gp_Pnt cornerCenter, double radius,
			gp_Dir toolVec, gp_Trsf transformation )
		{
			// calculate start and end angles
			double startAngle = Math.Atan2( tangentPoint1.Y() - cornerCenter.Y(), tangentPoint1.X() - cornerCenter.X() );
			double endAngle = Math.Atan2( tangentPoint2.Y() - cornerCenter.Y(), tangentPoint2.X() - cornerCenter.X() );

			// ensure clockwise direction (negative angle difference)
			double angleDiff = DiscreteUtility.GetClockwiseAngleDiff( startAngle, endAngle );

			// calculate number of segments based on arc length
			double arcLength = Math.Abs( angleDiff ) * radius;
			int segmentsByLength = Math.Max( 1, (int)Math.Ceiling( arcLength / DEFAULT_PRECISION_MAX_LENGTH ) );

			// calculate number of segments based on chord height (deflection)
			int segmentsByDeflection = int.MaxValue;
			if( radius > DISCRETE_MAX_DEFLECTION ) {
				double cosValue = 1.0 - ( DISCRETE_MAX_DEFLECTION / radius );
				if( cosValue >= -1.0 && cosValue <= 1.0 ) {
					double angleStepForDeflection = 2.0 * Math.Acos( cosValue );
					segmentsByDeflection = Math.Max( 1, (int)Math.Ceiling( Math.Abs( angleDiff ) / angleStepForDeflection ) );
				}
			}

			// use the more restrictive (larger) number of segments
			int segments = Math.Max( segmentsByLength, segmentsByDeflection );

			for( int j = 0; j < segments; j++ ) {
				double t = (double)j / segments;
				double angle = startAngle + t * angleDiff;

				gp_Pnt arcPoint = new gp_Pnt(
					cornerCenter.X() + radius * Math.Cos( angle ),
					cornerCenter.Y() + radius * Math.Sin( angle ),
					0
				);

				// tangent and normal directions
				gp_Dir arcTangent = DiscreteUtility.GetClockwiseTangent( angle );
				gp_Dir arcNormal = DiscreteUtility.GetRadialNormal( cornerCenter, arcPoint, radius );

				// create orientation point
				resultList.Add( DiscreteUtility.CreateOrientationPoint( arcPoint, toolVec, arcNormal, arcTangent, transformation ) );
			}
		}

		static void GenerateRoundedPolygonPath( int sides, double sideLength, double cornerRadius, double angleOffset, gp_Trsf transformation, double circumradius, out List<IOrientationPoint> resultList )
		{
			resultList = new List<IOrientationPoint>();
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );
			double angleStep = 2.0 * Math.PI / sides;

			// calculate interior angle and tangent distance
			double interiorAngle = ( sides - 2 ) * Math.PI / sides;
			double halfAngle = interiorAngle / 2.0;
			double tanHalfAngle = Math.Tan( halfAngle );
			double sinHalfAngle = Math.Sin( halfAngle );

			if( tanHalfAngle < DEFAULT_PRECISION_MIN_ERROR || sinHalfAngle < DEFAULT_PRECISION_MIN_ERROR ) {
				// fallback to sharp polygon
				GenerateSharpPolygonPath( sides, circumradius, angleOffset, transformation, out resultList );
				return;
			}

			// calculate tangent point distance and center distance
			double tangentDistance = cornerRadius / tanHalfAngle;
			double centerDistance = cornerRadius / sinHalfAngle;

			// generate vertices in clockwise order
			List<gp_Pnt> vertices = new List<gp_Pnt>();
			for( int i = 0; i < sides; i++ ) {
				double angle = angleOffset - i * angleStep;
				vertices.Add( new gp_Pnt(
					circumradius * Math.Cos( angle ),
					circumradius * Math.Sin( angle ),
					0
				) );
			}

			// check if edge length is sufficient for the corner radius
			double edgeLength = vertices[ 0 ].Distance( vertices[ 1 ] );
			if( tangentDistance * 2 > edgeLength ) {
				GenerateSharpPolygonPath( sides, circumradius, angleOffset, transformation, out resultList );
				return;
			}

			// calculate edge start/end points after corner filleting
			List<gp_Pnt> edgeStarts = new List<gp_Pnt>();
			List<gp_Pnt> edgeEnds = new List<gp_Pnt>();

			for( int i = 0; i < sides; i++ ) {
				gp_Pnt currentVertex = vertices[ i ];
				gp_Pnt nextVertex = vertices[ ( i + 1 ) % sides ];

				gp_Vec edgeVec = new gp_Vec( currentVertex, nextVertex );
				if( edgeVec.Magnitude() <= DEFAULT_PRECISION_MIN_ERROR ) {
					GenerateSharpPolygonPath( sides, circumradius, angleOffset, transformation, out resultList );
					return;
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
					GenerateSharpPolygonPath( sides, circumradius, angleOffset, transformation, out resultList );
					return;
				}
			}

			// generate path segments alternating between straight lines and arcs
			for( int i = 0; i < sides; i++ ) {
				// straight line segment
				AddStraightEdgePoints( resultList, edgeStarts[ i ], edgeEnds[ i ], toolVec, transformation );

				// corner arc
				int nextVertexIndex = ( i + 1 ) % sides;
				AddCornerArc( resultList, edgeEnds[ i ], edgeStarts[ nextVertexIndex ], vertices[ nextVertexIndex ],
					cornerRadius, centerDistance, toolVec, transformation );
			}
		}

		static void GenerateRunwayPath( double length, double width, gp_Trsf transformation, out List<IOrientationPoint> resultList )
		{
			resultList = new List<IOrientationPoint>();
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );
			double radius = width / 2.0;
			double straightLength = length - width; // total length minus the two semicircles
			double halfStraight = straightLength / 2.0;

			// Starting point: top of right semicircle (halfStraight, radius)
			gp_Pnt rightSemicircleCenter = new gp_Pnt( halfStraight, 0, 0 );
			gp_Pnt leftSemicircleCenter = new gp_Pnt( -halfStraight, 0, 0 );

			// 1. Right semicircle (from top to bottom, clockwise: 90¢X to -90¢X)
			AddSemicircle( resultList, rightSemicircleCenter, radius, Math.PI / 2.0, -Math.PI / 2.0,
				toolVec, transformation );

			// 2. Bottom edge (from right to left)
			gp_Pnt bottomStart = new gp_Pnt( halfStraight, -radius, 0 );
			gp_Pnt bottomEnd = new gp_Pnt( -halfStraight, -radius, 0 );
			AddStraightEdgePoints( resultList, bottomStart, bottomEnd, toolVec, transformation );

			// 3. Left semicircle (from bottom to top, clockwise: -90¢X to 90¢X)
			AddSemicircle( resultList, leftSemicircleCenter, radius, -Math.PI / 2.0, Math.PI / 2.0,
				toolVec, transformation );

			// 4. Top edge (from left to right)
			gp_Pnt topStart = new gp_Pnt( -halfStraight, radius, 0 );
			gp_Pnt topEnd = new gp_Pnt( halfStraight, radius, 0 );
			AddStraightEdgePoints( resultList, topStart, topEnd, toolVec, transformation );
		}

		static void AddSemicircle( List<IOrientationPoint> resultList, gp_Pnt center, double radius, double startAngle, double endAngle, gp_Dir toolVec, gp_Trsf transformation )
		{
			// ensure clockwise direction
			double angleDiff = DiscreteUtility.GetClockwiseAngleDiff( startAngle, endAngle );

			// calculate number of segments based on arc length
			double arcLength = Math.Abs( angleDiff ) * radius;
			int segmentsByLength = Math.Max( 1, (int)Math.Ceiling( arcLength / DEFAULT_PRECISION_MAX_LENGTH ) );

			// calculate number of segments based on chord height (deflection)
			int segmentsByDeflection = int.MaxValue;
			if( radius > DISCRETE_MAX_DEFLECTION ) {
				double cosValue = 1.0 - ( DISCRETE_MAX_DEFLECTION / radius );
				if( cosValue >= -1.0 && cosValue <= 1.0 ) {
					double angleStepForDeflection = 2.0 * Math.Acos( cosValue );
					segmentsByDeflection = Math.Max( 1, (int)Math.Ceiling( Math.Abs( angleDiff ) / angleStepForDeflection ) );
				}
			}

			// use the more restrictive (larger) number of segments
			int segments = Math.Max( segmentsByLength, segmentsByDeflection );

			for( int j = 0; j < segments; j++ ) {
				double t = (double)j / segments;
				double angle = startAngle + t * angleDiff;

				gp_Pnt arcPoint = new gp_Pnt(
					center.X() + radius * Math.Cos( angle ),
					center.Y() + radius * Math.Sin( angle ),
					0
				);

				// tangent and normal directions
				gp_Dir arcTangent = DiscreteUtility.GetClockwiseTangent( angle );
				gp_Dir arcNormal = DiscreteUtility.GetRadialNormal( center, arcPoint, radius );

				// create orientation point
				resultList.Add( DiscreteUtility.CreateOrientationPoint( arcPoint, toolVec, arcNormal, arcTangent, transformation ) );
			}
		}

		static void AddStraightEdgePoints( List<IOrientationPoint> resultList, gp_Pnt startPnt, gp_Pnt endPnt, gp_Dir toolVec, gp_Trsf transformation )
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

				// create orientation point
				resultList.Add( DiscreteUtility.CreateOrientationPoint( edgePoint, toolVec, normal, tangent, transformation ) );
			}
		}

		static void AddCornerArc( List<IOrientationPoint> resultList, gp_Pnt tangentPoint1, gp_Pnt tangentPoint2, gp_Pnt vertex, double radius, double centerDistance, gp_Dir toolVec, gp_Trsf transformation )
		{
			gp_Vec vec1 = new gp_Vec( vertex, tangentPoint1 );
			gp_Vec vec2 = new gp_Vec( vertex, tangentPoint2 );

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

			// verify arc center
			double dist1 = arcCenter.Distance( tangentPoint1 );
			double dist2 = arcCenter.Distance( tangentPoint2 );
			if( Math.Abs( dist1 - radius ) > DEFAULT_PRECISION_MIN_ERROR || Math.Abs( dist2 - radius ) > DEFAULT_PRECISION_MIN_ERROR ) {
				return;
			}

			double startAngle = Math.Atan2( tangentPoint1.Y() - arcCenter.Y(), tangentPoint1.X() - arcCenter.X() );
			double endAngle = Math.Atan2( tangentPoint2.Y() - arcCenter.Y(), tangentPoint2.X() - arcCenter.X() );

			// ensure clockwise direction (negative angle difference)
			double angleDiff = DiscreteUtility.GetClockwiseAngleDiff( startAngle, endAngle );

			// calculate number of segments based on arc length
			double arcLength = Math.Abs( angleDiff ) * radius;
			int segmentsByLength = Math.Max( 1, (int)Math.Ceiling( arcLength / DEFAULT_PRECISION_MAX_LENGTH ) );

			// calculate number of segments based on chord height (deflection)
			int segmentsByDeflection = int.MaxValue;
			if( radius > DISCRETE_MAX_DEFLECTION ) {
				double cosValue = 1.0 - ( DISCRETE_MAX_DEFLECTION / radius );
				if( cosValue >= -1.0 && cosValue <= 1.0 ) {
					double angleStepForDeflection = 2.0 * Math.Acos( cosValue );
					segmentsByDeflection = Math.Max( 1, (int)Math.Ceiling( Math.Abs( angleDiff ) / angleStepForDeflection ) );
				}
			}

			// use the more restrictive (larger) number of segments
			int segments = Math.Max( segmentsByLength, segmentsByDeflection );

			for( int j = 0; j < segments; j++ ) {
				double t = (double)j / segments;
				double angle = startAngle + t * angleDiff;

				gp_Pnt arcPoint = new gp_Pnt(
					arcCenter.X() + radius * Math.Cos( angle ),
					arcCenter.Y() + radius * Math.Sin( angle ),
					0
				);

				// tangent and normal directions
				gp_Dir arcTangent = DiscreteUtility.GetClockwiseTangent( angle );
				gp_Dir arcNormal = DiscreteUtility.GetRadialNormal( arcCenter, arcPoint, radius );

				// create orientation point
				resultList.Add( DiscreteUtility.CreateOrientationPoint( arcPoint, toolVec, arcNormal, arcTangent, transformation ) );
			}
		}

		static void GenerateSharpPolygonPath( int sides, double circumradius, double angleOffset, gp_Trsf transformation, out List<IOrientationPoint> resultList )
		{
			resultList = new List<IOrientationPoint>();
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );
			double angleStep = 2.0 * Math.PI / sides;

			// generate polygon vertices in clockwise order
			List<gp_Pnt> vertices = new List<gp_Pnt>();

			for( int i = 0; i < sides; i++ ) {
				// clockwise: reverse the angle direction
				double angle = angleOffset - i * angleStep;
				gp_Pnt vertex = new gp_Pnt(
					circumradius * Math.Cos( angle ),
					circumradius * Math.Sin( angle ),
					0
				);
				vertices.Add( vertex );
			}

			// close the polygon
			vertices.Add( vertices[ 0 ] );

			// generate points along each edge
			for( int i = 0; i < vertices.Count - 1; i++ ) {
				AddStraightEdgePoints( resultList, vertices[ i ], vertices[ i + 1 ], toolVec, transformation );
			}
		}

		#region Helper Methods

		static double CalculateMaxCornerRadius( int sides, double sideLength )
		{
			double interiorAngle = ( sides - 2 ) * Math.PI / sides;
			double halfAngle = interiorAngle / 2.0;
			double tanHalfAngle = Math.Tan( halfAngle );

			if( tanHalfAngle < DEFAULT_PRECISION_MIN_ERROR ) {
				return 0;
			}

			double maxTangentDistance = sideLength / 2.0;
			double maxRadius = maxTangentDistance * tanHalfAngle;

			return maxRadius * 0.9; // 90% safety margin
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

		// Default precision values
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
