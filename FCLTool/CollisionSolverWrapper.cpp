#include "CollisionSolver.h"

extern "C" {

	// construct
	FCLTOOL_API CollisionSolver *__stdcall CollisionSolver_Create()
	{
		return new CollisionSolver();
	}

	// destruct
	FCLTOOL_API void __stdcall CollisionSolver_Destroy( CollisionSolver *obj )
	{
		delete obj;
	}

	// AddModel
	FCLTOOL_API void __stdcall CollisionSolver_AddModel(
		CollisionSolver *obj,
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
	FCLTOOL_API bool __stdcall CollisionSolver_CheckCollision(
		CollisionSolver *obj,
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
