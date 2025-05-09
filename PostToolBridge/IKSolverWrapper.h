#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

namespace PostToolBridge
{
	public ref class IKSolverWrapper
	{
	public:
		// �ǤJ��V�V�q�P��L�ѼơA�^�Ǩ�� Master/Slave ���׸ѡA�P�Ѫ��Ӽ�
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
