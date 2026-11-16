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

			m_rdoWorldCoord.Tag = RefCoordinateType.World;
			m_rdoLocalCoord.Tag = RefCoordinateType.Local;
		}

		// Refresh the dialog UI to reflect the given param.
		// Pass null to indicate no point is selected.
		public void ResetParam( ContourEditParam param )
		{
			m_bSuppressValueChanged = true;
			m_Param = param;

			// should not happen, but just in case
			if( m_Param == null ) {
				m_pnlIndexParam.Enabled = false;
				m_numDX.Value = 0;
				m_numDY.Value = 0;
				m_numDZ.Value = 0;
				m_lblRelativeCurrentPath.Visible = false;
				m_bSuppressValueChanged = false;
				return;
			}

			m_pnlIndexParam.Enabled = true;

			m_rdoWorldCoord.Checked = ( m_Param.RefCoordinateType == RefCoordinateType.World );
			m_rdoLocalCoord.Checked = ( m_Param.RefCoordinateType == RefCoordinateType.Local );

			m_numDX.Value = 0;
			m_numDY.Value = 0;
			m_numDZ.Value = 0;

			SetInputEnabled( m_Param.IsModified );

			m_rdoToStart.Checked = m_Param.IsPathStartPoint == true;
			m_rdoToEnd.Checked = m_Param.IsPathStartPoint == false;

			m_lblRelativeCurrentPath.Visible = m_Param.IsPathStartPoint.HasValue;

			if( m_Param.IsPathStartPoint != null ) {
				m_btnAdd.Enabled = false;
				m_btnRemove.Enabled = false;
			}

			UpdateTotalDisplacement();
			m_bSuppressValueChanged = false;
		}

		void SetInputEnabled( bool enabled )
		{
			m_numDX.Enabled = enabled;
			m_numDY.Enabled = enabled;
			m_numDZ.Enabled = enabled;

			m_btnReset.Enabled = enabled;
			m_btnClear.Enabled = enabled;

			m_btnSave.Enabled = enabled;
			m_btnAdd.Enabled = !enabled;
			m_btnRemove.Enabled = enabled;
		}

		public void UpdateTotalDisplacement()
		{
			m_lblTotalDX.Text = ( m_Param?.TotalDX ?? 0 ).ToString( "F3" );
			m_lblTotalDY.Text = ( m_Param?.TotalDY ?? 0 ).ToString( "F3" );
			m_lblTotalDZ.Text = ( m_Param?.TotalDZ ?? 0 ).ToString( "F3" );
		}

		#region UI Events

		void m_numDX_DebouncedValueChanged( object sender, EventArgs e )
		{
			HandleDisplacementChanged();
		}

		void m_numDY_DebouncedValueChanged( object sender, EventArgs e )
		{
			HandleDisplacementChanged();
		}

		void m_numDZ_DebouncedValueChanged( object sender, EventArgs e )
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
				ToStartOrEnd?.Invoke( rbtn == m_rdoToStart );
			}
		}

		void m_btnReset_Click( object sender, EventArgs e )
		{
			m_numDX.Value = 0;
			m_numDY.Value = 0;
			m_numDZ.Value = 0;
			ResetDisplacement?.Invoke();
		}

		void m_btnClear_Click( object sender, EventArgs e )
		{
			m_numDX.Value = 0;
			m_numDY.Value = 0;
			m_numDZ.Value = 0;
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
			dx = (double)m_numDX.Value;
			dy = (double)m_numDY.Value;
			dz = (double)m_numDZ.Value;
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
