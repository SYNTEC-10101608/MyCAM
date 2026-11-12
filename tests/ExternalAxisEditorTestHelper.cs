using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;
using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.gp;
using OCC.V3d;

internal static class ExternalAxisEditorTestHelper
{
	static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

	public static object CreateAction( object manager, List<string> ids )
	{
		SelectionLifetime lifetime = new SelectionLifetime( manager, ids );
		try {
			Type type = typeof( CraftData ).Assembly.GetType( "MyCAM.Editor.ExternalAxisAction", true );
			object action = Activator.CreateInstance( type, Flags, null, new object[] { manager, ids, lifetime.Selector }, null );
			PropertyInfo end = type.GetProperty( "EndAction", Flags );
			end.SetValue( action, Delegate.CreateDelegate( end.PropertyType, lifetime, typeof( SelectionLifetime ).GetMethod( "End" ) ), null );
			return action;
		}
		catch {
			lifetime.Dispose();
			throw;
		}
	}

	public static void ChangeSelection( object action, params string[] ids )
	{
		object selector = action.GetType().GetField( "m_PathIndexAction", Flags ).GetValue( action );
		selector.GetType().GetMethod( "ClearSelection" ).Invoke( selector, null );
		foreach( string id in ids ) { selector.GetType().GetMethod( "SelectPathByID" ).Invoke( selector, new object[] { id } ); }
	}

	sealed class SelectionLifetime : IDisposable
	{
		readonly object m_Viewer;
		readonly Panel m_Viewport = new Panel { Width = 320, Height = 240 };
		readonly TreeView m_Tree;
		public object Selector;
		bool m_IsDisposed;
		public SelectionLifetime( object manager, List<string> ids )
		{
			Assembly app = typeof( CraftData ).Assembly;
			app.GetType( "MyCAM.App.MyApp", true ).GetMethod( "InitAppSetting", Flags ).Invoke( null, null );
			Type viewerType = Assembly.LoadFrom( Path.Combine( Path.GetDirectoryName( app.Location ), "OCCViewer.dll" ) ).GetType( "OCCViewer.Viewer", true );
			m_Viewer = Activator.CreateInstance( viewerType );
			m_Tree = (TreeView)Activator.CreateInstance( app.GetType( "MyCAM.Editor.MultiSelectTreeView", true ) );
			if( !(bool)viewerType.GetMethod( "InitViewer" ).Invoke( m_Viewer, new object[] { m_Viewport } ) ) { throw new Exception( "Cannot initialize selection test viewer." ); }
			object viewManager = Activator.CreateInstance( app.GetType( "MyCAM.Editor.ViewManager", true ), Flags, null, new object[] { m_Viewer }, null );
			Selector = Activator.CreateInstance( app.GetType( "MyCAM.Editor.SelectPathAction", true ), Flags, null, new object[] { manager, m_Viewer, m_Tree, viewManager }, null );
			foreach( string id in ids ) { Selector.GetType().GetMethod( "SelectPathByID" ).Invoke( Selector, new object[] { id } ); }
		}
		public void End( object action ) { Dispose(); }
		public void Dispose()
		{
			if( m_IsDisposed ) { return; }
			m_IsDisposed = true;
			( (IDisposable)m_Viewer ).Dispose(); m_Tree.Dispose(); m_Viewport.Dispose();
		}
	}

	public static void Optimize( object manager )
	{
		WithEditor( manager, delegate( object editor, object viewer ) { editor.GetType().GetMethod( "DoOptimizeIKContinuity", Flags ).Invoke( editor, null ); } );
	}

