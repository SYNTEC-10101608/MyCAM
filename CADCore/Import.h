#pragma once

#include "CADCoreCommon.h"
#include <string>

namespace Core {
	class CADCORE_API Import
	{
	public:
		bool ImportFile(const Standard_CString filename, int format);

	private:
		bool ImportBrep(const Standard_CString filename);
		bool ImportStep(const Standard_CString filename);
		bool ImportIges(const Standard_CString filename);
	};
}
