using OCC.gp;

namespace MyCAM.Data
{
	public abstract class StdPatternGeomDataBase : IStdPatternGeomData
	{
		public abstract PathType PathType
		{
			get;
		}

		public bool IsClosed
		{
			get
			{
				return true;
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

		public gp_Pnt CenterPnt
		{
			get
			{
				return new gp_Pnt( m_OriCenterPnt.XYZ() );
			}
		}

		public gp_Dir AverageNormalDir
		{
			get
			{
				return new gp_Dir( m_AverageNormalDir.XYZ() );
			}
		}

		public virtual void DoTransform( gp_Trsf transform )
		{
			m_OriCenterPnt.Transform( transform );
			m_AverageNormalDir.Transform( transform );
		}

		public abstract IGeomData Clone();

		protected double m_RotatedAngle_deg;
		gp_Pnt m_OriCenterPnt;
		gp_Dir m_AverageNormalDir;
	}
}
