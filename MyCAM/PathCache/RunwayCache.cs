using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.PathCache
{
	public class RunwayCache : StdPatternCacheBase
	{
		public RunwayCache( gp_Ax3 coordinateInfo, IStdPatternGeomData geomData, CraftData craftData )
			: base( coordinateInfo, craftData )
		{
			if( geomData == null || !( geomData is RunwayGeomData runwayGeomData ) ) {
				throw new ArgumentNullException( "RunwayCacheInfo constructing argument error - invalid geomData" );
			}
			m_RunwayGeomData = runwayGeomData;
			BuildCAMPointList();
		}

		public override PathType PathType
		{
			get
			{
				return PathType.Runway;
			}
		}

		public override IProcessPoint GetProcessRefPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			return m_RefPoint;


		}

		protected override void BuildCAMPointList()
		{
			ClearCraftDataDirty();
			m_RefPoint = RunwayRefPoint();
			m_StartPointList = RunwayCacheInfoExtensions.GetStartPointList( CoordinateInfo, m_RunwayGeomData.Length, m_RunwayGeomData.Width );
		}

		CAMPoint RunwayRefPoint()
		{
			// calculate runway parameters
			double length = m_RunwayGeomData.Length;
			double width = m_RunwayGeomData.Width;
			double radius = width / 2.0;
			double straightLength = length - width;

			// left arc center position in local coordinate system
			gp_Pnt leftArcCenter;

			if( straightLength <= 0.001 ) {

				// pure circle case: center is at origin
				leftArcCenter = new gp_Pnt( 0, 0, 0 );
			}
			else {
				// runway shape: left arc center is at (-straightLength/2, 0, 0)
				double halfStraight = straightLength / 2.0;
				leftArcCenter = new gp_Pnt( -halfStraight, 0, 0 );
			}

			// transform local coordinates to world coordinate system
			gp_Ax3 targetCoordSystem = new gp_Ax3(
				CoordinateInfo.Location(),
				CoordinateInfo.Direction(),
				CoordinateInfo.XDirection()
			);
			gp_Trsf transformation = new gp_Trsf();
			transformation.SetTransformation( targetCoordSystem, new gp_Ax3() );
			gp_Pnt worldLeftArcCenter = leftArcCenter.Transformed( transformation );

			return new CAMPoint(
				new CADPoint(
					worldLeftArcCenter,
					CoordinateInfo.Direction(),
					CoordinateInfo.XDirection(),
					CoordinateInfo.YDirection()
				),
				CoordinateInfo.Direction()
			);
		}
		RunwayGeomData m_RunwayGeomData;
	}

	internal static class RunwayCacheInfoExtensions
	{
		internal static List<CAMPoint> GetStartPointList( gp_Ax3 coordinateInfo, double length, double width )
		{
			gp_Pnt centerPoint = coordinateInfo.Location();
			gp_Pln plane = new gp_Pln( coordinateInfo );
			double radius = width / 2.0;
			double straightLength = length - width;

			// runway shape start points:
			// 1. arc midpoint intersecting with positive X-axis
			// 2. long edge midpoint intersecting with negative Y-axis
			double halfStraight = straightLength / 2.0;
			gp_Dir local_X_pos = gp.DX();
			gp_Dir local_X_neg = gp.DX().Reversed();
			gp_Dir local_Y_pos = gp.DY();
			gp_Dir local_Y_neg = gp.DY().Reversed();
			gp_Dir local_Z_pos = gp.DZ();

			// 1. arc midpoint intersecting with positive X-axis (right arc point on X-axis)
			gp_Pnt local_Pnt_RightArc = new gp_Pnt( halfStraight + radius, 0, 0 );
			gp_Dir local_N1_RightArc = local_Z_pos;
			gp_Dir local_N2_RightArc = local_X_neg;
			gp_Dir local_Tan_RightArc = local_Y_neg;
			gp_Dir local_Tool_RightArc = local_Z_pos;

			// 2. long edge midpoint intersecting with negative Y-axis (bottom edge midpoint)
			gp_Pnt local_Pnt_BottomEdge = new gp_Pnt( 0, -radius, 0 );
			gp_Dir local_N1_BottomEdge = local_Z_pos;
			gp_Dir local_N2_BottomEdge = local_Y_pos;
			gp_Dir local_Tan_BottomEdge = local_X_neg;
			gp_Dir local_Tool_BottomEdge = local_Z_pos;

			// create coordinate transformation
			gp_Ax3 targetCoordSystem = new gp_Ax3(
				centerPoint,
				plane.Axis().Direction(),
				plane.XAxis().Direction()
			);

			gp_Trsf transformation = new gp_Trsf();
			transformation.SetTransformation( targetCoordSystem, new gp_Ax3() );

			// transform right arc point
			CADPoint cad_RightArc = new CADPoint(
				local_Pnt_RightArc.Transformed( transformation ),
				local_N1_RightArc.Transformed( transformation ),
				local_N2_RightArc.Transformed( transformation ),
				local_Tan_RightArc.Transformed( transformation )
			);
			CAMPoint cam_RightArc = new CAMPoint( cad_RightArc, local_Tool_RightArc.Transformed( transformation ) );

			// transform bottom edge midpoint
			CADPoint cad_BottomEdge = new CADPoint(
				local_Pnt_BottomEdge.Transformed( transformation ),
				local_N1_BottomEdge.Transformed( transformation ),
				local_N2_BottomEdge.Transformed( transformation ),
				local_Tan_BottomEdge.Transformed( transformation )
			);
			CAMPoint cam_BottomEdge = new CAMPoint( cad_BottomEdge, local_Tool_BottomEdge.Transformed( transformation ) );

			List<CAMPoint> resultList = new List<CAMPoint>
			{
				cam_RightArc,
				cam_BottomEdge,
			};

			return resultList;
		}
	}
}