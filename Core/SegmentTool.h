#pragma once

#include <TopoDS_Edge.hxx>
#include <gp_Pnt.hxx>
#include <vector>

namespace Core
{
	namespace Tool
	{
		class SegmentTool
		{
		public:
			static void GetEdgeSegmentPoints( const TopoDS_Edge &edge, double dSegmentLength, bool bSimplify, std::vector<gp_Pnt> &vertexList );
		};
	}
}
