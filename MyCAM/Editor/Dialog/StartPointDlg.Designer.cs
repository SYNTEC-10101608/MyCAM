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
			this.m_flpnlSelectMode = new System.Windows.Forms.FlowLayoutPanel();
			this.m_rbtnManualSelect = new System.Windows.Forms.RadioButton();
			this.m_rbtnAutoSelect = new System.Windows.Forms.RadioButton();
			this.m_btnApply = new System.Windows.Forms.Button();
			this.m_clstStrategy = new System.Windows.Forms.CheckedListBox();
			this.m_btnIncreasePriority = new System.Windows.Forms.Button();
			this.m_btnDecreasePriority = new System.Windows.Forms.Button();
			this.m_tbxTolernece = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_pnlStartPoint = new System.Windows.Forms.TableLayoutPanel();
			this.m_pnlAutoSelect = new System.Windows.Forms.TableLayoutPanel();
			this.m_pnlPriority = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblTolerance = new System.Windows.Forms.Label();
			this.m_flpnlSelectMode.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxTolernece)).BeginInit();
			this.m_pnlStartPoint.SuspendLayout();
			this.m_pnlAutoSelect.SuspendLayout();
			this.m_pnlPriority.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_flpnlSelectMode
			// 
			this.m_flpnlSelectMode.Controls.Add(this.m_rbtnManualSelect);
			this.m_flpnlSelectMode.Controls.Add(this.m_rbtnAutoSelect);
			this.m_flpnlSelectMode.Location = new System.Drawing.Point(3, 3);
			this.m_flpnlSelectMode.Name = "m_flpnlSelectMode";
			this.m_flpnlSelectMode.Size = new System.Drawing.Size(369, 34);
			this.m_flpnlSelectMode.TabIndex = 11;
			// 
			// m_rbtnManualSelect
			// 
			this.m_rbtnManualSelect.AutoSize = true;
			this.m_rbtnManualSelect.Location = new System.Drawing.Point(3, 3);
			this.m_rbtnManualSelect.Name = "m_rbtnManualSelect";
			this.m_rbtnManualSelect.Size = new System.Drawing.Size(88, 19);
			this.m_rbtnManualSelect.TabIndex = 0;
			this.m_rbtnManualSelect.TabStop = true;
			this.m_rbtnManualSelect.Text = "手動選取";
			this.m_rbtnManualSelect.UseVisualStyleBackColor = true;
			this.m_rbtnManualSelect.CheckedChanged += new System.EventHandler(this.m_rbtnManualSelect_CheckedChanged);
			// 
			// m_rbtnAutoSelect
			// 
			this.m_rbtnAutoSelect.AutoSize = true;
			this.m_rbtnAutoSelect.Location = new System.Drawing.Point(97, 3);
			this.m_rbtnAutoSelect.Name = "m_rbtnAutoSelect";
			this.m_rbtnAutoSelect.Size = new System.Drawing.Size(118, 19);
			this.m_rbtnAutoSelect.TabIndex = 1;
			this.m_rbtnAutoSelect.TabStop = true;
			this.m_rbtnAutoSelect.Text = "自動批量選取";
			this.m_rbtnAutoSelect.UseVisualStyleBackColor = true;
			this.m_rbtnAutoSelect.CheckedChanged += new System.EventHandler(this.m_rbtnAutoSelect_CheckedChanged);
			// 
			// m_btnApply
			// 
			this.m_btnApply.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_btnApply.Location = new System.Drawing.Point(3, 180);
			this.m_btnApply.Name = "m_btnApply";
			this.m_btnApply.Size = new System.Drawing.Size(84, 33);
			this.m_btnApply.TabIndex = 10;
			this.m_btnApply.Text = "應用";
			this.m_btnApply.UseVisualStyleBackColor = true;
			this.m_btnApply.Click += new System.EventHandler(this.m_btnApply_Click);
			// 
			// m_clstStrategy
			// 
			this.m_clstStrategy.FormattingEnabled = true;
			this.m_clstStrategy.Location = new System.Drawing.Point(3, 3);
			this.m_clstStrategy.Name = "m_clstStrategy";
			this.m_clstStrategy.Size = new System.Drawing.Size(199, 164);
			this.m_clstStrategy.TabIndex = 5;
			this.m_clstStrategy.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.m_clstStrategy_ItemCheck);
			this.m_clstStrategy.SelectedIndexChanged += new System.EventHandler(this.m_clstStrategy_SelectedIndexChanged);
			this.m_clstStrategy.Format += new System.Windows.Forms.ListControlConvertEventHandler(this.m_clstStrategy_Format);
			// 
			// m_btnIncreasePriority
			// 
			this.m_btnIncreasePriority.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_btnIncreasePriority.Location = new System.Drawing.Point(3, 61);
			this.m_btnIncreasePriority.Name = "m_btnIncreasePriority";
			this.m_btnIncreasePriority.Size = new System.Drawing.Size(114, 33);
			this.m_btnIncreasePriority.TabIndex = 12;
			this.m_btnIncreasePriority.Text = "增加優先級";
			this.m_btnIncreasePriority.UseVisualStyleBackColor = true;
			this.m_btnIncreasePriority.Click += new System.EventHandler(this.m_btnIncreasePriority_Click);
			// 
			// m_btnDecreasePriority
			// 
			this.m_btnDecreasePriority.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_btnDecreasePriority.Location = new System.Drawing.Point(3, 110);
			this.m_btnDecreasePriority.Name = "m_btnDecreasePriority";
			this.m_btnDecreasePriority.Size = new System.Drawing.Size(114, 33);
			this.m_btnDecreasePriority.TabIndex = 13;
			this.m_btnDecreasePriority.Text = "減少優先級";
			this.m_btnDecreasePriority.UseVisualStyleBackColor = true;
			this.m_btnDecreasePriority.Click += new System.EventHandler(this.m_btnDecreasePriority_Click);
			// 
			// m_tbxTolernece
			// 
			this.m_tbxTolernece.DecimalPlaces = 3;
			this.m_tbxTolernece.Location = new System.Drawing.Point(3, 25);
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
			this.m_tbxTolernece.Size = new System.Drawing.Size(105, 25);
			this.m_tbxTolernece.TabIndex = 15;
			this.m_tbxTolernece.DebouncedValueChanged += new System.EventHandler(this.m_tbxTolerance_DebouncedValueChanged);
			// 
			// m_pnlStartPoint
			// 
			this.m_pnlStartPoint.ColumnCount = 1;
			this.m_pnlStartPoint.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_pnlStartPoint.Controls.Add(this.m_flpnlSelectMode, 0, 0);
			this.m_pnlStartPoint.Controls.Add(this.m_pnlAutoSelect, 0, 1);
			this.m_pnlStartPoint.Location = new System.Drawing.Point(12, 12);
			this.m_pnlStartPoint.Name = "m_pnlStartPoint";
			this.m_pnlStartPoint.RowCount = 2;
			this.m_pnlStartPoint.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.83871F));
			this.m_pnlStartPoint.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 85.16129F));
			this.m_pnlStartPoint.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_pnlStartPoint.Size = new System.Drawing.Size(378, 273);
			this.m_pnlStartPoint.TabIndex = 17;
			// 
			// m_pnlAutoSelect
			// 
			this.m_pnlAutoSelect.ColumnCount = 2;
			this.m_pnlAutoSelect.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
			this.m_pnlAutoSelect.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
			this.m_pnlAutoSelect.Controls.Add(this.m_clstStrategy, 0, 0);
			this.m_pnlAutoSelect.Controls.Add(this.m_btnApply, 0, 1);
			this.m_pnlAutoSelect.Controls.Add(this.m_pnlPriority, 1, 0);
			this.m_pnlAutoSelect.Location = new System.Drawing.Point(3, 43);
			this.m_pnlAutoSelect.Name = "m_pnlAutoSelect";
			this.m_pnlAutoSelect.RowCount = 2;
			this.m_pnlAutoSelect.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_pnlAutoSelect.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this.m_pnlAutoSelect.Size = new System.Drawing.Size(369, 219);
			this.m_pnlAutoSelect.TabIndex = 12;
			// 
			// m_pnlPriority
			// 
			this.m_pnlPriority.ColumnCount = 1;
			this.m_pnlPriority.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_pnlPriority.Controls.Add(this.m_lblTolerance, 0, 0);
			this.m_pnlPriority.Controls.Add(this.m_btnDecreasePriority, 0, 3);
			this.m_pnlPriority.Controls.Add(this.m_tbxTolernece, 0, 1);
			this.m_pnlPriority.Controls.Add(this.m_btnIncreasePriority, 0, 2);
			this.m_pnlPriority.Location = new System.Drawing.Point(224, 3);
			this.m_pnlPriority.Name = "m_pnlPriority";
			this.m_pnlPriority.RowCount = 4;
			this.m_pnlPriority.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
			this.m_pnlPriority.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.m_pnlPriority.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_pnlPriority.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_pnlPriority.Size = new System.Drawing.Size(139, 151);
			this.m_pnlPriority.TabIndex = 18;
			// 
			// m_lblTolerance
			// 
			this.m_lblTolerance.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblTolerance.AutoSize = true;
			this.m_lblTolerance.Location = new System.Drawing.Point(3, 3);
			this.m_lblTolerance.Name = "m_lblTolerance";
			this.m_lblTolerance.Size = new System.Drawing.Size(67, 15);
			this.m_lblTolerance.TabIndex = 16;
			this.m_lblTolerance.Text = "相等精度";
			// 
			// StartPointDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(395, 293);
			this.Controls.Add(this.m_pnlStartPoint);
			this.Name = "StartPointDlg";
			this.ShowIcon = false;
			this.Text = "起點";
			this.m_flpnlSelectMode.ResumeLayout(false);
			this.m_flpnlSelectMode.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxTolernece)).EndInit();
			this.m_pnlStartPoint.ResumeLayout(false);
			this.m_pnlAutoSelect.ResumeLayout(false);
			this.m_pnlPriority.ResumeLayout(false);
			this.m_pnlPriority.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RadioButton m_rbtnManualSelect;
		private System.Windows.Forms.RadioButton m_rbtnAutoSelect;
		private System.Windows.Forms.Button m_btnApply;
		private System.Windows.Forms.FlowLayoutPanel m_flpnlSelectMode;
		private System.Windows.Forms.CheckedListBox m_clstStrategy;
		private System.Windows.Forms.Button m_btnIncreasePriority;
		private System.Windows.Forms.Button m_btnDecreasePriority;
		private MyCAM.Editor.DebouncedNumericUpDown m_tbxTolernece;
		private System.Windows.Forms.TableLayoutPanel m_pnlStartPoint;
		private System.Windows.Forms.TableLayoutPanel m_pnlAutoSelect;
		private System.Windows.Forms.TableLayoutPanel m_pnlPriority;
		private System.Windows.Forms.Label m_lblTolerance;
	}
}
