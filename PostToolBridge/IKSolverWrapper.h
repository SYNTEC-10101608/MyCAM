#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

namespace PostToolBridge
{
	public ref class IKSolverWrapper
	{
	public:
		// 傳入方向向量與其他參數，回傳兩組 Master/Slave 角度解，與解的個數
		static int IJKtoMS(
			array<double> ^ToolDirection,
			array<double> ^ToolDirectionAtZero,
			array<double> ^DirectOfFirstRotAxis,
			array<double> ^DirectOfSecondRotAxis,
			double LastMasterRotAngle,
			double LastSlaveRotAngle,
			[ Out ] double %MRotAngle1,
			[ Out ] double %SRotAngle1,
			[ Out ] double %MRotAngle2,
			[ Out ] double %SRotAngle2 );
	};
}
