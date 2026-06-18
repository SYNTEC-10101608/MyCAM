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
			InitializeSortMethodComboBox();
			InitializeAxisComboBox();
			InitializeDirectionComboBoxes();
			InitializeBandwidthControls();
			LoadImages();
			UpdateSortMethodState();
		}

		public SortMethod CurrentSortMethod => GetSelectedSortMethod();

		public void UpdateSelectedPathStatus( string pathID )
		{
			m_SelectedPathID = pathID;
			if( string.IsNullOrEmpty( pathID ) ) {
				m_lblSelectedPath.Text = "未選取";
				m_lblSelectedPath.ForeColor = System.Drawing.Color.Red;
			}
			else {
				m_lblSelectedPath.Text = "已選取";
				m_lblSelectedPath.ForeColor = System.Drawing.Color.Green;
			}
			UpdateSortMethodState();
		}

		void InitializeSortMethodComboBox()
		{
			m_cmbSortMethod.Items.Clear();
			m_cmbSortMethod.Items.Add( "最短路徑" );
			m_cmbSortMethod.Items.Add( "圓管直排" );
			m_cmbSortMethod.Items.Add( "圓管橫排" );
			m_cmbSortMethod.SelectedIndex = 0;
		}

		void InitializeAxisComboBox()
		{
			m_cmbCylinderAxis.Items.Clear();
			m_cmbCylinderAxis.Items.Add( "X 軸" );
			m_cmbCylinderAxis.Items.Add( "Y 軸" );
			m_cmbCylinderAxis.Items.Add( "Z 軸" );
			m_cmbCylinderAxis.SelectedIndex = 2; // default Z
		}

		void InitializeDirectionComboBoxes()
		{
			m_cmbExtrusionDir.Items.Clear();
			m_cmbExtrusionDir.Items.Add( "從上往下" );
			m_cmbExtrusionDir.Items.Add( "從下往上" );
			m_cmbExtrusionDir.SelectedIndex = 0;

			m_cmbRotationDir.Items.Clear();
			m_cmbRotationDir.Items.Add( "順時針" );
			m_cmbRotationDir.Items.Add( "逆時針" );
			m_cmbRotationDir.SelectedIndex = 0;
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
				if( isFromUp ) {
					targetImage = m_imgExtrusionFirst_FromTop_Counterclockwise;
				}
				else {
					targetImage = m_imgExtrusionFirst_FromBottom_Counterclockwise;
				}
			}
			else if( method == SortMethod.CylinderVertical ) {
				// extrusion is hardcoded to "from top", only rotation direction varies
				bool isClockwise = m_cmbRotationDir.SelectedIndex == 0;
				if( isClockwise ) {
					targetImage = m_imgRotationFirst_FromTop_Clockwise;
				}
				else {
					targetImage = m_imgRotationFirst_FromTop_Counterclockwise;
				}
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
	}
}
