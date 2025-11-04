using MyCAM.Data;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	public partial class TraverseDlg : EditDialogBase<TraverseData>
	{
		public TraverseDlg( TraverseData data )
		{
			InitializeComponent();
			InitializeControlAndFieldValue( data );
		}

		void InitializeControlAndFieldValue( TraverseData Data )
		{
			if( Data == null ) {
				m_txbCutDownDistance.Text = m_CutDownDistance.ToString( "F3" );
				m_txbFollowSafeDistance.Text = m_FollowSafeDistance.ToString( "F3" );
				m_txbLiftUpDistance.Text = m_LifUpDistance.ToString( "F3" );
				m_txbFrogLeapDistance.Text = m_FrogLeapDistance.ToString( "F3" );
				return;
			}

			// set control value
			m_txbCutDownDistance.Text = Data.CutDownDistance.ToString( "F3" );
			m_txbFollowSafeDistance.Text = Data.FollowSafeDistance.ToString( "F3" );
			m_txbLiftUpDistance.Text = Data.LiftUpDistance.ToString( "F3" );
			m_txbFrogLeapDistance.Text = Data.FrogLeapDistance.ToString( "F3" );

			// set field value
			m_CutDownDistance = Data.CutDownDistance;
			m_FollowSafeDistance = Data.FollowSafeDistance;
			m_LifUpDistance = Data.LiftUpDistance;
			m_FrogLeapDistance = Data.FrogLeapDistance;
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
			if( double.TryParse( m_txbCutDownDistance.Text, out double cutDownDistance ) && cutDownDistance >= 0 && cutDownDistance < double.MaxValue ) {
				m_CutDownDistance = cutDownDistance;
				RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance ) );
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
			if( double.TryParse( m_txbFollowSafeDistance.Text, out double followSafeDistance ) && followSafeDistance >= 0 && followSafeDistance < double.MaxValue ) {
				m_FollowSafeDistance = followSafeDistance;
				RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance ) );
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
				RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance ) );
			}
			else {
				m_txbLiftUpDistance.Text = m_LifUpDistance.ToString();
			}
		}

		void m_txbFrogLeapDistance_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetFrogLeapDistance();
		}

		void m_txbFrogLeapDistance_Leave( object sender, EventArgs e )
		{
			SetFrogLeapDistance();
		}

		void SetFrogLeapDistance()
		{
			if( double.TryParse( m_txbFrogLeapDistance.Text, out double frogLeapDistance ) && frogLeapDistance >= 0 && frogLeapDistance < double.MaxValue ) {
				m_FrogLeapDistance = frogLeapDistance;
				RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance ) );
			}
			else {
				m_txbFrogLeapDistance.Text = m_FrogLeapDistance.ToString();
			}
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			RaiseConfirm( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance ) );
		}

		double m_CutDownDistance = TraverseData.CUT_DOWN_DISTANCE;
		double m_FollowSafeDistance = TraverseData.FOLLOW_SAFE_DISTANCE;
		double m_LifUpDistance = TraverseData.LIFT_UP_DISTANCE;
		double m_FrogLeapDistance = TraverseData.FROG_LEAP_DISTANCE;
	}
}
