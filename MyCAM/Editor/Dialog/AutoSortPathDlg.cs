using System;
using System.Windows.Forms;
using System.IO;

namespace MyCAM.Editor.Dialog
{
	public partial class AutoSortPathDlg : EditDialogBase<SortParams>
	{
		public AutoSortPathDlg()
		{
			InitializeComponent();
			InitializeSortMethodComboBox();
			InitializeDirectionComboBoxes();
			LoadPreviewImages();
			UpdateConfirmButtonState();
		}

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
			UpdateConfirmButtonState();
		}

		void InitializeSortMethodComboBox()
		{
			m_cmbSortMethod.Items.Clear();
			m_cmbSortMethod.Items.Add( "最短路徑" );
			m_cmbSortMethod.Items.Add( "圓管直排" );
			m_cmbSortMethod.Items.Add( "圓管橫排" );
			m_cmbSortMethod.SelectedIndex = 0;
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

		void LoadPreviewImages()
		{
			m_imgExtrusionFirst_FromTop_Clockwise = Properties.Resources.Sort_ExtrusionFirst_FromTop_Clockwise;
			m_imgExtrusionFirst_FromTop_Counterclockwise = Properties.Resources.Sort_ExtrusionFirst_FromTop_Counterclockwise;
			m_imgExtrusionFirst_FromBottom_Clockwise = Properties.Resources.Sort_ExtrusionFirst_FromBottom_Clockwise;
			m_imgExtrusionFirst_FromBottom_Counterclockwise = Properties.Resources.Sort_ExtrusionFirst_FromBottom_Counterclockwise;

			m_imgRotationFirst_FromTop_Clockwise = Properties.Resources.Sort_RotationFirst_FromTop_Clockwise;
			m_imgRotationFirst_FromTop_Counterclockwise = Properties.Resources.Sort_RotationFirst_FromTop_Counterclockwise;
			m_imgRotationFirst_FromBottom_Clockwise = Properties.Resources.Sort_RotationFirst_FromBottom_Clockwise;
			m_imgRotationFirst_FromBottom_Counterclockwise = Properties.Resources.Sort_RotationFirst_FromBottom_Counterclockwise;
		}

		void UpdateConfirmButtonState()
		{
			SortMethod method = GetSelectedSortMethod();
			bool isCylinder = ( method == SortMethod.CylinderVertical || method == SortMethod.CylinderHorizontal );

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

			// Cylinder direction options visibility
			m_lblExtrusionDir.Visible = isCylinder;
			m_cmbExtrusionDir.Visible = isCylinder;
			m_lblRotationDir.Visible = isCylinder;
			m_cmbRotationDir.Visible = isCylinder;
			m_picPreview.Visible = isCylinder;

			// Update preview image
			if( isCylinder ) {
				UpdatePreviewImage();
			}
		}

		void UpdatePreviewImage()
		{
			SortMethod method = GetSelectedSortMethod();
			bool isFromUp = m_cmbExtrusionDir.SelectedIndex == 0;
			bool isClockwise = m_cmbRotationDir.SelectedIndex == 0;

			System.Drawing.Image targetImage = null;

			if( method == SortMethod.CylinderHorizontal ) {
				// Extrusion First
				if( isFromUp && isClockwise ) {
					targetImage = m_imgExtrusionFirst_FromTop_Clockwise;
				}
				else if( !isFromUp && isClockwise ) {
					targetImage = m_imgExtrusionFirst_FromBottom_Clockwise;
				}
				else if( isFromUp && !isClockwise ) {
					targetImage = m_imgExtrusionFirst_FromTop_Counterclockwise;
				}
				else {
					targetImage = m_imgExtrusionFirst_FromBottom_Counterclockwise;
				}
			}
			else if( method == SortMethod.CylinderVertical ) {
				// Rotation First
				if( isFromUp && isClockwise ) {
					targetImage = m_imgRotationFirst_FromTop_Clockwise;
				}
				else if( !isFromUp && isClockwise ) {
					targetImage = m_imgRotationFirst_FromBottom_Clockwise;
				}
				else if( isFromUp && !isClockwise ) {
					targetImage = m_imgRotationFirst_FromTop_Counterclockwise;
				}
				else {
					targetImage = m_imgRotationFirst_FromBottom_Counterclockwise;
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

		void m_cmbSortMethod_SelectedIndexChanged( object sender, System.EventArgs e )
		{
			UpdateConfirmButtonState();
		}

		void m_cmbExtrusionDir_SelectedIndexChanged( object sender, System.EventArgs e )
		{
			UpdatePreviewImage();
		}

		void m_cmbRotationDir_SelectedIndexChanged( object sender, System.EventArgs e )
		{
			UpdatePreviewImage();
		}

		void m_btnConfirm_Click( object sender, System.EventArgs e )
		{
			SortMethod method = GetSelectedSortMethod();
			bool isFromTop = m_cmbExtrusionDir.SelectedIndex == 0;
			bool isClockwise = m_cmbRotationDir.SelectedIndex == 0;
			
			SortParams sortParams = new SortParams
			{
				Method = method,
				IsExtrusionDescending = isFromTop,
				IsRotationDescending = ( method == SortMethod.CylinderHorizontal ) ? !isClockwise : isClockwise
			};
			RaiseConfirm( sortParams );
		}

		string m_SelectedPathID;

		// Preview images - Extrusion First (Cylinder Horizontal)
		System.Drawing.Image m_imgExtrusionFirst_FromTop_Clockwise;
		System.Drawing.Image m_imgExtrusionFirst_FromTop_Counterclockwise;
		System.Drawing.Image m_imgExtrusionFirst_FromBottom_Clockwise;
		System.Drawing.Image m_imgExtrusionFirst_FromBottom_Counterclockwise;

		// Preview images - Rotation First (Cylinder Vertical)
		System.Drawing.Image m_imgRotationFirst_FromTop_Clockwise;
		System.Drawing.Image m_imgRotationFirst_FromTop_Counterclockwise;
		System.Drawing.Image m_imgRotationFirst_FromBottom_Clockwise;
		System.Drawing.Image m_imgRotationFirst_FromBottom_Counterclockwise;
	}
}
