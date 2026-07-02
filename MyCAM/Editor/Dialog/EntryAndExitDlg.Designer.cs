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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblExitDistanceUnit = new System.Windows.Forms.Label();
			this.m_lblEntryDistanceUnit = new System.Windows.Forms.Label();
			this.m_txbEntryDistance = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_txbExitDistance = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblEntryDistance = new System.Windows.Forms.Label();
			this.m_lblExitDistance = new System.Windows.Forms.Label();
			this.m_btnConfirm = new System.Windows.Forms.Button();
			this.m_lblFollowSafeDistance = new System.Windows.Forms.Label();
			this.m_txbFollowSafeDistance = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblFollowSafeDistanceUnit = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbEntryDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbExitDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbFollowSafeDistance)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
			this.tableLayoutPanel1.Controls.Add(this.m_lblExitDistanceUnit, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_lblEntryDistanceUnit, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_txbEntryDistance, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_txbExitDistance, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_lblEntryDistance, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_lblExitDistance, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_btnConfirm, 2, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_lblFollowSafeDistance, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_txbFollowSafeDistance, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_lblFollowSafeDistanceUnit, 2, 2);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(16, 15);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(436, 165);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// m_lblExitDistanceUnit
			// 
			this.m_lblExitDistanceUnit.AutoSize = true;
			this.m_lblExitDistanceUnit.Location = new System.Drawing.Point(271, 38);
			this.m_lblExitDistanceUnit.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this.m_lblExitDistanceUnit.Name = "m_lblExitDistanceUnit";
			this.m_lblExitDistanceUnit.Size = new System.Drawing.Size(29, 15);
			this.m_lblExitDistanceUnit.TabIndex = 13;
			this.m_lblExitDistanceUnit.Text = "mm";
			// 
			// m_lblEntryDistanceUnit
			// 
			this.m_lblEntryDistanceUnit.AutoSize = true;
			this.m_lblEntryDistanceUnit.Location = new System.Drawing.Point(271, 0);
			this.m_lblEntryDistanceUnit.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this.m_lblEntryDistanceUnit.Name = "m_lblEntryDistanceUnit";
			this.m_lblEntryDistanceUnit.Size = new System.Drawing.Size(29, 15);
			this.m_lblEntryDistanceUnit.TabIndex = 11;
			this.m_lblEntryDistanceUnit.Text = "mm";
			// 
			// m_txbEntryDistance
			// 
			this.m_txbEntryDistance.DecimalPlaces = 3;
			this.m_txbEntryDistance.Location = new System.Drawing.Point(138, 6);
			this.m_txbEntryDistance.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
			this.m_txbEntryDistance.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_txbEntryDistance.Name = "m_txbEntryDistance";
			this.m_txbEntryDistance.Size = new System.Drawing.Size(123, 25);
			this.m_txbEntryDistance.TabIndex = 7;
			this.m_txbEntryDistance.DebouncedValueChanged += new System.EventHandler(this.m_txbEntryDistance_DebouncedValueChanged);
			// 
			// m_txbExitDistance
			// 
			this.m_txbExitDistance.DecimalPlaces = 3;
			this.m_txbExitDistance.Location = new System.Drawing.Point(138, 44);
			this.m_txbExitDistance.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
			this.m_txbExitDistance.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_txbExitDistance.Name = "m_txbExitDistance";
			this.m_txbExitDistance.Size = new System.Drawing.Size(123, 25);
			this.m_txbExitDistance.TabIndex = 8;
			this.m_txbExitDistance.DebouncedValueChanged += new System.EventHandler(this.m_txbExitDistance_DebouncedValueChanged);
			// 
			// m_lblEntryDistance
			// 
			this.m_lblEntryDistance.AutoSize = true;
			this.m_lblEntryDistance.Location = new System.Drawing.Point(8, 0);
			this.m_lblEntryDistance.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
			this.m_lblEntryDistance.Name = "m_lblEntryDistance";
			this.m_lblEntryDistance.Size = new System.Drawing.Size(67, 15);
			this.m_lblEntryDistance.TabIndex = 10;
			this.m_lblEntryDistance.Text = "進入距離";
			// 
			// m_lblExitDistance
			// 
			this.m_lblExitDistance.AutoSize = true;
			this.m_lblExitDistance.Location = new System.Drawing.Point(8, 38);
			this.m_lblExitDistance.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
			this.m_lblExitDistance.Name = "m_lblExitDistance";
			this.m_lblExitDistance.Size = new System.Drawing.Size(67, 15);
			this.m_lblExitDistance.TabIndex = 12;
			this.m_lblExitDistance.Text = "退出距離";
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Location = new System.Drawing.Point(271, 120);
			this.m_btnConfirm.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(123, 29);
			this.m_btnConfirm.TabIndex = 9;
			this.m_btnConfirm.Text = "確認";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// m_lblFollowSafeDistance
			// 
			this.m_lblFollowSafeDistance.AutoSize = true;
			this.m_lblFollowSafeDistance.Location = new System.Drawing.Point(8, 76);
			this.m_lblFollowSafeDistance.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
			this.m_lblFollowSafeDistance.Name = "m_lblFollowSafeDistance";
			this.m_lblFollowSafeDistance.Size = new System.Drawing.Size(97, 15);
			this.m_lblFollowSafeDistance.TabIndex = 15;
			this.m_lblFollowSafeDistance.Text = "隨動安全距離";
			// 
			// m_txbFollowSafeDistance
			// 
			this.m_txbFollowSafeDistance.DecimalPlaces = 3;
			this.m_txbFollowSafeDistance.Location = new System.Drawing.Point(138, 82);
			this.m_txbFollowSafeDistance.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
			this.m_txbFollowSafeDistance.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_txbFollowSafeDistance.Name = "m_txbFollowSafeDistance";
			this.m_txbFollowSafeDistance.Size = new System.Drawing.Size(123, 25);
			this.m_txbFollowSafeDistance.TabIndex = 14;
			this.m_txbFollowSafeDistance.DebouncedValueChanged += new System.EventHandler(this.m_txbFollowSafeDistance_DebouncedValueChanged);
			// 
			// m_lblFollowSafeDistanceUnit
			// 
			this.m_lblFollowSafeDistanceUnit.AutoSize = true;
			this.m_lblFollowSafeDistanceUnit.Location = new System.Drawing.Point(271, 76);
			this.m_lblFollowSafeDistanceUnit.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this.m_lblFollowSafeDistanceUnit.Name = "m_lblFollowSafeDistanceUnit";
			this.m_lblFollowSafeDistanceUnit.Size = new System.Drawing.Size(29, 15);
			this.m_lblFollowSafeDistanceUnit.TabIndex = 16;
			this.m_lblFollowSafeDistanceUnit.Text = "mm";
			// 
			// EntryAndExitDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(460, 186);
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.Name = "EntryAndExitDlg";
			this.Text = "進入/退出";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_txbEntryDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbExitDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_txbFollowSafeDistance)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button m_btnConfirm;
		private System.Windows.Forms.Label m_lblExitDistanceUnit;
		private System.Windows.Forms.Label m_lblEntryDistanceUnit;
		private MyCAM.Editor.DebouncedNumericUpDown m_txbEntryDistance;
		private MyCAM.Editor.DebouncedNumericUpDown m_txbExitDistance;
		private System.Windows.Forms.Label m_lblEntryDistance;
		private System.Windows.Forms.Label m_lblExitDistance;
		private System.Windows.Forms.Label m_lblFollowSafeDistance;
		private MyCAM.Editor.DebouncedNumericUpDown m_txbFollowSafeDistance;
		private System.Windows.Forms.Label m_lblFollowSafeDistanceUnit;
	}
}
