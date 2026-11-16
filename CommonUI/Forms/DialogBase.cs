using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CommonUI.Theme;

namespace CommonUI.Forms
{
	/// <summary>
	/// Appearance layer of every dialog: frame, font, colors, no auto scaling, centering and language texts.
	/// It adds no behavior; behavior belongs to the derived bases (FormBase, EditDialogBase).
	/// The appearance properties are fixed here and hidden from the designer, so derived .Designer.cs files
	/// never contain them while the designer still shows them.
	/// </summary>
	public class DialogBase : Form
	{
		public DialogBase()
		{
			base.AutoScaleMode = AutoScaleMode.None;
			base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
			base.StartPosition = FormStartPosition.Manual;
			base.ShowInTaskbar = false;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.ShowIcon = false;
			base.Font = ThemeFonts.Body;
			base.BackColor = ThemeColors.WINDOW;
			base.ForeColor = ThemeColors.TEXT;
		}

		#region Appearance fixed by the base

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

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public new FormBorderStyle FormBorderStyle
		{
			get => base.FormBorderStyle;
			set => base.FormBorderStyle = value;
		}

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public new FormStartPosition StartPosition
		{
			get => base.StartPosition;
			set => base.StartPosition = value;
		}

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public new bool ShowInTaskbar
		{
			get => base.ShowInTaskbar;
			set => base.ShowInTaskbar = value;
		}

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public new bool MaximizeBox
		{
			get => base.MaximizeBox;
			set => base.MaximizeBox = value;
		}

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public new bool MinimizeBox
		{
			get => base.MinimizeBox;
			set => base.MinimizeBox = value;
		}

		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public new bool ShowIcon
		{
			get => base.ShowIcon;
			set => base.ShowIcon = value;
		}

		#endregion

		/// <summary>
		/// Language file name of this dialog; the class name by default.
		/// </summary>
		protected virtual string LanguageFileName => GetType().Name;

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			// center after Load handlers, which may change the size
			bool isDesignTime = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;
			if( isDesignTime == false ) {
				Location = DialogPlacement.GetCenterLocation( this );
			}
		}

		/// <summary>
		/// Apply the language texts: the dialog title by the class name, each caption control by its name.
		/// Every dialog calls it right after InitializeComponent(), the base constructor runs before the controls exist.
		/// </summary>
		protected void ApplyLanguage()
		{
			bool isDesignTime = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;
			if( isDesignTime ) {
				return;
			}
			LanguageManager.LanguageManager languageManager = new LanguageManager.LanguageManager( LanguageFileName );
			if( languageManager.HasString( LanguageFileName ) ) {
				Text = languageManager.GetString( LanguageFileName );
			}
			LanguageApplier.Apply( this, languageManager );
		}
	}
}
