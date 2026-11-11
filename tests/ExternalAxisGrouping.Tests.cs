using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.PathCache;
using OCC.gp;

public static class ExternalAxisGroupingTests
{
	static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	static readonly Assembly App = typeof( CraftData ).Assembly;
	static void Assert( bool value, string message ) { if( !value ) { throw new Exception( message ); } }
	static object New( string name, params object[] args ) { return Activator.CreateInstance( App.GetType( name, true ), Flags, null, args, null ); }
	static object Call( string name, string method, object target, params object[] args ) { return App.GetType( name, true ).GetMethods( Flags ).Single( m => m.Name == method && m.GetParameters().Length == args.Length ).Invoke( target, args ); }
	static object Get( object target, string name ) { return target.GetType().GetProperty( name, Flags ).GetValue( target, null ); }
	static void Set( object target, string name, object value ) { target.GetType().GetProperty( name, Flags ).SetValue( target, value, null ); }
	static object[] Items( object value ) { return ( (IEnumerable)value ).Cast<object>().ToArray(); }
	static void SamePoint( CAMPoint actual, CAMPoint expected )
	{
		Assert( actual.Point.Distance( expected.Point ) < 1e-8, "Endpoint XYZ changed." );
		Assert( actual.ToolVec.Angle( expected.ToolVec ) < 1e-8, "Endpoint tool vector changed." );
		Assert( actual.ModMaster_rad == expected.ModMaster_rad && actual.ModSlave_rad == expected.ModSlave_rad, "Endpoint M/S changed." );
		Assert( actual.ExternalAxisPos.Equals( expected.ExternalAxisPos ), "Endpoint external angles changed." );
	}

	sealed class Fixture
	{
		public object Manager;
		public FiveAxisMachineData Machine;
		public List<string> IDs;
		public Dictionary<string, IObject> Map;
		public Fixture( ExternalAxisType type, bool mix )
		{
			Machine = mix ? (FiveAxisMachineData)new MixTypeMachineData { ToolToMasterVec = new gp_Vec(), MCSToSlaveVec = new gp_Vec() }
				: new SpindleTypeMachineData { ToolToSlaveVec = new gp_Vec(), SlaveToMasterVec = new gp_Vec() };
			Machine.ToolDirection = ToolDirection.Z;
			Machine.ToolLength = 12;
			Machine.MasterRotaryAxis = mix ? RotaryAxis.X : RotaryAxis.Z;
			Machine.SlaveRotaryAxis = mix ? RotaryAxis.Z : RotaryAxis.X;
			Machine.MasterTiltedVec_deg = new gp_XYZ();
			Machine.SlaveTiltedVec_deg = new gp_XYZ();
			Machine.ExternalAxes.Type = type;
			Machine.ExternalAxes.Ext1RotaryAxis = RotaryAxis.Z;
			Machine.ExternalAxes.Ext2RotaryAxis = RotaryAxis.X;
			Machine.ExternalAxes.MCSToExt1Vec = new gp_Vec();
			Machine.ExternalAxes.Ext1ToExt2Vec = new gp_Vec();
			Machine.InitRotateAxes( RotaryDirection.RightHand, 0, 0, RotaryDirection.LeftHand, 0, 0 );
			Manager = New( "MyCAM.Data.DataManager" );
			Set( Manager, "MachineData", Machine );
			Call( "MyCAM.Data.DataGettingHelper", "Initialize", null, Manager );
			IDs = (List<string>)Get( Manager, "PathIDList" );
			Map = (Dictionary<string, IObject>)Get( Manager, "ObjectMap" );
		}
		public ContourPathObject Add( double first, double second )
		{
			string id = "Path" + IDs.Count;
			CraftData craft = new CraftData { ExternalAxisPos = new ExternalAxisPos( first, second ) };
			craft.TraverseData.LiftUpDistance = 5;
			craft.TraverseData.CutDownDistance = 5;
			List<CADPoint> points = new List<CADPoint>();
			foreach( gp_Pnt point in new[] { new gp_Pnt( 10, 0, 0 ), new gp_Pnt( 10, 10, 0 ), new gp_Pnt( 0, 10, 0 ), new gp_Pnt( 0, 0, 0 ) } ) {
				points.Add( new CADPoint( point, new gp_Dir( 0, 1, 1 ), new gp_Dir( 0, 0, 1 ), new gp_Dir( 1, 0, 0 ) ) );
			}
			ContourGeomData geom = new ContourGeomData( points, new Dictionary<CADPoint, CADPoint>(), true );
			ContourPathObject path = new ContourPathObject( id, null, geom, craft );
			IDs.Add( id ); Map.Add( id, path ); return path;
		}
		public object[] Groups() { return Items( Call( "MyCAM.Helper.ProcessPathGroupHelper", "GetProcessPathGroups", null, IDs, Machine ) ); }
		public object[] Convert( out List<PostData> flat )
		{
			object writer = New( "MyCAM.Post.NCWriter", Manager );
			object[] args = { null, null, null };
			Assert( (bool)Call( "MyCAM.Post.NCWriter", "ConvertPostDataListSuccess", writer, args ), "Conversion failed: " + args[0] );
			flat = (List<PostData>)args[1]; return Items( args[2] );
		}
	}

