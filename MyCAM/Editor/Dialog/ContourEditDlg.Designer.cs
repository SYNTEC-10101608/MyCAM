namespace MyCAM.Editor
{
	partial class ContourEditDlg
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
			this.m_gbxIndexParam = new CommonUI.Controls.SectionPanel();
			this.m_flpnlCoord = new System.Windows.Forms.Panel();
			this.m_lblRefCoordType = new System.Windows.Forms.Label();
			this.m_rbtnWorldCoord = new System.Windows.Forms.RadioButton();
			this.m_rbtnLocalCoord = new System.Windows.Forms.RadioButton();
			this.m_flpnlToStartOrEnd = new System.Windows.Forms.Panel();
			this.m_lblToStartOrEnd = new System.Windows.Forms.Label();
			this.m_rbtnToStart = new System.Windows.Forms.RadioButton();
			this.m_rbtnToEnd = new System.Windows.Forms.RadioButton();
			this.m_lblUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblTotalDiff = new System.Windows.Forms.Label();
			this.m_lblDX = new System.Windows.Forms.Label();
			this.m_tbxDX = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblTotalDX = new System.Windows.Forms.Label();
			this.m_lblDY = new System.Windows.Forms.Label();
			this.m_tbxDY = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblTotalDY = new System.Windows.Forms.Label();
			this.m_lblDZ = new System.Windows.Forms.Label();
			this.m_tbxDZ = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblTotalDZ = new System.Windows.Forms.Label();
			this.m_lblRelativeCurrentPath = new CommonUI.Controls.ThemedLabel();
			this.m_btnAdd = new CommonUI.Controls.ThemedButton();
			this.m_btnRemove = new CommonUI.Controls.ThemedButton();
			this.m_btnReset = new CommonUI.Controls.ThemedButton();
			this.m_btnClear = new CommonUI.Controls.ThemedButton();
			this.m_btnSave = new CommonUI.Controls.ThemedButton();
			this.m_gbxIndexParam.SuspendLayout();
			this.m_flpnlCoord.SuspendLayout();
			this.m_flpnlToStartOrEnd.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDZ)).BeginInit();
			this.SuspendLayout();
			// 
			// m_gbxIndexParam
			// 
			this.m_gbxIndexParam.Controls.Add(this.m_flpnlCoord);
			this.m_gbxIndexParam.Controls.Add(this.m_flpnlToStartOrEnd);
			this.m_gbxIndexParam.Controls.Add(this.m_lblUnit);
			this.m_gbxIndexParam.Controls.Add(this.m_lblTotalDiff);
			this.m_gbxIndexParam.Controls.Add(this.m_lblDX);
			this.m_gbxIndexParam.Controls.Add(this.m_tbxDX);
			this.m_gbxIndexParam.Controls.Add(this.m_lblTotalDX);
			this.m_gbxIndexParam.Controls.Add(this.m_lblDY);
			this.m_gbxIndexParam.Controls.Add(this.m_tbxDY);
			this.m_gbxIndexParam.Controls.Add(this.m_lblTotalDY);
			this.m_gbxIndexParam.Controls.Add(this.m_lblDZ);
			this.m_gbxIndexParam.Controls.Add(this.m_tbxDZ);
			this.m_gbxIndexParam.Controls.Add(this.m_lblTotalDZ);
			this.m_gbxIndexParam.Controls.Add(this.m_lblRelativeCurrentPath);
			this.m_gbxIndexParam.Controls.Add(this.m_btnAdd);
			this.m_gbxIndexParam.Controls.Add(this.m_btnRemove);
			this.m_gbxIndexParam.Controls.Add(this.m_btnReset);
			this.m_gbxIndexParam.Controls.Add(this.m_btnClear);
			this.m_gbxIndexParam.Controls.Add(this.m_btnSave);
			this.m_gbxIndexParam.Location = new System.Drawing.Point(12, 12);
			this.m_gbxIndexParam.Name = "m_gbxIndexParam";
			this.m_gbxIndexParam.Size = new System.Drawing.Size(416, 309);
			this.m_gbxIndexParam.TabIndex = 0;
			this.m_gbxIndexParam.Text = "控制點參數";
			// 
			// m_flpnlCoord
			// 
			this.m_flpnlCoord.Controls.Add(this.m_lblRefCoordType);
			this.m_flpnlCoord.Controls.Add(this.m_rbtnWorldCoord);
			this.m_flpnlCoord.Controls.Add(this.m_rbtnLocalCoord);
			this.m_flpnlCoord.Location = new System.Drawing.Point(10, 36);
			this.m_flpnlCoord.Name = "m_flpnlCoord";
			this.m_flpnlCoord.Size = new System.Drawing.Size(396, 23);
			this.m_flpnlCoord.TabIndex = 0;
			// 
			// m_lblRefCoordType
			// 
			this.m_lblRefCoordType.AutoSize = true;
			this.m_lblRefCoordType.Location = new System.Drawing.Point(0, 4);
			this.m_lblRefCoordType.Name = "m_lblRefCoordType";
			this.m_lblRefCoordType.Size = new System.Drawing.Size(43, 16);
			this.m_lblRefCoordType.TabIndex = 0;
			this.m_lblRefCoordType.Text = "座標系";
			// 
			// m_rbtnWorldCoord
			// 
			this.m_rbtnWorldCoord.AutoSize = true;
			this.m_rbtnWorldCoord.Location = new System.Drawing.Point(100, 2);
			this.m_rbtnWorldCoord.Name = "m_rbtnWorldCoord";
			this.m_rbtnWorldCoord.Size = new System.Drawing.Size(85, 20);
			this.m_rbtnWorldCoord.TabIndex = 1;
			this.m_rbtnWorldCoord.TabStop = true;
			this.m_rbtnWorldCoord.Text = "世界座標系";
			this.m_rbtnWorldCoord.CheckedChanged += new System.EventHandler(this.RefCoord_CheckedChanged);
			// 
			// m_rbtnLocalCoord
			// 
			this.m_rbtnLocalCoord.AutoSize = true;
			this.m_rbtnLocalCoord.Location = new System.Drawing.Point(210, 2);
			this.m_rbtnLocalCoord.Name = "m_rbtnLocalCoord";
			this.m_rbtnLocalCoord.Size = new System.Drawing.Size(85, 20);
			this.m_rbtnLocalCoord.TabIndex = 2;
			this.m_rbtnLocalCoord.TabStop = true;
			this.m_rbtnLocalCoord.Text = "區域座標系";
			this.m_rbtnLocalCoord.CheckedChanged += new System.EventHandler(this.RefCoord_CheckedChanged);
			// 
			// m_flpnlToStartOrEnd
			// 
			this.m_flpnlToStartOrEnd.Controls.Add(this.m_lblToStartOrEnd);
			this.m_flpnlToStartOrEnd.Controls.Add(this.m_rbtnToStart);
			this.m_flpnlToStartOrEnd.Controls.Add(this.m_rbtnToEnd);
			this.m_flpnlToStartOrEnd.Location = new System.Drawing.Point(10, 66);
			this.m_flpnlToStartOrEnd.Name = "m_flpnlToStartOrEnd";
			this.m_flpnlToStartOrEnd.Size = new System.Drawing.Size(396, 23);
			this.m_flpnlToStartOrEnd.TabIndex = 1;
			// 
			// m_lblToStartOrEnd
			// 
			this.m_lblToStartOrEnd.AutoSize = true;
			this.m_lblToStartOrEnd.Location = new System.Drawing.Point(0, 4);
			this.m_lblToStartOrEnd.Name = "m_lblToStartOrEnd";
			this.m_lblToStartOrEnd.Size = new System.Drawing.Size(67, 16);
			this.m_lblToStartOrEnd.TabIndex = 0;
			this.m_lblToStartOrEnd.Text = "起終點切換";
			// 
			// m_rbtnToStart
			// 
			this.m_rbtnToStart.AutoSize = true;
			this.m_rbtnToStart.Location = new System.Drawing.Point(100, 2);
			this.m_rbtnToStart.Name = "m_rbtnToStart";
			this.m_rbtnToStart.Size = new System.Drawing.Size(49, 20);
			this.m_rbtnToStart.TabIndex = 1;
			this.m_rbtnToStart.TabStop = true;
			this.m_rbtnToStart.Text = "起點";
			this.m_rbtnToStart.CheckedChanged += new System.EventHandler(this.ToStartOrEnd_CheckedChanged);
			// 
			// m_rbtnToEnd
			// 
			this.m_rbtnToEnd.AutoSize = true;
			this.m_rbtnToEnd.Location = new System.Drawing.Point(210, 2);
			this.m_rbtnToEnd.Name = "m_rbtnToEnd";
			this.m_rbtnToEnd.Size = new System.Drawing.Size(49, 20);
			this.m_rbtnToEnd.TabIndex = 2;
			this.m_rbtnToEnd.TabStop = true;
			this.m_rbtnToEnd.Text = "終點";
			this.m_rbtnToEnd.CheckedChanged += new System.EventHandler(this.ToStartOrEnd_CheckedChanged);
			// 
			// m_lblUnit
			// 
			this.m_lblUnit.AutoSize = true;
			this.m_lblUnit.Location = new System.Drawing.Point(110, 100);
			this.m_lblUnit.Name = "m_lblUnit";
			this.m_lblUnit.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblUnit.Size = new System.Drawing.Size(70, 16);
			this.m_lblUnit.TabIndex = 2;
			this.m_lblUnit.Text = "(單位：mm)";
			// 
			// m_lblTotalDiff
			// 
			this.m_lblTotalDiff.AutoSize = true;
			this.m_lblTotalDiff.Location = new System.Drawing.Point(276, 100);
			this.m_lblTotalDiff.Name = "m_lblTotalDiff";
			this.m_lblTotalDiff.Size = new System.Drawing.Size(43, 16);
			this.m_lblTotalDiff.TabIndex = 3;
			this.m_lblTotalDiff.Text = "總偏移";
			// 
			// m_lblDX
			// 
			this.m_lblDX.AutoSize = true;
			this.m_lblDX.Location = new System.Drawing.Point(10, 130);
			this.m_lblDX.Name = "m_lblDX";
			this.m_lblDX.Size = new System.Drawing.Size(39, 16);
			this.m_lblDX.TabIndex = 4;
			this.m_lblDX.Text = "X偏移";
			// 
			// m_tbxDX
			// 
			this.m_tbxDX.DecimalPlaces = 3;
			this.m_tbxDX.Increment = new decimal(new int[] { 25, 0, 0, 131072});
			this.m_tbxDX.Location = new System.Drawing.Point(110, 126);
			this.m_tbxDX.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_tbxDX.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_tbxDX.Name = "m_tbxDX";
			this.m_tbxDX.RestrictInvalidCharacters = false;
			this.m_tbxDX.Size = new System.Drawing.Size(150, 23);
			this.m_tbxDX.TabIndex = 5;
			this.m_tbxDX.DebouncedValueChanged += new System.EventHandler(this.m_tbxDX_DebouncedValueChanged);
			// 
			// m_lblTotalDX
			// 
			this.m_lblTotalDX.AutoSize = true;
			this.m_lblTotalDX.Location = new System.Drawing.Point(276, 130);
			this.m_lblTotalDX.Name = "m_lblTotalDX";
			this.m_lblTotalDX.Size = new System.Drawing.Size(38, 16);
			this.m_lblTotalDX.TabIndex = 6;
			this.m_lblTotalDX.Text = "0.000";
			// 
			// m_lblDY
			// 
			this.m_lblDY.AutoSize = true;
			this.m_lblDY.Location = new System.Drawing.Point(10, 160);
			this.m_lblDY.Name = "m_lblDY";
			this.m_lblDY.Size = new System.Drawing.Size(39, 16);
			this.m_lblDY.TabIndex = 7;
			this.m_lblDY.Text = "Y偏移";
			// 
			// m_tbxDY
			// 
			this.m_tbxDY.DecimalPlaces = 3;
			this.m_tbxDY.Increment = new decimal(new int[] { 25, 0, 0, 131072});
			this.m_tbxDY.Location = new System.Drawing.Point(110, 156);
			this.m_tbxDY.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_tbxDY.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_tbxDY.Name = "m_tbxDY";
			this.m_tbxDY.RestrictInvalidCharacters = false;
			this.m_tbxDY.Size = new System.Drawing.Size(150, 23);
			this.m_tbxDY.TabIndex = 8;
			this.m_tbxDY.DebouncedValueChanged += new System.EventHandler(this.m_tbxDY_DebouncedValueChanged);
			// 
			// m_lblTotalDY
			// 
			this.m_lblTotalDY.AutoSize = true;
			this.m_lblTotalDY.Location = new System.Drawing.Point(276, 160);
			this.m_lblTotalDY.Name = "m_lblTotalDY";
			this.m_lblTotalDY.Size = new System.Drawing.Size(38, 16);
			this.m_lblTotalDY.TabIndex = 9;
			this.m_lblTotalDY.Text = "0.000";
			// 
			// m_lblDZ
			// 
			this.m_lblDZ.AutoSize = true;
			this.m_lblDZ.Location = new System.Drawing.Point(10, 190);
			this.m_lblDZ.Name = "m_lblDZ";
			this.m_lblDZ.Size = new System.Drawing.Size(39, 16);
			this.m_lblDZ.TabIndex = 10;
			this.m_lblDZ.Text = "Z偏移";
			// 
			// m_tbxDZ
			// 
			this.m_tbxDZ.DecimalPlaces = 3;
			this.m_tbxDZ.Increment = new decimal(new int[] { 25, 0, 0, 131072});
			this.m_tbxDZ.Location = new System.Drawing.Point(110, 186);
			this.m_tbxDZ.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_tbxDZ.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_tbxDZ.Name = "m_tbxDZ";
			this.m_tbxDZ.RestrictInvalidCharacters = false;
			this.m_tbxDZ.Size = new System.Drawing.Size(150, 23);
			this.m_tbxDZ.TabIndex = 11;
			this.m_tbxDZ.DebouncedValueChanged += new System.EventHandler(this.m_tbxDZ_DebouncedValueChanged);
			// 
			// m_lblTotalDZ
			// 
			this.m_lblTotalDZ.AutoSize = true;
			this.m_lblTotalDZ.Location = new System.Drawing.Point(276, 190);
			this.m_lblTotalDZ.Name = "m_lblTotalDZ";
			this.m_lblTotalDZ.Size = new System.Drawing.Size(38, 16);
			this.m_lblTotalDZ.TabIndex = 12;
			this.m_lblTotalDZ.Text = "0.000";
			// 
			// m_lblRelativeCurrentPath
			// 
			this.m_lblRelativeCurrentPath.AutoSize = true;
			this.m_lblRelativeCurrentPath.Location = new System.Drawing.Point(110, 220);
			this.m_lblRelativeCurrentPath.Name = "m_lblRelativeCurrentPath";
			this.m_lblRelativeCurrentPath.Role = CommonUI.Theme.TextRole.Secondary;
			this.m_lblRelativeCurrentPath.Size = new System.Drawing.Size(103, 16);
			this.m_lblRelativeCurrentPath.TabIndex = 13;
			this.m_lblRelativeCurrentPath.Text = "（相對當前路徑）";
			// 
			// m_btnAdd
			// 
			this.m_btnAdd.Location = new System.Drawing.Point(10, 246);
			this.m_btnAdd.Name = "m_btnAdd";
			this.m_btnAdd.Size = new System.Drawing.Size(110, 23);
			this.m_btnAdd.TabIndex = 14;
			this.m_btnAdd.Text = "新增控制點";
			this.m_btnAdd.Click += new System.EventHandler(this.m_btnAdd_Click);
			// 
			// m_btnRemove
			// 
			this.m_btnRemove.Kind = CommonUI.Controls.ButtonKind.Danger;
			this.m_btnRemove.Location = new System.Drawing.Point(128, 246);
			this.m_btnRemove.Name = "m_btnRemove";
			this.m_btnRemove.Size = new System.Drawing.Size(110, 23);
			this.m_btnRemove.TabIndex = 15;
			this.m_btnRemove.Text = "移除控制點";
			this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
			// 
			// m_btnReset
			// 
			this.m_btnReset.Location = new System.Drawing.Point(10, 276);
			this.m_btnReset.Name = "m_btnReset";
			this.m_btnReset.Size = new System.Drawing.Size(110, 23);
			this.m_btnReset.TabIndex = 16;
			this.m_btnReset.Text = "重置資料";
			this.m_btnReset.Click += new System.EventHandler(this.m_btnReset_Click);
			// 
			// m_btnClear
			// 
			this.m_btnClear.Location = new System.Drawing.Point(128, 276);
			this.m_btnClear.Name = "m_btnClear";
			this.m_btnClear.Size = new System.Drawing.Size(110, 23);
			this.m_btnClear.TabIndex = 17;
			this.m_btnClear.Text = "清除資料";
			this.m_btnClear.Click += new System.EventHandler(this.m_btnClear_Click);
			// 
			// m_btnSave
			// 
			this.m_btnSave.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnSave.Location = new System.Drawing.Point(296, 276);
			this.m_btnSave.Name = "m_btnSave";
			this.m_btnSave.Size = new System.Drawing.Size(110, 23);
			this.m_btnSave.TabIndex = 18;
			this.m_btnSave.Text = "儲存資料";
			this.m_btnSave.Click += new System.EventHandler(this.m_btnSave_Click);
			// 
			// ContourEditDlg
			// 
			this.ClientSize = new System.Drawing.Size(440, 333);
			this.Controls.Add(this.m_gbxIndexParam);
			this.Name = "ContourEditDlg";
			this.Text = "輪廓編輯";
			this.m_gbxIndexParam.ResumeLayout(false);
			this.m_gbxIndexParam.PerformLayout();
			this.m_flpnlCoord.ResumeLayout(false);
			this.m_flpnlCoord.PerformLayout();
			this.m_flpnlToStartOrEnd.ResumeLayout(false);
			this.m_flpnlToStartOrEnd.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDZ)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_gbxIndexParam;
		private System.Windows.Forms.Panel m_flpnlCoord;
		private System.Windows.Forms.Label m_lblRefCoordType;
		private System.Windows.Forms.RadioButton m_rbtnWorldCoord;
		private System.Windows.Forms.RadioButton m_rbtnLocalCoord;
		private System.Windows.Forms.Panel m_flpnlToStartOrEnd;
		private System.Windows.Forms.Label m_lblToStartOrEnd;
		private System.Windows.Forms.RadioButton m_rbtnToStart;
		private System.Windows.Forms.RadioButton m_rbtnToEnd;
		private CommonUI.Controls.ThemedLabel m_lblUnit;
		private System.Windows.Forms.Label m_lblTotalDiff;
		private System.Windows.Forms.Label m_lblDX;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxDX;
		private System.Windows.Forms.Label m_lblTotalDX;
		private System.Windows.Forms.Label m_lblDY;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxDY;
		private System.Windows.Forms.Label m_lblTotalDY;
		private System.Windows.Forms.Label m_lblDZ;
		private CommonUI.Controls.DebouncedNumericUpDown m_tbxDZ;
		private System.Windows.Forms.Label m_lblTotalDZ;
		private CommonUI.Controls.ThemedLabel m_lblRelativeCurrentPath;
		private CommonUI.Controls.ThemedButton m_btnAdd;
		private CommonUI.Controls.ThemedButton m_btnRemove;
		private CommonUI.Controls.ThemedButton m_btnReset;
		private CommonUI.Controls.ThemedButton m_btnClear;
		private CommonUI.Controls.ThemedButton m_btnSave;
	}
}
