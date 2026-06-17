using OCC.Bnd;
using OCC.BOPTools;
using OCC.BRep;
using OCC.BRepAdaptor;
using OCC.BRepBndLib;
using OCC.BRepBuilderAPI;
using OCC.BRepGProp;
using OCC.ElCLib;
using OCC.gce;
using OCC.Geom;
using OCC.Geom2d;
using OCC.GeomAbs;
using OCC.GeomAdaptor;
using OCC.GeomLib;
using OCC.GeomLProp;
using OCC.gp;
using OCC.GProp;
using OCC.IntCurvesFace;
using OCC.Precision;
using OCC.ShapeAnalysis;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using System;
using System.Collections.Generic;

namespace OCCTool
{
	public enum IntersectType
	{
		Extend,
		ReverseExtend,
		Inbetween,
		NoIntersect
	}

	public class GeometryTool
	{
		public static gp_Pnt FindIntersectPoint( gp_Pnt p1, gp_Pnt p2, gp_Pnt p3, gp_Pnt p4,
			out IntersectType TypeL1, out IntersectType TypeL2 )
		{
			gp_Vec d1 = new gp_Vec( p2.XYZ() - p1.XYZ() ); // Direction vector of line 1
			gp_Vec d2 = new gp_Vec( p4.XYZ() - p3.XYZ() ); // Direction vector of line 2
			gp_Vec r = new gp_Vec( p1.XYZ() - p3.XYZ() );  // Vector between the origins of the two lines

			double a = d1.Dot( d1 ); // Dot product of d1 with itself
			double b = d1.Dot( d2 ); // Dot product of d1 with d2
			double c = d2.Dot( d2 ); // Dot product of d2 with itself
			double d = d1.Dot( r );  // Dot product of d1 with r
			double e = d2.Dot( r );  // Dot product of d2 with r

			// Denominator for solving t and s
			double denominator = a * c - b * b;
			if( Math.Abs( denominator ) < 1e-3 ) {
				TypeL1 = IntersectType.NoIntersect;
				TypeL2 = IntersectType.NoIntersect;
				return new gp_Pnt( ( p2.XYZ() + p4.XYZ() ) / 2 );
			}

			double t = ( b * e - c * d ) / denominator; // Parameter t for line 1
			double s = ( a * e - b * d ) / denominator; // Parameter s for line 2

			// Compute the closest points on both lines
			gp_Pnt closestPointOnLine1 = new gp_Pnt( p1.XYZ() + d1.XYZ() * t );
			gp_Pnt closestPointOnLine2 = new gp_Pnt( p3.XYZ() + d2.XYZ() * s );

			// Compute the midpoint of the shortest segment
			gp_Pnt midpoint = new gp_Pnt( ( closestPointOnLine1.XYZ() + closestPointOnLine2.XYZ() ) / 2 );
			const double TOLERANCE = 0.1;

			if( t <= -TOLERANCE ) {
				TypeL1 = IntersectType.ReverseExtend;
			}
			else if( t >= 1 + TOLERANCE ) {
				TypeL1 = IntersectType.Extend;
			}
			else {
				TypeL1 = IntersectType.Inbetween;
			}
			if( s <= -TOLERANCE ) {
				TypeL2 = IntersectType.ReverseExtend;
			}
			else if( s >= 1 + TOLERANCE ) {
				TypeL2 = IntersectType.Extend;
			}
			else {
				TypeL2 = IntersectType.Inbetween;
			}

			return midpoint;
		}

		public static bool IsApproximatelyLinear( TopoDS_Edge edge )
		{
			return IsApproximatelyLinear( edge, out _, out _ );
		}

		public static bool IsPlane( TopoDS_Face face, out gp_Pnt p, out gp_Dir dir )
		{
			p = new gp_Pnt();
			dir = new gp_Dir();
			if( face == null || face.IsNull() ) {
				return false;
			}
			BRepAdaptor_Surface surface = new BRepAdaptor_Surface( face );

			// normal plane
			if( surface.GetSurfaceType() == GeomAbs_SurfaceType.GeomAbs_Plane ) {
				return ExtractPlaneData( surface.Plane(), face, out p, out dir );
			}

			// other face type use planar checker to check
			try {
				double dTolerance = CalFaceFittingTolerance( face );
				Geom_Surface geomSurface = BRep_Tool.Surface( face );
				if( geomSurface == null || geomSurface.IsNull() ) {
					return false;
				}
				GeomLib_IsPlanarSurface planarChecker = new GeomLib_IsPlanarSurface( geomSurface, dTolerance );
				if( planarChecker.IsPlanar() ) {
					gp_Pln plane = planarChecker.Plan();
					return ExtractPlaneData( plane, face, out p, out dir );
				}
			}
			catch {
				p = new gp_Pnt();
				dir = new gp_Dir();
				return false;
			}
			return false;
		}

