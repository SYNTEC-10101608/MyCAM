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
			this.m_pnlCADCoord = new CommonUI.Controls.SectionPanel();
			this.m_lblCADPoint1 = new System.Windows.Forms.Label();
			this.m_lblCADX1 = new System.Windows.Forms.Label();
			this.m_lblCADX1Value = new System.Windows.Forms.Label();
			this.m_lblCADY1 = new System.Windows.Forms.Label();
			this.m_lblCADY1Value = new System.Windows.Forms.Label();
			this.m_lblCADZ1 = new System.Windows.Forms.Label();
			this.m_lblCADZ1Value = new System.Windows.Forms.Label();
			this.m_lblCADPoint2 = new System.Windows.Forms.Label();
			this.m_lblCADX2 = new System.Windows.Forms.Label();
			this.m_lblCADX2Value = new System.Windows.Forms.Label();
			this.m_lblCADY2 = new System.Windows.Forms.Label();
			this.m_lblCADY2Value = new System.Windows.Forms.Label();
			this.m_lblCADZ2 = new System.Windows.Forms.Label();
			this.m_lblCADZ2Value = new System.Windows.Forms.Label();
			this.m_lblCADPoint3 = new System.Windows.Forms.Label();
			this.m_lblCADX3 = new System.Windows.Forms.Label();
			this.m_lblCADX3Value = new System.Windows.Forms.Label();
			this.m_lblCADY3 = new System.Windows.Forms.Label();
			this.m_lblCADY3Value = new System.Windows.Forms.Label();
			this.m_lblCADZ3 = new System.Windows.Forms.Label();
			this.m_lblCADZ3Value = new System.Windows.Forms.Label();
			this.m_pnlMachineCoord = new CommonUI.Controls.SectionPanel();
			this.m_lblMachinePoint1 = new System.Windows.Forms.Label();
			this.m_lblMachineX1 = new System.Windows.Forms.Label();
			this.m_numMachineX1 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblMachineY1 = new System.Windows.Forms.Label();
			this.m_numMachineY1 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblMachineZ1 = new System.Windows.Forms.Label();
			this.m_numMachineZ1 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblMachinePoint2 = new System.Windows.Forms.Label();
			this.m_lblMachineX2 = new System.Windows.Forms.Label();
			this.m_numMachineX2 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblMachineY2 = new System.Windows.Forms.Label();
			this.m_numMachineY2 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblMachineZ2 = new System.Windows.Forms.Label();
			this.m_numMachineZ2 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblMachinePoint3 = new System.Windows.Forms.Label();
			this.m_lblMachineX3 = new System.Windows.Forms.Label();
			this.m_numMachineX3 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblMachineY3 = new System.Windows.Forms.Label();
			this.m_numMachineY3 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_lblMachineZ3 = new System.Windows.Forms.Label();
			this.m_numMachineZ3 = new CommonUI.Controls.DebouncedNumericUpDown();
			this.m_btnOK = new CommonUI.Controls.ThemedButton();
			this.m_pnlCADCoord.SuspendLayout();
			this.m_pnlMachineCoord.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineX1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineY1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineZ1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineX2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineY2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineZ2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineX3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineY3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineZ3)).BeginInit();
			this.SuspendLayout();
			// 
			// m_pnlCADCoord
			// 
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADPoint1);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADX1);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADX1Value);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADY1);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADY1Value);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADZ1);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADZ1Value);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADPoint2);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADX2);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADX2Value);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADY2);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADY2Value);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADZ2);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADZ2Value);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADPoint3);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADX3);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADX3Value);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADY3);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADY3Value);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADZ3);
			this.m_pnlCADCoord.Controls.Add(this.m_lblCADZ3Value);
			this.m_pnlCADCoord.Location = new System.Drawing.Point(12, 12);
			this.m_pnlCADCoord.Name = "m_pnlCADCoord";
			this.m_pnlCADCoord.Size = new System.Drawing.Size(376, 129);
			this.m_pnlCADCoord.TabIndex = 0;
			this.m_pnlCADCoord.Text = "CAD座標";
			// 
			// m_lblCADPoint1
			// 
			this.m_lblCADPoint1.AutoSize = true;
			this.m_lblCADPoint1.Location = new System.Drawing.Point(10, 40);
			this.m_lblCADPoint1.Name = "m_lblCADPoint1";
			this.m_lblCADPoint1.Size = new System.Drawing.Size(43, 16);
			this.m_lblCADPoint1.TabIndex = 0;
			this.m_lblCADPoint1.Text = "第一點";
			// 
			// m_lblCADX1
			// 
			this.m_lblCADX1.AutoSize = true;
			this.m_lblCADX1.Location = new System.Drawing.Point(70, 40);
			this.m_lblCADX1.Name = "m_lblCADX1";
			this.m_lblCADX1.Size = new System.Drawing.Size(14, 16);
			this.m_lblCADX1.TabIndex = 1;
			this.m_lblCADX1.Text = "X";
			// 
			// m_lblCADX1Value
			// 
			this.m_lblCADX1Value.Location = new System.Drawing.Point(86, 36);
			this.m_lblCADX1Value.Name = "m_lblCADX1Value";
			this.m_lblCADX1Value.Size = new System.Drawing.Size(76, 23);
			this.m_lblCADX1Value.TabIndex = 2;
			this.m_lblCADX1Value.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_lblCADY1
			// 
			this.m_lblCADY1.AutoSize = true;
			this.m_lblCADY1.Location = new System.Drawing.Point(170, 40);
			this.m_lblCADY1.Name = "m_lblCADY1";
			this.m_lblCADY1.Size = new System.Drawing.Size(14, 16);
			this.m_lblCADY1.TabIndex = 3;
			this.m_lblCADY1.Text = "Y";
			// 
			// m_lblCADY1Value
			// 
			this.m_lblCADY1Value.Location = new System.Drawing.Point(186, 36);
			this.m_lblCADY1Value.Name = "m_lblCADY1Value";
			this.m_lblCADY1Value.Size = new System.Drawing.Size(76, 23);
			this.m_lblCADY1Value.TabIndex = 4;
			this.m_lblCADY1Value.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_lblCADZ1
			// 
			this.m_lblCADZ1.AutoSize = true;
			this.m_lblCADZ1.Location = new System.Drawing.Point(270, 40);
			this.m_lblCADZ1.Name = "m_lblCADZ1";
			this.m_lblCADZ1.Size = new System.Drawing.Size(14, 16);
			this.m_lblCADZ1.TabIndex = 5;
			this.m_lblCADZ1.Text = "Z";
			// 
			// m_lblCADZ1Value
			// 
			this.m_lblCADZ1Value.Location = new System.Drawing.Point(286, 36);
			this.m_lblCADZ1Value.Name = "m_lblCADZ1Value";
			this.m_lblCADZ1Value.Size = new System.Drawing.Size(76, 23);
			this.m_lblCADZ1Value.TabIndex = 6;
			this.m_lblCADZ1Value.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_lblCADPoint2
			// 
			this.m_lblCADPoint2.AutoSize = true;
			this.m_lblCADPoint2.Location = new System.Drawing.Point(10, 70);
			this.m_lblCADPoint2.Name = "m_lblCADPoint2";
			this.m_lblCADPoint2.Size = new System.Drawing.Size(43, 16);
			this.m_lblCADPoint2.TabIndex = 7;
			this.m_lblCADPoint2.Text = "第二點";
			// 
			// m_lblCADX2
			// 
			this.m_lblCADX2.AutoSize = true;
			this.m_lblCADX2.Location = new System.Drawing.Point(70, 70);
			this.m_lblCADX2.Name = "m_lblCADX2";
			this.m_lblCADX2.Size = new System.Drawing.Size(14, 16);
			this.m_lblCADX2.TabIndex = 8;
			this.m_lblCADX2.Text = "X";
			// 
			// m_lblCADX2Value
			// 
			this.m_lblCADX2Value.Location = new System.Drawing.Point(86, 66);
			this.m_lblCADX2Value.Name = "m_lblCADX2Value";
			this.m_lblCADX2Value.Size = new System.Drawing.Size(76, 23);
			this.m_lblCADX2Value.TabIndex = 9;
			this.m_lblCADX2Value.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_lblCADY2
			// 
			this.m_lblCADY2.AutoSize = true;
			this.m_lblCADY2.Location = new System.Drawing.Point(170, 70);
			this.m_lblCADY2.Name = "m_lblCADY2";
			this.m_lblCADY2.Size = new System.Drawing.Size(14, 16);
			this.m_lblCADY2.TabIndex = 10;
			this.m_lblCADY2.Text = "Y";
			// 
			// m_lblCADY2Value
			// 
			this.m_lblCADY2Value.Location = new System.Drawing.Point(186, 66);
			this.m_lblCADY2Value.Name = "m_lblCADY2Value";
			this.m_lblCADY2Value.Size = new System.Drawing.Size(76, 23);
			this.m_lblCADY2Value.TabIndex = 11;
			this.m_lblCADY2Value.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_lblCADZ2
			// 
			this.m_lblCADZ2.AutoSize = true;
			this.m_lblCADZ2.Location = new System.Drawing.Point(270, 70);
			this.m_lblCADZ2.Name = "m_lblCADZ2";
			this.m_lblCADZ2.Size = new System.Drawing.Size(14, 16);
			this.m_lblCADZ2.TabIndex = 12;
			this.m_lblCADZ2.Text = "Z";
			// 
			// m_lblCADZ2Value
			// 
			this.m_lblCADZ2Value.Location = new System.Drawing.Point(286, 66);
			this.m_lblCADZ2Value.Name = "m_lblCADZ2Value";
			this.m_lblCADZ2Value.Size = new System.Drawing.Size(76, 23);
			this.m_lblCADZ2Value.TabIndex = 13;
			this.m_lblCADZ2Value.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_lblCADPoint3
			// 
			this.m_lblCADPoint3.AutoSize = true;
			this.m_lblCADPoint3.Location = new System.Drawing.Point(10, 100);
			this.m_lblCADPoint3.Name = "m_lblCADPoint3";
			this.m_lblCADPoint3.Size = new System.Drawing.Size(43, 16);
			this.m_lblCADPoint3.TabIndex = 14;
			this.m_lblCADPoint3.Text = "第三點";
			// 
			// m_lblCADX3
			// 
			this.m_lblCADX3.AutoSize = true;
			this.m_lblCADX3.Location = new System.Drawing.Point(70, 100);
			this.m_lblCADX3.Name = "m_lblCADX3";
			this.m_lblCADX3.Size = new System.Drawing.Size(14, 16);
			this.m_lblCADX3.TabIndex = 15;
			this.m_lblCADX3.Text = "X";
			// 
			// m_lblCADX3Value
			// 
			this.m_lblCADX3Value.Location = new System.Drawing.Point(86, 96);
			this.m_lblCADX3Value.Name = "m_lblCADX3Value";
			this.m_lblCADX3Value.Size = new System.Drawing.Size(76, 23);
			this.m_lblCADX3Value.TabIndex = 16;
			this.m_lblCADX3Value.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_lblCADY3
			// 
			this.m_lblCADY3.AutoSize = true;
			this.m_lblCADY3.Location = new System.Drawing.Point(170, 100);
			this.m_lblCADY3.Name = "m_lblCADY3";
			this.m_lblCADY3.Size = new System.Drawing.Size(14, 16);
			this.m_lblCADY3.TabIndex = 17;
			this.m_lblCADY3.Text = "Y";
			// 
			// m_lblCADY3Value
			// 
			this.m_lblCADY3Value.Location = new System.Drawing.Point(186, 96);
			this.m_lblCADY3Value.Name = "m_lblCADY3Value";
			this.m_lblCADY3Value.Size = new System.Drawing.Size(76, 23);
			this.m_lblCADY3Value.TabIndex = 18;
			this.m_lblCADY3Value.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_lblCADZ3
			// 
			this.m_lblCADZ3.AutoSize = true;
			this.m_lblCADZ3.Location = new System.Drawing.Point(270, 100);
			this.m_lblCADZ3.Name = "m_lblCADZ3";
			this.m_lblCADZ3.Size = new System.Drawing.Size(14, 16);
			this.m_lblCADZ3.TabIndex = 19;
			this.m_lblCADZ3.Text = "Z";
			// 
			// m_lblCADZ3Value
			// 
			this.m_lblCADZ3Value.Location = new System.Drawing.Point(286, 96);
			this.m_lblCADZ3Value.Name = "m_lblCADZ3Value";
			this.m_lblCADZ3Value.Size = new System.Drawing.Size(76, 23);
			this.m_lblCADZ3Value.TabIndex = 20;
			this.m_lblCADZ3Value.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_pnlMachineCoord
			// 
			this.m_pnlMachineCoord.Controls.Add(this.m_lblMachinePoint1);
			this.m_pnlMachineCoord.Controls.Add(this.m_lblMachineX1);
			this.m_pnlMachineCoord.Controls.Add(this.m_numMachineX1);
			this.m_pnlMachineCoord.Controls.Add(this.m_lblMachineY1);
			this.m_pnlMachineCoord.Controls.Add(this.m_numMachineY1);
			this.m_pnlMachineCoord.Controls.Add(this.m_lblMachineZ1);
			this.m_pnlMachineCoord.Controls.Add(this.m_numMachineZ1);
			this.m_pnlMachineCoord.Controls.Add(this.m_lblMachinePoint2);
			this.m_pnlMachineCoord.Controls.Add(this.m_lblMachineX2);
			this.m_pnlMachineCoord.Controls.Add(this.m_numMachineX2);
			this.m_pnlMachineCoord.Controls.Add(this.m_lblMachineY2);
			this.m_pnlMachineCoord.Controls.Add(this.m_numMachineY2);
			this.m_pnlMachineCoord.Controls.Add(this.m_lblMachineZ2);
			this.m_pnlMachineCoord.Controls.Add(this.m_numMachineZ2);
			this.m_pnlMachineCoord.Controls.Add(this.m_lblMachinePoint3);
			this.m_pnlMachineCoord.Controls.Add(this.m_lblMachineX3);
			this.m_pnlMachineCoord.Controls.Add(this.m_numMachineX3);
			this.m_pnlMachineCoord.Controls.Add(this.m_lblMachineY3);
			this.m_pnlMachineCoord.Controls.Add(this.m_numMachineY3);
			this.m_pnlMachineCoord.Controls.Add(this.m_lblMachineZ3);
			this.m_pnlMachineCoord.Controls.Add(this.m_numMachineZ3);
			this.m_pnlMachineCoord.Location = new System.Drawing.Point(12, 149);
			this.m_pnlMachineCoord.Name = "m_pnlMachineCoord";
			this.m_pnlMachineCoord.Size = new System.Drawing.Size(376, 129);
			this.m_pnlMachineCoord.TabIndex = 1;
			this.m_pnlMachineCoord.Text = "機械座標";
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
			// m_lblMachineX1
			// 
			this.m_lblMachineX1.AutoSize = true;
			this.m_lblMachineX1.Location = new System.Drawing.Point(70, 40);
			this.m_lblMachineX1.Name = "m_lblMachineX1";
			this.m_lblMachineX1.Size = new System.Drawing.Size(14, 16);
			this.m_lblMachineX1.TabIndex = 1;
			this.m_lblMachineX1.Text = "X";
			// 
			// m_numMachineX1
			// 
			this.m_numMachineX1.DecimalPlaces = 3;
			this.m_numMachineX1.Location = new System.Drawing.Point(86, 36);
			this.m_numMachineX1.Name = "m_numMachineX1";
			this.m_numMachineX1.Size = new System.Drawing.Size(76, 23);
			this.m_numMachineX1.TabIndex = 2;
			// 
			// m_lblMachineY1
			// 
			this.m_lblMachineY1.AutoSize = true;
			this.m_lblMachineY1.Location = new System.Drawing.Point(170, 40);
			this.m_lblMachineY1.Name = "m_lblMachineY1";
			this.m_lblMachineY1.Size = new System.Drawing.Size(14, 16);
			this.m_lblMachineY1.TabIndex = 3;
			this.m_lblMachineY1.Text = "Y";
			// 
			// m_numMachineY1
			// 
			this.m_numMachineY1.DecimalPlaces = 3;
			this.m_numMachineY1.Location = new System.Drawing.Point(186, 36);
			this.m_numMachineY1.Name = "m_numMachineY1";
			this.m_numMachineY1.Size = new System.Drawing.Size(76, 23);
			this.m_numMachineY1.TabIndex = 4;
			// 
			// m_lblMachineZ1
			// 
			this.m_lblMachineZ1.AutoSize = true;
			this.m_lblMachineZ1.Location = new System.Drawing.Point(270, 40);
			this.m_lblMachineZ1.Name = "m_lblMachineZ1";
			this.m_lblMachineZ1.Size = new System.Drawing.Size(14, 16);
			this.m_lblMachineZ1.TabIndex = 5;
			this.m_lblMachineZ1.Text = "Z";
			// 
			// m_numMachineZ1
			// 
			this.m_numMachineZ1.DecimalPlaces = 3;
			this.m_numMachineZ1.Location = new System.Drawing.Point(286, 36);
			this.m_numMachineZ1.Name = "m_numMachineZ1";
			this.m_numMachineZ1.Size = new System.Drawing.Size(76, 23);
			this.m_numMachineZ1.TabIndex = 6;
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
			// m_lblMachineX2
			// 
			this.m_lblMachineX2.AutoSize = true;
			this.m_lblMachineX2.Location = new System.Drawing.Point(70, 70);
			this.m_lblMachineX2.Name = "m_lblMachineX2";
			this.m_lblMachineX2.Size = new System.Drawing.Size(14, 16);
			this.m_lblMachineX2.TabIndex = 8;
			this.m_lblMachineX2.Text = "X";
			// 
			// m_numMachineX2
			// 
			this.m_numMachineX2.DecimalPlaces = 3;
			this.m_numMachineX2.Location = new System.Drawing.Point(86, 66);
			this.m_numMachineX2.Name = "m_numMachineX2";
			this.m_numMachineX2.Size = new System.Drawing.Size(76, 23);
			this.m_numMachineX2.TabIndex = 9;
			this.m_numMachineX2.Value = new decimal(new int[] { 1, 0, 0, 0});
			// 
			// m_lblMachineY2
			// 
			this.m_lblMachineY2.AutoSize = true;
			this.m_lblMachineY2.Location = new System.Drawing.Point(170, 70);
			this.m_lblMachineY2.Name = "m_lblMachineY2";
			this.m_lblMachineY2.Size = new System.Drawing.Size(14, 16);
			this.m_lblMachineY2.TabIndex = 10;
			this.m_lblMachineY2.Text = "Y";
			// 
			// m_numMachineY2
			// 
			this.m_numMachineY2.DecimalPlaces = 3;
			this.m_numMachineY2.Location = new System.Drawing.Point(186, 66);
			this.m_numMachineY2.Name = "m_numMachineY2";
			this.m_numMachineY2.Size = new System.Drawing.Size(76, 23);
			this.m_numMachineY2.TabIndex = 11;
			// 
			// m_lblMachineZ2
			// 
			this.m_lblMachineZ2.AutoSize = true;
			this.m_lblMachineZ2.Location = new System.Drawing.Point(270, 70);
			this.m_lblMachineZ2.Name = "m_lblMachineZ2";
			this.m_lblMachineZ2.Size = new System.Drawing.Size(14, 16);
			this.m_lblMachineZ2.TabIndex = 12;
			this.m_lblMachineZ2.Text = "Z";
			// 
			// m_numMachineZ2
			// 
			this.m_numMachineZ2.DecimalPlaces = 3;
			this.m_numMachineZ2.Location = new System.Drawing.Point(286, 66);
			this.m_numMachineZ2.Name = "m_numMachineZ2";
			this.m_numMachineZ2.Size = new System.Drawing.Size(76, 23);
			this.m_numMachineZ2.TabIndex = 13;
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
			// m_lblMachineX3
			// 
			this.m_lblMachineX3.AutoSize = true;
			this.m_lblMachineX3.Location = new System.Drawing.Point(70, 100);
			this.m_lblMachineX3.Name = "m_lblMachineX3";
			this.m_lblMachineX3.Size = new System.Drawing.Size(14, 16);
			this.m_lblMachineX3.TabIndex = 15;
			this.m_lblMachineX3.Text = "X";
			// 
			// m_numMachineX3
			// 
			this.m_numMachineX3.DecimalPlaces = 3;
			this.m_numMachineX3.Location = new System.Drawing.Point(86, 96);
			this.m_numMachineX3.Name = "m_numMachineX3";
			this.m_numMachineX3.Size = new System.Drawing.Size(76, 23);
			this.m_numMachineX3.TabIndex = 16;
			// 
			// m_lblMachineY3
			// 
			this.m_lblMachineY3.AutoSize = true;
			this.m_lblMachineY3.Location = new System.Drawing.Point(170, 100);
			this.m_lblMachineY3.Name = "m_lblMachineY3";
			this.m_lblMachineY3.Size = new System.Drawing.Size(14, 16);
			this.m_lblMachineY3.TabIndex = 17;
			this.m_lblMachineY3.Text = "Y";
			// 
			// m_numMachineY3
			// 
			this.m_numMachineY3.DecimalPlaces = 3;
			this.m_numMachineY3.Location = new System.Drawing.Point(186, 96);
			this.m_numMachineY3.Name = "m_numMachineY3";
			this.m_numMachineY3.Size = new System.Drawing.Size(76, 23);
			this.m_numMachineY3.TabIndex = 18;
			this.m_numMachineY3.Value = new decimal(new int[] { 1, 0, 0, 0});
			// 
			// m_lblMachineZ3
			// 
			this.m_lblMachineZ3.AutoSize = true;
			this.m_lblMachineZ3.Location = new System.Drawing.Point(270, 100);
			this.m_lblMachineZ3.Name = "m_lblMachineZ3";
			this.m_lblMachineZ3.Size = new System.Drawing.Size(14, 16);
			this.m_lblMachineZ3.TabIndex = 19;
			this.m_lblMachineZ3.Text = "Z";
			// 
			// m_numMachineZ3
			// 
			this.m_numMachineZ3.DecimalPlaces = 3;
			this.m_numMachineZ3.Location = new System.Drawing.Point(286, 96);
			this.m_numMachineZ3.Name = "m_numMachineZ3";
			this.m_numMachineZ3.Size = new System.Drawing.Size(76, 23);
			this.m_numMachineZ3.TabIndex = 20;
			// 
			// m_btnOK
			// 
			this.m_btnOK.CommonText = CommonUI.Controls.CommonButtonText.Confirm;
			this.m_btnOK.Kind = CommonUI.Controls.ButtonKind.Primary;
			this.m_btnOK.Location = new System.Drawing.Point(308, 294);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(80, 28);
			this.m_btnOK.TabIndex = 2;
			this.m_btnOK.Text = "確定";
			this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
			// 
			// ThreePtTransformDlg
			// 
			this.ClientSize = new System.Drawing.Size(400, 334);
			this.Controls.Add(this.m_pnlCADCoord);
			this.Controls.Add(this.m_pnlMachineCoord);
			this.Controls.Add(this.m_btnOK);
			this.Name = "ThreePtTransformDlg";
			this.Text = "三點轉換";
			this.m_pnlCADCoord.ResumeLayout(false);
			this.m_pnlCADCoord.PerformLayout();
			this.m_pnlMachineCoord.ResumeLayout(false);
			this.m_pnlMachineCoord.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineX1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineY1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineZ1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineX2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineY2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineZ2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineX3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineY3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numMachineZ3)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlCADCoord;
		private System.Windows.Forms.Label m_lblCADPoint1;
		private System.Windows.Forms.Label m_lblCADX1;
		private System.Windows.Forms.Label m_lblCADX1Value;
		private System.Windows.Forms.Label m_lblCADY1;
		private System.Windows.Forms.Label m_lblCADY1Value;
		private System.Windows.Forms.Label m_lblCADZ1;
		private System.Windows.Forms.Label m_lblCADZ1Value;
		private System.Windows.Forms.Label m_lblCADPoint2;
		private System.Windows.Forms.Label m_lblCADX2;
		private System.Windows.Forms.Label m_lblCADX2Value;
		private System.Windows.Forms.Label m_lblCADY2;
		private System.Windows.Forms.Label m_lblCADY2Value;
		private System.Windows.Forms.Label m_lblCADZ2;
		private System.Windows.Forms.Label m_lblCADZ2Value;
		private System.Windows.Forms.Label m_lblCADPoint3;
		private System.Windows.Forms.Label m_lblCADX3;
		private System.Windows.Forms.Label m_lblCADX3Value;
		private System.Windows.Forms.Label m_lblCADY3;
		private System.Windows.Forms.Label m_lblCADY3Value;
		private System.Windows.Forms.Label m_lblCADZ3;
		private System.Windows.Forms.Label m_lblCADZ3Value;
		private CommonUI.Controls.SectionPanel m_pnlMachineCoord;
		private System.Windows.Forms.Label m_lblMachinePoint1;
		private System.Windows.Forms.Label m_lblMachineX1;
		private CommonUI.Controls.DebouncedNumericUpDown m_numMachineX1;
		private System.Windows.Forms.Label m_lblMachineY1;
		private CommonUI.Controls.DebouncedNumericUpDown m_numMachineY1;
		private System.Windows.Forms.Label m_lblMachineZ1;
		private CommonUI.Controls.DebouncedNumericUpDown m_numMachineZ1;
		private System.Windows.Forms.Label m_lblMachinePoint2;
		private System.Windows.Forms.Label m_lblMachineX2;
		private CommonUI.Controls.DebouncedNumericUpDown m_numMachineX2;
		private System.Windows.Forms.Label m_lblMachineY2;
		private CommonUI.Controls.DebouncedNumericUpDown m_numMachineY2;
		private System.Windows.Forms.Label m_lblMachineZ2;
		private CommonUI.Controls.DebouncedNumericUpDown m_numMachineZ2;
		private System.Windows.Forms.Label m_lblMachinePoint3;
		private System.Windows.Forms.Label m_lblMachineX3;
		private CommonUI.Controls.DebouncedNumericUpDown m_numMachineX3;
		private System.Windows.Forms.Label m_lblMachineY3;
		private CommonUI.Controls.DebouncedNumericUpDown m_numMachineY3;
		private System.Windows.Forms.Label m_lblMachineZ3;
		private CommonUI.Controls.DebouncedNumericUpDown m_numMachineZ3;
		private CommonUI.Controls.ThemedButton m_btnOK;
	}
}
