using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.Helper.CAM;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	public class CircleCache : StdPatternCacheBase
	{
		public CircleCache( IStdPatternGeomData geomData, CraftData craftData )
			: base( geomData, craftData )
		{
			if( geomData == null || !( geomData is CircleGeomData circleGeomData ) ) {
				throw new ArgumentNullException( "CircleCache constructing argument error - invalid geomData" );
			}
			m_CircleGeomData = circleGeomData;
			BuildCADCAMPointList();
		}

		protected override void BuildCADCAMPointList()
		{
			ClearCADFactorDirty();

			// build CAD data
			SetCenterDir();
			m_RefCoord = StdPatternHelper.GetPatternRefCoord( m_ComputeRefCenterDir, m_CircleGeomData.IsCoordinateReversed );
			SetRefCoordSelfRotated( m_CircleGeomData.RotatedAngle_deg );
			m_RefPoint = new CAMPoint( new CADPoint( m_RefCoord.Location(), m_RefCoord.Direction(), m_RefCoord.XDirection(), m_RefCoord.YDirection() ), m_RefCoord.Direction() );

			m_ComputeGeomData = CreateCompensatedGeomData();
			m_StartCADPointList = StdPatternStartPointListFactory.GetStartPointList( m_RefCoord, m_ComputeGeomData );
			m_MainPathCADPointList = Discretize( (CircleGeomData)m_ComputeGeomData );

			// build CAM data
			BuildCAMPointList();
		}

		protected override void BuildCAMPointList()
		{
			ClearCAMFactorDirty();
			SetPathCAMPoint();
			SetStartPoint();

			// calculate max over cut length
			m_MaxOverCutLength = OverCutHelper.GetMaxOverCutLength( m_CircleGeomData, m_CraftData.StartPointIndex );

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
			m_ComputeRefCenterDir = m_CircleGeomData.RefCenterDir.Transformed( m_CraftData.CumulativeTrsfMatrix );
		}

		List<CADPoint> Discretize( CircleGeomData geomData )
		{
			if( geomData == null || m_RefCoord == null ) {
				return new List<CADPoint>();
			}

			gp_Trsf transformation = DiscreteUtility.CreateCoordTransformation( m_RefCoord );
			List<CADPoint> discretizedPoints = StdPatternDiscreteFactory.DiscretizeCircle( geomData.Diameter, transformation );

			// ensure all start points are included in the discretized list
			if( m_StartCADPointList != null && m_StartCADPointList.Count > 0 ) {
				discretizedPoints = StartPointHelper.EnsureStartPointsIncluded( m_RefCoord, discretizedPoints, m_StartCADPointList, geomData );
			}

			return discretizedPoints;
		}

		CircleGeomData CreateCompensatedGeomData()
		{
			double compensatedDist = m_CraftData.CompensatedDistance;
			if( compensatedDist == 0 ) {
				return m_CircleGeomData;
			}

			// Calculate compensated diameter
			// For circle: add/subtract compensation distance from radius (so 2× for diameter)
			double compensatedDiameter = m_CircleGeomData.Diameter + 2 * compensatedDist;

			// Ensure diameter is positive
			if( compensatedDiameter <= 0 ) {
				return m_CircleGeomData;
			}

			return new CircleGeomData(
				m_CircleGeomData.RefCenterDir,
				compensatedDiameter,
				m_CircleGeomData.RotatedAngle_deg,
				m_CircleGeomData.IsCoordinateReversed
			);
		}

		CircleGeomData m_CircleGeomData;
	}
}
