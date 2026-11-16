using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CommonUI.Controls;
using CommonUI.Theme;

namespace UICheck
{
	// Runtime layout check (ui-style.instructions.md, section 8). Compiled by CheckRuntimeLayout.ps1 into the
	// build output folder, so language files and dependencies are found exactly as by MyCAM.exe.
	//
	//   T1  text wider than its fixed-size control (button, label, check box, radio button)
	//   T2  auto-sized text that runs past its container
	//   T3  section title wider than its card
	//   T4  visible sibling controls that overlap
	//   T5  visible control outside its container (scrolling containers are skipped)
	//
	// Usage: UICheckRunner.exe <language> <screenshot folder|-> [type name filter]
	internal static class LayoutRunner
	{
		[STAThread]
		static int Main( string[] args )
		{
			if( args.Length < 2 ) {
				Console.WriteLine( "usage: UICheckRunner.exe <language> <screenshot folder|-> [filter]" );
				return 2;
			}
			string szLanguage = args[ 0 ];
			string szShotDir = args[ 1 ] == "-" ? null : Path.Combine( args[ 1 ], szLanguage );
			string szFilter = args.Length > 2 ? args[ 2 ] : null;
			if( szShotDir != null ) {
				Directory.CreateDirectory( szShotDir );
			}
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );

			// same start-up order as StartupForm
			string szBinDir = AppDomain.CurrentDomain.BaseDirectory;
			Assembly app = Assembly.LoadFrom( Path.Combine( szBinDir, "MyCAM.exe" ) );
			Assembly cnc = Assembly.LoadFrom( Path.Combine( szBinDir, "CncUI.dll" ) );
			app.GetType( "MyCAM.App.MyApp" ).GetMethod( "InitAppSetting" ).Invoke( null, null );
			LanguageManager.LanguageManager.CurrentUILanguage = szLanguage;
			ThemeFonts.Initialize( szLanguage );

			int nProblemCount = 0;
			int nCheckedCount = 0;
			foreach( Type type in GetScreenTypes( app, cnc ) ) {
				if( szFilter != null && type.Name.IndexOf( szFilter, StringComparison.OrdinalIgnoreCase ) < 0 ) {
					continue;
				}
				List<string> problemList;
				string szError = CheckScreen( type, szShotDir, out problemList );
				if( szError != null ) {
					Console.WriteLine( "SKIP  {0}: {1}", type.Name, szError );
					continue;
				}
				nCheckedCount++;
				foreach( string szProblem in problemList ) {
					Console.WriteLine( "FAIL  {0} {1}", type.Name, szProblem );
				}
				nProblemCount += problemList.Count;
				if( problemList.Count == 0 ) {
					Console.WriteLine( "OK    {0}", type.Name );
				}
			}
			Console.WriteLine( "[{0}] {1} screens checked, {2} problems", szLanguage, nCheckedCount, nProblemCount );
			return nProblemCount == 0 ? 0 : 1;
		}

		static IEnumerable<Type> GetScreenTypes( Assembly app, Assembly cnc )
		{
			Type[] allTypeArray = SafeGetTypes( app ).Concat( SafeGetTypes( cnc ) ).ToArray();
			foreach( Type type in allTypeArray.OrderBy( t => t.Name ) ) {
				bool isConcrete = type.IsClass && !type.IsAbstract && !type.ContainsGenericParameters;
				if( isConcrete == false || SKIP_TYPE_LIST.Contains( type.Name ) ) {
					continue;
				}
				bool isForm = typeof( Form ).IsAssignableFrom( type );
				bool isPanel = typeof( UserControl ).IsAssignableFrom( type ) && PANEL_SIZE_MAP.ContainsKey( type.Name );
				if( isForm || isPanel ) {
					yield return type;
				}
			}
		}

		static IEnumerable<Type> SafeGetTypes( Assembly assembly )
		{
			try {
				return assembly.GetTypes();
			}
			catch( ReflectionTypeLoadException ex ) {
				return ex.Types.Where( t => t != null );
			}
		}

		static string CheckScreen( Type type, string szShotDir, out List<string> problemList )
		{
			problemList = new List<string>();
			Form form = null;
			try {
				object instance = CreateInstance( type );
				form = instance as Form;
				if( form == null ) {
					form = HostPanel( (Control)instance );
				}

				// wizard dialogs show one step through a method of their own
				string szPrepare;
				if( PREPARE_METHOD_MAP.TryGetValue( type.Name, out szPrepare ) ) {
					BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
					type.GetMethod( szPrepare, flags ).Invoke( instance, null );
				}

				// show without disturbing the desktop; the layout and the language texts are applied on Load
				form.Opacity = 0;
				form.Show();
				Application.DoEvents();
				CheckChildren( form, problemList );
				if( szShotDir != null ) {
					SaveScreenshot( form, Path.Combine( szShotDir, type.Name + ".png" ) );
				}
				return null;
			}
			catch( Exception ex ) {
				Exception inner = ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex;
				return inner.GetType().Name + " " + inner.Message.Split( '\n' )[ 0 ].Trim();
			}
			finally {
				if( form != null && !form.IsDisposed ) {
					form.Hide();
					form.Dispose();
				}
			}
		}

