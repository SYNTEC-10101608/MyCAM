namespace MyCAM.Editor.Dialog
{
	partial class PathCompensateDlg
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
			this.m_NumUpDownCompensate = new MyCAM.Editor.DebouncedNumericUpDown();
			this.m_btnConfirm = new System.Windows.Forms.Button();
			this.m_btnClear = new System.Windows.Forms.Button();
			this.m_btnReset = new System.Windows.Forms.Button();
			this.m_pnlCompemsateParam = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblTotalCompensateValue = new System.Windows.Forms.Label();
			this.m_lblTotalCompensate = new System.Windows.Forms.Label();
			this.m_lblCompensate = new System.Windows.Forms.Label();
			this.m_pnlCompemsate = new System.Windows.Forms.TableLayoutPanel();
			this.m_flpnlEdit = new System.Windows.Forms.FlowLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.m_NumUpDownCompensate)).BeginInit();
			this.m_pnlCompemsateParam.SuspendLayout();
			this.m_pnlCompemsate.SuspendLayout();
			this.m_flpnlEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_NumUpDownCompensate
			// 
			this.m_NumUpDownCompensate.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.m_NumUpDownCompensate.DecimalPlaces = 3;
			this.m_NumUpDownCompensate.Location = new System.Drawing.Point(154, 35);
			this.m_NumUpDownCompensate.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.m_NumUpDownCompensate.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
			this.m_NumUpDownCompensate.Name = "m_NumUpDownCompensate";
			this.m_NumUpDownCompensate.RestrictInvalidCharacters = false;
			this.m_NumUpDownCompensate.Size = new System.Drawing.Size(145, 27);
			this.m_NumUpDownCompensate.TabIndex = 1;
			this.m_NumUpDownCompensate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.m_NumUpDownCompensate.DebouncedValueChanged += new System.EventHandler(this.m_NumUpDownCompensate_DebouncedValueChanged);
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Location = new System.Drawing.Point(115, 3);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(109, 35);
			this.m_btnConfirm.TabIndex = 2;
			this.m_btnConfirm.Text = "確認";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// m_btnClear
			// 
			this.m_btnClear.Location = new System.Drawing.Point(230, 3);
			this.m_btnClear.Name = "m_btnClear";
			this.m_btnClear.Size = new System.Drawing.Size(109, 35);
			this.m_btnClear.TabIndex = 3;
			this.m_btnClear.Text = "清除";
			this.m_btnClear.UseVisualStyleBackColor = true;
			this.m_btnClear.Click += new System.EventHandler(this.m_btnClear_Click);
			// 
			// m_btnReset
			// 
			this.m_btnReset.Location = new System.Drawing.Point(345, 3);
			this.m_btnReset.Name = "m_btnReset";
			this.m_btnReset.Size = new System.Drawing.Size(109, 35);
			this.m_btnReset.TabIndex = 4;
			this.m_btnReset.Text = "重置";
			this.m_btnReset.UseVisualStyleBackColor = true;
			this.m_btnReset.Click += new System.EventHandler(this.m_btnReset_Click);
			// 
			// m_pnlCompemsateParam
			// 
			this.m_pnlCompemsateParam.AutoSize = true;
			this.m_pnlCompemsateParam.ColumnCount = 3;
			this.m_pnlCompemsateParam.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.m_pnlCompemsateParam.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.m_pnlCompemsateParam.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.m_pnlCompemsateParam.Controls.Add(this.m_lblTotalCompensateValue, 2, 1);
			this.m_pnlCompemsateParam.Controls.Add(this.m_lblTotalCompensate, 2, 0);
			this.m_pnlCompemsateParam.Controls.Add(this.m_lblCompensate, 0, 1);
			this.m_pnlCompemsateParam.Controls.Add(this.m_NumUpDownCompensate, 1, 1);
			this.m_pnlCompemsateParam.Location = new System.Drawing.Point(3, 3);
			this.m_pnlCompemsateParam.Name = "m_pnlCompemsateParam";
			this.m_pnlCompemsateParam.RowCount = 2;
			this.m_pnlCompemsateParam.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.m_pnlCompemsateParam.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.m_pnlCompemsateParam.Size = new System.Drawing.Size(453, 64);
			this.m_pnlCompemsateParam.TabIndex = 5;
			// 
			// m_lblTotalCompensateValue
			// 
			this.m_lblTotalCompensateValue.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.m_lblTotalCompensateValue.AutoSize = true;
			this.m_lblTotalCompensateValue.Location = new System.Drawing.Point(353, 38);
			this.m_lblTotalCompensateValue.Name = "m_lblTotalCompensateValue";
			this.m_lblTotalCompensateValue.Size = new System.Drawing.Size(48, 19);
			this.m_lblTotalCompensateValue.TabIndex = 6;
			this.m_lblTotalCompensateValue.Text = "0.000";
			// 
			// m_lblTotalCompensate
			// 
			this.m_lblTotalCompensate.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.m_lblTotalCompensate.AutoSize = true;
			this.m_lblTotalCompensate.Location = new System.Drawing.Point(350, 6);
			this.m_lblTotalCompensate.Name = "m_lblTotalCompensate";
			this.m_lblTotalCompensate.Size = new System.Drawing.Size(54, 19);
			this.m_lblTotalCompensate.TabIndex = 2;
			this.m_lblTotalCompensate.Text = "總補償";
			// 
			// m_lblCompensate
			// 
			this.m_lblCompensate.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.m_lblCompensate.AutoSize = true;
			this.m_lblCompensate.Location = new System.Drawing.Point(41, 38);
			this.m_lblCompensate.Name = "m_lblCompensate";
			this.m_lblCompensate.Size = new System.Drawing.Size(69, 19);
			this.m_lblCompensate.TabIndex = 1;
			this.m_lblCompensate.Text = "補償距離";
			// 
			// m_pnlCompemsate
			// 
			this.m_pnlCompemsate.AutoSize = true;
			this.m_pnlCompemsate.ColumnCount = 1;
			this.m_pnlCompemsate.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_pnlCompemsate.Controls.Add(this.m_pnlCompemsateParam, 0, 0);
			this.m_pnlCompemsate.Controls.Add(this.m_flpnlEdit, 0, 1);
			this.m_pnlCompemsate.Location = new System.Drawing.Point(12, 12);
			this.m_pnlCompemsate.Name = "m_pnlCompemsate";
			this.m_pnlCompemsate.RowCount = 2;
			this.m_pnlCompemsate.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
			this.m_pnlCompemsate.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
			this.m_pnlCompemsate.Size = new System.Drawing.Size(463, 140);
			this.m_pnlCompemsate.TabIndex = 6;
			// 
			// m_flpnlEdit
			// 
			this.m_flpnlEdit.Controls.Add(this.m_btnReset);
			this.m_flpnlEdit.Controls.Add(this.m_btnClear);
			this.m_flpnlEdit.Controls.Add(this.m_btnConfirm);
			this.m_flpnlEdit.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.m_flpnlEdit.Location = new System.Drawing.Point(3, 87);
			this.m_flpnlEdit.Name = "m_flpnlEdit";
			this.m_flpnlEdit.Size = new System.Drawing.Size(457, 45);
			this.m_flpnlEdit.TabIndex = 6;
			// 
			// PathCompensateDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(488, 159);
			this.Controls.Add(this.m_pnlCompemsate);
			this.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "PathCompensateDlg";
			this.Text = "路徑補償";
			((System.ComponentModel.ISupportInitialize)(this.m_NumUpDownCompensate)).EndInit();
			this.m_pnlCompemsateParam.ResumeLayout(false);
			this.m_pnlCompemsateParam.PerformLayout();
			this.m_pnlCompemsate.ResumeLayout(false);
			this.m_pnlCompemsate.PerformLayout();
			this.m_flpnlEdit.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private MyCAM.Editor.DebouncedNumericUpDown m_NumUpDownCompensate;
		private System.Windows.Forms.Button m_btnConfirm;
		private System.Windows.Forms.Button m_btnClear;
		private System.Windows.Forms.Button m_btnReset;
		private System.Windows.Forms.TableLayoutPanel m_pnlCompemsateParam;
		private System.Windows.Forms.Label m_lblTotalCompensateValue;
		private System.Windows.Forms.Label m_lblTotalCompensate;
		private System.Windows.Forms.Label m_lblCompensate;
		private System.Windows.Forms.TableLayoutPanel m_pnlCompemsate;
		private System.Windows.Forms.FlowLayoutPanel m_flpnlEdit;
	}
}
