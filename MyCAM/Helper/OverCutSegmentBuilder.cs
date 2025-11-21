using System;
using System.Collections.Generic;
using MyCAM.Data;
using System.Linq;

namespace MyCAM.Helper
{
	internal static class OverCutSegmentBuilder
	{
		public static List<ICAMSegmentElement> BuildOverCutSegment( List<ICAMSegmentElement> camSegmentList, double dOverCutLength ,bool isReverse)
		{
			List<ICAMSegmentElement> PathSegment = camSegmentList?.Select( eachSegment => eachSegment.Clone() ).ToList() ?? new List<ICAMSegmentElement>();
			if( isReverse ) {
				List<ICAMSegmentElement> reversedSegment = NCHelper.ReverseSegment( camSegmentList );
				PathSegment = reversedSegment;
			}
			double dTotalLength = 0.0;
			List<ICAMSegmentElement> overCutSegmentList = new List<ICAMSegmentElement>();
			for( int i = 0; i < PathSegment.Count; i++ ) {
				double dAddThisSegmentLength = dTotalLength + PathSegment[ i ].TotalLength;

				// keep add
				if( dAddThisSegmentLength < dOverCutLength ) {
					overCutSegmentList.Add( PathSegment[ i ] );
					dTotalLength += PathSegment[ i ].TotalLength;
				}
				if( Math.Abs( dAddThisSegmentLength - dOverCutLength ) <= 1e-3 ) {
					return overCutSegmentList;
				}
				if( dAddThisSegmentLength > dOverCutLength ) {
					double dNeedLength = dOverCutLength - dTotalLength;
					int dNeedPartCount = (int)( dNeedLength / PathSegment[ i ].PerChordLength );
					List<CAMPoint2> camPointList = new List<CAMPoint2>();
					camPointList.AddRange( PathSegment[ i ].CAMPointList.GetRange( 0, dNeedPartCount + 1 ) );
					if( PathSegment[ i ].ContourType == EContourType.Line ) {
						bool isBuildSuccess = CADCAMSegmentBuilder.BuildCAMSegmentByCAMPoint( camPointList, PathSegment[ i ].ContourType, dNeedPartCount * camSegmentList[ i ].PerChordLength, camSegmentList[ i ].PerArcLength, camSegmentList[ i ].PerChordLength, out ICAMSegmentElement camSegment );
						overCutSegmentList.Add( camSegment );
						return overCutSegmentList;
					}
					if( camSegmentList[ i ].ContourType == EContourType.Arc ) {
						bool isBuildSuccess = CADCAMSegmentBuilder.BuildCAMSegmentByCAMPoint( camPointList, PathSegment[ i ].ContourType, dNeedPartCount * camSegmentList[ i ].PerArcLength, camSegmentList[ i ].PerArcLength, camSegmentList[ i ].PerChordLength, out ICAMSegmentElement camSegment );
						overCutSegmentList.Add( camSegment );
						return overCutSegmentList;
					}
				}
			}
			return overCutSegmentList;
		}
	}
}
