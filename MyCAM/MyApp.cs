using MyCAM.LogManager;
using System;
using System.Windows.Forms;

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
					throw new InvalidOperationException( "Logger還沒初始化，請先設定 MainForm。" );
				}
				return m_Logger;
			}
		}

		static Form m_MainForm;
		static LogHandler m_Logger;
		static Panel m_LogPanel;
	}
}