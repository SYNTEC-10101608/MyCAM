using System.Windows.Forms;
using MyCAM.LogManager;

namespace MyCAM.App
{
	internal static class MyApp
	{
		public static Form MainForm
		{
			get => m_MainForm;
			set
			{
				m_MainForm = value;

				if( value is StartupForm f ) {
					LogPanel = f.GetLogPanel;
					Logger = new LogHandler( LogPanel );
				}
			}
		}

		public static LogHandler Logger
		{
			get; private set;
		}

		static Panel LogPanel
		{
			get; set;
		}

		static Form m_MainForm;
	}
}