using System;
using System.Drawing;
using OCC.Quantity;

namespace MyCAM.Editor
{
	/// <summary>
	/// Conversion of the OCCT data colors (3D objects) to the screen colors of their swatches.
	/// </summary>
	internal static class DataColor
	{
		/// <summary>
		/// Screen color of an OCCT named color, as the 3D view shows it.
		/// </summary>
		/// <param name="colorName">OCCT named color.</param>
		/// <returns>sRGB color for a swatch.</returns>
		public static Color ToScreenColor( Quantity_NameOfColor colorName )
		{
			// the named colors are sRGB values, as the screen shows them
			Quantity_Color color = new Quantity_Color( colorName );
			double dRed = 0;
			double dGreen = 0;
			double dBlue = 0;
			color.Values( ref dRed, ref dGreen, ref dBlue, Quantity_TypeOfColor.Quantity_TOC_sRGB );
			return Color.FromArgb( ToByte( dRed ), ToByte( dGreen ), ToByte( dBlue ) );
		}

		static int ToByte( double dValue )
		{
			int nValue = (int)Math.Round( dValue * 255 );
			return Math.Max( 0, Math.Min( 255, nValue ) );
		}
	}
}
