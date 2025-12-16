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
			m_tbxLeadInStraightLength.Text = m_LeadData.LeadIn.StraightLength.ToString();
			m_tbxLeadInArcLength.Text = m_LeadData.LeadIn.ArcLength.ToString();
			m_tbxLeadInAngle.Text = m_LeadData.LeadIn.Angle.ToString();

			// lead out setting
			m_tbxLeadOutStraightLength.Text = m_LeadData.LeadOut.StraightLength.ToString();
			m_tbxLeadOutArcLength.Text = m_LeadData.LeadOut.ArcLength.ToString();
			m_tbxLeadOutAngle.Text = m_LeadData.LeadOut.Angle.ToString();

			// initialize textbox
			m_chkFlip.Checked = m_LeadData.IsChangeLeadDirection;

			IsStdPattern += SetLeadOutControlsEnabled;
		}

		public Action<bool> IsStdPattern;

		void SetLeadOutControlsEnabled( bool isEnabled )
		{
			if( this.InvokeRequired ) {
				this.Invoke( new Action( () => SetLeadOutControlsEnabled( isEnabled ) ) );
				return;
			}
			m_tbxLeadOutStraightLength.Enabled = isEnabled;
			m_tbxLeadOutArcLength.Enabled = isEnabled;
			m_tbxLeadOutAngle.Enabled = isEnabled;
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
			if( !ValidateLeadParam( m_LeadData.LeadIn, m_tbxLeadInStraightLength, m_tbxLeadInArcLength, m_tbxLeadInAngle, "LeadIn" ) ) {
				return false;
			}
			if( !ValidateLeadParam( m_LeadData.LeadOut, m_tbxLeadOutStraightLength, m_tbxLeadOutArcLength, m_tbxLeadOutAngle, "LeadOut" ) ) {
				return false;
			}
			return true;
		}

		bool ValidateLeadParam( LeadGeom leadData, TextBox tbxStraightLength, TextBox tbxArcLength, TextBox tbxAngle, string paramName )
		{
			if( leadData.StraightLength == DEFAULT_Value ) {
				leadData.ArcLength = DEFAULT_Value;
				leadData.Angle = DEFAULT_Value;
				return true;
			}
			if( !double.TryParse( tbxStraightLength.Text, out double dStraightLength ) ||
				!double.TryParse( tbxArcLength.Text, out double dArcLength ) ||
				!double.TryParse( tbxAngle.Text, out double dAngle ) ) {
				MyApp.Logger.ShowOnLogPanel( $"{paramName}:無效的數字格式", MyApp.NoticeType.Warning );
				return false;
			}
			if( dStraightLength < 0 || dArcLength < 0 ) {
				MyApp.Logger.ShowOnLogPanel( "長度需要大於等於0", MyApp.NoticeType.Warning );
				return false;
			}
			if( !IsValidStraightLineAngle( dAngle ) ) {
				MyApp.Logger.ShowOnLogPanel( "角度必須在0 ~ 180範圍內", MyApp.NoticeType.Warning );
				return false;
			}
			return true;
		}

		// before set param need to check params are valid
		void SetParam()
		{
			double.TryParse( m_tbxLeadInStraightLength.Text, out double dLeadInStraightLength );
			double.TryParse( m_tbxLeadInArcLength.Text, out double dLeadInArcLength );
			double.TryParse( m_tbxLeadInAngle.Text, out double dLeadInAngle );
			double.TryParse( m_tbxLeadOutStraightLength.Text, out double dLeadOutStraightLength );
			double.TryParse( m_tbxLeadOutArcLength.Text, out double dLeadOutArcLength );
			double.TryParse( m_tbxLeadOutAngle.Text, out double dLeadOutAngle );
			m_LeadData.LeadIn.StraightLength = dLeadInStraightLength;
			m_LeadData.LeadIn.ArcLength = dLeadInArcLength;
			m_LeadData.LeadIn.Angle = dLeadInAngle;
			m_LeadData.LeadOut.StraightLength = dLeadOutStraightLength;
			m_LeadData.LeadOut.ArcLength = dLeadOutArcLength;
			m_LeadData.LeadOut.Angle = dLeadOutAngle;
			m_LeadData.IsChangeLeadDirection = m_chkFlip.Checked;
		}

		#region key in event
		void m_tbxLeadInStraightLength_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				PreviewLeadResult();
			}
		}

		void m_tbxLeadInArcLength_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				PreviewLeadResult();
			}
		}

		void m_tbxLeadInAngle_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				PreviewLeadResult();
			}
		}

		void m_tbxLeadOutStraightLength_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				PreviewLeadResult();
			}
		}

		void m_tbxLeadOutArcLength_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				PreviewLeadResult();
			}
		}

		void m_tbxLeadOutAngle_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				PreviewLeadResult();
			}
		}
		void m_tbxLeadInStraightLength_Leave( object sender, EventArgs e )
		{
			PreviewLeadResult();
		}

		void m_tbxLeadInArcLength_Leave( object sender, EventArgs e )
		{
			PreviewLeadResult();
		}

		void m_tbxLeadInAngle_Leave( object sender, EventArgs e )
		{
			PreviewLeadResult();
		}

		void m_tbxLeadOutStraightLength_Leave( object sender, EventArgs e )
		{
			PreviewLeadResult();
		}

		void m_tbxLeadOutArcLength_Leave( object sender, EventArgs e )
		{
			PreviewLeadResult();
		}

		void m_tbxLeadOutAngle_Leave( object sender, EventArgs e )
		{
			PreviewLeadResult();
		}

		void m_chkFlip_CheckedChanged( object sender, EventArgs e )
		{
			PreviewLeadResult();
		}

		#endregion

		bool IsValidArcAngle( double dAngle )
		{
			if( dAngle <= 0 || dAngle > 180 ) {
				return false;
			}
			return true;
		}

		bool IsValidStraightLineAngle( double dAngle )
		{
			if( dAngle < 0 || dAngle > 180 ) {
				return false;
			}
			return true;
		}

		const double DEFAULT_Value = 0;
		const double DEFAULT_Angle = 45;
		const double DEFAULT_Length = 3;
		LeadData m_LeadData;


	}
}
