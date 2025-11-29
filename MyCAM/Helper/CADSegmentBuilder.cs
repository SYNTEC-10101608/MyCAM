using MyCAM.Data;
using OCC.gp;
using OCCTool;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	// fix: 命名 CADCAM 感覺可以省略，或是 builder 去分 CAD/CAM
	internal static class CADSegmentBuilder
	{
		const int LOWEST_PointsToBuildSegment = 2;

		public static BuildCADError BuildCADSegment( List<CADPoint> pointList, ESegmentType contourType, double dSegmentLength, double dSubSegmentLength, double dPerChordLength, out ICADSegment cadSegment )
		{
			cadSegment = null;
			if( pointList == null || pointList.Count < LOWEST_PointsToBuildSegment ) {
				return BuildCADError.PointCountError;
			}
			if( contourType == ESegmentType.Line ) {
				cadSegment = new LineCADSegment( pointList, dSegmentLength, dSubSegmentLength, dPerChordLength );
				return BuildCADError.Done;
			}
			if( contourType == ESegmentType.Arc ) {

				// arc is too short, build line instead
				if( pointList.Count == LOWEST_PointsToBuildSegment ) {
					cadSegment = new LineCADSegment( pointList, dSegmentLength, dSubSegmentLength, dPerChordLength );
				}
				else {
					// fix: 我記得當時有一個結論，是如果沒有兩點圓弧的話，這個插入工作就不需要了?
					cadSegment = new ArcCADSegment( pointList, dSegmentLength, dSubSegmentLength, dPerChordLength );
				}

				return BuildCADError.Done;
			}
			return BuildCADError.UnknownSegemntType;
		}
	}
}
