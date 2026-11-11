using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using MyCAM.Data;

internal static class ExternalAxisEditorTestHelper
{
	static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

	public static void Optimize( object manager )
	{
		Assembly app = typeof( CraftData ).Assembly;
		Type appType = app.GetType( "MyCAM.App.MyApp", true );
		appType.GetMethod( "InitAppSetting", Flags ).Invoke( null, null );
		Type viewerType = Assembly.LoadFrom( Path.Combine( Path.GetDirectoryName( app.Location ), "OCCViewer.dll" ) ).GetType( "OCCViewer.Viewer", true );
		object viewer = Activator.CreateInstance( viewerType );
		using( Panel viewport = new Panel() )
		using( Panel log = new Panel() )
		using( TreeView tree = new TreeView() )
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
				editorType.GetMethod( "DoOptimizeIKContinuity", Flags ).Invoke( editor, null );
			}
			finally {
				logger.SetValue( null, oldLogger );
			}
		}
	}
}
