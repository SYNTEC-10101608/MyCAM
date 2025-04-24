#pragma once

#include <TopoDS_Wire.hxx>
#include <TopTools_IndexedDataMapOfShapeListOfShape.hxx>

namespace Core
{
	namespace DataStructure
	{
		class CADData {
		public:
			CADData( const TopoDS_Wire &contour,
				const TopTools_IndexedDataMapOfShapeListOfShape &shellMap,
				const TopTools_IndexedDataMapOfShapeListOfShape &solidMap );

			const TopoDS_Wire &GetContour() const;
			const TopTools_IndexedDataMapOfShapeListOfShape &GetShellMap() const;
			const TopTools_IndexedDataMapOfShapeListOfShape &GetSolidMap() const;

		private:
			TopoDS_Wire m_contour;
			TopTools_IndexedDataMapOfShapeListOfShape m_shellMap;
			TopTools_IndexedDataMapOfShapeListOfShape m_solidMap;
		};
	}
}
