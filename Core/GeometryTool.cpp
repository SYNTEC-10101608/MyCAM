#include <TopoDS_Vertex.hxx>
#include <BRep_Tool.hxx>
#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepGProp.hxx>
#include <GProp_GProps.hxx>
#include <ShapeAnalysis.hxx>
#include <GeomAbs_SurfaceType.hxx>
#include <gp_Vec.hxx>

#include <cmath>

#include "GeometryTool.h"

using namespace Core::Tool;

gp_Pnt GeometryTool::FindIntersectPoint( const gp_Pnt &p1, const gp_Pnt &p2,
	const gp_Pnt &p3, const gp_Pnt &p4,
	IntersectType &TypeL1, IntersectType &TypeL2 )
{
	gp_Vec d1( p1, p2 );
	gp_Vec d2( p3, p4 );
	gp_Vec r( p3, p1 );

	double a = d1.Dot( d1 );
	double b = d1.Dot( d2 );
	double c = d2.Dot( d2 );
	double d = d1.Dot( r );
	double e = d2.Dot( r );

	double denominator = a * c - b * b;
	if( std::abs( denominator ) < 1e-6 ) {
		TypeL1 = IntersectType::NoIntersect;
		TypeL2 = IntersectType::NoIntersect;
		return gp_Pnt( ( p2.XYZ() + p4.XYZ() ) / 2 );
	}

	double t = ( b * e - c * d ) / denominator;
	double s = ( a * e - b * d ) / denominator;

	gp_Pnt point1 = p1.Translated( d1.Multiplied( t ) );
	gp_Pnt point2 = p3.Translated( d2.Multiplied( s ) );
	gp_Pnt midpoint( ( point1.XYZ() + point2.XYZ() ) / 2 );

	TypeL1 = ( t < 0 ) ? IntersectType::ReverseExtend : ( t > 1 ) ? IntersectType::Extend : IntersectType::Inbetween;
	TypeL2 = ( s < 0 ) ? IntersectType::ReverseExtend : ( s > 1 ) ? IntersectType::Extend : IntersectType::Inbetween;

	return midpoint;
}

bool GeometryTool::IsApproximatelyLinear( const TopoDS_Edge &edge )
{
	gp_Pnt p1, p2;
	return IsApproximatelyLinear( edge, p2, p1 );
}

bool GeometryTool::IsPlane( const TopoDS_Face &face, gp_Pnt &p, gp_Dir &dir )
{
	BRepAdaptor_Surface surface( face );
	if( surface.GetType() == GeomAbs_Plane ) {
		p = surface.Plane().Location();
		dir = surface.Plane().Axis().Direction();
		if( face.Orientation() == TopAbs_REVERSED ) {
			dir.Reverse();
		}
		return true;
	}
	return false;
}

bool GeometryTool::IsLine( const TopoDS_Edge &edge, gp_Pnt &p, gp_Dir &dir )
{
	BRepAdaptor_Curve curve( edge );
	if( curve.GetType() == GeomAbs_Line ) {
		p = curve.Line().Location();
		dir = curve.Line().Direction();
		return true;
	}

	gp_Pnt p1, p2;
	if( IsApproximatelyLinear( edge, p2, p1 ) ) {
		p = p1;
		dir = gp_Dir( gp_Vec( p1, p2 ) );
		return true;
	}
	return false;
}

bool GeometryTool::IsAxialSymmetry( const TopoDS_Face &face, gp_Pnt &p, gp_Dir &dir )
{
	BRepAdaptor_Surface surface( face );
	switch( surface.GetType() ) {
	case GeomAbs_Cylinder:
		p = surface.Cylinder().Location();
		dir = surface.Cylinder().Axis().Direction();
		return true;
	case GeomAbs_Cone:
		p = surface.Cone().Location();
		dir = surface.Cone().Axis().Direction();
		return true;
	case GeomAbs_Sphere:
		p = surface.Sphere().Location();
		dir = surface.Sphere().Position().Direction();
		return true;
	case GeomAbs_Torus:
		p = surface.Torus().Location();
		dir = surface.Torus().Axis().Direction();
		return true;
	case GeomAbs_SurfaceOfRevolution:
		p = surface.AxeOfRevolution().Location();
		dir = surface.AxeOfRevolution().Direction();
		return true;
	default:
		return false;
	}
}

bool GeometryTool::IsApproximatelyLinear( const TopoDS_Edge &edge, gp_Pnt &p2, gp_Pnt &p1 )
{
	GProp_GProps system;
	BRepGProp::LinearProperties( edge, system );
	double edgeLength = system.Mass();

	TopoDS_Vertex v1, v2;
	ShapeAnalysis::FindBounds( edge, v1, v2 );
	p1 = BRep_Tool::Pnt( v1 );
	p2 = BRep_Tool::Pnt( v2 );
	double distance = p1.Distance( p2 );

	const double tolerance = 1e-6;
	return std::abs( ( edgeLength - distance ) / distance ) < tolerance;
}
