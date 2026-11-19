using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CommonUI.Theme;

namespace CommonUI.Controls
{
	/// <summary>
	/// Read-only list with the theme colors: white surface, divider grid lines, flat header.
	/// The colors are fixed here and not written to the designer file.
	/// </summary>
	public class ThemedDataGridView : DataGridView
	{
		/// <summary>
		/// Create a read-only list in the theme colors and fonts.
		/// </summary>
		public ThemedDataGridView()
		{
			base.BackgroundColor = ThemeColors.SURFACE;
			base.BorderStyle = BorderStyle.None;
			base.GridColor = ThemeColors.DIVIDER;
			EnableHeadersVisualStyles = false;
			ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
			ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.SURFACE_ALT;
			ColumnHeadersDefaultCellStyle.ForeColor = ThemeColors.TEXT_SECONDARY;
			ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeColors.SURFACE_ALT;
			DefaultCellStyle.BackColor = ThemeColors.SURFACE;
			DefaultCellStyle.SelectionBackColor = ThemeColors.ACCENT_TINT_STRONG;
			DefaultCellStyle.SelectionForeColor = ThemeColors.TEXT;
		}

		/// <summary>
		/// Text role of the cells, e.g. Error for current alarms.
		/// </summary>
		[Category( "Appearance" )]
		[DefaultValue( TextRole.Body )]
		public TextRole CellRole
		{
			get => m_CellRole;
			set
			{
				m_CellRole = value;
				DefaultCellStyle.ForeColor = ThemeText.GetColor( value );
				Invalidate();
			}
		}

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public new Color BackgroundColor
		{
			get => base.BackgroundColor;
			set => base.BackgroundColor = value;
		}

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public new BorderStyle BorderStyle
		{
			get => base.BorderStyle;
			set => base.BorderStyle = value;
		}

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public new Color GridColor
		{
			get => base.GridColor;
			set => base.GridColor = value;
		}

		TextRole m_CellRole = TextRole.Body;
	}
}
