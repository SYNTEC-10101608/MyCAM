namespace MyCAM.Data
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
