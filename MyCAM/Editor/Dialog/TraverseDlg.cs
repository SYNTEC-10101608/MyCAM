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
			SetNumericUpDownRange();
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

		void SetNumericUpDownRange()
		{
			m_NumericUpDownCutDownDistance.Maximum = decimal.MaxValue;
			m_NumericUpDownCutDownDistance.Minimum = 0;
			m_NumericUpDownFollowSafeDistance.Maximum = decimal.MaxValue;
			m_NumericUpDownFollowSafeDistance.Minimum = 0;
			m_NumericUpDownLiftUpDistance.Maximum = decimal.MaxValue;
			m_NumericUpDownLiftUpDistance.Minimum = 0;
			m_NumericUpDownFrogLeapDistance.Maximum = decimal.MaxValue;
			m_NumericUpDownFrogLeapDistance.Minimum = 0;
			m_NumericUpDownSafePlaneDistance.Maximum = decimal.MaxValue;
			m_NumericUpDownSafePlaneDistance.Minimum = decimal.MinValue;
		}

		protected override void OnShown( EventArgs e )
		{
			base.OnShown( e );
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
		}

		void m_NumericUpDownCutDownDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_CutDownDistance = (double)m_NumericUpDownCutDownDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
		}

		void m_NumericUpDownFollowSafeDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_FollowSafeDistance = (double)m_NumericUpDownFollowSafeDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
		}

		void m_NumericUpDownLiftUpDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_LifUpDistance = (double)m_NumericUpDownLiftUpDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
		}

		void m_NumericUpDownFrogLeapDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_FrogLeapDistance = (double)m_NumericUpDownFrogLeapDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
		}

		void m_chkSafePlane_CheckedChanged( object sender, EventArgs e )
		{
			m_IsSafePlaneChecked = m_chkSafePlane.Checked;
			m_NumericUpDownSafePlaneDistance.Enabled = m_IsSafePlaneChecked;
			m_NumericUpDownFrogLeapDistance.Enabled = !m_IsSafePlaneChecked;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
		}

		void m_NumericUpDownSafePlaneDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_SafePlaneDistance = (double)m_NumericUpDownSafePlaneDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			GetValuesOfControls();
			RaiseConfirm( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance, m_FrogLeapDistance, m_IsSafePlaneChecked, m_SafePlaneDistance ) );
		}

		void GetValuesOfControls()
		{
			m_CutDownDistance = (double)m_NumericUpDownCutDownDistance.Value;
			m_FollowSafeDistance = (double)m_NumericUpDownFollowSafeDistance.Value;
			m_LifUpDistance = (double)m_NumericUpDownLiftUpDistance.Value;
			m_FrogLeapDistance = (double)m_NumericUpDownFrogLeapDistance.Value;
			m_IsSafePlaneChecked = m_chkSafePlane.Checked;
			m_SafePlaneDistance = (double)m_NumericUpDownSafePlaneDistance.Value;
		}

		double m_CutDownDistance = TraverseData.CUT_DOWN_DISTANCE;
		double m_FollowSafeDistance = TraverseData.FOLLOW_SAFE_DISTANCE;
		double m_LifUpDistance = TraverseData.LIFT_UP_DISTANCE;
		double m_FrogLeapDistance = TraverseData.FROG_LEAP_DISTANCE;
		double m_SafePlaneDistance = TraverseData.SAFE_PLANE_DISTANCE;
		bool m_IsSafePlaneChecked = TraverseData.IS_SAFE_PLANE_CHECKED;
	}
}