		public static bool IsAxialSymmetrySurface( TopoDS_Face face, out gp_Pnt p, out gp_Dir dir )
		{
			p = new gp_Pnt();
			dir = new gp_Dir();
			BRepAdaptor_Surface surface = new BRepAdaptor_Surface( face );
			if( surface.GetSurfaceType() == GeomAbs_SurfaceType.GeomAbs_Cylinder ) {
				p = surface.Cylinder().Location();
				dir = surface.Cylinder().Axis().Direction();
				return true;
			}
			else if( surface.GetSurfaceType() == GeomAbs_SurfaceType.GeomAbs_Cone ) {
				p = surface.Cone().Location();
				dir = surface.Cone().Axis().Direction();
				return true;
			}
			else if( surface.GetSurfaceType() == GeomAbs_SurfaceType.GeomAbs_Sphere ) {
				p = surface.Sphere().Location();
				dir = surface.Sphere().Position().Direction();
				return true;
			}
			else if( surface.GetSurfaceType() == GeomAbs_SurfaceType.GeomAbs_Torus ) {
				p = surface.Torus().Location();
				dir = surface.Torus().Axis().Direction();
				return true;
			}
			else if( surface.GetSurfaceType() == GeomAbs_SurfaceType.GeomAbs_SurfaceOfRevolution ) {
				p = surface.AxeOfRevolution().Location();
				dir = surface.AxeOfRevolution().Direction();
				return true;
			}
			else if( surface.GetSurfaceType() == GeomAbs_SurfaceType.GeomAbs_BSplineSurface ) {
				Geom_Surface geomSurface = BRep_Tool.Surface( face );
				if( geomSurface is Geom_BSplineSurface bsplineSurface ) {
					return TryFitRotationAxisToBSplineSurface( face, bsplineSurface, out p, out dir );
				}
				return false;
			}
			else if( surface.GetSurfaceType() == GeomAbs_SurfaceType.GeomAbs_BezierSurface ) {
				Geom_Surface geomSurface = BRep_Tool.Surface( face );
				if( geomSurface is Geom_BezierSurface bezierSurface ) {
					return TryFitRotationAxisToBezierSurface( face, bezierSurface, out p, out dir );
				}
				return false;
			}
			else {
				return false;
			}
		}

		public static bool IsLine( TopoDS_Edge edge, out gp_Pnt p, out gp_Dir dir )
		{
			p = new gp_Pnt();
			dir = new gp_Dir();
			BRepAdaptor_Curve curve = new BRepAdaptor_Curve( edge );
			if( curve.GetCurveType() == GeomAbs_CurveType.GeomAbs_Line ) {
				p = curve.Line().Location();
				dir = curve.Line().Direction();
				return true;
			}
			else if( IsApproximatelyLinear( edge, out gp_Pnt p2, out gp_Pnt p1 ) ) {
				p = p1;
				dir = new gp_Dir( new gp_Vec( p1, p2 ) );
				return true;
			}
			else {
				return false;
			}
		}

		public static bool IsCircularArc( TopoDS_Edge edge, out gp_Pnt p, out double r, out gp_Dir dir )
		{
			p = new gp_Pnt();
			dir = new gp_Dir();
			r = 0;
			BRepAdaptor_Curve adCurve = new BRepAdaptor_Curve( edge );
			if( adCurve.GetCurveType() == GeomAbs_CurveType.GeomAbs_Circle ) {
				p = adCurve.Circle().Location();
				dir = adCurve.Circle().Axis().Direction();
				r = adCurve.Circle().Radius();
				return true;
			}
			double dStartU = 0;
			double dEndU = 0;
			Geom_Curve geomCurve = BRep_Tool.Curve( edge, ref dStartU, ref dEndU );
			Geom_Circle geom_Circle = ComputeCircle( geomCurve, 1e-1, dStartU, dEndU,
				out double cf, out double cl, out double deviation );
			if( geom_Circle != null ) {
				p = geom_Circle.Location();
				dir = geom_Circle.Axis().Direction();
				r = geom_Circle.Radius();
				return true;
			}
			return false;
		}

		public static bool GetEdgeMidPoint( TopoDS_Edge edge, out gp_Pnt mid )
		{
			mid = new gp_Pnt();
			if( edge == null || edge.IsNull() ) {
				return false;
			}
			BRepAdaptor_Curve curve = new BRepAdaptor_Curve( edge );
			double dStartU = curve.FirstParameter();
			double dEndU = curve.LastParameter();
			double dMidU = ( dStartU + dEndU ) / 2;
			mid = curve.Value( dMidU );
			return true;
		}

		public static bool GetTwoVertexMidPoint( TopoDS_Vertex v1, TopoDS_Vertex v2, out gp_Pnt mid )
		{
			mid = new gp_Pnt();
			if( v1 == null || v1.IsNull() || v2 == null || v2.IsNull() || v1.IsEqual( v2 ) ) {
				return false;
			}
			gp_Pnt p1 = BRep_Tool.Pnt( v1 );
			gp_Pnt p2 = BRep_Tool.Pnt( v2 );
			mid = new gp_Pnt( ( p1.XYZ() + p2.XYZ() ) / 2 );
			return true;
		}

		public static bool IsD1Cont( TopoDS_Face f1, TopoDS_Face f2, TopoDS_Edge sharingEdge )
		{
			if( f1 == null || f2 == null || sharingEdge == null
				|| f1.IsNull() || f2.IsNull() || sharingEdge.IsNull() ) {
				return false;
			}

			// check if the edge is shared by both faces
			if( !IsEdgeBelongFace( sharingEdge, f1 ) || !IsEdgeBelongFace( sharingEdge, f2 ) ) {
				return false;
			}

			// get the middle point on the edge
			double dStartU = 0;
			double dEndU = 0;
			Geom_Curve oneGeomCurve = BRep_Tool.Curve( sharingEdge, ref dStartU, ref dEndU );
			gp_Pnt pMiddle = oneGeomCurve.Value( ( dStartU + dEndU ) / 2 );

			// get the surface normals at the middle point on both faces
			gp_Dir d1 = VectorTool.GetFaceNormalVec( f1, pMiddle );
			gp_Dir d2 = VectorTool.GetFaceNormalVec( f2, pMiddle );

			// check if the normals are parallel or reversed
			double dTolerance = 0.087;
			return d1.IsParallel( d2, dTolerance );
		}

