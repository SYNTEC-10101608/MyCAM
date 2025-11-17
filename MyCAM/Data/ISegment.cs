using System.Collections.Generic;
using System.Linq;
using OCC.gp;

namespace MyCAM.Data
{
	enum EContourType
	{
		Line,
		Arc,
	}

	#region CAD Segment

	internal interface ICADSegmentElement
	{
		EContourType ContourType
		{
			get;
		}

		CADPoint StartPoint
		{
			get;
		}

		CADPoint EndPoint
		{
			get;
		}

		List<CADPoint> PointList
		{
			get;
		}

		double TotalLength
		{
			get;
		}

		double PerArcLegnth
		{
			get;
		}

		double PerChordLength
		{
			get;
		}

		ICADSegmentElement Clone();

		void Transform( gp_Trsf transForm );

	}

	internal abstract class CADSegmentBase : ICADSegmentElement
	{
		protected CADSegmentBase( List<CADPoint> pointList, double dTotalLength, double dPerArcLength, double dPerChordLength )
		{
			if( pointList == null || pointList.Count < 2 ) {
				throw new System.ArgumentException( "CADSegmentBasis constructing argument pointList null or count less than 2." );
			}
			m_StartPoint = pointList[ 0 ];
			m_EndPoint = pointList[ pointList.Count - 1 ];
			m_PointList = pointList;
			m_TotalLength = dTotalLength;
			m_PerArcLength = dPerArcLength;
		}

		public abstract EContourType ContourType
		{
			get;
		}

		public virtual CADPoint StartPoint
		{
			get
			{
				return m_StartPoint;
			}
			private set
			{
				if( value != null ) {
					m_StartPoint = value;
				}
			}
		}

		public virtual CADPoint EndPoint
		{
			get
			{
				return m_EndPoint;
			}
			private set
			{
				if( value != null ) {
					m_EndPoint = value;
				}
			}
		}

		public virtual List<CADPoint> PointList
		{
			get
			{
				return new List<CADPoint>( m_PointList );
			}
			private set
			{
				if( value != null ) {
					m_PointList = value;
				}
			}
		}

		public abstract ICADSegmentElement Clone();

		public double TotalLength
		{
			get
			{
				return m_TotalLength;
			}
			private set
			{
				if( value >= 0.0 ) {
					m_TotalLength = value;
				}
			}
		}

		public double PerArcLegnth
		{
			get
			{
				return m_PerArcLength;
			}
			private set
			{
				if( value >= 0.0 ) {
					m_PerArcLength = value;
				}
			}
		}

		public double PerChordLength
		{
			get
			{
				return m_PerChordLength;
			}
			private set
			{
				if( value >= 0.0 ) {
					m_PerChordLength = value;
				}
			}
		}

		public virtual void Transform( gp_Trsf transForm )
		{
			foreach( CADPoint point in m_PointList ) {
				point.Transform( transForm );
			}
			m_StartPoint = m_PointList[ 0 ];
			m_EndPoint = m_PointList[ m_PointList.Count - 1 ];
		}

		protected List<CADPoint> m_PointList = new List<CADPoint>();
		protected CADPoint m_StartPoint;
		protected CADPoint m_EndPoint;
		protected double m_TotalLength = 0.0;
		protected double m_PerArcLength = 0.0;
		protected double m_PerChordLength = 0.0;
	}

	internal class LineCADSegment : CADSegmentBase
	{
		public LineCADSegment( List<CADPoint> linePointList, double dTotalLength, double dPerArcLegnth, double dPerChordLength )
			: base( linePointList, dTotalLength, dPerArcLegnth, dPerChordLength )
		{
		}

		public override EContourType ContourType
		{
			get
			{
				return EContourType.Line;
			}
		}

		public override ICADSegmentElement Clone()
		{
			List<CADPoint> clonedPointList = new List<CADPoint>();
			foreach( CADPoint point in m_PointList ) {
				clonedPointList.Add( point.Clone() as CADPoint );
			}
			return new LineCADSegment( clonedPointList, m_TotalLength, m_PerArcLength, m_PerChordLength );
		}
	}

	internal class ArcCADSegment : CADSegmentBase
	{
		public ArcCADSegment( List<CADPoint> arcPointList, double dTotalLength, double dPerArcLength, double dPerChordLength )
			: base( arcPointList, dTotalLength, dPerArcLength, dPerChordLength )
		{
			m_MidIndex = arcPointList.Count / 2;
			MidPoint = arcPointList[ m_MidIndex ];
			m_dStartToMidLength = PerArcLegnth * m_MidIndex;
		}

		public override EContourType ContourType
		{
			get
			{
				return EContourType.Arc;
			}
		}

		public CADPoint MidPoint
		{
			get
			{
				return m_MidPoint;
			}
			private set
			{
				if( value != null ) {
					m_MidPoint = value;
				}
			}
		}

		public override ICADSegmentElement Clone()
		{
			List<CADPoint> clonedPointList = new List<CADPoint>();
			foreach( CADPoint point in m_PointList ) {
				clonedPointList.Add( point.Clone() );
			}
			return new ArcCADSegment( clonedPointList, m_TotalLength, m_PerArcLength, m_PerChordLength );
		}

		public override void Transform( gp_Trsf transform )
		{
			base.Transform( transform );
			MidPoint = m_PointList[ m_MidIndex ];
		}

