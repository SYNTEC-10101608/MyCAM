using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CommonUI.Layout;

namespace UICheck
{
	// Main window screenshot, dialog placement and main window layout check.
	// Compiled by CaptureMainWindow.ps1 into the build output folder together with LayoutRunner.cs.
	//
	// A width and height of 0 keep the window maximized as the program shows it; CaptureMainWindow.ps1 -Screen runs it
	// so on the specified screen (VirtualDisplayRun.cs), and the picture is the part the screen shows. Other sizes are
	// set on the window: landscape / portrait only depends on its aspect ratio, so a portrait window larger than a
	// landscape screen is checked by sizing the window beyond the screen (the part outside is still rendered).
	//
	//   DialogCenter        dialog not centered in DialogPlacement's area (whole window in landscape, CAM area in
	//                       portrait); "modal:<name>" opens the dialog with ShowDialog, as the program does for it
	//   PortraitCamArea     portrait CAM area is not the part above the CNC host
	//   MainFontFamily      main window or a control in it does not use the font family of the UI language
	//   PanelInWindow       a visible CNC panel at the size the main window gives it fails the TextClipped /
	//                       TextOverflow / ControlOverlap checks of LayoutRunner
	//   MenuLanguageText    a menu / tool bar item whose name is a language ID does not show its text (tool tip:
	//                       "<name>_ToolTip")
	//   MainDesignerRegion  with "designer:<StartupForm.Designer.cs>", the window gets the client size of the designer
	//                       and a region (REGION_NAME_LIST) is not placed where the designer shows it
	//   a dialog that cannot be opened is a problem, not a skip
	//
	// Usage: UICheckMainWindow.exe <language> <mode> <width> <height> <png> [dialog type name ...]
	// [region] lines print the bounds of the regions MainLayout places, for comparing layouts.
	// <mode> is the page clicked before the capture: CAD (start page, nothing clicked), CAM, or CNC (machining).
	// The first dialog is drawn into the screenshot at its centered location.
	internal static class MainWindowRunner
	{
		[STAThread]
		static int Main( string[] args )
		{
			if( args.Length < 5 ) {
				Console.WriteLine(
					"usage: UICheckMainWindow.exe <language> <mode> <width> <height> <png> [dialog ...]" );
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
			List<string> dialogNameList = new List<string>();
			for( int i = 5; i < args.Length; i++ ) {
				if( args[ i ].StartsWith( DESIGNER_PREFIX, StringComparison.Ordinal ) ) {
					m_szDesignerFile = args[ i ].Substring( DESIGNER_PREFIX.Length );
					continue;
				}
				dialogNameList.Add( args[ i ] );
			}
			string[] dialogNameArray = dialogNameList.ToArray();
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
			m_szLanguage = szLanguage;

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
			// MainDesignerRegion runs with the client size of the designer, so the frame is added to it
			Dictionary<string, Rectangle> designerMap = null;
			if( m_szDesignerFile != null ) {
				Size designerClient;
				designerMap = ReadDesignerBounds( m_szDesignerFile, out designerClient );
				mainForm.WindowState = FormWindowState.Normal;
				Size frameSize = mainForm.Size - mainForm.ClientSize;
				windowSize = designerClient + frameSize;
			}

			// the screen VirtualDisplayRun prepared; the window is moved there before it is maximized or sized
			Screen targetScreen;
			if( LayoutRunner.TryFindTargetScreen( out targetScreen ) == false ) {
				return 2;
			}
			m_TargetScreen = targetScreen;
			if( targetScreen != null ) {
				mainForm.WindowState = FormWindowState.Normal;
				SetWindowPos( mainForm.Handle, IntPtr.Zero, targetScreen.Bounds.X, targetScreen.Bounds.Y, 0, 0,
					SWP_NOZORDER | SWP_NOACTIVATE | SWP_NOSIZE );
				Wait( 500 );
			}

			// no size given: the window stays maximized as the program shows it, on the screen it was opened on
			bool isMaximized = windowSize.IsEmpty;
			if( isMaximized ) {
				mainForm.WindowState = FormWindowState.Maximized;
				Wait( 3000 );
				Screen screen = Screen.FromControl( mainForm );
				Console.WriteLine( "main window maximized on {0} {1}x{2}, work area {3}x{4}, client {5}x{6}",
					screen.DeviceName, screen.Bounds.Width, screen.Bounds.Height, screen.WorkingArea.Width,
					screen.WorkingArea.Height, mainForm.ClientSize.Width, mainForm.ClientSize.Height );
			}
			else {
				// Form clamps its size to the screen; raising MaximumSize lets the native resize go beyond it
				mainForm.WindowState = FormWindowState.Normal;
				mainForm.MaximumSize = windowSize;
				Point origin = targetScreen != null ? targetScreen.Bounds.Location : Point.Empty;
				SetWindowPos( mainForm.Handle, IntPtr.Zero, origin.X, origin.Y, windowSize.Width, windowSize.Height,
					SWP_NOZORDER | SWP_NOACTIVATE );
				Wait( 3000 );
				if( mainForm.Size != windowSize ) {
					Console.WriteLine( "FAIL  main window is {0}, expected {1}", mainForm.Size, windowSize );
					return 1;
				}
			}
			string szScreenProblem = LayoutRunner.CheckOnTargetScreen( mainForm, targetScreen );
			if( szScreenProblem != null ) {
				Console.WriteLine( "FAIL  main window {0}", szScreenProblem );
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
			PrintRegions( mainForm );
			int nProblemCount = CheckArea( mainForm, isVertical, area );
			if( designerMap != null ) {
				nProblemCount += CheckDesignerRegions( mainForm, designerMap );
			}
			nProblemCount += CheckFontFamily( mainForm );
			nProblemCount += CheckPanels( mainForm );
			nProblemCount += CheckItemTexts( mainForm );

			using( Bitmap bitmap = Capture( mainForm.Handle, mainForm.Size ) ) {
				using( Graphics graphics = Graphics.FromImage( bitmap ) ) {
					DrawArea( graphics, mainForm, area );
					// only the first dialog is drawn into the screenshot, the others are checked only
					for( int i = 0; i < dialogNameArray.Length; i++ ) {
						nProblemCount += CheckDialog( mainForm, dialogNameArray[ i ], area, i == 0 ? graphics : null );
					}
				}
				// a maximized frame lies partly outside the screen; the picture is what the screen shows
				if( isMaximized ) {
					Rectangle visible = Rectangle.Intersect( mainForm.Bounds, Screen.FromControl( mainForm ).Bounds );
					visible.Offset( -mainForm.Left, -mainForm.Top );
					using( Bitmap screenPart = bitmap.Clone( visible, bitmap.PixelFormat ) ) {
						screenPart.Save( szOutFile, ImageFormat.Png );
					}
				}
				else {
					bitmap.Save( szOutFile, ImageFormat.Png );
				}
			}
			Console.WriteLine( "[capture] {0}", szOutFile );
			Console.WriteLine( "{0} problems", nProblemCount );
			return nProblemCount == 0 ? 0 : 1;
		}

		// the regions MainLayout places, in client coordinates of the main window, so a layout change can be compared
		static void PrintRegions( Form mainForm )
		{
			Console.WriteLine( "[region] client {0}x{1}", mainForm.ClientSize.Width, mainForm.ClientSize.Height );
			foreach( string szName in REGION_NAME_LIST ) {
				Control[] foundArray = mainForm.Controls.Find( szName, true );
				if( foundArray.Length == 0 ) {
					Console.WriteLine( "[region] {0} not found", szName );
					continue;
				}
				PrintRegion( mainForm, szName, foundArray[ 0 ] );
			}
			List<Control> panelList = new List<Control>();
			FindPanels( mainForm, panelList );
			foreach( Control panel in panelList ) {
				PrintRegion( mainForm, panel.GetType().Name, panel );
			}
		}

		static void PrintRegion( Form mainForm, string szName, Control control )
		{
			if( control.Visible == false ) {
				Console.WriteLine( "[region] {0} hidden", szName );
				return;
			}
			Rectangle bounds = mainForm.RectangleToClient( control.RectangleToScreen( control.ClientRectangle ) );
			Console.WriteLine( "[region] {0} {1},{2},{3},{4}", szName, bounds.X, bounds.Y, bounds.Width,
				bounds.Height );
		}

		// MainDesignerRegion: with the client size and page of the designer, MainLayout places each region where the
		// designer shows it, so the designer is what the user sees
		static int CheckDesignerRegions( Form mainForm, Dictionary<string, Rectangle> designerMap )
		{
			int nProblemCount = 0;
			foreach( string szName in REGION_NAME_LIST ) {
				Rectangle designerBounds;
				if( designerMap.TryGetValue( szName, out designerBounds ) == false ) {
					Console.WriteLine( "FAIL  MainDesignerRegion {0} has no Location / Size in the designer", szName );
					nProblemCount++;
					continue;
				}
				Rectangle bounds = FindControl( mainForm, szName ).Bounds;
				if( bounds != designerBounds ) {
					Console.WriteLine( "FAIL  MainDesignerRegion {0} is placed at {1}, the designer has {2}; " +
						"write Location ( {3}, {4} ) Size ( {5}, {6} )", szName, bounds, designerBounds, bounds.X,
						bounds.Y, bounds.Width, bounds.Height );
					nProblemCount++;
				}
			}
			Console.WriteLine( "MainDesignerRegion {0} regions compared with the designer, {1} problems",
				REGION_NAME_LIST.Length, nProblemCount );
			return nProblemCount;
		}

		// Location / Size of every control in the designer file, and the client size of the form
		static Dictionary<string, Rectangle> ReadDesignerBounds( string szPath, out Size clientSize )
		{
			Dictionary<string, Point> locationMap = new Dictionary<string, Point>();
			Dictionary<string, Size> sizeMap = new Dictionary<string, Size>();
			clientSize = Size.Empty;
			System.Text.RegularExpressions.Regex pairRegex = new System.Text.RegularExpressions.Regex(
				@"^\s*this\.(?:(\w+)\.)?(Location|Size|ClientSize) = new System\.Drawing\.(?:Point|Size)" +
				@"\(\s*(-?\d+),\s*(-?\d+)\s*\);" );
			foreach( string szLine in File.ReadAllLines( szPath ) ) {
				System.Text.RegularExpressions.Match match = pairRegex.Match( szLine );
				if( match.Success == false ) {
					continue;
				}
				string szName = match.Groups[ 1 ].Value;
				string szProperty = match.Groups[ 2 ].Value;
				int nX = int.Parse( match.Groups[ 3 ].Value );
				int nY = int.Parse( match.Groups[ 4 ].Value );
				if( szName == "" && szProperty == "ClientSize" ) {
					clientSize = new Size( nX, nY );
				}
				else if( szProperty == "Location" ) {
					locationMap[ szName ] = new Point( nX, nY );
				}
				else if( szProperty == "Size" ) {
					sizeMap[ szName ] = new Size( nX, nY );
				}
			}
			Dictionary<string, Rectangle> boundsMap = new Dictionary<string, Rectangle>();
			foreach( KeyValuePair<string, Size> pair in sizeMap ) {
				Point location;
				locationMap.TryGetValue( pair.Key, out location );
				boundsMap[ pair.Key ] = new Rectangle( location, pair.Value );
			}
			return boundsMap;
		}

		const string DESIGNER_PREFIX = "designer:";

		// host panels of the main window that MainLayout sizes
		static readonly string[] REGION_NAME_LIST =
		{
			"m_pnlBackGround", "m_pnlTreeView", "m_pnlViewer", "m_pnlCNC", "m_pnlBottom", "m_pnlMessages", "m_pnlLog",
			"m_pnlCNCInfo", "m_pnlCNCVertical", "m_pnlVerticalAlarm",
		};

		static int CheckArea( Form mainForm, bool isVertical, Rectangle area )
		{
			if( isVertical == false ) {
				if( area != mainForm.Bounds ) {
					Console.WriteLine( "FAIL  PortraitCamArea landscape dialog area {0} is not the window {1}", area,
						mainForm.Bounds );
					return 1;
				}
				return 0;
			}
			Control verticalHost = FindControl( mainForm, "m_pnlCNCVertical" );
			int nHostTop = verticalHost.Parent.PointToScreen( verticalHost.Location ).Y;
			bool isCAMArea = area.Top == mainForm.Top && area.Bottom == nHostTop && area.Width == mainForm.Width;
			if( isCAMArea == false ) {
				Console.WriteLine( "FAIL  PortraitCamArea portrait dialog area {0}, CNC host top {1}", area, nHostTop );
				return 1;
			}
			return 0;
		}

		static int CheckDialog( Form mainForm, string szName, Rectangle area, Graphics graphics )
		{
			if( szName.StartsWith( MODAL_PREFIX, StringComparison.Ordinal ) ) {
				return CheckModalDialog( mainForm, szName.Substring( MODAL_PREFIX.Length ), area );
			}
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

				// on the specified screen, whether the centering below is compared or not
				int nProblem = 0;
				string szScreenProblem = LayoutRunner.CheckOnTargetScreen( dialog, m_TargetScreen );
				if( szScreenProblem != null ) {
					Console.WriteLine( "FAIL  {0} {1}", szName, szScreenProblem );
					nProblem++;
				}

				// the dialog may be pushed inside the screen working area, which only matters beyond the screen
				Point expected = new Point( area.Left + ( area.Width - dialog.Width ) / 2,
					area.Top + ( area.Height - dialog.Height ) / 2 );
				Rectangle workingArea = Screen.FromControl( mainForm ).WorkingArea;
				Rectangle expectedBounds = new Rectangle( expected, dialog.Size );
				bool isInsideScreen = workingArea.Contains( expectedBounds );
				if( isInsideScreen && dialog.Location != expected ) {
					Console.WriteLine( "FAIL  DialogCenter {0} at {1}, expected {2}",
						szName, dialog.Location, expected );
					nProblem++;
				}
				else if( nProblem == 0 ) {
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

		// DialogCenter for a dialog the program opens with ShowDialog: the location is read when it is shown, then it
		// is closed
		static int CheckModalDialog( Form mainForm, string szName, Rectangle area )
		{
			Type type = FindType( szName );
			if( type == null ) {
				Console.WriteLine( "FAIL  {0}: type not found", szName );
				return 1;
			}
			Form dialog = null;
			try {
				dialog = (Form)LayoutRunner.CreateInstance( type );
				Point shownLocation = Point.Empty;
				string szScreenProblem = null;
				Form shownDialog = dialog;
				shownDialog.Shown += ( sender, e ) =>
				{
					shownLocation = shownDialog.Location;
					szScreenProblem = LayoutRunner.CheckOnTargetScreen( shownDialog, m_TargetScreen );
					shownDialog.BeginInvoke( new Action( shownDialog.Close ) );
				};
				dialog.ShowDialog( mainForm );

				// on the specified screen, whether the centering below is compared or not
				int nProblem = 0;
				if( szScreenProblem != null ) {
					Console.WriteLine( "FAIL  {0} (ShowDialog) {1}", szName, szScreenProblem );
					nProblem++;
				}
				Point expected = new Point( area.Left + ( area.Width - dialog.Width ) / 2,
					area.Top + ( area.Height - dialog.Height ) / 2 );
				Rectangle workingArea = Screen.FromControl( mainForm ).WorkingArea;
				bool isInsideScreen = workingArea.Contains( new Rectangle( expected, dialog.Size ) );
				if( isInsideScreen && shownLocation != expected ) {
					Console.WriteLine( "FAIL  DialogCenter {0} (ShowDialog) at {1}, expected {2}",
						szName, shownLocation, expected );
					return nProblem + 1;
				}
				if( nProblem == 0 ) {
					Console.WriteLine( "OK    {0} (ShowDialog) at {1}{2}", szName, shownLocation,
						isInsideScreen ? "" : " (expected " + expected + " is outside the screen)" );
				}
				return nProblem;
			}
			catch( Exception ex ) {
				Exception inner = ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex;
				Console.WriteLine( "FAIL  {0} cannot be opened: {1} {2}", szName, inner.GetType().Name,
					inner.Message.Split( '\n' )[ 0 ].Trim() );
				return 1;
			}
			finally {
				if( dialog != null && !dialog.IsDisposed ) {
					dialog.Dispose();
				}
			}
		}

		const string MODAL_PREFIX = "modal:";

		// MainFontFamily: every control of the main window uses the family of the UI language (the value font is
		// Consolas)
		static int CheckFontFamily( Form mainForm )
		{
			int nProblemCount = 0;
			if( mainForm.Font.Name != m_szExpectedFamily ) {
				Console.WriteLine( "FAIL  MainFontFamily main window font {0}, expected {1}", mainForm.Font.Name,
					m_szExpectedFamily );
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
					Console.WriteLine( "FAIL  MainFontFamily {0} ({1}) font {2}, expected {3}",
						child.Name, child.GetType().Name, szFamily, m_szExpectedFamily );
					nProblemCount++;

					// the children inherit the same font, report the topmost one only
					continue;
				}
				nProblemCount += CheckChildFontFamily( child );
			}
			return nProblemCount;
		}

		// MenuLanguageText: each menu / tool bar item whose name is an ID shows the text of the language file, its tool
		// tip the "<name>_ToolTip" text (the main window applies them by name like the dialogs)
		static int CheckItemTexts( Form mainForm )
		{
			string szPath = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "Resource", "Language",
				"StartupForm_" + m_szLanguage + ".xml" );
			if( File.Exists( szPath ) == false ) {
				Console.WriteLine( "FAIL  MenuLanguageText language file not found: {0}", szPath );
				return 1;
			}
			Dictionary<string, string> textMap = new Dictionary<string, string>();
			System.Xml.XmlDocument document = new System.Xml.XmlDocument();
			document.Load( szPath );
			foreach( System.Xml.XmlElement message in document.GetElementsByTagName( "Message" ) ) {
				textMap[ message.GetAttribute( "ID" ) ] = message.GetAttribute( "Content" );
			}
			List<ToolStripItem> itemList = new List<ToolStripItem>();
			CollectItems( mainForm, itemList );
			int nProblemCount = 0;
			foreach( ToolStripItem item in itemList ) {
				string szText;
				if( textMap.TryGetValue( item.Name, out szText ) && item.Text != szText ) {
					Console.WriteLine( "FAIL  MenuLanguageText {0} shows '{1}', language file has '{2}'",
						item.Name, item.Text, szText );
					nProblemCount++;
				}
				if( textMap.TryGetValue( item.Name + "_ToolTip", out szText ) && item.ToolTipText != szText ) {
					Console.WriteLine( "FAIL  MenuLanguageText {0} tool tip '{1}', language file has '{2}'", item.Name,
						item.ToolTipText, szText );
					nProblemCount++;
				}
			}
			Console.WriteLine( "items {0} checked against {1}", itemList.Count, Path.GetFileName( szPath ) );
			return nProblemCount;
		}

		static void CollectItems( Control parent, List<ToolStripItem> itemList )
		{
			foreach( Control child in parent.Controls ) {
				ToolStrip toolStrip = child as ToolStrip;
				if( toolStrip != null ) {
					CollectItems( toolStrip.Items, itemList );
				}
				CollectItems( child, itemList );
			}
		}

		static void CollectItems( ToolStripItemCollection itemCollection, List<ToolStripItem> itemList )
		{
			foreach( ToolStripItem item in itemCollection ) {
				itemList.Add( item );
				ToolStripDropDownItem dropDownItem = item as ToolStripDropDownItem;
				if( dropDownItem != null && dropDownItem.HasDropDownItems ) {
					CollectItems( dropDownItem.DropDownItems, itemList );
				}
			}
		}

		// PanelInWindow: the embedded panels at the size the main window gives them, not the design size LayoutRunner
		// uses
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
					Console.WriteLine( "FAIL  PanelInWindow {0}: {1}", panel.GetType().Name, szProblem );
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
				Rectangle local = new Rectangle( area.Left - mainForm.Left, area.Top - mainForm.Top, area.Width,
					area.Height );
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
		static extern bool SetWindowPos( IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy,
			uint uFlags );

		const uint SWP_NOZORDER = 0x0004;
		const uint SWP_NOACTIVATE = 0x0010;
		const uint SWP_NOSIZE = 0x0001;

		// menu item clicked for each mode; null is the start page
		static readonly Dictionary<string, string> MODE_MENU_MAP =
			new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
		{
			{ "CAD", null },
			{ "CAM", "m_tsmiCAM" },
			{ "CNC", "m_tsmiCNC" },
		};

		const string VALUE_FAMILY = "Consolas";

		static string m_szDesignerFile;

		// the screen VirtualDisplayRun prepared, null without it
		static Screen m_TargetScreen;
		static string m_szExpectedFamily;
		static string m_szLanguage;
		static Assembly m_App;
		static Assembly m_Cnc;
		static int m_nResult = 2;
	}
}
