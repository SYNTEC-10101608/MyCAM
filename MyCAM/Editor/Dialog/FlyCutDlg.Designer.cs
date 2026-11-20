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
			this.m_pnlFlyCut = new CommonUI.Controls.SectionPanel();
			this.m_pnlFlyCutType = new System.Windows.Forms.Panel();
			this.m_lblFlyCut = new CommonUI.Controls.ThemedLabel();
			this.m_rdoCircularFlyCut = new System.Windows.Forms.RadioButton();
			this.m_pnlSetting = new CommonUI.Controls.SectionPanel();
			this.m_pnlStartPosition = new System.Windows.Forms.Panel();
			this.m_lblStartPosition = new CommonUI.Controls.ThemedLabel();
			this.m_rdoLeftTop = new System.Windows.Forms.RadioButton();
			this.m_rdoLeftBottom = new System.Windows.Forms.RadioButton();
			this.m_rdoRightTop = new System.Windows.Forms.RadioButton();
			this.m_rdoRightBottom = new System.Windows.Forms.RadioButton();
			this.m_pnlDirection = new System.Windows.Forms.Panel();
			this.m_lblDirection = new CommonUI.Controls.ThemedLabel();
			this.m_rdoVertical = new System.Windows.Forms.RadioButton();
			this.m_rdoHorizontal = new System.Windows.Forms.RadioButton();
			this.m_lblGroupWidth = new CommonUI.Controls.ThemedLabel();
			this.m_numGroupWidth = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_btnRemove = new CommonUI.Controls.ThemedButton();
			this.m_btnAdd = new CommonUI.Controls.ThemedButton();
			this.m_pnlFlyCut.SuspendLayout();
			this.m_pnlFlyCutType.SuspendLayout();
			this.m_pnlSetting.SuspendLayout();
			this.m_pnlStartPosition.SuspendLayout();
			this.m_pnlDirection.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numGroupWidth)).BeginInit();
			this.SuspendLayout();
			//
			// m_pnlFlyCut
			//
			this.m_pnlFlyCut.Controls.Add(this.m_pnlFlyCutType);
			this.m_pnlFlyCut.Location = new System.Drawing.Point(12, 12);
			this.m_pnlFlyCut.Name = "m_pnlFlyCut";
			this.m_pnlFlyCut.Size = new System.Drawing.Size(356, 43);
			this.m_pnlFlyCut.TabIndex = 0;
			//
			// m_pnlFlyCutType
			//
			this.m_pnlFlyCutType.Controls.Add(this.m_lblFlyCut);
			this.m_pnlFlyCutType.Controls.Add(this.m_rdoCircularFlyCut);
			this.m_pnlFlyCutType.Location = new System.Drawing.Point(10, 10);
			this.m_pnlFlyCutType.Name = "m_pnlFlyCutType";
			this.m_pnlFlyCutType.Size = new System.Drawing.Size(336, 23);
			this.m_pnlFlyCutType.TabIndex = 0;
			//
			// m_lblFlyCut
			//
			this.m_lblFlyCut.AutoSize = true;
			this.m_lblFlyCut.Location = new System.Drawing.Point(0, 4);
			this.m_lblFlyCut.Name = "m_lblFlyCut";
			this.m_lblFlyCut.Size = new System.Drawing.Size(31, 16);
			this.m_lblFlyCut.TabIndex = 0;
			this.m_lblFlyCut.Text = "類型";
			//
			// m_rdoCircularFlyCut
			//
			this.m_rdoCircularFlyCut.AutoSize = true;
			this.m_rdoCircularFlyCut.Location = new System.Drawing.Point(90, 2);
			this.m_rdoCircularFlyCut.Name = "m_rdoCircularFlyCut";
			this.m_rdoCircularFlyCut.Size = new System.Drawing.Size(73, 20);
			this.m_rdoCircularFlyCut.TabIndex = 1;
			this.m_rdoCircularFlyCut.TabStop = true;
			this.m_rdoCircularFlyCut.Text = "圓弧飛切";
			//
			// m_pnlSetting
			//
			this.m_pnlSetting.Controls.Add(this.m_pnlStartPosition);
			this.m_pnlSetting.Controls.Add(this.m_pnlDirection);
			this.m_pnlSetting.Controls.Add(this.m_lblGroupWidth);
			this.m_pnlSetting.Controls.Add(this.m_numGroupWidth);
			this.m_pnlSetting.Location = new System.Drawing.Point(12, 63);
			this.m_pnlSetting.Name = "m_pnlSetting";
			this.m_pnlSetting.Size = new System.Drawing.Size(356, 129);
			this.m_pnlSetting.TabIndex = 1;
			this.m_pnlSetting.Text = "飛切參數";
			//
			// m_pnlStartPosition
			//
			this.m_pnlStartPosition.Controls.Add(this.m_lblStartPosition);
			this.m_pnlStartPosition.Controls.Add(this.m_rdoLeftTop);
			this.m_pnlStartPosition.Controls.Add(this.m_rdoLeftBottom);
			this.m_pnlStartPosition.Controls.Add(this.m_rdoRightTop);
			this.m_pnlStartPosition.Controls.Add(this.m_rdoRightBottom);
			this.m_pnlStartPosition.Location = new System.Drawing.Point(10, 36);
			this.m_pnlStartPosition.Name = "m_pnlStartPosition";
			this.m_pnlStartPosition.Size = new System.Drawing.Size(336, 23);
			this.m_pnlStartPosition.TabIndex = 0;
			//
			// m_lblStartPosition
			//
			this.m_lblStartPosition.AutoSize = true;
			this.m_lblStartPosition.Location = new System.Drawing.Point(0, 4);
			this.m_lblStartPosition.Name = "m_lblStartPosition";
			this.m_lblStartPosition.Size = new System.Drawing.Size(55, 16);
			this.m_lblStartPosition.TabIndex = 0;
			this.m_lblStartPosition.Text = "起刀位置";
			//
			// m_rdoLeftTop
			//
			this.m_rdoLeftTop.AutoSize = true;
			this.m_rdoLeftTop.Location = new System.Drawing.Point(90, 2);
			this.m_rdoLeftTop.Name = "m_rdoLeftTop";
			this.m_rdoLeftTop.Size = new System.Drawing.Size(49, 20);
			this.m_rdoLeftTop.TabIndex = 1;
			this.m_rdoLeftTop.TabStop = true;
			this.m_rdoLeftTop.Text = "左上";
			this.m_rdoLeftTop.CheckedChanged += new System.EventHandler(this.StartPosition_CheckChanged);
			//
			// m_rdoLeftBottom
			//
			this.m_rdoLeftBottom.AutoSize = true;
			this.m_rdoLeftBottom.Location = new System.Drawing.Point(150, 2);
			this.m_rdoLeftBottom.Name = "m_rdoLeftBottom";
			this.m_rdoLeftBottom.Size = new System.Drawing.Size(49, 20);
			this.m_rdoLeftBottom.TabIndex = 2;
			this.m_rdoLeftBottom.TabStop = true;
			this.m_rdoLeftBottom.Text = "左下";
			this.m_rdoLeftBottom.CheckedChanged += new System.EventHandler(this.StartPosition_CheckChanged);
			//
			// m_rdoRightTop
			//
			this.m_rdoRightTop.AutoSize = true;
			this.m_rdoRightTop.Location = new System.Drawing.Point(210, 2);
			this.m_rdoRightTop.Name = "m_rdoRightTop";
			this.m_rdoRightTop.Size = new System.Drawing.Size(49, 20);
			this.m_rdoRightTop.TabIndex = 3;
			this.m_rdoRightTop.TabStop = true;
			this.m_rdoRightTop.Text = "右上";
			this.m_rdoRightTop.CheckedChanged += new System.EventHandler(this.StartPosition_CheckChanged);
			//
			// m_rdoRightBottom
			//
			this.m_rdoRightBottom.AutoSize = true;
			this.m_rdoRightBottom.Location = new System.Drawing.Point(270, 2);
			this.m_rdoRightBottom.Name = "m_rdoRightBottom";
			this.m_rdoRightBottom.Size = new System.Drawing.Size(49, 20);
			this.m_rdoRightBottom.TabIndex = 4;
			this.m_rdoRightBottom.TabStop = true;
			this.m_rdoRightBottom.Text = "右下";
			this.m_rdoRightBottom.CheckedChanged += new System.EventHandler(this.StartPosition_CheckChanged);
			//
			// m_pnlDirection
			//
			this.m_pnlDirection.Controls.Add(this.m_lblDirection);
			this.m_pnlDirection.Controls.Add(this.m_rdoVertical);
			this.m_pnlDirection.Controls.Add(this.m_rdoHorizontal);
			this.m_pnlDirection.Location = new System.Drawing.Point(10, 66);
			this.m_pnlDirection.Name = "m_pnlDirection";
			this.m_pnlDirection.Size = new System.Drawing.Size(336, 23);
			this.m_pnlDirection.TabIndex = 1;
			//
			// m_lblDirection
			//
			this.m_lblDirection.AutoSize = true;
			this.m_lblDirection.Location = new System.Drawing.Point(0, 4);
			this.m_lblDirection.Name = "m_lblDirection";
			this.m_lblDirection.Size = new System.Drawing.Size(31, 16);
			this.m_lblDirection.TabIndex = 0;
			this.m_lblDirection.Text = "方向";
			//
			// m_rdoVertical
			//
			this.m_rdoVertical.AutoSize = true;
			this.m_rdoVertical.Location = new System.Drawing.Point(90, 2);
			this.m_rdoVertical.Name = "m_rdoVertical";
			this.m_rdoVertical.Size = new System.Drawing.Size(49, 20);
			this.m_rdoVertical.TabIndex = 1;
			this.m_rdoVertical.TabStop = true;
			this.m_rdoVertical.Text = "直排";
			this.m_rdoVertical.CheckedChanged += new System.EventHandler(this.Direction_CheckChanged);
			//
			// m_rdoHorizontal
			//
			this.m_rdoHorizontal.AutoSize = true;
			this.m_rdoHorizontal.Location = new System.Drawing.Point(150, 2);
			this.m_rdoHorizontal.Name = "m_rdoHorizontal";
			this.m_rdoHorizontal.Size = new System.Drawing.Size(49, 20);
			this.m_rdoHorizontal.TabIndex = 2;
			this.m_rdoHorizontal.TabStop = true;
			this.m_rdoHorizontal.Text = "橫排";
			this.m_rdoHorizontal.CheckedChanged += new System.EventHandler(this.Direction_CheckChanged);
			//
			// m_lblGroupWidth
			//
			this.m_lblGroupWidth.AutoSize = true;
			this.m_lblGroupWidth.Location = new System.Drawing.Point(10, 100);
			this.m_lblGroupWidth.Name = "m_lblGroupWidth";
			this.m_lblGroupWidth.Size = new System.Drawing.Size(55, 16);
			this.m_lblGroupWidth.TabIndex = 2;
			this.m_lblGroupWidth.Text = "分組間距";
			//
			// m_numGroupWidth
			//
			this.m_numGroupWidth.DecimalPlaces = 3;
			this.m_numGroupWidth.Location = new System.Drawing.Point(100, 96);
			this.m_numGroupWidth.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_numGroupWidth.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_numGroupWidth.Name = "m_numGroupWidth";
			this.m_numGroupWidth.RestrictInvalidCharacters = false;
			this.m_numGroupWidth.Size = new System.Drawing.Size(150, 23);
			this.m_numGroupWidth.TabIndex = 3;
			this.m_numGroupWidth.ValueChanged += new System.EventHandler(this.m_numGroupWidth_ValueChanged);
			//
			// m_btnRemove
			//
			this.m_btnRemove.Location = new System.Drawing.Point(200, 208);
			this.m_btnRemove.Kind = CommonUI.Controls.ButtonKind.Danger;
			this.m_btnRemove.Name = "m_btnRemove";
			this.m_btnRemove.Size = new System.Drawing.Size(80, 28);
			this.m_btnRemove.TabIndex = 2;
			this.m_btnRemove.Text = "移除";
			this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
			//
			// m_btnAdd
			//
			this.m_btnAdd.Location = new System.Drawing.Point(288, 208);
			this.m_btnAdd.Name = "m_btnAdd";
			this.m_btnAdd.Size = new System.Drawing.Size(80, 28);
			this.m_btnAdd.TabIndex = 3;
			this.m_btnAdd.Text = "新增";
			this.m_btnAdd.Click += new System.EventHandler(this.m_btnAdd_Click);
			//
			// FlyCutDlg
			//
			this.ClientSize = new System.Drawing.Size(380, 248);
			this.Controls.Add(this.m_pnlFlyCut);
			this.Controls.Add(this.m_pnlSetting);
			this.Controls.Add(this.m_btnRemove);
			this.Controls.Add(this.m_btnAdd);
			this.Name = "FlyCutDlg";
			this.Text = "飛行切割";
			this.m_pnlFlyCut.ResumeLayout(false);
			this.m_pnlFlyCutType.ResumeLayout(false);
			this.m_pnlFlyCutType.PerformLayout();
			this.m_pnlSetting.ResumeLayout(false);
			this.m_pnlSetting.PerformLayout();
			this.m_pnlStartPosition.ResumeLayout(false);
			this.m_pnlStartPosition.PerformLayout();
			this.m_pnlDirection.ResumeLayout(false);
			this.m_pnlDirection.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numGroupWidth)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlFlyCut;
		private System.Windows.Forms.Panel m_pnlFlyCutType;
		private CommonUI.Controls.ThemedLabel m_lblFlyCut;
		private System.Windows.Forms.RadioButton m_rdoCircularFlyCut;
		private CommonUI.Controls.SectionPanel m_pnlSetting;
		private System.Windows.Forms.Panel m_pnlStartPosition;
		private CommonUI.Controls.ThemedLabel m_lblStartPosition;
		private System.Windows.Forms.RadioButton m_rdoLeftTop;
		private System.Windows.Forms.RadioButton m_rdoLeftBottom;
		private System.Windows.Forms.RadioButton m_rdoRightTop;
		private System.Windows.Forms.RadioButton m_rdoRightBottom;
		private System.Windows.Forms.Panel m_pnlDirection;
		private CommonUI.Controls.ThemedLabel m_lblDirection;
		private System.Windows.Forms.RadioButton m_rdoVertical;
		private System.Windows.Forms.RadioButton m_rdoHorizontal;
		private CommonUI.Controls.ThemedLabel m_lblGroupWidth;
		private CommonUI.Controls.DebouncedNumericUpDown m_numGroupWidth;
		private CommonUI.Controls.ThemedButton m_btnRemove;
		private CommonUI.Controls.ThemedButton m_btnAdd;
	}
}
