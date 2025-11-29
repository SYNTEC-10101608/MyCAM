using System;
using System.Collections.Generic;
using MyCAM.Data;
using System.Linq;

namespace MyCAM.Helper
{
	internal static class OverCutSegmentBuilder
	{
		public static List<ICAMSegment> BuildOverCutSegment( List<ICAMSegment> camSegmentList, double dOverCutLength ,bool isReverse)
		{
			List<ICAMSegment> PathSegment = camSegmentList?.Select( eachSegment => eachSegment.Clone() ).ToList() ?? new List<ICAMSegment>();
			if( isReverse ) {
				List<ICAMSegment> reversedSegment = NCHelper.ReverseSegment( camSegmentList );
				PathSegment = reversedSegment;
			}
			double dTotalLength = 0.0;
			List<ICAMSegment> overCutSegmentList = new List<ICAMSegment>();
			for( int i = 0; i < PathSegment.Count; i++ ) {
				double dAddThisSegmentLength = dTotalLength + PathSegment[ i ].SegmentLength;

				// keep add
				if( dAddThisSegmentLength < dOverCutLength ) {
					overCutSegmentList.Add( PathSegment[ i ] );
					dTotalLength += PathSegment[ i ].SegmentLength;
				}
				if( Math.Abs( dAddThisSegmentLength - dOverCutLength ) <= 1e-3 ) {
					return overCutSegmentList;
				}
				if( dAddThisSegmentLength > dOverCutLength ) {
					double dNeedLength = dOverCutLength - dTotalLength;
					int dNeedPartCount = (int)( dNeedLength / PathSegment[ i ].SubChordLength );
					List<CAMPoint2> camPointList = new List<CAMPoint2>();
					camPointList.AddRange( PathSegment[ i ].CAMPointList.GetRange( 0, dNeedPartCount + 1 ) );
					if( PathSegment[ i ].ContourType == ESegmentType.Line ) {
						bool isBuildSuccess = CAMSegmentBuilder.BuildCAMSegmentByCAMPoint( camPointList, PathSegment[ i ].ContourType, dNeedPartCount * camSegmentList[ i ].SubChordLength, camSegmentList[ i ].SubSegmentLength, camSegmentList[ i ].SubChordLength, out ICAMSegment camSegment );
						overCutSegmentList.Add( camSegment );
						return overCutSegmentList;
					}
					if( camSegmentList[ i ].ContourType == ESegmentType.Arc ) {
						bool isBuildSuccess = CAMSegmentBuilder.BuildCAMSegmentByCAMPoint( camPointList, PathSegment[ i ].ContourType, dNeedPartCount * camSegmentList[ i ].SubSegmentLength, camSegmentList[ i ].SubSegmentLength, camSegmentList[ i ].SubChordLength, out ICAMSegment camSegment );
						overCutSegmentList.Add( camSegment );
						return overCutSegmentList;
					}
				}
			}
			return overCutSegmentList;
		}
	}
}
