using System;
using CommonUI.Controls;

namespace CommonUI.Language
{
	/// <summary>
	/// Texts shared by all screens (Language/CommonUI_&lt;language&gt;.xml).
	/// </summary>
	public static class CommonTexts
	{
		/// <summary>
		/// Localized text of a common button.
		/// </summary>
		/// <param name="text">Common button text.</param>
		/// <returns>Text in the current UI language; the zh-TW default when the ID is missing.</returns>
		public static string Get( CommonButtonText text )
		{
			return m_LanguageManager.Value.TryGetString( text.ToString(), GetDefault( text ) );
		}

		/// <summary>
		/// zh-TW text shown in the designer.
		/// </summary>
		/// <param name="text">Common button text.</param>
		/// <returns>Default text.</returns>
		public static string GetDefault( CommonButtonText text )
		{
			switch( text ) {
				case CommonButtonText.None:
					return string.Empty;
				case CommonButtonText.Confirm:
					return CONFIRM_TEXT;
				case CommonButtonText.Cancel:
					return CANCEL_TEXT;
				case CommonButtonText.Reset:
					return RESET_TEXT;
				case CommonButtonText.Preview:
					return PREVIEW_TEXT;
				case CommonButtonText.Close:
					return CLOSE_TEXT;
				default:
					throw new ArgumentOutOfRangeException( nameof( text ) );
			}
		}

		const string LANGUAGE_FILE_NAME = "CommonUI";
		const string CONFIRM_TEXT = "確定";
		const string CANCEL_TEXT = "取消";
		const string RESET_TEXT = "重設";
		const string PREVIEW_TEXT = "預覽";
		const string CLOSE_TEXT = "關閉";

		// created on first use, after the UI language is set
		static readonly Lazy<LanguageManager.LanguageManager> m_LanguageManager =
			new Lazy<LanguageManager.LanguageManager>(
				() => new LanguageManager.LanguageManager( LANGUAGE_FILE_NAME ) );
	}
}
