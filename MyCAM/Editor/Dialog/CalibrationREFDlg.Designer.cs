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
			this.m_pnlPnt1 = new CommonUI.Controls.SectionPanel();
			this.m_pnlPnt2 = new CommonUI.Controls.SectionPanel();
			this.m_pnlPnt3 = new CommonUI.Controls.SectionPanel();
			this.m_lblPnt1X = new System.Windows.Forms.Label();
			this.m_lblPnt1XValue = new System.Windows.Forms.Label();
			this.m_lblPnt1Y = new System.Windows.Forms.Label();
			this.m_lblPnt1YValue = new System.Windows.Forms.Label();
			this.m_lblPnt1Z = new System.Windows.Forms.Label();
			this.m_lblPnt1ZValue = new System.Windows.Forms.Label();
			this.m_btnDelPnt1 = new CommonUI.Controls.ThemedButton();
			this.m_lblPnt2X = new System.Windows.Forms.Label();
			this.m_lblPnt2XValue = new System.Windows.Forms.Label();
			this.m_lblPnt2Y = new System.Windows.Forms.Label();
			this.m_lblPnt2YValue = new System.Windows.Forms.Label();
			this.m_lblPnt2Z = new System.Windows.Forms.Label();
			this.m_lblPnt2ZValue = new System.Windows.Forms.Label();
			this.m_btnDelPnt2 = new CommonUI.Controls.ThemedButton();
			this.m_lblPnt3X = new System.Windows.Forms.Label();
			this.m_lblPnt3XValue = new System.Windows.Forms.Label();
			this.m_lblPnt3Y = new System.Windows.Forms.Label();
			this.m_lblPnt3YValue = new System.Windows.Forms.Label();
			this.m_lblPnt3Z = new System.Windows.Forms.Label();
			this.m_lblPnt3ZValue = new System.Windows.Forms.Label();
			this.m_btnDelPnt3 = new CommonUI.Controls.ThemedButton();
			this.m_btnDeletAll = new CommonUI.Controls.ThemedButton();
			this.m_btnSure = new CommonUI.Controls.ThemedButton();
			this.m_pnlPnt1.SuspendLayout();
			this.m_pnlPnt2.SuspendLayout();
			this.m_pnlPnt3.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_lblPnt1X
			// 
			this.m_lblPnt1X.AutoSize = true;
			this.m_lblPnt1X.Location = new System.Drawing.Point(10, 40);
			this.m_lblPnt1X.Name = "m_lblPnt1X";
			this.m_lblPnt1X.Size = new System.Drawing.Size(14, 16);
			this.m_lblPnt1X.TabIndex = 0;
			this.m_lblPnt1X.Text = "X";
			// 
			// m_lblPnt1XValue
			// 
			this.m_lblPnt1XValue.Location = new System.Drawing.Point(28, 36);
			this.m_lblPnt1XValue.Name = "m_lblPnt1XValue";
			this.m_lblPnt1XValue.Size = new System.Drawing.Size(80, 23);
			this.m_lblPnt1XValue.TabIndex = 1;
			this.m_lblPnt1XValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_lblPnt1Y
			// 
			this.m_lblPnt1Y.AutoSize = true;
			this.m_lblPnt1Y.Location = new System.Drawing.Point(116, 40);
			this.m_lblPnt1Y.Name = "m_lblPnt1Y";
			this.m_lblPnt1Y.Size = new System.Drawing.Size(14, 16);
			this.m_lblPnt1Y.TabIndex = 2;
			this.m_lblPnt1Y.Text = "Y";
			// 
			// m_lblPnt1YValue
			// 
			this.m_lblPnt1YValue.Location = new System.Drawing.Point(134, 36);
			this.m_lblPnt1YValue.Name = "m_lblPnt1YValue";
			this.m_lblPnt1YValue.Size = new System.Drawing.Size(80, 23);
			this.m_lblPnt1YValue.TabIndex = 3;
			this.m_lblPnt1YValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_lblPnt1Z
			// 
			this.m_lblPnt1Z.AutoSize = true;
			this.m_lblPnt1Z.Location = new System.Drawing.Point(222, 40);
			this.m_lblPnt1Z.Name = "m_lblPnt1Z";
			this.m_lblPnt1Z.Size = new System.Drawing.Size(14, 16);
			this.m_lblPnt1Z.TabIndex = 4;
			this.m_lblPnt1Z.Text = "Z";
			// 
			// m_lblPnt1ZValue
			// 
			this.m_lblPnt1ZValue.Location = new System.Drawing.Point(240, 36);
			this.m_lblPnt1ZValue.Name = "m_lblPnt1ZValue";
			this.m_lblPnt1ZValue.Size = new System.Drawing.Size(80, 23);
			this.m_lblPnt1ZValue.TabIndex = 5;
			this.m_lblPnt1ZValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
			// m_pnlPnt1
			// 
			this.m_pnlPnt1.Controls.Add(this.m_lblPnt1X);
			this.m_pnlPnt1.Controls.Add(this.m_lblPnt1XValue);
			this.m_pnlPnt1.Controls.Add(this.m_lblPnt1Y);
			this.m_pnlPnt1.Controls.Add(this.m_lblPnt1YValue);
			this.m_pnlPnt1.Controls.Add(this.m_lblPnt1Z);
			this.m_pnlPnt1.Controls.Add(this.m_lblPnt1ZValue);
			this.m_pnlPnt1.Controls.Add(this.m_btnDelPnt1);
			this.m_pnlPnt1.Location = new System.Drawing.Point(12, 12);
			this.m_pnlPnt1.Name = "m_pnlPnt1";
			this.m_pnlPnt1.Size = new System.Drawing.Size(410, 69);
			this.m_pnlPnt1.TabIndex = 0;
			this.m_pnlPnt1.Text = "參考點1";
			// 
			// m_lblPnt2X
			// 
			this.m_lblPnt2X.AutoSize = true;
			this.m_lblPnt2X.Location = new System.Drawing.Point(10, 40);
			this.m_lblPnt2X.Name = "m_lblPnt2X";
			this.m_lblPnt2X.Size = new System.Drawing.Size(14, 16);
			this.m_lblPnt2X.TabIndex = 0;
			this.m_lblPnt2X.Text = "X";
			// 
			// m_lblPnt2XValue
			// 
			this.m_lblPnt2XValue.Location = new System.Drawing.Point(28, 36);
			this.m_lblPnt2XValue.Name = "m_lblPnt2XValue";
			this.m_lblPnt2XValue.Size = new System.Drawing.Size(80, 23);
			this.m_lblPnt2XValue.TabIndex = 1;
			this.m_lblPnt2XValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_lblPnt2Y
			// 
			this.m_lblPnt2Y.AutoSize = true;
			this.m_lblPnt2Y.Location = new System.Drawing.Point(116, 40);
			this.m_lblPnt2Y.Name = "m_lblPnt2Y";
			this.m_lblPnt2Y.Size = new System.Drawing.Size(14, 16);
			this.m_lblPnt2Y.TabIndex = 2;
			this.m_lblPnt2Y.Text = "Y";
			// 
			// m_lblPnt2YValue
			// 
			this.m_lblPnt2YValue.Location = new System.Drawing.Point(134, 36);
			this.m_lblPnt2YValue.Name = "m_lblPnt2YValue";
			this.m_lblPnt2YValue.Size = new System.Drawing.Size(80, 23);
			this.m_lblPnt2YValue.TabIndex = 3;
			this.m_lblPnt2YValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_lblPnt2Z
			// 
			this.m_lblPnt2Z.AutoSize = true;
			this.m_lblPnt2Z.Location = new System.Drawing.Point(222, 40);
			this.m_lblPnt2Z.Name = "m_lblPnt2Z";
			this.m_lblPnt2Z.Size = new System.Drawing.Size(14, 16);
			this.m_lblPnt2Z.TabIndex = 4;
			this.m_lblPnt2Z.Text = "Z";
			// 
			// m_lblPnt2ZValue
			// 
			this.m_lblPnt2ZValue.Location = new System.Drawing.Point(240, 36);
			this.m_lblPnt2ZValue.Name = "m_lblPnt2ZValue";
			this.m_lblPnt2ZValue.Size = new System.Drawing.Size(80, 23);
			this.m_lblPnt2ZValue.TabIndex = 5;
			this.m_lblPnt2ZValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
			// m_pnlPnt2
			// 
			this.m_pnlPnt2.Controls.Add(this.m_lblPnt2X);
			this.m_pnlPnt2.Controls.Add(this.m_lblPnt2XValue);
			this.m_pnlPnt2.Controls.Add(this.m_lblPnt2Y);
			this.m_pnlPnt2.Controls.Add(this.m_lblPnt2YValue);
			this.m_pnlPnt2.Controls.Add(this.m_lblPnt2Z);
			this.m_pnlPnt2.Controls.Add(this.m_lblPnt2ZValue);
			this.m_pnlPnt2.Controls.Add(this.m_btnDelPnt2);
			this.m_pnlPnt2.Location = new System.Drawing.Point(12, 89);
			this.m_pnlPnt2.Name = "m_pnlPnt2";
			this.m_pnlPnt2.Size = new System.Drawing.Size(410, 69);
			this.m_pnlPnt2.TabIndex = 1;
			this.m_pnlPnt2.Text = "參考點2";
			// 
			// m_lblPnt3X
			// 
			this.m_lblPnt3X.AutoSize = true;
			this.m_lblPnt3X.Location = new System.Drawing.Point(10, 40);
			this.m_lblPnt3X.Name = "m_lblPnt3X";
			this.m_lblPnt3X.Size = new System.Drawing.Size(14, 16);
			this.m_lblPnt3X.TabIndex = 0;
			this.m_lblPnt3X.Text = "X";
			// 
			// m_lblPnt3XValue
			// 
			this.m_lblPnt3XValue.Location = new System.Drawing.Point(28, 36);
			this.m_lblPnt3XValue.Name = "m_lblPnt3XValue";
			this.m_lblPnt3XValue.Size = new System.Drawing.Size(80, 23);
			this.m_lblPnt3XValue.TabIndex = 1;
			this.m_lblPnt3XValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_lblPnt3Y
			// 
			this.m_lblPnt3Y.AutoSize = true;
			this.m_lblPnt3Y.Location = new System.Drawing.Point(116, 40);
			this.m_lblPnt3Y.Name = "m_lblPnt3Y";
			this.m_lblPnt3Y.Size = new System.Drawing.Size(14, 16);
			this.m_lblPnt3Y.TabIndex = 2;
			this.m_lblPnt3Y.Text = "Y";
			// 
			// m_lblPnt3YValue
			// 
			this.m_lblPnt3YValue.Location = new System.Drawing.Point(134, 36);
			this.m_lblPnt3YValue.Name = "m_lblPnt3YValue";
			this.m_lblPnt3YValue.Size = new System.Drawing.Size(80, 23);
			this.m_lblPnt3YValue.TabIndex = 3;
			this.m_lblPnt3YValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_lblPnt3Z
			// 
			this.m_lblPnt3Z.AutoSize = true;
			this.m_lblPnt3Z.Location = new System.Drawing.Point(222, 40);
			this.m_lblPnt3Z.Name = "m_lblPnt3Z";
			this.m_lblPnt3Z.Size = new System.Drawing.Size(14, 16);
			this.m_lblPnt3Z.TabIndex = 4;
			this.m_lblPnt3Z.Text = "Z";
			// 
			// m_lblPnt3ZValue
			// 
			this.m_lblPnt3ZValue.Location = new System.Drawing.Point(240, 36);
			this.m_lblPnt3ZValue.Name = "m_lblPnt3ZValue";
			this.m_lblPnt3ZValue.Size = new System.Drawing.Size(80, 23);
			this.m_lblPnt3ZValue.TabIndex = 5;
			this.m_lblPnt3ZValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
			// m_pnlPnt3
			// 
			this.m_pnlPnt3.Controls.Add(this.m_lblPnt3X);
			this.m_pnlPnt3.Controls.Add(this.m_lblPnt3XValue);
			this.m_pnlPnt3.Controls.Add(this.m_lblPnt3Y);
			this.m_pnlPnt3.Controls.Add(this.m_lblPnt3YValue);
			this.m_pnlPnt3.Controls.Add(this.m_lblPnt3Z);
			this.m_pnlPnt3.Controls.Add(this.m_lblPnt3ZValue);
			this.m_pnlPnt3.Controls.Add(this.m_btnDelPnt3);
			this.m_pnlPnt3.Location = new System.Drawing.Point(12, 166);
			this.m_pnlPnt3.Name = "m_pnlPnt3";
			this.m_pnlPnt3.Size = new System.Drawing.Size(410, 69);
			this.m_pnlPnt3.TabIndex = 2;
			this.m_pnlPnt3.Text = "參考點3";
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
			this.Controls.Add(this.m_pnlPnt1);
			this.Controls.Add(this.m_pnlPnt2);
			this.Controls.Add(this.m_pnlPnt3);
			this.Controls.Add(this.m_btnDeletAll);
			this.Controls.Add(this.m_btnSure);
			this.Name = "CalibrationREFDlg";
			this.Text = "三點校正";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CalibrationREFDlg_KeyDown);
			this.m_pnlPnt1.ResumeLayout(false);
			this.m_pnlPnt1.PerformLayout();
			this.m_pnlPnt2.ResumeLayout(false);
			this.m_pnlPnt2.PerformLayout();
			this.m_pnlPnt3.ResumeLayout(false);
			this.m_pnlPnt3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlPnt1;
		private System.Windows.Forms.Label m_lblPnt1X;
		private System.Windows.Forms.Label m_lblPnt1XValue;
		private System.Windows.Forms.Label m_lblPnt1Y;
		private System.Windows.Forms.Label m_lblPnt1YValue;
		private System.Windows.Forms.Label m_lblPnt1Z;
		private System.Windows.Forms.Label m_lblPnt1ZValue;
		private CommonUI.Controls.ThemedButton m_btnDelPnt1;
		private CommonUI.Controls.SectionPanel m_pnlPnt2;
		private System.Windows.Forms.Label m_lblPnt2X;
		private System.Windows.Forms.Label m_lblPnt2XValue;
		private System.Windows.Forms.Label m_lblPnt2Y;
		private System.Windows.Forms.Label m_lblPnt2YValue;
		private System.Windows.Forms.Label m_lblPnt2Z;
		private System.Windows.Forms.Label m_lblPnt2ZValue;
		private CommonUI.Controls.ThemedButton m_btnDelPnt2;
		private CommonUI.Controls.SectionPanel m_pnlPnt3;
		private System.Windows.Forms.Label m_lblPnt3X;
		private System.Windows.Forms.Label m_lblPnt3XValue;
		private System.Windows.Forms.Label m_lblPnt3Y;
		private System.Windows.Forms.Label m_lblPnt3YValue;
		private System.Windows.Forms.Label m_lblPnt3Z;
		private System.Windows.Forms.Label m_lblPnt3ZValue;
		private CommonUI.Controls.ThemedButton m_btnDelPnt3;
		private CommonUI.Controls.ThemedButton m_btnDeletAll;
		private CommonUI.Controls.ThemedButton m_btnSure;
	}
}
