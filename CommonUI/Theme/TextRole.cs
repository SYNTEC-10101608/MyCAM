namespace CommonUI.Theme
{
	/// <summary>
	/// Text roles; each role fixes a font and a color.
	/// </summary>
	public enum TextRole
	{
		/// <summary>Body text (default), inherits the font and color of the parent.</summary>
		Body,

		/// <summary>Bold body text: axis names, fields that need emphasis.</summary>
		Emphasis,

		/// <summary>Larger text: mode tabs of the main window.</summary>
		Title,

		/// <summary>Monospaced text: machine values that change continuously.</summary>
		Value,

		/// <summary>Secondary text color: units and hints.</summary>
		Secondary,

		/// <summary>Error state text.</summary>
		Error,

		/// <summary>Warning state text.</summary>
		Warning,

		/// <summary>Success state text.</summary>
		Success
	}
}
