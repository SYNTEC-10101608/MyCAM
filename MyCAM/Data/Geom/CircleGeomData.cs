namespace MyCAM.Data
{
	public class CircleGeomData : StdPatternGeomDataBase
	{
		public CircleGeomData( double diameter, double rotatedAngle_deg, bool isCoordinateReversed )
		{
			m_Diameter = diameter;
			m_RotatedAngle_deg = rotatedAngle_deg;
			m_IsCoordinateReversed = isCoordinateReversed;
		}

		public CircleGeomData()
		{
			m_Diameter = DEFAULT_DIAMETER;
			m_RotatedAngle_deg = DEFAULT_ROTATED_ANGLE;
			m_IsCoordinateReversed = false;
		}

		public override PathType PathType
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

		public override IGeomData Clone()
		{
			return new CircleGeomData( m_Diameter, m_RotatedAngle_deg, m_IsCoordinateReversed );
		}

		public const double DEFAULT_DIAMETER = 20.0;
		public const double DEFAULT_ROTATED_ANGLE = 0.0;
		double m_Diameter;
	}
}
