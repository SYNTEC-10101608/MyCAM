#pragma once

#include <gp_Pnt.hxx>
#include <gp_Dir.hxx>
#include <TopoDS_Edge.hxx>
#include <TopoDS_Face.hxx>

namespace Core
{
	namespace Tool
	{

		enum class IntersectType
		{
			Extend,
			ReverseExtend,
			Inbetween,
			NoIntersect
		};

		class GeometryTool
		{
		public:
			static gp_Pnt FindIntersectPoint( const gp_Pnt &p1, const gp_Pnt &p2,
				const gp_Pnt &p3, const gp_Pnt &p4,
				IntersectType &TypeL1, IntersectType &TypeL2 );

			static bool IsApproximatelyLinear( const TopoDS_Edge &edge );
			static bool IsLine( const TopoDS_Edge &edge, gp_Pnt &p, gp_Dir &dir );
			static bool IsPlane( const TopoDS_Face &face, gp_Pnt &p, gp_Dir &dir );
			static bool IsAxialSymmetry( const TopoDS_Face &face, gp_Pnt &p, gp_Dir &dir );

		private:
			static bool IsApproximatelyLinear( const TopoDS_Edge &edge, gp_Pnt &p2, gp_Pnt &p1 );
		};
	}
}
