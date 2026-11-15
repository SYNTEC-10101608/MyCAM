using System.Windows.Forms;
using CommonUI.Forms;
using MyCAM.Helper.FormShow;

namespace MyCAM.Editor
{
	// behavior base: how a tool dialog is shown; its appearance comes from DialogBase
	public class FormBase : DialogBase
	{
		protected FormBase()
		{
		}

		public new void Show( IWin32Window owner )
		{
			DialogShowHelper.ShowCentered( this, owner );
		}
	}
}
