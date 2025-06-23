using OCC.BRep;
using OCC.BRepAdaptor;
using OCC.BRepGProp;
using OCC.ElCLib;
using OCC.gce;
using OCC.Geom;
using OCC.GeomAbs;
using OCC.GeomAdaptor;
using OCC.gp;
using OCC.GProp;
using OCC.Precision;
using OCC.ShapeAnalysis;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using System;

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
			if( t <= 0 ) {
				TypeL1 = IntersectType.ReverseExtend;
			}
			else if( t >= 1 ) {
				TypeL1 = IntersectType.Extend;
			}
			else {
				TypeL1 = IntersectType.Inbetween;
			}
			if( s <= 0 ) {
				TypeL2 = IntersectType.ReverseExtend;
			}
			else if( s >= 1 ) {
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
			BRepAdaptor_Surface surface = new BRepAdaptor_Surface( face );
			if( surface.GetSurfaceType() == GeomAbs_SurfaceType.GeomAbs_Plane ) {
				p = surface.Plane().Location();
				dir = surface.Plane().Axis().Direction();
				if( face.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
					dir.Reverse();
				}
				return true;
			}
			else {
				return false;
			}
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
			Geom_Circle geom_Circle = ComputeCircle( geomCurve, 1e-3, dStartU, dEndU,
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
			return d1.IsParallel( d2, 1e-3 );
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
			const double dToleranceRatio = 1e-3;
			if( Math.Abs( ( dEdgeLength - dDistance ) / dDistance ) < dToleranceRatio ) {
				return true;
			}
			else {
				return false;
			}
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
	}
}
