using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using CommonData;
using MyCAM.Data;
using MyCAM.FileManager;
using MyCAM.Helper;
using MyCAM.PathCache;
using MyCAM.Post;
using OCC.gp;

public static class ExternalAxisProgrammingTests
{
	static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	static readonly Assembly App = typeof( CraftData ).Assembly;
	const string ActionType = "MyCAM.Editor.ExternalAxisAction";
	const string DialogType = "MyCAM.Editor.ExternalAxisDlg";
	static void Assert( bool condition, string message ) { if( !condition ) { throw new Exception( message ); } }
	static object New( string type, params object[] args ) { return Activator.CreateInstance( App.GetType( type, true ), Flags, null, args, null ); }
	static object Call( string type, string method, object target, params object[] args ) { return App.GetType( type, true ).GetMethods( Flags ).Single( m => m.Name == method && m.GetParameters().Length == args.Length ).Invoke( target, args ); }
	static object Get( object target, string name ) { return target.GetType().GetProperty( name, Flags ).GetValue( target, null ); }
	static void Set( object target, string name, object value ) { target.GetType().GetProperty( name, Flags ).SetValue( target, value, null ); }
	static object Field( object target, string name ) { return target.GetType().GetField( name, Flags ).GetValue( target ); }
	static void SetField( object target, string name, object value ) { target.GetType().GetField( name, Flags ).SetValue( target, value ); }
	static object[] Items( object value ) { return ( (IEnumerable)value ).Cast<object>().ToArray(); }
	static object Edit( double? first, double? second, string id ) { return New( "MyCAM.Editor.ExternalAxisEditData", first, second, id ); }
	static void Click( object dialog, string button ) { Call( DialogType, "m_btn" + button + "_Click", dialog, null, EventArgs.Empty ); }
	static TextBox Text( object dialog, string name ) { return (TextBox)Field( dialog, "m_txb" + name ); }
	static void Matrix( gp_Trsf a, gp_Trsf b )
	{
		for( int r = 1; r <= 3; r++ ) { for( int c = 1; c <= 4; c++ ) { Assert( Math.Abs( a.Value( r, c ) - b.Value( r, c ) ) < 1e-8, "Preview transform differs." ); } }
	}
	static void Reject( Action action )
	{
		try { action(); }
		catch( TargetInvocationException ex ) { Assert( ex.InnerException is ArgumentException, "Wrong failure: " + ex ); return; }
		throw new Exception( "Invalid input was accepted." );
	}