	public static void VerifyEditableSelection( object manager )
	{
		WithEditor( manager, delegate( object editor, object viewer ) {
			Type type = editor.GetType();
			FieldInfo status = type.GetField( "RaiseCAMActionStatusChange", Flags );
			ParameterExpression[] parameters = status.FieldType.GetMethod( "Invoke" ).GetParameters().Select( p => Expression.Parameter( p.ParameterType ) ).ToArray();
			status.SetValue( editor, Expression.Lambda( status.FieldType, Expression.Empty(), parameters ).Compile() );
			TreeView tree = (TreeView)type.GetField( "m_TreeView", Flags ).GetValue( editor );
			V3d_View view = (V3d_View)viewer.GetType().GetMethod( "GetView" ).Invoke( viewer, null );
			double[] camera = Camera( view );
			type.GetMethod( "EditStart" ).Invoke( editor, null );
			try {
				type.GetMethod( "SetExternalAxisPos" ).Invoke( editor, null );
				object action = type.GetField( "m_CurrentAction", Flags ).GetValue( editor );
				object dialog = action.GetType().GetField( "m_Dlg", Flags ).GetValue( action );
				NumericUpDown number = (NumericUpDown)dialog.GetType().GetField( "m_dbtnExt1", Flags ).GetValue( dialog );
				if( !tree.Enabled || number.Enabled ) { throw new Exception( "Empty-selection editor must stay open with selection enabled and numeric input disabled." ); }
				ChangeSelection( action, "Path0" ); number.Value = 73;
				ChangeSelection( action );
				if( number.Enabled || ( (Form)dialog ).IsDisposed ) { throw new Exception( "Clearing selection ended the edit or left input enabled." ); }
				ChangeSelection( action, "Path0", "Path1" );
				if( number.Value != 0 ) { throw new Exception( "Mixed values were not silently synchronized." ); }
				number.Value = 42;
				ChangeSelection( action, "Path1" );
				if( number.Value != 42 || !tree.Enabled ) { throw new Exception( "Selection changes did not synchronize values or locked the tree." ); }
				dialog.GetType().GetMethod( "ProcessDialogKey", Flags ).Invoke( dialog, new object[] { Keys.Enter } );
				if( ( (Form)dialog ).IsDisposed ) { throw new Exception( "Enter ended the editor." ); }
				AssertCamera( camera, Camera( view ) );
				( (Form)dialog ).Close();
				AssertCamera( camera, Camera( view ) );
				object defaultAction = type.GetField( "m_DefaultAction", Flags ).GetValue( editor );
				if( !ReferenceEquals( defaultAction, type.GetField( "m_CurrentAction", Flags ).GetValue( editor ) ) ) { throw new Exception( "Closing did not restore the default selection action." ); }
				defaultAction.GetType().GetMethod( "ClearSelection" ).Invoke( defaultAction, null );
				defaultAction.GetType().GetMethod( "SelectPathByID" ).Invoke( defaultAction, new object[] { "Path0" } );
				if( ( (List<string>)defaultAction.GetType().GetMethod( "GetSelectedIDs" ).Invoke( defaultAction, null ) ).Single() != "Path0" ) { throw new Exception( "Default selection did not resume after closing." ); }
				foreach( string id in new[] { "Path0", "Path1" } ) {
					if( CacheHelper.GetProcessStartPoint( id ).ExternalAxisPos.Ext1 != 42 ) { throw new Exception( "Closing restored an already-applied edit." ); }
				}
			}
			finally { type.GetMethod( "EditEnd" ).Invoke( editor, null ); }
		} );
	}

