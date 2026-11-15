namespace MyCAM.Editor
{
	partial class ThreePtTransformDlg
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
			this.m_gbCADCoord = new CommonUI.Controls.SectionPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.tbxCADX1 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.tbxCADY1 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.tbxCADZ1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.tbxCADX2 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.tbxCADY2 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.tbxCADZ2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.tbxCADX3 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.tbxCADY3 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.tbxCADZ3 = new System.Windows.Forms.Label();
			this.m_gbMachineCoord = new CommonUI.Controls.SectionPanel();
			this.m_lblMachinePoint1 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.tbxMachineX1 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label14 = new System.Windows.Forms.Label();
			this.tbxMachineY1 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label15 = new System.Windows.Forms.Label();
			this.tbxMachineZ1 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblMachinePoint2 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.tbxMachineX2 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label17 = new System.Windows.Forms.Label();
			this.tbxMachineY2 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label18 = new System.Windows.Forms.Label();
			this.tbxMachineZ2 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblMachinePoint3 = new System.Windows.Forms.Label();
			this.label19 = new System.Windows.Forms.Label();
			this.tbxMachineX3 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label20 = new System.Windows.Forms.Label();
			this.tbxMachineY3 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.label21 = new System.Windows.Forms.Label();
			this.tbxMachineZ3 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.btnOK = new CommonUI.Controls.ThemedButton();
			this.m_gbCADCoord.SuspendLayout();
			this.m_gbMachineCoord.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineX1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineY1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineZ1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineX2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineY2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineZ2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineX3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineY3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineZ3)).BeginInit();
			this.SuspendLayout();
			// 
			// m_gbCADCoord
			// 
			this.m_gbCADCoord.Controls.Add(this.label1);
			this.m_gbCADCoord.Controls.Add(this.label4);
			this.m_gbCADCoord.Controls.Add(this.tbxCADX1);
			this.m_gbCADCoord.Controls.Add(this.label5);
			this.m_gbCADCoord.Controls.Add(this.tbxCADY1);
			this.m_gbCADCoord.Controls.Add(this.label6);
			this.m_gbCADCoord.Controls.Add(this.tbxCADZ1);
			this.m_gbCADCoord.Controls.Add(this.label2);
			this.m_gbCADCoord.Controls.Add(this.label7);
			this.m_gbCADCoord.Controls.Add(this.tbxCADX2);
			this.m_gbCADCoord.Controls.Add(this.label8);
			this.m_gbCADCoord.Controls.Add(this.tbxCADY2);
			this.m_gbCADCoord.Controls.Add(this.label9);
			this.m_gbCADCoord.Controls.Add(this.tbxCADZ2);
			this.m_gbCADCoord.Controls.Add(this.label3);
			this.m_gbCADCoord.Controls.Add(this.label10);
			this.m_gbCADCoord.Controls.Add(this.tbxCADX3);
			this.m_gbCADCoord.Controls.Add(this.label11);
			this.m_gbCADCoord.Controls.Add(this.tbxCADY3);
			this.m_gbCADCoord.Controls.Add(this.label12);
			this.m_gbCADCoord.Controls.Add(this.tbxCADZ3);
			this.m_gbCADCoord.Location = new System.Drawing.Point(12, 12);
			this.m_gbCADCoord.Name = "m_gbCADCoord";
			this.m_gbCADCoord.Size = new System.Drawing.Size(376, 129);
			this.m_gbCADCoord.TabIndex = 0;
			this.m_gbCADCoord.Text = "CAD座標";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(10, 40);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(43, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "第一點";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(70, 40);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(14, 16);
			this.label4.TabIndex = 1;
			this.label4.Text = "X";
			// 
			// tbxCADX1
			// 
			this.tbxCADX1.Location = new System.Drawing.Point(86, 36);
			this.tbxCADX1.Name = "tbxCADX1";
			this.tbxCADX1.Size = new System.Drawing.Size(76, 23);
			this.tbxCADX1.TabIndex = 2;
			this.tbxCADX1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(170, 40);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(14, 16);
			this.label5.TabIndex = 3;
			this.label5.Text = "Y";
			// 
			// tbxCADY1
			// 
			this.tbxCADY1.Location = new System.Drawing.Point(186, 36);
			this.tbxCADY1.Name = "tbxCADY1";
			this.tbxCADY1.Size = new System.Drawing.Size(76, 23);
			this.tbxCADY1.TabIndex = 4;
			this.tbxCADY1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(270, 40);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(14, 16);
			this.label6.TabIndex = 5;
			this.label6.Text = "Z";
			// 
			// tbxCADZ1
			// 
			this.tbxCADZ1.Location = new System.Drawing.Point(286, 36);
			this.tbxCADZ1.Name = "tbxCADZ1";
			this.tbxCADZ1.Size = new System.Drawing.Size(76, 23);
			this.tbxCADZ1.TabIndex = 6;
			this.tbxCADZ1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(10, 70);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(43, 16);
			this.label2.TabIndex = 7;
			this.label2.Text = "第二點";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(70, 70);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(14, 16);
			this.label7.TabIndex = 8;
			this.label7.Text = "X";
			// 
			// tbxCADX2
			// 
			this.tbxCADX2.Location = new System.Drawing.Point(86, 66);
			this.tbxCADX2.Name = "tbxCADX2";
			this.tbxCADX2.Size = new System.Drawing.Size(76, 23);
			this.tbxCADX2.TabIndex = 9;
			this.tbxCADX2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(170, 70);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(14, 16);
			this.label8.TabIndex = 10;
			this.label8.Text = "Y";
			// 
			// tbxCADY2
			// 
			this.tbxCADY2.Location = new System.Drawing.Point(186, 66);
			this.tbxCADY2.Name = "tbxCADY2";
			this.tbxCADY2.Size = new System.Drawing.Size(76, 23);
			this.tbxCADY2.TabIndex = 11;
			this.tbxCADY2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(270, 70);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(14, 16);
			this.label9.TabIndex = 12;
			this.label9.Text = "Z";
			// 
			// tbxCADZ2
			// 
			this.tbxCADZ2.Location = new System.Drawing.Point(286, 66);
			this.tbxCADZ2.Name = "tbxCADZ2";
			this.tbxCADZ2.Size = new System.Drawing.Size(76, 23);
			this.tbxCADZ2.TabIndex = 13;
			this.tbxCADZ2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(10, 100);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(43, 16);
			this.label3.TabIndex = 14;
			this.label3.Text = "第三點";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(70, 100);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(14, 16);
			this.label10.TabIndex = 15;
			this.label10.Text = "X";
			// 
			// tbxCADX3
			// 
			this.tbxCADX3.Location = new System.Drawing.Point(86, 96);
			this.tbxCADX3.Name = "tbxCADX3";
			this.tbxCADX3.Size = new System.Drawing.Size(76, 23);
			this.tbxCADX3.TabIndex = 16;
			this.tbxCADX3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(170, 100);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(14, 16);
			this.label11.TabIndex = 17;
			this.label11.Text = "Y";
			// 
			// tbxCADY3
			// 
			this.tbxCADY3.Location = new System.Drawing.Point(186, 96);
			this.tbxCADY3.Name = "tbxCADY3";
			this.tbxCADY3.Size = new System.Drawing.Size(76, 23);
			this.tbxCADY3.TabIndex = 18;
			this.tbxCADY3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(270, 100);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(14, 16);
			this.label12.TabIndex = 19;
			this.label12.Text = "Z";
			// 
			// tbxCADZ3
			// 
			this.tbxCADZ3.Location = new System.Drawing.Point(286, 96);
			this.tbxCADZ3.Name = "tbxCADZ3";
			this.tbxCADZ3.Size = new System.Drawing.Size(76, 23);
			this.tbxCADZ3.TabIndex = 20;
			this.tbxCADZ3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_gbMachineCoord
			// 
			this.m_gbMachineCoord.Controls.Add(this.m_lblMachinePoint1);
			this.m_gbMachineCoord.Controls.Add(this.label13);
			this.m_gbMachineCoord.Controls.Add(this.tbxMachineX1);
			this.m_gbMachineCoord.Controls.Add(this.label14);
			this.m_gbMachineCoord.Controls.Add(this.tbxMachineY1);
			this.m_gbMachineCoord.Controls.Add(this.label15);
			this.m_gbMachineCoord.Controls.Add(this.tbxMachineZ1);
			this.m_gbMachineCoord.Controls.Add(this.m_lblMachinePoint2);
			this.m_gbMachineCoord.Controls.Add(this.label16);
			this.m_gbMachineCoord.Controls.Add(this.tbxMachineX2);
			this.m_gbMachineCoord.Controls.Add(this.label17);
			this.m_gbMachineCoord.Controls.Add(this.tbxMachineY2);
			this.m_gbMachineCoord.Controls.Add(this.label18);
			this.m_gbMachineCoord.Controls.Add(this.tbxMachineZ2);
			this.m_gbMachineCoord.Controls.Add(this.m_lblMachinePoint3);
			this.m_gbMachineCoord.Controls.Add(this.label19);
			this.m_gbMachineCoord.Controls.Add(this.tbxMachineX3);
			this.m_gbMachineCoord.Controls.Add(this.label20);
			this.m_gbMachineCoord.Controls.Add(this.tbxMachineY3);
			this.m_gbMachineCoord.Controls.Add(this.label21);
			this.m_gbMachineCoord.Controls.Add(this.tbxMachineZ3);
			this.m_gbMachineCoord.Location = new System.Drawing.Point(12, 149);
			this.m_gbMachineCoord.Name = "m_gbMachineCoord";
			this.m_gbMachineCoord.Size = new System.Drawing.Size(376, 129);
			this.m_gbMachineCoord.TabIndex = 1;
			this.m_gbMachineCoord.Text = "機械座標";
			// 
			// m_lblMachinePoint1
			// 
			this.m_lblMachinePoint1.AutoSize = true;
			this.m_lblMachinePoint1.Location = new System.Drawing.Point(10, 40);
			this.m_lblMachinePoint1.Name = "m_lblMachinePoint1";
			this.m_lblMachinePoint1.Size = new System.Drawing.Size(43, 16);
			this.m_lblMachinePoint1.TabIndex = 0;
			this.m_lblMachinePoint1.Text = "第一點";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(70, 40);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(14, 16);
			this.label13.TabIndex = 1;
			this.label13.Text = "X";
			// 
			// tbxMachineX1
			// 
			this.tbxMachineX1.DecimalPlaces = 3;
			this.tbxMachineX1.Location = new System.Drawing.Point(86, 36);
			this.tbxMachineX1.Name = "tbxMachineX1";
			this.tbxMachineX1.Size = new System.Drawing.Size(76, 23);
			this.tbxMachineX1.TabIndex = 2;
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(170, 40);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(14, 16);
			this.label14.TabIndex = 3;
			this.label14.Text = "Y";
			// 
			// tbxMachineY1
			// 
			this.tbxMachineY1.DecimalPlaces = 3;
			this.tbxMachineY1.Location = new System.Drawing.Point(186, 36);
			this.tbxMachineY1.Name = "tbxMachineY1";
			this.tbxMachineY1.Size = new System.Drawing.Size(76, 23);
			this.tbxMachineY1.TabIndex = 4;
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(270, 40);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(14, 16);
			this.label15.TabIndex = 5;
			this.label15.Text = "Z";
			// 
			// tbxMachineZ1
			// 
			this.tbxMachineZ1.DecimalPlaces = 3;
			this.tbxMachineZ1.Location = new System.Drawing.Point(286, 36);
			this.tbxMachineZ1.Name = "tbxMachineZ1";
			this.tbxMachineZ1.Size = new System.Drawing.Size(76, 23);
			this.tbxMachineZ1.TabIndex = 6;
			// 
			// m_lblMachinePoint2
			// 
			this.m_lblMachinePoint2.AutoSize = true;
			this.m_lblMachinePoint2.Location = new System.Drawing.Point(10, 70);
			this.m_lblMachinePoint2.Name = "m_lblMachinePoint2";
			this.m_lblMachinePoint2.Size = new System.Drawing.Size(43, 16);
			this.m_lblMachinePoint2.TabIndex = 7;
			this.m_lblMachinePoint2.Text = "第二點";
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(70, 70);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(14, 16);
			this.label16.TabIndex = 8;
			this.label16.Text = "X";
			// 
			// tbxMachineX2
			// 
			this.tbxMachineX2.DecimalPlaces = 3;
			this.tbxMachineX2.Location = new System.Drawing.Point(86, 66);
			this.tbxMachineX2.Name = "tbxMachineX2";
			this.tbxMachineX2.Size = new System.Drawing.Size(76, 23);
			this.tbxMachineX2.TabIndex = 9;
			this.tbxMachineX2.Value = new decimal(new int[] { 1, 0, 0, 0});
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(170, 70);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(14, 16);
			this.label17.TabIndex = 10;
			this.label17.Text = "Y";
			// 
			// tbxMachineY2
			// 
			this.tbxMachineY2.DecimalPlaces = 3;
			this.tbxMachineY2.Location = new System.Drawing.Point(186, 66);
			this.tbxMachineY2.Name = "tbxMachineY2";
			this.tbxMachineY2.Size = new System.Drawing.Size(76, 23);
			this.tbxMachineY2.TabIndex = 11;
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(270, 70);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(14, 16);
			this.label18.TabIndex = 12;
			this.label18.Text = "Z";
			// 
			// tbxMachineZ2
			// 
			this.tbxMachineZ2.DecimalPlaces = 3;
			this.tbxMachineZ2.Location = new System.Drawing.Point(286, 66);
			this.tbxMachineZ2.Name = "tbxMachineZ2";
			this.tbxMachineZ2.Size = new System.Drawing.Size(76, 23);
			this.tbxMachineZ2.TabIndex = 13;
			// 
			// m_lblMachinePoint3
			// 
			this.m_lblMachinePoint3.AutoSize = true;
			this.m_lblMachinePoint3.Location = new System.Drawing.Point(10, 100);
			this.m_lblMachinePoint3.Name = "m_lblMachinePoint3";
			this.m_lblMachinePoint3.Size = new System.Drawing.Size(43, 16);
			this.m_lblMachinePoint3.TabIndex = 14;
			this.m_lblMachinePoint3.Text = "第三點";
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(70, 100);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(14, 16);
			this.label19.TabIndex = 15;
			this.label19.Text = "X";
			// 
			// tbxMachineX3
			// 
			this.tbxMachineX3.DecimalPlaces = 3;
			this.tbxMachineX3.Location = new System.Drawing.Point(86, 96);
			this.tbxMachineX3.Name = "tbxMachineX3";
			this.tbxMachineX3.Size = new System.Drawing.Size(76, 23);
			this.tbxMachineX3.TabIndex = 16;
			// 
			// label20
			// 
			this.label20.AutoSize = true;
			this.label20.Location = new System.Drawing.Point(170, 100);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(14, 16);
			this.label20.TabIndex = 17;
			this.label20.Text = "Y";
			// 
			// tbxMachineY3
			// 
			this.tbxMachineY3.DecimalPlaces = 3;
			this.tbxMachineY3.Location = new System.Drawing.Point(186, 96);
			this.tbxMachineY3.Name = "tbxMachineY3";
			this.tbxMachineY3.Size = new System.Drawing.Size(76, 23);
			this.tbxMachineY3.TabIndex = 18;
			this.tbxMachineY3.Value = new decimal(new int[] { 1, 0, 0, 0});
			// 
			// label21
			// 
			this.label21.AutoSize = true;
			this.label21.Location = new System.Drawing.Point(270, 100);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(14, 16);
			this.label21.TabIndex = 19;
			this.label21.Text = "Z";
			// 
			// tbxMachineZ3
			// 
			this.tbxMachineZ3.DecimalPlaces = 3;
			this.tbxMachineZ3.Location = new System.Drawing.Point(286, 96);
			this.tbxMachineZ3.Name = "tbxMachineZ3";
			this.tbxMachineZ3.Size = new System.Drawing.Size(76, 23);
			this.tbxMachineZ3.TabIndex = 20;
			// 
			// btnOK
			// 
			this.btnOK.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.btnOK.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.btnOK.Location = new System.Drawing.Point(308, 294);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(80, 28);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = "確定";
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// ThreePtTransformDlg
			// 
			this.ClientSize = new System.Drawing.Size(400, 334);
			this.Controls.Add(this.m_gbCADCoord);
			this.Controls.Add(this.m_gbMachineCoord);
			this.Controls.Add(this.btnOK);
			this.Name = "ThreePtTransformDlg";
			this.Text = "三點轉換";
			this.m_gbCADCoord.ResumeLayout(false);
			this.m_gbCADCoord.PerformLayout();
			this.m_gbMachineCoord.ResumeLayout(false);
			this.m_gbMachineCoord.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineX1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineY1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineZ1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineX2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineY2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineZ2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineX3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineY3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tbxMachineZ3)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_gbCADCoord;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label tbxCADX1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label tbxCADY1;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label tbxCADZ1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label tbxCADX2;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label tbxCADY2;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label tbxCADZ2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label tbxCADX3;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label tbxCADY3;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label tbxCADZ3;
		private CommonUI.Controls.SectionPanel m_gbMachineCoord;
		private System.Windows.Forms.Label m_lblMachinePoint1;
		private System.Windows.Forms.Label label13;
		private CommonUI.Controls.DebouncedNumericUpDown tbxMachineX1;
		private System.Windows.Forms.Label label14;
		private CommonUI.Controls.DebouncedNumericUpDown tbxMachineY1;
		private System.Windows.Forms.Label label15;
		private CommonUI.Controls.DebouncedNumericUpDown tbxMachineZ1;
		private System.Windows.Forms.Label m_lblMachinePoint2;
		private System.Windows.Forms.Label label16;
		private CommonUI.Controls.DebouncedNumericUpDown tbxMachineX2;
		private System.Windows.Forms.Label label17;
		private CommonUI.Controls.DebouncedNumericUpDown tbxMachineY2;
		private System.Windows.Forms.Label label18;
		private CommonUI.Controls.DebouncedNumericUpDown tbxMachineZ2;
		private System.Windows.Forms.Label m_lblMachinePoint3;
		private System.Windows.Forms.Label label19;
		private CommonUI.Controls.DebouncedNumericUpDown tbxMachineX3;
		private System.Windows.Forms.Label label20;
		private CommonUI.Controls.DebouncedNumericUpDown tbxMachineY3;
		private System.Windows.Forms.Label label21;
		private CommonUI.Controls.DebouncedNumericUpDown tbxMachineZ3;
		private CommonUI.Controls.ThemedButton btnOK;
	}
}
