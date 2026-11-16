using System;
using System.Windows.Forms;
using MyCAM.App;

namespace MyCAM
{
	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );

			// setting, UI language and theme fonts must be ready before the main window is constructed
			MyApp.Initialize();
			Application.Run( new StartupForm() );
		}
	}
}
