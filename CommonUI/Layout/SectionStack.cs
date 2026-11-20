using System.Windows.Forms;
using CommonUI.Theme;

namespace CommonUI.Layout
{
	/// <summary>
	/// Re-arrange sections of a dialog after some of them are shown or hidden at runtime,
	/// so dialogs never calculate coordinates themselves.
	/// </summary>
	public static class SectionStack
	{
		/// <summary>
		/// Stack the given sections from the top margin with the section gap. Pass only the sections that are
		/// shown (Control.Visible is false for every control before the form itself is shown).
		/// </summary>
		/// <param name="sectionArray">Shown sections in display order.</param>
		/// <returns>Bottom of the last section (the top margin if none is given).</returns>
		public static int Arrange( params Control[] sectionArray )
		{
			int nTop = ThemeMetrics.DIALOG_MARGIN;
			bool hasSection = false;
			foreach( Control section in sectionArray ) {
				// skip missing sections
				if( section == null ) {
					continue;
				}
				if( hasSection ) {
					nTop += ThemeMetrics.SECTION_GAP;
				}
				section.Top = nTop;
				nTop = section.Bottom;
				hasSection = true;
			}
			return nTop;
		}

		/// <summary>
		/// Put the button bar below the content and fit the client height of the form.
		/// </summary>
		/// <param name="form">Dialog to resize.</param>
		/// <param name="nContentBottom">Bottom of the content, e.g. from <see cref="Arrange"/>.</param>
		/// <param name="buttonArray">Buttons of the button bar; their horizontal positions are kept.</param>
		public static void PlaceButtonBar( Form form, int nContentBottom, params Control[] buttonArray )
		{
			int nButtonTop = nContentBottom + ThemeMetrics.BUTTON_BAR_GAP;
			foreach( Control button in buttonArray ) {
				button.Top = nButtonTop;
			}
			int nClientHeight = buttonArray.Length > 0 ?
				nButtonTop + ThemeMetrics.BUTTON_HEIGHT + ThemeMetrics.DIALOG_MARGIN :
				nContentBottom + ThemeMetrics.DIALOG_MARGIN;
			form.ClientSize = new System.Drawing.Size( form.ClientSize.Width, nClientHeight );
		}

		/// <summary>
		/// Show one page of a wizard and hide the others; the form fits the page and is centered again.
		/// Each page sits at the top-left margin and holds its own sections and buttons.
		/// </summary>
		/// <param name="form">Dialog to switch.</param>
		/// <param name="page">Page to show.</param>
		/// <param name="pageArray">All pages of the wizard.</param>
		public static void ShowPage( Form form, Control page, params Control[] pageArray )
		{
			// hide the form during the switch so the user does not see the intermediate states
			bool isShown = form.Visible;
			if( isShown ) {
				form.Hide();
			}
			form.SuspendLayout();

			// hide all pages first so the form resizes without showing stale content
			foreach( Control otherPage in pageArray ) {
				otherPage.Visible = false;
			}
			FitToPage( form, page );
			form.ResumeLayout( false );
			form.PerformLayout();

			// show the page only after the form has been resized, then center the new size
			page.Visible = true;
			DialogPlacement.MoveToCenter( form );
			if( isShown ) {
				form.Show();
			}
			form.Refresh();
		}

		static void FitToPage( Form form, Control page )
		{
			form.ClientSize = new System.Drawing.Size( page.Right + ThemeMetrics.DIALOG_MARGIN,
				page.Bottom + ThemeMetrics.DIALOG_MARGIN );
		}
	}
}
