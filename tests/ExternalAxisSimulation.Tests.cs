using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.PathCache;
using MyCAM.Post;
using OCC.AIS;
using OCC.gp;

public static class ExternalAxisSimulationTests
{
	static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	static readonly Assembly App = typeof( CraftData ).Assembly;
	static void Assert( bool value, string message ) { if( !value ) { throw new Exception( message ); } }
	static void Close( double a, double b ) { Assert( Math.Abs( a - b ) < 1e-8, "Expected " + b + ", got " + a ); }
	static void Point( gp_Pnt a, gp_Pnt b ) { Close( a.X(), b.X() ); Close( a.Y(), b.Y() ); Close( a.Z(), b.Z() ); }
	static object New( string name, params object[] args ) { return Activator.CreateInstance( App.GetType( name, true ), Flags, null, args, null ); }
	static object Call( string name, string method, object target, params object[] args ) { return App.GetType( name, true ).GetMethods( Flags ).Single( m => m.Name == method && m.GetParameters().Length == args.Length ).Invoke( target, args ); }
	static object Get( object target, string name ) { return target.GetType().GetProperty( name, Flags ).GetValue( target, null ); }
	static void Set( object target, string name, object value ) { target.GetType().GetProperty( name, Flags ).SetValue( target, value, null ); }
	static object[] Items( object value ) { return ( (IEnumerable)value ).Cast<object>().ToArray(); }
	static void Matrix( gp_Trsf a, gp_Trsf b ) { for( int r = 1; r <= 3; r++ ) { for( int c = 1; c <= 4; c++ ) { Close( a.Value( r, c ), b.Value( r, c ) ); } } }

	sealed class Fixture
	{
		public object Manager, Groups;
		public FiveAxisMachineData Machine;
		public List<string> IDs;
		public Dictionary<string, IObject> Map;
		public List<PostData> Flat;
		public EntryAndExitData Entry;
		public PostSolver Solver;
		public Fixture( string kind, ExternalAxisType type )
		{
			Machine = kind == "Mix" ? (FiveAxisMachineData)new MixTypeMachineData { ToolToMasterVec = new gp_Vec( 4, 5, 6 ), MCSToSlaveVec = new gp_Vec( 11, 13, 19 ) }
				: new SpindleTypeMachineData { ToolToSlaveVec = new gp_Vec( 4, 5, 6 ), SlaveToMasterVec = new gp_Vec( 7, 8, 9 ) };
			Machine.ToolDirection = ToolDirection.Z;
			Machine.MasterRotaryAxis = kind == "Mix" ? RotaryAxis.X : RotaryAxis.Z;
			Machine.SlaveRotaryAxis = kind == "Mix" ? RotaryAxis.Z : RotaryAxis.X;
			Machine.MasterTiltedVec_deg = new gp_XYZ(); Machine.SlaveTiltedVec_deg = new gp_XYZ();
			Machine.ToolLength = 12; Machine.SimulationOffset = new gp_Vec( 17, -23, 31 );
			Machine.ExternalAxes.Type = type;
			Machine.ExternalAxes.Ext1RotaryAxis = RotaryAxis.Z; Machine.ExternalAxes.Ext2RotaryAxis = RotaryAxis.X;
			Machine.ExternalAxes.MCSToExt1Vec = new gp_Vec( 10, 20, 30 ); Machine.ExternalAxes.Ext1ToExt2Vec = new gp_Vec( 5, 7, 11 );
			Machine.InitRotateAxes( RotaryDirection.RightHand, 0, 0, RotaryDirection.LeftHand, 0, 0 );
			Manager = New( "MyCAM.Data.DataManager" ); Set( Manager, "MachineData", Machine );
			Call( "MyCAM.Data.DataGettingHelper", "Initialize", null, Manager );
			IDs = (List<string>)Get( Manager, "PathIDList" ); Map = (Dictionary<string, IObject>)Get( Manager, "ObjectMap" );
			Entry = (EntryAndExitData)Get( Manager, "EntryAndExitData" ); Entry.EntryDistance = 5; Entry.ExitDistance = 7;
			Solver = (PostSolver)New( "MyCAM.Post.PostSolver", Machine );
		}
		public ContourPathObject Add( double first, double second )
		{
			string id = "Path" + IDs.Count;
			CraftData craft = new CraftData { ExternalAxisPos = new ExternalAxisPos( first, second ) };
			craft.TraverseData.LiftUpDistance = 3; craft.TraverseData.CutDownDistance = 4;
			double x = 10 + IDs.Count * 20;
			List<CADPoint> points = new List<CADPoint>();
			foreach( gp_Pnt point in new[] { new gp_Pnt( x, 0, 0 ), new gp_Pnt( x, 10, 0 ), new gp_Pnt( x - 10, 10, 0 ), new gp_Pnt( x - 10, 0, 0 ) } ) {
				points.Add( new CADPoint( point, new gp_Dir( 0, 1, 1 ), new gp_Dir( 0, 0, 1 ), new gp_Dir( 1, 0, 0 ) ) );
			}
			ContourGeomData geom = new ContourGeomData( points, new Dictionary<CADPoint, CADPoint>(), true );
			ContourPathObject path = new ContourPathObject( id, null, geom, craft ); IDs.Add( id ); Map.Add( id, path ); return path;
		}
		public void Convert()
		{
			object writer = New( "MyCAM.Post.NCWriter", Manager ); object[] args = { null, null, null };
			Assert( (bool)Call( "MyCAM.Post.NCWriter", "ConvertPostDataListSuccess", writer, args ), "Conversion failed: " + args[0] );
			Flat = (List<PostData>)args[1]; Groups = args[2];
		}
		public object Input()
		{
			object input = New( "MyCAM.Data.SimuData+SimuRequiredData+SimuInputSet" );
			foreach( var pair in new Dictionary<string, object> { { "EachPathIKPostDataList", Flat }, { "ProcessGroups", Groups }, { "LastPathLastPnt", Items( Groups ).Length == 0 ? null : Get( Items( Groups ).Last(), "ProcessEndPoint" ) },
				{ "EntryAndExitData", Entry }, { "PostSolver", Solver }, { "MachineData", Machine } } ) { input.GetType().GetField( pair.Key, Flags ).SetValue( input, pair.Value ); }
			return input;
		}
		public List<PathSimuIKData> Collect()
		{
			object[] args = { Groups, Entry, Solver, null };
			Assert( (bool)Call( "MyCAM.Helper.FiveAxisSimuPathBuilder", "GetGroupedPathIKPnt", new FiveAxisSimuPathBuilder(), args ), "Grouped collection failed." );
			return (List<PathSimuIKData>)args[3];
		}
		public Dictionary<MachineComponentType, List<gp_Trsf>> Frames( out object[] ranges, out int count )
		{
			object[] args = { Input(), null, null, 0 };
			Assert( (bool)Call( "MyCAM.Helper.FiveAxisSimuPathBuilder", "BuildFrameTransMap", new FiveAxisSimuPathBuilder(), args ), "Grouped frame building failed." );
			ranges = Items( args[2] ); count = (int)args[3]; return (Dictionary<MachineComponentType, List<gp_Trsf>>)args[1];
		}
		public object Renderer() { return New( "MyCAM.Editor.Renderer.TraverseRenderer", null, Manager ); }
	}

