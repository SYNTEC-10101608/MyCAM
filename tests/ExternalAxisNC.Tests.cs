using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CommonData;
using FileTranslator;
using MyCAM.Data;
using MyCAM.PathCache;
using OCC.gp;

public static class ExternalAxisNCTests
{
	static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	static readonly Assembly App = typeof( CraftData ).Assembly;
	static void Assert( bool condition, string message ) { if( !condition ) { throw new Exception( message ); } }
	static object New( string name, params object[] args ) { return Activator.CreateInstance( App.GetType( name, true ), Flags, null, args, null ); }
	static object Call( string name, string method, object target, params object[] args ) { return App.GetType( name, true ).GetMethods( Flags ).Single( m => m.Name == method && m.GetParameters().Length == args.Length ).Invoke( target, args ); }
	static object Get( object target, string name ) { return target.GetType().GetProperty( name, Flags ).GetValue( target, null ); }
	static void Set( object target, string name, object value ) { target.GetType().GetProperty( name, Flags ).SetValue( target, value, null ); }
	static object[] Items( object value ) { return ( (IEnumerable)value ).Cast<object>().ToArray(); }
	static FieldInfo PostField { get { return App.GetType( "MyCAM.App.MyApp", true ).GetField( "m_CustPost", Flags ); } }
	static CustPost Custom( string section )
	{
		return new CustPost( new Dictionary<string, string> {
			{ "Header", "// FILE" }, { "Tail", "// END" }, { "TraverseHeader", "// PATH {PathIndex}" },
			{ "PathHeader", "// CUT" }, { "PathTail", "// CUT END" }, { "ExternalAxisHeader", section }
		} );
	}
	static string[] Lines( string text ) { return text.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries ); }
	static string Capture( Action<StreamWriter> write )
	{
		string path = Path.GetTempFileName();
		try { using( StreamWriter writer = new StreamWriter( path ) ) { write( writer ); } return File.ReadAllText( path ); }
		finally { File.Delete( path ); }
	}

	sealed class Fixture
	{
		public object Manager, Writer;
		public FiveAxisMachineData Machine;
		public List<string> IDs;
		public Dictionary<string, IObject> Map;
		public EntryAndExitData Entry;
		public Fixture( ExternalAxisType type, bool mix )
		{
			Machine = mix ? (FiveAxisMachineData)new MixTypeMachineData { ToolToMasterVec = new gp_Vec( 4, 5, 6 ), MCSToSlaveVec = new gp_Vec( 11, 13, 19 ) }
				: new SpindleTypeMachineData { ToolToSlaveVec = new gp_Vec( 4, 5, 6 ), SlaveToMasterVec = new gp_Vec( 7, 8, 9 ) };
			Machine.ToolDirection = ToolDirection.Z; Machine.MasterRotaryAxis = mix ? RotaryAxis.X : RotaryAxis.Z; Machine.SlaveRotaryAxis = mix ? RotaryAxis.Z : RotaryAxis.X;
			Machine.MasterTiltedVec_deg = new gp_XYZ(); Machine.SlaveTiltedVec_deg = new gp_XYZ(); Machine.ToolLength = 12;
			Machine.SimulationOffset = new gp_Vec( 17, -23, 31 ); Machine.ExternalAxes.Type = type;
			Machine.ExternalAxes.Ext1RotaryAxis = RotaryAxis.Z; Machine.ExternalAxes.Ext2RotaryAxis = RotaryAxis.X;
			Machine.ExternalAxes.MCSToExt1Vec = new gp_Vec( 10, 20, 30 ); Machine.ExternalAxes.Ext1ToExt2Vec = new gp_Vec( 5, 7, 11 );
			Machine.InitRotateAxes( RotaryDirection.RightHand, 0, 0, RotaryDirection.LeftHand, 0, 0 );
			Manager = New( "MyCAM.Data.DataManager" ); Set( Manager, "MachineData", Machine ); Call( "MyCAM.Data.DataGettingHelper", "Initialize", null, Manager );
			IDs = (List<string>)Get( Manager, "PathIDList" ); Map = (Dictionary<string, IObject>)Get( Manager, "ObjectMap" );
			Entry = (EntryAndExitData)Get( Manager, "EntryAndExitData" ); Entry.EntryDistance = 5; Entry.ExitDistance = 7;
			Writer = New( "MyCAM.Post.NCWriter", Manager );
		}
		public ContourPathObject Add( double first, double second )
		{
			string id = "Path" + IDs.Count; double x = 10 + IDs.Count * 20;
			CraftData craft = new CraftData { ExternalAxisPos = new ExternalAxisPos( first, second ) }; craft.TraverseData.LiftUpDistance = 3; craft.TraverseData.CutDownDistance = 4;
			List<CADPoint> points = new List<CADPoint>();
			foreach( gp_Pnt point in new[] { new gp_Pnt( x, 0, 0 ), new gp_Pnt( x, 10, 0 ), new gp_Pnt( x - 10, 10, 0 ), new gp_Pnt( x - 10, 0, 0 ) } ) {
				points.Add( new CADPoint( point, new gp_Dir( 0, 1, 1 ), new gp_Dir( 0, 0, 1 ), new gp_Dir( 1, 0, 0 ) ) );
			}
			ContourPathObject path = new ContourPathObject( id, null, new ContourGeomData( points, new Dictionary<CADPoint, CADPoint>(), true ), craft ); IDs.Add( id ); Map.Add( id, path ); return path;
		}
		public PathObject Circle( ContourPathObject source, double angle )
		{
			CircleGeomData geom = new CircleGeomData( new gp_Ax1( new gp_Pnt( 90, 0, 0 ), new gp_Dir( 0, 0, 1 ) ), 20, 23, false );
			CraftData craft = new CraftData { ExternalAxisPos = new ExternalAxisPos( angle, 19 ), LeadData = new LeadData( 2 ) };
			PathObject path = (PathObject)New( "MyCAM.Data.CirclePathObject", "Circle" + IDs.Count, geom, craft, source ); IDs.Add( path.UID ); Map.Add( path.UID, path ); return path;
		}
		public object[] Groups() { return Items( Call( "MyCAM.PathCache.CacheHelper", "GetProcessPathGroups", null, IDs, Machine ) ); }
		public string Export( CustPost custom )
		{
			object old = PostField.GetValue( null ); string path = Path.GetTempFileName();
			try {
				PostField.SetValue( null, custom ); object[] args = { "Test", path, null };
				Assert( (bool)Call( "MyCAM.Post.NCWriter", "ConvertSuccess", Writer, args ), "NC failed: " + args[2] ); return File.ReadAllText( path );
			}
			finally { PostField.SetValue( null, old ); File.Delete( path ); }
		}
	}

	// Independent MCS rotation, not the production transform or simulation FK.
	static gp_Pnt Rotate( gp_Pnt p, RotateAxisDef axis, double degrees )
	{
		gp_Pnt c = axis.RotateCenter; gp_Dir d = axis.RotateDir;
		double x = p.X() - c.X(), y = p.Y() - c.Y(), z = p.Z() - c.Z(), a = d.X(), b = d.Y(), k = d.Z();
		double angle = -degrees * Math.PI / 180, co = Math.Cos( angle ), si = Math.Sin( angle ), dot = a * x + b * y + k * z;
		return new gp_Pnt( c.X() + x * co + ( b * z - k * y ) * si + a * dot * ( 1 - co ),
			c.Y() + y * co + ( k * x - a * z ) * si + b * dot * ( 1 - co ), c.Z() + z * co + ( a * y - b * x ) * si + k * dot * ( 1 - co ) );
	}
	static gp_Pnt Positioned( Fixture f, PostPoint p )
	{
		gp_Pnt result = new gp_Pnt( p.X, p.Y, p.Z ); if( f.Machine.ExternalAxes.Type == ExternalAxisType.None ) { return result; }
		result = result.Translated( f.Machine.SimulationOffset );
		if( f.Machine.ExternalAxes.Type == ExternalAxisType.Dual ) { result = Rotate( result, f.Machine.ExternalAxes.Ext2Axis, p.ExternalAxisPos.Ext2 ); }
		result = Rotate( result, f.Machine.ExternalAxes.Ext1Axis, p.ExternalAxisPos.Ext1 );
		return result.Translated( f.Machine.SimulationOffset.Reversed() );
	}
	sealed class Move { public string Kind; public PostPoint[] Points; public Move( string kind, params PostPoint[] points ) { Kind = kind; Points = points; } }
	static void AddMove( List<Move> moves, string kind, PostPoint point ) { if( point != null ) { moves.Add( new Move( kind, point ) ); } }
	static PostPoint P( object data, string name ) { return (PostPoint)Get( data, name ); }
	static List<Move> ExpectedMoves( Fixture f, out List<string> macroLines )
	{
		List<Move> moves = new List<Move>(); macroLines = new List<string>(); object axis = New( "MyCAM.Post.AxisConfig", f.Machine );
		foreach( object group in f.Groups() ) {
			object end = null;
			foreach( object index in Items( Get( group, "PathIndices" ) ) ) {
				int i = (int)index; PathObject path = (PathObject)f.Map[f.IDs[i]]; bool entry = end == null;
				object[] args = path.PathType == PathType.Contour ? new object[] { i, end, null, null } : new object[] { i, end, null, null, null };
				Assert( (bool)Call( "MyCAM.Post.NCWriter", path.PathType == PathType.Contour ? "GetContourPostData" : "GetStdPostData", f.Writer, args ), "Expected post conversion failed." );
				end = args[1]; object data = args[2]; PostPoint start = P( data, "ProcessStartPoint" ), down = P( data, "CutDownPostPoint" );
				if( entry ) { AddMove( moves, "G00", down ); AddMove( moves, "G00", start ); }
				else {
					AddMove( moves, "G01", P( data, "LiftUpPostPoint" ) );
					List<PostPoint> boundary = (List<PostPoint>)Get( data, "SafeBoundaryPostPointList" );
					if( boundary != null ) { foreach( PostPoint p in boundary.Skip( 1 ) ) { AddMove( moves, "G01", p ); } }
					else if( P( data, "FrogLeapMidPostPoint" ) != null ) { moves.Add( new Move( "FROG", P( data, "FrogLeapMidPostPoint" ), down ?? start ) ); }
					else {
						if( P( data, "LiftUpPostSafePlanePoint" ) != null && P( data, "CutDownPostSafePlanePoint" ) != null ) { AddMove( moves, "G01", P( data, "LiftUpPostSafePlanePoint" ) ); AddMove( moves, "G01", P( data, "CutDownPostSafePlanePoint" ) ); }
						AddMove( moves, "G01", down ?? start );
					}
					if( down != null && new gp_Pnt( down.X, down.Y, down.Z ).Distance( new gp_Pnt( start.X, start.Y, start.Z ) ) > 0 ) { AddMove( moves, "G01", start ); }
				}
				if( path.PathType == PathType.Contour ) {
					PostData contour = (PostData)data;
					foreach( PostPoint p in contour.LeadInPostPointList.Concat( contour.MainPathPostPointList ).Concat( contour.OverCutPostPointList ).Concat( contour.LeadOutPostPointList ) ) { AddMove( moves, "G01", p ); }
				}
				else {
					object strategy = Call( "MyCAM.Post.StandardPatternNCStrategyFactory", "GetStrategy", null, path.PathType );
					object geom = Get( args[3], "GeomData" );
					macroLines.Add( Capture( w => strategy.GetType().GetMethod( "WriteNCCode", Flags ).Invoke( strategy, new[] { w, data, path.CraftData, geom, axis } ) ).Trim() );
				}
			}
			object[] exitArgs = { end, f.Entry, null }; Call( "MyCAM.Post.PostTraverseBuilder", "CalculateExit", null, exitArgs ); AddMove( moves, "G00", (PostPoint)exitArgs[2] );
		}
		return moves;
	}
	static double Number( string line, string key )
	{
		Match match = Regex.Match( line, @"(?:^|\s)" + key + @"(-?\d+(?:\.\d+)?(?:[Ee][+-]?\d+)?)" );
		Assert( match.Success, "Missing " + key + " in " + line ); return double.Parse( match.Groups[1].Value, CultureInfo.InvariantCulture );
	}
	static void Close( double actual, double expected, string line ) { Assert( Math.Abs( actual - expected ) <= .000501, "Expected " + expected + ", got " + actual + " in " + line ); }
	static void CheckNC( Fixture f )
	{
		CultureInfo old = CultureInfo.CurrentCulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
			List<string> macros; List<Move> expected = ExpectedMoves( f, out macros );
			string output = f.Export( Custom( "// POSITION {ExtAxisPos1} {ExtAxisPos2}" ) );
			string[] actual = Lines( output ).Where( s => s.StartsWith( "G00 " ) || s.StartsWith( "G01 " ) || s.StartsWith( "G65 P\"FROG_LEAP\"" ) ).ToArray();
			Assert( actual.Length == expected.Count, "Wrong NC move count: expected " + expected.Count + ", got " + actual.Length );
			object axis = New( "MyCAM.Post.AxisConfig", f.Machine );
			for( int i = 0; i < actual.Length; i++ ) {
				Move move = expected[i]; Assert( actual[i].StartsWith( move.Kind == "FROG" ? "G65 P\"FROG_LEAP\"" : move.Kind + " " ), "Wrong motion at " + i );
				for( int j = 0; j < move.Points.Length; j++ ) {
					PostPoint p = move.Points[j]; gp_Pnt positioned = Positioned( f, p ); string suffix = move.Kind == "FROG" ? ( j + 1 ).ToString() + "=" : "";
					Close( Number( actual[i], "X" + suffix ), positioned.X(), actual[i] ); Close( Number( actual[i], "Y" + suffix ), positioned.Y(), actual[i] ); Close( Number( actual[i], "Z" + suffix ), positioned.Z(), actual[i] );
					if( (bool)Get( axis, "WriteMaster" ) ) { Close( Number( actual[i], (string)Get( axis, "MasterAxisName" ) + suffix ), p.Master * 180 / Math.PI, actual[i] ); }
					if( (bool)Get( axis, "WriteSlave" ) ) { Close( Number( actual[i], (string)Get( axis, "SlaveAxisName" ) + suffix ), p.Slave * 180 / Math.PI, actual[i] ); }
				}
			}
			Assert( Lines( output ).Where( s => s.StartsWith( "G65 P\"SY_" ) ).SequenceEqual( macros ), "Standard macro contract changed." );
			Assert( output == f.Export( Custom( "// POSITION {ExtAxisPos1} {ExtAxisPos2}" ) ), "Repeated export changed coordinates." );
		}
		finally { CultureInfo.CurrentCulture = old; }
	}

	public static void OldCustomFilesRemainCompatible()
	{
		string old = "<Header>H<Tail>T<PathHeader>PH<PathTail>PT<StdPathHeader>SH<StdPathTail>ST<TraverseHeader>TH<TraverseTail>TT";
		TxtToCustPostTranslator translator = new TxtToCustPostTranslator(); CustPost post;
		Assert( translator.Translate( old, out post ) && post.ExternalAxisHeader == null, "Missing optional section was not preserved." );
		Assert( translator.Translate( old + "<ExternalAxisHeader>G65 P\"POSITION\" A{ExtAxisPos1} B{ExtAxisPos2};", out post ), "New section became mandatory or invalid." );
		Assert( post.ExternalAxisHeader.Contains( "{ExtAxisPos2}" ), "Optional section lost." );
		Assert( translator.Translate( old + "<ExternalAxisHeader>", out post ) && post.ExternalAxisHeader == "", "Explicit empty section was not preserved." );
		Assert( translator.Translate( old + "<ExternalAxisHeader>   ", out post ) && post.ExternalAxisHeader == "", "Optional empty section changed compatibility." );
		Assert( !translator.Translate( "<ExternalAxisHeader>FIXED", out post ), "Missing original sections no longer reported." );
		Assert( new CustPost().ExternalAxisHeader == null, "Default post must leave external output to the machine-specific fallback." );
	}
	public static void ExistingSectionsUseDefaultsOnlyWhenMissing()
	{
		TxtToCustPostTranslator translator = new TxtToCustPostTranslator(); CustPost post;
		Assert( !translator.Translate( "<Tail><PathHeader> \r\n\t", out post ), "Missing original sections were not reported." );
		Assert( post.Header == CustPost.DEFAULT_Header && post.TraverseHeader == CustPost.DEFAULT_TraverseHeader, "Missing sections did not retain defaults." );
		Assert( post.Tail == "" && post.PathHeader == "", "Explicit empty sections were replaced with defaults." );
	}
	public static void FixedSectionsDoNotRequireVariables()
	{
		foreach( ExternalAxisType type in new[] { ExternalAxisType.Single, ExternalAxisType.Dual } ) {
			Fixture f = new Fixture( type, false ); f.Add( 0, 0 ); f.Add( 37, 19 );
			string output = f.Export( Custom( "G65 P\"DEBUG_POSITION\";" ) ); Assert( Lines( output ).Count( s => s == "G65 P\"DEBUG_POSITION\";" ) == 2, "Fixed debug section was rejected or skipped." );
			Assert( !Lines( output ).Any( s => Regex.IsMatch( s, @"^[ABC] = " ) ), "Default output was appended to a custom section." );
		}
	}
	public static void MissingSectionsUseDefaultOutput()
	{
		TxtToCustPostTranslator translator = new TxtToCustPostTranslator(); CustPost translated;
		translator.Translate( "<Header>// FILE<Tail>// END", out translated );
		foreach( ExternalAxisType type in new[] { ExternalAxisType.Single, ExternalAxisType.Dual } ) {
			Fixture f = new Fixture( type, false ); f.Add( 37, -53 );
			foreach( CustPost custom in new[] { new CustPost(), new CustPost( new Dictionary<string, string>() ), translated, Custom( null ), null } ) {
				string[] sections = Lines( f.Export( custom ) ).Where( s => Regex.IsMatch( s, @"^[ABC] = " ) ).ToArray();
				Assert( sections.SequenceEqual( new[] { type == ExternalAxisType.Single ? "C = 37;" : "A = -53 C = 37;" } ), "Missing section did not use sorted, machine-specific default output." );
			}
		}
	}
	public static void ExplicitEmptySectionsSuppressDefaultOutput()
	{
		TxtToCustPostTranslator translator = new TxtToCustPostTranslator(); CustPost empty, blank;
		translator.Translate( "<ExternalAxisHeader>", out empty ); translator.Translate( "<ExternalAxisHeader> \r\n\t", out blank );
		foreach( ExternalAxisType type in new[] { ExternalAxisType.Single, ExternalAxisType.Dual } ) {
			Fixture f = new Fixture( type, false ); f.Add( 0, 0 ); f.Add( 37, -53 );
			foreach( CustPost custom in new[] { Custom( "" ), Custom( " \r\n\t" ), empty, blank } ) {
				string output = f.Export( custom );
				Assert( !Lines( output ).Any( s => Regex.IsMatch( s, @"^[ABC] = " ) ), "Explicit empty section incorrectly used default output." );
				Assert( Lines( output ).Count( s => s == "// External axis move" ) == 2, "Empty section suppressed the fixed block comment." );
				Assert( Lines( output ).Count( s => s == "// Exit" ) == 2 && output.Contains( "G00 " ), "Empty section suppressed machining groups or exits." );
			}
		}
	}
	public static void ExternalAxisBlocksAlwaysStartWithComment()
	{
		foreach( ExternalAxisType type in new[] { ExternalAxisType.Single, ExternalAxisType.Dual } ) {
			Fixture f = new Fixture( type, false ); f.Add( 0, 0 ); f.Add( 0, 0 ); f.Add( 37, -53 );
			foreach( string content in new[] { null, "", " \r\n\t", "FIXED {ExtAxisPos1}\nSECOND" } ) {
				string[] lines = Lines( f.Export( Custom( content ) ) ).Where( s => !string.IsNullOrWhiteSpace( s ) ).ToArray();
				int[] headers = Enumerable.Range( 0, lines.Length ).Where( i => lines[i] == "// External axis move" ).ToArray();
				Assert( headers.Length == 2, "Fixed block comment was missing or emitted per path rather than per group." );
				for( int group = 0; group < headers.Length; group++ ) {
					string next = lines[headers[group] + 1];
					if( content == null ) {
						Assert( Regex.IsMatch( next, @"^[ABC] = .*;$" ), "Fixed comment did not immediately precede the terminated default command." );
					}
					else if( string.IsNullOrWhiteSpace( content ) ) {
						Assert( next == ( group == 0 ? "// PATH 1" : "// PATH 3" ), "Empty section added commands after its fixed comment." );
					}
					else {
						Assert( next == "FIXED " + ( group == 0 ? "0" : "37" ) && lines[headers[group] + 2] == "SECOND", "Fixed comment was misplaced or custom content was altered." );
					}
				}
			}
		}
	}
	public static void DefaultOutputMapsAxisNamesAndKeepsSortedValues()
	{
		RotaryAxis[] axes = { RotaryAxis.X, RotaryAxis.Y, RotaryAxis.Z }; string[] names = { "A", "B", "C" };
		for( int first = 0; first < axes.Length; first++ ) {
			foreach( bool mix in new[] { false, true } ) {
				Fixture f = new Fixture( ExternalAxisType.Single, mix ); f.Machine.ExternalAxes.Ext1RotaryAxis = axes[first];
				f.Machine.InitRotateAxes( RotaryDirection.RightHand, 0, 0, RotaryDirection.LeftHand, 0, 0 ); f.Add( 12.5, 91 );
				Assert( Lines( f.Export( Custom( null ) ) ).Single( s => Regex.IsMatch( s, @"^[ABC] = " ) ) == names[first] + " = 12.5;", "Single-axis mapping or inactive-axis filtering failed." );
			}
			for( int second = 0; second < axes.Length; second++ ) {
				if( first == second ) { continue; }
				Fixture f = new Fixture( ExternalAxisType.Dual, false ); f.Machine.ExternalAxes.Ext1RotaryAxis = axes[first]; f.Machine.ExternalAxes.Ext2RotaryAxis = axes[second];
				f.Machine.InitRotateAxes( RotaryDirection.RightHand, 0, 0, RotaryDirection.LeftHand, 0, 0 ); f.Add( 12.5, -23.75 );
				string expected = first < second ? names[first] + " = 12.5 " + names[second] + " = -23.75" : names[second] + " = -23.75 " + names[first] + " = 12.5";
				Assert( Lines( f.Export( Custom( null ) ) ).Single( s => Regex.IsMatch( s, @"^[ABC] = " ) ) == expected + ";", "Dual-axis sorting detached values from their axes or split output lines." );
			}
		}
	}
	public static void DefaultGroupsWriteExitThenPositionThenEntry()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false ); foreach( double a in new[] { 0.0, 0, 180, 180, 0, 360 } ) { f.Add( a, 91 ); }
		string[] relevant = Lines( f.Export( Custom( null ) ) ).Where( s => Regex.IsMatch( s, @"^[ABC] = " ) || s.StartsWith( "// PATH" ) || s == "// Exit" || s == "// External axis move" ).ToArray();
		Assert( relevant.SequenceEqual( new[] { "// External axis move", "C = 0;", "// PATH 1", "// PATH 2", "// Exit", "// External axis move", "C = 180;", "// PATH 3", "// PATH 4", "// Exit", "// External axis move", "C = 0;", "// PATH 5", "// Exit", "// External axis move", "C = 360;", "// PATH 6", "// Exit" } ), "Default output changed group boundaries, zero/full-turn angles, or Entry/Exit ordering." );
	}
	public static void DefaultOutputUsesInvariantDegreesWithoutRounding()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); f.Add( 12.5, -23.75 ); f.Add( 360, 1e-12 ); CultureInfo old = CultureInfo.CurrentCulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "de-DE" ); string[] sections = Lines( f.Export( Custom( null ) ) ).Where( s => Regex.IsMatch( s, @"^[ABC] = " ) ).ToArray();
			Assert( sections.Length == 2 && sections[0] == "A = -23.75 C = 12.5;", "Default values were not degree/invariant numbers." );
			string[] words = sections[1].Split( ' ' ); Assert( double.Parse( words[2], CultureInfo.InvariantCulture ) == 1e-12 && words[5] == "360;", "Default output rounded small angles or normalized turns." );
		}
		finally { CultureInfo.CurrentCulture = old; }
	}
	public static void NoneUsesMachineTypeNotAssignedAngles()
	{
		Fixture f = new Fixture( ExternalAxisType.None, false ); f.Add( 37, 19 ); f.Add( 180, -53 ); CheckNC( f );
		string before = f.Export( Custom( "SHOULD NOT APPEAR" ) ); Assert( !before.Contains( "SHOULD NOT APPEAR" ), "None emitted external section." );
		Assert( !before.Contains( "// External axis move" ), "None emitted an external block comment." );
		Assert( before == f.Export( Custom( null ) ), "None emitted default external output." );
		foreach( PathObject p in f.Map.Values ) { p.CraftData.ExternalAxisPos = default( ExternalAxisPos ); }
		Assert( before == f.Export( Custom( "" ) ), "Inactive angles or optional section changed legacy NC." );
	}
	public static void GroupsWriteExitThenPositionThenEntry()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false ); foreach( double a in new[] { 0.0, 0, 180, 180, 0 } ) { f.Add( a, 91 ); }
		CheckNC( f ); string[] relevant = Lines( f.Export( Custom( "// POSITION {ExtAxisPos1} {ExtAxisPos2}" ) ) ).Where( s => s.StartsWith( "// POSITION" ) || s.StartsWith( "// PATH" ) || s == "// Exit" || s == "// External axis move" ).ToArray();
		Assert( relevant.SequenceEqual( new[] { "// External axis move", "// POSITION 0 0", "// PATH 1", "// PATH 2", "// Exit", "// External axis move", "// POSITION 180 0", "// PATH 3", "// PATH 4", "// Exit", "// External axis move", "// POSITION 0 0", "// PATH 5", "// Exit" } ), "Wrong grouped NC ordering." );
	}
	public static void FullTurnsAndExactGroupAngles()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false ); foreach( double a in new[] { 0.0, 360, 0, 1e-12 } ) { f.Add( a, 0 ); }
		CheckNC( f ); string[] sections = Lines( f.Export( Custom( "POSITION {ExtAxisPos1}" ) ) ).Where( s => s.StartsWith( "POSITION " ) ).ToArray();
		Assert( sections.Length == 4 && sections[1] == "POSITION 360", "Full turns merged." );
		Assert( double.Parse( sections[3].Substring( 9 ), CultureInfo.InvariantCulture ) == 1e-12, "Assigned small angle was rounded away." );
	}
	public static void SingleIgnoresInactiveSecondAndDualUsesBoth()
	{
		foreach( ExternalAxisType type in new[] { ExternalAxisType.Single, ExternalAxisType.Dual } ) {
			Fixture f = new Fixture( type, false ); f.Add( 37, 19 ); f.Add( 37, -53 ); CheckNC( f );
			Assert( Lines( f.Export( Custom( "POSITION {ExtAxisPos1} {ExtAxisPos2}" ) ) ).Count( s => s.StartsWith( "POSITION " ) ) == ( type == ExternalAxisType.Single ? 1 : 2 ), "Wrong effective group count." );
			Assert( Lines( f.Export( Custom( null ) ) ).Count( s => Regex.IsMatch( s, @"^[ABC] = " ) ) == ( type == ExternalAxisType.Single ? 1 : 2 ), "Default output used inactive angles or ignored the second active axis." );
		}
	}
	static void Supported( ExternalAxisType type, bool mix ) { Fixture f = new Fixture( type, mix ); f.Add( 37, -53 ); f.Add( 37, -53 ); f.Add( -23, 41 ); CheckNC( f ); }
	public static void SpindleSinglePositionsWithoutRTCP() { Supported( ExternalAxisType.Single, false ); }
	public static void SpindleDualPositionsWithoutRTCP() { Supported( ExternalAxisType.Dual, false ); }
	public static void MixSinglePositionsWithoutRTCP() { Supported( ExternalAxisType.Single, true ); }
	public static void EveryTraverseModeTransformsAllWrittenPoints()
	{
		foreach( TraverseData.TraverseType mode in new[] { TraverseData.TraverseType.FrogLeap, TraverseData.TraverseType.SafePlane, TraverseData.TraverseType.SafeCylinder, TraverseData.TraverseType.SafeSphere } ) {
			Fixture f = new Fixture( ExternalAxisType.Dual, false );
			foreach( double a in new[] { 37.0, 37, -23 } ) {
				ContourPathObject p = f.Add( a, -53 ); p.CraftData.TraverseData.TraverseMode = mode; p.CraftData.TraverseData.FrogLeapDistance = 8; p.CraftData.TraverseData.SafePlaneDistance = 50; p.CraftData.TraverseData.SafeCylinderRadius = 150; p.CraftData.TraverseData.SafeSphereRadius = 150;
			}
			CheckNC( f );
		}
	}
	public static void LeadsAndOverCutDefineGroupExits()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject lead = f.Add( 37, -53 ); lead.CraftData.LeadData = new LeadData( 2, 0, 90, 3, 0, 90 ); lead.CraftData.OverCutLength = 2;
		ContourPathObject over = f.Add( -23, 41 ); over.CraftData.OverCutLength = 2; CheckNC( f );
	}
	public static void StandardMacrosStayOriginalWhileCAMMovesTransform()
	{
		foreach( bool sameGroup in new[] { false, true } ) {
			Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject source = f.Add( 37, 19 ); f.Circle( source, sameGroup ? 37 : -23 ); f.Add( sameGroup ? 37 : 73, 19 ); CheckNC( f );
		}
	}
	public static void ZeroAnglesAndZeroDistances()
	{
		foreach( double distance in new[] { 0.0, 5 } ) {
			Fixture f = new Fixture( ExternalAxisType.Single, false ); f.Entry.EntryDistance = distance; f.Entry.ExitDistance = distance;
			foreach( double a in new[] { 0.0, 0, 360 } ) { ContourPathObject p = f.Add( a, 91 ); p.CraftData.TraverseData.CutDownDistance = distance; p.CraftData.TraverseData.LiftUpDistance = distance; }
			CheckNC( f ); Assert( Lines( f.Export( Custom( "POSITION {ExtAxisPos1}" ) ) ).Count( s => s.StartsWith( "POSITION " ) ) == 2, "Zero angle group was skipped." );
		}
	}
	public static void DisabledPathsKeepOriginalNumbersWithoutExtraGroups()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false ); f.Add( 37, 0 ); f.Add( 180, 0 ).CraftData.ProcessPath = false; f.Add( 37, 0 ); f.Add( -23, 0 ); CheckNC( f );
		string output = f.Export( Custom( "POSITION {ExtAxisPos1}" ) ); Assert( output.Contains( "N2;" ) && output.Contains( "// PATH 3" ) && !output.Contains( "// PATH 2" ), "Disabled path numbering changed." );
		Assert( Lines( output ).Count( s => s.StartsWith( "POSITION " ) ) == 2, "Disabled path introduced a group." );
	}
	public static void EmptyAndAllDisabledDoNotPosition()
	{
		foreach( ExternalAxisType type in new[] { ExternalAxisType.None, ExternalAxisType.Single, ExternalAxisType.Dual } ) {
			foreach( double distance in new[] { 0.0, 5 } ) {
				Fixture f = new Fixture( type, false ); f.Entry.EntryDistance = distance; f.Entry.ExitDistance = distance;
				for( int count = 0; count <= 3; count++ ) {
					if( count > 0 ) { f.Add( 37 * count, 19 ).CraftData.ProcessPath = false; }
					string output = f.Export( Custom( "POSITION" ) );
					Assert( output.Contains( "// FILE" ) && output.Contains( "// END" ), "Non-machining file header/tail lost." );
					Assert( !output.Contains( "POSITION" ) && !output.Contains( "G00 " ) && !output.Contains( "G01 " ) && !output.Contains( "// Exit" ) && !output.Contains( "// External axis move" ), "Non-machining input emitted movement or Exit: " + type + ", distance=" + distance + ", count=" + count );
					Assert( Lines( output ).Count( s => s == "// None Process Path" ) == count, "Disabled path annotations lost." );
					string defaultOutput = f.Export( Custom( null ) );
					Assert( !defaultOutput.Contains( "// External axis move" ) && !Lines( defaultOutput ).Any( s => Regex.IsMatch( s, @"^[ABC] = " ) ), "Non-machining input emitted default external output." );
				}
			}
		}
	}
	public static void DisabledFirstAndLastDoNotChangeMachiningEndpoints()
	{
		foreach( ExternalAxisType type in new[] { ExternalAxisType.None, ExternalAxisType.Single, ExternalAxisType.Dual } ) {
			foreach( double distance in new[] { 0.0, 5 } ) {
				Fixture f = new Fixture( type, false ); f.Entry.EntryDistance = distance; f.Entry.ExitDistance = distance;
				f.Add( 180, 0 ).CraftData.ProcessPath = false; f.Add( 37, 19 ); f.Add( 37, 19 ); f.Add( -23, 41 ); f.Add( 360, 0 ).CraftData.ProcessPath = false;
				CheckNC( f );
				string output = f.Export( Custom( "POSITION" ) );
				Assert( output.Contains( "N1;" ) && output.Contains( "N5;" ) && output.Contains( "// PATH 2" ) && output.Contains( "// PATH 4" ), "Original path numbering changed." );
				Assert( Lines( output ).Count( s => s == "// Exit" ) == f.Groups().Length, "Valid machining group Exit was skipped or duplicated." );
			}
		}
	}
	public static void VariableReplacementUsesInvariantDegrees()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); f.Add( 12.5, -23.75 ); CultureInfo old = CultureInfo.CurrentCulture;
		try { CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "de-DE" ); string output = f.Export( Custom( "VALUES {ExtAxisPos1} {ExtAxisPos2} {ExtAxisPos1}" ) ); Assert( output.Contains( "VALUES 12.5 -23.75 12.5" ), "External variables were not fully replaced in degree/invariant units." ); }
		finally { CultureInfo.CurrentCulture = old; }
	}
	public static void PointConversionClonesAndFlyCutUsesConvertedCoordinates()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); f.Add( 37, -53 ); f.Export( Custom( "FIXED" ) );
		PostPoint point = new PostPoint { X = 2, Y = 3, Z = 5, Master = .4, Slave = -.6, TiltAngle_deg = 17, IsMicroJointStart = true, IsMicroJointEnd = true, ExternalAxisPos = new ExternalAxisPos( 37, -53 ) };
		PostPoint converted = (PostPoint)Call( "MyCAM.Post.NCWriter", "TransformNCPoint", f.Writer, point ); gp_Pnt expected = Positioned( f, point );
		Assert( !object.ReferenceEquals( point, converted ) && point.X == 2 && point.Y == 3 && point.Z == 5, "NC conversion mutated source." );
		Assert( converted.Master == point.Master && converted.Slave == point.Slave && converted.TiltAngle_deg == 17 && converted.IsMicroJointStart && converted.IsMicroJointEnd && converted.ExternalAxisPos.Equals( point.ExternalAxisPos ), "Point metadata was lost." );
		string output = Capture( w => { f.Writer.GetType().GetField( "m_StreamWriter", Flags ).SetValue( f.Writer, w ); Call( "MyCAM.Post.NCWriter", "WriteOnePoint", f.Writer, point, null, null, false, false ); } );
		Assert( Lines( output ).Count( s => s.Contains( "P\"FlyCut\"" ) ) == 2, "Microjoint commands lost." );
		foreach( string line in Lines( output ) ) { Close( Number( line, "X" ), expected.X(), line ); Close( Number( line, "Y" ), expected.Y(), line ); Close( Number( line, "Z" ), expected.Z(), line ); }
	}
}
