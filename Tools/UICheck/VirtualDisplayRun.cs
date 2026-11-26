using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace UICheck
{
	// Runs a command line while the primary screen is the specified screen of the product: 1920 x 1080 (landscape) or
	// 1080 x 1920 (portrait) at 100 % scaling (96 DPI), so the main window is maximized, dialogs are centered and
	// clamped on that screen as on the target machine. Compiled by the UICheck scripts into the build output folder;
	// stdout / stderr are inherited and the exit code is passed through.
	//
	// A primary screen that already is the specified screen is used as it is. Otherwise a virtual screen of the
	// Parsec Virtual Display Driver (winget install Parsec.ParsecVDD, administrator rights once) is added for the run:
	// it is set to the specified mode and 100 % scaling and made the primary screen, so it gets the task bar; the
	// other screens are moved to its right. Afterwards the screens are put back and the virtual screen is removed.
	// The driver removes its screens about a second after this process stops pinging it, so a killed run does not
	// leave the virtual screen behind; Windows then makes a real screen primary again.
	//
	// Usage: UICheckDisplay.exe <Landscape|Portrait> <exe> [args ...]
	// Exit code 2 when the screen cannot be prepared.
	internal static class VirtualDisplayRun
	{
		static int Main( string[] args )
		{
			if( args.Length < 2 ) {
				Console.Error.WriteLine( "usage: UICheckDisplay.exe <Landscape|Portrait> <exe> [args ...]" );
				return 2;
			}
			bool isPortrait;
			if( string.Equals( args[ 0 ], "Portrait", StringComparison.OrdinalIgnoreCase ) ) {
				isPortrait = true;
			}
			else if( string.Equals( args[ 0 ], "Landscape", StringComparison.OrdinalIgnoreCase ) ) {
				isPortrait = false;
			}
			else {
				Console.Error.WriteLine( "[display] unknown orientation " + args[ 0 ] +
					", expected Landscape / Portrait" );
				return 2;
			}
			string[] commandArray = new string[ args.Length - 1 ];
			Array.Copy( args, 1, commandArray, 0, commandArray.Length );

			// monitor sizes and DPI are only reported unscaled to a per-monitor DPI aware process
			SetProcessDpiAwarenessContext( new IntPtr( DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 ) );
			int nWidth = isPortrait ? SPEC_SHORT_SIDE : SPEC_LONG_SIDE;
			int nHeight = isPortrait ? SPEC_LONG_SIDE : SPEC_SHORT_SIDE;
			List<DisplayState> originalList = GetAttachedDisplays();
			DisplayState primary = originalList.Find( display => display.IsPrimary );
			if( primary != null && IsSpecScreen( primary.DeviceName, nWidth, nHeight ) ) {
				PrintScreen( "primary screen", primary.DeviceName );
				return RunCommand( commandArray );
			}
			return RunOnVirtualScreen( commandArray, nWidth, nHeight, isPortrait, originalList );
		}

		static int RunOnVirtualScreen( string[] commandArray, int nWidth, int nHeight, bool isPortrait,
			List<DisplayState> originalList )
		{
			IntPtr hDevice = OpenParsecDevice();

			// the driver is not installed (or not running)
			if( hDevice == INVALID_HANDLE_VALUE ) {
				Console.Error.WriteLine( "[display] the primary screen is not " + nWidth + "x" + nHeight +
					" at 96 DPI and the Parsec Virtual Display Driver is not available;" );
				Console.Error.WriteLine( "[display] install it once with: winget install Parsec.ParsecVDD" );
				return 2;
			}
			m_hDevice = hDevice;
			Thread keepAlive = new Thread( KeepAlive );
			keepAlive.IsBackground = true;
			keepAlive.Start();
			int nDisplayIndex = -1;
			string szDevice = null;
			try {
				nDisplayIndex = IoControl( IOCTL_ADD, null );
				IoControl( IOCTL_UPDATE, null );

				// the driver could not add a screen
				if( nDisplayIndex < 0 ) {
					Console.Error.WriteLine( "[display] the virtual display driver did not add a screen" );
					return 2;
				}
				szDevice = WaitForNewDisplay( originalList );

				// Windows did not attach the new screen to the desktop
				if( szDevice == null ) {
					Console.Error.WriteLine( "[display] the virtual screen did not appear" );
					return 2;
				}
				string szError = MakeSpecPrimary( szDevice, nWidth, nHeight, isPortrait, originalList );
				if( szError == null ) {
					szError = SetScale100( szDevice );
				}

				// the task bar moves to the new primary screen and the work area follows it
				Thread.Sleep( SETTLE_TIME_MS );
				if( szError == null && IsSpecScreen( szDevice, nWidth, nHeight ) == false ) {
					szError = "the virtual screen is not " + nWidth + "x" + nHeight + " at 96 DPI";
				}

				// the screen could not be prepared
				if( szError != null ) {
					Console.Error.WriteLine( "[display] " + szError );
					PrintScreen( "virtual screen", szDevice );
					return 2;
				}
				PrintScreen( "virtual screen", szDevice );
				return RunCommand( commandArray );
			}
			finally {
				if( szDevice != null ) {
					RestoreDisplays( szDevice, originalList );
				}
				if( nDisplayIndex >= 0 ) {
					// the index is passed as a 16 bit big-endian number
					byte[] indexArray = { (byte)( ( nDisplayIndex >> 8 ) & 0xFF ), (byte)( nDisplayIndex & 0xFF ) };
					IoControl( IOCTL_REMOVE, indexArray );
					IoControl( IOCTL_UPDATE, null );
				}
				m_isStopping = true;
				keepAlive.Join();
				CloseHandle( hDevice );
			}
		}

		static int RunCommand( string[] commandArray )
		{
			ProcessStartInfo startInfo = new ProcessStartInfo( commandArray[ 0 ], BuildArguments( commandArray ) );
			startInfo.UseShellExecute = false;
			using( Process process = Process.Start( startInfo ) ) {
				process.WaitForExit();
				return process.ExitCode;
			}
		}

		static string BuildArguments( string[] commandArray )
		{
			StringBuilder argumentText = new StringBuilder();
			for( int i = 1; i < commandArray.Length; i++ ) {
				if( argumentText.Length > 0 ) {
					argumentText.Append( ' ' );
				}
				argumentText.Append( '"' ).Append( commandArray[ i ].Replace( "\"", "\\\"" ) ).Append( '"' );
			}
			return argumentText.ToString();
		}

		#region Screens

		sealed class DisplayState
		{
			public string DeviceName;
			public bool IsPrimary;
			public int X;
			public int Y;
		}

		static List<DisplayState> GetAttachedDisplays()
		{
			List<DisplayState> displayList = new List<DisplayState>();
			DISPLAY_DEVICE device = new DISPLAY_DEVICE();
			device.cb = Marshal.SizeOf( typeof( DISPLAY_DEVICE ) );
			for( uint i = 0; EnumDisplayDevices( null, i, ref device, 0 ); i++ ) {
				bool isAttached = ( device.StateFlags & DISPLAY_DEVICE_ATTACHED_TO_DESKTOP ) != 0;
				DEVMODE mode = NewDevMode();
				if( isAttached && EnumDisplaySettings( device.DeviceName, ENUM_CURRENT_SETTINGS, ref mode ) ) {
					DisplayState display = new DisplayState();
					display.DeviceName = device.DeviceName;
					display.IsPrimary = ( device.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE ) != 0;
					display.X = mode.dmPositionX;
					display.Y = mode.dmPositionY;
					displayList.Add( display );
				}
				device.cb = Marshal.SizeOf( typeof( DISPLAY_DEVICE ) );
			}
			return displayList;
		}

		static string WaitForNewDisplay( List<DisplayState> originalList )
		{
			for( int nWaited = 0; nWaited < APPEAR_TIMEOUT_MS; nWaited += POLL_INTERVAL_MS ) {
				foreach( DisplayState display in GetAttachedDisplays() ) {
					bool isNew = originalList.Find( original => original.DeviceName == display.DeviceName ) == null;
					if( isNew ) {
						return display.DeviceName;
					}
				}
				Thread.Sleep( POLL_INTERVAL_MS );
			}
			return null;
		}

		// the virtual screen at (0, 0) as primary, the original screens keep their arrangement to its right
		static string MakeSpecPrimary( string szDevice, int nWidth, int nHeight, bool isPortrait,
			List<DisplayState> originalList )
		{
			DEVMODE mode = NewDevMode();
			EnumDisplaySettings( szDevice, ENUM_CURRENT_SETTINGS, ref mode );
			mode.dmFields = DM_POSITION | DM_PELSWIDTH | DM_PELSHEIGHT | DM_DISPLAYORIENTATION;
			mode.dmPositionX = 0;
			mode.dmPositionY = 0;
			mode.dmPelsWidth = nWidth;
			mode.dmPelsHeight = nHeight;

			// portrait is the landscape mode rotated, as on a rotated monitor
			mode.dmDisplayOrientation = isPortrait ? DMDO_90 : DMDO_DEFAULT;
			int nResult = ChangeDisplaySettingsEx( szDevice, ref mode, IntPtr.Zero,
				CDS_UPDATEREGISTRY | CDS_NORESET | CDS_SET_PRIMARY, IntPtr.Zero );

			// the driver has no rotation for this mode: ask for the portrait mode itself
			if( nResult != DISP_CHANGE_SUCCESSFUL && isPortrait ) {
				mode.dmDisplayOrientation = DMDO_DEFAULT;
				nResult = ChangeDisplaySettingsEx( szDevice, ref mode, IntPtr.Zero,
					CDS_UPDATEREGISTRY | CDS_NORESET | CDS_SET_PRIMARY, IntPtr.Zero );
			}
			if( nResult != DISP_CHANGE_SUCCESSFUL ) {
				return "cannot set the virtual screen to " + nWidth + "x" + nHeight + " (" + nResult + ")";
			}
			int nMinX = int.MaxValue;
			int nMinY = int.MaxValue;
			foreach( DisplayState display in originalList ) {
				nMinX = Math.Min( nMinX, display.X );
				nMinY = Math.Min( nMinY, display.Y );
			}
			foreach( DisplayState display in originalList ) {
				nResult = MoveDisplay( display.DeviceName, nWidth + display.X - nMinX, display.Y - nMinY, false );
				if( nResult != DISP_CHANGE_SUCCESSFUL ) {
					return "cannot move " + display.DeviceName + " (" + nResult + ")";
				}
			}
			nResult = ChangeDisplaySettingsEx( null, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero );
			return nResult == DISP_CHANGE_SUCCESSFUL ? null : "cannot apply the screen layout (" + nResult + ")";
		}

		// the original primary screen back at (0, 0), the virtual screen right of all others until it is removed
		static void RestoreDisplays( string szDevice, List<DisplayState> originalList )
		{
			int nRight = 0;
			foreach( DisplayState display in originalList ) {
				DEVMODE mode = NewDevMode();
				EnumDisplaySettings( display.DeviceName, ENUM_CURRENT_SETTINGS, ref mode );
				nRight = Math.Max( nRight, display.X + mode.dmPelsWidth );
				MoveDisplay( display.DeviceName, display.X, display.Y, display.IsPrimary );
			}
			MoveDisplay( szDevice, nRight, 0, false );
			int nResult = ChangeDisplaySettingsEx( null, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero );
			if( nResult != DISP_CHANGE_SUCCESSFUL ) {
				Console.Error.WriteLine( "[display] cannot restore the screen layout (" + nResult + ")" );
			}
		}

		static int MoveDisplay( string szDevice, int nX, int nY, bool isPrimary )
		{
			DEVMODE mode = NewDevMode();
			mode.dmFields = DM_POSITION;
			mode.dmPositionX = nX;
			mode.dmPositionY = nY;
			uint nFlags = CDS_UPDATEREGISTRY | CDS_NORESET | ( isPrimary ? CDS_SET_PRIMARY : 0 );
			return ChangeDisplaySettingsEx( szDevice, ref mode, IntPtr.Zero, nFlags, IntPtr.Zero );
		}

		static bool IsSpecScreen( string szDevice, int nWidth, int nHeight )
		{
			MONITORINFOEX info;
			uint nDpi;
			if( GetMonitor( szDevice, out info, out nDpi ) == false ) {
				return false;
			}
			int nMonitorWidth = info.rcMonitor.Right - info.rcMonitor.Left;
			int nMonitorHeight = info.rcMonitor.Bottom - info.rcMonitor.Top;
			return nMonitorWidth == nWidth && nMonitorHeight == nHeight && nDpi == SPEC_DPI;
		}

		static void PrintScreen( string szKind, string szDevice )
		{
			MONITORINFOEX info;
			uint nDpi;
			if( GetMonitor( szDevice, out info, out nDpi ) == false ) {
				Console.WriteLine( "[display] {0} {1} not found", szKind, szDevice );
				return;
			}
			Console.WriteLine( "[display] {0} {1}: {2}x{3}, {4} DPI, work area {5}x{6}", szKind, szDevice,
				info.rcMonitor.Right - info.rcMonitor.Left, info.rcMonitor.Bottom - info.rcMonitor.Top, nDpi,
				info.rcWork.Right - info.rcWork.Left, info.rcWork.Bottom - info.rcWork.Top );
		}

		static bool GetMonitor( string szDevice, out MONITORINFOEX found, out uint nDpi )
		{
			MONITORINFOEX result = new MONITORINFOEX();
			uint nResultDpi = 0;
			bool isFound = false;
			MonitorEnumProc callback = ( IntPtr hMonitor, IntPtr hdc, IntPtr pRect, IntPtr data ) =>
			{
				MONITORINFOEX info = new MONITORINFOEX();
				info.cbSize = Marshal.SizeOf( typeof( MONITORINFOEX ) );
				if( GetMonitorInfo( hMonitor, ref info ) && info.szDevice == szDevice ) {
					uint nDpiX;
					uint nDpiY;
					GetDpiForMonitor( hMonitor, MDT_EFFECTIVE_DPI, out nDpiX, out nDpiY );
					result = info;
					nResultDpi = nDpiX;
					isFound = true;
				}
				return true;
			};
			EnumDisplayMonitors( IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero );
			GC.KeepAlive( callback );
			found = result;
			nDpi = nResultDpi;
			return isFound;
		}

		// the scale of a source is set relative to the recommended one; the smallest relative value is 100 %
		static string SetScale100( string szDevice )
		{
			DISPLAYCONFIG_SOURCE_DEVICE_NAME source;
			if( FindSource( szDevice, out source ) == false ) {
				return "cannot find the display path of " + szDevice;
			}
			DISPLAYCONFIG_SOURCE_DPI_SCALE_GET scale = new DISPLAYCONFIG_SOURCE_DPI_SCALE_GET();
			scale.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_DPI_SCALE;
			scale.header.size = (uint)Marshal.SizeOf( typeof( DISPLAYCONFIG_SOURCE_DPI_SCALE_GET ) );
			scale.header.adapterId = source.header.adapterId;
			scale.header.id = source.header.id;
			int nResult = DisplayConfigGetDeviceInfo( ref scale );
			if( nResult != 0 ) {
				return "cannot read the scale of " + szDevice + " (" + nResult + ")";
			}
			DISPLAYCONFIG_SOURCE_DPI_SCALE_SET setScale = new DISPLAYCONFIG_SOURCE_DPI_SCALE_SET();
			setScale.header.type = DISPLAYCONFIG_DEVICE_INFO_SET_DPI_SCALE;
			setScale.header.size = (uint)Marshal.SizeOf( typeof( DISPLAYCONFIG_SOURCE_DPI_SCALE_SET ) );
			setScale.header.adapterId = source.header.adapterId;
			setScale.header.id = source.header.id;
			setScale.scaleRel = scale.minScaleRel;
			nResult = DisplayConfigSetDeviceInfo( ref setScale );
			return nResult == 0 ? null : "cannot set the scale of " + szDevice + " to 100 % (" + nResult + ")";
		}

		static bool FindSource( string szDevice, out DISPLAYCONFIG_SOURCE_DEVICE_NAME found )
		{
			found = new DISPLAYCONFIG_SOURCE_DEVICE_NAME();
			uint nPathCount;
			uint nModeCount;
			if( GetDisplayConfigBufferSizes( QDC_ONLY_ACTIVE_PATHS, out nPathCount, out nModeCount ) != 0 ) {
				return false;
			}
			DISPLAYCONFIG_PATH_INFO[] pathArray = new DISPLAYCONFIG_PATH_INFO[ nPathCount ];
			DISPLAYCONFIG_MODE_INFO[] modeArray = new DISPLAYCONFIG_MODE_INFO[ nModeCount ];
			if( QueryDisplayConfig( QDC_ONLY_ACTIVE_PATHS, ref nPathCount, pathArray, ref nModeCount, modeArray,
				IntPtr.Zero ) != 0 ) {
				return false;
			}
			for( int i = 0; i < nPathCount; i++ ) {
				DISPLAYCONFIG_SOURCE_DEVICE_NAME source = new DISPLAYCONFIG_SOURCE_DEVICE_NAME();
				source.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME;
				source.header.size = (uint)Marshal.SizeOf( typeof( DISPLAYCONFIG_SOURCE_DEVICE_NAME ) );
				source.header.adapterId = pathArray[ i ].sourceInfo.adapterId;
				source.header.id = pathArray[ i ].sourceInfo.id;
				if( DisplayConfigGetDeviceInfo( ref source ) == 0 && source.viewGdiDeviceName == szDevice ) {
					found = source;
					return true;
				}
			}
			return false;
		}

		static DEVMODE NewDevMode()
		{
			DEVMODE mode = new DEVMODE();
			mode.dmDeviceName = string.Empty;
			mode.dmFormName = string.Empty;
			mode.dmSize = (short)Marshal.SizeOf( typeof( DEVMODE ) );
			return mode;
		}

		#endregion

		#region Parsec Virtual Display Driver

		static IntPtr OpenParsecDevice()
		{
			Guid adapterGuid = PARSEC_ADAPTER_GUID;
			IntPtr hDevInfo = SetupDiGetClassDevs( ref adapterGuid, IntPtr.Zero, IntPtr.Zero,
				DIGCF_PRESENT | DIGCF_DEVICEINTERFACE );
			if( hDevInfo == INVALID_HANDLE_VALUE ) {
				return INVALID_HANDLE_VALUE;
			}
			try {
				SP_DEVICE_INTERFACE_DATA interfaceData = new SP_DEVICE_INTERFACE_DATA();
				interfaceData.cbSize = Marshal.SizeOf( typeof( SP_DEVICE_INTERFACE_DATA ) );
				for( uint i = 0; SetupDiEnumDeviceInterfaces( hDevInfo, IntPtr.Zero, ref adapterGuid, i,
					ref interfaceData ); i++ ) {
					IntPtr hDevice = OpenInterface( hDevInfo, ref interfaceData );
					if( hDevice != INVALID_HANDLE_VALUE ) {
						return hDevice;
					}
				}
				return INVALID_HANDLE_VALUE;
			}
			finally {
				SetupDiDestroyDeviceInfoList( hDevInfo );
			}
		}

		static IntPtr OpenInterface( IntPtr hDevInfo, ref SP_DEVICE_INTERFACE_DATA interfaceData )
		{
			uint nRequiredSize;
			SetupDiGetDeviceInterfaceDetail( hDevInfo, ref interfaceData, IntPtr.Zero, 0, out nRequiredSize,
				IntPtr.Zero );
			IntPtr pDetail = Marshal.AllocHGlobal( (int)nRequiredSize );
			try {
				// cbSize of SP_DEVICE_INTERFACE_DETAIL_DATA_W: the size field and the first character, aligned
				Marshal.WriteInt32( pDetail, IntPtr.Size == 8 ? 8 : 6 );
				if( SetupDiGetDeviceInterfaceDetail( hDevInfo, ref interfaceData, pDetail, nRequiredSize,
					out nRequiredSize, IntPtr.Zero ) == false ) {
					return INVALID_HANDLE_VALUE;
				}
				string szPath = Marshal.PtrToStringUni( new IntPtr( pDetail.ToInt64() + 4 ) );
				return CreateFile( szPath, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE,
					IntPtr.Zero, OPEN_EXISTING,
					FILE_ATTRIBUTE_NORMAL | FILE_FLAG_NO_BUFFERING | FILE_FLAG_OVERLAPPED | FILE_FLAG_WRITE_THROUGH,
					IntPtr.Zero );
			}
			finally {
				Marshal.FreeHGlobal( pDetail );
			}
		}

		// the driver unplugs its screens when it is not pinged for about a second
		static void KeepAlive()
		{
			while( m_isStopping == false ) {
				IoControl( IOCTL_UPDATE, null );
				Thread.Sleep( KEEP_ALIVE_INTERVAL_MS );
			}
		}

		// every request sends a 32 byte input buffer and receives a 32 bit result; -1 when it fails
		static int IoControl( uint nCode, byte[] dataArray )
		{
			lock( m_IoLock ) {
				IntPtr pInput = Marshal.AllocHGlobal( IO_INPUT_SIZE );
				IntPtr pOutput = Marshal.AllocHGlobal( sizeof( int ) );
				IntPtr pOverlapped = Marshal.AllocHGlobal( Marshal.SizeOf( typeof( NativeOverlapped ) ) );
				IntPtr hEvent = CreateEvent( IntPtr.Zero, true, false, null );
				try {
					Marshal.Copy( new byte[ IO_INPUT_SIZE ], 0, pInput, IO_INPUT_SIZE );
					if( dataArray != null ) {
						Marshal.Copy( dataArray, 0, pInput, Math.Min( dataArray.Length, IO_INPUT_SIZE ) );
					}
					Marshal.WriteInt32( pOutput, 0 );
					NativeOverlapped overlapped = new NativeOverlapped();
					overlapped.EventHandle = hEvent;
					Marshal.StructureToPtr( overlapped, pOverlapped, false );
					DeviceIoControl( m_hDevice, nCode, pInput, IO_INPUT_SIZE, pOutput, sizeof( int ), IntPtr.Zero,
						pOverlapped );
					uint nTransferred;
					bool isDone = GetOverlappedResultEx( m_hDevice, pOverlapped, out nTransferred, IO_TIMEOUT_MS,
						false );
					if( isDone == false ) {
						return -1;
					}
					return Marshal.ReadInt32( pOutput );
				}
				finally {
					CloseHandle( hEvent );
					Marshal.FreeHGlobal( pOverlapped );
					Marshal.FreeHGlobal( pOutput );
					Marshal.FreeHGlobal( pInput );
				}
			}
		}

		#endregion

		static readonly object m_IoLock = new object();
		static IntPtr m_hDevice;
		static volatile bool m_isStopping;

		const int SPEC_LONG_SIDE = 1920;
		const int SPEC_SHORT_SIDE = 1080;
		const uint SPEC_DPI = 96;
		const int APPEAR_TIMEOUT_MS = 10000;
		const int POLL_INTERVAL_MS = 200;
		const int SETTLE_TIME_MS = 3000;
		const int KEEP_ALIVE_INTERVAL_MS = 50;
		const int IO_INPUT_SIZE = 32;
		const uint IO_TIMEOUT_MS = 5000;

		// Parsec Virtual Display Adapter interface and its requests
		static readonly Guid PARSEC_ADAPTER_GUID = new Guid( "00b41627-04c4-429e-a26e-0265cf50c8fa" );
		const uint IOCTL_ADD = 0x0022e004;
		const uint IOCTL_REMOVE = 0x0022a008;
		const uint IOCTL_UPDATE = 0x0022a00c;

		const int DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;
		const int MDT_EFFECTIVE_DPI = 0;
		const uint DISPLAY_DEVICE_ATTACHED_TO_DESKTOP = 0x1;
		const uint DISPLAY_DEVICE_PRIMARY_DEVICE = 0x4;
		const int ENUM_CURRENT_SETTINGS = -1;
		const int DM_POSITION = 0x20;
		const int DM_DISPLAYORIENTATION = 0x80;
		const int DM_PELSWIDTH = 0x80000;
		const int DM_PELSHEIGHT = 0x100000;
		const int DMDO_DEFAULT = 0;
		const int DMDO_90 = 1;
		const uint CDS_UPDATEREGISTRY = 0x1;
		const uint CDS_SET_PRIMARY = 0x10;
		const uint CDS_NORESET = 0x10000000;
		const int DISP_CHANGE_SUCCESSFUL = 0;
		const uint QDC_ONLY_ACTIVE_PATHS = 0x2;
		const int DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1;
		const int DISPLAYCONFIG_DEVICE_INFO_GET_DPI_SCALE = -3;
		const int DISPLAYCONFIG_DEVICE_INFO_SET_DPI_SCALE = -4;
		const uint DIGCF_PRESENT = 0x2;
		const uint DIGCF_DEVICEINTERFACE = 0x10;
		const uint GENERIC_READ = 0x80000000;
		const uint GENERIC_WRITE = 0x40000000;
		const uint FILE_SHARE_READ = 0x1;
		const uint FILE_SHARE_WRITE = 0x2;
		const uint OPEN_EXISTING = 3;
		const uint FILE_ATTRIBUTE_NORMAL = 0x80;
		const uint FILE_FLAG_NO_BUFFERING = 0x20000000;
		const uint FILE_FLAG_OVERLAPPED = 0x40000000;
		const uint FILE_FLAG_WRITE_THROUGH = 0x80000000;
		static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr( -1 );

		#region Native

		[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Unicode )]
		struct DISPLAY_DEVICE
		{
			public int cb;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = 32 )]
			public string DeviceName;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = 128 )]
			public string DeviceString;
			public uint StateFlags;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = 128 )]
			public string DeviceID;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = 128 )]
			public string DeviceKey;
		}

		[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Unicode )]
		struct DEVMODE
		{
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = 32 )]
			public string dmDeviceName;
			public short dmSpecVersion;
			public short dmDriverVersion;
			public short dmSize;
			public short dmDriverExtra;
			public int dmFields;
			public int dmPositionX;
			public int dmPositionY;
			public int dmDisplayOrientation;
			public int dmDisplayFixedOutput;
			public short dmColor;
			public short dmDuplex;
			public short dmYResolution;
			public short dmTTOption;
			public short dmCollate;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = 32 )]
			public string dmFormName;
			public short dmLogPixels;
			public int dmBitsPerPel;
			public int dmPelsWidth;
			public int dmPelsHeight;
			public int dmDisplayFlags;
			public int dmDisplayFrequency;
			public int dmICMMethod;
			public int dmICMIntent;
			public int dmMediaType;
			public int dmDitherType;
			public int dmReserved1;
			public int dmReserved2;
			public int dmPanningWidth;
			public int dmPanningHeight;
		}

		[StructLayout( LayoutKind.Sequential )]
		struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Unicode )]
		struct MONITORINFOEX
		{
			public int cbSize;
			public RECT rcMonitor;
			public RECT rcWork;
			public uint dwFlags;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = 32 )]
			public string szDevice;
		}

		[StructLayout( LayoutKind.Sequential )]
		struct LUID
		{
			public uint LowPart;
			public int HighPart;
		}

		[StructLayout( LayoutKind.Sequential )]
		struct DISPLAYCONFIG_PATH_SOURCE_INFO
		{
			public LUID adapterId;
			public uint id;
			public uint modeInfoIdx;
			public uint statusFlags;
		}

		[StructLayout( LayoutKind.Sequential )]
		struct DISPLAYCONFIG_PATH_TARGET_INFO
		{
			public LUID adapterId;
			public uint id;
			public uint modeInfoIdx;
			public int outputTechnology;
			public int rotation;
			public int scaling;
			public uint refreshRateNumerator;
			public uint refreshRateDenominator;
			public int scanLineOrdering;
			public int targetAvailable;
			public uint statusFlags;
		}

		[StructLayout( LayoutKind.Sequential )]
		struct DISPLAYCONFIG_PATH_INFO
		{
			public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
			public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
			public uint flags;
		}

		// the mode union is not read, only its size matters
		[StructLayout( LayoutKind.Sequential, Size = 64 )]
		struct DISPLAYCONFIG_MODE_INFO
		{
			public int infoType;
			public uint id;
			public LUID adapterId;
		}

		[StructLayout( LayoutKind.Sequential )]
		struct DISPLAYCONFIG_DEVICE_INFO_HEADER
		{
			public int type;
			public uint size;
			public LUID adapterId;
			public uint id;
		}

		[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Unicode )]
		struct DISPLAYCONFIG_SOURCE_DEVICE_NAME
		{
			public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = 32 )]
			public string viewGdiDeviceName;
		}

		[StructLayout( LayoutKind.Sequential )]
		struct DISPLAYCONFIG_SOURCE_DPI_SCALE_GET
		{
			public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
			public int minScaleRel;
			public int curScaleRel;
			public int maxScaleRel;
		}

		[StructLayout( LayoutKind.Sequential )]
		struct DISPLAYCONFIG_SOURCE_DPI_SCALE_SET
		{
			public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
			public int scaleRel;
		}

		[StructLayout( LayoutKind.Sequential )]
		struct SP_DEVICE_INTERFACE_DATA
		{
			public int cbSize;
			public Guid InterfaceClassGuid;
			public uint Flags;
			public IntPtr Reserved;
		}

		delegate bool MonitorEnumProc( IntPtr hMonitor, IntPtr hdc, IntPtr pRect, IntPtr data );

		[DllImport( "user32.dll" )]
		static extern bool SetProcessDpiAwarenessContext( IntPtr value );

		[DllImport( "user32.dll", CharSet = CharSet.Unicode )]
		static extern bool EnumDisplayDevices( string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice,
			uint dwFlags );

		[DllImport( "user32.dll", CharSet = CharSet.Unicode )]
		static extern bool EnumDisplaySettings( string deviceName, int modeNum, ref DEVMODE devMode );

		[DllImport( "user32.dll", CharSet = CharSet.Unicode )]
		static extern int ChangeDisplaySettingsEx( string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd,
			uint dwflags, IntPtr lParam );

		[DllImport( "user32.dll", CharSet = CharSet.Unicode )]
		static extern int ChangeDisplaySettingsEx( string lpszDeviceName, IntPtr lpDevMode, IntPtr hwnd,
			uint dwflags, IntPtr lParam );

		[DllImport( "user32.dll" )]
		static extern bool EnumDisplayMonitors( IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum,
			IntPtr dwData );

		[DllImport( "user32.dll", CharSet = CharSet.Unicode )]
		static extern bool GetMonitorInfo( IntPtr hMonitor, ref MONITORINFOEX lpmi );

		[DllImport( "shcore.dll" )]
		static extern int GetDpiForMonitor( IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY );

		[DllImport( "user32.dll" )]
		static extern int GetDisplayConfigBufferSizes( uint flags, out uint numPathArrayElements,
			out uint numModeInfoArrayElements );

		[DllImport( "user32.dll" )]
		static extern int QueryDisplayConfig( uint flags, ref uint numPathArrayElements,
			[Out] DISPLAYCONFIG_PATH_INFO[] pathArray, ref uint numModeInfoArrayElements,
			[Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray, IntPtr currentTopologyId );

		[DllImport( "user32.dll" )]
		static extern int DisplayConfigGetDeviceInfo( ref DISPLAYCONFIG_SOURCE_DEVICE_NAME requestPacket );

		[DllImport( "user32.dll" )]
		static extern int DisplayConfigGetDeviceInfo( ref DISPLAYCONFIG_SOURCE_DPI_SCALE_GET requestPacket );

		[DllImport( "user32.dll" )]
		static extern int DisplayConfigSetDeviceInfo( ref DISPLAYCONFIG_SOURCE_DPI_SCALE_SET setPacket );

		[DllImport( "setupapi.dll", SetLastError = true )]
		static extern IntPtr SetupDiGetClassDevs( ref Guid classGuid, IntPtr enumerator, IntPtr hwndParent,
			uint flags );

		[DllImport( "setupapi.dll", SetLastError = true )]
		static extern bool SetupDiEnumDeviceInterfaces( IntPtr deviceInfoSet, IntPtr deviceInfoData,
			ref Guid interfaceClassGuid, uint memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData );

		[DllImport( "setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode,
			EntryPoint = "SetupDiGetDeviceInterfaceDetailW" )]
		static extern bool SetupDiGetDeviceInterfaceDetail( IntPtr deviceInfoSet,
			ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData,
			uint deviceInterfaceDetailDataSize, out uint requiredSize, IntPtr deviceInfoData );

		[DllImport( "setupapi.dll" )]
		static extern bool SetupDiDestroyDeviceInfoList( IntPtr deviceInfoSet );

		[DllImport( "kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode )]
		static extern IntPtr CreateFile( string lpFileName, uint dwDesiredAccess, uint dwShareMode,
			IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile );

		[DllImport( "kernel32.dll", CharSet = CharSet.Unicode )]
		static extern IntPtr CreateEvent( IntPtr lpEventAttributes, bool bManualReset, bool bInitialState,
			string lpName );

		[DllImport( "kernel32.dll", SetLastError = true )]
		static extern bool DeviceIoControl( IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer,
			int nInBufferSize, IntPtr lpOutBuffer, int nOutBufferSize, IntPtr lpBytesReturned, IntPtr lpOverlapped );

		[DllImport( "kernel32.dll", SetLastError = true )]
		static extern bool GetOverlappedResultEx( IntPtr hFile, IntPtr lpOverlapped,
			out uint lpNumberOfBytesTransferred, uint dwMilliseconds, bool bAlertable );

		[DllImport( "kernel32.dll" )]
		static extern bool CloseHandle( IntPtr hObject );

		#endregion
	}
}
