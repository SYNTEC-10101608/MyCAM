using System;
using System.Windows.Forms;
using CommonUI.Theme;

namespace CommonUI.Controls
{
	/// <summary>
	/// Progress display with a topic and a stage text; shown by LoadingBarDlg.
	/// </summary>
	public partial class LoadingBar : UserControl
	{
		/// <summary>
		/// Create the progress display.
		/// </summary>
		/// <param name="szTopic">Topic shown above the progress bar.</param>
		public LoadingBar( string szTopic )
		{
			InitializeComponent();
			m_TopicLabel.Text = szTopic;
			m_ProgressBar.ForeColor = ThemeColors.ACCENT;
		}

		/// <summary>
		/// Show the current stage and progress.
		/// </summary>
		/// <param name="szStage">Stage text.</param>
		/// <param name="nPercent">Progress from 0 to 100.</param>
		public void SetStage( string szStage, int nPercent )
		{
			m_StageLabel.Text = szStage;
			m_ProgressBar.Value = Math.Max( m_ProgressBar.Value, Math.Min( 100, nPercent ) );
		}
	}
}
