using System.Drawing;
using System.Windows.Forms;

namespace CommonUI.Theme
{
	/// <summary>
	/// Flat drawing of menus and tool bars with the theme colors. Item back colors set by the screen
	/// (layer legend, tool running state) are kept and drawn as they are.
	/// </summary>
	public class ThemedToolStripRenderer : ToolStripProfessionalRenderer
	{
		/// <summary>
		/// Create the renderer with the theme colors.
		/// </summary>
		public ThemedToolStripRenderer()
			: base( new ThemedColorTable() )
		{
			RoundedEdges = false;
		}

		protected override void OnRenderToolStripBorder( ToolStripRenderEventArgs e )
		{
			// drop-down menus keep a thin border; tool bars have none
			if( e.ToolStrip is ToolStripDropDown ) {
				using( Pen pen = new Pen( ThemeColors.BORDER ) ) {
					Rectangle bounds = new Rectangle( Point.Empty, e.ToolStrip.Size );
					e.Graphics.DrawRectangle( pen, 0, 0, bounds.Width - 1, bounds.Height - 1 );
				}
			}
		}

		protected override void OnRenderItemText( ToolStripItemTextRenderEventArgs e )
		{
			// the screen may color an item text (e.g. the layer legend); otherwise use the theme text colors
			bool isDefaultColor = e.Item.ForeColor == SystemColors.ControlText
				|| e.Item.ForeColor == SystemColors.MenuText;
			if( e.Item.Enabled == false ) {
				e.TextColor = ThemeColors.TEXT_DISABLED;
			}
			else if( isDefaultColor ) {
				e.TextColor = ThemeColors.TEXT;
			}
			base.OnRenderItemText( e );
		}

		protected override void OnRenderSeparator( ToolStripSeparatorRenderEventArgs e )
		{
			Rectangle bounds = new Rectangle( Point.Empty, e.Item.Size );
			using( Pen pen = new Pen( ThemeColors.DIVIDER ) ) {
				if( e.Vertical ) {
					int nX = bounds.Width / 2;
					e.Graphics.DrawLine( pen, nX, bounds.Top + 4, nX, bounds.Bottom - 4 );
				}
				else {
					int nY = bounds.Height / 2;
					e.Graphics.DrawLine( pen, bounds.Left + 4, nY, bounds.Right - 4, nY );
				}
			}
		}

		class ThemedColorTable : ProfessionalColorTable
		{
			public ThemedColorTable()
			{
				UseSystemColors = false;
			}

			// tool bars and menu bar
			public override Color ToolStripGradientBegin => ThemeColors.WINDOW;
			public override Color ToolStripGradientMiddle => ThemeColors.WINDOW;
			public override Color ToolStripGradientEnd => ThemeColors.WINDOW;
			public override Color ToolStripBorder => ThemeColors.BORDER;
			public override Color MenuStripGradientBegin => ThemeColors.WINDOW;
			public override Color MenuStripGradientEnd => ThemeColors.WINDOW;
			public override Color ToolStripContentPanelGradientBegin => ThemeColors.WINDOW;
			public override Color ToolStripContentPanelGradientEnd => ThemeColors.WINDOW;
			public override Color ToolStripPanelGradientBegin => ThemeColors.WINDOW;
			public override Color ToolStripPanelGradientEnd => ThemeColors.WINDOW;
			public override Color GripDark => ThemeColors.BORDER_STRONG;
			public override Color GripLight => ThemeColors.WINDOW;
			public override Color SeparatorDark => ThemeColors.DIVIDER;
			public override Color SeparatorLight => ThemeColors.DIVIDER;

			// drop-down menus
			public override Color ToolStripDropDownBackground => ThemeColors.SURFACE;
			public override Color ImageMarginGradientBegin => ThemeColors.SURFACE;
			public override Color ImageMarginGradientMiddle => ThemeColors.SURFACE;
			public override Color ImageMarginGradientEnd => ThemeColors.SURFACE;
			public override Color MenuBorder => ThemeColors.BORDER;
			public override Color MenuItemBorder => ThemeColors.ACCENT_TINT_STRONG;

			// hover
			public override Color MenuItemSelected => ThemeColors.ACCENT_TINT;
			public override Color MenuItemSelectedGradientBegin => ThemeColors.HOVER;
			public override Color MenuItemSelectedGradientEnd => ThemeColors.HOVER;
			public override Color ButtonSelectedBorder => ThemeColors.BORDER_STRONG;
			public override Color ButtonSelectedHighlight => ThemeColors.HOVER;
			public override Color ButtonSelectedGradientBegin => ThemeColors.HOVER;
			public override Color ButtonSelectedGradientMiddle => ThemeColors.HOVER;
			public override Color ButtonSelectedGradientEnd => ThemeColors.HOVER;

			// pressed
			public override Color MenuItemPressedGradientBegin => ThemeColors.SURFACE;
			public override Color MenuItemPressedGradientMiddle => ThemeColors.SURFACE;
			public override Color MenuItemPressedGradientEnd => ThemeColors.SURFACE;
			public override Color ButtonPressedBorder => ThemeColors.ACCENT;
			public override Color ButtonPressedHighlight => ThemeColors.PRESSED;
			public override Color ButtonPressedGradientBegin => ThemeColors.PRESSED;
			public override Color ButtonPressedGradientMiddle => ThemeColors.PRESSED;
			public override Color ButtonPressedGradientEnd => ThemeColors.PRESSED;

			// checked (selected mode, toggles)
			public override Color ButtonCheckedHighlight => ThemeColors.ACCENT_TINT_STRONG;
			public override Color ButtonCheckedGradientBegin => ThemeColors.ACCENT_TINT_STRONG;
			public override Color ButtonCheckedGradientMiddle => ThemeColors.ACCENT_TINT_STRONG;
			public override Color ButtonCheckedGradientEnd => ThemeColors.ACCENT_TINT_STRONG;
			public override Color CheckBackground => ThemeColors.ACCENT_TINT_STRONG;
			public override Color CheckSelectedBackground => ThemeColors.ACCENT_TINT_STRONG;
			public override Color CheckPressedBackground => ThemeColors.ACCENT_TINT_STRONG;
			public override Color OverflowButtonGradientBegin => ThemeColors.WINDOW;
			public override Color OverflowButtonGradientMiddle => ThemeColors.WINDOW;
			public override Color OverflowButtonGradientEnd => ThemeColors.WINDOW;
		}
	}
}
