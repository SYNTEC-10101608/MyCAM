using System.Collections.Generic;

namespace MyCAM.Data
{
	internal interface ICacheInfo
	{
		List<CAMPoint> LeadInCAMPointList
		{
			get;
		}

		List<CAMPoint> LeadOutCAMPointList
		{
			get;
		}

		List<CAMPoint> OverCutCAMPointList
		{
			get;
		}
	}
}
