using OCC.gp;
using System;

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

	public interface IContourGeomData : IGeomData, ITransformableGeom
	{
	}

	public interface IStdPatternGeomData : IGeomData, ITransformableGeom
	{
		event Action CADFactorChanged;

		gp_Ax1 RefCenterDir
		{
			get;
			set;
		}

		double RotatedAngle_deg
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
	}
}
