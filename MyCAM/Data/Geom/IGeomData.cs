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

	public interface IRotatable
	{
		double RotatedAngle_deg
		{
			get; set;
		}
	}

	public interface IContourGeomData : IGeomData, ITransformableGeom
	{
	}

	public interface IStdPatternGeomData : IGeomData, ITransformableGeom, IRotatable
	{
		gp_Ax3 RefCoord
		{
			get;
			set;
		}

		// TODO: this should be removed
		bool IsCoordinateReversed
		{
			get;
			set;
		}
		// TOD: use field above
		void SetRefCoord( gp_Ax3 refCoordinate );
	}
}
