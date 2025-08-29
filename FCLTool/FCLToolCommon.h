#pragma once

#ifdef FCLTOOL_EXPORTS
#define FCLTOOL_API __declspec(dllexport)
#else
#define FCLTOOL_API __declspec(dllimport)
#endif
