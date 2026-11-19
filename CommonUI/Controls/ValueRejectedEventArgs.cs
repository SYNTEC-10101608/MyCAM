using System;

namespace CommonUI.Controls
{
	/// <summary>
	/// Arguments of <see cref="DebouncedNumericUpDown.ValueRejected"/>: an input that was out of range or not a number.
	/// </summary>
	public class ValueRejectedEventArgs : EventArgs
	{
		/// <summary>
		/// The rejected input; 0 when the input was empty or not a number.
		/// </summary>
		public decimal InputValue
		{
			get;
		}

		/// <summary>
		/// The range limit that the input exceeded (Minimum or Maximum).
		/// </summary>
		public decimal Limit
		{
			get;
		}

		/// <summary>
		/// True if the input was below the minimum (also for an empty or non-numeric input).
		/// </summary>
		public bool IsBelowMinimum
		{
			get;
		}

		/// <summary>
		/// Create the arguments of a rejected input.
		/// </summary>
		/// <param name="inputValue">The rejected input, 0 when empty or not a number.</param>
		/// <param name="limit">The range limit that the input exceeded.</param>
		/// <param name="isBelowMinimum">True if the input was below the minimum.</param>
		public ValueRejectedEventArgs( decimal inputValue, decimal limit, bool isBelowMinimum )
		{
			InputValue = inputValue;
			Limit = limit;
			IsBelowMinimum = isBelowMinimum;
		}
	}
}