	// Independent Rodrigues rotation about an MCS zero-pose axis.
	static gp_Pnt Rotate( gp_Pnt p, gp_Pnt center, gp_Dir dir, double angle )
	{
		double x = p.X() - center.X(), y = p.Y() - center.Y(), z = p.Z() - center.Z();
		double a = dir.X(), b = dir.Y(), c = dir.Z(), co = Math.Cos( angle ), si = Math.Sin( angle ), dot = a * x + b * y + c * z;
		return new gp_Pnt( center.X() + x * co + ( b * z - c * y ) * si + a * dot * ( 1 - co ),
			center.Y() + y * co + ( c * x - a * z ) * si + b * dot * ( 1 - co ),
			center.Z() + z * co + ( a * y - b * x ) * si + c * dot * ( 1 - co ) );
	}
	static gp_Pnt ExternalWorldPoint( Fixture f, gp_Pnt p, ExternalAxisPos pos )
	{
		p = p.Translated( f.Machine.SimulationOffset );
		if( f.Machine.ExternalAxes.Type == ExternalAxisType.Dual ) { p = Rotate( p, f.Machine.ExternalAxes.Ext2Axis.RotateCenter, f.Machine.ExternalAxes.Ext2Axis.RotateDir, -pos.Ext2 * Math.PI / 180 ); }
		if( f.Machine.ExternalAxes.Type != ExternalAxisType.None ) { p = Rotate( p, f.Machine.ExternalAxes.Ext1Axis.RotateCenter, f.Machine.ExternalAxes.Ext1Axis.RotateDir, -pos.Ext1 * Math.PI / 180 ); }
		return p;
	}
	static void Alignment( Fixture f, PostPoint p, Dictionary<MachineComponentType, List<gp_Trsf>> frames, int index )
	{
		gp_Pnt source = new gp_Pnt( p.X, p.Y, p.Z );
		gp_Pnt expected = ExternalWorldPoint( f, source, p.ExternalAxisPos );
		if( f.Machine.FiveAxisType == FiveAxisType.Mix ) { expected = Rotate( expected, f.Machine.SlaveAxis.RotateCenter, f.Machine.SlaveAxis.RotateDir, -p.Slave ); }
		gp_Pnt work = source.Transformed( frames[MachineComponentType.WorkPiece][index] );
		Point( work, expected );
		gp_Pnt tip = new gp_Pnt( -f.Machine.ToolDir.X() * f.Machine.ToolLength, -f.Machine.ToolDir.Y() * f.Machine.ToolLength, -f.Machine.ToolDir.Z() * f.Machine.ToolLength );
		Point( tip.Transformed( frames[MachineComponentType.Laser][index] ), work );
	}
	static void Preview( string kind, ExternalAxisType type )
	{
		Fixture f = new Fixture( kind, type );
		PostPoint p = new PostPoint { X = 2, Y = 3, Z = 5, Master = .4, Slave = -.6, ExternalAxisPos = new ExternalAxisPos( 37, -53 ) };
		Dictionary<MachineComponentType, List<gp_Trsf>> frames;
		Assert( new FiveAxisSimuPathBuilder().BuildSinglePointFrameTransform( f.Machine, new gp_Pnt( p.X, p.Y, p.Z ), p, out frames ), "Preview failed." );
		Alignment( f, p, frames, 0 );
		Assert( frames.Count == f.Machine.ComponentTypes.Count && frames.Values.All( list => list.Count == 1 ), "Preview component count changed." );
	}
	static ExternalAxisPos Effective( Fixture f, ExternalAxisPos pos ) { return f.Machine.ExternalAxes.Type == ExternalAxisType.None ? default( ExternalAxisPos ) : new ExternalAxisPos( pos.Ext1, f.Machine.ExternalAxes.Type == ExternalAxisType.Dual ? pos.Ext2 : 0 ); }
	static IEnumerable<PostPoint> AllPoints( IPathSimuData path ) { return path.TraversePoints.Concat( path.PathPoints ).Concat( path.ExitPoints ).Cast<PostPoint>(); }
	static void CheckGroups( Fixture f )
	{
		f.Convert(); List<PathSimuIKData> paths = f.Collect(); object[] groups = Items( f.Groups );
		Assert( paths.Count == f.Flat.Count, "Grouped paths lost order or count." );
		int pathIndex = 0;
		foreach( object group in groups ) {
			object[] posts = Items( Get( group, "PostDataList" ) );
			ExternalAxisPos effective = (ExternalAxisPos)Get( Get( group, "Group" ), "ExternalAxisPos" );
			CAMPoint start = (CAMPoint)Get( group, "ProcessStartPoint" ), end = (CAMPoint)Get( group, "ProcessEndPoint" );
			List<PostPointBase> first = paths[pathIndex].TraversePntIKList;
			if( f.Entry.EntryDistance > 0 ) {
				PostPoint entry = (PostPoint)first.First();
				Point( new gp_Pnt( entry.X, entry.Y, entry.Z ), TraverseHelper.GetCutDownOrLiftUpPoint( start, f.Entry.EntryDistance ).Point );
				Close( entry.Master, start.ModMaster_rad ); Close( entry.Slave, start.ModSlave_rad );
			}
			for( int i = 0; i < posts.Length; i++, pathIndex++ ) {
				PathSimuIKData path = paths[pathIndex];
				Assert( AllPoints( path ).All( p => Effective( f, p.ExternalAxisPos ).Equals( effective ) ), "Cross-group angle interpolation found." );
				Assert( path.PathPoints.SequenceEqual( f.Flat[pathIndex].LeadInPostPointList.Concat( f.Flat[pathIndex].MainPathPostPointList ).Concat( f.Flat[pathIndex].OverCutPostPointList ).Concat( f.Flat[pathIndex].LeadOutPostPointList ).Cast<PostPointBase>() ), "Processing segments changed." );
				if( i != posts.Length - 1 ) { Assert( path.ExitPoints.Count == 0, "Exit added inside a group." ); }
				else {
					PostPoint last = (PostPoint)path.ExitPoints.Last();
					Point( new gp_Pnt( last.X, last.Y, last.Z ), TraverseHelper.GetCutDownOrLiftUpPoint( end, f.Entry.ExitDistance ).Point );
					Close( last.Master, end.ModMaster_rad ); Close( last.Slave, end.ModSlave_rad );
				}
			}
		}
		object[] ranges; int count; var frames = f.Frames( out ranges, out count );
		int expectedCount = paths.Sum( p => p.TraversePoints.Count + p.PathPoints.Count + p.ExitPoints.Count );
		Assert( count == expectedCount && frames.Values.All( list => list.Count == count ), "Hidden transition frames were inserted or frames lost." );
		int frame = 0;
		for( int i = 0; i < paths.Count; i++ ) {
			Assert( (int)Get( ranges[i], "StartIndex" ) == frame, "Wrong grouped start index." );
			foreach( PostPoint p in AllPoints( paths[i] ) ) {
				Alignment( f, p, frames, frame );
				Dictionary<MachineComponentType, List<gp_Trsf>> preview;
				Assert( new FiveAxisSimuPathBuilder().BuildSinglePointFrameTransform( f.Machine, new gp_Pnt( p.X, p.Y, p.Z ), p, out preview ), "Point preview failed." );
				foreach( var pair in frames ) { Matrix( pair.Value[frame], preview[pair.Key][0] ); }
				frame++;
			}
			Assert( (int)Get( ranges[i], "EndIndex" ) == frame - 1, "Wrong grouped end index." );
		}
	}

