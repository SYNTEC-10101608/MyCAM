#pragma once

#include "CoreCommon.h"
#include <TopoDS_Shape.hxx>
#include <vector>

namespace Core
{
	namespace Tool
	{
		class CORE_API ShapeTool
		{
		public:
			static TopoDS_Shape SewShape( const std::vector<TopoDS_Shape> &shapeList );
			static TopoDS_Shape MakeCompound( const std::vector<TopoDS_Shape> &shapeList );
		};
	}
}
