using MyCAM.App;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal partial class MicroJointDlg : EditDialogBase<object>
	{
		public Action ConfirmAction;
		public Action ClearAllAction;
		public Action DeleteAction;
		public Action<double> LengthChangedAction;

		public MicroJointDlg()
		{
			InitializeComponent();
			m_tbxMicroJointLength.Text = DEFAULT_LENGTH.ToString();
		}

		public void SetMicroJointLength( double length )
		{
			m_tbxMicroJointLength.Text = length.ToString();
		}

		public double GetMicroJointLength()
		{
			if( double.TryParse( m_tbxMicroJointLength.Text, out double result ) && result > 0 ) {
				return result;
			}
			return DEFAULT_LENGTH;
		}

		public void DisableDeleteButton( bool bDisable )
		{
			m_btnDelete.Enabled = !bDisable;
		}

		public void DisableClearAllButton( bool bDisable )
		{
			m_btnClearAll.Enabled = !bDisable;
		}

		const double DEFAULT_LENGTH = 2.0;

		void OnTextBoxLeave( object sender, EventArgs e )
		{
			ValidateAndNotify();
		}

		void OnTextBoxKeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				ValidateAndNotify();
				e.Handled = true;
			}
		}

		void ValidateAndNotify()
		{
			if( ValidateInput() ) {
				double newLength = GetMicroJointLength();
				LengthChangedAction?.Invoke( newLength );
			}
			else {
				MyApp.Logger.ShowOnLogPanel( "请输入大于0的数值", MyApp.NoticeType.Hint );
			}
		}

		bool ValidateInput()
		{
			if( double.TryParse( m_tbxMicroJointLength.Text, out double result ) && result > 0 ) {
				m_tbxMicroJointLength.Text = Math.Round( result, 3).ToString();
				return true;
			}
			return false;
		}

		void m_btnCancel_Click( object sender, EventArgs e )
		{
			Close();
		}

		void m_btnClearAll_Click( object sender, EventArgs e )
		{
			ClearAllAction?.Invoke();
		}

		void m_btnDelete_Click( object sender, EventArgs e )
		{
			DeleteAction?.Invoke();
		}
	}
}
