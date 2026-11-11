using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.PathCache;
using MyCAM.Post;
using OCC.gp;

public static class ExternalAxisKinematicsTests
{
	static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	static readonly Assembly App = typeof( CraftData ).Assembly;
	static readonly ExternalAxisPos Assigned = new ExternalAxisPos( 37, -53 );
	static readonly gp_Vec Offset = new gp_Vec( 17, -23, 31 );

	static void Assert( bool condition, string message )
	{
		if( !condition ) { throw new Exception( message ); }
	}

	static void Close( double actual, double expected )
	{
		Assert( Math.Abs( actual - expected ) < 1e-8, "Expected " + expected + ", got " + actual );
	}

	static void Point( gp_Pnt actual, gp_Pnt expected )
	{
		Close( actual.X(), expected.X() ); Close( actual.Y(), expected.Y() ); Close( actual.Z(), expected.Z() );
	}

	static void Vector( gp_Vec actual, gp_Vec expected )
	{
		Close( actual.X(), expected.X() ); Close( actual.Y(), expected.Y() ); Close( actual.Z(), expected.Z() );
	}

	static void Direction( gp_Dir actual, gp_Dir expected )
	{
		Close( actual.X(), expected.X() ); Close( actual.Y(), expected.Y() ); Close( actual.Z(), expected.Z() );
	}

	static object New( string type, params object[] args )
	{
		return Activator.CreateInstance( App.GetType( type, true ), Flags, null, args, null );
	}

	static object Invoke( string type, string method, object instance, params object[] args )
	{
		return App.GetType( type, true ).GetMethods( Flags ).Single( m => m.Name == method && m.GetParameters().Length == args.Length ).Invoke( instance, args );
	}

	static FiveAxisMachineData Machine( string kind, ExternalAxisType type )
	{
		FiveAxisMachineData machine;
		if( kind == "Spindle" ) {
			machine = new SpindleTypeMachineData { ToolToSlaveVec = new gp_Vec( 4, 5, 6 ), SlaveToMasterVec = new gp_Vec( 7, 8, 9 ) };
		} else if( kind == "Mix" ) {
			machine = new MixTypeMachineData { ToolToMasterVec = new gp_Vec( 4, 5, 6 ), MCSToSlaveVec = new gp_Vec( 11, 13, 19 ) };
		} else {
			machine = new TableTypeMachineData { MCSToMasterVec = new gp_Vec( 4, 5, 6 ), MasterToSlaveVec = new gp_Vec( 7, 8, 9 ) };
		}
		machine.ToolDirection = ToolDirection.Z;
		machine.MasterRotaryAxis = kind == "Mix" ? RotaryAxis.X : RotaryAxis.Z;
		machine.SlaveRotaryAxis = kind == "Mix" ? RotaryAxis.Z : RotaryAxis.X;
		machine.MasterTiltedVec_deg = new gp_XYZ();
		machine.SlaveTiltedVec_deg = new gp_XYZ();
		machine.ToolLength = 12;
		machine.ExternalAxes.Type = type;
		machine.ExternalAxes.Ext1RotaryAxis = RotaryAxis.Z;
		machine.ExternalAxes.Ext2RotaryAxis = RotaryAxis.X;
		machine.ExternalAxes.MCSToExt1Vec = new gp_Vec( 10, 20, 30 );
		machine.ExternalAxes.Ext1ToExt2Vec = new gp_Vec( 5, 7, 11 );
		machine.InitRotateAxes( RotaryDirection.RightHand, 0, 0, RotaryDirection.LeftHand, 0, 0 );
		return machine;
	}

	static PostSolver Solver( FiveAxisMachineData machine )
	{
		return (PostSolver)New( "MyCAM.Post.PostSolver", machine );
	}

