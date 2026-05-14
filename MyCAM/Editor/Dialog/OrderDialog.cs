using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal partial class OrderDialog : Form
	{
		// --- events raised to Action ---
		public Action<OrderMode> OrderModeChanged;
		public Action SequentialOrderApply;
		public Action EnterKeyPressed;
		public Action MoveUp;
		public Action MoveDown;
		public Action MoveToTop;

		// events
		public Action Confirm;
		public Action Cancel;


		public OrderDialog()
		{
			InitializeComponent();
			InitializeDialog();
		}

		public void CloseDialog()
		{
			m_IsClosingProgrammatically = true;
			Close();
		}

		void InitializeDialog()
		{
			// initial state
			m_rbSequentialOrder.Checked = true;
			UpdateUIByMode();

			// register enter event
			this.KeyDown += OrderDialog_KeyDown;
			this.KeyPreview = true;

			// register form closing event
			this.FormClosing += OrderDialog_FormClosing;
		}

		void OrderDialog_FormClosing( object sender, FormClosingEventArgs e )
		{
			// trigger cancel event when user closes the dialog
			// only if not already closing programmatically
			if( !m_IsClosingProgrammatically ) {
				Cancel?.Invoke();
			}
		}

		// Refresh the dialog UI to reflect the given mode
		public void SetOrderMode( OrderMode mode )
		{
			m_bSuppressValueChanged = true;

			switch( mode ) {
				case OrderMode.Sequential:
					m_rbSequentialOrder.Checked = true;
					break;
				case OrderMode.Swap:
					m_rbPathSwap.Checked = true;
					break;
			}

			UpdateUIByMode();
			m_bSuppressValueChanged = false;
		}

		// ---- UI events ----

		protected 

		void m_rbSequentialOrder_CheckedChanged( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged ) {
				return;
			}

			if( m_rbSequentialOrder.Checked ) {
				UpdateUIByMode();
				OrderModeChanged?.Invoke( OrderMode.Sequential );
			}
		}

		void m_rbPathSwap_CheckedChanged( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged ) {
				return;
			}

			if( m_rbPathSwap.Checked ) {
				UpdateUIByMode();
				OrderModeChanged?.Invoke( OrderMode.Swap );
			}
		}

		void m_btnApply_Click( object sender, EventArgs e )
		{
			Confirm?.Invoke( );
		}

		void m_btnMoveToTop_Click( object sender, EventArgs e )
		{
			MoveToTop?.Invoke();

			// remove focus from the button to prevent sequential mode "enter" event be locked
			//this.ActiveControl = null;
		}

		void m_btnMoveUp_Click( object sender, EventArgs e )
		{
			MoveUp?.Invoke();
			//this.ActiveControl = null;
		}

		void m_btnMoveDown_Click( object sender, EventArgs e )
		{
			MoveDown?.Invoke();
			//ActiveControl = null;
		}

		void OrderDialog_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				EnterKeyPressed?.Invoke();
				e.Handled = true;
			}
		}

		void UpdateUIByMode()
		{
			if( m_rbSequentialOrder.Checked ) {
				m_gbxContent.Text = "說明";
				m_lblSequentialDesc.Visible = true;
				m_btnMoveUp.Visible = false;
				m_btnMoveDown.Visible = false;
			}
			else if( m_rbPathSwap.Checked ) {
				m_gbxContent.Text = "執行操作";
				m_lblSequentialDesc.Visible = false;
				m_btnMoveUp.Visible = true;
				m_btnMoveDown.Visible = true;
			}
		}

		bool m_bSuppressValueChanged = false;
		bool m_IsClosingProgrammatically = false;
	}

	public enum OrderMode
	{
		Sequential,
		Swap
	}
}
