using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CommonUI.Theme;

namespace CommonUI.Forms
{
	/// <summary>
	/// Appearance layer of embedded panels (UserControl): font, colors and no auto scaling.
	/// Like <see cref="DialogBase"/>, the appearance is fixed here and hidden from the designer.
	/// </summary>
	public class PanelBase : UserControl
	{
		public PanelBase()
		{
			base.AutoScaleMode = AutoScaleMode.None;
			base.Font = ThemeFonts.Body;
			base.BackColor = ThemeColors.WINDOW;
			base.ForeColor = ThemeColors.TEXT;
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
