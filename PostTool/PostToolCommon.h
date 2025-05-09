#pragma once
#ifdef POSTTOOL_EXPORTS
#define POSTTOOL_API __declspec(dllexport)
#else
#define POSTTOOL_API __declspec(dllimport)
#endif

