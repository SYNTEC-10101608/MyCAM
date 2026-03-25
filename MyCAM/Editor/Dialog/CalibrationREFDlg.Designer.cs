namespace MyCAM.Editor.Dialog
{
	partial class CalibrationREFDlg
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
			this.m_gbPnt1 = new System.Windows.Forms.GroupBox();
			this.m_btnDelPnt1 = new System.Windows.Forms.Button();
			this.m_tbxPnt1Z = new System.Windows.Forms.TextBox();
			this.m_tbxPnt1Y = new System.Windows.Forms.TextBox();
			this.m_tbxPnt1X = new System.Windows.Forms.TextBox();
			this.m_LblPnt1Z = new System.Windows.Forms.Label();
			this.m_LblPnt1Y = new System.Windows.Forms.Label();
			this.m_LblPnt1X = new System.Windows.Forms.Label();
			this.m_gbPnt2 = new System.Windows.Forms.GroupBox();
			this.m_btnDelPnt2 = new System.Windows.Forms.Button();
			this.m_tbxPnt2Z = new System.Windows.Forms.TextBox();
			this.m_tbxPnt2Y = new System.Windows.Forms.TextBox();
			this.m_tbxPnt2X = new System.Windows.Forms.TextBox();
			this.m_LblPnt2Z = new System.Windows.Forms.Label();
			this.m_LblPnt2Y = new System.Windows.Forms.Label();
			this.m_LblPnt2X = new System.Windows.Forms.Label();
			this.m_gbPnt3 = new System.Windows.Forms.GroupBox();
			this.m_btnDelPnt3 = new System.Windows.Forms.Button();
			this.m_tbxPnt3Z = new System.Windows.Forms.TextBox();
			this.m_tbxPnt3Y = new System.Windows.Forms.TextBox();
			this.m_tbxPnt3X = new System.Windows.Forms.TextBox();
			this.m_LblPnt3Z = new System.Windows.Forms.Label();
			this.m_LblPnt3Y = new System.Windows.Forms.Label();
			this.m_LblPnt3X = new System.Windows.Forms.Label();
			this.m_btnSure = new System.Windows.Forms.Button();
			this.m_btnDeletAll = new System.Windows.Forms.Button();
			this.m_gbPnt1.SuspendLayout();
			this.m_gbPnt2.SuspendLayout();
			this.m_gbPnt3.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_gbPnt1
			// 
			this.m_gbPnt1.Controls.Add(this.m_btnDelPnt1);
			this.m_gbPnt1.Controls.Add(this.m_tbxPnt1Z);
			this.m_gbPnt1.Controls.Add(this.m_tbxPnt1Y);
			this.m_gbPnt1.Controls.Add(this.m_tbxPnt1X);
			this.m_gbPnt1.Controls.Add(this.m_LblPnt1Z);
			this.m_gbPnt1.Controls.Add(this.m_LblPnt1Y);
			this.m_gbPnt1.Controls.Add(this.m_LblPnt1X);
			this.m_gbPnt1.Location = new System.Drawing.Point(12, 25);
			this.m_gbPnt1.Name = "m_gbPnt1";
			this.m_gbPnt1.Size = new System.Drawing.Size(152, 183);
			this.m_gbPnt1.TabIndex = 0;
			this.m_gbPnt1.TabStop = false;
			this.m_gbPnt1.Text = "參考點1  ";
			// 
			// m_btnDelPnt1
			// 
			this.m_btnDelPnt1.Location = new System.Drawing.Point(74, 153);
			this.m_btnDelPnt1.Name = "m_btnDelPnt1";
			this.m_btnDelPnt1.Size = new System.Drawing.Size(66, 27);
			this.m_btnDelPnt1.TabIndex = 10;
			this.m_btnDelPnt1.Text = "刪除";
			this.m_btnDelPnt1.UseVisualStyleBackColor = true;
			this.m_btnDelPnt1.Click += new System.EventHandler(this.m_btnDelPnt1_Click);
			// 
			// m_tbxPnt1Z
			// 
			this.m_tbxPnt1Z.BackColor = System.Drawing.SystemColors.Window;
			this.m_tbxPnt1Z.Location = new System.Drawing.Point(46, 122);
			this.m_tbxPnt1Z.Name = "m_tbxPnt1Z";
			this.m_tbxPnt1Z.ReadOnly = true;
			this.m_tbxPnt1Z.Size = new System.Drawing.Size(94, 25);
			this.m_tbxPnt1Z.TabIndex = 5;
			// 
			// m_tbxPnt1Y
			// 
			this.m_tbxPnt1Y.BackColor = System.Drawing.SystemColors.Window;
			this.m_tbxPnt1Y.Location = new System.Drawing.Point(46, 77);
			this.m_tbxPnt1Y.Name = "m_tbxPnt1Y";
			this.m_tbxPnt1Y.ReadOnly = true;
			this.m_tbxPnt1Y.Size = new System.Drawing.Size(94, 25);
			this.m_tbxPnt1Y.TabIndex = 4;
			// 
			// m_tbxPnt1X
			// 
			this.m_tbxPnt1X.BackColor = System.Drawing.SystemColors.Window;
			this.m_tbxPnt1X.Location = new System.Drawing.Point(46, 32);
			this.m_tbxPnt1X.Name = "m_tbxPnt1X";
			this.m_tbxPnt1X.ReadOnly = true;
			this.m_tbxPnt1X.Size = new System.Drawing.Size(94, 25);
			this.m_tbxPnt1X.TabIndex = 3;
			// 
			// m_LblPnt1Z
			// 
			this.m_LblPnt1Z.AutoSize = true;
			this.m_LblPnt1Z.Location = new System.Drawing.Point(16, 125);
			this.m_LblPnt1Z.Name = "m_LblPnt1Z";
			this.m_LblPnt1Z.Size = new System.Drawing.Size(24, 15);
			this.m_LblPnt1Z.TabIndex = 2;
			this.m_LblPnt1Z.Text = "Z :";
			// 
			// m_LblPnt1Y
			// 
			this.m_LblPnt1Y.AutoSize = true;
			this.m_LblPnt1Y.Location = new System.Drawing.Point(15, 80);
			this.m_LblPnt1Y.Name = "m_LblPnt1Y";
			this.m_LblPnt1Y.Size = new System.Drawing.Size(25, 15);
			this.m_LblPnt1Y.TabIndex = 1;
			this.m_LblPnt1Y.Text = "Y :";
			// 
			// m_LblPnt1X
			// 
			this.m_LblPnt1X.AutoSize = true;
			this.m_LblPnt1X.Location = new System.Drawing.Point(15, 35);
			this.m_LblPnt1X.Name = "m_LblPnt1X";
			this.m_LblPnt1X.Size = new System.Drawing.Size(25, 15);
			this.m_LblPnt1X.TabIndex = 0;
			this.m_LblPnt1X.Text = "X :";
			// 
			// m_gbPnt2
			// 
			this.m_gbPnt2.Controls.Add(this.m_btnDelPnt2);
			this.m_gbPnt2.Controls.Add(this.m_tbxPnt2Z);
			this.m_gbPnt2.Controls.Add(this.m_tbxPnt2Y);
			this.m_gbPnt2.Controls.Add(this.m_tbxPnt2X);
			this.m_gbPnt2.Controls.Add(this.m_LblPnt2Z);
			this.m_gbPnt2.Controls.Add(this.m_LblPnt2Y);
			this.m_gbPnt2.Controls.Add(this.m_LblPnt2X);
			this.m_gbPnt2.Location = new System.Drawing.Point(194, 25);
			this.m_gbPnt2.Name = "m_gbPnt2";
			this.m_gbPnt2.Size = new System.Drawing.Size(152, 183);
			this.m_gbPnt2.TabIndex = 6;
			this.m_gbPnt2.TabStop = false;
			this.m_gbPnt2.Text = "參考點2 ";
			// 
			// m_btnDelPnt2
			// 
			this.m_btnDelPnt2.Location = new System.Drawing.Point(72, 153);
			this.m_btnDelPnt2.Name = "m_btnDelPnt2";
			this.m_btnDelPnt2.Size = new System.Drawing.Size(66, 27);
			this.m_btnDelPnt2.TabIndex = 11;
			this.m_btnDelPnt2.Text = "刪除";
			this.m_btnDelPnt2.UseVisualStyleBackColor = true;
			this.m_btnDelPnt2.Click += new System.EventHandler(this.m_btnDelPnt2_Click);
			// 
			// m_tbxPnt2Z
			// 
			this.m_tbxPnt2Z.BackColor = System.Drawing.SystemColors.Window;
			this.m_tbxPnt2Z.Location = new System.Drawing.Point(46, 122);
			this.m_tbxPnt2Z.Name = "m_tbxPnt2Z";
			this.m_tbxPnt2Z.ReadOnly = true;
			this.m_tbxPnt2Z.Size = new System.Drawing.Size(94, 25);
			this.m_tbxPnt2Z.TabIndex = 5;
			// 
			// m_tbxPnt2Y
			// 
			this.m_tbxPnt2Y.BackColor = System.Drawing.SystemColors.Window;
			this.m_tbxPnt2Y.Location = new System.Drawing.Point(46, 77);
			this.m_tbxPnt2Y.Name = "m_tbxPnt2Y";
			this.m_tbxPnt2Y.ReadOnly = true;
			this.m_tbxPnt2Y.Size = new System.Drawing.Size(94, 25);
			this.m_tbxPnt2Y.TabIndex = 4;
			// 
			// m_tbxPnt2X
			// 
			this.m_tbxPnt2X.BackColor = System.Drawing.SystemColors.Window;
			this.m_tbxPnt2X.Location = new System.Drawing.Point(46, 32);
			this.m_tbxPnt2X.Name = "m_tbxPnt2X";
			this.m_tbxPnt2X.ReadOnly = true;
			this.m_tbxPnt2X.Size = new System.Drawing.Size(94, 25);
			this.m_tbxPnt2X.TabIndex = 3;
			// 
			// m_LblPnt2Z
			// 
			this.m_LblPnt2Z.AutoSize = true;
			this.m_LblPnt2Z.Location = new System.Drawing.Point(16, 125);
			this.m_LblPnt2Z.Name = "m_LblPnt2Z";
			this.m_LblPnt2Z.Size = new System.Drawing.Size(24, 15);
			this.m_LblPnt2Z.TabIndex = 2;
			this.m_LblPnt2Z.Text = "Z :";
			// 
			// m_LblPnt2Y
			// 
			this.m_LblPnt2Y.AutoSize = true;
			this.m_LblPnt2Y.Location = new System.Drawing.Point(15, 80);
			this.m_LblPnt2Y.Name = "m_LblPnt2Y";
			this.m_LblPnt2Y.Size = new System.Drawing.Size(25, 15);
			this.m_LblPnt2Y.TabIndex = 1;
			this.m_LblPnt2Y.Text = "Y :";
			// 
			// m_LblPnt2X
			// 
			this.m_LblPnt2X.AutoSize = true;
			this.m_LblPnt2X.Location = new System.Drawing.Point(15, 35);
			this.m_LblPnt2X.Name = "m_LblPnt2X";
			this.m_LblPnt2X.Size = new System.Drawing.Size(25, 15);
			this.m_LblPnt2X.TabIndex = 0;
			this.m_LblPnt2X.Text = "X :";
			// 
			// m_gbPnt3
			// 
			this.m_gbPnt3.Controls.Add(this.m_btnDelPnt3);
			this.m_gbPnt3.Controls.Add(this.m_tbxPnt3Z);
			this.m_gbPnt3.Controls.Add(this.m_tbxPnt3Y);
			this.m_gbPnt3.Controls.Add(this.m_tbxPnt3X);
			this.m_gbPnt3.Controls.Add(this.m_LblPnt3Z);
			this.m_gbPnt3.Controls.Add(this.m_LblPnt3Y);
			this.m_gbPnt3.Controls.Add(this.m_LblPnt3X);
			this.m_gbPnt3.Location = new System.Drawing.Point(379, 25);
			this.m_gbPnt3.Name = "m_gbPnt3";
			this.m_gbPnt3.Size = new System.Drawing.Size(152, 183);
			this.m_gbPnt3.TabIndex = 6;
			this.m_gbPnt3.TabStop = false;
			this.m_gbPnt3.Text = "參考點3";
			// 
			// m_btnDelPnt3
			// 
			this.m_btnDelPnt3.Location = new System.Drawing.Point(74, 153);
			this.m_btnDelPnt3.Name = "m_btnDelPnt3";
			this.m_btnDelPnt3.Size = new System.Drawing.Size(66, 27);
			this.m_btnDelPnt3.TabIndex = 12;
			this.m_btnDelPnt3.Text = "刪除";
			this.m_btnDelPnt3.UseVisualStyleBackColor = true;
			this.m_btnDelPnt3.Click += new System.EventHandler(this.m_btnDelPnt3_Click);
			// 
			// m_tbxPnt3Z
			// 
			this.m_tbxPnt3Z.BackColor = System.Drawing.SystemColors.Window;
			this.m_tbxPnt3Z.Location = new System.Drawing.Point(46, 122);
			this.m_tbxPnt3Z.Name = "m_tbxPnt3Z";
			this.m_tbxPnt3Z.ReadOnly = true;
			this.m_tbxPnt3Z.Size = new System.Drawing.Size(94, 25);
			this.m_tbxPnt3Z.TabIndex = 5;
			// 
			// m_tbxPnt3Y
			// 
			this.m_tbxPnt3Y.BackColor = System.Drawing.SystemColors.Window;
			this.m_tbxPnt3Y.Location = new System.Drawing.Point(46, 77);
			this.m_tbxPnt3Y.Name = "m_tbxPnt3Y";
			this.m_tbxPnt3Y.ReadOnly = true;
			this.m_tbxPnt3Y.Size = new System.Drawing.Size(94, 25);
			this.m_tbxPnt3Y.TabIndex = 4;
			// 
			// m_tbxPnt3X
			// 
			this.m_tbxPnt3X.BackColor = System.Drawing.SystemColors.Window;
			this.m_tbxPnt3X.Location = new System.Drawing.Point(46, 32);
			this.m_tbxPnt3X.Name = "m_tbxPnt3X";
			this.m_tbxPnt3X.ReadOnly = true;
			this.m_tbxPnt3X.Size = new System.Drawing.Size(94, 25);
			this.m_tbxPnt3X.TabIndex = 3;
			// 
			// m_LblPnt3Z
			// 
			this.m_LblPnt3Z.AutoSize = true;
			this.m_LblPnt3Z.Location = new System.Drawing.Point(16, 125);
			this.m_LblPnt3Z.Name = "m_LblPnt3Z";
			this.m_LblPnt3Z.Size = new System.Drawing.Size(24, 15);
			this.m_LblPnt3Z.TabIndex = 2;
			this.m_LblPnt3Z.Text = "Z :";
			// 
			// m_LblPnt3Y
			// 
			this.m_LblPnt3Y.AutoSize = true;
			this.m_LblPnt3Y.Location = new System.Drawing.Point(15, 80);
			this.m_LblPnt3Y.Name = "m_LblPnt3Y";
			this.m_LblPnt3Y.Size = new System.Drawing.Size(25, 15);
			this.m_LblPnt3Y.TabIndex = 1;
			this.m_LblPnt3Y.Text = "Y :";
			// 
			// m_LblPnt3X
			// 
			this.m_LblPnt3X.AutoSize = true;
			this.m_LblPnt3X.Location = new System.Drawing.Point(15, 35);
			this.m_LblPnt3X.Name = "m_LblPnt3X";
			this.m_LblPnt3X.Size = new System.Drawing.Size(25, 15);
			this.m_LblPnt3X.TabIndex = 0;
			this.m_LblPnt3X.Text = "X :";
			// 
			// m_btnSure
			// 
			this.m_btnSure.Location = new System.Drawing.Point(437, 218);
			this.m_btnSure.Name = "m_btnSure";
			this.m_btnSure.Size = new System.Drawing.Size(94, 36);
			this.m_btnSure.TabIndex = 7;
			this.m_btnSure.Text = "確認";
			this.m_btnSure.UseVisualStyleBackColor = true;
			this.m_btnSure.Click += new System.EventHandler(this.m_btnSure_Click);
			// 
			// m_btnDeletAll
			// 
			this.m_btnDeletAll.Location = new System.Drawing.Point(337, 218);
			this.m_btnDeletAll.Name = "m_btnDeletAll";
			this.m_btnDeletAll.Size = new System.Drawing.Size(94, 36);
			this.m_btnDeletAll.TabIndex = 8;
			this.m_btnDeletAll.Text = "重置所有";
			this.m_btnDeletAll.UseVisualStyleBackColor = true;
			this.m_btnDeletAll.Click += new System.EventHandler(this.m_btnDeletAll_Click);
			// 
			// CalibrationREFDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(557, 266);
			this.Controls.Add(this.m_btnDeletAll);
			this.Controls.Add(this.m_btnSure);
			this.Controls.Add(this.m_gbPnt3);
			this.Controls.Add(this.m_gbPnt2);
			this.Controls.Add(this.m_gbPnt1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CalibrationREFDlg";
			this.ShowIcon = false;
			this.Text = "三點校正參考點設置";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CalibrationREFDlg_KeyDown);
			this.m_gbPnt1.ResumeLayout(false);
			this.m_gbPnt1.PerformLayout();
			this.m_gbPnt2.ResumeLayout(false);
			this.m_gbPnt2.PerformLayout();
			this.m_gbPnt3.ResumeLayout(false);
			this.m_gbPnt3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox m_gbPnt1;
		private System.Windows.Forms.Label m_LblPnt1X;
		private System.Windows.Forms.Label m_LblPnt1Z;
		private System.Windows.Forms.Label m_LblPnt1Y;
		private System.Windows.Forms.TextBox m_tbxPnt1Z;
		private System.Windows.Forms.TextBox m_tbxPnt1Y;
		private System.Windows.Forms.TextBox m_tbxPnt1X;
		private System.Windows.Forms.GroupBox m_gbPnt2;
		private System.Windows.Forms.TextBox m_tbxPnt2Z;
		private System.Windows.Forms.TextBox m_tbxPnt2Y;
		private System.Windows.Forms.TextBox m_tbxPnt2X;
		private System.Windows.Forms.Label m_LblPnt2Z;
		private System.Windows.Forms.Label m_LblPnt2Y;
		private System.Windows.Forms.Label m_LblPnt2X;
		private System.Windows.Forms.GroupBox m_gbPnt3;
		private System.Windows.Forms.TextBox m_tbxPnt3Z;
		private System.Windows.Forms.TextBox m_tbxPnt3Y;
		private System.Windows.Forms.TextBox m_tbxPnt3X;
		private System.Windows.Forms.Label m_LblPnt3Z;
		private System.Windows.Forms.Label m_LblPnt3Y;
		private System.Windows.Forms.Label m_LblPnt3X;
		private System.Windows.Forms.Button m_btnSure;
		private System.Windows.Forms.Button m_btnDelPnt1;
		private System.Windows.Forms.Button m_btnDelPnt2;
		private System.Windows.Forms.Button m_btnDelPnt3;
		private System.Windows.Forms.Button m_btnDeletAll;
	}
}