using MyCAM.App;
using MyCAM.Data;
using System;
using System.Windows.Forms;

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
			m_LeadData = leadData;

			// lead in setting
			m_tbxLeadInStraightLength.SilentSetValue = (decimal)m_LeadData.LeadIn.StraightLength;
			m_tbxLeadInArcLength.SilentSetValue = (decimal)m_LeadData.LeadIn.ArcLength;
			m_tbxLeadInAngle.SilentSetValue = (decimal)m_LeadData.LeadIn.Angle_deg;

			// lead out setting
			m_tbxLeadOutStraightLength.SilentSetValue = (decimal)m_LeadData.LeadOut.StraightLength;
			m_tbxLeadOutArcLength.SilentSetValue = (decimal)m_LeadData.LeadOut.ArcLength;
			m_tbxLeadOutAngle.SilentSetValue = (decimal)m_LeadData.LeadOut.Angle_deg;

			// initialize textbox
			m_chkFlip.Checked = m_LeadData.IsChangeLeadDirection;

			// for std pattern lead setting disable lead out controls
			IsStdPattern += SetLeadOutControlsEnabled;
		}

		public Action<bool> IsStdPattern;

		void SetLeadOutControlsEnabled( bool isEnabled )
		{
			// ensure run on UI thread
			if( this.InvokeRequired ) {
				this.Invoke( new Action( () => SetLeadOutControlsEnabled( isEnabled ) ) );
				return;
			}
			m_tbxLeadOutStraightLength.Enabled = isEnabled;
			m_tbxLeadOutStraightLength.SilentSetValue = (decimal)DEFAULT_VALUE;
			m_tbxLeadOutArcLength.Enabled = isEnabled;
			m_tbxLeadOutArcLength.SilentSetValue = (decimal)DEFAULT_VALUE;
			m_tbxLeadOutAngle.Enabled = isEnabled;
			m_tbxLeadOutAngle.SilentSetValue = (decimal)DEFAULT_ANGLE_DEG;
			m_tbxLeadInAngle.Enabled = isEnabled;
			m_tbxLeadInAngle.SilentSetValue = (decimal)DEFAULT_ANGLE_DEG;
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
			double dLeadInAngle = (double)m_tbxLeadInAngle.Value;
			double dLeadOutAngle = (double)m_tbxLeadOutAngle.Value;
			if( !IsValidAngle( dLeadInAngle ) ) {
				MyApp.Logger.ShowOnLogPanel( "角度必須在0 ~ 180範圍內", MyApp.NoticeType.Warning );
				return false;
			}
			if( !IsValidAngle( dLeadOutAngle ) ) {
				MyApp.Logger.ShowOnLogPanel( "角度必須在0 ~ 180範圍內", MyApp.NoticeType.Warning );
				return false;
			}
			return true;
		}

		// before set param need to check params are valid
		void SetParam()
		{
			m_LeadData.LeadIn.StraightLength = (double)m_tbxLeadInStraightLength.Value;
			m_LeadData.LeadIn.ArcLength = (double)m_tbxLeadInArcLength.Value;
			m_LeadData.LeadIn.Angle_deg = (double)m_tbxLeadInAngle.Value;
			m_LeadData.LeadOut.StraightLength = (double)m_tbxLeadOutStraightLength.Value;
			m_LeadData.LeadOut.ArcLength = (double)m_tbxLeadOutArcLength.Value;
			m_LeadData.LeadOut.Angle_deg = (double)m_tbxLeadOutAngle.Value;
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


	}
}
