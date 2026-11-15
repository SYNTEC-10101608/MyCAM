using System.Windows.Forms;
using CommonUI.Controls;

namespace CommonUI.Forms
{
	/// <summary>
	/// Apply language texts by control name (see ui-style.instructions.md, section 7).
	/// Only controls whose Text is a caption are changed; input controls keep their content.
	/// </summary>
	public static class LanguageApplier
	{
		/// <summary>
		/// Set the caption of every descendant whose name is an ID in the language file.
		/// Buttons with a common text always take it from the common language file.
		/// </summary>
		/// <param name="root">Form or panel; its own Text is not changed.</param>
		/// <param name="languageManager">Language file of the screen.</param>
		public static void Apply( Control root, LanguageManager.LanguageManager languageManager )
		{
			// nothing to apply
			if( root == null || languageManager == null ) {
				return;
			}
			foreach( Control control in root.Controls ) {
				ApplyToControl( control, languageManager );
				Apply( control, languageManager );
			}
		}

		static void ApplyToControl( Control control, LanguageManager.LanguageManager languageManager )
		{
			// only captions are texts to translate
			bool isCaption = control is Label || control is ButtonBase || control is SectionPanel || control is GroupBox;
			if( isCaption == false || string.IsNullOrEmpty( control.Name ) ) {
				return;
			}
			// common button texts such as "OK" and "Cancel" are the same in every dialog
			if( control is ThemedButton button && button.CommonText != CommonButtonText.None ) {
				button.Text = CommonTexts.Get( button.CommonText );
			}
			else if( languageManager.HasString( control.Name ) ) {
				control.Text = languageManager.GetString( control.Name );
			}
		}
	}
}
