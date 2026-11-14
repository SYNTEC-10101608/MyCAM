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
			InitializeControlAndFieldValue( data );
			UpdateFollowSafeDistanceRange();
			m_NumericUpDownCutDownDistance.ValueChanged += m_NumericUpDownCutDownDistance_ValueChanged;
			ApplyResourcesToUI();
		}

		public void ApplyResourcesToUI()
		{
			this.Text = m_LanguageManager.TryGetString( "TraverseDlg", this.Text );

			m_lblLiftUpDistance.Text = m_LanguageManager.TryGetString( "m_lblLiftUpDistance", m_lblLiftUpDistance.Text );
			m_lblCutDownDistance.Text = m_LanguageManager.TryGetString( "m_lblCutDownDistance", m_lblCutDownDistance.Text );
			m_lblFollowSaveDistance.Text = m_LanguageManager.TryGetString( "m_lblFollowSaveDistance", m_lblFollowSaveDistance.Text );

			m_rbtnFrogLeap.Text = m_LanguageManager.TryGetString( "m_rbtnFrogLeap", m_rbtnFrogLeap.Text );
			m_rbtnSafePlane.Text = m_LanguageManager.TryGetString( "m_rbtnSafePlane", m_rbtnSafePlane.Text );
			m_rbtnSafeCylinder.Text = m_LanguageManager.TryGetString( "m_rbtnSafeCylinder", m_rbtnSafeCylinder.Text );
			m_rbtnSafeSphere.Text = m_LanguageManager.TryGetString( "m_rbtnSafeSphere", m_rbtnSafeSphere.Text );

			m_gbxTraverse.Text = m_LanguageManager.TryGetString( "m_gbxTraverse", m_gbxTraverse.Text );

			m_lblFrogLeapDistance.Text = m_LanguageManager.TryGetString( "m_lblFrogLeapDistance", m_lblFrogLeapDistance.Text );
			m_lblSafePlaneDistance.Text = m_LanguageManager.TryGetString( "m_lblSafePlaneDistance", m_lblSafePlaneDistance.Text );
			m_lblSafeCylinderRadius.Text = m_LanguageManager.TryGetString( "m_lblSafeCylinderRadius", m_lblSafeCylinderRadius.Text );
			m_lblSafeSphereRadius.Text = m_LanguageManager.TryGetString( "m_lblSafeSphereRadius", m_lblSafeSphereRadius.Text );

			m_btnConfirm.Text = m_LanguageManager.TryGetString( "m_btnConfirm", m_btnConfirm.Text );
		}

		void InitializeControlAndFieldValue( TraverseData Data )
		{
			m_rbtnFrogLeap.Tag = TraverseData.TraverseType.FrogLeap;
			m_rbtnSafePlane.Tag = TraverseData.TraverseType.SafePlane;
			m_rbtnSafeCylinder.Tag = TraverseData.TraverseType.SafeCylinder;
			m_rbtnSafeSphere.Tag = TraverseData.TraverseType.SafeSphere;

			SetNumericUpDownRange();
			if( Data == null ) {
				m_NumericUpDownCutDownDistance.Value = (decimal)m_CutDownDistance;
				m_NumericUpDownFollowSafeDistance.Value = (decimal)m_FollowSafeDistance;
				m_NumericUpDownLiftUpDistance.Value = (decimal)m_LifUpDistance;

				m_rbtnFrogLeap.Checked = (TraverseData.TraverseType)m_rbtnFrogLeap.Tag == m_TraverseMode;
				m_rbtnSafePlane.Checked = (TraverseData.TraverseType)m_rbtnSafePlane.Tag == m_TraverseMode;
				m_rbtnSafeCylinder.Checked = (TraverseData.TraverseType)m_rbtnSafeCylinder.Tag == m_TraverseMode;
				m_rbtnSafeSphere.Checked = (TraverseData.TraverseType)m_rbtnSafeSphere.Tag == m_TraverseMode;

				m_NumericUpDownFrogLeapDistance.Value = (decimal)m_FrogLeapDistance;
				m_NumericUpDownSafePlaneDistance.Value = (decimal)m_SafePlaneDistance;
				m_NumericUpDownSafeCylinderRadius.Value = (decimal)m_SafeCylinderRadius;
				m_NumericUpDownSafeSphereRadius.Value = (decimal)m_SafeSphereRadius;
				return;
			}

			// set control value
			m_NumericUpDownCutDownDistance.Value = (decimal)Data.CutDownDistance;
			m_NumericUpDownFollowSafeDistance.Value = (decimal)Data.FollowSafeDistance;
			m_NumericUpDownLiftUpDistance.Value = (decimal)Data.LiftUpDistance;

			m_rbtnFrogLeap.Checked = (TraverseData.TraverseType)m_rbtnFrogLeap.Tag == Data.TraverseMode;
			m_rbtnSafePlane.Checked = (TraverseData.TraverseType)m_rbtnSafePlane.Tag == Data.TraverseMode;
			m_rbtnSafeCylinder.Checked = (TraverseData.TraverseType)m_rbtnSafeCylinder.Tag == Data.TraverseMode;
			m_rbtnSafeSphere.Checked = (TraverseData.TraverseType)m_rbtnSafeSphere.Tag == Data.TraverseMode;

			m_NumericUpDownFrogLeapDistance.Value = (decimal)Data.FrogLeapDistance;
			m_NumericUpDownSafePlaneDistance.Value = (decimal)Data.SafePlaneDistance;
			m_NumericUpDownSafeCylinderRadius.Value = (decimal)Data.SafeCylinderRadius;
			m_NumericUpDownSafeSphereRadius.Value = (decimal)Data.SafeSphereRadius;

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
			m_NumericUpDownSafeCylinderRadius.Maximum = decimal.MaxValue;
			m_NumericUpDownSafeCylinderRadius.Minimum = 0;
			m_NumericUpDownSafeSphereRadius.Maximum = decimal.MaxValue;
			m_NumericUpDownSafeSphereRadius.Minimum = 0;
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
				m_pnlFrogLeap.Visible = m_rbtnFrogLeap.Checked;
				m_pnlSafePlane.Visible = m_rbtnSafePlane.Checked;
				m_pnlSafeCylinder.Visible = m_rbtnSafeCylinder.Checked;
				m_pnlSafeSphere.Visible = m_rbtnSafeSphere.Checked;

				m_TraverseMode = (TraverseData.TraverseType)rbtn.Tag;
				RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
					m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
			}
		}

		void UpdateFollowSafeDistanceRange()
		{
			decimal maximum = m_NumericUpDownCutDownDistance.Value;
			m_NumericUpDownFollowSafeDistance.SilentSetValue = Math.Min( m_NumericUpDownFollowSafeDistance.Value, maximum );
			m_NumericUpDownFollowSafeDistance.Maximum = maximum;
			m_CutDownDistance = (double)maximum;
			m_FollowSafeDistance = (double)m_NumericUpDownFollowSafeDistance.Value;
		}

		void m_NumericUpDownCutDownDistance_ValueChanged( object sender, EventArgs e )
		{
			UpdateFollowSafeDistanceRange();
		}

		void m_NumericUpDownCutDownDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_CutDownDistance = (double)m_NumericUpDownCutDownDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
				m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
		}

		void m_NumericUpDownFollowSafeDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_FollowSafeDistance = (double)m_NumericUpDownFollowSafeDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
				m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
		}

		void m_NumericUpDownLiftUpDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_LifUpDistance = (double)m_NumericUpDownLiftUpDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
				m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
		}

		void m_NumericUpDownFrogLeapDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_FrogLeapDistance = (double)m_NumericUpDownFrogLeapDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
				m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
		}

		void m_NumericUpDownSafePlaneDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_SafePlaneDistance = (double)m_NumericUpDownSafePlaneDistance.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
				m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
		}
		void m_NumericUpDownSafeCylinderRadius_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_SafeCylinderRadius = (double)m_NumericUpDownSafeCylinderRadius.Value;
			RaisePreview( new TraverseData( m_LifUpDistance, m_CutDownDistance, m_FollowSafeDistance,
				m_TraverseMode, m_FrogLeapDistance, m_SafePlaneDistance, m_SafeCylinderRadius, m_SafeSphereRadius ) );
		}
		void m_NumericUpDownSafeSphereRadius_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_SafeSphereRadius = (double)m_NumericUpDownSafeSphereRadius.Value;
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
			m_LifUpDistance = (double)m_NumericUpDownLiftUpDistance.Value;
			m_FrogLeapDistance = (double)m_NumericUpDownFrogLeapDistance.Value;
			m_SafePlaneDistance = (double)m_NumericUpDownSafePlaneDistance.Value;
			m_SafeCylinderRadius = (double)m_NumericUpDownSafeCylinderRadius.Value;
			m_SafeSphereRadius = (double)m_NumericUpDownSafeSphereRadius.Value;
		}

		double m_CutDownDistance = TraverseData.CUT_DOWN_DISTANCE;
		double m_FollowSafeDistance = TraverseData.FOLLOW_SAFE_DISTANCE;
		double m_LifUpDistance = TraverseData.LIFT_UP_DISTANCE;
		TraverseData.TraverseType m_TraverseMode = TraverseData.TRAVERSE_MODE;
		double m_FrogLeapDistance = TraverseData.FROG_LEAP_DISTANCE;
		double m_SafePlaneDistance = TraverseData.SAFE_PLANE_DISTANCE;
		double m_SafeCylinderRadius = TraverseData.SAFE_CYLINDER_RADIUS;
		double m_SafeSphereRadius = TraverseData.SAFE_SPHERE_RADIUS;

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "TraverseDlg" );
	}
}
