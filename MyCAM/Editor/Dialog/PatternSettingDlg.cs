using System;
using System.Windows.Forms;
using MyCAM.Data;

namespace MyCAM.Editor.Dialog
{
	public partial class PatternSettingDlg : EditDialogBase<IStdPatternGeomData>
	{
		public PatternSettingDlg( IStdPatternGeomData standardPatternGeomData )
		{
			m_StandardPatternGeomData = standardPatternGeomData;
			m_PathType = standardPatternGeomData == null ? PathType.Contour : standardPatternGeomData.PathType;
			InitializeComponent();
			ApplyLanguage();
			m_LanguageManager = new LanguageManager.LanguageManager( "PatternSettingDlg" );
			InitLocalizedTexts();
			InitializeControlsValue();
		}

		// the labels of the pattern panels share their texts; the path types are localized at runtime
		void InitLocalizedTexts()
		{
			// circle panel labels
			m_lblCircleDiameter.Text = m_LanguageManager.TryGetString( "m_lblDiameter", m_lblCircleDiameter.Text );
			m_lblCircleRotateAngle.Text = m_LanguageManager.TryGetString( "m_lblRotatedAngle", m_lblCircleRotateAngle.Text );

			// rectangle panel labels
			m_lblRectLength.Text = m_LanguageManager.TryGetString( "m_lblLength", m_lblRectLength.Text );
			m_lblRectWidth.Text = m_LanguageManager.TryGetString( "m_lblWidth", m_lblRectWidth.Text );
			m_lblRectFilletRadius.Text = m_LanguageManager.TryGetString( "m_lblCornerRadius", m_lblRectFilletRadius.Text );
			m_lblRectRotateAngle.Text = m_LanguageManager.TryGetString( "m_lblRotatedAngle", m_lblRectRotateAngle.Text );

			// runway panel labels
			m_lblRunwayLength.Text = m_LanguageManager.TryGetString( "m_lblLength", m_lblRunwayLength.Text );
			m_lblRunwayWidth.Text = m_LanguageManager.TryGetString( "m_lblWidth", m_lblRunwayWidth.Text );
			m_lblRunwayRotateAngle.Text = m_LanguageManager.TryGetString( "m_lblRotatedAngle", m_lblRunwayRotateAngle.Text );

			// polygon panel labels
			m_lblPolygonSideLength.Text = m_LanguageManager.TryGetString( "m_lblSideLength", m_lblPolygonSideLength.Text );
			m_lblPolygonFilletRadius.Text = m_LanguageManager.TryGetString( "m_lblCornerRadius", m_lblPolygonFilletRadius.Text );
			m_lblPolygonRotateAngle.Text = m_LanguageManager.TryGetString( "m_lblRotatedAngle", m_lblPolygonRotateAngle.Text );

			// Update combo box items with localized strings
			UpdatePathTypeComboBoxItems();
		}

		void UpdatePathTypeComboBoxItems()
		{
			int selectedIndex = m_cmbPathType.SelectedIndex;
			m_cmbPathType.Items.Clear();

			m_cmbPathType.Items.Add( m_LanguageManager.TryGetString( "PathType_Contour", PATH_TYPE_CONTOUR ) );
			m_cmbPathType.Items.Add( m_LanguageManager.TryGetString( "PathType_Circle", PATH_TYPE_CIRCLE ) );
			m_cmbPathType.Items.Add( m_LanguageManager.TryGetString( "PathType_Rectangle", PATH_TYPE_RECTANGLE ) );
			m_cmbPathType.Items.Add( m_LanguageManager.TryGetString( "PathType_Runway", PATH_TYPE_RUNWAY ) );
			m_cmbPathType.Items.Add( m_LanguageManager.TryGetString( "PathType_Triangle", PATH_TYPE_TRIANGLE ) );
			m_cmbPathType.Items.Add( m_LanguageManager.TryGetString( "PathType_Square", PATH_TYPE_SQUARE ) );
			m_cmbPathType.Items.Add( m_LanguageManager.TryGetString( "PathType_Pentagon", PATH_TYPE_PENTAGON ) );
			m_cmbPathType.Items.Add( m_LanguageManager.TryGetString( "PathType_Hexagon", PATH_TYPE_HEXAGON ) );
			if( selectedIndex >= 0 && selectedIndex < m_cmbPathType.Items.Count ) {
				m_cmbPathType.SelectedIndex = selectedIndex;
			}
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
					m_numCircleDiameter.Text = circleGeomData.Diameter.ToString();
					m_numCircleRotatedAngle.Text = circleGeomData.RotatedAngle_deg.ToString();
					break;
				case PathType.Rectangle:
					if( !( m_StandardPatternGeomData is RectangleGeomData rectangleGeomData ) ) {
						return;
					}
					m_numRecLength.Text = rectangleGeomData.Length.ToString();
					m_numRecWidth.Text = rectangleGeomData.Width.ToString();
					m_numRecCornerRadius.Text = rectangleGeomData.CornerRadius.ToString();
					m_numRecRotatedAngle.Text = rectangleGeomData.RotatedAngle_deg.ToString();
					break;
				case PathType.Runway:
					if( !( m_StandardPatternGeomData is RunwayGeomData runwayGeomData ) ) {
						return;
					}
					m_numRunwayLength.Text = runwayGeomData.Length.ToString();
					m_numRunwayWidth.Text = runwayGeomData.Width.ToString();
					m_numRunwayRotatedAngle.Text = runwayGeomData.RotatedAngle_deg.ToString();
					break;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					if( !( m_StandardPatternGeomData is PolygonGeomData polygonGeomData ) ) {
						return;
					}
					m_numPolygonSideLength.Text = polygonGeomData.SideLength.ToString();
					m_numPolygonCornerRadius.Text = polygonGeomData.CornerRadius.ToString();
					m_numPolygonRotatedAngle.Text = polygonGeomData.RotatedAngle_deg.ToString();
					break;
				default:
					break;
			}
			if( m_StandardPatternGeomData != null ) {
				m_chkCoordReverse.Checked = m_StandardPatternGeomData.IsCoordinateReversed;
			}
		}

