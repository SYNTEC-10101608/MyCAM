namespace MyCAM.Editor
{
	partial class EntryAndExitDlg
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) ) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.m_pnlDistance = new CommonUI.Controls.SectionPanel();
			this.m_lblEntryDistance = new System.Windows.Forms.Label();
			this.m_numEntryDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblEntryDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblExitDistance = new System.Windows.Forms.Label();
			this.m_numExitDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblExitDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblFollowSafeDistance = new System.Windows.Forms.Label();
			this.m_numFollowSafeDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblFollowSafeDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_btnConfirm = new CommonUI.Controls.ThemedButton();
			this.m_pnlDistance.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numEntryDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numExitDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numFollowSafeDistance)).BeginInit();
			this.SuspendLayout();
			//
			// m_pnlDistance
			//
			this.m_pnlDistance.Controls.Add(this.m_lblEntryDistance);
			this.m_pnlDistance.Controls.Add(this.m_numEntryDistance);
			this.m_pnlDistance.Controls.Add(this.m_lblEntryDistanceUnit);
			this.m_pnlDistance.Controls.Add(this.m_lblExitDistance);
			this.m_pnlDistance.Controls.Add(this.m_numExitDistance);
			this.m_pnlDistance.Controls.Add(this.m_lblExitDistanceUnit);
			this.m_pnlDistance.Controls.Add(this.m_lblFollowSafeDistance);
			this.m_pnlDistance.Controls.Add(this.m_numFollowSafeDistance);
			this.m_pnlDistance.Controls.Add(this.m_lblFollowSafeDistanceUnit);
			this.m_pnlDistance.Location = new System.Drawing.Point(12, 12);
			this.m_pnlDistance.Name = "m_pnlDistance";
			this.m_pnlDistance.Size = new System.Drawing.Size(296, 103);
			this.m_pnlDistance.TabIndex = 0;
			//
			// m_lblEntryDistance
			//
			this.m_lblEntryDistance.AutoSize = true;
			this.m_lblEntryDistance.Location = new System.Drawing.Point(10, 14);
			this.m_lblEntryDistance.Name = "m_lblEntryDistance";
			this.m_lblEntryDistance.Size = new System.Drawing.Size(55, 16);
			this.m_lblEntryDistance.TabIndex = 0;
			this.m_lblEntryDistance.Text = "進入距離";
			//
			// m_numEntryDistance
			//
			this.m_numEntryDistance.DecimalPlaces = 3;
			this.m_numEntryDistance.Location = new System.Drawing.Point(110, 10);
			this.m_numEntryDistance.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_numEntryDistance.Name = "m_numEntryDistance";
			this.m_numEntryDistance.Size = new System.Drawing.Size(150, 23);
			this.m_numEntryDistance.TabIndex = 1;
			this.m_numEntryDistance.DebouncedValueChanged += new System.EventHandler(this.m_numEntryDistance_DebouncedValueChanged);
			//
			// m_lblEntryDistanceUnit
			//
			this.m_lblEntryDistanceUnit.AutoSize = true;
			this.m_lblEntryDistanceUnit.Location = new System.Drawing.Point(266, 14);
			this.m_lblEntryDistanceUnit.Name = "m_lblEntryDistanceUnit";
			this.m_lblEntryDistanceUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblEntryDistanceUnit.Size = new System.Drawing.Size(20, 16);
			this.m_lblEntryDistanceUnit.TabIndex = 2;
			this.m_lblEntryDistanceUnit.Text = "mm";
			//
			// m_lblExitDistance
			//
			this.m_lblExitDistance.AutoSize = true;
			this.m_lblExitDistance.Location = new System.Drawing.Point(10, 44);
			this.m_lblExitDistance.Name = "m_lblExitDistance";
			this.m_lblExitDistance.Size = new System.Drawing.Size(55, 16);
			this.m_lblExitDistance.TabIndex = 3;
			this.m_lblExitDistance.Text = "退出距離";
			//
			// m_numExitDistance
			//
			this.m_numExitDistance.DecimalPlaces = 3;
			this.m_numExitDistance.Location = new System.Drawing.Point(110, 40);
			this.m_numExitDistance.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_numExitDistance.Name = "m_numExitDistance";
			this.m_numExitDistance.Size = new System.Drawing.Size(150, 23);
			this.m_numExitDistance.TabIndex = 4;
			this.m_numExitDistance.DebouncedValueChanged += new System.EventHandler(this.m_numExitDistance_DebouncedValueChanged);
			//
			// m_lblExitDistanceUnit
			//
			this.m_lblExitDistanceUnit.AutoSize = true;
			this.m_lblExitDistanceUnit.Location = new System.Drawing.Point(266, 44);
			this.m_lblExitDistanceUnit.Name = "m_lblExitDistanceUnit";
			this.m_lblExitDistanceUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblExitDistanceUnit.Size = new System.Drawing.Size(20, 16);
			this.m_lblExitDistanceUnit.TabIndex = 5;
			this.m_lblExitDistanceUnit.Text = "mm";
			//
			// m_lblFollowSafeDistance
			//
			this.m_lblFollowSafeDistance.AutoSize = true;
			this.m_lblFollowSafeDistance.Location = new System.Drawing.Point(10, 74);
			this.m_lblFollowSafeDistance.Name = "m_lblFollowSafeDistance";
			this.m_lblFollowSafeDistance.Size = new System.Drawing.Size(79, 16);
			this.m_lblFollowSafeDistance.TabIndex = 6;
			this.m_lblFollowSafeDistance.Text = "隨動安全距離";
			//
			// m_numFollowSafeDistance
			//
			this.m_numFollowSafeDistance.DecimalPlaces = 3;
			this.m_numFollowSafeDistance.Location = new System.Drawing.Point(110, 70);
			this.m_numFollowSafeDistance.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_numFollowSafeDistance.Name = "m_numFollowSafeDistance";
			this.m_numFollowSafeDistance.Size = new System.Drawing.Size(150, 23);
			this.m_numFollowSafeDistance.TabIndex = 7;
			this.m_numFollowSafeDistance.DebouncedValueChanged += new System.EventHandler(this.m_numFollowSafeDistance_DebouncedValueChanged);
			//
			// m_lblFollowSafeDistanceUnit
			//
			this.m_lblFollowSafeDistanceUnit.AutoSize = true;
			this.m_lblFollowSafeDistanceUnit.Location = new System.Drawing.Point(266, 74);
			this.m_lblFollowSafeDistanceUnit.Name = "m_lblFollowSafeDistanceUnit";
			this.m_lblFollowSafeDistanceUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblFollowSafeDistanceUnit.Size = new System.Drawing.Size(20, 16);
			this.m_lblFollowSafeDistanceUnit.TabIndex = 8;
			this.m_lblFollowSafeDistanceUnit.Text = "mm";
			//
			// m_btnConfirm
			//
			this.m_btnConfirm.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnConfirm.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnConfirm.Location = new System.Drawing.Point(228, 131);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(80, 28);
			this.m_btnConfirm.TabIndex = 1;
			this.m_btnConfirm.Text = "確定";
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			//
			// EntryAndExitDlg
			//
			this.ClientSize = new System.Drawing.Size(320, 171);
			this.Controls.Add(this.m_pnlDistance);
			this.Controls.Add(this.m_btnConfirm);
			this.Name = "EntryAndExitDlg";
			this.Text = "進入/退出";
			this.m_pnlDistance.ResumeLayout(false);
			this.m_pnlDistance.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numEntryDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numExitDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numFollowSafeDistance)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlDistance;
		private System.Windows.Forms.Label m_lblEntryDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_numEntryDistance;
		private CommonUI.Controls.ThemedLabel m_lblEntryDistanceUnit;
		private System.Windows.Forms.Label m_lblExitDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_numExitDistance;
		private CommonUI.Controls.ThemedLabel m_lblExitDistanceUnit;
		private System.Windows.Forms.Label m_lblFollowSafeDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_numFollowSafeDistance;
		private CommonUI.Controls.ThemedLabel m_lblFollowSafeDistanceUnit;
		private CommonUI.Controls.ThemedButton m_btnConfirm;
	}
}
