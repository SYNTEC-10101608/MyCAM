using System;
using System.Drawing;
using System.Windows.Forms;
using CommonUI.Controls;
using CommonUI.Forms;

namespace MyCAM.Editor
{
	sealed class LoadingBarDlg : DialogBase
	{
		readonly LoadingBar m_LoadingBar;
		bool m_AllowClose;

		public LoadingBarDlg( string szTitle, string szTopic, string szInitialStage )
		{
			Text = szTitle;
			ControlBox = false;
			ClientSize = new Size( 480, 140 );

			m_LoadingBar = new LoadingBar( szTopic ) { Dock = DockStyle.Fill };
			m_LoadingBar.SetStage( szInitialStage, 0 );
			Controls.Add( m_LoadingBar );
		}

		protected override void OnFormClosing( FormClosingEventArgs e )
		{
			base.OnFormClosing( e );
			if( !m_AllowClose && e.CloseReason == CloseReason.UserClosing ) {
				e.Cancel = true;
			}
		}

		protected override void SetVisibleCore( bool value )
		{
			base.SetVisibleCore( value );
			if( value ) {
				Refresh();
			}
		}

		public void CloseForCleanup()
		{
			m_AllowClose = true;
			Close();
		}

		public void SetStage( string szStage, int nPercent )
		{
			m_LoadingBar.SetStage( szStage, nPercent );
		}
	}
}
