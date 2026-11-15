namespace MyCAM.Editor.Dialog
{
	partial class OverCutDlg
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
			this.m_secParam = new CommonUI.Controls.SectionPanel();
			this.m_lblOverCutLength = new System.Windows.Forms.Label();
			this.m_tbxOverCutLength = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_OverCutUnit = new CommonUI.Controls.ThemedLabel();
			this.m_btnSure = new CommonUI.Controls.ThemedButton();
			this.m_secParam.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxOverCutLength)).BeginInit();
			this.SuspendLayout();
			//
			// m_secParam
			//
			this.m_secParam.Controls.Add(this.m_lblOverCutLength);
			this.m_secParam.Controls.Add(this.m_tbxOverCutLength);
			this.m_secParam.Controls.Add(this.m_OverCutUnit);
			this.m_secParam.Location = new System.Drawing.Point(12, 12);
			this.m_secParam.Name = "m_secParam";
			this.m_secParam.Size = new System.Drawing.Size(296, 43);
			this.m_secParam.TabIndex = 0;
			//
			// m_lblOverCutLength
			//
			this.m_lblOverCutLength.AutoSize = true;
			this.m_lblOverCutLength.Location = new System.Drawing.Point(10, 14);
			this.m_lblOverCutLength.Name = "m_lblOverCutLength";
			this.m_lblOverCutLength.Size = new System.Drawing.Size(59, 16);
			this.m_lblOverCutLength.TabIndex = 0;
			this.m_lblOverCutLength.Text = "過切長度:";
			//
			// m_tbxOverCutLength
			//
			this.m_tbxOverCutLength.DecimalPlaces = 3;
			this.m_tbxOverCutLength.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.m_tbxOverCutLength.Location = new System.Drawing.Point(100, 10);
			this.m_tbxOverCutLength.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_tbxOverCutLength.Name = "m_tbxOverCutLength";
			this.m_tbxOverCutLength.Size = new System.Drawing.Size(150, 23);
			this.m_tbxOverCutLength.TabIndex = 1;
			this.m_tbxOverCutLength.DebouncedValueChanged += new System.EventHandler(this.m_tbxOverCutLength_DebouncedValueChanged);
			//
			// m_OverCutUnit
			//
			this.m_OverCutUnit.AutoSize = true;
			this.m_OverCutUnit.Location = new System.Drawing.Point(256, 14);
			this.m_OverCutUnit.Name = "m_OverCutUnit";
			this.m_OverCutUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_OverCutUnit.Size = new System.Drawing.Size(30, 16);
			this.m_OverCutUnit.TabIndex = 2;
			this.m_OverCutUnit.Text = "mm";
			//
			// m_btnSure
			//
			this.m_btnSure.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnSure.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnSure.Location = new System.Drawing.Point(228, 71);
			this.m_btnSure.Name = "m_btnSure";
			this.m_btnSure.Size = new System.Drawing.Size(80, 28);
			this.m_btnSure.TabIndex = 1;
			this.m_btnSure.Text = "確定";
			this.m_btnSure.Click += new System.EventHandler(this.m_btnSure_Click);
			//
			// OverCutDlg
			//
			this.ClientSize = new System.Drawing.Size(320, 111);
			this.Controls.Add(this.m_secParam);
			this.Controls.Add(this.m_btnSure);
			this.Name = "OverCutDlg";
			this.Text = "過切";
			this.m_secParam.ResumeLayout(false);
			this.m_secParam.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxOverCutLength)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_secParam;
		private System.Windows.Forms.Label m_lblOverCutLength;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxOverCutLength;
		private CommonUI.Controls.ThemedLabel m_OverCutUnit;
		private CommonUI.Controls.ThemedButton m_btnSure;
	}
}
