using MyCAM.Data;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
{
	public partial class PatternSettingDlg : EditDialogBase<IStdPatternGeomData>
	{
		public PatternSettingDlg( IStdPatternGeomData standardPatternGeomData )
		{
			m_StandardPatternGeomData = standardPatternGeomData;
			m_PathType = standardPatternGeomData == null ? PathType.Contour : standardPatternGeomData.PathType;
			InitializeComponent();
			InitializeControlsValue();
		}

		void InitializeControlsValue()
		{
			m_cmbPathType.SelectedIndex = GetComboIndexFromPathType( m_PathType );
			ShowSpecificPanel( m_cmbPathType.SelectedIndex );
			switch( m_PathType ) {
				case PathType.Contour:
					m_cmbPathType.SelectedIndex = 0;
					break;
				case PathType.Circle:
					if( !( m_StandardPatternGeomData is CircleGeomData circleGeomData ) ) {
						return;
					}
					m_txbCircleDiameter.Text = circleGeomData.Diameter.ToString();
					m_txbCircleRotatedAngle.Text = circleGeomData.RotatedAngle_deg.ToString();
					break;
				case PathType.Rectangle:
					if( !( m_StandardPatternGeomData is RectangleGeomData rectangleGeomData ) ) {
						return;
					}
					m_txbRecLength.Text = rectangleGeomData.Length.ToString();
					m_txbRecWidth.Text = rectangleGeomData.Width.ToString();
					m_txbRecCornerRadius.Text = rectangleGeomData.CornerRadius.ToString();
					m_txbRecRotatedAngle.Text = rectangleGeomData.RotatedAngle_deg.ToString();
					break;
				case PathType.Runway:
					if( !( m_StandardPatternGeomData is RunwayGeomData runwayGeomData ) ) {
						return;
					}
					m_txbRunwayLength.Text = runwayGeomData.Length.ToString();
					m_txbRunwayWidth.Text = runwayGeomData.Width.ToString();
					m_txbRunwayRotatedAngle.Text = runwayGeomData.RotatedAngle_deg.ToString();
					break;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					if( !( m_StandardPatternGeomData is PolygonGeomData polygonGeomData ) ) {
						return;
					}
					m_txbPolygonSideLength.Text = polygonGeomData.SideLength.ToString();
					m_txbPolygonCornerRadius.Text = polygonGeomData.CornerRadius.ToString();
					m_txbPolygonRotatedAngle.Text = polygonGeomData.RotatedAngle_deg.ToString();
					break;
				default:
					break;
			}
			if( m_StandardPatternGeomData != null ) {
				m_chkCoordReverse.Checked = m_StandardPatternGeomData.IsCoordinateReversed;
			}
		}

		void m_cmbPathType_SelectedIndexChanged( object sender, EventArgs e )
		{
			m_chkCoordReverse.Checked = false;
			IStdPatternGeomData newStandardPatternGeomData = null;
			ShowSpecificPanel( m_cmbPathType.SelectedIndex );

			PathType selectedPathType = (PathType)m_cmbPathType.SelectedIndex;
			PathType currentPathType = m_StandardPatternGeomData == null ? PathType.Contour : m_StandardPatternGeomData.PathType;

			switch( selectedPathType ) {
				case PathType.Circle:
					newStandardPatternGeomData = CreateCirclePattern( currentPathType );
					break;
				case PathType.Rectangle:
					newStandardPatternGeomData = CreateRectanglePattern( currentPathType );
					break;
				case PathType.Runway:
					newStandardPatternGeomData = CreateRunwayPattern( currentPathType );
					break;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					newStandardPatternGeomData = CreatePolygonPattern( selectedPathType, currentPathType );
					break;
				default:
					newStandardPatternGeomData = null;
					break;
			}
			RaisePreview( newStandardPatternGeomData );
			m_NewStandardPatternGeomData = newStandardPatternGeomData;
		}

		IStdPatternGeomData CreateCirclePattern( PathType currentPathType )
		{
			IStdPatternGeomData standardPatternGeomData = null;

			// If current type is not Circle, create new default CircleGeomData for each item
			if( currentPathType != PathType.Circle ) {
				standardPatternGeomData = new CircleGeomData();
			}
			else {
				standardPatternGeomData = m_StandardPatternGeomData;
			}

			CircleGeomData circleGeomData = (CircleGeomData)standardPatternGeomData;
			SetCircleParam( circleGeomData.Diameter, circleGeomData.RotatedAngle_deg );
			return standardPatternGeomData;
		}

		IStdPatternGeomData CreateRectanglePattern( PathType currentPathType )
		{
			IStdPatternGeomData standardPatternGeomData = null;

			// If current type is not Rectangle, create new default RectangleGeomData for each item
			if( currentPathType != PathType.Rectangle ) {
				standardPatternGeomData = new RectangleGeomData();
			}
			else {
				standardPatternGeomData = m_StandardPatternGeomData;
			}

			RectangleGeomData rectGeomData = (RectangleGeomData)standardPatternGeomData;
			SetRectangleParam( rectGeomData.Length, rectGeomData.Width, rectGeomData.CornerRadius, rectGeomData.RotatedAngle_deg );
			return standardPatternGeomData;
		}

		IStdPatternGeomData CreateRunwayPattern( PathType currentPathType )
		{
			IStdPatternGeomData standardPatternGeomData = null;

			// If current type is not Runway, create new default RunwayGeomData for each item
			if( currentPathType != PathType.Runway ) {
				standardPatternGeomData = new RunwayGeomData();
			}
			else {
				standardPatternGeomData = m_StandardPatternGeomData;
			}

			RunwayGeomData runwayGeomData = (RunwayGeomData)standardPatternGeomData;
			SetRunwayParam( runwayGeomData.Length, runwayGeomData.Width, runwayGeomData.RotatedAngle_deg );
			return standardPatternGeomData;
		}

		IStdPatternGeomData CreatePolygonPattern( PathType selectedPathType, PathType currentPathType )
		{
			IStdPatternGeomData standardPatternGeomData = null;

			// Check if current type is already a polygon type
			bool isCurrentPolygon = IsPolygonSide( currentPathType, out int currentSides );

			// Get the number of sides for the selected polygon type
			if( !IsPolygonSide( selectedPathType, out int selectedSides ) ) {

				// This should not happen as selectedPathType is already confirmed to be a polygon
				return standardPatternGeomData;
			}

			// If current type is not a polygon OR it's a different polygon type, create new PolygonGeomData
			if( !isCurrentPolygon || currentPathType != selectedPathType ) {
				standardPatternGeomData = new PolygonGeomData( selectedSides );
			}
			else {
				standardPatternGeomData = m_StandardPatternGeomData;
			}

			PolygonGeomData polygonGeomData = (PolygonGeomData)standardPatternGeomData;
			SetPolygonParam( polygonGeomData.SideLength, polygonGeomData.CornerRadius, polygonGeomData.RotatedAngle_deg );
			return standardPatternGeomData;
		}

		void HideAllPanels()
		{
			m_panelRectangle.Visible = false;
			m_panelCircle.Visible = false;
			m_panelRunway.Visible = false;
			m_panelPolygon.Visible = false;
			m_chkCoordReverse.Visible = false;
		}

		void ShowSpecificPanel( int nIndex )
		{
			HideAllPanels();
			switch( (PathType)nIndex ) {
				case PathType.Circle:
					m_panelCircle.Visible = true;
					m_chkCoordReverse.Visible = true;
					break;
				case PathType.Rectangle:
					m_panelRectangle.Visible = true;
					m_chkCoordReverse.Visible = true;
					break;
				case PathType.Runway:
					m_panelRunway.Visible = true;
					m_chkCoordReverse.Visible = true;
					break;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					m_panelPolygon.Visible = true;
					m_chkCoordReverse.Visible = true;
					break;
				default:
					break;
			}
		}

		void SetCircleParam( double diameter, double rotatedAngle_deg )
		{
			m_txbCircleDiameter.Text = diameter.ToString( "F3" );
			m_txbCircleRotatedAngle.Text = rotatedAngle_deg.ToString( "F3" );
			m_CircleDiameter = diameter;
			m_CirRotatedAngle_deg = rotatedAngle_deg;
		}

		void SetRectangleParam( double length, double width, double cornerRadius, double rotatedAngle_deg )
		{
			m_txbRecLength.Text = length.ToString( "F3" );
			m_txbRecWidth.Text = width.ToString( "F3" );
			m_txbRecCornerRadius.Text = cornerRadius.ToString( "F3" );
			m_txbRecRotatedAngle.Text = rotatedAngle_deg.ToString( "F3" );
			m_RectLength = length;
			m_RectWidth = width;
			m_RectCornerRadius = cornerRadius;
			m_RectRotatedAngle_deg = rotatedAngle_deg;
		}

		void SetRunwayParam( double length, double width, double rotatedAngle_deg )
		{
			m_txbRunwayLength.Text = length.ToString( "F3" );
			m_txbRunwayWidth.Text = width.ToString( "F3" );
			m_txbRunwayRotatedAngle.Text = rotatedAngle_deg.ToString( "F3" );
			m_RunwayLength = length;
			m_RunwayWidth = width;
			m_RunwayRotatedAngle_deg = rotatedAngle_deg;
		}

		void SetPolygonParam( double sideLength, double cornerRadius, double rotatedAngle_deg )
		{
			m_txbPolygonSideLength.Text = sideLength.ToString( "F3" );
			m_txbPolygonCornerRadius.Text = cornerRadius.ToString( "F3" );
			m_txbPolygonRotatedAngle.Text = rotatedAngle_deg.ToString( "F3" );
			m_PolygonSideLength = sideLength;
			m_PolygonCornerRadius = cornerRadius;
			m_PolygonRotatedAngle_deg = rotatedAngle_deg;
		}

		bool IsPolygonSide( PathType pathType, out int nSides )
		{
			switch( pathType ) {
				case PathType.Triangle:
					nSides = 3;
					return true;
				case PathType.Square:
					nSides = 4;
					return true;
				case PathType.Pentagon:
					nSides = 5;
					return true;
				case PathType.Hexagon:
					nSides = 6;
					return true;
				default:
					nSides = -1;
					return false;
			}
		}

		int GetComboIndexFromPathType( PathType pathType )
		{
			switch( pathType ) {
				case PathType.Contour:
					return 0;
				case PathType.Circle:
					return 1;
				case PathType.Rectangle:
					return 2;
				case PathType.Runway:
					return 3;
				case PathType.Triangle:
					return 4;
				case PathType.Square:
					return 5;
				case PathType.Pentagon:
					return 6;
				case PathType.Hexagon:
					return 7;
				default:
					return 0;
			}
		}

		// circle
		void m_txbCircleDiameter_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetCircleDiameter();
		}

		void m_txbCircleDiameter_Leave( object sender, System.EventArgs e )
		{
			SetCircleDiameter();
		}

		void SetCircleDiameter()
		{
			if( double.TryParse( m_txbCircleDiameter.Text, out double diameter ) && diameter > 0 ) {
				if( !( m_NewStandardPatternGeomData is CircleGeomData circleGeomData ) ) {
					return;
				}
				m_CircleDiameter = diameter;
				circleGeomData.Diameter = m_CircleDiameter;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_txbCircleDiameter.Text = m_CircleDiameter.ToString( "F3" );
			}
		}

		void m_txbCircleRotatedAngle_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetCircleRotatedAngle();
		}

		void m_txbCircleRotatedAngle_Leave( object sender, EventArgs e )
		{
			SetCircleRotatedAngle();
		}

		void SetCircleRotatedAngle()
		{
			if( double.TryParse( m_txbCircleRotatedAngle.Text, out double angle ) ) {
				if( !( m_NewStandardPatternGeomData is CircleGeomData circleGeomData ) ) {
					return;
				}
				m_CirRotatedAngle_deg = angle;
				circleGeomData.RotatedAngle_deg = m_CirRotatedAngle_deg;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_txbCircleRotatedAngle.Text = m_CirRotatedAngle_deg.ToString( "F3" );
			}
		}

		// rectangle
		void m_txbRecLength_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetRectangleLength();
		}

		void m_txbRecLength_Leave( object sender, System.EventArgs e )
		{
			SetRectangleLength();
		}

		void SetRectangleLength()
		{
			if( double.TryParse( m_txbRecLength.Text, out double length ) && length > 0 && length > 2 * m_RectCornerRadius ) {
				if( !( m_NewStandardPatternGeomData is RectangleGeomData rectangleGeomData ) ) {
					return;
				}
				m_RectLength = length;
				rectangleGeomData.Length = m_RectLength;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_txbRecLength.Text = m_RectLength.ToString( "F3" );
			}
		}

		void m_txbRecWidth_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetRectangleWidth();
		}

		void m_txbRecWidth_Leave( object sender, System.EventArgs e )
		{
			SetRectangleWidth();
		}

		void SetRectangleWidth()
		{
			if( double.TryParse( m_txbRecWidth.Text, out double width ) && width > 0 && width > 2 * m_RectCornerRadius ) {
				if( !( m_NewStandardPatternGeomData is RectangleGeomData rectangleGeomData ) ) {
					return;
				}
				m_RectWidth = width;
				rectangleGeomData.Width = m_RectWidth;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_txbRecWidth.Text = m_RectWidth.ToString( "F3" );
			}
		}

		void m_txbRecCornerRadius_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetRectangleCornerRadius();
		}

		void m_txbRecCornerRadius_Leave( object sender, System.EventArgs e )
		{
			SetRectangleCornerRadius();
		}

		void SetRectangleCornerRadius()
		{
			if( double.TryParse( m_txbRecCornerRadius.Text, out double cornerRadius ) && cornerRadius >= 0 && 2 * cornerRadius < m_RectLength && 2 * cornerRadius < m_RectWidth ) {
				if( !( m_NewStandardPatternGeomData is RectangleGeomData rectangleGeomData ) ) {
					return;
				}
				m_RectCornerRadius = cornerRadius;
				rectangleGeomData.CornerRadius = m_RectCornerRadius;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_txbRecCornerRadius.Text = m_RectCornerRadius.ToString( "F3" );
			}
		}

		void m_txbRecRotatedAngle_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetRectangleRotatedAngle();
		}

		void m_txbRecRotatedAngle_Leave( object sender, System.EventArgs e )
		{
			SetRectangleRotatedAngle();
		}

		void SetRectangleRotatedAngle()
		{
			if( double.TryParse( m_txbRecRotatedAngle.Text, out double rotatedAngle_deg ) ) {
				if( !( m_NewStandardPatternGeomData is RectangleGeomData rectangleGeomData ) ) {
					return;
				}
				m_RectRotatedAngle_deg = rotatedAngle_deg;
				rectangleGeomData.RotatedAngle_deg = m_RectRotatedAngle_deg;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_txbRecRotatedAngle.Text = m_RectRotatedAngle_deg.ToString( "F3" );
			}
		}

		// runway
		void m_txbRunwayLength_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetRunwayLength();
		}

		void m_txbRunwayLength_Leave( object sender, System.EventArgs e )
		{
			SetRunwayLength();
		}

		void SetRunwayLength()
		{
			if( double.TryParse( m_txbRunwayLength.Text, out double length ) && length > m_RunwayWidth && length > 0 ) {
				if( !( m_NewStandardPatternGeomData is RunwayGeomData runwayGeomData ) ) {
					return;
				}
				m_RunwayLength = length;
				runwayGeomData.Length = m_RunwayLength;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_txbRunwayLength.Text = m_RunwayLength.ToString( "F3" );
			}
		}

		void m_txbRunwayWidth_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetRunwayWidth();
		}

		void m_txbRunwayWidth_Leave( object sender, System.EventArgs e )
		{
			SetRunwayWidth();
		}

		void SetRunwayWidth()
		{
			if( double.TryParse( m_txbRunwayWidth.Text, out double width ) && width > 0 && width < m_RunwayLength ) {
				if( !( m_NewStandardPatternGeomData is RunwayGeomData runwayGeomData ) ) {
					return;
				}
				m_RunwayWidth = width;
				runwayGeomData.Width = m_RunwayWidth;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_txbRunwayWidth.Text = m_RunwayWidth.ToString( "F3" );
			}
		}

		void m_txbRunwayRotatedAngle_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetRunwayRotatedAngle();
		}

		void m_txbRunwayRotatedAngle_Leave( object sender, System.EventArgs e )
		{
			SetRunwayRotatedAngle();
		}

		void SetRunwayRotatedAngle()
		{
			if( double.TryParse( m_txbRunwayRotatedAngle.Text, out double rotatedAngle_deg ) ) {
				if( !( m_NewStandardPatternGeomData is RunwayGeomData runwayGeomData ) ) {
					return;
				}
				m_RunwayRotatedAngle_deg = rotatedAngle_deg;
				runwayGeomData.RotatedAngle_deg = m_RunwayRotatedAngle_deg;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_txbRunwayRotatedAngle.Text = m_RunwayRotatedAngle_deg.ToString( "F3" );
			}
		}

		// polygon
		void m_txbPolygonSideLength_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetPolygonSideLength();
		}

		void m_txbPolygonSideLength_Leave( object sender, System.EventArgs e )
		{
			SetPolygonSideLength();
		}

		void SetPolygonSideLength()
		{
			if( double.TryParse( m_txbPolygonSideLength.Text, out double sideLength ) && sideLength > 0 && sideLength > 2 * m_PolygonCornerRadius ) {
				if( !( m_NewStandardPatternGeomData is PolygonGeomData polygonGeomData ) ) {
					return;
				}

				// validate that the edge length is sufficient for the corner radius
				if( m_PolygonCornerRadius > 0 && !IsPolygonGeometryValid( polygonGeomData.Sides, sideLength, m_PolygonCornerRadius ) ) {
					m_txbPolygonSideLength.Text = m_PolygonSideLength.ToString( "F3" );
					return;
				}

				m_PolygonSideLength = sideLength;
				polygonGeomData.SideLength = m_PolygonSideLength;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_txbPolygonSideLength.Text = m_PolygonSideLength.ToString( "F3" );
			}
		}

		void m_txbPolygonRotatedAngle_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetPolygonRotatedAngle();
		}

		void m_txbPolygonRotatedAngle_Leave( object sender, System.EventArgs e )
		{
			SetPolygonRotatedAngle();
		}

		void SetPolygonRotatedAngle()
		{
			if( double.TryParse( m_txbPolygonRotatedAngle.Text, out double rotatedAngle_deg ) ) {
				if( !( m_NewStandardPatternGeomData is PolygonGeomData polygonGeomData ) ) {
					return;
				}
				m_PolygonRotatedAngle_deg = rotatedAngle_deg;
				polygonGeomData.RotatedAngle_deg = m_PolygonRotatedAngle_deg;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_txbPolygonRotatedAngle.Text = m_PolygonRotatedAngle_deg.ToString( "F3" );
			}
		}

		void m_txbPolygonCornerRadius_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetPolygonCornerRadius();
		}

		void m_txbPolygonCornerRadius_Leave( object sender, System.EventArgs e )
		{
			SetPolygonCornerRadius();
		}

		void SetPolygonCornerRadius()
		{
			if( double.TryParse( m_txbPolygonCornerRadius.Text, out double cornerRadius ) && cornerRadius >= 0 && 2 * cornerRadius < m_PolygonSideLength ) {
				if( !( m_NewStandardPatternGeomData is PolygonGeomData polygonGeomData ) ) {
					return;
				}

				// validate that the corner radius is compatible with the edge length
				if( cornerRadius > 0 && !IsPolygonGeometryValid( polygonGeomData.Sides, m_PolygonSideLength, cornerRadius ) ) {
					m_txbPolygonCornerRadius.Text = m_PolygonCornerRadius.ToString( "F3" );
					return;
				}

				m_PolygonCornerRadius = cornerRadius;
				polygonGeomData.CornerRadius = m_PolygonCornerRadius;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_txbPolygonCornerRadius.Text = m_PolygonCornerRadius.ToString( "F3" );
			}
		}

		void m_chkCoordReverse_CheckedChanged( object sender, EventArgs e )
		{
			m_NewStandardPatternGeomData.IsCoordinateReversed = m_chkCoordReverse.Checked;
			RaisePreview( m_NewStandardPatternGeomData );
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			RaiseConfirm( m_NewStandardPatternGeomData );
		}

		bool IsPolygonGeometryValid( int sides, double sideLength, double cornerRadius )
		{
			// interior angle calculation constant
			const int INTERIOR_ANGLE_NUMERATOR_OFFSET = 2;

			// edge tangent distance multiplier
			const double EDGE_TANGENT_MULTIPLIER = 2.0;

			// calculate interior angle and tangent distance
			// interior angle = (n-2) * π / n
			double interiorAngle = ( sides - INTERIOR_ANGLE_NUMERATOR_OFFSET ) * Math.PI / sides;
			double halfAngle = interiorAngle / EDGE_TANGENT_MULTIPLIER;
			double tangentDistance = cornerRadius / Math.Tan( halfAngle );

			// check if edge length is sufficient: 2 * tangentDistance must be less than edge length
			return tangentDistance * EDGE_TANGENT_MULTIPLIER <= sideLength;
		}

		IStdPatternGeomData m_StandardPatternGeomData;
		IStdPatternGeomData m_NewStandardPatternGeomData;
		PathType m_PathType;

		// circle
		double m_CircleDiameter;
		double m_CirRotatedAngle_deg;

		// rectangle
		double m_RectLength;
		double m_RectWidth;
		double m_RectCornerRadius;
		double m_RectRotatedAngle_deg;

		// runway
		double m_RunwayLength;
		double m_RunwayWidth;
		double m_RunwayRotatedAngle_deg;

		// polygon
		double m_PolygonSideLength;
		double m_PolygonCornerRadius;
		double m_PolygonRotatedAngle_deg;
	}
}
