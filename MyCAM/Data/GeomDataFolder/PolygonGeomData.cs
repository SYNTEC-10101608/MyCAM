namespace MyCAM.Data
{
	public class PolygonGeomData : IStandardPatternGeomData
	{
		public PolygonGeomData( int sides, double sideLength, double cornerRadius, double rotatedAngle_deg )
		{
			if( sides < 3 || sides > 6 ) {
				throw new System.ArgumentOutOfRangeException( nameof( sides ), "Polygon must have 3 to 6 sides" );
			}
			Sides = sides;
			m_SideLength = sideLength;
			m_CornerRadius = cornerRadius;
			m_RotatedAngle_deg = rotatedAngle_deg;
		}

		public PolygonGeomData( int sides )
		{
			if( sides < 3 || sides > 6 ) {
				throw new System.ArgumentOutOfRangeException( nameof( sides ), "Polygon must have 3 to 6 sides" );
			}
			Sides = sides;
			m_SideLength = DEFAULT_SIDE_LENGTH;
			m_CornerRadius = DEFAULT_CORNER_RADIUS;
			m_RotatedAngle_deg = DEFAULT_ROTATED_ANGLE;
		}

		public PathType PathType
		{
			get
			{
				switch( Sides ) {
					case 3:
						return PathType.Triangle;
					case 4:
						return PathType.Square;
					case 5:
						return PathType.Pentagon;
					case 6:
						return PathType.Hexagon;
					default:
						throw new System.InvalidOperationException( $"Unsupported number of sides: {Sides}" );
				}
			}
		}

		public int Sides
		{
			get;
			private set;
		}

		public double SideLength
		{
			get
			{
				return m_SideLength;
			}
			set
			{
				m_SideLength = value;
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
			return new PolygonGeomData( Sides, m_SideLength, m_CornerRadius, m_RotatedAngle_deg );
		}

		public const int DEFAULT_SIDES = 3;
		public const double DEFAULT_SIDE_LENGTH = 20.0;
		public const double DEFAULT_CORNER_RADIUS = 2.0;
		public const double DEFAULT_ROTATED_ANGLE = 0.0;
		double m_SideLength;
		double m_CornerRadius;
		double m_RotatedAngle_deg;
	}
}
