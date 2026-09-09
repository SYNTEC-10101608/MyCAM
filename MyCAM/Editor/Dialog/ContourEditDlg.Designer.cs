namespace MyCAM.Editor
{
    partial class ContourEditDlg
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose( bool disposing )
        {
            if( disposing && ( components != null ) ) {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
			this.m_gbxIndexParam = new System.Windows.Forms.GroupBox();
			this.m_pnlContourEdit = new System.Windows.Forms.TableLayoutPanel();
			this.m_flpnlCoord = new System.Windows.Forms.FlowLayoutPanel();
			this.m_lblRefCoordType = new System.Windows.Forms.Label();
			this.m_rbtnWorldCoord = new System.Windows.Forms.RadioButton();
			this.m_rbtnLocalCoord = new System.Windows.Forms.RadioButton();
			this.m_flpnlToStartOrEnd = new System.Windows.Forms.FlowLayoutPanel();
			this.m_lblToStartOrEnd = new System.Windows.Forms.Label();
			this.m_rbtnToStart = new System.Windows.Forms.RadioButton();
			this.m_rbtnToEnd = new System.Windows.Forms.RadioButton();
			this.m_flpnlResetClear = new System.Windows.Forms.FlowLayoutPanel();
			this.m_btnClear = new System.Windows.Forms.Button();
			this.m_btnReset = new System.Windows.Forms.Button();
			this.m_flpnlAddRemove = new System.Windows.Forms.FlowLayoutPanel();
			this.m_btnRemove = new System.Windows.Forms.Button();
			this.m_btnAdd = new System.Windows.Forms.Button();
			this.m_tableParams = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblDX = new System.Windows.Forms.Label();
			this.m_tbxDX = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblDY = new System.Windows.Forms.Label();
			this.m_tbxDY = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblDZ = new System.Windows.Forms.Label();
			this.m_tbxDZ = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblUnit = new System.Windows.Forms.Label();
			this.m_gbxIndexParam.SuspendLayout();
			this.m_pnlContourEdit.SuspendLayout();
			this.m_flpnlCoord.SuspendLayout();
			this.m_flpnlToStartOrEnd.SuspendLayout();
			this.m_flpnlResetClear.SuspendLayout();
			this.m_flpnlAddRemove.SuspendLayout();
			this.m_tableParams.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDZ)).BeginInit();
			this.SuspendLayout();
			// 
			// m_gbxIndexParam
			// 
			this.m_gbxIndexParam.AutoSize = true;
			this.m_gbxIndexParam.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_gbxIndexParam.Controls.Add(this.m_pnlContourEdit);
			this.m_gbxIndexParam.Location = new System.Drawing.Point(13, 13);
			this.m_gbxIndexParam.Margin = new System.Windows.Forms.Padding(4);
			this.m_gbxIndexParam.Name = "m_gbxIndexParam";
			this.m_gbxIndexParam.Padding = new System.Windows.Forms.Padding(7, 8, 7, 8);
			this.m_gbxIndexParam.Size = new System.Drawing.Size(388, 354);
			this.m_gbxIndexParam.TabIndex = 0;
			this.m_gbxIndexParam.TabStop = false;
			this.m_gbxIndexParam.Text = "控制點參數";
			// 
			// m_pnlContourEdit
			// 
			this.m_pnlContourEdit.AutoSize = true;
			this.m_pnlContourEdit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_pnlContourEdit.ColumnCount = 1;
			this.m_pnlContourEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_pnlContourEdit.Controls.Add(this.m_flpnlCoord, 0, 0);
			this.m_pnlContourEdit.Controls.Add(this.m_flpnlToStartOrEnd, 0, 1);
			this.m_pnlContourEdit.Controls.Add(this.m_flpnlResetClear, 0, 3);
			this.m_pnlContourEdit.Controls.Add(this.m_flpnlAddRemove, 0, 4);
			this.m_pnlContourEdit.Controls.Add(this.m_tableParams, 0, 2);
			this.m_pnlContourEdit.Location = new System.Drawing.Point(10, 29);
			this.m_pnlContourEdit.Name = "m_pnlContourEdit";
			this.m_pnlContourEdit.RowCount = 5;
			this.m_pnlContourEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.m_pnlContourEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.m_pnlContourEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlContourEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.m_pnlContourEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.m_pnlContourEdit.Size = new System.Drawing.Size(368, 296);
			this.m_pnlContourEdit.TabIndex = 11;
			// 
			// m_flpnlCoord
			// 
			this.m_flpnlCoord.AutoSize = true;
			this.m_flpnlCoord.Controls.Add(this.m_lblRefCoordType);
			this.m_flpnlCoord.Controls.Add(this.m_rbtnWorldCoord);
			this.m_flpnlCoord.Controls.Add(this.m_rbtnLocalCoord);
			this.m_flpnlCoord.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_flpnlCoord.Location = new System.Drawing.Point(3, 3);
			this.m_flpnlCoord.Name = "m_flpnlCoord";
			this.m_flpnlCoord.Size = new System.Drawing.Size(362, 29);
			this.m_flpnlCoord.TabIndex = 0;
			// 
			// m_lblRefCoordType
			// 
			this.m_lblRefCoordType.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblRefCoordType.AutoSize = true;
			this.m_lblRefCoordType.Location = new System.Drawing.Point(3, 5);
			this.m_lblRefCoordType.Name = "m_lblRefCoordType";
			this.m_lblRefCoordType.Size = new System.Drawing.Size(52, 15);
			this.m_lblRefCoordType.TabIndex = 8;
			this.m_lblRefCoordType.Text = "座標系";
			// 
			// m_rbtnWorldCoord
			// 
			this.m_rbtnWorldCoord.AutoSize = true;
			this.m_rbtnWorldCoord.Location = new System.Drawing.Point(61, 3);
			this.m_rbtnWorldCoord.Name = "m_rbtnWorldCoord";
			this.m_rbtnWorldCoord.Size = new System.Drawing.Size(103, 19);
			this.m_rbtnWorldCoord.TabIndex = 9;
			this.m_rbtnWorldCoord.TabStop = true;
			this.m_rbtnWorldCoord.Text = "世界座標系";
			this.m_rbtnWorldCoord.UseVisualStyleBackColor = true;
			this.m_rbtnWorldCoord.CheckedChanged += new System.EventHandler(this.RefCoord_CheckedChanged);
			// 
			// m_rbtnLocalCoord
			// 
			this.m_rbtnLocalCoord.AutoSize = true;
			this.m_rbtnLocalCoord.Location = new System.Drawing.Point(170, 3);
			this.m_rbtnLocalCoord.Name = "m_rbtnLocalCoord";
			this.m_rbtnLocalCoord.Size = new System.Drawing.Size(103, 19);
			this.m_rbtnLocalCoord.TabIndex = 10;
			this.m_rbtnLocalCoord.TabStop = true;
			this.m_rbtnLocalCoord.Text = "區域座標系";
			this.m_rbtnLocalCoord.UseVisualStyleBackColor = true;
			this.m_rbtnLocalCoord.CheckedChanged += new System.EventHandler(this.RefCoord_CheckedChanged);
			// 
			// m_flpnlToStartOrEnd
			// 
			this.m_flpnlToStartOrEnd.AutoSize = true;
			this.m_flpnlToStartOrEnd.Controls.Add(this.m_lblToStartOrEnd);
			this.m_flpnlToStartOrEnd.Controls.Add(this.m_rbtnToStart);
			this.m_flpnlToStartOrEnd.Controls.Add(this.m_rbtnToEnd);
			this.m_flpnlToStartOrEnd.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_flpnlToStartOrEnd.Location = new System.Drawing.Point(3, 38);
			this.m_flpnlToStartOrEnd.Name = "m_flpnlToStartOrEnd";
			this.m_flpnlToStartOrEnd.Size = new System.Drawing.Size(362, 29);
			this.m_flpnlToStartOrEnd.TabIndex = 7;
			// 
			// m_lblToStartOrEnd
			// 
			this.m_lblToStartOrEnd.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblToStartOrEnd.AutoSize = true;
			this.m_lblToStartOrEnd.Location = new System.Drawing.Point(3, 5);
			this.m_lblToStartOrEnd.Name = "m_lblToStartOrEnd";
			this.m_lblToStartOrEnd.Size = new System.Drawing.Size(82, 15);
			this.m_lblToStartOrEnd.TabIndex = 0;
			this.m_lblToStartOrEnd.Text = "起終點切換";
			// 
			// m_rbtnToStart
			// 
			this.m_rbtnToStart.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_rbtnToStart.AutoSize = true;
			this.m_rbtnToStart.Location = new System.Drawing.Point(91, 3);
			this.m_rbtnToStart.Name = "m_rbtnToStart";
			this.m_rbtnToStart.Size = new System.Drawing.Size(58, 19);
			this.m_rbtnToStart.TabIndex = 1;
			this.m_rbtnToStart.TabStop = true;
			this.m_rbtnToStart.Text = "起點";
			this.m_rbtnToStart.UseVisualStyleBackColor = true;
			this.m_rbtnToStart.CheckedChanged += new System.EventHandler(this.ToStartOrEnd_CheckedChanged);
			// 
			// m_rbtnToEnd
			// 
			this.m_rbtnToEnd.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_rbtnToEnd.AutoSize = true;
			this.m_rbtnToEnd.Location = new System.Drawing.Point(155, 3);
			this.m_rbtnToEnd.Name = "m_rbtnToEnd";
			this.m_rbtnToEnd.Size = new System.Drawing.Size(58, 19);
			this.m_rbtnToEnd.TabIndex = 2;
			this.m_rbtnToEnd.TabStop = true;
			this.m_rbtnToEnd.Text = "終點";
			this.m_rbtnToEnd.UseVisualStyleBackColor = true;
			this.m_rbtnToEnd.CheckedChanged += new System.EventHandler(this.ToStartOrEnd_CheckedChanged);
			// 
			// m_flpnlResetClear
			// 
			this.m_flpnlResetClear.AutoSize = true;
			this.m_flpnlResetClear.Controls.Add(this.m_btnClear);
			this.m_flpnlResetClear.Controls.Add(this.m_btnReset);
			this.m_flpnlResetClear.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_flpnlResetClear.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.m_flpnlResetClear.Location = new System.Drawing.Point(3, 199);
			this.m_flpnlResetClear.Name = "m_flpnlResetClear";
			this.m_flpnlResetClear.Size = new System.Drawing.Size(362, 44);
			this.m_flpnlResetClear.TabIndex = 8;
			// 
			// m_btnClear
			// 
			this.m_btnClear.AutoSize = true;
			this.m_btnClear.Location = new System.Drawing.Point(249, 4);
			this.m_btnClear.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnClear.Name = "m_btnClear";
			this.m_btnClear.Size = new System.Drawing.Size(109, 35);
			this.m_btnClear.TabIndex = 10;
			this.m_btnClear.Text = "清除資料";
			this.m_btnClear.UseVisualStyleBackColor = true;
			this.m_btnClear.Click += new System.EventHandler(this.m_btnClear_Click);
			// 
			// m_btnReset
			// 
			this.m_btnReset.AutoSize = true;
			this.m_btnReset.Location = new System.Drawing.Point(132, 4);
			this.m_btnReset.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnReset.Name = "m_btnReset";
			this.m_btnReset.Size = new System.Drawing.Size(109, 35);
			this.m_btnReset.TabIndex = 9;
			this.m_btnReset.Text = "重置資料";
			this.m_btnReset.UseVisualStyleBackColor = true;
			this.m_btnReset.Click += new System.EventHandler(this.m_btnReset_Click);
			// 
			// m_flpnlAddRemove
			// 
			this.m_flpnlAddRemove.AutoSize = true;
			this.m_flpnlAddRemove.Controls.Add(this.m_btnRemove);
			this.m_flpnlAddRemove.Controls.Add(this.m_btnAdd);
			this.m_flpnlAddRemove.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_flpnlAddRemove.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.m_flpnlAddRemove.Location = new System.Drawing.Point(3, 249);
			this.m_flpnlAddRemove.Name = "m_flpnlAddRemove";
			this.m_flpnlAddRemove.Size = new System.Drawing.Size(362, 44);
			this.m_flpnlAddRemove.TabIndex = 9;
			// 
			// m_btnRemove
			// 
			this.m_btnRemove.AutoSize = true;
			this.m_btnRemove.Location = new System.Drawing.Point(249, 4);
			this.m_btnRemove.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnRemove.Name = "m_btnRemove";
			this.m_btnRemove.Size = new System.Drawing.Size(109, 35);
			this.m_btnRemove.TabIndex = 7;
			this.m_btnRemove.Text = "移除控制點";
			this.m_btnRemove.UseVisualStyleBackColor = true;
			this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
			// 
			// m_btnAdd
			// 
			this.m_btnAdd.AutoSize = true;
			this.m_btnAdd.Location = new System.Drawing.Point(132, 4);
			this.m_btnAdd.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnAdd.Name = "m_btnAdd";
			this.m_btnAdd.Size = new System.Drawing.Size(109, 35);
			this.m_btnAdd.TabIndex = 6;
			this.m_btnAdd.Text = "新增控制點";
			this.m_btnAdd.UseVisualStyleBackColor = true;
			this.m_btnAdd.Click += new System.EventHandler(this.m_btnAdd_Click);
			// 
			// m_tableParams
			// 
			this.m_tableParams.AutoSize = true;
			this.m_tableParams.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_tableParams.ColumnCount = 2;
			this.m_tableParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.m_tableParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
			this.m_tableParams.Controls.Add(this.m_lblDX, 0, 0);
			this.m_tableParams.Controls.Add(this.m_tbxDX, 1, 0);
			this.m_tableParams.Controls.Add(this.m_lblDY, 0, 1);
			this.m_tableParams.Controls.Add(this.m_tbxDY, 1, 1);
			this.m_tableParams.Controls.Add(this.m_lblDZ, 0, 2);
			this.m_tableParams.Controls.Add(this.m_tbxDZ, 1, 2);
			this.m_tableParams.Controls.Add(this.m_lblUnit, 0, 3);
			this.m_tableParams.Location = new System.Drawing.Point(4, 74);
			this.m_tableParams.Margin = new System.Windows.Forms.Padding(4);
			this.m_tableParams.Name = "m_tableParams";
			this.m_tableParams.RowCount = 4;
			this.m_tableParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.m_tableParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.m_tableParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.m_tableParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
			this.m_tableParams.Size = new System.Drawing.Size(360, 118);
			this.m_tableParams.TabIndex = 0;
			// 
			// m_lblDX
			// 
			this.m_lblDX.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblDX.AutoSize = true;
			this.m_lblDX.Location = new System.Drawing.Point(4, 8);
			this.m_lblDX.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblDX.Name = "m_lblDX";
			this.m_lblDX.Size = new System.Drawing.Size(47, 15);
			this.m_lblDX.TabIndex = 0;
			this.m_lblDX.Text = "X偏移";
			// 
			// m_tbxDX
			// 
			this.m_tbxDX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tbxDX.AutoSize = true;
			this.m_tbxDX.DecimalPlaces = 3;
			this.m_tbxDX.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
			this.m_tbxDX.Location = new System.Drawing.Point(64, 4);
			this.m_tbxDX.Margin = new System.Windows.Forms.Padding(4);
			this.m_tbxDX.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_tbxDX.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_tbxDX.Name = "m_tbxDX";
			this.m_tbxDX.RestrictInvalidCharacters = false;
			this.m_tbxDX.Size = new System.Drawing.Size(292, 25);
			this.m_tbxDX.TabIndex = 1;
			this.m_tbxDX.DebouncedValueChanged += new System.EventHandler(this.m_tbxDX_DebouncedValueChanged);
			// 
			// m_lblDY
			// 
			this.m_lblDY.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblDY.AutoSize = true;
			this.m_lblDY.Location = new System.Drawing.Point(4, 40);
			this.m_lblDY.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblDY.Name = "m_lblDY";
			this.m_lblDY.Size = new System.Drawing.Size(47, 15);
			this.m_lblDY.TabIndex = 2;
			this.m_lblDY.Text = "Y偏移";
			// 
			// m_tbxDY
			// 
			this.m_tbxDY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tbxDY.AutoSize = true;
			this.m_tbxDY.DecimalPlaces = 3;
			this.m_tbxDY.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
			this.m_tbxDY.Location = new System.Drawing.Point(64, 36);
			this.m_tbxDY.Margin = new System.Windows.Forms.Padding(4);
			this.m_tbxDY.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_tbxDY.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_tbxDY.Name = "m_tbxDY";
			this.m_tbxDY.RestrictInvalidCharacters = false;
			this.m_tbxDY.Size = new System.Drawing.Size(292, 25);
			this.m_tbxDY.TabIndex = 3;
			this.m_tbxDY.DebouncedValueChanged += new System.EventHandler(this.m_tbxDY_DebouncedValueChanged);
			// 
			// m_lblDZ
			// 
			this.m_lblDZ.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblDZ.AutoSize = true;
			this.m_lblDZ.Location = new System.Drawing.Point(4, 72);
			this.m_lblDZ.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblDZ.Name = "m_lblDZ";
			this.m_lblDZ.Size = new System.Drawing.Size(46, 15);
			this.m_lblDZ.TabIndex = 4;
			this.m_lblDZ.Text = "Z偏移";
			// 
			// m_tbxDZ
			// 
			this.m_tbxDZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tbxDZ.AutoSize = true;
			this.m_tbxDZ.DecimalPlaces = 3;
			this.m_tbxDZ.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
			this.m_tbxDZ.Location = new System.Drawing.Point(64, 68);
			this.m_tbxDZ.Margin = new System.Windows.Forms.Padding(4);
			this.m_tbxDZ.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_tbxDZ.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_tbxDZ.Name = "m_tbxDZ";
			this.m_tbxDZ.RestrictInvalidCharacters = false;
			this.m_tbxDZ.Size = new System.Drawing.Size(292, 25);
			this.m_tbxDZ.TabIndex = 5;
			this.m_tbxDZ.DebouncedValueChanged += new System.EventHandler(this.m_tbxDZ_DebouncedValueChanged);
			// 
			// m_lblUnit
			// 
			this.m_lblUnit.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblUnit.AutoSize = true;
			this.m_tableParams.SetColumnSpan(this.m_lblUnit, 2);
			this.m_lblUnit.ForeColor = System.Drawing.SystemColors.GrayText;
			this.m_lblUnit.Location = new System.Drawing.Point(4, 99);
			this.m_lblUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblUnit.Name = "m_lblUnit";
			this.m_lblUnit.Size = new System.Drawing.Size(84, 15);
			this.m_lblUnit.TabIndex = 6;
			this.m_lblUnit.Text = "(單位：mm)";
			// 
			// ContourEditDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(471, 431);
			this.Controls.Add(this.m_gbxIndexParam);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "ContourEditDlg";
			this.Padding = new System.Windows.Forms.Padding(9);
			this.Text = "輪廓編輯";
			this.m_gbxIndexParam.ResumeLayout(false);
			this.m_gbxIndexParam.PerformLayout();
			this.m_pnlContourEdit.ResumeLayout(false);
			this.m_pnlContourEdit.PerformLayout();
			this.m_flpnlCoord.ResumeLayout(false);
			this.m_flpnlCoord.PerformLayout();
			this.m_flpnlToStartOrEnd.ResumeLayout(false);
			this.m_flpnlToStartOrEnd.PerformLayout();
			this.m_flpnlResetClear.ResumeLayout(false);
			this.m_flpnlResetClear.PerformLayout();
			this.m_flpnlAddRemove.ResumeLayout(false);
			this.m_flpnlAddRemove.PerformLayout();
			this.m_tableParams.ResumeLayout(false);
			this.m_tableParams.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDZ)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        System.Windows.Forms.GroupBox m_gbxIndexParam;
        System.Windows.Forms.Button m_btnAdd;
        System.Windows.Forms.Button m_btnRemove;
		private System.Windows.Forms.Label m_lblRefCoordType;
		private System.Windows.Forms.Button m_btnClear;
		private System.Windows.Forms.Button m_btnReset;
		private System.Windows.Forms.FlowLayoutPanel m_flpnlToStartOrEnd;
		private System.Windows.Forms.Label m_lblToStartOrEnd;
		private System.Windows.Forms.RadioButton m_rbtnToStart;
		private System.Windows.Forms.RadioButton m_rbtnToEnd;
		private System.Windows.Forms.TableLayoutPanel m_pnlContourEdit;
		private System.Windows.Forms.FlowLayoutPanel m_flpnlCoord;
		private System.Windows.Forms.RadioButton m_rbtnWorldCoord;
		private System.Windows.Forms.RadioButton m_rbtnLocalCoord;
		private System.Windows.Forms.FlowLayoutPanel m_flpnlResetClear;
		private System.Windows.Forms.FlowLayoutPanel m_flpnlAddRemove;
		private System.Windows.Forms.TableLayoutPanel m_tableParams;
		private System.Windows.Forms.Label m_lblDX;
		private DebouncedNumericUpDown m_tbxDX;
		private System.Windows.Forms.Label m_lblDY;
		private DebouncedNumericUpDown m_tbxDY;
		private System.Windows.Forms.Label m_lblDZ;
		private DebouncedNumericUpDown m_tbxDZ;
		private System.Windows.Forms.Label m_lblUnit;
	}
}
