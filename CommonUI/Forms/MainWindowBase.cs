using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CommonUI.Language;
using CommonUI.Theme;

namespace CommonUI.Forms
{
	/// <summary>
	/// Appearance layer of the main window: font, colors, no auto scaling and the themed tool bar drawing.
	/// Like <see cref="DialogBase"/>, the appearance is fixed here and hidden from the designer.
	/// </summary>
	public class MainWindowBase : Form
	{
		/// <summary>
		/// Apply the main window appearance and the theme renderer of menus and tool strips.
		/// </summary>
		public MainWindowBase()
		{
			// every menu and tool bar in the application draws with the theme
			ToolStripManager.Renderer = new ThemedToolStripRenderer();
			base.AutoScaleMode = AutoScaleMode.None;
			base.Font = ThemeFonts.Body;
			base.BackColor = ThemeColors.WINDOW;
			base.ForeColor = ThemeColors.TEXT;
		}

		/// <summary>
		/// Apply the font of the current UI language after the language is switched at runtime.
		/// </summary>
		public void ApplyThemeFont()
		{
			base.Font = ThemeFonts.Body;
			ApplyToolStripFont( this );
		}

		protected override void OnLoad( EventArgs e )
		{
			ApplyToolStripFont( this );
			base.OnLoad( e );
		}

		/// <summary>
		/// Apply the language texts by name, like <see cref="DialogBase"/>: caption controls, menu and tool bar items
		/// (tool tips from "&lt;name&gt;_ToolTip"), including the context menus of the controls.
		/// The main window calls it at start-up and when the language changes; runtime texts are set after it.
		/// </summary>
		protected void ApplyLanguage( LanguageManager.LanguageManager languageManager )
		{
			bool isDesignTime = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;
			if( isDesignTime ) {
				return;
			}
			LanguageApplier.Apply( this, languageManager );
			ApplyContextMenuLanguage( this, languageManager );
		}

		static void ApplyContextMenuLanguage( Control parent, LanguageManager.LanguageManager languageManager )
		{
			foreach( Control child in parent.Controls ) {
				if( child.ContextMenuStrip != null ) {
					LanguageApplier.ApplyToItems( child.ContextMenuStrip.Items, languageManager );
				}
				ApplyContextMenuLanguage( child, languageManager );
			}
		}

		// a tool strip does not inherit its parent's font but uses the system menu font, which ignores the UI language;
		// drop-down menus take the font of their owner item, so only the top-level strips are set
		static void ApplyToolStripFont( Control parent )
		{
			foreach( Control child in parent.Controls ) {
				if( child is ToolStrip ) {
					child.Font = ThemeFonts.Body;
				}
				if( child.ContextMenuStrip != null ) {
					child.ContextMenuStrip.Font = ThemeFonts.Body;
				}
				ApplyToolStripFont( child );
			}
		}

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public override Font Font
		{
			get => base.Font;
			set => base.Font = value;
		}

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public override Color BackColor
		{
			get => base.BackColor;
			set => base.BackColor = value;
		}

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public override Color ForeColor
		{
			get => base.ForeColor;
			set => base.ForeColor = value;
		}

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public new AutoScaleMode AutoScaleMode
		{
			get => base.AutoScaleMode;
			set => base.AutoScaleMode = value;
		}

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public new SizeF AutoScaleDimensions
		{
			get => base.AutoScaleDimensions;
			set => base.AutoScaleDimensions = value;
		}
	}
}
