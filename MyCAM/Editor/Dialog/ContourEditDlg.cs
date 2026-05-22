using MyCAM.App;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal partial class ContourEditDlg : EditDialogBase<object>
	{
		// --- events raised to Action ---
		public Action AddEditIndex;
		public Action RemoveEditIndex;
		public Action<double, double, double> DisplacementChanged;

		public ContourEditDlg()
		{
			InitializeComponent();
		}

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
			m_tbxDX.Value = (decimal)m_Param.DX;
			m_tbxDY.Value = (decimal)m_Param.DY;
			m_tbxDZ.Value = (decimal)m_Param.DZ;
			m_btnAdd.Enabled = !m_Param.IsModified;
			m_btnRemove.Enabled = m_Param.IsModified;
			SetInputEnabled( m_Param.IsModified );
			m_bSuppressValueChanged = false;
		}

		// ---- UI events ----

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
			if( dx == m_Param.DX && dy == m_Param.DY && dz == m_Param.DZ ) {
				return;
			}
			DisplacementChanged?.Invoke( dx, dy, dz );
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

		void SetInputEnabled( bool enabled )
		{
			m_tbxDX.Enabled = enabled;
			m_tbxDY.Enabled = enabled;
			m_tbxDZ.Enabled = enabled;
		}

		bool m_bSuppressValueChanged = false;
		ContourEditParam m_Param = null;
	}

	// Data transfer object between dialog and action
	internal class ContourEditParam
	{
		public double DX
		{
			get; set;
		}
		public double DY
		{
			get; set;
		}
		public double DZ
		{
			get; set;
		}

		// true = this index already has a control point in ContourEditMap
		public bool IsModified
		{
			get; set;
		}

		public ContourEditParam( double dx, double dy, double dz, bool isModified )
		{
			DX = dx;
			DY = dy;
			DZ = dz;
			IsModified = isModified;
		}
	}
}
