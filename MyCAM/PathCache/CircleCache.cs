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
			m_StartCADPointList = StdPatternStartPointListFactory.GetStartPointList( m_RefCoord, m_CircleGeomData );
			m_MainPathCADPointList = Discretize();

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

		protected override List<CADPoint> Discretize()
		{
			if( m_CircleGeomData == null || m_RefCoord == null ) {
				return new List<CADPoint>();
			}

			gp_Trsf transformation = DiscreteUtility.CreateCoordTransformation( m_RefCoord );
			List<CADPoint> discretizedPoints = StdPatternDiscreteFactory.DiscretizeCircle( m_CircleGeomData.Diameter, transformation );

			// ensure all start points are included in the discretized list
			if( m_StartCADPointList != null && m_StartCADPointList.Count > 0 ) {
				discretizedPoints = StartPointHelper.EnsureStartPointsIncluded( m_RefCoord, discretizedPoints, m_StartCADPointList, m_CircleGeomData );
			}

			return discretizedPoints;
		}

		protected override void SetCenterDir()
		{
			m_ComputeRefCenterDir = m_CircleGeomData.RefCenterDir.Transformed( m_CraftData.CumulativeTrsfMatrix );
		}

		CircleGeomData m_CircleGeomData;
	}
}
