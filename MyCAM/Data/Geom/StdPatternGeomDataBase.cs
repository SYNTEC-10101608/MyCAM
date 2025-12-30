using OCC.gp;

namespace MyCAM.Data
{
	// TODO: the derived construct should check input parameters itself
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

		public bool IsCoordinateReversed
		{
			get
			{
				return m_IsCoordinateReversed;
			}
			set
			{
				m_IsCoordinateReversed = value;
			}
		}

		public virtual void DoTransform( gp_Trsf transform )
		{
			// currently no implementation
		}

		public abstract IGeomData Clone();

		protected double m_RotatedAngle_deg;
		protected bool m_IsCoordinateReversed;
	}
}
