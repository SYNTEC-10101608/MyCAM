using System;

namespace MyCAM.Editor
{
	internal partial class ContourEditDlg : EditDialogBase<object>
	{
		// --- events raised to Action ---
		public Action AddEditIndex;
		public Action RemoveEditIndex;
		public Action<double, double, double, RefCoordinateType> DisplacementChanged;
		public Action ResetDisplacement;

		public ContourEditDlg()
		{
			InitializeComponent();
			m_LanguageManager = new LanguageManager.LanguageManager( "ContourEditDlg" );
			ApplyResourcesToUI();
		}

		void ApplyResourcesToUI()
		{
			this.Text = m_LanguageManager.TryGetString( "ContourEditDlg", this.Text );
			m_gbxIndexParam.Text = m_LanguageManager.TryGetString( "m_gbxIndexParam", m_gbxIndexParam.Text );
			m_lblDX.Text = m_LanguageManager.TryGetString( "m_lblDX", m_lblDX.Text );
			m_lblDY.Text = m_LanguageManager.TryGetString( "m_lblDY", m_lblDY.Text );
			m_lblDZ.Text = m_LanguageManager.TryGetString( "m_lblDZ", m_lblDZ.Text );
			m_lblUnit.Text = m_LanguageManager.TryGetString( "m_lblUnit", m_lblUnit.Text );
			m_btnReset.Text = m_LanguageManager.TryGetString( "m_btnReset", m_btnReset.Text );
			m_btnClear.Text = m_LanguageManager.TryGetString( "m_btnClear", m_btnClear.Text );
			m_btnAdd.Text = m_LanguageManager.TryGetString( "m_btnAdd", m_btnAdd.Text );
			m_btnRemove.Text = m_LanguageManager.TryGetString( "m_btnRemove", m_btnRemove.Text );

			m_lblRefCoordType.Text = m_LanguageManager.TryGetString( "m_lblRefCoordType", m_lblRefCoordType.Text );
			m_cmbRefCoordType.Items.Add( m_LanguageManager.TryGetString( "RefCoordType_World", REF_COORD_TYPE_WORLD ) );
			m_cmbRefCoordType.Items.Add( m_LanguageManager.TryGetString( "RefCoordType_Local", REF_COORD_TYPE_LOCAL ) );
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
			m_tbxDX.Value = 0;
			m_tbxDY.Value = 0;
			m_tbxDZ.Value = 0;
			m_cmbRefCoordType.SelectedIndex = (int)m_Param.RefCoordinateType;

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
			DisplacementChanged?.Invoke( dx, dy, dz, m_Param.RefCoordinateType );
		}

		void m_btnAdd_Click( object sender, EventArgs e )
		{
			AddEditIndex?.Invoke();
		}

		void m_btnRemove_Click( object sender, EventArgs e )
		{
			RemoveEditIndex?.Invoke();
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

		void m_cmbRefCoordType_SelectedIndexChanged( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged ) {
				return;
			}

			RefCoordinateType refCoordType = (RefCoordinateType)m_cmbRefCoordType.SelectedIndex;
			if( refCoordType == m_Param.RefCoordinateType ) {
				return;
			}

			GetDisplacementFromDialog( out double dx, out double dy, out double dz );
			DisplacementChanged?.Invoke( dx, dy, dz, refCoordType );
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
		}

		bool m_bSuppressValueChanged = false;
		ContourEditParam m_Param = null;

		readonly LanguageManager.LanguageManager m_LanguageManager;

		const string REF_COORD_TYPE_LOCAL = "區域座標系";
		const string REF_COORD_TYPE_WORLD = "世界坐標系";
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

		public ContourEditParam( RefCoordinateType refCoordinateType, bool isModified )
		{
			RefCoordinateType = refCoordinateType;
			IsModified = isModified;
		}
	}
}
