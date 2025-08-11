using System;
using System.Windows.Forms;
using MyCAM.Data;

namespace MyCAM.Editor.Dialog
{
	internal partial class LeadLineForm : Form
	{
		public Action Preview;
		public Action OnComfirm;
		public Action OnCancel;

		public LeadLineForm( LeadData leadParam )
		{
			InitializeComponent();
			m_LeadParam = leadParam;

			// lead in setting
			switch( m_LeadParam.LeadIn.Type ) {
				case LeadType.LeadLineType.Line:
					m_cbxLeadInType.SelectedIndex = (int)LeadType.LeadLineType.Line;
					m_tbxLeadInLength.Text = m_LeadParam.LeadIn.Length.ToString();
					m_tbxLeadInAngle.Text = m_LeadParam.LeadIn.Angle.ToString();
					break;
				case LeadType.LeadLineType.Arc:
					m_cbxLeadInType.SelectedIndex = (int)LeadType.LeadLineType.Arc;
					m_tbxLeadInLength.Text = m_LeadParam.LeadIn.Length.ToString();
					m_tbxLeadInAngle.Text = m_LeadParam.LeadIn.Angle.ToString();
					break;
				default:
					m_cbxLeadInType.SelectedIndex = (int)LeadType.LeadLineType.None;
					m_tbxLeadInLength.Text = DEFAULT_Value.ToString();
					m_tbxLeadInAngle.Text = DEFAULT_Value.ToString();
					break;
			}

			// lead out setting
			switch( m_LeadParam.LeadOut.Type ) {
				case LeadType.LeadLineType.Line:
					m_cbxLeadOutType.SelectedIndex = (int)LeadType.LeadLineType.Line;
					m_tbxLeadOutLength.Text = m_LeadParam.LeadOut.Length.ToString();
					m_tbxLeadOutAngle.Text = m_LeadParam.LeadOut.Angle.ToString();
					break;
				case LeadType.LeadLineType.Arc:
					m_cbxLeadOutType.SelectedIndex = (int)LeadType.LeadLineType.Arc;
					m_tbxLeadOutLength.Text = m_LeadParam.LeadOut.Length.ToString();
					m_tbxLeadOutAngle.Text = m_LeadParam.LeadOut.Angle.ToString();
					break;
				default:
					m_cbxLeadOutType.SelectedIndex = (int)LeadType.LeadLineType.None;
					m_tbxLeadOutLength.Text = DEFAULT_Value.ToString();
					m_tbxLeadOutAngle.Text = DEFAULT_Value.ToString();
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

		bool m_IsConfirmed = false;
		const double DEFAULT_Value = 0;
		LeadData m_LeadParam;
		const double DEFAULT_Angle = 45;
		const double DEFAULT_Length = 3;


		void m_btnOK_Click( object sender, EventArgs e )
		{
			if( IsValidParam() ) {
				SetParam();
				m_IsConfirmed = true;
				OnComfirm?.Invoke();
				Close();
			}
		}

		void m_cbxLeadInType_SelectedIndexChanged( object sender, EventArgs e )
		{
			m_LeadParam.LeadIn.Type = (LeadType.LeadLineType)m_cbxLeadInType.SelectedIndex;
			switch( m_LeadParam.LeadIn.Type ) {
				case LeadType.LeadLineType.None:
					m_tbxLeadInLength.Enabled = false;
					m_tbxLeadInAngle.Enabled = false;
					break;
				default:
					m_tbxLeadInLength.Enabled = true;
					m_tbxLeadInAngle.Enabled = true;

					// set length
					if( !double.TryParse( m_tbxLeadInLength.Text, out double dLeadInLength ) || dLeadInLength <= 0 ) {
						dLeadInLength = DEFAULT_Length;
					}
					m_LeadParam.LeadIn.Length = dLeadInLength;
					m_tbxLeadInLength.Text = m_LeadParam.LeadIn.Length.ToString();

					// default angle
					double dLegalAngle = DEFAULT_Angle;

					// get legal param
					if( double.TryParse( m_tbxLeadInAngle.Text, out double dLeadInAngle ) ) {
						if( ( m_LeadParam.LeadIn.Type == LeadType.LeadLineType.Arc && IsValidArcAngle( dLeadInAngle ) ) ||
							( m_LeadParam.LeadIn.Type == LeadType.LeadLineType.Line && IsValidStraightLineAngle( dLeadInAngle ) ) ) {
							dLegalAngle = dLeadInAngle;
						}
					}
					m_LeadParam.LeadIn.Angle = dLegalAngle;
					m_tbxLeadInAngle.Text = m_LeadParam.LeadIn.Angle.ToString();
					break;
			}

			// combobox change means lead type changed 
			Preview?.Invoke();
		}

		void m_cbxLeadOutType_SelectedIndexChanged( object sender, EventArgs e )
		{
			m_LeadParam.LeadOut.Type = (LeadType.LeadLineType)m_cbxLeadOutType.SelectedIndex;
			switch( m_LeadParam.LeadOut.Type ) {
				case LeadType.LeadLineType.None:
					m_tbxLeadOutLength.Enabled = false;
					m_tbxLeadOutAngle.Enabled = false;
					break;
				default:
					m_tbxLeadOutLength.Enabled = true;
					m_tbxLeadOutAngle.Enabled = true;

					// set length
					if( !double.TryParse( m_tbxLeadOutLength.Text, out double dLeadOutLength ) || dLeadOutLength <= 0 ) {
						dLeadOutLength = DEFAULT_Length;
					}
					m_LeadParam.LeadOut.Length = dLeadOutLength;
					m_tbxLeadOutLength.Text = m_LeadParam.LeadOut.Length.ToString();

					// default angle
					double dLegalAngle = DEFAULT_Angle;

					// get legal param
					if( double.TryParse( m_tbxLeadOutAngle.Text, out double dLeadInAngle ) ) {
						if( ( m_LeadParam.LeadOut.Type == LeadType.LeadLineType.Arc && IsValidArcAngle( dLeadInAngle ) ) ||
							( m_LeadParam.LeadOut.Type == LeadType.LeadLineType.Line && IsValidStraightLineAngle( dLeadInAngle ) ) ) {
							dLegalAngle = dLeadInAngle;
						}
					}
					m_LeadParam.LeadOut.Angle = dLegalAngle;
					m_tbxLeadOutAngle.Text = m_LeadParam.LeadOut.Angle.ToString();
					break;
			}

			// combobox change means lead type changed 
			Preview?.Invoke();
		}

		void PreviewLeadResult()
		{
			if( IsValidParam() ) {
				SetParam();
				Preview?.Invoke();
			}
		}

		bool IsValidParam()
		{
			m_LeadParam.IsChangeLeadDirection = m_chkFlip.Checked;

			// check leadin param
			if( m_LeadParam.LeadIn.Type == LeadType.LeadLineType.None ) {
				m_LeadParam.LeadIn.Length = DEFAULT_Value;
				m_LeadParam.LeadIn.Angle = DEFAULT_Value;
			}
			else {

				if( !double.TryParse( m_tbxLeadInLength.Text, out double dLeadInLength )
					|| !double.TryParse( m_tbxLeadInAngle.Text, out double dLeadInAngle ) ) {
					MessageBox.Show( "invalid string" );
					return false;
				}

				if( dLeadInLength <= 0 ) {
					MessageBox.Show( "length must be greater than 0" );
					return false;
				}

				// check angle
				switch( m_LeadParam.LeadIn.Type ) {
					case LeadType.LeadLineType.Line:
						if( !IsValidStraightLineAngle( dLeadInAngle ) ) {
							MessageBox.Show( "angle must be in range (0 ~ 180)" );
							return false;
						}
						break;
					case LeadType.LeadLineType.Arc:
						if( !IsValidArcAngle( dLeadInAngle ) ) {
							MessageBox.Show( " angle needs to be greater than 0 and less than 180" );
							return false;
						}
						break;
					default:
						break;
				}
			}

			// check leadout param
			if( m_LeadParam.LeadOut.Type == LeadType.LeadLineType.None ) {
				m_LeadParam.LeadOut.Length = DEFAULT_Value;
				m_LeadParam.LeadOut.Angle = DEFAULT_Value;
			}
			else {

				if( !double.TryParse( m_tbxLeadOutLength.Text, out double dLeadOutLength )
					|| !double.TryParse( m_tbxLeadOutAngle.Text, out double dLeadOutAngle ) ) {
					MessageBox.Show( "invalid string" );
					return false;
				}

				if( dLeadOutLength <= 0 ) {
					MessageBox.Show( "length must be greater than 0" );
					return false;
				}

				// check angle
				switch( m_LeadParam.LeadOut.Type ) {
					case LeadType.LeadLineType.Line:
						if( !IsValidStraightLineAngle( dLeadOutAngle ) ) {
							MessageBox.Show( "angle must be in range (0 ~ 180)" );
							return false;
						}
						break;
					case LeadType.LeadLineType.Arc:
						if( !IsValidArcAngle( dLeadOutAngle ) ) {
							MessageBox.Show( " angle needs to be greater than 0 and less than 180" );
							return false;
						}
						break;
					default:
						break;
				}
			}
			return true;
		}

		// before set param need to check params are valid
		void SetParam()
		{
			double.TryParse( m_tbxLeadInLength.Text, out m_LeadParam.LeadIn.Length );
			double.TryParse( m_tbxLeadInAngle.Text, out m_LeadParam.LeadIn.Angle );
			double.TryParse( m_tbxLeadOutLength.Text, out m_LeadParam.LeadOut.Length );
			double.TryParse( m_tbxLeadOutAngle.Text, out m_LeadParam.LeadOut.Angle );
			m_LeadParam.IsChangeLeadDirection = m_chkFlip.Checked;
		}

		void LeadLineForm_FormClosing( object sender, FormClosingEventArgs e )
		{
			if( !m_IsConfirmed && e.CloseReason == CloseReason.UserClosing ) {
				OnCancel?.Invoke();
			}
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
	}
}
