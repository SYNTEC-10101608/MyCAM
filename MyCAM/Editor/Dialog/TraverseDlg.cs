using System;
using System.Windows.Forms;
using MyCAM.Data;

namespace MyCAM.Editor
{
	public partial class TraverseDlg : EditDialogBase<TraverseData>
	{
		public TraverseDlg( TraverseData data )
		{
			InitializeComponent();
			ApplyLanguage();
			InitializeControlAndFieldValue( data );
			UpdateFollowSafeDistanceRange();
			m_numCutDownDistance.ValueChanged += m_numCutDownDistance_ValueChanged;
		}

		void InitializeControlAndFieldValue( TraverseData Data )
		{
			m_rdoFrogLeap.Tag = TraverseData.TraverseType.FrogLeap;
			m_rdoSafePlane.Tag = TraverseData.TraverseType.SafePlane;
			m_rdoSafeCylinder.Tag = TraverseData.TraverseType.SafeCylinder;
			m_rdoSafeSphere.Tag = TraverseData.TraverseType.SafeSphere;

			SetNumericUpDownRange();
			if( Data == null ) {
				m_numCutDownDistance.Value = (decimal)m_CutDownDistance;
				m_numFollowSafeDistance.Value = (decimal)m_FollowSafeDistance;
				m_numLiftUpDistance.Value = (decimal)m_LifUpDistance;

				m_rdoFrogLeap.Checked = (TraverseData.TraverseType)m_rdoFrogLeap.Tag == m_TraverseMode;
				m_rdoSafePlane.Checked = (TraverseData.TraverseType)m_rdoSafePlane.Tag == m_TraverseMode;
				m_rdoSafeCylinder.Checked = (TraverseData.TraverseType)m_rdoSafeCylinder.Tag == m_TraverseMode;
				m_rdoSafeSphere.Checked = (TraverseData.TraverseType)m_rdoSafeSphere.Tag == m_TraverseMode;

				m_numFrogLeapDistance.Value = (decimal)m_FrogLeapDistance;
				m_numSafePlaneDistance.Value = (decimal)m_SafePlaneDistance;
				m_numSafeCylinderRadius.Value = (decimal)m_SafeCylinderRadius;
				m_numSafeSphereRadius.Value = (decimal)m_SafeSphereRadius;
				return;
			}

			// set control value
			m_numCutDownDistance.Value = (decimal)Data.CutDownDistance;
			m_numFollowSafeDistance.Value = (decimal)Data.FollowSafeDistance;
			m_numLiftUpDistance.Value = (decimal)Data.LiftUpDistance;

			m_rdoFrogLeap.Checked = (TraverseData.TraverseType)m_rdoFrogLeap.Tag == Data.TraverseMode;
			m_rdoSafePlane.Checked = (TraverseData.TraverseType)m_rdoSafePlane.Tag == Data.TraverseMode;
			m_rdoSafeCylinder.Checked = (TraverseData.TraverseType)m_rdoSafeCylinder.Tag == Data.TraverseMode;
			m_rdoSafeSphere.Checked = (TraverseData.TraverseType)m_rdoSafeSphere.Tag == Data.TraverseMode;

			m_numFrogLeapDistance.Value = (decimal)Data.FrogLeapDistance;
			m_numSafePlaneDistance.Value = (decimal)Data.SafePlaneDistance;
			m_numSafeCylinderRadius.Value = (decimal)Data.SafeCylinderRadius;
			m_numSafeSphereRadius.Value = (decimal)Data.SafeSphereRadius;

			// set field value
			m_CutDownDistance = Data.CutDownDistance;
			m_FollowSafeDistance = Data.FollowSafeDistance;
			m_LifUpDistance = Data.LiftUpDistance;
			m_TraverseMode = Data.TraverseMode;
			m_FrogLeapDistance = Data.FrogLeapDistance;
			m_SafePlaneDistance = Data.SafePlaneDistance;
			m_SafeCylinderRadius = Data.SafeCylinderRadius;
			m_SafeSphereRadius = Data.SafeSphereRadius;
		}

