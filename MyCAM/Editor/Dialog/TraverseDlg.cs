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
			InitializeControlValue( data );
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
				RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance ) );
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
				RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance ) );
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
				RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance ) );
			}
			else {
				m_txbLiftUpDistance.Text = m_LifUpDistance.ToString();
			}
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			RaiseConfirm( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance ) );
		}

		double m_CutDownDistance = 0;
		double m_FollowSafeDistance = 0;
		double m_LifUpDistance = 0;
	}
}
