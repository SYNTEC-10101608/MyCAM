using System;
using System.Windows.Forms;
using MyCAM.App;
using MyCAM.Helper.FormShow;

namespace MyCAM.Editor
{
	public class FormBase : Form
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