		static Form HostPanel( Control panel )
		{
			Size size = PANEL_SIZE_MAP[ panel.GetType().Name ];
			Form host = new Form();
			host.FormBorderStyle = FormBorderStyle.None;
			host.ShowInTaskbar = false;
			host.StartPosition = FormStartPosition.Manual;
			host.Location = new Point( 0, 0 );
			host.ClientSize = size;
			host.Font = ThemeFonts.Body;
			host.BackColor = ThemeColors.WINDOW;
			panel.Bounds = new Rectangle( Point.Empty, size );
			host.Controls.Add( panel );
			return host;
		}

		#region Placeholder constructor arguments

		internal static object CreateInstance( Type type )
		{
			ConstructorInfo[] ctorArray = type.GetConstructors( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
			ConstructorInfo ctor = ctorArray.OrderBy( c => c.GetParameters().Length ).FirstOrDefault();
			if( ctor == null ) {
				throw new InvalidOperationException( "no constructor" );
			}
			object[] argArray = ctor.GetParameters().Select( p => CreateArgument( p.ParameterType, 0 ) ).ToArray();
			return ctor.Invoke( argArray );
		}

		static object CreateArgument( Type type, int nDepth )
		{
			if( type.IsByRef ) {
				type = type.GetElementType();
			}
			if( type == typeof( string ) ) {
				return "Sample";
			}
			if( type == typeof( bool ) ) {
				return false;
			}
			if( type.IsEnum ) {
				return Enum.GetValues( type ).GetValue( 0 );
			}
			if( type.IsPrimitive || type == typeof( decimal ) ) {
				return Convert.ChangeType( 1, type );
			}
			if( type.IsArray ) {
				return Array.CreateInstance( type.GetElementType(), 0 );
			}
			if( type.IsValueType ) {
				// a struct with a constructor gets its members filled, e.g. points that must not be null
				ConstructorInfo structCtor = type.GetConstructors().OrderByDescending( c => c.GetParameters().Length ).FirstOrDefault();
				if( structCtor == null || nDepth > 3 ) {
					return Activator.CreateInstance( type );
				}
				return structCtor.Invoke( structCtor.GetParameters().Select( p => CreateArgument( p.ParameterType, nDepth + 1 ) ).ToArray() );
			}
			if( type.IsInterface || type.IsAbstract || nDepth > 3 ) {
				return FindImplementation( type, nDepth );
			}
			if( typeof( IEnumerable ).IsAssignableFrom( type ) && type.GetConstructor( Type.EmptyTypes ) != null ) {
				return Activator.CreateInstance( type );
			}
			ConstructorInfo ctor = type.GetConstructors().OrderBy( c => c.GetParameters().Length ).FirstOrDefault();
			if( ctor == null ) {
				return null;
			}
			object[] argArray = ctor.GetParameters().Select( p => CreateArgument( p.ParameterType, nDepth + 1 ) ).ToArray();
			return ctor.Invoke( argArray );
		}

		static object FindImplementation( Type type, int nDepth )
		{
			// pick the first concrete type with a parameterless constructor
			Type implementation = SafeGetTypes( type.Assembly ).FirstOrDefault( t => t.IsClass && !t.IsAbstract &&
				!t.ContainsGenericParameters && type.IsAssignableFrom( t ) && t.GetConstructor( Type.EmptyTypes ) != null );
			return implementation == null ? null : Activator.CreateInstance( implementation );
		}

		#endregion

		#region Checks

		static void CheckChildren( Control parent, List<string> problemList )
		{
			List<Control> visibleList = parent.Controls.Cast<Control>().Where( c => c.Visible ).ToList();
			bool isScrolling = parent is ScrollableControl && ( (ScrollableControl)parent ).AutoScroll;
			foreach( Control control in visibleList ) {
				CheckText( control, problemList );
				if( isScrolling == false && !( parent is Form && control.Dock != DockStyle.None ) ) {
					CheckInside( parent, control, problemList );
				}
				if( IsLeaf( control ) == false ) {
					CheckChildren( control, problemList );
				}
			}
			for( int i = 0; i < visibleList.Count; i++ ) {
				for( int j = i + 1; j < visibleList.Count; j++ ) {
					Rectangle overlap = Rectangle.Intersect( visibleList[ i ].Bounds, visibleList[ j ].Bounds );
					if( overlap.Width > 0 && overlap.Height > 0 && visibleList[ i ].Dock == DockStyle.None &&
						visibleList[ j ].Dock == DockStyle.None ) {
						problemList.Add( string.Format( "T4 {0} overlaps {1}", Describe( visibleList[ i ] ),
							Describe( visibleList[ j ] ) ) );
					}
				}
			}
		}

