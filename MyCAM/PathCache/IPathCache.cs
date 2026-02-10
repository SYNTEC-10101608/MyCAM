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

		gp_Ax3 RefCoord
		{
			get;
		}

		gp_Ax1 ComputeRefCenterDir
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
		List<CADPoint> KeyCADPointList
		{
			get;
		}

		List<CAMPoint> KeyCAMPointList
		{
			get;
		}

		List<CADPoint> MainPathCADPointList
		{
			get;
		}

		CAMPoint RefPoint
		{
			get;
		}

		IStdPatternGeomData ComputeGeomData
		{
			get;
		}
	}
}