		public static bool IsEdgeBelongFace( TopoDS_Edge edge, TopoDS_Face face )
		{
			if( edge == null || face == null || edge.IsNull() || face.IsNull() ) {
				return false;
			}

			// check if the edge is shared by the face
			TopExp_Explorer exp = new TopExp_Explorer( face, TopAbs_ShapeEnum.TopAbs_EDGE );
			for( ; exp.More(); exp.Next() ) {
				if( exp.Current().IsSame( edge ) ) {
					return true;
				}
			}
			return false;
		}

		public static bool CreateLineFromTwoVertex( TopoDS_Vertex v1, TopoDS_Vertex v2, out TopoDS_Edge edge )
		{
			edge = new TopoDS_Edge();
			if( v1 == null || v1.IsNull() || v2 == null || v2.IsNull() || v1.IsEqual( v2 ) ) {
				return false;
			}
			gp_Pnt p1 = BRep_Tool.Pnt( v1 );
			gp_Pnt p2 = BRep_Tool.Pnt( v2 );
			BRepBuilderAPI_MakeEdge brepEdge = new BRepBuilderAPI_MakeEdge( p1, p2 );
			if( !brepEdge.IsDone() ) {
				return false;
			}
			edge = brepEdge.Edge();
			return true;
		}

		public static List<gp_Ax1> BuildRayAxesFromBBox( gp_Dir rayDir, BoundingBox bbox )
		{
			if( rayDir == null || bbox == null ) {
				return new List<gp_Ax1>();
			}

			// rayDir must be a principal axis (X, Y, or Z)
			bool isAlongX = rayDir.IsParallel( new gp_Dir( 1, 0, 0 ), 1e-3 );
			bool isAlongY = rayDir.IsParallel( new gp_Dir( 0, 1, 0 ), 1e-3 );
			bool isAlongZ = rayDir.IsParallel( new gp_Dir( 0, 0, 1 ), 1e-3 );
			if( isAlongX == false && isAlongY == false && isAlongZ == false ) {
				return new List<gp_Ax1>();
			}

			// Build ray strategies from bbox center toward 4 corner midpoints along the given axis.
			// The two axes perpendicular to rayDir are used to offset origin toward bbox corners.
			// Strategy order: center → mid-to-right-upper → mid-to-left-upper → mid-to-left-lower → mid-to-right-lower
			double cx = bbox.XCenter;
			double cy = bbox.YCenter;
			double cz = bbox.ZCenter;
			double xMin = bbox.Xmin, xMax = bbox.Xmax;
			double yMin = bbox.Ymin, yMax = bbox.Ymax;
			double zMin = bbox.Zmin, zMax = bbox.Zmax;

			// Determine the two axes perpendicular to rayDir
			// and generate 4 corner midpoint candidates on that plane
			double ox1, oy1, oz1; // right-upper
			double ox2, oy2, oz2; // left-upper
			double ox3, oy3, oz3; // left-lower
			double ox4, oy4, oz4; // right-lower

			if( isAlongX ) {
				// along X, perpendicular plane: Y-Z
				ox1 = cx;
				oy1 = ( cy + yMax ) / 2.0;
				oz1 = ( cz + zMax ) / 2.0;
				ox2 = cx;
				oy2 = ( cy + yMin ) / 2.0;
				oz2 = ( cz + zMax ) / 2.0;
				ox3 = cx;
				oy3 = ( cy + yMin ) / 2.0;
				oz3 = ( cz + zMin ) / 2.0;
				ox4 = cx;
				oy4 = ( cy + yMax ) / 2.0;
				oz4 = ( cz + zMin ) / 2.0;
			}
			else if( isAlongY ) {
				// along Y, perpendicular plane: X-Z
				ox1 = ( cx + xMax ) / 2.0;
				oy1 = cy;
				oz1 = ( cz + zMax ) / 2.0;
				ox2 = ( cx + xMin ) / 2.0;
				oy2 = cy;
				oz2 = ( cz + zMax ) / 2.0;
				ox3 = ( cx + xMin ) / 2.0;
				oy3 = cy;
				oz3 = ( cz + zMin ) / 2.0;
				ox4 = ( cx + xMax ) / 2.0;
				oy4 = cy;
				oz4 = ( cz + zMin ) / 2.0;
			}
			else {
				// along Z, perpendicular plane: X-Y
				ox1 = ( cx + xMax ) / 2.0;
				oy1 = ( cy + yMax ) / 2.0;
				oz1 = cz;
				ox2 = ( cx + xMin ) / 2.0;
				oy2 = ( cy + yMax ) / 2.0;
				oz2 = cz;
				ox3 = ( cx + xMin ) / 2.0;
				oy3 = ( cy + yMin ) / 2.0;
				oz3 = cz;
				ox4 = ( cx + xMax ) / 2.0;
				oy4 = ( cy + yMin ) / 2.0;
				oz4 = cz;
			}

			return new List<gp_Ax1>
			{
				new gp_Ax1( new gp_Pnt( cx,  cy,  cz  ), rayDir ), // center
				new gp_Ax1( new gp_Pnt( ox1, oy1, oz1 ), rayDir ), // mid to right-upper
				new gp_Ax1( new gp_Pnt( ox2, oy2, oz2 ), rayDir ), // mid to left-upper
				new gp_Ax1( new gp_Pnt( ox3, oy3, oz3 ), rayDir ), // mid to left-lower
				new gp_Ax1( new gp_Pnt( ox4, oy4, oz4 ), rayDir ), // mid to right-lower
			};
		}

