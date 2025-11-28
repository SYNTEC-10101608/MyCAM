using System.Collections.Generic;
using System.Linq;
using MyCAM.CacheInfo;
using MyCAM.Data;

namespace MyCAM.Helper
{
	internal static class NCHelper
	{
		public static bool BuildNCPackage( ContourCacheInfo contourCacheInfo, CraftData craftData, EntryAndExitData entryAndExitData, out PathNCPackage ncPackage )
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
			ncPackage.CtrlSegIdx = new List<int>( contourCacheInfo.CtrlToolSegIdxList);

			// set process points and traverse data
			SetProcessPoints( ncPackage, contourCacheInfo );
			SetTraverseData( ncPackage, craftData.TraverseData );
			SetEntryAndExitData( ncPackage, entryAndExitData );
			return true;
		}

		static bool IsValidInput( ContourCacheInfo contourCacheInfo )
		{
			return contourCacheInfo != null &&
				   contourCacheInfo.CADSegmentList != null && contourCacheInfo.CADSegmentList.Count > 0 &&
				   contourCacheInfo.CAMSegmentList != null && contourCacheInfo.CAMSegmentList.Count > 0;
		}

		static List<ICAMSegment> CloneSegmentList( List<ICAMSegment> segmentList )
		{
			return segmentList?.Select( segment => segment.Clone() ).ToList() ?? new List<ICAMSegment>();
		}

		static List<ICAMSegment> BuildMainPathSegment( ContourCacheInfo contourCacheInfo )
		{
			if ( contourCacheInfo.CAMSegmentList == null || contourCacheInfo.CAMSegmentList.Count == 0 ) {
				return new List<ICAMSegment>();
			}
			var clonedSegments = CloneSegmentList( contourCacheInfo.CAMSegmentList );
			return clonedSegments;
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

		static void SetEntryAndExitData( PathNCPackage ncPackage, EntryAndExitData entryAndExitData )
		{
			ncPackage.EntryDistance = entryAndExitData.EntryDistance;
			ncPackage.Entry_FollowSafeDistance = entryAndExitData.FollowSafeDistance;
		}

		public static List<ICAMSegment> ReverseSegment( List<ICAMSegment> camSegmentList )
		{
			if( camSegmentList == null || camSegmentList.Count == 0 ) {
				return new List<ICAMSegment>();
			}
			List<ICAMSegment> reversedSegment = new List<ICAMSegment>();

			foreach( ICAMSegment segment in camSegmentList ) {
				List<CAMPoint2> reversedCAMPoints = new List<CAMPoint2>( segment.CAMPointList );
				reversedCAMPoints.Reverse();

				if( segment is ArcCAMSegment ) {
					reversedSegment.Add( new ArcCAMSegment( reversedCAMPoints, segment.TotalLength,
						segment.PerArcLength, segment.PerChordLength ) );
				}
				else if( segment is LineCAMSegment ) {
					reversedSegment.Add( new LineCAMSegment( reversedCAMPoints, segment.TotalLength,
						segment.PerArcLength, segment.PerChordLength) );
				}
			}
			reversedSegment.Reverse();
			return reversedSegment;
		}
	}
}
