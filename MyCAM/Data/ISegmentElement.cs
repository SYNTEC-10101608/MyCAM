using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Data
{
	enum EContourType
	{
		Line,
		Arc,
	}

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

		double PointSpace
		{
			get;
		}
	}

	internal interface ICAMSegmentElement
	{
		EContourType ContourType
		{
			get;
		}

		CAMPoint StartPoint
		{
			get;
		}

		CAMPoint EndPoint
		{
			get;
		}
	}

	internal abstract class CADSegmentBase : ICADSegmentElement
	{
		protected CADSegmentBase( List<CADPoint> pointList, double dTotalLength, double dPointSpace )
		{
			if( pointList == null || pointList.Count < 2 ) {
				throw new System.ArgumentException( "CADSegmentBasis constructing argument pointList null or count less than 2." );
			}
			m_StartPoint = pointList[ 0 ];
			m_EndPoint = pointList[ pointList.Count - 1 ];
			m_PointList = pointList;
			m_TotalLength = dTotalLength;
			m_PointSapce = dPointSpace;
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

		public virtual CADSegmentBase Clone()
		{
			List<CADPoint> clonedPoints = new List<CADPoint>();
			clonedPoints.AddRange( m_PointList.Select( p => p.Clone() ) );
			return null;
		}

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

		public double PointSpace
		{
			get
			{
				return m_PointSapce;
			}
			private set
			{
				if( value >= 0.0 ) {
					m_PointSapce = value;
				}
			}
		}

		protected List<CADPoint> m_PointList = new List<CADPoint>();
		protected CADPoint m_StartPoint;
		protected CADPoint m_EndPoint;
		protected double m_TotalLength = 0.0;
		protected double m_PointSapce = 0.0;
	}

	internal abstract class CAMSegmentBase : ICAMSegmentElement
	{
		protected CAMSegmentBase( CAMPoint startPoint, CAMPoint endPoint )
		{
			if( startPoint == null || endPoint == null ) {
				throw new System.ArgumentException( " CAMSegmentBasis constructing points are null" );
			}
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		public abstract EContourType ContourType
		{
			get;
		}

		public virtual CAMPoint StartPoint
		{
			get;
			private set;
		}

		public virtual CAMPoint EndPoint
		{
			get;
			private set;
		}
	}
}
