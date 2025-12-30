using MyCAM.Data;
using OCC.gp;
using PostTool.Interop;
using System;

namespace MyCAM.Post
{
	/// <summary>
	/// Constants for IK/FK solver
	/// </summary>
	internal static class SolverConstants
	{
		// we are not using different system, so just set it to 1
		public const double IU_TO_BLU_ROTARY = 1;
	}

	public enum IKSolveResult
	{
		NoError = 0,
		NoSolution = 1,
		MasterInfinityOfSolution = 2,
		SlaveInfinityOfSolution = 3,
		InvalidInput = 4,
		OutOfRange = 5,
	}

	/// <summary>
	/// this is design based on spindle type
	/// for table and mix type, just exchange the master and slave axis
	/// </summary>
	internal class IKSolver
	{
		public IKSolver( gp_Dir toolDir, gp_Dir masterRotateDir, gp_Dir slaveRotateDir,
			double masterAxisStart_deg, double masterAxisEnd_deg,
			double slaveAxisStart_deg, double slaveAxisEnd_deg, bool isReverseMS )
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
			m_ToolDir = new double[ 3 ] { toolDir.X(), toolDir.Y(), toolDir.Z() };
			m_MasterRotateDir = new double[ 3 ] { masterRotateDir.X(), masterRotateDir.Y(), masterRotateDir.Z() };
			m_SlaveRotateDir = new double[ 3 ] { slaveRotateDir.X(), slaveRotateDir.Y(), slaveRotateDir.Z() };
			m_isReverseMS = isReverseMS;
			m_MasterAxisStart = masterAxisStart_deg * Math.PI / 180.0;
			m_MasterAxisEnd = masterAxisEnd_deg * Math.PI / 180.0;
			m_SlaveAxisStart = slaveAxisStart_deg * Math.PI / 180.0;
			m_SlaveAxisEnd = slaveAxisEnd_deg * Math.PI / 180.0;
		}

		// the angle for spindle is right-handed, for table is left-handed
		public IKSolveResult Solve( gp_Dir toolVec_In, double dM_In, double dS_In, out double dM_Out, out double dS_Out )
		{
			// prevent from sigular area
			if( toolVec_In.IsEqual( new gp_Dir( m_MasterRotateDir[ 0 ], m_MasterRotateDir[ 1 ], m_MasterRotateDir[ 2 ] ), SINGULAR_AREA_DEGREE * Math.PI / 180 ) ) {

				// just make it singular to prevent unexpected result
				toolVec_In = new gp_Dir( m_MasterRotateDir[ 0 ], m_MasterRotateDir[ 1 ], m_MasterRotateDir[ 2 ] );
			}
			else if( toolVec_In.IsOpposite( new gp_Dir( m_MasterRotateDir[ 0 ], m_MasterRotateDir[ 1 ], m_MasterRotateDir[ 2 ] ), SINGULAR_AREA_DEGREE * Math.PI / 180 ) ) {
				toolVec_In = new gp_Dir( -m_MasterRotateDir[ 0 ], -m_MasterRotateDir[ 1 ], -m_MasterRotateDir[ 2 ] );
			}

			// calculate the M and S angle
			double[] ToolDirection = new double[ 3 ] { toolVec_In.X(), toolVec_In.Y(), toolVec_In.Z() };
			int solveResult = FiveAxisSolver.IJKtoMS( ToolDirection, m_ToolDir, m_MasterRotateDir, m_SlaveRotateDir,
				dM_In, dS_In, out double dM1, out double dS1, out double dM2, out double dS2, SolverConstants.IU_TO_BLU_ROTARY );

			// no solution
			if( solveResult == (int)IKSolveResult.NoSolution ) {
				dM_Out = 0;
				dS_Out = 0;
				return IKSolveResult.NoSolution;
			}

			// the system is solvable, choose the best solution using FiveAxisSolver.ChooseSolution
			// Note: Rotation directions are fixed to 1 (right-hand) for both axes
			int chooseResult = FiveAxisSolver.ChooseSolution(
				dM1, dS1, dM2, dS2,
				dM_In, dS_In,
				out dM_Out, out dS_Out,
				SolutionType.ShortestDist,
				m_MasterAxisStart, m_MasterAxisEnd,
				m_SlaveAxisStart, m_SlaveAxisEnd,
				1,  // Master axis rotation direction (right-hand)
				1,  // Slave axis rotation direction (right-hand)
				SolverConstants.IU_TO_BLU_ROTARY );

			if( chooseResult != (int)IKSolveResult.NoError ) {

				// solvanle but can not choose a valid solution within range
				return IKSolveResult.OutOfRange;
			}
			return (IKSolveResult)solveResult;
		}

