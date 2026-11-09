using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using MyCAM.Data;
using MyCAM.FileManager;
using MyCAM.Helper;
using MyCAM.PathCache;
using MyCAM.Post;
using OCC.gp;

public static class ExternalAxisPointDataTests
{
	static readonly ExternalAxisPos Assigned = new ExternalAxisPos( 123.25, -46.5 );
	static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	static readonly Assembly App = typeof( CraftData ).Assembly;

	static void Assert( bool condition, string message )
	{
		if( !condition ) { throw new Exception( message ); }
	}

	static void Check( ExternalAxisPos actual )
	{
		Assert( actual.Equals( Assigned ), "External positions changed: " + actual.Ext1 + ", " + actual.Ext2 );
	}

	static object Invoke( string type, string method, object instance, params object[] arguments )
	{
		MethodInfo info = App.GetType( type, true ).GetMethods( Flags ).Single( m => m.Name == method && m.GetParameters().Length == arguments.Length );
		return info.Invoke( instance, arguments );
	}

	static object New( string type, params object[] arguments )
	{
		return Activator.CreateInstance( App.GetType( type, true ), Flags, null, arguments, null );
	}

	static CADPoint CAD( double x, double y, double z )
	{
		return new CADPoint( new gp_Pnt( x, y, z ), new gp_Dir( 0, 0, 1 ), new gp_Dir( 0, 1, 0 ), new gp_Dir( 1, 0, 0 ) );
	}

	static CAMPoint CAM( double x, double y, double z )
	{
		CAMPoint point = new CAMPoint( CAD( x, y, z ) );
		point.ExternalAxisPos = Assigned;
		point.ModMaster_rad = 0.25;
		point.ModSlave_rad = -0.5;
		return point;
	}

	static PostPoint Post( double x, double y, double z )
	{
		return new PostPoint { X = x, Y = y, Z = z, Master = 0.25, Slave = -0.5, ExternalAxisPos = Assigned };
	}

	static CraftDataDTO DTO( CraftData craft )
	{
		return (CraftDataDTO)New( "MyCAM.FileManager.CraftDataDTO", craft );
	}

	static CraftData FromDTO( CraftDataDTO dto )
	{
		return (CraftData)Invoke( "MyCAM.FileManager.CraftDataDTO", "ToCraftData", dto );
	}

	public static void DefaultsAndExactAssignedValues()
	{
		Assert( new CraftData().ExternalAxisPos.Equals( default( ExternalAxisPos ) ), "Craft default is not zero." );
		Assert( new CAMPoint( CAD( 0, 0, 0 ) ).ExternalAxisPos.Equals( default( ExternalAxisPos ) ), "CAM default is not zero." );
		Assert( new PostPoint().ExternalAxisPos.Equals( default( ExternalAxisPos ) ), "Post default is not zero." );
		Assert( !new ExternalAxisPos( 0, 0 ).Equals( new ExternalAxisPos( 360, 0 ) ), "Assigned turns were normalized." );
		Assert( !Assigned.Equals( new ExternalAxisPos( Assigned.Ext1, 0 ) ), "Second axis was discarded." );
	}

	public static void NotificationAndIndependentPaths()
	{
		CraftData first = new CraftData();
		CraftData second = new CraftData();
		int changes = 0;
		first.CAMFactorChanged += delegate { changes++; };
		object special = first.SpecialToolVecData;
		first.ExternalAxisPos = Assigned;
		second.ExternalAxisPos = first.ExternalAxisPos;
		first.ExternalAxisPos = Assigned;
		Assert( changes == 1, "Same assigned value triggered rebuild." );
		first.ExternalAxisPos = new ExternalAxisPos( 360, 17 );
		Assert( changes == 2, "Changed value did not notify." );
		Check( second.ExternalAxisPos );
		Assert( ReferenceEquals( special, first.SpecialToolVecData ), "Tool vector settings were replaced." );
	}

