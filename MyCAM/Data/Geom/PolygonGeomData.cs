using OCC.gp;

namespace MyCAM.Data
{
	public class PolygonGeomData : StdPatternGeomDataBase
	{
		public PolygonGeomData( gp_Ax1 refCenterDir, int sides, double sideLength, double cornerRadius, double rotatedAngle_deg, bool isCoordinateReversed )
		{
			if( sides < 3 || sides > 6 ) {
				throw new System.ArgumentOutOfRangeException( nameof( sides ), "Polygon must have 3 to 6 sides" );
			}
			m_RefCenterDir = refCenterDir;
			m_Sides = sides;
			m_SideLength = sideLength;
			m_CornerRadius = cornerRadius;
			m_RotatedAngle_deg = rotatedAngle_deg;
			m_IsCoordinateReversed = isCoordinateReversed;
		}

		public PolygonGeomData( int sides )
		{
			if( sides < 3 || sides > 6 ) {
				throw new System.ArgumentOutOfRangeException( nameof( sides ), "Polygon must have 3 to 6 sides" );
			}
			m_Sides = sides;
			m_SideLength = DEFAULT_SIDE_LENGTH;
			m_CornerRadius = DEFAULT_CORNER_RADIUS;
			m_RotatedAngle_deg = DEFAULT_ROTATED_ANGLE;
			m_IsCoordinateReversed = false;
		}

		public override PathType PathType
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
			get
			{
				return m_Sides;
			}
			set
			{
				if( m_Sides != value ) {
					if( value < 3 || value > 6 ) {
						throw new System.ArgumentOutOfRangeException( nameof( Sides ), "Polygon must have 3 to 6 sides" );
					}
					m_Sides = value;
					OnCADFactorChanged();
				}
			}
		}

		public double SideLength
		{
			get
			{
				return m_SideLength;
			}
			set
			{
				if( m_SideLength != value ) {
					m_SideLength = value;
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
			return new PolygonGeomData( m_RefCenterDir, Sides, m_SideLength, m_CornerRadius, m_RotatedAngle_deg, m_IsCoordinateReversed );
		}

		public const int DEFAULT_SIDES = 3;
		public const double DEFAULT_SIDE_LENGTH = 20.0;
		public const double DEFAULT_CORNER_RADIUS = 2.0;
		public const double DEFAULT_ROTATED_ANGLE = 0.0;
		double m_SideLength;
		double m_CornerRadius;
		int m_Sides;
	}
}
