using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CommonUI.Controls
{
	public class GroupPanel : Panel
	{
		public GroupPanel()
		{
			SetStyle( ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true );
			BackColor = Color.White;
		}

		[Category( "Appearance" )]
		[DefaultValue( 7 )]
		public int CornerRadius
		{
			get
			{
				return m_CornerRadius;
			}
			set
			{
				m_CornerRadius = Math.Max( 0, value );
				Invalidate();
			}
		}

		[Category( "Appearance" )]
		[DefaultValue( typeof( Color ), "209, 223, 237" )]
		public Color BorderColor
		{
			get
			{
				return m_BorderColor;
			}
			set
			{
				m_BorderColor = value;
				Invalidate();
			}
		}

		[Category( "Appearance" )]
		[DefaultValue( true )]
		public bool ShowAccent
		{
			get
			{
				return m_ShowAccent;
			}
			set
			{
				if( m_ShowAccent == value ) {
					return;
				}
				m_ShowAccent = value;
				Invalidate();
			}
		}

		[Category( "Appearance" )]
		[DefaultValue( typeof( Color ), "0, 142, 203" )]
		public Color AccentColor
		{
			get
			{
				return m_AccentColor;
			}
			set
			{
				m_AccentColor = value;
				Invalidate();
			}
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
			e.Graphics.Clear( Parent == null ? SystemColors.Control : Parent.BackColor );
			if( Width < 2 || Height < 2 ) {
				return;
			}
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using( GraphicsPath path = RoundDrawing.CreatePath(
				new RectangleF( 0.5F, 0.5F, Width - 1F, Height - 1F ), CornerRadius * DeviceDpi / 96F ) )
			using( SolidBrush brush = new SolidBrush( SystemInformation.HighContrast ? SystemColors.Control : BackColor ) ) {
				e.Graphics.FillPath( brush, path );
			}
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );
			if( Width < 2 || Height < 2 ) {
				return;
			}
			float scale = DeviceDpi / 96F;
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using( GraphicsPath path = RoundDrawing.CreatePath(
				new RectangleF( 0.5F, 0.5F, Width - 1F, Height - 1F ), CornerRadius * scale ) )
			using( Pen pen = new Pen( SystemInformation.HighContrast ? SystemColors.WindowText : BorderColor, scale ) ) {
				e.Graphics.DrawPath( pen, path );
			}
			if( ShowAccent ) {
				using( GraphicsPath accent = RoundDrawing.CreatePath( new RectangleF( 5 * scale, 7 * scale, 3 * scale, 12 * scale ), scale ) )
				using( SolidBrush brush = new SolidBrush( SystemInformation.HighContrast ? SystemColors.Highlight : AccentColor ) ) {
					e.Graphics.FillPath( brush, accent );
				}
			}
		}

		int m_CornerRadius = 7;
		Color m_BorderColor = Color.FromArgb( 209, 223, 237 );
		bool m_ShowAccent = true;
		Color m_AccentColor = Color.FromArgb( 0, 142, 203 );
	}
}
