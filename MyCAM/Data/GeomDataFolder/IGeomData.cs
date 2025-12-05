namespace MyCAM.Data
{
	public interface IGeomData
	{
		string UID
		{
			get;
		}

		PathType PathType
		{
			get;
		}

		IGeomData Clone();
	}

	public interface IRotatable
	{
		double RotatedAngle_deg
		{
			get; set;
		}
	}
}
