namespace MyCAM.Editor.Dialog
{
	partial class StartPointDlg
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
			this.m_pnlStartPoint = new CommonUI.Controls.SectionPanel();
			this.m_rbtnManualSelect = new System.Windows.Forms.RadioButton();
			this.m_rbtnAutoSelect = new System.Windows.Forms.RadioButton();
			this.m_pnlAutoSelect = new System.Windows.Forms.Panel();
			this.m_clstStrategy = new System.Windows.Forms.CheckedListBox();
			this.m_lblTolerance = new System.Windows.Forms.Label();
			this.m_tbxTolernece = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_btnIncreasePriority = new CommonUI.Controls.ThemedButton();
			this.m_btnDecreasePriority = new CommonUI.Controls.ThemedButton();
			this.m_btnApply = new CommonUI.Controls.ThemedButton();
			this.m_pnlStartPoint.SuspendLayout();
			this.m_pnlAutoSelect.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxTolernece)).BeginInit();
			this.SuspendLayout();
			//
			// m_pnlStartPoint
			//
			this.m_pnlStartPoint.Controls.Add(this.m_rbtnManualSelect);
			this.m_pnlStartPoint.Controls.Add(this.m_rbtnAutoSelect);
			this.m_pnlStartPoint.Controls.Add(this.m_pnlAutoSelect);
			this.m_pnlStartPoint.Location = new System.Drawing.Point(12, 12);
			this.m_pnlStartPoint.Name = "m_pnlStartPoint";
			this.m_pnlStartPoint.Size = new System.Drawing.Size(356, 223);
			this.m_pnlStartPoint.TabIndex = 0;
			//
			// m_rbtnManualSelect
			//
			this.m_rbtnManualSelect.AutoSize = true;
			this.m_rbtnManualSelect.Location = new System.Drawing.Point(10, 12);
			this.m_rbtnManualSelect.Name = "m_rbtnManualSelect";
			this.m_rbtnManualSelect.Size = new System.Drawing.Size(73, 20);
			this.m_rbtnManualSelect.TabIndex = 0;
			this.m_rbtnManualSelect.TabStop = true;
			this.m_rbtnManualSelect.Text = "手動選取";
			this.m_rbtnManualSelect.CheckedChanged += new System.EventHandler(this.m_rbtnManualSelect_CheckedChanged);
			//
			// m_rbtnAutoSelect
			//
			this.m_rbtnAutoSelect.AutoSize = true;
			this.m_rbtnAutoSelect.Location = new System.Drawing.Point(110, 12);
			this.m_rbtnAutoSelect.Name = "m_rbtnAutoSelect";
			this.m_rbtnAutoSelect.Size = new System.Drawing.Size(97, 20);
			this.m_rbtnAutoSelect.TabIndex = 1;
			this.m_rbtnAutoSelect.TabStop = true;
			this.m_rbtnAutoSelect.Text = "自動批量選取";
			this.m_rbtnAutoSelect.CheckedChanged += new System.EventHandler(this.m_rbtnAutoSelect_CheckedChanged);
			//
			// m_pnlAutoSelect
			//
			this.m_pnlAutoSelect.Controls.Add(this.m_clstStrategy);
			this.m_pnlAutoSelect.Controls.Add(this.m_lblTolerance);
			this.m_pnlAutoSelect.Controls.Add(this.m_tbxTolernece);
			this.m_pnlAutoSelect.Controls.Add(this.m_btnIncreasePriority);
			this.m_pnlAutoSelect.Controls.Add(this.m_btnDecreasePriority);
			this.m_pnlAutoSelect.Controls.Add(this.m_btnApply);
			this.m_pnlAutoSelect.Location = new System.Drawing.Point(10, 40);
			this.m_pnlAutoSelect.Name = "m_pnlAutoSelect";
			this.m_pnlAutoSelect.Size = new System.Drawing.Size(336, 173);
			this.m_pnlAutoSelect.TabIndex = 2;
			//
			// m_clstStrategy
			//
			this.m_clstStrategy.FormattingEnabled = true;
			this.m_clstStrategy.Location = new System.Drawing.Point(0, 0);
			this.m_clstStrategy.Name = "m_clstStrategy";
			this.m_clstStrategy.Size = new System.Drawing.Size(200, 140);
			this.m_clstStrategy.TabIndex = 0;
			this.m_clstStrategy.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.m_clstStrategy_ItemCheck);
			this.m_clstStrategy.SelectedIndexChanged += new System.EventHandler(this.m_clstStrategy_SelectedIndexChanged);
			this.m_clstStrategy.Format += new System.Windows.Forms.ListControlConvertEventHandler(this.m_clstStrategy_Format);
			//
			// m_lblTolerance
			//
			this.m_lblTolerance.AutoSize = true;
			this.m_lblTolerance.Location = new System.Drawing.Point(212, 4);
			this.m_lblTolerance.Name = "m_lblTolerance";
			this.m_lblTolerance.Size = new System.Drawing.Size(55, 16);
			this.m_lblTolerance.TabIndex = 1;
			this.m_lblTolerance.Text = "相等精度";
			//
			// m_tbxTolernece
			//
			this.m_tbxTolernece.DecimalPlaces = 3;
			this.m_tbxTolernece.Location = new System.Drawing.Point(212, 30);
			this.m_tbxTolernece.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_tbxTolernece.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_tbxTolernece.Name = "m_tbxTolernece";
			this.m_tbxTolernece.RestrictInvalidCharacters = false;
			this.m_tbxTolernece.Size = new System.Drawing.Size(124, 23);
			this.m_tbxTolernece.TabIndex = 2;
			this.m_tbxTolernece.DebouncedValueChanged += new System.EventHandler(this.m_tbxTolerance_DebouncedValueChanged);
			//
			// m_btnIncreasePriority
			//
			this.m_btnIncreasePriority.Location = new System.Drawing.Point(212, 60);
			this.m_btnIncreasePriority.Name = "m_btnIncreasePriority";
			this.m_btnIncreasePriority.Size = new System.Drawing.Size(124, 23);
			this.m_btnIncreasePriority.TabIndex = 3;
			this.m_btnIncreasePriority.Text = "增加優先級";
			this.m_btnIncreasePriority.Click += new System.EventHandler(this.m_btnIncreasePriority_Click);
			//
			// m_btnDecreasePriority
			//
			this.m_btnDecreasePriority.Location = new System.Drawing.Point(212, 90);
			this.m_btnDecreasePriority.Name = "m_btnDecreasePriority";
			this.m_btnDecreasePriority.Size = new System.Drawing.Size(124, 23);
			this.m_btnDecreasePriority.TabIndex = 4;
			this.m_btnDecreasePriority.Text = "減少優先級";
			this.m_btnDecreasePriority.Click += new System.EventHandler(this.m_btnDecreasePriority_Click);
			//
			// m_btnApply
			//
			this.m_btnApply.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnApply.Location = new System.Drawing.Point(0, 150);
			this.m_btnApply.Name = "m_btnApply";
			this.m_btnApply.Size = new System.Drawing.Size(80, 23);
			this.m_btnApply.TabIndex = 5;
			this.m_btnApply.Text = "應用";
			this.m_btnApply.Click += new System.EventHandler(this.m_btnApply_Click);
			//
			// StartPointDlg
			//
			this.ClientSize = new System.Drawing.Size(380, 247);
			this.Controls.Add(this.m_pnlStartPoint);
			this.Name = "StartPointDlg";
			this.Text = "起點";
			this.m_pnlStartPoint.ResumeLayout(false);
			this.m_pnlStartPoint.PerformLayout();
			this.m_pnlAutoSelect.ResumeLayout(false);
			this.m_pnlAutoSelect.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxTolernece)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlStartPoint;
		private System.Windows.Forms.RadioButton m_rbtnManualSelect;
		private System.Windows.Forms.RadioButton m_rbtnAutoSelect;
		private System.Windows.Forms.Panel m_pnlAutoSelect;
		private System.Windows.Forms.CheckedListBox m_clstStrategy;
		private System.Windows.Forms.Label m_lblTolerance;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxTolernece;
		private CommonUI.Controls.ThemedButton m_btnIncreasePriority;
		private CommonUI.Controls.ThemedButton m_btnDecreasePriority;
		private CommonUI.Controls.ThemedButton m_btnApply;
	}
}
