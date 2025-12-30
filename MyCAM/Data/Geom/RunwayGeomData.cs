namespace MyCAM.Data
{
	public class RunwayGeomData : StdPatternGeomDataBase
	{
		public RunwayGeomData( double length, double width, double rotatedAngle_deg, bool isCoordinateReversed )
		{
			m_Length = length;
			m_Width = width;
			m_RotatedAngle_deg = rotatedAngle_deg;
			m_IsCoordinateReversed = isCoordinateReversed;
		}

		public RunwayGeomData()
		{
			m_Length = DEFAULT_LENGTH;
			m_Width = DEFAULT_WIDTH;
			m_RotatedAngle_deg = DEFAULT_ROTATED_ANGLE;
			m_IsCoordinateReversed = false;
		}

		public override PathType PathType
		{
			get
			{
				return PathType.Runway;
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

		public override IGeomData Clone()
		{
			return new RunwayGeomData( m_Length, m_Width, m_RotatedAngle_deg, m_IsCoordinateReversed );
		}

		public const double DEFAULT_LENGTH = 40.0;
		public const double DEFAULT_WIDTH = 20.0;
		public const double DEFAULT_ROTATED_ANGLE = 0.0;
		double m_Length;
		double m_Width;
	}
}
