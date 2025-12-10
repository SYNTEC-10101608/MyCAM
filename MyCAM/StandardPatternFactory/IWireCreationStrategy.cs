using MyCAM.Data;
using MyCAM.Data.GeomDataFolder;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopoDS;
using System;

namespace MyCAM.StandardPatternFactory
{
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

	internal static class WireCreationStrategyFactory
	{
		// create singleton strategy instances using generic WireCreationStrategy with lambda expressions
		static readonly IWireCreationStrategy s_CircleStrategy =
			new WireCreationStrategy<CircleGeomData>( ( center, plane, geom ) =>
			{
				bool success = CircleFactory.CreateCircleWireOnPlane( center, geom.Diameter / 2.0, plane, out TopoDS_Wire wire );
				return new WireResult( success, wire );
			} );

		static readonly IWireCreationStrategy s_RectangleStrategy =
			new WireCreationStrategy<RectangleGeomData>( ( center, plane, geom ) =>
			{
				bool success = RectangleFactory.CreateRoundedRectangleWireOnPlane( center, geom.Length, geom.Width, geom.CornerRadius, plane, out TopoDS_Wire wire );
				return new WireResult( success, wire );
			} );

		static readonly IWireCreationStrategy s_RunwayStrategy =
			new WireCreationStrategy<RunwayGeomData>( ( center, plane, geom ) =>
			{
				bool success = RunwayFactory.CreateRunwayWireOnPlane( center, geom.Length, geom.Width, plane, out TopoDS_Wire wire );
				return new WireResult( success, wire );
			} );

