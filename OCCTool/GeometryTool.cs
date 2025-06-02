using OCC.BRep;
using OCC.BRepAdaptor;
using OCC.BRepGProp;
using OCC.GeomAbs;
using OCC.gp;
using OCC.GProp;
using OCC.ShapeAnalysis;
using OCC.TopAbs;
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
			if( Math.Abs( denominator ) < 1e-6 ) {
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
			BRepAdaptor_Curve curve = new BRepAdaptor_Curve( edge );
			if( curve.GetCurveType() == GeomAbs_CurveType.GeomAbs_Circle ) {
				p = curve.Circle().Location();
				dir = curve.Circle().Axis().Direction();
				r = curve.Circle().Radius();
				return true;
			}
			else {
				return false;
			}
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
			const double dToleranceRatio = 1e-6;
			if( Math.Abs( ( dEdgeLength - dDistance ) / dDistance ) < dToleranceRatio ) {
				return true;
			}
			else {
				return false;
			}
		}
	}
}
