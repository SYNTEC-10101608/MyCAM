#include <BRep_Tool.hxx>
#include <BRepGProp.hxx>
#include <GProp_GProps.hxx>
#include <Geom_Curve.hxx>
#include <ShapeAnalysis.hxx>
#include <TopAbs_Orientation.hxx>
#include <TopoDS_Vertex.hxx>
#include <gp_Pnt.hxx>

#include "GeometryTool.h" // Assume this exists and provides IsApproximatelyLinear
#include "SegmentTool.h"

using namespace Core::Tool;

void SegmentTool::GetEdgeSegmentPoints( const TopoDS_Edge &edge, double dSegmentLength, bool bSimplify, std::vector<gp_Pnt> &vertexList )
{
	vertexList.clear();

	if( bSimplify && GeometryTool::IsApproximatelyLinear( edge ) ) {
		TopoDS_Vertex v1, v2;
		ShapeAnalysis::FindBounds( edge, v1, v2 );
		gp_Pnt p1 = BRep_Tool::Pnt( v1 );
		gp_Pnt p2 = BRep_Tool::Pnt( v2 );
		vertexList.push_back( p1 );
		vertexList.push_back( p2 );
		return;
	}

	GProp_GProps system;
	BRepGProp::LinearProperties( edge, system );
	double dEdgeLength = system.Mass();

	int nSegments = static_cast< int >( ceil( dEdgeLength / dSegmentLength ) );

	double dStartU = 0, dEndU = 0;
	Handle( Geom_Curve ) oneGeomCurve = BRep_Tool::Curve( edge, dStartU, dEndU );

	if( edge.Orientation() == TopAbs_REVERSED ) {
		std::swap( dStartU, dEndU );
	}

	double dIncrement = ( dEndU - dStartU ) / nSegments;

	for( int i = 0; i <= nSegments; ++i ) {
		double U = dStartU + dIncrement * i;
		vertexList.push_back( oneGeomCurve->Value( U ) );
	}
}
