using MyCAM.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCAM.PathCache
{
	public interface IPathCache
	{
		PathType PathType
		{
			get;
		}
	}

	public interface IStdPatternCache : IPathCache
	{
	}

	public interface IStdPatternRefPointCache
	{
		IProcessPoint GetProcessRefPoint();
	}

	public interface IMainPathStartPointCache : IPathCache
	{
		CAMPoint GetMainPathStartCAMPoint();
	}

	public interface ILeadCache : IPathCache
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

	public interface IPathReverseCache : IPathCache
	{
		bool IsPathReverse
		{
			get;
		}
	}

	public interface IToolVecCache : IPathCache
	{
		IReadOnlyList<IProcessPoint> GetToolVecList();

		bool IsToolVecModifyPoint( ISetToolVecPoint point );
	}

	public interface IOverCutCache : IPathCache
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

	public interface IProcessPathStartEndCache : IPathCache
	{
		IProcessPoint GetProcessStartPoint();

		IProcessPoint GetProcessEndPoint();
	}
}
