using System;

namespace MyCAM.Editor.Dialog
{
	public partial class PathEditDlg : EditDialogBase<PathEditData>
	{
		public PathEditDlg()
		{
			InitializeComponent();
			m_LanguageManager = new LanguageManager.LanguageManager( "PathEditDlg" );
			InitRefCoordTypeItems();
		}

		// the items are localized at runtime
		void InitRefCoordTypeItems()
		{
			m_cmbRefCoordType.Items.Clear();
			m_cmbRefCoordType.Items.Add( m_LanguageManager.TryGetString( "RefCoordType_Local", REF_COORD_TYPE_LOCAL ) );
			m_cmbRefCoordType.Items.Add( m_LanguageManager.TryGetString( "RefCoordType_World", REF_COORD_TYPE_WORLD ) );
			if( m_cmbRefCoordType.SelectedIndex < 0 ) {
				m_cmbRefCoordType.SelectedIndex = 0;
			}
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

		public void SetTotaloffset( double? x, double? y, double? z )
		{
			m_lblTotalDX.Text = x.HasValue ? x.Value.ToString( "F3" ) : m_LanguageManager.TryGetString( "IsMixData", IS_MIX_DATA );
			m_lblTotalDY.Text = y.HasValue ? y.Value.ToString( "F3" ) : m_LanguageManager.TryGetString( "IsMixData", IS_MIX_DATA );
			m_lblTotalDZ.Text = z.HasValue ? z.Value.ToString( "F3" ) : m_LanguageManager.TryGetString( "IsMixData", IS_MIX_DATA );
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
		const string IS_MIX_DATA = "偏移不一致";
	}
}