	public static void AdjacentRunsKeepOrder()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false );
		foreach( double value in new[] { 0.0, 0, 180, 180, 0 } ) { f.Add( value, 0 ); }
		object[] groups = f.Groups();
		Assert( groups.Length == 3, "0,0,180,180,0 must make three groups." );
		Assert( groups.SelectMany( g => Items( Get( g, "PathIDs" ) ) ).Cast<string>().SequenceEqual( f.IDs ), "Paths reordered or lost." );
		Assert( Items( Get( groups[0], "PathIDs" ) ).Length == 2 && Items( Get( groups[1], "PathIDs" ) ).Length == 2, "Wrong run sizes." );
		for( int i = 0; i < groups.Length; i++ ) { Assert( (int)Get( groups[i], "Index" ) == i, "Wrong group index." ); }
	}
	public static void AngleToleranceAndFullTurns()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false );
		f.Add( 0, 0 ); f.Add( 360, 0 ); f.Add( 0, 0 );
		Assert( f.Groups().Length == 3, "Full turns were merged." );
		f.Add( 1e-12, 0 );
		Assert( f.Groups().Length == 3, "Angles within tolerance were split." );
	}
	public static void ToleranceComparesWithGroupStart()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false );
		foreach( double angle in new[] { 0.0, 0.0008, 0.0016, 0.0024, 360, 360.0008 } ) { f.Add( angle, 0 ); }
		object[] groups = f.Groups();
		Assert( groups.Length == 3, "Tolerance chained through previous paths instead of group starts." );
		Assert( groups.All( g => Items( Get( g, "PathIDs" ) ).Length == 2 ), "Wrong tolerance group sizes." );
		Assert( ( (ExternalAxisPos)Get( groups[1], "ExternalAxisPos" ) ).Ext1 == 0.0016, "Group start angle was rounded." );
	}
	public static void ToleranceBoundaryAndDualAxes()
	{
		Fixture single = new Fixture( ExternalAxisType.Single, false );
		single.Add( 0, 99 ); single.Add( 1e-3, -99 ); single.Add( 0.001001, 0 );
		Assert( single.Groups().Length == 2 && Items( Get( single.Groups()[0], "PathIDs" ) ).Length == 2, "Single-axis tolerance boundary changed." );
		Fixture dual = new Fixture( ExternalAxisType.Dual, false );
		dual.Add( 0, 0 ); dual.Add( 0.0008, -0.0008 ); dual.Add( 0.0008, 0.0011 );
		Assert( dual.Groups().Length == 2 && Items( Get( dual.Groups()[0], "PathIDs" ) ).Length == 2, "Dual axes did not apply tolerance independently." );
	}
	public static void TolerancePreservesOriginalPointAndFKAngles()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false );
		ContourPathObject first = f.Add( 30, 91 ), second = f.Add( 30.0008, -83 );
		List<PostData> flat; object[] groups = f.Convert( out flat );
		Assert( groups.Length == 1, "Tolerance did not combine nearby processing positions." );
		Assert( ( (ExternalAxisPos)Get( Get( groups[0], "Group" ), "ExternalAxisPos" ) ).Equals( new ExternalAxisPos( 30, 0 ) ), "Group representative is not the first effective assignment." );
		ContourPathObject[] paths = { first, second };
		object solver = New( "MyCAM.Post.PostSolver", f.Machine );
		for( int i = 0; i < paths.Length; i++ ) {
			ExternalAxisPos original = new ExternalAxisPos( i == 0 ? 30 : 30.0008, i == 0 ? 91 : -83 );
			Assert( paths[i].CraftData.ExternalAxisPos.Equals( original ), "Grouping changed original craft angles." );
			Assert( paths[i].ContourCache.MainPathPointList.All( p => p.ExternalAxisPos.Equals( original ) ), "Grouping replaced original CAM angles." );
			Assert( flat[i].MainPathPostPointList.All( p => p.ExternalAxisPos.Equals( original ) ), "Grouping replaced original Post angles." );
			object[] args = { solver, flat[i].MainPathPostPointList[0], null, f.Machine.SimulationOffset };
			Assert( (bool)Call( "MyCAM.Helper.FiveAxisSimuPathBuilder", "BuildFKPostPntFiveAxis", null, args ), "FK conversion failed." );
			Assert( ( (PostPoint)args[2] ).ExternalAxisPos.Equals( original ), "FK discarded original angles within tolerance." );
		}
	}
	public static void IKSeedSettersOwnNotificationsAndTolerance()
	{
		CraftData craft = new CraftData(); int changes = 0;
		Action handler = delegate { changes++; };
		craft.CAMFactorChanged += handler;
		craft.InitMaster_rad = 0; craft.InitSlave_rad = 0;
		craft.InitMaster_rad = 1e-3; craft.InitSlave_rad = -1e-3;
		Assert( changes == 0 && craft.InitMaster_rad == 0 && craft.InitSlave_rad == 0, "Tolerance boundary wrote or notified." );
		craft.InitMaster_rad = 0.2; craft.InitSlave_rad = -0.3;
		Assert( changes == 2, "Seed changes did not notify internally." );
		craft.InitMaster_rad = 0.2005; craft.InitSlave_rad = -0.3005;
		Assert( changes == 2 && craft.InitMaster_rad == 0.2 && craft.InitSlave_rad == -0.3, "Close seeds wrote or notified." );
		craft.InitMaster_rad = 0.202; craft.InitSlave_rad = -0.302;
		Assert( changes == 4 && craft.InitMaster_rad == 0.202 && craft.InitSlave_rad == -0.302, "Seed changes beyond tolerance were ignored." );
		craft.CAMFactorChanged -= handler;
		craft.InitMaster_rad = 0;
		Assert( changes == 4, "Event unsubscription failed." );
	}
	public static void CraftFactorNotificationsAreEvents()
	{
		foreach( string name in new[] { "CAMFactorChanged", "CADFactorChanged" } ) {
			EventInfo notification = typeof( CraftData ).GetEvent( name );
			Assert( notification != null && notification.GetAddMethod().IsPublic && notification.GetRemoveMethod().IsPublic, "Notification cannot be subscribed to." );
			Assert( notification.GetRaiseMethod() == null && typeof( CraftData ).GetField( name ) == null, "Notification can be invoked externally." );
		}
	}
	public static void SeedChangesInvalidateCacheWithoutExternalNotification()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false ); ContourPathObject path = f.Add( 0, 0 );
		CAMPoint initial = path.ContourCache.MainPathPointList[0];
		path.CraftData.InitMaster_rad = 0.2;
		CAMPoint changed = path.ContourCache.MainPathPointList[0];
		Assert( !ReferenceEquals( initial, changed ), "Changed master seed did not dirty cache." );
		path.CraftData.InitMaster_rad = 0.2005;
		Assert( ReferenceEquals( changed, path.ContourCache.MainPathPointList[0] ), "Close master seed rebuilt cache." );
		path.CraftData.InitSlave_rad = -0.3;
		Assert( !ReferenceEquals( changed, path.ContourCache.MainPathPointList[0] ), "Changed slave seed did not dirty cache." );
	}
	public static void OptimizationDoesNotRebuildUnchangedSeeds()
	{
		Fixture f = new Fixture( ExternalAxisType.None, false ); ContourPathObject path = f.Add( 0, 0 );
		CAMPoint initial = path.ContourCache.MainPathPointList[0]; int changes = 0;
		path.CraftData.CAMFactorChanged += delegate { changes++; };
		ExternalAxisEditorTestHelper.Optimize( f.Manager );
		Assert( changes == 0 && ReferenceEquals( initial, path.ContourCache.MainPathPointList[0] ), "Unchanged optimization forced a rebuild." );
	}
	public static void EnabledPathSubsetMatrixKeepsExactRunsAndOriginalIndices()
	{
		foreach( int configuration in new[] { 0, 1, 2, 3 } ) {
			ExternalAxisType type = configuration == 0 ? ExternalAxisType.None : configuration == 2 ? ExternalAxisType.Dual : ExternalAxisType.Single;
			Fixture f = new Fixture( type, configuration == 3 );
			double[] first = { 0, 360, 0, 180, 180, 0 };
			double[] second = { 17, 17, 23, 23, 41, 17 };
			ContourPathObject[] paths = first.Select( (angle, index) => f.Add( angle, second[index] ) ).ToArray();
			for( int mask = 0; mask < 64; mask++ ) {
				List<List<int>> expected = new List<List<int>>();
				int previous = -1;
				for( int i = 0; i < paths.Length; i++ ) {
					paths[i].CraftData.ProcessPath = ( mask & ( 1 << i ) ) != 0;
					if( !paths[i].CraftData.ProcessPath ) { continue; }
					bool split = previous < 0 || ( type != ExternalAxisType.None && ( first[i] != first[previous]
						|| ( type == ExternalAxisType.Dual && second[i] != second[previous] ) ) );
					if( split ) { expected.Add( new List<int>() ); }
					expected.Last().Add( i ); previous = i;
				}
				object[] actual = f.Groups();
				string context = "Configuration " + configuration + ", mask " + mask;
				Assert( actual.Length == expected.Count, context + ": wrong group count." );
				for( int g = 0; g < actual.Length; g++ ) {
					Assert( (int)Get( actual[g], "Index" ) == g, context + ": wrong group index." );
					Assert( Items( Get( actual[g], "PathIndices" ) ).Cast<int>().SequenceEqual( expected[g] ), context + ": original indices changed." );
					Assert( Items( Get( actual[g], "PathIDs" ) ).Cast<string>().SequenceEqual( expected[g].Select( i => f.IDs[i] ) ), context + ": path order changed." );
					int start = expected[g][0];
					ExternalAxisPos effective = type == ExternalAxisType.None ? default( ExternalAxisPos )
						: new ExternalAxisPos( first[start], type == ExternalAxisType.Dual ? second[start] : 0 );
					Assert( effective.Equals( Get( actual[g], "ExternalAxisPos" ) ), context + ": wrong effective angles." );
				}
			}
			for( int i = 0; i < paths.Length; i++ ) {
				Assert( paths[i].CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( first[i], second[i] ) ), "Grouping mutated assigned angles." );
			}
		}
	}
	public static void EffectiveSingleAxisPreservesAssignedSecond()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false );
		ContourPathObject a = f.Add( 17, 91 ); f.Add( 17, -83 );
		object[] groups = f.Groups();
		Assert( groups.Length == 1, "Inactive Ext2 split a single-axis group." );
		Assert( ( (ExternalAxisPos)Get( groups[0], "ExternalAxisPos" ) ).Equals( new ExternalAxisPos( 17, 0 ) ), "Wrong effective position." );
		Assert( a.CraftData.ExternalAxisPos.Ext2 == 91, "Assigned inactive angle was erased." );
	}
	public static void DualUsesBothAxes()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false );
		f.Add( 12, 0 ); f.Add( 12, 1 ); f.Add( 12, 1 ); f.Add( 13, 1 );
		Assert( f.Groups().Length == 3, "Dual comparison did not use both axes." );
	}
	public static void NoExternalAxisRegression()
	{
		Fixture f = new Fixture( ExternalAxisType.None, false );
		f.Add( 0, 0 ); f.Add( 180, 91 ); f.Add( 360, -45 );
		Assert( f.Groups().Length == 1, "No-external-axis machine was split." );
		List<PostData> flat; object[] groups = f.Convert( out flat );
		Assert( groups.Length == 1 && flat.Count == 3, "Legacy post-data count changed." );
		Assert( flat[0].LiftUpPostPoint == null && flat[1].LiftUpPostPoint != null, "Legacy Entry/Traverse changed." );
	}
	public static void DisabledAndMissingPathsDoNotSplitRuns()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false );
		f.Add( 0, 0 ); ContourPathObject disabled = f.Add( 180, 0 ); disabled.CraftData.ProcessPath = false;
		f.IDs.Add( "Missing" ); f.Add( 0, 0 );
		object[] groups = f.Groups();
		Assert( groups.Length == 1, "Nonprocessing paths changed grouping." );
		Assert( Items( Get( groups[0], "PathIndices" ) ).Cast<int>().SequenceEqual( new[] { 0, 3 } ), "Original path indices were lost." );
		disabled.CraftData.ProcessPath = true;
		Assert( f.Groups().Length == 3, "Re-enabled path did not split runs." );
	}
	public static void EmptyAndAllDisabled()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false );
		Assert( f.Groups().Length == 0, "Empty list created a group." );
		f.Add( 90, 0 ).CraftData.ProcessPath = false;
		List<PostData> flat; Assert( f.Convert( out flat ).Length == 0 && flat.Count == 0, "Disabled path produced post-data." );
	}
	public static void GroupEntryAndPostDataCorrespondence()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false );
		foreach( double angle in new[] { 0.0, 0, 180, 180, 0 } ) { f.Add( angle, 0 ); }
		List<PostData> flat; object[] groups = f.Convert( out flat );
		Assert( groups.Length == 3 && flat.Count == 5, "Grouped conversion lost paths." );
		Assert( flat[0].LiftUpPostPoint == null && flat[2].LiftUpPostPoint == null && flat[4].LiftUpPostPoint == null, "Group start contains cross-group lift-up." );
		Assert( flat[1].LiftUpPostPoint != null && flat[3].LiftUpPostPoint != null, "Within-group traverse missing." );
		Assert( groups.SelectMany( g => Items( Get( g, "PostDataList" ) ) ).SequenceEqual( flat.Cast<object>() ), "Flat and group post-data differ." );
		foreach( object group in groups ) {
			string[] ids = Items( Get( Get( group, "Group" ), "PathIDs" ) ).Cast<string>().ToArray();
			SamePoint( (CAMPoint)Get( group, "ProcessStartPoint" ), CacheHelper.GetProcessStartPoint( ids[0] ) );
			SamePoint( (CAMPoint)Get( group, "ProcessEndPoint" ), CacheHelper.GetProcessEndPoint( ids.Last() ) );
		}
	}
	public static void LeadOutOverCutAndEndpointSnapshots()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false );
		ContourPathObject lead = f.Add( 0, 0 );
		lead.CraftData.LeadData = new LeadData( 2, 0, 90, 3, 0, 90 ); lead.CraftData.OverCutLength = 2;
		ContourPathObject over = f.Add( 90, 0 ); over.CraftData.OverCutLength = 2;
		ContourPathObject main = f.Add( 180, 0 );
		List<PostData> flat; object[] groups = f.Convert( out flat );
		SamePoint( (CAMPoint)Get( groups[0], "ProcessStartPoint" ), lead.ContourCache.LeadInPointList.First() );
		SamePoint( (CAMPoint)Get( groups[0], "ProcessEndPoint" ), lead.ContourCache.LeadOutPointList.Last() );
		SamePoint( (CAMPoint)Get( groups[1], "ProcessEndPoint" ), over.ContourCache.OverCutPointList.Last() );
		SamePoint( (CAMPoint)Get( groups[2], "ProcessEndPoint" ), main.ContourCache.MainPathPointList.Last() );
		CAMPoint snapshot = (CAMPoint)Get( groups[0], "ProcessEndPoint" );
		lead.ContourCache.LeadOutPointList.Last().ModMaster_rad += 1;
		Assert( snapshot.ModMaster_rad != lead.ContourCache.LeadOutPointList.Last().ModMaster_rad, "Group endpoint aliases Cache data." );
	}
	public static void IKSeedsResetAtEachGroup()
	{
		foreach( bool mix in new[] { false, true } ) {
			Fixture f = new Fixture( ExternalAxisType.Single, mix );
			List<ContourPathObject> paths = new List<ContourPathObject>();
			foreach( double angle in new[] { 0.0, 0, 180, 180, 0 } ) { paths.Add( f.Add( angle, 0 ) ); }
			foreach( ContourPathObject path in paths ) { path.CraftData.InitMaster_rad = 19; path.CraftData.InitSlave_rad = 23; }
			ExternalAxisEditorTestHelper.Optimize( f.Manager );
			foreach( int index in new[] { 0, 2, 4 } ) {
				Assert( paths[index].CraftData.InitMaster_rad == 0 && paths[index].CraftData.InitSlave_rad == 0, "Group seed did not reset to M=S=0." );
			}
			foreach( int index in new[] { 1, 3 } ) {
				CAMPoint previous = CacheHelper.GetProcessEndPoint( paths[index - 1].UID );
				Assert( paths[index].CraftData.InitMaster_rad == previous.ModMaster_rad && paths[index].CraftData.InitSlave_rad == previous.ModSlave_rad, "Within-group seed did not use previous processing end." );
			}
		}
	}
	public static void MixedStandardPatternGroups()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false );
		ContourPathObject source = f.Add( 0, 0 );
		CraftData craft = new CraftData { ExternalAxisPos = new ExternalAxisPos( 180, 0 ), LeadData = new LeadData( 2 ) };
		CircleGeomData geom = new CircleGeomData( new gp_Ax1( new gp_Pnt( 30, 0, 0 ), new gp_Dir( 0, 0, 1 ) ), 20, 0, false );
		PathObject circle = (PathObject)New( "MyCAM.Data.CirclePathObject", "Circle", geom, craft, source );
		f.IDs.Add( circle.UID ); f.Map.Add( circle.UID, circle ); f.Add( 0, 0 );
		List<PostData> flat; object[] groups = f.Convert( out flat );
		Assert( groups.Length == 3 && flat.Count == 3, "Mixed path types were not grouped." );
		SamePoint( (CAMPoint)Get( groups[1], "ProcessEndPoint" ), CacheHelper.GetProcessEndPoint( circle.UID ) );
		Assert( flat[1].MainPathPostPointList.All( p => p.ExternalAxisPos.Equals( craft.ExternalAxisPos ) ), "Standard pattern lost assigned position." );
	}
	public static void SimulationInputRetainsAllGroupEndpoints()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false );
		f.Add( 0, 0 ); f.Add( 180, 0 ); f.Add( 0, 0 );
		object writer = New( "MyCAM.Post.NCWriter", f.Manager );
		object[] args = { null, null, null };
		Assert( (bool)Call( "MyCAM.Post.NCWriter", "ConvertPostDataListSuccess", writer, args ), "Conversion failed: " + args[0] );
		Type type = App.GetType( "MyCAM.Editor.SimuEditor", true );
		object editor = System.Runtime.Serialization.FormatterServices.GetUninitializedObject( type );
		type.BaseType.GetField( "m_DataManager", Flags ).SetValue( editor, f.Manager );
		type.GetField( "m_SimuPathList", Flags ).SetValue( editor, f.IDs );
		type.GetField( "m_PostDataList", Flags ).SetValue( editor, args[1] );
		type.GetField( "m_ProcessGroups", Flags ).SetValue( editor, args[2] );
		object input = Call( "MyCAM.Editor.SimuEditor", "GetSimulationInputData", editor );
		object[] groups = Items( input.GetType().GetField( "ProcessGroups", Flags ).GetValue( input ) );
		Assert( groups.Length == 3, "Simulation input discarded groups." );
		Assert( ReferenceEquals( input.GetType().GetField( "EachPathIKPostDataList", Flags ).GetValue( input ), args[1] ), "Flat compatibility list changed." );
		CAMPoint last = (CAMPoint)input.GetType().GetField( "LastPathLastPnt", Flags ).GetValue( input );
		SamePoint( last, (CAMPoint)Get( groups[2], "ProcessEndPoint" ) );
		Assert( !ReferenceEquals( last, Get( groups[2], "ProcessEndPoint" ) ), "Legacy last point aliases group endpoint." );
	}

	static List<PostPointBase> FramePoints( int count )
	{
		return count < 0 ? null : Enumerable.Range( 0, count ).Select( i => (PostPointBase)new PostPoint() ).ToList();
	}
	static int FramePointCount( IReadOnlyList<PostPointBase> points ) { return points == null ? 0 : points.Count; }
	static void AssertFrameRanges( List<PathSimuFKData> paths, int frameCount )
	{
		object ranges = Call( "MyCAM.Helper.SimulationHelper", "GetPathStartEndIndex", null, paths );
		object[] indices = Items( ranges );
		Assert( indices.Length == paths.Count, "Frame ranges lost paths." );
		Type type = App.GetType( "MyCAM.Editor.SimuEditor", true );
		object editor = System.Runtime.Serialization.FormatterServices.GetUninitializedObject( type );
		type.GetField( "m_PathStartEndIndex", Flags ).SetValue( editor, ranges );
		int next = 0;
		for( int i = 0; i < paths.Count; i++ ) {
			int count = FramePointCount( paths[i].TraversePoints ) + FramePointCount( paths[i].PathPoints ) + FramePointCount( paths[i].ExitPoints );
			Assert( (int)Get( indices[i], "StartIndex" ) == next, "Wrong start index for path " + i + "." );
			Assert( (int)Get( indices[i], "EndIndex" ) == next + count - 1, "Wrong inclusive end index for path " + i + "." );
			for( int frame = next; frame < next + count; frame++ ) {
				type.GetField( "m_CurrentFrameIndex", Flags ).SetValue( editor, frame );
				Assert( (int)Call( "MyCAM.Editor.SimuEditor", "GetCurrentPathByframe", editor ) == i, "Playback assigned frame " + frame + " to the wrong path." );
			}
			next += count;
		}
		Assert( next == frameCount, "Frame ranges disagree with the actual frame count." );
		if( paths.Count > 0 ) {
			Assert( (int)Get( indices.Last(), "EndIndex" ) == frameCount - 1, "Final frame index exceeds the actual frames." );
		}
	}
	public static void FrameIndexSegmentEdgeCases()
	{
		AssertFrameRanges( new List<PathSimuFKData>(), 0 );
		int[][] cases = { new[] { 1, 0, 0 }, new[] { 0, 1, 0 }, new[] { 0, 0, 1 }, new[] { 1, 1, 0 },
			new[] { 1, 0, 1 }, new[] { 0, 1, 1 }, new[] { 1, 1, 1 }, new[] { 2, 2, 0 }, new[] { 2, 2, 2 },
			new[] { -1, 5, -1 }, new[] { 6, 5, 6 } };
		foreach( int[] counts in cases ) {
			AssertFrameRanges( new List<PathSimuFKData> { new PathSimuFKData( FramePoints( counts[0] ), FramePoints( counts[1] ), FramePoints( counts[2] ) ) }, counts.Sum( n => Math.Max( n, 0 ) ) );
		}
	}
	public static void FrameIndexMultiplePathsAndGroupExits()
	{
		foreach( bool groupExits in new[] { false, true } ) {
			List<PathSimuFKData> paths = new List<PathSimuFKData>();
			for( int i = 0; i < 5; i++ ) {
				paths.Add( new PathSimuFKData( FramePoints( i == 0 ? 0 : 2 ), FramePoints( 2 ), FramePoints( groupExits || i == 4 ? 2 : 0 ) ) );
				AssertFrameRanges( paths, paths.Sum( p => p.TraversePoints.Count + p.PathPoints.Count + p.ExitPoints.Count ) );
			}
		}
	}
	public static void SimulationFrameIndicesMatchPlayback()
	{
		foreach( bool mix in new[] { false, true } ) {
			foreach( int count in new[] { 1, 2, 3 } ) {
				foreach( double distance in new[] { 0.0, 5.0 } ) {
					Fixture f = new Fixture( ExternalAxisType.None, mix );
					EntryAndExitData entry = (EntryAndExitData)Get( f.Manager, "EntryAndExitData" );
					entry.EntryDistance = distance; entry.ExitDistance = distance;
					for( int i = 0; i < count; i++ ) { f.Add( 0, 0 ); }
					List<PostData> flat; object[] groups = f.Convert( out flat );
					object solver = New( "MyCAM.Post.PostSolver", f.Machine );
					object builder = New( "MyCAM.Helper.FiveAxisSimuPathBuilder" );
					object[] ikArgs = { flat, Get( groups.Last(), "ProcessEndPoint" ), entry, solver, null };
					Assert( (bool)Call( "MyCAM.Helper.FiveAxisSimuPathBuilder", "GetWholePathIKPnt", builder, ikArgs ), "Simulation IK collection failed." );
					object[] fkArgs = { ikArgs[4], solver, f.Machine.SimulationOffset, null };
					Assert( (bool)Call( "MyCAM.Helper.FiveAxisSimuPathBuilder", "BuildPathFKList", builder, fkArgs ), "Simulation FK conversion failed." );
					List<PathSimuFKData> paths = (List<PathSimuFKData>)fkArgs[3];
					object[] frameArgs = { paths, solver, f.Machine.WorkPieceChain, f.Machine, f.Machine.ChainListMap, null, 0 };
					Assert( (bool)Call( "MyCAM.Helper.SimulationHelper", "BuildFrame", null, frameArgs ), "Simulation frame building failed." );
					int frameCount = (int)frameArgs[6];
					Dictionary<MachineComponentType, List<gp_Trsf>> frames = (Dictionary<MachineComponentType, List<gp_Trsf>>)frameArgs[5];
					Assert( frameCount > 0 && frames.Count > 0 && frames.Values.All( list => list.Count == frameCount ), "Component frame counts disagree." );
					AssertFrameRanges( paths, frameCount );
				}
			}
		}
	}
}
