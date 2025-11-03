namespace MyCAM.Editor
{
	partial class TraverseDlg
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
			this.m_btnConfirm = new System.Windows.Forms.Button();
			this.m_lblFollowSaveDistanceUnit = new System.Windows.Forms.Label();
			this.m_lblCutDownDistanceUnit = new System.Windows.Forms.Label();
			this.m_txbFollowSafeDistance = new System.Windows.Forms.TextBox();
			this.m_txbCutDownDistance = new System.Windows.Forms.TextBox();
			this.m_lblFollowSaveDistance = new System.Windows.Forms.Label();
			this.m_lblCutDownDistance = new System.Windows.Forms.Label();
			this.m_lblLiftUpDistanceUnit = new System.Windows.Forms.Label();
			this.m_txbLiftUpDistance = new System.Windows.Forms.TextBox();
			this.m_lblLiftUpDistance = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblFrogLeapDistance = new System.Windows.Forms.Label();
			this.m_txbFrogLeapDistance = new System.Windows.Forms.TextBox();
			this.m_lblFrogLeapDistanceUnit = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Location = new System.Drawing.Point(274, 205);
			this.m_btnConfirm.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(116, 34);
			this.m_btnConfirm.TabIndex = 5;
			this.m_btnConfirm.Text = "確認";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// m_lblFollowSaveDistanceUnit
			// 
			this.m_lblFollowSaveDistanceUnit.AutoSize = true;
			this.m_lblFollowSaveDistanceUnit.Location = new System.Drawing.Point(274, 80);
			this.m_lblFollowSaveDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblFollowSaveDistanceUnit.Name = "m_lblFollowSaveDistanceUnit";
			this.m_lblFollowSaveDistanceUnit.Size = new System.Drawing.Size(34, 18);
			this.m_lblFollowSaveDistanceUnit.TabIndex = 6;
			this.m_lblFollowSaveDistanceUnit.Text = "mm";
			// 
			// m_lblCutDownDistanceUnit
			// 
			this.m_lblCutDownDistanceUnit.AutoSize = true;
			this.m_lblCutDownDistanceUnit.Location = new System.Drawing.Point(274, 50);
			this.m_lblCutDownDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblCutDownDistanceUnit.Name = "m_lblCutDownDistanceUnit";
			this.m_lblCutDownDistanceUnit.Size = new System.Drawing.Size(34, 18);
			this.m_lblCutDownDistanceUnit.TabIndex = 6;
			this.m_lblCutDownDistanceUnit.Text = "mm";
			// 
			// m_txbFollowSafeDistance
			// 
			this.m_txbFollowSafeDistance.Location = new System.Drawing.Point(174, 85);
			this.m_txbFollowSafeDistance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_txbFollowSafeDistance.Name = "m_txbFollowSafeDistance";
			this.m_txbFollowSafeDistance.Size = new System.Drawing.Size(92, 26);
			this.m_txbFollowSafeDistance.TabIndex = 2;
			this.m_txbFollowSafeDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbSafeDistance_KeyDown);
			this.m_txbFollowSafeDistance.Leave += new System.EventHandler(this.m_txbFollowSafeDistance_Leave);
			// 
			// m_txbCutDownDistance
			// 
			this.m_txbCutDownDistance.Location = new System.Drawing.Point(174, 55);
			this.m_txbCutDownDistance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_txbCutDownDistance.Name = "m_txbCutDownDistance";
			this.m_txbCutDownDistance.Size = new System.Drawing.Size(92, 26);
			this.m_txbCutDownDistance.TabIndex = 1;
			this.m_txbCutDownDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbCutDownDistance_KeyDown);
			this.m_txbCutDownDistance.Leave += new System.EventHandler(this.m_txbCutDownDistance_Leave);
			// 
			// m_lblFollowSaveDistance
			// 
			this.m_lblFollowSaveDistance.AutoSize = true;
			this.m_lblFollowSaveDistance.Location = new System.Drawing.Point(26, 80);
			this.m_lblFollowSaveDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblFollowSaveDistance.Name = "m_lblFollowSaveDistance";
			this.m_lblFollowSaveDistance.Size = new System.Drawing.Size(104, 18);
			this.m_lblFollowSaveDistance.TabIndex = 6;
			this.m_lblFollowSaveDistance.Text = "隨動安全距離";
			// 
			// m_lblCutDownDistance
			// 
			this.m_lblCutDownDistance.AutoSize = true;
			this.m_lblCutDownDistance.Location = new System.Drawing.Point(26, 50);
			this.m_lblCutDownDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblCutDownDistance.Name = "m_lblCutDownDistance";
			this.m_lblCutDownDistance.Size = new System.Drawing.Size(72, 18);
			this.m_lblCutDownDistance.TabIndex = 6;
			this.m_lblCutDownDistance.Text = "下刀距離";
			// 
			// m_lblLiftUpDistanceUnit
			// 
			this.m_lblLiftUpDistanceUnit.AutoSize = true;
			this.m_lblLiftUpDistanceUnit.Location = new System.Drawing.Point(274, 20);
			this.m_lblLiftUpDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblLiftUpDistanceUnit.Name = "m_lblLiftUpDistanceUnit";
			this.m_lblLiftUpDistanceUnit.Size = new System.Drawing.Size(34, 18);
			this.m_lblLiftUpDistanceUnit.TabIndex = 6;
			this.m_lblLiftUpDistanceUnit.Text = "mm";
			// 
			// m_txbLiftUpDistance
			// 
			this.m_txbLiftUpDistance.Location = new System.Drawing.Point(174, 25);
			this.m_txbLiftUpDistance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_txbLiftUpDistance.Name = "m_txbLiftUpDistance";
			this.m_txbLiftUpDistance.Size = new System.Drawing.Size(92, 26);
			this.m_txbLiftUpDistance.TabIndex = 0;
			this.m_txbLiftUpDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbLiftUpDistance_KeyDown);
			this.m_txbLiftUpDistance.Leave += new System.EventHandler(this.m_txbLiftUpDistance_Leave);
			// 
			// m_lblLiftUpDistance
			// 
			this.m_lblLiftUpDistance.AutoSize = true;
			this.m_lblLiftUpDistance.Location = new System.Drawing.Point(26, 20);
			this.m_lblLiftUpDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblLiftUpDistance.Name = "m_lblLiftUpDistance";
			this.m_lblLiftUpDistance.Size = new System.Drawing.Size(72, 18);
			this.m_lblLiftUpDistance.TabIndex = 6;
			this.m_lblLiftUpDistance.Text = "抬刀距離";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
			this.tableLayoutPanel1.Controls.Add(this.m_lblLiftUpDistance, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_txbLiftUpDistance, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_lblLiftUpDistanceUnit, 3, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_lblCutDownDistance, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_txbCutDownDistance, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_lblCutDownDistanceUnit, 3, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_lblFollowSaveDistance, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_txbFollowSafeDistance, 2, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_lblFollowSaveDistanceUnit, 3, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_lblFrogLeapDistance, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.m_txbFrogLeapDistance, 2, 4);
			this.tableLayoutPanel1.Controls.Add(this.m_lblFrogLeapDistanceUnit, 3, 4);
			this.tableLayoutPanel1.Controls.Add(this.m_btnConfirm, 3, 7);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 8;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(410, 310);
			this.tableLayoutPanel1.TabIndex = 17;
			// 
			// m_lblFrogLeapDistance
			// 
			this.m_lblFrogLeapDistance.AutoSize = true;
			this.m_lblFrogLeapDistance.Location = new System.Drawing.Point(26, 110);
			this.m_lblFrogLeapDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblFrogLeapDistance.Name = "m_lblFrogLeapDistance";
			this.m_lblFrogLeapDistance.Size = new System.Drawing.Size(72, 18);
			this.m_lblFrogLeapDistance.TabIndex = 6;
			this.m_lblFrogLeapDistance.Text = "蛙跳距離";
			// 
			// m_txbFrogLeapDistance
			// 
			this.m_txbFrogLeapDistance.Location = new System.Drawing.Point(174, 115);
			this.m_txbFrogLeapDistance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_txbFrogLeapDistance.Name = "m_txbFrogLeapDistance";
			this.m_txbFrogLeapDistance.Size = new System.Drawing.Size(92, 26);
			this.m_txbFrogLeapDistance.TabIndex = 3;
			this.m_txbFrogLeapDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbFrogLeapDistance_KeyDown);
			this.m_txbFrogLeapDistance.Leave += new System.EventHandler(this.m_txbFrogLeapDistance_Leave);
			// 
			// m_lblFrogLeapDistanceUnit
			// 
			this.m_lblFrogLeapDistanceUnit.AutoSize = true;
			this.m_lblFrogLeapDistanceUnit.Location = new System.Drawing.Point(274, 110);
			this.m_lblFrogLeapDistanceUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblFrogLeapDistanceUnit.Name = "m_lblFrogLeapDistanceUnit";
			this.m_lblFrogLeapDistanceUnit.Size = new System.Drawing.Size(34, 18);
			this.m_lblFrogLeapDistanceUnit.TabIndex = 6;
			this.m_lblFrogLeapDistanceUnit.Text = "mm";
			// 
			// TraverseDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(402, 250);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "TraverseDlg";
			this.Text = "後處理參數設定";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button m_btnConfirm;
		private System.Windows.Forms.Label m_lblFollowSaveDistanceUnit;
		private System.Windows.Forms.Label m_lblCutDownDistanceUnit;
		private System.Windows.Forms.TextBox m_txbFollowSafeDistance;
		private System.Windows.Forms.TextBox m_txbCutDownDistance;
		private System.Windows.Forms.Label m_lblFollowSaveDistance;
		private System.Windows.Forms.Label m_lblCutDownDistance;
		private System.Windows.Forms.Label m_lblLiftUpDistanceUnit;
		private System.Windows.Forms.TextBox m_txbLiftUpDistance;
		private System.Windows.Forms.Label m_lblLiftUpDistance;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TextBox m_txbFrogLeapDistance;
		private System.Windows.Forms.Label m_lblFrogLeapDistance;
		private System.Windows.Forms.Label m_lblFrogLeapDistanceUnit;
	}
}