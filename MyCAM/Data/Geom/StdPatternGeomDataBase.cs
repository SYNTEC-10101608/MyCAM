using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public abstract class StdPatternGeomDataBase : IStdPatternGeomData
	{
		public abstract PathType PathType
		{
			get;
		}

		public event Action CADFactorChanged;

		public gp_Ax1 RefCenterDir
		{
			get
			{
				return m_RefCenterDir;
			}
			set
			{
				// this only be set once when constructing GeomData, if it's changed later, it means the pattern is changed to another one, which is not supported currently, so no need to trigger CADFactorChanged event
				m_RefCenterDir = value;
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
				if( m_RotatedAngle_deg != value ) {
					m_RotatedAngle_deg = value;
					CADFactorChanged?.Invoke();
				}
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
				if( m_IsCoordinateReversed != value ) {
					m_IsCoordinateReversed = value;
					CADFactorChanged?.Invoke();
				}
			}
		}

		public virtual void DoTransform( gp_Trsf transform )
		{
			// currently no implementation
		}

		public abstract IGeomData Clone();

		protected void OnCADFactorChanged()
		{
			CADFactorChanged?.Invoke();
		}

		protected double m_RotatedAngle_deg;
		protected bool m_IsCoordinateReversed;
		protected List<CADPoint> m_CADPointList;
		protected gp_Ax1 m_RefCenterDir = new gp_Ax1();
	}
}
