using MyCAM.Data;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopoDS;
using System;

namespace MyCAM.StandardPatternFactory
{
	#region Strategy Pattern Interfaces and Classes

	internal interface IWireCreationStrategy
	{
		bool CreateWire( gp_Pnt centerPoint, gp_Pln plane, IStandardPatternGeomData standardPatternGeomData, out TopoDS_Wire wire );
	}

	internal class WireCreationStrategy<TGeomData> : IWireCreationStrategy
		where TGeomData : class, IStandardPatternGeomData
	{
		readonly Func<gp_Pnt, gp_Pln, TGeomData, WireResult> m_WireFactory;

		public WireCreationStrategy( Func<gp_Pnt, gp_Pln, TGeomData, WireResult> wireFactory )
		{
			m_WireFactory = wireFactory ?? throw new ArgumentNullException( nameof( wireFactory ) );
		}

		public bool CreateWire( gp_Pnt centerPoint, gp_Pln plane, IStandardPatternGeomData standardPatternGeomData, out TopoDS_Wire wire )
		{
			wire = null;

			// type-safe casting
			if( !( standardPatternGeomData is TGeomData typedGeomData ) ) {
				return false;
			}

			// call the factory delegate with typed geometry data
			WireResult result = m_WireFactory( centerPoint, plane, typedGeomData );
			wire = result.Wire;
			return result.Success;
		}
	}

	// because lambda expressions cannot capture out parameters and reference variables, define a struct to hold the result
	internal struct WireResult
	{
		public bool Success;
		public TopoDS_Wire Wire;

		public WireResult( bool success, TopoDS_Wire wire )
		{
			Success = success;
			Wire = wire;
		}
	}

	#endregion

	#region Strategy Factory

	internal static class WireCreationStrategyFactory
	{
		// create singleton strategy instances using generic WireCreationStrategy with lambda expressions
		static readonly IWireCreationStrategy s_CircleStrategy =
			new WireCreationStrategy<CircleGeomData>( ( center, plane, geom ) =>
			{
				bool success = StandardPatternWireFactory.CreateCircleWire( center, geom.Diameter, plane, out TopoDS_Wire wire );
				return new WireResult( success, wire );
			} );

		static readonly IWireCreationStrategy s_RectangleStrategy =
			new WireCreationStrategy<RectangleGeomData>( ( center, plane, geom ) =>
			{
				bool success = StandardPatternWireFactory.CreateRoundedRectangleWire( center, geom.Length, geom.Width, geom.CornerRadius, plane, out TopoDS_Wire wire );
				return new WireResult( success, wire );
			} );

		static readonly IWireCreationStrategy s_RunwayStrategy =
			new WireCreationStrategy<RunwayGeomData>( ( center, plane, geom ) =>
			{
				bool success = StandardPatternWireFactory.CreateRunwayWire( center, geom.Length, geom.Width, plane, out TopoDS_Wire wire );
				return new WireResult( success, wire );
			} );

		static readonly IWireCreationStrategy s_PolygonStrategy =
			new WireCreationStrategy<PolygonGeomData>( ( center, plane, geom ) =>
			{
				bool success = StandardPatternWireFactory.CreatePolygonWire( center, geom.Sides, geom.SideLength, geom.CornerRadius, plane, out TopoDS_Wire wire );
				return new WireResult( success, wire );
			} );

		public static IWireCreationStrategy GetStrategy( PathType pathType )
		{
			switch( pathType ) {
				case PathType.Circle:
					return s_CircleStrategy;
				case PathType.Rectangle:
					return s_RectangleStrategy;
				case PathType.Runway:
					return s_RunwayStrategy;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					return s_PolygonStrategy;
				case PathType.Contour:
				default:
					return null;
			}
		}
	}

	#endregion

	#region Unified Standard Pattern Wire Factory

	internal static class StandardPatternWireFactory
	{
		#region Circle Wire Creation

