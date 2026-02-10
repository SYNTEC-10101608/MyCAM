namespace MyCAM.Editor.Dialog
{
	partial class PathEditDlg
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
			this.m_gpbMove = new System.Windows.Forms.GroupBox();
			this.m_NumUpDownZ = new System.Windows.Forms.NumericUpDown();
			this.m_NumUpDownY = new System.Windows.Forms.NumericUpDown();
			this.m_NumUpDownX = new System.Windows.Forms.NumericUpDown();
			this.m_lblZ = new System.Windows.Forms.Label();
			this.m_lblY = new System.Windows.Forms.Label();
			this.m_lblX = new System.Windows.Forms.Label();
			this.m_lblRefCoordType = new System.Windows.Forms.Label();
			this.m_cmbRefCoordType = new System.Windows.Forms.ComboBox();
			this.m_btnConfirm = new System.Windows.Forms.Button();
			this.m_btnClear = new System.Windows.Forms.Button();
			this.m_btnReset = new System.Windows.Forms.Button();
			this.m_gpbMove.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumUpDownZ)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumUpDownY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumUpDownX)).BeginInit();
			this.SuspendLayout();
			// 
			// m_gpbMove
			// 
			this.m_gpbMove.Controls.Add(this.m_NumUpDownZ);
			this.m_gpbMove.Controls.Add(this.m_NumUpDownY);
			this.m_gpbMove.Controls.Add(this.m_NumUpDownX);
			this.m_gpbMove.Controls.Add(this.m_lblZ);
			this.m_gpbMove.Controls.Add(this.m_lblY);
			this.m_gpbMove.Controls.Add(this.m_lblX);
			this.m_gpbMove.Controls.Add(this.m_lblRefCoordType);
			this.m_gpbMove.Controls.Add(this.m_cmbRefCoordType);
			this.m_gpbMove.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_gpbMove.Location = new System.Drawing.Point(12, 12);
			this.m_gpbMove.Name = "m_gpbMove";
			this.m_gpbMove.Size = new System.Drawing.Size(255, 204);
			this.m_gpbMove.TabIndex = 0;
			this.m_gpbMove.TabStop = false;
			this.m_gpbMove.Text = "平移";
			// 
			// m_NumUpDownZ
			// 
			this.m_NumUpDownZ.Location = new System.Drawing.Point(104, 145);
			this.m_NumUpDownZ.Name = "m_NumUpDownZ";
			this.m_NumUpDownZ.Size = new System.Drawing.Size(120, 21);
			this.m_NumUpDownZ.TabIndex = 7;
			this.m_NumUpDownZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.m_NumUpDownZ.Click += new System.EventHandler(this.m_NumUpDownZ_Click);
			this.m_NumUpDownZ.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_NumUpDownZ_KeyDown);
			this.m_NumUpDownZ.Leave += new System.EventHandler(this.m_NumUpDownZ_Leave);
			// 
			// m_NumUpDownY
			// 
			this.m_NumUpDownY.Location = new System.Drawing.Point(104, 110);
			this.m_NumUpDownY.Name = "m_NumUpDownY";
			this.m_NumUpDownY.Size = new System.Drawing.Size(120, 21);
			this.m_NumUpDownY.TabIndex = 6;
			this.m_NumUpDownY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.m_NumUpDownY.Click += new System.EventHandler(this.m_NumUpDownY_Click);
			this.m_NumUpDownY.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_NumUpDownY_KeyDown);
			this.m_NumUpDownY.Leave += new System.EventHandler(this.m_NumUpDownY_Leave);
			// 
			// m_NumUpDownX
			// 
			this.m_NumUpDownX.Location = new System.Drawing.Point(104, 71);
			this.m_NumUpDownX.Name = "m_NumUpDownX";
			this.m_NumUpDownX.Size = new System.Drawing.Size(120, 21);
			this.m_NumUpDownX.TabIndex = 5;
			this.m_NumUpDownX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.m_NumUpDownX.Click += new System.EventHandler(this.m_NumUpDownX_Click);
			this.m_NumUpDownX.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_NumUpDownX_KeyDown);
			this.m_NumUpDownX.Leave += new System.EventHandler(this.m_NumUpDownX_Leave);
			// 
			// m_lblZ
			// 
			this.m_lblZ.AutoSize = true;
			this.m_lblZ.Location = new System.Drawing.Point(31, 147);
			this.m_lblZ.Name = "m_lblZ";
			this.m_lblZ.Size = new System.Drawing.Size(58, 15);
			this.m_lblZ.TabIndex = 4;
			this.m_lblZ.Text = "Z(增量)：";
			// 
			// m_lblY
			// 
			this.m_lblY.AutoSize = true;
			this.m_lblY.Location = new System.Drawing.Point(31, 112);
			this.m_lblY.Name = "m_lblY";
			this.m_lblY.Size = new System.Drawing.Size(58, 15);
			this.m_lblY.TabIndex = 3;
			this.m_lblY.Text = "Y(增量)：";
			// 
			// m_lblX
			// 
			this.m_lblX.AutoSize = true;
			this.m_lblX.Location = new System.Drawing.Point(31, 71);
			this.m_lblX.Name = "m_lblX";
			this.m_lblX.Size = new System.Drawing.Size(58, 15);
			this.m_lblX.TabIndex = 2;
			this.m_lblX.Text = "X(增量)：";
			// 
			// m_lblRefCoordType
			// 
			this.m_lblRefCoordType.AutoSize = true;
			this.m_lblRefCoordType.Location = new System.Drawing.Point(31, 24);
			this.m_lblRefCoordType.Name = "m_lblRefCoordType";
			this.m_lblRefCoordType.Size = new System.Drawing.Size(67, 15);
			this.m_lblRefCoordType.TabIndex = 1;
			this.m_lblRefCoordType.Text = "參考坐標系";
			// 
			// m_cmbRefCoordType
			// 
			this.m_cmbRefCoordType.FormattingEnabled = true;
			this.m_cmbRefCoordType.Items.AddRange(new object[] {
            "區域坐標系",
            "大地坐標系"});
			this.m_cmbRefCoordType.Location = new System.Drawing.Point(104, 21);
			this.m_cmbRefCoordType.Name = "m_cmbRefCoordType";
			this.m_cmbRefCoordType.Size = new System.Drawing.Size(121, 23);
			this.m_cmbRefCoordType.TabIndex = 0;
			this.m_cmbRefCoordType.SelectedIndexChanged += new System.EventHandler(this.m_cmbRefCoordType_SelectedIndexChanged);
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Location = new System.Drawing.Point(192, 222);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(75, 23);
			this.m_btnConfirm.TabIndex = 1;
			this.m_btnConfirm.Text = "確認";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// m_btnClear
			// 
			this.m_btnClear.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_btnClear.Location = new System.Drawing.Point(111, 222);
			this.m_btnClear.Name = "m_btnClear";
			this.m_btnClear.Size = new System.Drawing.Size(75, 23);
			this.m_btnClear.TabIndex = 2;
			this.m_btnClear.Text = "清除";
			this.m_btnClear.UseVisualStyleBackColor = true;
			this.m_btnClear.Click += new System.EventHandler(this.m_btnClear_Click);
			// 
			// m_btnReset
			// 
			this.m_btnReset.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_btnReset.Location = new System.Drawing.Point(26, 222);
			this.m_btnReset.Name = "m_btnReset";
			this.m_btnReset.Size = new System.Drawing.Size(75, 23);
			this.m_btnReset.TabIndex = 3;
			this.m_btnReset.Text = "重置";
			this.m_btnReset.UseVisualStyleBackColor = true;
			this.m_btnReset.Click += new System.EventHandler(this.m_btnReset_Click);
			// 
			// PathEditDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(283, 257);
			this.Controls.Add(this.m_btnReset);
			this.Controls.Add(this.m_btnClear);
			this.Controls.Add(this.m_btnConfirm);
			this.Controls.Add(this.m_gpbMove);
			this.Name = "PathEditDlg";
			this.Text = "路徑編修";
			this.Shown += new System.EventHandler(this.PathEditDlg_Shown);
			this.m_gpbMove.ResumeLayout(false);
			this.m_gpbMove.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_NumUpDownZ)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumUpDownY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_NumUpDownX)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox m_gpbMove;
		private System.Windows.Forms.Label m_lblRefCoordType;
		private System.Windows.Forms.ComboBox m_cmbRefCoordType;
		private System.Windows.Forms.NumericUpDown m_NumUpDownZ;
		private System.Windows.Forms.NumericUpDown m_NumUpDownY;
		private System.Windows.Forms.NumericUpDown m_NumUpDownX;
		private System.Windows.Forms.Label m_lblZ;
		private System.Windows.Forms.Label m_lblY;
		private System.Windows.Forms.Label m_lblX;
		private System.Windows.Forms.Button m_btnConfirm;
		private System.Windows.Forms.Button m_btnClear;
		private System.Windows.Forms.Button m_btnReset;
	}
}