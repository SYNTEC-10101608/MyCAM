using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	/// <summary>
	/// NumericUpDown with configurable increment and debounced ValueChanged event.
	/// Supports manual input, scroll wheel, and up/down buttons.
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
				m_IsProgrammaticChange = true;
				Value = value;
				m_LastValue = value;
				m_IsProgrammaticChange = false;
			}
		}

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
		}

		protected override void OnValueChanged( EventArgs e )
		{
			base.OnValueChanged( e );

			// programmatic change, don't trigger debounce
			if( m_IsProgrammaticChange ) {
				return;
			}

			// if value actually changed beyond epsilon threshold
			if( Math.Abs( (double)Value - (double)m_LastValue ) <= EPSILON ) {
				return;
			}

			m_DebounceTimer.Stop();
			m_DebounceTimer.Start();
		}

		protected override void OnMouseWheel( MouseEventArgs e )
		{
			if( e is HandledMouseEventArgs hme ) {
				hme.Handled = true;
			}

			if( e.Delta > 0 ) {
				if( Value + Increment <= Maximum )
					Value += Increment;
				else
					Value = Maximum;
			}
			else if( e.Delta < 0 ) {
				if( Value - Increment >= Minimum )
					Value -= Increment;
				else
					Value = Minimum;
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

		const double EPSILON = 1e-3;
		Timer m_DebounceTimer;
		int m_DebounceInterval = 100;
		bool m_IsProgrammaticChange = false;
		decimal m_LastValue = 0;
	}
}
