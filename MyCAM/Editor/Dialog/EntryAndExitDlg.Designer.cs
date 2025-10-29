namespace MyCAM.Editor.Dialog
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
			this.m_lblEntryDistance = new System.Windows.Forms.Label();
			this.m_txbEntryDistance = new System.Windows.Forms.TextBox();
			this.m_lblEntryDistanceUnit = new System.Windows.Forms.Label();
			this.m_lblExitDistance = new System.Windows.Forms.Label();
			this.m_txbExitDistance = new System.Windows.Forms.TextBox();
			this.m_lblExitDistanceUnit = new System.Windows.Forms.Label();
			this.m_btnConfirm = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.Controls.Add(this.m_btnConfirm, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_lblExitDistanceUnit, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_lblEntryDistanceUnit, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_txbEntryDistance, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_txbExitDistance, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_lblEntryDistance, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_lblExitDistance, 0, 1);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(303, 93);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// m_lblEntryDistance
			// 
			this.m_lblEntryDistance.AutoSize = true;
			this.m_lblEntryDistance.Location = new System.Drawing.Point(6, 0);
			this.m_lblEntryDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblEntryDistance.Name = "m_lblEntryDistance";
			this.m_lblEntryDistance.Size = new System.Drawing.Size(53, 12);
			this.m_lblEntryDistance.TabIndex = 10;
			this.m_lblEntryDistance.Text = "進入距離";
			// 
			// m_txbEntryDistance
			// 
			this.m_txbEntryDistance.Location = new System.Drawing.Point(104, 5);
			this.m_txbEntryDistance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_txbEntryDistance.Name = "m_txbEntryDistance";
			this.m_txbEntryDistance.Size = new System.Drawing.Size(92, 22);
			this.m_txbEntryDistance.TabIndex = 7;
			this.m_txbEntryDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbEntryDistance_KeyDown);
			this.m_txbEntryDistance.Leave += new System.EventHandler(this.m_txbEntryDistance_Leave);
			// 
			// m_lblEntryDistanceUnit
			// 
			this.m_lblEntryDistanceUnit.AutoSize = true;
			this.m_lblEntryDistanceUnit.Location = new System.Drawing.Point(204, 0);
			this.m_lblEntryDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblEntryDistanceUnit.Name = "m_lblEntryDistanceUnit";
			this.m_lblEntryDistanceUnit.Size = new System.Drawing.Size(23, 12);
			this.m_lblEntryDistanceUnit.TabIndex = 11;
			this.m_lblEntryDistanceUnit.Text = "mm";
			// 
			// m_lblExitDistance
			// 
			this.m_lblExitDistance.AutoSize = true;
			this.m_lblExitDistance.Location = new System.Drawing.Point(6, 30);
			this.m_lblExitDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblExitDistance.Name = "m_lblExitDistance";
			this.m_lblExitDistance.Size = new System.Drawing.Size(53, 12);
			this.m_lblExitDistance.TabIndex = 12;
			this.m_lblExitDistance.Text = "退出距離";
			// 
			// m_txbExitDistance
			// 
			this.m_txbExitDistance.Location = new System.Drawing.Point(104, 35);
			this.m_txbExitDistance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_txbExitDistance.Name = "m_txbExitDistance";
			this.m_txbExitDistance.Size = new System.Drawing.Size(92, 22);
			this.m_txbExitDistance.TabIndex = 8;
			this.m_txbExitDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbExitDistance_KeyDown);
			this.m_txbExitDistance.Leave += new System.EventHandler(this.m_txbExitDistance_Leave);
			// 
			// m_lblExitDistanceUnit
			// 
			this.m_lblExitDistanceUnit.AutoSize = true;
			this.m_lblExitDistanceUnit.Location = new System.Drawing.Point(204, 30);
			this.m_lblExitDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblExitDistanceUnit.Name = "m_lblExitDistanceUnit";
			this.m_lblExitDistanceUnit.Size = new System.Drawing.Size(23, 12);
			this.m_lblExitDistanceUnit.TabIndex = 13;
			this.m_lblExitDistanceUnit.Text = "mm";
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Location = new System.Drawing.Point(204, 65);
			this.m_btnConfirm.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(92, 23);
			this.m_btnConfirm.TabIndex = 9;
			this.m_btnConfirm.Text = "確認";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// EntryAndExitDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(336, 118);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "EntryAndExitDlg";
			this.Text = "進入/退出";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button m_btnConfirm;
		private System.Windows.Forms.Label m_lblExitDistanceUnit;
		private System.Windows.Forms.Label m_lblEntryDistanceUnit;
		private System.Windows.Forms.TextBox m_txbEntryDistance;
		private System.Windows.Forms.TextBox m_txbExitDistance;
		private System.Windows.Forms.Label m_lblEntryDistance;
		private System.Windows.Forms.Label m_lblExitDistance;
	}
}