namespace MyCAM.Editor
{
	partial class TraverseParamSettingForm
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
			this.m_btnConfirm = new System.Windows.Forms.Button();
			this.m_lblUnit1 = new System.Windows.Forms.Label();
			this.m_lblUnit = new System.Windows.Forms.Label();
			this.m_txbFollowSafeDistance = new System.Windows.Forms.TextBox();
			this.m_txbCutDownDistance = new System.Windows.Forms.TextBox();
			this.m_lblFollowSaveDistance = new System.Windows.Forms.Label();
			this.m_lblCutDownDistance = new System.Windows.Forms.Label();
			this.m_lblUnit2 = new System.Windows.Forms.Label();
			this.m_txbLiftUpDistance = new System.Windows.Forms.TextBox();
			this.m_lblLiftUpDistance = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// m_btnConfirm
			// 
			this.m_btnConfirm.Location = new System.Drawing.Point(225, 217);
			this.m_btnConfirm.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_btnConfirm.Name = "m_btnConfirm";
			this.m_btnConfirm.Size = new System.Drawing.Size(116, 34);
			this.m_btnConfirm.TabIndex = 13;
			this.m_btnConfirm.Text = "確認";
			this.m_btnConfirm.UseVisualStyleBackColor = true;
			this.m_btnConfirm.Click += new System.EventHandler(this.m_btnConfirm_Click);
			// 
			// m_lblUnit1
			// 
			this.m_lblUnit1.AutoSize = true;
			this.m_lblUnit1.Location = new System.Drawing.Point(299, 105);
			this.m_lblUnit1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblUnit1.Name = "m_lblUnit1";
			this.m_lblUnit1.Size = new System.Drawing.Size(34, 18);
			this.m_lblUnit1.TabIndex = 12;
			this.m_lblUnit1.Text = "mm";
			// 
			// m_lblUnit
			// 
			this.m_lblUnit.AutoSize = true;
			this.m_lblUnit.Location = new System.Drawing.Point(299, 52);
			this.m_lblUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblUnit.Name = "m_lblUnit";
			this.m_lblUnit.Size = new System.Drawing.Size(34, 18);
			this.m_lblUnit.TabIndex = 11;
			this.m_lblUnit.Text = "mm";
			// 
			// m_txbFollowSafeDistance
			// 
			this.m_txbFollowSafeDistance.Location = new System.Drawing.Point(176, 98);
			this.m_txbFollowSafeDistance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_txbFollowSafeDistance.Name = "m_txbFollowSafeDistance";
			this.m_txbFollowSafeDistance.Size = new System.Drawing.Size(115, 26);
			this.m_txbFollowSafeDistance.TabIndex = 10;
			this.m_txbFollowSafeDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbSafeDistance_KeyDown);
			this.m_txbFollowSafeDistance.Leave += new System.EventHandler(this.m_txbFollowSafeDistance_Leave);
			// 
			// m_txbCutDownDistance
			// 
			this.m_txbCutDownDistance.Location = new System.Drawing.Point(176, 49);
			this.m_txbCutDownDistance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_txbCutDownDistance.Name = "m_txbCutDownDistance";
			this.m_txbCutDownDistance.Size = new System.Drawing.Size(115, 26);
			this.m_txbCutDownDistance.TabIndex = 9;
			this.m_txbCutDownDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbCutDownDistance_KeyDown);
			this.m_txbCutDownDistance.Leave += new System.EventHandler(this.m_txbCutDownDistance_Leave);
			// 
			// m_lblFollowSaveDistance
			// 
			this.m_lblFollowSaveDistance.AutoSize = true;
			this.m_lblFollowSaveDistance.Location = new System.Drawing.Point(15, 98);
			this.m_lblFollowSaveDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblFollowSaveDistance.Name = "m_lblFollowSaveDistance";
			this.m_lblFollowSaveDistance.Size = new System.Drawing.Size(98, 18);
			this.m_lblFollowSaveDistance.TabIndex = 8;
			this.m_lblFollowSaveDistance.Text = "隨動安全距離";
			// 
			// m_lblCutDownDistance
			// 
			this.m_lblCutDownDistance.AutoSize = true;
			this.m_lblCutDownDistance.Location = new System.Drawing.Point(15, 49);
			this.m_lblCutDownDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblCutDownDistance.Name = "m_lblCutDownDistance";
			this.m_lblCutDownDistance.Size = new System.Drawing.Size(68, 18);
			this.m_lblCutDownDistance.TabIndex = 7;
			this.m_lblCutDownDistance.Text = "下刀距離";
			// 
			// m_lblUnit2
			// 
			this.m_lblUnit2.AutoSize = true;
			this.m_lblUnit2.Location = new System.Drawing.Point(299, 157);
			this.m_lblUnit2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.m_lblUnit2.Name = "m_lblUnit2";
			this.m_lblUnit2.Size = new System.Drawing.Size(34, 18);
			this.m_lblUnit2.TabIndex = 16;
			this.m_lblUnit2.Text = "mm";
			// 
			// m_txbLiftUpDistance
			// 
			this.m_txbLiftUpDistance.Location = new System.Drawing.Point(172, 154);
			this.m_txbLiftUpDistance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_txbLiftUpDistance.Name = "m_txbLiftUpDistance";
			this.m_txbLiftUpDistance.Size = new System.Drawing.Size(115, 26);
			this.m_txbLiftUpDistance.TabIndex = 15;
			this.m_txbLiftUpDistance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_txbLiftUpDistance_KeyDown);
			this.m_txbLiftUpDistance.Leave += new System.EventHandler(this.m_txbLiftUpDistance_Leave);
			// 
			// m_lblLiftUpDistance
			// 
			this.m_lblLiftUpDistance.AutoSize = true;
			this.m_lblLiftUpDistance.Location = new System.Drawing.Point(15, 154);
			this.m_lblLiftUpDistance.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_lblLiftUpDistance.Name = "m_lblLiftUpDistance";
			this.m_lblLiftUpDistance.Size = new System.Drawing.Size(68, 18);
			this.m_lblLiftUpDistance.TabIndex = 14;
			this.m_lblLiftUpDistance.Text = "抬刀距離";
			// 
			// TraverseParamSettingForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(349, 270);
			this.Controls.Add(this.m_lblUnit2);
			this.Controls.Add(this.m_txbLiftUpDistance);
			this.Controls.Add(this.m_lblLiftUpDistance);
			this.Controls.Add(this.m_btnConfirm);
			this.Controls.Add(this.m_lblUnit1);
			this.Controls.Add(this.m_lblUnit);
			this.Controls.Add(this.m_txbFollowSafeDistance);
			this.Controls.Add(this.m_txbCutDownDistance);
			this.Controls.Add(this.m_lblFollowSaveDistance);
			this.Controls.Add(this.m_lblCutDownDistance);
			this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "TraverseParamSettingForm";
			this.Text = "後處理參數設定";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_btnConfirm;
		private System.Windows.Forms.Label m_lblUnit1;
		private System.Windows.Forms.Label m_lblUnit;
		private System.Windows.Forms.TextBox m_txbFollowSafeDistance;
		private System.Windows.Forms.TextBox m_txbCutDownDistance;
		private System.Windows.Forms.Label m_lblFollowSaveDistance;
		private System.Windows.Forms.Label m_lblCutDownDistance;
		private System.Windows.Forms.Label m_lblUnit2;
		private System.Windows.Forms.TextBox m_txbLiftUpDistance;
		private System.Windows.Forms.Label m_lblLiftUpDistance;
	}
}