		public Action AutoRecognizeAction;
		void m_btnAutoRecognize_Click( object sender, EventArgs e )
		{
			AutoRecognizeAction?.Invoke();
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
			m_pnlRectangle.Visible = false;
			m_pnlCircle.Visible = false;
			m_pnlRunway.Visible = false;
			m_pnlPolygon.Visible = false;
			m_chkCoordReverse.Visible = false;
		}

		void ShowSpecificPanel( int nIndex )
		{
			HideAllPanels();
			switch( (PathType)nIndex ) {
				case PathType.Circle:
					m_pnlCircle.Visible = true;
					m_chkCoordReverse.Visible = true;
					break;
				case PathType.Rectangle:
					m_pnlRectangle.Visible = true;
					m_chkCoordReverse.Visible = true;
					break;
				case PathType.Runway:
					m_pnlRunway.Visible = true;
					m_chkCoordReverse.Visible = true;
					break;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					m_pnlPolygon.Visible = true;
					m_chkCoordReverse.Visible = true;
					break;
				default:
					break;
			}
		}

		void SetCircleParam( double diameter, double rotatedAngle_deg )
		{
			m_numCircleDiameter.Text = diameter.ToString( "F3" );
			m_numCircleRotatedAngle.Text = rotatedAngle_deg.ToString( "F3" );
			m_CircleDiameter = diameter;
			m_CirRotatedAngle_deg = rotatedAngle_deg;
		}

		void SetRectangleParam( double length, double width, double cornerRadius, double rotatedAngle_deg )
		{
			m_numRecLength.Text = length.ToString( "F3" );
			m_numRecWidth.Text = width.ToString( "F3" );
			m_numRecCornerRadius.Text = cornerRadius.ToString( "F3" );
			m_numRecRotatedAngle.Text = rotatedAngle_deg.ToString( "F3" );
			m_RectLength = length;
			m_RectWidth = width;
			m_RectCornerRadius = cornerRadius;
			m_RectRotatedAngle_deg = rotatedAngle_deg;
		}

		void SetRunwayParam( double length, double width, double rotatedAngle_deg )
		{
			m_numRunwayLength.Text = length.ToString( "F3" );
			m_numRunwayWidth.Text = width.ToString( "F3" );
			m_numRunwayRotatedAngle.Text = rotatedAngle_deg.ToString( "F3" );
			m_RunwayLength = length;
			m_RunwayWidth = width;
			m_RunwayRotatedAngle_deg = rotatedAngle_deg;
		}

		void SetPolygonParam( double sideLength, double cornerRadius, double rotatedAngle_deg )
		{
			m_numPolygonSideLength.Text = sideLength.ToString( "F3" );
			m_numPolygonCornerRadius.Text = cornerRadius.ToString( "F3" );
			m_numPolygonRotatedAngle.Text = rotatedAngle_deg.ToString( "F3" );
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
		void m_numCircleDiameter_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetCircleDiameter();
		}

		void m_numCircleDiameter_Leave( object sender, System.EventArgs e )
		{
			SetCircleDiameter();
		}

		void SetCircleDiameter()
		{
			if( double.TryParse( m_numCircleDiameter.Text, out double diameter ) && diameter > 0 ) {
				if( !( m_NewStandardPatternGeomData is CircleGeomData circleGeomData ) ) {
					return;
				}
				m_CircleDiameter = diameter;
				circleGeomData.Diameter = m_CircleDiameter;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_numCircleDiameter.Text = m_CircleDiameter.ToString( "F3" );
			}
		}

