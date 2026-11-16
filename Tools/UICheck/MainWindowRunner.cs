using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CommonUI.Forms;

namespace UICheck
{
	// Main window screenshot and dialog placement check (ui-style.instructions.md, sections 4.3 and 6).
	// Compiled by CaptureMainWindow.ps1 into the build output folder together with LayoutRunner.cs.
	//
	// Landscape / portrait only depends on the main window's aspect ratio, so a portrait window larger than a
	// landscape screen is checked by sizing the window beyond the screen (the part outside is still rendered).
	//
	//   P1  dialog not centered in DialogPlacement's area (whole window in landscape, CAM area in portrait)
	//   P2  portrait CAM area is not the part above the CNC host
	//   P3  main window or a control in it does not use the font family of the UI language
	//   P4  a visible CNC panel at the size the main window gives it fails the T1 / T2 / T4 checks of LayoutRunner
	//   a dialog that cannot be opened is a problem, not a skip
	//
	// Usage: UICheckMainWindow.exe <language> <mode> <width> <height> <png> [dialog type name ...]
	// <mode> is the page clicked before the capture: CAD (start page, nothing clicked), CAM, or CNC (machining).
	// The first dialog is drawn into the screenshot at its centered location.
	internal static class MainWindowRunner
	{
		[STAThread]
		static int Main( string[] args )
		{
			if( args.Length < 5 ) {
				Console.WriteLine( "usage: UICheckMainWindow.exe <language> <mode> <width> <height> <png> [dialog ...]" );
				return 2;
			}
			string szLanguage = args[ 0 ];
			string szMenuName;
			if( MODE_MENU_MAP.TryGetValue( args[ 1 ], out szMenuName ) == false ) {
				Console.WriteLine( "unknown mode {0}, expected CAD / CAM / CNC", args[ 1 ] );
				return 2;
			}
			Size windowSize = new Size( int.Parse( args[ 2 ] ), int.Parse( args[ 3 ] ) );
			string szOutFile = Path.GetFullPath( args[ 4 ] );
			string[] dialogNameArray = new string[ args.Length - 5 ];
			Array.Copy( args, 5, dialogNameArray, 0, dialogNameArray.Length );
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );

			// same start-up as Program.Main; the language is set in the setting in memory only, the file is untouched,
			// then MyApp.Initialize runs again to apply it
			string szBinDir = AppDomain.CurrentDomain.BaseDirectory;
			m_App = Assembly.LoadFrom( Path.Combine( szBinDir, "MyCAM.exe" ) );
			m_Cnc = Assembly.LoadFrom( Path.Combine( szBinDir, "CncUI.dll" ) );
			Type myApp = m_App.GetType( "MyCAM.App.MyApp" );
			MethodInfo initialize = myApp.GetMethod( "Initialize" );
			initialize.Invoke( null, null );
			object setting = myApp.GetProperty( "AppSetting" ).GetValue( null );
			setting.GetType().GetProperty( "UILanguage" ).SetValue( setting, szLanguage );
			initialize.Invoke( null, null );
			m_szExpectedFamily = GetExpectedFamily( szLanguage );

			Form mainForm = (Form)Activator.CreateInstance( m_App.GetType( "MyCAM.StartupForm" ) );
			mainForm.Shown += ( sender, e ) => mainForm.BeginInvoke( new Action( () =>
			{
				m_nResult = Run( mainForm, szMenuName, windowSize, szOutFile, dialogNameArray );
				mainForm.Close();
			} ) );
			Application.Run( mainForm );
			return m_nResult;
		}

		static int Run( Form mainForm, string szMenuName, Size windowSize, string szOutFile, string[] dialogNameArray )
		{
			// Form clamps its size to the screen; raising MaximumSize lets the native resize go beyond it
			mainForm.WindowState = FormWindowState.Normal;
			mainForm.MaximumSize = windowSize;
			SetWindowPos( mainForm.Handle, IntPtr.Zero, 0, 0, windowSize.Width, windowSize.Height,
				SWP_NOZORDER | SWP_NOACTIVATE );
			Wait( 3000 );
			if( mainForm.Size != windowSize ) {
				Console.WriteLine( "FAIL  main window is {0}, expected {1}", mainForm.Size, windowSize );
				return 1;
			}

			// switch the page the way the user does, after resizing so the layout already matches the orientation
			if( szMenuName != null ) {
				ClickMenu( mainForm, szMenuName );
				Wait( 2000 );
			}
			bool isVertical = mainForm.Height > mainForm.Width;
			Rectangle area = DialogPlacement.AreaProvider();
			Console.WriteLine( "main window {0}x{1} ({2}), dialog area {3}", mainForm.Width, mainForm.Height,
				isVertical ? "portrait" : "landscape", area );
			int nProblemCount = CheckArea( mainForm, isVertical, area );
			nProblemCount += CheckFontFamily( mainForm );
			nProblemCount += CheckPanels( mainForm );

			using( Bitmap bitmap = Capture( mainForm.Handle, mainForm.Size ) ) {
				using( Graphics graphics = Graphics.FromImage( bitmap ) ) {
					DrawArea( graphics, mainForm, area );
					// only the first dialog is drawn into the screenshot, the others are checked only
					for( int i = 0; i < dialogNameArray.Length; i++ ) {
						nProblemCount += CheckDialog( mainForm, dialogNameArray[ i ], area, i == 0 ? graphics : null );
					}
				}
				bitmap.Save( szOutFile, ImageFormat.Png );
			}
			Console.WriteLine( "[capture] {0}", szOutFile );
			Console.WriteLine( "{0} problems", nProblemCount );
			return nProblemCount == 0 ? 0 : 1;
		}

