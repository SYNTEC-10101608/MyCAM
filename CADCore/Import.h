#pragma once

#include "CADCoreCommon.h"
#include <string>

namespace Core {
	class CADCORE_API Import
	{
	public:
		bool ImportFile(const std::string& filePath, int format);
	};
}
