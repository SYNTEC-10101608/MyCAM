using System;

namespace MyCAM.Editor
{
	internal partial class RCornerDlg : EditDialogBase<object>
	{
		public Action DeleteRequest;
		public Action SwitchStartOrEndRequest;

		public RCornerDlg()
		{
			InitializeComponent();
			ApplyLanguage();
		}

		public void SetStatus( string szStatus )
		{
			m_lblStatus.Text = szStatus;
		}

		public void SetDeleteEnabled( bool isEnabled )
		{
			m_btnDelete.Enabled = isEnabled;
		}

		public void SetSwitchEnabled( bool isEnabled )
		{
			m_btnSwitchStartOrEnd.Enabled = isEnabled;
		}

		void m_btnDelete_Click( object sender, EventArgs e )
		{
			DeleteRequest?.Invoke();
		}

		void m_btnSwitchStartOrEnd_Click( object sender, EventArgs e )
		{
			SwitchStartOrEndRequest?.Invoke();
		}

		void m_btnDone_Click( object sender, EventArgs e )
		{
			Close();
		}
	}
}
