using System.Drawing;

namespace CommonUI.Theme
{
	/// <summary>
	/// Color tokens of the light theme. Screens use these tokens only; data colors are defined elsewhere.
	/// </summary>
	public static class ThemeColors
	{
		// surfaces
		public static readonly Color WINDOW = ColorTranslator.FromHtml( "#F1F3F6" );
		public static readonly Color SURFACE = ColorTranslator.FromHtml( "#FFFFFF" );
		public static readonly Color SURFACE_ALT = ColorTranslator.FromHtml( "#F8F9FB" );

		// lines
		public static readonly Color BORDER = ColorTranslator.FromHtml( "#D6DCE4" );
		public static readonly Color BORDER_STRONG = ColorTranslator.FromHtml( "#BDC6D1" );
		public static readonly Color DIVIDER = ColorTranslator.FromHtml( "#E4E8EE" );

		// text
		public static readonly Color TEXT = ColorTranslator.FromHtml( "#1E2936" );
		public static readonly Color TEXT_SECONDARY = ColorTranslator.FromHtml( "#5E6A79" );
		public static readonly Color TEXT_DISABLED = ColorTranslator.FromHtml( "#A0A9B5" );

		// accent
		public static readonly Color ACCENT = ColorTranslator.FromHtml( "#0067AD" );
		public static readonly Color ACCENT_HOVER = ColorTranslator.FromHtml( "#005A99" );
		public static readonly Color ACCENT_PRESSED = ColorTranslator.FromHtml( "#004C82" );
		public static readonly Color ACCENT_TINT = ColorTranslator.FromHtml( "#E8F1F9" );
		public static readonly Color ACCENT_TINT_STRONG = ColorTranslator.FromHtml( "#CFE2F3" );
		public static readonly Color TEXT_ON_ACCENT = ColorTranslator.FromHtml( "#FFFFFF" );

		// interaction on white controls
		public static readonly Color HOVER = ColorTranslator.FromHtml( "#ECF0F5" );
		public static readonly Color PRESSED = ColorTranslator.FromHtml( "#DFE5EC" );

		// status
		public static readonly Color ERROR = ColorTranslator.FromHtml( "#C42B1C" );
		public static readonly Color WARNING = ColorTranslator.FromHtml( "#A86200" );
		public static readonly Color SUCCESS = ColorTranslator.FromHtml( "#107C41" );

		// CNC machining buttons (ButtonKind.Start / Hold / Stop): light tint fill, border and text in the color
		public static readonly Color CNC_START = ColorTranslator.FromHtml( "#2E8452" );
		public static readonly Color CNC_START_TINT = ColorTranslator.FromHtml( "#E5F3EA" );
		public static readonly Color CNC_START_TINT_HOVER = ColorTranslator.FromHtml( "#D5ECDE" );
		public static readonly Color CNC_START_TINT_PRESSED = ColorTranslator.FromHtml( "#C3E3CF" );
		public static readonly Color CNC_HOLD = ColorTranslator.FromHtml( "#C26E0A" );
		public static readonly Color CNC_HOLD_TINT = ColorTranslator.FromHtml( "#FDF0DE" );
		public static readonly Color CNC_HOLD_TINT_HOVER = ColorTranslator.FromHtml( "#FAE3C4" );
		public static readonly Color CNC_HOLD_TINT_PRESSED = ColorTranslator.FromHtml( "#F6D4A8" );
		public static readonly Color CNC_STOP = ColorTranslator.FromHtml( "#C2413B" );
		public static readonly Color CNC_STOP_TINT = ColorTranslator.FromHtml( "#FBE6E4" );
		public static readonly Color CNC_STOP_TINT_HOVER = ColorTranslator.FromHtml( "#F7D5D2" );
		public static readonly Color CNC_STOP_TINT_PRESSED = ColorTranslator.FromHtml( "#F1C1BD" );
	}
}
