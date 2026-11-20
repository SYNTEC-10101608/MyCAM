using System;
using System.Collections.Generic;
using System.Drawing;
using CommonUI.Theme;
using OCC.Quantity;

namespace MyCAM.Editor
{
	/// <summary>
	/// The only definition of the path layer colors (data colors): the 3D path color and the layer swatch of the
	/// main window tool bar both come from here.
	/// </summary>
	internal static class PathLayerColor
	{
		/// <summary>
		/// Number of colors; valid indexes are 0 (not machined) to Count - 1.
		/// </summary>
		public static int Count => LAYER_COLOR_LIST.Count;

		/// <summary>
		/// 3D color of a path.
		/// </summary>
		/// <param name="nColorIndex">0 for a path that is not machined, otherwise the tech layer.</param>
		/// <returns>OCCT color; the default layer color if the index is out of range.</returns>
		public static Quantity_Color GetPathColor( int nColorIndex )
		{
			return new Quantity_Color( GetColorName( nColorIndex ) );
		}

		/// <summary>
		/// Swatch color of a tech layer button, the same color as the 3D path.
		/// </summary>
		/// <param name="nColorIndex">Tech layer.</param>
		/// <returns>Background color of the swatch.</returns>
		public static Color GetSwatchColor( int nColorIndex )
		{
			return DataColor.ToScreenColor( GetColorName( nColorIndex ) );
		}

		/// <summary>
		/// Text color on a swatch: the one of the body text and the light text with the higher contrast.
		/// </summary>
		/// <param name="swatchColor">Background color of the swatch.</param>
		/// <returns>Text color.</returns>
		public static Color GetSwatchTextColor( Color swatchColor )
		{
			double dLuminance = GetRelativeLuminance( swatchColor );
			double dLightContrast = ( GetRelativeLuminance( ThemeColors.TEXT_ON_ACCENT ) + CONTRAST_OFFSET )
				/ ( dLuminance + CONTRAST_OFFSET );
			double dDarkContrast = ( dLuminance + CONTRAST_OFFSET )
				/ ( GetRelativeLuminance( ThemeColors.TEXT ) + CONTRAST_OFFSET );
			return dLightContrast > dDarkContrast ? ThemeColors.TEXT_ON_ACCENT : ThemeColors.TEXT;
		}

		// index 0 is the path that is not machined, 1 to 10 are the tech layers
		static readonly List<Quantity_NameOfColor> LAYER_COLOR_LIST = new List<Quantity_NameOfColor> {
			Quantity_NameOfColor.Quantity_NOC_GRAY50,
			Quantity_NameOfColor.Quantity_NOC_BLUE,
			Quantity_NameOfColor.Quantity_NOC_DARKORANGE2,
			Quantity_NameOfColor.Quantity_NOC_PURPLE,
			Quantity_NameOfColor.Quantity_NOC_YELLOW2,
			Quantity_NameOfColor.Quantity_NOC_GREEN3,
			Quantity_NameOfColor.Quantity_NOC_TOMATO2,
			Quantity_NameOfColor.Quantity_NOC_YELLOWGREEN,
			Quantity_NameOfColor.Quantity_NOC_BROWN,
			Quantity_NameOfColor.Quantity_NOC_MAGENTA1,
			Quantity_NameOfColor.Quantity_NOC_CYAN1,
		};

		const int DEFAULT_COLOR_INDEX = 1;

		// WCAG contrast ratio: ( L1 + 0.05 ) / ( L2 + 0.05 )
		const double CONTRAST_OFFSET = 0.05;
		const double SRGB_LINEAR_LIMIT = 0.04045;
		const double SRGB_LINEAR_SLOPE = 12.92;

		static Quantity_NameOfColor GetColorName( int nColorIndex )
		{
			// index out of range
			if( nColorIndex < 0 || nColorIndex >= LAYER_COLOR_LIST.Count ) {
				return LAYER_COLOR_LIST[ DEFAULT_COLOR_INDEX ];
			}
			return LAYER_COLOR_LIST[ nColorIndex ];
		}

		// WCAG relative luminance of an sRGB color
		static double GetRelativeLuminance( Color color )
		{
			return 0.2126 * ToLinear( color.R ) + 0.7152 * ToLinear( color.G ) + 0.0722 * ToLinear( color.B );
		}

		static double ToLinear( byte nChannel )
		{
			double dValue = nChannel / 255.0;

			// sRGB transfer function
			if( dValue <= SRGB_LINEAR_LIMIT ) {
				return dValue / SRGB_LINEAR_SLOPE;
			}
			return Math.Pow( ( dValue + 0.055 ) / 1.055, 2.4 );
		}
	}
}
