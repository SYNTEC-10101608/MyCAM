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

bool Import::ImportFile(const Standard_CString filename, int format)
{
	switch (format)
	{
	case 0: // BREP
		return ImportBrep(filename);
	case 1: // STEP
		return ImportStep(filename);
	case 2: // IGES
		return ImportIges(filename);
	default:
		return false;
	}
}

bool Import::ImportBrep(const Standard_CString filename)
{
	// Import BREP file
	BRep_Builder builder;
	TopoDS_Shape shape;
	if (!BRepTools::Read(shape, filename, builder))
	{
		return false;
	}

	// sew the shape
	std::vector<TopoDS_Shape> shapes = { shape };
	g_ImportedShape = ShapeTool::SewShape(shapes);
	return true;
}

bool Import::ImportStep(const Standard_CString filename)
{
	// Import STEP file
	STEPControl_Reader reader;
	IFSelect_ReturnStatus status = reader.ReadFile(filename);
	if (status != IFSelect_RetDone)
	{
		return false;
	}
	reader.TransferRoots();
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
	return true;
}

bool Import::ImportIges(const Standard_CString filename)
{
	// Import IGES file
	IGESControl_Reader reader;
	IFSelect_ReturnStatus status = reader.ReadFile(filename);
	if (status != IFSelect_RetDone)
	{
		return false;
	}
	reader.TransferRoots();
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
	return true;
}
