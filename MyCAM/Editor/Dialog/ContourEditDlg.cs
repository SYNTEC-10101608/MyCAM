using MyCAM.Data;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal partial class ContourEditDlg : EditDialogBase<object>
	{
		// --- events raised to Action ---
		public Action AddEditIndex;
		public Action RemoveEditIndex;
		public Action<double, double, double, RefCoordinateType> DisplacementChanged;
		public Action ResetDisplacement;
		public Action<bool> ToStartOrEnd;

		public ContourEditDlg()
		{
			InitializeComponent();
			ApplyResourcesToUI();

			m_rbtnWorldCoord.Tag = RefCoordinateType.World;
			m_rbtnLocalCoord.Tag = RefCoordinateType.Local;
		}

		void ApplyResourcesToUI()
		{
			this.Text = m_LanguageManager.TryGetString( "ContourEditDlg", this.Text );
			m_gbxIndexParam.Text = m_LanguageManager.TryGetString( "m_gbxIndexParam", m_gbxIndexParam.Text );

			m_lblRefCoordType.Text = m_LanguageManager.TryGetString( "m_lblRefCoordType", m_lblRefCoordType.Text );
			m_rbtnWorldCoord.Text = m_LanguageManager.TryGetString( "m_rbtnWorldCoord", m_rbtnWorldCoord.Text );
			m_rbtnLocalCoord.Text = m_LanguageManager.TryGetString( "m_rbtnLocalCoord", m_rbtnLocalCoord.Text );

			m_lblToStartOrEnd.Text = m_LanguageManager.TryGetString( "m_lblToStartOrEnd", m_lblToStartOrEnd.Text );
			m_rbtnToStart.Text = m_LanguageManager.TryGetString( "m_rbtnToStart", m_rbtnToStart.Text );
			m_rbtnToEnd.Text = m_LanguageManager.TryGetString( "m_rbtnToEnd", m_rbtnToEnd.Text );

			m_lblDX.Text = m_LanguageManager.TryGetString( "m_lblDX", m_lblDX.Text );
			m_lblDY.Text = m_LanguageManager.TryGetString( "m_lblDY", m_lblDY.Text );
			m_lblDZ.Text = m_LanguageManager.TryGetString( "m_lblDZ", m_lblDZ.Text );
			m_lblUnit.Text = m_LanguageManager.TryGetString( "m_lblUnit", m_lblUnit.Text );

			m_btnReset.Text = m_LanguageManager.TryGetString( "m_btnReset", m_btnReset.Text );
			m_btnClear.Text = m_LanguageManager.TryGetString( "m_btnClear", m_btnClear.Text );
			m_btnAdd.Text = m_LanguageManager.TryGetString( "m_btnAdd", m_btnAdd.Text );
			m_btnRemove.Text = m_LanguageManager.TryGetString( "m_btnRemove", m_btnRemove.Text );
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

			m_rbtnWorldCoord.Checked = ( m_Param.RefCoordinateType == RefCoordinateType.World );
			m_rbtnLocalCoord.Checked = ( m_Param.RefCoordinateType == RefCoordinateType.Local );

			m_tbxDX.Value = 0;
			m_tbxDY.Value = 0;
			m_tbxDZ.Value = 0;

			SetInputEnabled( m_Param.IsModified );

			m_rbtnToStart.Checked = m_Param.IsPathStartPoint == true;
			m_rbtnToEnd.Checked = m_Param.IsPathStartPoint == false;

			if( m_Param.IsPathStartPoint != null ) {
				m_btnAdd.Enabled = false;
				m_btnRemove.Enabled = false;
			}

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

			m_btnReset.Enabled = enabled;
			m_btnClear.Enabled = enabled;

			m_btnAdd.Enabled = !enabled;
			m_btnRemove.Enabled = enabled;
		}

		bool m_bSuppressValueChanged = false;
		ContourEditParam m_Param = null;

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ContourEditDlg" );
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

		public ContourEditParam( RefCoordinateType refCoordinateType, bool isModified, bool? isPathStartPoint )
		{
			RefCoordinateType = refCoordinateType;
			IsModified = isModified;
			IsPathStartPoint = isPathStartPoint;
		}
	}
}
