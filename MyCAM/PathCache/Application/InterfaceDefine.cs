using MyCAM.Data;
using OCC.gp;
using System.Collections.Generic;

namespace MyCAM.PathCache
{
	public interface IPathCache
	{
		List<CAMPoint> MainPathPointList
		{
			get;
		}

		List<CAMPoint> LeadInPointList
		{
			get;
		}

		List<CAMPoint> LeadOutPointList
		{
			get;
		}

		List<CAMPoint> OverCutPointList
		{
			get;
		}

		List<CADPoint> MainPathCADPointList
		{
			get;
		}

		List<CAMPoint> StartPointList
		{
			get;
		}
	}

	public interface ITransformableCache
	{
		void DoTransform( gp_Trsf transform );
	}

	public interface IContourCache : IPathCache, ITransformableCache
	{
	}

	public interface IStdPatternCache : IPathCache, ITransformableCache
	{
		CAMPoint RefPoint
		{
			get;
		}

		double MaxOverCutLength
		{
			get;
		}
	}

	public interface IStdPatternMaxOverCutCache
	{
		double GetMaxOverCutLength();
	}

	public interface IStdPatternRefPointCache
	{
		IProcessPoint GetProcessRefPoint();
	}

	public interface IProcessPathStartEndCache
	{
		IProcessPoint GetProcessStartPoint();

		IProcessPoint GetProcessEndPoint();
	}

	public interface IOrientationCache
	{
		IOrientationPoint LeadInStartPoint
		{
			get;
		}

		IOrientationPoint LeadOutEndPoint
		{
			get;
		}

		IOrientationPoint MainPathStartPoint
		{
			get;
		}
	}

	public interface ILeadCache
	{
		IReadOnlyList<IProcessPoint> LeadInCAMPointList
		{
			get;
		}

		IReadOnlyList<IProcessPoint> LeadOutCAMPointList
		{
			get;
		}

		LeadData LeadData
		{
			get;
		}
	}

	public interface IOverCutCache
	{
		IReadOnlyList<IProcessPoint> OverCutCAMPointList
		{
			get;
		}

		double OverCutLength
		{
			get;
		}
	}

	public interface IPathReverseCache
	{
		bool IsPathReverse
		{
			get;
		}
	}

	public interface ITraverseDataCache : IProcessPathStartEndCache
	{
		TraverseData TraverseData
		{
			get;
		}
	}

	public interface IMainPathCache
	{
		IReadOnlyList<IProcessPoint> MainPathPointList
		{
			get;
		}
	}

	public interface IToolVecCache : IStartPointRendererCache
	{
		bool GetToolVecModify( int index, out double dRA_deg, out double dRB_deg );

		bool IsToolVecModifyPoint( IProcessPoint point );

		bool GetToolVecInterpolateType( out EToolVecInterpolateType interpolateType );
	}

	#region Action Cache Interfaces

	public interface IStartPointActionCache
	{
		IReadOnlyList<ISetToolVecPoint> StartPointList
		{
			get;
		}
	}

	#endregion

	#region Renderer Cache Interfaces

	public interface IMainPathRendererCache
	{
		IReadOnlyList<gp_Pnt> MainPathPointList
		{
			get;
		}
	}

	public interface IStartPointRendererCache
	{
		IProcessPoint GetStartCAMPoint();

		IReadOnlyList<IProcessPoint> StartPointList
		{
			get;
		}
	}

	#endregion
}
