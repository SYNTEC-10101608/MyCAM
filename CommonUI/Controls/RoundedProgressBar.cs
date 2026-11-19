using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CommonUI.Theme;
namespace CommonUI.Controls
{
	/// <summary>
	/// Progress bar with rounded track and fill in the theme colors; the fill uses ForeColor.
	/// </summary>
	public class RoundedProgressBar : Control
	{
		int m_Minimum = 0;
		int m_Maximum = 100;
		int m_Value = 0;

		const int BORDER_WIDTH = 3;

		/// <summary>
		/// Create a progress bar from 0 to 100.
		/// </summary>
		public RoundedProgressBar()
		{
			SetStyle( ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
				ControlStyles.SupportsTransparentBackColor, true );
			BackColor = Color.Transparent;
			ForeColor = ThemeColors.ACCENT;
		}

		/// <summary>
		/// Lower bound of the value.
		/// </summary>
		public int Minimum
		{
			get { return m_Minimum; }
			set
			{
				m_Minimum = value;
				if( m_Maximum < m_Minimum ) {
					m_Maximum = m_Minimum;
				}
				Value = m_Value;
			}
		}

		/// <summary>
		/// Upper bound of the value.
		/// </summary>
		public int Maximum
		{
			get { return m_Maximum; }
			set
			{
				m_Maximum = Math.Max( value, m_Minimum );
				Value = m_Value;
			}
		}

		/// <summary>
		/// Current value, clamped to Minimum and Maximum.
		/// </summary>
		public int Value
		{
			get { return m_Value; }
			set
			{
				int nValue = Math.Max( m_Minimum, Math.Min( m_Maximum, value ) );
				if( nValue == m_Value ) {
					return;
				}
				m_Value = nValue;
				Invalidate();
			}
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaintBackground( e );
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			RectangleF outer = new RectangleF( BORDER_WIDTH / 2f, BORDER_WIDTH / 2f,
				Width - BORDER_WIDTH, Height - BORDER_WIDTH );
			if( outer.Width <= 0 || outer.Height <= 0 ) {
				return;
			}

			using( GraphicsPath trackPath = RoundDrawing.CreatePath( outer, outer.Height / 2f ) ) {
				using( SolidBrush trackBrush = new SolidBrush( ThemeColors.DIVIDER ) ) {
					e.Graphics.FillPath( trackBrush, trackPath );
				}
				using( Pen borderPen = new Pen( ThemeColors.DIVIDER, BORDER_WIDTH ) ) {
					e.Graphics.DrawPath( borderPen, trackPath );
				}
			}

			RectangleF inner = new RectangleF( BORDER_WIDTH, BORDER_WIDTH,
				Width - BORDER_WIDTH * 2, Height - BORDER_WIDTH * 2 );
			if( inner.Width <= 0 || inner.Height <= 0 || m_Maximum <= m_Minimum ) {
				return;
			}

			float fFillWidth = (float)( inner.Width * ( m_Value - m_Minimum ) / ( m_Maximum - m_Minimum ) );
			if( fFillWidth > 0 ) {
				RectangleF fillRect = new RectangleF( inner.X, inner.Y, fFillWidth, inner.Height );
				using( GraphicsPath fillPath = RoundDrawing.CreatePath( fillRect, fillRect.Height / 2f ) ) {
					using( SolidBrush fillBrush = new SolidBrush( ForeColor ) ) {
						e.Graphics.FillPath( fillBrush, fillPath );
					}
				}
			}
		}
	}
}
