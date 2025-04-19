#pragma once

#include "CoreCommon.h"
#include <string>

namespace Core
{
	class CORE_API Import
	{
	public:
		bool ImportFile( const Standard_CString filename, int format );
		TopoDS_Shape GetImportedShape();

	private:

		// imported shape
		TopoDS_Shape m_ImportedShape;
	};
}
