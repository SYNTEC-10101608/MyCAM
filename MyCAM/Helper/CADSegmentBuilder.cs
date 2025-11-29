using MyCAM.Data;

namespace MyCAM.Helper
{
	internal static class CADSegmentBuilder
	{
		const int LOWEST_PointsToBuildSegment = 2;
		const int LOWEST_PointsToBuildArcSegment = 3;

		public static BuildCADError BuildCADSegment( DiscretizedCADData data, ESegmentType contourType, out ICADSegment cadSegment )
		{
			cadSegment = null;
			if( data.DiscCADPointList == null || data.DiscCADPointList.Count < LOWEST_PointsToBuildSegment ) {
				return BuildCADError.InvalidPointCount;
			}
			if( contourType == ESegmentType.Line ) {
				cadSegment = new LineCADSegment( data.DiscCADPointList, data.SegmentLength, data.SubSegmentLength, data.SubChordLength );
				return BuildCADError.Done;
			}
			if( contourType == ESegmentType.Arc ) {

				// arc is too short, build line instead
				if( data.DiscCADPointList.Count < LOWEST_PointsToBuildArcSegment ) {
					cadSegment = new LineCADSegment( data.DiscCADPointList, data.SegmentLength, data.SubSegmentLength, data.SubChordLength );
				}
				else {
					cadSegment = new ArcCADSegment( data.DiscCADPointList, data.SegmentLength, data.SubSegmentLength, data.SubChordLength );
				}

				return BuildCADError.Done;
			}
			return BuildCADError.UnknownSegemntType;
		}
	}
}