		// Check if a ray (defined by origin and direction) can potentially intersect a bounding box.
		public static bool RayIntersectsBBox( gp_Pnt rayOrigin, gp_Dir rayDir, BoundingBox bbox )
		{
			if( rayOrigin == null || rayDir == null || bbox == null ) {
				return false;
			}

			bool isAlongX = rayDir.IsParallel( new gp_Dir( 1, 0, 0 ), 1e-3 );
			bool isAlongY = rayDir.IsParallel( new gp_Dir( 0, 1, 0 ), 1e-3 );
			bool isAlongZ = rayDir.IsParallel( new gp_Dir( 0, 0, 1 ), 1e-3 );
			if( isAlongX == false && isAlongY == false && isAlongZ == false ) {
				return false;
			}

			if( isAlongX ) {
				// along X
				return rayOrigin.Y() >= bbox.Ymin && rayOrigin.Y() <= bbox.Ymax
					&& rayOrigin.Z() >= bbox.Zmin && rayOrigin.Z() <= bbox.Zmax;
			}
			else if( isAlongY ) {
				// along Y
				return rayOrigin.X() >= bbox.Xmin && rayOrigin.X() <= bbox.Xmax
					&& rayOrigin.Z() >= bbox.Zmin && rayOrigin.Z() <= bbox.Zmax;
			}
			else {
				// along Z
				return rayOrigin.X() >= bbox.Xmin && rayOrigin.X() <= bbox.Xmax
					&& rayOrigin.Y() >= bbox.Ymin && rayOrigin.Y() <= bbox.Ymax;
			}
		}

		public static bool FindOutermostFaceAlongPrincipalAxis( List<TopoDS_Face> faceList, gp_Pnt rayOrigin, gp_Dir rayDir, gp_Pnt referenceCenter, out TopoDS_Face outermostFace )
		{
			outermostFace = null;
			if( faceList == null || rayOrigin == null || rayDir == null || referenceCenter == null ) {
				return false;
			}

			gp_Lin ray = new gp_Lin( rayOrigin, rayDir );

			bool isAlongX = rayDir.IsParallel( new gp_Dir( 1, 0, 0 ), 1e-3 );
			bool isAlongY = rayDir.IsParallel( new gp_Dir( 0, 1, 0 ), 1e-3 );
			bool isAlongZ = rayDir.IsParallel( new gp_Dir( 0, 0, 1 ), 1e-3 );
			if( isAlongX == false && isAlongY == false && isAlongZ == false ) {
				return false;
			}

			double maxDistance = -1.0;

			foreach( TopoDS_Face face in faceList ) {
				BoundingBox bbox = new BoundingBox( face );

				// Phase 1: BBox pre-filter on the two axes perpendicular to rayDir
				bool passes;
				if( isAlongX ) {
					passes = rayOrigin.Y() >= bbox.Ymin && rayOrigin.Y() <= bbox.Ymax
						&& rayOrigin.Z() >= bbox.Zmin && rayOrigin.Z() <= bbox.Zmax;
				}
				else if( isAlongY ) {
					passes = rayOrigin.X() >= bbox.Xmin && rayOrigin.X() <= bbox.Xmax
						&& rayOrigin.Z() >= bbox.Zmin && rayOrigin.Z() <= bbox.Zmax;
				}
				else {
					// along Z
					passes = rayOrigin.X() >= bbox.Xmin && rayOrigin.X() <= bbox.Xmax
						&& rayOrigin.Y() >= bbox.Ymin && rayOrigin.Y() <= bbox.Ymax;
				}

				if( passes == false ) {
					continue;
				}

				// Phase 2: Precise intersection
				IntCurvesFace_ShapeIntersector intersector = new IntCurvesFace_ShapeIntersector();
				intersector.Load( face, 1e-6 );
				intersector.Perform( ray, double.MinValue, double.MaxValue );

				if( intersector.IsDone() == false || intersector.NbPnt() == 0 ) {
					continue;
				}

				// Phase 3: Find the intersection point farthest from the reference center along the ray axis
				for( int i = 1; i <= intersector.NbPnt(); i++ ) {
					gp_Pnt pnt = intersector.Pnt( i );
					double distance;
					if( isAlongX ) {
						distance = Math.Abs( pnt.X() - referenceCenter.X() );
					}
					else if( isAlongY ) {
						distance = Math.Abs( pnt.Y() - referenceCenter.Y() );
					}
					else {
						distance = Math.Abs( pnt.Z() - referenceCenter.Z() );
					}

					if( distance > maxDistance ) {
						maxDistance = distance;
						outermostFace = face;
					}
				}
			}
			return outermostFace != null;
		}

		// private
		static bool IsApproximatelyLinear( TopoDS_Edge edge, out gp_Pnt p2, out gp_Pnt p1 )
		{
			// get edge length
			GProp_GProps system = new GProp_GProps();
			BRepGProp.LinearProperties( edge, ref system );
			double dEdgeLength = system.Mass();

			// get distance between start and end points of edge
			TopoDS_Vertex v1 = new TopoDS_Vertex();
			TopoDS_Vertex v2 = new TopoDS_Vertex();
			ShapeAnalysis.FindBounds( edge, ref v1, ref v2 );
			p1 = BRep_Tool.Pnt( v1 );
			p2 = BRep_Tool.Pnt( v2 );
			double dDistance = p1.Distance( p2 );

			// compare edge length and distance (arc and sine)
			const double dToleranceRatio = 1e-1;
			if( Math.Abs( ( dEdgeLength - dDistance ) / dDistance ) < dToleranceRatio ) {
				return true;
			}
			else {
				return false;
			}
		}

