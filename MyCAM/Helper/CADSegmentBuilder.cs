using MyCAM.Data;
using OCC.gp;
using OCCTool;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	internal static class CADSegmentBuilder
	{
		const int LOWEST_PointsToBuildSegment = 2;

		public static bool BuildCADSegment( List<CADPoint> pointList, ESegmentType contourType, double dTotalLength, double dPerArcLength, double dPerChordLength, out ICADSegmentElement cadSegment )
		{
			cadSegment = null;
			if( pointList == null || pointList.Count < LOWEST_PointsToBuildSegment ) {
				return false;
			}
			if( contourType == ESegmentType.Line ) {
				cadSegment = new LineCADSegment( pointList, dTotalLength, dPerArcLength, dPerChordLength );
				return true;
			}
			if( contourType == ESegmentType.Arc ) {

				// arc is too short, build line instead
				if( pointList.Count == LOWEST_PointsToBuildSegment ) {
					cadSegment = new LineCADSegment( pointList, dTotalLength, dPerArcLength, dPerChordLength );
				}
				else {

					// arc points count should be odd number
					if( pointList.Count % 2 == 0 ) {
						gp_Pnt midPoint = new gp_Pnt(
							( pointList[ pointList.Count / 2 - 1 ].Point.X() + pointList[ pointList.Count / 2 ].Point.X() ) / 2.0,
							( pointList[ pointList.Count / 2 - 1 ].Point.Y() + pointList[ pointList.Count / 2 ].Point.Y() ) / 2.0,
							( pointList[ pointList.Count / 2 - 1 ].Point.Z() + pointList[ pointList.Count / 2 ].Point.Z() ) / 2.0 );
						gp_Dir normalVec1 = GeometryTool.GetDirAverage( pointList[ pointList.Count / 2 - 1 ].NormalVec_1st, pointList[ pointList.Count / 2 ].NormalVec_1st );
						gp_Dir normalVec2 = GeometryTool.GetDirAverage( pointList[ pointList.Count / 2 - 1 ].NormalVec_2nd, pointList[ pointList.Count / 2 ].NormalVec_2nd );
						gp_Dir tangentVec = GeometryTool.GetDirAverage( pointList[ pointList.Count / 2 - 1 ].TangentVec, pointList[ pointList.Count / 2 ].TangentVec );
						CADPoint cADPoint = new CADPoint( midPoint, normalVec1, normalVec2, tangentVec );
						pointList.Insert( pointList.Count / 2, cADPoint );
					}
					cadSegment = new ArcCADSegment( pointList, dTotalLength, dPerArcLength, dPerChordLength );
				}

				return true;
			}
			return false;
		}
	}
}