	public static void VerifyRendererPause( object manager )
	{
		WithEditor( manager, delegate( object editor, object viewer ) {
			Type type = editor.GetType();
			FieldInfo status = type.GetField( "RaiseCAMActionStatusChange", Flags );
			ParameterExpression[] parameters = status.FieldType.GetMethod( "Invoke" ).GetParameters().Select( p => Expression.Parameter( p.ParameterType ) ).ToArray();
			status.SetValue( editor, Expression.Lambda( status.FieldType, Expression.Empty(), parameters ).Compile() );
			type.GetMethod( "EditStart" ).Invoke( editor, null );
			try {
				string[] names = { "m_MainPathRenderer", "m_ToolVecRenderer", "m_TraverseRenderer" };
				string[] dictionaries = { "m_OriginalPathAISDict", "m_ToolVecAISDict", "m_TraverseAISDict" };
				object[] renderers = names.Select( name => type.GetField( name, Flags ).GetValue( editor ) ).ToArray();
				AIS_InteractiveObject[][] before = new AIS_InteractiveObject[renderers.Length][];
				for( int i = 0; i < renderers.Length; i++ ) {
					renderers[i].GetType().GetMethod( "SetShow" ).Invoke( renderers[i], new object[] { true } );
					renderers[i].GetType().GetMethods().Single( method => method.Name == "Show" && method.GetParameters().Length == 1 ).Invoke( renderers[i], new object[] { false } );
					before[i] = RendererAIS( renderers[i], dictionaries[i] );
					if( before[i].Length == 0 ) { throw new Exception( "Renderer pause fixture has no AIS: " + names[i] ); }
				}
				type.GetMethod( "SetExternalAxisPos" ).Invoke( editor, null );
				object action = type.GetField( "m_CurrentAction", Flags ).GetValue( editor );
				object dialog = action.GetType().GetField( "m_Dlg", Flags ).GetValue( action );
				ChangeSelection( action, "Path0" );
				( (NumericUpDown)dialog.GetType().GetField( "m_dbtnExt1", Flags ).GetValue( dialog ) ).Value = 73;
				ChangeSelection( action );
				ChangeSelection( action, "Path1" );
				AIS_InteractiveContext context = (AIS_InteractiveContext)viewer.GetType().GetMethod( "GetAISContext" ).Invoke( viewer, null );
				for( int i = 0; i < renderers.Length; i++ ) {
					if( !(bool)renderers[i].GetType().BaseType.GetField( "m_IsPauseRefreshAndHide", Flags ).GetValue( renderers[i] ) ) { throw new Exception( "Renderer was not paused during external-axis editing: " + names[i] ); }
					AIS_InteractiveObject[] after = RendererAIS( renderers[i], dictionaries[i] );
					if( after.Length != before[i].Length || !before[i].Zip( after, ReferenceEquals ).All( same => same ) || after.Any( context.IsDisplayed ) ) { throw new Exception( "Paused renderer rebuilt or displayed its AIS: " + names[i] ); }
				}
				( (Form)dialog ).Close();
				for( int i = 0; i < renderers.Length; i++ ) {
					if( (bool)renderers[i].GetType().BaseType.GetField( "m_IsPauseRefreshAndHide", Flags ).GetValue( renderers[i] ) ) { throw new Exception( "Renderer remained paused after editing: " + names[i] ); }
					AIS_InteractiveObject[] after = RendererAIS( renderers[i], dictionaries[i] );
					if( after.Length == 0 || !after.All( context.IsDisplayed ) ) { throw new Exception( "Renderer display was not restored after editing: " + names[i] ); }
				}
			}
			finally { type.GetMethod( "EditEnd" ).Invoke( editor, null ); }
		} );
	}

	static AIS_InteractiveObject[] RendererAIS( object renderer, string field )
	{
		List<AIS_InteractiveObject> result = new List<AIS_InteractiveObject>();
		foreach( object value in ( (IDictionary)renderer.GetType().GetField( field, Flags ).GetValue( renderer ) ).Values ) {
			AIS_InteractiveObject shape = value as AIS_InteractiveObject;
			if( shape != null ) { result.Add( shape ); }
			else { result.AddRange( ( (IEnumerable)value ).Cast<AIS_InteractiveObject>() ); }
		}
		return result.ToArray();
	}