		static bool ExtractPlaneData( gp_Pln plane, TopoDS_Face face, out gp_Pnt p, out gp_Dir dir )
		{
			p = new gp_Pnt();
			dir = new gp_Dir();

			// check face is valid
			if( face == null || face.IsNull() || plane == null ) {
				return false;
			}

			// get plane location
			p = plane.Location();
			if( p == null ) {
				p = new gp_Pnt();
				return false;
			}

			// get plane normal direction
			gp_Ax1 axis = plane.Axis();
			if( axis == null ) {
				return false;
			}
			dir = axis.Direction();
			if( dir == null ) {
				dir = new gp_Dir();
				return false;
			}

			// reverse direction if face orientation is reversed
			if( face.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				dir.Reverse();
			}
			return true;
		}

		static double CalFaceFittingTolerance( TopoDS_Face face )
		{
			const double DEFAULT_TOLERANCE = 1e-3;
			if( face == null || face.IsNull() ) {
				return DEFAULT_TOLERANCE;
			}
			BoundingBox bbox = new BoundingBox( face );
			double dCharacteristicSize = Math.Max(
				Math.Max( bbox.XLength, bbox.YLength ),
				bbox.ZLength
			);

			// check if characteristic size is valid
			if( dCharacteristicSize <= 0 || double.IsNaN( dCharacteristicSize ) || double.IsInfinity( dCharacteristicSize ) ) {
				return DEFAULT_TOLERANCE;
			}
			double dTolerance = dCharacteristicSize * 0.01;
			if( dTolerance < 0 || dTolerance == double.NaN || dTolerance == double.PositiveInfinity ) {
				return DEFAULT_TOLERANCE;
			}
			return dCharacteristicSize * 0.01;
		}

		static Geom_Circle ComputeCircle(
		Geom_Curve c3d,
		double tol,
		double c1,
		double c2,
		out double cf,
		out double cl,
		out double deviation )
		{
			cf = c1;
			cl = c2;
			deviation = 0.0;

			// If already a circle
			GeomAdaptor_Curve adC = new GeomAdaptor_Curve( c3d );
			if( adC.GetCurveType() == GeomAbs_CurveType.GeomAbs_Circle ) {
				deviation = 0.0;
				return Geom_Circle.DownCast( c3d );
			}

			// Sample 3 points
			gp_Pnt P0 = c3d.Value( c1 );
			gp_Pnt P1 = c3d.Value( ( 2 * c1 + c2 ) / 3.0 );
			gp_Pnt P2 = c3d.Value( ( c1 + 2 * c2 ) / 3.0 );

			// Try to make a circle from these points
			if( !GetCircle( out gp_Circ gpCircle, P0, P1, P2 ) ) {
				return null;
			}

			// Validate that all points on the curve lie close to the circle
			double maxDeviation = 0.0;
			int nbSamples = 20;
			double du = ( c2 - c1 ) / nbSamples;
			for( int i = 0; i <= nbSamples; ++i ) {
				double u = c1 + i * du;
				gp_Pnt pt = c3d.Value( u );
				double dist = gpCircle.Distance( pt );
				if( dist > tol ) {
					return null; // too far, not a circle
				}
				if( dist > maxDeviation ) {
					maxDeviation = dist;
				}
			}
			deviation = maxDeviation;

			// Set parameter values on the circle
			double PI2 = 2.0 * Math.PI;
			cf = ElCLib.Parameter( gpCircle, c3d.Value( c1 ) );
			cf = ElCLib.InPeriod( cf, 0.0, PI2 );
			if( Math.Abs( cf ) < Precision.Confusion() || Math.Abs( PI2 - cf ) < Precision.Confusion() ) {
				cf = 0.0;
			}
			double cm = ElCLib.Parameter( gpCircle, c3d.Value( ( c1 + c2 ) / 2.0 ) );
			cm = ElCLib.InPeriod( cm, cf, cf + PI2 );
			cl = ElCLib.Parameter( gpCircle, c3d.Value( c2 ) );
			cl = ElCLib.InPeriod( cl, cm, cm + PI2 );
			return new Geom_Circle( gpCircle );
		}

		static bool GetCircle( out gp_Circ crc, gp_Pnt P0, gp_Pnt P1, gp_Pnt P2 )
		{
			crc = null;

			// Step 1: Check point coordinates against "infinite"
			double maxCoord = Math.Sqrt( Precision.Infinite() );
			foreach( var p in new[] { P0, P1, P2 } ) {
				if( Math.Abs( p.X() ) > maxCoord || Math.Abs( p.Y() ) > maxCoord || Math.Abs( p.Z() ) > maxCoord ) {
					return false;
				}
			}

			// Step 2: Try to make the circle using 3 points
			var mkc = new gce_MakeCirc( P0, P1, P2 );
			if( !mkc.IsDone() ) {
				return false;
			}
			crc = mkc.Value();

			// Step 3: Reject if radius is too small
			if( crc.Radius() < gp.Resolution() ) {
				return false;
			}

			// Step 4: Reorient circle so X direction points from center to P0
			gp_Pnt center = crc.Location();
			gp_Ax2 axis = crc.Position();
			gp_Vec vx = new gp_Vec( center, P0 );
			axis.SetXDirection( new gp_Dir( vx ) );
			crc.SetPosition( axis );

			return true;
		}

