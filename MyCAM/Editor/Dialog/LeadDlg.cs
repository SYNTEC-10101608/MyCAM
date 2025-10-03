using System;
using System.Windows.Forms;
using MyCAM.App;
using MyCAM.Data;

namespace MyCAM.Editor.Dialog
{
	internal partial class LeadDlg : EditDialogBase<LeadData>
	{
		public LeadDlg( LeadData leadParam )
		{
			if( leadParam == null ) {
				leadParam = new LeadData();
			}
			InitializeComponent();
			m_LeadParam = leadParam;

			// lead in setting
			m_tbxLeadInLength.Text = m_LeadParam.LeadIn.Length.ToString();
			m_tbxLeadInAngle.Text = m_LeadParam.LeadIn.Angle.ToString();

			// set lead type must be the last step, cause it will trigger event but u need need length and angle
			switch( m_LeadParam.LeadIn.Type ) {
				case LeadLineType.Line:
					m_cbxLeadInType.SelectedIndex = (int)LeadLineType.Line;
					break;
				case LeadLineType.Arc:
					m_cbxLeadInType.SelectedIndex = (int)LeadLineType.Arc;
					break;
				default:
					m_cbxLeadInType.SelectedIndex = (int)LeadLineType.None;
					break;
			}

			// lead out setting
			m_tbxLeadOutLength.Text = m_LeadParam.LeadOut.Length.ToString();
			m_tbxLeadOutAngle.Text = m_LeadParam.LeadOut.Angle.ToString();

			// set lead type must be the last step, cause it will trigger event but u need need length and angle
			switch( m_LeadParam.LeadOut.Type ) {
				case LeadLineType.Line:
					m_cbxLeadOutType.SelectedIndex = (int)LeadLineType.Line;
					break;
				case LeadLineType.Arc:
					m_cbxLeadOutType.SelectedIndex = (int)LeadLineType.Arc;
					break;
				default:
					m_cbxLeadOutType.SelectedIndex = (int)LeadLineType.None;
					break;
			}

			// initialize textbox
			m_chkFlip.Checked = m_LeadParam.IsChangeLeadDirection;
		}

		public LeadData LeadLindParam
		{
			get
			{
				return m_LeadParam;
			}
		}

		void m_btnOK_Click( object sender, EventArgs e )
		{
			if( IsValidParam() ) {
				SetParam();
				RaiseConfirm( m_LeadParam );
			}
		}

		void m_cbxLeadInType_SelectedIndexChanged( object sender, EventArgs e )
		{
			HandleLeadTypeChange( m_cbxLeadInType, m_tbxLeadInLength, m_tbxLeadInAngle, m_LeadParam.LeadIn );
		}

		void m_cbxLeadOutType_SelectedIndexChanged( object sender, EventArgs e )
		{
			HandleLeadTypeChange( m_cbxLeadOutType, m_tbxLeadOutLength, m_tbxLeadOutAngle, m_LeadParam.LeadOut );
		}

		void HandleLeadTypeChange( ComboBox comboBox, TextBox tbxLength, TextBox tbxAngle, LeadParam leadData )
		{
			leadData.Type = (LeadLineType)comboBox.SelectedIndex;

			switch( leadData.Type ) {
				case LeadLineType.None:
					tbxLength.Enabled = false;
					tbxAngle.Enabled = false;
					break;
				default:
					tbxLength.Enabled = true;
					tbxAngle.Enabled = true;

					// set length
					if( !double.TryParse( tbxLength.Text, out double dLength ) || dLength <= 0 ) {
						dLength = DEFAULT_Length;
					}
					leadData.Length = dLength;
					tbxLength.Text = leadData.Length.ToString();

					// default angle
					double dLegalAngle = DEFAULT_Angle;

					// get legal param
					if( double.TryParse( tbxAngle.Text, out double dAngle ) ) {
						if( ( leadData.Type == LeadLineType.Arc && IsValidArcAngle( dAngle ) ) ||
							( leadData.Type == LeadLineType.Line && IsValidStraightLineAngle( dAngle ) ) ) {
							dLegalAngle = dAngle;
						}
					}
					leadData.Angle = dLegalAngle;
					tbxAngle.Text = leadData.Angle.ToString();
					break;
			}

			// combobox change means lead type changed
			RaisePreview( m_LeadParam );
		}

		void PreviewLeadResult()
		{
			if( IsValidParam() ) {
				SetParam();
				RaisePreview( m_LeadParam );
			}
		}

		bool IsValidParam()
		{
			if( !ValidateLeadParam( m_LeadParam.LeadIn, m_tbxLeadInLength, m_tbxLeadInAngle, "LeadIn" ) ) {
				return false;
			}
			if( !ValidateLeadParam( m_LeadParam.LeadOut, m_tbxLeadOutLength, m_tbxLeadOutAngle, "LeadOut" ) ) {
				return false;
			}
			return true;
		}

		bool ValidateLeadParam( LeadParam leadData, TextBox tbxLength, TextBox tbxAngle, string paramName )
		{
			if( leadData.Type == LeadLineType.None ) {
				leadData.Length = DEFAULT_Value;
				leadData.Angle = DEFAULT_Value;
				return true;
			}
			if( !double.TryParse( tbxLength.Text, out double dLength ) ||
				!double.TryParse( tbxAngle.Text, out double dAngle ) ) {
				MyApp.Logger.ShowOnLogPanel( $"{paramName}:無效的數字格式", MyApp.NoticeType.Warning );
				return false;
			}
			if( dLength <= 0 ) {
				MyApp.Logger.ShowOnLogPanel( "長度需要大於0", MyApp.NoticeType.Warning );
				return false;
			}
			switch( leadData.Type ) {
				case LeadLineType.Line:
					if( !IsValidStraightLineAngle( dAngle ) ) {
						MyApp.Logger.ShowOnLogPanel( "角度必須在0 ~ 180範圍內", MyApp.NoticeType.Warning );
						return false;
					}
					break;
				case LeadLineType.Arc:
					if( !IsValidArcAngle( dAngle ) ) {
						MyApp.Logger.ShowOnLogPanel( "角度必須在0 ~ 180範圍內", MyApp.NoticeType.Warning );
						return false;
					}
					break;
			}
			return true;
		}

		// before set param need to check params are valid
		void SetParam()
		{
			double.TryParse( m_tbxLeadInLength.Text, out double dLeadInLength );
			double.TryParse( m_tbxLeadInAngle.Text, out double dLeadInAngle );
			double.TryParse( m_tbxLeadOutLength.Text, out double dLeadOutLength );
			double.TryParse( m_tbxLeadOutAngle.Text, out double dLeadOutAngle );
			m_LeadParam.LeadIn.Length = dLeadInLength;
			m_LeadParam.LeadIn.Angle = dLeadInAngle;
			m_LeadParam.LeadOut.Length = dLeadOutLength;
			m_LeadParam.LeadOut.Angle = dLeadOutAngle;
			m_LeadParam.IsChangeLeadDirection = m_chkFlip.Checked;
		}

		#region key in event

		void m_tbxLeadInLength_KeyDown( object sender, KeyEventArgs e )
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

		void m_tbxLeadInAngle_Leave( object sender, EventArgs e )
		{
			PreviewLeadResult();
		}

		void m_tbxLeadInLength_Leave( object sender, EventArgs e )
		{
			PreviewLeadResult();
		}

		void m_tbxLeadOutLength_KeyDown( object sender, KeyEventArgs e )
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

		void m_tbxLeadOutLength_Leave( object sender, EventArgs e )
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
		LeadData m_LeadParam;
	}
}
