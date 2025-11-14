using MyCAM.Data;

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
}
