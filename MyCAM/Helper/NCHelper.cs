using System;
using System.Collections.Generic;
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
			ncPackage.MainPathSegment = contourCacheInfo.CAMSegmentList;
			ncPackage.LeadInSegment = contourCacheInfo.LeadInSegment;
			ncPackage.LeadOutSegment = contourCacheInfo.LeadOutSegment;
			ncPackage.OverCutSegment = contourCacheInfo.OverCutSegment;
			return true;
		}

	


	}
}
