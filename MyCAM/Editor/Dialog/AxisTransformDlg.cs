using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
{
	public partial class AxisTransformDlg : Form
	{
		internal Action<decimal, EAxisTransformDirection> Displace;
		internal Action<decimal, EAxisTransformDirection> Rotate;
		internal Action FormConfirm;
		internal Action FormCancel;
		internal Action FormReset;
		decimal m_RotationIncrement = 1;
		decimal m_DisplacementIncrement = 1;
		const decimal ANGLE_RANGE_DEG = 360;
		const int DISPLACEMENT_DECIMAL_PLACES = 3;

		// Previous values for delta calculation
		readonly Dictionary<NumericUpDown, decimal> m_PrevValues = new Dictionary<NumericUpDown, decimal>();

		// If value is change by action (not user input from dlg), ignore the ValueChanged event to prevent feedback loop
		bool m_isIgnoreValueChange = false;

		public AxisTransformDlg()
		{
			InitializeComponent();
			InitialControlBoundarySetting();
			InitialControlValue();
			RegisterValueChangedEvents();
		}

		internal void UpdateDisplacementFromManipulator( decimal dx, decimal dy, decimal dz )
		{
			m_isIgnoreValueChange = true;
			UpdateNumericValue( m_nudX, dx );
			UpdateNumericValue( m_nudY, dy );
			UpdateNumericValue( m_nudZ, dz );
			m_isIgnoreValueChange = false;
		}

		internal void UpdateRotationFromManipulator( decimal drx, decimal dry, decimal drz )
		{
			m_isIgnoreValueChange = true;
			UpdateNumericValue( m_nudRx, drx, clampAngle: true );
			UpdateNumericValue( m_nudRy, dry, clampAngle: true );
			UpdateNumericValue( m_nudRz, drz, clampAngle: true );
			m_isIgnoreValueChange = false;
		}

		internal void ResetAllValues()
		{
			m_isIgnoreValueChange = true;
			foreach( var nud in GetAllNumericControls() ) {
				nud.Value = 0;
				m_PrevValues[ nud ] = 0;
			}
			m_isIgnoreValueChange = false;
		}

		protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
		{
			// Get esc to close dlg
			if( keyData == Keys.Escape ) {
				m_btnCancel_Click( null, null );
				return true;
			}
			return base.ProcessCmdKey( ref msg, keyData );
		}

		void InitialControlBoundarySetting()
		{
			// Set rotation controls
			foreach( var nud in GetRotationControls() ) {
				nud.Maximum = ANGLE_RANGE_DEG;
				nud.Minimum = -ANGLE_RANGE_DEG;
			}

			// Set displacement controls
			foreach( var nud in GetDisplacementControls() ) {
				nud.Maximum = decimal.MaxValue;
				nud.Minimum = decimal.MinValue;
				nud.DecimalPlaces = DISPLACEMENT_DECIMAL_PLACES;
			}
		}

		void InitialControlValue()
		{
			m_txbStepDistance.Text = m_DisplacementIncrement.ToString();
			m_txbStepAngle.Text = m_RotationIncrement.ToString();

			foreach( var nud in GetDisplacementControls() ) {
				nud.Increment = m_DisplacementIncrement;
				m_PrevValues[ nud ] = 0;
			}

			foreach( var nud in GetRotationControls() ) {
				nud.Increment = m_RotationIncrement;
				m_PrevValues[ nud ] = 0;
			}
		}

		void RegisterValueChangedEvents()
		{
			// ValueChanged events (manually registered, not in Designer)
			m_nudX.ValueChanged += m_nudX_ValueChanged;
			m_nudY.ValueChanged += m_nudY_ValueChanged;
			m_nudZ.ValueChanged += m_nudZ_ValueChanged;
			m_nudRx.ValueChanged += m_nudRx_ValueChanged;
			m_nudRy.ValueChanged += m_nudRy_ValueChanged;
			m_nudRz.ValueChanged += m_nudRz_ValueChanged;
		}

		// ValueChanged event handlers (unified implementation)
		void m_nudX_ValueChanged( object sender, EventArgs e )
		{
			HandleValueChanged( m_nudX, EAxisTransformDirection.XAxis, Displace );
		}

		void m_nudY_ValueChanged( object sender, EventArgs e )
		{
			HandleValueChanged( m_nudY, EAxisTransformDirection.YAxis, Displace );
		}

		void m_nudZ_ValueChanged( object sender, EventArgs e )
		{
			HandleValueChanged( m_nudZ, EAxisTransformDirection.ZAxis, Displace );
		}

		void m_nudRx_ValueChanged( object sender, EventArgs e )
		{
			HandleValueChanged( m_nudRx, EAxisTransformDirection.XAxis, Rotate );
		}

		void m_nudRy_ValueChanged( object sender, EventArgs e )
		{
			HandleValueChanged( m_nudRy, EAxisTransformDirection.YAxis, Rotate );
		}

		void m_nudRz_ValueChanged( object sender, EventArgs e )
		{
			HandleValueChanged( m_nudRz, EAxisTransformDirection.ZAxis, Rotate );
		}

		// MouseWheel event handlers (called by Designer)
		void m_nudX_MouseWheel( object sender, MouseEventArgs e )
		{
			HandleMouseWheel( m_nudX, e, m_DisplacementIncrement );
		}

		void m_nudY_MouseWheel( object sender, MouseEventArgs e )
		{
			HandleMouseWheel( m_nudY, e, m_DisplacementIncrement );
		}

		void m_nudZ_MouseWheel( object sender, MouseEventArgs e )
		{
			HandleMouseWheel( m_nudZ, e, m_DisplacementIncrement );
		}

		void m_nudRx_MouseWheel( object sender, MouseEventArgs e )
		{
			HandleMouseWheel( m_nudRx, e, m_RotationIncrement );
		}

		void m_nudRy_MouseWheel( object sender, MouseEventArgs e )
		{
			HandleMouseWheel( m_nudRy, e, m_RotationIncrement );
		}

		void m_nudRz_MouseWheel( object sender, MouseEventArgs e )
		{
			HandleMouseWheel( m_nudRz, e, m_RotationIncrement );
		}

		// Unified value change handler
		void HandleValueChanged( NumericUpDown nud, EAxisTransformDirection axis, Action<decimal, EAxisTransformDirection> callback )
		{
			if( m_isIgnoreValueChange ) {
				return;
			}

			decimal delta = nud.Value - m_PrevValues[ nud ];
			m_PrevValues[ nud ] = nud.Value;

			if( delta != 0 ) {
				callback?.Invoke( delta, axis );
			}
		}

		// Unified mouse wheel handler
		void HandleMouseWheel( NumericUpDown nud, MouseEventArgs e, decimal increment )
		{
			if( e is HandledMouseEventArgs handledE ) {
				handledE.Handled = true;
			}

			decimal step = e.Delta > 0 ? increment : -increment;
			decimal newValue = nud.Value + step;

			if( newValue >= nud.Minimum && newValue <= nud.Maximum ) {
				nud.Value = newValue;
			}
		}

		void UpdateNumericValue( NumericUpDown nud, decimal delta, bool clampAngle = false )
		{
			decimal newValue = nud.Value + delta;
			nud.Value = clampAngle ? ClampAngle( newValue ) : newValue;
			m_PrevValues[ nud ] = nud.Value;
		}

		decimal ClampAngle( decimal value )
		{
			return Math.Max( -ANGLE_RANGE_DEG, Math.Min( ANGLE_RANGE_DEG, value ) );
		}

		void m_btnOK_Click( object sender, EventArgs e )
		{
			FormConfirm?.Invoke();
			Close();
		}

		void m_btnCancel_Click( object sender, EventArgs e )
		{
			FormCancel?.Invoke();
			FormCancel = null;
			Close();
		}

		void m_btnReset_Click( object sender, EventArgs e )
		{
			FormReset?.Invoke();
		}

		void AxisTransformDlg_FormClosing( object sender, FormClosingEventArgs e )
		{
			FormCancel?.Invoke();
		}

		void m_txbStepDistance_Leave( object sender, EventArgs e )
		{
			if( decimal.TryParse( m_txbStepDistance.Text, out decimal stepDistance ) && stepDistance > 0 && stepDistance < decimal.MaxValue ) {
				m_DisplacementIncrement = stepDistance;
				foreach( var nud in GetDisplacementControls() ) {
					nud.Increment = stepDistance;
				}
			}
			else {
				m_txbStepDistance.Text = m_DisplacementIncrement.ToString();
			}
		}

		void m_txbStepAngle_Leave( object sender, EventArgs e )
		{
			if( decimal.TryParse( m_txbStepAngle.Text, out decimal stepAngle ) && stepAngle > 0 && stepAngle <= ANGLE_RANGE_DEG ) {
				m_RotationIncrement = stepAngle;
				foreach( var nud in GetRotationControls() ) {
					nud.Increment = stepAngle;
				}
			}
			else {
				m_txbStepAngle.Text = m_RotationIncrement.ToString();
			}
		}

		void m_txbStepAngle_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				m_txbStepAngle_Leave( null, null );
			}
		}

		void m_txbStepDistance_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				m_txbStepDistance_Leave( null, null );
			}
		}

		// Helper methods to group controls
		IEnumerable<NumericUpDown> GetDisplacementControls()
		{
			yield return m_nudX;
			yield return m_nudY;
			yield return m_nudZ;
		}

		IEnumerable<NumericUpDown> GetRotationControls()
		{
			yield return m_nudRx;
			yield return m_nudRy;
			yield return m_nudRz;
		}

		IEnumerable<NumericUpDown> GetAllNumericControls()
		{
			foreach( var nud in GetDisplacementControls() )
				yield return nud;
			foreach( var nud in GetRotationControls() )
				yield return nud;
		}
	}
}