		void SetNumericUpDownRange()
		{
			m_numCutDownDistance.Maximum = decimal.MaxValue;
			m_numCutDownDistance.Minimum = 0;
			m_numFollowSafeDistance.Maximum = decimal.MaxValue;
			m_numFollowSafeDistance.Minimum = 0;
			m_numLiftUpDistance.Maximum = decimal.MaxValue;
			m_numLiftUpDistance.Minimum = 0;
			m_numFrogLeapDistance.Maximum = decimal.MaxValue;
			m_numFrogLeapDistance.Minimum = 0;
			m_numSafePlaneDistance.Maximum = decimal.MaxValue;
			m_numSafePlaneDistance.Minimum = decimal.MinValue;
			m_numSafeCylinderRadius.Maximum = decimal.MaxValue;
			m_numSafeCylinderRadius.Minimum = 0;
			m_numSafeSphereRadius.Maximum = decimal.MaxValue;
			m_numSafeSphereRadius.Minimum = 0;
		}

		protected override void OnShown( EventArgs e )
		{
			base.OnShown( e );
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
				m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
		}

		void TraverseMode_CheckChanged( object sender, EventArgs e )
		{
			RadioButton rbtn = sender as RadioButton;

			if( rbtn != null && rbtn.Checked ) {
				m_pnlFrogLeap.Visible = m_rdoFrogLeap.Checked;
				m_pnlSafePlane.Visible = m_rdoSafePlane.Checked;
				m_pnlSafeCylinder.Visible = m_rdoSafeCylinder.Checked;
				m_pnlSafeSphere.Visible = m_rdoSafeSphere.Checked;

				m_TraverseMode = (TraverseData.TraverseType)rbtn.Tag;
				RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
					m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
			}
		}

		void UpdateFollowSafeDistanceRange()
		{
			decimal maximum = m_numCutDownDistance.Value;
			m_numFollowSafeDistance.SilentSetValue = Math.Min( m_numFollowSafeDistance.Value, maximum );
			m_numFollowSafeDistance.Maximum = maximum;
			m_CutDownDistance = (double)maximum;
			m_FollowSafeDistance = (double)m_numFollowSafeDistance.Value;
		}

		void m_numCutDownDistance_ValueChanged( object sender, EventArgs e )
		{
			UpdateFollowSafeDistanceRange();
		}

		void m_numCutDownDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_CutDownDistance = (double)m_numCutDownDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
				m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
		}

		void m_numFollowSafeDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_FollowSafeDistance = (double)m_numFollowSafeDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
				m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
		}

		void m_numLiftUpDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_LifUpDistance = (double)m_numLiftUpDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
				m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
		}

		void m_numFrogLeapDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_FrogLeapDistance = (double)m_numFrogLeapDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
				m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
		}

		void m_numSafePlaneDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_SafePlaneDistance = (double)m_numSafePlaneDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
				m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
		}
		void m_numSafeCylinderRadius_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_SafeCylinderRadius = (double)m_numSafeCylinderRadius.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
				m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
		}
		void m_numSafeSphereRadius_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_SafeSphereRadius = (double)m_numSafeSphereRadius.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
				m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			GetValuesOfControls();
			RaiseConfirm( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
				m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
		}

		void GetValuesOfControls()
		{
			UpdateFollowSafeDistanceRange();
			m_LifUpDistance = (double)m_numLiftUpDistance.Value;
			m_FrogLeapDistance = (double)m_numFrogLeapDistance.Value;
			m_SafePlaneDistance = (double)m_numSafePlaneDistance.Value;
			m_SafeCylinderRadius = (double)m_numSafeCylinderRadius.Value;
			m_SafeSphereRadius = (double)m_numSafeSphereRadius.Value;
		}

		double m_CutDownDistance = TraverseData.CUT_DOWN_DISTANCE;
		double m_FollowSafeDistance = TraverseData.FOLLOW_SAFE_DISTANCE;
		double m_LifUpDistance = TraverseData.LIFT_UP_DISTANCE;
		TraverseData.TraverseType m_TraverseMode = TraverseData.TRAVERSE_MODE;
		double m_FrogLeapDistance = TraverseData.FROG_LEAP_DISTANCE;
		double m_SafePlaneDistance = TraverseData.SAFE_PLANE_DISTANCE;
		double m_SafeCylinderRadius = TraverseData.SAFE_CYLINDER_RADIUS;
		double m_SafeSphereRadius = TraverseData.SAFE_SPHERE_RADIUS;

	}
}
