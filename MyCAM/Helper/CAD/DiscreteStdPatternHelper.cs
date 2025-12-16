using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	internal static class DiscreteStdPatternHelper
	{
		public static List<IOrientationPoint> DiscretizeCircle( gp_Ax3 refCoord, CircleGeomData circleData )
		{
			if( refCoord == null || circleData == null ) {
				throw new ArgumentNullException( "DiscretizeCircle: refCoord or circleData is null" );
			}

			double diameter = circleData.Diameter;

			// validate inputs
			if( diameter <= 0 ) {
				throw new ArgumentException( "Circle diameter must be positive" );
			}

			double radius = diameter / 2.0;

			// create coordinate system transformation
			gp_Trsf transformation = GeomUitility.CreateCoordTransformation( refCoord );

			List<IOrientationPoint> resultList = new List<IOrientationPoint>();

			GenerateCirclePath( resultList, radius, transformation );

			return resultList;
		}

		public static List<IOrientationPoint> DiscretizeRectangle( gp_Ax3 refCoord, RectangleGeomData rectangleData )
		{
			if( refCoord == null || rectangleData == null ) {
				throw new ArgumentNullException( "DiscretizeRectangle: refCoord or rectangleData is null" );
			}

			double width = rectangleData.Width;
			double length = rectangleData.Length;
			double cornerRadius = rectangleData.CornerRadius;

			// validate inputs
			if( width <= 0 || length <= 0 ) {
				throw new ArgumentException( "Width and length must be positive" );
			}
			if( cornerRadius < 0 ) {
				throw new ArgumentException( "Corner radius cannot be negative" );
			}

			// limit corner radius to avoid overlap
			double maxCornerRadius = Math.Min( width, length ) / 2.0 * 0.9; // 90% safety margin
			if( cornerRadius > maxCornerRadius ) {
				cornerRadius = maxCornerRadius;
			}

			// create coordinate system transformation
			gp_Trsf transformation = GeomUitility.CreateCoordTransformation( refCoord );

			List<IOrientationPoint> resultList = new List<IOrientationPoint>();

			if( cornerRadius <= DEFAULT_PRECISION_MIN_ERROR ) {
				// sharp corners
				GenerateSharpRectanglePath( resultList, width, length, transformation );
			}
			else {
				// rounded corners
				GenerateRoundedRectanglePath( resultList, width, length, cornerRadius, transformation );
			}

			return resultList;
		}

		public static List<IOrientationPoint> DiscretizeRunway( gp_Ax3 refCoord, RunwayGeomData runwayData )
		{
			if( refCoord == null || runwayData == null ) {
				throw new ArgumentNullException( "DiscretizeRunway: refCoord or runwayData is null" );
			}

			double length = runwayData.Length;
			double width = runwayData.Width;

			// validate inputs
			if( length <= 0 || width <= 0 ) {
				throw new ArgumentException( "Length and width must be positive" );
			}
			if( length < width ) {
				throw new ArgumentException( "Runway length must be greater than or equal to width" );
			}

			// create coordinate system transformation
			gp_Trsf transformation = GeomUitility.CreateCoordTransformation( refCoord );

			List<IOrientationPoint> resultList = new List<IOrientationPoint>();

			GenerateRunwayPath( resultList, length, width, transformation );

			return resultList;
		}

		public static List<IOrientationPoint> DiscretizePolygon( gp_Ax3 refCoord, PolygonGeomData polygonData )
		{
			if( refCoord == null || polygonData == null ) {
				throw new ArgumentNullException( "DiscretizePolygon: refCoord or polygonData is null" );
			}

			int sides = polygonData.Sides;
			double sideLength = polygonData.SideLength;
			double cornerRadius = polygonData.CornerRadius;

			// validate inputs
			if( sides < 3 ) {
				throw new ArgumentException( "Polygon must have at least 3 sides" );
			}
			if( sideLength <= 0 ) {
				throw new ArgumentException( "Side length must be positive" );
			}
			if( cornerRadius < 0 ) {
				throw new ArgumentException( "Corner radius cannot be negative" );
			}

			// calculate circumradius (distance from center to vertex)
			double circumradius = sideLength / ( 2.0 * Math.Sin( Math.PI / sides ) );

			// limit corner radius to avoid overlap
			double maxCornerRadius = CalculateMaxCornerRadius( sides, sideLength );
			if( cornerRadius > maxCornerRadius ) {
				cornerRadius = maxCornerRadius;
			}

			// create coordinate system transformation
			gp_Trsf transformation = GeomUitility.CreateCoordTransformation( refCoord );

			// calculate angle offset based on polygon type
			double angleOffset = GetPolygonAngleOffset( sides );

			List<IOrientationPoint> resultList = new List<IOrientationPoint>();

			if( cornerRadius <= DEFAULT_PRECISION_MIN_ERROR ) {
				// sharp corners
				GenerateSharpPolygonPath( resultList, sides, circumradius, angleOffset, transformation );
			}
			else {
				// rounded corners
				GenerateRoundedPolygonPath( resultList, sides, sideLength, cornerRadius, angleOffset, transformation, circumradius );
			}

			return resultList;
		}

		static void GenerateSharpPolygonPath( List<IOrientationPoint> resultList, int sides, double circumradius, double angleOffset, gp_Trsf transformation )
		{
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

		static void GenerateSharpRectanglePath( List<IOrientationPoint> resultList, double width, double length, gp_Trsf transformation )
		{
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

		static void GenerateRoundedRectanglePath( List<IOrientationPoint> resultList, double width, double length, double cornerRadius, gp_Trsf transformation )
		{
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );

			double halfWidth = width / 2.0;
			double halfLength = length / 2.0;

			// check if corner radius is valid
			if( cornerRadius * 2 > width || cornerRadius * 2 > length ) {
				// fallback to sharp corners
				GenerateSharpRectanglePath( resultList, width, length, transformation );
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
			double angleDiff = GeomUitility.GetClockwiseAngleDiff( startAngle, endAngle );

			double arcLength = Math.Abs( angleDiff ) * radius;
			int segments = Math.Max( 1, (int)Math.Ceiling( arcLength / DEFAULT_PRECISION_MAX_LENGTH ) );

			for( int j = 0; j < segments; j++ ) {
				double t = (double)j / segments;
				double angle = startAngle + t * angleDiff;

				gp_Pnt arcPoint = new gp_Pnt(
					cornerCenter.X() + radius * Math.Cos( angle ),
					cornerCenter.Y() + radius * Math.Sin( angle ),
					0
				);

				// tangent and normal directions
				gp_Dir arcTangent = GeomUitility.GetClockwiseTangent( angle );
				gp_Dir arcNormal = GeomUitility.GetRadialNormal( cornerCenter, arcPoint, radius );

				// create orientation point
				resultList.Add( GeomUitility.CreateOrientationPoint( arcPoint, toolVec, arcNormal, arcTangent, transformation ) );
			}
		}

		static void GenerateRoundedPolygonPath( List<IOrientationPoint> resultList, int sides, double sideLength, double cornerRadius, double angleOffset,
			gp_Trsf transformation, double circumradius )
		{
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );
			double angleStep = 2.0 * Math.PI / sides;

			// calculate interior angle and tangent distance
			double interiorAngle = ( sides - 2 ) * Math.PI / sides;
			double halfAngle = interiorAngle / 2.0;
			double tanHalfAngle = Math.Tan( halfAngle );
			double sinHalfAngle = Math.Sin( halfAngle );

			if( tanHalfAngle < DEFAULT_PRECISION_MIN_ERROR || sinHalfAngle < DEFAULT_PRECISION_MIN_ERROR ) {
				// fallback to sharp polygon
				GenerateSharpPolygonPath( resultList, sides, circumradius, angleOffset, transformation );
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
				GenerateSharpPolygonPath( resultList, sides, circumradius, angleOffset, transformation );
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
					GenerateSharpPolygonPath( resultList, sides, circumradius, angleOffset, transformation );
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
					GenerateSharpPolygonPath( resultList, sides, circumradius, angleOffset, transformation );
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

		static void GenerateCirclePath( List<IOrientationPoint> resultList, double radius, gp_Trsf transformation )
		{
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );

			// calculate circumference and number of segments
			double circumference = 2.0 * Math.PI * radius;
			int segments = Math.Max( 8, (int)Math.Ceiling( circumference / DEFAULT_PRECISION_MAX_LENGTH ) );

			// start angle: 0 (can be adjusted if CircleGeomData has rotation)
			double startAngle = 0.0;

			// generate points around the circle in clockwise direction
			for( int i = 0; i < segments; i++ ) {
				double t = (double)i / segments;
				// clockwise: use negative angle progression
				double angle = startAngle - t * 2.0 * Math.PI;

				gp_Pnt circlePoint = new gp_Pnt(
					radius * Math.Cos( angle ),
					radius * Math.Sin( angle ),
					0
				);

				// tangent and normal directions
				gp_Dir tangent = GeomUitility.GetClockwiseTangent( angle );
				gp_Dir normal = GeomUitility.GetRadialNormal( new gp_Pnt( 0, 0, 0 ), circlePoint, radius );

				// create orientation point
				resultList.Add( GeomUitility.CreateOrientationPoint( circlePoint, toolVec, normal, tangent, transformation ) );
			}
		}

		static void GenerateRunwayPath( List<IOrientationPoint> resultList, double length, double width, gp_Trsf transformation )
		{
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );

			double radius = width / 2.0;
			double straightLength = length - width; // total length minus the two semicircles
			double halfStraight = straightLength / 2.0;

			// Runway geometry (in local coordinates, centered at origin):
			// - Right semicircle center at (halfStraight, 0)
			// - Left semicircle center at (-halfStraight, 0)
			// - Top edge from (-halfStraight, radius) to (halfStraight, radius)
			// - Bottom edge from (halfStraight, -radius) to (-halfStraight, -radius)

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
			double angleDiff = GeomUitility.GetClockwiseAngleDiff( startAngle, endAngle );

			double arcLength = Math.Abs( angleDiff ) * radius;
			int segments = Math.Max( 1, (int)Math.Ceiling( arcLength / DEFAULT_PRECISION_MAX_LENGTH ) );

			for( int j = 0; j < segments; j++ ) {
				double t = (double)j / segments;
				double angle = startAngle + t * angleDiff;

				gp_Pnt arcPoint = new gp_Pnt(
					center.X() + radius * Math.Cos( angle ),
					center.Y() + radius * Math.Sin( angle ),
					0
				);

				// tangent and normal directions
				gp_Dir arcTangent = GeomUitility.GetClockwiseTangent( angle );
				gp_Dir arcNormal = GeomUitility.GetRadialNormal( center, arcPoint, radius );

				// create orientation point
				resultList.Add( GeomUitility.CreateOrientationPoint( arcPoint, toolVec, arcNormal, arcTangent, transformation ) );
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
			gp_Dir normal = GeomUitility.GetEdgeNormal( edgeVec );

			int segments = Math.Max( 1, (int)Math.Ceiling( edgeLength / DEFAULT_PRECISION_MAX_LENGTH ) );

			for( int j = 0; j < segments; j++ ) {
				double t = (double)j / segments;
				gp_Pnt edgePoint = new gp_Pnt(
					startPnt.X() + t * ( endPnt.X() - startPnt.X() ),
					startPnt.Y() + t * ( endPnt.Y() - startPnt.Y() ),
					0
				);

				// create orientation point
				resultList.Add( GeomUitility.CreateOrientationPoint( edgePoint, toolVec, normal, tangent, transformation ) );
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
			if( Math.Abs( dist1 - radius ) > 1e-3 || Math.Abs( dist2 - radius ) > 1e-3 ) {
				return;
			}

			double startAngle = Math.Atan2( tangentPoint1.Y() - arcCenter.Y(), tangentPoint1.X() - arcCenter.X() );
			double endAngle = Math.Atan2( tangentPoint2.Y() - arcCenter.Y(), tangentPoint2.X() - arcCenter.X() );

			// ensure clockwise direction
			double angleDiff = GeomUitility.GetClockwiseAngleDiff( startAngle, endAngle );

			double arcLength = Math.Abs( angleDiff ) * radius;
			int segments = Math.Max( 1, (int)Math.Ceiling( arcLength / DEFAULT_PRECISION_MAX_LENGTH ) );

			for( int j = 0; j < segments; j++ ) {
				double t = (double)j / segments;
				double angle = startAngle + t * angleDiff;

				gp_Pnt arcPoint = new gp_Pnt(
					arcCenter.X() + radius * Math.Cos( angle ),
					arcCenter.Y() + radius * Math.Sin( angle ),
					0
				);

				// tangent and normal directions
				gp_Dir arcTangent = GeomUitility.GetClockwiseTangent( angle );
				gp_Dir arcNormal = GeomUitility.GetRadialNormal( arcCenter, arcPoint, radius );

				// create orientation point
				resultList.Add( GeomUitility.CreateOrientationPoint( arcPoint, toolVec, arcNormal, arcTangent, transformation ) );
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
		private const double DEFAULT_PRECISION_MIN_ERROR = 0.001;
		private const double DEFAULT_PRECISION_MAX_LENGTH = 1.0;
	}

	public static class GeomUitility
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