	public static void SpindleSinglePreviewAligns() { Preview( "Spindle", ExternalAxisType.Single ); }
	public static void SpindleDualPreviewAligns() { Preview( "Spindle", ExternalAxisType.Dual ); }
	public static void MixSinglePreviewAligns() { Preview( "Mix", ExternalAxisType.Single ); }
	public static void ExternalLocalFramesAndZeroRegression()
	{
		foreach( string kind in new[] { "Spindle", "Mix" } ) {
			foreach( ExternalAxisType type in kind == "Mix" ? new[] { ExternalAxisType.None, ExternalAxisType.Single } : new[] { ExternalAxisType.None, ExternalAxisType.Single, ExternalAxisType.Dual } ) {
				foreach( RotaryAxis axis in new[] { RotaryAxis.X, RotaryAxis.Y, RotaryAxis.Z } ) {
					Fixture f = new Fixture( kind, type ); f.Machine.ExternalAxes.Ext1RotaryAxis = axis;
					f.Machine.ExternalAxes.Ext1TiltedVec_deg = new gp_XYZ( 17, 23, 31 );
					f.Machine.ExternalAxes.InitRotateAxes();
					FiveAxisPerFrameTransformBuilder builder = new FiveAxisPerFrameTransformBuilder( f.Machine );
					foreach( ExternalAxisPos pos in new[] { default( ExternalAxisPos ), new ExternalAxisPos( 37, -53 ) } ) {
						gp_Trsf[] buf = new gp_Trsf[builder.ComponentTypes.Count];
						builder.BuildFrameTransforms( new PostPoint { ExternalAxisPos = pos }, f.Machine.SimulationOffset, f.Machine.WorkPieceChain, buf );
						foreach( MachineComponentType component in new[] { MachineComponentType.Ext1, MachineComponentType.Ext2 } ) {
							int index = builder.IndexOf( component ); if( index < 0 ) { continue; }
							RotateAxisDef def = component == MachineComponentType.Ext1 ? f.Machine.ExternalAxes.Ext1Axis : f.Machine.ExternalAxes.Ext2Axis;
							double angle = component == MachineComponentType.Ext1 ? pos.Ext1 : pos.Ext2;
							foreach( gp_Pnt probe in new[] { new gp_Pnt(), new gp_Pnt( 2, 3, 5 ), new gp_Pnt( -7, 11, 13 ) } ) { Point( probe.Transformed( buf[index] ), Rotate( probe, def.RotateCenter, def.RotateDir, -angle * Math.PI / 180 ) ); }
						}
						Dictionary<MachineComponentType, List<gp_Trsf>> frames;
						PostPoint p = new PostPoint { X = 2, Y = 3, Z = 5, Master = .4, Slave = -.6, ExternalAxisPos = pos };
						Assert( new FiveAxisSimuPathBuilder().BuildSinglePointFrameTransform( f.Machine, new gp_Pnt( 2, 3, 5 ), p, out frames ), "Zero/nominal-axis preview failed." ); Alignment( f, p, frames, 0 );
					}
				}
			}
		}
	}
	public static void GroupRunsHaveIndependentEntryExit()
	{
		foreach( string kind in new[] { "Spindle", "Mix" } ) {
			foreach( ExternalAxisType type in kind == "Mix" ? new[] { ExternalAxisType.Single } : new[] { ExternalAxisType.Single, ExternalAxisType.Dual } ) {
				Fixture f = new Fixture( kind, type ); foreach( double a in new[] { 0.0, 0, 180, 180, 0 } ) { f.Add( a, type == ExternalAxisType.Dual ? a / 3 : 91 ); }
				CheckGroups( f ); Assert( Items( f.Groups ).Length == 3, "Wrong run count." );
			}
		}
	}
	public static void FullTurnsAndZeroDistancesKeepBoundaries()
	{
		foreach( double distance in new[] { 0.0, 5.0 } ) {
			Fixture f = new Fixture( "Spindle", ExternalAxisType.Single ); f.Entry.EntryDistance = distance; f.Entry.ExitDistance = distance;
			foreach( double a in new[] { 0.0, 360, 0 } ) { f.Add( a, 17 ); }
			CheckGroups( f ); Assert( Items( f.Groups ).Length == 3, "Full turns merged." );
		}
	}
	public static void LeadOutOverCutAndStandardGroupEndpoints()
	{
		Fixture f = new Fixture( "Spindle", ExternalAxisType.Dual );
		ContourPathObject lead = f.Add( 37, -53 ); lead.CraftData.LeadData = new LeadData( 2, 0, 90, 3, 0, 90 ); lead.CraftData.OverCutLength = 2;
		ContourPathObject over = f.Add( -23, 41 ); over.CraftData.OverCutLength = 2;
		ContourPathObject source = f.Add( 0, 0 ); source.CraftData.ProcessPath = false;
		CraftData craft = new CraftData { ExternalAxisPos = new ExternalAxisPos( 73, 19 ), LeadData = new LeadData( 2 ) };
		CircleGeomData geom = new CircleGeomData( new gp_Ax1( new gp_Pnt( 90, 0, 0 ), new gp_Dir( 0, 0, 1 ) ), 20, 0, false );
		PathObject circle = (PathObject)New( "MyCAM.Data.CirclePathObject", "Circle", geom, craft, source ); f.IDs.Add( circle.UID ); f.Map.Add( circle.UID, circle );
		CheckGroups( f ); object[] groups = Items( f.Groups );
		Point( ( (CAMPoint)Get( groups[0], "ProcessEndPoint" ) ).Point, lead.ContourCache.LeadOutPointList.Last().Point );
		Point( ( (CAMPoint)Get( groups[1], "ProcessEndPoint" ) ).Point, over.ContourCache.OverCutPointList.Last().Point );
	}
	public static void TraverseModesStayWithinGroups()
	{
		foreach( TraverseData.TraverseType mode in new[] { TraverseData.TraverseType.FrogLeap, TraverseData.TraverseType.SafePlane, TraverseData.TraverseType.SafeCylinder, TraverseData.TraverseType.SafeSphere } ) {
			Fixture f = new Fixture( "Spindle", ExternalAxisType.Single );
			foreach( double a in new[] { 37.0, 37, -23 } ) {
				ContourPathObject p = f.Add( a, 0 ); p.CraftData.TraverseData.TraverseMode = mode; p.CraftData.TraverseData.FrogLeapDistance = 8;
				p.CraftData.TraverseData.SafePlaneDistance = 50; p.CraftData.TraverseData.SafeCylinderRadius = 150; p.CraftData.TraverseData.SafeSphereRadius = 150;
			}
			CheckGroups( f ); Assert( f.Collect()[1].TraversePoints.Count > 0, "Within-group traverse missing for " + mode );
		}
	}
	static Dictionary<string, List<AIS_Line>> Lines( object renderer ) { return (Dictionary<string, List<AIS_Line>>)renderer.GetType().GetField( "m_TraverseAISDict", Flags ).GetValue( renderer ); }
	static void BuildRenderer( Fixture f, object renderer )
	{
		Call( "MyCAM.Editor.Renderer.TraverseRenderer", "ShowTraversePathWithTrans", renderer, f.IDs, null );
		Call( "MyCAM.Editor.Renderer.TraverseRenderer", "ShowEntryAndExitWithTrans", renderer, new object[] { null } );
	}
	public static void RendererUsesSameGroupBoundaries()
	{
		Fixture f = new Fixture( "Spindle", ExternalAxisType.Single ); foreach( double a in new[] { 0.0, 0, 180, 180, 0 } ) { f.Add( a, 0 ); }
		f.Convert(); object renderer = f.Renderer(); BuildRenderer( f, renderer ); var lines = Lines( renderer );
		Assert( lines.Keys.Count( k => k.StartsWith( "KEY_ENTRY:" ) ) == 3 && lines.Keys.Count( k => k.StartsWith( "KEY_EXIT:" ) ) == 3, "Group endpoint keys collide." );
		Assert( lines.ContainsKey( f.IDs[1] ) && lines.ContainsKey( f.IDs[3] ), "Within-group lines missing." );
		foreach( int index in new[] { 0, 2, 4 } ) { Assert( !lines.ContainsKey( f.IDs[index] ), "Cross-group traverse drawn." ); }
		foreach( object group in Items( f.Groups ) ) {
			string[] ids = Items( Get( Get( group, "Group" ), "PathIDs" ) ).Cast<string>().ToArray();
			Assert( lines["KEY_ENTRY:" + ids.First()].Count == 1 && lines["KEY_EXIT:" + ids.Last()].Count == 1, "Entry/Exit line missing." );
		}
	}
	public static void SafeBoundarySettingsPreviewRetainsAdjacentReferenceAcrossGroups()
	{
		foreach( int configuration in new[] { 0, 1, 2, 3 } ) {
			ExternalAxisType type = configuration == 0 ? ExternalAxisType.None : configuration == 2 ? ExternalAxisType.Dual : ExternalAxisType.Single;
			foreach( double angle in new[] { 0.0, 90, 360 } ) {
				Fixture f = new Fixture( configuration == 3 ? "Mix" : "Spindle", type );
				ContourPathObject a = f.Add( 0, 0 ), b = f.Add( angle, 0 );
				f.Convert();
				bool sameGroup = type == ExternalAxisType.None || angle == 0;
				Assert( Items( f.Groups ).Length == ( sameGroup ? 1 : 2 ), "Wrong actual group boundaries." );
				var previous = (Dictionary<string, string>)Call( "MyCAM.Editor.Renderer.TraverseRenderer", "GetPreviousProcessPathMap", f.Renderer() );
				Assert( previous.ContainsKey( b.UID ) == sameGroup, "Settings preview changed the actual traverse predecessor." );
				object action = New( "MyCAM.Editor.TraverseAction", f.Manager, new List<string> { b.UID }, null );
				foreach( TraverseData.TraverseType mode in new[] { TraverseData.TraverseType.SafePlane, TraverseData.TraverseType.SafeCylinder, TraverseData.TraverseType.SafeSphere } ) {
					TraverseData data = new TraverseData { TraverseMode = mode, LiftUpDistance = 3, CutDownDistance = 4,
						SafePlaneDistance = 25, SafeCylinderRadius = 80, SafeSphereRadius = 80 };
					gp_Dir axis = TraverseHelper.GetSafeBoundaryAxis();
					var results = (List<TraversePathResult>)Call( "MyCAM.Editor.TraverseAction", "CollectTraverseResults", action, data,
						mode == TraverseData.TraverseType.SafePlane ? null : axis );
					Assert( results.Count == 1, "Settings preview must retain its adjacent-path reference even across groups." );
					Point( results[0].LiftUpPoint.Point, TraverseHelper.GetCutDownOrLiftUpPoint( CacheHelper.GetProcessEndPoint( a.UID ), 3 ).Point );
					Point( results[0].CutDownPoint.Point, TraverseHelper.GetCutDownOrLiftUpPoint( CacheHelper.GetProcessStartPoint( b.UID ), 4 ).Point );
					bool isPlane = mode == TraverseData.TraverseType.SafePlane;
					object[] bounds = isPlane ? new object[] { results, null, null, null, null }
						: new object[] { results, new gp_Ax3( new gp_Pnt( 0, 0, 0 ), axis ), 80.0, mode == TraverseData.TraverseType.SafeSphere, null, null, null, null };
					bool hasBounds = (bool)Call( "MyCAM.Helper.TraverseHelper", isPlane ? "TryGetPlanePreviewBounds" : "TryGetRadialPreviewBounds", null, bounds );
					int firstBound = isPlane ? 1 : 4;
					Assert( hasBounds && (double)bounds[firstBound] < (double)bounds[firstBound + 1]
						&& (double)bounds[firstBound + 2] < (double)bounds[firstBound + 3], "Settings preview has no usable reference bounds." );
				}
			}
		}
	}
	public static void RendererCleanupAfterMergeDeleteAndDisable()
	{
		Fixture f = new Fixture( "Spindle", ExternalAxisType.Single ); var a = f.Add( 0, 0 ); var b = f.Add( 180, 0 ); var c = f.Add( 0, 0 );
		object renderer = f.Renderer(); BuildRenderer( f, renderer ); var lines = Lines( renderer ); string[] oldKeys = lines.Keys.ToArray();
		b.CraftData.ExternalAxisPos = default( ExternalAxisPos ); c.CraftData.ProcessPath = false; f.IDs.Remove( a.UID ); f.Map.Remove( a.UID );
		var keys = (List<string>)Call( "MyCAM.Editor.Renderer.TraverseRenderer", "GetKeysToRemove", renderer, new List<string> { b.UID } );
		Assert( oldKeys.All( keys.Contains ), "Stale endpoint keys were not selected for removal." );
		// No graphics context in this harness: verify dictionary removal with empty AIS lists.
		foreach( var list in lines.Values ) { list.Clear(); }
		Call( "MyCAM.Editor.Renderer.TraverseRenderer", "RemoveTraverseLines", renderer, new List<string> { b.UID } );
		Assert( lines.Count == 0, "Stale keys survived cleanup." ); BuildRenderer( f, renderer );
		Assert( lines.Keys.OrderBy( k => k ).SequenceEqual( new[] { "KEY_ENTRY:" + b.UID, "KEY_EXIT:" + b.UID }.OrderBy( k => k ) ), "Merged endpoint keys not rebuilt." );
		f.Entry.EntryDistance = 0; f.Entry.ExitDistance = 0; foreach( var list in lines.Values ) { list.Clear(); }
		Call( "MyCAM.Editor.Renderer.TraverseRenderer", "RemoveTraverseLines", renderer, f.IDs ); BuildRenderer( f, renderer ); Assert( lines.Count == 0, "Zero-distance endpoints retained." );
	}
	public static void RendererPartialRefreshSkipsDisabledPaths()
	{
		Fixture f = new Fixture( "Mix", ExternalAxisType.Single ); var a = f.Add( 0, 0 ); f.Add( 180, 0 ).CraftData.ProcessPath = false; var c = f.Add( 0, 0 );
		object renderer = f.Renderer(); var paths = (List<string>)Call( "MyCAM.Editor.Renderer.TraverseRenderer", "GetPathsToUpdate", renderer, new List<string> { a.UID } );
		Assert( paths.SequenceEqual( new[] { a.UID, c.UID } ), "Partial update missed the next processing path." );
		BuildRenderer( f, renderer ); Assert( Lines( renderer ).ContainsKey( c.UID ), "Disabled path split a group." );
		f.IDs.Remove( a.UID ); f.Map.Remove( a.UID );
		paths = (List<string>)Call( "MyCAM.Editor.Renderer.TraverseRenderer", "GetPathsToUpdate", renderer, new List<string> { a.UID } );
		Assert( paths.SequenceEqual( f.IDs ), "Deleted-path refresh did not rebuild the surviving successors." );
	}
	public static void EmptyGroupsDoNotFallBackToFlatPaths()
	{
		Fixture f = new Fixture( "Spindle", ExternalAxisType.Single ); f.Add( 37, 0 ).CraftData.ProcessPath = false; f.Convert();
		object[] args = { f.Input(), null, null, 0 };
		Assert( !(bool)Call( "MyCAM.Helper.FiveAxisSimuPathBuilder", "BuildFrameTransMap", new FiveAxisSimuPathBuilder(), args ) && (int)args[3] == 0, "Empty groups built frames." );
		object renderer = f.Renderer(); BuildRenderer( f, renderer ); Assert( Lines( renderer ).Count == 0, "Disabled-only input drew lines." );
	}
	public static void LegacyAndZeroSingleGroupFrameRegression()
	{
		foreach( string kind in new[] { "Spindle", "Mix" } ) {
			foreach( ExternalAxisType type in new[] { ExternalAxisType.None, ExternalAxisType.Single } ) {
				Fixture f = new Fixture( kind, type ); f.Add( 0, 0 ); f.Add( 0, 0 ); CheckGroups( f );
				object[] ranges; int count; var grouped = f.Frames( out ranges, out count );
				object input = f.Input(); input.GetType().GetField( "ProcessGroups", Flags ).SetValue( input, null );
				object[] args = { input, null, null, 0 };
				Assert( (bool)Call( "MyCAM.Helper.FiveAxisSimuPathBuilder", "BuildFrameTransMap", new FiveAxisSimuPathBuilder(), args ), "Legacy flat input failed." );
				Assert( (int)args[3] == count, "Single-group frame count changed." );
				var legacy = (Dictionary<MachineComponentType, List<gp_Trsf>>)args[1];
				foreach( var component in grouped.Keys ) { for( int i = 0; i < count; i++ ) { Matrix( grouped[component][i], legacy[component][i] ); } }
			}
		}
	}
	public static void RendererAppliesPreviewTransformToEveryGroup()
	{
		Fixture f = new Fixture( "Spindle", ExternalAxisType.Dual ); f.Add( 37, -53 ); f.Add( 37, -53 ); f.Add( -23, 41 );
		object renderer = f.Renderer(); gp_Trsf transform = f.Solver.GetExternalAxisTransform( new ExternalAxisPos( 37, -53 ) );
		Call( "MyCAM.Editor.Renderer.TraverseRenderer", "ShowTraversePathWithTrans", renderer, f.IDs, transform );
		Call( "MyCAM.Editor.Renderer.TraverseRenderer", "ShowEntryAndExitWithTrans", renderer, transform );
		foreach( var list in Lines( renderer ).Values ) { foreach( AIS_Line line in list ) { Matrix( line.LocalTransformation(), transform ); } }
		gp_Trsf next = f.Solver.GetExternalAxisTransform( new ExternalAxisPos( -23, 41 ) );
		Call( "MyCAM.Editor.Renderer.TraverseRenderer", "Trans", renderer, next, false );
		foreach( var list in Lines( renderer ).Values ) { foreach( AIS_Line line in list ) { Matrix( line.LocalTransformation(), next ); } }
	}
	public static void StandardAndContourShareWithinGroupTraverse()
	{
		Fixture f = new Fixture( "Mix", ExternalAxisType.Single ); ContourPathObject source = f.Add( 37, 0 );
		CraftData craft = new CraftData { ExternalAxisPos = new ExternalAxisPos( 37, 0 ), LeadData = new LeadData( 2 ) };
		craft.TraverseData.LiftUpDistance = 3; craft.TraverseData.CutDownDistance = 4;
		CircleGeomData geom = new CircleGeomData( new gp_Ax1( new gp_Pnt( 50, 0, 0 ), new gp_Dir( 0, 0, 1 ) ), 20, 0, false );
		PathObject circle = (PathObject)New( "MyCAM.Data.CirclePathObject", "Circle", geom, craft, source ); f.IDs.Add( circle.UID ); f.Map.Add( circle.UID, circle ); f.Add( -23, 0 );
		CheckGroups( f ); Assert( Items( f.Groups ).Length == 2 && f.Collect()[1].TraversePoints.Count > 0, "Mixed within-group traverse missing." );
		object renderer = f.Renderer(); BuildRenderer( f, renderer ); Assert( Lines( renderer ).ContainsKey( circle.UID ), "Mixed renderer traverse missing." );
	}
	public static void CollisionIncludesExternalComponentsWithoutTransitionFrames()
	{
		Fixture f = new Fixture( "Spindle", ExternalAxisType.Dual ); f.Add( 37, -53 ); f.Add( -23, 41 ); f.Convert();
		object[] ranges; int count; var frames = f.Frames( out ranges, out count );
		int[] indices = { 0, 2, 1, 0, 3, 2, 4, 5, 6, 4, 6, 7, 0, 1, 5, 0, 5, 4, 2, 3, 7, 2, 7, 6, 0, 4, 7, 0, 7, 3, 1, 2, 6, 1, 6, 5 };
		using( CollisionSolver engine = new CollisionSolver() ) {
			foreach( var component in frames.Keys ) {
				double shift = component == MachineComponentType.Master || component == MachineComponentType.Ext1 ? 0 : 1000 + (int)component * 100;
				List<double> vertices = new List<double>();
				foreach( var xyz in new[] { new[] { -1.0, -1, -1 }, new[] { 1.0, -1, -1 }, new[] { 1.0, 1, -1 }, new[] { -1.0, 1, -1 }, new[] { -1.0, -1, 1 }, new[] { 1.0, -1, 1 }, new[] { 1.0, 1, 1 }, new[] { -1.0, 1, 1 } } ) {
					gp_Pnt local = new gp_Pnt( xyz[0] + shift, xyz[1], xyz[2] ).Transformed( frames[component][0].Inverted() ); vertices.AddRange( new[] { local.X(), local.Y(), local.Z() } );
				}
				engine.AddModel( component.ToString(), indices, vertices.ToArray() );
			}
			object[] args = { count, f.Machine, f.Machine.ChainListMap, engine, null, frames, null };
			Assert( (bool)Call( "MyCAM.Helper.CollisionHelper", "BuildCollision", null, args ), "Collision building failed." );
			var hits = (Dictionary<MachineComponentType, List<bool>>)args[6];
			Assert( hits[MachineComponentType.Ext1].Count == count && hits[MachineComponentType.Ext2].Count == count, "External collision index/count mismatch." );
			for( int i = 0; i < count; i++ ) {
				double[] master = (double[])Call( "MyCAM.Helper.CollisionHelper", "ConvertTransform", null, frames[MachineComponentType.Master][i] );
				double[] ext = (double[])Call( "MyCAM.Helper.CollisionHelper", "ConvertTransform", null, frames[MachineComponentType.Ext1][i] );
				Assert( hits[MachineComponentType.Ext1][i] == engine.CheckCollision( "Master", "Ext1", master, ext ), "External collision used the wrong transform." );
			}
			Assert( hits[MachineComponentType.Ext1][0], "Controlled external collision was not detected." );
		}
	}
}