	sealed class Fixture
	{
		public object Manager;
		public FiveAxisMachineData Machine;
		public List<string> IDs;
		public Dictionary<string, IObject> Map;
		public Fixture( ExternalAxisType type, bool mix )
		{
			Machine = mix ? (FiveAxisMachineData)new MixTypeMachineData { ToolToMasterVec = new gp_Vec( 4, 5, 6 ), MCSToSlaveVec = new gp_Vec( 11, 13, 19 ) }
				: new SpindleTypeMachineData { ToolToSlaveVec = new gp_Vec( 4, 5, 6 ), SlaveToMasterVec = new gp_Vec( 7, 8, 9 ) };
			Machine.ToolDirection = ToolDirection.Z; Machine.MasterRotaryAxis = mix ? RotaryAxis.X : RotaryAxis.Z; Machine.SlaveRotaryAxis = mix ? RotaryAxis.Z : RotaryAxis.X;
			Machine.MasterTiltedVec_deg = new gp_XYZ(); Machine.SlaveTiltedVec_deg = new gp_XYZ(); Machine.ToolLength = 12;
			Machine.SimulationOffset = new gp_Vec( 17, -23, 31 ); Machine.ExternalAxes.Type = type;
			Machine.ExternalAxes.Ext1RotaryAxis = RotaryAxis.Z; Machine.ExternalAxes.Ext2RotaryAxis = RotaryAxis.X;
			Machine.ExternalAxes.MCSToExt1Vec = new gp_Vec( 10, 20, 30 ); Machine.ExternalAxes.Ext1ToExt2Vec = new gp_Vec( 5, 7, 11 );
			Machine.ExternalAxes.Ext1TiltedVec_deg = new gp_XYZ( 2, 3, 4 ); Machine.ExternalAxes.Ext2TiltedVec_deg = new gp_XYZ( 5, 6, 7 );
			Machine.InitRotateAxes( RotaryDirection.RightHand, 0, 0, RotaryDirection.LeftHand, 0, 0 );
			Manager = New( "MyCAM.Data.DataManager" ); Set( Manager, "MachineData", Machine ); Call( "MyCAM.Data.DataGettingHelper", "Initialize", null, Manager );
			IDs = (List<string>)Get( Manager, "PathIDList" ); Map = (Dictionary<string, IObject>)Get( Manager, "ObjectMap" );
			EntryAndExitData entry = (EntryAndExitData)Get( Manager, "EntryAndExitData" ); entry.EntryDistance = 5; entry.ExitDistance = 7;
		}
		public ContourPathObject Add( double first, double second )
		{
			string id = "Path" + IDs.Count; double x = 10 + IDs.Count * 20;
			CraftData craft = new CraftData { ExternalAxisPos = new ExternalAxisPos( first, second ) };
			craft.TraverseData.LiftUpDistance = 3; craft.TraverseData.CutDownDistance = 4;
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
		public object Action( params string[] selection ) { return New( ActionType, Manager, new List<string>( selection.Length == 0 ? IDs : selection.AsEnumerable() ) ); }
		public PathObject Pattern( int kind )
		{
			gp_Ax1 reference = new gp_Ax1( new gp_Pnt( 90, 0, 0 ), new gp_Dir( 0, 0, 1 ) );
			gp_Trsf transform = new gp_Trsf(); transform.SetTranslation( new gp_Vec( 90, 0, 0 ) );
			IGeomData geometry; List<CADPoint> points; string objectType;
			if( kind == 1 ) {
				geometry = new RectangleGeomData( reference, 20, 30, 2, 0, false ); objectType = "Rectangle";
				points = (List<CADPoint>)Call( "MyCAM.Helper.StdPatternDiscreteFactory", "DiscretizeRectangle", null, 20.0, 30.0, 2.0, transform );
			} else if( kind == 2 ) {
				geometry = new RunwayGeomData( reference, 30, 20, 0, false ); objectType = "Runway";
				points = (List<CADPoint>)Call( "MyCAM.Helper.StdPatternDiscreteFactory", "DiscretizeRunway", null, 30.0, 20.0, transform );
			} else {
				geometry = new PolygonGeomData( reference, kind, 20, 2, 0, false ); objectType = "Polygon";
				points = (List<CADPoint>)Call( "MyCAM.Helper.StdPatternDiscreteFactory", "DiscretizePolygon", null, kind, 20.0, 2.0, transform );
			}
			string id = "Pattern" + IDs.Count;
			ContourPathObject source = new ContourPathObject( id + "Source", null, new ContourGeomData( points, new Dictionary<CADPoint, CADPoint>(), true ), new CraftData() );
			PathObject path = (PathObject)New( "MyCAM.Data." + objectType + "PathObject", id, geometry, new CraftData { LeadData = new LeadData( 2 ) }, source );
			IDs.Add( id ); Map.Add( id, path ); return path;
		}
		public void Optimize() { ExternalAxisEditorTestHelper.Optimize( Manager ); }
		public object[] Groups() { return Items( Call( "MyCAM.Helper.ProcessPathGroupHelper", "GetProcessPathGroups", null, IDs, Machine ) ); }
	}

	public static void FiniteAnglesCultureAndFullTurns()
	{
		CultureInfo old = Thread.CurrentThread.CurrentCulture;
		try {
			foreach( string culture in new[] { "en-US", "de-DE", "fr-FR", "zh-TW" } ) {
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo( culture );
				foreach( double value in new[] { -1080.25, 0, 360, 123456789.123456, 1e100 } ) {
					object[] args = { value.ToString( "G17", CultureInfo.CurrentCulture ), null };
					Assert( (bool)Call( DialogType, "TryParseAngle", null, args ) && (double)args[1] == value, "Finite angle changed in " + culture );
				}
				foreach( string text in new[] { "NaN", "Infinity", "-Infinity", "1e9999", "bad" } ) {
					object[] args = { text, null }; Assert( !(bool)Call( DialogType, "TryParseAngle", null, args ), "Invalid angle accepted." );
				}
				object[] blank = { "  ", null }; Assert( (bool)Call( DialogType, "TryParseAngle", null, blank ) && blank[1] == null, "Blank must preserve values." );
			}
		}
		finally { Thread.CurrentThread.CurrentCulture = old; }
	}

