using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace CommonUI.Controls
{
	/// <summary>
	/// The numeric input of every screen: the field's limits are Minimum / Maximum / DecimalPlaces / Increment,
	/// and DebouncedValueChanged notifies a value once the user stops changing it.
	/// <list type="number">
	/// <item>Input that cannot be parsed or is out of range is rejected: the field returns to the latest valid
	/// value (not clamped to a limit; a value more precise than the shown decimals is kept as it is) without raising
	/// ValueChanged, and ValueRejected is raised. The limits themselves can be entered.</item>
	/// <item>Every change, also one step of the last decimal place or a Value set by code, raises
	/// DebouncedValueChanged once after the changes stop; changes within DebounceInterval are notified once with the
	/// last value, never earlier than the interval after the last change. SilentSetValue raises nothing and cancels a
	/// pending notification.</item>
	/// <item>The mouse wheel and the up / down keys change the value by one Increment and stop at the limits;
	/// ReadOnly blocks them. Typed text that is not validated yet is accepted or rejected first, then stepped.</item>
	/// <item>Rejects invalid edits before Enter or Leave reaches consumers; Enter on a rejected edit is consumed.
	/// Leaving the field sends a pending notification at once.</item>
	/// </list>
	/// </summary>
	public class DebouncedNumericUpDown : NumericUpDown
	{
		/// <summary>
		/// Fires after the user stops changing the value for <see cref="DebounceInterval"/> milliseconds.
		/// </summary>
		[Category( "Behavior" )]
		[Description( "Occurs after value changes have stopped for the debounce interval duration." )]
		public event EventHandler DebouncedValueChanged;

		/// <summary>
		/// Debounce interval in milliseconds.
		/// </summary>
		[Category( "Behavior" )]
		[DefaultValue( 100 )]
		[Description( "The delay in milliseconds before DebouncedValueChanged fires after the last value change." )]
		public int DebounceInterval
		{
			get => m_DebounceInterval;
			set
			{
				m_DebounceInterval = value;
				if( m_DebounceTimer != null ) {
					m_DebounceTimer.Interval = value;
				}
			}
		}

		/// <summary>
		/// Set value programmatically without triggering DebouncedValueChanged event.
		/// </summary>
		public decimal SilentSetValue
		{
			set
			{
				// the UI value is set by user, stop timer do not need to wait
				m_DebounceTimer.Stop();
				m_IsProgrammaticChange = true;
				Value = value;
				m_LastValue = value;
				m_IsProgrammaticChange = false;
			}
		}

		/// <summary>
		/// Create a numeric box with 3 decimals, range -99999 to 99999 and increment 1.
		/// </summary>
		public DebouncedNumericUpDown()
		{
			m_DebounceTimer = new Timer();
			m_DebounceTimer.Interval = m_DebounceInterval;
			m_DebounceTimer.Tick += OnDebounceTimerTick;

			this.DecimalPlaces = 3;
			this.Minimum = -99999;
			this.Maximum = 99999;
			this.Increment = 1;

			m_LastValue = this.Value;
			RevertOnOutOfRange = true;
		}

		// font and colors come from the parent (theme), never from the designer file
		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public override System.Drawing.Font Font
		{
			get => base.Font;
			set => base.Font = value;
		}

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public override System.Drawing.Color ForeColor
		{
			get => base.ForeColor;
			set => base.ForeColor = value;
		}

		public override void UpButton()
		{
			// disable up button when ReadOnly
			if( ReadOnly ) {
				return;
			}

			// the native step parses and clamps typed text by itself; validate it first (rejected text is restored),
			// so the step starts from a valid value as with the mouse wheel
			ValidateEditText();
			base.UpButton();
		}

		public override void DownButton()
		{
			// disable down button when ReadOnly
			if( ReadOnly ) {
				return;
			}

			// the native step parses and clamps typed text by itself; validate it first (rejected text is restored),
			// so the step starts from a valid value as with the mouse wheel
			ValidateEditText();
			base.DownButton();
		}

		/// <summary>
		/// When true, out-of-range or unparseable (including empty) input reverts to the last valid value
		/// </summary>
		[Category( "Behavior" )]
		[DefaultValue( true )]
		public bool RevertOnOutOfRange
		{
			get; set;
		}

		/// <summary>
		/// Fires when an input value is rejected (out of range)
		/// </summary>
		[Category( "Behavior" )]
		public event EventHandler<ValueRejectedEventArgs> ValueRejected;

		/// <summary>
		/// Reads the current text without committing it, using the same format and limits as validation.
		/// </summary>
		public bool TryGetInputValue( out decimal value )
		{
			return TryParseInput( out value ) && value >= Minimum && value <= Maximum;
		}

		protected override void OnLeave( EventArgs e )
		{
			RejectInvalidEdit();
			base.OnLeave( e );
		}

		protected override void OnLostFocus( EventArgs e )
		{
			if( RevertOnOutOfRange && !m_IsProgrammaticChange && string.IsNullOrWhiteSpace( Text ) ) {
				RevertToLastValue( 0m, Minimum, isBelowMin: true );
			}

			// this will trigger onValueChange -> timer start
			base.OnLostFocus( e );
			RaiseValueChangeImmediately();
		}

		protected override void OnKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter && RejectInvalidEdit() ) {
				e.Handled = true;
				e.SuppressKeyPress = true;
				return;
			}
			base.OnKeyDown( e );
		}

		protected override void ValidateEditText()
		{
			// nothing typed since the value was shown; parsing the shown (rounded) text would change the value
			if( !UserEdit ) {
				return;
			}
			if( !RejectInvalidEdit() ) {
				base.ValidateEditText();
			}
		}

		bool RejectInvalidEdit()
		{
			if( !UserEdit || !RevertOnOutOfRange || m_IsProgrammaticChange ) {
				return false;
			}
			if( !TryParseInput( out decimal parsed ) ) {
				RevertToLastValue( 0m, Minimum, isBelowMin: true );
				return true;
			}
			if( parsed >= Minimum && parsed <= Maximum ) {
				return false;
			}
			bool isBelowMin = parsed < Minimum;
			RevertToLastValue( parsed, isBelowMin ? Minimum : Maximum, isBelowMin );
			return true;
		}

		bool TryParseInput( out decimal value )
		{
			return decimal.TryParse( Text, NumberStyles.Number, CultureInfo.CurrentCulture, out value );
		}

		void RaiseValueChangeImmediately()
		{
			if( !m_DebounceTimer.Enabled ) {
				return;
			}
			OnDebounceTimerTick( this, EventArgs.Empty );
		}

		void RevertToLastValue( decimal inputValue, decimal limit, bool isBelowMin )
		{
			ValueRejected?.Invoke( this, new ValueRejectedEventArgs( inputValue, limit, isBelowMin ) );

			// the rejected text never reached the value, so the current value is the latest valid one;
			// drop the user edit before reading and showing it, or the shown (rounded) text is parsed as a new value
			UserEdit = false;
			string formatted = Value.ToString( "F" + DecimalPlaces, System.Globalization.CultureInfo.CurrentCulture );
			m_IsProgrammaticChange = true;
			ChangingText = true;
			Text = formatted;
			m_IsProgrammaticChange = false;
		}

		protected override void OnValueChanged( EventArgs e )
		{
			base.OnValueChanged( e );

			// programmatic change, don't trigger debounce
			if( m_IsProgrammaticChange ) {
				return;
			}

			// decimal is exact, so any change the user can make (even one step of the last decimal place) is notified
			if( Value == m_LastValue ) {
				return;
			}

			m_DebounceTimer.Stop();
			m_DebounceTimer.Start();
		}

		/// <summary>
		/// When true, only digits, the decimal separator and '-' (if the minimum is negative) can be typed.
		/// </summary>
		public bool RestrictInvalidCharacters
		{
			get; set;
		}

		protected override void OnKeyPress( KeyPressEventArgs e )
		{
			if( !RestrictInvalidCharacters ) {
				base.OnKeyPress( e );
				return;
			}

			char ch = e.KeyChar;

			// always allow control keys (Backspace, Ctrl+C/V, etc.)
			if( char.IsControl( ch ) ) {
				base.OnKeyPress( e );
				return;
			}

			// reject letters and other symbols (only digits, decimal separator, and '-' are candidates)
			string decimalSeparator = System.Globalization.CultureInfo.CurrentCulture
				.NumberFormat.NumberDecimalSeparator;
			bool isDigit = char.IsDigit( ch );
			bool isDecimalSeparator = decimalSeparator.Length == 1 && ch == decimalSeparator[ 0 ];
			bool isNegativeSign = ch == '-';
			if( !isDigit && !isDecimalSeparator && !isNegativeSign ) {
				e.Handled = true;
				return;
			}

			// reject '-' when the range does not allow negative values
			if( isNegativeSign && Minimum >= 0 ) {
				e.Handled = true;
				return;
			}

			base.OnKeyPress( e );
		}

		protected override void OnMouseWheel( MouseEventArgs e )
		{
			if( e is HandledMouseEventArgs hme ) {
				hme.Handled = true;
			}

			// disable mouse wheel when ReadOnly
			if( ReadOnly ) {
				return;
			}

			if( e.Delta > 0 ) {
				if( Value + Increment <= Maximum ) {
					Value += Increment;
				}
				else {
					Value = Maximum;
				}
			}
			else if( e.Delta < 0 ) {
				if( Value - Increment >= Minimum ) {
					Value -= Increment;
				}
				else {
					Value = Minimum;
				}
			}
		}

		void OnDebounceTimerTick( object sender, EventArgs e )
		{
			m_DebounceTimer.Stop();
			m_LastValue = Value;
			DebouncedValueChanged?.Invoke( this, EventArgs.Empty );
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing ) {
				m_DebounceTimer?.Stop();
				m_DebounceTimer?.Dispose();
			}
			base.Dispose( disposing );
		}

		Timer m_DebounceTimer;
		int m_DebounceInterval = 100;
		bool m_IsProgrammaticChange = false;
		decimal m_LastValue = 0;
	}
}
