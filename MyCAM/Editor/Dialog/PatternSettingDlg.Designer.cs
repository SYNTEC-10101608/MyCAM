namespace MyCAM.Editor.Dialog
{
	partial class PatternSettingDlg
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
			this.m_panelCircle = new System.Windows.Forms.Panel();
			this.m_txbCircleRotatedAngle = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.m_txbCircleDiameter = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.m_panelRunway = new System.Windows.Forms.Panel();
			this.m_txbRunwayRotatedAngle = new System.Windows.Forms.TextBox();
			this.label12 = new System.Windows.Forms.Label();
			this.m_txbRunwayWidth = new System.Windows.Forms.TextBox();
			this.label14 = new System.Windows.Forms.Label();
			this.m_txbRunwayLength = new System.Windows.Forms.TextBox();
			this.label15 = new System.Windows.Forms.Label();
			this.m_panelPolygon = new System.Windows.Forms.Panel();
			this.m_txbPolygonCornerRadius = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.m_txbPolygonRotatedAngle = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.m_txbPolygonSideLength = new System.Windows.Forms.TextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.m_btnConfirm = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.m_panelRectangle = new System.Windows.Forms.Panel();
			this.m_txbRecRotatedAngle = new System.Windows.Forms.TextBox();
			this.m_txbRecCornerRadius = new System.Windows.Forms.TextBox();
			this.m_txbRecWidth = new System.Windows.Forms.TextBox();
			this.m_txbRecLength = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.m_cmbPathType = new System.Windows.Forms.ComboBox();
			this.m_panelCircle.SuspendLayout();
			this.m_panelRunway.SuspendLayout();
			this.m_panelPolygon.SuspendLayout();
			this.m_panelRectangle.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_panelCircle
			// 
			this.m_panelCircle.Controls.Add(this.m_txbCircleRotatedAngle);
			this.m_panelCircle.Controls.Add(this.label7);
			this.m_panelCircle.Controls.Add(this.m_txbCircleDiameter);
			this.m_panelCircle.Controls.Add(this.label6);
			this.m_panelCircle.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_panelCircle.Location = new System.Drawing.Point(22, 54);
			this.m_panelCircle.Name = "m_panelCircle";
			this.m_panelCircle.Size = new System.Drawing.Size(260, 260);
			this.m_panelCircle.TabIndex = 31;
			this.m_panelCircle.Visible = false;
			// 
			// m_txbCircleRotatedAngle
			// 
			this.m_txbCircleRotatedAngle.Location = new System.Drawing.Point(140, 80);
			this.m_txbCircleRotatedAngle.Name = "m_txbCircleRotatedAngle";
			this.m_txbCircleRotatedAngle.Size = new System.Drawing.Size(100, 22);
			this.m_txbCircleRotatedAngle.TabIndex = 19;
			this.m_txbCircleRotatedAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbCircleRotatedAngle_KeyDown);
			this.m_txbCircleRotatedAngle.Leave += new System.EventHandler(this.m_txbCircleRotatedAngle_Leave);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(20, 80);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(76, 17);
			this.label7.TabIndex = 18;
			this.label7.Text = "旋轉角度:";
			// 
			// m_txbCircleDiameter
			// 
			this.m_txbCircleDiameter.Location = new System.Drawing.Point(140, 30);
			this.m_txbCircleDiameter.Name = "m_txbCircleDiameter";
			this.m_txbCircleDiameter.Size = new System.Drawing.Size(100, 22);
			this.m_txbCircleDiameter.TabIndex = 17;
			this.m_txbCircleDiameter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbCircleDiameter_KeyDown);
			this.m_txbCircleDiameter.Leave += new System.EventHandler(this.m_txbCircleDiameter_Leave);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label6.Location = new System.Drawing.Point(20, 30);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(72, 17);
			this.label6.TabIndex = 16;
			this.label6.Text = "圓直徑：";
			// 
			// m_panelRunway
			// 
			this.m_panelRunway.Controls.Add(this.m_txbRunwayRotatedAngle);
			this.m_panelRunway.Controls.Add(this.label12);
			this.m_panelRunway.Controls.Add(this.m_txbRunwayWidth);
			this.m_panelRunway.Controls.Add(this.label14);
			this.m_panelRunway.Controls.Add(this.m_txbRunwayLength);
			this.m_panelRunway.Controls.Add(this.label15);
			this.m_panelRunway.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_panelRunway.Location = new System.Drawing.Point(22, 54);
			this.m_panelRunway.Name = "m_panelRunway";
			this.m_panelRunway.Size = new System.Drawing.Size(260, 260);
			this.m_panelRunway.TabIndex = 30;
			this.m_panelRunway.Visible = false;
			// 
			// m_txbRunwayRotatedAngle
			// 
			this.m_txbRunwayRotatedAngle.Location = new System.Drawing.Point(140, 130);
			this.m_txbRunwayRotatedAngle.Name = "m_txbRunwayRotatedAngle";
			this.m_txbRunwayRotatedAngle.Size = new System.Drawing.Size(100, 22);
			this.m_txbRunwayRotatedAngle.TabIndex = 23;
			this.m_txbRunwayRotatedAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbRunwayRotatedAngle_KeyDown);
			this.m_txbRunwayRotatedAngle.Leave += new System.EventHandler(this.m_txbRunwayRotatedAngle_Leave);
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label12.Location = new System.Drawing.Point(20, 130);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(76, 17);
			this.label12.TabIndex = 22;
			this.label12.Text = "旋轉角度:";
			// 
			// m_txbRunwayWidth
			// 
			this.m_txbRunwayWidth.Location = new System.Drawing.Point(140, 80);
			this.m_txbRunwayWidth.Name = "m_txbRunwayWidth";
			this.m_txbRunwayWidth.Size = new System.Drawing.Size(100, 22);
			this.m_txbRunwayWidth.TabIndex = 21;
			this.m_txbRunwayWidth.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbRunwayWidth_KeyDown);
			this.m_txbRunwayWidth.Leave += new System.EventHandler(this.m_txbRunwayWidth_Leave);
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label14.Location = new System.Drawing.Point(20, 80);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(76, 17);
			this.label14.TabIndex = 20;
			this.label14.Text = "寬邊距離:";
			// 
			// m_txbRunwayLength
			// 
			this.m_txbRunwayLength.Location = new System.Drawing.Point(140, 30);
			this.m_txbRunwayLength.Name = "m_txbRunwayLength";
			this.m_txbRunwayLength.Size = new System.Drawing.Size(100, 22);
			this.m_txbRunwayLength.TabIndex = 19;
			this.m_txbRunwayLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbRunwayLength_KeyDown);
			this.m_txbRunwayLength.Leave += new System.EventHandler(this.m_txbRunwayLength_Leave);
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label15.Location = new System.Drawing.Point(20, 30);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(76, 17);
			this.label15.TabIndex = 18;
			this.label15.Text = "長邊距離:";
			// 
			// m_panelPolygon
			// 
			this.m_panelPolygon.Controls.Add(this.m_txbPolygonCornerRadius);
			this.m_panelPolygon.Controls.Add(this.label8);
			this.m_panelPolygon.Controls.Add(this.m_txbPolygonRotatedAngle);
			this.m_panelPolygon.Controls.Add(this.label11);
			this.m_panelPolygon.Controls.Add(this.m_txbPolygonSideLength);
			this.m_panelPolygon.Controls.Add(this.label13);
			this.m_panelPolygon.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_panelPolygon.Location = new System.Drawing.Point(22, 54);
			this.m_panelPolygon.Name = "m_panelPolygon";
			this.m_panelPolygon.Size = new System.Drawing.Size(260, 260);
			this.m_panelPolygon.TabIndex = 29;
			this.m_panelPolygon.Visible = false;
			// 
			// m_txbPolygonCornerRadius
			// 
			this.m_txbPolygonCornerRadius.Location = new System.Drawing.Point(140, 80);
			this.m_txbPolygonCornerRadius.Name = "m_txbPolygonCornerRadius";
			this.m_txbPolygonCornerRadius.Size = new System.Drawing.Size(100, 22);
			this.m_txbPolygonCornerRadius.TabIndex = 19;
			this.m_txbPolygonCornerRadius.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbPolygonCornerRadius_KeyDown);
			this.m_txbPolygonCornerRadius.Leave += new System.EventHandler(this.m_txbPolygonCornerRadius_Leave);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label8.Location = new System.Drawing.Point(20, 80);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(76, 17);
			this.label8.TabIndex = 18;
			this.label8.Text = "倒角半徑:";
			// 
			// m_txbPolygonRotatedAngle
			// 
			this.m_txbPolygonRotatedAngle.Location = new System.Drawing.Point(140, 130);
			this.m_txbPolygonRotatedAngle.Name = "m_txbPolygonRotatedAngle";
			this.m_txbPolygonRotatedAngle.Size = new System.Drawing.Size(100, 22);
			this.m_txbPolygonRotatedAngle.TabIndex = 17;
			this.m_txbPolygonRotatedAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbPolygonRotatedAngle_KeyDown);
			this.m_txbPolygonRotatedAngle.Leave += new System.EventHandler(this.m_txbPolygonRotatedAngle_Leave);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label11.Location = new System.Drawing.Point(20, 130);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(76, 17);
			this.label11.TabIndex = 16;
			this.label11.Text = "旋轉角度:";
			// 
			// m_txbPolygonSideLength
			// 
			this.m_txbPolygonSideLength.Location = new System.Drawing.Point(140, 30);
			this.m_txbPolygonSideLength.Name = "m_txbPolygonSideLength";
			this.m_txbPolygonSideLength.Size = new System.Drawing.Size(100, 22);
			this.m_txbPolygonSideLength.TabIndex = 13;
			this.m_txbPolygonSideLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbPolygonSideLength_KeyDown);
			this.m_txbPolygonSideLength.Leave += new System.EventHandler(this.m_txbPolygonSideLength_Leave);
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label13.Location = new System.Drawing.Point(20, 30);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(44, 17);
			this.label13.TabIndex = 12;
			this.label13.Text = "邊長:";
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_btnConfirm.Location = new System.Drawing.Point(203, 320);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(80, 30);
			this.m_btnConfirm.TabIndex = 28;
			this.m_btnConfirm.Text = "確認";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(31, 18);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(76, 17);
			this.label1.TabIndex = 27;
			this.label1.Text = "輪廓種類:";
			// 
			// m_panelRectangle
			// 
			this.m_panelRectangle.Controls.Add(this.m_txbRecRotatedAngle);
			this.m_panelRectangle.Controls.Add(this.m_txbRecCornerRadius);
			this.m_panelRectangle.Controls.Add(this.m_txbRecWidth);
			this.m_panelRectangle.Controls.Add(this.m_txbRecLength);
			this.m_panelRectangle.Controls.Add(this.label5);
			this.m_panelRectangle.Controls.Add(this.label4);
			this.m_panelRectangle.Controls.Add(this.label3);
			this.m_panelRectangle.Controls.Add(this.label2);
			this.m_panelRectangle.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_panelRectangle.Location = new System.Drawing.Point(22, 54);
			this.m_panelRectangle.Name = "m_panelRectangle";
			this.m_panelRectangle.Size = new System.Drawing.Size(260, 260);
			this.m_panelRectangle.TabIndex = 26;
			this.m_panelRectangle.Visible = false;
			// 
			// m_txbRecRotatedAngle
			// 
			this.m_txbRecRotatedAngle.Location = new System.Drawing.Point(140, 180);
			this.m_txbRecRotatedAngle.Name = "m_txbRecRotatedAngle";
			this.m_txbRecRotatedAngle.Size = new System.Drawing.Size(100, 22);
			this.m_txbRecRotatedAngle.TabIndex = 16;
			this.m_txbRecRotatedAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbRecRotatedAngle_KeyDown);
			this.m_txbRecRotatedAngle.Leave += new System.EventHandler(this.m_txbRecRotatedAngle_Leave);
			// 
			// m_txbRecCornerRadius
			// 
			this.m_txbRecCornerRadius.Location = new System.Drawing.Point(140, 130);
			this.m_txbRecCornerRadius.Name = "m_txbRecCornerRadius";
			this.m_txbRecCornerRadius.Size = new System.Drawing.Size(100, 22);
			this.m_txbRecCornerRadius.TabIndex = 10;
			this.m_txbRecCornerRadius.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbRecCornerRadius_KeyDown);
			this.m_txbRecCornerRadius.Leave += new System.EventHandler(this.m_txbRecCornerRadius_Leave);
			// 
			// m_txbRecWidth
			// 
			this.m_txbRecWidth.Location = new System.Drawing.Point(140, 80);
			this.m_txbRecWidth.Name = "m_txbRecWidth";
			this.m_txbRecWidth.Size = new System.Drawing.Size(100, 22);
			this.m_txbRecWidth.TabIndex = 9;
			this.m_txbRecWidth.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbRecWidth_KeyDown);
			this.m_txbRecWidth.Leave += new System.EventHandler(this.m_txbRecWidth_Leave);
			// 
			// m_txbRecLength
			// 
			this.m_txbRecLength.Location = new System.Drawing.Point(140, 30);
			this.m_txbRecLength.Name = "m_txbRecLength";
			this.m_txbRecLength.Size = new System.Drawing.Size(100, 22);
			this.m_txbRecLength.TabIndex = 8;
			this.m_txbRecLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbRecLength_KeyDown);
			this.m_txbRecLength.Leave += new System.EventHandler(this.m_txbRecLength_Leave);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(20, 180);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(88, 17);
			this.label5.TabIndex = 7;
			this.label5.Text = "旋轉角度：";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(20, 130);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(88, 17);
			this.label4.TabIndex = 6;
			this.label4.Text = "倒角半徑：";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(20, 80);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(88, 17);
			this.label3.TabIndex = 5;
			this.label3.Text = "寬邊距離：";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(20, 30);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(88, 17);
			this.label2.TabIndex = 4;
			this.label2.Text = "長邊距離：";
			// 
			// m_cmbPathType
			// 
			this.m_cmbPathType.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_cmbPathType.FormattingEnabled = true;
			this.m_cmbPathType.Items.AddRange(new object[] {
            "輪廓",
            "圓形",
            "矩形",
            "跑道形",
            "三角形",
            "正方形",
            "五邊形",
            "六邊形"});
			this.m_cmbPathType.Location = new System.Drawing.Point(113, 16);
			this.m_cmbPathType.Name = "m_cmbPathType";
			this.m_cmbPathType.Size = new System.Drawing.Size(149, 24);
			this.m_cmbPathType.TabIndex = 25;
			this.m_cmbPathType.SelectedIndexChanged += new System.EventHandler(this.m_cmbPathType_SelectedIndexChanged);
			// 
			// PatternSettingDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(305, 366);
			this.Controls.Add(this.m_panelCircle);
			this.Controls.Add(this.m_panelRunway);
			this.Controls.Add(this.m_panelPolygon);
			this.Controls.Add(this.m_btnConfirm);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.m_panelRectangle);
			this.Controls.Add(this.m_cmbPathType);
			this.Location = new System.Drawing.Point(22, 54);
			this.Name = "PatternSettingDlg";
			this.Text = "PatternSettingDlg";
			this.m_panelCircle.ResumeLayout(false);
			this.m_panelCircle.PerformLayout();
			this.m_panelRunway.ResumeLayout(false);
			this.m_panelRunway.PerformLayout();
			this.m_panelPolygon.ResumeLayout(false);
			this.m_panelPolygon.PerformLayout();
			this.m_panelRectangle.ResumeLayout(false);
			this.m_panelRectangle.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel m_panelCircle;
		private System.Windows.Forms.TextBox m_txbCircleRotatedAngle;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox m_txbCircleDiameter;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Panel m_panelRunway;
		private System.Windows.Forms.TextBox m_txbRunwayRotatedAngle;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.TextBox m_txbRunwayWidth;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.TextBox m_txbRunwayLength;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Panel m_panelPolygon;
		private System.Windows.Forms.TextBox m_txbPolygonCornerRadius;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox m_txbPolygonRotatedAngle;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.TextBox m_txbPolygonSideLength;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Button m_btnConfirm;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel m_panelRectangle;
		private System.Windows.Forms.TextBox m_txbRecRotatedAngle;
		private System.Windows.Forms.TextBox m_txbRecCornerRadius;
		private System.Windows.Forms.TextBox m_txbRecWidth;
		private System.Windows.Forms.TextBox m_txbRecLength;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox m_cmbPathType;
	}
}