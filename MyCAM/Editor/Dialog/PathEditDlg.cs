using System;

namespace MyCAM.Editor.Dialog
{
	public partial class PathEditDlg : EditDialogBase<PathEditData>
	{
		public PathEditDlg()
		{
			InitializeComponent();
			m_LanguageManager = new LanguageManager.LanguageManager( "PathEditDlg" );
			ApplyResourcesToUI();
		}

		void ApplyResourcesToUI()
		{
			this.Text = m_LanguageManager.TryGetString( "PathEditDlg", this.Text );
			m_lblRefCoordType.Text = m_LanguageManager.TryGetString( "m_lblRefCoordType", m_lblRefCoordType.Text );
			m_lblX.Text = m_LanguageManager.TryGetString( "m_lblX", m_lblX.Text );
			m_lblY.Text = m_LanguageManager.TryGetString( "m_lblY", m_lblY.Text );
			m_lblZ.Text = m_LanguageManager.TryGetString( "m_lblZ", m_lblZ.Text );
			m_btnConfirm.Text = m_LanguageManager.TryGetString( "m_btnConfirm", m_btnConfirm.Text );
			m_btnClear.Text = m_LanguageManager.TryGetString( "m_btnClear", m_btnClear.Text );
			m_btnReset.Text = m_LanguageManager.TryGetString( "m_btnReset", m_btnReset.Text );

			// Update combo box items
			m_cmbRefCoordType.Items.Clear();

			m_cmbRefCoordType.Items.Add( m_LanguageManager.TryGetString( "RefCoordType_Local", REF_COORD_TYPE_LOCAL ) );
			m_cmbRefCoordType.Items.Add( m_LanguageManager.TryGetString( "RefCoordType_World", REF_COORD_TYPE_WORLD ) );
			if( m_cmbRefCoordType.SelectedIndex < 0 )
				m_cmbRefCoordType.SelectedIndex = 0;
		}

		void Initialization()
		{
			// initialize reference coordinate type combo box
			m_cmbRefCoordType.SelectedIndex = 0;

			// initialize numeric up down controls
			m_NumUpDownX.Maximum = decimal.MaxValue;
			m_NumUpDownX.Minimum = decimal.MinValue;
			m_NumUpDownX.DecimalPlaces = 3;
			m_NumUpDownX.Increment = 0.25m;
			m_NumUpDownY.Maximum = decimal.MaxValue;
			m_NumUpDownY.Minimum = decimal.MinValue;
			m_NumUpDownY.DecimalPlaces = 3;
			m_NumUpDownY.Increment = 0.25m;
			m_NumUpDownZ.Maximum = decimal.MaxValue;
			m_NumUpDownZ.Minimum = decimal.MinValue;
			m_NumUpDownZ.DecimalPlaces = 3;
			m_NumUpDownZ.Increment = 0.25m;

			// Trigger initial preview to display coordinate system
			GetValuesOfControls();
			m_PathEditData = new PathEditData( m_RefCoordinateType, m_XOffset, m_YOffset, m_ZOffset );
			RaisePreview( m_PathEditData );
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

		void m_NumUpDownX_DebouncedValueChanged( object sender, EventArgs e )
		{
			if( m_XOffset == (double)m_NumUpDownX.Value ) {
				return;
			}
			GetValuesOfControls();
			m_PathEditData = new PathEditData( m_RefCoordinateType, m_XOffset, m_YOffset, m_ZOffset );
			RaisePreview( m_PathEditData );
		}

		void m_NumUpDownY_DebouncedValueChanged( object sender, EventArgs e )
		{
			if( m_YOffset == (double)m_NumUpDownY.Value ) {
				return;
			}
			GetValuesOfControls();
			m_PathEditData = new PathEditData( m_RefCoordinateType, m_XOffset, m_YOffset, m_ZOffset );
			RaisePreview( m_PathEditData );
		}

		void m_NumUpDownZ_DebouncedValueChanged( object sender, EventArgs e )
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
		readonly LanguageManager.LanguageManager m_LanguageManager;

		const string REF_COORD_TYPE_LOCAL = "區域座標系";
		const string REF_COORD_TYPE_WORLD = "世界座標系";
	}
}
