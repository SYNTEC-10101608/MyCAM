namespace CommonUI.Theme
{
	/// <summary>
	/// Layout sizes in pixels at 96 DPI (see ui-style.instructions.md, section 2.3).
	/// </summary>
	public static class ThemeMetrics
	{
		/// <summary>Margin around the content of a dialog.</summary>
		public const int DIALOG_MARGIN = 12;

		/// <summary>From the bottom of the content to the top of the button bar.</summary>
		public const int BUTTON_BAR_GAP = 16;

		/// <summary>Between two cards.</summary>
		public const int SECTION_GAP = 8;

		/// <summary>Left, right and bottom padding of a card; top padding of a card without title.</summary>
		public const int SECTION_PADDING = 10;

		/// <summary>Height of the title bar of a card.</summary>
		public const int SECTION_TITLE_HEIGHT = 32;

		/// <summary>Top of the first row in a card with title.</summary>
		public const int SECTION_CONTENT_TOP = 36;

		/// <summary>Height of input fields and drop-down lists.</summary>
		public const int INPUT_HEIGHT = 23;

		/// <summary>From the top of one input row to the top of the next.</summary>
		public const int ROW_PITCH = 30;

		/// <summary>Between controls on the same row (label, input, unit).</summary>
		public const int CONTROL_GAP = 6;

		/// <summary>Downward offset of a label from the top of its row.</summary>
		public const int LABEL_OFFSET = 4;

		/// <summary>Downward offset of a CheckBox / RadioButton from the top of its row.</summary>
		public const int CHECK_OFFSET = 2;

		/// <summary>Margin around the content of an embedded panel.</summary>
		public const int PANEL_MARGIN = 8;

		/// <summary>Height of a read-only value row in an embedded panel.</summary>
		public const int INFO_ROW_HEIGHT = 22;

		/// <summary>From the top of one read-only value row to the top of the next.</summary>
		public const int INFO_ROW_PITCH = 26;

		/// <summary>Height of a button in a button bar.</summary>
		public const int BUTTON_HEIGHT = 28;

		/// <summary>Minimum width of a button in a button bar.</summary>
		public const int BUTTON_MIN_WIDTH = 80;

		/// <summary>Between two buttons of a button bar.</summary>
		public const int BUTTON_GAP = 8;

		/// <summary>Corner radius of buttons, drawn by the controls themselves.</summary>
		public const int BUTTON_CORNER_RADIUS = 3;
	}
}
