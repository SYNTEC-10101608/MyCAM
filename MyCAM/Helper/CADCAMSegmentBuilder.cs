using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCAM.Data;
using OCC.gp;
using OCCTool;

namespace MyCAM.Helper
{
	// re: 命名 CADCAM 感覺可以省略，或是 builder 去分 CAD/CAM
	internal static  class CADCAMSegmentBuilder
	{
		const int LOWEST_PointsToBuildSegment = 2;

		public static bool BuildCADSegment( List<CADPoint> pointList, EContourType contourType, double dTotalLength, double dPerArcLength, double dPerChordLength, out ICADSegmentElement cadSegment )
		{
			cadSegment = null;
			if( pointList == null || pointList.Count < LOWEST_PointsToBuildSegment ) {
				return false;
			}
			if( contourType == EContourType.Line ) {
				cadSegment = new LineCADSegment( pointList, dTotalLength, dPerArcLength, dPerChordLength );
				return true;
			}
			if( contourType == EContourType.Arc ) {

				// arc is too short, build line instead
				if( pointList.Count == LOWEST_PointsToBuildSegment ) {
					cadSegment = new LineCADSegment( pointList, dTotalLength, dPerArcLength, dPerChordLength );
				}
				else {

					// re: 我記得當時有一個結論，是如果沒有兩點圓弧的話，這個插入工作就不需要了?
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

		public static bool BuildCAMSegment( ICADSegmentElement cadSegment, out ICAMSegmentElement camSegment )
		{
			camSegment = null;
			if( cadSegment == null ) {
				return false;
			}
			return BuildCAMSegment( cadSegment.PointList, cadSegment.ContourType, cadSegment.TotalLength, cadSegment.PerArcLegnth, cadSegment.PerChordLength, out camSegment );
		}

		public static bool BuildCAMSegmentByCAMPoint( List<CAMPoint2> camPointList, EContourType contourType, double dTotalLength, double dPerArcLength, double dPerChordLength, out ICAMSegmentElement camSegment )
		{
			camSegment = null;
			if( camPointList == null || camPointList.Count < LOWEST_PointsToBuildSegment ) {
				return false;
			}

			if( contourType == EContourType.Line ) {
				camSegment = new LineCAMSegment( camPointList, dTotalLength, dPerArcLength, dPerChordLength );
				return true;
			}
			if( contourType == EContourType.Arc ) {

				// arc is too short, build line instead
				if( camPointList.Count == LOWEST_PointsToBuildSegment ) {
					camSegment = new LineCAMSegment( camPointList, dTotalLength, dPerArcLength, dPerChordLength );
					return true;
				}
				else {
					camSegment = new ArcCAMSegment( camPointList, dTotalLength, dPerArcLength, dPerChordLength );
					return true;
				}
			}
			return false;
		}

		public static bool BuildCAMSegment( List<CADPoint> cadPointList, EContourType contourType, double dTotalLength, double dPerArcLength, double dPerChordLength, out ICAMSegmentElement camSegment )
		{
			camSegment = null;
			if( cadPointList == null || cadPointList.Count < LOWEST_PointsToBuildSegment ) {
				return false;
			}
			List<CAMPoint2> camPointList = new List<CAMPoint2>( cadPointList.Count );
			for( int i = 0; i < cadPointList.Count; i++ ) {
				CADPoint cadPoint = cadPointList[ i ];

				// original tool vec is same as NormalVec_1st
				CAMPoint2 cAMPoint2 = new CAMPoint2( cadPoint.Point, cadPoint.NormalVec_1st, cadPoint.NormalVec_2nd, cadPoint.TangentVec, cadPoint.NormalVec_1st );
				camPointList.Add( cAMPoint2 );
			}
			bool isBuildSuccess = BuildCAMSegmentByCAMPoint( camPointList, contourType, dTotalLength, dPerArcLength, dPerChordLength, out camSegment );
			if( isBuildSuccess ) {
				return true;
			}
			return false;
		}
	}
}