	// Independent principal-axis reference, not the production OCC transform.
	static gp_Pnt Rotate( gp_Pnt point, gp_Vec center, RotaryAxis axis, double degrees )
	{
		double x = point.X() - center.X(), y = point.Y() - center.Y(), z = point.Z() - center.Z();
		double angle = -degrees * Math.PI / 180.0, c = Math.Cos( angle ), s = Math.Sin( angle );
		if( axis == RotaryAxis.X ) { return new gp_Pnt( x + center.X(), c * y - s * z + center.Y(), s * y + c * z + center.Z() ); }
		if( axis == RotaryAxis.Y ) { return new gp_Pnt( c * x + s * z + center.X(), y + center.Y(), -s * x + c * z + center.Z() ); }
		return new gp_Pnt( c * x - s * y + center.X(), s * x + c * y + center.Y(), z + center.Z() );
	}

	static gp_Pnt Positioned( FiveAxisMachineData machine, gp_Pnt point, ExternalAxisPos position, gp_Vec offset )
	{
		gp_Pnt expected = point.Translated( offset );
		if( machine.ExternalAxes.Type == ExternalAxisType.Dual ) {
			expected = Rotate( expected, machine.ExternalAxes.MCSToExt1Vec + machine.ExternalAxes.Ext1ToExt2Vec, machine.ExternalAxes.Ext2RotaryAxis, position.Ext2 );
		}
		if( machine.ExternalAxes.Type != ExternalAxisType.None ) {
			expected = Rotate( expected, machine.ExternalAxes.MCSToExt1Vec, machine.ExternalAxes.Ext1RotaryAxis, position.Ext1 );
		}
		return expected.Translated( offset.Reversed() );
	}

	public static void NoExternalAxisAndZeroRegression()
	{
		foreach( string kind in new[] { "Spindle", "Mix", "Table" } ) {
			FiveAxisMachineData machine = Machine( kind, ExternalAxisType.None );
			PostSolver solver = Solver( machine );
			gp_Pnt point = new gp_Pnt( 1, 2, 3 );
			Point( solver.TransformExternalPosition( point, Assigned, Offset ), point );
			Vector( solver.GetExternalDisplacement( point, Assigned, Offset ), new gp_Vec() );
			Vector( solver.SolveFK( .4, .6, point, Offset, Assigned ), solver.SolveFK( .4, .6, point, Offset ) );
			Direction( solver.SolveToolVec( .4, .6, Assigned ), solver.SolveToolVec( .4, .6 ) );
		}
		foreach( string kind in new[] { "Spindle", "Mix" } ) {
			PostSolver solver = Solver( Machine( kind, ExternalAxisType.Single ) );
			gp_Pnt point = new gp_Pnt( 1, 2, 3 );
			Point( solver.TransformExternalPosition( point, new ExternalAxisPos( 0, 123 ), Offset ), point );
			Vector( solver.SolveFK( .4, .6, point, Offset, default( ExternalAxisPos ) ), solver.SolveFK( .4, .6, point, Offset ) );
		}
	}

	public static void LeftHandConventionAndDirectionMetadata()
	{
		FiveAxisMachineData machine = Machine( "Spindle", ExternalAxisType.Single );
		foreach( RotaryDirection direction in new[] { RotaryDirection.None, RotaryDirection.RightHand, RotaryDirection.LeftHand } ) {
			machine.ExternalAxes.Ext1RotaryDirection = direction;
			machine.ExternalAxes.InitRotateAxes();
			PostSolver solver = Solver( machine );
			Point( solver.TransformExternalPosition( new gp_Pnt( 12, 20, 30 ), new ExternalAxisPos( 90, 0 ) ), new gp_Pnt( 10, 18, 30 ) );
			Direction( solver.TransformExternalDirection( new gp_Dir( 1, 0, 0 ), new ExternalAxisPos( 90, 0 ) ), new gp_Dir( 0, -1, 0 ) );
		}
	}

	public static void OffsetAndBiasedPivot()
	{
		foreach( RotaryAxis axis in new[] { RotaryAxis.X, RotaryAxis.Y, RotaryAxis.Z } ) {
			FiveAxisMachineData machine = Machine( "Spindle", ExternalAxisType.Single );
			machine.ExternalAxes.Ext1RotaryAxis = axis;
			machine.ExternalAxes.InitRotateAxes();
			gp_Pnt point = new gp_Pnt( 2, 3, 5 );
			PostSolver solver = Solver( machine );
			Point( solver.TransformExternalPosition( point, Assigned, Offset ), Positioned( machine, point, Assigned, Offset ) );
			Vector( solver.GetExternalDisplacement( point, Assigned, Offset ), new gp_Vec( point, Positioned( machine, point, Assigned, Offset ) ) );
			Point( point, new gp_Pnt( 2, 3, 5 ) );
			Vector( Offset, new gp_Vec( 17, -23, 31 ) );
		}
	}

