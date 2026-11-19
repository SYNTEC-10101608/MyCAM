using System;
using System.Drawing;
using System.Windows.Forms;

namespace CommonUI.Layout
{
	/// <summary>
	/// The only place that calculates where a dialog is centered (see ui-style.instructions.md, section 4.3).
	/// </summary>
	public static class DialogPlacement
	{
		/// <summary>
		/// Screen rectangle that dialogs are centered in, registered by the main window.
		/// Landscape: the whole main window; portrait: the CAM area above the CNC host.
		/// An empty rectangle falls back to the owner, then to the screen working area.
		/// </summary>
		public static Func<Rectangle> AreaProvider
		{
			get; set;
		}

		/// <summary>
		/// Location that centers the form in the dialog area, kept inside the screen working area.
		/// </summary>
		/// <param name="form">Form to place; its current size is used.</param>
		/// <returns>Screen location of the form.</returns>
		public static Point GetCenterLocation( Form form )
		{
			Rectangle area = GetArea( form );
			Rectangle workingArea = Screen.FromRectangle( area ).WorkingArea;
			int nLeft = area.Left + ( area.Width - form.Width ) / 2;
			int nTop = area.Top + ( area.Height - form.Height ) / 2;
			nLeft = Math.Max( workingArea.Left, Math.Min( nLeft, workingArea.Right - form.Width ) );
			nTop = Math.Max( workingArea.Top, Math.Min( nTop, workingArea.Bottom - form.Height ) );
			return new Point( nLeft, nTop );
		}

		/// <summary>
		/// Center a form again, e.g. after it changed its size.
		/// </summary>
		/// <param name="form">Form to move.</param>
		public static void MoveToCenter( Form form )
		{
			// nothing to move
			if( form == null || form.IsDisposed ) {
				return;
			}
			form.Location = GetCenterLocation( form );
		}

		static Rectangle GetArea( Form form )
		{
			Rectangle area = AreaProvider?.Invoke() ?? Rectangle.Empty;
			if( area.Width > 0 && area.Height > 0 ) {
				return area;
			}

			// no main window registered yet
			if( form.Owner != null ) {
				return form.Owner.Bounds;
			}
			return Screen.FromControl( form ).WorkingArea;
		}
	}
}
