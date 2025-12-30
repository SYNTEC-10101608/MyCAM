using MyCAM.App;
using MyCAM.Helper.FormShow;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	public class EditDialogBase<TEditDataType> : Form
	{
		protected EditDialogBase()
		{
			FormClosing += RaiseCancel;
		}

		// events
		public Action<TEditDataType> Preview;
		public Action<TEditDataType> Confirm;
		public Action Cancel;

		public new void Show( IWin32Window owner = null )
		{
			MyApp.RegistDialogMoveWithMainForm( this );
			DialogShowHelper.ShowCentered( this );
		}

		public void OnParentLocationChange()
		{
			DialogShowHelper.MoveToCenter( this );
		}

		public void EnsureDialogOnTop()
		{
			// mouse is not using
			if( !IsViewerInMouseOperation() ) {
				DialogShowHelper.EnsureDialogOnTop( this );
			}

			// mouse is using, start monitoring
			else {
				StartMouseOperationMonitoring();
			}
		}

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

		protected override void OnFormClosed( FormClosedEventArgs e )
		{
			CleanupTimer();
			base.OnFormClosed( e );
		}

		void StartMouseOperationMonitoring()
		{
			if( m_MouseOperationTimer != null ) {
				return;
			}
			m_MouseOperationTimer = new Timer
			{
				Interval = 100
			};
			m_MouseOperationTimer.Tick += OnMouseOperationTimerTick;
			m_MouseOperationTimer.Start();
		}

		void OnMouseOperationTimerTick( object sender, EventArgs e )
		{
			// mouse using done
			if( !IsViewerInMouseOperation() ) {

				// stop timer
				CleanupTimer();

				// put from to the front
				if( !IsDisposed && Visible ) {
					DialogShowHelper.EnsureDialogOnTop( this );
				}
			}
		}

		void CleanupTimer()
		{
			if( m_MouseOperationTimer != null ) {
				try {
					m_MouseOperationTimer.Tick -= OnMouseOperationTimerTick;
					m_MouseOperationTimer.Stop();
					m_MouseOperationTimer.Dispose();
				}
				catch( ObjectDisposedException ) {
				}
				catch( Exception ex ) {
					MyApp.Logger?.ShowOnLogPanel( $"清理 Timer 時發生錯誤: {ex.Message}", MyApp.NoticeType.Warning );
				}
				finally {
					m_MouseOperationTimer = null;
				}
			}
		}

		bool IsViewerInMouseOperation()
		{
			// check is it mouse is clicked
			return ( Control.MouseButtons & MouseButtons.Left ) == MouseButtons.Left ||
				   ( Control.MouseButtons & MouseButtons.Right ) == MouseButtons.Right ||
				   ( Control.MouseButtons & MouseButtons.Middle ) == MouseButtons.Middle;
		}
		protected bool m_ConfirmCheck = false;
		Timer m_MouseOperationTimer;
	}
}