	public static void VerifyCamera( object manager )
	{
		WithEditor( manager, delegate( object editor, object viewer ) {
			V3d_View view = (V3d_View)viewer.GetType().GetMethod( "GetView" ).Invoke( viewer, null );
			double[] before = Camera( view );
			FiveAxisMachineData machine = (FiveAxisMachineData)manager.GetType().GetProperty( "MachineData" ).GetValue( manager, null );
			List<string> ids = (List<string>)manager.GetType().GetProperty( "PathIDList" ).GetValue( manager, null );
			CAMPoint point = CacheHelper.GetProcessStartPoint( ids[0] ); Dictionary<MachineComponentType, List<gp_Trsf>> frames;
			if( !new FiveAxisSimuPathBuilder().BuildSinglePointFrameTransform( machine, point.Point,
				new PostPoint { Master = point.ModMaster_rad, Slave = point.ModSlave_rad, ExternalAxisPos = point.ExternalAxisPos }, out frames ) ) {
				throw new Exception( "Cannot build camera test frames." );
			}
			MethodInfo preview = editor.GetType().GetMethod( "SetExternalAxisPreview", Flags );
			MethodInfo reset = editor.GetType().GetMethods( Flags ).Single( method => method.Name == "ResetPartDisplay" && method.GetParameters().Length == 1 );
			for( int i = 0; i < 2; i++ ) {
				view.SetImmediateUpdate( true );
				preview.Invoke( editor, new object[] { frames, true } );
				if( !view.SetImmediateUpdate( false ) ) { throw new Exception( "Preview did not restore immediate view updates." ); }
				preview.Invoke( editor, new object[] { frames, true } );
				if( !view.SetImmediateUpdate( true ) ) { throw new Exception( "Preview did not enable immediate updates like ToolVectorAction." ); }
				AssertCamera( before, Camera( view ) );
				reset.Invoke( editor, new object[] { false } );
				AssertCamera( before, Camera( view ) );
			}
			bool rejected = false;
			try { preview.Invoke( editor, new object[] { new Dictionary<MachineComponentType, List<gp_Trsf>>(), true } ); }
			catch( TargetInvocationException ex ) { rejected = ex.InnerException is KeyNotFoundException; }
			if( !rejected || !view.SetImmediateUpdate( true ) ) { throw new Exception( "Failed preview did not restore immediate view updates." ); }
			editor.GetType().GetMethods( Flags ).Single( method => method.Name == "SetTrans" && method.GetParameters().Length == 2 ).Invoke( editor, new object[] { frames, true } );
			if( before.Zip( Camera( view ), ( a, b ) => Math.Abs( a - b ) ).All( difference => difference < 1e-8 ) ) {
				throw new Exception( "Existing tool-vector preview no longer follows the workpiece." );
			}
		} );
	}

	public static void VerifyMachinePreview( object manager )
	{
		WithEditor( manager, delegate( object editor, object viewer ) {
			V3d_View view = (V3d_View)viewer.GetType().GetMethod( "GetView" ).Invoke( viewer, null );
			AIS_InteractiveContext context = (AIS_InteractiveContext)viewer.GetType().GetMethod( "GetAISContext" ).Invoke( viewer, null );
			object renderer = editor.GetType().GetField( "m_MachineRender", Flags ).GetValue( editor );
			Dictionary<MachineComponentType, List<AIS_InteractiveObject>> meshAIS = (Dictionary<MachineComponentType, List<AIS_InteractiveObject>>)renderer.GetType().BaseType.GetField( "m_MachineMeshAISDict", Flags ).GetValue( renderer );
			AIS_InteractiveObject mechanism = meshAIS[MachineComponentType.Ext1].Single();
			FiveAxisMachineData machine = (FiveAxisMachineData)manager.GetType().GetProperty( "MachineData" ).GetValue( manager, null );
			double[] camera = Camera( view );
			foreach( double angle in new[] { 0.0, 73.0, -29.0 } ) {
				Dictionary<MachineComponentType, List<gp_Trsf>> frames;
				PostPoint orientation = new PostPoint { ExternalAxisPos = new ExternalAxisPos( angle, 0 ) };
				if( !new FiveAxisSimuPathBuilder().BuildSinglePointFrameTransform( machine, new gp_Pnt( 10, 0, 0 ), orientation, out frames ) ) { throw new Exception( "Cannot build mechanism preview." ); }
				view.SetImmediateUpdate( false );
				editor.GetType().GetMethod( "SetExternalAxisPreview", Flags ).Invoke( editor, new object[] { frames, true } );
				if( !view.SetImmediateUpdate( true ) || !context.IsDisplayed( mechanism ) ) { throw new Exception( "Mechanism preview did not enable updates and display its AIS." ); }
				gp_Trsf actual = mechanism.LocalTransformation();
				gp_Trsf expected = frames[MachineComponentType.Ext1].Last();
				for( int row = 1; row <= 3; row++ ) { for( int column = 1; column <= 4; column++ ) {
					if( Math.Abs( actual.Value( row, column ) - expected.Value( row, column ) ) > 1e-8 ) { throw new Exception( "Displayed mechanism posture was not updated." ); }
				} }
				AssertCamera( camera, Camera( view ) );
			}
			editor.GetType().GetMethod( "HideMachine", Flags ).Invoke( editor, null );
			if( context.IsDisplayed( mechanism ) ) { throw new Exception( "Mechanism cleanup did not remove its AIS." ); }
		} );
	}

