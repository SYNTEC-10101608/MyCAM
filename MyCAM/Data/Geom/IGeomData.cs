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

	public interface IRefCenterDir
	{
		gp_Ax1 RefCenterDir
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

	public interface IContourGeomData : IGeomData, ITransformableGeom, IRefCenterDir
	{
	}

	public interface IStdPatternGeomData : IGeomData, ITransformableGeom, IRotatable
	{
		gp_Ax3 RefCoord
		{
			get;
			set;
		}

		bool IsCoordinateReversed
		{
			get;
			set;
		}

		void SetRefCoord( gp_Ax3 refCoordinate );
	}
}
