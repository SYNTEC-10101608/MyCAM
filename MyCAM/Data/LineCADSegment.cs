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
	}
}