		public static bool CreateCircleWire( gp_Pnt centerPoint, double diameterius, gp_Pln plane, out TopoDS_Wire wire )
		{
			wire = null;
			double radius = diameterius / DIAMETER_TO_RADIUS_FACTOR;
			if( centerPoint == null || radius <= GEOMETRIC_TOLERANCE || plane == null ) {
				return false;
			}

			// create circle in local coordinate system (XY plane)
			gp_Pnt localCenter = new gp_Pnt( LOCAL_ORIGIN_X, LOCAL_ORIGIN_Y, LOCAL_ORIGIN_Z );
			gp_Dir localZDir = new gp_Dir( LOCAL_Z_DIRECTION_X, LOCAL_Z_DIRECTION_Y, LOCAL_Z_DIRECTION_Z );
			gp_Ax2 localAxis = new gp_Ax2( localCenter, localZDir );
			gp_Circ circle = new gp_Circ( localAxis, radius );

			// create edge
			BRepBuilderAPI_MakeEdge edgeBuilder = new BRepBuilderAPI_MakeEdge( circle );
			if( !edgeBuilder.IsDone() ) {
				return false;
			}
			TopoDS_Edge circleEdge = edgeBuilder.Edge();

			// create wire
			BRepBuilderAPI_MakeWire wireBuilder = new BRepBuilderAPI_MakeWire( circleEdge );
			if( !wireBuilder.IsDone() ) {
				return false;
			}
			TopoDS_Wire localWire = wireBuilder.Wire();

			// transform to target plane
			return TransformWireToPlane( localWire, centerPoint, plane, out wire );
		}

		#endregion

		#region Rectangle Wire Creation

		public static bool CreateRoundedRectangleWire( gp_Pnt centerPoint, double length, double width, double radius, gp_Pln plane, out TopoDS_Wire wire )
		{
			wire = null;
			if( centerPoint == null || plane == null || length <= GEOMETRIC_TOLERANCE || width <= GEOMETRIC_TOLERANCE || radius < ZERO_RADIUS_THRESHOLD ) {
				return false;
			}

			double halfL = length / HALF_DIMENSION_FACTOR;
			double halfW = width / HALF_DIMENSION_FACTOR;
			double r = Math.Min( radius, Math.Min( halfL, halfW ) );
			if( r < GEOMETRIC_TOLERANCE ) {
				r = ZERO_RADIUS_THRESHOLD;
			}

			BRepBuilderAPI_MakeWire wireBuilder = new BRepBuilderAPI_MakeWire();

			// create rounded or sharp rectangle based on radius
			if( r > ZERO_RADIUS_THRESHOLD ) {
				if( !CreateRoundedRectangleEdges( wireBuilder, halfL, halfW, r ) ) {
					return false;
				}
			}
			else {
				if( !CreateSharpRectangleEdges( wireBuilder, halfL, halfW ) ) {
					return false;
				}
			}

			// check wire creation
			if( !wireBuilder.IsDone() ) {
				return false;
			}
			TopoDS_Wire localWire = wireBuilder.Wire();

			// transform to target plane
			return TransformWireToPlane( localWire, centerPoint, plane, out wire );
		}