	public static void CraftXmlRoundTripAndLegacyDefault()
	{
		CraftData craft = new CraftData { ExternalAxisPos = Assigned };
		CraftDataDTO dto = DTO( craft );
		XmlSerializer serializer = new XmlSerializer( typeof( CraftDataDTO ) );
		StringWriter writer = new StringWriter();
		serializer.Serialize( writer, dto );
		CraftDataDTO restored = (CraftDataDTO)serializer.Deserialize( new StringReader( writer.ToString() ) );
		CraftData copied = FromDTO( restored );
		Check( copied.ExternalAxisPos );
		copied.ExternalAxisPos = new ExternalAxisPos( 360, 0 );
		Check( craft.ExternalAxisPos );
		Check( FromDTO( dto ).ExternalAxisPos );
		dto.ExternalAxisPos = null;
		Assert( FromDTO( dto ).ExternalAxisPos.Equals( default( ExternalAxisPos ) ), "Legacy project enabled external positions." );
	}

	public static void CAMAndInterfaceClones()
	{
		CAMPoint original = CAM( 1, 2, 3 );
		CAMPoint clone = original.Clone();
		Check( clone.ExternalAxisPos );
		Check( ( (IProcessPoint)original ).Clone().ExternalAxisPos );
		Check( ( (IOrientationPoint)original ).Clone().ExternalAxisPos );
		Check( ( (ISetToolVecPoint)original ).Clone().ExternalAxisPos );
		clone.ExternalAxisPos = new ExternalAxisPos( 360, 0 );
		Check( original.ExternalAxisPos );
	}

	public static void PostClone()
	{
		PostPoint original = Post( 1, 2, 3 );
		PostPoint clone = original.Clone();
		Check( clone.ExternalAxisPos );
		clone.ExternalAxisPos = new ExternalAxisPos( 360, 0 );
		Check( original.ExternalAxisPos );
	}

	public static void InitializeCaches()
	{
		object manager = New( "MyCAM.Data.DataManager" );
		Invoke( "MyCAM.Data.DataGettingHelper", "Initialize", null, manager );
	}

	static ArgumentException ExpectArgumentException( Action action )
	{
		try {
			action();
		}
		catch( TargetInvocationException ex ) {
			Assert( ex.InnerException is ArgumentException, "Unexpected deserialization error: " + ex.InnerException );
			return (ArgumentException)ex.InnerException;
		}
		catch( ArgumentException ex ) {
			return ex;
		}
		throw new Exception( "Invalid external angle was accepted." );
	}

	static CraftDataDTO XmlRoundTrip( CraftDataDTO dto )
	{
		XmlSerializer serializer = new XmlSerializer( typeof( CraftDataDTO ) );
		using( StringWriter writer = new StringWriter() ) {
			serializer.Serialize( writer, dto );
			using( StringReader reader = new StringReader( writer.ToString() ) ) {
				return (CraftDataDTO)serializer.Deserialize( reader );
			}
		}
	}

