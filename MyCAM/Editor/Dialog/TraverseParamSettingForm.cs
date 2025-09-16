using MyCAM.Data;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	public partial class TraverseParamSettingForm : ActionDialogBase<TraverseData>
	{
		public TraverseParamSettingForm( TraverseData Data )
		{
			InitializeComponent();
			InitializeControlValue( Data );
		}

		void InitializeControlValue( TraverseData Data )
		{
			if( Data == null ) {
				m_txbCutDownDistance.Text = m_CutDownDistance.ToString();
				m_txbFollowSafeDistance.Text = m_FollowSafeDistance.ToString();
				m_txbLiftUpDistance.Text = m_LifUpDistance.ToString();
				return;
			}

			m_txbCutDownDistance.Text = Data.CutDownDistance.ToString();
			m_txbFollowSafeDistance.Text = Data.FollowSafeDistance.ToString();
			m_txbLiftUpDistance.Text = Data.LiftUpDistance.ToString();
			m_CutDownDistance = Data.LiftUpDistance;
			m_FollowSafeDistance = Data.FollowSafeDistance;
			m_LifUpDistance = Data.LiftUpDistance;
		}

		void m_txbCutDownDistance_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetCutDownDistance();
		}

		void m_txbCutDownDistance_Leave( object sender, EventArgs e )
		{
			SetCutDownDistance();
		}

		void SetCutDownDistance()
		{
			if( double.TryParse( m_txbCutDownDistance.Text, out double toolUpDownDistance ) && toolUpDownDistance >= 0 && toolUpDownDistance < double.MaxValue && toolUpDownDistance >= m_FollowSafeDistance ) {
				m_CutDownDistance = toolUpDownDistance;
				Preview?.Invoke( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance ) );
			}
			else {
				m_txbCutDownDistance.Text = m_CutDownDistance.ToString();
			}
		}

		void m_txbSafeDistance_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetFollowSafeDistance();
		}

		void m_txbFollowSafeDistance_Leave( object sender, EventArgs e )
		{
			SetFollowSafeDistance();
		}

		void SetFollowSafeDistance()
		{
			if( double.TryParse( m_txbFollowSafeDistance.Text, out double followSafeDistance ) && followSafeDistance >= 0 && followSafeDistance < double.MaxValue && followSafeDistance <= m_CutDownDistance ) {
				m_FollowSafeDistance = followSafeDistance;
				Preview?.Invoke( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance ) );
			}
			else {
				m_txbFollowSafeDistance.Text = m_FollowSafeDistance.ToString();
			}
		}

		void m_txbLiftUpDistance_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetLiftUpDistance();
		}

		void m_txbLiftUpDistance_Leave( object sender, EventArgs e )
		{
			SetLiftUpDistance();
		}

		void SetLiftUpDistance()
		{
			if( double.TryParse( m_txbLiftUpDistance.Text, out double liftUpDistance ) && liftUpDistance >= 0 && liftUpDistance < double.MaxValue ) {
				m_LifUpDistance = liftUpDistance;
				Preview?.Invoke( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance ) );
			}
			else {
				m_txbLiftUpDistance.Text = m_LifUpDistance.ToString();
			}
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			TraverseData traverseParamData = new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance );
			Confirm?.Invoke( traverseParamData );
			m_ConfirmCheck = true;
			Close();
		}

		void TraverseParamSettingForm_FormClosing( object sender, FormClosingEventArgs e )
		{
			if( m_ConfirmCheck == false && e.CloseReason == CloseReason.UserClosing ) {
				Cancel?.Invoke();
			}
		}

		double m_CutDownDistance = 0;
		double m_FollowSafeDistance = 0;
		double m_LifUpDistance = 0;
	}
}
