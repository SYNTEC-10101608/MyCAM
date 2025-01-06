namespace ProcessEdit
{
	partial class TraverseForm
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblPos = new System.Windows.Forms.Label();
			this.m_lblToolVec = new System.Windows.Forms.Label();
			this.m_lblX = new System.Windows.Forms.Label();
			this.m_lblY = new System.Windows.Forms.Label();
			this.m_lblZ = new System.Windows.Forms.Label();
			this.m_lblXR = new System.Windows.Forms.Label();
			this.m_lblYR = new System.Windows.Forms.Label();
			this.m_lblZR = new System.Windows.Forms.Label();
			this.m_tbxX = new System.Windows.Forms.TextBox();
			this.m_tbxY = new System.Windows.Forms.TextBox();
			this.m_tbxZ = new System.Windows.Forms.TextBox();
			this.m_tbxXR = new System.Windows.Forms.TextBox();
			this.m_tbxYR = new System.Windows.Forms.TextBox();
			this.m_tbxZR = new System.Windows.Forms.TextBox();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 81F));
			this.tableLayoutPanel1.Controls.Add(this.m_lblPos, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_lblToolVec, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_lblX, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_lblY, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_lblZ, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_tbxX, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_tbxY, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_tbxZ, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_btnOK, 3, 5);
			this.tableLayoutPanel1.Controls.Add(this.m_lblXR, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_lblYR, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_lblZR, 2, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_tbxXR, 3, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_tbxYR, 3, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_tbxZR, 3, 3);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(241, 183);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// m_lblPos
			// 
			this.m_lblPos.AutoSize = true;
			this.m_lblPos.Location = new System.Drawing.Point(3, 0);
			this.m_lblPos.Name = "m_lblPos";
			this.m_lblPos.Size = new System.Drawing.Size(29, 12);
			this.m_lblPos.TabIndex = 0;
			this.m_lblPos.Text = "位置";
			// 
			// m_lblToolVec
			// 
			this.m_lblToolVec.AutoSize = true;
			this.m_lblToolVec.Location = new System.Drawing.Point(123, 0);
			this.m_lblToolVec.Name = "m_lblToolVec";
			this.m_lblToolVec.Size = new System.Drawing.Size(29, 12);
			this.m_lblToolVec.TabIndex = 1;
			this.m_lblToolVec.Text = "姿態";
			// 
			// m_lblX
			// 
			this.m_lblX.AutoSize = true;
			this.m_lblX.Location = new System.Drawing.Point(3, 30);
			this.m_lblX.Name = "m_lblX";
			this.m_lblX.Size = new System.Drawing.Size(13, 12);
			this.m_lblX.TabIndex = 2;
			this.m_lblX.Text = "X";
			// 
			// m_lblY
			// 
			this.m_lblY.AutoSize = true;
			this.m_lblY.Location = new System.Drawing.Point(3, 60);
			this.m_lblY.Name = "m_lblY";
			this.m_lblY.Size = new System.Drawing.Size(13, 12);
			this.m_lblY.TabIndex = 3;
			this.m_lblY.Text = "Y";
			// 
			// m_lblZ
			// 
			this.m_lblZ.AutoSize = true;
			this.m_lblZ.Location = new System.Drawing.Point(3, 90);
			this.m_lblZ.Name = "m_lblZ";
			this.m_lblZ.Size = new System.Drawing.Size(12, 12);
			this.m_lblZ.TabIndex = 4;
			this.m_lblZ.Text = "Z";
			// 
			// m_lblXR
			// 
			this.m_lblXR.AutoSize = true;
			this.m_lblXR.Location = new System.Drawing.Point(123, 30);
			this.m_lblXR.Name = "m_lblXR";
			this.m_lblXR.Size = new System.Drawing.Size(21, 12);
			this.m_lblXR.TabIndex = 5;
			this.m_lblXR.Text = "XR";
			// 
			// m_lblYR
			// 
			this.m_lblYR.AutoSize = true;
			this.m_lblYR.Location = new System.Drawing.Point(123, 60);
			this.m_lblYR.Name = "m_lblYR";
			this.m_lblYR.Size = new System.Drawing.Size(21, 12);
			this.m_lblYR.TabIndex = 6;
			this.m_lblYR.Text = "YR";
			// 
			// m_lblZR
			// 
			this.m_lblZR.AutoSize = true;
			this.m_lblZR.Location = new System.Drawing.Point(123, 90);
			this.m_lblZR.Name = "m_lblZR";
			this.m_lblZR.Size = new System.Drawing.Size(20, 12);
			this.m_lblZR.TabIndex = 7;
			this.m_lblZR.Text = "ZR";
			// 
			// m_tbxX
			// 
			this.m_tbxX.Location = new System.Drawing.Point(43, 33);
			this.m_tbxX.Name = "m_tbxX";
			this.m_tbxX.Size = new System.Drawing.Size(74, 22);
			this.m_tbxX.TabIndex = 14;
			// 
			// m_tbxY
			// 
			this.m_tbxY.Location = new System.Drawing.Point(43, 63);
			this.m_tbxY.Name = "m_tbxY";
			this.m_tbxY.Size = new System.Drawing.Size(74, 22);
			this.m_tbxY.TabIndex = 15;
			// 
			// m_tbxZ
			// 
			this.m_tbxZ.Location = new System.Drawing.Point(43, 93);
			this.m_tbxZ.Name = "m_tbxZ";
			this.m_tbxZ.Size = new System.Drawing.Size(74, 22);
			this.m_tbxZ.TabIndex = 16;
			// 
			// m_tbxXR
			// 
			this.m_tbxXR.Location = new System.Drawing.Point(163, 33);
			this.m_tbxXR.Name = "m_tbxXR";
			this.m_tbxXR.Size = new System.Drawing.Size(74, 22);
			this.m_tbxXR.TabIndex = 17;
			// 
			// m_tbxYR
			// 
			this.m_tbxYR.Location = new System.Drawing.Point(163, 63);
			this.m_tbxYR.Name = "m_tbxYR";
			this.m_tbxYR.Size = new System.Drawing.Size(74, 22);
			this.m_tbxYR.TabIndex = 18;
			// 
			// m_tbxZR
			// 
			this.m_tbxZR.Location = new System.Drawing.Point(163, 93);
			this.m_tbxZR.Name = "m_tbxZR";
			this.m_tbxZR.Size = new System.Drawing.Size(74, 22);
			this.m_tbxZR.TabIndex = 19;
			// 
			// m_btnOK
			// 
			this.m_btnOK.Location = new System.Drawing.Point(163, 153);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(75, 23);
			this.m_btnOK.TabIndex = 26;
			this.m_btnOK.Text = "OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
			// 
			// AddTraverseForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(263, 207);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "AddTraverseForm";
			this.Text = "安全點";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label m_lblPos;
		private System.Windows.Forms.Label m_lblToolVec;
		private System.Windows.Forms.Label m_lblX;
		private System.Windows.Forms.Label m_lblY;
		private System.Windows.Forms.Label m_lblZ;
		private System.Windows.Forms.Label m_lblXR;
		private System.Windows.Forms.Label m_lblYR;
		private System.Windows.Forms.Label m_lblZR;
		private System.Windows.Forms.TextBox m_tbxX;
		private System.Windows.Forms.TextBox m_tbxY;
		private System.Windows.Forms.TextBox m_tbxZ;
		private System.Windows.Forms.TextBox m_tbxXR;
		private System.Windows.Forms.TextBox m_tbxYR;
		private System.Windows.Forms.TextBox m_tbxZR;
		private System.Windows.Forms.Button m_btnOK;
	}
}