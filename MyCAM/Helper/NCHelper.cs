using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.CacheInfo;
using MyCAM.Data;
using OCC.BRepAdaptor;
using OCC.GC;
using OCC.GCPnts;
using OCC.Geom;
using OCC.gp;

namespace MyCAM.Helper
{
	internal static class NCHelper
	{
		public static bool BuildNCPackage( ContourCacheInfo contourCacheInfo, out PathNCPackage ncPackage )
		{
			ncPackage = new PathNCPackage();

			if( contourCacheInfo == null || contourCacheInfo.CADSegmentList == null || contourCacheInfo.CADSegmentList.Count == 0 ||
				contourCacheInfo.CAMSegmentList == null || contourCacheInfo.CAMSegmentList.Count == 0 ) {
				return false;
			}
			ncPackage.LeadInSegment = contourCacheInfo.LeadInSegment?.Select( eachSegment => eachSegment.Clone() ).ToList() ?? new List<ICAMSegmentElement>();
			ncPackage.MainPathSegment = contourCacheInfo.CAMSegmentList?.Select( eachSegment => eachSegment.Clone() ).ToList() ?? new List<ICAMSegmentElement>();
			ncPackage.LeadOutSegment = contourCacheInfo.LeadOutSegment?.Select( eachSegment => eachSegment.Clone() ).ToList() ?? new List<ICAMSegmentElement>();
			ncPackage.OverCutSegment = contourCacheInfo.OverCutSegment?.Select( eachSegment => eachSegment.Clone() ).ToList() ?? new List<ICAMSegmentElement>();
			return true;
		}
	}
}
