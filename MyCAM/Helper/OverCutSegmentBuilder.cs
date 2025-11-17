using System;
using System.Collections.Generic;
using MyCAM.Data;

namespace MyCAM.Helper
{
	internal static class OverCutSegmentBuilder
	{
		public static List<ICAMSegmentElement> BuildOverCutSegment( List<ICAMSegmentElement> camSegmentList, double dOverCutLength )
		{
			double dTotalLength = 0.0;
			List<ICAMSegmentElement> overCutSegmentList = new List<ICAMSegmentElement>();
			for( int i = 0; i < camSegmentList.Count; i++ ) {
				double dAddThisSegmentLength = dTotalLength + camSegmentList[ i ].TotalLength;

				// keep add
				if( dAddThisSegmentLength < dOverCutLength ) {
					overCutSegmentList.Add( camSegmentList[ i ] );
					dTotalLength += camSegmentList[ i ].TotalLength;
				}
				if( Math.Abs( dAddThisSegmentLength - dOverCutLength ) <= 1e-3 ) {
					return overCutSegmentList;
				}
				if( dAddThisSegmentLength > dOverCutLength ) {
					double dNeedLength = dOverCutLength - dTotalLength;
					int dNeedPartCount = (int)( dNeedLength / camSegmentList[ i ].PerChordLength );
					List<CAMPoint2> camPointList = new List<CAMPoint2>();
					camPointList.AddRange( camSegmentList[ i ].CAMPointList.GetRange( 0, dNeedPartCount + 1 ) );
					if( camSegmentList[ i ].ContourType == EContourType.Line ) {
						bool isBuildSuccess = CADCAMSegmentBuilder.BuildCAMSegmentByCAMPoint( camPointList, camSegmentList[ i ].ContourType, dNeedPartCount * camSegmentList[ i ].PerChordLength, camSegmentList[ i ].PerArcLength, camSegmentList[ i ].PerChordLength, out ICAMSegmentElement camSegment );
						overCutSegmentList.Add( camSegment );
						return overCutSegmentList;
					}
					if( camSegmentList[ i ].ContourType == EContourType.Arc ) {
						bool isBuildSuccess = CADCAMSegmentBuilder.BuildCAMSegmentByCAMPoint( camPointList, camSegmentList[ i ].ContourType, dNeedPartCount * camSegmentList[ i ].PerArcLength, camSegmentList[ i ].PerArcLength, camSegmentList[ i ].PerChordLength, out ICAMSegmentElement camSegment );
						overCutSegmentList.Add( camSegment );
						return overCutSegmentList;
					}
				}
			}
			return overCutSegmentList;
		}
	}
}
