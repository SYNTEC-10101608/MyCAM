namespace CommonUI.Controls
{
	/// <summary>
	/// Look of a ThemedButton (ui-style.instructions.md, section 3.1).
	/// </summary>
	public enum ButtonKind
	{
		/// <summary>White: other actions such as cancel, reset, browse, CNC functions.</summary>
		Normal,

		/// <summary>Filled accent: the action that completes the dialog or the step (confirm, generate, next).</summary>
		Primary,

		/// <summary>Red border and text: actions that may lose data (delete, remove, clear data).</summary>
		Danger,

		/// <summary>CNC panel only: cycle start (light green).</summary>
		Start,

		/// <summary>CNC panel only: feed hold and resume (light amber).</summary>
		Hold,

		/// <summary>CNC panel only: stop (light red).</summary>
		Stop
	}
}
