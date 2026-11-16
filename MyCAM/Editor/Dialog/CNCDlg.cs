using System;
using System.Drawing;
using System.Windows.Forms;
using CommonUI.Forms;
using MyCAM.App;
using VNCFormsApp;

namespace MyCAM.Editor.Dialog
{
	public partial class CNCDlg : DialogBase
	{
		public CNCDlg()
		{
			InitializeComponent();
			ApplyLanguage();
			InitAspectRatio();
		}

		void InitAspectRatio()
		{
			// calculate and store original aspect ratio
			m_DlgHeightAndWidthRatio = (double)ClientSize.Width / ClientSize.Height;

			// subscribe to ResizeEnd event to maintain aspect ratio
			ResizeEnd += CNCDlg_ResizeEnd;
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );
			if( Owner != null && Owner.Height > Owner.Width ) {
				FitToWorkingArea( Screen.FromControl( Owner ).WorkingArea );
				PositionInArea( m_PlacementArea );
			}
		}

		internal void PositionInArea( Rectangle area )
		{
			m_PlacementArea = area;
			if( Owner == null || Owner.Height <= Owner.Width ) {
				DialogPlacement.MoveToCenter( this );
				return;
			}
			Rectangle workingArea = Screen.FromControl( Owner ).WorkingArea;
			if( area.IsEmpty ) {
				area = workingArea;
			}
			int left = area.Left + ( area.Width - Width ) / 2;
			int top = area.Top + ( area.Height - Height ) / 2;
			Location = new Point(
				Math.Max( workingArea.Left, Math.Min( left, workingArea.Right - Width ) ),
				Math.Max( workingArea.Top, Math.Min( top, workingArea.Bottom - Height ) ) );
		}

		void FitToWorkingArea( Rectangle workingArea )
		{
			Size frameSize = Size - ClientSize;
			Size availableSize = new Size(
				Math.Max( 0, Math.Min( ClientSize.Width, workingArea.Width - frameSize.Width ) ),
				Math.Max( 0, Math.Min( ClientSize.Height, workingArea.Height - frameSize.Height ) ) );
			Size fittedSize = VNCUserControl.GetScreenSize( availableSize );
			if( fittedSize.IsEmpty ) {
				return;
			}
			ClientSize = fittedSize;
			m_DlgHeightAndWidthRatio = (double)ClientSize.Width / ClientSize.Height;
			UpdateVNCScreen();
		}

		protected override void OnFormClosed( FormClosedEventArgs e )
		{
			// remove VNC screen pointer from this dialog
			this.Controls.Clear();
			base.OnFormClosed( e );
		}

		public void PutVNCOnDlg()
		{
			// add VNC screen to this dialog
			Controls.Add( MyApp.VNCScreen );
			UpdateVNCScreen();
		}

		void UpdateVNCScreen()
		{
			if( MyApp.VNCScreen == null || MyApp.VNCScreen.IsDisposed || !Controls.Contains( MyApp.VNCScreen ) ) {
				return;
			}
			MyApp.VNCScreen.ScreenResizeByParentSize( ClientSize.Width, ClientSize.Height, out int startX, out int startY );
			MyApp.VNCScreen.Location = new Point( startX, startY );
		}

		Rectangle m_PlacementArea;
		double m_DlgHeightAndWidthRatio;

		// flag for preventing recursive resizing
		bool m_IsResizing = false;

		void CNCDlg_ResizeEnd( object sender, System.EventArgs e )
		{
			// prevent recursive resizing
			if( m_IsResizing ) {
				return;
			}

			m_IsResizing = true;

			// maintain aspect ratio based on width
			int newHeight = (int)( ClientSize.Width / m_DlgHeightAndWidthRatio );
			ClientSize = new Size( ClientSize.Width, newHeight );

			// resize VNC screen to fit new dialog size
			UpdateVNCScreen();
			m_IsResizing = false;
		}
	}
}
