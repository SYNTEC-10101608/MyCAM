#pragma once

#include "CoreCommon.h"
#include <string>

namespace Core
{
	class CORE_API Import
	{
	public:
		bool ImportFile( const Standard_CString filename, int format );
	};
}
