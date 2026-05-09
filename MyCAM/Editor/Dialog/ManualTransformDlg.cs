using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal partial class ManualTransformDlg : EditDialogBase<ETrsfConstraintType>
	{
		public ManualTransformDlg()
		{
			InitializeComponent();
			InitMethodComboBox();

			// set default selection to Point and trigger the mode change event
			m_cmbMethod.SelectedIndex = 0;
		}

		public void UpdateSelectionStatusAndEnableConfirm( bool isWorkpieceSelected )
		{
			if( m_cmbMethod.SelectedIndex < 0 ) {
				return;
			}

			// update confirm button status
			m_btnConfirm.Enabled = isWorkpieceSelected;

			// update workpiece hint text
			if( isWorkpieceSelected ) {
				m_lblMoveStatus.Text = "✔ 工件";
				m_lblMoveStatus.ForeColor = System.Drawing.Color.Green;
			}
			else {
				m_lblMoveStatus.Text = "✘ 工件";
				m_lblMoveStatus.ForeColor = System.Drawing.Color.Gray;
			}
		}

		public void SetG54ComboBoxIndex( int index )
		{
			m_cmbAxisPlane.SelectedIndexChanged -= m_cmbAxisPlane_SelectedIndexChanged;
			if( index >= 0 && index < m_cmbAxisPlane.Items.Count ) {
				m_cmbAxisPlane.SelectedIndex = index;
			}
			m_cmbAxisPlane.SelectedIndexChanged += m_cmbAxisPlane_SelectedIndexChanged;
		}

		public void ShowConstraintError( string message )
		{
			m_lblError.Text = message;
			m_lblError.Visible = true;
		}

		public void ClearConstraintError()
		{
			m_lblError.Text = string.Empty;
			m_lblError.Visible = false;
		}

		public void MarkConfirmSuccess()
		{
			m_ConfirmCheck = true;
		}

		public Action<ETrsfConstraintType> ConstraintMethodChanged;
		public Action<int> G54AxisPlaneSelectionChanged;

		protected override void RaiseConfirm( ETrsfConstraintType data )
		{
			Confirm?.Invoke( data );
		}

		protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
		{
			if( keyData == Keys.Escape ) {
				Close();
				return true;
			}
			return base.ProcessCmdKey( ref msg, keyData );
		}

		void InitMethodComboBox()
		{
			m_cmbMethod.Items.Add( "點對齊" );          // Point
			m_cmbMethod.Items.Add( "軸向對齊" );        // Axial
			m_cmbMethod.Items.Add( "軸向平行" );        // AxialParallel
			m_cmbMethod.Items.Add( "平面對齊" );        // Plane
			m_cmbMethod.Items.Add( "平面平行" );        // PlaneParallel
		}

		void RefreshAxisPlaneComboBox( ETrsfConstraintType type )
		{
			m_cmbAxisPlane.SelectedIndexChanged -= m_cmbAxisPlane_SelectedIndexChanged;
			m_cmbAxisPlane.Items.Clear();

			switch( type ) {
				case ETrsfConstraintType.Point:
					m_cmbAxisPlane.Items.Add( "原點" );
					m_lblAxisPlane.Text = "對齊點";
					m_lblAxisPlane.Visible = true;
					m_cmbAxisPlane.Visible = true;
					break;
				case ETrsfConstraintType.Axial:
				case ETrsfConstraintType.AxialParallel:
					m_cmbAxisPlane.Items.Add( "X 軸" );
					m_cmbAxisPlane.Items.Add( "Y 軸" );
					m_cmbAxisPlane.Items.Add( "Z 軸" );
					m_lblAxisPlane.Text = "對齊軸";
					m_lblAxisPlane.Visible = true;
					m_cmbAxisPlane.Visible = true;
					break;
				case ETrsfConstraintType.Plane:
				case ETrsfConstraintType.PlaneParallel:
					m_cmbAxisPlane.Items.Add( "XY 平面" );
					m_cmbAxisPlane.Items.Add( "YZ 平面" );
					m_cmbAxisPlane.Items.Add( "XZ 平面" );
					m_lblAxisPlane.Text = "對齊面";
					m_lblAxisPlane.Visible = true;
					m_cmbAxisPlane.Visible = true;
					break;
				default:
					m_lblAxisPlane.Visible = false;
					m_cmbAxisPlane.Visible = false;
					m_cmbAxisPlane.SelectedIndexChanged += m_cmbAxisPlane_SelectedIndexChanged;
					return;
			}

			m_cmbAxisPlane.SelectedIndexChanged += m_cmbAxisPlane_SelectedIndexChanged;
			m_cmbAxisPlane.SelectedIndex = 0;
		}

		void m_cmbMethod_SelectedIndexChanged( object sender, EventArgs e )
		{
			if( m_cmbMethod.SelectedIndex < 0 ) {
				return;
			}
			ETrsfConstraintType type = (ETrsfConstraintType)m_cmbMethod.SelectedIndex;
			m_lblGuide.Text = GetGuideText( type );
			m_btnConfirm.Enabled = false;
			ClearConstraintError();
			ConstraintMethodChanged?.Invoke( type );
			RefreshAxisPlaneComboBox( type );
		}

		void m_cmbAxisPlane_SelectedIndexChanged( object sender, EventArgs e )
		{
			if( m_cmbAxisPlane.SelectedIndex < 0 || m_cmbMethod.SelectedIndex < 0 ) {
				return;
			}
			G54AxisPlaneSelectionChanged?.Invoke( m_cmbAxisPlane.SelectedIndex );
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			if( m_cmbMethod.SelectedIndex < 0 ) {
				return;
			}
			ETrsfConstraintType type = (ETrsfConstraintType)m_cmbMethod.SelectedIndex;
			RaiseConfirm( type );
		}

		string GetGuideText( ETrsfConstraintType type )
		{
			switch( type ) {
				case ETrsfConstraintType.Point:
					return "請在畫布選擇一個工件點，與原點對齊";
				case ETrsfConstraintType.Axial:
					return "請在畫布選擇一條工件邊，與座標系軸向對齊";
				case ETrsfConstraintType.AxialParallel:
					return "請在畫布選擇一條工件邊，與座標系軸向平行";
				case ETrsfConstraintType.Plane:
					return "請在畫布選擇一個工件面，與座標系軸面對齊";
				case ETrsfConstraintType.PlaneParallel:
					return "請在畫布選擇一個工件面，與座標系軸面平行";
				default:
					return "請先選擇對齊方法";
			}
		}
	}
}
