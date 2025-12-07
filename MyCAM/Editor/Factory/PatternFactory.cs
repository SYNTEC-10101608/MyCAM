using MyCAM.Data;
using MyCAM.Data.GeomDataFolder;
using OCC.BRepBuilderAPI;
using OCC.Geom;
using OCC.gp;
using OCC.TColgp;
using OCC.TopoDS;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor.Factory
{
	public class PatternFactory
	{
		public PatternFactory( List<CADPoint> contourCADPointList, IGeomData geomData )
		{
			if( contourCADPointList == null || contourCADPointList.Count == 0 || geomData == null ) {
				throw new ArgumentNullException( "PatternFactory constructing argument null" );
			}

			m_GeomData = geomData;
			m_ContourCADPointList = contourCADPointList;
			GetContrePointAndNormalDir( m_ContourCADPointList, out m_CenterPoint, out m_NormalDir );

			double rotatedAngleInDegrees = 0.0;
			switch( geomData.PathType ) {
				case PathType.Circle:
					CircleGeomData circleGeom = (CircleGeomData)geomData;
					rotatedAngleInDegrees = circleGeom.RotatedAngle_deg;
					break;
				case PathType.Rectangle:
					RectangleGeomData rectangleGeom = (RectangleGeomData)geomData;
					rotatedAngleInDegrees = rectangleGeom.RotatedAngle_deg;
					break;
				case PathType.Runway:
					RunwayGeomData runwayGeom = (RunwayGeomData)geomData;
					rotatedAngleInDegrees = runwayGeom.RotatedAngle_deg;
					break;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					PolygonGeomData polygonGeomData = (PolygonGeomData)geomData;
					rotatedAngleInDegrees = polygonGeomData.RotatedAngle_deg;
					break;
				case PathType.Contour:
				default:
					rotatedAngleInDegrees = 0.0;
					break;
			}

			GetLocalCoordination( m_CenterPoint, m_NormalDir, rotatedAngleInDegrees, out m_XAxisDir, out m_YAxisDir );

			TopoDS_Wire wire;
			if( geomData.PathType == PathType.Contour ) {
				ContourGeomData contourGeomData = (ContourGeomData)geomData;
				if( ContourFactory.CreateWireFromGeomData( m_ContourCADPointList, contourGeomData, out wire ) ) {
					m_ShapeWire = wire;
				}
			}
			else {
				if( CreateStandardPatternWire( out wire ) ) {
					m_ShapeWire = wire;
				}
			}
		}

		public TopoDS_Shape GetShape()
		{
			return m_ShapeWire;
		}

		public gp_Ax3 GetCoordinateInfo()
		{
			return new gp_Ax3( m_CenterPoint, m_NormalDir, m_XAxisDir );
		}

		void GetContrePointAndNormalDir( List<CADPoint> points, out gp_Pnt originalPoint, out gp_Dir normalDirect )
		{
			originalPoint = new gp_Pnt();
			normalDirect = new gp_Dir();

			if( points == null || points.Count == 0 ) {
				return;
			}

			// ensure point spacing does not exceed 0.01
			List<CADPoint> densifiedPoints = DensifyCADPointList( points, 0.01 );

			int nPointNumber = densifiedPoints.Count;
			double xValue = 0.0;
			double yValue = 0.0;
			double zValue = 0.0;
			double xDirValue = 0.0;
			double yDirValue = 0.0;
			double zDirValue = 0.0;

			foreach( var point in densifiedPoints ) {
				xValue += point.Point.X();
				yValue += point.Point.Y();
				zValue += point.Point.Z();
				xDirValue += point.NormalVec_1st.X();
				yDirValue += point.NormalVec_1st.Y();
				zDirValue += point.NormalVec_1st.Z();
			}
			originalPoint.SetCoord( xValue / nPointNumber, yValue / nPointNumber, zValue / nPointNumber );
			normalDirect.SetCoord( xDirValue / nPointNumber, yDirValue / nPointNumber, zDirValue / nPointNumber );
		}

		List<CADPoint> DensifyCADPointList( List<CADPoint> originalPoints, double maxSpacing )
		{
			List<CADPoint> densifiedPoints = new List<CADPoint>();

			if( originalPoints == null || originalPoints.Count == 0 ) {
				return densifiedPoints;
			}

			// add first point
			densifiedPoints.Add( originalPoints[ 0 ] );

			// process each segment
			for( int i = 0; i < originalPoints.Count - 1; i++ ) {
				CADPoint currentPoint = originalPoints[ i ];
				CADPoint nextPoint = originalPoints[ i + 1 ];

				// calculate distance between current and next point
				double distance = currentPoint.Point.Distance( nextPoint.Point );

				if( distance > maxSpacing ) {
					// need to insert intermediate points
					int numSegments = (int)Math.Ceiling( distance / maxSpacing );

					for( int j = 1; j < numSegments; j++ ) {
						double t = (double)j / numSegments;
						CADPoint interpolatedPoint = InterpolateCADPoints( currentPoint, nextPoint, t );
						densifiedPoints.Add( interpolatedPoint );
					}
				}

				// add next point
				densifiedPoints.Add( nextPoint );
			}

			return densifiedPoints;
		}

		CADPoint InterpolateCADPoints( CADPoint point1, CADPoint point2, double t )
		{
			// interpolate position
			gp_Pnt pos1 = point1.Point;
			gp_Pnt pos2 = point2.Point;
			gp_Pnt interpolatedPos = new gp_Pnt(
				pos1.X() + t * ( pos2.X() - pos1.X() ),
				pos1.Y() + t * ( pos2.Y() - pos1.Y() ),
				pos1.Z() + t * ( pos2.Z() - pos1.Z() )
			);

			// interpolate vectors using spherical linear interpolation (SLERP)
			gp_Dir interpolatedNormal1st = InterpolateDirections( point1.NormalVec_1st, point2.NormalVec_1st, t );
			gp_Dir interpolatedNormal2nd = InterpolateDirections( point1.NormalVec_2nd, point2.NormalVec_2nd, t );
			gp_Dir interpolatedTangent = InterpolateDirections( point1.TangentVec, point2.TangentVec, t );

			return new CADPoint( interpolatedPos, interpolatedNormal1st, interpolatedNormal2nd, interpolatedTangent );
		}

		gp_Dir InterpolateDirections( gp_Dir dir1, gp_Dir dir2, double t )
		{
			// handle edge cases
			if( t <= 0.0 )
				return new gp_Dir( dir1.XYZ() );
			if( t >= 1.0 )
				return new gp_Dir( dir2.XYZ() );

			// convert to vectors for calculation
			gp_Vec vec1 = new gp_Vec( dir1 );
			gp_Vec vec2 = new gp_Vec( dir2 );

			// check if vectors are opposite (dot product close to -1)
			double dotProduct = vec1.Dot( vec2 );
			if( dotProduct < -0.999 ) {
				// vectors are nearly opposite, use simple interpolation
				// find a perpendicular vector for intermediate direction
				gp_Vec perpVec;
				if( Math.Abs( vec1.X() ) < 0.9 ) {
					perpVec = new gp_Vec( 1, 0, 0 );
				}
				else {
					perpVec = new gp_Vec( 0, 1, 0 );
				}

				gp_Vec crossVec = vec1.Crossed( perpVec );
				crossVec.Normalize();

				// rotate around the perpendicular axis
				double angle = Math.PI * t;
				gp_Ax1 rotAxis = new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( crossVec ) );
				gp_Trsf rotation = new gp_Trsf();
				rotation.SetRotation( rotAxis, angle );

				gp_Vec resultVec = vec1.Transformed( rotation );
				return new gp_Dir( resultVec );
			}

			// use spherical linear interpolation (SLERP)
			try {
				// clamp dot product to avoid numerical errors
				dotProduct = Math.Max( -1.0, Math.Min( 1.0, dotProduct ) );

				double angle = Math.Acos( Math.Abs( dotProduct ) );

				if( angle < 1e-6 ) {
					// vectors are nearly parallel, use linear interpolation
					gp_Vec linearVec = vec1.Multiplied( 1.0 - t ).Added( vec2.Multiplied( t ) );
					linearVec.Normalize();
					return new gp_Dir( linearVec );
				}

				double sinAngle = Math.Sin( angle );
				double factor1 = Math.Sin( ( 1.0 - t ) * angle ) / sinAngle;
				double factor2 = Math.Sin( t * angle ) / sinAngle;

				// handle sign correction for dot product
				if( dotProduct < 0 ) {
					factor2 = -factor2;
				}

				gp_Vec slerpVec = vec1.Multiplied( factor1 ).Added( vec2.Multiplied( factor2 ) );
				slerpVec.Normalize();

				return new gp_Dir( slerpVec );
			}
			catch {
				// fallback to simple linear interpolation if SLERP fails
				gp_Vec fallbackVec = vec1.Multiplied( 1.0 - t ).Added( vec2.Multiplied( t ) );
				fallbackVec.Normalize();
				return new gp_Dir( fallbackVec );
			}
		}

		void GetLocalCoordination( gp_Pnt centerPoint, gp_Dir normalDir, double rotationAngleInDegrees, out gp_Dir xDir, out gp_Dir yDir )
		{
			if( normalDir.IsParallel( new gp_Dir( 0, 0, 1 ), 0.001 ) ) {
				gp_Trsf trsf = new gp_Trsf();
				trsf.SetRotation( new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ) ), 0 );
				gp_Dir refDir = new gp_Dir( 1, 0, 0 );
				xDir = refDir.Transformed( trsf );
			}
			else {
				gp_Dir refDir = new gp_Dir( 0, 0, 1 );
				xDir = refDir.Crossed( normalDir );
			}
			yDir = normalDir.Crossed( xDir );

			gp_Ax3 coordSystem = new gp_Ax3( centerPoint, normalDir, xDir );
			if( Math.Abs( rotationAngleInDegrees ) > 1e-9 ) {
				double rotationAngleInRadians = rotationAngleInDegrees * Math.PI / 180.0;
				gp_Ax1 rotationAxis = new gp_Ax1( centerPoint, normalDir );
				coordSystem.Rotate( rotationAxis, rotationAngleInRadians );
			}
			xDir = coordSystem.XDirection();
			yDir = coordSystem.YDirection();
		}

		bool CreateStandardPatternWire( out TopoDS_Wire wire )
		{
			gp_Ax3 ax3 = new gp_Ax3( m_CenterPoint, m_NormalDir, m_XAxisDir );
			m_Plane = new gp_Pln( ax3 );
			wire = null;
			switch( m_GeomData.PathType ) {
				case PathType.Circle:
					CircleGeomData circleGeom = (CircleGeomData)m_GeomData;
					return CircleFactory.CreateCircleWireOnPlane( m_CenterPoint, circleGeom.Diameter / 2.0, m_Plane, out wire );
				case PathType.Rectangle:
					RectangleGeomData rectangleGeomData = (RectangleGeomData)m_GeomData;
					return RectangleFactory.CreateRoundedRectangleWireOnPlane( m_CenterPoint, rectangleGeomData.Length, rectangleGeomData.Width, rectangleGeomData.CornerRadius, m_Plane, out wire );
				case PathType.Runway:
					RunwayGeomData runwayGeomData = (RunwayGeomData)m_GeomData;
					return RunwayFactory.CreateRunwayWireOnPlane( m_CenterPoint, runwayGeomData.Length, runwayGeomData.Width, m_Plane, out wire );
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					PolygonGeomData polygonGeomData = (PolygonGeomData)m_GeomData;
					return PolygonFactory.CreatePolygonWireOnPlane( m_CenterPoint, polygonGeomData.Sides, polygonGeomData.SideLength, polygonGeomData.CornerRadius, m_Plane, out wire );
				default:
					return false;
			}
		}

		IGeomData m_GeomData;
		TopoDS_Wire m_ShapeWire;
		gp_Pln m_Plane;
		gp_Pnt m_CenterPoint;
		gp_Dir m_NormalDir;
		gp_Dir m_XAxisDir;
		gp_Dir m_YAxisDir;
		List<CADPoint> m_ContourCADPointList;
	}

	public static class ContourFactory
	{
		public static bool CreateWireFromGeomData( List<CADPoint> contourCADPointList, ContourGeomData contourGeomData, out TopoDS_Wire wire )
		{
			wire = null;
			if( contourGeomData == null || contourCADPointList == null || contourCADPointList.Count == 0 ) {
				return false;
			}
			List<CADPoint> cadPointList = contourCADPointList;
			bool isClosed = DetermineIfClosed( cadPointList );
			return CreateWireFromCADPointList( cadPointList, out wire, isClosed );
		}

		static bool CreateWireFromCADPointList( List<CADPoint> cadPointList, out TopoDS_Wire wire, bool isClosed = false, double tolerance = -1 )
		{
			wire = null;

			// parameter validation
			if( cadPointList == null || cadPointList.Count < 2 ) {
				return false;
			}

			// set default tolerance value
			if( tolerance < 0 ) {
				tolerance = 0.001;
			}

			BRepBuilderAPI_MakeWire wireBuilder = new BRepBuilderAPI_MakeWire();

			// create edge sequence
			for( int i = 0; i < cadPointList.Count - 1; i++ ) {
				gp_Pnt startPoint = cadPointList[ i ].Point;
				gp_Pnt endPoint = cadPointList[ i + 1 ].Point;

				// check if points are coincident
				if( startPoint.Distance( endPoint ) > tolerance ) {
					TopoDS_Edge edge;
					if( !CreateEdgeFromCADPoints( cadPointList[ i ], cadPointList[ i + 1 ], out edge ) ) {
						return false;
					}
					wireBuilder.Add( edge );
				}
			}
			if( !wireBuilder.IsDone() ) {
				return false;
			}
			wire = wireBuilder.Wire();
			return true;
		}

		static bool CreateEdgeFromCADPoints( CADPoint startCADPoint, CADPoint endCADPoint, out TopoDS_Edge edge )
		{
			edge = null;
			gp_Pnt startPnt = startCADPoint.Point;
			gp_Pnt endPnt = endCADPoint.Point;

			// get tangent vectors
			gp_Dir startTangent = startCADPoint.TangentVec;
			gp_Dir endTangent = endCADPoint.TangentVec;

			// check if linear connection is possible
			if( IsLinearConnection( startPnt, endPnt, startTangent, endTangent ) ) {
				BRepBuilderAPI_MakeEdge edgeBuilder = new BRepBuilderAPI_MakeEdge( startPnt, endPnt );
				if( !edgeBuilder.IsDone() ) {
					return false;
				}
				edge = edgeBuilder.Edge();
				return true;
			}
			else {

				// use Bezier curve for interpolation
				return CreateBezierEdge( startCADPoint, endCADPoint, out edge );
			}
		}

		static bool IsLinearConnection( gp_Pnt startPnt, gp_Pnt endPnt, gp_Dir startTangent, gp_Dir endTangent )
		{
			// calculate vector between points
			gp_Vec connectionVec = new gp_Vec( startPnt, endPnt );
			if( connectionVec.Magnitude() < 0.001 ) {
				return true;
			}

			gp_Dir connectionDir = new gp_Dir( connectionVec );

			// check if tangent vectors are parallel to connection vector
			double angleTolerance = 0.1; // in radians (~5.7 degrees)
			bool startParallel = startTangent.IsParallel( connectionDir, angleTolerance );
			bool endParallel = endTangent.IsParallel( connectionDir, angleTolerance );

			return startParallel && endParallel;
		}

		static bool CreateBezierEdge( CADPoint startCADPoint, CADPoint endCADPoint, out TopoDS_Edge edge )
		{
			edge = null;
			gp_Pnt startPnt = startCADPoint.Point;
			gp_Pnt endPnt = endCADPoint.Point;
			gp_Dir startTangent = startCADPoint.TangentVec;
			gp_Dir endTangent = endCADPoint.TangentVec;

			// calculate control point distance (based on 1/3 of distance between points)
			double distance = startPnt.Distance( endPnt );
			double controlDistance = distance / 3.0;

			// calculate control points
			gp_Pnt controlPnt1 = new gp_Pnt(
				startPnt.X() + startTangent.X() * controlDistance,
				startPnt.Y() + startTangent.Y() * controlDistance,
				startPnt.Z() + startTangent.Z() * controlDistance
			);

			gp_Pnt controlPnt2 = new gp_Pnt(
				endPnt.X() - endTangent.X() * controlDistance,
				endPnt.Y() - endTangent.Y() * controlDistance,
				endPnt.Z() - endTangent.Z() * controlDistance
			);

			// create Bezier curve
			TColgp_Array1OfPnt poles = new TColgp_Array1OfPnt( 1, 4 );
			poles.SetValue( 1, startPnt );
			poles.SetValue( 2, controlPnt1 );
			poles.SetValue( 3, controlPnt2 );
			poles.SetValue( 4, endPnt );

			Geom_BezierCurve bezierCurve = new Geom_BezierCurve( poles );
			BRepBuilderAPI_MakeEdge edgeBuilder = new BRepBuilderAPI_MakeEdge( bezierCurve );

			if( !edgeBuilder.IsDone() ) {

				// if Bezier curve creation fails, fallback to straight line
				BRepBuilderAPI_MakeEdge straightEdgeBuilder = new BRepBuilderAPI_MakeEdge( startCADPoint.Point, endCADPoint.Point );
				if( !straightEdgeBuilder.IsDone() ) {
					return false;
				}
				edge = straightEdgeBuilder.Edge();
				return true;
			}
			edge = edgeBuilder.Edge();
			return true;
		}

		static bool DetermineIfClosed( List<CADPoint> cadPointList )
		{
			if( cadPointList == null || cadPointList.Count < 3 ) {
				return false;
			}

			gp_Pnt firstPoint = cadPointList[ 0 ].Point;
			gp_Pnt lastPoint = cadPointList[ cadPointList.Count - 1 ].Point;

			return firstPoint.Distance( lastPoint ) <= 0.001;
		}
	}

	public static class CircleFactory
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

	public static class RectangleFactory
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

	public static class RunwayFactory
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

	public static class PolygonFactory
	{
		public static bool CreatePolygonWireOnPlane( gp_Pnt centerPoint, int sides, double sideLength, double cornerRadius, gp_Pln plane, out TopoDS_Wire wire )
		{
			wire = null;

			// parameter validation
			if( sides < 3 || sides > 6 || sideLength <= 1e-9 || cornerRadius < 0 ) {
				return false;
			}

			// calculate circumradius R = sideLength / (2 * sin(π/n))
			double angleStep = 2.0 * Math.PI / sides;
			double radius = sideLength / ( 2.0 * Math.Sin( Math.PI / sides ) );

			// determine initial angle offset based on the number of sides
			double angleOffset = 0.0;
			switch( sides ) {
				case 3: // triangle: vertex points in the positive Y direction (90°)
					angleOffset = Math.PI / 2.0; // 90°
					break;
				case 4: // square: midpoint of the edge in the positive Y direction, rotate 45°
					angleOffset = Math.PI / 4.0; // 45°, edges vertical to Y-axis
					break;
				case 5: // pentagon: vertex points in the positive Y direction (90°)
					angleOffset = Math.PI / 2.0; // 90°
					break;
				case 6: // hexagon: midpoint of the edge in the positive Y direction, edges vertical to Y-axis
					angleOffset = 0.0; // 0°, hexagon default has edges vertical to Y-axis
					break;
				default:
					angleOffset = 0.0; // default start from the positive X direction
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

			// for regular polygons, the interior angle is fixed: (n-2) * π / n
			double interiorAngle = ( sides - 2 ) * Math.PI / sides;
			double halfAngle = interiorAngle / 2.0;
			double tanHalfAngle = Math.Tan( halfAngle );
			double sinHalfAngle = Math.Sin( halfAngle );

			if( tanHalfAngle < 1e-9 || sinHalfAngle < 1e-9 ) {
				return false;
			}

			// calculate tangent point distance and center distance (same for all vertices)
			double tangentDistance = cornerRadius / tanHalfAngle;
			double centerDistance = cornerRadius / sinHalfAngle;

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
				double currentEdgeLength = edgeVec.Magnitude();

				if( currentEdgeLength <= 1e-9 ) {
					return false;
				}

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

				// check if the shortened edge still has a valid length
				if( edgeStarts[ i ].Distance( edgeEnds[ i ] ) <= 1e-9 ) {
					return false;
				}
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

			if( vec1.Magnitude() < 1e-9 || vec2.Magnitude() < 1e-9 ) {
				return false;
			}

			vec1.Normalize();
			vec2.Normalize();

			// calculate bisector direction
			gp_Vec bisector = vec1.Added( vec2 );
			if( bisector.Magnitude() < 1e-9 ) {
				return false;
			}
			bisector.Normalize();

			// inscribed circle center: move inward along the bisector
			gp_Pnt arcCenter = new gp_Pnt(
				vertex.X() + bisector.X() * centerDistance,
				vertex.Y() + bisector.Y() * centerDistance,
				vertex.Z()
			);

			// verify if the distances from the center to the two tangent points are equal to the radius
			double dist1 = arcCenter.Distance( tangentPoint1 );
			double dist2 = arcCenter.Distance( tangentPoint2 );

			if( Math.Abs( dist1 - radius ) > 1e-3 || Math.Abs( dist2 - radius ) > 1e-3 ) {
				return false;
			}

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
