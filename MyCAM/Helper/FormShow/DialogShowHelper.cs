using MyCAM.App;
using System.Drawing;
using System.Windows.Forms;

namespace MyCAM.Helper.FormShow
{
	internal static class DialogShowHelper
	{
		public static void ShowCentered( Form dialog )
		{
			SetFormConfig( dialog );

			// avoid zero size issue
			if( dialog.Width == 0 || dialog.Height == 0 ) {
				dialog.PerformLayout();
			}
			Point centerLocation = MyApp.CalculateDialogCenterLocation( dialog );
			if( centerLocation != null ) {
				dialog.StartPosition = FormStartPosition.Manual;
				dialog.Location = centerLocation;
			}
			dialog.Show();
		}

		public static void MoveToCenter( Form dialog )
		{
			if( dialog == null || dialog.IsDisposed || !dialog.Visible ) {
				return;
			}
			Point centerLocation = MyApp.CalculateDialogCenterLocation( dialog );
			if( centerLocation == null ) {
				return;
			}
			dialog.Location = centerLocation;
		}

		public static void EnsureDialogOnTop( Form dialog )
		{
			if( dialog == null || dialog.IsDisposed || !dialog.Visible ) {
				return;
			}

			// bring to the front
			dialog.BringToFront();
		}

		static void SetFormConfig( Form dialog )
		{
			// avoid dialog show in taskbar
			dialog.ShowInTaskbar = false;

			// avoid user resize
			dialog.MaximizeBox = false;
			dialog.MinimizeBox = false;
		}
	}
}