	public static void EditDataRejectsNonFiniteBeforeMutation()
	{
		foreach( double value in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity } ) {
			Reject( delegate { Edit( value, 0, "Path0" ); } ); Reject( delegate { Edit( 0, value, "Path0" ); } );
		}
	}

	static object Dialog( ExternalAxisType type, params ExternalAxisPos[] positions )
	{
		List<string> ids = Enumerable.Range( 0, positions.Length ).Select( i => "Path" + i ).ToList();
		return New( DialogType, type, ids, ids, positions.ToList() );
	}
	public static void DialogSingleDualAndMixedValues()
	{
		foreach( ExternalAxisType type in new[] { ExternalAxisType.Single, ExternalAxisType.Dual } ) {
			using( Form dialog = (Form)Dialog( type, new ExternalAxisPos( 0, 9 ), new ExternalAxisPos( 360, 9 ) ) ) {
				dialog.Show();
				Assert( Text( dialog, "Ext2" ).Visible == ( type == ExternalAxisType.Dual ), "Wrong effective axis UI." );
				Assert( Text( dialog, "Ext1" ).Text == "", "Mixed 0/360 must not become one value." );
				Assert( Text( dialog, "Ext2" ).Text == "9", "Common Ext2 value missing." );
				Assert( ( (ComboBox)Field( dialog, "m_cbxPreviewPath" ) ).Items.Count == 2, "Preview path selection missing." );
			}
		}
	}
	public static void DialogInvalidInputDoesNotPreviewOrConfirm()
	{
		object dialog = Dialog( ExternalAxisType.Dual, new ExternalAxisPos( 12, 34 ) );
		try {
			foreach( string axis in new[] { "Ext1", "Ext2" } ) {
				Text( dialog, axis ).Text = "NaN";
				Click( dialog, "Preview" ); Click( dialog, "Confirm" );
				Assert( !(bool)Field( dialog, "m_ConfirmCheck" ), "Invalid input confirmed." );
				Assert( ( (Label)Field( dialog, "m_lblError" ) ).Text.Length > 0, "No validation feedback." );
				Text( dialog, axis ).Text = "12";
			}
		}
		finally { ( (Form)dialog ).Dispose(); }
	}

