using OCC.gp;

namespace MyCAM.Data
{
	public class CircleGeomData : StdPatternGeomDataBase
	{
		public CircleGeomData( gp_Ax1 refCenterDir, double diameter, double rotatedAngle_deg, bool isCoordinateReversed )
		{
			m_RefCenterDir = refCenterDir;
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
				if( m_Diameter != value ) {
					m_Diameter = value;
					OnCADFactorChanged();
				}
			}
		}

		public override IGeomData Clone()
		{
			return new CircleGeomData( m_RefCenterDir, m_Diameter, m_RotatedAngle_deg, m_IsCoordinateReversed );
		}

		public const double DEFAULT_DIAMETER = 20.0;
		public const double DEFAULT_ROTATED_ANGLE = 0.0;
		double m_Diameter;
	}
}
