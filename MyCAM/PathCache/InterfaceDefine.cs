using MyCAM.Data;
using OCC.gp;
using System.Collections.Generic;

namespace MyCAM.PathCache
{
	public interface IPathCache
	{
		PathType PathType
		{
			get;
		}
	}

	public interface ITransformableCache
	{
		void DoTransform( gp_Trsf transform );
	}

	public interface IStdPatternCache : IPathCache, ITransformableCache
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