		static bool CreateRoundedRectangleEdges( BRepBuilderAPI_MakeWire wireBuilder, double halfL, double halfW, double r )
		{
			gp_Dir zDir = gp.DZ();

			// line points
			gp_Pnt p1 = new gp_Pnt( halfL - r, halfW, LOCAL_ORIGIN_Z );
			gp_Pnt p2 = new gp_Pnt( halfL, halfW - r, LOCAL_ORIGIN_Z );
			gp_Pnt p3 = new gp_Pnt( halfL, -( halfW - r ), LOCAL_ORIGIN_Z );
			gp_Pnt p4 = new gp_Pnt( halfL - r, -halfW, LOCAL_ORIGIN_Z );
			gp_Pnt p5 = new gp_Pnt( -( halfL - r ), -halfW, LOCAL_ORIGIN_Z );
			gp_Pnt p6 = new gp_Pnt( -halfL, -( halfW - r ), LOCAL_ORIGIN_Z );
			gp_Pnt p7 = new gp_Pnt( -halfL, halfW - r, LOCAL_ORIGIN_Z );
			gp_Pnt p8 = new gp_Pnt( -( halfL - r ), halfW, LOCAL_ORIGIN_Z );

			// arc centers
			gp_Pnt c1 = new gp_Pnt( halfL - r, halfW - r, LOCAL_ORIGIN_Z );
			gp_Pnt c2 = new gp_Pnt( halfL - r, -( halfW - r ), LOCAL_ORIGIN_Z );
			gp_Pnt c3 = new gp_Pnt( -( halfL - r ), -( halfW - r ), LOCAL_ORIGIN_Z );
			gp_Pnt c4 = new gp_Pnt( -( halfL - r ), halfW - r, LOCAL_ORIGIN_Z );

			// create straight edges
			BRepBuilderAPI_MakeEdge l1Builder = new BRepBuilderAPI_MakeEdge( p8, p1 );
			BRepBuilderAPI_MakeEdge l2Builder = new BRepBuilderAPI_MakeEdge( p2, p3 );
			BRepBuilderAPI_MakeEdge l3Builder = new BRepBuilderAPI_MakeEdge( p4, p5 );
			BRepBuilderAPI_MakeEdge l4Builder = new BRepBuilderAPI_MakeEdge( p6, p7 );
			if( !l1Builder.IsDone() || !l2Builder.IsDone() || !l3Builder.IsDone() || !l4Builder.IsDone() ) {
				return false;
			}

			// create arc edges
			BRepBuilderAPI_MakeEdge a1Builder = new BRepBuilderAPI_MakeEdge( new gp_Circ( new gp_Ax2( c1, zDir ), r ), p2, p1 );
			BRepBuilderAPI_MakeEdge a2Builder = new BRepBuilderAPI_MakeEdge( new gp_Circ( new gp_Ax2( c2, zDir ), r ), p4, p3 );
			BRepBuilderAPI_MakeEdge a3Builder = new BRepBuilderAPI_MakeEdge( new gp_Circ( new gp_Ax2( c3, zDir ), r ), p6, p5 );
			BRepBuilderAPI_MakeEdge a4Builder = new BRepBuilderAPI_MakeEdge( new gp_Circ( new gp_Ax2( c4, zDir ), r ), p8, p7 );
			if( !a1Builder.IsDone() || !a2Builder.IsDone() || !a3Builder.IsDone() || !a4Builder.IsDone() ) {
				return false;
			}

			// reverse arcs to ensure correct orientation
			TopoDS_Edge a1 = TopoDS.ToEdge( a1Builder.Edge().Reversed() );
			TopoDS_Edge a2 = TopoDS.ToEdge( a2Builder.Edge().Reversed() );
			TopoDS_Edge a3 = TopoDS.ToEdge( a3Builder.Edge().Reversed() );
			TopoDS_Edge a4 = TopoDS.ToEdge( a4Builder.Edge().Reversed() );

			// add edges in sequence
			wireBuilder.Add( l1Builder.Edge() );
			wireBuilder.Add( a1 );
			wireBuilder.Add( l2Builder.Edge() );
			wireBuilder.Add( a2 );
			wireBuilder.Add( l3Builder.Edge() );
			wireBuilder.Add( a3 );
			wireBuilder.Add( l4Builder.Edge() );
			wireBuilder.Add( a4 );

			return true;
		}

