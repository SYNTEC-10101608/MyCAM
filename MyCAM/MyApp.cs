using System;
using System.Windows.Forms;
using MyCAM.LogManager;

namespace MyCAM.App
{
	internal static class MyApp
	{
		public enum NoticeType
		{
			Hint,
			Warning,
			Error
		}

		public static Form MainForm
		{
			get => m_MainForm;
			set
			{
				m_MainForm = value;

				if( value is StartupForm f ) {
					m_LogPanel = f.GetLogPanel;
					m_Logger = new LogHandler( LogPanel );
				}
			}
		}

		public static LogHandler Logger
		{
			get
			{
				if( m_Logger == null ) {
					throw new InvalidOperationException( "Logger還沒初始化，請先設定 MainForm。" );
				}
				return m_Logger;
			}
		}

		public static Panel LogPanel
		{
			get
			{
				if( m_LogPanel == null ) {
					throw new InvalidOperationException( "LogPanel還沒初始化，請先設定 MainForm。" );
				}
				return m_LogPanel;
			}
		}

		static Form m_MainForm;
		static LogHandler m_Logger;
		static Panel m_LogPanel;
	}
}