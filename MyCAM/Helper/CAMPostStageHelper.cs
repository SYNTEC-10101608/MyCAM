using MyCAM.Data;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	public interface ICAMSegElementInfo
	{
		CAMPoint2 MainPoint
		{
			get;
		}
		CAMPoint2 SharingPoint
		{
			get;
		}
		ICADSegment MainPointSeg
		{
			get;
		}
		ICADSegment SharingPointSeg
		{
			get;
		}
		bool IsStartPnt
		{
			get;
		}
		bool IsToolVecPnt
		{
			get;
		}
	}

	public static class CAMPostStageHelper
	{
		public static bool ReBuildCAMSegment( List<ICAMSegElementInfo> pathCAMInfo, bool isClosed, out List<ICAMSegment> PathCAMSegList, out List<int> CtrlSegIdx )
		{
			PathCAMSegList = new List<ICAMSegment>();
			CtrlSegIdx = new List<int>();

			// validation
			if( pathCAMInfo == null || pathCAMInfo.Count < 2 ) {
				return false;
			}

			// build segments
			for( int i = 0; i < pathCAMInfo.Count; i++ ) {
				// take the segment till the point with sharing point
			}

			return true;
		}
	}
}
