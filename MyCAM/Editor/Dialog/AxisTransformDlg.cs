using System;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
{
	public partial class AxisTransformDlg : Form
	{
		internal Action<decimal, EAxisTransformDirection> Displace;
		internal Action<decimal, EAxisTransformDirection> Rotate;
		internal Action<decimal, EAxisTransformDirection> Reset;
		internal Action<decimal, EAxisTransformDirection> ResetR;
		internal Action FormClose;
		internal decimal m_OldValueX = 0;
		internal decimal m_OldValueY = 0;
		internal decimal m_OldValueZ = 0;
		internal decimal m_OldValueRx = 0;
		internal decimal m_OldValueRy = 0;
		internal decimal m_OldValueRz = 0;
		internal decimal m_RotationIncrement = 1;
		internal decimal m_DisplacementIncrement = 1;
		const decimal ANGLE_RANGE_DEG = 360;

		public AxisTransformDlg()
		{
			InitializeComponent();
			InitialControlBoundarySetting();
			InitialControlValue();
		}

		void InitialControlBoundarySetting()
		{
			this.m_nudRx.Maximum = ANGLE_RANGE_DEG;
			this.m_nudRx.Minimum = -ANGLE_RANGE_DEG;
			this.m_nudRy.Maximum = ANGLE_RANGE_DEG;
			this.m_nudRy.Minimum = -ANGLE_RANGE_DEG;
			this.m_nudRz.Maximum = ANGLE_RANGE_DEG;
			this.m_nudRz.Minimum = -ANGLE_RANGE_DEG;
			this.m_nudX.Maximum = decimal.MaxValue;
			this.m_nudX.Minimum = decimal.MinValue;
			this.m_nudY.Maximum = decimal.MaxValue;
			this.m_nudY.Minimum = decimal.MinValue;
			this.m_nudZ.Maximum = decimal.MaxValue;
			this.m_nudZ.Minimum = decimal.MinValue;
		}

		void InitialControlValue()
		{
			m_txbStepDistance.Text = m_DisplacementIncrement.ToString();
			m_txbStepAngle.Text = m_RotationIncrement.ToString();
		}

		void m_nudX_MouseWheel( object sender, MouseEventArgs e )
		{
			Displacement( sender, e, EAxisTransformDirection.XAxis );
		}

		void m_nudY_MouseWheel( object sender, MouseEventArgs e )
		{
			Displacement( sender, e, EAxisTransformDirection.YAxis );
		}

		void m_nudZ_MouseWheel( object sender, MouseEventArgs e )
		{
			Displacement( sender, e, EAxisTransformDirection.ZAxis );
		}

		void m_nudRx_MouseWheel( object sender, MouseEventArgs e )
		{
			Rotation( sender, e, EAxisTransformDirection.XAxis );
		}

		void m_nudRy_MouseWheel( object sender, MouseEventArgs e )
		{
			Rotation( sender, e, EAxisTransformDirection.YAxis );
		}

		void m_nudRz_MouseWheel( object sender, MouseEventArgs e )
		{
			Rotation( sender, e, EAxisTransformDirection.ZAxis );
		}

		void Rotation( object sender, MouseEventArgs e, EAxisTransformDirection axis )
		{
			// set the handled to ensure that the mouse wheel event does not propagate further
			if( e is HandledMouseEventArgs handledE ) {
				handledE.Handled = true;
			}

			// determine the rotation direction and increment value
			decimal angleIncrement = 0;
			if( e.Delta > 0 ) {
				angleIncrement = m_RotationIncrement;
			}
			else if( e.Delta < 0 ) {
				angleIncrement = -m_RotationIncrement;
			}

			// check the boundary
			if( ( (NumericUpDown)sender ).Value + angleIncrement > ( (NumericUpDown)sender ).Maximum
				|| ( (NumericUpDown)sender ).Value + angleIncrement < ( (NumericUpDown)sender ).Minimum ) {
				return;
			}
			( (NumericUpDown)sender ).Value += angleIncrement;
			Rotate?.Invoke( angleIncrement, axis );
		}

		void Displacement( object sender, MouseEventArgs e, EAxisTransformDirection axis )
		{
			if( e is HandledMouseEventArgs handledE ) {
				handledE.Handled = true;
			}

			// determine the displacement direction and increment value
			decimal displacementIncrement = 0;
			if( e.Delta > 0 ) {
				displacementIncrement = m_DisplacementIncrement;
			}
			else if( e.Delta < 0 ) {
				displacementIncrement = -m_DisplacementIncrement;
			}

			// check the boundary
			try {
				( (NumericUpDown)sender ).Value += displacementIncrement;
			}
			catch {
				return;
			}
			Displace?.Invoke( displacementIncrement, axis );
		}

		void m_btnResetX_Click( object sender, EventArgs e )
		{
			Reset?.Invoke( -( m_nudX.Value ), EAxisTransformDirection.XAxis );
			m_nudX.Value = m_OldValueX;
		}

		void m_btnResetY_Click( object sender, EventArgs e )
		{
			Reset?.Invoke( -( m_nudY.Value ), EAxisTransformDirection.YAxis );
			m_nudY.Value = m_OldValueY;
		}

		void m_btnResetZ_Click( object sender, EventArgs e )
		{
			Reset?.Invoke( -( m_nudZ.Value ), EAxisTransformDirection.ZAxis );
			m_nudZ.Value = m_OldValueZ;
		}

		void m_btnResetRx_Click( object sender, EventArgs e )
		{
			ResetR?.Invoke( -( m_nudRx.Value ), EAxisTransformDirection.XAxis );
			m_nudRx.Value = m_OldValueRx;
		}

		void m_btnResetRy_Click( object sender, EventArgs e )
		{
			ResetR?.Invoke( -( m_nudRy.Value ), EAxisTransformDirection.YAxis );
			m_nudRy.Value = m_OldValueRy;
		}

		void m_btnResetRz_Click( object sender, EventArgs e )
		{
			ResetR?.Invoke( -( m_nudRz.Value ), EAxisTransformDirection.ZAxis );
			m_nudRz.Value = m_OldValueRz;
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			Close();
		}

		void AxisTransformDlg_FormClosing( object sender, FormClosingEventArgs e )
		{
			FormClose?.Invoke();
		}

		void m_txbStepDistance_Leave( object sender, EventArgs e )
		{
			if( decimal.TryParse( m_txbStepDistance.Text, out decimal stepDistance ) && stepDistance < decimal.MaxValue && stepDistance > 0 ) {
				m_DisplacementIncrement = stepDistance;
				m_nudX.Increment = stepDistance;
				m_nudY.Increment = stepDistance;
				m_nudZ.Increment = stepDistance;
			}
			else {
				m_txbStepDistance.Text = m_DisplacementIncrement.ToString();
			}
		}

		void m_txbStepAngle_Leave( object sender, EventArgs e )
		{
			if( decimal.TryParse( m_txbStepAngle.Text, out decimal stepAngle ) && stepAngle <= ANGLE_RANGE_DEG && stepAngle > 0 ) {
				m_RotationIncrement = stepAngle;
				m_nudRx.Increment = stepAngle;
				m_nudRy.Increment = stepAngle;
				m_nudRz.Increment = stepAngle;
			}
			else {
				m_txbStepAngle.Text = m_RotationIncrement.ToString();
			}
		}

		void m_txbStepAngle_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			m_txbStepAngle_Leave( null, null );
		}

		void m_txbStepDistance_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			m_txbStepDistance_Leave( null, null );
		}
	}
}
