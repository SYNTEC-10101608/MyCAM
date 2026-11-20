using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CommonUI.Theme;

namespace CommonUI.Controls
{
	/// <summary>
	/// Swatch of a data color (layer, axis, 3D object color) in a legend or next to a value.
	/// The data color is not a theme color, so it is set at runtime from the named constant that also colors the
	/// 3D object; the designer only shows a neutral placeholder.
	/// </summary>
	public class ColorSwatch : Control
	{
		/// <summary>
		/// Create an empty swatch.
		/// </summary>
		public ColorSwatch()
		{
			SetStyle( ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
				ControlStyles.SupportsTransparentBackColor, true );
			SetStyle( ControlStyles.Selectable, false );
			TabStop = false;
			base.BackColor = Color.Transparent;
		}

		/// <summary>
		/// Data color shown by the swatch; set at runtime, never serialized into the designer file.
		/// Color.Empty shows the placeholder.
		/// </summary>
		[Browsable( false )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Color SwatchColor
		{
			get => m_SwatchColor;
			set
			{
				m_SwatchColor = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Square legend or round marker.
		/// </summary>
		[Category( "Appearance" )]
		[DefaultValue( SwatchShape.Square )]
		public SwatchShape Shape
		{
			get => m_Shape;
			set
			{
				m_Shape = value;
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

		protected override void OnEnabledChanged( EventArgs e )
		{
			base.OnEnabledChanged( e );
			Invalidate();
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			// a disabled or unset swatch is drawn as a neutral placeholder
			bool hasColor = Enabled && m_SwatchColor != Color.Empty;
			Color fillColor = hasColor ? m_SwatchColor : ThemeColors.DIVIDER;
			using( SolidBrush brush = new SolidBrush( fillColor ) ) {
				if( m_Shape == SwatchShape.Dot ) {
					PaintDot( e.Graphics, brush );
				}
				else {
					PaintSquare( e.Graphics, brush );
				}
			}
		}

		void PaintSquare( Graphics graphics, Brush brush )
		{
			Rectangle bounds = new Rectangle( 0, 0, Width - 1, Height - 1 );
			graphics.FillRectangle( brush, bounds );
			using( Pen pen = new Pen( ThemeColors.BORDER_STRONG ) ) {
				graphics.DrawRectangle( pen, bounds );
			}
		}

		void PaintDot( Graphics graphics, Brush brush )
		{
			// keep the dot round when the panel scales the control unevenly
			int nDiameter = Math.Min( Width, Height );
			float fLeft = ( Width - nDiameter ) / 2F;
			float fTop = ( Height - nDiameter ) / 2F;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.FillEllipse( brush, fLeft, fTop, nDiameter, nDiameter );
		}

		Color m_SwatchColor = Color.Empty;
		SwatchShape m_Shape = SwatchShape.Square;
	}
}