	public static void RejectNonFiniteExternalAnglesWithoutMutatingCraft()
	{
		CraftData craft = new CraftData { ExternalAxisPos = Assigned };
		int changes = 0;
		craft.CAMFactorChanged += delegate { changes++; };
		foreach( double invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity } ) {
			ArgumentException first = ExpectArgumentException( delegate { craft.ExternalAxisPos = new ExternalAxisPos( invalid, Assigned.Ext2 ); } );
			Assert( first is ArgumentOutOfRangeException && first.ParamName == "ext1", "Ext1 constructor error lost its parameter." );
			ArgumentException second = ExpectArgumentException( delegate { craft.ExternalAxisPos = new ExternalAxisPos( Assigned.Ext1, invalid ); } );
			Assert( second is ArgumentOutOfRangeException && second.ParamName == "ext2", "Ext2 constructor error lost its parameter." );
			Check( craft.ExternalAxisPos );
		}
		Assert( changes == 0, "Rejected input changed craft or notified caches." );
	}

	public static void CraftXmlRejectsNonFiniteExternalAngles()
	{
		foreach( double invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity } ) {
			for( int axis = 1; axis <= 2; axis++ ) {
				CraftDataDTO dto = DTO( new CraftData { ExternalAxisPos = Assigned } );
				if( axis == 1 ) { dto.ExternalAxisPos.Ext1 = invalid; }
				else { dto.ExternalAxisPos.Ext2 = invalid; }
				ArgumentException direct = ExpectArgumentException( delegate { FromDTO( dto ); } );
				Assert( direct.Message.Contains( "ExternalAxisPos.Ext" + axis ), "DTO error lost its axis." );
				CraftDataDTO loaded = XmlRoundTrip( dto );
				ArgumentException serialized = ExpectArgumentException( delegate { FromDTO( loaded ); } );
				Assert( serialized.Message.Contains( "ExternalAxisPos.Ext" + axis ), "XML load error lost its axis." );
			}
		}
	}

	public static void CraftDTORejectsIncompleteExternalAngles()
	{
		foreach( ExternalAxisPosDTO incomplete in new[] {
			new ExternalAxisPosDTO(),
			new ExternalAxisPosDTO { Ext1 = Assigned.Ext1 },
			new ExternalAxisPosDTO { Ext2 = Assigned.Ext2 }
		} ) {
			CraftDataDTO dto = DTO( new CraftData() );
			dto.ExternalAxisPos = incomplete;
			ArgumentException failure = ExpectArgumentException( delegate { FromDTO( dto ); } );
			Assert( failure.Message.Contains( "Missing required fields" ), "Incomplete DTO was not rejected." );
		}
		CraftDataDTO legacy = DTO( new CraftData { ExternalAxisPos = Assigned } );
		legacy.ExternalAxisPos = null;
		Assert( FromDTO( XmlRoundTrip( legacy ) ).ExternalAxisPos.Equals( default( ExternalAxisPos ) ), "Legacy XML without external fields did not load as zero." );
	}

	public static void FiniteMultipleTurnsAreNotClamped()
	{
		ExternalAxisPos position = new ExternalAxisPos( 720.125, -1080.5 );
		CraftData restored = FromDTO( XmlRoundTrip( DTO( new CraftData { ExternalAxisPos = position } ) ) );
		Assert( restored.ExternalAxisPos.Equals( position ), "Finite assigned angles were clamped or normalized." );
	}

	public static void ContourCacheRebuild()
	{
		CraftData craft = new CraftData { ExternalAxisPos = Assigned, OverCutLength = 2, LeadData = new LeadData( 2, 0, 90, 2, 0, 90 ) };
		ContourGeomData geom = new ContourGeomData( new List<CADPoint> { CAD( 10, 0, 0 ), CAD( 10, 10, 0 ), CAD( 0, 10, 0 ), CAD( 0, 0, 0 ) }, new Dictionary<CADPoint, CADPoint>(), true );
		ContourCache cache = new ContourCache( geom, craft );
		List<CAMPoint> old = cache.MainPathPointList;
		CheckCache( cache );
		gp_Pnt position = old[0].Point;
		double master = old[0].ModMaster_rad;
		craft.ExternalAxisPos = new ExternalAxisPos( 360, 17 );
		Assert( !ReferenceEquals( old, cache.MainPathPointList ), "Contour cache did not rebuild." );
		Assert( cache.MainPathPointList.All( p => p.ExternalAxisPos.Equals( craft.ExternalAxisPos ) ), "Contour rebuild lost positions." );
		Assert( position.Distance( cache.MainPathPointList[0].Point ) < 1e-9 && master == cache.MainPathPointList[0].ModMaster_rad, "Step2 changed kinematics." );
		Check( old[0].ExternalAxisPos );
	}

	static void CheckCache( IPathCache cache )
	{
		Assert( cache.MainPathPointList.Count > 0, "Empty main path." );
		foreach( CAMPoint p in cache.MainPathPointList.Concat( cache.LeadInPointList ).Concat( cache.LeadOutPointList ).Concat( cache.OverCutPointList ) ) { Check( p.ExternalAxisPos ); }
	}

	static void StandardCache( IStdPatternGeomData geom )
	{
		CraftData craft = new CraftData { ExternalAxisPos = Assigned, OverCutLength = 2, LeadData = new LeadData( 2 ) };
		StdPatternCacheBase cache = (StdPatternCacheBase)New( "MyCAM.PathCache." + geom.GetType().Name.Replace( "GeomData", "Cache" ), geom, craft );
		CheckCache( cache );
		Check( cache.RefPoint.ExternalAxisPos );
		foreach( CAMPoint p in cache.KeyCAMPointList ) { Check( p.ExternalAxisPos ); }
		List<CAMPoint> old = cache.MainPathPointList;
		craft.ExternalAxisPos = new ExternalAxisPos( 360, 17 );
		Assert( !ReferenceEquals( old, cache.MainPathPointList ), "Standard cache did not rebuild." );
		Assert( cache.RefPoint.ExternalAxisPos.Equals( craft.ExternalAxisPos ), "Reference was not updated before IK." );
		Assert( cache.MainPathPointList.Concat( cache.KeyCAMPointList ).Concat( cache.LeadInPointList ).Concat( cache.OverCutPointList ).All( p => p.ExternalAxisPos.Equals( craft.ExternalAxisPos ) ), "Standard rebuild lost positions." );
		Check( old[0].ExternalAxisPos );
	}

	static gp_Ax1 Center()
	{
		return new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ) );
	}

	public static void CircleCacheRebuild() { StandardCache( new CircleGeomData( Center(), 20, 0, false ) ); }
	public static void RectangleCacheRebuild() { StandardCache( new RectangleGeomData( Center(), 20, 30, 2, 0, false ) ); }
	public static void RunwayCacheRebuild() { StandardCache( new RunwayGeomData( Center(), 30, 20, 0, false ) ); }
	public static void PolygonCacheRebuild() { StandardCache( new PolygonGeomData( Center(), 5, 20, 2, 0, false ) ); }

	public static void LeadAndOverCutSupplementalPoints()
	{
		CAMPoint reference = CAM( 0, 0, 0 );
		List<CAMPoint> result;
		LeadData lead = new LeadData( 2, 2, 90, 2, 2, 90 );
		LeadHelper.SetLeadIn( reference, out result, lead, false, false );
		Assert( result.Count > 0, "Empty lead-in." );
		foreach( CAMPoint p in result ) { Check( p.ExternalAxisPos ); }
		LeadHelper.SetLeadOut( reference, out result, lead, false, false );
		Assert( result.Count > 0, "Empty lead-out." );
		foreach( CAMPoint p in result ) { Check( p.ExternalAxisPos ); }
		CAMPoint geometryStart = CAM( 0, 0, 0 );
		geometryStart.ExternalAxisPos = default( ExternalAxisPos );
		OverCutHelper.SetOverCut( new List<CAMPoint> { geometryStart, CAM( 10, 0, 0 ), reference }, out result, 2, true, false );
		Assert( result.Count == 2, "Overcut interior endpoint was not created." );
		foreach( CAMPoint p in result ) { Check( p.ExternalAxisPos ); }
	}

	public static void MicroJointSupplementalPoint()
	{
		object[] args = { CAM( 0, 0, 0 ), CAM( 10, 0, 0 ), 2.0, null };
		Assert( (bool)Invoke( "MyCAM.Helper.CAM.MicroJointHelper", "CalculatePointAtDistance", null, args ), "No microjoint endpoint." );
		Check( ( (CAMPoint)args[3] ).ExternalAxisPos );
	}

	public static void TraverseGeometryAndZeroDistanceClones()
	{
		CAMPoint start = CAM( 10, 0, 0 );
		CAMPoint end = CAM( 0, 10, 0 );
		Check( TraverseHelper.GetCutDownOrLiftUpPoint( start, 5 ).ExternalAxisPos );
		Check( TraverseHelper.GetCutDownOrLiftUpPoint( start, 0 ).ExternalAxisPos );
		Check( TraverseHelper.GetFrogLeapMiddlePoint( start, end, 0 ).ExternalAxisPos );
		Check( TraverseHelper.GetFrogLeapMiddlePoint( start, end, 5 ).ExternalAxisPos );
	}

	static object Package( CAMPoint start, CAMPoint end, TraverseData traverse )
	{
		IProcessPoint[] empty = new IProcessPoint[0];
		return New( "MyCAM.Post.ContourNCPackage", new LeadData(), 0.0, new IProcessPoint[] { start, end }, empty, empty, empty, traverse, start, end );
	}

	public static void PostConversionEntryExitAndTraverse()
	{
		CAMPoint start = CAM( 10, 0, 0 );
		CAMPoint end = CAM( 0, 10, 0 );
		TraverseData traverse = new TraverseData( 2, 3, 0, TraverseData.TraverseType.FrogLeap, 5, 30, 30, 30 );
		object pack = Package( start, end, traverse );
		EntryAndExitData entryExit = new EntryAndExitData( 5, 5, 0 );
		object[] args = { pack, null, entryExit, null, null };
		Assert( (bool)Invoke( "MyCAM.Post.ContourPostHelper", "GetPostData", null, args ), "Contour post failed." );
		PostData entry = (PostData)args[3];
		foreach( PostPoint p in entry.MainPathPostPointList ) { Check( p.ExternalAxisPos ); }
		Check( entry.CutDownPostPoint.ExternalAxisPos );
		object endInfo = args[4];
		object[] exitArgs = { endInfo, entryExit, null };
		Invoke( "MyCAM.Post.PostTraverseBuilder", "CalculateExit", null, exitArgs );
		Check( ( (PostPoint)exitArgs[2] ).ExternalAxisPos );
		foreach( TraverseData.TraverseType mode in Enum.GetValues( typeof( TraverseData.TraverseType ) ) ) {
			traverse.TraverseMode = mode;
			PostData data = new PostData { ProcessStartPoint = Post( 10, 0, 0 ) };
			Assert( (bool)Invoke( "MyCAM.Post.PostTraverseBuilder", "CalculateTraverse", null, endInfo, pack, data ), "Traverse post failed: " + mode );
			foreach( PostPoint p in new[] { data.LiftUpPostPoint, data.CutDownPostPoint, data.FrogLeapMidPostPoint, data.LiftUpPostSafePlanePoint, data.CutDownPostSafePlanePoint }.Where( p => p != null ) ) { Check( p.ExternalAxisPos ); }
			if( data.SafeBoundaryPostPointList != null ) { foreach( PostPoint p in data.SafeBoundaryPostPointList ) { Check( p.ExternalAxisPos ); } }
		}
		object[] standardArgs = { start, null };
		Assert( (bool)Invoke( "MyCAM.Post.StdPatternPostHelper", "ConvertProcessPntToPostPnt", null, standardArgs ), "Standard conversion failed." );
		Check( ( (PostPoint)standardArgs[1] ).ExternalAxisPos );
	}

	public static void SimulationLinearArcAndCoincidentPoints()
	{
		FiveAxisSimuPathBuilder builder = new FiveAxisSimuPathBuilder();
		PostPoint start = Post( 10, 0, 0 );
		PostPoint end = Post( 0, 10, 0 );
		object[] args = { start, end, null };
		Assert( (bool)Invoke( "MyCAM.Helper.FiveAxisSimuPathBuilder", "BuildLinearTraverseIKPnt", builder, args ), "Linear interpolation failed." );
		foreach( PostPoint p in (List<PostPointBase>)args[2] ) { Check( p.ExternalAxisPos ); }
		args = new object[] { start, Post( 5, 5, 5 ), end, null };
		Assert( (bool)Invoke( "MyCAM.Helper.FiveAxisSimuPathBuilder", "BuildFrogLeapTraverseIKPnt", builder, args ), "Arc interpolation failed." );
		foreach( PostPoint p in (List<PostPointBase>)args[3] ) { Check( p.ExternalAxisPos ); }
		args = new object[] { start, start.Clone(), null };
		Assert( (bool)Invoke( "MyCAM.Helper.FiveAxisSimuPathBuilder", "BuildLinearTraverseIKPnt", builder, args ), "Coincident linear failed." );
		foreach( PostPoint p in (List<PostPointBase>)args[2] ) { Check( p.ExternalAxisPos ); }
		args = new object[] { new List<gp_Pnt> { new gp_Pnt(), new gp_Pnt(), new gp_Pnt() }, start, start.Clone(), null };
		Assert( (bool)Invoke( "MyCAM.Helper.FiveAxisSimuPathBuilder", "AddInterpolatedPoints", null, args ), "Coincident arc failed." );
		foreach( PostPoint p in (List<PostPoint>)args[3] ) { Check( p.ExternalAxisPos ); }
	}

	public static void FKRetainsExternalAnglesWithoutNewKinematics()
	{
		FiveAxisMachineData machine;
		Assert( DataGettingHelper.GetMachineData( out machine ), "No machine data." );
		PostSolver solver = (PostSolver)New( "MyCAM.Post.PostSolver", machine );
		PostPoint input = Post( 1, 2, 3 );
		object[] args = { solver, input, null, new gp_Vec() };
		Assert( (bool)Invoke( "MyCAM.Helper.FiveAxisSimuPathBuilder", "BuildFKPostPntFiveAxis", null, args ), "FK failed." );
		PostPoint output = (PostPoint)args[2];
		Check( output.ExternalAxisPos );
		gp_Pnt expected = new gp_Pnt( 1, 2, 3 ).Translated( solver.SolveFK( input.Master, input.Slave, new gp_Pnt( 1, 2, 3 ), new gp_Vec() ) );
		Assert( expected.Distance( new gp_Pnt( output.X, output.Y, output.Z ) ) < 1e-9, "Step2 changed FK responsibility." );
	}

	public static void SimulationExitRetainsExternalAngles()
	{
		FiveAxisMachineData machine;
		Assert( DataGettingHelper.GetMachineData( out machine ), "No machine data." );
		PostSolver solver = (PostSolver)New( "MyCAM.Post.PostSolver", machine );
		object result = Invoke( "MyCAM.Helper.FiveAxisSimuPathBuilder", "GetExitPathIKPnt", new FiveAxisSimuPathBuilder(), CAM( 10, 0, 0 ), new EntryAndExitData( 5, 5, 0 ), Post( 10, 0, 0 ), solver );
		List<PostPointBase> points = (List<PostPointBase>)result;
		Assert( points.Count > 0, "Empty simulation exit." );
		foreach( PostPoint point in points ) { Check( point.ExternalAxisPos ); }
	}

	public static void StandardSimulationReferenceOverride()
	{
		object writer = New( "MyCAM.Post.NCWriter", New( "MyCAM.Data.DataManager" ) );
		object standard = New( "MyCAM.Data.StdPatternPostData" );
		standard.GetType().GetProperty( "RefPoint" ).SetValue( standard, Post( 0, 0, 0 ), null );
		PostData data = new PostData();
		data.MainPathPostPointList.Add( new PostPoint() );
		data.LeadInPostPointList.Add( new PostPoint() );
		data.OverCutPostPointList.Add( new PostPoint() );
		Invoke( "MyCAM.Post.NCWriter", "ApplyStdPatternPostDataToPostData", writer, data, standard );
		foreach( PostPoint point in data.MainPathPostPointList.Concat( data.LeadInPostPointList ).Concat( data.OverCutPostPointList ) ) { Check( point.ExternalAxisPos ); }
	}
}
