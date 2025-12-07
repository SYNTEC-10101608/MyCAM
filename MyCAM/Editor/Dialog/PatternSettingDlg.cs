using MyCAM.Data;
using MyCAM.Data.GeomDataFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
{
	public partial class PatternSettingDlg : EditDialogBase<List<PatternSettingInfo>>
	{
		public PatternSettingDlg( List<PatternSettingInfo> patternSettingInfoList )
		{
			if( patternSettingInfoList == null || patternSettingInfoList.Count == 0 || patternSettingInfoList.Any( info => info.GeomData == null || info.ContourPathObject == null ) ) {
				throw new ArgumentNullException( "PatternSettingDlg constructing argument null" );
			}
			m_PatternSettingInfoList = patternSettingInfoList;
			InitializeComponent();
			InitializeControlsValue( patternSettingInfoList[ 0 ] );
		}

		void InitializeControlsValue( PatternSettingInfo patternSettingInfo )
		{
			PathType initialPathType = patternSettingInfo.GeomData.PathType;
			m_cmbPathType.SelectedIndex = GetComboIndexFromPathType( initialPathType );
			ShowSpecificPanel( m_cmbPathType.SelectedIndex );
			switch( initialPathType ) {
				case PathType.Contour:
					m_cmbPathType.SelectedIndex = 0;
					break;
				case PathType.Circle:
					m_txbCircleDiameter.Text = ( (CircleGeomData)patternSettingInfo.GeomData ).Diameter.ToString();
					m_txbCircleRotatedAngle.Text = ( (CircleGeomData)patternSettingInfo.GeomData ).RotatedAngle_deg.ToString();
					break;
				case PathType.Rectangle:
					m_txbRecLength.Text = ( (RectangleGeomData)patternSettingInfo.GeomData ).Length.ToString();
					m_txbRecWidth.Text = ( (RectangleGeomData)patternSettingInfo.GeomData ).Width.ToString();
					m_txbRecCornerRadius.Text = ( (RectangleGeomData)patternSettingInfo.GeomData ).CornerRadius.ToString();
					m_txbRecRotatedAngle.Text = ( (RectangleGeomData)patternSettingInfo.GeomData ).RotatedAngle_deg.ToString();
					break;
				case PathType.Runway:
					m_txbRunwayLength.Text = ( (RunwayGeomData)patternSettingInfo.GeomData ).Length.ToString();
					m_txbRunwayWidth.Text = ( (RunwayGeomData)patternSettingInfo.GeomData ).Width.ToString();
					m_txbRunwayRotatedAngle.Text = ( (RunwayGeomData)patternSettingInfo.GeomData ).RotatedAngle_deg.ToString();
					break;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					PolygonGeomData polygonData = (PolygonGeomData)patternSettingInfo.GeomData;
					m_txbPolygonSideLength.Text = polygonData.SideLength.ToString();
					m_txbPolygonCornerRadius.Text = polygonData.CornerRadius.ToString();
					m_txbPolygonRotatedAngle.Text = polygonData.RotatedAngle_deg.ToString();
					break;
				default:
					break;
			}
		}

		void m_cmbPathType_SelectedIndexChanged( object sender, EventArgs e )
		{
			m_NewPatternSettingInfoList = new List<PatternSettingInfo>();
			ShowSpecificPanel( m_cmbPathType.SelectedIndex );
			switch( m_cmbPathType.SelectedIndex ) {
				case 0:
					for( int i = 0; i < m_PatternSettingInfoList.Count; i++ ) {
						m_NewPatternSettingInfoList.Add( new PatternSettingInfo( m_PatternSettingInfoList[ i ].ContourPathObject.ContourGeomData, m_PatternSettingInfoList[ i ].ContourPathObject ) );
					}
					break;
				case 1:
					if( m_PatternSettingInfoList[ 0 ].GeomData.PathType != PathType.Circle ) {
						for( int i = 0; i < m_PatternSettingInfoList.Count; i++ ) {
							m_NewPatternSettingInfoList.Add( new PatternSettingInfo( new CircleGeomData( m_PatternSettingInfoList[ i ].GeomData.UID ), m_PatternSettingInfoList[ i ].ContourPathObject ) );
						}
					}
					else {
						for( int i = 0; i < m_PatternSettingInfoList.Count; i++ ) {
							m_NewPatternSettingInfoList.Add( new PatternSettingInfo( m_PatternSettingInfoList[ 0 ].GeomData, m_PatternSettingInfoList[ i ].ContourPathObject ) );
						}
					}
					CircleGeomData circleGeomData = (CircleGeomData)m_NewPatternSettingInfoList[ 0 ].GeomData;
					SetCircleParam( circleGeomData.Diameter, circleGeomData.RotatedAngle_deg );
					break;
				case 2:
					if( m_PatternSettingInfoList[ 0 ].GeomData.PathType != PathType.Rectangle ) {
						for( int i = 0; i < m_PatternSettingInfoList.Count; i++ ) {
							m_NewPatternSettingInfoList.Add( new PatternSettingInfo( new RectangleGeomData( m_PatternSettingInfoList[ i ].GeomData.UID ), m_PatternSettingInfoList[ i ].ContourPathObject ) );
						}
					}
					else {
						for( int i = 0; i < m_PatternSettingInfoList.Count; i++ ) {
							m_NewPatternSettingInfoList.Add( new PatternSettingInfo( m_PatternSettingInfoList[ 0 ].GeomData, m_PatternSettingInfoList[ i ].ContourPathObject ) );
						}
					}
					RectangleGeomData rectGeomData = (RectangleGeomData)m_NewPatternSettingInfoList[ 0 ].GeomData;
					SetRectangleParam( rectGeomData.Length, rectGeomData.Width, rectGeomData.CornerRadius, rectGeomData.RotatedAngle_deg );
					break;
				case 3:
					if( m_PatternSettingInfoList[ 0 ].GeomData.PathType != PathType.Runway ) {
						for( int i = 0; i < m_PatternSettingInfoList.Count; i++ ) {
							m_NewPatternSettingInfoList.Add( new PatternSettingInfo( new RunwayGeomData( m_PatternSettingInfoList[ i ].GeomData.UID ), m_PatternSettingInfoList[ i ].ContourPathObject ) );
						}
					}
					else {
						for( int i = 0; i < m_PatternSettingInfoList.Count; i++ ) {
							m_NewPatternSettingInfoList.Add( new PatternSettingInfo( m_PatternSettingInfoList[ 0 ].GeomData, m_PatternSettingInfoList[ i ].ContourPathObject ) );
						}
					}
					RunwayGeomData runwayGeomData = (RunwayGeomData)m_NewPatternSettingInfoList[ 0 ].GeomData;
					SetRunwayParam( runwayGeomData.Length, runwayGeomData.Width, runwayGeomData.RotatedAngle_deg );
					break;
				case 4:
				case 5:
				case 6:
				case 7:
					if( !IsPolygonSide( m_PatternSettingInfoList[ 0 ].GeomData.PathType, out int nSides ) ) {
						for( int i = 0; i < m_PatternSettingInfoList.Count; i++ ) {
							m_NewPatternSettingInfoList.Add( new PatternSettingInfo( new PolygonGeomData( m_PatternSettingInfoList[ i ].GeomData.UID, nSides ), m_PatternSettingInfoList[ i ].ContourPathObject ) );
						}
					}
					else {
						for( int i = 0; i < m_PatternSettingInfoList.Count; i++ ) {
							m_NewPatternSettingInfoList.Add( new PatternSettingInfo( m_PatternSettingInfoList[ 0 ].GeomData, m_PatternSettingInfoList[ i ].ContourPathObject ) );
						}
					}
					PolygonGeomData polygonGeomData = (PolygonGeomData)m_NewPatternSettingInfoList[ 0 ].GeomData;
					SetPolygonParam( polygonGeomData.SideLength, polygonGeomData.CornerRadius, polygonGeomData.RotatedAngle_deg );
					break;
				default:

					break;
			}
			RaisePreview( m_NewPatternSettingInfoList );
		}

		void HideAllPanels()
		{
			m_panelRectangle.Visible = false;
			m_panelCircle.Visible = false;
			m_panelRunway.Visible = false;
			m_panelPolygon.Visible = false;
		}

		void ShowSpecificPanel( int nIndex )
		{
			HideAllPanels();
			switch( nIndex ) {
				case 0:
				default:
					break;
				case 1:
					m_panelCircle.Visible = true;
					break;
				case 2:
					m_panelRectangle.Visible = true;
					break;
				case 3:
					m_panelRunway.Visible = true;
					break;
				case 4:
				case 5:
				case 6:
				case 7:
					m_panelPolygon.Visible = true;
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
			int nIndexOffset = 1;
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
					nSides = m_cmbPathType.SelectedIndex - nIndexOffset;
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
			if( double.TryParse( m_txbCircleDiameter.Text, out double diameter ) && diameter != m_CircleDiameter && diameter > 0 ) {
				m_CircleDiameter = diameter;
				foreach( PatternSettingInfo settingInfo in m_NewPatternSettingInfoList ) {
					if( settingInfo.GeomData is CircleGeomData circleData ) {
						circleData.Diameter = m_CircleDiameter;
					}
				}
				RaisePreview( m_NewPatternSettingInfoList );
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
			if( double.TryParse( m_txbCircleRotatedAngle.Text, out double angle ) && angle != m_CirRotatedAngle_deg ) {
				m_CirRotatedAngle_deg = angle;
				foreach( PatternSettingInfo settingInfo in m_NewPatternSettingInfoList ) {
					if( settingInfo.GeomData is CircleGeomData circleData ) {
						circleData.RotatedAngle_deg = m_CirRotatedAngle_deg;
					}
				}
				RaisePreview( m_NewPatternSettingInfoList );
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
			if( double.TryParse( m_txbRecLength.Text, out double length ) && length != m_RectLength && length > 0 ) {
				m_RectLength = length;
				foreach( PatternSettingInfo settingInfo in m_NewPatternSettingInfoList ) {
					if( settingInfo.GeomData is RectangleGeomData rectangleGeomData ) {
						rectangleGeomData.Length = m_RectLength;
					}
				}
				RaisePreview( m_NewPatternSettingInfoList );
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
			if( double.TryParse( m_txbRecWidth.Text, out double width ) && width != m_RectWidth && width > 0 ) {
				m_RectWidth = width;
				foreach( PatternSettingInfo settingInfo in m_NewPatternSettingInfoList ) {
					if( settingInfo.GeomData is RectangleGeomData rectangleGeomData ) {
						rectangleGeomData.Width = m_RectWidth;
					}
				}
				RaisePreview( m_NewPatternSettingInfoList );
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
			if( double.TryParse( m_txbRecCornerRadius.Text, out double cornerRadius ) && cornerRadius != m_RectCornerRadius && cornerRadius >= 0 ) {
				m_RectCornerRadius = cornerRadius;
				foreach( PatternSettingInfo settingInfo in m_NewPatternSettingInfoList ) {
					if( settingInfo.GeomData is RectangleGeomData rectangleGeomData ) {
						rectangleGeomData.CornerRadius = m_RectCornerRadius;
					}
				}
				RaisePreview( m_NewPatternSettingInfoList );
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
			if( double.TryParse( m_txbRecRotatedAngle.Text, out double rotatedAngle_deg ) && rotatedAngle_deg != m_RectRotatedAngle_deg ) {
				m_RectRotatedAngle_deg = rotatedAngle_deg;
				foreach( PatternSettingInfo settingInfo in m_NewPatternSettingInfoList ) {
					if( settingInfo.GeomData is RectangleGeomData rectangleGeomData ) {
						rectangleGeomData.RotatedAngle_deg = m_RectRotatedAngle_deg;
					}
				}
				RaisePreview( m_NewPatternSettingInfoList );
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
			if( double.TryParse( m_txbRunwayLength.Text, out double length ) && length != m_RunwayLength && length > m_RunwayWidth ) {
				m_RunwayLength = length;
				foreach( PatternSettingInfo settingInfo in m_NewPatternSettingInfoList ) {
					if( settingInfo.GeomData is RunwayGeomData runwayGeomData ) {
						runwayGeomData.Length = m_RunwayLength;
					}
				}
				RaisePreview( m_NewPatternSettingInfoList );
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
			if( double.TryParse( m_txbRunwayWidth.Text, out double width ) && width != m_RunwayWidth && width > 0 && width < m_RunwayLength ) {
				m_RunwayWidth = width;
				foreach( PatternSettingInfo settingInfo in m_NewPatternSettingInfoList ) {
					if( settingInfo.GeomData is RunwayGeomData runwayGeomData ) {
						runwayGeomData.Width = m_RunwayWidth;
					}
				}
				RaisePreview( m_NewPatternSettingInfoList );
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
			if( double.TryParse( m_txbRunwayRotatedAngle.Text, out double rotatedAngle_deg ) && rotatedAngle_deg != m_RunwayRotatedAngle_deg ) {
				m_RunwayRotatedAngle_deg = rotatedAngle_deg;
				foreach( PatternSettingInfo settingInfo in m_NewPatternSettingInfoList ) {
					if( settingInfo.GeomData is RunwayGeomData runwayGeomData ) {
						runwayGeomData.RotatedAngle_deg = m_RunwayRotatedAngle_deg;
					}
				}
				RaisePreview( m_NewPatternSettingInfoList );
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
			if( double.TryParse( m_txbPolygonSideLength.Text, out double sideLength ) && sideLength != m_PolygonSideLength && sideLength > 0 ) {
				m_PolygonSideLength = sideLength;
				foreach( PatternSettingInfo settingInfo in m_NewPatternSettingInfoList ) {
					if( settingInfo.GeomData is PolygonGeomData polygonGeomData ) {
						polygonGeomData.SideLength = m_PolygonSideLength;
					}
				}
				RaisePreview( m_NewPatternSettingInfoList );
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
			if( double.TryParse( m_txbPolygonRotatedAngle.Text, out double rotatedAngle_deg ) && rotatedAngle_deg != m_PolygonRotatedAngle_deg ) {
				m_PolygonRotatedAngle_deg = rotatedAngle_deg;
				foreach( PatternSettingInfo settingInfo in m_NewPatternSettingInfoList ) {
					if( settingInfo.GeomData is PolygonGeomData polygonGeomData ) {
						polygonGeomData.RotatedAngle_deg = m_PolygonRotatedAngle_deg;
					}
				}
				RaisePreview( m_NewPatternSettingInfoList );
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
			if( double.TryParse( m_txbPolygonCornerRadius.Text, out double cornerRadius ) && cornerRadius != m_PolygonCornerRadius && cornerRadius >= 0 ) {
				m_PolygonCornerRadius = cornerRadius;
				foreach( PatternSettingInfo settingInfo in m_NewPatternSettingInfoList ) {
					if( settingInfo.GeomData is PolygonGeomData polygonGeomData ) {
						polygonGeomData.CornerRadius = m_PolygonCornerRadius;
					}
				}
				RaisePreview( m_NewPatternSettingInfoList );
			}
			else {
				m_txbPolygonCornerRadius.Text = m_PolygonCornerRadius.ToString( "F3" );
			}
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			RaiseConfirm( m_NewPatternSettingInfoList );
		}

		List<PatternSettingInfo> m_PatternSettingInfoList;
		List<PatternSettingInfo> m_NewPatternSettingInfoList;

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


	public class PatternSettingInfo
	{
		public PatternSettingInfo( IGeomData geomData, ContourPathObject contourPathObject )
		{
			if( geomData == null || contourPathObject == null ) {
				throw new ArgumentNullException( "PatternSettingInfo constructing argument null" );
			}
			GeomData = geomData;
			ContourPathObject = contourPathObject;
		}

		public IGeomData GeomData
		{
			get;
			private set;
		}

		public ContourPathObject ContourPathObject
		{
			get; private set;
		}

		public PatternSettingInfo Clone()
		{
			return new PatternSettingInfo( GeomData, ContourPathObject );
		}
	}
}
