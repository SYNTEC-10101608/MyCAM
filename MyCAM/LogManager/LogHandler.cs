using System;
using System.Drawing;
using System.Windows.Forms;
using MyCAM.App;

namespace MyCAM.LogManager
{
	internal class LogHandler
	{
		public LogHandler( Panel panelControl )
		{
			m_PanelShowMessage = panelControl;
		}

		public void ShowOnLogPanel( string message, MyApp.NoticeType type )
		{
			switch( type ) {
				case MyApp.NoticeType.Error:
					ShowError( message );
					break;

				case MyApp.NoticeType.Warning:
					ShowWarning( message );
					break;

				case MyApp.NoticeType.Hint:
				default:
					AddLog( message, Color.Black );
					break;
			}
		}

		Panel m_PanelShowMessage;

		void ShowError( string message )
		{
			MessageBox.Show(
				message,
				"Error",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error
			);

			// sync to log panel
			AddLog( message, Color.Red );
		}

		void ShowWarning( string message )
		{
			MessageBox.Show(
				message,
				"Warning",
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning
			);
			AddLog( message, Color.OrangeRed );
		}

		void AddLog( string message, Color color )
		{

			Label lblStickOnPanel = new Label
			{
				AutoSize = true,
				ForeColor = color,
				Text = $"{DateTime.Now:HH:mm:ss} - {message}",
				Dock = DockStyle.Top
			};

			m_PanelShowMessage.Controls.Add( lblStickOnPanel );

			// put on the top
			m_PanelShowMessage.Controls.SetChildIndex( lblStickOnPanel, 0 );
		}
	}
}