		static bool CreateSharpRectangleEdges( BRepBuilderAPI_MakeWire wireBuilder, double halfL, double halfW )
		{
			gp_Pnt pTopRight = new gp_Pnt( halfL, halfW, LOCAL_ORIGIN_Z );
			gp_Pnt pBotRight = new gp_Pnt( halfL, -halfW, LOCAL_ORIGIN_Z );
			gp_Pnt pBotLeft = new gp_Pnt( -halfL, -halfW, LOCAL_ORIGIN_Z );
			gp_Pnt pTopLeft = new gp_Pnt( -halfL, halfW, LOCAL_ORIGIN_Z );

			BRepBuilderAPI_MakeEdge eTopBuilder = new BRepBuilderAPI_MakeEdge( pTopLeft, pTopRight );
			BRepBuilderAPI_MakeEdge eRightBuilder = new BRepBuilderAPI_MakeEdge( pTopRight, pBotRight );
			BRepBuilderAPI_MakeEdge eBottomBuilder = new BRepBuilderAPI_MakeEdge( pBotRight, pBotLeft );
			BRepBuilderAPI_MakeEdge eLeftBuilder = new BRepBuilderAPI_MakeEdge( pBotLeft, pTopLeft );
			if( !eTopBuilder.IsDone() || !eRightBuilder.IsDone() || !eBottomBuilder.IsDone() || !eLeftBuilder.IsDone() ) {
				return false;
			}

			wireBuilder.Add( eTopBuilder.Edge() );
			wireBuilder.Add( eRightBuilder.Edge() );
			wireBuilder.Add( eBottomBuilder.Edge() );
			wireBuilder.Add( eLeftBuilder.Edge() );

			return true;
		}

		#endregion

		#region Runway Wire Creation

		public static bool CreateRunwayWire( gp_Pnt centerPoint, double length, double width, gp_Pln plane, out TopoDS_Wire wire )
		{
			wire = null;

			// parameter validation
			if( length <= GEOMETRIC_TOLERANCE || width <= GEOMETRIC_TOLERANCE || width > length ) {
				return false;
			}

			double radius = width / HALF_DIMENSION_FACTOR;
			double straightLength = length - width;
			double halfStraight = straightLength / HALF_DIMENSION_FACTOR;
			double halfWidth = width / HALF_DIMENSION_FACTOR;

			BRepBuilderAPI_MakeWire wireBuilder = new BRepBuilderAPI_MakeWire();

			// straight segment endpoints
			gp_Pnt leftTop = new gp_Pnt( -halfStraight, halfWidth, LOCAL_ORIGIN_Z );
			gp_Pnt rightTop = new gp_Pnt( halfStraight, halfWidth, LOCAL_ORIGIN_Z );
			gp_Pnt rightBottom = new gp_Pnt( halfStraight, -halfWidth, LOCAL_ORIGIN_Z );
			gp_Pnt leftBottom = new gp_Pnt( -halfStraight, -halfWidth, LOCAL_ORIGIN_Z );

			// arc centers at both ends of straight segments
			gp_Pnt leftCenter = new gp_Pnt( -halfStraight, LOCAL_ORIGIN_Y, LOCAL_ORIGIN_Z );
			gp_Pnt rightCenter = new gp_Pnt( halfStraight, LOCAL_ORIGIN_Y, LOCAL_ORIGIN_Z );

			gp_Dir localZ = new gp_Dir( LOCAL_Z_DIRECTION_X, LOCAL_Z_DIRECTION_Y, LOCAL_Z_DIRECTION_Z );

			// create straight segments
			BRepBuilderAPI_MakeEdge topEdgeBuilder = new BRepBuilderAPI_MakeEdge( leftTop, rightTop );
			BRepBuilderAPI_MakeEdge bottomEdgeBuilder = new BRepBuilderAPI_MakeEdge( leftBottom, rightBottom );
			if( !topEdgeBuilder.IsDone() || !bottomEdgeBuilder.IsDone() ) {
				return false;
			}

			// create arc segments
			// right semicircle: from rightBottom to rightTop
			gp_Circ rightCircle = new gp_Circ( new gp_Ax2( rightCenter, localZ ), radius );
			BRepBuilderAPI_MakeEdge rightArcBuilder = new BRepBuilderAPI_MakeEdge( rightCircle, rightBottom, rightTop );

			// left semicircle: from leftTop to leftBottom
			gp_Circ leftCircle = new gp_Circ( new gp_Ax2( leftCenter, localZ ), radius );
			BRepBuilderAPI_MakeEdge leftArcBuilder = new BRepBuilderAPI_MakeEdge( leftCircle, leftTop, leftBottom );
			if( !rightArcBuilder.IsDone() || !leftArcBuilder.IsDone() ) {
				return false;
			}

			// add edges in sequence to form closed runway (counterclockwise direction)
			wireBuilder.Add( topEdgeBuilder.Edge() );
			wireBuilder.Add( rightArcBuilder.Edge() );
			wireBuilder.Add( bottomEdgeBuilder.Edge() );
			wireBuilder.Add( leftArcBuilder.Edge() );
			if( !wireBuilder.IsDone() ) {
				return false;
			}
			TopoDS_Wire localWire = wireBuilder.Wire();

			// transform to target plane
			return TransformWireToPlane( localWire, centerPoint, plane, out wire );
		}

