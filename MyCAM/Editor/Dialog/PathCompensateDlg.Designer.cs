namespace MyCAM.Editor.Dialog
{
	partial class PathCompensateDlg
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
			this.m_pnlCompemsate = new CommonUI.Controls.SectionPanel();
			this.m_lblCompensate = new System.Windows.Forms.Label();
			this.m_NumUpDownCompensate = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblTotalCompensate = new System.Windows.Forms.Label();
			this.m_lblTotalCompensateValue = new System.Windows.Forms.Label();
			this.m_btnReset = new CommonUI.Controls.ThemedButton();
			this.m_btnClear = new CommonUI.Controls.ThemedButton();
			this.m_btnConfirm = new CommonUI.Controls.ThemedButton();
			this.m_pnlCompemsate.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumUpDownCompensate)).BeginInit();
			this.SuspendLayout();
			//
			// m_pnlCompemsate
			//
			this.m_pnlCompemsate.Controls.Add(this.m_lblCompensate);
			this.m_pnlCompemsate.Controls.Add(this.m_NumUpDownCompensate);
			this.m_pnlCompemsate.Controls.Add(this.m_lblTotalCompensate);
			this.m_pnlCompemsate.Controls.Add(this.m_lblTotalCompensateValue);
			this.m_pnlCompemsate.Location = new System.Drawing.Point(12, 12);
			this.m_pnlCompemsate.Name = "m_pnlCompemsate";
			this.m_pnlCompemsate.Size = new System.Drawing.Size(296, 73);
			this.m_pnlCompemsate.TabIndex = 0;
			//
			// m_lblCompensate
			//
			this.m_lblCompensate.AutoSize = true;
			this.m_lblCompensate.Location = new System.Drawing.Point(10, 14);
			this.m_lblCompensate.Name = "m_lblCompensate";
			this.m_lblCompensate.Size = new System.Drawing.Size(55, 16);
			this.m_lblCompensate.TabIndex = 0;
			this.m_lblCompensate.Text = "補償距離";
			//
			// m_NumUpDownCompensate
			//
			this.m_NumUpDownCompensate.DecimalPlaces = 3;
			this.m_NumUpDownCompensate.Location = new System.Drawing.Point(100, 10);
			this.m_NumUpDownCompensate.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_NumUpDownCompensate.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_NumUpDownCompensate.Name = "m_NumUpDownCompensate";
			this.m_NumUpDownCompensate.RestrictInvalidCharacters = false;
			this.m_NumUpDownCompensate.Size = new System.Drawing.Size(150, 23);
			this.m_NumUpDownCompensate.TabIndex = 1;
			this.m_NumUpDownCompensate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.m_NumUpDownCompensate.DebouncedValueChanged += new System.EventHandler(this.m_NumUpDownCompensate_DebouncedValueChanged);
			//
			// m_lblTotalCompensate
			//
			this.m_lblTotalCompensate.AutoSize = true;
			this.m_lblTotalCompensate.Location = new System.Drawing.Point(10, 44);
			this.m_lblTotalCompensate.Name = "m_lblTotalCompensate";
			this.m_lblTotalCompensate.Size = new System.Drawing.Size(43, 16);
			this.m_lblTotalCompensate.TabIndex = 2;
			this.m_lblTotalCompensate.Text = "總補償";
			//
			// m_lblTotalCompensateValue
			//
			this.m_lblTotalCompensateValue.AutoSize = true;
			this.m_lblTotalCompensateValue.Location = new System.Drawing.Point(100, 44);
			this.m_lblTotalCompensateValue.Name = "m_lblTotalCompensateValue";
			this.m_lblTotalCompensateValue.Size = new System.Drawing.Size(38, 16);
			this.m_lblTotalCompensateValue.TabIndex = 3;
			this.m_lblTotalCompensateValue.Text = "0.000";
			//
			// m_btnReset
			//
			this.m_btnReset.CommonText = CommonUI.Controls.CommonButtonText.Reset;
			this.m_btnReset.Location = new System.Drawing.Point(12, 101);
			this.m_btnReset.Name = "m_btnReset";
			this.m_btnReset.Size = new System.Drawing.Size(80, 28);
			this.m_btnReset.TabIndex = 1;
			this.m_btnReset.Text = "重設";
			this.m_btnReset.Click += new System.EventHandler(this.m_btnReset_Click);
			//
			// m_btnClear
			//
			this.m_btnClear.Location = new System.Drawing.Point(100, 101);
			this.m_btnClear.Name = "m_btnClear";
			this.m_btnClear.Size = new System.Drawing.Size(80, 28);
			this.m_btnClear.TabIndex = 2;
			this.m_btnClear.Text = "清除";
			this.m_btnClear.Click += new System.EventHandler(this.m_btnClear_Click);
			//
			// m_btnConfirm
			//
			this.m_btnConfirm.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnConfirm.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnConfirm.Location = new System.Drawing.Point(228, 101);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(80, 28);
			this.m_btnConfirm.TabIndex = 3;
			this.m_btnConfirm.Text = "確定";
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			//
			// PathCompensateDlg
			//
			this.ClientSize = new System.Drawing.Size(320, 141);
			this.Controls.Add(this.m_pnlCompemsate);
			this.Controls.Add(this.m_btnReset);
			this.Controls.Add(this.m_btnClear);
			this.Controls.Add(this.m_btnConfirm);
			this.Name = "PathCompensateDlg";
			this.Text = "路徑補償";
			this.m_pnlCompemsate.ResumeLayout(false);
			this.m_pnlCompemsate.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumUpDownCompensate)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlCompemsate;
		private System.Windows.Forms.Label m_lblCompensate;
		private CommonUI.Controls.DebouncedNumericUpDown m_NumUpDownCompensate;
		private System.Windows.Forms.Label m_lblTotalCompensate;
		private System.Windows.Forms.Label m_lblTotalCompensateValue;
		private CommonUI.Controls.ThemedButton m_btnReset;
		private CommonUI.Controls.ThemedButton m_btnClear;
		private CommonUI.Controls.ThemedButton m_btnConfirm;
	}
}
