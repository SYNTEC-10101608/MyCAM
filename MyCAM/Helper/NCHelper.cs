using System.Collections.Generic;
using System.Linq;
using MyCAM.CacheInfo;
using MyCAM.Data;

namespace MyCAM.Helper
{
	internal static class NCHelper
	{
		public static bool BuildNCPackage( ContourCacheInfo contourCacheInfo, CraftData craftData, out PathNCPackage ncPackage )
		{
			ncPackage = new PathNCPackage();

			if( !IsValidInput( contourCacheInfo ) ) {
				return false;
			}

			// get path segments
			ncPackage.LeadInSegment = CloneSegmentList( contourCacheInfo.LeadInSegment );
			ncPackage.MainPathSegment = BuildMainPathSegment( contourCacheInfo );
			ncPackage.LeadOutSegment = CloneSegmentList( contourCacheInfo.LeadOutSegment );
			ncPackage.OverCutSegment = CloneSegmentList( contourCacheInfo.OverCutSegment );

			// set process points and traverse data
			SetProcessPoints( ncPackage, contourCacheInfo );
			SetTraverseData( ncPackage, craftData.TraverseData );

			return true;
		}

		static bool IsValidInput( ContourCacheInfo contourCacheInfo )
		{
			return contourCacheInfo != null &&
				   contourCacheInfo.CADSegmentList != null && contourCacheInfo.CADSegmentList.Count > 0 &&
				   contourCacheInfo.CAMSegmentList != null && contourCacheInfo.CAMSegmentList.Count > 0;
		}

		static List<ICAMSegmentElement> CloneSegmentList( List<ICAMSegmentElement> segmentList )
		{
			return segmentList?.Select( segment => segment.Clone() ).ToList() ?? new List<ICAMSegmentElement>();
		}

		static List<ICAMSegmentElement> BuildMainPathSegment( ContourCacheInfo contourCacheInfo )
		{
			var clonedSegments = CloneSegmentList( contourCacheInfo.CAMSegmentList );
			return contourCacheInfo.GetPathIsReverse() ? ReverseSegment( clonedSegments ) : clonedSegments;
		}

		static void SetProcessPoints( PathNCPackage ncPackage, ContourCacheInfo contourCacheInfo )
		{
			ncPackage.ProcessStartPnt = contourCacheInfo.GetProcessStartPoint();
			ncPackage.ProcessEndPnt = contourCacheInfo.GetProcessEndPoint();
		}

		static void SetTraverseData( PathNCPackage ncPackage, TraverseData traverseData )
		{
			ncPackage.LiftUpDistance = traverseData.LiftUpDistance;
			ncPackage.FrogLeapDistance = traverseData.FrogLeapDistance;
			ncPackage.CutDownDistance = traverseData.CutDownDistance;
			ncPackage.FollowSafeDistance = traverseData.FollowSafeDistance;
		}

		public static List<ICAMSegmentElement> ReverseSegment( List<ICAMSegmentElement> camSegmentList )
		{
			if( camSegmentList == null || camSegmentList.Count == 0 ) {
				return new List<ICAMSegmentElement>();
			}
			List<ICAMSegmentElement> reversedSegment = new List<ICAMSegmentElement>();

			foreach( ICAMSegmentElement segment in camSegmentList ) {
				List<CAMPoint2> reversedCAMPoints = new List<CAMPoint2>( segment.CAMPointList );
				reversedCAMPoints.Reverse();

				if( segment is ArcCAMSegment ) {
					reversedSegment.Add( new ArcCAMSegment( reversedCAMPoints, segment.TotalLength,
						segment.PerArcLength, segment.PerChordLength, segment.IsModify ) );
				}
				else if( segment is LineCAMSegment ) {
					reversedSegment.Add( new LineCAMSegment( reversedCAMPoints, segment.TotalLength,
						segment.PerArcLength, segment.PerChordLength, segment.IsModify ) );
				}
			}
			reversedSegment.Reverse();
			return reversedSegment;
		}
	}
}