		#endregion

		#region Polygon Wire Creation

		public static bool CreatePolygonWire( gp_Pnt centerPoint, int sides, double sideLength, double cornerRadius, gp_Pln plane, out TopoDS_Wire wire )
		{
			wire = null;

			// parameter validation
			if( sides < MIN_POLYGON_SIDES || sides > MAX_POLYGON_SIDES || sideLength <= GEOMETRIC_TOLERANCE || cornerRadius < ZERO_RADIUS_THRESHOLD ) {
				return false;
			}

			// calculate circumradius R = sideLength / (2 * sin(£k/n))
			double angleStep = HALF_DIMENSION_FACTOR * Math.PI / sides;
			double radius = sideLength / ( HALF_DIMENSION_FACTOR * Math.Sin( Math.PI / sides ) );

			// determine initial angle offset based on the number of sides
			double angleOffset = GetAngleOffsetForSides( sides );

			// calculate polygon vertices
			gp_Pnt[] vertices = new gp_Pnt[ sides ];
			for( int i = 0; i < sides; i++ ) {
				double angle = i * angleStep + angleOffset;
				double x = radius * Math.Cos( angle );
				double y = radius * Math.Sin( angle );
				vertices[ i ] = new gp_Pnt( x, y, LOCAL_ORIGIN_Z );
			}

			BRepBuilderAPI_MakeWire wireBuilder = new BRepBuilderAPI_MakeWire();

			// create sharp-cornered polygon if fillet radius is 0 or very small
			if( cornerRadius <= GEOMETRIC_TOLERANCE ) {
				if( !CreateSharpPolygonEdges( wireBuilder, vertices, sides ) ) {
					return false;
				}
			}
			else {
				// create rounded polygon
				if( !CreateRoundedPolygonEdges( wireBuilder, vertices, cornerRadius, sides ) ) {
					return false;
				}
			}

			if( !wireBuilder.IsDone() ) {
				return false;
			}
			TopoDS_Wire localWire = wireBuilder.Wire();

			// transform to target plane
			return TransformWireToPlane( localWire, centerPoint, plane, out wire );
		}

		static double GetAngleOffsetForSides( int sides )
		{
			switch( sides ) {
				case TRIANGLE_SIDES:
					return TRIANGLE_ANGLE_OFFSET;
				case SQUARE_SIDES:
					return SQUARE_ANGLE_OFFSET;
				case PENTAGON_SIDES:
					return PENTAGON_ANGLE_OFFSET;
				case HEXAGON_SIDES:
					return HEXAGON_ANGLE_OFFSET;
				default:
					return DEFAULT_ANGLE_OFFSET;
			}
		}

