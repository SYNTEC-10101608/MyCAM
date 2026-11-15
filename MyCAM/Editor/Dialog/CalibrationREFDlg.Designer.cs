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
			this.m_gbPnt1 = new CommonUI.Controls.SectionPanel();
			this.m_gbPnt2 = new CommonUI.Controls.SectionPanel();
			this.m_gbPnt3 = new CommonUI.Controls.SectionPanel();
			this.m_LblPnt1X = new System.Windows.Forms.Label();
			this.m_tbxPnt1X = new System.Windows.Forms.Label();
			this.m_LblPnt1Y = new System.Windows.Forms.Label();
			this.m_tbxPnt1Y = new System.Windows.Forms.Label();
			this.m_LblPnt1Z = new System.Windows.Forms.Label();
			this.m_tbxPnt1Z = new System.Windows.Forms.Label();
			this.m_btnDelPnt1 = new CommonUI.Controls.ThemedButton();
			this.m_LblPnt2X = new System.Windows.Forms.Label();
			this.m_tbxPnt2X = new System.Windows.Forms.Label();
			this.m_LblPnt2Y = new System.Windows.Forms.Label();
			this.m_tbxPnt2Y = new System.Windows.Forms.Label();
			this.m_LblPnt2Z = new System.Windows.Forms.Label();
			this.m_tbxPnt2Z = new System.Windows.Forms.Label();
			this.m_btnDelPnt2 = new CommonUI.Controls.ThemedButton();
			this.m_LblPnt3X = new System.Windows.Forms.Label();
			this.m_tbxPnt3X = new System.Windows.Forms.Label();
			this.m_LblPnt3Y = new System.Windows.Forms.Label();
			this.m_tbxPnt3Y = new System.Windows.Forms.Label();
			this.m_LblPnt3Z = new System.Windows.Forms.Label();
			this.m_tbxPnt3Z = new System.Windows.Forms.Label();
			this.m_btnDelPnt3 = new CommonUI.Controls.ThemedButton();
			this.m_btnDeletAll = new CommonUI.Controls.ThemedButton();
			this.m_btnSure = new CommonUI.Controls.ThemedButton();
			this.m_gbPnt1.SuspendLayout();
			this.m_gbPnt2.SuspendLayout();
			this.m_gbPnt3.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_LblPnt1X
			// 
			this.m_LblPnt1X.AutoSize = true;
			this.m_LblPnt1X.Location = new System.Drawing.Point(10, 40);
			this.m_LblPnt1X.Name = "m_LblPnt1X";
			this.m_LblPnt1X.Size = new System.Drawing.Size(14, 16);
			this.m_LblPnt1X.TabIndex = 0;
			this.m_LblPnt1X.Text = "X";
			// 
			// m_tbxPnt1X
			// 
			this.m_tbxPnt1X.Location = new System.Drawing.Point(28, 36);
			this.m_tbxPnt1X.Name = "m_tbxPnt1X";
			this.m_tbxPnt1X.Size = new System.Drawing.Size(80, 23);
			this.m_tbxPnt1X.TabIndex = 1;
			this.m_tbxPnt1X.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_LblPnt1Y
			// 
			this.m_LblPnt1Y.AutoSize = true;
			this.m_LblPnt1Y.Location = new System.Drawing.Point(116, 40);
			this.m_LblPnt1Y.Name = "m_LblPnt1Y";
			this.m_LblPnt1Y.Size = new System.Drawing.Size(14, 16);
			this.m_LblPnt1Y.TabIndex = 2;
			this.m_LblPnt1Y.Text = "Y";
			// 
			// m_tbxPnt1Y
			// 
			this.m_tbxPnt1Y.Location = new System.Drawing.Point(134, 36);
			this.m_tbxPnt1Y.Name = "m_tbxPnt1Y";
			this.m_tbxPnt1Y.Size = new System.Drawing.Size(80, 23);
			this.m_tbxPnt1Y.TabIndex = 3;
			this.m_tbxPnt1Y.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_LblPnt1Z
			// 
			this.m_LblPnt1Z.AutoSize = true;
			this.m_LblPnt1Z.Location = new System.Drawing.Point(222, 40);
			this.m_LblPnt1Z.Name = "m_LblPnt1Z";
			this.m_LblPnt1Z.Size = new System.Drawing.Size(14, 16);
			this.m_LblPnt1Z.TabIndex = 4;
			this.m_LblPnt1Z.Text = "Z";
			// 
			// m_tbxPnt1Z
			// 
			this.m_tbxPnt1Z.Location = new System.Drawing.Point(240, 36);
			this.m_tbxPnt1Z.Name = "m_tbxPnt1Z";
			this.m_tbxPnt1Z.Size = new System.Drawing.Size(80, 23);
			this.m_tbxPnt1Z.TabIndex = 5;
			this.m_tbxPnt1Z.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_btnDelPnt1
			// 
			this.m_btnDelPnt1.Kind = CommonUI.Controls.ButtonKind.Danger;
			this.m_btnDelPnt1.Location = new System.Drawing.Point(330, 36);
			this.m_btnDelPnt1.Name = "m_btnDelPnt1";
			this.m_btnDelPnt1.Size = new System.Drawing.Size(70, 23);
			this.m_btnDelPnt1.TabIndex = 6;
			this.m_btnDelPnt1.Text = "刪除";
			this.m_btnDelPnt1.Click += new System.EventHandler(this.m_btnDelPnt1_Click);
			// 
			// m_gbPnt1
			// 
			this.m_gbPnt1.Controls.Add(this.m_LblPnt1X);
			this.m_gbPnt1.Controls.Add(this.m_tbxPnt1X);
			this.m_gbPnt1.Controls.Add(this.m_LblPnt1Y);
			this.m_gbPnt1.Controls.Add(this.m_tbxPnt1Y);
			this.m_gbPnt1.Controls.Add(this.m_LblPnt1Z);
			this.m_gbPnt1.Controls.Add(this.m_tbxPnt1Z);
			this.m_gbPnt1.Controls.Add(this.m_btnDelPnt1);
			this.m_gbPnt1.Location = new System.Drawing.Point(12, 12);
			this.m_gbPnt1.Name = "m_gbPnt1";
			this.m_gbPnt1.Size = new System.Drawing.Size(410, 69);
			this.m_gbPnt1.TabIndex = 0;
			this.m_gbPnt1.Text = "參考點1";
			// 
			// m_LblPnt2X
			// 
			this.m_LblPnt2X.AutoSize = true;
			this.m_LblPnt2X.Location = new System.Drawing.Point(10, 40);
			this.m_LblPnt2X.Name = "m_LblPnt2X";
			this.m_LblPnt2X.Size = new System.Drawing.Size(14, 16);
			this.m_LblPnt2X.TabIndex = 0;
			this.m_LblPnt2X.Text = "X";
			// 
			// m_tbxPnt2X
			// 
			this.m_tbxPnt2X.Location = new System.Drawing.Point(28, 36);
			this.m_tbxPnt2X.Name = "m_tbxPnt2X";
			this.m_tbxPnt2X.Size = new System.Drawing.Size(80, 23);
			this.m_tbxPnt2X.TabIndex = 1;
			this.m_tbxPnt2X.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_LblPnt2Y
			// 
			this.m_LblPnt2Y.AutoSize = true;
			this.m_LblPnt2Y.Location = new System.Drawing.Point(116, 40);
			this.m_LblPnt2Y.Name = "m_LblPnt2Y";
			this.m_LblPnt2Y.Size = new System.Drawing.Size(14, 16);
			this.m_LblPnt2Y.TabIndex = 2;
			this.m_LblPnt2Y.Text = "Y";
			// 
			// m_tbxPnt2Y
			// 
			this.m_tbxPnt2Y.Location = new System.Drawing.Point(134, 36);
			this.m_tbxPnt2Y.Name = "m_tbxPnt2Y";
			this.m_tbxPnt2Y.Size = new System.Drawing.Size(80, 23);
			this.m_tbxPnt2Y.TabIndex = 3;
			this.m_tbxPnt2Y.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_LblPnt2Z
			// 
			this.m_LblPnt2Z.AutoSize = true;
			this.m_LblPnt2Z.Location = new System.Drawing.Point(222, 40);
			this.m_LblPnt2Z.Name = "m_LblPnt2Z";
			this.m_LblPnt2Z.Size = new System.Drawing.Size(14, 16);
			this.m_LblPnt2Z.TabIndex = 4;
			this.m_LblPnt2Z.Text = "Z";
			// 
			// m_tbxPnt2Z
			// 
			this.m_tbxPnt2Z.Location = new System.Drawing.Point(240, 36);
			this.m_tbxPnt2Z.Name = "m_tbxPnt2Z";
			this.m_tbxPnt2Z.Size = new System.Drawing.Size(80, 23);
			this.m_tbxPnt2Z.TabIndex = 5;
			this.m_tbxPnt2Z.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_btnDelPnt2
			// 
			this.m_btnDelPnt2.Kind = CommonUI.Controls.ButtonKind.Danger;
			this.m_btnDelPnt2.Location = new System.Drawing.Point(330, 36);
			this.m_btnDelPnt2.Name = "m_btnDelPnt2";
			this.m_btnDelPnt2.Size = new System.Drawing.Size(70, 23);
			this.m_btnDelPnt2.TabIndex = 6;
			this.m_btnDelPnt2.Text = "刪除";
			this.m_btnDelPnt2.Click += new System.EventHandler(this.m_btnDelPnt2_Click);
			// 
			// m_gbPnt2
			// 
			this.m_gbPnt2.Controls.Add(this.m_LblPnt2X);
			this.m_gbPnt2.Controls.Add(this.m_tbxPnt2X);
			this.m_gbPnt2.Controls.Add(this.m_LblPnt2Y);
			this.m_gbPnt2.Controls.Add(this.m_tbxPnt2Y);
			this.m_gbPnt2.Controls.Add(this.m_LblPnt2Z);
			this.m_gbPnt2.Controls.Add(this.m_tbxPnt2Z);
			this.m_gbPnt2.Controls.Add(this.m_btnDelPnt2);
			this.m_gbPnt2.Location = new System.Drawing.Point(12, 89);
			this.m_gbPnt2.Name = "m_gbPnt2";
			this.m_gbPnt2.Size = new System.Drawing.Size(410, 69);
			this.m_gbPnt2.TabIndex = 1;
			this.m_gbPnt2.Text = "參考點2";
			// 
			// m_LblPnt3X
			// 
			this.m_LblPnt3X.AutoSize = true;
			this.m_LblPnt3X.Location = new System.Drawing.Point(10, 40);
			this.m_LblPnt3X.Name = "m_LblPnt3X";
			this.m_LblPnt3X.Size = new System.Drawing.Size(14, 16);
			this.m_LblPnt3X.TabIndex = 0;
			this.m_LblPnt3X.Text = "X";
			// 
			// m_tbxPnt3X
			// 
			this.m_tbxPnt3X.Location = new System.Drawing.Point(28, 36);
			this.m_tbxPnt3X.Name = "m_tbxPnt3X";
			this.m_tbxPnt3X.Size = new System.Drawing.Size(80, 23);
			this.m_tbxPnt3X.TabIndex = 1;
			this.m_tbxPnt3X.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_LblPnt3Y
			// 
			this.m_LblPnt3Y.AutoSize = true;
			this.m_LblPnt3Y.Location = new System.Drawing.Point(116, 40);
			this.m_LblPnt3Y.Name = "m_LblPnt3Y";
			this.m_LblPnt3Y.Size = new System.Drawing.Size(14, 16);
			this.m_LblPnt3Y.TabIndex = 2;
			this.m_LblPnt3Y.Text = "Y";
			// 
			// m_tbxPnt3Y
			// 
			this.m_tbxPnt3Y.Location = new System.Drawing.Point(134, 36);
			this.m_tbxPnt3Y.Name = "m_tbxPnt3Y";
			this.m_tbxPnt3Y.Size = new System.Drawing.Size(80, 23);
			this.m_tbxPnt3Y.TabIndex = 3;
			this.m_tbxPnt3Y.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_LblPnt3Z
			// 
			this.m_LblPnt3Z.AutoSize = true;
			this.m_LblPnt3Z.Location = new System.Drawing.Point(222, 40);
			this.m_LblPnt3Z.Name = "m_LblPnt3Z";
			this.m_LblPnt3Z.Size = new System.Drawing.Size(14, 16);
			this.m_LblPnt3Z.TabIndex = 4;
			this.m_LblPnt3Z.Text = "Z";
			// 
			// m_tbxPnt3Z
			// 
			this.m_tbxPnt3Z.Location = new System.Drawing.Point(240, 36);
			this.m_tbxPnt3Z.Name = "m_tbxPnt3Z";
			this.m_tbxPnt3Z.Size = new System.Drawing.Size(80, 23);
			this.m_tbxPnt3Z.TabIndex = 5;
			this.m_tbxPnt3Z.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_btnDelPnt3
			// 
			this.m_btnDelPnt3.Kind = CommonUI.Controls.ButtonKind.Danger;
			this.m_btnDelPnt3.Location = new System.Drawing.Point(330, 36);
			this.m_btnDelPnt3.Name = "m_btnDelPnt3";
			this.m_btnDelPnt3.Size = new System.Drawing.Size(70, 23);
			this.m_btnDelPnt3.TabIndex = 6;
			this.m_btnDelPnt3.Text = "刪除";
			this.m_btnDelPnt3.Click += new System.EventHandler(this.m_btnDelPnt3_Click);
			// 
			// m_gbPnt3
			// 
			this.m_gbPnt3.Controls.Add(this.m_LblPnt3X);
			this.m_gbPnt3.Controls.Add(this.m_tbxPnt3X);
			this.m_gbPnt3.Controls.Add(this.m_LblPnt3Y);
			this.m_gbPnt3.Controls.Add(this.m_tbxPnt3Y);
			this.m_gbPnt3.Controls.Add(this.m_LblPnt3Z);
			this.m_gbPnt3.Controls.Add(this.m_tbxPnt3Z);
			this.m_gbPnt3.Controls.Add(this.m_btnDelPnt3);
			this.m_gbPnt3.Location = new System.Drawing.Point(12, 166);
			this.m_gbPnt3.Name = "m_gbPnt3";
			this.m_gbPnt3.Size = new System.Drawing.Size(410, 69);
			this.m_gbPnt3.TabIndex = 2;
			this.m_gbPnt3.Text = "參考點3";
			// 
			// m_btnDeletAll
			// 
			this.m_btnDeletAll.Kind = CommonUI.Controls.ButtonKind.Danger;
			this.m_btnDeletAll.Location = new System.Drawing.Point(12, 251);
			this.m_btnDeletAll.Name = "m_btnDeletAll";
			this.m_btnDeletAll.Size = new System.Drawing.Size(80, 28);
			this.m_btnDeletAll.TabIndex = 3;
			this.m_btnDeletAll.Text = "全部刪除";
			this.m_btnDeletAll.Click += new System.EventHandler(this.m_btnDeletAll_Click);
			// 
			// m_btnSure
			// 
			this.m_btnSure.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnSure.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnSure.Location = new System.Drawing.Point(342, 251);
			this.m_btnSure.Name = "m_btnSure";
			this.m_btnSure.Size = new System.Drawing.Size(80, 28);
			this.m_btnSure.TabIndex = 4;
			this.m_btnSure.Text = "確定";
			this.m_btnSure.Click += new System.EventHandler(this.m_btnSure_Click);
			// 
			// CalibrationREFDlg
			// 
			this.ClientSize = new System.Drawing.Size(434, 291);
			this.Controls.Add(this.m_gbPnt1);
			this.Controls.Add(this.m_gbPnt2);
			this.Controls.Add(this.m_gbPnt3);
			this.Controls.Add(this.m_btnDeletAll);
			this.Controls.Add(this.m_btnSure);
			this.Name = "CalibrationREFDlg";
			this.Text = "三點校正";
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

		private CommonUI.Controls.SectionPanel m_gbPnt1;
		private System.Windows.Forms.Label m_LblPnt1X;
		private System.Windows.Forms.Label m_tbxPnt1X;
		private System.Windows.Forms.Label m_LblPnt1Y;
		private System.Windows.Forms.Label m_tbxPnt1Y;
		private System.Windows.Forms.Label m_LblPnt1Z;
		private System.Windows.Forms.Label m_tbxPnt1Z;
		private CommonUI.Controls.ThemedButton m_btnDelPnt1;
		private CommonUI.Controls.SectionPanel m_gbPnt2;
		private System.Windows.Forms.Label m_LblPnt2X;
		private System.Windows.Forms.Label m_tbxPnt2X;
		private System.Windows.Forms.Label m_LblPnt2Y;
		private System.Windows.Forms.Label m_tbxPnt2Y;
		private System.Windows.Forms.Label m_LblPnt2Z;
		private System.Windows.Forms.Label m_tbxPnt2Z;
		private CommonUI.Controls.ThemedButton m_btnDelPnt2;
		private CommonUI.Controls.SectionPanel m_gbPnt3;
		private System.Windows.Forms.Label m_LblPnt3X;
		private System.Windows.Forms.Label m_tbxPnt3X;
		private System.Windows.Forms.Label m_LblPnt3Y;
		private System.Windows.Forms.Label m_tbxPnt3Y;
		private System.Windows.Forms.Label m_LblPnt3Z;
		private System.Windows.Forms.Label m_tbxPnt3Z;
		private CommonUI.Controls.ThemedButton m_btnDelPnt3;
		private CommonUI.Controls.ThemedButton m_btnDeletAll;
		private CommonUI.Controls.ThemedButton m_btnSure;
	}
}
