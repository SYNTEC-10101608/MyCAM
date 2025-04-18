#pragma once

#include "CADCoreCommon.h"
#include <string>

namespace Core {
	class CADCORE_API Import
	{
	public:
		bool ImportFile(const Standard_CString filename, int format);
	};
}
