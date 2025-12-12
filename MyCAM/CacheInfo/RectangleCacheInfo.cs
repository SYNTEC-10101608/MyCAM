using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.CacheInfo
{
	public class RectangleCacheInfo : StandardPatternBasedCacheInfo
	{
		public RectangleCacheInfo( gp_Ax3 coordinateInfo, IStdPatternGeomData geomData, CraftData craftData )
			: base( coordinateInfo, craftData )
		{
			if( geomData == null || !( geomData is RectangleGeomData rectangleGeomData ) ) {
				throw new ArgumentNullException( "RectangleCacheInfo constructing argument error - invalid geomData" );
			}
			m_RectangleGeomData = rectangleGeomData;
			BuildCAMPointList();
		}

		public override PathType PathType
		{
			get
			{
				return PathType.Rectangle;
			}
		}

		protected override void BuildCAMPointList()
		{
			ClearCraftDataDirty();
			m_RefPoint = new CAMPoint( new CADPoint( m_CoordinateInfo.Location(), m_CoordinateInfo.Direction(), m_CoordinateInfo.XDirection(), m_CoordinateInfo.YDirection() ), m_CoordinateInfo.Direction() );
			m_StartPointList = RectangleCacheInfoExtensions.GetStartPointList( CoordinateInfo, m_RectangleGeomData.Length, m_RectangleGeomData.Width );
		}

		RectangleGeomData m_RectangleGeomData;
	}

	internal static class RectangleCacheInfoExtensions
	{
		internal static List<CAMPoint> GetStartPointList( gp_Ax3 coordinateInfo, double longSideLength, double shortSideLength )
		{
			gp_Pnt centerPoint = coordinateInfo.Location();
			gp_Pln plane = new gp_Pln( coordinateInfo );
			double halfL = longSideLength / 2.0;
			double halfW = shortSideLength / 2.0;

			gp_Dir local_X_pos = gp.DX();
			gp_Dir local_X_neg = gp.DX().Reversed();
			gp_Dir local_Y_pos = gp.DY();
			gp_Dir local_Y_neg = gp.DY().Reversed();
			gp_Dir local_Z_pos = gp.DZ();

			gp_Pnt local_Pnt_L_Pos = new gp_Pnt( halfL, 0, 0 );
			gp_Dir local_N1_L_Pos = local_Z_pos;
			gp_Dir local_N2_L_Pos = local_X_neg;
			gp_Dir local_Tan_L_Pos = local_Y_neg;
			gp_Dir local_Tool_L_Pos = local_Z_pos;

			gp_Pnt local_Pnt_S_Neg = new gp_Pnt( 0, -halfW, 0 );
			gp_Dir local_N1_S_Neg = local_Z_pos;
			gp_Dir local_N2_S_Neg = local_Y_pos;
			gp_Dir local_Tan_S_Neg = local_X_neg;
			gp_Dir local_Tool_S_Neg = local_Z_pos;

			gp_Ax3 targetCoordSystem = new gp_Ax3( centerPoint, plane.Axis().Direction(), plane.XAxis().Direction() );

			gp_Ax3 finalCoordSystem = targetCoordSystem.Rotated( plane.Axis(), 0 );
			gp_Trsf transformation = new gp_Trsf();
			transformation.SetTransformation( finalCoordSystem, new gp_Ax3() );

			CADPoint cad_L_Pos = new CADPoint(
				local_Pnt_L_Pos.Transformed( transformation ),
				local_N1_L_Pos.Transformed( transformation ),
				local_N2_L_Pos.Transformed( transformation ),
				local_Tan_L_Pos.Transformed( transformation )
			);
			CAMPoint cam_L_Pos = new CAMPoint( cad_L_Pos, local_Tool_L_Pos.Transformed( transformation ) );

			CADPoint cad_S_Neg = new CADPoint(
				local_Pnt_S_Neg.Transformed( transformation ),
				local_N1_S_Neg.Transformed( transformation ),
				local_N2_S_Neg.Transformed( transformation ),
				local_Tan_S_Neg.Transformed( transformation )
			);
			CAMPoint cam_S_Neg = new CAMPoint( cad_S_Neg, local_Tool_S_Neg.Transformed( transformation ) );

			List<CAMPoint> resultList = new List<CAMPoint>
			{
				cam_L_Pos,
				cam_S_Neg,
			};

			return resultList;
		}
	}
}