		#region BSpline Revolution Surface Detection

		const double ROTATION_AXIS_TOLERANCE = 1e-2;

		delegate void SurfaceD1Evaluator( double u, double v, ref gp_Pnt pnt, ref gp_Vec d1u, ref gp_Vec d1v );

		static bool TryFitRotationAxisToParametricSurface(
			TopoDS_Face face,
			int nU,
			int nV,
			double uMin,
			double uMax,
			double vMin,
			double vMax,
			SurfaceD1Evaluator surfaceEvaluator,
			out gp_Pnt p,
			out gp_Dir dir )
		{
			p = new gp_Pnt();
			dir = new gp_Dir();

			try {
				// get sample points
				List<gp_Pnt> points = new List<gp_Pnt>();
				List<gp_Dir> normals = new List<gp_Dir>();

				for( int i = 0; i < nU; i++ ) {
					double u = uMin + ( uMax - uMin ) * i / ( nU - 1 );
					for( int j = 0; j < nV; j++ ) {
						double v = vMin + ( vMax - vMin ) * j / ( nV - 1 );

						gp_Pnt pnt = new gp_Pnt();
						gp_Vec d1u = new gp_Vec();
						gp_Vec d1v = new gp_Vec();
						surfaceEvaluator( u, v, ref pnt, ref d1u, ref d1v );
						gp_Vec normal = d1u.Crossed( d1v );

						// filter singular points where normal cannot be defined
						if( normal.Magnitude() > 1e-10 ) {
							normal.Normalize();
							points.Add( pnt );
							normals.Add( new gp_Dir( normal ) );
						}
					}
				}
				if( points.Count < 4 ) {
					return false;
				}

				// find intersection points of non-parallel normal lines
				List<gp_Pnt> intersections = new List<gp_Pnt>();
				double diagonalLength = GetFaceBoundingBoxDiagonal( face );

				// 1% of bounding box diagonal
				double maxDistance = diagonalLength * 0.01;

				for( int i = 0; i < normals.Count - 1; i++ ) {
					for( int j = i + 1; j < normals.Count; j++ ) {

						// skip parallel normals (not useful)
						double dotProduct = Math.Abs( normals[ i ].Dot( normals[ j ] ) );
						if( 1.0 - dotProduct < 1e-6 ) {
							continue;
						}

						// compute closest points between two normal lines
						var closestPoints = ComputeClosestPointsBetween3DLine( points[ i ], normals[ i ], points[ j ], normals[ j ] );
						if( closestPoints.Success ) {
							double distance = closestPoints.Point1.Distance( closestPoints.Point2 );

							// if the lines nearly intersect, record the midpoint
							if( distance < maxDistance ) {
								gp_Pnt midPoint = new gp_Pnt(
									( closestPoints.Point1.X() + closestPoints.Point2.X() ) / 2.0,
									( closestPoints.Point1.Y() + closestPoints.Point2.Y() ) / 2.0,
									( closestPoints.Point1.Z() + closestPoints.Point2.Z() ) / 2.0
								);
								intersections.Add( midPoint );
							}
						}
					}
				}

				// need sufficient intersection points to fit a reliable axis
				if( intersections.Count < 4 ) {
					return false;
				}

				// fit a line in 3D point
				if( !FitLineToPoints( intersections, out gp_Dir axisDir, out gp_Pnt axisPnt, out double error ) ) {
					return false;
				}

				// normalize error relative to face size
				double errorNormalized = error / diagonalLength;

				// check if the fitted axis is within acceptable tolerance
				if( errorNormalized < ROTATION_AXIS_TOLERANCE ) {
					p = axisPnt;
					dir = axisDir;
					return true;
				}
				return false;
			}
			catch {
				return false;
			}
		}

		static bool TryFitRotationAxisToBSplineSurface( TopoDS_Face face, Geom_BSplineSurface bsplineSurface, out gp_Pnt p, out gp_Dir dir )
		{
			int nU = Math.Max( 5, Math.Min( 10, bsplineSurface.NbUPoles() / 2 ) );
			int nV = Math.Max( 5, Math.Min( 10, bsplineSurface.NbVPoles() / 2 ) );

			double uMin = bsplineSurface.UKnot( 1 );
			double uMax = bsplineSurface.UKnot( bsplineSurface.NbUKnots() );
			double vMin = bsplineSurface.VKnot( 1 );
			double vMax = bsplineSurface.VKnot( bsplineSurface.NbVKnots() );

			return TryFitRotationAxisToParametricSurface(
				face, nU, nV, uMin, uMax, vMin, vMax,
				bsplineSurface.D1,
				out p, out dir );
		}

		static bool TryFitRotationAxisToBezierSurface( TopoDS_Face face, Geom_BezierSurface bezierSurface, out gp_Pnt p, out gp_Dir dir )
		{
			int nU = Math.Max( 5, Math.Min( 10, bezierSurface.NbUPoles() ) );
			int nV = Math.Max( 5, Math.Min( 10, bezierSurface.NbVPoles() ) );

			double uMin = 0.0;
			double uMax = 1.0;
			double vMin = 0.0;
			double vMax = 1.0;

			return TryFitRotationAxisToParametricSurface(
				face, nU, nV, uMin, uMax, vMin, vMax,
				bezierSurface.D1,
				out p, out dir );
		}

