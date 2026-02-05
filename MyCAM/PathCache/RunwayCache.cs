using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.Helper.CAM;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	public class RunwayCache : StdPatternCacheBase
	{
		public RunwayCache( IStdPatternGeomData geomData, CraftData craftData )
			: base( geomData, craftData )
		{
			if( geomData == null || !( geomData is RunwayGeomData runwayGeomData ) ) {
				throw new ArgumentNullException( "RunwayCache constructing argument error - invalid geomData" );
			}
			m_RunwayGeomData = runwayGeomData;
			BuildCADCAMPointList();
		}

		protected override void BuildCADCAMPointList()
		{
			ClearCADFactorDirty();
			SetCenterDir();
			m_RefCoord = StdPatternHelper.GetPatternRefCoord( m_ComputeRefCenterDir, m_RunwayGeomData.IsCoordinateReversed );
			SetRefCoordSelfRotated( m_RunwayGeomData.RotatedAngle_deg );

			// set reference point (for runway, it's the left arc center)
			m_RefPoint = RunwayRefPoint();
			m_StartCADPointList = StdPatternStartPointListFactory.GetStartPointList( m_RefCoord, m_RunwayGeomData );
			m_MainPathCADPointList = Discretize();
			BuildCAMPointList();
		}

		protected override void BuildCAMPointList()
		{
			ClearCAMFactorDirty();
			SetPathCAMPoint();
			SetStartPoint();

			// calculate max over cut length
			m_MaxOverCutLength = OverCutHelper.GetMaxOverCutLength( m_RunwayGeomData, m_CraftData.StartPointIndex );

			// set over cut using unified method
			List<IOrientationPoint> mainPathOriPointList = m_CAMPointList.Cast<IOrientationPoint>().ToList();
			OverCutHelper.SetOverCut( mainPathOriPointList, out List<IOrientationPoint> overCutPointList, m_CraftData.OverCutLength, isClosed: true );
			m_OverCutCAMPointList = overCutPointList.Cast<CAMPoint>().ToList();

			// set lead
			List<IOrientationPoint> mainPointList = m_StartCAMPointList.Cast<IOrientationPoint>().ToList();
			LeadHelper.SetLeadIn( mainPointList, out List<IOrientationPoint> leadInPointList, m_CraftData.LeadData, m_CraftData.IsPathReverse );
			m_LeadInCAMPointList = leadInPointList.Cast<CAMPoint>().ToList();
		}

		protected override List<CADPoint> Discretize()
		{
			if( m_RunwayGeomData == null || m_RefCoord == null ) {
				return new List<CADPoint>();
			}

			gp_Trsf transformation = DiscreteUtility.CreateCoordTransformation( m_RefCoord );
			List<CADPoint> discretizedPoints = StdPatternDiscreteFactory.DiscretizeRunway( m_RunwayGeomData.Length, m_RunwayGeomData.Width, transformation );

			// ensure all start points are included in the discretized list
			if( m_StartCADPointList != null && m_StartCADPointList.Count > 0 ) {
				discretizedPoints = StartPointHelper.EnsureStartPointsIncluded( m_RefCoord, discretizedPoints, m_StartCADPointList, m_RunwayGeomData );
			}

			return discretizedPoints;
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
				m_RefCoord.Location(),
				m_RefCoord.Direction(),
				m_RefCoord.XDirection()
			);
			gp_Trsf transformation = new gp_Trsf();
			transformation.SetTransformation( targetCoordSystem, new gp_Ax3() );
			gp_Pnt worldLeftArcCenter = leftArcCenter.Transformed( transformation );

			return new CAMPoint(
				new CADPoint(
					worldLeftArcCenter,
					m_RefCoord.Direction(),
					m_RefCoord.XDirection(),
					m_RefCoord.YDirection()
				),
				m_RefCoord.Direction()
			);
		}

		protected override void SetCenterDir()
		{
			m_ComputeRefCenterDir = m_RunwayGeomData.RefCenterDir.Transformed( m_CraftData.CumulativeTrsfMatrix );
		}

		RunwayGeomData m_RunwayGeomData;
	}
}