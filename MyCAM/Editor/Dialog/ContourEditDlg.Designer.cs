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
			this.m_btnClear = new System.Windows.Forms.Button();
			this.m_btnReset = new System.Windows.Forms.Button();
			this.m_lblRefCoordType = new System.Windows.Forms.Label();
			this.m_tableParams = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblDX = new System.Windows.Forms.Label();
			this.m_tbxDX = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblDY = new System.Windows.Forms.Label();
			this.m_tbxDY = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblDZ = new System.Windows.Forms.Label();
			this.m_tbxDZ = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_lblUnit = new System.Windows.Forms.Label();
			this.m_btnRemove = new System.Windows.Forms.Button();
			this.m_btnAdd = new System.Windows.Forms.Button();
			this.m_cmbRefCoordType = new System.Windows.Forms.ComboBox();
			this.m_gbxIndexParam.SuspendLayout();
			this.m_tableParams.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDZ)).BeginInit();
			this.SuspendLayout();
			// 
			// m_gbxIndexParam
			// 
			this.m_gbxIndexParam.Controls.Add(this.m_btnClear);
			this.m_gbxIndexParam.Controls.Add(this.m_btnReset);
			this.m_gbxIndexParam.Controls.Add(this.m_lblRefCoordType);
			this.m_gbxIndexParam.Controls.Add(this.m_tableParams);
			this.m_gbxIndexParam.Controls.Add(this.m_btnRemove);
			this.m_gbxIndexParam.Controls.Add(this.m_btnAdd);
			this.m_gbxIndexParam.Controls.Add(this.m_cmbRefCoordType);
			this.m_gbxIndexParam.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_gbxIndexParam.Location = new System.Drawing.Point(9, 9);
			this.m_gbxIndexParam.Margin = new System.Windows.Forms.Padding(4);
			this.m_gbxIndexParam.Name = "m_gbxIndexParam";
			this.m_gbxIndexParam.Padding = new System.Windows.Forms.Padding(7, 8, 7, 8);
			this.m_gbxIndexParam.Size = new System.Drawing.Size(330, 295);
			this.m_gbxIndexParam.TabIndex = 0;
			this.m_gbxIndexParam.TabStop = false;
			this.m_gbxIndexParam.Text = "控制點參數";
			// 
			// m_btnClear
			// 
			this.m_btnClear.Location = new System.Drawing.Point(206, 205);
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
			this.m_btnReset.Location = new System.Drawing.Point(89, 205);
			this.m_btnReset.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnReset.Name = "m_btnReset";
			this.m_btnReset.Size = new System.Drawing.Size(109, 35);
			this.m_btnReset.TabIndex = 9;
			this.m_btnReset.Text = "重置資料";
			this.m_btnReset.UseVisualStyleBackColor = true;
			this.m_btnReset.Click += new System.EventHandler(this.m_btnReset_Click);
			// 
			// m_lblRefCoordType
			// 
			this.m_lblRefCoordType.AutoSize = true;
			this.m_lblRefCoordType.Location = new System.Drawing.Point(15, 31);
			this.m_lblRefCoordType.Name = "m_lblRefCoordType";
			this.m_lblRefCoordType.Size = new System.Drawing.Size(52, 15);
			this.m_lblRefCoordType.TabIndex = 8;
			this.m_lblRefCoordType.Text = "座標系";
			// 
			// m_tableParams
			// 
			this.m_tableParams.ColumnCount = 2;
			this.m_tableParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.m_tableParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tableParams.Controls.Add(this.m_lblDX, 0, 0);
			this.m_tableParams.Controls.Add(this.m_tbxDX, 1, 0);
			this.m_tableParams.Controls.Add(this.m_lblDY, 0, 1);
			this.m_tableParams.Controls.Add(this.m_tbxDY, 1, 1);
			this.m_tableParams.Controls.Add(this.m_lblDZ, 0, 2);
			this.m_tableParams.Controls.Add(this.m_tbxDZ, 1, 2);
			this.m_tableParams.Controls.Add(this.m_lblUnit, 0, 3);
			this.m_tableParams.Location = new System.Drawing.Point(11, 72);
			this.m_tableParams.Margin = new System.Windows.Forms.Padding(4);
			this.m_tableParams.Name = "m_tableParams";
			this.m_tableParams.RowCount = 5;
			this.m_tableParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.m_tableParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.m_tableParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.m_tableParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
			this.m_tableParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tableParams.Size = new System.Drawing.Size(309, 120);
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
			this.m_tbxDX.Size = new System.Drawing.Size(241, 25);
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
			this.m_tbxDY.Size = new System.Drawing.Size(241, 25);
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
			this.m_tbxDZ.Size = new System.Drawing.Size(241, 25);
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
			this.m_lblUnit.Size = new System.Drawing.Size(74, 15);
			this.m_lblUnit.TabIndex = 6;
			this.m_lblUnit.Text = "單位：mm";
			// 
			// m_btnRemove
			// 
			this.m_btnRemove.Location = new System.Drawing.Point(206, 248);
			this.m_btnRemove.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnRemove.Name = "m_btnRemove";
			this.m_btnRemove.Size = new System.Drawing.Size(109, 35);
			this.m_btnRemove.TabIndex = 7;
			this.m_btnRemove.Text = "刪除控制點";
			this.m_btnRemove.UseVisualStyleBackColor = true;
			this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
			// 
			// m_btnAdd
			// 
			this.m_btnAdd.Location = new System.Drawing.Point(89, 248);
			this.m_btnAdd.Margin = new System.Windows.Forms.Padding(4);
			this.m_btnAdd.Name = "m_btnAdd";
			this.m_btnAdd.Size = new System.Drawing.Size(109, 35);
			this.m_btnAdd.TabIndex = 6;
			this.m_btnAdd.Text = "新增控制點";
			this.m_btnAdd.UseVisualStyleBackColor = true;
			this.m_btnAdd.Click += new System.EventHandler(this.m_btnAdd_Click);
			// 
			// m_cmbRefCoordType
			// 
			this.m_cmbRefCoordType.FormattingEnabled = true;
			this.m_cmbRefCoordType.Location = new System.Drawing.Point(75, 28);
			this.m_cmbRefCoordType.Name = "m_cmbRefCoordType";
			this.m_cmbRefCoordType.Size = new System.Drawing.Size(228, 23);
			this.m_cmbRefCoordType.TabIndex = 7;
			this.m_cmbRefCoordType.SelectedIndexChanged += new System.EventHandler(this.m_cmbRefCoordType_SelectedIndexChanged);
			// 
			// ContourEditDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(348, 313);
			this.Controls.Add(this.m_gbxIndexParam);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "ContourEditDlg";
			this.Padding = new System.Windows.Forms.Padding(9);
			this.Text = "路徑點局部偏移";
			this.m_gbxIndexParam.ResumeLayout(false);
			this.m_gbxIndexParam.PerformLayout();
			this.m_tableParams.ResumeLayout(false);
			this.m_tableParams.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_tbxDZ)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        System.Windows.Forms.GroupBox m_gbxIndexParam;
        System.Windows.Forms.Button m_btnAdd;
        System.Windows.Forms.Button m_btnRemove;
        System.Windows.Forms.TableLayoutPanel m_tableParams;
		System.Windows.Forms.Label m_lblDX;
		MyCAM.Editor.DebouncedNumericUpDown m_tbxDX;
		System.Windows.Forms.Label m_lblDY;
		MyCAM.Editor.DebouncedNumericUpDown m_tbxDY;
		System.Windows.Forms.Label m_lblDZ;
		MyCAM.Editor.DebouncedNumericUpDown m_tbxDZ;
        System.Windows.Forms.Label m_lblUnit;
		private System.Windows.Forms.ComboBox m_cmbRefCoordType;
		private System.Windows.Forms.Label m_lblRefCoordType;
		private System.Windows.Forms.Button m_btnClear;
		private System.Windows.Forms.Button m_btnReset;
	}
}
