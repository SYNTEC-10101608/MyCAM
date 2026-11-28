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
			this.m_pnlIndexParam = new CommonUI.Controls.SectionPanel();
			this.m_pnlCoord = new System.Windows.Forms.Panel();
			this.m_lblRefCoordType = new CommonUI.Controls.ThemedLabel();
			this.m_rdoWorldCoord = new System.Windows.Forms.RadioButton();
			this.m_rdoLocalCoord = new System.Windows.Forms.RadioButton();
			this.m_pnlToStartOrEnd = new System.Windows.Forms.Panel();
			this.m_lblToStartOrEnd = new CommonUI.Controls.ThemedLabel();
			this.m_rdoToStart = new System.Windows.Forms.RadioButton();
			this.m_rdoToEnd = new System.Windows.Forms.RadioButton();
			this.m_lblUnit = new CommonUI.Controls.ThemedLabel();
			this.m_lblTotalDiff = new CommonUI.Controls.ThemedLabel();
			this.m_lblDX = new CommonUI.Controls.ThemedLabel();
			this.m_numDX = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblTotalDX = new CommonUI.Controls.ThemedLabel();
			this.m_lblDY = new CommonUI.Controls.ThemedLabel();
			this.m_numDY = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblTotalDY = new CommonUI.Controls.ThemedLabel();
			this.m_lblDZ = new CommonUI.Controls.ThemedLabel();
			this.m_numDZ = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblTotalDZ = new CommonUI.Controls.ThemedLabel();
			this.m_btnAdd = new CommonUI.Controls.ThemedButton();
			this.m_btnRemove = new CommonUI.Controls.ThemedButton();
			this.m_btnReset = new CommonUI.Controls.ThemedButton();
			this.m_btnClear = new CommonUI.Controls.ThemedButton();
			this.m_btnSave = new CommonUI.Controls.ThemedButton();
			this.m_pnlIndexParam.SuspendLayout();
			this.m_pnlCoord.SuspendLayout();
			this.m_pnlToStartOrEnd.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numDX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numDY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numDZ)).BeginInit();
			this.SuspendLayout();
			// 
			// m_pnlIndexParam
			// 
			this.m_pnlIndexParam.Controls.Add(this.m_pnlCoord);
			this.m_pnlIndexParam.Controls.Add(this.m_pnlToStartOrEnd);
			this.m_pnlIndexParam.Controls.Add(this.m_lblUnit);
			this.m_pnlIndexParam.Controls.Add(this.m_lblTotalDiff);
			this.m_pnlIndexParam.Controls.Add(this.m_lblDX);
			this.m_pnlIndexParam.Controls.Add(this.m_numDX);
			this.m_pnlIndexParam.Controls.Add(this.m_lblTotalDX);
			this.m_pnlIndexParam.Controls.Add(this.m_lblDY);
			this.m_pnlIndexParam.Controls.Add(this.m_numDY);
			this.m_pnlIndexParam.Controls.Add(this.m_lblTotalDY);
			this.m_pnlIndexParam.Controls.Add(this.m_lblDZ);
			this.m_pnlIndexParam.Controls.Add(this.m_numDZ);
			this.m_pnlIndexParam.Controls.Add(this.m_lblTotalDZ);
			this.m_pnlIndexParam.Controls.Add(this.m_btnAdd);
			this.m_pnlIndexParam.Controls.Add(this.m_btnRemove);
			this.m_pnlIndexParam.Controls.Add(this.m_btnReset);
			this.m_pnlIndexParam.Controls.Add(this.m_btnClear);
			this.m_pnlIndexParam.Controls.Add(this.m_btnSave);
			this.m_pnlIndexParam.Location = new System.Drawing.Point(12, 12);
			this.m_pnlIndexParam.Name = "m_pnlIndexParam";
			this.m_pnlIndexParam.Size = new System.Drawing.Size(416, 279);
			this.m_pnlIndexParam.TabIndex = 0;
			this.m_pnlIndexParam.Text = "控制點參數";
			// 
			// m_pnlCoord
			// 
			this.m_pnlCoord.Controls.Add(this.m_lblRefCoordType);
			this.m_pnlCoord.Controls.Add(this.m_rdoWorldCoord);
			this.m_pnlCoord.Controls.Add(this.m_rdoLocalCoord);
			this.m_pnlCoord.Location = new System.Drawing.Point(10, 36);
			this.m_pnlCoord.Name = "m_pnlCoord";
			this.m_pnlCoord.Size = new System.Drawing.Size(396, 23);
			this.m_pnlCoord.TabIndex = 0;
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
			// m_rdoWorldCoord
			// 
			this.m_rdoWorldCoord.AutoSize = true;
			this.m_rdoWorldCoord.Location = new System.Drawing.Point(100, 2);
			this.m_rdoWorldCoord.Name = "m_rdoWorldCoord";
			this.m_rdoWorldCoord.Size = new System.Drawing.Size(85, 20);
			this.m_rdoWorldCoord.TabIndex = 1;
			this.m_rdoWorldCoord.TabStop = true;
			this.m_rdoWorldCoord.Text = "世界座標系";
			this.m_rdoWorldCoord.CheckedChanged += new System.EventHandler(this.RefCoord_CheckedChanged);
			// 
			// m_rdoLocalCoord
			// 
			this.m_rdoLocalCoord.AutoSize = true;
			this.m_rdoLocalCoord.Location = new System.Drawing.Point(210, 2);
			this.m_rdoLocalCoord.Name = "m_rdoLocalCoord";
			this.m_rdoLocalCoord.Size = new System.Drawing.Size(85, 20);
			this.m_rdoLocalCoord.TabIndex = 2;
			this.m_rdoLocalCoord.TabStop = true;
			this.m_rdoLocalCoord.Text = "區域座標系";
			this.m_rdoLocalCoord.CheckedChanged += new System.EventHandler(this.RefCoord_CheckedChanged);
			// 
			// m_pnlToStartOrEnd
			// 
			this.m_pnlToStartOrEnd.Controls.Add(this.m_lblToStartOrEnd);
			this.m_pnlToStartOrEnd.Controls.Add(this.m_rdoToStart);
			this.m_pnlToStartOrEnd.Controls.Add(this.m_rdoToEnd);
			this.m_pnlToStartOrEnd.Location = new System.Drawing.Point(10, 66);
			this.m_pnlToStartOrEnd.Name = "m_pnlToStartOrEnd";
			this.m_pnlToStartOrEnd.Size = new System.Drawing.Size(396, 23);
			this.m_pnlToStartOrEnd.TabIndex = 1;
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
			// m_rdoToStart
			// 
			this.m_rdoToStart.AutoSize = true;
			this.m_rdoToStart.Location = new System.Drawing.Point(100, 2);
			this.m_rdoToStart.Name = "m_rdoToStart";
			this.m_rdoToStart.Size = new System.Drawing.Size(49, 20);
			this.m_rdoToStart.TabIndex = 1;
			this.m_rdoToStart.TabStop = true;
			this.m_rdoToStart.Text = "起點";
			this.m_rdoToStart.CheckedChanged += new System.EventHandler(this.ToStartOrEnd_CheckedChanged);
			// 
			// m_rdoToEnd
			// 
			this.m_rdoToEnd.AutoSize = true;
			this.m_rdoToEnd.Location = new System.Drawing.Point(210, 2);
			this.m_rdoToEnd.Name = "m_rdoToEnd";
			this.m_rdoToEnd.Size = new System.Drawing.Size(49, 20);
			this.m_rdoToEnd.TabIndex = 2;
			this.m_rdoToEnd.TabStop = true;
			this.m_rdoToEnd.Text = "終點";
			this.m_rdoToEnd.CheckedChanged += new System.EventHandler(this.ToStartOrEnd_CheckedChanged);
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
			// m_numDX
			// 
			this.m_numDX.DecimalPlaces = 3;
			this.m_numDX.Increment = new decimal(new int[] { 25, 0, 0, 131072});
			this.m_numDX.Location = new System.Drawing.Point(110, 126);
			this.m_numDX.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numDX.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_numDX.Name = "m_numDX";
			this.m_numDX.RestrictInvalidCharacters = false;
			this.m_numDX.Size = new System.Drawing.Size(150, 23);
			this.m_numDX.TabIndex = 5;
			this.m_numDX.DebouncedValueChanged += new System.EventHandler(this.m_numDX_DebouncedValueChanged);
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
			// m_numDY
			// 
			this.m_numDY.DecimalPlaces = 3;
			this.m_numDY.Increment = new decimal(new int[] { 25, 0, 0, 131072});
			this.m_numDY.Location = new System.Drawing.Point(110, 156);
			this.m_numDY.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numDY.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_numDY.Name = "m_numDY";
			this.m_numDY.RestrictInvalidCharacters = false;
			this.m_numDY.Size = new System.Drawing.Size(150, 23);
			this.m_numDY.TabIndex = 8;
			this.m_numDY.DebouncedValueChanged += new System.EventHandler(this.m_numDY_DebouncedValueChanged);
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
			// m_numDZ
			// 
			this.m_numDZ.DecimalPlaces = 3;
			this.m_numDZ.Increment = new decimal(new int[] { 25, 0, 0, 131072});
			this.m_numDZ.Location = new System.Drawing.Point(110, 186);
			this.m_numDZ.Maximum = new decimal(new int[] { 99999, 0, 0, 0});
			this.m_numDZ.Minimum = new decimal(new int[] { 99999, 0, 0, -2147483648});
			this.m_numDZ.Name = "m_numDZ";
			this.m_numDZ.RestrictInvalidCharacters = false;
			this.m_numDZ.Size = new System.Drawing.Size(150, 23);
			this.m_numDZ.TabIndex = 11;
			this.m_numDZ.DebouncedValueChanged += new System.EventHandler(this.m_numDZ_DebouncedValueChanged);
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
			// m_btnAdd
			// 
			this.m_btnAdd.Location = new System.Drawing.Point(10, 216);
			this.m_btnAdd.Name = "m_btnAdd";
			this.m_btnAdd.Size = new System.Drawing.Size(110, 23);
			this.m_btnAdd.TabIndex = 13;
			this.m_btnAdd.Text = "新增控制點";
			this.m_btnAdd.Click += new System.EventHandler(this.m_btnAdd_Click);
			// 
			// m_btnRemove
			// 
			this.m_btnRemove.Kind = CommonUI.Controls.ButtonKind.Danger;
			this.m_btnRemove.Location = new System.Drawing.Point(128, 216);
			this.m_btnRemove.Name = "m_btnRemove";
			this.m_btnRemove.Size = new System.Drawing.Size(110, 23);
			this.m_btnRemove.TabIndex = 14;
			this.m_btnRemove.Text = "移除控制點";
			this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
			// 
			// m_btnReset
			// 
			this.m_btnReset.Location = new System.Drawing.Point(10, 246);
			this.m_btnReset.Name = "m_btnReset";
			this.m_btnReset.Size = new System.Drawing.Size(110, 23);
			this.m_btnReset.TabIndex = 15;
			this.m_btnReset.Text = "重置資料";
			this.m_btnReset.Click += new System.EventHandler(this.m_btnReset_Click);
			// 
			// m_btnClear
			// 
			this.m_btnClear.Location = new System.Drawing.Point(128, 246);
			this.m_btnClear.Name = "m_btnClear";
			this.m_btnClear.Size = new System.Drawing.Size(110, 23);
			this.m_btnClear.TabIndex = 16;
			this.m_btnClear.Text = "清除資料";
			this.m_btnClear.Click += new System.EventHandler(this.m_btnClear_Click);
			// 
			// m_btnSave
			// 
			this.m_btnSave.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnSave.Location = new System.Drawing.Point(296, 246);
			this.m_btnSave.Name = "m_btnSave";
			this.m_btnSave.Size = new System.Drawing.Size(110, 23);
			this.m_btnSave.TabIndex = 17;
			this.m_btnSave.Text = "儲存資料";
			this.m_btnSave.Click += new System.EventHandler(this.m_btnSave_Click);
			// 
			// ContourEditDlg
			// 
			this.ClientSize = new System.Drawing.Size(440, 303);
			this.Controls.Add(this.m_pnlIndexParam);
			this.Name = "ContourEditDlg";
			this.Text = "輪廓編輯";
			this.m_pnlIndexParam.ResumeLayout(false);
			this.m_pnlIndexParam.PerformLayout();
			this.m_pnlCoord.ResumeLayout(false);
			this.m_pnlCoord.PerformLayout();
			this.m_pnlToStartOrEnd.ResumeLayout(false);
			this.m_pnlToStartOrEnd.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numDX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numDY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numDZ)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlIndexParam;
		private System.Windows.Forms.Panel m_pnlCoord;
		private CommonUI.Controls.ThemedLabel m_lblRefCoordType;
		private System.Windows.Forms.RadioButton m_rdoWorldCoord;
		private System.Windows.Forms.RadioButton m_rdoLocalCoord;
		private System.Windows.Forms.Panel m_pnlToStartOrEnd;
		private CommonUI.Controls.ThemedLabel m_lblToStartOrEnd;
		private System.Windows.Forms.RadioButton m_rdoToStart;
		private System.Windows.Forms.RadioButton m_rdoToEnd;
		private CommonUI.Controls.ThemedLabel m_lblUnit;
		private CommonUI.Controls.ThemedLabel m_lblTotalDiff;
		private CommonUI.Controls.ThemedLabel m_lblDX;
		private CommonUI.Controls.DebouncedNumericUpDown m_numDX;
		private CommonUI.Controls.ThemedLabel m_lblTotalDX;
		private CommonUI.Controls.ThemedLabel m_lblDY;
		private CommonUI.Controls.DebouncedNumericUpDown m_numDY;
		private CommonUI.Controls.ThemedLabel m_lblTotalDY;
		private CommonUI.Controls.ThemedLabel m_lblDZ;
		private CommonUI.Controls.DebouncedNumericUpDown m_numDZ;
		private CommonUI.Controls.ThemedLabel m_lblTotalDZ;
		private CommonUI.Controls.ThemedButton m_btnAdd;
		private CommonUI.Controls.ThemedButton m_btnRemove;
		private CommonUI.Controls.ThemedButton m_btnReset;
		private CommonUI.Controls.ThemedButton m_btnClear;
		private CommonUI.Controls.ThemedButton m_btnSave;
	}
}
