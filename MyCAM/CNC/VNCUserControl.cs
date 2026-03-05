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
		// syntec controller need to set as the same 4:3 aspect ratio
		const int CONTROLLER_WIDTH = 400;
		const int CONTROLLER_HEIGHT = 300;

		internal VNCConnectionResult ConnectionResult
		{
			get;
			private set;
		}

		public VNCUserControl( string szIP )
		{
			InitializeComponent();
			ConnectionResult = StartUltraVNC( CONTROLLER_WIDTH, CONTROLLER_HEIGHT, szIP );
		}

		public void ScreenResizeByParentSize( int nParentWidth, int nParentHeight, out int nVNCStartX, out int nVNCStartY )
		{
			if( nParentWidth <= 0 || nParentHeight <= 0 ) {
				nVNCStartX = 0;
				nVNCStartY = 0;
				return;
			}
			const int CONTROLLER_Width = 4;
			const int CONTROLLER_Height = 3;
			int nVNCWidth;
			int nVNCHeight;

			// calculate the scaling ratio based on parent size and controller aspect ratio
			int dWidthRatio = nParentWidth / CONTROLLER_Width;
			int dHeightRatio = nParentHeight / CONTROLLER_Height;
			int baseRatio = Math.Min( dWidthRatio, dHeightRatio );

			// cal VNC screen resized size
			nVNCWidth = CONTROLLER_Width * baseRatio;
			nVNCHeight = CONTROLLER_Height * baseRatio;

			// resize VNC screen
			VNCScreenResize( nVNCWidth, nVNCHeight );

			// screen start position (to set on parent)
			nVNCStartX = ( nParentWidth - nVNCWidth ) / 2;
			nVNCStartY = ( nParentHeight - nVNCHeight ) / 2;
		}

		public void VNCScreenResize( int nWidth, int nHeight )
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