		static bool CreateSharpPolygonEdges( BRepBuilderAPI_MakeWire wireBuilder, gp_Pnt[] vertices, int sides )
		{
			for( int i = 0; i < sides; i++ ) {
				gp_Pnt startPoint = vertices[ i ];
				gp_Pnt endPoint = vertices[ ( i + 1 ) % sides ];
				BRepBuilderAPI_MakeEdge edgeBuilder = new BRepBuilderAPI_MakeEdge( startPoint, endPoint );
				if( !edgeBuilder.IsDone() ) {
					return false;
				}
				wireBuilder.Add( edgeBuilder.Edge() );
			}
			return true;
		}

		static bool CreateRoundedPolygonEdges( BRepBuilderAPI_MakeWire wireBuilder, gp_Pnt[] vertices, double cornerRadius, int sides )
		{
			gp_Dir localZ = new gp_Dir( LOCAL_Z_DIRECTION_X, LOCAL_Z_DIRECTION_Y, LOCAL_Z_DIRECTION_Z );

			// for regular polygons, the interior angle is fixed: (n-2) * £k / n
			double interiorAngle = ( sides - INTERIOR_ANGLE_NUMERATOR_OFFSET ) * Math.PI / sides;
			double halfAngle = interiorAngle / HALF_DIMENSION_FACTOR;
			double tangentDistance = cornerRadius / Math.Tan( halfAngle );
			double centerDistance = cornerRadius / Math.Sin( halfAngle );

			// check if edge length is sufficient
			double edgeLength = vertices[ 0 ].Distance( vertices[ 1 ] );
			if( tangentDistance * EDGE_TANGENT_MULTIPLIER > edgeLength ) {
				return false;
			}

			// calculate start and end points of the edges after being shortened by the fillet
			gp_Pnt[] edgeStarts = new gp_Pnt[ sides ];
			gp_Pnt[] edgeEnds = new gp_Pnt[ sides ];

			for( int i = 0; i < sides; i++ ) {
				gp_Pnt currentVertex = vertices[ i ];
				gp_Pnt nextVertex = vertices[ ( i + 1 ) % sides ];

				// current edge vector
				gp_Vec edgeVec = new gp_Vec( currentVertex, nextVertex );
				edgeVec.Normalize();

				// edge start point: shortened by the fillet of the current vertex
				edgeStarts[ i ] = new gp_Pnt(
					currentVertex.X() + edgeVec.X() * tangentDistance,
					currentVertex.Y() + edgeVec.Y() * tangentDistance,
					currentVertex.Z()
				);

				// edge end point: shortened by the fillet of the next vertex
				edgeEnds[ i ] = new gp_Pnt(
					nextVertex.X() - edgeVec.X() * tangentDistance,
					nextVertex.Y() - edgeVec.Y() * tangentDistance,
					nextVertex.Z()
				);
			}

			// add all edges and fillets
			for( int i = 0; i < sides; i++ ) {
				// add shortened straight edge
				BRepBuilderAPI_MakeEdge edgeBuilder = new BRepBuilderAPI_MakeEdge( edgeStarts[ i ], edgeEnds[ i ] );
				if( !edgeBuilder.IsDone() ) {
					return false;
				}
				wireBuilder.Add( edgeBuilder.Edge() );

				// add inscribed circular fillet at the next vertex
				int nextVertexIndex = ( i + 1 ) % sides;
				if( !CreateInscribedCornerArc( wireBuilder, edgeEnds[ i ], edgeStarts[ nextVertexIndex ],
					vertices[ nextVertexIndex ], cornerRadius, centerDistance, localZ ) ) {
					return false;
				}
			}

			return true;
		}

