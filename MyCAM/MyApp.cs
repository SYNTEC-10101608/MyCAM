using System;
using System.Windows.Forms;

namespace MyCAM.App
{
	internal static class MyApp
	{
		public static Form MainForm
		{
			get; set;
		}

		public static event Action<string> NoticeHandler;

		public static void Notify( string message )
		{
			if( NoticeHandler != null )
				NoticeHandler.Invoke( message );
			else
				MessageBox.Show( message, "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information );
		}


		public static void Notify( Exception ex )
		{
			string msg = $"Error: {ex.Message}\n\n{ex.StackTrace}";
			Notify( msg );
		}
	}
}
