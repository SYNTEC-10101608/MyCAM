using MyCAM.Data;
using OCC.gp;
using System.Collections.Generic;

namespace MyCAM.CacheInfo
{
	public interface IPathHeadTailCache
	{
		CAMPoint GetProcessStartPoint();

		CAMPoint GetProcessEndPoint();
	}

	internal interface IStartPointCache
	{
		gp_Pnt GetMainPathStartPoint();

		CAMPoint GetFirstCAMPoint();
	}

	internal interface ILeadCache
	{
		List<CAMPoint> LeadInCAMPointList
		{
			get;
		}

		List<CAMPoint> LeadOutCAMPointList
		{
			get;
		}

		LeadData LeadData
		{
			get;
		}
	}

	internal interface IPathReverseCache
	{
		bool IsPathReverse
		{
			get;
		}
	}

	internal interface IToolVecCache
	{
		List<CAMPoint> GetToolVecList();

		bool IsToolVecModifyPoint( ISetToolVecPoint point );
	}

	internal interface IOverCutCache
	{
		List<CAMPoint> OverCutCAMPointList
		{
			get;
		}

		double OverCutLength
		{
			get;
		}
	}
}
