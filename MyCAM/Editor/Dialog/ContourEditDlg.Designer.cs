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
			this.m_tableParams = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblDX = new System.Windows.Forms.Label();
			this.m_tbxDX = new System.Windows.Forms.TextBox();
			this.m_lblDY = new System.Windows.Forms.Label();
			this.m_tbxDY = new System.Windows.Forms.TextBox();
			this.m_lblDZ = new System.Windows.Forms.Label();
			this.m_tbxDZ = new System.Windows.Forms.TextBox();
			this.m_lblUnit = new System.Windows.Forms.Label();
			this.m_btnRemove = new System.Windows.Forms.Button();
			this.m_btnAdd = new System.Windows.Forms.Button();
			this.m_gbxIndexParam.SuspendLayout();
			this.m_tableParams.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_gbxIndexParam
			// 
			this.m_gbxIndexParam.Controls.Add(this.m_tableParams);
			this.m_gbxIndexParam.Controls.Add(this.m_btnRemove);
			this.m_gbxIndexParam.Controls.Add(this.m_btnAdd);
			this.m_gbxIndexParam.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_gbxIndexParam.Location = new System.Drawing.Point(9, 9);
			this.m_gbxIndexParam.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_gbxIndexParam.Name = "m_gbxIndexParam";
			this.m_gbxIndexParam.Padding = new System.Windows.Forms.Padding(7, 8, 7, 8);
			this.m_gbxIndexParam.Size = new System.Drawing.Size(251, 208);
			this.m_gbxIndexParam.TabIndex = 0;
			this.m_gbxIndexParam.TabStop = false;
			this.m_gbxIndexParam.Text = "北翴把计";
			// 
			// m_tableParams
			// 
			this.m_tableParams.ColumnCount = 2;
			this.m_tableParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this.m_tableParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableParams.Controls.Add(this.m_lblDX, 0, 0);
			this.m_tableParams.Controls.Add(this.m_tbxDX, 1, 0);
			this.m_tableParams.Controls.Add(this.m_lblDY, 0, 1);
			this.m_tableParams.Controls.Add(this.m_tbxDY, 1, 1);
			this.m_tableParams.Controls.Add(this.m_lblDZ, 0, 2);
			this.m_tableParams.Controls.Add(this.m_tbxDZ, 1, 2);
			this.m_tableParams.Controls.Add(this.m_lblUnit, 0, 3);
			this.m_tableParams.Location = new System.Drawing.Point(9, 25);
			this.m_tableParams.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_tableParams.Name = "m_tableParams";
			this.m_tableParams.RowCount = 4;
			this.m_tableParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.m_tableParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.m_tableParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.m_tableParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
			this.m_tableParams.Size = new System.Drawing.Size(228, 120);
			this.m_tableParams.TabIndex = 0;
			// 
			// m_lblDX
			// 
			this.m_lblDX.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblDX.AutoSize = true;
			this.m_lblDX.Location = new System.Drawing.Point(4, 8);
			this.m_lblDX.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblDX.Name = "m_lblDX";
			this.m_lblDX.Size = new System.Drawing.Size(27, 15);
			this.m_lblDX.TabIndex = 0;
			this.m_lblDX.Text = "DX";
			// 
			// m_tbxDX
			// 
			this.m_tbxDX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tbxDX.Location = new System.Drawing.Point(49, 4);
			this.m_tbxDX.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_tbxDX.Name = "m_tbxDX";
			this.m_tbxDX.Size = new System.Drawing.Size(175, 25);
			this.m_tbxDX.TabIndex = 1;
			this.m_tbxDX.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxDX_KeyDown);
			this.m_tbxDX.Leave += new System.EventHandler(this.m_tbxDX_Leave);
			// 
			// m_lblDY
			// 
			this.m_lblDY.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblDY.AutoSize = true;
			this.m_lblDY.Location = new System.Drawing.Point(4, 40);
			this.m_lblDY.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblDY.Name = "m_lblDY";
			this.m_lblDY.Size = new System.Drawing.Size(27, 15);
			this.m_lblDY.TabIndex = 2;
			this.m_lblDY.Text = "DY";
			// 
			// m_tbxDY
			// 
			this.m_tbxDY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tbxDY.Location = new System.Drawing.Point(49, 36);
			this.m_tbxDY.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_tbxDY.Name = "m_tbxDY";
			this.m_tbxDY.Size = new System.Drawing.Size(175, 25);
			this.m_tbxDY.TabIndex = 3;
			this.m_tbxDY.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxDY_KeyDown);
			this.m_tbxDY.Leave += new System.EventHandler(this.m_tbxDY_Leave);
			// 
			// m_lblDZ
			// 
			this.m_lblDZ.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblDZ.AutoSize = true;
			this.m_lblDZ.Location = new System.Drawing.Point(4, 72);
			this.m_lblDZ.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblDZ.Name = "m_lblDZ";
			this.m_lblDZ.Size = new System.Drawing.Size(26, 15);
			this.m_lblDZ.TabIndex = 4;
			this.m_lblDZ.Text = "DZ";
			// 
			// m_tbxDZ
			// 
			this.m_tbxDZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tbxDZ.Location = new System.Drawing.Point(49, 68);
			this.m_tbxDZ.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_tbxDZ.Name = "m_tbxDZ";
			this.m_tbxDZ.Size = new System.Drawing.Size(175, 25);
			this.m_tbxDZ.TabIndex = 5;
			this.m_tbxDZ.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_tbxDZ_KeyDown);
			this.m_tbxDZ.Leave += new System.EventHandler(this.m_tbxDZ_Leave);
			// 
			// m_lblUnit
			// 
			this.m_lblUnit.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblUnit.AutoSize = true;
			this.m_tableParams.SetColumnSpan(this.m_lblUnit, 2);
			this.m_lblUnit.ForeColor = System.Drawing.SystemColors.GrayText;
			this.m_lblUnit.Location = new System.Drawing.Point(4, 100);
			this.m_lblUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblUnit.Name = "m_lblUnit";
			this.m_lblUnit.Size = new System.Drawing.Size(74, 15);
			this.m_lblUnit.TabIndex = 6;
			this.m_lblUnit.Text = "虫mm";
			// 
			// m_btnRemove
			// 
			this.m_btnRemove.Location = new System.Drawing.Point(125, 155);
			this.m_btnRemove.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_btnRemove.Name = "m_btnRemove";
			this.m_btnRemove.Size = new System.Drawing.Size(109, 35);
			this.m_btnRemove.TabIndex = 7;
			this.m_btnRemove.Text = "埃北翴";
			this.m_btnRemove.UseVisualStyleBackColor = true;
			this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
			// 
			// m_btnAdd
			// 
			this.m_btnAdd.Location = new System.Drawing.Point(9, 155);
			this.m_btnAdd.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.m_btnAdd.Name = "m_btnAdd";
			this.m_btnAdd.Size = new System.Drawing.Size(109, 35);
			this.m_btnAdd.TabIndex = 6;
			this.m_btnAdd.Text = "穝糤北翴";
			this.m_btnAdd.UseVisualStyleBackColor = true;
			this.m_btnAdd.Click += new System.EventHandler(this.m_btnAdd_Click);
			// 
			// ContourEditDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(269, 226);
			this.Controls.Add(this.m_gbxIndexParam);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.Name = "ContourEditDlg";
			this.Padding = new System.Windows.Forms.Padding(9, 9, 9, 9);
			this.Text = "隔畖翴Ы场熬簿";
			this.TopMost = true;
			this.m_gbxIndexParam.ResumeLayout(false);
			this.m_tableParams.ResumeLayout(false);
			this.m_tableParams.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        System.Windows.Forms.GroupBox m_gbxIndexParam;
        System.Windows.Forms.Button m_btnAdd;
        System.Windows.Forms.Button m_btnRemove;
        System.Windows.Forms.TableLayoutPanel m_tableParams;
        System.Windows.Forms.Label m_lblDX;
        System.Windows.Forms.TextBox m_tbxDX;
        System.Windows.Forms.Label m_lblDY;
        System.Windows.Forms.TextBox m_tbxDY;
        System.Windows.Forms.Label m_lblDZ;
        System.Windows.Forms.TextBox m_tbxDZ;
        System.Windows.Forms.Label m_lblUnit;
    }
}
