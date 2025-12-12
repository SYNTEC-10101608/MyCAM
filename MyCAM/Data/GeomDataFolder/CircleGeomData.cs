namespace MyCAM.Data
{
	public class CircleGeomData : IStandardPatternGeomData
	{
		public CircleGeomData( double diameter, double rotatedAngle_deg )
		{
			m_Diameter = diameter;
			m_RotatedAngle_deg = rotatedAngle_deg;
		}

		public CircleGeomData()
		{
			m_Diameter = DEFAULT_DIAMETER;
			m_RotatedAngle_deg = DEFAULT_ROTATED_ANGLE;
		}

		public PathType PathType
		{
			get
			{
				return PathType.Circle;
			}
		}

		public double Diameter
		{
			get
			{
				return m_Diameter;
			}
			set
			{
				m_Diameter = value;
			}
		}

		public double RotatedAngle_deg
		{
			get
			{
				return m_RotatedAngle_deg;
			}
			set
			{
				m_RotatedAngle_deg = value;
			}
		}

		public bool IsClosed
		{
			get
			{
				return true;
			}
		}

		public IGeomData Clone()
		{
			return new CircleGeomData( m_Diameter, m_RotatedAngle_deg );
		}

		public const double DEFAULT_DIAMETER = 20.0;
		public const double DEFAULT_ROTATED_ANGLE = 0.0;
		double m_Diameter;
		double m_RotatedAngle_deg = 0;
	}
}
