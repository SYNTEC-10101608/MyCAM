namespace CommonUI.Controls
{
	partial class LoadingBar
	{
		internal System.Windows.Forms.Label m_TopicLabel;
		internal System.Windows.Forms.Label m_StageLabel;
		internal RoundedProgressBar m_ProgressBar;

		private void InitializeComponent()
		{
			this.m_TopicLabel = new System.Windows.Forms.Label();
			this.m_StageLabel = new System.Windows.Forms.Label();
			this.m_ProgressBar = new RoundedProgressBar();
			this.SuspendLayout();

			this.m_TopicLabel.AutoSize = false;
			this.m_TopicLabel.Location = new System.Drawing.Point( 12, 12 );
			this.m_TopicLabel.Name = "m_TopicLabel";
			this.m_TopicLabel.Size = new System.Drawing.Size( 456, 20 );

			this.m_StageLabel.AutoSize = false;
			this.m_StageLabel.Location = new System.Drawing.Point( 12, 40 );
			this.m_StageLabel.Name = "m_StageLabel";
			this.m_StageLabel.Size = new System.Drawing.Size( 456, 44 );

			this.m_ProgressBar.Location = new System.Drawing.Point( 12, 98 );
			this.m_ProgressBar.Maximum = 100;
			this.m_ProgressBar.Minimum = 0;
			this.m_ProgressBar.Name = "m_ProgressBar";
			this.m_ProgressBar.Size = new System.Drawing.Size( 456, 23 );

			this.Controls.Add( this.m_TopicLabel );
			this.Controls.Add( this.m_StageLabel );
			this.Controls.Add( this.m_ProgressBar );
			this.Name = "LoadingBar";
			this.Size = new System.Drawing.Size( 480, 140 );
			this.ResumeLayout( false );
		}
	}
}
