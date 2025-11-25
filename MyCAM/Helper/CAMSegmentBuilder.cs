using MyCAM.Data;
using OCC.gp;
using OCCTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCAM.Helper
{
	internal class CAMSegmentBuilder
	{
		const int LOWEST_PointsToBuildSegment = 2;

		public static bool BuildCAMSegment( ICADSegmentElement cadSegment, out ICAMSegmentElement camSegment )
		{
			camSegment = null;
			if( cadSegment == null ) {
				return false;
			}
			return BuildCAMSegment( cadSegment.PointList, cadSegment.SegmentType, cadSegment.TotalLength, cadSegment.PerArcLegnth, cadSegment.PerChordLength, out camSegment );
		}

		public static bool BuildCAMSegmentByCAMPoint( List<CAMPoint2> camPointList, ESegmentType contourType, double dTotalLength, double dPerArcLength, double dPerChordLength, out ICAMSegmentElement camSegment )
		{
			camSegment = null;
			if( camPointList == null || camPointList.Count < LOWEST_PointsToBuildSegment ) {
				return false;
			}

			if( contourType == ESegmentType.Line ) {
				camSegment = new LineCAMSegment( camPointList, dTotalLength, dPerArcLength, dPerChordLength );
				return true;
			}
			if( contourType == ESegmentType.Arc ) {

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

		public static bool BuildCAMSegment( List<CADPoint> cadPointList, ESegmentType contourType, double dTotalLength, double dPerArcLength, double dPerChordLength, out ICAMSegmentElement camSegment )
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
