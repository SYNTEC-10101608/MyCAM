namespace CncUI
{
	partial class CNCAlarmPanel
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose( bool disposing )
		{
			if( disposing && components != null ) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.m_lblTitle = new System.Windows.Forms.Label();
			this.m_lblStatus = new System.Windows.Forms.Label();
			this.m_gridAlarms = new System.Windows.Forms.DataGridView();
			this.m_colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_colText = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_ToolTip = new System.Windows.Forms.ToolTip( this.components );
			( (System.ComponentModel.ISupportInitialize)( this.m_gridAlarms ) ).BeginInit();
			this.SuspendLayout();
			//
			// m_lblTitle
			//
			this.m_lblTitle.Font = new System.Drawing.Font( "Microsoft JhengHei", 10F, System.Drawing.FontStyle.Bold );
			this.m_lblTitle.ForeColor = System.Drawing.Color.Firebrick;
			this.m_lblTitle.Location = new System.Drawing.Point( 8, 4 );
			this.m_lblTitle.Name = "m_lblTitle";
			this.m_lblTitle.Size = new System.Drawing.Size( 364, 24 );
			this.m_lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.m_lblTitle.Text = "當前警報（0）";
			//
			// m_lblStatus
			//
			this.m_lblStatus.AutoEllipsis = true;
			this.m_lblStatus.ForeColor = System.Drawing.Color.DarkGoldenrod;
			this.m_lblStatus.Location = new System.Drawing.Point( 8, 30 );
			this.m_lblStatus.Name = "m_lblStatus";
			this.m_lblStatus.Size = new System.Drawing.Size( 364, 22 );
			this.m_lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.m_lblStatus.Visible = false;
			//
			// m_gridAlarms
			//
			this.m_gridAlarms.AllowUserToAddRows = false;
			this.m_gridAlarms.AllowUserToDeleteRows = false;
			this.m_gridAlarms.AllowUserToResizeRows = false;
			this.m_gridAlarms.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.m_gridAlarms.BackgroundColor = System.Drawing.Color.White;
			this.m_gridAlarms.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_gridAlarms.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
			this.m_gridAlarms.Columns.AddRange( new System.Windows.Forms.DataGridViewColumn[] { this.m_colCode, this.m_colText } );
			this.m_gridAlarms.DefaultCellStyle.ForeColor = System.Drawing.Color.Firebrick;
			this.m_gridAlarms.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.m_gridAlarms.Location = new System.Drawing.Point( 8, 30 );
			this.m_gridAlarms.Name = "m_gridAlarms";
			this.m_gridAlarms.ReadOnly = true;
			this.m_gridAlarms.RowHeadersVisible = false;
			this.m_gridAlarms.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.m_gridAlarms.Size = new System.Drawing.Size( 364, 142 );
			this.m_gridAlarms.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			//
			// m_colCode
			//
			this.m_colCode.HeaderText = "代碼";
			this.m_colCode.Name = "m_colCode";
			this.m_colCode.ReadOnly = true;
			this.m_colCode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.m_colCode.Width = 128;
			//
			// m_colText
			//
			this.m_colText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.m_colText.HeaderText = "警報內容";
			this.m_colText.Name = "m_colText";
			this.m_colText.ReadOnly = true;
			this.m_colText.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			//
			// CNCAlarmPanel
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add( this.m_lblTitle );
			this.Controls.Add( this.m_lblStatus );
			this.Controls.Add( this.m_gridAlarms );
			this.Font = new System.Drawing.Font( "Microsoft JhengHei", 9F );
			this.Name = "CNCAlarmPanel";
			this.Size = new System.Drawing.Size( 380, 180 );
			( (System.ComponentModel.ISupportInitialize)( this.m_gridAlarms ) ).EndInit();
			this.ResumeLayout( false );
		}

		private System.Windows.Forms.Label m_lblTitle;
		private System.Windows.Forms.Label m_lblStatus;
		private System.Windows.Forms.DataGridView m_gridAlarms;
		private System.Windows.Forms.DataGridViewTextBoxColumn m_colCode;
		private System.Windows.Forms.DataGridViewTextBoxColumn m_colText;
		private System.Windows.Forms.ToolTip m_ToolTip;
	}
}
