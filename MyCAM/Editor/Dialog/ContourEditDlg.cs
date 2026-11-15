using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal partial class ContourEditDlg : EditDialogBase<object>
	{
		// --- events raised to Action ---
		public Action AddEditIndex;
		public Action RemoveEditIndex;
		public Action SaveEditIndex;
		public Action<double, double, double, RefCoordinateType> DisplacementChanged;
		public Action ResetDisplacement;
		public Action<bool> ToStartOrEnd;

		public ContourEditDlg()
		{
			InitializeComponent();

			m_rbtnWorldCoord.Tag = RefCoordinateType.World;
			m_rbtnLocalCoord.Tag = RefCoordinateType.Local;
		}

		#region Update UI

		// Refresh the dialog UI to reflect the given param.
		// Pass null to indicate no point is selected.
		public void ResetParam( ContourEditParam param )
		{
			m_bSuppressValueChanged = true;
			m_Param = param;

			// should not happen, but just in case
			if( m_Param == null ) {
				m_gbxIndexParam.Enabled = false;
				m_tbxDX.Value = 0;
				m_tbxDY.Value = 0;
				m_tbxDZ.Value = 0;
				m_bSuppressValueChanged = false;
				return;
			}

			m_gbxIndexParam.Enabled = true;

			m_rbtnWorldCoord.Checked = ( m_Param.RefCoordinateType == RefCoordinateType.World );
			m_rbtnLocalCoord.Checked = ( m_Param.RefCoordinateType == RefCoordinateType.Local );

			m_tbxDX.Value = 0;
			m_tbxDY.Value = 0;
			m_tbxDZ.Value = 0;

			SetInputEnabled( m_Param.IsModified );

			m_rbtnToStart.Checked = m_Param.IsPathStartPoint == true;
			m_rbtnToEnd.Checked = m_Param.IsPathStartPoint == false;

			UpdateTotalDisplacement();
			m_bSuppressValueChanged = false;
		}

		public void UpdateTotalDisplacement()
		{
			m_lblTotalDX.Text = ( m_Param?.TotalDX ?? 0 ).ToString( "F3" );
			m_lblTotalDY.Text = ( m_Param?.TotalDY ?? 0 ).ToString( "F3" );
			m_lblTotalDZ.Text = ( m_Param?.TotalDZ ?? 0 ).ToString( "F3" );
		}

		void SetInputEnabled( bool enabled )
		{
			m_tbxDX.Enabled = enabled;
			m_tbxDY.Enabled = enabled;
			m_tbxDZ.Enabled = enabled;

			m_btnReset.Enabled = enabled;
			m_btnClear.Enabled = enabled;

			m_btnSave.Enabled = enabled;
			m_btnAdd.Enabled = !enabled;
			m_btnRemove.Enabled = enabled;
		}

		#endregion

		#region UI Events

		void m_tbxDX_DebouncedValueChanged( object sender, EventArgs e )
		{
			HandleDisplacementChanged();
		}

		void m_tbxDY_DebouncedValueChanged( object sender, EventArgs e )
		{
			HandleDisplacementChanged();
		}

		void m_tbxDZ_DebouncedValueChanged( object sender, EventArgs e )
		{
			HandleDisplacementChanged();
		}

		void HandleDisplacementChanged()
		{
			if( m_bSuppressValueChanged ) {
				return;
			}
			if( m_Param == null || !m_Param.IsModified ) {
				return;
			}
			if( !GetDisplacementFromDialog( out double dx, out double dy, out double dz ) ) {
				return;
			}
			DisplacementChanged?.Invoke( dx, dy, dz, m_Param.RefCoordinateType );
		}

		void RefCoord_CheckedChanged( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			if( sender is RadioButton rbtn && rbtn.Checked ) {
				GetDisplacementFromDialog( out double dx, out double dy, out double dz );
				DisplacementChanged?.Invoke( dx, dy, dz, (RefCoordinateType)rbtn.Tag );
			}
		}

		void ToStartOrEnd_CheckedChanged( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			if( sender is RadioButton rbtn && rbtn.Checked ) {
				ToStartOrEnd?.Invoke( rbtn == m_rbtnToStart );
			}
		}

		void m_btnReset_Click( object sender, EventArgs e )
		{
			m_tbxDX.Value = 0;
			m_tbxDY.Value = 0;
			m_tbxDZ.Value = 0;
			ResetDisplacement?.Invoke();
		}

		void m_btnClear_Click( object sender, EventArgs e )
		{
			m_tbxDX.Value = 0;
			m_tbxDY.Value = 0;
			m_tbxDZ.Value = 0;
			DisplacementChanged?.Invoke( 0, 0, 0, m_Param.RefCoordinateType );
		}

		void m_btnSave_Click( object sender, EventArgs e )
		{
			SaveEditIndex?.Invoke();
		}

		void m_btnAdd_Click( object sender, EventArgs e )
		{
			AddEditIndex?.Invoke();
		}

		void m_btnRemove_Click( object sender, EventArgs e )
		{
			RemoveEditIndex?.Invoke();
		}

		bool GetDisplacementFromDialog( out double dx, out double dy, out double dz )
		{
			dx = (double)m_tbxDX.Value;
			dy = (double)m_tbxDY.Value;
			dz = (double)m_tbxDZ.Value;
			return true;
		}

		#endregion

		bool m_bSuppressValueChanged = false;
		ContourEditParam m_Param = null;

	}

	internal class ContourEditParam
	{
		public RefCoordinateType RefCoordinateType
		{
			get; set;
		}

		// true = this index already has a control point in ContourEditMap
		public bool IsModified
		{
			get; set;
		}

		// true = start point, false = end point, null = others
		public bool? IsPathStartPoint
		{
			get; set;
		}

		public double TotalDX
		{
			get; set;
		}

		public double TotalDY
		{
			get; set;
		}

		public double TotalDZ
		{
			get; set;
		}

		public ContourEditParam( RefCoordinateType refCoordinateType, bool isModified, bool? isPathStartPoint,
			double totalDX, double totalDY, double totalDZ )
		{
			RefCoordinateType = refCoordinateType;
			IsModified = isModified;
			IsPathStartPoint = isPathStartPoint;
			TotalDX = totalDX;
			TotalDY = totalDY;
			TotalDZ = totalDZ;
		}
	}
}
