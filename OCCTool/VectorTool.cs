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
			GeomLProp_SLProps props = new GeomLProp_SLProps( surface, u, v, 1, 0.01 );
			if( !props.IsNormalDefined() ) {
				return new gp_Dir();
			}
			gp_Dir normal = props.Normal();
			if( face.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				normal.Reverse();
			}
			return normal;
		}

		public static gp_Dir GetEdgeTangentVec( TopoDS_Edge edge, gp_Pnt point )
		{
			// get curve from edge
			double dStartU = 0;
			double dEndU = 0;
			Geom_Curve curve = BRep_Tool.Curve( edge, ref dStartU, ref dEndU );

			// Project the point onto the curve to get the u parameter
			GeomAPI_ProjectPointOnCurve proj = new GeomAPI_ProjectPointOnCurve( point, curve );

			// u might be out of range if the curve is periodic
			double u = proj.LowerDistanceParameter();

			// get tangent vector
			GeomLProp_CLProps props = new GeomLProp_CLProps( curve, u, 1, 0.01 );
			if( !props.IsTangentDefined() ) {
				return new gp_Dir();
			}
			gp_Dir tangent = new gp_Dir();
			props.Tangent( ref tangent );
			if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				tangent.Reverse();
			}
			return tangent;
		}

		public static gp_Vec GetVec( gp_Pnt point1, gp_Pnt point2 )
		{
			return new gp_Vec( point1, point2 );
		}
	}
}
