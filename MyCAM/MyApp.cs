using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CommonData;
using CommonUI.Theme;
using FileTranslator;
using MyCAM.LogManager;
using MyCAM.SettingManager;
using VNCFormsApp;

namespace MyCAM.App
{
	internal static class MyApp
	{
		static string m_szCNCIP = string.Empty;
		static AppSettingManager m_AppSetting = null;

		const string CUSTOM_root_Folder_Name = "Custom";
		const string CUST_post_file_Name = "CustPost.cpf";

		const string LOG_CUST_POST_FILE_NOT_EXIST = "客製後處理檔案不存在";
		const string LOG_READ_CUST_POST_FILE_FAILED = "讀取客製化後處理器檔案失敗";
		const string LOG_CUST_POST_FILE_CONTENT_ERROR = "客製化後處理器檔案內容不完整或格式錯誤,請確認檔案內容。";

		public enum NoticeType
		{
			Hint,
			Warning,
			Error
		}

		public static string CNCIP
		{
			get
			{
				return m_szCNCIP;
			}
			set
			{
				m_szCNCIP = value;
			}
		}

		public static Form MainForm
		{
			get => m_MainForm;
			set
			{
				m_MainForm = value;

				if( value is StartupForm f ) {

					// get main form log panel
					m_LogPanel = f.GetLogPanel;

					// send panel to logger
					m_Logger = new LogHandler( m_LogPanel );
				}
			}
		}

		public static LogHandler Logger
		{
			get
			{
				if( m_Logger == null ) {
					throw new InvalidOperationException( m_LanguageManager.TryGetString( "LoggerNotInitialized", LOGGER_NOT_INITIALIZED ) );
				}
				return m_Logger;
			}
		}

		public static AppSettingManager AppSetting
		{
			get
			{
				if( m_AppSetting == null ) {
					throw new InvalidOperationException( "AppSetting is not initialized." );
				}
				return m_AppSetting;
			}
		}

		/// <summary>
		/// Start-up initialization. Call once in Program.Main before any screen is created.
		/// </summary>
		public static void Initialize()
		{
			// other modules depend on the app setting
			if( m_AppSetting == null ) {
				m_AppSetting = new AppSettingManager();
			}

			// apply saved UI language
			LanguageManager.LanguageManager.CurrentUILanguage = m_AppSetting.UILanguage;

			// the font family follows the UI language, and the main window base reads it in its constructor
			ThemeFonts.Initialize( LanguageManager.LanguageManager.CurrentUILanguage );
		}

		public static VNCUserControl VNCScreen
		{
			get => m_VNCUserControl;
			set
			{
				if( m_VNCUserControl != null ) {
					m_VNCUserControl.CloseVNCProcess();
					m_VNCUserControl.Dispose();
				}
				m_VNCUserControl = value;
			}
		}

		public static CustPost CustomizedPostInfo
		{
			get => m_CustPost;
		}

		#region Show Dialog at Form Center

		static void UnRegistDialogMoveWithMainForm( Form dialog )
		{
			if( m_MainForm == null || dialog == null ) {
				return;
			}

			// find registered event info
			DialogEventInfo eventInfo = m_RegisteredDialogs.Find( info => info.Dialog == dialog );
			if( eventInfo == null ) {
				return;
			}

			// unsubscribe event
			m_MainForm.LocationChanged -= eventInfo.LocationChangedHandler;
			m_MainForm.SizeChanged -= eventInfo.SizeChangedHandler;
			m_MainForm.Activated -= eventInfo.MainFormActivatedHandler;

			// remove from registered list
			m_RegisteredDialogs.Remove( eventInfo );
		}

		#endregion

		#region Read Customized Post File

		public static void SetCustPost()
		{
			// read cust post file
			string filePath = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, CUSTOM_root_Folder_Name, CUST_post_file_Name );
			if( !System.IO.File.Exists( filePath ) ) {
				Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CustPostFileNotExist", LOG_CUST_POST_FILE_NOT_EXIST ), NoticeType.Hint );

				// use default value
				m_CustPost = new CustPost();
				return;
			}
			string fileContent;

			// protect io exception
			try {
				fileContent = System.IO.File.ReadAllText( filePath );
			}
			catch( Exception ) {
				Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "ReadCustPostFileFailed", LOG_READ_CUST_POST_FILE_FAILED ), NoticeType.Error );

				// use default value
				m_CustPost = new CustPost();
				return;
			}

			// translate to DTO
			TxtToCustPostTranslator custPostTranslater = new TxtToCustPostTranslator();
			bool isGetAllCustContent = custPostTranslater.Translate( fileContent, out CustPost post );
			if( isGetAllCustContent == false ) {
				Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CustPostFileContentError", LOG_CUST_POST_FILE_CONTENT_ERROR ), NoticeType.Error, true );
			}

			// store info
			m_CustPost = post;
		}

		#endregion

		static Form m_MainForm;
		static LogHandler m_Logger;
		static Panel m_LogPanel;
		static VNCUserControl m_VNCUserControl;
		static readonly List<DialogEventInfo> m_RegisteredDialogs = new List<DialogEventInfo>();
		static CustPost m_CustPost = null;
		static readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "MyApp" );
		const string LOGGER_NOT_INITIALIZED = "Logger還沒初始化,請先設定 Logger。";

		class DialogEventInfo
		{
			public Form Dialog
			{
				get; set;
			}

			public EventHandler LocationChangedHandler
			{
				get; set;
			}

			public EventHandler SizeChangedHandler
			{
				get; set;
			}

			public EventHandler MainFormActivatedHandler
			{
				get; set;
			}
		}
	}
}
