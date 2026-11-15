using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CommonUI.Controls
{
	public class PanelButton : Button
	{
		public PanelButton()
		{
			SetStyle( ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true );
			FlatStyle = FlatStyle.Flat;
			FlatAppearance.BorderColor = Color.FromArgb( 208, 220, 234 );
			BackColor = Color.FromArgb( 242, 246, 250 );
			ForeColor = Color.FromArgb( 28, 58, 85 );
			UseVisualStyleBackColor = false;
		}

		[Category( "Appearance" )]
		[DefaultValue( 5 )]
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
		[DefaultValue( ButtonIndicator.None )]
		public ButtonIndicator Indicator
		{
			get
			{
				return m_Indicator;
			}
			set
			{
				m_Indicator = value;
				Invalidate();
			}
		}

		[Category( "Appearance" )]
		[DefaultValue( false )]
		public bool Active
		{
			get
			{
				return m_Active;
			}
			set
			{
				if( m_Active == value ) {
					return;
				}
				m_Active = value;
				Invalidate();
			}
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			if( ClientSize.Width < 2 || ClientSize.Height < 2 ) {
				return;
			}
			Graphics graphics = e.Graphics;
			if( Parent != null && Parent.BackColor.A < 255 ) {
				GraphicsState state = graphics.Save();
				try {
					graphics.TranslateTransform( -Left, -Top );
					using( PaintEventArgs parentArgs = new PaintEventArgs( graphics, Bounds ) ) {
						InvokePaintBackground( Parent, parentArgs );
						InvokePaint( Parent, parentArgs );
					}
				}
				finally {
					graphics.Restore( state );
				}
			}
			else {
				graphics.Clear( Parent == null ? SystemColors.Control : Parent.BackColor );
			}
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			float scale = DeviceDpi / 96F;
			Color fill = Active ? Color.FromArgb( 0, 142, 203 ) : BackColor;
			Color border = Active ? Color.FromArgb( 0, 142, 203 ) : FlatAppearance.BorderColor;
			Color text = Active ? Color.White : ForeColor;
			if( SystemInformation.HighContrast ) {
				fill = Active && Enabled ? SystemColors.Highlight : SystemColors.Control;
				border = SystemColors.WindowText;
				text = !Enabled ? SystemColors.GrayText : Active ? SystemColors.HighlightText : SystemColors.ControlText;
			}
			else if( !Enabled ) {
				fill = Color.FromArgb( 245, 247, 249 );
				border = Color.FromArgb( 222, 229, 236 );
				text = Color.FromArgb( 148, 160, 173 );
			}
			else if( m_Pressed ) {
				fill = ControlPaint.Dark( fill, 0.08F );
			}
			else if( m_Hovered ) {
				fill = ControlPaint.Light( fill, 0.12F );
			}
			RectangleF bounds = new RectangleF( 0.5F, 0.5F, Width - 1F, Height - 1F );
			using( GraphicsPath path = RoundDrawing.CreatePath( bounds, CornerRadius * scale ) )
			using( SolidBrush brush = new SolidBrush( fill ) )
			using( Pen pen = new Pen( border, scale ) ) {
				graphics.FillPath( brush, path );
				graphics.DrawPath( pen, path );
			}
			Rectangle content = Rectangle.Inflate( ClientRectangle, -(int)( 3 * scale ), -(int)( 2 * scale ) );
			if( Indicator != ButtonIndicator.None ) {
				int diameter = Math.Max( 4, (int)( 6 * scale ) );
				Rectangle lamp = new Rectangle( Width - diameter - (int)( 5 * scale ), ( Height - diameter ) / 2, diameter, diameter );
				Color lampColor = Color.FromArgb( 189, 199, 210 );
				if( Enabled && Indicator == ButtonIndicator.On ) {
					lampColor = Color.FromArgb( 0, 180, 137 );
				}
				else if( Enabled && Indicator == ButtonIndicator.Off ) {
					lampColor = Color.FromArgb( 105, 123, 142 );
				}
				using( SolidBrush lampBrush = new SolidBrush( lampColor ) ) {
					graphics.FillEllipse( lampBrush, lamp );
				}
				content.Width = Math.Max( 0, lamp.Left - content.Left - (int)( 2 * scale ) );
			}
			if( Image != null ) {
				graphics.DrawImage( Image, content.Left, ( Height - Image.Height ) / 2, Image.Width, Image.Height );
				content.X += Image.Width + (int)( 3 * scale );
				content.Width = Math.Max( 0, content.Width - Image.Width - (int)( 3 * scale ) );
			}
			TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
			if( TextAlign == ContentAlignment.MiddleLeft ) {
				flags |= TextFormatFlags.Left;
			}
			else if( TextAlign == ContentAlignment.MiddleRight ) {
				flags |= TextFormatFlags.Right;
			}
			else {
				flags |= TextFormatFlags.HorizontalCenter;
			}
			if( !UseMnemonic ) {
				flags |= TextFormatFlags.NoPrefix;
			}
			else if( !ShowKeyboardCues ) {
				flags |= TextFormatFlags.HidePrefix;
			}
			TextRenderer.DrawText( graphics, Text, Font, content, text, flags );
			if( Focused && ShowFocusCues ) {
				ControlPaint.DrawFocusRectangle( graphics, Rectangle.Inflate( ClientRectangle, -4, -4 ), text, fill );
			}
		}

		protected override void OnMouseEnter( EventArgs e )
		{
			base.OnMouseEnter( e );
			m_Hovered = true;
			Invalidate();
		}

		protected override void OnMouseLeave( EventArgs e )
		{
			base.OnMouseLeave( e );
			m_Hovered = false;
			Invalidate();
		}

		protected override void OnMouseDown( MouseEventArgs e )
		{
			base.OnMouseDown( e );
			if( e.Button == MouseButtons.Left ) {
				m_Pressed = true;
				Invalidate();
			}
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			base.OnMouseUp( e );
			m_Pressed = false;
			Invalidate();
		}

		protected override void OnMouseCaptureChanged( EventArgs e )
		{
			base.OnMouseCaptureChanged( e );
			m_Pressed = false;
			Invalidate();
		}

		protected override void OnKeyDown( KeyEventArgs e )
		{
			base.OnKeyDown( e );
			if( e.KeyCode == Keys.Space ) {
				m_Pressed = true;
				Invalidate();
			}
		}

		protected override void OnKeyUp( KeyEventArgs e )
		{
			base.OnKeyUp( e );
			m_Pressed = false;
			Invalidate();
		}

		protected override void OnLostFocus( EventArgs e )
		{
			base.OnLostFocus( e );
			m_Pressed = false;
			Invalidate();
		}

		protected override void OnEnabledChanged( EventArgs e )
		{
			base.OnEnabledChanged( e );
			m_Pressed = false;
			m_Hovered = false;
			Invalidate();
		}

		int m_CornerRadius = 5;
		ButtonIndicator m_Indicator;
		bool m_Active;
		bool m_Hovered;
		bool m_Pressed;
	}
}
