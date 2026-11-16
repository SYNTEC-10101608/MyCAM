using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CommonUI.Theme;

namespace CommonUI.Controls
{
	/// <summary>
	/// Section card: square white surface with a border; Text is the section title drawn in the title bar.
	/// Children are placed with fixed coordinates (first row at SECTION_CONTENT_TOP when titled).
	/// </summary>
	public class SectionPanel : Panel
	{
		public SectionPanel()
		{
			SetStyle( ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true );
			base.BackColor = ThemeColors.SURFACE;
		}

		/// <summary>
		/// Section title; empty for a card without title bar.
		/// </summary>
		[Browsable( true )]
		[EditorBrowsable( EditorBrowsableState.Always )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Visible )]
		[Category( "Appearance" )]
		public override string Text
		{
			get => base.Text;
			set
			{
				base.Text = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Mark the section as the current one (e.g. the current step of a wizard): accent title bar.
		/// </summary>
		[Category( "Appearance" )]
		[DefaultValue( false )]
		public bool Highlighted
		{
			get => m_isHighlighted;
			set
			{
				if( m_isHighlighted == value ) {
					return;
				}
				m_isHighlighted = value;
				Invalidate();
			}
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
		public new BorderStyle BorderStyle
		{
			get => base.BorderStyle;
			set => base.BorderStyle = value;
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );
			Graphics graphics = e.Graphics;
			using( Pen pen = new Pen( ThemeColors.BORDER ) ) {
				graphics.DrawRectangle( pen, 0, 0, Width - 1, Height - 1 );
			}

			// untitled card
			if( string.IsNullOrEmpty( Text ) ) {
				return;
			}
			if( m_isHighlighted ) {
				using( SolidBrush brush = new SolidBrush( ThemeColors.ACCENT_TINT ) ) {
					graphics.FillRectangle( brush, 1, 1, Width - 2, ThemeMetrics.SECTION_TITLE_HEIGHT - 1 );
				}
			}
			int nAccentTop = ( ThemeMetrics.SECTION_TITLE_HEIGHT - ACCENT_HEIGHT ) / 2;
			using( SolidBrush brush = new SolidBrush( ThemeColors.ACCENT ) ) {
				graphics.FillRectangle( brush, ThemeMetrics.SECTION_PADDING, nAccentTop, ACCENT_WIDTH, ACCENT_HEIGHT );
			}
			int nTextLeft = ThemeMetrics.SECTION_PADDING + ACCENT_WIDTH + ThemeMetrics.CONTROL_GAP;
			Rectangle textBounds = new Rectangle( nTextLeft, 0,
				Width - nTextLeft - ThemeMetrics.SECTION_PADDING, ThemeMetrics.SECTION_TITLE_HEIGHT );
			Color titleColor = Enabled == false ? ThemeColors.TEXT_DISABLED :
				m_isHighlighted ? ThemeColors.ACCENT : ThemeColors.TEXT;
			TextRenderer.DrawText( graphics, Text, ThemeFonts.Emphasis, textBounds, titleColor,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
				TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix );
		}

		protected override void OnEnabledChanged( System.EventArgs e )
		{
			base.OnEnabledChanged( e );
			Invalidate();
		}

		protected override void OnTextChanged( System.EventArgs e )
		{
			base.OnTextChanged( e );
			Invalidate();
		}

		const int ACCENT_WIDTH = 3;
		const int ACCENT_HEIGHT = 14;

		bool m_isHighlighted;
	}
}