	public static void SelectionCopiedAndUnselectedPathsUntouched()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 1, 2 ), b = f.Add( 3, 4 );
		List<string> selection = new List<string> { a.UID }; object action = New( ActionType, f.Manager, selection ); selection.Clear();
		Call( ActionType, "ApplyPositions", action, Edit( 360, -70, a.UID ) );
		Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 360, -70 ) ), "Copied selection was lost." );
		Assert( b.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 3, 4 ) ), "Unselected path changed." );
		Call( ActionType, "End", action );
		Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 1, 2 ) ), "Original position not restored." );
	}
	public static void SingleAxisRetainsInactiveAssignmentAndIgnoresLimits()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false ); ContourPathObject a = f.Add( 1, 83 ), b = f.Add( 2, -91 );
		object action = f.Action(); Call( ActionType, "ApplyPositions", action, Edit( 1080.25, 999, a.UID ) );
		Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 1080.25, 83 ) ) && b.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 1080.25, -91 ) ), "Inactive axis erased or finite angle limited." );
		Assert( f.Groups().Length == 1, "Inactive Ext2 split group." ); Call( ActionType, "End", action );
	}
	public static void BlankAfterPreviewRestoresIndividualOriginals()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 1, 2 ), b = f.Add( 3, 4 ); object action = f.Action();
		Call( ActionType, "ApplyPositions", action, Edit( 80, 90, a.UID ) );
		Call( ActionType, "ApplyPositions", action, Edit( null, 70, a.UID ) );
		Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 1, 70 ) ) && b.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 3, 70 ) ), "Blank retained previous preview instead of originals." );
		Call( ActionType, "End", action );
	}
	public static void CancelAndRepeatedEndPreserveSettingsAndSeeds()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 10, 20 ); ContourPathObject b = f.Add( 30, 40 );
		a.CraftData.InitMaster_rad = 0.21; a.CraftData.InitSlave_rad = -0.32;
		a.CraftData.SetSpecialToolVecData( new SpecialToolVecData( 11, -22, EMachiningDir.None, true, true ) );
		object special = a.CraftData.SpecialToolVecData, modifications = a.CraftData.ToolVecModifyMap;
		int optimized = 0, ended = 0; object action = f.Action();
		SetField( action, "NeedOptimizedIK", (Action)delegate { optimized++; } );
		SetField( action, "PropertyChanged", (Action<List<string>>)delegate { ended++; } );
		Call( ActionType, "ApplyPositions", action, Edit( 88, -77, a.UID ) );
		Call( ActionType, "End", action ); Call( ActionType, "End", action );
		Assert( optimized == 0 && ended == 1, "Cancel optimized seeds or ended twice." );
		Assert( a.CraftData.InitMaster_rad == 0.21 && a.CraftData.InitSlave_rad == -0.32, "Cancel changed IK seeds." );
		Assert( ReferenceEquals( special, a.CraftData.SpecialToolVecData ) && ReferenceEquals( modifications, a.CraftData.ToolVecModifyMap ), "Existing tool settings replaced." );
		Assert( a.CraftData.SpecialToolVecData.Mode == ToolVecMode.AssignMS && a.CraftData.SpecialToolVecData.MasterAngle_deg == 11 && a.CraftData.SpecialToolVecData.SlaveAngle_deg == -22, "Lock mode changed." );
		Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 10, 20 ) ) && b.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 30, 40 ) ), "Cancel lost positions." );
	}
	public static void ActualDialogCancelAndWindowCloseRestore()
	{
		foreach( bool closeWindow in new[] { false, true } ) {
			Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 12, 34 ); object action = f.Action();
			Call( ActionType, "Start", action ); object dialog = Field( action, "m_Dlg" );
			try {
				Text( dialog, "Ext1" ).Text = "720"; Text( dialog, "Ext2" ).Text = "-360";
				Call( ActionType, "ApplyPositions", action, Edit( 720, -360, a.UID ) );
				if( closeWindow ) { ( (Form)dialog ).Close(); } else { Click( dialog, "Cancel" ); }
				Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 12, 34 ) ), "User close did not restore original values." );
				Assert( Field( action, "m_Dlg" ) == null && ( (Form)dialog ).IsDisposed, "Dialog or handlers retained." );
			}
			finally { Call( ActionType, "End", action ); }
		}
	}
	public static void ActualDialogConfirmOptimizesOnceAndKeepsModes()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 10, 20 ); f.Add( 30, 40 );
		a.CraftData.SetSpecialToolVecData( new SpecialToolVecData( 11, -22, EMachiningDir.None, true, true ) ); object special = a.CraftData.SpecialToolVecData;
		int optimized = 0; object action = f.Action(); SetField( action, "NeedOptimizedIK", (Action)delegate { optimized++; f.Optimize(); } );
		Call( ActionType, "Start", action ); object dialog = Field( action, "m_Dlg" );
		try {
			Text( dialog, "Ext1" ).Text = "360"; Text( dialog, "Ext2" ).Text = "-45"; Click( dialog, "Confirm" );
			Call( ActionType, "End", action );
			Assert( optimized == 1, "Confirm must optimize once." );
			foreach( string id in f.IDs ) { Assert( ( (PathObject)f.Map[id] ).CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 360, -45 ) ), "Confirmed positions lost." ); }
			Assert( ReferenceEquals( special, a.CraftData.SpecialToolVecData ) && a.CraftData.SpecialToolVecData.Mode == ToolVecMode.AssignMS, "Confirm cleared lock mode." );
			Assert( a.CraftData.InitMaster_rad == 0 && a.CraftData.InitSlave_rad == 0, "Group-first IK seeds not reset." );
		}
		finally { Call( ActionType, "End", action ); }
	}
	public static void NoExternalMachineAndForeignPreviewRejected()
	{
		Fixture none = new Fixture( ExternalAxisType.None, false ); none.Add( 0, 0 ); Reject( delegate { none.Action(); } );
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 12, 34 ); object action = f.Action();
		Reject( delegate { Call( ActionType, "ApplyPositions", action, Edit( 1, 2, "Missing" ) ); } );
		Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 12, 34 ) ), "Foreign preview partially changed data." ); Call( ActionType, "End", action );
	}
	public static void OptimizationRunsAfterActionCompletion()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 12, 34 ); object action = f.Action();
		int displayUpdates = 0, optimized = 0;
		SetField( action, "PropertyChanged", (Action<List<string>>)delegate { displayUpdates++; } );
		SetField( action, "NeedOptimizedIK", (Action)delegate {
			Assert( (bool)Field( action, "m_IsConfirmed" ) && (bool)Field( action, "m_IsEnded" ), "Optimization ran before the edit completed." );
			Assert( Field( action, "m_Dlg" ) == null && displayUpdates == 1, "Optimization ran before dialog and display cleanup." );
			optimized++; f.Optimize();
		} );
		Call( ActionType, "Start", action ); object dialog = Field( action, "m_Dlg" );
		try {
			Text( dialog, "Ext1" ).Text = "360"; Text( dialog, "Ext2" ).Text = "-45"; Click( dialog, "Confirm" );
			Assert( optimized == 1, "Optimization was not invoked exactly once after completion." );
		}
		finally { Call( ActionType, "End", action ); }
	}
	public static void OptimizationFailureDoesNotUndoCompletedEdit()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 12, 34 ); object action = f.Action();
		InvalidOperationException failure = new InvalidOperationException( "Controlled optimization failure." );
		SetField( action, "NeedOptimizedIK", (Action)delegate { throw failure; } );
		Call( ActionType, "Start", action ); object dialog = Field( action, "m_Dlg" ); bool observed = false;
		try {
			Text( dialog, "Ext1" ).Text = "360"; Text( dialog, "Ext2" ).Text = "-45";
			try { Click( dialog, "Confirm" ); }
			catch( TargetInvocationException ex ) { observed = ReferenceEquals( ex.GetBaseException(), failure ); }
			Assert( observed, "Optimization failure was silently swallowed." );
			Assert( (bool)Field( action, "m_IsConfirmed" ) && (bool)Field( action, "m_IsEnded" ), "Optimization failure left the edit active." );
			Assert( Field( action, "m_Dlg" ) == null && ( (Form)dialog ).IsDisposed, "Completed dialog remained open after optimization failure." );
			Call( ActionType, "End", action );
			Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 360, -45 ) ), "Separate optimization failure undid the completed edit." );
		}
		finally { Call( ActionType, "End", action ); }
	}

	static void PreviewCase( ExternalAxisType type, bool mix, bool circle )
	{
		Fixture f = new Fixture( type, mix ); ContourPathObject a = f.Add( 0, 17 ); a.CraftData.LeadData = new LeadData( 2, 2, 90, 2, 2, 90 );
		PathObject b = circle ? f.Circle( a, 0 ) : f.Add( 0, 19 ); object action = f.Action();
		int optimized = 0; SetField( action, "NeedOptimizedIK", (Action)delegate { optimized++; } );
		Dictionary<MachineComponentType, List<gp_Trsf>> actual = null;
		SetField( action, "RaiseTrans", (Action<Dictionary<MachineComponentType, List<gp_Trsf>>, bool>)delegate( Dictionary<MachineComponentType, List<gp_Trsf>> frames, bool unlock ) { Assert( unlock, "Viewer update left locked." ); actual = frames; } );
		Call( ActionType, "Start", action ); object dialog = Field( action, "m_Dlg" );
		try {
			Text( dialog, "Ext1" ).Text = "73"; Text( dialog, "Ext2" ).Text = "-29";
			( (ComboBox)Field( dialog, "m_cbxPreviewPath" ) ).SelectedIndex = 1; Click( dialog, "Preview" );
			Assert( optimized == 0, "Preview invoked global optimization." );
			Assert( actual != null && actual.Values.All( list => list.Count == 1 ), "Preview is not exactly one frame." );
			CAMPoint point = CacheHelper.GetProcessStartPoint( b.UID ); Dictionary<MachineComponentType, List<gp_Trsf>> expected;
			Assert( new FiveAxisSimuPathBuilder().BuildSinglePointFrameTransform( f.Machine, point.Point,
				new PostPoint { Master = point.ModMaster_rad, Slave = point.ModSlave_rad, ExternalAxisPos = point.ExternalAxisPos }, out expected ), "Reference preview failed." );
			Assert( point.ExternalAxisPos.Ext1 == 73, "Cache was not rebuilt for external assignment." );
			foreach( var pair in expected ) { Matrix( actual[pair.Key][0], pair.Value[0] ); }
		}
		finally { Call( ActionType, "End", action ); }
	}
	public static void SpindleSinglePreview() { PreviewCase( ExternalAxisType.Single, false, false ); }
	public static void SpindleDualPreview() { PreviewCase( ExternalAxisType.Dual, false, false ); }
	public static void MixSinglePreview() { PreviewCase( ExternalAxisType.Single, true, false ); }
	public static void StandardPatternPreview() { PreviewCase( ExternalAxisType.Dual, false, true ); }

	static void Integrated( ExternalAxisType type, bool mix, bool mixed, bool zero, int pattern = 0 )
	{
		Fixture f = new Fixture( type, mix ); ContourPathObject a = f.Add( 0, 0 ); f.Add( 0, 0 );
		if( pattern != 0 ) { f.Pattern( pattern ); } else if( mixed ) { f.Circle( a, 0 ); }
		object action = f.Action(); SetField( action, "NeedOptimizedIK", (Action)f.Optimize );
		Call( ActionType, "ConfirmPositions", action, Edit( zero ? 0 : 73, zero ? 0 : -29, a.UID ) );
		Assert( f.Groups().Length == 1, "Single assignment did not produce one group." );
		object writer = New( "MyCAM.Post.NCWriter", f.Manager ); object[] postArgs = { null, null, null };
		Assert( (bool)Call( "MyCAM.Post.NCWriter", "ConvertPostDataListSuccess", writer, postArgs ), "Post conversion failed: " + postArgs[0] );
		List<PostData> flat = (List<PostData>)postArgs[1]; object groups = postArgs[2];
		object input = New( "MyCAM.Data.SimuData+SimuRequiredData+SimuInputSet" );
		foreach( var pair in new Dictionary<string, object> { { "EachPathIKPostDataList", flat }, { "ProcessGroups", groups },
			{ "LastPathLastPnt", Get( Items( groups ).Last(), "ProcessEndPoint" ) }, { "EntryAndExitData", Get( f.Manager, "EntryAndExitData" ) },
			{ "PostSolver", New( "MyCAM.Post.PostSolver", f.Machine ) }, { "MachineData", f.Machine } } ) { SetField( input, pair.Key, pair.Value ); }
		object[] frameArgs = { input, null, null, 0 };
		Assert( (bool)Call( "MyCAM.Helper.FiveAxisSimuPathBuilder", "BuildFrameTransMap", new FiveAxisSimuPathBuilder(), frameArgs ) && (int)frameArgs[3] > 0, "Programmed paths did not simulate." );
		FieldInfo postField = App.GetType( "MyCAM.App.MyApp", true ).GetField( "m_CustPost", Flags ); object old = postField.GetValue( null ); string file = Path.GetTempFileName();
		try {
			postField.SetValue( null, new CustPost( new Dictionary<string, string> { { "Header", "// FILE" }, { "Tail", "// END" }, { "TraverseHeader", "// PATH {PathIndex}" },
				{ "PathHeader", "// CUT" }, { "PathTail", "// CUT END" }, { "ExternalAxisHeader", "// EXT {ExtAxisPos1} {ExtAxisPos2}" } } ) );
			object[] args = { "Programming", file, null }; Assert( (bool)Call( "MyCAM.Post.NCWriter", "ConvertSuccess", writer, args ), "Programmed NC failed: " + args[2] );
			string nc = File.ReadAllText( file ); Assert( nc.Split( new[] { "// EXT " }, StringSplitOptions.None ).Length == 2, "Wrong programmed group header count." );
			Assert( !nc.Contains( "{ExtAxisPos" ), "External variables unresolved." );
			Assert( nc.Contains( zero ? "// EXT 0 0" : type == ExternalAxisType.Dual ? "// EXT 73 -29" : "// EXT 73 0" ), "Wrong effective angles." );
			Assert( !mixed || nc.Contains( "SY_CIRC" ), "Standard macro contract changed." );
			if( pattern != 0 ) {
				string macro = pattern == 1 ? "SY_RECT" : pattern == 2 ? "SY_RUNWAY" : "SY_POLYGON";
				Assert( nc.Contains( macro ), "Missing standard pattern macro: " + macro );
				Assert( flat.Count == 3 && flat.All( data => data.MainPathPostPointList.Count > 0 ), "Mixed pattern simulation lost a machining path." );
			}
		}
		finally { postField.SetValue( null, old ); File.Delete( file ); }
	}
	public static void SpindleSingleAllStandardPatternContracts()
	{
		for( int pattern = 1; pattern <= 6; pattern++ ) { Integrated( ExternalAxisType.Single, false, false, false, pattern ); }
	}
	public static void SpindleDualAllStandardPatternContracts()
	{
		for( int pattern = 1; pattern <= 6; pattern++ ) { Integrated( ExternalAxisType.Dual, false, false, false, pattern ); }
	}
	public static void MixSingleAllStandardPatternContracts()
	{
		for( int pattern = 1; pattern <= 6; pattern++ ) { Integrated( ExternalAxisType.Single, true, false, false, pattern ); }
	}
	public static void SpindleSingleProgrammingEndToEnd() { Integrated( ExternalAxisType.Single, false, false, false ); }
	public static void SpindleDualProgrammingEndToEnd() { Integrated( ExternalAxisType.Dual, false, false, false ); }
	public static void MixSingleProgrammingEndToEnd() { Integrated( ExternalAxisType.Single, true, false, false ); }
	public static void MixedContourStandardEndToEnd() { Integrated( ExternalAxisType.Dual, false, true, false ); }
	public static void AllZeroProgrammingEndToEnd() { Integrated( ExternalAxisType.Single, false, false, true ); }
	public static void ProgrammingFullTurnsAndGlobalBoundaryOptimization()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false ); ContourPathObject a = f.Add( 0, 0 ), b = f.Add( 0, 0 ), c = f.Add( 0, 0 );
		object action = f.Action( b.UID ); SetField( action, "NeedOptimizedIK", (Action)f.Optimize );
		Call( ActionType, "ConfirmPositions", action, Edit( 360, null, b.UID ) );
		Assert( f.Groups().Length == 3, "0/360/0 merged or reordered." );
		foreach( var path in new[] { a, b, c } ) { Assert( path.CraftData.InitMaster_rad == 0 && path.CraftData.InitSlave_rad == 0, "New group boundary did not reset global seeds." ); }
	}
	public static void ProgrammedProjectRoundTrip()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 0, 0 ); f.Circle( a, 0 );
		PartObject part = new PartObject( "Part0", new OCC.BRepPrimAPI.BRepPrimAPI_MakeBox( 10, 20, 30 ).Shape() );
		f.Map.Add( part.UID, part ); ( (List<string>)Get( f.Manager, "PartIDList" ) ).Add( part.UID );
		object action = f.Action(); SetField( action, "NeedOptimizedIK", (Action)f.Optimize ); Call( ActionType, "ConfirmPositions", action, Edit( 360, -29, a.UID ) );
		DataManagerDTO dto = (DataManagerDTO)New( "MyCAM.FileManager.DataManagerDTO", f.Manager ); XmlSerializer serializer = new XmlSerializer( typeof( DataManagerDTO ) );
		DataManagerDTO restored;
		using( StringWriter writer = new StringWriter() ) { serializer.Serialize( writer, dto ); restored = (DataManagerDTO)serializer.Deserialize( new StringReader( writer.ToString() ) ); }
		object[] args = new object[8]; Call( "MyCAM.FileManager.DataManagerDTO", "DataMgrDTO2Data", restored, args );
		Dictionary<string, IObject> map = (Dictionary<string, IObject>)args[0]; Assert( ( (List<string>)args[2] ).SequenceEqual( f.IDs ), "Project path ordering changed." );
		foreach( string id in f.IDs ) { Assert( ( (PathObject)map[id] ).CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 360, -29 ) ), "Programmed project lost external positions." ); }
	}
}
