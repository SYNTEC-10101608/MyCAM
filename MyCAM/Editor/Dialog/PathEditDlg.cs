using System;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
{
	public partial class PathEditDlg : EditDialogBase<PathEditData>
	{
		public PathEditDlg()
		{
			InitializeComponent();
		}

		void Initialization()
		{
			// initialize reference coordinate type combo box
			m_cmbRefCoordType.SelectedIndex = 0;

			// initialize numeric up down controls
			m_NumUpDownX.Maximum = decimal.MaxValue;
			m_NumUpDownX.Minimum = decimal.MinValue;
			m_NumUpDownY.Maximum = decimal.MaxValue;
			m_NumUpDownY.Minimum = decimal.MinValue;
			m_NumUpDownZ.Maximum = decimal.MaxValue;
			m_NumUpDownZ.Minimum = decimal.MinValue;
		}

		void PathEditDlg_Shown( object sender, EventArgs e )
		{
			Initialization();
		}

		void m_cmbRefCoordType_SelectedIndexChanged( object sender, EventArgs e )
		{
			switch( m_cmbRefCoordType.SelectedIndex ) {
				case 0:
					m_RefCoordinateType = RefCoordinateType.Local;
					break;
				case 1:
					m_RefCoordinateType = RefCoordinateType.World;
					break;
			}
			GetValuesOfControls();
			m_PathEditData = new PathEditData( m_RefCoordinateType, m_XOffset, m_YOffset, m_ZOffset );
			RaisePreview( m_PathEditData );
		}

		void m_NumUpDownX_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				SetXOffsetValue();
			}
		}

		void m_NumUpDownX_Leave( object sender, EventArgs e )
		{
			SetXOffsetValue();
		}

		void m_NumUpDownX_Click( object sender, EventArgs e )
		{
			SetXOffsetValue();
		}

		void SetXOffsetValue()
		{
			if( m_XOffset == (double)m_NumUpDownX.Value ) {
				return;
			}
			GetValuesOfControls();
			m_PathEditData = new PathEditData( m_RefCoordinateType, m_XOffset, m_YOffset, m_ZOffset );
			RaisePreview( m_PathEditData );
		}

		void m_NumUpDownY_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				SetYOffsetValue();
			}
		}

		void m_NumUpDownY_Leave( object sender, EventArgs e )
		{
			SetYOffsetValue();
		}

		void m_NumUpDownY_Click( object sender, EventArgs e )
		{
			SetYOffsetValue();
		}

		void SetYOffsetValue()
		{
			if( m_YOffset == (double)m_NumUpDownY.Value ) {
				return;
			}
			GetValuesOfControls();
			m_PathEditData = new PathEditData( m_RefCoordinateType, m_XOffset, m_YOffset, m_ZOffset );
			RaisePreview( m_PathEditData );
		}

		void m_NumUpDownZ_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				SetZOffsetValue();
			}
		}

		void m_NumUpDownZ_Leave( object sender, EventArgs e )
		{
			SetZOffsetValue();
		}

		void m_NumUpDownZ_Click( object sender, EventArgs e )
		{
			SetZOffsetValue();
		}

		void SetZOffsetValue()
		{
			if( m_ZOffset == (double)m_NumUpDownZ.Value ) {
				return;
			}
			GetValuesOfControls();
			m_PathEditData = new PathEditData( m_RefCoordinateType, m_XOffset, m_YOffset, m_ZOffset );
			RaisePreview( m_PathEditData );
		}

		void m_btnClear_Click( object sender, EventArgs e )
		{
			ClearValues();
			m_PathEditData = new PathEditData( m_RefCoordinateType, m_XOffset, m_YOffset, m_ZOffset );
			RaisePreview( m_PathEditData );
		}

		void m_btnReset_Click( object sender, EventArgs e )
		{
			ClearValues();
			m_PathEditData = new PathEditData( m_RefCoordinateType, m_XOffset, m_YOffset, m_ZOffset );
			RaiseReset( m_PathEditData );
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			GetValuesOfControls();
			m_PathEditData = new PathEditData( m_RefCoordinateType, m_XOffset, m_YOffset, m_ZOffset );
			RaiseConfirm( m_PathEditData );
		}

		void GetValuesOfControls()
		{
			m_XOffset = (double)m_NumUpDownX.Value;
			m_YOffset = (double)m_NumUpDownY.Value;
			m_ZOffset = (double)m_NumUpDownZ.Value;
		}

		void ClearValues()
		{
			m_XOffset = 0;
			m_YOffset = 0;
			m_ZOffset = 0;
			m_NumUpDownX.Value = 0;
			m_NumUpDownY.Value = 0;
			m_NumUpDownZ.Value = 0;
		}

		RefCoordinateType m_RefCoordinateType;
		double m_XOffset = 0;
		double m_YOffset = 0;
		double m_ZOffset = 0;
		PathEditData m_PathEditData = new PathEditData();
	}
}
