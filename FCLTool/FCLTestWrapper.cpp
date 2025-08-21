#include "FCLTest.h"

extern "C" {

	// construct
	FCLTOOL_API FCLTest *FCLTest_Create()
	{
		return new FCLTest();
	}

	// destruct
	FCLTOOL_API void FCLTest_Destroy( FCLTest *obj )
	{
		delete obj;
	}

	// AddModel
	FCLTOOL_API void FCLTest_AddModel(
		FCLTest *obj,
		const char *szID,
		int triCount, const int *indexList,
		int vertexCount, const double *vertexList )
	{
		if( !obj ) {
			return;
		}
		obj->AddModel( szID, triCount, indexList, vertexCount, vertexList );
	}

	// CheckCollision
	FCLTOOL_API bool FCLTest_CheckCollision(
		FCLTest *obj,
		const char *szID1,
		const char *szID2,
		const double *trsf1,
		const double *trsf2 )
	{
		if( !obj ) {
			return false;
		}
		return obj->CheckCollision( szID1, szID2, trsf1, trsf2 );
	}

} // extern "C"
