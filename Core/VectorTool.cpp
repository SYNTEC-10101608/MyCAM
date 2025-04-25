#include <BRep_Tool.hxx>
#include <Geom_Surface.hxx>
#include <Geom_Curve.hxx>
#include <GeomAPI_ProjectPointOnSurf.hxx>
#include <GeomAPI_ProjectPointOnCurve.hxx>
#include <GeomLProp_SLProps.hxx>
#include <GeomLProp_CLProps.hxx>
#include <TopAbs_Orientation.hxx>

#include "VectorTool.h"

using namespace Core::Tool;


gp_Dir VectorTool::GetFaceNormalVec( const TopoDS_Face &face, const gp_Pnt &point )
{
	Handle( Geom_Surface ) surface = BRep_Tool::Surface( face );
	GeomAPI_ProjectPointOnSurf proj( point, surface );

	if( !proj.IsDone() ) {
		return gp_Dir();
	}

	double u = 0, v = 0;
	proj.LowerDistanceParameters( u, v );

	GeomLProp_SLProps props( surface, u, v, 1, 1e-3 );
	if( !props.IsNormalDefined() ) {
		return gp_Dir();
	}

	gp_Dir normal = props.Normal();
	if( face.Orientation() == TopAbs_REVERSED ) {
		normal.Reverse();
	}
	return normal;
}

gp_Dir VectorTool::GetEdgeTangentVec( const TopoDS_Edge &edge, const gp_Pnt &point )
{
	double dStartU = 0, dEndU = 0;
	Handle( Geom_Curve ) curve = BRep_Tool::Curve( edge, dStartU, dEndU );

	GeomAPI_ProjectPointOnCurve proj( point, curve );
	double u = proj.LowerDistanceParameter();

	GeomLProp_CLProps props( curve, u, 1, 1e-3 );
	if( !props.IsTangentDefined() ) {
		return gp_Dir();
	}

	gp_Dir tangent;
	props.Tangent( tangent );
	if( edge.Orientation() == TopAbs_REVERSED ) {
		tangent.Reverse();
	}
	return tangent;
}
