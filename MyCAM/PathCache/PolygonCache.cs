using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.Helper.CAM;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	public class PolygonCache : StdPatternCacheBase
	{
		public PolygonCache( IStdPatternGeomData geomData, CraftData craftData )
			: base( geomData, craftData )
		{
			if( geomData == null || !( geomData is PolygonGeomData polygonGeomData ) ) {
				throw new ArgumentNullException( "PolygonCache constructing argument error - invalid geomData" );
			}
			m_PolygonGeomData = polygonGeomData;
			BuildCADCAMPointList();
		}

		protected override void BuildCADCAMPointList()
		{
			ClearCADFactorDirty();
			SetCenterDir();
			m_RefCoord = StdPatternHelper.GetPatternRefCoord( m_ComputeRefCenterDir, m_PolygonGeomData.IsCoordinateReversed );
			SetRefCoordSelfRotated( m_PolygonGeomData.RotatedAngle_deg );
			m_RefPoint = new CAMPoint( new CADPoint( m_RefCoord.Location(), m_RefCoord.Direction(), m_RefCoord.XDirection(), m_RefCoord.YDirection() ), m_RefCoord.Direction() );

			// Create compensated geom data for calculation
			// Compensated distance: negative = shrink inward, positive = expand outward
			m_ComputeGeomData = CreateCompensatedGeomData();

			m_StartCADPointList = StdPatternStartPointListFactory.GetStartPointList( m_RefCoord, m_ComputeGeomData );
			m_MainPathCADPointList = Discretize( (PolygonGeomData)m_ComputeGeomData );
			BuildCAMPointList();
		}

		protected override void BuildCAMPointList()
		{
			ClearCAMFactorDirty();
			SetPathCAMPoint();
			SetStartPoint();

			// calculate max over cut length
			m_MaxOverCutLength = OverCutHelper.GetMaxOverCutLength( m_PolygonGeomData, m_CraftData.StartPointIndex );

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
			m_ComputeRefCenterDir = m_PolygonGeomData.RefCenterDir.Transformed( m_CraftData.CumulativeTrsfMatrix );
		}

		List<CADPoint> Discretize( PolygonGeomData geomData )
		{
			if( geomData == null || m_RefCoord == null ) {
				return new List<CADPoint>();
			}

			gp_Trsf transformation = DiscreteUtility.CreateCoordTransformation( m_RefCoord );
			List<CADPoint> discretizedPoints = StdPatternDiscreteFactory.DiscretizePolygon( geomData.Sides, geomData.SideLength, geomData.CornerRadius, transformation );

			// ensure all start points are included in the discretized list
			if( m_StartCADPointList != null && m_StartCADPointList.Count > 0 ) {
				discretizedPoints = StartPointHelper.EnsureStartPointsIncluded( m_RefCoord, discretizedPoints, m_StartCADPointList, geomData );
			}

			return discretizedPoints;
		}

		PolygonGeomData CreateCompensatedGeomData()
		{
			double compensatedDist = m_CraftData.CompensatedDistance;
			if( compensatedDist == 0 ) {
				return m_PolygonGeomData;
			}

			// Calculate compensated dimensions
			// For polygon: side length affected by apothem change
			// The compensation distance affects the perpendicular distance from center to side (apothem)
			// For a regular polygon: apothem = sideLength / (2 * tan(π/n))
			// New side length can be calculated from new apothem

			int sides = m_PolygonGeomData.Sides;
			double sideLength = m_PolygonGeomData.SideLength;
			double cornerRadius = m_PolygonGeomData.CornerRadius;

			// Calculate current apothem (perpendicular distance from center to side)
			double angle = Math.PI / sides;
			double apothem = sideLength / ( 2.0 * Math.Tan( angle ) );

			// Apply compensation to apothem
			double compensatedApothem = apothem + compensatedDist;

			// Calculate new side length from compensated apothem
			double compensatedSideLength = 2.0 * compensatedApothem * Math.Tan( angle );

			// Apply compensation to corner radius
			double compensatedCornerRadius = 0;
			if( m_PolygonGeomData.CornerRadius != 0 ) {
				compensatedCornerRadius = cornerRadius + compensatedDist;
			}

			// Ensure side length is positive
			if( compensatedSideLength <= 0 ) {
				return m_PolygonGeomData;
			}

			// If corner radius becomes non-positive after compensation, set to 0 (sharp corner polygon)
			if( compensatedCornerRadius <= 0 ) {
				compensatedCornerRadius = 0;
			}

			return new PolygonGeomData(
				m_PolygonGeomData.RefCenterDir,
				sides,
				compensatedSideLength,
				compensatedCornerRadius,
				m_PolygonGeomData.RotatedAngle_deg,
				m_PolygonGeomData.IsCoordinateReversed
			);
		}

		PolygonGeomData m_PolygonGeomData;
	}
}