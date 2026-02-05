using OCC.gp;

namespace MyCAM.Data
{
	public class RectangleGeomData : StdPatternGeomDataBase
	{
		public RectangleGeomData( gp_Ax1 refCenterDir, double width, double height, double cornerRadius, double rotatedAngle_deg, bool isCoordinateReversed )
		{
			m_RefCenterDir = refCenterDir;
			m_Width = width;
			m_Length = height;
			m_CornerRadius = cornerRadius;
			m_RotatedAngle_deg = rotatedAngle_deg;
			m_IsCoordinateReversed = isCoordinateReversed;
		}

		public RectangleGeomData()
		{
			m_Width = DEFAULT_WIDTH;
			m_Length = DEFAULT_LENGTH;
			m_CornerRadius = DEFAULT_CORNER_RADIUS;
			m_RotatedAngle_deg = DEFAULT_ROTATED_ANGLE;
			m_IsCoordinateReversed = false;
		}

		public override PathType PathType
		{
			get
			{
				return PathType.Rectangle;
			}
		}

		public double Width
		{
			get
			{
				return m_Width;
			}
			set
			{
				if( m_Width != value ) {
					m_Width = value;
					OnCADFactorChanged();
				}
			}
		}

		public double Length
		{
			get
			{
				return m_Length;
			}
			set
			{
				if( m_Length != value ) {
					m_Length = value;
					OnCADFactorChanged();
				}
			}
		}

		public double CornerRadius
		{
			get
			{
				return m_CornerRadius;
			}
			set
			{
				if( m_CornerRadius != value ) {
					m_CornerRadius = value;
					OnCADFactorChanged();
				}
			}
		}

		public override IGeomData Clone()
		{
			return new RectangleGeomData( m_RefCenterDir, m_Width, m_Length, m_CornerRadius, m_RotatedAngle_deg, m_IsCoordinateReversed );
		}

		public const double DEFAULT_CORNER_RADIUS = 5.0;
		public const double DEFAULT_ROTATED_ANGLE = 0.0;
		public const double DEFAULT_WIDTH = 20.0;
		public const double DEFAULT_LENGTH = 30.0;
		double m_Width;
		double m_Length;
		double m_CornerRadius;
	}
}
