using MyCAM.App;
using System.Drawing;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
{
	public partial class ControllerDlg : Form
	{
		public ControllerDlg()
		{
			InitializeComponent();

			// calculate and store original aspect ratio
			m_DlgHeightAndWidthRatio = (double)ClientSize.Width / ClientSize.Height;

			// subscribe to ResizeEnd event to maintain aspect ratio
			ResizeEnd += ControllerDlg_ResizeEnd;
		}

		protected override void OnFormClosed( FormClosedEventArgs e )
		{
			// remove VNC screen pointer from this dialog
			this.Controls.Clear();
			base.OnFormClosed( e );
		}

		public void PutVNCOnDlg()
		{
			MyApp.VNCScreen.ScreenResizeByParentSize( this.ClientSize.Width, this.ClientSize.Height, out int nVNCStartX, out int nVNCStartY );
			MyApp.VNCScreen.Location = new Point( nVNCStartX, nVNCStartY );

			// add VNC screen to this dialog
			Controls.Add( MyApp.VNCScreen );
		}

		double m_DlgHeightAndWidthRatio;

		// flag for preventing recursive resizing
		bool m_IsResizing = false;

		void ControllerDlg_ResizeEnd( object sender, System.EventArgs e )
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
			if( Controls.Contains( MyApp.VNCScreen ) ) {
				MyApp.VNCScreen.ScreenResizeByParentSize( ClientSize.Width, ClientSize.Height, out int nVNCStartX, out int nVNCStartY );
				MyApp.VNCScreen.Location = new Point( nVNCStartX, nVNCStartY );
			}
			m_IsResizing = false;
		}
	}
}
