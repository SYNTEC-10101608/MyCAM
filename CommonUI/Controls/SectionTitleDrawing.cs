using System.Drawing;
using System.Windows.Forms;
using CommonUI.Theme;

namespace CommonUI.Controls
{
	// Section title look shared by SectionPanel's title bar and ThemedLabel with TextRole.SectionTitle:
	// a short accent bar followed by emphasized text, both vertically centered.
	internal static class SectionTitleDrawing
	{
		// width taken by the accent bar and the gap before the text
		public const int TEXT_INDENT = ACCENT_WIDTH + ThemeMetrics.CONTROL_GAP;

		public static void Draw( Graphics graphics, string szText, Rectangle bounds, Color textColor )
		{
			int nAccentTop = bounds.Top + ( bounds.Height - ACCENT_HEIGHT ) / 2;
			using( SolidBrush brush = new SolidBrush( ThemeColors.ACCENT ) ) {
				graphics.FillRectangle( brush, bounds.Left, nAccentTop, ACCENT_WIDTH, ACCENT_HEIGHT );
			}
			int nTextLeft = bounds.Left + TEXT_INDENT;
			Rectangle textBounds = new Rectangle( nTextLeft, bounds.Top, bounds.Right - nTextLeft, bounds.Height );
			TextRenderer.DrawText( graphics, szText, ThemeFonts.Emphasis, textBounds, textColor,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
				TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix );
		}

		const int ACCENT_WIDTH = 3;
		const int ACCENT_HEIGHT = 14;
	}
}
