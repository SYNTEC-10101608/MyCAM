using System.Windows.Forms;
using CommonUI.Layout;

namespace MyCAM.Helper.FormShow
{
	internal static class DialogShowHelper
	{
		public static void ShowCentered( Form dialog, IWin32Window owner )
		{
			SetFormConfig( dialog );

			// avoid zero size issue
			if( dialog.Width == 0 || dialog.Height == 0 ) {
				dialog.PerformLayout();
			}

			// a dialog shown again keeps no Load event, so it is centered here every time
			dialog.StartPosition = FormStartPosition.Manual;
			dialog.Location = DialogPlacement.GetCenterLocation( dialog );
			dialog.Show( owner );
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
