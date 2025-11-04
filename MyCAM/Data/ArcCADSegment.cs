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

		CADPoint m_MidPoint;
	}
}
