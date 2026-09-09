using MyCAM.Data;
using MyCAM.PathCache;
using System.Collections.Generic;
using System;

namespace MyCAM.Post
{
	internal static class RCornerWriter
	{
		// build R corner CAM index map for post-processing output
		// key: start CAM index, value: RCornerData with converted CAM indices
		public static Dictionary<int, RCornerData> BuildRCornerCAMIndexMap( string szPathID, CraftData craftData, int nMainPathPointCount )
		{
			Dictionary<int, RCornerData> resultMap = new Dictionary<int, RCornerData>();

			if( craftData == null || craftData.RCornerMap == null || nMainPathPointCount <= 0 ) {
				return resultMap;
			}

			// need contour cache to convert CAD index to CAM index
			if( !DataGettingHelper.GetContourCacheByID( szPathID, out ContourCache contourCache ) ) {
				return resultMap;
			}

			foreach( RCornerData range in craftData.RCornerMap.Values ) {

				// convert to an ordered CAM interval (machining order)
				if( !contourCache.TryGetRCornerCAMRange( range, out int machiningStartCamIdx, out int machiningEndCamIdx ) ) {
					continue;
				}

				// defensive check: indices must be valid
				if( machiningStartCamIdx < 0 || machiningEndCamIdx >= nMainPathPointCount || machiningStartCamIdx >= machiningEndCamIdx ) {
					continue;
				}

				resultMap[ machiningStartCamIdx ] = new RCornerData( machiningStartCamIdx, machiningEndCamIdx );
			}

			return resultMap;
		}
	}
}
