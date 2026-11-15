namespace CommonUI.Controls
{
	/// <summary>
	/// Visual kind of a <see cref="ThemedButton"/>.
	/// </summary>
	public enum ButtonKind
	{
		// secondary actions: cancel, reset, browse, CNC functions
		Normal,

		// the main action of a dialog: confirm, generate
		Primary,

		// destructive actions: delete, stop
		Danger
	}
}
