using MyCAM.Data;
using OCC.gp;
using System;
using System.Runtime.InteropServices;

namespace MyCAM.Post
{
	// P/INVOKE from C++ PostTool.dll
	public static class IKSolverInterop
	{
		[DllImport( "PostTool.dll", CallingConvention = CallingConvention.StdCall )]
		public static extern int IKSolver_IJKtoMS(
			double[] ToolDirection,
			double[] ToolDirectionAtZero,
			double[] DirectOfFirstRotAxis,
			double[] DirectOfSecondRotAxis,
			double LastMasterRotAngle,
			double LastSlaveRotAngle,
			out double MRotAngle1,
			out double SRotAngle1,
			out double MRotAngle2,
			out double SRotAngle2
		);
	}

	public enum IKSolveResult
	{
		NoError = 0,
		NoSolution = 1,
		MasterInfinityOfSolution = 2,
		SlaveInfinityOfSolution = 3,
	}

	public class IKSolver
	{
		public IKSolver( gp_Dir toolDir, gp_Dir masterRotateDir, gp_Dir slaveRotateDir )
		{
			// give a defaul Z dir BC type
			if( toolDir == null ) {
				toolDir = new gp_Dir( 0, 0, 1 );
			}
			if( masterRotateDir == null ) {
				masterRotateDir = new gp_Dir( 0, 0, 1 );
			}
			if( slaveRotateDir == null ) {
				slaveRotateDir = new gp_Dir( 0, 1, 0 );
			}
			ToolDir = new double[ 3 ] { toolDir.X(), toolDir.Y(), toolDir.Z() };
			MasterRotateDir = new double[ 3 ] { masterRotateDir.X(), masterRotateDir.Y(), masterRotateDir.Z() };
			SlaveRotateDir = new double[ 3 ] { slaveRotateDir.X(), slaveRotateDir.Y(), slaveRotateDir.Z() };
		}

		public IKSolveResult ConvertIJKToABC( gp_Dir toolVec_In, double dM_In, double dS_In, out double dM_Out, out double dS_Out )
		{
			// prevent from sigular area
			if( toolVec_In.IsParallel( new gp_Dir( MasterRotateDir[ 0 ], MasterRotateDir[ 1 ], MasterRotateDir[ 2 ] ), 1e-1 ) ) {

				// just make it singular to prevent unexpected result
				toolVec_In = new gp_Dir( MasterRotateDir[ 0 ], MasterRotateDir[ 1 ], MasterRotateDir[ 2 ] );
			}

			// calculate the A and C angle
			double[] ToolDirection = new double[ 3 ] { toolVec_In.X(), toolVec_In.Y(), toolVec_In.Z() };
			int solveResult = IKSolverInterop.IKSolver_IJKtoMS( ToolDirection, ToolDir, MasterRotateDir, SlaveRotateDir,
				dM_In, dS_In, out double dM1, out double dS1, out double dM2, out double dS2 );

			// master has infinite solution
			if( solveResult == (int)IKSolveResult.MasterInfinityOfSolution ) {
				dM_Out = dM1;
				dS_Out = dS1;
				return IKSolveResult.MasterInfinityOfSolution;
			}

			// slave has infinite solution, this may almost not happen in real world
			else if( solveResult == (int)IKSolveResult.SlaveInfinityOfSolution ) {
				dM_Out = dM1;
				dS_Out = dS1;
				return IKSolveResult.SlaveInfinityOfSolution;
			}

			// no solution
			else if( solveResult == (int)IKSolveResult.NoSolution ) {
				dM_Out = 0;
				dS_Out = 0;
				return IKSolveResult.NoSolution;
			}

			// the system is solvable, choose the closest solution
			dM1 = FindClosetCoterminalAngle( dM_In, dM1 );
			dM2 = FindClosetCoterminalAngle( dM_In, dM2 );
			double diff1 = Math.Abs( dM_In - dM1 ) + Math.Abs( dS_In - dS1 );
			double diff2 = Math.Abs( dM_In - dM2 ) + Math.Abs( dS_In - dS2 );
			if( diff1 < diff2 ) {
				dM_Out = dM1;
				dS_Out = dS1;
			}
			else {
				dM_Out = dM2;
				dS_Out = dS2;
			}
			return IKSolveResult.NoError;
		}

		double FindClosetCoterminalAngle( double dM_In, double dM )
		{
			// case when they are originally coterminal
			if( Math.Abs( dM_In - dM ) % ( 2 * Math.PI ) < 1e-6 ) {
				return dM;
			}

			// find the closest coterminal angle
			double valueP;
			double valueN;
			if( dM < dM_In ) {
				valueP = dM;
				while( valueP < dM ) {
					valueP += 2 * Math.PI;
				}
				valueN = valueP - 2 * Math.PI;
			}
			else {
				valueN = dM;
				while( valueN > dM ) {
					valueN -= 2 * Math.PI;
				}
				valueP = valueN + 2 * Math.PI;
			}
			if( Math.Abs( dM_In - valueP ) < Math.Abs( dM_In - valueN ) ) {
				return valueP;
			}
			else {
				return valueN;
			}
		}

		// machine properties
		double[] ToolDir;
		double[] MasterRotateDir;
		double[] SlaveRotateDir;
	}

	internal class PostSolver
	{
		public PostSolver( MachineData machineData )
		{
			if( machineData == null ) {
				throw new ArgumentNullException( "Machine data cannot be null." );
			}
			m_MachineData = machineData;
		}

		public virtual PostFrameData Solve( CAMPoint G54Data )
		{
			return new PostFrameData();
		}

		MachineData m_MachineData;
	}
}
