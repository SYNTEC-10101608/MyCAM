using System;
using System.Windows.Forms;
using CommonUI.Controls;
using CommonUI.Theme;
using MyCAM.App;

namespace MyCAM.LogManager
{
	internal class LogHandler
	{
		public LogHandler( Panel panelControl )
		{
			m_PanelShowMessage = panelControl;
		}

		public void ShowOnLogPanel( string message, MyApp.NoticeType type, bool needMessageBox = false )
		{
			// Get text role and icon based on type
			TextRole role;
			MessageBoxIcon icon;
			bool forceMessageBox = false;

			switch( type ) {
				case MyApp.NoticeType.Error:
					role = TextRole.Error;
					icon = MessageBoxIcon.Error;
					forceMessageBox = true;
					break;
				case MyApp.NoticeType.Warning:
					role = TextRole.Warning;
					icon = MessageBoxIcon.Warning;
					break;
				case MyApp.NoticeType.Hint:
				default:
					role = TextRole.Body;
					icon = MessageBoxIcon.Information;
					break;
			}

			AddLog( message, role, icon, forceMessageBox || needMessageBox );
		}

		Panel m_PanelShowMessage;

		void AddLog( string message, TextRole role, MessageBoxIcon icon, bool needMessageBox )
		{
			// Add label to panel
			ThemedLabel lblStickOnPanel = new ThemedLabel
			{
				AutoSize = true,
				Role = role,
				Text = $"{DateTime.Now:HH:mm:ss} - {message}",
				Dock = DockStyle.Top
			};

			m_PanelShowMessage.Controls.Add( lblStickOnPanel );
			m_PanelShowMessage.ScrollControlIntoView( lblStickOnPanel );

			// Show message box if needed
			if( needMessageBox ) {
				MessageBox.Show( MyApp.MainForm, message, "Message", MessageBoxButtons.OK, icon );
			}
		}
	}
}
