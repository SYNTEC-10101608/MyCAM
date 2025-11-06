using System.Collections.Generic;
using OCC.gp;

namespace MyCAM.Data
{
	internal class ArcCADSegment : CADSegmentBase
	{
		public ArcCADSegment( List<CADPoint> arcPointList, double dTotalLength, double dPointSpace )
			: base( arcPointList, dTotalLength , dPointSpace)
		{
			MidPoint = arcPointList[ arcPointList.Count / 2 ];
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

		CADPoint m_MidPoint;
	}
}
