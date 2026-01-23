using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace VNCFormsApp
{
	internal enum VNCConnectionResult
	{
		Success,
		FileNotFound,
		ConnectionFailed,
		OtherError
	}

	public partial class VNCUserControl : UserControl
	{
		internal VNCConnectionResult ConnectionResult
		{
			get;
			private set;
		}

		public VNCUserControl( int nWidth, int nHeight, string szIP )
		{
			InitializeComponent();
			ConnectionResult = StartUltraVNC( nWidth, nHeight, szIP );
		}

		public void WindowResize( int nWidth, int nHeight )
		{
			Width = nWidth;
			Height = nHeight;

			// use arguments
			SetWindowPos( exeProcess.MainWindowHandle, IntPtr.Zero, 0, 0, Width, Height, SWP_SHOWWINDOW );
		}

		// Close the external VNC process and clean up resources
		public void CloseVNCProcess()
		{
			if( exeProcess != null ) {
				try {
					if( !exeProcess.HasExited ) {
						exeProcess.CloseMainWindow();

						// give the process some time to exit gracefully
						if( !exeProcess.WaitForExit( 2000 ) ) {

							// if it does not exit, kill it
							exeProcess.Kill();
						}
					}
				}
				catch {
					// ignore exceptions during closing
				}
				finally {
					exeProcess?.Dispose();
					exeProcess = null;
				}
			}
		}

		//
		// private members and functions
		//
		delegate bool EnumWindowsProc( IntPtr hWnd, IntPtr lParam );

		Process exeProcess;
		const int SWP_SHOWWINDOW = 0x0040;

		#region Dll Import

		[DllImport( "user32.dll" )]
		static extern IntPtr SetParent( IntPtr hWndChild, IntPtr hWndNewParent );

		[DllImport( "user32.dll" )]
		static extern bool SetWindowPos( IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags );

		#endregion

		VNCConnectionResult StartUltraVNC( int nWidth, int nHeight, string szIP )
		{
			VNCConnectionResult result = VNCConnectionResult.OtherError;

			try {
				Width = nWidth;
				Height = nHeight;
				ProcessStartInfo startInfo = new ProcessStartInfo();

				// Set vnc viewer program path
				startInfo.FileName = @".\vncviewer_1.0.2.exe";

				// use arguments
				startInfo.Arguments = szIP;

				startInfo.UseShellExecute = true;
				exeProcess = new Process();
				exeProcess.StartInfo = startInfo;
				exeProcess.Start();
				exeProcess.WaitForInputIdle();

				// Embed exe window into UserControl
				IntPtr result_ptr = IntPtr.Zero;
				while( result_ptr == IntPtr.Zero ) {
					Thread.Sleep( 100 );
					result_ptr = SetParent( exeProcess.MainWindowHandle, Handle );
				}

				SetWindowPos( exeProcess.MainWindowHandle, IntPtr.Zero, 0, 0, Width, Height, SWP_SHOWWINDOW );

				// check if VNC connect connect successfully
				bool isConnected = exeProcess.MainWindowTitle == "VNC client";
				if( isConnected ) {
					result = VNCConnectionResult.Success;
				}
				else {
					result = VNCConnectionResult.ConnectionFailed;
				}
			}
			catch( System.ComponentModel.Win32Exception ) {

				// Failed to start the external VNC process (file not found, permission denied, etc.)
				CloseVNCProcess();
				result = VNCConnectionResult.FileNotFound;
			}
			catch( Exception ) {

				// Other exceptions during VNC connection process
				CloseVNCProcess();
				result = VNCConnectionResult.OtherError;
			}

			return result;
		}
	}
}
