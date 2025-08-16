using OCC.BRep;
using OCC.Geom;
using OCC.GeomAPI;
using OCC.GeomLProp;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;

namespace OCCTool
{
	public class VectorTool
	{
		public static gp_Dir GetFaceNormalVec( TopoDS_Face face, gp_Pnt point )
		{
			// get surface from face
			Geom_Surface surface = BRep_Tool.Surface( face );

			// Project the point onto the surface to get the u, v parameters
			GeomAPI_ProjectPointOnSurf proj = new GeomAPI_ProjectPointOnSurf( point, surface );
			if( !proj.IsDone() ) {
				return new gp_Dir();
			}
			double u = 0;
			double v = 0;
			proj.LowerDistanceParameters( ref u, ref v );

			// get normal vector
			GeomLProp_SLProps props = new GeomLProp_SLProps( surface, u, v, 1, 1e-3 );
			if( !props.IsNormalDefined() ) {
				return new gp_Dir();
			}
			gp_Dir normal = props.Normal();
			if( face.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				normal.Reverse();
			}
			return normal;
		}

		public static bool Create3PCoordSystem( gp_Pnt p1, gp_Pnt p2, gp_Pnt p3, out gp_Ax3 coordSys )
		{
			coordSys = new gp_Ax3();
			gp_Vec v12 = new gp_Vec( p1, p2 );
			gp_Vec v13 = new gp_Vec( p1, p3 );
			gp_Vec vZ = v12.Crossed( v13 );
			if( vZ.Magnitude() < 1e-3 ) {
				return false;
			}
			coordSys = new gp_Ax3( p1, new gp_Dir( vZ ), new gp_Dir( v12 ) );
			return true;
		}
	}
}
