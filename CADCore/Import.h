#pragma once
#include <string>

namespace Core {
	class Import
	{
	public:
		bool ImportFile(const std::string& filePath, int format);
	};
}