	public static void DualParentOrder()
	{
		FiveAxisMachineData machine = Machine( "Spindle", ExternalAxisType.Dual );
		PostSolver solver = Solver( machine );
		gp_Pnt point = new gp_Pnt( 2, 3, 5 );
		gp_Pnt expected = Positioned( machine, point, Assigned, Offset );
		Point( solver.TransformExternalPosition( point, Assigned, Offset ), expected );
		gp_Pnt wrong = Rotate( Rotate( point.Translated( Offset ), machine.ExternalAxes.MCSToExt1Vec, RotaryAxis.Z, Assigned.Ext1 ), machine.ExternalAxes.MCSToExt1Vec + machine.ExternalAxes.Ext1ToExt2Vec, RotaryAxis.X, Assigned.Ext2 ).Translated( Offset.Reversed() );
		Assert( expected.Distance( wrong ) > 1, "Test does not discriminate parent order." );
	}

	public static void PositionDirectionAndVectorInverse()
	{
		foreach( ExternalAxisType type in new[] { ExternalAxisType.Single, ExternalAxisType.Dual } ) {
			PostSolver solver = Solver( Machine( "Spindle", type ) );
			foreach( ExternalAxisPos position in new[] { Assigned, new ExternalAxisPos( 360, -720 ), new ExternalAxisPos( -81, 119 ) } ) {
				gp_Pnt point = new gp_Pnt( 2, 3, 5 );
				Point( solver.InverseTransformExternalPosition( solver.TransformExternalPosition( point, position, Offset ), position, Offset ), point );
				gp_Dir direction = new gp_Dir( 2, 3, 5 );
				Direction( solver.InverseTransformExternalDirection( solver.TransformExternalDirection( direction, position ), position ), direction );
				gp_Vec vector = new gp_Vec( 2, 3, 5 );
				Vector( solver.InverseTransformExternalVector( solver.TransformExternalVector( vector, position ), position ), vector );
				Close( solver.TransformExternalVector( vector, position ).Magnitude(), vector.Magnitude() );
			}
		}
	}

	public static void TiltRetainedButNotApplied()
	{
		FiveAxisMachineData machine = Machine( "Spindle", ExternalAxisType.Dual );
		gp_Pnt point = new gp_Pnt( 2, 3, 5 );
		gp_Pnt expected = Solver( machine ).TransformExternalPosition( point, Assigned, Offset );
		machine.ExternalAxes.Ext1TiltedVec_deg = new gp_XYZ( 11, 13, 17 );
		machine.ExternalAxes.Ext2TiltedVec_deg = new gp_XYZ( 19, 23, 29 );
		machine.ExternalAxes.InitRotateAxes();
		Point( Solver( machine ).TransformExternalPosition( point, Assigned, Offset ), expected );
		Close( machine.ExternalAxes.Ext1TiltedVec_deg.X(), 11 );
		Close( machine.ExternalAxes.Ext2TiltedVec_deg.X(), 19 );
	}

	public static void IKAndToolVectorFrames()
	{
		foreach( string kind in new[] { "Spindle", "Mix" } ) {
			foreach( ExternalAxisType type in kind == "Mix" ? new[] { ExternalAxisType.Single } : new[] { ExternalAxisType.Single, ExternalAxisType.Dual } ) {
				PostSolver solver = Solver( Machine( kind, type ) );
				gp_Dir fiveAxisVector = solver.SolveToolVec( .4, .6 );
				gp_Dir originalVector = solver.InverseTransformExternalDirection( fiveAxisVector, Assigned );
				double expectedM, expectedS, actualM, actualS;
				IKSolveResult expected = solver.SolveIK( fiveAxisVector, .4, .6, out expectedM, out expectedS );
				IKSolveResult actual = solver.SolveIK( originalVector, .4, .6, out actualM, out actualS, Assigned );
				Assert( actual == expected && actual != IKSolveResult.InvalidInput && actual != IKSolveResult.NoSolution, "IK result differs from five-axis input." );
				Close( actualM, expectedM ); Close( actualS, expectedS );
				Direction( solver.SolveToolVec( actualM, actualS, Assigned ), solver.InverseTransformExternalDirection( solver.SolveToolVec( expectedM, expectedS ), Assigned ) );
				Direction( solver.SolveToolVec( .4, .6, Assigned ), originalVector );
			}
		}
	}

