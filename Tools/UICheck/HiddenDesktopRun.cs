using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UICheck
{
	// Runs a command line on a separate desktop that is never shown, so the UI check runners do not appear on the
	// user's screen or take focus while the user keeps working. Compiled by the UICheck scripts into the build
	// output folder; stdout / stderr are inherited and the exit code is passed through.
	//
	// Windows on a desktop without a display still paint for DrawToBitmap / PrintWindow, so dialog screenshots are
	// identical to the visible desktop; only the OpenGL 3D view stays empty.
	//
	// Usage: UICheckHidden.exe <exe> [args ...]
	internal static class HiddenDesktopRun
	{
		static int Main( string[] args )
		{
			if( args.Length < 1 ) {
				Console.Error.WriteLine( "usage: UICheckHidden.exe <exe> [args ...]" );
				return 2;
			}

			// one desktop per run, so parallel runs do not share windows
			string szDesktop = "UICheck_" + System.Diagnostics.Process.GetCurrentProcess().Id;
			IntPtr hDesktop = CreateDesktop( szDesktop, IntPtr.Zero, IntPtr.Zero, 0, GENERIC_ALL, IntPtr.Zero );

			// no desktop, the runner cannot start
			if( hDesktop == IntPtr.Zero ) {
				Console.Error.WriteLine( "[UICheck] CreateDesktop failed: " + Marshal.GetLastWin32Error() );
				return 2;
			}
			try {
				return Run( szDesktop, BuildCommandLine( args ) );
			}
			finally {
				CloseDesktop( hDesktop );
			}
		}

		static int Run( string szDesktop, StringBuilder commandLine )
		{
			STARTUPINFO startup = new STARTUPINFO();
			startup.cb = Marshal.SizeOf( typeof( STARTUPINFO ) );
			startup.lpDesktop = @"WinSta0\" + szDesktop;
			startup.dwFlags = STARTF_USESTDHANDLES;
			startup.hStdInput = GetStdHandle( STD_INPUT_HANDLE );
			startup.hStdOutput = GetStdHandle( STD_OUTPUT_HANDLE );
			startup.hStdError = GetStdHandle( STD_ERROR_HANDLE );
			PROCESS_INFORMATION info;
			bool isCreated = CreateProcess( null, commandLine, IntPtr.Zero, IntPtr.Zero, true, 0, IntPtr.Zero, null,
				ref startup, out info );

			// executable not found or not startable
			if( isCreated == false ) {
				Console.Error.WriteLine( "[UICheck] CreateProcess failed: " + Marshal.GetLastWin32Error() );
				return 2;
			}
			WaitForSingleObject( info.hProcess, INFINITE );
			uint nExitCode;
			GetExitCodeProcess( info.hProcess, out nExitCode );
			CloseHandle( info.hThread );
			CloseHandle( info.hProcess );
			return (int)nExitCode;
		}

		static StringBuilder BuildCommandLine( string[] argArray )
		{
			StringBuilder commandLine = new StringBuilder();
			foreach( string szArg in argArray ) {
				if( commandLine.Length > 0 ) {
					commandLine.Append( ' ' );
				}
				commandLine.Append( '"' ).Append( szArg.Replace( "\"", "\\\"" ) ).Append( '"' );
			}
			return commandLine;
		}

		const uint GENERIC_ALL = 0x10000000;
		const int STARTF_USESTDHANDLES = 0x100;
		const int STD_INPUT_HANDLE = -10;
		const int STD_OUTPUT_HANDLE = -11;
		const int STD_ERROR_HANDLE = -12;
		const uint INFINITE = 0xFFFFFFFF;

		[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Unicode )]
		struct STARTUPINFO
		{
			public int cb;
			public string lpReserved;
			public string lpDesktop;
			public string lpTitle;
			public int dwX;
			public int dwY;
			public int dwXSize;
			public int dwYSize;
			public int dwXCountChars;
			public int dwYCountChars;
			public int dwFillAttribute;
			public int dwFlags;
			public short wShowWindow;
			public short cbReserved2;
			public IntPtr lpReserved2;
			public IntPtr hStdInput;
			public IntPtr hStdOutput;
			public IntPtr hStdError;
		}

		[StructLayout( LayoutKind.Sequential )]
		struct PROCESS_INFORMATION
		{
			public IntPtr hProcess;
			public IntPtr hThread;
			public int dwProcessId;
			public int dwThreadId;
		}

		[DllImport( "user32.dll", SetLastError = true, CharSet = CharSet.Unicode )]
		static extern IntPtr CreateDesktop( string lpszDesktop, IntPtr lpszDevice, IntPtr pDevmode, int dwFlags,
			uint dwDesiredAccess, IntPtr lpsa );

		[DllImport( "user32.dll" )]
		static extern bool CloseDesktop( IntPtr hDesktop );

		[DllImport( "kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode )]
		static extern bool CreateProcess( string lpApplicationName, StringBuilder lpCommandLine,
			IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags,
			IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo,
			out PROCESS_INFORMATION lpProcessInformation );

		[DllImport( "kernel32.dll" )]
		static extern IntPtr GetStdHandle( int nStdHandle );

		[DllImport( "kernel32.dll" )]
		static extern uint WaitForSingleObject( IntPtr hHandle, uint dwMilliseconds );

		[DllImport( "kernel32.dll" )]
		static extern bool GetExitCodeProcess( IntPtr hProcess, out uint lpExitCode );

		[DllImport( "kernel32.dll" )]
		static extern bool CloseHandle( IntPtr hObject );
	}
}
