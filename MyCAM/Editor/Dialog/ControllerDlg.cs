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
	}
}