	public static void RTCPDoesNotIncludeExternalDisplacement()
	{
		foreach( string kind in new[] { "Spindle", "Mix" } ) {
			FiveAxisMachineData machine = Machine( kind, ExternalAxisType.Single );
			PostSolver solver = Solver( machine );
			gp_Pnt original = new gp_Pnt( 2, 3, 5 );
			gp_Pnt positioned = Positioned( machine, original, Assigned, Offset );
			gp_Vec expected = solver.SolveFK( .4, .6, positioned, Offset );
			gp_Vec actual = solver.SolveFK( .4, .6, original, Offset, Assigned );
			Vector( actual, expected );
			Vector( actual, solver.SolveFKAtPositionedPoint( .4, .6, positioned, Offset ) );
			Assert( ( actual - ( expected + solver.GetExternalDisplacement( original, Assigned, Offset ) ) ).Magnitude() > 1, "RTCP includes external displacement." );
			if( kind == "Mix" ) {
				Assert( ( actual - solver.SolveFK( .4, .6, original, Offset ) ).Magnitude() > 1, "Mix did not use positioned point." );
				MixTypeMachineData mix = (MixTypeMachineData)machine;
				gp_Pnt mcs = positioned.Translated( Offset );
				gp_Pnt turned = Rotate( mcs, mix.MCSToSlaveVec, mix.SlaveRotaryAxis, .6 * 180 / Math.PI );
				gp_Vec tableDelta = new gp_Vec( mcs, turned );
				Vector( actual, tableDelta + solver.SolveFK( .4, 0, positioned, Offset ) );
			}
		}
	}

	public static void FKIntermediatePointAppliesPositioningOnce()
	{
		foreach( string kind in new[] { "Spindle", "Mix" } ) {
			foreach( ExternalAxisType type in kind == "Mix" ? new[] { ExternalAxisType.Single } : new[] { ExternalAxisType.Single, ExternalAxisType.Dual } ) {
				FiveAxisMachineData machine = Machine( kind, type );
				PostSolver solver = Solver( machine );
				PostPoint input = new PostPoint { X = 2, Y = 3, Z = 5, Master = .4, Slave = .6, ExternalAxisPos = Assigned };
				object[] args = { solver, input, null, Offset };
				Assert( (bool)Invoke( "MyCAM.Helper.FiveAxisSimuPathBuilder", "BuildFKPostPntFiveAxis", null, args ), "FK point failed." );
				PostPoint output = (PostPoint)args[2];
				gp_Pnt positioned = Positioned( machine, new gp_Pnt( 2, 3, 5 ), Assigned, Offset );
				gp_Pnt expected = positioned.Translated( solver.SolveFKAtPositionedPoint( .4, .6, positioned, Offset ) );
				Point( new gp_Pnt( output.X, output.Y, output.Z ), expected );
				Assert( output.ExternalAxisPos.Equals( Assigned ), "FK angles lost." );
				Close( output.Master, input.Master ); Close( output.Slave, input.Slave );
				Close( input.X, 2 ); Close( input.Y, 3 ); Close( input.Z, 5 );
			}
		}
	}

	static void Initialize( FiveAxisMachineData machine )
	{
		object manager = New( "MyCAM.Data.DataManager" );
		manager.GetType().GetProperty( "MachineData", Flags ).SetValue( manager, machine, null );
		Invoke( "MyCAM.Data.DataGettingHelper", "Initialize", null, manager );
	}

	static CADPoint CAD( double x, double y )
	{
		return new CADPoint( new gp_Pnt( x, y, 0 ), new gp_Dir( 1, 2, 3 ), new gp_Dir( 0, 1, 0 ), new gp_Dir( 1, 0, 0 ) );
	}