		static int CheckArea( Form mainForm, bool isVertical, Rectangle area )
		{
			if( isVertical == false ) {
				if( area != mainForm.Bounds ) {
					Console.WriteLine( "FAIL  P2 landscape dialog area {0} is not the window {1}", area, mainForm.Bounds );
					return 1;
				}
				return 0;
			}
			Control verticalHost = FindControl( mainForm, "m_pnlCNCVertical" );
			int nHostTop = verticalHost.Parent.PointToScreen( verticalHost.Location ).Y;
			bool isCAMArea = area.Top == mainForm.Top && area.Bottom == nHostTop && area.Width == mainForm.Width;
			if( isCAMArea == false ) {
				Console.WriteLine( "FAIL  P2 portrait dialog area {0}, CNC host top {1}", area, nHostTop );
				return 1;
			}
			return 0;
		}

		static int CheckDialog( Form mainForm, string szName, Rectangle area, Graphics graphics )
		{
			Type type = FindType( szName );
			if( type == null ) {
				Console.WriteLine( "FAIL  {0}: type not found", szName );
				return 1;
			}
			Form dialog = null;
			try {
				dialog = (Form)LayoutRunner.CreateInstance( type );
				dialog.Show( mainForm );
				Wait( 800 );

				// the dialog may be pushed inside the screen working area, which only matters beyond the screen
				Point expected = new Point( area.Left + ( area.Width - dialog.Width ) / 2,
					area.Top + ( area.Height - dialog.Height ) / 2 );
				Rectangle workingArea = Screen.FromControl( mainForm ).WorkingArea;
				Rectangle expectedBounds = new Rectangle( expected, dialog.Size );
				bool isInsideScreen = workingArea.Contains( expectedBounds );
				int nProblem = 0;
				if( isInsideScreen && dialog.Location != expected ) {
					Console.WriteLine( "FAIL  P1 {0} at {1}, expected {2}", szName, dialog.Location, expected );
					nProblem = 1;
				}
				else {
					Console.WriteLine( "OK    {0} at {1}{2}", szName, dialog.Location,
						isInsideScreen ? "" : " (expected " + expected + " is outside the screen)" );
				}

				// draw the dialog where it would be relative to the main window
				if( graphics == null ) {
					return nProblem;
				}
				using( Bitmap dialogBitmap = Capture( dialog.Handle, dialog.Size ) ) {
					Point offset = new Point( expected.X - mainForm.Left, expected.Y - mainForm.Top );
					graphics.DrawImage( dialogBitmap, offset );
				}
				return nProblem;
			}
			catch( Exception ex ) {
				Exception inner = ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex;
				// a dialog that cannot be opened is not checked, so it counts as a problem
				Console.WriteLine( "FAIL  {0} cannot be opened: {1} {2}", szName, inner.GetType().Name,
					inner.Message.Split( '\n' )[ 0 ].Trim() );
				return 1;
			}
			finally {
				if( dialog != null && !dialog.IsDisposed ) {
					dialog.Close();
					dialog.Dispose();
				}
			}
		}

		// P3: every control of the main window uses the family of the UI language (the value font is Consolas)
		static int CheckFontFamily( Form mainForm )
		{
			int nProblemCount = 0;
			if( mainForm.Font.Name != m_szExpectedFamily ) {
				Console.WriteLine( "FAIL  P3 main window font {0}, expected {1}", mainForm.Font.Name, m_szExpectedFamily );
				nProblemCount++;
			}
			nProblemCount += CheckChildFontFamily( mainForm );
			return nProblemCount;
		}

		static int CheckChildFontFamily( Control parent )
		{
			int nProblemCount = 0;
			foreach( Control child in parent.Controls ) {
				string szFamily = child.Font.Name;
				bool isExpected = szFamily == m_szExpectedFamily || szFamily == VALUE_FAMILY;
				if( isExpected == false ) {
					Console.WriteLine( "FAIL  P3 {0} ({1}) font {2}, expected {3}", child.Name, child.GetType().Name,
						szFamily, m_szExpectedFamily );
					nProblemCount++;

					// the children inherit the same font, report the topmost one only
					continue;
				}
				nProblemCount += CheckChildFontFamily( child );
			}
			return nProblemCount;
		}

