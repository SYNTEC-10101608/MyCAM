namespace CommonUI.Controls
{
	/// <summary>
	/// Look of a ThemedButton (ui-style.instructions.md, section 3).
	/// </summary>
	public enum ButtonKind
	{
		// secondary actions: cancel, reset, browse, CNC functions
		Normal,

		// the action that completes the dialog or the step: confirm, generate
		Primary,

		// actions that may lose data: delete, remove, clear data
		Danger,

		// CNC panel only: cycle start (filled green)
		Start,

		// CNC panel only: feed hold and resume (filled amber)
		Hold,

		// CNC panel only: stop (filled red)
		Stop
	}
}
