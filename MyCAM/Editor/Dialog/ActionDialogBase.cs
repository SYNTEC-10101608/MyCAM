using MyCAM.Data;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	public class ActionDialogBase<TEditDataType> : Form
	{
		// events
		public Action<TEditDataType> Preview;
		public Action<TEditDataType> Confirm;
		public Action Cancel;

		protected bool m_ConfirmCheck = false;
	}
}