		// P4: the embedded panels at the size the main window gives them, not the design size LayoutRunner uses
		static int CheckPanels( Form mainForm )
		{
			List<Control> panelList = new List<Control>();
			FindPanels( mainForm, panelList );
			int nProblemCount = 0;
			foreach( Control panel in panelList ) {
				List<string> problemList = new List<string>();
				LayoutRunner.CheckChildren( panel, problemList );
				Console.WriteLine( "panel {0} {1}x{2}, {3} problems", panel.GetType().Name, panel.Width, panel.Height,
					problemList.Count );
				foreach( string szProblem in problemList ) {
					Console.WriteLine( "FAIL  P4 {0}: {1}", panel.GetType().Name, szProblem );
				}
				nProblemCount += problemList.Count;
			}
			return nProblemCount;
		}

		static void FindPanels( Control parent, List<Control> panelList )
		{
			foreach( Control child in parent.Controls ) {
				if( child.Visible == false ) {
					continue;
				}
				if( PANEL_TYPE_LIST.Contains( child.GetType().Name ) ) {
					panelList.Add( child );
					continue;
				}
				FindPanels( child, panelList );
			}
		}

		static readonly HashSet<string> PANEL_TYPE_LIST = new HashSet<string>
		{
			"CNCPanel",
			"CNCInfoPanel",
			"CNCAlarmPanel",
		};

		// independent of ThemeFonts on purpose, so a wrong initialization order is caught
		static string GetExpectedFamily( string szLanguage )
		{
			if( string.Equals( szLanguage, "zh-TW", StringComparison.OrdinalIgnoreCase ) ) {
				return "Microsoft JhengHei UI";
			}
			else if( string.Equals( szLanguage, "zh-CN", StringComparison.OrdinalIgnoreCase ) ) {
				return "Microsoft YaHei UI";
			}
			else {
				return "Segoe UI";
			}
		}

		static void DrawArea( Graphics graphics, Form mainForm, Rectangle area )
		{
			// dashed outline of the area dialogs are centered in
			using( Pen pen = new Pen( Color.FromArgb( 196, 43, 28 ), 3 ) ) {
				pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
				Rectangle local = new Rectangle( area.Left - mainForm.Left, area.Top - mainForm.Top, area.Width, area.Height );
				local.Inflate( -2, -2 );
				graphics.DrawRectangle( pen, local );
			}
		}

		static Bitmap Capture( IntPtr handle, Size size )
		{
			Bitmap bitmap = new Bitmap( size.Width, size.Height );
			using( Graphics graphics = Graphics.FromImage( bitmap ) ) {
				IntPtr hdc = graphics.GetHdc();

				// PW_RENDERFULLCONTENT also renders the 3D view and the part outside the screen
				PrintWindow( handle, hdc, 2 );
				graphics.ReleaseHdc( hdc );
			}
			return bitmap;
		}

		static Type FindType( string szName )
		{
			foreach( Assembly assembly in new[] { m_App, m_Cnc } ) {
				foreach( Type type in assembly.GetTypes() ) {
					if( type.Name == szName ) {
						return type;
					}
				}
			}
			return null;
		}

		static void ClickMenu( Form mainForm, string szMenuName )
		{
			FieldInfo field = mainForm.GetType().GetField( szMenuName, BindingFlags.Instance | BindingFlags.NonPublic );
			if( field == null ) {
				throw new InvalidOperationException( szMenuName + " not found" );
			}
			( (ToolStripItem)field.GetValue( mainForm ) ).PerformClick();
		}

		static Control FindControl( Control parent, string szName )
		{
			Control[] foundArray = parent.Controls.Find( szName, true );
			if( foundArray.Length == 0 ) {
				throw new InvalidOperationException( szName + " not found" );
			}
			return foundArray[ 0 ];
		}

		static void Wait( int nMilliseconds )
		{
			DateTime end = DateTime.Now.AddMilliseconds( nMilliseconds );
			while( DateTime.Now < end ) {
				Application.DoEvents();
				System.Threading.Thread.Sleep( 20 );
			}
		}

		[DllImport( "user32.dll" )]
		static extern bool PrintWindow( IntPtr hWnd, IntPtr hdc, uint nFlags );

		[DllImport( "user32.dll" )]
		static extern bool SetWindowPos( IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags );

		const uint SWP_NOZORDER = 0x0004;
		const uint SWP_NOACTIVATE = 0x0010;

		// menu item clicked for each mode; null is the start page
		static readonly Dictionary<string, string> MODE_MENU_MAP =
			new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
		{
			{ "CAD", null },
			{ "CAM", "m_tsmiCAM" },
			{ "CNC", "m_tsmiCNC" },
		};

		const string VALUE_FAMILY = "Consolas";

		static string m_szExpectedFamily;
		static Assembly m_App;
		static Assembly m_Cnc;
		static int m_nResult = 2;
	}
}
