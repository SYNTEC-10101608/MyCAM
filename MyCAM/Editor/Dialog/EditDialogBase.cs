using System;
using System.Windows.Forms;
using MyCAM.App;

namespace MyCAM.Editor
{
	public class EditDialogBase<TEditDataType> : FormBase
	{
		protected EditDialogBase()
		{
			FormClosing += RaiseCancel;
		}

		// events
		public Action<TEditDataType> Preview;
		public Action<TEditDataType> Confirm;
		public Action Cancel;
		public Action<TEditDataType> Reset;

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

		protected virtual void RaiseReset( TEditDataType data )
		{
			Reset?.Invoke( data );
		}

		protected bool m_ConfirmCheck = false;
	}
}
