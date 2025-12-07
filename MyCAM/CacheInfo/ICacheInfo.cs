using MyCAM.Data;
using OCC.gp;

namespace MyCAM.CacheInfo
{
	internal interface ICacheInfo
	{
		PathType PathType
		{
			get;
		}

		CAMPoint GetProcessStartPoint();

		CAMPoint GetProcessEndPoint();
	}

	internal interface IStartPnt
	{
		gp_Pnt GetMainPathStartPoint();
	}
}
