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
			this.m_secDistance = new CommonUI.Controls.SectionPanel();
			this.m_lblEntryDistance = new System.Windows.Forms.Label();
			this.m_txbEntryDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblEntryDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblExitDistance = new System.Windows.Forms.Label();
			this.m_txbExitDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblExitDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblFollowSafeDistance = new System.Windows.Forms.Label();
			this.m_txbFollowSafeDistance = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblFollowSafeDistanceUnit = new CommonUI.Controls.ThemedLabel();
			this.m_btnConfirm = new CommonUI.Controls.ThemedButton();
			this.m_secDistance.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbEntryDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbExitDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbFollowSafeDistance)).BeginInit();
			this.SuspendLayout();
			//
			// m_secDistance
			//
			this.m_secDistance.Controls.Add(this.m_lblEntryDistance);
			this.m_secDistance.Controls.Add(this.m_txbEntryDistance);
			this.m_secDistance.Controls.Add(this.m_lblEntryDistanceUnit);
			this.m_secDistance.Controls.Add(this.m_lblExitDistance);
			this.m_secDistance.Controls.Add(this.m_txbExitDistance);
			this.m_secDistance.Controls.Add(this.m_lblExitDistanceUnit);
			this.m_secDistance.Controls.Add(this.m_lblFollowSafeDistance);
			this.m_secDistance.Controls.Add(this.m_txbFollowSafeDistance);
			this.m_secDistance.Controls.Add(this.m_lblFollowSafeDistanceUnit);
			this.m_secDistance.Location = new System.Drawing.Point(12, 12);
			this.m_secDistance.Name = "m_secDistance";
			this.m_secDistance.Size = new System.Drawing.Size(296, 103);
			this.m_secDistance.TabIndex = 0;
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
			// m_txbEntryDistance
			//
			this.m_txbEntryDistance.DecimalPlaces = 3;
			this.m_txbEntryDistance.Location = new System.Drawing.Point(110, 10);
			this.m_txbEntryDistance.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_txbEntryDistance.Name = "m_txbEntryDistance";
			this.m_txbEntryDistance.Size = new System.Drawing.Size(150, 23);
			this.m_txbEntryDistance.TabIndex = 1;
			this.m_txbEntryDistance.DebouncedValueChanged += new System.EventHandler(this.m_txbEntryDistance_DebouncedValueChanged);
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
			// m_txbExitDistance
			//
			this.m_txbExitDistance.DecimalPlaces = 3;
			this.m_txbExitDistance.Location = new System.Drawing.Point(110, 40);
			this.m_txbExitDistance.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_txbExitDistance.Name = "m_txbExitDistance";
			this.m_txbExitDistance.Size = new System.Drawing.Size(150, 23);
			this.m_txbExitDistance.TabIndex = 4;
			this.m_txbExitDistance.DebouncedValueChanged += new System.EventHandler(this.m_txbExitDistance_DebouncedValueChanged);
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
			// m_txbFollowSafeDistance
			//
			this.m_txbFollowSafeDistance.DecimalPlaces = 3;
			this.m_txbFollowSafeDistance.Location = new System.Drawing.Point(110, 70);
			this.m_txbFollowSafeDistance.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_txbFollowSafeDistance.Name = "m_txbFollowSafeDistance";
			this.m_txbFollowSafeDistance.Size = new System.Drawing.Size(150, 23);
			this.m_txbFollowSafeDistance.TabIndex = 7;
			this.m_txbFollowSafeDistance.DebouncedValueChanged += new System.EventHandler(this.m_txbFollowSafeDistance_DebouncedValueChanged);
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
			this.Controls.Add(this.m_secDistance);
			this.Controls.Add(this.m_btnConfirm);
			this.Name = "EntryAndExitDlg";
			this.Text = "進入/退出";
			this.m_secDistance.ResumeLayout(false);
			this.m_secDistance.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbEntryDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbExitDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbFollowSafeDistance)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_secDistance;
		private System.Windows.Forms.Label m_lblEntryDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbEntryDistance;
		private CommonUI.Controls.ThemedLabel m_lblEntryDistanceUnit;
		private System.Windows.Forms.Label m_lblExitDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbExitDistance;
		private CommonUI.Controls.ThemedLabel m_lblExitDistanceUnit;
		private System.Windows.Forms.Label m_lblFollowSafeDistance;
		private CommonUI.Controls.DebouncedNumericUpDown m_txbFollowSafeDistance;
		private CommonUI.Controls.ThemedLabel m_lblFollowSafeDistanceUnit;
		private CommonUI.Controls.ThemedButton m_btnConfirm;
	}
}
