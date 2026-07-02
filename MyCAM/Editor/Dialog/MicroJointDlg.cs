using System;

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
			m_tbxMicroJointLength.SilentSetValue = (decimal)DEFAULT_LENGTH;
			ApplyResourcesToUI();
		}

		public void ApplyResourcesToUI()
		{
			this.Text = m_LanguageManager.TryGetString( "MicroJointDlg", this.Text );
			m_lblLength.Text = m_LanguageManager.TryGetString( "m_lblLength", m_lblLength.Text );
			m_btnClearAll.Text = m_LanguageManager.TryGetString( "m_btnClearAll", m_btnClearAll.Text );
			m_btnDelete.Text = m_LanguageManager.TryGetString( "m_btnDelete", m_btnDelete.Text );
			m_btnCancel.Text = m_LanguageManager.TryGetString( "m_btnCancel", m_btnCancel.Text );
		}

		public void SetMicroJointLength( double length )
		{
			m_tbxMicroJointLength.SilentSetValue = (decimal)length;
		}

		public double GetMicroJointLength()
		{
			double result = (double)m_tbxMicroJointLength.Value;
			return result > 0 ? result : DEFAULT_LENGTH;
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

		void m_tbxMicroJointLength_DebouncedValueChanged( object sender, EventArgs e )
		{
			double newLength = GetMicroJointLength();
			LengthChangedAction?.Invoke( newLength );
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

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "MicroJointDlg" );
	}
}
