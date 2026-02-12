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

			// Create compensated geom data for calculation
			// Compensated distance: negative = shrink inward, positive = expand outward
			m_ComputeGeomData = CreateCompensatedGeomData();

			// set reference point (for runway, it's the left arc center)
			m_RefPoint = RunwayRefPoint( (RunwayGeomData)m_ComputeGeomData );
			m_StartCADPointList = StdPatternStartPointListFactory.GetStartPointList( m_RefCoord, m_ComputeGeomData );
			m_MainPathCADPointList = Discretize( (RunwayGeomData)m_ComputeGeomData );
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

		protected override void SetCenterDir()
		{
			m_ComputeRefCenterDir = m_RunwayGeomData.RefCenterDir.Transformed( m_CraftData.CumulativeTrsfMatrix );
		}

		List<CADPoint> Discretize( RunwayGeomData geomData )
		{
			if( geomData == null || m_RefCoord == null ) {
				return new List<CADPoint>();
			}

			gp_Trsf transformation = DiscreteUtility.CreateCoordTransformation( m_RefCoord );
			List<CADPoint> discretizedPoints = StdPatternDiscreteFactory.DiscretizeRunway( geomData.Length, geomData.Width, transformation );

			// ensure all start points are included in the discretized list
			if( m_StartCADPointList != null && m_StartCADPointList.Count > 0 ) {
				discretizedPoints = StartPointHelper.EnsureStartPointsIncluded( m_RefCoord, discretizedPoints, m_StartCADPointList, geomData );
			}

			return discretizedPoints;
		}

		CAMPoint RunwayRefPoint( RunwayGeomData geomData )
		{
			// calculate runway parameters
			double length = geomData.Length;
			double width = geomData.Width;
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

		RunwayGeomData CreateCompensatedGeomData()
		{
			double compensatedDist = m_CraftData.CompensatedDistance;
			if( compensatedDist == 0 ) {
				return m_RunwayGeomData;
			}

			// Calculate compensated dimensions
			// For runway: length and width both affected
			double compensatedLength = m_RunwayGeomData.Length + 2 * compensatedDist;
			double compensatedWidth = m_RunwayGeomData.Width + 2 * compensatedDist;

			// Ensure dimensions are positive
			if( compensatedLength <= 0 || compensatedWidth <= 0 ) {
				return m_RunwayGeomData;
			}

			return new RunwayGeomData(
				m_RunwayGeomData.RefCenterDir,
				compensatedLength,
				compensatedWidth,
				m_RunwayGeomData.RotatedAngle_deg,
				m_RunwayGeomData.IsCoordinateReversed
			);
		}

		RunwayGeomData m_RunwayGeomData;
	}
}