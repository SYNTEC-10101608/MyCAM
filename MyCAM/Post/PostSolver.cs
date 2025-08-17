using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MyCAM.Post
{
	// P/INVOKE from C++ PostTool.dll
	internal static class IKSolverInterop
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

	/// <summary>
	/// this is design based on spindle type
	/// for table and mix type, just exchange the master and slave axis
	/// </summary>
	internal class IKSolver
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
			if( toolVec_In.IsParallel( new gp_Dir( MasterRotateDir[ 0 ], MasterRotateDir[ 1 ], MasterRotateDir[ 2 ] ), 1e-2 ) ) {

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

	/// <summary>
	/// this is design based on spindle type
	/// for table and mix type, just exchange the master and slave axis
	/// </summary>
	internal class FKSolver
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
			gp_Vec tcpOnMasterAtZero = new gp_Vec() - MCSToSlave - SlaveToMaster;

			// rotate slave
			gp_Vec tcpOnSlave = ToolVec.Reversed() - MCSToSlave; // TCP on slave coordinate system
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
		gp_Vec ToolVec; // -L (-DT)
		gp_Dir MasterRotateDir; // (abc)
		gp_Dir SlaveRotateDir; // (def)
	}

	internal abstract class WorkPieceToWorldSolver
	{
		public abstract gp_Trsf Solve( double masterAngle, double slaveAngle );

		gp_Trsf m_Transform;
	}

	internal class SpindleTypeWorkPieceToWorldSolver : WorkPieceToWorldSolver
	{
		public override gp_Trsf Solve( double masterAngle, double slaveAngle )
		{
			return new gp_Trsf();
		}
	}

	internal class TableTypeWorkPieceToWorldSolver : WorkPieceToWorldSolver
	{
		public TableTypeWorkPieceToWorldSolver( gp_Pnt ptOnMaster, gp_Pnt ptOnSlave, gp_Dir masterRotateDir, gp_Dir slaveRotateDir )
		{
			// give a default Z dir BC type
			if( ptOnMaster == null ) {
				ptOnMaster = new gp_Pnt( 0, 0, 0 );
			}
			if( ptOnSlave == null ) {
				ptOnSlave = new gp_Pnt( 0, 0, 0 );
			}
			if( masterRotateDir == null ) {
				masterRotateDir = new gp_Dir( 0, 0, 1 );
			}
			if( slaveRotateDir == null ) {
				slaveRotateDir = new gp_Dir( 0, 1, 0 );
			}
			m_PtOnMaster = ptOnMaster;
			m_PtOnSlave = ptOnSlave;
			m_MasterRotateDir = masterRotateDir;
			m_SlaveRotateDir = slaveRotateDir;
		}

		public override gp_Trsf Solve( double masterAngle, double slaveAngle )
		{
			// world coord
			gp_Ax3 world = new gp_Ax3();

			// workpiece coord
			gp_Ax3 workpiece = new gp_Ax3();
			gp_Ax1 masterRotateDir = new gp_Ax1( m_PtOnMaster, m_MasterRotateDir );
			gp_Ax1 slaveRotateDir = new gp_Ax1( m_PtOnSlave, m_SlaveRotateDir );
			gp_Trsf slaveTrsf = new gp_Trsf();
			slaveTrsf.SetRotation( slaveRotateDir, slaveAngle );
			workpiece.Transform( slaveTrsf );
			gp_Trsf masterTrsf = new gp_Trsf();
			masterTrsf.SetRotation( masterRotateDir, masterAngle );
			workpiece.Transform( masterTrsf );

			// calculate the transform from workpiece to world
			gp_Trsf transform = new gp_Trsf();
			transform.SetDisplacement( workpiece, world );
			return transform;
		}

		gp_Pnt m_PtOnMaster;
		gp_Pnt m_PtOnSlave;
		gp_Dir m_MasterRotateDir;
		gp_Dir m_SlaveRotateDir;
	}

	internal class MixTypeWorkPieceToWorldSolver : WorkPieceToWorldSolver
	{
		public MixTypeWorkPieceToWorldSolver( gp_Pnt ptOnSlave, gp_Dir slaveRotateDir )
		{
			// give a default Z dir BC type
			if( ptOnSlave == null ) {
				ptOnSlave = new gp_Pnt( 0, 0, 0 );
			}
			if( slaveRotateDir == null ) {
				slaveRotateDir = new gp_Dir( 0, 1, 0 );
			}
			m_PtOnSlave = ptOnSlave;
			m_SlaveRotateDir = slaveRotateDir;
		}

		public override gp_Trsf Solve( double masterAngle, double slaveAngle )
		{
			return new gp_Trsf();
		}

		gp_Pnt m_PtOnSlave;
		gp_Dir m_SlaveRotateDir;
	}

	internal class RotaryAxisSolver
	{
		public RotaryAxisSolver( gp_Pnt ptOnMaster, gp_Pnt ptOnSlave, gp_Dir masterRotateDir, gp_Dir slaveRotateDir )
		{
			// give a default Z dir BC type
			if( ptOnMaster == null ) {
				ptOnMaster = new gp_Pnt( 0, 0, 0 );
			}
			if( ptOnSlave == null ) {
				ptOnSlave = new gp_Pnt( 0, 0, 0 );
			}
			if( masterRotateDir == null ) {
				masterRotateDir = new gp_Dir( 0, 0, 1 );
			}
			if( slaveRotateDir == null ) {
				slaveRotateDir = new gp_Dir( 0, 1, 0 );
			}
			m_PtOnMaster = ptOnMaster;
			m_PtOnSlave = ptOnSlave;
			m_MasterRotateDir = masterRotateDir;
			m_SlaveRotateDir = slaveRotateDir;
		}

		public gp_Dir MasterRotateDir
		{
			get
			{
				return m_MasterRotateDir;
			}
		}

		public gp_Dir SlaveRotateDir
		{
			get
			{
				return m_SlaveRotateDir;
			}
		}

		public gp_Pnt PtOnMaster
		{
			get
			{
				return m_PtOnMaster;
			}
		}

		public gp_Pnt PtOnSlave
		{
			get
			{
				return m_PtOnSlave;
			}
		}

		gp_Pnt m_PtOnMaster;
		gp_Pnt m_PtOnSlave;
		gp_Dir m_MasterRotateDir;
		gp_Dir m_SlaveRotateDir;
	}

	internal class PostSolver
	{
		public PostSolver( MachineData machineData )
		{
			if( machineData == null ) {
				throw new ArgumentException( "Invalid machine data" );
			}
			ISolverBuilder solverBuilder = CreateSolverBuilder( machineData );
			m_FKSolver = solverBuilder.BuildFKSolver();
			m_IKSolver = solverBuilder.BuildIKSolver();
			m_WorkPieceToWorldSolver = solverBuilder.BuildWorkPieceToWorldSolver();
			m_RotaryAxisSolver = solverBuilder.BuildRotaryAxisSolver();
		}

		public bool Solve( CAMData camData,
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
				tcpOffset.Transform( m_WorkPieceToWorldSolver.Solve( -rotateAngleList[ i ].Item2, -rotateAngleList[ i ].Item1 ) );
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
					Master = rotateAngleList[ i ].Item2,
					Slave = rotateAngleList[ i ].Item1
				};
				resultMCS.Add( frameDataMCS );
			}
			return true;
		}

		public RotaryAxisSolver RotaryAxisSolver
		{
			get
			{
				return m_RotaryAxisSolver;
			}
		}

		// solver builder factory
		ISolverBuilder CreateSolverBuilder( MachineData machineData )
		{
			if( machineData == null ) {
				throw new ArgumentException( "Invalid machine data" );
			}
			switch( machineData.FiveAxisType ) {
				case FiveAxisType.Spindle:
					return new SpindleSolverBuilder( (SpindleTypeMachineData)machineData );
				case FiveAxisType.Table:
					return new TableSolverBuilder( (TableTypeMachineData)machineData );
				case FiveAxisType.Mix:
					return new MixSolverBuilder( (MixTypeMachineData)machineData );
				default:
					throw new NotSupportedException( "Unsupported machine type" );
			}
		}

		IKSolver m_IKSolver;
		FKSolver m_FKSolver;
		WorkPieceToWorldSolver m_WorkPieceToWorldSolver;
		RotaryAxisSolver m_RotaryAxisSolver;
	}

	internal interface ISolverBuilder
	{
		FKSolver BuildFKSolver();

		IKSolver BuildIKSolver();

		WorkPieceToWorldSolver BuildWorkPieceToWorldSolver();

		RotaryAxisSolver BuildRotaryAxisSolver();
	}

	internal abstract class SolverBuilderBase<TMachineData> : ISolverBuilder
		where TMachineData : MachineData
	{
		protected SolverBuilderBase( TMachineData machineData )
		{
			if( machineData == null ) {
				throw new ArgumentException( "Invalid machine data" );
			}
			m_MachineData = machineData;
			CalculateMasterRotateDir();
			CalculateSlaveRotateDir();
			CalculateToolDir();
		}

		public abstract FKSolver BuildFKSolver();

		public abstract IKSolver BuildIKSolver();

		public abstract WorkPieceToWorldSolver BuildWorkPieceToWorldSolver();

		public abstract RotaryAxisSolver BuildRotaryAxisSolver();

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

		protected TMachineData m_MachineData;
		protected gp_Dir m_MasterRotateDir;
		protected gp_Dir m_SlaveRotateDir;
		protected gp_Dir m_ToolDir;
	}

	internal class SpindleSolverBuilder : SolverBuilderBase<SpindleTypeMachineData>, ISolverBuilder
	{
		public SpindleSolverBuilder( SpindleTypeMachineData machineData )
			: base( machineData )
		{
		}

		public override FKSolver BuildFKSolver()
		{
			gp_Vec mcsToSlave = new gp_Vec( m_MachineData.ToolToSlaveVec.XYZ() ); // DE
			gp_Vec slaveToMaster = new gp_Vec( m_MachineData.SlaveToMasterVec.XYZ() ); // EF
			gp_Vec toolVec = new gp_Vec( m_ToolDir );
			toolVec.Multiply( m_MachineData.ToolLength );
			return new FKSolver( mcsToSlave, slaveToMaster, toolVec, m_MasterRotateDir, m_SlaveRotateDir );
		}

		public override IKSolver BuildIKSolver()
		{
			return new IKSolver( m_ToolDir, m_MasterRotateDir, m_SlaveRotateDir );
		}

		public override WorkPieceToWorldSolver BuildWorkPieceToWorldSolver()
		{
			return new SpindleTypeWorkPieceToWorldSolver();
		}

		public override RotaryAxisSolver BuildRotaryAxisSolver()
		{
			gp_Pnt ptOnSlave = new gp_Pnt();
			ptOnSlave.Translate( m_MachineData.ToolToSlaveVec );
			gp_Pnt ptOnMaster = new gp_Pnt( ptOnSlave.XYZ() );
			ptOnMaster.Translate( m_MachineData.SlaveToMasterVec );
			return new RotaryAxisSolver( ptOnMaster, ptOnSlave, m_MasterRotateDir, m_SlaveRotateDir );
		}
	}

	internal class TableSolverBuilder : SolverBuilderBase<TableTypeMachineData>, ISolverBuilder
	{
		public TableSolverBuilder( TableTypeMachineData machineData )
			: base( machineData )
		{
		}

		public override FKSolver BuildFKSolver()
		{
			gp_Vec mcsToSlave = new gp_Vec( m_MachineData.MCSToMasterVec.XYZ() ); // DE
			gp_Vec slaveToMaster = new gp_Vec( m_MachineData.MasterToSlaveVec.XYZ() ); // EF
			gp_Vec toolVec = new gp_Vec( m_ToolDir );
			toolVec.Multiply( m_MachineData.ToolLength );
			return new FKSolver( mcsToSlave, slaveToMaster, toolVec, m_SlaveRotateDir, m_MasterRotateDir );
		}

		public override IKSolver BuildIKSolver()
		{
			// for table type, just exchange the master and slave axis
			return new IKSolver( m_ToolDir, m_SlaveRotateDir, m_MasterRotateDir );
		}

		public override WorkPieceToWorldSolver BuildWorkPieceToWorldSolver()
		{
			gp_Pnt ptOnMaster = new gp_Pnt();
			ptOnMaster.Translate( m_MachineData.MCSToMasterVec );
			gp_Pnt ptOnSlave = new gp_Pnt( ptOnMaster.XYZ() );
			ptOnSlave.Translate( m_MachineData.MasterToSlaveVec );
			return new TableTypeWorkPieceToWorldSolver( ptOnMaster, ptOnSlave, m_MasterRotateDir, m_SlaveRotateDir );
		}

		public override RotaryAxisSolver BuildRotaryAxisSolver()
		{
			gp_Pnt ptOnMaster = new gp_Pnt();
			ptOnMaster.Translate( m_MachineData.MCSToMasterVec );
			gp_Pnt ptOnSlave = new gp_Pnt( ptOnMaster.XYZ() );
			ptOnSlave.Translate( m_MachineData.MasterToSlaveVec );
			return new RotaryAxisSolver( ptOnMaster, ptOnSlave, m_MasterRotateDir, m_SlaveRotateDir );
		}
	}

	internal class MixSolverBuilder : SolverBuilderBase<MixTypeMachineData>, ISolverBuilder
	{
		public MixSolverBuilder( MixTypeMachineData machineData )
			: base( machineData )
		{
		}

		public override FKSolver BuildFKSolver()
		{
			gp_Vec mcsToSlave = new gp_Vec( m_MachineData.ToolToMasterVec.XYZ() ); // DE
			gp_Vec slaveToMaster = m_MachineData.MCSToSlaveVec - m_MachineData.ToolToMasterVec; // EF = DF - DE
			gp_Vec toolVec = new gp_Vec( m_ToolDir );
			toolVec.Multiply( m_MachineData.ToolLength );
			return new FKSolver( mcsToSlave, slaveToMaster, toolVec, m_MasterRotateDir, m_SlaveRotateDir );
		}

		public override IKSolver BuildIKSolver()
		{
			// for mix type, just exchange the master and slave axis
			return new IKSolver( m_ToolDir, m_SlaveRotateDir, m_MasterRotateDir );
		}

		public override WorkPieceToWorldSolver BuildWorkPieceToWorldSolver()
		{
			gp_Pnt ptOnSlave = new gp_Pnt();
			ptOnSlave.Translate( m_MachineData.ToolToMasterVec );
			return new MixTypeWorkPieceToWorldSolver( ptOnSlave, m_SlaveRotateDir );
		}

		public override RotaryAxisSolver BuildRotaryAxisSolver()
		{
			gp_Pnt ptOnMaster = new gp_Pnt();
			ptOnMaster.Translate( m_MachineData.ToolToMasterVec );
			gp_Pnt ptOnSlave = new gp_Pnt();
			ptOnSlave.Translate( m_MachineData.MCSToSlaveVec );
			return new RotaryAxisSolver( ptOnMaster, ptOnSlave, m_MasterRotateDir, m_SlaveRotateDir );
		}
	}
}
