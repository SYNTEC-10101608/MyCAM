#include "IKSolverWrapper.h"
#include "IKSolverCore.h"

using Eigen::Vector3d;
using namespace PostTool;

namespace PostToolBridge
{
	int IKSolverWrapper::IJKtoMS(
		array<double> ^ToolDirection,
		array<double> ^ToolDirectionAtZero,
		array<double> ^DirectOfFirstRotAxis,
		array<double> ^DirectOfSecondRotAxis,
		double LastMasterRotAngle,
		double LastSlaveRotAngle,
		double %MRotAngle1,
		double %SRotAngle1,
		double %MRotAngle2,
		double %SRotAngle2 )
	{
		// 檢查陣列長度
		if( ToolDirection->Length != 3 || ToolDirectionAtZero->Length != 3 ||
			DirectOfFirstRotAxis->Length != 3 || DirectOfSecondRotAxis->Length != 3 )
			throw gcnew ArgumentException( "All input vectors must be of length 3." );

		// 轉換 managed array<double>^ → Eigen::Vector3d
		Vector3d toolDir;
		Vector3d toolZero;
		Vector3d axis1;
		Vector3d axis2;
		for( int i = 0; i < 3; i++ ) {
			toolDir[ i ] = ToolDirection[ i ];
			toolZero[ i ] = ToolDirectionAtZero[ i ];
			axis1[ i ] = DirectOfFirstRotAxis[ i ];
			axis2[ i ] = DirectOfSecondRotAxis[ i ];
		}

		// 結果角度
		double m1, s1, m2, s2;

		// 呼叫核心函式
		IKSolverCore solver;
		int result = solver.IJKtoMS(
			toolDir, toolZero, axis1, axis2,
			LastMasterRotAngle, LastSlaveRotAngle,
			m1, s1, m2, s2 );

		// 回傳給 C#
		MRotAngle1 = m1;
		SRotAngle1 = s1;
		MRotAngle2 = m2;
		SRotAngle2 = s2;

		return result;
	}
}