		static readonly IWireCreationStrategy s_PolygonStrategy =
			new WireCreationStrategy<PolygonGeomData>( ( center, plane, geom ) =>
			{
				bool success = PolygonFactory.CreatePolygonWireOnPlane( center, geom.Sides, geom.SideLength, geom.CornerRadius, plane, out TopoDS_Wire wire );
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

	internal static class CircleFactory
	{
		public static bool CreateCircleWireOnPlane( gp_Pnt centerPoint, double radius, gp_Pln plane, out TopoDS_Wire wire )
		{
			wire = null;
			if( centerPoint == null || radius <= 1e-9 || plane == null ) {
				return false;
			}

			// create circle in local coordinate system (XY plane)
			gp_Pnt localCenter = new gp_Pnt( 0, 0, 0 );
			gp_Dir localZDir = new gp_Dir( 0, 0, 1 );
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
	}

	internal static class RectangleFactory
	{
		public static bool CreateRoundedRectangleWireOnPlane( gp_Pnt centerPoint, double length, double width, double radius, gp_Pln plane, out TopoDS_Wire wire )
		{
			wire = null;
			if( centerPoint == null || plane == null || length <= 1e-9 || width <= 1e-9 || radius < 0 ) {
				return false;
			}

			double halfL = length / 2.0;
			double halfW = width / 2.0;
			double r = Math.Min( radius, Math.Min( halfL, halfW ) );
			if( r < 1e-9 ) {
				r = 0;
			}

			BRepBuilderAPI_MakeWire wireBuilder = new BRepBuilderAPI_MakeWire();

			// rounded rectangle in XY plane
			if( r > 0 ) {
				gp_Dir zDir = gp.DZ();

				// line points
				gp_Pnt p1 = new gp_Pnt( halfL - r, halfW, 0 );
				gp_Pnt p2 = new gp_Pnt( halfL, halfW - r, 0 );
				gp_Pnt p3 = new gp_Pnt( halfL, -( halfW - r ), 0 );
				gp_Pnt p4 = new gp_Pnt( halfL - r, -halfW, 0 );
				gp_Pnt p5 = new gp_Pnt( -( halfL - r ), -halfW, 0 );
				gp_Pnt p6 = new gp_Pnt( -halfL, -( halfW - r ), 0 );
				gp_Pnt p7 = new gp_Pnt( -halfL, halfW - r, 0 );
				gp_Pnt p8 = new gp_Pnt( -( halfL - r ), halfW, 0 );

				// arc centers
				gp_Pnt c1 = new gp_Pnt( halfL - r, halfW - r, 0 );
				gp_Pnt c2 = new gp_Pnt( halfL - r, -( halfW - r ), 0 );
				gp_Pnt c3 = new gp_Pnt( -( halfL - r ), -( halfW - r ), 0 );
				gp_Pnt c4 = new gp_Pnt( -( halfL - r ), halfW - r, 0 );

				BRepBuilderAPI_MakeEdge l1Builder = new BRepBuilderAPI_MakeEdge( p8, p1 );
				BRepBuilderAPI_MakeEdge l2Builder = new BRepBuilderAPI_MakeEdge( p2, p3 );
				BRepBuilderAPI_MakeEdge l3Builder = new BRepBuilderAPI_MakeEdge( p4, p5 );
				BRepBuilderAPI_MakeEdge l4Builder = new BRepBuilderAPI_MakeEdge( p6, p7 );
				if( !l1Builder.IsDone() || !l2Builder.IsDone() || !l3Builder.IsDone() || !l4Builder.IsDone() ) {
					return false;
				}
				TopoDS_Edge l1 = l1Builder.Edge();
				TopoDS_Edge l2 = l2Builder.Edge();
				TopoDS_Edge l3 = l3Builder.Edge();
				TopoDS_Edge l4 = l4Builder.Edge();

				// arc edges
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

				wireBuilder.Add( l1 );
				wireBuilder.Add( a1 );
				wireBuilder.Add( l2 );
				wireBuilder.Add( a2 );
				wireBuilder.Add( l3 );
				wireBuilder.Add( a3 );
				wireBuilder.Add( l4 );
				wireBuilder.Add( a4 );
			}
			// sharp rectangle
			else {
				gp_Pnt pTopRight = new gp_Pnt( halfL, halfW, 0 );
				gp_Pnt pBotRight = new gp_Pnt( halfL, -halfW, 0 );
				gp_Pnt pBotLeft = new gp_Pnt( -halfL, -halfW, 0 );
				gp_Pnt pTopLeft = new gp_Pnt( -halfL, halfW, 0 );

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
			}

			// check wire creation
			if( !wireBuilder.IsDone() ) {
				return false;
			}
			TopoDS_Wire localWire = wireBuilder.Wire();

			// create target coordinate system
			gp_Ax3 targetCoordSystem = new gp_Ax3( centerPoint, plane.Axis().Direction(), plane.XAxis().Direction() );
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
	}

	internal static class RunwayFactory
	{
		public static bool CreateRunwayWireOnPlane( gp_Pnt centerPoint, double length, double width, gp_Pln plane, out TopoDS_Wire wire )
		{
			wire = null;

			// parameter validation
			if( length <= 1e-9 || width <= 1e-9 || width > length ) {
				return false;
			}
			double radius = width / 2.0;
			double straightLength = length - width;

			BRepBuilderAPI_MakeWire wireBuilder = new BRepBuilderAPI_MakeWire();
			double halfLength = length / 2.0;
			double halfWidth = width / 2.0;
			double halfStraight = straightLength / 2.0;

			// straight segment endpoints
			gp_Pnt leftTop = new gp_Pnt( -halfStraight, halfWidth, 0 );
			gp_Pnt rightTop = new gp_Pnt( halfStraight, halfWidth, 0 );
			gp_Pnt rightBottom = new gp_Pnt( halfStraight, -halfWidth, 0 );
			gp_Pnt leftBottom = new gp_Pnt( -halfStraight, -halfWidth, 0 );

			// arc centers at both ends of straight segments
			gp_Pnt leftCenter = new gp_Pnt( -halfStraight, 0, 0 );
			gp_Pnt rightCenter = new gp_Pnt( halfStraight, 0, 0 );

			gp_Dir localZ = new gp_Dir( 0, 0, 1 );

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

			// create target coordinate system
			gp_Ax3 targetCoordSystem = new gp_Ax3( centerPoint, plane.Axis().Direction(), plane.XAxis().Direction() );

			// create transformation matrix
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
	}

	internal static class PolygonFactory
	{
		public static bool CreatePolygonWireOnPlane( gp_Pnt centerPoint, int sides, double sideLength, double cornerRadius, gp_Pln plane, out TopoDS_Wire wire )
		{
			wire = null;

			// parameter validation
			if( sides < 3 || sides > 6 || sideLength <= 1e-9 || cornerRadius < 0 ) {
				return false;
			}

			// calculate circumradius R = sideLength / (2 * sin(£k/n))
			double angleStep = 2.0 * Math.PI / sides;
			double radius = sideLength / ( 2.0 * Math.Sin( Math.PI / sides ) );

			// determine initial angle offset based on the number of sides
			double angleOffset;
			switch( sides ) {
				case 3:
					angleOffset = Math.PI / 2.0;
					break;
				case 4:
					angleOffset = Math.PI / 4.0;
					break;
				case 5:
					angleOffset = Math.PI / 2.0;
					break;
				case 6:
					angleOffset = 0.0;
					break;
				default:
					angleOffset = 0.0;
					break;
			}

			// calculate polygon vertices
			gp_Pnt[] vertices = new gp_Pnt[ sides ];
			for( int i = 0; i < sides; i++ ) {
				double angle = i * angleStep + angleOffset;
				double x = radius * Math.Cos( angle );
				double y = radius * Math.Sin( angle );
				vertices[ i ] = new gp_Pnt( x, y, 0 );
			}

			BRepBuilderAPI_MakeWire wireBuilder = new BRepBuilderAPI_MakeWire();

			// create sharp-cornered polygon if fillet radius is 0 or very small
			if( cornerRadius <= 1e-9 ) {
				for( int i = 0; i < sides; i++ ) {
					gp_Pnt startPoint = vertices[ i ];
					gp_Pnt endPoint = vertices[ ( i + 1 ) % sides ];
					BRepBuilderAPI_MakeEdge edgeBuilder = new BRepBuilderAPI_MakeEdge( startPoint, endPoint );
					if( !edgeBuilder.IsDone() ) {
						return false;
					}
					wireBuilder.Add( edgeBuilder.Edge() );
				}
			}
			else {
				// attempt to create rounded polygon
				if( !CreateRoundedPolygonWire( wireBuilder, vertices, cornerRadius, sides ) ) {
					return false;
				}
			}

			if( !wireBuilder.IsDone() ) {
				return false;
			}
			TopoDS_Wire localWire = wireBuilder.Wire();

			// create target coordinate system
			gp_Ax3 targetCoordSystem = new gp_Ax3( centerPoint, plane.Axis().Direction(), plane.XAxis().Direction() );

			// create transformation matrix
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

		static bool CreateRoundedPolygonWire( BRepBuilderAPI_MakeWire wireBuilder, gp_Pnt[] vertices, double cornerRadius, int sides )
		{
			gp_Dir localZ = new gp_Dir( 0, 0, 1 );

			// for regular polygons, the interior angle is fixed: (n-2) * £k / n
			double interiorAngle = ( sides - 2 ) * Math.PI / sides;
			double halfAngle = interiorAngle / 2.0;
			double tangentDistance = cornerRadius / Math.Tan( halfAngle );
			double centerDistance = cornerRadius / Math.Sin( halfAngle );

			// check if edge length is sufficient
			double edgeLength = vertices[ 0 ].Distance( vertices[ 1 ] );
			if( tangentDistance * 2 > edgeLength ) {
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
				//add shortened straight edge
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
	}
}
