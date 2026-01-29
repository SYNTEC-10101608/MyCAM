using OCC.gp;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public abstract class StdPatternGeomDataBase : IStdPatternGeomData
	{
		public abstract PathType PathType
		{
			get;
		}

		public gp_Ax3 RefCoord
		{
			get
			{
				return m_RefCoord;
			}
			set
			{
				m_RefCoord = value;
			}
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

		public void SetRefCoord( gp_Ax3 refCoord )
		{
			m_RefCoord = refCoord;
		}

		protected double m_RotatedAngle_deg;
		protected bool m_IsCoordinateReversed;
		protected List<CADPoint> m_CADPointList;
		protected gp_Ax3 m_RefCoord = new gp_Ax3();
	}
}
