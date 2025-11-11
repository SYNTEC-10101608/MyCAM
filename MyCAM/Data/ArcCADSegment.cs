using System;
using System.Collections.Generic;
using System.Linq;
using OCC.gp;
using OCCTool;

namespace MyCAM.Data
{
	internal class ArcCADSegment : CADSegmentBase
	{
		// re: use builder to consider build a arc or line segment
		// re: we should also bring chord length
		public ArcCADSegment( List<CADPoint> arcPointList, double dTotalLength, double dPointSpace )
			: base( arcPointList, dTotalLength, dPointSpace )
		{
			MidPoint = FindMidPoint( arcPointList );
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
				clonedPointList.Add( point.Clone() as CADPoint );
			}
			return new ArcCADSegment( clonedPointList, m_TotalLength, m_PointSapce );
		}

		CADPoint FindMidPoint( List<CADPoint> arcPointList )
		{
			if( arcPointList == null || arcPointList.Count < 2 ) {
				throw new ArgumentException( "Point list must contain at least 2 points." );
			}
			if( arcPointList.Count == 2 ) {

				gp_Pnt midPoint = GeometryTool.FindMidPoint( arcPointList.First().Point, arcPointList.Last().Point );
				gp_Dir normalVec = GeometryTool.GetDirAverage( arcPointList.First().NormalVec_1st, arcPointList.Last().NormalVec_1st );
				gp_Dir tanVec = GeometryTool.GetDirAverage( arcPointList.First().TangentVec, arcPointList.Last().TangentVec );	
				
				CADPoint cadPoint = new CADPoint( midPoint, normalVec, normalVec, tanVec );
				return cadPoint;
			}
			else {
				return arcPointList[ arcPointList.Count / 2 ];
			}
		}
		CADPoint m_MidPoint;
	}
}
