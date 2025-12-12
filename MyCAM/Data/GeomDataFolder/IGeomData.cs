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

	public interface ITransformableGeom
	{
		void DoTransform( gp_Trsf transform );
	}

	public interface ICenterAvgDir
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

	public interface IRotatable
	{
		double RotatedAngle_deg
		{
			get; set;
		}
	}

	public interface IContourGeomData : IGeomData, ITransformableGeom, ICenterAvgDir
	{
	}

	public interface IStdPatternGeomData : IGeomData, ITransformableGeom, IRotatable, ICenterAvgDir
	{
	}
}
