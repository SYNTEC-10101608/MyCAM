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
				m_NumericUpDownCutDownDistance.Value = (decimal)m_CutDownDistance;
				m_NumericUpDownFollowSafeDistance.Value = (decimal)m_FollowSafeDistance;
				m_NumericUpDownLiftUpDistance.Value = (decimal)m_LifUpDistance;
				m_NumericUpDownFrogLeapDistance.Value = (decimal)m_FrogLeapDistance;
				m_chkSafePlane.Checked = m_IsSafePlaneChecked;
				m_NumericUpDownSafePlaneDistance.Value = (decimal)m_SafePlaneDistance;
				return;
			}

			// set control value
			m_NumericUpDownCutDownDistance.Value = (decimal)Data.CutDownDistance;
			m_NumericUpDownFollowSafeDistance.Value = (decimal)Data.FollowSafeDistance;
			m_NumericUpDownLiftUpDistance.Value = (decimal)Data.LiftUpDistance;
			m_NumericUpDownFrogLeapDistance.Value = (decimal)Data.FrogLeapDistance;
			m_chkSafePlane.Checked = Data.IsSafePlaneEnable;
			m_NumericUpDownSafePlaneDistance.Value = (decimal)Data.SafePlaneDistance;

			// set field value
			m_CutDownDistance = Data.CutDownDistance;
			m_FollowSafeDistance = Data.FollowSafeDistance;
			m_LifUpDistance = Data.LiftUpDistance;
			m_FrogLeapDistance = Data.FrogLeapDistance;
			m_IsSafePlaneChecked = Data.IsSafePlaneEnable;
			m_SafePlaneDistance = Data.SafePlaneDistance;
		}

		protected override void OnShown( EventArgs e )
		{
			base.OnShown( e );
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
		}

		void m_NumericUpDownCutDownDistance_Click( object sender, EventArgs e )
		{
			SetCutDownDistance();
		}

		void m_NumericUpDownCutDownDistance_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetCutDownDistance();
		}

		void m_NumericUpDownCutDownDistance_Leave( object sender, EventArgs e )
		{
			SetCutDownDistance();
		}

		void SetCutDownDistance()
		{
			if( double.TryParse( m_NumericUpDownCutDownDistance.Text, out double cutDownDistance ) && cutDownDistance >= 0 && cutDownDistance < double.MaxValue ) {
				m_CutDownDistance = cutDownDistance;
				RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
			}
			else {
				m_NumericUpDownCutDownDistance.Text = m_CutDownDistance.ToString();
			}
		}

		void m_NumericUpDownFollowSafeDistance_Click( object sender, EventArgs e )
		{
			SetFollowSafeDistance();
		}

		void m_NumericUpDownFollowSafeDistance_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetFollowSafeDistance();
		}

		void m_NumericUpDownFollowSafeDistance_Leave( object sender, EventArgs e )
		{
			SetFollowSafeDistance();
		}

		void SetFollowSafeDistance()
		{
			if( double.TryParse( m_NumericUpDownFollowSafeDistance.Text, out double followSafeDistance ) && followSafeDistance >= 0 && followSafeDistance < double.MaxValue ) {
				m_FollowSafeDistance = followSafeDistance;
				RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
			}
			else {
				m_NumericUpDownFollowSafeDistance.Text = m_FollowSafeDistance.ToString();
			}
		}

		void m_NumericUpDownLiftUpDistance_Click( object sender, EventArgs e )
		{
			SetLiftUpDistance();
		}

		void m_NumericUpDownLiftUpDistance_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetLiftUpDistance();
		}

		void m_NumericUpDownLiftUpDistance_Leave( object sender, EventArgs e )
		{
			SetLiftUpDistance();
		}

		void SetLiftUpDistance()
		{
			if( double.TryParse( m_NumericUpDownLiftUpDistance.Text, out double liftUpDistance ) && liftUpDistance >= 0 && liftUpDistance < double.MaxValue ) {
				m_LifUpDistance = liftUpDistance;
				RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
			}
			else {
				m_NumericUpDownLiftUpDistance.Text = m_LifUpDistance.ToString();
			}
		}

		void m_NumericUpDownFrogLeapDistance_Click( object sender, EventArgs e )
		{
			SetFrogLeapDistance();
		}

		void m_NumericUpDownFrogLeapDistance_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetFrogLeapDistance();
		}

		void m_NumericUpDownFrogLeapDistance_Leave( object sender, EventArgs e )
		{
			SetFrogLeapDistance();
		}

		void SetFrogLeapDistance()
		{
			if( double.TryParse( m_NumericUpDownFrogLeapDistance.Text, out double frogLeapDistance ) && frogLeapDistance >= 0 && frogLeapDistance < double.MaxValue ) {
				m_FrogLeapDistance = frogLeapDistance;
				RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
			}
			else {
				m_NumericUpDownFrogLeapDistance.Text = m_FrogLeapDistance.ToString();
			}
		}

		void m_chkSafePlane_CheckedChanged( object sender, EventArgs e )
		{
			m_IsSafePlaneChecked = m_chkSafePlane.Checked;
			m_NumericUpDownSafePlaneDistance.Enabled = m_IsSafePlaneChecked;
			m_NumericUpDownFrogLeapDistance.Enabled = !m_IsSafePlaneChecked;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
		}

		void m_NumericUpDownSafePlaneDistance_Click( object sender, EventArgs e )
		{
			SetNumericUpDownSafePlaneDistance();
		}

		void m_NumericUpDownSafePlaneDistance_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetNumericUpDownSafePlaneDistance();
		}

		void m_NumericUpDownSafePlaneDistance_Leave( object sender, EventArgs e )
		{
			SetNumericUpDownSafePlaneDistance();
		}

		void SetNumericUpDownSafePlaneDistance()
		{
			m_SafePlaneDistance = (double)m_NumericUpDownSafePlaneDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			RaiseConfirm( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
		}

		double m_CutDownDistance = TraverseData.CUT_DOWN_DISTANCE;
		double m_FollowSafeDistance = TraverseData.FOLLOW_SAFE_DISTANCE;
		double m_LifUpDistance = TraverseData.LIFT_UP_DISTANCE;
		double m_FrogLeapDistance = TraverseData.FROG_LEAP_DISTANCE;
		double m_SafePlaneDistance = TraverseData.SAFE_PLANE_DISTANCE;
		bool m_IsSafePlaneChecked = TraverseData.IS_SAFE_PLANE_CHECKED;
	}
}
