using MyCAM.Data;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	public class ActionDialogBase<TEditDataType> : Form
	{
		protected ActionDialogBase()
		{
			FormClosing += RaiseCancel;
		}

		// events
		public Action<TEditDataType> Preview;
		public Action<TEditDataType> Confirm;
		public Action Cancel;

		protected virtual void RaisePreview( TEditDataType data )
		{
			Preview?.Invoke( data );
		}

		protected virtual void RaiseConfirm( TEditDataType data )
		{
			m_ConfirmCheck = true;
			Confirm?.Invoke( data );
			Close();
		}

		// user raise cancel
		protected virtual void RaiseCancel( object sender, FormClosingEventArgs e )
		{
			if( m_ConfirmCheck == false && e.CloseReason == CloseReason.UserClosing ) {
				Cancel?.Invoke();
			}
		}

		protected bool m_ConfirmCheck = false;
	}
}
