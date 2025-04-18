#pragma once

#include <TopoDS_Shape.hxx>
#include <TopoDS_Compound.hxx>
#include <BRep_Builder.hxx>
#include <BRepBuilderAPI_Sewing.hxx>
#include <vector>

namespace Core
{
	namespace Tool {
		class ShapeTool
		{
		public:
			static TopoDS_Shape SewShape(const std::vector<TopoDS_Shape>& shapeList);
			static TopoDS_Shape MakeCompound(const std::vector<TopoDS_Shape>& shapeList);
		};
	}
}
