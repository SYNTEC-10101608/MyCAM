using System;
using System.Windows.Forms;
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
			m_LeadData = leadData;

			// lead in setting
			m_tbxLeadInLength.Text = m_LeadData.LeadIn.Length.ToString();
			m_tbxLeadInAngle.Text = m_LeadData.LeadIn.Angle.ToString();

			// set lead type must be the last step, cause it will trigger event but u need need length and angle
			switch( m_LeadData.LeadIn.Type ) {
				case LeadGeomType.Line:
					m_cbxLeadInType.SelectedIndex = (int)LeadGeomType.Line;
					break;
				case LeadGeomType.Arc:
					m_cbxLeadInType.SelectedIndex = (int)LeadGeomType.Arc;
					break;
				default:
					m_cbxLeadInType.SelectedIndex = (int)LeadGeomType.None;
					break;
			}

			// lead out setting
			m_tbxLeadOutLength.Text = m_LeadData.LeadOut.Length.ToString();
			m_tbxLeadOutAngle.Text = m_LeadData.LeadOut.Angle.ToString();

			// set lead type must be the last step, cause it will trigger event but u need need length and angle
			switch( m_LeadData.LeadOut.Type ) {
				case LeadGeomType.Line:
					m_cbxLeadOutType.SelectedIndex = (int)LeadGeomType.Line;
					break;
				case LeadGeomType.Arc:
					m_cbxLeadOutType.SelectedIndex = (int)LeadGeomType.Arc;
					break;
				default:
					m_cbxLeadOutType.SelectedIndex = (int)LeadGeomType.None;
					break;
			}

			// initialize textbox
			m_chkFlip.Checked = m_LeadData.IsChangeLeadDirection;
		}

		public LeadData LeadLindParam
		{
			get
			{
				return m_LeadData;
			}
		}

		void m_btnOK_Click( object sender, EventArgs e )
		{
			if( IsValidParam() ) {
				SetParam();
				RaiseConfirm( m_LeadData );
			}
		}

		void m_cbxLeadInType_SelectedIndexChanged( object sender, EventArgs e )
		{
			HandleLeadTypeChange( m_cbxLeadInType, m_tbxLeadInLength, m_tbxLeadInAngle, m_LeadData.LeadIn );
		}

		void m_cbxLeadOutType_SelectedIndexChanged( object sender, EventArgs e )
		{
			HandleLeadTypeChange( m_cbxLeadOutType, m_tbxLeadOutLength, m_tbxLeadOutAngle, m_LeadData.LeadOut );
		}

		void HandleLeadTypeChange( ComboBox comboBox, TextBox tbxLength, TextBox tbxAngle, LeadGeom leadData )
		{
			leadData.Type = (LeadGeomType)comboBox.SelectedIndex;

			switch( leadData.Type ) {
				case LeadGeomType.None:
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
						if( ( leadData.Type == LeadGeomType.Arc && IsValidArcAngle( dAngle ) ) ||
							( leadData.Type == LeadGeomType.Line && IsValidStraightLineAngle( dAngle ) ) ) {
							dLegalAngle = dAngle;
						}
					}
					leadData.Angle = dLegalAngle;
					tbxAngle.Text = leadData.Angle.ToString();
					break;
			}

			// combobox change means lead type changed
			RaisePreview( m_LeadData );
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
			if( !ValidateLeadParam( m_LeadData.LeadIn, m_tbxLeadInLength, m_tbxLeadInAngle, "LeadIn" ) ) {
				return false;
			}
			if( !ValidateLeadParam( m_LeadData.LeadOut, m_tbxLeadOutLength, m_tbxLeadOutAngle, "LeadOut" ) ) {
				return false;
			}
			return true;
		}

		bool ValidateLeadParam( LeadGeom leadData, TextBox tbxLength, TextBox tbxAngle, string paramName )
		{
			if( leadData.Type == LeadGeomType.None ) {
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
				case LeadGeomType.Line:
					if( !IsValidStraightLineAngle( dAngle ) ) {
						MyApp.Logger.ShowOnLogPanel( "角度必須在0 ~ 180範圍內", MyApp.NoticeType.Warning );
						return false;
					}
					break;
				case LeadGeomType.Arc:
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
			m_LeadData.LeadIn.Length = dLeadInLength;
			m_LeadData.LeadIn.Angle = dLeadInAngle;
			m_LeadData.LeadOut.Length = dLeadOutLength;
			m_LeadData.LeadOut.Angle = dLeadOutAngle;
			m_LeadData.IsChangeLeadDirection = m_chkFlip.Checked;
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
		LeadData m_LeadData;
	}
}