	public static void CacheUsesAssignedAnglesBeforeIK()
	{
		FiveAxisMachineData machine = Machine( "Spindle", ExternalAxisType.Dual );
		Initialize( machine );
		PostSolver solver = Solver( machine );
		CraftData craft = new CraftData { ExternalAxisPos = Assigned };
		ContourCache cache = new ContourCache( new ContourGeomData( new List<CADPoint> { CAD( 10, 0 ), CAD( 10, 10 ), CAD( 0, 10 ) }, new Dictionary<CADPoint, CADPoint>(), false ), craft );
		CAMPoint first = cache.MainPathPointList[0];
		double m, s;
		solver.SolveIK( first.InitToolVec, craft.InitMaster_rad, craft.InitSlave_rad, out m, out s, Assigned );
		Close( first.InitMaster_rad, m ); Close( first.InitSlave_rad, s );
		Direction( first.ToolVec, solver.SolveToolVec( first.ModMaster_rad, first.ModSlave_rad, Assigned ) );
		StdPatternCacheBase circle = (StdPatternCacheBase)New( "MyCAM.PathCache.CircleCache", new CircleGeomData( new gp_Ax1( new gp_Pnt(), new gp_Dir( 1, 2, 3 ) ), 20, 0, false ), craft );
		CAMPoint reference = circle.RefPoint;
		solver.SolveIK( reference.InitToolVec, craft.InitMaster_rad, craft.InitSlave_rad, out m, out s, Assigned );
		Close( reference.InitMaster_rad, m ); Close( reference.InitSlave_rad, s );
		Assert( circle.MainPathPointList.Count > 0, "Empty standard path." );
	}

	public static void LocalABAndAssignedLockedAngles()
	{
		FiveAxisMachineData machine = Machine( "Spindle", ExternalAxisType.Dual );
		Initialize( machine );
		PostSolver solver = Solver( machine );
		CAMPoint point = new CAMPoint( new CADPoint( new gp_Pnt( 10, 0, 0 ), new gp_Dir( 1, 2, 3 ), new gp_Dir( 0, 1, 0 ), new gp_Dir( 2, -1, 0 ) ) );
		point.ExternalAxisPos = Assigned;
		Close( point.InitToolVec.Dot( point.TangentVec ), 0 );
		gp_Dir desired = (gp_Dir)Invoke( "MyCAM.Helper.ToolVecHelper", "ConvertABAngleToToolVec", null, point, 10.0, -12.0 );
		Tuple<double, double> ms = ToolVecHelper.GetMSAngleFromABAngle( 10, -12, point );
		Direction( ToolVecHelper.ConvertMSAngleToToolVec( ms.Item1, ms.Item2, Assigned ), desired );
		Tuple<double, double> ab = ToolVecHelper.GetABAngleFromMSAngle( ms.Item1, ms.Item2, point );
		Direction( (gp_Dir)Invoke( "MyCAM.Helper.ToolVecHelper", "ConvertABAngleToToolVec", null, point, ab.Item1, ab.Item2 ), desired );
		List<ISetToolVecPoint> points = new List<ISetToolVecPoint> { point, point.Clone() };
		ToolVecHelper.SetToolVecByAssignedMS( ref points, 23, 41 );
		foreach( ISetToolVecPoint p in points ) { Direction( p.ToolVec, solver.SolveToolVec( p.ModMaster_rad, p.ModSlave_rad, Assigned ) ); }
		ToolVecHelper.SetToolVecByLockedAxis( ref points, true, Tuple.Create( 11.0, 0.0 ) );
		foreach( ISetToolVecPoint p in points ) { Direction( p.ToolVec, solver.SolveToolVec( p.ModMaster_rad, p.ModSlave_rad, Assigned ) ); }
	}

