using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
{
	public partial class AxisTransformDlg : FormBase
	{
		internal Action<decimal, EAxisTransformDirection> Displace;
		internal Action<decimal, EAxisTransformDirection> Rotate;
		internal Action FormConfirm;
		internal Action FormCancel;
		internal Action FormReset;

		public AxisTransformDlg()
		{
			InitializeComponent();
			ApplyLanguage();
			InitialControlBoundarySetting();
			InitialControlValue();
			RegisterValueChangedEvents();
		}

		internal void UpdateDisplacementFromManipulator( double dx, double dy, double dz )
		{
			m_isIgnoreValueChange = true;
			UpdateNumericValue( m_numX, dx );
			UpdateNumericValue( m_numY, dy );
			UpdateNumericValue( m_numZ, dz );
			m_isIgnoreValueChange = false;
		}

		internal void UpdateRotationFromManipulator( double drx, double dry, double drz )
		{
			m_isIgnoreValueChange = true;
			UpdateRotationValueFromManipulator( m_numRx, drx );
			UpdateRotationValueFromManipulator( m_numRy, dry );
			UpdateRotationValueFromManipulator( m_numRz, drz );
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
			// Set rotation controls - allow unlimited rotation angle
			foreach( var nud in GetRotationControls() ) {
				nud.Maximum = decimal.MaxValue;
				nud.Minimum = decimal.MinValue;
				nud.DecimalPlaces = ROTATION_DECIMAL_PLACES;
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
			m_numStepDistance.Text = m_DisplacementIncrement.ToString();
			m_numStepAngle.Text = m_RotationIncrement.ToString();

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
			m_numX.ValueChanged += m_numX_ValueChanged;
			m_numY.ValueChanged += m_numY_ValueChanged;
			m_numZ.ValueChanged += m_numZ_ValueChanged;
			m_numRx.ValueChanged += m_numRx_ValueChanged;
			m_numRy.ValueChanged += m_numRy_ValueChanged;
			m_numRz.ValueChanged += m_numRz_ValueChanged;
		}

		// ValueChanged event handlers (unified implementation)
		void m_numX_ValueChanged( object sender, EventArgs e )
		{
			HandleValueChanged( m_numX, EAxisTransformDirection.XAxis, Displace );
		}

		void m_numY_ValueChanged( object sender, EventArgs e )
		{
			HandleValueChanged( m_numY, EAxisTransformDirection.YAxis, Displace );
		}

		void m_numZ_ValueChanged( object sender, EventArgs e )
		{
			HandleValueChanged( m_numZ, EAxisTransformDirection.ZAxis, Displace );
		}

		void m_numRx_ValueChanged( object sender, EventArgs e )
		{
			HandleValueChanged( m_numRx, EAxisTransformDirection.XAxis, Rotate );
		}

		void m_numRy_ValueChanged( object sender, EventArgs e )
		{
			HandleValueChanged( m_numRy, EAxisTransformDirection.YAxis, Rotate );
		}

		void m_numRz_ValueChanged( object sender, EventArgs e )
		{
			HandleValueChanged( m_numRz, EAxisTransformDirection.ZAxis, Rotate );
		}

		// MouseWheel event handlers (called by Designer)
		void m_numX_MouseWheel( object sender, MouseEventArgs e )
		{
			HandleMouseWheel( m_numX, e, m_DisplacementIncrement );
		}

		void m_numY_MouseWheel( object sender, MouseEventArgs e )
		{
			HandleMouseWheel( m_numY, e, m_DisplacementIncrement );
		}

		void m_numZ_MouseWheel( object sender, MouseEventArgs e )
		{
			HandleMouseWheel( m_numZ, e, m_DisplacementIncrement );
		}

		void m_numRx_MouseWheel( object sender, MouseEventArgs e )
		{
			HandleMouseWheel( m_numRx, e, m_RotationIncrement );
		}

		void m_numRy_MouseWheel( object sender, MouseEventArgs e )
		{
			HandleMouseWheel( m_numRy, e, m_RotationIncrement );
		}

		void m_numRz_MouseWheel( object sender, MouseEventArgs e )
		{
			HandleMouseWheel( m_numRz, e, m_RotationIncrement );
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

		// Update NumericUpDown by adding delta (used for displacement from manipulator)
		void UpdateNumericValue( NumericUpDown nud, double delta )
		{
			if( delta == 0 ) {
				return;
			}

			decimal newValue = nud.Value + (decimal)delta;
			nud.Value = newValue;
			m_PrevValues[ nud ] = nud.Value;
		}

		// Update rotation value from manipulator with integer-degree snapping
		void UpdateRotationValueFromManipulator( NumericUpDown nud, double delta )
		{
			if( delta == 0 ) {
				return;
			}

			// Manipulator's underlying SetRotationSteps(1 deg) guarantees delta only changes
			double intDegreeChange = Math.Round( delta, ROTATION_FROM_ACTION_PLACES );
			if( intDegreeChange == 0 ) {
				return;
			}

			// Apply integer change while preserving user-entered fractional part
			decimal newValue = nud.Value + (decimal)intDegreeChange;
			nud.Value = newValue;
			m_PrevValues[ nud ] = nud.Value;
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

		void m_numStepDistance_Leave( object sender, EventArgs e )
		{
			if( decimal.TryParse( m_numStepDistance.Text, out decimal stepDistance ) && stepDistance > 0 && stepDistance < decimal.MaxValue ) {
				m_DisplacementIncrement = stepDistance;
				foreach( var nud in GetDisplacementControls() ) {
					nud.Increment = stepDistance;
				}
			}
			else {
				m_numStepDistance.Text = m_DisplacementIncrement.ToString();
			}
		}

		void m_numStepAngle_Leave( object sender, EventArgs e )
		{
			if( decimal.TryParse( m_numStepAngle.Text, out decimal stepAngle ) && stepAngle > 0 ) {
				// Limit to specified decimal places
				stepAngle = Math.Round( stepAngle, ROTATION_DECIMAL_PLACES );
				m_RotationIncrement = stepAngle;
				foreach( var nud in GetRotationControls() ) {
					nud.Increment = stepAngle;
				}
				// Display with up to ROTATION_DECIMAL_PLACES digits (trailing zeros omitted)
				m_numStepAngle.Text = stepAngle.ToString( $"F{ROTATION_DECIMAL_PLACES}" );
			}
			else {
				m_numStepAngle.Text = m_RotationIncrement.ToString( $"F{ROTATION_DECIMAL_PLACES}" );
			}
		}

		void m_numStepAngle_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				m_numStepAngle_Leave( null, null );
			}
		}

		void m_numStepDistance_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				m_numStepDistance_Leave( null, null );
			}
		}

		// Helper methods to group controls
		IEnumerable<NumericUpDown> GetDisplacementControls()
		{
			yield return m_numX;
			yield return m_numY;
			yield return m_numZ;
		}

		IEnumerable<NumericUpDown> GetRotationControls()
		{
			yield return m_numRx;
			yield return m_numRy;
			yield return m_numRz;
		}

		IEnumerable<NumericUpDown> GetAllNumericControls()
		{
			foreach( var nud in GetDisplacementControls() ) {
				yield return nud;
			}

			foreach( var nud in GetRotationControls() ) {
				yield return nud;
			}
		}


		decimal m_RotationIncrement = 1;
		decimal m_DisplacementIncrement = 1;
		const int DISPLACEMENT_DECIMAL_PLACES = 3;
		const int ROTATION_DECIMAL_PLACES = 3;
		const int ROTATION_FROM_ACTION_PLACES = 0;

		// Previous values for delta calculation
		readonly Dictionary<NumericUpDown, decimal> m_PrevValues = new Dictionary<NumericUpDown, decimal>();

		// If value is change by action (not user input from dlg), ignore the ValueChanged event to prevent feedback loop
		bool m_isIgnoreValueChange = false;
	}
}
