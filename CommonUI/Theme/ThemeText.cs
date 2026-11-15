using System;
using System.Drawing;
using System.Windows.Forms;

namespace CommonUI.Theme
{
	/// <summary>
	/// Apply a text role to controls that are not <see cref="Controls.ThemedLabel"/>.
	/// </summary>
	public static class ThemeText
	{
		/// <summary>
		/// Color of a text role.
		/// </summary>
		/// <param name="role">Text role.</param>
		/// <returns>Theme color of the role.</returns>
		public static Color GetColor( TextRole role )
		{
			switch( role ) {
				case TextRole.Body:
				case TextRole.Emphasis:
				case TextRole.Title:
				case TextRole.Value:
					return ThemeColors.TEXT;
				case TextRole.Secondary:
					return ThemeColors.TEXT_SECONDARY;
				case TextRole.Error:
					return ThemeColors.ERROR;
				case TextRole.Warning:
					return ThemeColors.WARNING;
				case TextRole.Success:
					return ThemeColors.SUCCESS;
				default:
					throw new ArgumentOutOfRangeException( nameof( role ) );
			}
		}

		/// <summary>
		/// Set the font and the text color of the controls to a text role.
		/// </summary>
		/// <param name="role">Text role.</param>
		/// <param name="controlArray">Controls to change.</param>
		public static void SetRole( TextRole role, params Control[] controlArray )
		{
			if( controlArray == null ) {
				return;
			}
			Font font = ThemeFonts.GetFont( role );
			Color color = GetColor( role );
			foreach( Control control in controlArray ) {
				if( control == null ) {
					continue;
				}
				control.Font = font;
				control.ForeColor = color;
			}
		}
	}
}
