#pragma once
#include "PostToolCommon.h"
#include <Dense>

namespace PostTool
{
	class IKSolverCore
	{
	public:
		int POSTTOOL_API IJKtoMS(
			Eigen::Vector3d ToolDirection,
			const Eigen::Vector3d &ToolDirectionAtZero,
			const Eigen::Vector3d &DirectOfFirstRotAxis,
			const Eigen::Vector3d &DirectOfSecondRotAxis,
			double LastMasterRotAngle,
			double LastSlaveRotAngle,
			double &MRotAngle1,
			double &SRotAngle1,
			double &MRotAngle2,
			double &SRotAngle2 );
	};
}
