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
			ApplyResourcesToUI();
		}

		public Action<ETrsfConstraintType> ConstraintMethodChanged;
		public Action<int> G54AxisPlaneSelectionChanged;

		void ApplyResourcesToUI()
		{
			this.Text = m_LanguageManager.TryGetString( "ManualTransformDlg", this.Text );
			m_lblMethod.Text = m_LanguageManager.TryGetString( "m_lblMethod", m_lblMethod.Text );
			m_btnConfirm.Text = m_LanguageManager.TryGetString( "m_btnConfirm", m_btnConfirm.Text );
		}

		public void Initialize()
		{
			ETrsfConstraintType defaultType = ETrsfConstraintType.Point;

			// Disable event handlers to prevent triggering business logic during initialization
			m_enableEventHandlers = false;

			// Set UI initial values
			m_cmbMethod.SelectedIndex = (int)defaultType;
			RefreshAxisPlaneComboBox( defaultType );
			m_lblGuide.Text = GetGuideText( defaultType );

			// Re-enable event handlers
			m_enableEventHandlers = true;

			// Trigger business logic once in a unified manner
			ConstraintMethodChanged?.Invoke( defaultType );

			G54AxisPlaneSelectionChanged?.Invoke( DEFAULT_AXIS_PLANE_INDEX );
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
				m_lblMoveStatus.Text = m_LanguageManager.TryGetString( "m_lblMoveStatus_Selected", m_lblMoveStatus.Text );
				m_lblMoveStatus.ForeColor = System.Drawing.Color.Green;
			}
			else {
				m_lblMoveStatus.Text = m_LanguageManager.TryGetString( "m_lblMoveStatus_NotSelected", m_lblMoveStatus.Text );
				m_lblMoveStatus.ForeColor = System.Drawing.Color.Gray;
			}
		}

		public void SetG54ComboBoxIndex( int index )
		{
			// Disable event handlers to avoid triggering business logic
			m_enableEventHandlers = false;
			if( index >= 0 && index < m_cmbAxisPlane.Items.Count ) {
				m_cmbAxisPlane.SelectedIndex = index;
			}
			m_enableEventHandlers = true;
		}

		public void ShowConstraintError( string message )
		{
			m_lblError.Text = m_LanguageManager.TryGetString( message, ALIGNMENT_INVALID );
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
			m_cmbMethod.Items.Add( m_LanguageManager.TryGetString( "Method_Point", METHOD_POINT ) );
			m_cmbMethod.Items.Add( m_LanguageManager.TryGetString( "Method_Axial", METHOD_AXIAL ) );
			m_cmbMethod.Items.Add( m_LanguageManager.TryGetString( "Method_AxialParallel", METHOD_AXIAL_PARALLEL ) );
			m_cmbMethod.Items.Add( m_LanguageManager.TryGetString( "Method_Plane", METHOD_PLANE ) );
			m_cmbMethod.Items.Add( m_LanguageManager.TryGetString( "Method_PlaneParallel", METHOD_PLANE_PARALLEL ) );
		}

		void RefreshAxisPlaneComboBox( ETrsfConstraintType type )
		{
			m_cmbAxisPlane.Items.Clear();

			switch( type ) {
				case ETrsfConstraintType.Point:
					m_cmbAxisPlane.Items.Add( m_LanguageManager.TryGetString( "AxisPlane_Origin", AXIS_PLANE_ORIGIN ) );
					m_lblAxisPlane.Text = m_LanguageManager.TryGetString( "m_lblAxisPlane", m_lblAxisPlane.Text );
					break;
				case ETrsfConstraintType.Axial:
				case ETrsfConstraintType.AxialParallel:
					m_cmbAxisPlane.Items.Add( m_LanguageManager.TryGetString( "AxisPlane_XAxis", AXIS_PLANE_X_AXIS ) );
					m_cmbAxisPlane.Items.Add( m_LanguageManager.TryGetString( "AxisPlane_YAxis", AXIS_PLANE_Y_AXIS ) );
					m_cmbAxisPlane.Items.Add( m_LanguageManager.TryGetString( "AxisPlane_ZAxis", AXIS_PLANE_Z_AXIS ) );
					m_lblAxisPlane.Text = m_LanguageManager.TryGetString( "m_lblAxisPlane_Axis", m_lblAxisPlane.Text );
					break;
				case ETrsfConstraintType.Plane:
				case ETrsfConstraintType.PlaneParallel:
					m_cmbAxisPlane.Items.Add( m_LanguageManager.TryGetString( "AxisPlane_XYPlane", AXIS_PLANE_XY_PLANE ) );
					m_cmbAxisPlane.Items.Add( m_LanguageManager.TryGetString( "AxisPlane_YZPlane", AXIS_PLANE_YZ_PLANE ) );
					m_cmbAxisPlane.Items.Add( m_LanguageManager.TryGetString( "AxisPlane_XZPlane", AXIS_PLANE_XZ_PLANE ) );
					m_lblAxisPlane.Text = m_LanguageManager.TryGetString( "m_lblAxisPlane_Plane", m_lblAxisPlane.Text );
					break;
				default:
					m_lblAxisPlane.Visible = false;
					m_cmbAxisPlane.Visible = false;
					return;
			}

			m_lblAxisPlane.Visible = true;
			m_cmbAxisPlane.Visible = true;

			// Disable event handlers to avoid triggering business logic
			m_enableEventHandlers = false;
			m_cmbAxisPlane.SelectedIndex = 0;
			m_enableEventHandlers = true;
		}

		void m_cmbMethod_SelectedIndexChanged( object sender, EventArgs e )
		{
			// Return early if event handlers are disabled
			if( !m_enableEventHandlers || m_cmbMethod.SelectedIndex < 0 ) {
				return;
			}

			ETrsfConstraintType type = (ETrsfConstraintType)m_cmbMethod.SelectedIndex;
			m_lblGuide.Text = GetGuideText( type );
			m_btnConfirm.Enabled = false;
			ClearConstraintError();

			// Notify Action to switch constraint mode
			ConstraintMethodChanged?.Invoke( type );

			// Update AxisPlane ComboBox (event handlers disabled internally)
			RefreshAxisPlaneComboBox( type );

			// Notify Action to set default G54 reference
			G54AxisPlaneSelectionChanged?.Invoke( DEFAULT_AXIS_PLANE_INDEX );
		}

		void m_cmbAxisPlane_SelectedIndexChanged( object sender, EventArgs e )
		{
			// Return early if event handlers are disabled
			if( !m_enableEventHandlers || m_cmbAxisPlane.SelectedIndex < 0 || m_cmbMethod.SelectedIndex < 0 ) {
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
					return m_LanguageManager.TryGetString( "Guide_Point", GUIDE_POINT );
				case ETrsfConstraintType.Axial:
					return m_LanguageManager.TryGetString( "Guide_Axial", GUIDE_AXIAL );
				case ETrsfConstraintType.AxialParallel:
					return m_LanguageManager.TryGetString( "Guide_AxialParallel", GUIDE_AXIAL_PARALLEL );
				case ETrsfConstraintType.Plane:
					return m_LanguageManager.TryGetString( "Guide_Plane", GUIDE_PLANE );
				case ETrsfConstraintType.PlaneParallel:
					return m_LanguageManager.TryGetString( "Guide_PlaneParallel", GUIDE_PLANE_PARALLEL );
				default:
					return m_LanguageManager.TryGetString( "m_lblGuide_Default", GUIDE_DEFAULT );
			}
		}

		const int DEFAULT_AXIS_PLANE_INDEX = 0;
		const string GUIDE_POINT = "請先從觸控選一個工件面點，作為參考點位置";
		const string GUIDE_AXIAL = "請先從觸控選一條工件邊，作為參考方向軸";
		const string GUIDE_AXIAL_PARALLEL = "請先從觸控選一條工件邊，作為參考方向軸（平行）";
		const string GUIDE_PLANE = "請先從觸控選一個工件面，作為參考面";
		const string GUIDE_PLANE_PARALLEL = "請先從觸控選一個工件面，作為參考面（平行）";
		const string GUIDE_DEFAULT = "請選擇輸入方式";
		const string METHOD_POINT = "點對齊";
		const string METHOD_AXIAL = "軸向對齊";
		const string METHOD_AXIAL_PARALLEL = "軸向平行";
		const string METHOD_PLANE = "平面對齊";
		const string METHOD_PLANE_PARALLEL = "平面平行";
		const string AXIS_PLANE_ORIGIN = "原點";
		const string AXIS_PLANE_X_AXIS = "X 軸";
		const string AXIS_PLANE_Y_AXIS = "Y 軸";
		const string AXIS_PLANE_Z_AXIS = "Z 軸";
		const string AXIS_PLANE_XY_PLANE = "XY 平面";
		const string AXIS_PLANE_YZ_PLANE = "YZ 平面";
		const string AXIS_PLANE_XZ_PLANE = "XZ 平面";
		const string ALIGNMENT_INVALID = "對齊無效，選取的物件並非平面、直線、或正圓弧，請重新選擇";

		// Event handlers are enabled by default
		bool m_enableEventHandlers = true;
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ManualTransformDlg" );
	}
}
