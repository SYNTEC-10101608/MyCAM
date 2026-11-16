using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CommonUI.Forms;
using CommonUI.Theme;

namespace CommonUI.Controls
{
	/// <summary>
	/// The only button of the UI. Kind selects the look; CNC buttons also use Indicator and Active.
	/// Colors and font come from the theme and are not written to the designer file.
	/// </summary>
	public class ThemedButton : Button
	{
		public ThemedButton()
		{
			SetStyle( ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true );
			base.FlatStyle = FlatStyle.Flat;
			base.UseVisualStyleBackColor = false;
			base.BackColor = ThemeColors.SURFACE;
		}

		[Category( "Appearance" )]
		[DefaultValue( ButtonKind.Normal )]
		public ButtonKind Kind
		{
			get => m_Kind;
			set
			{
				m_Kind = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Shared text (OK, Cancel ...) taken from the common language file at runtime.
		/// </summary>
		[Category( "Appearance" )]
		[DefaultValue( CommonButtonText.None )]
		public CommonButtonText CommonText
		{
			get => m_CommonText;
			set
			{
				m_CommonText = value;
				if( value != CommonButtonText.None ) {
					Text = CommonTexts.GetDefault( value );
				}
			}
		}

		[Category( "Appearance" )]
		[DefaultValue( ButtonIndicator.None )]
		public ButtonIndicator Indicator
		{
			get => m_Indicator;
			set
			{
				m_Indicator = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Selected state of a toggle-like button (e.g. the current CNC mode).
		/// </summary>
		[Category( "Appearance" )]
		[DefaultValue( false )]
		public bool Active
		{
			get => m_isActive;
			set
			{
				if( m_isActive == value ) {
					return;
				}
				m_isActive = value;
				Invalidate();
			}
		}

		#region Appearance fixed by the theme

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
		public new FlatStyle FlatStyle
		{
			get => base.FlatStyle;
			set => base.FlatStyle = value;
		}

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public new bool UseVisualStyleBackColor
		{
			get => base.UseVisualStyleBackColor;
			set => base.UseVisualStyleBackColor = value;
		}

		#endregion

		protected override void OnPaint( PaintEventArgs e )
		{
			if( ClientSize.Width < 2 || ClientSize.Height < 2 ) {
				return;
			}
			Graphics graphics = e.Graphics;
			graphics.Clear( Parent == null ? ThemeColors.WINDOW : Parent.BackColor );
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			GetColors( out Color fill, out Color border, out Color text );
			RectangleF bounds = new RectangleF( 0.5F, 0.5F, Width - 1F, Height - 1F );
			using( GraphicsPath path = RoundDrawing.CreatePath( bounds, ThemeMetrics.BUTTON_CORNER_RADIUS ) )
			using( SolidBrush brush = new SolidBrush( fill ) )
			using( Pen pen = new Pen( border ) ) {
				graphics.FillPath( brush, path );
				graphics.DrawPath( pen, path );
			}
			graphics.SmoothingMode = SmoothingMode.None;
			Rectangle content = Rectangle.Inflate( ClientRectangle, -CONTENT_PADDING, -2 );
			content = DrawIndicator( graphics, content );
			content = DrawImage( graphics, content );
			TextRenderer.DrawText( graphics, Text, Font, content, text, GetTextFlags() );
			if( Focused && ShowFocusCues ) {
				ControlPaint.DrawFocusRectangle( graphics, Rectangle.Inflate( ClientRectangle, -3, -3 ), text, fill );
			}
		}

		void GetColors( out Color fill, out Color border, out Color text )
		{
			// disabled looks the same for every kind
			if( Enabled == false ) {
				fill = ThemeColors.SURFACE_ALT;
				border = ThemeColors.DIVIDER;
				text = ThemeColors.TEXT_DISABLED;
				return;
			}

			// primary buttons and active toggles are filled with the accent
			bool isAccent = m_Kind == ButtonKind.Primary || m_isActive;
			if( isAccent ) {
				fill = m_isPressed ? ThemeColors.ACCENT_PRESSED : m_isHovered ? ThemeColors.ACCENT_HOVER : ThemeColors.ACCENT;
				border = fill;
				text = ThemeColors.TEXT_ON_ACCENT;
				return;
			}

			// CNC machining buttons are filled with the status color of their severity
			bool isMachining = m_Kind == ButtonKind.Start || m_Kind == ButtonKind.Hold || m_Kind == ButtonKind.Stop;
			if( isMachining ) {
				fill = GetMachiningFill();
				border = fill;
				text = ThemeColors.TEXT_ON_ACCENT;
				return;
			}
			fill = m_isPressed ? ThemeColors.PRESSED : m_isHovered ? ThemeColors.HOVER : ThemeColors.SURFACE;
			if( m_Kind == ButtonKind.Danger ) {
				border = ThemeColors.ERROR;
				text = ThemeColors.ERROR;
			}
			else {
				border = ThemeColors.BORDER_STRONG;
				text = ThemeColors.TEXT;
			}
		}

		Color GetMachiningFill()
		{
			switch( m_Kind ) {
				case ButtonKind.Start:
					return m_isPressed ? ThemeColors.SUCCESS_PRESSED : m_isHovered ? ThemeColors.SUCCESS_HOVER : ThemeColors.SUCCESS;
				case ButtonKind.Hold:
					return m_isPressed ? ThemeColors.WARNING_PRESSED : m_isHovered ? ThemeColors.WARNING_HOVER : ThemeColors.WARNING;
				case ButtonKind.Stop:
					return m_isPressed ? ThemeColors.ERROR_PRESSED : m_isHovered ? ThemeColors.ERROR_HOVER : ThemeColors.ERROR;
				default:
					throw new InvalidOperationException( "not a machining button kind: " + m_Kind );
			}
		}

		Rectangle DrawIndicator( Graphics graphics, Rectangle content )
		{
			// no lamp
			if( m_Indicator == ButtonIndicator.None ) {
				return content;
			}
			Rectangle lamp = new Rectangle( Width - LAMP_SIZE - CONTENT_PADDING, ( Height - LAMP_SIZE ) / 2,
				LAMP_SIZE, LAMP_SIZE );
			Color lampColor = ThemeColors.BORDER_STRONG;
			if( Enabled && m_Indicator == ButtonIndicator.On ) {
				lampColor = ThemeColors.SUCCESS;
			}
			else if( Enabled && m_Indicator == ButtonIndicator.Off ) {
				lampColor = ThemeColors.TEXT_SECONDARY;
			}
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using( SolidBrush brush = new SolidBrush( lampColor ) ) {
				graphics.FillEllipse( brush, lamp );
			}
			graphics.SmoothingMode = SmoothingMode.None;
			content.Width = Math.Max( 0, lamp.Left - content.Left - 2 );
			return content;
		}

		Rectangle DrawImage( Graphics graphics, Rectangle content )
		{
			// text only
			if( Image == null ) {
				return content;
			}

			// image only: centered
			if( string.IsNullOrEmpty( Text ) ) {
				graphics.DrawImage( Image, content.Left + ( content.Width - Image.Width ) / 2,
					( Height - Image.Height ) / 2, Image.Width, Image.Height );
				return content;
			}
			graphics.DrawImage( Image, content.Left, ( Height - Image.Height ) / 2, Image.Width, Image.Height );
			int nOffset = Image.Width + ThemeMetrics.CONTROL_GAP;
			content.X += nOffset;
			content.Width = Math.Max( 0, content.Width - nOffset );
			return content;
		}

		TextFormatFlags GetTextFlags()
		{
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
			if( UseMnemonic == false ) {
				flags |= TextFormatFlags.NoPrefix;
			}
			else if( ShowKeyboardCues == false ) {
				flags |= TextFormatFlags.HidePrefix;
			}
			return flags;
		}

		#region Hover and pressed states

		protected override void OnMouseEnter( EventArgs e )
		{
			base.OnMouseEnter( e );
			SetHovered( true );
		}

		protected override void OnMouseLeave( EventArgs e )
		{
			base.OnMouseLeave( e );
			SetHovered( false );
		}

		protected override void OnMouseDown( MouseEventArgs e )
		{
			base.OnMouseDown( e );
			if( e.Button == MouseButtons.Left ) {
				SetPressed( true );
			}
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			base.OnMouseUp( e );
			SetPressed( false );
		}

		protected override void OnMouseCaptureChanged( EventArgs e )
		{
			base.OnMouseCaptureChanged( e );
			SetPressed( false );
		}

		protected override void OnKeyDown( KeyEventArgs e )
		{
			base.OnKeyDown( e );
			if( e.KeyCode == Keys.Space ) {
				SetPressed( true );
			}
		}

		protected override void OnKeyUp( KeyEventArgs e )
		{
			base.OnKeyUp( e );
			SetPressed( false );
		}

		protected override void OnLostFocus( EventArgs e )
		{
			base.OnLostFocus( e );
			SetPressed( false );
		}

		protected override void OnEnabledChanged( EventArgs e )
		{
			base.OnEnabledChanged( e );
			m_isPressed = false;
			m_isHovered = false;
			Invalidate();
		}

		void SetHovered( bool isHovered )
		{
			m_isHovered = isHovered;
			Invalidate();
		}

		void SetPressed( bool isPressed )
		{
			m_isPressed = isPressed;
			Invalidate();
		}

		#endregion

		const int CONTENT_PADDING = 6;
		const int LAMP_SIZE = 6;

		ButtonKind m_Kind = ButtonKind.Normal;
		CommonButtonText m_CommonText = CommonButtonText.None;
		ButtonIndicator m_Indicator = ButtonIndicator.None;
		bool m_isActive;
		bool m_isHovered;
		bool m_isPressed;
	}
}
