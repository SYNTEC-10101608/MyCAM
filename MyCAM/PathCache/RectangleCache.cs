using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.Helper.CAM;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	public class RectangleCache : StdPatternCacheBase
	{
		public RectangleCache( IStdPatternGeomData geomData, CraftData craftData )
			: base( geomData, craftData )
		{
			if( geomData == null || !( geomData is RectangleGeomData rectangleGeomData ) ) {
				throw new ArgumentNullException( "RectangleCache constructing argument error - invalid geomData" );
			}
			m_RectangleGeomData = rectangleGeomData;
			BuildCADCAMPointList();
		}

		protected override void BuildCADCAMPointList()
		{
			ClearCADFactorDirty();
			SetCenterDir();
			m_RefCoord = StdPatternHelper.GetPatternRefCoord( m_ComputeRefCenterDir, m_RectangleGeomData.IsCoordinateReversed );
			SetRefCoordSelfRotated( m_RectangleGeomData.RotatedAngle_deg );
			m_RefPoint = new CAMPoint( new CADPoint( m_RefCoord.Location(), m_RefCoord.Direction(), m_RefCoord.XDirection(), m_RefCoord.YDirection() ), m_RefCoord.Direction() );

			// Create compensated geom data for calculation
			// Compensated distance: negative = shrink inward, positive = expand outward
			m_ComputeGeomData = CreateCompensatedGeomData();

			m_StartCADPointList = StdPatternStartPointListFactory.GetStartPointList( m_RefCoord, m_ComputeGeomData );
			m_MainPathCADPointList = Discretize( (RectangleGeomData)m_ComputeGeomData );
			BuildCAMPointList();
		}

		protected override void BuildCAMPointList()
		{
			ClearCAMFactorDirty();
			SetPathCAMPoint();
			SetStartPoint();

			// calculate max over cut length
			m_MaxOverCutLength = OverCutHelper.GetMaxOverCutLength( m_RectangleGeomData, m_CraftData.StartPointIndex );

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
			m_ComputeRefCenterDir = m_RectangleGeomData.RefCenterDir.Transformed( m_CraftData.CumulativeTrsfMatrix );
		}

		RectangleGeomData CreateCompensatedGeomData()
		{
			double compensatedDist = m_CraftData.CompensatedDistance;
			if( compensatedDist == 0 ) {
				return m_RectangleGeomData;
			}

			// Calculate compensated dimensions
			// For rectangle: subtract/add compensation distance from all sides
			double compensatedWidth = m_RectangleGeomData.Width + 2 * compensatedDist;
			double compensatedLength = m_RectangleGeomData.Length + 2 * compensatedDist;
			double compensatedCornerRadius = 0;
			if( m_RectangleGeomData.CornerRadius != 0 ) {
				compensatedCornerRadius = m_RectangleGeomData.CornerRadius + compensatedDist;
			}

			// Ensure dimensions are positive
			if( compensatedWidth <= 0 || compensatedLength <= 0 ) {
				return m_RectangleGeomData;
			}

			// If corner radius becomes non-positive after compensation, set to 0 (sharp corner rectangle)
			if( compensatedCornerRadius <= 0 ) {
				compensatedCornerRadius = 0;
			}

			return new RectangleGeomData(
				m_RectangleGeomData.RefCenterDir,
				compensatedWidth,
				compensatedLength,
				compensatedCornerRadius,
				m_RectangleGeomData.RotatedAngle_deg,
				m_RectangleGeomData.IsCoordinateReversed
			);
		}

		List<CADPoint> Discretize( RectangleGeomData geomData )
		{
			if( geomData == null || m_RefCoord == null ) {
				return new List<CADPoint>();
			}

			gp_Trsf transformation = DiscreteUtility.CreateCoordTransformation( m_RefCoord );
			List<CADPoint> discretizedPoints = StdPatternDiscreteFactory.DiscretizeRectangle( geomData.Width, geomData.Length, geomData.CornerRadius, transformation );

			// ensure all start points are included in the discretized list
			if( m_StartCADPointList != null && m_StartCADPointList.Count > 0 ) {
				discretizedPoints = StartPointHelper.EnsureStartPointsIncluded( m_RefCoord, discretizedPoints, m_StartCADPointList, geomData );
			}

			return discretizedPoints;
		}

		RectangleGeomData m_RectangleGeomData;
	}
}
