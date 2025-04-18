#ifdef CADCORE_EXPORTS
#define CADCORE_API    __declspec(dllexport)
#else
#define CADCORE_API    __declspec(dllimport)
#endif
