using System;
using MyCAM.App;
using MyCAM.Data;

namespace MyCAM.Editor.Dialog
{
	internal partial class LeadDlg : EditDialogBase<LeadData>
	{
		public LeadDlg( LeadData leadData )
		{
			if( leadData == null ) {
				leadData = new LeadData();
			}
			InitializeComponent();
			ApplyLanguage();
			m_LeadData = leadData;

			// lead in setting
			m_numLeadInStraightLength.SilentSetValue = (decimal)m_LeadData.LeadIn.StraightLength;
			m_numLeadInArcLength.SilentSetValue = (decimal)m_LeadData.LeadIn.ArcLength;
			m_numLeadInAngle.SilentSetValue = (decimal)m_LeadData.LeadIn.Angle_deg;

			// lead out setting
			m_numLeadOutStraightLength.SilentSetValue = (decimal)m_LeadData.LeadOut.StraightLength;
			m_numLeadOutArcLength.SilentSetValue = (decimal)m_LeadData.LeadOut.ArcLength;
			m_numLeadOutAngle.SilentSetValue = (decimal)m_LeadData.LeadOut.Angle_deg;

			// initialize textbox
			m_chkFlip.Checked = m_LeadData.IsChangeLeadDirection;

			// for std pattern lead setting disable lead out controls
			IsStdPattern += SetLeadOutControlsEnabled;

			InitLocalizedTexts();
		}

		// the lead-in and lead-out rows share their texts
		void InitLocalizedTexts()
		{
			m_lblLeadInLineLength.Text = m_LanguageManager.TryGetString( "m_lblLeadLineLength",
				m_lblLeadInLineLength.Text );
			m_lblLeadOutLineLength.Text = m_LanguageManager.TryGetString( "m_lblLeadLineLength",
				m_lblLeadOutLineLength.Text );
			m_lblLeadInAngle.Text = m_LanguageManager.TryGetString( "m_lblAngle", m_lblLeadInAngle.Text );
			m_lblLeadOutAngle.Text = m_LanguageManager.TryGetString( "m_lblAngle", m_lblLeadOutAngle.Text );
			m_lblLeadInLength.Text = m_LanguageManager.TryGetString( "m_lblRadius", m_lblLeadInLength.Text );
			m_lblLeadOutLength.Text = m_LanguageManager.TryGetString( "m_lblRadius", m_lblLeadOutLength.Text );
		}

		public Action<bool> IsStdPattern;

		void SetLeadOutControlsEnabled( bool isEnabled )
		{
			// ensure run on UI thread
			if( this.InvokeRequired ) {
				this.Invoke( new Action( () => SetLeadOutControlsEnabled( isEnabled ) ) );
				return;
			}
			m_numLeadOutStraightLength.Enabled = isEnabled;
			m_numLeadOutStraightLength.SilentSetValue = (decimal)DEFAULT_VALUE;
			m_numLeadOutArcLength.Enabled = isEnabled;
			m_numLeadOutArcLength.SilentSetValue = (decimal)DEFAULT_VALUE;
			m_numLeadOutAngle.Enabled = isEnabled;
			m_numLeadOutAngle.SilentSetValue = (decimal)DEFAULT_ANGLE_DEG;
			m_numLeadInAngle.Enabled = isEnabled;
			m_numLeadInAngle.SilentSetValue = (decimal)DEFAULT_ANGLE_DEG;
			m_chkFlip.Enabled = isEnabled;
		}

		void m_btnOK_Click( object sender, EventArgs e )
		{
			if( IsValidParam() ) {
				SetParam();
				RaiseConfirm( m_LeadData );
			}
		}

		void PreviewLeadResult()
		{
			if( IsValidParam() ) {
				SetParam();
				RaisePreview( m_LeadData );
			}
		}

		bool IsValidParam()
		{
			double dLeadInAngle = (double)m_numLeadInAngle.Value;
			double dLeadOutAngle = (double)m_numLeadOutAngle.Value;
			if( !IsValidAngle( dLeadInAngle ) ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Error_AngleRange", ERROR_ANGLE_RANGE ), MyApp.NoticeType.Warning );
				return false;
			}
			if( !IsValidAngle( dLeadOutAngle ) ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Error_AngleRange", ERROR_ANGLE_RANGE ), MyApp.NoticeType.Warning );
				return false;
			}
			return true;
		}

		// before set param need to check params are valid
		void SetParam()
		{
			m_LeadData.LeadIn.StraightLength = (double)m_numLeadInStraightLength.Value;
			m_LeadData.LeadIn.ArcLength = (double)m_numLeadInArcLength.Value;
			m_LeadData.LeadIn.Angle_deg = (double)m_numLeadInAngle.Value;
			m_LeadData.LeadOut.StraightLength = (double)m_numLeadOutStraightLength.Value;
			m_LeadData.LeadOut.ArcLength = (double)m_numLeadOutArcLength.Value;
			m_LeadData.LeadOut.Angle_deg = (double)m_numLeadOutAngle.Value;
			m_LeadData.IsChangeLeadDirection = m_chkFlip.Checked;
		}

		#region debounced value changed event
		void m_tbxLeadIn_DebouncedValueChanged( object sender, EventArgs e )
		{
			PreviewLeadResult();
		}

		void m_tbxLeadOut_DebouncedValueChanged( object sender, EventArgs e )
		{
			PreviewLeadResult();
		}

		void m_chkFlip_CheckedChanged( object sender, EventArgs e )
		{
			PreviewLeadResult();
		}

		#endregion

		bool IsValidAngle( double dAngle )
		{
			if( dAngle < 0 || dAngle > 180 ) {
				return false;
			}
			return true;
		}

		const double DEFAULT_ANGLE_DEG = 90;
		const double DEFAULT_VALUE = 0;
		LeadData m_LeadData;

		const string ERROR_ANGLE_RANGE = "角度必須在0 ~ 180範圍內";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "LeadDlg" );
	}
}
