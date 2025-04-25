#pragma once

#include <TopoDS_Face.hxx>
#include <TopoDS_Edge.hxx>
#include <gp_Pnt.hxx>
#include <gp_Dir.hxx>

namespace Core
{
	namespace Tool
	{
		class VectorTool
		{
		public:
			static gp_Dir GetFaceNormalVec( const TopoDS_Face &face, const gp_Pnt &point );
			static gp_Dir GetEdgeTangentVec( const TopoDS_Edge &edge, const gp_Pnt &point );
		};
	}
}
