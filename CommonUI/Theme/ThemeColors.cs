using System.Drawing;

namespace CommonUI.Theme
{
	/// <summary>
	/// Color tokens of the light theme. Screens use these tokens only; data colors are defined elsewhere.
	/// </summary>
	public static class ThemeColors
	{
		/// <summary>Background of forms and panels.</summary>
		public static readonly Color WINDOW = ColorTranslator.FromHtml( "#F1F3F6" );

		/// <summary>Surface of cards, input fields and buttons.</summary>
		public static readonly Color SURFACE = ColorTranslator.FromHtml( "#FFFFFF" );

		/// <summary>Surface of table headers, read-only fields and disabled buttons.</summary>
		public static readonly Color SURFACE_ALT = ColorTranslator.FromHtml( "#F8F9FB" );

		/// <summary>Border of cards.</summary>
		public static readonly Color BORDER = ColorTranslator.FromHtml( "#D6DCE4" );

		/// <summary>Border of buttons, input fields and swatches.</summary>
		public static readonly Color BORDER_STRONG = ColorTranslator.FromHtml( "#BDC6D1" );

		/// <summary>Grid lines, separators and progress bar tracks.</summary>
		public static readonly Color DIVIDER = ColorTranslator.FromHtml( "#E4E8EE" );

		/// <summary>Body text.</summary>
		public static readonly Color TEXT = ColorTranslator.FromHtml( "#1E2936" );

		/// <summary>Units and hints.</summary>
		public static readonly Color TEXT_SECONDARY = ColorTranslator.FromHtml( "#5E6A79" );

		/// <summary>Disabled text.</summary>
		public static readonly Color TEXT_DISABLED = ColorTranslator.FromHtml( "#A0A9B5" );

		/// <summary>Primary buttons, selection and the accent line of card titles.</summary>
		public static readonly Color ACCENT = ColorTranslator.FromHtml( "#0067AD" );

		/// <summary>Accent under the mouse.</summary>
		public static readonly Color ACCENT_HOVER = ColorTranslator.FromHtml( "#005A99" );

		/// <summary>Accent while pressed.</summary>
		public static readonly Color ACCENT_PRESSED = ColorTranslator.FromHtml( "#004C82" );

		/// <summary>Selected rows and highlighted card titles.</summary>
		public static readonly Color ACCENT_TINT = ColorTranslator.FromHtml( "#E8F1F9" );

		/// <summary>Tool bar item of the running tool.</summary>
		public static readonly Color ACCENT_TINT_STRONG = ColorTranslator.FromHtml( "#CFE2F3" );

		/// <summary>Text on an accent background.</summary>
		public static readonly Color TEXT_ON_ACCENT = ColorTranslator.FromHtml( "#FFFFFF" );

		/// <summary>White controls under the mouse.</summary>
		public static readonly Color HOVER = ColorTranslator.FromHtml( "#ECF0F5" );

		/// <summary>White controls while pressed.</summary>
		public static readonly Color PRESSED = ColorTranslator.FromHtml( "#DFE5EC" );

		/// <summary>Error state.</summary>
		public static readonly Color ERROR = ColorTranslator.FromHtml( "#C42B1C" );

		/// <summary>Warning state.</summary>
		public static readonly Color WARNING = ColorTranslator.FromHtml( "#A86200" );

		/// <summary>Success state.</summary>
		public static readonly Color SUCCESS = ColorTranslator.FromHtml( "#107C41" );

		// CNC machining buttons (ButtonKind.Start / Hold / Stop): light tint fill, border and text in the color

		/// <summary>Border and text of the CNC start button.</summary>
		public static readonly Color CNC_START = ColorTranslator.FromHtml( "#2E8452" );

		/// <summary>Fill of the CNC start button.</summary>
		public static readonly Color CNC_START_TINT = ColorTranslator.FromHtml( "#E5F3EA" );

		/// <summary>Fill of the CNC start button under the mouse.</summary>
		public static readonly Color CNC_START_TINT_HOVER = ColorTranslator.FromHtml( "#D5ECDE" );

		/// <summary>Fill of the CNC start button while pressed.</summary>
		public static readonly Color CNC_START_TINT_PRESSED = ColorTranslator.FromHtml( "#C3E3CF" );

		/// <summary>Border and text of the CNC hold button.</summary>
		public static readonly Color CNC_HOLD = ColorTranslator.FromHtml( "#C26E0A" );

		/// <summary>Fill of the CNC hold button.</summary>
		public static readonly Color CNC_HOLD_TINT = ColorTranslator.FromHtml( "#FDF0DE" );

		/// <summary>Fill of the CNC hold button under the mouse.</summary>
		public static readonly Color CNC_HOLD_TINT_HOVER = ColorTranslator.FromHtml( "#FAE3C4" );

		/// <summary>Fill of the CNC hold button while pressed.</summary>
		public static readonly Color CNC_HOLD_TINT_PRESSED = ColorTranslator.FromHtml( "#F6D4A8" );

		/// <summary>Border and text of the CNC stop button.</summary>
		public static readonly Color CNC_STOP = ColorTranslator.FromHtml( "#C2413B" );

		/// <summary>Fill of the CNC stop button.</summary>
		public static readonly Color CNC_STOP_TINT = ColorTranslator.FromHtml( "#FBE6E4" );

		/// <summary>Fill of the CNC stop button under the mouse.</summary>
		public static readonly Color CNC_STOP_TINT_HOVER = ColorTranslator.FromHtml( "#F7D5D2" );

		/// <summary>Fill of the CNC stop button while pressed.</summary>
		public static readonly Color CNC_STOP_TINT_PRESSED = ColorTranslator.FromHtml( "#F1C1BD" );
	}
}
