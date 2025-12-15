namespace MyCAM.Data
{
	public class RectangleGeomData : StdPatternGeomDataBase
	{
		public RectangleGeomData( double width, double height, double cornerRadius, double rotatedAngle_deg )
		{
			m_Width = width;
			m_Length = height;
			m_CornerRadius = cornerRadius;
			m_RotatedAngle_deg = rotatedAngle_deg;
		}

		public RectangleGeomData()
		{
			m_Width = DEFAULT_WIDTH;
			m_Length = DEFAULT_LENGTH;
			m_CornerRadius = DEFAULT_CORNER_RADIUS;
			m_RotatedAngle_deg = DEFAULT_ROTATED_ANGLE;
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
				m_Width = value;
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
				m_Length = value;
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
				m_CornerRadius = value;
			}
		}

		public override IGeomData Clone()
		{
			return new RectangleGeomData( m_Width, m_Length, m_CornerRadius, m_RotatedAngle_deg );
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
