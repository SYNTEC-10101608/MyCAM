using OCC.gp;

namespace MyCAM.Data
{
	public class CADPoint
	{
		public CADPoint( gp_Pnt point, gp_Dir normalVec_1st, gp_Dir normalVec_2nd, gp_Dir tangentVec )
		{
			m_Point = new gp_Pnt( point.X(), point.Y(), point.Z() );
			m_NormalVec_1st = new gp_Dir( normalVec_1st.XYZ() );
			m_NormalVec_2nd = new gp_Dir( normalVec_2nd.XYZ() );
			m_TangentVec = new gp_Dir( tangentVec.XYZ() );
		}

		public gp_Pnt Point
		{
			get
			{
				return new gp_Pnt( m_Point.XYZ() );
			}
		}

		public gp_Dir NormalVec_1st
		{
			get
			{
				return new gp_Dir( m_NormalVec_1st.XYZ() );
			}
		}

		// normal vector on co-face
		public gp_Dir NormalVec_2nd
		{
			get
			{
				return new gp_Dir( m_NormalVec_2nd.XYZ() );
			}
		}

		// tangent vector on path
		public gp_Dir TangentVec
		{
			get
			{
				return new gp_Dir( m_TangentVec.XYZ() );
			}
		}

		public void Transform( gp_Trsf transform )
		{
			m_Point.Transform( transform );
			m_NormalVec_1st.Transform( transform );
			m_NormalVec_2nd.Transform( transform );
			m_TangentVec.Transform( transform );
		}

		public CADPoint Clone()
		{
			return new CADPoint( Point, NormalVec_1st, NormalVec_2nd, TangentVec );
		}

		// using backing fields to prevent modified outside
		gp_Pnt m_Point;
		gp_Dir m_NormalVec_1st;
		gp_Dir m_NormalVec_2nd;
		gp_Dir m_TangentVec;
	}

	public interface IToolVecPoint
	{
		gp_Pnt Point
		{
			get;
		}

		gp_Dir InitToolVec
		{
			get;
		}

		gp_Dir TangentVec
		{
			get;
		}

		gp_Dir ToolVec
		{
			get;
			set;
		}
	}

	public interface IOverCutPoint
	{
		gp_Pnt Point
		{
			get;
		}

		gp_Dir TangentVec
		{
			get;
		}

		gp_Dir ToolVec
		{
			get;
		}

		IOverCutPoint Clone();
	}

	public interface ILeadLinePoint
	{
		gp_Pnt Point
		{
			get;
		}

		gp_Dir TangentVec
		{
			get;
		}

		gp_Dir ToolVec
		{
			get;
		}
	}

	public interface IProcessPoint
	{
		gp_Pnt Point
		{
			get;
		}

		gp_Dir ToolVec
		{
			get;
		}

		IProcessPoint Clone();
	}

	public class CAMPoint : IToolVecPoint, IOverCutPoint, ILeadLinePoint, IProcessPoint
	{
		public CAMPoint( CADPoint cadPoint )
		{
			m_CADPoint = cadPoint.Clone();
			m_ToolVec = new gp_Dir( cadPoint.NormalVec_1st.XYZ() );
		}

		public CAMPoint( CADPoint cadPoint, gp_Dir toolVec )
		{
			m_CADPoint = cadPoint.Clone();
			m_ToolVec = new gp_Dir( toolVec.XYZ() );
		}

		public gp_Dir ToolVec
		{
			get
			{
				return new gp_Dir( m_ToolVec.XYZ() );
			}
			set
			{
				m_ToolVec = new gp_Dir( value.XYZ() );
			}
		}

		public gp_Pnt Point
		{
			get
			{
				return m_CADPoint.Point;
			}
		}

		public gp_Dir InitToolVec
		{
			get
			{
				return m_CADPoint.NormalVec_1st;
			}
		}

		public gp_Dir TangentVec
		{
			get
			{
				return m_CADPoint.TangentVec;
			}
		}

		public CAMPoint Clone()
		{
			return new CAMPoint( m_CADPoint.Clone() );
		}

		// the explicit interface implementation for IOverCutPoint.Clone
		IOverCutPoint IOverCutPoint.Clone()
		{
			return Clone();
		}

		IProcessPoint IProcessPoint.Clone()
		{
			return Clone();
		}

		// using backing field to prevent modified outside
		CADPoint m_CADPoint;
		gp_Dir m_ToolVec;
	}
}
