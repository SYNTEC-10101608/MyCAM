#include "ShapeTool.h"

using namespace Core::Tool;

TopoDS_Shape ShapeTool::SewShape( const std::vector<TopoDS_Shape> &shapeList )
{
	const double dSewingTolerance = 1e-3;
	Handle( BRepBuilderAPI_Sewing ) pSewing = new BRepBuilderAPI_Sewing( dSewingTolerance );
	for( const auto &shape : shapeList ) {
		pSewing->Add( shape );
	}
	pSewing->Perform();
	return pSewing->SewedShape();
}

TopoDS_Shape ShapeTool::MakeCompound( const std::vector<TopoDS_Shape> &shapeList )
{
	BRep_Builder builder;
	TopoDS_Compound compound;
	builder.MakeCompound( compound );
	for( const auto &shape : shapeList ) {
		builder.Add( compound, shape );
	}
	return compound;
}