		void m_numCircleRotatedAngle_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetCircleRotatedAngle();
		}

		void m_numCircleRotatedAngle_Leave( object sender, EventArgs e )
		{
			SetCircleRotatedAngle();
		}

		void SetCircleRotatedAngle()
		{
			if( double.TryParse( m_numCircleRotatedAngle.Text, out double angle ) ) {
				if( !( m_NewStandardPatternGeomData is CircleGeomData circleGeomData ) ) {
					return;
				}
				m_CirRotatedAngle_deg = angle;
				circleGeomData.RotatedAngle_deg = m_CirRotatedAngle_deg;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_numCircleRotatedAngle.Text = m_CirRotatedAngle_deg.ToString( "F3" );
			}
		}

		// rectangle
		void m_numRecLength_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetRectangleLength();
		}

		void m_numRecLength_Leave( object sender, System.EventArgs e )
		{
			SetRectangleLength();
		}

		void SetRectangleLength()
		{
			if( double.TryParse( m_numRecLength.Text, out double length ) && length > 0 && length > 2 * m_RectCornerRadius ) {
				if( !( m_NewStandardPatternGeomData is RectangleGeomData rectangleGeomData ) ) {
					return;
				}
				m_RectLength = length;
				rectangleGeomData.Length = m_RectLength;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_numRecLength.Text = m_RectLength.ToString( "F3" );
			}
		}

		void m_numRecWidth_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetRectangleWidth();
		}

		void m_numRecWidth_Leave( object sender, System.EventArgs e )
		{
			SetRectangleWidth();
		}

		void SetRectangleWidth()
		{
			if( double.TryParse( m_numRecWidth.Text, out double width ) && width > 0 && width > 2 * m_RectCornerRadius ) {
				if( !( m_NewStandardPatternGeomData is RectangleGeomData rectangleGeomData ) ) {
					return;
				}
				m_RectWidth = width;
				rectangleGeomData.Width = m_RectWidth;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_numRecWidth.Text = m_RectWidth.ToString( "F3" );
			}
		}

		void m_numRecCornerRadius_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetRectangleCornerRadius();
		}

		void m_numRecCornerRadius_Leave( object sender, System.EventArgs e )
		{
			SetRectangleCornerRadius();
		}

		void SetRectangleCornerRadius()
		{
			if( double.TryParse( m_numRecCornerRadius.Text, out double cornerRadius ) && cornerRadius >= 0 && 2 * cornerRadius < m_RectLength && 2 * cornerRadius < m_RectWidth ) {
				if( !( m_NewStandardPatternGeomData is RectangleGeomData rectangleGeomData ) ) {
					return;
				}
				m_RectCornerRadius = cornerRadius;
				rectangleGeomData.CornerRadius = m_RectCornerRadius;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_numRecCornerRadius.Text = m_RectCornerRadius.ToString( "F3" );
			}
		}

		void m_numRecRotatedAngle_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetRectangleRotatedAngle();
		}

		void m_numRecRotatedAngle_Leave( object sender, System.EventArgs e )
		{
			SetRectangleRotatedAngle();
		}

		void SetRectangleRotatedAngle()
		{
			if( double.TryParse( m_numRecRotatedAngle.Text, out double rotatedAngle_deg ) ) {
				if( !( m_NewStandardPatternGeomData is RectangleGeomData rectangleGeomData ) ) {
					return;
				}
				m_RectRotatedAngle_deg = rotatedAngle_deg;
				rectangleGeomData.RotatedAngle_deg = m_RectRotatedAngle_deg;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_numRecRotatedAngle.Text = m_RectRotatedAngle_deg.ToString( "F3" );
			}
		}

		// runway
		void m_numRunwayLength_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetRunwayLength();
		}

		void m_numRunwayLength_Leave( object sender, System.EventArgs e )
		{
			SetRunwayLength();
		}

		void SetRunwayLength()
		{
			if( double.TryParse( m_numRunwayLength.Text, out double length ) && length > m_RunwayWidth && length > 0 ) {
				if( !( m_NewStandardPatternGeomData is RunwayGeomData runwayGeomData ) ) {
					return;
				}
				m_RunwayLength = length;
				runwayGeomData.Length = m_RunwayLength;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_numRunwayLength.Text = m_RunwayLength.ToString( "F3" );
			}
		}

		void m_numRunwayWidth_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetRunwayWidth();
		}

		void m_numRunwayWidth_Leave( object sender, System.EventArgs e )
		{
			SetRunwayWidth();
		}

		void SetRunwayWidth()
		{
			if( double.TryParse( m_numRunwayWidth.Text, out double width ) && width > 0 && width < m_RunwayLength ) {
				if( !( m_NewStandardPatternGeomData is RunwayGeomData runwayGeomData ) ) {
					return;
				}
				m_RunwayWidth = width;
				runwayGeomData.Width = m_RunwayWidth;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_numRunwayWidth.Text = m_RunwayWidth.ToString( "F3" );
			}
		}

		void m_numRunwayRotatedAngle_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetRunwayRotatedAngle();
		}

		void m_numRunwayRotatedAngle_Leave( object sender, System.EventArgs e )
		{
			SetRunwayRotatedAngle();
		}

		void SetRunwayRotatedAngle()
		{
			if( double.TryParse( m_numRunwayRotatedAngle.Text, out double rotatedAngle_deg ) ) {
				if( !( m_NewStandardPatternGeomData is RunwayGeomData runwayGeomData ) ) {
					return;
				}
				m_RunwayRotatedAngle_deg = rotatedAngle_deg;
				runwayGeomData.RotatedAngle_deg = m_RunwayRotatedAngle_deg;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_numRunwayRotatedAngle.Text = m_RunwayRotatedAngle_deg.ToString( "F3" );
			}
		}

		// polygon
		void m_numPolygonSideLength_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetPolygonSideLength();
		}

		void m_numPolygonSideLength_Leave( object sender, System.EventArgs e )
		{
			SetPolygonSideLength();
		}

		void SetPolygonSideLength()
		{
			if( double.TryParse( m_numPolygonSideLength.Text, out double sideLength ) && sideLength > 0 && sideLength > 2 * m_PolygonCornerRadius ) {
				if( !( m_NewStandardPatternGeomData is PolygonGeomData polygonGeomData ) ) {
					return;
				}

				// validate that the edge length is sufficient for the corner radius
				if( m_PolygonCornerRadius > 0 && !IsPolygonGeometryValid( polygonGeomData.Sides, sideLength, m_PolygonCornerRadius ) ) {
					m_numPolygonSideLength.Text = m_PolygonSideLength.ToString( "F3" );
					return;
				}

				m_PolygonSideLength = sideLength;
				polygonGeomData.SideLength = m_PolygonSideLength;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_numPolygonSideLength.Text = m_PolygonSideLength.ToString( "F3" );
			}
		}

		void m_numPolygonRotatedAngle_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetPolygonRotatedAngle();
		}

		void m_numPolygonRotatedAngle_Leave( object sender, System.EventArgs e )
		{
			SetPolygonRotatedAngle();
		}

		void SetPolygonRotatedAngle()
		{
			if( double.TryParse( m_numPolygonRotatedAngle.Text, out double rotatedAngle_deg ) ) {
				if( !( m_NewStandardPatternGeomData is PolygonGeomData polygonGeomData ) ) {
					return;
				}
				m_PolygonRotatedAngle_deg = rotatedAngle_deg;
				polygonGeomData.RotatedAngle_deg = m_PolygonRotatedAngle_deg;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_numPolygonRotatedAngle.Text = m_PolygonRotatedAngle_deg.ToString( "F3" );
			}
		}

		void m_numPolygonCornerRadius_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetPolygonCornerRadius();
		}

		void m_numPolygonCornerRadius_Leave( object sender, System.EventArgs e )
		{
			SetPolygonCornerRadius();
		}

		void SetPolygonCornerRadius()
		{
			if( double.TryParse( m_numPolygonCornerRadius.Text, out double cornerRadius ) && cornerRadius >= 0 && 2 * cornerRadius < m_PolygonSideLength ) {
				if( !( m_NewStandardPatternGeomData is PolygonGeomData polygonGeomData ) ) {
					return;
				}

				// validate that the corner radius is compatible with the edge length
				if( cornerRadius > 0 && !IsPolygonGeometryValid( polygonGeomData.Sides, m_PolygonSideLength, cornerRadius ) ) {
					m_numPolygonCornerRadius.Text = m_PolygonCornerRadius.ToString( "F3" );
					return;
				}

				m_PolygonCornerRadius = cornerRadius;
				polygonGeomData.CornerRadius = m_PolygonCornerRadius;
				RaisePreview( m_NewStandardPatternGeomData );
			}
			else {
				m_numPolygonCornerRadius.Text = m_PolygonCornerRadius.ToString( "F3" );
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

		readonly LanguageManager.LanguageManager m_LanguageManager;

		const string PATH_TYPE_CONTOUR = "輪廓";
		const string PATH_TYPE_CIRCLE = "圓形";
		const string PATH_TYPE_RECTANGLE = "長方形";
		const string PATH_TYPE_RUNWAY = "跑道形";
		const string PATH_TYPE_TRIANGLE = "三角形";
		const string PATH_TYPE_SQUARE = "正方形";
		const string PATH_TYPE_PENTAGON = "五角形";
		const string PATH_TYPE_HEXAGON = "六角形";
	}
}
