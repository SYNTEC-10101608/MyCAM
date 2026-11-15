namespace CncUI.Dialog
{
	partial class CNCOffsetDialog
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
			this.m_pnlOffset = new CommonUI.Controls.SectionPanel();
			this.m_lblX = new CommonUI.Controls.ThemedLabel();
			this.m_lblXValue = new CommonUI.Controls.ThemedLabel();
			this.m_btnXZero = new CommonUI.Controls.ThemedButton();
			this.m_lblY = new CommonUI.Controls.ThemedLabel();
			this.m_lblYValue = new CommonUI.Controls.ThemedLabel();
			this.m_btnYZero = new CommonUI.Controls.ThemedButton();
			this.m_lblZ = new CommonUI.Controls.ThemedLabel();
			this.m_lblZValue = new CommonUI.Controls.ThemedLabel();
			this.m_btnZZero = new CommonUI.Controls.ThemedButton();
			this.m_lblRotary1 = new CommonUI.Controls.ThemedLabel();
			this.m_lblRotary1Value = new CommonUI.Controls.ThemedLabel();
			this.m_btnRotary1Zero = new CommonUI.Controls.ThemedButton();
			this.m_lblRotary2 = new CommonUI.Controls.ThemedLabel();
			this.m_lblRotary2Value = new CommonUI.Controls.ThemedLabel();
			this.m_btnRotary2Zero = new CommonUI.Controls.ThemedButton();
			this.m_btnClear = new CommonUI.Controls.ThemedButton();
			this.m_pnlOffset.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_pnlOffset
			// 
			this.m_pnlOffset.Controls.Add(this.m_lblX);
			this.m_pnlOffset.Controls.Add(this.m_lblXValue);
			this.m_pnlOffset.Controls.Add(this.m_btnXZero);
			this.m_pnlOffset.Controls.Add(this.m_lblY);
			this.m_pnlOffset.Controls.Add(this.m_lblYValue);
			this.m_pnlOffset.Controls.Add(this.m_btnYZero);
			this.m_pnlOffset.Controls.Add(this.m_lblZ);
			this.m_pnlOffset.Controls.Add(this.m_lblZValue);
			this.m_pnlOffset.Controls.Add(this.m_btnZZero);
			this.m_pnlOffset.Controls.Add(this.m_lblRotary1);
			this.m_pnlOffset.Controls.Add(this.m_lblRotary1Value);
			this.m_pnlOffset.Controls.Add(this.m_btnRotary1Zero);
			this.m_pnlOffset.Controls.Add(this.m_lblRotary2);
			this.m_pnlOffset.Controls.Add(this.m_lblRotary2Value);
			this.m_pnlOffset.Controls.Add(this.m_btnRotary2Zero);
			this.m_pnlOffset.Location = new System.Drawing.Point(12, 12);
			this.m_pnlOffset.Name = "m_pnlOffset";
			this.m_pnlOffset.Size = new System.Drawing.Size(296, 163);
			this.m_pnlOffset.TabIndex = 0;
			// 
			// m_lblX
			// 
			this.m_lblX.Location = new System.Drawing.Point(10, 14);
			this.m_lblX.Name = "m_lblX";
			this.m_lblX.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblX.Size = new System.Drawing.Size(24, 19);
			this.m_lblX.TabIndex = 0;
			this.m_lblX.Text = "X";
			// 
			// m_lblXValue
			// 
			this.m_lblXValue.Location = new System.Drawing.Point(40, 10);
			this.m_lblXValue.Name = "m_lblXValue";
			this.m_lblXValue.Role = CommonUI.Theme.TextRole.Value;
			this.m_lblXValue.Size = new System.Drawing.Size(160, 23);
			this.m_lblXValue.TabIndex = 1;
			this.m_lblXValue.Text = "— mm";
			this.m_lblXValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_btnXZero
			// 
			this.m_btnXZero.Location = new System.Drawing.Point(206, 10);
			this.m_btnXZero.Name = "m_btnXZero";
			this.m_btnXZero.Size = new System.Drawing.Size(80, 23);
			this.m_btnXZero.TabIndex = 2;
			this.m_btnXZero.Text = "設為 0";
			// 
			// m_lblY
			// 
			this.m_lblY.Location = new System.Drawing.Point(10, 44);
			this.m_lblY.Name = "m_lblY";
			this.m_lblY.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblY.Size = new System.Drawing.Size(24, 19);
			this.m_lblY.TabIndex = 3;
			this.m_lblY.Text = "Y";
			// 
			// m_lblYValue
			// 
			this.m_lblYValue.Location = new System.Drawing.Point(40, 40);
			this.m_lblYValue.Name = "m_lblYValue";
			this.m_lblYValue.Role = CommonUI.Theme.TextRole.Value;
			this.m_lblYValue.Size = new System.Drawing.Size(160, 23);
			this.m_lblYValue.TabIndex = 4;
			this.m_lblYValue.Text = "— mm";
			this.m_lblYValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_btnYZero
			// 
			this.m_btnYZero.Location = new System.Drawing.Point(206, 40);
			this.m_btnYZero.Name = "m_btnYZero";
			this.m_btnYZero.Size = new System.Drawing.Size(80, 23);
			this.m_btnYZero.TabIndex = 5;
			this.m_btnYZero.Text = "設為 0";
			// 
			// m_lblZ
			// 
			this.m_lblZ.Location = new System.Drawing.Point(10, 74);
			this.m_lblZ.Name = "m_lblZ";
			this.m_lblZ.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblZ.Size = new System.Drawing.Size(24, 19);
			this.m_lblZ.TabIndex = 6;
			this.m_lblZ.Text = "Z";
			// 
			// m_lblZValue
			// 
			this.m_lblZValue.Location = new System.Drawing.Point(40, 70);
			this.m_lblZValue.Name = "m_lblZValue";
			this.m_lblZValue.Role = CommonUI.Theme.TextRole.Value;
			this.m_lblZValue.Size = new System.Drawing.Size(160, 23);
			this.m_lblZValue.TabIndex = 7;
			this.m_lblZValue.Text = "— mm";
			this.m_lblZValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_btnZZero
			// 
			this.m_btnZZero.Location = new System.Drawing.Point(206, 70);
			this.m_btnZZero.Name = "m_btnZZero";
			this.m_btnZZero.Size = new System.Drawing.Size(80, 23);
			this.m_btnZZero.TabIndex = 8;
			this.m_btnZZero.Text = "設為 0";
			// 
			// m_lblRotary1
			// 
			this.m_lblRotary1.Location = new System.Drawing.Point(10, 104);
			this.m_lblRotary1.Name = "m_lblRotary1";
			this.m_lblRotary1.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblRotary1.Size = new System.Drawing.Size(24, 19);
			this.m_lblRotary1.TabIndex = 9;
			this.m_lblRotary1.Text = "—";
			// 
			// m_lblRotary1Value
			// 
			this.m_lblRotary1Value.Location = new System.Drawing.Point(40, 100);
			this.m_lblRotary1Value.Name = "m_lblRotary1Value";
			this.m_lblRotary1Value.Role = CommonUI.Theme.TextRole.Value;
			this.m_lblRotary1Value.Size = new System.Drawing.Size(160, 23);
			this.m_lblRotary1Value.TabIndex = 10;
			this.m_lblRotary1Value.Text = "— deg";
			this.m_lblRotary1Value.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_btnRotary1Zero
			// 
			this.m_btnRotary1Zero.Location = new System.Drawing.Point(206, 100);
			this.m_btnRotary1Zero.Name = "m_btnRotary1Zero";
			this.m_btnRotary1Zero.Size = new System.Drawing.Size(80, 23);
			this.m_btnRotary1Zero.TabIndex = 11;
			this.m_btnRotary1Zero.Text = "設為 0";
			// 
			// m_lblRotary2
			// 
			this.m_lblRotary2.Location = new System.Drawing.Point(10, 134);
			this.m_lblRotary2.Name = "m_lblRotary2";
			this.m_lblRotary2.Role = CommonUI.Theme.TextRole.Emphasis;
			this.m_lblRotary2.Size = new System.Drawing.Size(24, 19);
			this.m_lblRotary2.TabIndex = 12;
			this.m_lblRotary2.Text = "—";
			// 
			// m_lblRotary2Value
			// 
			this.m_lblRotary2Value.Location = new System.Drawing.Point(40, 130);
			this.m_lblRotary2Value.Name = "m_lblRotary2Value";
			this.m_lblRotary2Value.Role = CommonUI.Theme.TextRole.Value;
			this.m_lblRotary2Value.Size = new System.Drawing.Size(160, 23);
			this.m_lblRotary2Value.TabIndex = 13;
			this.m_lblRotary2Value.Text = "— deg";
			this.m_lblRotary2Value.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_btnRotary2Zero
			// 
			this.m_btnRotary2Zero.Location = new System.Drawing.Point(206, 130);
			this.m_btnRotary2Zero.Name = "m_btnRotary2Zero";
			this.m_btnRotary2Zero.Size = new System.Drawing.Size(80, 23);
			this.m_btnRotary2Zero.TabIndex = 14;
			this.m_btnRotary2Zero.Text = "設為 0";
			// 
			// m_btnClear
			// 
			this.m_btnClear.Kind = CommonUI.Controls.ButtonKind.Danger;
			this.m_btnClear.Location = new System.Drawing.Point(228, 191);
			this.m_btnClear.Name = "m_btnClear";
			this.m_btnClear.Size = new System.Drawing.Size(80, 28);
			this.m_btnClear.TabIndex = 1;
			this.m_btnClear.Text = "全部清除";
			// 
			// CNCOffsetDialog
			// 
			this.ClientSize = new System.Drawing.Size(320, 231);
			this.Controls.Add(this.m_pnlOffset);
			this.Controls.Add(this.m_btnClear);
			this.Name = "CNCOffsetDialog";
			this.Text = "程式座標偏置";
			this.m_pnlOffset.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CommonUI.Controls.SectionPanel m_pnlOffset;
		private CommonUI.Controls.ThemedLabel m_lblX;
		private CommonUI.Controls.ThemedLabel m_lblXValue;
		private CommonUI.Controls.ThemedButton m_btnXZero;
		private CommonUI.Controls.ThemedLabel m_lblY;
		private CommonUI.Controls.ThemedLabel m_lblYValue;
		private CommonUI.Controls.ThemedButton m_btnYZero;
		private CommonUI.Controls.ThemedLabel m_lblZ;
		private CommonUI.Controls.ThemedLabel m_lblZValue;
		private CommonUI.Controls.ThemedButton m_btnZZero;
		private CommonUI.Controls.ThemedLabel m_lblRotary1;
		private CommonUI.Controls.ThemedLabel m_lblRotary1Value;
		private CommonUI.Controls.ThemedButton m_btnRotary1Zero;
		private CommonUI.Controls.ThemedLabel m_lblRotary2;
		private CommonUI.Controls.ThemedLabel m_lblRotary2Value;
		private CommonUI.Controls.ThemedButton m_btnRotary2Zero;
		private CommonUI.Controls.ThemedButton m_btnClear;
	}
}
