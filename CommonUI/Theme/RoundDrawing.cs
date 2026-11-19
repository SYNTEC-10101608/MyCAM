using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CommonUI.Theme
{
	/// <summary>
	/// Rounded rectangle paths for the drawing of CommonUI controls.
	/// </summary>
	public static class RoundDrawing
	{
		/// <summary>
		/// Create a rounded rectangle path.
		/// </summary>
		/// <param name="bounds">Rectangle of the path.</param>
		/// <param name="radius">Corner radius.</param>
		/// <returns>The path; the caller disposes it.</returns>
		public static GraphicsPath CreatePath( RectangleF bounds, float radius )
		{
			GraphicsPath path = new GraphicsPath();
			if( bounds.Width <= 0 || bounds.Height <= 0 ) {
				return path;
			}
			float diameter = Math.Min( Math.Max( 0, radius * 2 ), Math.Min( bounds.Width, bounds.Height ) );
			if( diameter == 0 ) {
				path.AddRectangle( bounds );
			}
			else {
				path.AddArc( bounds.Left, bounds.Top, diameter, diameter, 180, 90 );
				path.AddArc( bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90 );
				path.AddArc( bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90 );
				path.AddArc( bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90 );
			}
			path.CloseFigure();
			return path;
		}
	}
}
