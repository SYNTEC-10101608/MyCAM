#include "IKSolverCore.h"
//#include <cstring>

using namespace Eigen;
using namespace PostTool;

extern "C" POSTTOOL_API
int __stdcall IKSolver_IJKtoMS(
	const double *ToolDirection,           // 3 elements
	const double *ToolDirectionAtZero,     // 3 elements
	const double *DirectOfFirstRotAxis,    // 3 elements
	const double *DirectOfSecondRotAxis,   // 3 elements
	double LastMasterRotAngle,
	double LastSlaveRotAngle,
	double *MRotAngle1,
	double *SRotAngle1,
	double *MRotAngle2,
	double *SRotAngle2 )
{
	IKSolverCore solver;
	Vector3d td( ToolDirection[ 0 ], ToolDirection[ 1 ], ToolDirection[ 2 ] );
	Vector3d td0( ToolDirectionAtZero[ 0 ], ToolDirectionAtZero[ 1 ], ToolDirectionAtZero[ 2 ] );
	Vector3d axis1( DirectOfFirstRotAxis[ 0 ], DirectOfFirstRotAxis[ 1 ], DirectOfFirstRotAxis[ 2 ] );
	Vector3d axis2( DirectOfSecondRotAxis[ 0 ], DirectOfSecondRotAxis[ 1 ], DirectOfSecondRotAxis[ 2 ] );

	return solver.IJKtoMS(
		td,
		td0,
		axis1,
		axis2,
		LastMasterRotAngle,
		LastSlaveRotAngle,
		*MRotAngle1,
		*SRotAngle1,
		*MRotAngle2,
		*SRotAngle2
	);
}