		CADPoint m_MidPoint;
		int m_MidIndex = 0;
		double m_dStartToMidLength = 0.0;
	}

	#endregion

	#region CAM Segment

	internal interface ICAMSegmentElement
	{
		EContourType ContourType
		{
			get;
		}

		CAMPoint2 StartPoint
		{
			get;
		}

		CAMPoint2 EndPoint
		{
			get;
		}

		List<CAMPoint2> CAMPointList
		{
			get;
		}

		double TotalLength
		{
			get;
		}

		double PerChordLength
		{
			get;
		}

		double PerArcLength
		{
			get;
		}

		gp_Dir GetStartPointToolVec();

		gp_Dir GetEndPointToolVec();

		void SetStartPointToolVec( gp_Dir startPointToolVec );

		void SetEndPointToolVec( gp_Dir endPointToolVec );
	}

	internal abstract class CAMSegmentBase : ICAMSegmentElement
	{
		protected CAMSegmentBase( List<CAMPoint2> camPointList, double dTotalLength, double dArcLength, double dChordLength )
		{
			if( camPointList.Count < 2 ) {
				throw new System.ArgumentException( " CAMSegmentBasis constructing points are null" );
			}
			StartPoint = camPointList.First().Clone();
			EndPoint = camPointList.Last().Clone();
			CAMPointList = camPointList;
			TotalLength = dTotalLength;
			PerArcLength = dArcLength;
			PerChordLength = dChordLength;
		}

		public abstract EContourType ContourType
		{
			get;
		}

		public virtual CAMPoint2 StartPoint
		{
			get;
			private set;
		}

		public virtual CAMPoint2 EndPoint
		{
			get;
			private set;
		}

		public virtual List<CAMPoint2> CAMPointList
		{
			get => m_CAMPointList.Select( p => p.Clone() ).ToList();
			private set
			{
				if( value != null ) {
					m_CAMPointList = value;
				}
			}
		}

		public virtual double TotalLength
		{
			get;
			private set;
		}

		public virtual double PerChordLength
		{
			get;
			private set;
		}

		public virtual double PerArcLength
		{
			get;
			private set;
		}

		public virtual gp_Dir GetStartPointToolVec()
		{
			return new gp_Dir( StartPoint.ToolVec.XYZ() );
		}

		public virtual gp_Dir GetEndPointToolVec()
		{
			return new gp_Dir( EndPoint.ToolVec.XYZ() );
		}

		public virtual void SetStartPointToolVec( gp_Dir startPointToolVec )
		{
			if( startPointToolVec == null ) {
				return;
			}
			StartPoint.ToolVec = new gp_Dir( startPointToolVec.XYZ() );
			CalculatePointLisToolVec();
		}

		public virtual void SetEndPointToolVec( gp_Dir endPointToolVec )
		{
			if( endPointToolVec == null ) {
				return;
			}
			EndPoint.ToolVec = new gp_Dir( endPointToolVec.XYZ() );
			CalculatePointLisToolVec();
		}

		protected virtual void CalculatePointLisToolVec()
		{
			gp_Vec startPointToolVec = new gp_Vec( StartPoint.ToolVec );
			gp_Vec endPointToolVec = new gp_Vec( EndPoint.ToolVec );

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( startPointToolVec, endPointToolVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );

			gp_Quaternion q = new gp_Quaternion();
			for( int i = 0; i < m_CAMPointList.Count - 1; i++ ) {
				slerp.Interpolate( PerArcLength * i / TotalLength, ref q );
				gp_Trsf trsf = new gp_Trsf();
				trsf.SetRotation( q );
				gp_Dir toolVecDir = new gp_Dir( startPointToolVec.Transformed( trsf ) );
				m_CAMPointList[ i ].ToolVec = toolVecDir;
			}
			m_CAMPointList[ m_CAMPointList.Count - 1 ].ToolVec = new gp_Dir( endPointToolVec );
		}

		protected List<CAMPoint2> m_CAMPointList = new List<CAMPoint2>();
	}

	internal class LineCAMSegment : CAMSegmentBase
	{
		public LineCAMSegment( List<CAMPoint2> camPointList, double dTotalLength, double dArcLength, double dChordLength )
			: base( camPointList, dTotalLength, dArcLength, dChordLength )
		{
		}

		public override EContourType ContourType
		{
			get
			{
				return EContourType.Line;
			}
		}
	}

	internal class ArcCAMSegment : CAMSegmentBase
	{
		public ArcCAMSegment( List<CAMPoint2> camPointList, double dTotalLength, double dArcLength, double dChordLength )
			: base( camPointList, dTotalLength, dArcLength, dChordLength )
		{
			m_MidIndex = camPointList.Count / 2;
			MidPoint = camPointList[ m_MidIndex ];
			m_dStartToMidLength = PerArcLength * m_MidIndex;
		}

		public override EContourType ContourType
		{
			get
			{
				return EContourType.Arc;
			}
		}

		public double dStartToMidLength
		{
			get
			{
				return m_dStartToMidLength;
			}
		}

		public CAMPoint2 MidPoint
		{
			get
			{
				return m_MidPoint;
			}
			private set
			{
				if( value != null ) {
					m_MidPoint = value;
				}
			}
		}

		CAMPoint2 m_MidPoint;
		int m_MidIndex = 0;
		double m_dStartToMidLength = 0.0;
	}

	#endregion
}
