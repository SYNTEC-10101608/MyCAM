using MyCAM.Data;
using MyCAM.Machine;
using OCC.gp;
using System;
using System.Collections.Generic;
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

		public IKSolveResult Solve( gp_Dir toolVec_In, double dM_In, double dS_In, out double dM_Out, out double dS_Out )
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

	public class FKSolver
	{
		public FKSolver( gp_Vec mcsToSlave, gp_Vec slaveToMaster, gp_Vec toolVec, gp_Dir masterRotateDir, gp_Dir slaveRotateDir )
		{
			// give a defaul Z dir BC type
			if( mcsToSlave == null ) {
				mcsToSlave = new gp_Vec( 0, 0, 0 );
			}
			if( slaveToMaster == null ) {
				slaveToMaster = new gp_Vec( 0, 0, 0 );
			}
			if( toolVec == null ) {
				toolVec = new gp_Vec( 0, 0, 1 );
			}
			if( masterRotateDir == null ) {
				masterRotateDir = new gp_Dir( 0, 0, 1 );
			}
			if( slaveRotateDir == null ) {
				slaveRotateDir = new gp_Dir( 0, 1, 0 );
			}
			MCSToSlave = mcsToSlave;
			SlaveToMaster = slaveToMaster;
			ToolVec = toolVec;
			MasterRotateDir = masterRotateDir;
			SlaveRotateDir = slaveRotateDir;
		}

		public gp_Vec Solve( double masterAngle, double slaveAngle )
		{
			// the original TCP on master coordinate system
			gp_Vec tcpOnMasterAtZero = ToolVec - MCSToSlave - SlaveToMaster;

			// rotate slave
			gp_Vec tcpOnSlave = ToolVec - MCSToSlave; // TCP on slave coordinate system
			gp_Trsf slaveTrsf = new gp_Trsf();
			slaveTrsf.SetRotation( new gp_Ax1( new gp_Pnt(), SlaveRotateDir ), slaveAngle );
			tcpOnSlave.Transform( slaveTrsf );

			// rotate master
			gp_Vec tcpOnMaster = tcpOnSlave - SlaveToMaster; // TCP on master coordinate system
			gp_Trsf masterTrsf = new gp_Trsf();
			masterTrsf.SetRotation( new gp_Ax1( new gp_Pnt(), MasterRotateDir ), masterAngle );
			tcpOnMaster.Transform( masterTrsf );

			// the result is the vector from new TCP to original TCP
			return tcpOnMasterAtZero - tcpOnMaster;
		}

		// machine properties
		gp_Vec MCSToSlave; // DE
		gp_Vec SlaveToMaster; // EF
		gp_Vec ToolVec; // L (DT)
		gp_Dir MasterRotateDir; // (abc)
		gp_Dir SlaveRotateDir; // (def)
	}

	internal class PostSolver<TMachineData> where TMachineData : MachineData
	{
		public PostSolver( TMachineData machineData )
		{
			if( machineData == null ) {
				throw new ArgumentException( "Invalid machine data" );
			}
			m_MachineData = machineData;
			CalculateMasterRotateDir();
			CalculateSlaveRotateDir();
			CalculateToolDir();
			BuildFKSolver();
			BuildIKSolver();
		}

		public virtual bool Solve( CAMData camData,
			out List<PostData> resultG54, out List<PostData> resultMCS )
		{
			resultG54 = new List<PostData>();
			resultMCS = new List<PostData>();
			if( camData == null ) {
				return false;
			}

			// solve IK
			List<Tuple<double, double>> rotateAngleList = new List<Tuple<double, double>>();
			List<bool> sigularTagList = new List<bool>();
			double dM = 0;
			double dS = 0;
			foreach( CAMPoint point in camData.CAMPointList ) {
				IKSolveResult ikResult = m_IKSolver.Solve( point.ToolVec, dM, dS, out dM, out dS );
				if( ikResult == IKSolveResult.NoError ) {
					rotateAngleList.Add( new Tuple<double, double>( dM, dS ) );
					sigularTagList.Add( false );
				}
				else if( ikResult == IKSolveResult.MasterInfinityOfSolution || ikResult == IKSolveResult.SlaveInfinityOfSolution ) {
					rotateAngleList.Add( new Tuple<double, double>( dM, dS ) );
					sigularTagList.Add( true );
				}

				// some point in the path is unsolvable
				else {
					return false;
				}
			}

			// TODO: filter the sigular points
			// solve FK
			for( int i = 0; i < camData.CAMPointList.Count; i++ ) {
				gp_Pnt pointG54 = camData.CAMPointList[ i ].CADPoint.Point;
				gp_Vec tcpOffset = m_FKSolver.Solve( rotateAngleList[ i ].Item1, rotateAngleList[ i ].Item2 );
				gp_Pnt pointMCS = pointG54.Translated( tcpOffset );

				// add G54 frame data
				PostData frameDataG54 = new PostData()
				{
					X = pointG54.X(),
					Y = pointG54.Y(),
					Z = pointG54.Z(),
					Master = rotateAngleList[ i ].Item1,
					Slave = rotateAngleList[ i ].Item2
				};
				resultG54.Add( frameDataG54 );

				// add MCS frame data
				PostData frameDataMCS = new PostData()
				{
					X = pointMCS.X(),
					Y = pointMCS.Y(),
					Z = pointMCS.Z(),
					Master = rotateAngleList[ i ].Item1,
					Slave = rotateAngleList[ i ].Item2
				};
				resultMCS.Add( frameDataMCS );
			}
			return true;
		}

		// TODO: cosider tilted
		protected virtual void CalculateMasterRotateDir()
		{
			switch( m_MachineData.MasterRotaryAxis ) {
				case RotaryAxis.X:
					m_MasterRotateDir = new gp_Dir( 1, 0, 0 );
					break;
				case RotaryAxis.Y:
					m_MasterRotateDir = new gp_Dir( 0, 1, 0 );
					break;
				case RotaryAxis.Z:
					m_MasterRotateDir = new gp_Dir( 0, 0, 1 );
					break;
				default:
					m_MasterRotateDir = new gp_Dir( 0, 0, 1 );
					break;
			}
		}

		// TODO: cosider tilted
		protected virtual void CalculateSlaveRotateDir()
		{
			switch( m_MachineData.SlaveRotaryAxis ) {
				case RotaryAxis.X:
					m_SlaveRotateDir = new gp_Dir( 1, 0, 0 );
					break;
				case RotaryAxis.Y:
					m_SlaveRotateDir = new gp_Dir( 0, 1, 0 );
					break;
				case RotaryAxis.Z:
					m_SlaveRotateDir = new gp_Dir( 0, 0, 1 );
					break;
				default:
					m_SlaveRotateDir = new gp_Dir( 0, 1, 0 );
					break;
			}
		}

		protected virtual void CalculateToolDir()
		{
			switch( m_MachineData.ToolDirection ) {
				case ToolDirection.X:
					m_ToolDir = new gp_Dir( 1, 0, 0 );
					break;
				case ToolDirection.Y:
					m_ToolDir = new gp_Dir( 0, 1, 0 );
					break;
				case ToolDirection.Z:
					m_ToolDir = new gp_Dir( 0, 0, 1 );
					break;
				default:
					m_ToolDir = new gp_Dir( 0, 0, 1 );
					break;
			}
		}

		// hook
		protected virtual void BuildFKSolver()
		{
			gp_Vec mcsToSlave = new gp_Vec();
			gp_Vec slaveToMaster = new gp_Vec();
			gp_Vec toolVec = new gp_Vec( m_ToolDir );
			toolVec.Multiply( m_MachineData.ToolLength );
			m_FKSolver = new FKSolver( mcsToSlave, slaveToMaster, toolVec, m_MasterRotateDir, m_SlaveRotateDir );
		}

		// hook
		protected virtual void BuildIKSolver()
		{
			m_IKSolver = new IKSolver( m_ToolDir, m_MasterRotateDir, m_SlaveRotateDir );
		}

		protected TMachineData m_MachineData;
		protected IKSolver m_IKSolver;
		protected FKSolver m_FKSolver;
		protected gp_Dir m_MasterRotateDir;
		protected gp_Dir m_SlaveRotateDir;
		protected gp_Dir m_ToolDir;
	}

	internal class SpindlePostSolver : PostSolver<SpindleTypeMachineData>
	{
		public SpindlePostSolver( SpindleTypeMachineData machineData ) : base( machineData )
		{
		}

		protected override void BuildFKSolver()
		{
			gp_Vec mcsToSlave = new gp_Vec( m_MachineData.ToolToSlaveVec.XYZ() );
			gp_Vec slaveToMaster = new gp_Vec( m_MachineData.SlaveToMasterVec.XYZ() );
			gp_Vec toolVec = new gp_Vec( m_ToolDir );
			toolVec.Multiply( m_MachineData.ToolLength );
			m_FKSolver = new FKSolver( mcsToSlave, slaveToMaster, toolVec, m_MasterRotateDir, m_SlaveRotateDir );
		}

		protected override void BuildIKSolver()
		{
			m_IKSolver = new IKSolver( m_ToolDir, m_MasterRotateDir, m_SlaveRotateDir );
		}
	}
}
