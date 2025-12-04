#pragma once
#include "PostToolCommon.h"
#include <Eigen/Dense>

namespace PostTool
{
	enum class SolutionType
	{
		ShortestDist = 0,
		MasterPos = 1,
		MasterNeg = 2,
		MSAngleShortestDist = 3
	};

	class FiveAxisSolverCore
	{
	public:
		/// <summary>
		/// Calculate rotation angles from tool direction vectors
		/// </summary>
		/// <param name="ToolDirection">Current tool direction vector</param>
		/// <param name="ToolDirectionAtZero">Tool direction at zero position</param>
		/// <param name="DirectOfFirstRotAxis">Direction of first rotation axis</param>
		/// <param name="DirectOfSecondRotAxis">Direction of second rotation axis</param>
		/// <param name="LastMasterRotAngle">Last master rotation angle</param>
		/// <param name="LastSlaveRotAngle">Last slave rotation angle</param>
		/// <param name="MRotAngle1">Output: Master rotation angle solution 1</param>
		/// <param name="SRotAngle1">Output: Slave rotation angle solution 1</param>
		/// <param name="MRotAngle2">Output: Master rotation angle solution 2</param>
		/// <param name="SRotAngle2">Output: Slave rotation angle solution 2</param>
		/// <returns>Error code (0 = no error)</returns>
		int POSTTOOL_API IJKtoMS(
			Eigen::Vector3d ToolDirection,
			const Eigen::Vector3d& ToolDirectionAtZero,
			const Eigen::Vector3d& DirectOfFirstRotAxis,
			const Eigen::Vector3d& DirectOfSecondRotAxis,
			double LastMasterRotAngle,
			double LastSlaveRotAngle,
			double& MRotAngle1,
			double& SRotAngle1,
			double& MRotAngle2,
			double& SRotAngle2,
			double IUtoBLU_Rotary );

		/// <summary>
		/// Choose the best solution from two angle pairs
		/// </summary>
		/// <param name="MRotAngle1">Master rotation angle solution 1</param>
		/// <param name="SRotAngle1">Slave rotation angle solution 1</param>
		/// <param name="MRotAngle2">Master rotation angle solution 2</param>
		/// <param name="SRotAngle2">Slave rotation angle solution 2</param>
		/// <param name="LastMasterRotAngle">Last master rotation angle</param>
		/// <param name="LastSlaveRotAngle">Last slave rotation angle</param>
		/// <param name="MasterRotAngle">Output: Selected master rotation angle</param>
		/// <param name="SlaveRotAngle">Output: Selected slave rotation angle</param>
		/// <param name="type">Solution selection type</param>
		/// <param name="FStart">First axis start limit</param>
		/// <param name="FEnd">First axis end limit</param>
		/// <param name="SStart">Second axis start limit</param>
		/// <param name="SEnd">Second axis end limit</param>
		/// <param name="nRDOfFirst">Rotation direction of first axis (1 or -1)</param>
		/// <param name="nRDOfSecond">Rotation direction of second axis (1 or -1)</param>
		/// <param name="IUtoBLU_Rotary">IU to BLU conversion factor for rotary axes</param>
		/// <returns>Error code (0 = no error)</returns>
		int POSTTOOL_API ChooseSolution(
			double MRotAngle1,
			double SRotAngle1,
			double MRotAngle2,
			double SRotAngle2,
			double LastMasterRotAngle,
			double LastSlaveRotAngle,
			double& MasterRotAngle,
			double& SlaveRotAngle,
			SolutionType type,
			double FStart,
			double FEnd,
			double SStart,
			double SEnd,
			int nRDOfFirst,
			int nRDOfSecond,
			double IUtoBLU_Rotary );

	private:
		bool SolveQuadEq( double a, double b, double c, double& x1, double& x2 );
		double Determinant( double a, double b, double c, double d );
		bool IsAtPermissibleRange( double Angle, double start, double end, double IUtoBLU_Rotary );
		void ConvertAngleRange( double& angle, double IUtoBLU_Rotary );
		bool ToPermissibleCoterminalAng( double& TarPos, double LastPos, SolutionType Prefer, 
			double LimitStart, double LimitEnd, int nRDofAX, double IUtoBLU_Rotary );
		bool IsPathPermissible( double Tar, double LastPos, double Start, double End, double IUtoBLU_Rotary );
		bool IsSolutionPermissible( double& MasterTarPos, double& SlaveTarPos, 
			double MasterLastPos, double SlaveLastPos, SolutionType type,
			double FStart, double FEnd, double SStart, double SEnd,
			int nRDOfFirst, int nRDOfSecond, double IUtoBLU_Rotary );
	};
}
