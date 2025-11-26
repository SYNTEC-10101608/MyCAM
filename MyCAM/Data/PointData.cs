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
				return new gp_Pnt( m_Point.X(), m_Point.Y(), m_Point.Z() );
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

	// currently assuming CAM = CAD + ToolVec
	public class CAMPoint
	{
		public CAMPoint( CADPoint cadPoint, gp_Dir toolVec )
		{
			CADPoint = cadPoint;
			m_ToolVec = new gp_Dir( toolVec.XYZ() );
		}

		public CADPoint CADPoint
		{
			get; private set;
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

		public CAMPoint Clone()
		{
			return new CAMPoint( CADPoint.Clone(), ToolVec );
		}

		// using backing field to prevent modified outside
		gp_Dir m_ToolVec;
	}

	public class CAMPoint2
	{
		public CAMPoint2( gp_Pnt point, gp_Dir normal1, gp_Dir normal2, gp_Dir tangVec, gp_Dir toolVec )
		{
			m_Point = new gp_Pnt( point.X(), point.Y(), point.Z() );
			m_NormalVec_1st = new gp_Dir( normal1.XYZ() );
			m_NormalVec_2nd = new gp_Dir( normal2.XYZ() );
			m_TangentVec = new gp_Dir( tangVec.XYZ() );
			ToolVec = toolVec;
			// m_ToolVec = new gp_Dir( toolVec.XYZ() );
		}

		public gp_Pnt Point
		{
			get
			{
				return new gp_Pnt( m_Point.X(), m_Point.Y(), m_Point.Z() );
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

		public gp_Dir ToolVec
		{
			/*
			get
			{
				return new gp_Dir( m_ToolVec.XYZ() );
			}
			set
			{
				m_ToolVec = new gp_Dir( value.XYZ() );
			}
			*/
			get;set;
		}

		public CAMPoint2 Clone()
		{
			return new CAMPoint2( Point, NormalVec_1st, NormalVec_2nd, TangentVec, ToolVec );
		}

		public bool Equals( CAMPoint2 other )
		{
			if( other == null ) {
				return false;
			}
			if( ReferenceEquals( this, other ) ) {
				return true;
			}
			const double TOLERANCE = 1e-3;
			if( m_Point.IsEqual( other.m_Point, TOLERANCE ) == false ) {
				return false;
			}
			if( ToolVec.IsEqual( other.ToolVec, TOLERANCE ) == false ) {
				return false;
			}
			return true;
		}

		public override bool Equals( object obj )
		{
			return Equals( obj as CAMPoint2 );
		}

		public override int GetHashCode()
		{
			unchecked {
				int hash = 17;
				hash = hash * 23 + m_Point.X().GetHashCode();
				hash = hash * 23 + m_Point.Y().GetHashCode();
				hash = hash * 23 + m_Point.Z().GetHashCode();
				return hash;
			}
		}

		public static bool operator ==( CAMPoint2 left, CAMPoint2 right )
		{
			if( ReferenceEquals( left, right ) ) {
				return true;
			}
			if( left is null || right is null ) {
				return false;
			}
			return left.Equals( right );
		}

		public static bool operator !=( CAMPoint2 left, CAMPoint2 right )
		{
			return !( left == right );
		}

		// using backing field to prevent modified outside
		gp_Dir m_ToolVec;
		gp_Pnt m_Point;
		gp_Dir m_NormalVec_1st;
		gp_Dir m_NormalVec_2nd;
		gp_Dir m_TangentVec;
	}
}
