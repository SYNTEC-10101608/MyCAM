namespace MyCAM.Editor.Dialog
{
	partial class FlyCutDlg
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
			this.m_pnlFlyCut = new System.Windows.Forms.TableLayoutPanel();
			this.m_flpnlFlyCutType = new System.Windows.Forms.FlowLayoutPanel();
			this.m_lblFlyCut = new System.Windows.Forms.Label();
			this.m_rbtnCircularFlyCut = new System.Windows.Forms.RadioButton();
			this.m_gbxSetting = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.m_lblStartPosition = new System.Windows.Forms.Label();
			this.m_rbtnLeftTop = new System.Windows.Forms.RadioButton();
			this.m_rbtnLeftBottom = new System.Windows.Forms.RadioButton();
			this.m_rbtnRightTop = new System.Windows.Forms.RadioButton();
			this.m_rbtnRightBottom = new System.Windows.Forms.RadioButton();
			this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.m_lblDirection = new System.Windows.Forms.Label();
			this.m_rbtnVertical = new System.Windows.Forms.RadioButton();
			this.m_rbtnHorizontal = new System.Windows.Forms.RadioButton();
			this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
			this.m_btnAdd = new System.Windows.Forms.Button();
			this.m_btnRemove = new System.Windows.Forms.Button();
			this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
			this.m_lblGroupWidth = new System.Windows.Forms.Label();
			this.m_nudGroupWidth = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_pnlFlyCut.SuspendLayout();
			this.m_flpnlFlyCutType.SuspendLayout();
			this.m_gbxSetting.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel2.SuspendLayout();
			this.flowLayoutPanel3.SuspendLayout();
			this.flowLayoutPanel4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_nudGroupWidth)).BeginInit();
			this.SuspendLayout();
			//
			// m_pnlFlyCut
			//
			this.m_pnlFlyCut.ColumnCount = 1;
			this.m_pnlFlyCut.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_pnlFlyCut.Controls.Add(this.m_flpnlFlyCutType, 0, 0);
			this.m_pnlFlyCut.Controls.Add(this.m_gbxSetting, 0, 1);
			this.m_pnlFlyCut.Location = new System.Drawing.Point(12, 12);
			this.m_pnlFlyCut.Name = "m_pnlFlyCut";
			this.m_pnlFlyCut.RowCount = 2;
			this.m_pnlFlyCut.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.m_pnlFlyCut.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
			this.m_pnlFlyCut.Size = new System.Drawing.Size(512, 230);
			this.m_pnlFlyCut.TabIndex = 3;
			//
			// m_flpnlFlyCutType
			//
			this.m_flpnlFlyCutType.Controls.Add(this.m_lblFlyCut);
			this.m_flpnlFlyCutType.Controls.Add(this.m_rbtnCircularFlyCut);
			this.m_flpnlFlyCutType.Location = new System.Drawing.Point(3, 3);
			this.m_flpnlFlyCutType.Name = "m_flpnlFlyCutType";
			this.m_flpnlFlyCutType.Size = new System.Drawing.Size(182, 28);
			this.m_flpnlFlyCutType.TabIndex = 0;
			//
			// m_lblFlyCut
			//
			this.m_lblFlyCut.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblFlyCut.AutoSize = true;
			this.m_lblFlyCut.Location = new System.Drawing.Point(3, 5);
			this.m_lblFlyCut.Name = "m_lblFlyCut";
			this.m_lblFlyCut.Size = new System.Drawing.Size(37, 15);
			this.m_lblFlyCut.TabIndex = 1;
			this.m_lblFlyCut.Text = "類型";
			//
			// m_rbtnCircularFlyCut
			//
			this.m_rbtnCircularFlyCut.AutoSize = true;
			this.m_rbtnCircularFlyCut.Location = new System.Drawing.Point(46, 3);
			this.m_rbtnCircularFlyCut.Name = "m_rbtnCircularFlyCut";
			this.m_rbtnCircularFlyCut.Size = new System.Drawing.Size(88, 19);
			this.m_rbtnCircularFlyCut.TabIndex = 0;
			this.m_rbtnCircularFlyCut.TabStop = true;
			this.m_rbtnCircularFlyCut.Text = "圓弧飛切";
			this.m_rbtnCircularFlyCut.UseVisualStyleBackColor = true;
			//
			// m_gbxSetting
			//
			this.m_gbxSetting.Controls.Add(this.tableLayoutPanel1);
			this.m_gbxSetting.Location = new System.Drawing.Point(3, 49);
			this.m_gbxSetting.Name = "m_gbxSetting";
			this.m_gbxSetting.Size = new System.Drawing.Size(490, 178);
			this.m_gbxSetting.TabIndex = 2;
			this.m_gbxSetting.TabStop = false;
			this.m_gbxSetting.Text = "飛切參數";
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel3, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel4, 0, 2);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 24);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21.42857F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21.42857F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21.42857F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35.71429F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(472, 145);
			this.tableLayoutPanel1.TabIndex = 6;
			//
			// flowLayoutPanel1
			//
			this.flowLayoutPanel1.Controls.Add(this.m_lblStartPosition);
			this.flowLayoutPanel1.Controls.Add(this.m_rbtnLeftTop);
			this.flowLayoutPanel1.Controls.Add(this.m_rbtnLeftBottom);
			this.flowLayoutPanel1.Controls.Add(this.m_rbtnRightTop);
			this.flowLayoutPanel1.Controls.Add(this.m_rbtnRightBottom);
			this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(466, 25);
			this.flowLayoutPanel1.TabIndex = 0;
			//
			// m_lblStartPosition
			//
			this.m_lblStartPosition.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblStartPosition.AutoSize = true;
			this.m_lblStartPosition.Location = new System.Drawing.Point(3, 5);
			this.m_lblStartPosition.Name = "m_lblStartPosition";
			this.m_lblStartPosition.Size = new System.Drawing.Size(67, 15);
			this.m_lblStartPosition.TabIndex = 5;
			this.m_lblStartPosition.Text = "起刀位置";
			//
			// m_rbtnLeftTop
			//
			this.m_rbtnLeftTop.AutoSize = true;
			this.m_rbtnLeftTop.Location = new System.Drawing.Point(76, 3);
			this.m_rbtnLeftTop.Name = "m_rbtnLeftTop";
			this.m_rbtnLeftTop.Size = new System.Drawing.Size(58, 19);
			this.m_rbtnLeftTop.TabIndex = 6;
			this.m_rbtnLeftTop.TabStop = true;
			this.m_rbtnLeftTop.Text = "左上";
			this.m_rbtnLeftTop.UseVisualStyleBackColor = true;
			this.m_rbtnLeftTop.CheckedChanged += new System.EventHandler(this.StartPosition_CheckChanged);
			//
			// m_rbtnLeftBottom
			//
			this.m_rbtnLeftBottom.AutoSize = true;
			this.m_rbtnLeftBottom.Location = new System.Drawing.Point(140, 3);
			this.m_rbtnLeftBottom.Name = "m_rbtnLeftBottom";
			this.m_rbtnLeftBottom.Size = new System.Drawing.Size(58, 19);
			this.m_rbtnLeftBottom.TabIndex = 7;
			this.m_rbtnLeftBottom.TabStop = true;
			this.m_rbtnLeftBottom.Text = "左下";
			this.m_rbtnLeftBottom.UseVisualStyleBackColor = true;
			this.m_rbtnLeftBottom.CheckedChanged += new System.EventHandler(this.StartPosition_CheckChanged);
			//
			// m_rbtnRightTop
			//
			this.m_rbtnRightTop.AutoSize = true;
			this.m_rbtnRightTop.Location = new System.Drawing.Point(204, 3);
			this.m_rbtnRightTop.Name = "m_rbtnRightTop";
			this.m_rbtnRightTop.Size = new System.Drawing.Size(58, 19);
			this.m_rbtnRightTop.TabIndex = 8;
			this.m_rbtnRightTop.TabStop = true;
			this.m_rbtnRightTop.Text = "右上";
			this.m_rbtnRightTop.UseVisualStyleBackColor = true;
			this.m_rbtnRightTop.CheckedChanged += new System.EventHandler(this.StartPosition_CheckChanged);
			//
			// m_rbtnRightBottom
			//
			this.m_rbtnRightBottom.AutoSize = true;
			this.m_rbtnRightBottom.Location = new System.Drawing.Point(268, 3);
			this.m_rbtnRightBottom.Name = "m_rbtnRightBottom";
			this.m_rbtnRightBottom.Size = new System.Drawing.Size(58, 19);
			this.m_rbtnRightBottom.TabIndex = 9;
			this.m_rbtnRightBottom.TabStop = true;
			this.m_rbtnRightBottom.Text = "右下";
			this.m_rbtnRightBottom.UseVisualStyleBackColor = true;
			this.m_rbtnRightBottom.CheckedChanged += new System.EventHandler(this.StartPosition_CheckChanged);
			//
			// flowLayoutPanel2
			//
			this.flowLayoutPanel2.Controls.Add(this.m_lblDirection);
			this.flowLayoutPanel2.Controls.Add(this.m_rbtnVertical);
			this.flowLayoutPanel2.Controls.Add(this.m_rbtnHorizontal);
			this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 34);
			this.flowLayoutPanel2.Name = "flowLayoutPanel2";
			this.flowLayoutPanel2.Size = new System.Drawing.Size(339, 25);
			this.flowLayoutPanel2.TabIndex = 1;
			//
			// m_lblDirection
			//
			this.m_lblDirection.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblDirection.AutoSize = true;
			this.m_lblDirection.Location = new System.Drawing.Point(3, 5);
			this.m_lblDirection.Name = "m_lblDirection";
			this.m_lblDirection.Size = new System.Drawing.Size(37, 15);
			this.m_lblDirection.TabIndex = 0;
			this.m_lblDirection.Text = "方向";
			//
			// m_rbtnVertical
			//
			this.m_rbtnVertical.AutoSize = true;
			this.m_rbtnVertical.Location = new System.Drawing.Point(46, 3);
			this.m_rbtnVertical.Name = "m_rbtnVertical";
			this.m_rbtnVertical.Size = new System.Drawing.Size(58, 19);
			this.m_rbtnVertical.TabIndex = 1;
			this.m_rbtnVertical.TabStop = true;
			this.m_rbtnVertical.Text = "直排";
			this.m_rbtnVertical.UseVisualStyleBackColor = true;
			this.m_rbtnVertical.CheckedChanged += new System.EventHandler(this.Direction_CheckChanged);
			//
			// m_rbtnHorizontal
			//
			this.m_rbtnHorizontal.AutoSize = true;
			this.m_rbtnHorizontal.Location = new System.Drawing.Point(110, 3);
			this.m_rbtnHorizontal.Name = "m_rbtnHorizontal";
			this.m_rbtnHorizontal.Size = new System.Drawing.Size(58, 19);
			this.m_rbtnHorizontal.TabIndex = 2;
			this.m_rbtnHorizontal.TabStop = true;
			this.m_rbtnHorizontal.Text = "橫排";
			this.m_rbtnHorizontal.UseVisualStyleBackColor = true;
			this.m_rbtnHorizontal.CheckedChanged += new System.EventHandler(this.Direction_CheckChanged);
			//
			// flowLayoutPanel3
			//
			this.flowLayoutPanel3.Controls.Add(this.m_btnAdd);
			this.flowLayoutPanel3.Controls.Add(this.m_btnRemove);
			this.flowLayoutPanel3.Location = new System.Drawing.Point(3, 96);
			this.flowLayoutPanel3.Name = "flowLayoutPanel3";
			this.flowLayoutPanel3.Size = new System.Drawing.Size(426, 46);
			this.flowLayoutPanel3.TabIndex = 2;
			//
			// m_btnAdd
			//
			this.m_btnAdd.Location = new System.Drawing.Point(3, 3);
			this.m_btnAdd.Name = "m_btnAdd";
			this.m_btnAdd.Size = new System.Drawing.Size(92, 38);
			this.m_btnAdd.TabIndex = 3;
			this.m_btnAdd.Text = "新增";
			this.m_btnAdd.UseVisualStyleBackColor = true;
			this.m_btnAdd.Click += new System.EventHandler(this.m_btnAdd_Click);
			//
			// m_btnRemove
			//
			this.m_btnRemove.Location = new System.Drawing.Point(101, 3);
			this.m_btnRemove.Name = "m_btnRemove";
			this.m_btnRemove.Size = new System.Drawing.Size(92, 38);
			this.m_btnRemove.TabIndex = 4;
			this.m_btnRemove.Text = "移除";
			this.m_btnRemove.UseVisualStyleBackColor = true;
			this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
			//
			// flowLayoutPanel4
			//
			this.flowLayoutPanel4.Controls.Add(this.m_lblGroupWidth);
			this.flowLayoutPanel4.Controls.Add(this.m_nudGroupWidth);
			this.flowLayoutPanel4.Location = new System.Drawing.Point(3, 65);
			this.flowLayoutPanel4.Name = "flowLayoutPanel4";
			this.flowLayoutPanel4.Size = new System.Drawing.Size(200, 25);
			this.flowLayoutPanel4.TabIndex = 3;
			//
			// m_lblGroupWidth
			//
			this.m_lblGroupWidth.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.m_lblGroupWidth.AutoSize = true;
			this.m_lblGroupWidth.Location = new System.Drawing.Point(3, 8);
			this.m_lblGroupWidth.Name = "m_lblGroupWidth";
			this.m_lblGroupWidth.Size = new System.Drawing.Size(67, 15);
			this.m_lblGroupWidth.TabIndex = 0;
			this.m_lblGroupWidth.Text = "分組間距";
			//
			// m_nudGroupWidth
			//
			this.m_nudGroupWidth.DecimalPlaces = 3;
			this.m_nudGroupWidth.Location = new System.Drawing.Point(76, 3);
			this.m_nudGroupWidth.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_nudGroupWidth.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_nudGroupWidth.Name = "m_nudGroupWidth";
			this.m_nudGroupWidth.RestrictInvalidCharacters = false;
			this.m_nudGroupWidth.Size = new System.Drawing.Size(114, 25);
			this.m_nudGroupWidth.TabIndex = 5;
			this.m_nudGroupWidth.ValueChanged += new System.EventHandler(this.m_nudGroupWidth_ValueChanged);
			//
			// FlyCutDlg
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(535, 251);
			this.Controls.Add(this.m_pnlFlyCut);
			this.Name = "FlyCutDlg";
			this.ShowIcon = false;
			this.Text = "飛行切割";
			this.m_pnlFlyCut.ResumeLayout(false);
			this.m_flpnlFlyCutType.ResumeLayout(false);
			this.m_flpnlFlyCutType.PerformLayout();
			this.m_gbxSetting.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.flowLayoutPanel2.ResumeLayout(false);
			this.flowLayoutPanel2.PerformLayout();
			this.flowLayoutPanel3.ResumeLayout(false);
			this.flowLayoutPanel4.ResumeLayout(false);
			this.flowLayoutPanel4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_nudGroupWidth)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RadioButton m_rbtnCircularFlyCut;
		private System.Windows.Forms.Label m_lblFlyCut;
		private System.Windows.Forms.GroupBox m_gbxSetting;
		private System.Windows.Forms.Button m_btnRemove;
		private System.Windows.Forms.Button m_btnAdd;
		private System.Windows.Forms.TableLayoutPanel m_pnlFlyCut;
		private System.Windows.Forms.FlowLayoutPanel m_flpnlFlyCutType;
		private System.Windows.Forms.Label m_lblStartPosition;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.RadioButton m_rbtnLeftTop;
		private System.Windows.Forms.RadioButton m_rbtnLeftBottom;
		private System.Windows.Forms.RadioButton m_rbtnRightTop;
		private System.Windows.Forms.RadioButton m_rbtnRightBottom;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private System.Windows.Forms.Label m_lblDirection;
		private System.Windows.Forms.RadioButton m_rbtnVertical;
		private System.Windows.Forms.RadioButton m_rbtnHorizontal;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
		private System.Windows.Forms.Label m_lblGroupWidth;
		private CommonUI.Controls.DebouncedNumericUpDown m_nudGroupWidth;
	}
}
