using MyCAM.App;
using System;
using System.Drawing;
using System.Windows.Forms;

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
			// Get color and icon based on type
			Color color;
			MessageBoxIcon icon;
			bool forceMessageBox = false;

			switch( type ) {
				case MyApp.NoticeType.Error:
					color = ERROR_Color;
					icon = MessageBoxIcon.Error;
					forceMessageBox = true;
					break;
				case MyApp.NoticeType.Warning:
					color = WARNING_Color;
					icon = MessageBoxIcon.Warning;
					break;
				case MyApp.NoticeType.Hint:
				default:
					color = NORMAL_HintColor;
					icon = MessageBoxIcon.Information;
					break;
			}

			AddLog( message, color, icon, forceMessageBox || needMessageBox );
		}

		Panel m_PanelShowMessage;
		readonly Color ERROR_Color = Color.FromArgb( 253, 99, 132 );
		readonly Color WARNING_Color = Color.Yellow;
		readonly Color NORMAL_HintColor = Color.White;

		void AddLog( string message, Color color, MessageBoxIcon icon, bool needMessageBox )
		{
			// Add label to panel
			Label lblStickOnPanel = new Label
			{
				AutoSize = true,
				ForeColor = color,
				Text = $"{DateTime.Now:HH:mm:ss} - {message}",
				Dock = DockStyle.Top
			};

			m_PanelShowMessage.Controls.Add( lblStickOnPanel );
			m_PanelShowMessage.ScrollControlIntoView( lblStickOnPanel );

			// Show message box if needed
			if( needMessageBox ) {
				MessageBox.Show( message, "Message", MessageBoxButtons.OK, icon );
			}
		}
	}
}