	public static void FlipAndSingularityStayInFiveAxisFrame()
	{
		FiveAxisMachineData machine = Machine( "Spindle", ExternalAxisType.Single );
		machine.ExternalAxes.Ext1RotaryAxis = RotaryAxis.X;
		machine.ExternalAxes.InitRotateAxes();
		Initialize( machine );
		PostSolver solver = Solver( machine );
		Assert( solver.IsToolVecSingular( 0, 0 ), "Zero pose should be singular." );
		Assert( !solver.SolveToolVec( 0, 0, new ExternalAxisPos( 90, 0 ) ).IsParallel( machine.MasterRotateDir, 1e-8 ), "Test does not distinguish frames." );
		Tuple<double, double> flipped = ToolVecHelper.FlipRotaryAxis( 0, 0, true );
		Close( flipped.Item1, 180 ); Close( flipped.Item2, 0 );
		gp_Dir before = solver.SolveToolVec( .4, .6, Assigned );
		flipped = ToolVecHelper.FlipRotaryAxis( .4 * 180 / Math.PI, .6 * 180 / Math.PI, true );
		Direction( solver.SolveToolVec( flipped.Item1 * Math.PI / 180, flipped.Item2 * Math.PI / 180, Assigned ), before );
	}

	public static void MachiningDirectionPhaseRemainsBeforeExternalPositioning()
	{
		FiveAxisMachineData machine = Machine( "Mix", ExternalAxisType.Single );
		Initialize( machine );
		foreach( EMachiningDir direction in new[] { EMachiningDir.RefAxisPositive, EMachiningDir.CrossAxisPositive,
			EMachiningDir.RefAxisNegative, EMachiningDir.CrossAxisNegative } ) {
			CraftData craft = new CraftData();
			craft.SetSpecialToolVecData( new SpecialToolVecData( machiningDir: direction, lockMaster: true, lockSlave: true ) );
			ContourCache cache = new ContourCache( new ContourGeomData( new List<CADPoint> { CAD( 10, 4 ), CAD( 10, 10 ), CAD( 0, 10 ) }, new Dictionary<CADPoint, CADPoint>(), false ), craft );
			CAMPoint before = cache.MainPathPointList[0].Clone();
			craft.ExternalAxisPos = new ExternalAxisPos( 90, 0 );
			CAMPoint after = cache.MainPathPointList[0];
			Point( after.Point, before.Point );
			Close( after.ModSlave_rad, before.ModSlave_rad );
			Assert( after.ExternalAxisPos.Ext1 == 90, "Cache did not retain the assigned external angle." );
			Direction( after.ToolVec, Solver( machine ).SolveToolVec( after.ModMaster_rad, after.ModSlave_rad, after.ExternalAxisPos ) );
		}
	}

	public static void SafePlaneRemainsInPrePositioningWorkpieceCoordinates()
	{
		FiveAxisMachineData machine = Machine( "Spindle", ExternalAxisType.Dual );
		Initialize( machine );
		CAMPoint start = new CAMPoint( CAD( 10, 4 ) );
		CAMPoint end = new CAMPoint( CAD( 20, 7 ) );
		TraverseData traverse = new TraverseData { TraverseMode = TraverseData.TraverseType.SafePlane,
			LiftUpDistance = 3, CutDownDistance = 4, SafePlaneDistance = 25 };
		TraversePathResult before, after;
		Assert( TraverseHelper.TryCalculateTraversePoints( start, end, traverse, out before ), "Zero-position safe plane failed." );
		start.ExternalAxisPos = end.ExternalAxisPos = Assigned;
		Assert( TraverseHelper.TryCalculateTraversePoints( start, end, traverse, out after ), "Positioned safe plane failed." );
		Point( after.SafePlaneLiftUpProjPoint, before.SafePlaneLiftUpProjPoint );
		Point( after.SafePlaneCutDownProjPoint, before.SafePlaneCutDownProjPoint );
		Close( after.SafePlaneLiftUpProjPoint.Z(), 25 );
		Assert( after.LiftUpPoint.ExternalAxisPos.Equals( Assigned ), "Safe-plane source lost external angles." );
		gp_Pnt positioned = Solver( machine ).TransformExternalPosition( after.SafePlaneLiftUpProjPoint, Assigned, Offset );
		Assert( positioned.Distance( after.SafePlaneLiftUpProjPoint ) > 1, "Test does not distinguish reference frames." );
	}
}
