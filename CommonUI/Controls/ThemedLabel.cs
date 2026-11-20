using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CommonUI.Theme;

namespace CommonUI.Controls
{
	/// <summary>
	/// The only label of the screens. Body (the default) is plain text that inherits from its parent; the other roles
	/// are emphasis, value, secondary and status.
	/// Font and color follow the role and are not written to the designer file.
	/// </summary>
	public class ThemedLabel : Label
	{
		/// <summary>
		/// Text role; changing it at runtime is the way to show a status color.
		/// </summary>
		[Category( "Appearance" )]
		[DefaultValue( TextRole.Body )]
		public TextRole Role
		{
			get => m_Role;
			set
			{
				m_Role = value;
				ApplyRole();
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
		public override Color ForeColor
		{
			get => base.ForeColor;
			set => base.ForeColor = value;
		}

		void ApplyRole()
		{
			// body text inherits from the parent like a plain label
			if( m_Role == TextRole.Body ) {
				ResetFont();
				ResetForeColor();
				return;
			}
			base.Font = ThemeFonts.GetFont( m_Role );
			base.ForeColor = ThemeText.GetColor( m_Role );
		}

		TextRole m_Role = TextRole.Body;
	}
}
