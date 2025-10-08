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
					AddLog( message, m_WarningColor );
					break;
				case MyApp.NoticeType.Hint:
				default:
					AddLog( message, m_NormalHintColor );
					break;
			}
		}

		Panel m_PanelShowMessage;
		Color m_ErrorColor = Color.FromArgb( 253, 99, 132 );
		Color m_WarningColor = Color.Yellow;
		Color m_NormalHintColor = Color.White;

		void ShowError( string message )
		{
			MessageBox.Show(
				message,
				"Error",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error
			);

			// sync to log panel
			AddLog( message, m_ErrorColor );
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

			// make sure message is on the top of panel
			m_PanelShowMessage.Controls.Add( lblStickOnPanel );
			m_PanelShowMessage.ScrollControlIntoView( lblStickOnPanel );
		}
	}
}
