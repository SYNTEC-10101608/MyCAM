using System;

namespace MyCAM.Editor.Dialog
{
	public partial class PathCompensateDlg : EditDialogBase<double>
	{
		public PathCompensateDlg()
		{
			InitializeComponent();
			m_LanguageManager = new LanguageManager.LanguageManager( "PathCompensateDlg" );
			Initialization();
		}

		public Func<double, bool> IsGeomConstraintExceedsLimit;

		void Initialization()
		{
			// initialize numeric up down controls
			m_NumUpDownCompensate.Maximum = decimal.MaxValue;
			m_NumUpDownCompensate.Minimum = decimal.MinValue;
			m_NumUpDownCompensate.DecimalPlaces = 3;
			m_NumUpDownCompensate.Increment = 0.25m;
		}

		void m_NumUpDownCompensate_DebouncedValueChanged( object sender, System.EventArgs e )
		{
			SetCompensateValue();
		}

		public void SetTotalCompsensation( double? totalCompsensation )
		{
			if( totalCompsensation == null ) {
				m_lblTotalCompensateValue.Text = m_LanguageManager.TryGetString( "IsMixData", IS_MIX_DATA );
			}
			else {
				m_lblTotalCompensateValue.Text = totalCompsensation.Value.ToString( "F3" );
			}

		}

		void SetCompensateValue()
		{
			bool isExceedsLimit = IsGeomConstraintExceedsLimit?.Invoke( (double)m_NumUpDownCompensate.Value ) ?? false;
			if( isExceedsLimit ) {
				m_NumUpDownCompensate.SilentSetValue = (decimal)m_CompensateData;
				return;
			}
			m_CompensateData = (double)m_NumUpDownCompensate.Value;
			RaisePreview( m_CompensateData );
		}

		void m_btnReset_Click( object sender, System.EventArgs e )
		{
			m_CompensateData = 0;
			m_NumUpDownCompensate.SilentSetValue = 0;
			RaiseReset( m_CompensateData );
		}

		void m_btnClear_Click( object sender, System.EventArgs e )
		{
			m_CompensateData = 0.0;
			m_NumUpDownCompensate.SilentSetValue = 0;
			RaisePreview( m_CompensateData );
		}

		void m_btnConfirm_Click( object sender, System.EventArgs e )
		{
			bool isExceedsLimit = IsGeomConstraintExceedsLimit?.Invoke( (double)m_NumUpDownCompensate.Value ) ?? false;
			if( isExceedsLimit ) {
				m_NumUpDownCompensate.SilentSetValue = (decimal)m_CompensateData;
				return;
			}
			m_CompensateData = (double)m_NumUpDownCompensate.Value;
			RaiseConfirm( m_CompensateData );
		}

		double m_CompensateData = 0.0;
		readonly LanguageManager.LanguageManager m_LanguageManager;

		const string IS_MIX_DATA = "補償不一致";
	}
}
