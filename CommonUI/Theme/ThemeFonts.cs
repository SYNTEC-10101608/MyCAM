using System;
using System.Drawing;

namespace CommonUI.Theme
{
	/// <summary>
	/// Fonts of the text roles. The family follows the UI language; the designer always shows the zh-TW family.
	/// </summary>
	public static class ThemeFonts
	{
		/// <summary>
		/// Choose the font family by UI language. Call once at start-up, before any screen is created.
		/// </summary>
		/// <param name="szLanguage">UI language tag, e.g. "zh-TW".</param>
		public static void Initialize( string szLanguage )
		{
			string szFamily = GetFamilyName( szLanguage );
			if( szFamily == m_szFamily ) {
				return;
			}

			// fonts already handed out stay valid, so the old ones are not disposed
			m_szFamily = szFamily;
			CreateFonts();
		}

		public static Font Body => m_Body;

		public static Font Emphasis => m_Emphasis;

		public static Font Title => m_Title;

		public static Font Value => m_Value;

		/// <summary>
		/// Font of a text role.
		/// </summary>
		/// <param name="role">Text role.</param>
		/// <returns>The shared font; do not dispose.</returns>
		public static Font GetFont( TextRole role )
		{
			switch( role ) {
				case TextRole.Body:
				case TextRole.Secondary:
				case TextRole.Error:
				case TextRole.Warning:
				case TextRole.Success:
					return m_Body;
				case TextRole.Emphasis:
					return m_Emphasis;
				case TextRole.Title:
					return m_Title;
				case TextRole.Value:
					return m_Value;
				default:
					throw new ArgumentOutOfRangeException( nameof( role ) );
			}
		}

		static ThemeFonts()
		{
			CreateFonts();
		}

		static string GetFamilyName( string szLanguage )
		{
			if( string.Equals( szLanguage, LANGUAGE_ZH_TW, StringComparison.OrdinalIgnoreCase ) ) {
				return FAMILY_ZH_TW;
			}
			else if( string.Equals( szLanguage, LANGUAGE_ZH_CN, StringComparison.OrdinalIgnoreCase ) ) {
				return FAMILY_ZH_CN;
			}
			else {
				return FAMILY_OTHER;
			}
		}

		static void CreateFonts()
		{
			m_Body = new Font( m_szFamily, BODY_SIZE, FontStyle.Regular, GraphicsUnit.Point );
			m_Emphasis = new Font( m_szFamily, BODY_SIZE, FontStyle.Bold, GraphicsUnit.Point );
			m_Title = new Font( m_szFamily, TITLE_SIZE, FontStyle.Regular, GraphicsUnit.Point );
			m_Value = new Font( FAMILY_VALUE, VALUE_SIZE, FontStyle.Regular, GraphicsUnit.Point );
		}

		const string LANGUAGE_ZH_TW = "zh-TW";
		const string LANGUAGE_ZH_CN = "zh-CN";
		const string FAMILY_ZH_TW = "Microsoft JhengHei UI";
		const string FAMILY_ZH_CN = "Microsoft YaHei UI";
		const string FAMILY_OTHER = "Segoe UI";
		const string FAMILY_VALUE = "Consolas";
		const float BODY_SIZE = 9F;
		const float TITLE_SIZE = 10.5F;
		const float VALUE_SIZE = 10F;

		static string m_szFamily = FAMILY_ZH_TW;
		static Font m_Body;
		static Font m_Emphasis;
		static Font m_Title;
		static Font m_Value;
	}
}
