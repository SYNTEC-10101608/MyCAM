using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CommonUI.Theme;

namespace CommonUI.Forms
{
	/// <summary>
	/// Appearance layer of the main window: font, colors, no auto scaling and the themed tool bar drawing.
	/// Like <see cref="DialogBase"/>, the appearance is fixed here and hidden from the designer.
	/// </summary>
	public class MainWindowBase : Form
	{
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
