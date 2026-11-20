namespace CommonUI.Controls
{
	/// <summary>
	/// Indicator lamp on a CNC button.
	/// </summary>
	public enum ButtonIndicator
	{
		/// <summary>No lamp.</summary>
		None,

		/// <summary>The state is not known yet (e.g. not connected).</summary>
		Unknown,

		/// <summary>The function is off.</summary>
		Off,

		/// <summary>The function is on.</summary>
		On
	}
}