	static double[] Camera( V3d_View view )
	{
		double x = 0, y = 0, z = 0;
		view.Eye( ref x, ref y, ref z ); double[] eye = { x, y, z };
		view.At( ref x, ref y, ref z ); double[] at = { x, y, z };
		view.Up( ref x, ref y, ref z );
		return eye.Concat( at ).Concat( new[] { x, y, z } ).ToArray();
	}

	static void AssertCamera( double[] expected, double[] actual )
	{
		if( expected.Zip( actual, ( a, b ) => Math.Abs( a - b ) ).Any( difference => difference > 1e-8 ) ) {
			throw new Exception( "External-axis preview or reset moved the camera." );
		}
	}

	static void WithEditor( object manager, Action<object, object> verify )
	{
		Assembly app = typeof( CraftData ).Assembly;
		Type appType = app.GetType( "MyCAM.App.MyApp", true );
		appType.GetMethod( "InitAppSetting", Flags ).Invoke( null, null );
		Type viewerType = Assembly.LoadFrom( Path.Combine( Path.GetDirectoryName( app.Location ), "OCCViewer.dll" ) ).GetType( "OCCViewer.Viewer", true );
		object viewer = Activator.CreateInstance( viewerType );
		using( Panel viewport = new Panel() )
		using( Panel log = new Panel() )
		using( TreeView tree = (TreeView)Activator.CreateInstance( app.GetType( "MyCAM.Editor.MultiSelectTreeView", true ) ) )
		using( IDisposable viewerLifetime = (IDisposable)viewer ) {
			viewport.Width = 320;
			viewport.Height = 240;
			if( !(bool)viewerType.GetMethod( "InitViewer" ).Invoke( viewer, new object[] { viewport } ) ) {
				throw new Exception( "Cannot initialize the editor test viewer." );
			}
			FieldInfo logger = appType.GetField( "m_Logger", Flags );
			object oldLogger = logger.GetValue( null );
			try {
				logger.SetValue( null, Activator.CreateInstance( app.GetType( "MyCAM.LogManager.LogHandler", true ), Flags, null, new object[] { log }, null ) );
				object viewManager = Activator.CreateInstance( app.GetType( "MyCAM.Editor.ViewManager", true ), Flags, null, new object[] { viewer }, null );
				Type editorType = app.GetType( "MyCAM.Editor.CAMEditor", true );
				object editor = Activator.CreateInstance( editorType, Flags, null, new object[] { manager, viewer, tree, viewManager }, null );
				verify( editor, viewer );
			}
			finally {
				logger.SetValue( null, oldLogger );
			}
		}
	}
}