		static bool CreateInscribedCornerArc( BRepBuilderAPI_MakeWire wireBuilder, gp_Pnt tangentPoint1, gp_Pnt tangentPoint2,
			gp_Pnt vertex, double radius, double centerDistance, gp_Dir normalDir )
		{
			// calculate vectors from vertex to the two tangent points
			gp_Vec vec1 = new gp_Vec( vertex, tangentPoint1 );
			gp_Vec vec2 = new gp_Vec( vertex, tangentPoint2 );

			vec1.Normalize();
			vec2.Normalize();

			// calculate bisector direction
			gp_Vec bisector = vec1.Added( vec2 );
			bisector.Normalize();

			// inscribed circle center: move inward along the bisector
			gp_Pnt arcCenter = new gp_Pnt(
				vertex.X() + bisector.X() * centerDistance,
				vertex.Y() + bisector.Y() * centerDistance,
				vertex.Z()
			);

			// create inscribed circular arc
			gp_Ax2 circleAxis = new gp_Ax2( arcCenter, normalDir );
			gp_Circ circle = new gp_Circ( circleAxis, radius );

			// create arc: from tangentPoint1 to tangentPoint2 (on the inside)
			BRepBuilderAPI_MakeEdge arcEdgeBuilder = new BRepBuilderAPI_MakeEdge( circle, tangentPoint1, tangentPoint2 );
			if( !arcEdgeBuilder.IsDone() ) {
				return false;
			}
			wireBuilder.Add( arcEdgeBuilder.Edge() );
			return true;
		}

		#endregion

		#region Common Transformation Utilities

		static bool TransformWireToPlane( TopoDS_Wire localWire, gp_Pnt centerPoint, gp_Pln plane, out TopoDS_Wire wire )
		{
			wire = null;

			// create target coordinate system
			gp_Ax3 targetCoordSystem = new gp_Ax3( centerPoint, plane.Axis().Direction(), plane.XAxis().Direction() );

			// create transformation
			gp_Trsf transformation = new gp_Trsf();
			transformation.SetTransformation( targetCoordSystem, new gp_Ax3() );

			// apply transformation
			BRepBuilderAPI_Transform transformBuilder = new BRepBuilderAPI_Transform( localWire, transformation, true );
			if( !transformBuilder.IsDone() ) {
				return false;
			}

			wire = TopoDS.ToWire( transformBuilder.Shape() );
			return true;
		}

		#endregion

		// tolerance for geometric comparisons
		public const double GEOMETRIC_TOLERANCE = 1e-9;

		// half dimension factor for calculating half measurements
		public const double HALF_DIMENSION_FACTOR = 2.0;

		// diameter to radius conversion factor
		public const double DIAMETER_TO_RADIUS_FACTOR = 2.0;

		// local coordinate system origin (XY plane)
		public const double LOCAL_ORIGIN_X = 0.0;
		public const double LOCAL_ORIGIN_Y = 0.0;
		public const double LOCAL_ORIGIN_Z = 0.0;

		// local Z-axis direction (pointing up in XY plane)
		public const double LOCAL_Z_DIRECTION_X = 0.0;
		public const double LOCAL_Z_DIRECTION_Y = 0.0;
		public const double LOCAL_Z_DIRECTION_Z = 1.0;

		// zero radius threshold for sharp corners
		public const double ZERO_RADIUS_THRESHOLD = 0.0;

		// polygon side constraints
		const int MIN_POLYGON_SIDES = 3;
		const int MAX_POLYGON_SIDES = 6;

		// polygon-specific angle offsets
		const int TRIANGLE_SIDES = 3;
		const double TRIANGLE_ANGLE_OFFSET = Math.PI / 2.0;    // 90 degrees

		const int SQUARE_SIDES = 4;
		const double SQUARE_ANGLE_OFFSET = Math.PI / 4.0;      // 45 degrees

		const int PENTAGON_SIDES = 5;
		const double PENTAGON_ANGLE_OFFSET = Math.PI / 2.0;    // 90 degrees

		const int HEXAGON_SIDES = 6;
		const double HEXAGON_ANGLE_OFFSET = 0.0;               // 0 degrees

		const double DEFAULT_ANGLE_OFFSET = 0.0;

		// interior angle calculation constant
		const int INTERIOR_ANGLE_NUMERATOR_OFFSET = 2;

		// edge tangent distance multiplier
		const double EDGE_TANGENT_MULTIPLIER = 2.0;
	}

	#endregion
}
