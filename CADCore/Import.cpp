// OCCT headers
#include <TopoDS_Shape.hxx>
#include <BRep_Builder.hxx>
#include <BRepTools.hxx>
#include <STEPControl_Reader.hxx>
#include <IGESControl_Reader.hxx>
#include <XSControl_Reader.hxx>
#include <IFSelect_ReturnStatus.hxx>

// c++ headers
#include <vector>
#include <string>

// project headers
#include "ShapeTool.h"
#include "Import.h"

using namespace Core::Tool;
using namespace Core;

TopoDS_Shape g_ImportedShape;

bool Import::ImportFile(const std::string& filePath, int format)
{
	XSControl_Reader reader;
	switch (format)
	{
	case 0: // BREP
	{
		reader = XSControl_Reader();
	}
	case 1: // STEP
	{
		reader = STEPControl_Reader();
	}
	case 2: // IGES
	{
		reader = IGESControl_Reader();
	}
	default:
		return false;
	}
	IFSelect_ReturnStatus status = reader.ReadFile(filePath.c_str());

	// check the status
	if (status != IFSelect_RetDone)
	{
		return false;
	}
	reader.TransferRoots();

	// prevent from empty shape or null shape
	if (reader.NbShapes() == 0)
	{
		return false;
	}
	if (reader.OneShape().IsNull())
	{
		return false;
	}

	// sew the shape
	std::vector<TopoDS_Shape> shapes = { reader.OneShape() };
	g_ImportedShape = ShapeTool::SewShape(shapes);
}
