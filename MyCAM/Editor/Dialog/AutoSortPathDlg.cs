namespace MyCAM.Editor.Dialog
{
	public enum SortMethod
	{
		ShortestPath,
		CylinderVertical,
		CylinderHorizontal
	}

	public enum CylinderAxisType
	{
		X,
		Y,
		Z
	}

	public class SortParams
	{
		public SortMethod Method;
		public CylinderAxisType CylinderAxis = CylinderAxisType.Z;
		public bool IsExtrusionDescending;
		public bool IsRotationDescending;

		// group bandwidth settings
		public double ExtrusionBandwidth_mm = DEFAULT_EXTRUSION_BANDWIDTH_MM;
		public double RotationBandwidth_deg = DEFAULT_ROTATION_BANDWIDTH_DEG;

		public const double DEFAULT_EXTRUSION_BANDWIDTH_MM = 5.0;
		public const double DEFAULT_ROTATION_BANDWIDTH_DEG = 3.0;
	}

	public partial class AutoSortPathDlg : EditDialogBase<SortParams>
	{
		public AutoSortPathDlg()
		{
			InitializeComponent();
			InitializeBandwidthControls();
			LoadImages();
			ApplyResourcesToUI();
			UpdateSortMethodState();
		}

		public SortMethod CurrentSortMethod => GetSelectedSortMethod();

		public void UpdateSelectedPathStatus( string pathID )
		{
			m_SelectedPathID = pathID;
			if( string.IsNullOrEmpty( pathID ) ) {
				m_lblSelectedPath.Text = m_LanguageManager.TryGetString( "Status_NotSelected", m_lblSelectedPath.Text );
				m_lblSelectedPath.ForeColor = System.Drawing.Color.Red;
			}
			else {
				m_lblSelectedPath.Text = m_LanguageManager.TryGetString( "Status_Selected", m_lblSelectedPath.Text );
				m_lblSelectedPath.ForeColor = System.Drawing.Color.Green;
			}
			UpdateSortMethodState();
		}

		void ApplyResourcesToUI()
		{
			this.Text = m_LanguageManager.TryGetString( "FormTitle", this.Text );
			m_lblSortMethod.Text = m_LanguageManager.TryGetString( "m_lblSortMethod", m_lblSortMethod.Text );
			m_lblPathHint.Text = m_LanguageManager.TryGetString( "m_lblPathHint", m_lblPathHint.Text );
			m_lblCylinderAxis.Text = m_LanguageManager.TryGetString( "m_lblCylinderAxis", m_lblCylinderAxis.Text );
			m_lblExtrusionDir.Text = m_LanguageManager.TryGetString( "m_lblExtrusionDir", m_lblExtrusionDir.Text );
			m_lblRotationDir.Text = m_LanguageManager.TryGetString( "m_lblRotationDir", m_lblRotationDir.Text );
			m_lblExtrusionBandwidth.Text = m_LanguageManager.TryGetString( "m_lblBandwidth", m_lblExtrusionBandwidth.Text );
			m_lblRotationBandwidth.Text = m_LanguageManager.TryGetString( "m_lblBandwidth", m_lblRotationBandwidth.Text );
			m_lblRotationUnit.Text = m_LanguageManager.TryGetString( "m_lblRotationUnit", m_lblRotationUnit.Text );
			m_btnConfirm.Text = m_LanguageManager.TryGetString( "m_btnConfirm", m_btnConfirm.Text );

			// Sort method combobox
			int sortIdx = m_cmbSortMethod.SelectedIndex < 0 ? 0 : m_cmbSortMethod.SelectedIndex;
			m_cmbSortMethod.Items.Clear();

			m_cmbSortMethod.Items.Add( m_LanguageManager.TryGetString( "SortMethod_ShortestPath", SORT_METHOD_SHORTEST_PATH ) );
			m_cmbSortMethod.Items.Add( m_LanguageManager.TryGetString( "SortMethod_CylinderVertical", SORT_METHOD_CYLINDER_VERTICAL ) );
			m_cmbSortMethod.Items.Add( m_LanguageManager.TryGetString( "SortMethod_CylinderHorizontal", SORT_METHOD_CYLINDER_HORIZONTAL ) );
			m_cmbSortMethod.SelectedIndex = sortIdx;

			// Cylinder axis combobox
			int axisIdx = m_cmbCylinderAxis.SelectedIndex < 0 ? 2 : m_cmbCylinderAxis.SelectedIndex;
			m_cmbCylinderAxis.Items.Clear();

			m_cmbCylinderAxis.Items.Add( m_LanguageManager.TryGetString( "Axis_X", AXIS_X ) );
			m_cmbCylinderAxis.Items.Add( m_LanguageManager.TryGetString( "Axis_Y", AXIS_Y ) );
			m_cmbCylinderAxis.Items.Add( m_LanguageManager.TryGetString( "Axis_Z", AXIS_Z ) );
			m_cmbCylinderAxis.SelectedIndex = axisIdx;

			// Extrusion direction combobox
			int extrusionIdx = m_cmbExtrusionDir.SelectedIndex < 0 ? 0 : m_cmbExtrusionDir.SelectedIndex;
			m_cmbExtrusionDir.Items.Clear();

			m_cmbExtrusionDir.Items.Add( m_LanguageManager.TryGetString( "ExtrusionDir_FromTop", EXTRUSION_DIR_FROM_TOP ) );
			m_cmbExtrusionDir.Items.Add( m_LanguageManager.TryGetString( "ExtrusionDir_FromBottom", EXTRUSION_DIR_FROM_BOTTOM ) );
			m_cmbExtrusionDir.SelectedIndex = extrusionIdx;

			// Rotation direction combobox
			int rotationIdx = m_cmbRotationDir.SelectedIndex < 0 ? 0 : m_cmbRotationDir.SelectedIndex;
			m_cmbRotationDir.Items.Clear();

			m_cmbRotationDir.Items.Add( m_LanguageManager.TryGetString( "RotationDir_Clockwise", ROTATION_DIR_CLOCKWISE ) );
			m_cmbRotationDir.Items.Add( m_LanguageManager.TryGetString( "RotationDir_Counterclockwise", ROTATION_DIR_COUNTERCLOCKWISE ) );
			m_cmbRotationDir.SelectedIndex = rotationIdx;
		}

		void InitializeBandwidthControls()
		{
			m_nudExtrusionBandwidth.Value = (decimal)SortParams.DEFAULT_EXTRUSION_BANDWIDTH_MM;
			m_nudRotationBandwidth.Value = (decimal)SortParams.DEFAULT_ROTATION_BANDWIDTH_DEG;
		}

		void LoadImages()
		{
			// Only load images that will actually be displayed
			m_imgExtrusionFirst_FromTop_Counterclockwise = Properties.Resources.Sort_ExtrusionFirst_FromTop_Counterclockwise;
			m_imgExtrusionFirst_FromBottom_Counterclockwise = Properties.Resources.Sort_ExtrusionFirst_FromBottom_Counterclockwise;

			m_imgRotationFirst_FromTop_Clockwise = Properties.Resources.Sort_RotationFirst_FromTop_Clockwise;
			m_imgRotationFirst_FromTop_Counterclockwise = Properties.Resources.Sort_RotationFirst_FromTop_Counterclockwise;
		}

		void UpdateSortMethodState()
		{
			SortMethod method = GetSelectedSortMethod();

			// Shortest path: need path selected
			if( method == SortMethod.ShortestPath ) {
				m_btnConfirm.Enabled = !string.IsNullOrEmpty( m_SelectedPathID );
				m_lblPathHint.Visible = true;
				m_lblSelectedPath.Visible = true;
			}
			else {
				m_btnConfirm.Enabled = true;
				m_lblPathHint.Visible = false;
				m_lblSelectedPath.Visible = false;
			}

			bool isCylinder = ( method == SortMethod.CylinderVertical || method == SortMethod.CylinderHorizontal );
			bool isVertical = ( method == SortMethod.CylinderVertical );
			bool isHorizontal = ( method == SortMethod.CylinderHorizontal );

			// Cylinder axis selection visibility
			m_lblCylinderAxis.Visible = isCylinder;
			m_cmbCylinderAxis.Visible = isCylinder;

			// CylinderVertical: hide extrusion direction (hardcoded to "from top")
			// CylinderHorizontal: show extrusion direction
			m_lblExtrusionDir.Visible = isHorizontal;
			m_cmbExtrusionDir.Visible = isHorizontal;

			// CylinderVertical: show rotation direction
			// CylinderHorizontal: hide rotation direction (hardcoded to counterclockwise)
			m_lblRotationDir.Visible = isVertical;
			m_cmbRotationDir.Visible = isVertical;

			m_picPreview.Visible = isCylinder;

			// bandwidth: horizontal uses mm (extrusion), vertical uses degree (rotation)
			m_lblExtrusionBandwidth.Visible = isHorizontal;
			m_nudExtrusionBandwidth.Visible = isHorizontal;
			m_lblExtrusionUnit.Visible = isHorizontal;

			m_lblRotationBandwidth.Visible = isVertical;
			m_nudRotationBandwidth.Visible = isVertical;
			m_lblRotationUnit.Visible = isVertical;

			// Update preview image
			if( isCylinder ) {
				UpdateImage();
			}
		}

		void UpdateImage()
		{
			SortMethod method = GetSelectedSortMethod();

			System.Drawing.Image targetImage = null;

			if( method == SortMethod.CylinderHorizontal ) {
				// rotation is hardcoded to counterclockwise, only extrusion direction varies
				bool isFromUp = m_cmbExtrusionDir.SelectedIndex == 0;
				targetImage = isFromUp
					? m_imgExtrusionFirst_FromTop_Counterclockwise
					: m_imgExtrusionFirst_FromBottom_Counterclockwise;
			}
			else if( method == SortMethod.CylinderVertical ) {
				// extrusion is hardcoded to "from top", only rotation direction varies
				bool isClockwise = m_cmbRotationDir.SelectedIndex == 0;
				targetImage = isClockwise
					? m_imgRotationFirst_FromTop_Clockwise
					: m_imgRotationFirst_FromTop_Counterclockwise;
			}

			m_picPreview.Image = targetImage;
		}

		SortMethod GetSelectedSortMethod()
		{
			switch( m_cmbSortMethod.SelectedIndex ) {
				case 0:
					return SortMethod.ShortestPath;
				case 1:
					return SortMethod.CylinderVertical;
				case 2:
					return SortMethod.CylinderHorizontal;
				default:
					return SortMethod.ShortestPath;
			}
		}

		CylinderAxisType GetSelectedCylinderAxis()
		{
			switch( m_cmbCylinderAxis.SelectedIndex ) {
				case 0:
					return CylinderAxisType.X;
				case 1:
					return CylinderAxisType.Y;
				case 2:
					return CylinderAxisType.Z;
				default:
					return CylinderAxisType.Z;
			}
		}

		void m_cmbSortMethod_SelectedIndexChanged( object sender, System.EventArgs e )
		{
			UpdateSortMethodState();
		}

		void m_cmbExtrusionDir_SelectedIndexChanged( object sender, System.EventArgs e )
		{
			UpdateImage();
		}

		void m_cmbRotationDir_SelectedIndexChanged( object sender, System.EventArgs e )
		{
			UpdateImage();
		}

		void m_btnConfirm_Click( object sender, System.EventArgs e )
		{
			SortMethod method = GetSelectedSortMethod();
			bool isFromTop = m_cmbExtrusionDir.SelectedIndex == 0;
			bool isClockwise = m_cmbRotationDir.SelectedIndex == 0;

			SortParams sortParams = new SortParams
			{
				Method = method,
				CylinderAxis = GetSelectedCylinderAxis(),
				ExtrusionBandwidth_mm = (double)m_nudExtrusionBandwidth.Value,
				RotationBandwidth_deg = (double)m_nudRotationBandwidth.Value
			};

			// CylinderVertical: hardcode extrusion to "from top" (descending = true)
			// CylinderHorizontal: hardcode rotation to counterclockwise (descending = false)
			switch( method ) {
				case SortMethod.CylinderVertical:
					sortParams.IsExtrusionDescending = true;
					sortParams.IsRotationDescending = isClockwise;
					break;
				case SortMethod.CylinderHorizontal:
					sortParams.IsExtrusionDescending = isFromTop;
					sortParams.IsRotationDescending = false;
					break;
				default:
					sortParams.IsExtrusionDescending = isFromTop;
					sortParams.IsRotationDescending = isClockwise;
					break;
			}

			Confirm?.Invoke( sortParams );
		}

		string m_SelectedPathID;

		// Preview images - only the 4 that are actually used
		System.Drawing.Image m_imgExtrusionFirst_FromTop_Counterclockwise;
		System.Drawing.Image m_imgExtrusionFirst_FromBottom_Counterclockwise;
		System.Drawing.Image m_imgRotationFirst_FromTop_Clockwise;
		System.Drawing.Image m_imgRotationFirst_FromTop_Counterclockwise;
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "AutoSortPathDlg" );

		const string SORT_METHOD_SHORTEST_PATH = "最短路徑";
		const string SORT_METHOD_CYLINDER_VERTICAL = "圓管直排";
		const string SORT_METHOD_CYLINDER_HORIZONTAL = "圓管橫排";
		const string AXIS_X = "X 軸";
		const string AXIS_Y = "Y 軸";
		const string AXIS_Z = "Z 軸";
		const string EXTRUSION_DIR_FROM_TOP = "從上往下";
		const string EXTRUSION_DIR_FROM_BOTTOM = "從下往上";
		const string ROTATION_DIR_CLOCKWISE = "順時針";
		const string ROTATION_DIR_COUNTERCLOCKWISE = "逆時針";
	}
}