		static bool FitLineToPoints( List<gp_Pnt> points, out gp_Dir direction, out gp_Pnt point, out double error )
		{
			direction = null;
			point = new gp_Pnt();
			error = double.PositiveInfinity;

			if( points.Count < 2 ) {
				return false;
			}

			double sumX = 0, sumY = 0, sumZ = 0;
			foreach( var p in points ) {
				sumX += p.X();
				sumY += p.Y();
				sumZ += p.Z();
			}

			gp_Pnt centroid = new gp_Pnt(
				sumX / points.Count,
				sumY / points.Count,
				sumZ / points.Count
			);

			double[,] covMatrix = new double[ 3, 3 ];
			foreach( var p in points ) {
				double dx = p.X() - centroid.X();
				double dy = p.Y() - centroid.Y();
				double dz = p.Z() - centroid.Z();

				covMatrix[ 0, 0 ] += dx * dx;
				covMatrix[ 0, 1 ] += dx * dy;
				covMatrix[ 0, 2 ] += dx * dz;
				covMatrix[ 1, 0 ] += dy * dx;
				covMatrix[ 1, 1 ] += dy * dy;
				covMatrix[ 1, 2 ] += dy * dz;
				covMatrix[ 2, 0 ] += dz * dx;
				covMatrix[ 2, 1 ] += dz * dy;
				covMatrix[ 2, 2 ] += dz * dz;
			}

			var eigenResult = ComputeLargestEigenvector3x3( covMatrix );
			if( !eigenResult.Success ) {
				return false;
			}

			direction = new gp_Dir( eigenResult.Vx, eigenResult.Vy, eigenResult.Vz );
			point = centroid;
			error = CalculateFittingError( points, centroid, direction );
			return true;
		}

		static (bool Success, gp_Pnt Point1, gp_Pnt Point2) ComputeClosestPointsBetween3DLine( gp_Pnt p1, gp_Dir dir1, gp_Pnt p2, gp_Dir dir2 )
		{
			try {
				double p1_x = p1.X(), p1_y = p1.Y(), p1_z = p1.Z();
				double u1_x = dir1.X(), u1_y = dir1.Y(), u1_z = dir1.Z();
				double p2_x = p2.X(), p2_y = p2.Y(), p2_z = p2.Z();
				double u2_x = dir2.X(), u2_y = dir2.Y(), u2_z = dir2.Z();

				double t1_t1_coef = u1_x * u1_x + u1_y * u1_y + u1_z * u1_z;
				double t1_t2_coef = -2.0 * ( u1_x * u2_x + u1_y * u2_y + u1_z * u2_z );
				double t2_t2_coef = u2_x * u2_x + u2_y * u2_y + u2_z * u2_z;

				double t1_coef = 2.0 * ( p1_x * u1_x + p1_y * u1_y + p1_z * u1_z - p2_x * u1_x - p2_y * u1_y - p2_z * u1_z );
				double t2_coef = 2.0 * ( -p1_x * u2_x - p1_y * u2_y - p1_z * u2_z + p2_x * u2_x + p2_y * u2_y + p2_z * u2_z );

				double a11 = 2.0 * t1_t1_coef;
				double a12 = t1_t2_coef;
				double a21 = t1_t2_coef;
				double a22 = 2.0 * t2_t2_coef;

				double det = a11 * a22 - a12 * a21;
				if( Math.Abs( det ) < 1e-10 ) {
					return (false, null, null);
				}

				double t1 = -( t1_coef * a22 - t2_coef * a12 ) / det;
				double t2 = -( a11 * t2_coef - a21 * t1_coef ) / det;

				gp_Pnt point1 = new gp_Pnt( p1_x + t1 * u1_x, p1_y + t1 * u1_y, p1_z + t1 * u1_z );
				gp_Pnt point2 = new gp_Pnt( p2_x + t2 * u2_x, p2_y + t2 * u2_y, p2_z + t2 * u2_z );

				return (true, point1, point2);
			}
			catch {
				return (false, null, null);
			}
		}

		static double CalculateFittingError( List<gp_Pnt> points, gp_Pnt axisPnt, gp_Dir axisDir )
		{
			double sumError = 0;
			foreach( var pt in points ) {
				gp_Vec v = new gp_Vec( axisPnt, pt );
				double projection = v.Dot( new gp_Vec( axisDir ) );
				gp_Pnt projectedPnt = axisPnt.Translated( new gp_Vec( axisDir ).Multiplied( projection ) );
				sumError += pt.Distance( projectedPnt );
			}
			return sumError / points.Count;
		}

		static (bool Success, double Vx, double Vy, double Vz) ComputeLargestEigenvector3x3( double[,] A )
		{
			try {
				double vx = 1.0, vy = 0.0, vz = 0.0;

				for( int iter = 0; iter < 30; iter++ ) {
					double newVx = A[ 0, 0 ] * vx + A[ 0, 1 ] * vy + A[ 0, 2 ] * vz;
					double newVy = A[ 1, 0 ] * vx + A[ 1, 1 ] * vy + A[ 1, 2 ] * vz;
					double newVz = A[ 2, 0 ] * vx + A[ 2, 1 ] * vy + A[ 2, 2 ] * vz;

					double mag = Math.Sqrt( newVx * newVx + newVy * newVy + newVz * newVz );
					if( mag < 1e-10 ) {
						return (false, 0, 0, 0);
					}

					vx = newVx / mag;
					vy = newVy / mag;
					vz = newVz / mag;
				}

				return (true, vx, vy, vz);
			}
			catch {
				return (false, 0, 0, 0);
			}
		}

