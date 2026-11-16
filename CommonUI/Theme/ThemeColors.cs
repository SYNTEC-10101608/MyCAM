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

		// filled CNC machining buttons (ButtonKind.Start / Hold / Stop), lighter than the status text colors
		public static readonly Color CNC_START = ColorTranslator.FromHtml( "#3A9E62" );
		public static readonly Color CNC_START_HOVER = ColorTranslator.FromHtml( "#35915A" );
		public static readonly Color CNC_START_PRESSED = ColorTranslator.FromHtml( "#318552" );
		public static readonly Color CNC_HOLD = ColorTranslator.FromHtml( "#E8901E" );
		public static readonly Color CNC_HOLD_HOVER = ColorTranslator.FromHtml( "#D5841C" );
		public static readonly Color CNC_HOLD_PRESSED = ColorTranslator.FromHtml( "#C37919" );
		public static readonly Color CNC_STOP = ColorTranslator.FromHtml( "#D6504A" );
		public static readonly Color CNC_STOP_HOVER = ColorTranslator.FromHtml( "#C54A44" );
		public static readonly Color CNC_STOP_PRESSED = ColorTranslator.FromHtml( "#B4433E" );
	}
}
