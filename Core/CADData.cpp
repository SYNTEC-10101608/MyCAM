#include "CADData.h"

using namespace Core::DataStructure;

CADData::CADData( const TopoDS_Wire &contour,
	const TopTools_IndexedDataMapOfShapeListOfShape &shellMap,
	const TopTools_IndexedDataMapOfShapeListOfShape &solidMap )
	: m_contour( contour ), m_shellMap( shellMap ), m_solidMap( solidMap )
{
}

const TopoDS_Wire &CADData::GetContour() const {
	return m_contour;
}

const TopTools_IndexedDataMapOfShapeListOfShape &CADData::GetShellMap() const {
	return m_shellMap;
}

const TopTools_IndexedDataMapOfShapeListOfShape &CADData::GetSolidMap() const {
	return m_solidMap;
}
