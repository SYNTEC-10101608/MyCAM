using OCC.gp;

namespace MyCAM.Data
{
	public interface IGeomData
	{
		PathType PathType
		{
			get;
		}

		bool IsClosed
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

	public interface ICenterPointCache
	{
		gp_Pnt CenterPnt
		{
			get;
		}

		gp_Dir AverageNormalDir
		{
			get;
		}
	}

	public interface IStandardPatternGeomData : IRotatable, IGeomData
	{

	}
}
