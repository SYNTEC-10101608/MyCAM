namespace CncUI
{
	partial class CNCAlarmPanel
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) ) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.m_secAlarm = new CommonUI.Controls.SectionPanel();
			this.m_lblStatus = new CommonUI.Controls.ThemedLabel();
			this.m_gridAlarms = new CommonUI.Controls.ThemedDataGridView();
			this.m_colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_colText = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_ToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.m_secAlarm.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_gridAlarms)).BeginInit();
			this.SuspendLayout();
			//
			// m_secAlarm
			//
			this.m_secAlarm.Controls.Add(this.m_lblStatus);
			this.m_secAlarm.Controls.Add(this.m_gridAlarms);
			this.m_secAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_secAlarm.Location = new System.Drawing.Point(0, 0);
			this.m_secAlarm.Name = "m_secAlarm";
			this.m_secAlarm.Size = new System.Drawing.Size(380, 180);
			this.m_secAlarm.TabIndex = 0;
			this.m_secAlarm.Text = "當前警報（0）";
			//
			// m_lblStatus
			//
			this.m_lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblStatus.AutoEllipsis = true;
			this.m_lblStatus.Location = new System.Drawing.Point(10, 36);
			this.m_lblStatus.Name = "m_lblStatus";
			this.m_lblStatus.Role = CommonUI.Theme.TextRole.Warning;
			this.m_lblStatus.Size = new System.Drawing.Size(360, 19);
			this.m_lblStatus.TabIndex = 0;
			this.m_lblStatus.Visible = false;
			//
			// m_gridAlarms
			//
			this.m_gridAlarms.AllowUserToAddRows = false;
			this.m_gridAlarms.AllowUserToDeleteRows = false;
			this.m_gridAlarms.AllowUserToResizeRows = false;
			this.m_gridAlarms.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.m_gridAlarms.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
			this.m_gridAlarms.CellRole = CommonUI.Theme.TextRole.Error;
			this.m_gridAlarms.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.m_colCode,
            this.m_colText});
			this.m_gridAlarms.Location = new System.Drawing.Point(10, 36);
			this.m_gridAlarms.Name = "m_gridAlarms";
			this.m_gridAlarms.ReadOnly = true;
			this.m_gridAlarms.RowHeadersVisible = false;
			this.m_gridAlarms.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.m_gridAlarms.Size = new System.Drawing.Size(360, 134);
			this.m_gridAlarms.TabIndex = 1;
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
			this.Controls.Add(this.m_secAlarm);
			this.Name = "CNCAlarmPanel";
			this.Size = new System.Drawing.Size(380, 180);
			this.m_secAlarm.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_gridAlarms)).EndInit();
			this.ResumeLayout(false);

		}

		private CommonUI.Controls.SectionPanel m_secAlarm;
		private CommonUI.Controls.ThemedLabel m_lblStatus;
		private CommonUI.Controls.ThemedDataGridView m_gridAlarms;
		private System.Windows.Forms.DataGridViewTextBoxColumn m_colCode;
		private System.Windows.Forms.DataGridViewTextBoxColumn m_colText;
		private System.Windows.Forms.ToolTip m_ToolTip;
	}
}