		static bool IsLeaf( Control control )
		{
			return control is UpDownBase || control is TextBoxBase || control is ComboBox || control is DataGridView ||
				control is ListControl || control is TrackBar || control is TreeView || control is ToolStrip;
		}

		static void CheckText( Control control, List<string> problemList )
		{
			string szText = control.Text;
			if( string.IsNullOrEmpty( szText ) ) {
				return;
			}
			SectionPanel section = control as SectionPanel;
			if( section != null ) {
				int nTitleWidth = TextRenderer.MeasureText( szText, ThemeFonts.Emphasis ).Width + TITLE_INDENT +
					ThemeMetrics.SECTION_PADDING;
				if( nTitleWidth > section.Width ) {
					problemList.Add( string.Format( "T3 title of {0} needs {1}px", Describe( control ), nTitleWidth ) );
				}
				return;
			}
			bool isButton = control is Button;
			bool isCheck = control is CheckBox || control is RadioButton;
			bool isLabel = control is Label;
			if( isButton == false && isCheck == false && isLabel == false ) {
				return;
			}
			Size textSize = TextRenderer.MeasureText( szText, control.Font, Size.Empty,
				TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix );
			// a section title label draws the accent bar before its text
			ThemedLabel themedLabel = control as ThemedLabel;
			bool isSectionTitle = themedLabel != null && themedLabel.Role == TextRole.SectionTitle;
			int nPadding = isButton ? BUTTON_TEXT_PADDING : isCheck ? CHECK_GLYPH_WIDTH :
				isSectionTitle ? TITLE_INDENT - ThemeMetrics.SECTION_PADDING : 0;
			if( control.AutoSize ) {
				// an auto-sized control grows; it must stay inside its parent
				int nRight = control.Left + textSize.Width + nPadding;
				if( control.Parent != null && nRight > control.Parent.ClientSize.Width ) {
					problemList.Add( string.Format( "T2 {0} reaches x={1}, container width {2}", Describe( control ),
						nRight, control.Parent.ClientSize.Width ) );
				}
				return;
			}

			// multi-line labels wrap
			int nLineCount = Math.Max( 1, control.Height / Math.Max( 1, textSize.Height ) );
			bool isWrapping = isLabel && nLineCount > 1;
			int nAvailable = control.Width - nPadding;
			if( isWrapping == false && textSize.Width > nAvailable ) {
				problemList.Add( string.Format( "T1 {0} text needs {1}px, has {2}px", Describe( control ),
					textSize.Width, nAvailable ) );
			}
		}

		static void CheckInside( Control parent, Control control, List<string> problemList )
		{
			Rectangle client = parent.ClientRectangle;
			Rectangle bounds = control.Bounds;
			if( client.Contains( bounds ) == false ) {
				problemList.Add( string.Format( "T5 {0} {1} outside {2} {3}", Describe( control ), bounds,
					Describe( parent ), client.Size ) );
			}
		}

		static string Describe( Control control )
		{
			return string.IsNullOrEmpty( control.Name ) ? control.GetType().Name : control.Name;
		}

		#endregion

		static void SaveScreenshot( Form form, string szPath )
		{
			using( Bitmap bitmap = new Bitmap( form.Width, form.Height ) ) {
				form.DrawToBitmap( bitmap, new Rectangle( Point.Empty, form.Size ) );
				bitmap.Save( szPath, ImageFormat.Png );
			}
		}

		// SectionPanel: accent bar + gap before the title
		const int TITLE_INDENT = ThemeMetrics.SECTION_PADDING + 3 + ThemeMetrics.CONTROL_GAP;
		const int BUTTON_TEXT_PADDING = 12;
		const int CHECK_GLYPH_WIDTH = 18;

		// the main window is checked by screenshots; the remote screen only hosts the VNC view
		// method called before the check, e.g. to show the main step of a wizard
		static readonly Dictionary<string, string> PREPARE_METHOD_MAP = new Dictionary<string, string>
		{
			{ "DesignSupportBracketDlg", "ShowInputParametersPanel" }
		};

		static readonly HashSet<string> SKIP_TYPE_LIST = new HashSet<string> { "StartupForm", "CNCDlg" };

		// embedded panels are hosted at a typical size of their area in the main window
		static readonly Dictionary<string, Size> PANEL_SIZE_MAP = new Dictionary<string, Size>
		{
			{ "CNCPanel", new Size( 320, 900 ) },
			{ "CNCInfoPanel", new Size( 1000, 260 ) },
			{ "CNCAlarmPanel", new Size( 600, 220 ) }
		};
	}
}
