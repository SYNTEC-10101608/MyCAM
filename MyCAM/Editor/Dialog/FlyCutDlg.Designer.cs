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
			this.m_flpnlFlyCutType = new System.Windows.Forms.Panel();
			this.m_lblFlyCut = new System.Windows.Forms.Label();
			this.m_rbtnCircularFlyCut = new System.Windows.Forms.RadioButton();
			this.m_gbxSetting = new CommonUI.Controls.SectionPanel();
			this.m_pnlStartPosition = new System.Windows.Forms.Panel();
			this.m_lblStartPosition = new System.Windows.Forms.Label();
			this.m_rbtnLeftTop = new System.Windows.Forms.RadioButton();
			this.m_rbtnLeftBottom = new System.Windows.Forms.RadioButton();
			this.m_rbtnRightTop = new System.Windows.Forms.RadioButton();
			this.m_rbtnRightBottom = new System.Windows.Forms.RadioButton();
			this.m_pnlDirection = new System.Windows.Forms.Panel();
			this.m_lblDirection = new System.Windows.Forms.Label();
			this.m_rbtnVertical = new System.Windows.Forms.RadioButton();
			this.m_rbtnHorizontal = new System.Windows.Forms.RadioButton();
			this.m_lblGroupWidth = new System.Windows.Forms.Label();
			this.m_nudGroupWidth = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_btnRemove = new CommonUI.Controls.ThemedButton();
			this.m_btnAdd = new CommonUI.Controls.ThemedButton();
			this.m_pnlFlyCut.SuspendLayout();
			this.m_flpnlFlyCutType.SuspendLayout();
			this.m_gbxSetting.SuspendLayout();
			this.m_pnlStartPosition.SuspendLayout();
			this.m_pnlDirection.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_nudGroupWidth)).BeginInit();
			this.SuspendLayout();
			//
			// m_pnlFlyCut
			//
			this.m_pnlFlyCut.Controls.Add(this.m_flpnlFlyCutType);
			this.m_pnlFlyCut.Location = new System.Drawing.Point(12, 12);
			this.m_pnlFlyCut.Name = "m_pnlFlyCut";
			this.m_pnlFlyCut.Size = new System.Drawing.Size(356, 43);
			this.m_pnlFlyCut.TabIndex = 0;
			//
			// m_flpnlFlyCutType
			//
			this.m_flpnlFlyCutType.Controls.Add(this.m_lblFlyCut);
			this.m_flpnlFlyCutType.Controls.Add(this.m_rbtnCircularFlyCut);
			this.m_flpnlFlyCutType.Location = new System.Drawing.Point(10, 10);
			this.m_flpnlFlyCutType.Name = "m_flpnlFlyCutType";
			this.m_flpnlFlyCutType.Size = new System.Drawing.Size(336, 23);
			this.m_flpnlFlyCutType.TabIndex = 0;
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
			// m_rbtnCircularFlyCut
			//
			this.m_rbtnCircularFlyCut.AutoSize = true;
			this.m_rbtnCircularFlyCut.Location = new System.Drawing.Point(90, 2);
			this.m_rbtnCircularFlyCut.Name = "m_rbtnCircularFlyCut";
			this.m_rbtnCircularFlyCut.Size = new System.Drawing.Size(73, 20);
			this.m_rbtnCircularFlyCut.TabIndex = 1;
			this.m_rbtnCircularFlyCut.TabStop = true;
			this.m_rbtnCircularFlyCut.Text = "圓弧飛切";
			//
			// m_gbxSetting
			//
			this.m_gbxSetting.Controls.Add(this.m_pnlStartPosition);
			this.m_gbxSetting.Controls.Add(this.m_pnlDirection);
			this.m_gbxSetting.Controls.Add(this.m_lblGroupWidth);
			this.m_gbxSetting.Controls.Add(this.m_nudGroupWidth);
			this.m_gbxSetting.Location = new System.Drawing.Point(12, 63);
			this.m_gbxSetting.Name = "m_gbxSetting";
			this.m_gbxSetting.Size = new System.Drawing.Size(356, 129);
			this.m_gbxSetting.TabIndex = 1;
			this.m_gbxSetting.Text = "飛切參數";
			//
			// m_pnlStartPosition
			//
			this.m_pnlStartPosition.Controls.Add(this.m_lblStartPosition);
			this.m_pnlStartPosition.Controls.Add(this.m_rbtnLeftTop);
			this.m_pnlStartPosition.Controls.Add(this.m_rbtnLeftBottom);
			this.m_pnlStartPosition.Controls.Add(this.m_rbtnRightTop);
			this.m_pnlStartPosition.Controls.Add(this.m_rbtnRightBottom);
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
			// m_rbtnLeftTop
			//
			this.m_rbtnLeftTop.AutoSize = true;
			this.m_rbtnLeftTop.Location = new System.Drawing.Point(90, 2);
			this.m_rbtnLeftTop.Name = "m_rbtnLeftTop";
			this.m_rbtnLeftTop.Size = new System.Drawing.Size(49, 20);
			this.m_rbtnLeftTop.TabIndex = 1;
			this.m_rbtnLeftTop.TabStop = true;
			this.m_rbtnLeftTop.Text = "左上";
			this.m_rbtnLeftTop.CheckedChanged += new System.EventHandler(this.StartPosition_CheckChanged);
			//
			// m_rbtnLeftBottom
			//
			this.m_rbtnLeftBottom.AutoSize = true;
			this.m_rbtnLeftBottom.Location = new System.Drawing.Point(150, 2);
			this.m_rbtnLeftBottom.Name = "m_rbtnLeftBottom";
			this.m_rbtnLeftBottom.Size = new System.Drawing.Size(49, 20);
			this.m_rbtnLeftBottom.TabIndex = 2;
			this.m_rbtnLeftBottom.TabStop = true;
			this.m_rbtnLeftBottom.Text = "左下";
			this.m_rbtnLeftBottom.CheckedChanged += new System.EventHandler(this.StartPosition_CheckChanged);
			//
			// m_rbtnRightTop
			//
			this.m_rbtnRightTop.AutoSize = true;
			this.m_rbtnRightTop.Location = new System.Drawing.Point(210, 2);
			this.m_rbtnRightTop.Name = "m_rbtnRightTop";
			this.m_rbtnRightTop.Size = new System.Drawing.Size(49, 20);
			this.m_rbtnRightTop.TabIndex = 3;
			this.m_rbtnRightTop.TabStop = true;
			this.m_rbtnRightTop.Text = "右上";
			this.m_rbtnRightTop.CheckedChanged += new System.EventHandler(this.StartPosition_CheckChanged);
			//
			// m_rbtnRightBottom
			//
			this.m_rbtnRightBottom.AutoSize = true;
			this.m_rbtnRightBottom.Location = new System.Drawing.Point(270, 2);
			this.m_rbtnRightBottom.Name = "m_rbtnRightBottom";
			this.m_rbtnRightBottom.Size = new System.Drawing.Size(49, 20);
			this.m_rbtnRightBottom.TabIndex = 4;
			this.m_rbtnRightBottom.TabStop = true;
			this.m_rbtnRightBottom.Text = "右下";
			this.m_rbtnRightBottom.CheckedChanged += new System.EventHandler(this.StartPosition_CheckChanged);
			//
			// m_pnlDirection
			//
			this.m_pnlDirection.Controls.Add(this.m_lblDirection);
			this.m_pnlDirection.Controls.Add(this.m_rbtnVertical);
			this.m_pnlDirection.Controls.Add(this.m_rbtnHorizontal);
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
			// m_rbtnVertical
			//
			this.m_rbtnVertical.AutoSize = true;
			this.m_rbtnVertical.Location = new System.Drawing.Point(90, 2);
			this.m_rbtnVertical.Name = "m_rbtnVertical";
			this.m_rbtnVertical.Size = new System.Drawing.Size(49, 20);
			this.m_rbtnVertical.TabIndex = 1;
			this.m_rbtnVertical.TabStop = true;
			this.m_rbtnVertical.Text = "直排";
			this.m_rbtnVertical.CheckedChanged += new System.EventHandler(this.Direction_CheckChanged);
			//
			// m_rbtnHorizontal
			//
			this.m_rbtnHorizontal.AutoSize = true;
			this.m_rbtnHorizontal.Location = new System.Drawing.Point(150, 2);
			this.m_rbtnHorizontal.Name = "m_rbtnHorizontal";
			this.m_rbtnHorizontal.Size = new System.Drawing.Size(49, 20);
			this.m_rbtnHorizontal.TabIndex = 2;
			this.m_rbtnHorizontal.TabStop = true;
			this.m_rbtnHorizontal.Text = "橫排";
			this.m_rbtnHorizontal.CheckedChanged += new System.EventHandler(this.Direction_CheckChanged);
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
			// m_nudGroupWidth
			//
			this.m_nudGroupWidth.DecimalPlaces = 3;
			this.m_nudGroupWidth.Location = new System.Drawing.Point(100, 96);
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
			this.m_nudGroupWidth.Size = new System.Drawing.Size(150, 23);
			this.m_nudGroupWidth.TabIndex = 3;
			this.m_nudGroupWidth.ValueChanged += new System.EventHandler(this.m_nudGroupWidth_ValueChanged);
			//
			// m_btnRemove
			//
			this.m_btnRemove.Location = new System.Drawing.Point(200, 208);
			this.m_btnRemove.Name = "m_btnRemove";
			this.m_btnRemove.Size = new System.Drawing.Size(80, 28);
			this.m_btnRemove.TabIndex = 2;
			this.m_btnRemove.Text = "移除";
			this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
			//
			// m_btnAdd
			//
			this.m_btnAdd.Kind = CommonUI.Controls.ButtonKind.Primary;
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
			this.Controls.Add(this.m_gbxSetting);
			this.Controls.Add(this.m_btnRemove);
			this.Controls.Add(this.m_btnAdd);
			this.Name = "FlyCutDlg";
			this.Text = "飛行切割";
			this.m_pnlFlyCut.ResumeLayout(false);
			this.m_flpnlFlyCutType.ResumeLayout(false);
			this.m_flpnlFlyCutType.PerformLayout();
			this.m_gbxSetting.ResumeLayout(false);
			this.m_gbxSetting.PerformLayout();
			this.m_pnlStartPosition.ResumeLayout(false);
			this.m_pnlStartPosition.PerformLayout();
			this.m_pnlDirection.ResumeLayout(false);
			this.m_pnlDirection.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_nudGroupWidth)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlFlyCut;
		private System.Windows.Forms.Panel m_flpnlFlyCutType;
		private System.Windows.Forms.Label m_lblFlyCut;
		private System.Windows.Forms.RadioButton m_rbtnCircularFlyCut;
		private CommonUI.Controls.SectionPanel m_gbxSetting;
		private System.Windows.Forms.Panel m_pnlStartPosition;
		private System.Windows.Forms.Label m_lblStartPosition;
		private System.Windows.Forms.RadioButton m_rbtnLeftTop;
		private System.Windows.Forms.RadioButton m_rbtnLeftBottom;
		private System.Windows.Forms.RadioButton m_rbtnRightTop;
		private System.Windows.Forms.RadioButton m_rbtnRightBottom;
		private System.Windows.Forms.Panel m_pnlDirection;
		private System.Windows.Forms.Label m_lblDirection;
		private System.Windows.Forms.RadioButton m_rbtnVertical;
		private System.Windows.Forms.RadioButton m_rbtnHorizontal;
		private System.Windows.Forms.Label m_lblGroupWidth;
		private CommonUI.Controls.DebouncedNumericUpDown m_nudGroupWidth;
		private CommonUI.Controls.ThemedButton m_btnRemove;
		private CommonUI.Controls.ThemedButton m_btnAdd;
	}
}
