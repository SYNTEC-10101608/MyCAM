using OCC.gp;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Data
{
	public enum ESegmentType
	{
		Line,
		Arc,
	}

	#region CAD Segment
	public interface ICADSegment
	{
		ESegmentType SegmentType
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

		double SegmentLength
		{
			get;
		}

		double SubSegmentLength
		{
			get;
		}

		double SubChordLength
		{
			get;
		}

		ICADSegment Clone();

		void Transform( gp_Trsf transForm );
	}

	public abstract class CADSegmentBase : ICADSegment
	{
		protected CADSegmentBase( List<CADPoint> pointList, double dTotalLength, double dPerArcLength, double dSubChordLength )
		{
			if( pointList == null || pointList.Count < 2 ) {
				throw new System.ArgumentException( "CADSegmentBasis constructing argument pointList null or count less than 2." );
			}
			m_StartPoint = pointList[ 0 ];
			m_EndPoint = pointList[ pointList.Count - 1 ];
			m_PointList = pointList;
			m_TotalLength = dTotalLength;
			m_SubSegmentLength = dPerArcLength;
			m_SubChordLength = dSubChordLength;
		}

		public abstract ESegmentType SegmentType
		{
			get;
		}

		public virtual CADPoint StartPoint
		{
			get
			{
				return m_StartPoint;
			}
		}

		public virtual CADPoint EndPoint
		{
			get
			{
				return m_EndPoint;
			}
		}

		public virtual List<CADPoint> PointList
		{
			get
			{
				return m_PointList;
			}
		}

		public abstract ICADSegment Clone();

		public double SegmentLength
		{
			get
			{
				return m_TotalLength;
			}
		}

		public double SubSegmentLength
		{
			get
			{
				return m_SubSegmentLength;
			}
		}

		public double SubChordLength
		{
			get
			{
				return m_SubChordLength;
			}
		}

		public virtual void Transform( gp_Trsf transForm )
		{
			foreach( CADPoint point in m_PointList ) {
				point.Transform( transForm );
			}
		}

		protected List<CADPoint> m_PointList;
		protected CADPoint m_StartPoint;
		protected CADPoint m_EndPoint;
		protected double m_TotalLength;
		protected double m_SubSegmentLength;
		protected double m_SubChordLength;
	}

	public class LineCADSegment : CADSegmentBase
	{
		public LineCADSegment( List<CADPoint> linePointList, double dTotalLength, double dSubSegmentLegnth, double dSubChordLength )
			: base( linePointList, dTotalLength, dSubSegmentLegnth, dSubChordLength )
		{
		}

		public override ESegmentType SegmentType
		{
			get
			{
				return ESegmentType.Line;
			}
		}

		public override ICADSegment Clone()
		{
			List<CADPoint> clonedPointList = new List<CADPoint>();
			foreach( CADPoint point in m_PointList ) {
				clonedPointList.Add( point.Clone() );
			}
			return new LineCADSegment( clonedPointList, m_TotalLength, m_SubSegmentLength, m_SubChordLength );
		}
	}

	public class ArcCADSegment : CADSegmentBase
	{
		public ArcCADSegment( List<CADPoint> arcPointList, double dTotalLength, double dSubSegmentLength, double dSubChordLength )
			: base( arcPointList, dTotalLength, dSubSegmentLength, dSubChordLength )
		{
			if( arcPointList.Count <= 2 ) {
				throw new System.ArgumentException( "ArcCADSegment requires at least 3 points to define a valid arc." );
			}
			m_MidIndex = arcPointList.Count / 2;
			m_MidPoint = arcPointList[ m_MidIndex ];
		}

		public override ESegmentType SegmentType
		{
			get
			{
				return ESegmentType.Arc;
			}
		}

		public CADPoint MidPoint
		{
			get
			{
				return m_MidPoint;
			}
		}

		public override ICADSegment Clone()
		{
			List<CADPoint> clonedPointList = new List<CADPoint>();
			foreach( CADPoint point in m_PointList ) {
				clonedPointList.Add( point.Clone() );
			}
			return new ArcCADSegment( clonedPointList, m_TotalLength, m_SubSegmentLength, m_SubChordLength );
		}

		CADPoint m_MidPoint;
		int m_MidIndex = 0;
	}

	#endregion

	#region CAM Segment

	internal interface ICAMSegment
	{
		ESegmentType ContourType
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

		double SegmentLength
		{
			get;
		}

		double SubChordLength
		{
			get;
		}

		double SubSegmentLength
		{
			get;
		}

		gp_Dir GetStartPointToolVec();

		gp_Dir GetEndPointToolVec();

		void SetStartPointToolVec( gp_Dir startPointToolVec );

		void SetEndPointToolVec( gp_Dir endPointToolVec );

		ICAMSegment Clone();
	}

	internal abstract class CAMSegmentBase : ICAMSegment
	{
		protected CAMSegmentBase( List<CAMPoint2> camPointList, double dSegmentLength, double dSubSegmentLength, double dSubChordLength )
		{
			if( camPointList.Count < 2 ) {
				throw new System.ArgumentException( " CAMSegmentBasis constructing points are null" );
			}
			m_StartPoint = camPointList.First();
			m_EndPoint = camPointList.Last();
			m_CAMPointList = camPointList;
			m_SegmentLength = dSegmentLength;
			m_SubSegmentLength = dSubSegmentLength;
			m_SubChordLength = dSubChordLength;
		}

		public abstract ESegmentType ContourType
		{
			get;
		}


		public virtual CAMPoint2 StartPoint
		{
			get
			{
				return m_StartPoint;
			}
		}

		public virtual CAMPoint2 EndPoint
		{
			get
			{
				return m_EndPoint;
			}
		}

		public virtual List<CAMPoint2> CAMPointList
		{
			get
			{
				return m_CAMPointList;
			}
		}

		public virtual double SegmentLength
		{
			get
			{
				return m_SegmentLength;
			}
		}

		public virtual double SubChordLength
		{
			get
			{
				return m_SubChordLength;
			}
		}

		public virtual double SubSegmentLength
		{
			get
			{
				return m_SubSegmentLength;
			}
		}

		public virtual gp_Dir GetStartPointToolVec()
		{
			return new gp_Dir( m_StartPoint.ToolVec.XYZ() );
		}

		public virtual gp_Dir GetEndPointToolVec()
		{
			return new gp_Dir( m_EndPoint.ToolVec.XYZ() );
		}

		public virtual void SetStartPointToolVec( gp_Dir startPointToolVec )
		{
			if( startPointToolVec == null ) {
				return;
			}
			m_StartPoint.ToolVec = new gp_Dir( startPointToolVec.XYZ() );
			CalculatePointLisToolVec();
		}

		public virtual void SetEndPointToolVec( gp_Dir endPointToolVec )
		{
			if( endPointToolVec == null ) {
				return;
			}
			m_EndPoint.ToolVec = new gp_Dir( endPointToolVec.XYZ() );
			CalculatePointLisToolVec();
		}

		public abstract ICAMSegment Clone();

		protected virtual void CalculatePointLisToolVec()
		{
			gp_Vec startPointToolVec = new gp_Vec( m_StartPoint.ToolVec );
			gp_Vec endPointToolVec = new gp_Vec( m_EndPoint.ToolVec );

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( startPointToolVec, endPointToolVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );

			gp_Quaternion q = new gp_Quaternion();
			for( int i = 0; i < m_CAMPointList.Count - 1; i++ ) {
				slerp.Interpolate( m_SubSegmentLength * i / m_SegmentLength, ref q );
				gp_Trsf trsf = new gp_Trsf();
				trsf.SetRotation( q );
				gp_Dir toolVecDir = new gp_Dir( startPointToolVec.Transformed( trsf ) );
				m_CAMPointList[ i ].ToolVec = toolVecDir;
			}
			m_CAMPointList[ m_CAMPointList.Count - 1 ].ToolVec = new gp_Dir( endPointToolVec );
		}

		protected List<CAMPoint2> m_CAMPointList = new List<CAMPoint2>();
		protected CAMPoint2 m_StartPoint;
		protected CAMPoint2 m_EndPoint;
		protected double m_SegmentLength = 0.0;
		protected double m_SubSegmentLength = 0.0;
		protected double m_SubChordLength = 0.0;
		protected bool m_isModifySegment = false;
	}

	internal class LineCAMSegment : CAMSegmentBase
	{
		public LineCAMSegment( List<CAMPoint2> camPointList, double dTotalLength, double dSubSegmentLength, double dSubChordLength )
			: base( camPointList, dTotalLength, dSubSegmentLength, dSubChordLength )
		{
		}

		public override ESegmentType ContourType
		{
			get
			{
				return ESegmentType.Line;
			}
		}

		public override ICAMSegment Clone()
		{
			List<CAMPoint2> clonedPointList = new List<CAMPoint2>();
			foreach( CAMPoint2 point in CAMPointList ) {
				clonedPointList.Add( point.Clone() );
			}
			return new LineCAMSegment( clonedPointList, m_SegmentLength, m_SubSegmentLength, m_SubChordLength );
		}
	}

	internal class ArcCAMSegment : CAMSegmentBase
	{
		public ArcCAMSegment( List<CAMPoint2> camPointList, double dTotalLength, double dSubSegmentLength, double dSubChordLength )
			: base( camPointList, dTotalLength, dSubSegmentLength, dSubChordLength )
		{
			if( camPointList.Count <= 2 ) {
				throw new System.ArgumentException( "ArcCAMSegment requires at least 3 points to define a valid arc." );
			}
			m_MidIndex = camPointList.Count / 2;

			// share pointer
			m_MidPoint = camPointList[ m_MidIndex ];
		}

		public override ESegmentType ContourType
		{
			get
			{
				return ESegmentType.Arc;
			}
		}

		public override ICAMSegment Clone()
		{
			List<CAMPoint2> clonedPointList = new List<CAMPoint2>();
			foreach( CAMPoint2 point in CAMPointList ) {
				clonedPointList.Add( point.Clone() );
			}
			return new ArcCAMSegment( clonedPointList, m_SegmentLength, m_SubSegmentLength, m_SubChordLength );
		}

		public CAMPoint2 MidPoint
		{
			get
			{
				return m_MidPoint;
			}
		}

		CAMPoint2 m_MidPoint;
		int m_MidIndex = 0;
	}

	#endregion
}
