using MyCAM.Data;

namespace MyCAM.Helper
{
	internal static class CADSegmentBuilder
	{
		const int LOWEST_PointsToBuildSegment = 2;

		public static BuildCADError BuildCADSegment( DiscretizedCADData data, ESegmentType contourType, out ICADSegment cadSegment )
		{
			cadSegment = null;
			if( data.DiscCADPointList == null || data.DiscCADPointList.Count < LOWEST_PointsToBuildSegment ) {
				return BuildCADError.PointCountError;
			}
			if( contourType == ESegmentType.Line ) {
				cadSegment = new LineCADSegment( data.DiscCADPointList, data.SegmentLength, data.SubSegmentLength, data.SubChordLength );
				return BuildCADError.Done;
			}
			if( contourType == ESegmentType.Arc ) {

				// arc is too short, build line instead
				if( data.DiscCADPointList.Count == LOWEST_PointsToBuildSegment ) {
					cadSegment = new LineCADSegment( data.DiscCADPointList, data.SegmentLength, data.SubSegmentLength, data.SubChordLength );
				}
				else {
					// fix: 我記得當時有一個結論，是如果沒有兩點圓弧的話，這個插入工作就不需要了?
					cadSegment = new ArcCADSegment( data.DiscCADPointList, data.SegmentLength, data.SubSegmentLength, data.SubChordLength );
				}

				return BuildCADError.Done;
			}
			return BuildCADError.UnknownSegemntType;
		}
	}
}
