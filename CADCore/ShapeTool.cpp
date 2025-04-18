#include "ShapeTool.h"

using namespace Core::Tool;

TopoDS_Shape ShapeTool::SewShape(const std::vector<TopoDS_Shape>& shapeList)
{
	const double dSewingTolerance = 1e-3;
	BRepBuilderAPI_Sewing sewing(dSewingTolerance);
	for (const auto& shape : shapeList) {
		sewing.Add(shape);
	}
	sewing.Perform();
	return sewing.SewedShape();
}

TopoDS_Shape ShapeTool::MakeCompound(const std::vector<TopoDS_Shape>& shapeList)
{
	BRep_Builder builder;
	TopoDS_Compound compound;
	builder.MakeCompound(compound);
	for (const auto& shape : shapeList) {
		builder.Add(compound, shape);
	}
	return compound;
}
