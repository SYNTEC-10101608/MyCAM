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
	static object Edit( double? first, double? second ) { return New( "MyCAM.Editor.ExternalAxisEditData", first, second ); }
	static void Click( object dialog, string button ) { Call( DialogType, "m_btn" + button + "_Click", dialog, null, EventArgs.Empty ); }
	static NumericUpDown Number( object dialog, string name ) { return (NumericUpDown)Field( dialog, "m_dbtn" + name ); }
	static void Input( object dialog, string name, string text ) { Number( dialog, name ).Controls.OfType<TextBox>().Single().Text = text; }
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
		public object Action( params string[] selection ) { return ExternalAxisEditorTestHelper.CreateAction( Manager, new List<string>( selection.Length == 0 ? IDs : selection.AsEnumerable() ) ); }
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
				foreach( double value in new[] { -99999, -1080.25, 0, 360, 12345.123, 99999 } ) {
					object[] args = { value.ToString( "G17", CultureInfo.CurrentCulture ), null };
					Assert( (bool)Call( DialogType, "TryParseAngle", null, args ) && (double)args[1] == value, "Finite angle changed in " + culture );
				}
				foreach( string text in new[] { "NaN", "Infinity", "-Infinity", "1e9999", "bad", "-100000", "100000", "" } ) {
					object[] args = { text, null }; Assert( !(bool)Call( DialogType, "TryParseAngle", null, args ), "Invalid angle accepted." );
				}
				object[] blank = { "  ", null }; Assert( !(bool)Call( DialogType, "TryParseAngle", null, blank ), "Blank input must not assign an angle." );
			}
		}
		finally { Thread.CurrentThread.CurrentCulture = old; }
	}

	public static void EditDataRejectsNonFiniteBeforeMutation()
	{
		foreach( double value in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity } ) {
			Reject( delegate { Edit( value, 0 ); } ); Reject( delegate { Edit( 0, value ); } );
		}
	}

	static object Dialog( ExternalAxisType type, params ExternalAxisPos[] positions )
	{
		return New( DialogType, type, positions.ToList() );
	}
	public static void DialogSingleDualAndMixedValues()
	{
		foreach( ExternalAxisType type in new[] { ExternalAxisType.Single, ExternalAxisType.Dual } ) {
			using( Form dialog = (Form)Dialog( type, new ExternalAxisPos( 0, 9 ), new ExternalAxisPos( 360, 9 ) ) ) {
				dialog.Show();
				Assert( Number( dialog, "Ext2" ).Visible == ( type == ExternalAxisType.Dual ), "Wrong effective axis UI." );
				Assert( Number( dialog, "Ext1" ).Value == 0, "Mixed values must be displayed as 0." );
				Assert( Number( dialog, "Ext2" ).Value == 9, "Common Ext2 value missing." );
				Assert( Number( dialog, "Ext1" ).Minimum == -99999 && Number( dialog, "Ext1" ).Maximum == 99999 && Number( dialog, "Ext1" ).DecimalPlaces == 3, "Numeric input differs from LockAB." );
				Assert( dialog.AcceptButton == null && dialog.Controls.OfType<NumericUpDown>().Count() == 2, "Enter must not confirm and angle fields must be NumericUpDown." );
				Assert( !dialog.Controls.OfType<ComboBox>().Any(), "Preview must not require choosing a path." );
				Assert( dialog.Controls.OfType<Button>().Count() == 2, "Only confirm and cancel buttons are needed." );
			}
		}
	}
	public static void DialogInvalidInputDoesNotPreviewOrConfirm()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 12, 34 ); object action = f.Action();
		int previews = 0;
		SetField( action, "RaiseTrans", (Action<Dictionary<MachineComponentType, List<gp_Trsf>>, bool>)delegate { previews++; } );
		Call( ActionType, "Start", action ); object dialog = Field( action, "m_Dlg" );
		try {
			foreach( string axis in new[] { "Ext1", "Ext2" } ) {
				foreach( string invalid in new[] { "-", "NaN", "Infinity", "bad", "", "100000", "-100000" } ) {
					int before = previews; ExternalAxisPos position = a.CraftData.ExternalAxisPos;
					Input( dialog, axis, invalid );
					Call( DialogType, "ProcessDialogKey", dialog, Keys.Enter ); Click( dialog, "Confirm" );
					Assert( previews == before && a.CraftData.ExternalAxisPos.Equals( position ), "Invalid input changed the preview or data: " + axis + " '" + invalid + "', previews " + before + " -> " + previews + ", text '" + Number( dialog, axis ).Text + "'." );
					Assert( !(bool)Field( dialog, "m_ConfirmCheck" ), "Invalid input confirmed." );
					Assert( ( (Label)Field( dialog, "m_lblError" ) ).Text.Length > 0, "No validation feedback." );
				}
				Input( dialog, axis, "12" );
			}
		}
		finally { Call( ActionType, "End", action ); }
	}

	public static void SelectionCopiedAndUnselectedPathsUntouched()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 1, 2 ), b = f.Add( 3, 4 );
		List<string> selection = new List<string> { a.UID }; object action = ExternalAxisEditorTestHelper.CreateAction( f.Manager, selection ); selection.Clear();
		Call( ActionType, "ApplyPositions", action, Edit( 360, -70 ) );
		Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 360, -70 ) ), "Copied selection was lost." );
		Assert( b.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 3, 4 ) ), "Unselected path changed." );
		Call( ActionType, "End", action );
		Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 360, -70 ) ), "Ending the action undid an applied assignment." );
	}
	public static void SingleAxisRetainsInactiveAssignmentAndIgnoresLimits()
	{
		Fixture f = new Fixture( ExternalAxisType.Single, false ); ContourPathObject a = f.Add( 1, 83 ), b = f.Add( 2, -91 );
		object action = f.Action(); Call( ActionType, "ApplyPositions", action, Edit( 1080.25, 999 ) );
		Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 1080.25, 83 ) ) && b.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 1080.25, -91 ) ), "Inactive axis erased or finite angle limited." );
		Assert( f.Groups().Length == 1, "Inactive Ext2 split group." ); Call( ActionType, "End", action );
	}
	public static void UneditedAxesPreserveCurrentAssignments()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 1, 2 ), b = f.Add( 3, 4 ); object action = f.Action();
		Call( ActionType, "ApplyPositions", action, Edit( 80, 90 ) );
		Call( ActionType, "ApplyPositions", action, Edit( null, 70 ) );
		Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 80, 70 ) ) && b.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 80, 70 ) ), "Unedited axis restored an old value." );
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
		Call( ActionType, "ApplyPositions", action, Edit( 88, -77 ) );
		Call( ActionType, "End", action ); Call( ActionType, "End", action );
		Assert( optimized == 1 && ended == 1, "End must clean up and optimize exactly once." );
		Assert( a.CraftData.InitMaster_rad == 0.21 && a.CraftData.InitSlave_rad == -0.32, "Cancel changed IK seeds." );
		Assert( ReferenceEquals( special, a.CraftData.SpecialToolVecData ) && ReferenceEquals( modifications, a.CraftData.ToolVecModifyMap ), "Existing tool settings replaced." );
		Assert( a.CraftData.SpecialToolVecData.Mode == ToolVecMode.AssignMS && a.CraftData.SpecialToolVecData.MasterAngle_deg == 11 && a.CraftData.SpecialToolVecData.SlaveAngle_deg == -22, "Lock mode changed." );
		Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 88, -77 ) ) && b.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 88, -77 ) ), "End undid applied assignments." );
	}
	public static void ActualDialogCancelAndWindowCloseKeepAssignments()
	{
		foreach( bool closeWindow in new[] { false, true } ) {
			Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 12, 34 ); object action = f.Action();
			Call( ActionType, "Start", action ); object dialog = Field( action, "m_Dlg" );
			try {
				Number( dialog, "Ext1" ).Value = 720; Number( dialog, "Ext2" ).Value = -360;
				Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 720, -360 ) ), "Typing did not immediately apply the preview." );
				if( closeWindow ) { ( (Form)dialog ).Close(); } else { Click( dialog, "Cancel" ); }
				Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 720, -360 ) ), "User close undid applied assignments." );
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
			Number( dialog, "Ext1" ).Value = 360; Number( dialog, "Ext2" ).Value = -45; Click( dialog, "Confirm" );
			Call( ActionType, "End", action );
			Assert( optimized == 1, "Confirm must optimize once." );
			foreach( string id in f.IDs ) { Assert( ( (PathObject)f.Map[id] ).CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 360, -45 ) ), "Confirmed positions lost." ); }
			Assert( ReferenceEquals( special, a.CraftData.SpecialToolVecData ) && a.CraftData.SpecialToolVecData.Mode == ToolVecMode.AssignMS, "Confirm cleared lock mode." );
			Assert( a.CraftData.InitMaster_rad == 0 && a.CraftData.InitSlave_rad == 0, "Group-first IK seeds not reset." );
		}
		finally { Call( ActionType, "End", action ); }
	}
	public static void NoExternalMachineAndNullEditRejected()
	{
		Fixture none = new Fixture( ExternalAxisType.None, false ); none.Add( 0, 0 ); Reject( delegate { none.Action(); } );
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 12, 34 ); object action = f.Action();
		Reject( delegate { Call( ActionType, "ApplyPositions", action, new object[] { null } ); } );
		Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 12, 34 ) ), "Null edit partially changed data." ); Call( ActionType, "End", action );
	}
	public static void OptimizationRunsAfterActionCompletion()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 12, 34 ); object action = f.Action();
		int displayUpdates = 0, optimized = 0;
		SetField( action, "PropertyChanged", (Action<List<string>>)delegate { displayUpdates++; } );
		SetField( action, "NeedOptimizedIK", (Action)delegate {
			Assert( (bool)Field( action, "m_IsEnded" ) && !(bool)Field( action, "m_IsSelectionStarted" ), "Optimization ran before the edit completed." );
			Assert( Field( action, "m_Dlg" ) == null && displayUpdates == 1, "Optimization ran before dialog and display cleanup." );
			optimized++; f.Optimize();
		} );
		Call( ActionType, "Start", action ); object dialog = Field( action, "m_Dlg" );
		try {
			Number( dialog, "Ext1" ).Value = 360; Number( dialog, "Ext2" ).Value = -45; Click( dialog, "Confirm" );
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
			Number( dialog, "Ext1" ).Value = 360; Number( dialog, "Ext2" ).Value = -45;
			try { Click( dialog, "Confirm" ); }
			catch( TargetInvocationException ex ) { observed = ReferenceEquals( ex.GetBaseException(), failure ); }
			Assert( observed, "Optimization failure was silently swallowed." );
			Assert( (bool)Field( action, "m_IsEnded" ), "Optimization failure left the edit active." );
			Assert( Field( action, "m_Dlg" ) == null && ( (Form)dialog ).IsDisposed, "Completed dialog remained open after optimization failure." );
			Call( ActionType, "End", action );
			Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 360, -45 ) ), "Separate optimization failure undid the completed edit." );
		}
		finally { Call( ActionType, "End", action ); }
	}

	public static void NumericPreviewDoesNotResetCanvasBeforeTransform()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); f.Add( 12, 34 ); object action = f.Action();
		int cleanups = 0, previews = 0;
		SetField( action, "PropertyChanged", (Action<List<string>>)delegate { cleanups++; } );
		SetField( action, "RaiseTrans", (Action<Dictionary<MachineComponentType, List<gp_Trsf>>, bool>)delegate { previews++; } );
		Call( ActionType, "Start", action ); object dialog = Field( action, "m_Dlg" );
		try {
			Number( dialog, "Ext1" ).Value = 73;
			Number( dialog, "Ext2" ).Value = -29;
			Call( DialogType, "ProcessDialogKey", dialog, Keys.Enter );
			Assert( previews == 4 && cleanups == 0, "Valid previews reset the canvas before applying the posture." );
			ExternalAxisEditorTestHelper.ChangeSelection( action );
			Assert( cleanups == 1 && previews == 4, "Clearing selection must clean up the previous posture without a new preview." );
			ExternalAxisEditorTestHelper.ChangeSelection( action, f.IDs[0] );
			Assert( cleanups == 2 && previews == 5, "Selecting a path performed a cleanup before its preview." );
			Call( ActionType, "End", action );
			Assert( cleanups == 3, "End must clean up the preview once." );
		}
		finally { Call( ActionType, "End", action ); }
	}

	public static void EnterPreviewsWithoutConfirmingOrClosing()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 12, 34 ); object action = f.Action();
		int previews = 0; SetField( action, "RaiseTrans", (Action<Dictionary<MachineComponentType, List<gp_Trsf>>, bool>)delegate { previews++; } );
		Call( ActionType, "Start", action ); object dialog = Field( action, "m_Dlg" );
		try {
			Input( dialog, "Ext1", "73.125" );
			Assert( (bool)Call( DialogType, "ProcessDialogKey", dialog, Keys.Enter ), "Enter was not consumed." );
			Assert( previews == 2 && a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 73.125, 34 ) ), "Enter did not apply and preview the typed value." );
			KeyEventArgs key = new KeyEventArgs( Keys.Enter ); Call( DialogType, "OnAngleKeyDown", dialog, Number( dialog, "Ext1" ), key );
			Assert( key.Handled && key.SuppressKeyPress && previews == 3, "Numeric Enter did not preview and suppress confirmation." );
			Assert( !(bool)Field( dialog, "m_ConfirmCheck" ) && !( (Form)dialog ).IsDisposed && ReferenceEquals( Field( action, "m_Dlg" ), dialog ), "Enter completed the edit." );
		}
		finally { Call( ActionType, "End", action ); }
	}

	public static void SelectionChangesRetainAssignmentsAndScopeAxisEdits()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 11, 12 ), b = f.Add( 21, 22 ), c = f.Add( 31, 32 );
		object action = f.Action( a.UID ); int optimized = 0; SetField( action, "NeedOptimizedIK", (Action)delegate { optimized++; } );
		Call( ActionType, "Start", action ); object dialog = Field( action, "m_Dlg" );
		try {
			Number( dialog, "Ext1" ).Value = 80;
			ExternalAxisEditorTestHelper.ChangeSelection( action, b.UID, c.UID );
			Assert( a.CraftData.ExternalAxisPos.Ext1 == 80 && b.CraftData.ExternalAxisPos.Ext1 == 21 && c.CraftData.ExternalAxisPos.Ext1 == 31, "Changing selection undid or copied an old assignment." );
			Assert( Number( dialog, "Ext1" ).Value == 0 && Number( dialog, "Ext2" ).Value == 0 && optimized == 2, "Mixed selection was not silently synchronized." );
			Number( dialog, "Ext2" ).Value = 90;
			Assert( b.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 21, 90 ) ) && c.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 31, 90 ) ), "Editing Ext2 overwrote unedited Ext1." );
			ExternalAxisEditorTestHelper.ChangeSelection( action );
			Assert( !Number( dialog, "Ext1" ).Enabled && !( (Form)dialog ).IsDisposed, "Empty selection must not end the action." );
			ExternalAxisEditorTestHelper.ChangeSelection( action, a.UID, c.UID );
			Call( DialogType, "ProcessDialogKey", dialog, Keys.Enter );
			Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 80, 12 ) ) && c.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 31, 90 ) ), "Enter assigned a mixed-value display without an edit." );
			Input( dialog, "Ext1", "0" ); Call( DialogType, "ProcessDialogKey", dialog, Keys.Enter );
			Assert( a.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 0, 12 ) ) && c.CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 0, 90 ) ) && b.CraftData.ExternalAxisPos.Ext1 == 21, "Explicit zero was not restricted to the current selection and axis." );
			Call( ActionType, "End", action ); Call( ActionType, "End", action );
			Assert( optimized == 5, "Selection changes and End optimized an unexpected number of times." );
			object selector = Field( action, "m_PathIndexAction" );
			Assert( selector.GetType().GetField( "SelectionChange", Flags ).GetValue( selector ) == null, "Selection handler remained after End." );
		}
		finally { Call( ActionType, "End", action ); }
	}

	public static void ExistingOutOfRangeValuesAreNotImplicitAssignments()
	{
		using( Form dialog = (Form)Dialog( ExternalAxisType.Single, new ExternalAxisPos( 1e100, 34 ) ) ) {
			Assert( Number( dialog, "Ext1" ).Value == 0 && ( (Label)Field( dialog, "m_lblError" ) ).Text.Length > 0, "Out-of-range existing value needs an explicit display warning." );
			object[] args = { null }; Assert( (bool)Call( DialogType, "TryGetEditData", dialog, args ), "Unedited existing value should not prevent ending the editor." );
			Assert( Get( args[0], "Ext1" ) == null && Get( args[0], "Ext2" ) == null, "Opening the editor clamped an existing assignment." );
		}
	}

	public static void EditorSupportsEmptySingleAndMultipleSelection()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); f.Add( 0, 0 ); f.Add( 21, 33 );
		ExternalAxisEditorTestHelper.VerifyEditableSelection( f.Manager );
	}

	static void PreviewCase( ExternalAxisType type, bool mix, bool circle )
	{
		Fixture f = new Fixture( type, mix ); ContourPathObject a = f.Add( 0, 17 ); a.CraftData.LeadData = new LeadData( 2, 2, 90, 2, 2, 90 );
		PathObject b = circle ? f.Circle( a, 0 ) : f.Add( 0, 19 ); b.CraftData.LeadData = new LeadData( 2 ); object action = f.Action( b.UID, a.UID );
		int optimized = 0; SetField( action, "NeedOptimizedIK", (Action)delegate { optimized++; } );
		int previews = 0;
		Dictionary<MachineComponentType, List<gp_Trsf>> actual = null;
		SetField( action, "RaiseTrans", (Action<Dictionary<MachineComponentType, List<gp_Trsf>>, bool>)delegate( Dictionary<MachineComponentType, List<gp_Trsf>> frames, bool unlock ) { Assert( unlock, "Viewer update left locked." ); actual = frames; previews++; } );
		Call( ActionType, "Start", action ); object dialog = Field( action, "m_Dlg" );
		try {
			Assert( previews == 1, "Opening the editor must preview the current first selected path." );
			Number( dialog, "Ext1" ).Value = 73;
			Assert( previews == 2, "Ext1 did not immediately refresh the preview." );
			if( type == ExternalAxisType.Dual ) {
				Number( dialog, "Ext2" ).Value = -29;
				Assert( previews == 3, "Ext2 did not immediately refresh the preview." );
			}
			Assert( optimized == 0, "Preview invoked global optimization." );
			Assert( actual != null && actual.Values.All( list => list.Count == 1 ), "Preview is not exactly one frame." );
			CAMPoint point = CacheHelper.GetProcessStartPoint( b.UID ); Dictionary<MachineComponentType, List<gp_Trsf>> expected;
			Assert( new FiveAxisSimuPathBuilder().BuildSinglePointFrameTransform( f.Machine, point.Point,
				new PostPoint { Master = point.ModMaster_rad, Slave = point.ModSlave_rad, ExternalAxisPos = point.ExternalAxisPos }, out expected ), "Reference preview failed." );
			Assert( point.ExternalAxisPos.Ext1 == 73, "Cache was not rebuilt for external assignment." );
			foreach( var pair in expected ) { Matrix( actual[pair.Key][0], pair.Value[0] ); }
			Input( dialog, "Ext1", "" );
			Call( DialogType, "ProcessDialogKey", dialog, Keys.Enter );
			Assert( a.CraftData.ExternalAxisPos.Ext1 == 73 && b.CraftData.ExternalAxisPos.Ext1 == 73, "Blank input undid applied assignments." );
		}
		finally { Call( ActionType, "End", action ); }
	}
	public static void SpindleSinglePreview() { PreviewCase( ExternalAxisType.Single, false, false ); }
	public static void SpindleDualPreview() { PreviewCase( ExternalAxisType.Dual, false, false ); }
	public static void MixSinglePreview() { PreviewCase( ExternalAxisType.Single, true, false ); }
	public static void StandardPatternPreview() { PreviewCase( ExternalAxisType.Dual, false, true ); }
	public static void ExternalAxisPreviewAndResetDoNotFollowCamera()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); f.Add( 73, -29 );
		ExternalAxisEditorTestHelper.VerifyCamera( f.Manager );
	}
	public static void ExternalAxisMechanismRemainsDisplayedAfterAngleUpdates()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); f.Add( 0, 0 );
		OCC.Poly.Poly_Triangulation mesh = new OCC.Poly.Poly_Triangulation( 3, 1, false );
		mesh.SetNode( 1, new gp_Pnt( 0, 0, 0 ) );
		mesh.SetNode( 2, new gp_Pnt( 10, 0, 0 ) );
		mesh.SetNode( 3, new gp_Pnt( 0, 10, 0 ) );
		mesh.SetTriangle( 1, new OCC.Poly.Poly_Triangle( 1, 2, 3 ) );
		f.Machine.MachineMeshes.Meshes[MachineComponentType.Ext1] = mesh;
		ExternalAxisEditorTestHelper.VerifyMachinePreview( f.Manager );
	}
	public static void ToolbarsPreserveFunctionGroups()
	{
		Type type = App.GetType( "MyCAM.StartupForm", true );
		using( Form form = (Form)System.Runtime.Serialization.FormatterServices.GetUninitializedObject( type ) ) {
			typeof( Form ).GetConstructor( Type.EmptyTypes ).Invoke( form, new object[0] );
			Call( "MyCAM.StartupForm", "InitializeComponent", form );
			Dictionary<string, string[]> groups = new Dictionary<string, string[]> {
				{ "m_tsCAMFunction", new[] {
					"m_tsbAddPath,m_tsbDeletePath", "m_tsbAutoOrder,m_tsbManualOrder",
					"m_tsbStartPoint,m_tsbReverse,m_tsbEntryAndExitSetting,m_tsbSetTraverseData",
					"m_tsbSetLead,m_tsbOverCut,m_tsbMicroJoint,m_tsbRCorner,m_tsbExtendedGCode",
					"m_tsbToolVec,m_tsbFixedToolVec,m_tsbToolVecReverse,m_tsbExternalAxis,m_tsbOptimizeIK",
					"m_tsbPatternSetting,m_tsbPathEdit,m_tsbPathCompensation,m_btnContourEdit", "m_tsbMeasureCAM", "m_tsbCalibrationPnt,m_tsbConvertNC"
				} },
				{ "m_tsFileFunction", new[] {
					"m_tsbImport3DFile,m_tsbAddRevolution,m_tsbAddTube", "m_tsbReadProjectFile,m_tsbSaveProjectFile"
				} },
				{ "m_tsCADFunction", new[] {
					"m_tsbSewPart,m_tsbDesignSupportBracket",
					"m_tsbAddPoint_BoundingBoxCenter,m_tsbAddPoint_EdgeCenter,m_tsbAddPoint_PointCenter,m_tsbAddPoint_AcrCenter,m_tsbAddLine_TwoVertexConnect,m_tsbWireDiscreteVertex",
					"m_tsbAxisTransform,m_tsbManualTransform,m_tsb3PntTransform", "m_tsbMeasureCAD"
				} },
				{ "m_tsSimuFunc", new[] {
					"m_btnPlay,m_btnPause,m_btnStop", "m_btnPreviousPath,m_btnNextPath", "m_btnPreCollision,m_btnNextCol",
					"m_btnPrePoint,m_btnNextPoint", "m_btnSpeedUp,m_btnSlowDonw", "m_btnCheck"
				} },
				{ "m_tsViewerToolBar", new[] {
					"m_tsbAXO,m_tsbXPos,m_tsbXNeg,m_tsbYPos,m_tsbYNeg,m_tsbZPos,m_tsbZNeg",
					"m_tsbShowVec,m_tsbShowOrder,m_tsbShowOrientation,m_tsbShowTraverse,m_tsbShowMarker"
				} },
				{ "m_tsLayer", new[] {
					"m_tsbLayer1,m_tsbLayer2,m_tsbLayer3,m_tsbLayer4,m_tsbLayer5,m_tsbLayer6,m_tsbLayer7,m_tsbLayer8,m_tsbLayer9,m_tsbLayer10,m_tsbLayerNone,m_tsbNeedProcess"
				} },
				{ "m_msMode", new[] { "m_tsmiFile,m_tsmiCAD,m_tsmiCAM,m_tsmiSimulation,m_tsbSetting,m_tsbController" } }
			};
			foreach( var group in groups ) {
				ToolStrip toolbar = (ToolStrip)Field( form, group.Key );
				string actual = string.Join( ",", toolbar.Items.Cast<ToolStripItem>().Select( item => item is ToolStripSeparator ? "|" : item.Name ).ToArray() );
				Assert( actual == string.Join( ",|,", group.Value ), "Toolbar items or groups differ: " + group.Key );
				foreach( ToolStripSeparator separator in toolbar.Items.OfType<ToolStripSeparator>() ) {
					Assert( separator.Name.Length > 0 && ReferenceEquals( Field( form, separator.Name ), separator ), "Separators must have named designer fields: " + group.Key );
				}
			}
		}
	}

	static void Integrated( ExternalAxisType type, bool mix, bool mixed, bool zero, int pattern = 0 )
	{
		Fixture f = new Fixture( type, mix ); ContourPathObject a = f.Add( 0, 0 ); f.Add( 0, 0 );
		if( pattern != 0 ) { f.Pattern( pattern ); } else if( mixed ) { f.Circle( a, 0 ); }
		object action = f.Action(); SetField( action, "NeedOptimizedIK", (Action)f.Optimize );
		Call( ActionType, "ConfirmPositions", action, Edit( zero ? 0 : 73, zero ? 0 : -29 ) );
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
		Call( ActionType, "ConfirmPositions", action, Edit( 360, null ) );
		Assert( f.Groups().Length == 3, "0/360/0 merged or reordered." );
		foreach( var path in new[] { a, b, c } ) { Assert( path.CraftData.InitMaster_rad == 0 && path.CraftData.InitSlave_rad == 0, "New group boundary did not reset global seeds." ); }
	}
	public static void ProgrammedProjectRoundTrip()
	{
		Fixture f = new Fixture( ExternalAxisType.Dual, false ); ContourPathObject a = f.Add( 0, 0 ); f.Circle( a, 0 );
		PartObject part = new PartObject( "Part0", new OCC.BRepPrimAPI.BRepPrimAPI_MakeBox( 10, 20, 30 ).Shape() );
		f.Map.Add( part.UID, part ); ( (List<string>)Get( f.Manager, "PartIDList" ) ).Add( part.UID );
		object action = f.Action(); SetField( action, "NeedOptimizedIK", (Action)f.Optimize ); Call( ActionType, "ConfirmPositions", action, Edit( 360, -29 ) );
		DataManagerDTO dto = (DataManagerDTO)New( "MyCAM.FileManager.DataManagerDTO", f.Manager ); XmlSerializer serializer = new XmlSerializer( typeof( DataManagerDTO ) );
		DataManagerDTO restored;
		using( StringWriter writer = new StringWriter() ) { serializer.Serialize( writer, dto ); restored = (DataManagerDTO)serializer.Deserialize( new StringReader( writer.ToString() ) ); }
		object[] args = new object[8]; Call( "MyCAM.FileManager.DataManagerDTO", "DataMgrDTO2Data", restored, args );
		Dictionary<string, IObject> map = (Dictionary<string, IObject>)args[0]; Assert( ( (List<string>)args[2] ).SequenceEqual( f.IDs ), "Project path ordering changed." );
		foreach( string id in f.IDs ) { Assert( ( (PathObject)map[id] ).CraftData.ExternalAxisPos.Equals( new ExternalAxisPos( 360, -29 ) ), "Programmed project lost external positions." ); }
	}
}