		public bool IsReverseMS
		{
			get
			{
				return m_isReverseMS;
			}
		}

		// machine properties
		double[] m_ToolDir;
		double[] m_MasterRotateDir;
		double[] m_SlaveRotateDir;
		bool m_isReverseMS;

		// axis limits (0,0 means no limit)
		double m_MasterAxisStart;
		double m_MasterAxisEnd;
		double m_SlaveAxisStart;
		double m_SlaveAxisEnd;

		// singular area
		const double SINGULAR_AREA_DEGREE = 5.0;
	}

	internal interface FKSolver
	{
		gp_Vec Solve( double masterAngle, double slaveAngle, gp_Vec G54XYZ, gp_Vec G54Offset );
	}

	internal class SpindleTypeFKSolver : FKSolver
	{
		public SpindleTypeFKSolver( gp_Vec toolToSlave, gp_Vec slaveToMaster, gp_Vec toolVec, gp_Dir masterRotateDir, gp_Dir slaveRotateDir )
		{
			// give a defaul Z dir BC type
			if( toolToSlave == null ) {
				toolToSlave = new gp_Vec( 0, 0, 0 );
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
			m_ToolToSlave = toolToSlave;
			m_SlaveToMaster = slaveToMaster;
			m_ToolVec = toolVec;
			m_MasterRotateDir = masterRotateDir;
			m_SlaveRotateDir = slaveRotateDir;
		}

		public gp_Vec Solve( double masterAngle, double slaveAngle, gp_Vec G54XYZ, gp_Vec G54Offset )
		{
			// original pt on slave coord
			gp_Pnt ptOnSlave = new gp_Pnt();
			ptOnSlave.Translate( m_ToolToSlave.Reversed() );

			// original pt on master coord
			gp_Pnt ptOnMaster0 = ptOnSlave.Translated( m_SlaveToMaster.Reversed() );

			// rotate the pt on slave coord
			ptOnSlave.Translate( m_ToolVec.Reversed() );
			gp_Trsf slaveTrsf = new gp_Trsf();
			slaveTrsf.SetRotation( new gp_Ax1( new gp_Pnt(), m_SlaveRotateDir ), slaveAngle );
			ptOnSlave.Transform( slaveTrsf );
			gp_Pnt ptOnMaster1 = ptOnSlave.Translated( m_SlaveToMaster.Reversed() );
			gp_Trsf masterTrsf = new gp_Trsf();
			masterTrsf.SetRotation( new gp_Ax1( new gp_Pnt(), m_MasterRotateDir ), masterAngle );
			ptOnMaster1.Transform( masterTrsf );

			// calculate the offset
			return new gp_Vec( ptOnMaster0.XYZ() - ptOnMaster1.XYZ() );
		}

		// machine properties
		gp_Vec m_ToolToSlave;
		gp_Vec m_SlaveToMaster;
		gp_Vec m_ToolVec;
		gp_Dir m_MasterRotateDir;
		gp_Dir m_SlaveRotateDir;
	}

	internal class TableTypeFKSolver : FKSolver
	{
		public TableTypeFKSolver( gp_Vec mcsToSlave, gp_Vec slaveToMaster, gp_Vec toolVec, gp_Dir masterRotateDir, gp_Dir slaveRotateDir )
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
			m_MCSToMaster = mcsToSlave;
			m_MasterToSlave = slaveToMaster;
			m_ToolVec = toolVec;
			m_MasterRotateDir = masterRotateDir;
			m_SlaveRotateDir = slaveRotateDir;
		}

		public gp_Vec Solve( double masterAngle, double slaveAngle, gp_Vec G54XYZ, gp_Vec G54Offset )
		{
			// original pt on slave coord
			gp_Pnt ptOnSlave = new gp_Pnt();
			ptOnSlave.Translate( m_MasterToSlave.Reversed() );
			ptOnSlave.Translate( m_MCSToMaster.Reversed() );
			ptOnSlave.Translate( G54XYZ );
			ptOnSlave.Translate( G54Offset );

			// original pt on master coord
			gp_Pnt ptOnMaster0 = ptOnSlave.Translated( m_MasterToSlave );

			// rotate the pt on slave coord
			gp_Trsf slaveTrsf = new gp_Trsf();
			slaveTrsf.SetRotation( new gp_Ax1( new gp_Pnt(), m_SlaveRotateDir ), slaveAngle );
			ptOnSlave.Transform( slaveTrsf );
			gp_Pnt ptOnMaster1 = ptOnSlave.Translated( m_MasterToSlave );
			gp_Trsf masterTrsf = new gp_Trsf();
			masterTrsf.SetRotation( new gp_Ax1( new gp_Pnt(), m_MasterRotateDir ), masterAngle );
			ptOnMaster1.Transform( masterTrsf );

			// calculate the offset
			return new gp_Vec( ptOnMaster1.XYZ() - ptOnMaster0.XYZ() ) + m_ToolVec; // offset tool vector
		}

		// machine properties
		gp_Vec m_MCSToMaster;
		gp_Vec m_MasterToSlave;
		gp_Vec m_ToolVec;
		gp_Dir m_MasterRotateDir;
		gp_Dir m_SlaveRotateDir;
	}

	internal class MixTypeFKSolver : FKSolver
	{
		public MixTypeFKSolver( gp_Vec toolToMaster, gp_Vec mcsToSlave, gp_Vec toolVec, gp_Dir masterRotateDir, gp_Dir slaveRotateDir )
		{
			// give a defaul Z dir BC type
			if( toolToMaster == null ) {
				toolToMaster = new gp_Vec( 0, 0, 0 );
			}
			if( mcsToSlave == null ) {
				mcsToSlave = new gp_Vec( 0, 0, 0 );
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
			m_ToolToMaster = toolToMaster;
			m_MCSToSlave = mcsToSlave;
			m_ToolVec = toolVec;
			m_MasterRotateDir = masterRotateDir;
			m_SlaveRotateDir = slaveRotateDir;
		}

		public gp_Vec Solve( double masterAngle, double slaveAngle, gp_Vec G54XYZ, gp_Vec G54Offset )
		{
			// the effect form slave
			gp_Pnt ptOnSlave = new gp_Pnt();
			ptOnSlave.Translate( m_MCSToSlave.Reversed() );
			ptOnSlave.Translate( G54XYZ );
			ptOnSlave.Translate( G54Offset );
			gp_Trsf slaveTrsf = new gp_Trsf();
			slaveTrsf.SetRotation( new gp_Ax1( new gp_Pnt(), m_SlaveRotateDir ), slaveAngle );
			gp_Pnt ptOnSlave1 = ptOnSlave.Transformed( slaveTrsf );
			gp_Vec slaveOffset = new gp_Vec( ptOnSlave1.XYZ() - ptOnSlave.XYZ() );

			// the effect from master
			gp_Pnt ptOnMaster0 = new gp_Pnt();
			ptOnMaster0.Translate( m_ToolToMaster.Reversed() );
			gp_Pnt ptOnMaster1 = ptOnMaster0.Translated( m_ToolVec.Reversed() );
			gp_Trsf masterTrsf = new gp_Trsf();
			masterTrsf.SetRotation( new gp_Ax1( new gp_Pnt(), m_MasterRotateDir ), masterAngle );
			ptOnMaster1.Transform( masterTrsf );
			gp_Vec masterOffset = new gp_Vec( ptOnMaster0.XYZ() - ptOnMaster1.XYZ() );

			// total offset
			return slaveOffset + masterOffset;
		}

		// machine properties
		gp_Vec m_ToolToMaster;
		gp_Vec m_MCSToSlave;
		gp_Vec m_ToolVec;
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
		}

		public IKSolveResult SolveIK( IProcessPoint point, double dM_In, double dS_In, out double dM_Out, out double dS_Out )
		{
			dM_Out = 0;
			dS_Out = 0;
			if( point == null || point.ToolVec == null ) {
				return IKSolveResult.InvalidInput;
			}

			// swap the input master and slave axis if needed
			if( m_IKSolver.IsReverseMS ) {
				(dS_In, dM_In) = (dM_In, dS_In);
			}
			IKSolveResult ikResult = m_IKSolver.Solve( point.ToolVec, dM_In, dS_In, out dM_Out, out dS_Out );

			// swap the output master and slave axis if needed
			if( m_IKSolver.IsReverseMS ) {
				(dS_Out, dM_Out) = (dM_Out, dS_Out);

				// machine type is table type, need to exchange the ik result
				if( ikResult == IKSolveResult.MasterInfinityOfSolution ) {
					ikResult = IKSolveResult.SlaveInfinityOfSolution;
				}
				else if( ikResult == IKSolveResult.SlaveInfinityOfSolution ) {
					ikResult = IKSolveResult.MasterInfinityOfSolution;
				}
			}
			return ikResult;
		}

		public gp_Vec SolveFK( double dM, double dS, gp_Pnt pointG54 )
		{
			if( pointG54 == null ) {
				return new gp_Vec();
			}
			return m_FKSolver.Solve( dM, dS, new gp_Vec( pointG54.XYZ() ), m_G54Offset );
		}

		public gp_Vec G54Offset
		{
			set
			{
				m_G54Offset = new gp_Vec( value.X(), value.Y(), value.Z() );
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
		gp_Vec m_G54Offset = new gp_Vec();
	}

	internal interface ISolverBuilder
	{
		FKSolver BuildFKSolver();

		IKSolver BuildIKSolver();
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
		}

		public abstract FKSolver BuildFKSolver();

		public abstract IKSolver BuildIKSolver();

		protected TMachineData m_MachineData;
	}

	internal class SpindleSolverBuilder : SolverBuilderBase<SpindleTypeMachineData>, ISolverBuilder
	{
		public SpindleSolverBuilder( SpindleTypeMachineData machineData )
			: base( machineData )
		{
		}

		public override FKSolver BuildFKSolver()
		{
			gp_Vec mcsToSlave = new gp_Vec( m_MachineData.ToolToSlaveVec.XYZ() );
			gp_Vec slaveToMaster = new gp_Vec( m_MachineData.SlaveToMasterVec.XYZ() );
			gp_Vec toolVec = new gp_Vec( m_MachineData.ToolDir );
			toolVec.Multiply( m_MachineData.ToolLength );
			return new SpindleTypeFKSolver( mcsToSlave, slaveToMaster, toolVec, m_MachineData.MasterRotateDir, m_MachineData.SlaveRotateDir );
		}

		public override IKSolver BuildIKSolver()
		{
			return new IKSolver( m_MachineData.ToolDir, m_MachineData.MasterRotateDir, m_MachineData.SlaveRotateDir,
				m_MachineData.MasterAxisStart_deg, m_MachineData.MasterAxisEnd_deg,
				m_MachineData.SlaveAxisStart_deg, m_MachineData.SlaveAxisEnd_deg, false );
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
			gp_Vec mcsToMaster = new gp_Vec( m_MachineData.MCSToMasterVec.XYZ() );
			gp_Vec masterToSlave = new gp_Vec( m_MachineData.MasterToSlaveVec.XYZ() );
			gp_Vec toolVec = new gp_Vec( m_MachineData.ToolDir );
			toolVec.Multiply( m_MachineData.ToolLength );

			// the table follow left-hand rule
			return new TableTypeFKSolver( mcsToMaster, masterToSlave, toolVec, m_MachineData.MasterRotateDir.Reversed(), m_MachineData.SlaveRotateDir.Reversed() );
		}

		public override IKSolver BuildIKSolver()
		{
			// for table type, just exchange the master and slave axis
			return new IKSolver( m_MachineData.ToolDir, m_MachineData.SlaveRotateDir, m_MachineData.MasterRotateDir,
				m_MachineData.SlaveAxisStart_deg, m_MachineData.SlaveAxisEnd_deg,
				m_MachineData.MasterAxisStart_deg, m_MachineData.MasterAxisEnd_deg, true );
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
			gp_Vec toolToMaster = new gp_Vec( m_MachineData.ToolToMasterVec.XYZ() );
			gp_Vec mcsToSLave = new gp_Vec( m_MachineData.MCSToSlaveVec.XYZ() );
			gp_Vec toolVec = new gp_Vec( m_MachineData.ToolDir );
			toolVec.Multiply( m_MachineData.ToolLength );

			// the table follow left-hand rule
			return new MixTypeFKSolver( toolToMaster, mcsToSLave, toolVec, m_MachineData.MasterRotateDir, m_MachineData.SlaveRotateDir.Reversed() );
		}

		public override IKSolver BuildIKSolver()
		{
			// for mix type, just exchange the master and slave axis
			return new IKSolver( m_MachineData.ToolDir, m_MachineData.SlaveRotateDir, m_MachineData.MasterRotateDir,
				m_MachineData.SlaveAxisStart_deg, m_MachineData.SlaveAxisEnd_deg,
				m_MachineData.MasterAxisStart_deg, m_MachineData.MasterAxisEnd_deg, true );
		}
	}
}

