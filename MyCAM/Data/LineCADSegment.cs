using System.Collections.Generic;

namespace MyCAM.Data
{
	internal class LineCADSegment : CADSegmentBase
	{
		public LineCADSegment( List<CADPoint> linePointList, double dTotalLength , double dPointSpace)
			: base( linePointList, dTotalLength, dPointSpace )
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
			return new LineCADSegment( clonedPointList, m_TotalLength, m_PointSapce );
		}
	}
}
