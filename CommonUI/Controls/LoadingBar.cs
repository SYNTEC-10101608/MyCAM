using System;
using System.Windows.Forms;

namespace CommonUI.Controls
{
	public partial class LoadingBar : UserControl
	{
		public LoadingBar( string szTopic )
		{
			InitializeComponent();
			m_TopicLabel.Text = szTopic;
			m_ProgressBar.ForeColor = Theme.ThemeColors.ACCENT;
		}

		public void SetStage( string szStage, int nPercent )
		{
			m_StageLabel.Text = szStage;
			m_ProgressBar.Value = Math.Max( m_ProgressBar.Value, Math.Min( 100, nPercent ) );
		}
	}
}
