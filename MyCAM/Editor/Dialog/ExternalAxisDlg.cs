using MyCAM.App;
using System;

namespace MyCAM.Editor.Dialog
{
	public partial class ExternalAxisDlg : EditDialogBase<double>
	{
		public ExternalAxisDlg( double externalAxis_deg )
		{
			InitializeComponent();
			m_ExternalAxis_deg = externalAxis_deg;
			m_tbxExternalAxis.SilentSetValue = (decimal)m_ExternalAxis_deg;
			ApplyResourcesToUI();
		}

		void ApplyResourcesToUI()
		{
			this.Text = m_LanguageManager.TryGetString( "ExternalAxisDlg", this.Text );
			m_lblExternalAxis.Text = m_LanguageManager.TryGetString( "m_lblExternalAxis", m_lblExternalAxis.Text );
			m_lblExternalAxisUnit.Text = m_LanguageManager.TryGetString( "m_lblExternalAxisUnit", m_lblExternalAxisUnit.Text );
			m_btnSure.Text = m_LanguageManager.TryGetString( "m_btnSure", m_btnSure.Text );
		}

		void m_tbxExternalAxis_DebouncedValueChanged( object sender, EventArgs e )
		{
			PreviewExternalAxisResult();
		}

		void m_btnSure_Click( object sender, EventArgs e )
		{
			if( IsValidParam() ) {
				RaiseConfirm( m_ExternalAxis_deg );
			}
		}

		void PreviewExternalAxisResult()
		{
			if( IsValidParam() ) {
				RaisePreview( m_ExternalAxis_deg );
			}
		}

		bool IsValidParam()
		{
			double externalAxis_deg = (double)m_tbxExternalAxis.Value;
			m_ExternalAxis_deg = externalAxis_deg;
			return true;
		}

		double m_ExternalAxis_deg = 0;
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ExternalAxisDlg" );
	}
}
