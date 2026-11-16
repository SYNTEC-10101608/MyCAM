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
			this.m_pnlAlarm = new CommonUI.Controls.SectionPanel();
			this.m_lblStatus = new CommonUI.Controls.ThemedLabel();
			this.m_dgvAlarms = new CommonUI.Controls.ThemedDataGridView();
			this.m_colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_colText = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_ttpAlarm = new System.Windows.Forms.ToolTip(this.components);
			this.m_pnlAlarm.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dgvAlarms)).BeginInit();
			this.SuspendLayout();
			//
			// m_pnlAlarm
			//
			this.m_pnlAlarm.Controls.Add(this.m_lblStatus);
			this.m_pnlAlarm.Controls.Add(this.m_dgvAlarms);
			this.m_pnlAlarm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.m_pnlAlarm.Location = new System.Drawing.Point(8, 8);
			this.m_pnlAlarm.Name = "m_pnlAlarm";
			this.m_pnlAlarm.Size = new System.Drawing.Size(364, 164);
			this.m_pnlAlarm.TabIndex = 0;
			this.m_pnlAlarm.Text = "當前警報（0）";
			//
			// m_lblStatus
			//
			this.m_lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblStatus.AutoEllipsis = true;
			this.m_lblStatus.Location = new System.Drawing.Point(10, 36);
			this.m_lblStatus.Name = "m_lblStatus";
			this.m_lblStatus.Role = CommonUI.Theme.TextRole.Warning;
			this.m_lblStatus.Size = new System.Drawing.Size(344, 19);
			this.m_lblStatus.TabIndex = 0;
			this.m_lblStatus.Visible = false;
			//
			// m_dgvAlarms
			//
			this.m_dgvAlarms.AllowUserToAddRows = false;
			this.m_dgvAlarms.AllowUserToDeleteRows = false;
			this.m_dgvAlarms.AllowUserToResizeRows = false;
			this.m_dgvAlarms.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.m_dgvAlarms.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
			this.m_dgvAlarms.CellRole = CommonUI.Theme.TextRole.Error;
			this.m_dgvAlarms.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.m_colCode,
            this.m_colText});
			this.m_dgvAlarms.Location = new System.Drawing.Point(10, 36);
			this.m_dgvAlarms.Name = "m_dgvAlarms";
			this.m_dgvAlarms.ReadOnly = true;
			this.m_dgvAlarms.RowHeadersVisible = false;
			this.m_dgvAlarms.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.m_dgvAlarms.Size = new System.Drawing.Size(344, 118);
			this.m_dgvAlarms.TabIndex = 1;
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
			this.Controls.Add(this.m_pnlAlarm);
			this.Name = "CNCAlarmPanel";
			this.Size = new System.Drawing.Size(380, 180);
			this.m_pnlAlarm.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_dgvAlarms)).EndInit();
			this.ResumeLayout(false);

		}

		private CommonUI.Controls.SectionPanel m_pnlAlarm;
		private CommonUI.Controls.ThemedLabel m_lblStatus;
		private CommonUI.Controls.ThemedDataGridView m_dgvAlarms;
		private System.Windows.Forms.DataGridViewTextBoxColumn m_colCode;
		private System.Windows.Forms.DataGridViewTextBoxColumn m_colText;
		private System.Windows.Forms.ToolTip m_ttpAlarm;
	}
}
