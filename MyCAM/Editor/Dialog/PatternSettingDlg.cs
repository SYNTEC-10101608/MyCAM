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
			switch( initialPathType ) {
				case PathType.Contour:
					HideAllPanels();
					m_cmbPathType.SelectedIndex = 0;
					break;
				case PathType.Circle:
					ShowSpecificPanel( initialPathType );
					m_txbCircleDiameter.Text = ( (CircleGeomData)patternSettingInfo.GeomData ).Diameter.ToString();
					m_txbCircleRotatedAngle.Text = ( (CircleGeomData)patternSettingInfo.GeomData ).RotatedAngle_deg.ToString();
					m_cmbPathType.SelectedIndex = 1;
					break;
				case PathType.Rectangle:
					ShowSpecificPanel( initialPathType );
					m_txbRecLength.Text = ( (RectangleGeomData)patternSettingInfo.GeomData ).Length.ToString();
					m_txbRecWidth.Text = ( (RectangleGeomData)patternSettingInfo.GeomData ).Width.ToString();
					m_txbRecCornerRadius.Text = ( (RectangleGeomData)patternSettingInfo.GeomData ).CornerRadius.ToString();
					m_txbRecRotatedAngle.Text = ( (RectangleGeomData)patternSettingInfo.GeomData ).RotatedAngle_deg.ToString();
					m_cmbPathType.SelectedIndex = 2;
					break;
				default:
					break;
			}
		}

		void m_cmbPathType_SelectedIndexChanged( object sender, EventArgs e )
		{
			m_NewPatternSettingInfoList = new List<PatternSettingInfo>();
			switch( m_cmbPathType.SelectedIndex ) {
				case 0:
					HideAllPanels();
					for( int i = 0; i < m_PatternSettingInfoList.Count; i++ ) {
						m_NewPatternSettingInfoList.Add( new PatternSettingInfo( m_PatternSettingInfoList[ i ].ContourPathObject.ContourGeomData, m_PatternSettingInfoList[ i ].ContourPathObject ) );
					}
					break;
				case 1:
					ShowSpecificPanel( PathType.Circle );
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
					ShowSpecificPanel( PathType.Rectangle );
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

		void ShowSpecificPanel( PathType pathType )
		{
			HideAllPanels();
			switch( pathType ) {
				case PathType.Circle:
					m_panelCircle.Visible = true;
					break;
				case PathType.Rectangle:
					m_panelRectangle.Visible = true;
					break;
				case PathType.Runway:
					m_panelRunway.Visible = true;
					break;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
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
