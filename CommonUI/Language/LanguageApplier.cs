using System.Windows.Forms;
using CommonUI.Controls;
using CommonUI.Forms;

namespace CommonUI.Language
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
				// an embedded panel is a screen of its own with its own language file
				if( control is PanelBase ) {
					continue;
				}
				ApplyToControl( control, languageManager );
				ApplyToColumns( control as DataGridView, languageManager );
				ToolStrip toolStrip = control as ToolStrip;
				if( toolStrip != null ) {
					ApplyToItems( toolStrip.Items, languageManager );
				}
				Apply( control, languageManager );
			}
		}

		/// <summary>
		/// Set the text of every menu / tool bar item whose name is an ID, and its tool tip from the ID
		/// "&lt;name&gt;_ToolTip"; drop-down items are included.
		/// </summary>
		/// <param name="itemCollection">Items of a menu, tool bar or context menu.</param>
		/// <param name="languageManager">Language file of the screen.</param>
		public static void ApplyToItems( ToolStripItemCollection itemCollection,
			LanguageManager.LanguageManager languageManager )
		{
			// nothing to apply
			if( itemCollection == null || languageManager == null ) {
				return;
			}
			foreach( ToolStripItem item in itemCollection ) {
				if( string.IsNullOrEmpty( item.Name ) == false ) {
					if( languageManager.HasString( item.Name ) ) {
						item.Text = languageManager.GetString( item.Name );
					}
					string szToolTipId = item.Name + TOOL_TIP_SUFFIX;
					if( languageManager.HasString( szToolTipId ) ) {
						item.ToolTipText = languageManager.GetString( szToolTipId );
					}
				}
				ToolStripDropDownItem dropDownItem = item as ToolStripDropDownItem;
				if( dropDownItem != null && dropDownItem.HasDropDownItems ) {
					ApplyToItems( dropDownItem.DropDownItems, languageManager );
				}
			}
		}

		const string TOOL_TIP_SUFFIX = "_ToolTip";

		static void ApplyToControl( Control control, LanguageManager.LanguageManager languageManager )
		{
			// only captions are texts to translate
			bool isCaption = control is Label || control is ButtonBase || control is SectionPanel;
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

		// column headers of a list are captions as well, by column name
		static void ApplyToColumns( DataGridView grid, LanguageManager.LanguageManager languageManager )
		{
			if( grid == null ) {
				return;
			}
			foreach( DataGridViewColumn column in grid.Columns ) {
				if( string.IsNullOrEmpty( column.Name ) == false && languageManager.HasString( column.Name ) ) {
					column.HeaderText = languageManager.GetString( column.Name );
				}
			}
		}
	}
}
