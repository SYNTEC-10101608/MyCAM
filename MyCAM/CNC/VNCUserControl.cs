using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using CommonUI.Forms;

namespace VNCFormsApp
{
	internal enum VNCConnectionResult
	{
		Success,
		FileNotFound,
		ConnectionFailed,
		OtherError
	}

	public partial class VNCUserControl : PanelBase
	{
		// syntec controller need to set as the same 4:3 aspect ratio
		const int CONTROLLER_WIDTH = 400;
		const int CONTROLLER_HEIGHT = 300;

		internal VNCConnectionResult ConnectionResult
		{
			get;
			private set;
		}

		// true only when the VNC connection was established successfully
		internal bool IsConnected
		{
			get;
			private set;
		}

		public VNCUserControl( string szIP )
		{
			InitializeComponent();
			ConnectionResult = StartUltraVNC( CONTROLLER_WIDTH, CONTROLLER_HEIGHT, szIP );
			IsConnected = ConnectionResult == VNCConnectionResult.Success;
		}

		public static Size GetScreenSize( Size availableSize )
		{
			if( availableSize.Width <= 0 || availableSize.Height <= 0 ) {
				return Size.Empty;
			}
			const int WIDTH_RATIO = 4;
			const int HEIGHT_RATIO = 3;
			int scale = Math.Min( availableSize.Width / WIDTH_RATIO, availableSize.Height / HEIGHT_RATIO );
			return new Size( WIDTH_RATIO * scale, HEIGHT_RATIO * scale );
		}

		public void ScreenResizeByParentSize( int nParentWidth, int nParentHeight, out int nVNCStartX, out int nVNCStartY )
		{
			if( nParentWidth <= 0 || nParentHeight <= 0 ) {
				nVNCStartX = 0;
				nVNCStartY = 0;
				return;
			}
			Size screenSize = GetScreenSize( new Size( nParentWidth, nParentHeight ) );

			// resize VNC screen
			VNCScreenResize( screenSize.Width, screenSize.Height );

			// screen start position (to set on parent)
			nVNCStartX = ( nParentWidth - screenSize.Width ) / 2;
			nVNCStartY = ( nParentHeight - screenSize.Height ) / 2;
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