		static double GetFaceBoundingBoxDiagonal( TopoDS_Face face )
		{
			Bnd_Box box = new Bnd_Box();
			BRepBndLib.Add( face, ref box );

			double xMin = 0.0, yMin = 0.0, zMin = 0.0, xMax = 0.0, yMax = 0.0, zMax = 0.0;
			box.Get( ref xMin, ref yMin, ref zMin, ref xMax, ref yMax, ref zMax );

			double dx = xMax - xMin;
			double dy = yMax - yMin;
			double dz = zMax - zMin;

			return Math.Sqrt( dx * dx + dy * dy + dz * dz );
		}

		#endregion

		#region Face Topology Analysis

		public static List<TopoDS_Face> FindD1ContinuousFaces(
			List<TopoDS_Face> seedFaces,
			TopTools_IndexedDataMapOfShapeListOfShape edgeFaceMap )
		{
			List<TopoDS_Face> result = new List<TopoDS_Face>();

			if( seedFaces == null || seedFaces.Count == 0 || edgeFaceMap == null ) {
				return result;
			}

			List<TopoDS_Face> pendingFaces = new List<TopoDS_Face>( seedFaces );
			List<TopoDS_Face> allD1ContinuousFaceList = new List<TopoDS_Face>( seedFaces );
			TopTools_MapOfShape visitedFaceMap = new TopTools_MapOfShape();
			TopTools_MapOfShape visitedEdgeMap = new TopTools_MapOfShape();

			foreach( TopoDS_Face oneFace in seedFaces ) {
				visitedFaceMap.Add( oneFace );
			}

			while( pendingFaces.Count > 0 ) {
				TopoDS_Face currentFace = pendingFaces[ 0 ];
				pendingFaces.RemoveAt( 0 );

				// get all edges of the current face
				List<TopoDS_Edge> edgeList = new List<TopoDS_Edge>();
				TopExp_Explorer exp = new TopExp_Explorer( currentFace, TopAbs_ShapeEnum.TopAbs_EDGE );
				for( ; exp.More(); exp.Next() ) {
					if( visitedEdgeMap.Contains( exp.Current() ) ) {
						continue;
					}
					edgeList.Add( TopoDS.ToEdge( exp.Current() ) );
					visitedEdgeMap.Add( exp.Current() );
				}

				// find all D1 continuous faces
				foreach( TopoDS_Edge oneEdge in edgeList ) {
					foreach( TopoDS_Shape _oneConnectedFace in edgeFaceMap.FindFromKey( oneEdge ) ) {

						// check visited
						if( visitedFaceMap.Contains( _oneConnectedFace ) ) {
							continue;
						}

						// check D1 continuity
						TopoDS_Face oneConnectedFace = TopoDS.ToFace( _oneConnectedFace );
						if( IsD1Cont( currentFace, oneConnectedFace, oneEdge ) ) {
							visitedFaceMap.Add( oneConnectedFace );
							allD1ContinuousFaceList.Add( oneConnectedFace );
							pendingFaces.Add( oneConnectedFace );
						}
					}
				}
			}
			return allD1ContinuousFaceList;
		}


		public static gp_Dir GetSurfaceNormal( TopoDS_Edge edge, TopoDS_Face face, double param )
		{
			// step 1: safety check
			if( edge == null || edge.IsNull() || face == null || face.IsNull() ) {
				return null;
			}

			// step 2: get face surface
			Geom_Surface surf = BRep_Tool.Surface( face );
			if( surf == null || surf.IsNull() ) {
				return null;
			}

			// step 3: create adaptor with (edge, face)
			// this ensures correct parameter range and direction
			BRepAdaptor_Curve adC = new BRepAdaptor_Curve( edge, face );

			// step 4: calculate uv parameters
			double u, v;
			double first2d = 0, last2d = 0;

			// try to get pcurve safely
			Geom2d_Curve pcurve = null;
			bool hasPCurve = false;

			pcurve = BRep_Tool.CurveOnSurface( edge, face, ref first2d, ref last2d );
			hasPCurve = ( pcurve != null && !pcurve.IsNull() );

			if( hasPCurve ) {
				// method a: use pcurve (more accurate)
				gp_Pnt2d uv = pcurve.Value( param );
				u = uv.X();
				v = uv.Y();
			}
			else {
				// method b: inverse calc uv (from 3d point)
				// using adaptor that considers face direction
				gp_Pnt pt = adC.Value( param );
				ShapeAnalysis_Surface sas = new ShapeAnalysis_Surface( surf );
				gp_Pnt2d uv = sas.ValueOfUV( pt, Precision.Confusion() );
				u = uv.X();
				v = uv.Y();
			}

			// step 5: calculate normal vector
			GeomLProp_SLProps props = new GeomLProp_SLProps( surf, u, v, 1, Precision.Confusion() );
			if( props.IsNormalDefined() ) {
				gp_Dir normal = props.Normal();

				// adjust normal based on face orientation
				if( face.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
					normal.Reverse();
				}

				return normal;
			}

			// step 6: fallback method, suppose not getting a better result, just in case
			gp_Dir fallback = new gp_Dir();
			BOPTools_AlgoTools3D.GetNormalToFaceOnEdge( edge, face, param, ref fallback );

			if( face.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				fallback.Reverse();
			}
			return fallback;
		}

		#endregion
	}
}
